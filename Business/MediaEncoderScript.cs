using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using EmergenceGuardian.FFmpeg;

namespace Business {
    public static class MediaEncoderScript {
        public static AviSynthScriptBuilder GenerateVideoScript(MediaEncoderSettings settings, string inputFile, bool preview, bool multiThreaded) {
            int CPU = multiThreaded ? Environment.ProcessorCount : 1;
            if (CPU <= 1)
                multiThreaded = false;
            bool RunMultiProcesses = false;
            if (settings.Deshaker && multiThreaded) {
                RunMultiProcesses = true; // Run through MP_Pipeline
                multiThreaded = false; // Deshaker doesn't work with MT
                CPU = 1;
            }
            // multiThreaded = true;

            settings.CalculateSize();

            // Calculate encoding and final frame rates.
            double ChangeSpeedValue = (settings.ChangeSpeed && settings.CanAlterAudio) ? (double)settings.ChangeSpeedValue / 100 : 1;
            double FrameRateBefore = settings.SourceFrameRate.Value * ChangeSpeedValue;

            // Generate video script.
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            bool IsYV24 = false;
            Script.AppendLine(); // After clean-up, this adds a line after LoadPlugin commands.
            Script.AddPluginPath();
            if (multiThreaded)
                Script.LoadPluginAvsi("AviSynthMT.avsi");
            if (settings.ConvertToAvi || inputFile.ToLower().EndsWith(".avi"))
                Script.OpenAvi(inputFile, !string.IsNullOrEmpty(settings.SourceAudioFormat));
            else
                Script.OpenDirect(inputFile, !string.IsNullOrEmpty(settings.SourceAudioFormat));
            if (FrameRateBefore != settings.SourceFrameRate)
                Script.AppendLine(CultureInfo.InvariantCulture, "AssumeFPS({0}, true)", FrameRateBefore);
            if (settings.Trim && settings.CanAlterAudio) {
                Script.AppendLine("Trim({0}, {1})",
                    (settings.TrimStart ?? 0) > 0 ? (int)(settings.TrimStart.Value * settings.SourceFrameRate.Value) : 0,
                    (settings.TrimEnd ?? 0) > 0 && !preview ? (int)(settings.TrimEnd.Value * settings.SourceFrameRate.Value) : 0);
            }
            if (settings.CropSource.HasValue) {
                Script.AppendLine("Crop({0}, {1}, -{2}, -{3})", settings.CropSource.Left, settings.CropSource.Top, settings.CropSource.Right, settings.CropSource.Bottom);
            }

            // If possible, KNLMeans will output 16-bit frames before upscaling.
            bool IsHD = (settings.Denoise || settings.Dering || settings.Degrain || settings.SourceColorMatrix != ColorMatrix.Rec601 || (settings.FrameDouble > 0 && (settings.SuperRes || settings.UpscaleMethod == UpscaleMethods.SuperXbr)));
            bool IsColorMatrixHD = IsHD;
            bool IsChromaFixed = settings.SourceChromaPlacement == ChromaPlacement.MPEG2;
            if (IsHD) {
                Script.LoadPluginDll("masktools2.dll");
                Script.AppendLine("ConvertBits(16)");
                if (settings.SourceColorMatrix != ColorMatrix.Rec709)
                    Script.AppendLine("[ColorMatrixShader]"); // Placeholder that will be replaced after generating the rest.
                if (settings.Denoise && settings.FrameDouble > 0) {
                    Script.AppendLine(@"ConvertToYUV444(chromaresample=""Spline36""{0})",
                        !IsChromaFixed ? string.Format(@", ChromaInPlacement=""{0}""", settings.SourceChromaPlacement.ToString()) : "");
                    IsChromaFixed = true;
                }
            }

            bool OpenCL12 = AvisynthTools.GpuSupport == SupportedOpenClVersion.v12;
            if (settings.Denoise || (settings.Degrain && settings.DegrainPrefilter == DegrainPrefilters.KNLMeans))
                Script.LoadPluginDll(OpenCL12 ? "KNLMeansCL.dll" : "KNLMeansCL-6.11.dll");

            if (settings.Denoise) {
                OpenCL12 = false; // temporary fix
                // Only use cmode=true (denoise chroma) when upscaling.
                if (settings.FrameDouble > 0) {
                    if (!IsHD) {
                        Script.AppendLine("ConvertToYUV444(){0}",
                            !IsChromaFixed ? string.Format(@", ChromaInPlacement=""{0}""", settings.SourceChromaPlacement.ToString()) : "");
                        IsChromaFixed = true;
                    }
                    IsYV24 = true;
                }
                if (!OpenCL12 && IsHD) {
                    Script.LoadPluginDll("ConvertStacked.dll");
                    Script.AppendLine("ConvertToStacked()");
                }
                Script.AppendLine(CultureInfo.InvariantCulture, @"KNLMeansCL(D={0}, A={1}, h={2}{3}, device_type=""{4}""{5}{6})",
                    settings.DenoiseD, settings.DenoiseA,
                    ((double)settings.DenoiseStrength / 10).ToString(CultureInfo.InvariantCulture),
                    settings.FrameDouble > 0 ? (AvisynthTools.GpuSupport == SupportedOpenClVersion.v12 ? ", channels=\"YUV\"" : ", cmode=true") : "",
                    AvisynthTools.GpuSupport != SupportedOpenClVersion.None ? "GPU" : "CPU",
                    AvisynthTools.GpuSupport != SupportedOpenClVersion.None ? ", device_id=" + Settings.SavedFile.GraphicDeviceId.ToString() : "",
                    !OpenCL12 && IsHD ? ", lsb_inout=true" : "");
                if (!OpenCL12 && IsHD)
                    Script.AppendLine("ConvertFromStacked()");
            }

            if (settings.Dering) {
                Script.LoadPluginAvsi("HQDeringmod.avsi");
                Script.LoadPluginDll("MedianBlur2.dll");
                Script.LoadPluginDll("dfttest.dll");
                Script.LoadPluginDll("RgTools.dll");
                Script.LoadPluginDll("SmoothAdjust.dll");
                Script.AppendLine("HQDeringmod({0})", IsHD ? "lsb_in=true, lsb=true" : "");
            }

            string FinalSize = string.Format("fWidth={0}, fHeight={1}",
                settings.OutputWidth + settings.CropAfter.Left + settings.CropAfter.Right,
                settings.OutputHeight + settings.CropAfter.Top + settings.CropAfter.Bottom);
            string FinalResize = FinalSize;
            if (settings.DownscaleMethod == DownscaleMethods.SSim)
                FinalResize += string.Format(@", fKernel=""SSim"", fB={0}{1}",
                    settings.SSimStrength / 100, settings.SSimSoft ? ", fC=1" : "");
            else // DownscaleMethod == Bicubic
                FinalResize += string.Format(@", fKernel=""Bicubic"", fB=0, fC=.75");

            if (settings.FrameDouble > 0) {
                bool IsLastDouble = false;
                if (settings.UpscaleMethod == UpscaleMethods.NNedi3) {
                    DitherPost(Script, ref IsHD, ref IsYV24, true);

                    Script.LoadPluginDll("nnedi3.dll");
                    Script.LoadPluginAvsi("edi_rpow2.avsi");
                    Script.LoadPluginAvsi("ResizeX.avsi");

                    for (int i = 0; i < settings.FrameDouble; i++) {
                        IsLastDouble = i == settings.FrameDouble - 1;
                        string DoubleScript = string.Format("edi_rpow2(2, nns=4, {0}, Threads=2)",
                            IsLastDouble && settings.DownscaleMethod == DownscaleMethods.Bicubic ? @"cshift=""Bicubic"", a1=0, a2=.75, " + FinalSize : @"cshift=""Spline16Resize""");
                        if (settings.SuperRes) {
                            Script.AppendLine(CultureInfo.InvariantCulture, @"SuperRes({0}, {1}, {2}, """"""{3}""""""{4}{5})",
                                settings.SuperRes3Passes ? (i == 0 && settings.FrameDouble > 1 ? 5 : 3) : 2,
                                settings.SuperResStrength / 100f, settings.SuperResSoftness / 100f, DoubleScript,
                                i == 0 ? GetMatrixIn(settings) : "",
                                IsLastDouble && settings.DownscaleMethod == DownscaleMethods.SSim ? ", " + FinalResize : "");
                        } else {
                            Script.AppendLine(DoubleScript);
                            if (IsLastDouble && settings.DownscaleMethod == DownscaleMethods.SSim) {
                                Script.AppendLine("ResizeShader({0}{1})",
                                    FinalResize.Replace("fWidth=", "").Replace("fHeight=", "").Replace(" f", " "), // Use same string as FinalResize but remove 'f' parameter sufix.
                                    GetMatrixIn(settings));
                            }
                        }
                        if (i == 0)
                            ApplySpecialFilters(Script, settings, ref IsHD, ref IsYV24, CPU, RunMultiProcesses);
                    }
                } else { // UpscaleMethod == SuperXBR
                    for (int i = 0; i < settings.FrameDouble; i++) {
                        IsLastDouble = i == settings.FrameDouble - 1;
                        if (!IsYV24) {
                            if (!IsHD) {
                                Script.AppendLine("ConvertBits(16)");
                                IsHD = true;
                            }
                            Script.AppendLine(@"ConvertToYUV444(chromaresample=""Spline36""{0})");
                            IsYV24 = true;
                        }
                        if (settings.SuperRes) {
                            Script.AppendLine(CultureInfo.InvariantCulture, @"SuperResXBR({0}, {1}, {2}, XbrStr={3}, XbrSharp={4}{5}{6}{7})",
                                settings.SuperRes3Passes ? (i == 0 && settings.FrameDouble > 1 ? 5 : 3) : 2,
                                settings.SuperResStrength / 100f, settings.SuperResSoftness / 100f,
                                settings.SuperXbrStrength / 10f, settings.SuperXbrSharpness / 10f,
                                i == 0 ? GetMatrixIn(settings) : "",
                                IsLastDouble ? ", " + FinalResize + ", FormatOut=\"YV12\"" : "",
                                IsHD && i > 0 ? ", PrecisionOut=1" : "");
                        } else {
                            Script.AppendLine("SuperXBR(Str={0}, Sharp={1}{2}{3}{4})",
                                settings.SuperXbrStrength / 10f, settings.SuperXbrSharpness / 10f,
                                i == 0 ? GetMatrixIn(settings) : "",
                                IsLastDouble ? ", " + FinalResize + ", FormatOut=\"YV12\"" : "",
                                IsHD && i > 0 ? ", PrecisionOut=1" : "");
                        }
                        if (i == 0)
                            ApplySpecialFilters(Script, settings, ref IsHD, ref IsYV24, CPU, RunMultiProcesses);
                        else
                            IsHD = false;
                        if (IsLastDouble)
                            IsYV24 = false;
                    }
                }
            }

            if (settings.FrameDouble == 0)
                ApplySpecialFilters(Script, settings, ref IsHD, ref IsYV24, CPU, RunMultiProcesses);
            if (settings.DownscaleMethod != DownscaleMethods.SSim)
                DitherPost(Script, ref IsHD, ref IsYV24, settings.IncreaseFrameRate);

            string[] ShaderCommands = new string[] { "SuperRes", "SuperXBR", "SuperResXBR", "ResizeShader" };
            if (settings.CropAfter.HasValue || settings.FrameDouble == 0) {
                if (settings.CropAfter.HasValue || settings.SourceAspectRatio != 1 || settings.OutputWidth != settings.SourceWidth || settings.OutputHeight != settings.OutputHeight) {
                    if (settings.DownscaleMethod == DownscaleMethods.SSim && settings.FrameDouble == 0) {
                        Script.AppendLine("ResizeShader({0}{1})",
                            FinalResize.Replace("fWidth=", "").Replace("fHeight=", "").Replace(" f", " "), // Use same string as FinalResize but remove 'f' parameter sufix.
                            GetMatrixIn(settings));
                    }
                    DitherPost(Script, ref IsHD, ref IsYV24, false);

                    if (settings.CropAfter.HasValue || settings.DownscaleMethod == DownscaleMethods.Bicubic) {
                        Script.LoadPluginAvsi("ResizeX.avsi");
                        bool ApplyResize = settings.FrameDouble == 0 || !Script.ContainsAny(ShaderCommands);
                        string CropFormat = "ResizeX";
                        CropFormat += settings.CropAfter.HasValue ? "({0}, {1}, {2}, {3}, -{4}, -{5}{6})" : "({0}, {1}{6})";
                        Script.AppendLine(CropFormat, settings.OutputWidth, settings.OutputHeight,
                            settings.CropAfter.Left, settings.CropAfter.Top, settings.CropAfter.Right, settings.CropAfter.Bottom,
                            ApplyResize ? @", kernel=""Bicubic"", a1=0, a2=.75" : "");
                    }
                }
            }

            // Use ColorMatrixShader only if it wasn't already applied through any other shader.
            if (Script.ContainsAny(ShaderCommands)) {
                // ColorMatrix was applied though another shader.
                Script.Replace("[ColorMatrixShader]" + Environment.NewLine, "");
                Script.LoadPluginDll("Shader.dll");
                Script.LoadPluginAvsi("Shader.avsi");
            } else {
                string ColorMatrixScript = "";
                // Apply color matrix
                if (settings.SourceColorMatrix == ColorMatrix.Pc709) {
                    Script.LoadPluginDll("SmoothAdjust.dll");
                    ColorMatrixScript = string.Format(@"SmoothLevels{0}(preset=""pc2tv"")", IsColorMatrixHD ? "16" : "");
                } else if (settings.SourceColorMatrix != ColorMatrix.Rec709) {
                    Script.LoadPluginDll("Shader.dll");
                    Script.LoadPluginAvsi("Shader.avsi");
                    ColorMatrixScript = string.Format(@"ResizeShader(Kernel=""ColorMatrix""{0})", GetMatrixIn(settings));
                }
                Script.Replace("[ColorMatrixShader]" + Environment.NewLine, ColorMatrixScript + Environment.NewLine);
            }

            if (preview) {
                if (settings.Crop)
                    Script.AppendLine("AddBorders(2, 2, 2, 2, $FFFFFF)");
                Script.AppendLine(@"ConvertToRGB32(matrix=""Rec709"")");
            }
            if (multiThreaded)
                Script.AppendLine("Prefetch({0})", CPU);

            if (RunMultiProcesses)
                Script.ConvertToMultiProcesses(settings.SourceFrameRate.Value);
            else
                Script.Cleanup();

            return Script;
        }

        /// <summary>
        /// Applies filters after the first frame double.
        /// </summary>
        private static void ApplySpecialFilters(AviSynthScriptBuilder Script, MediaEncoderSettings settings, ref bool IsLsb, ref bool IsYV24, int CPU, bool multiProcess) {
            if (settings.Degrain)
                ApplyDegrain(Script, settings, ref IsLsb, ref IsYV24);
            if (settings.Deshaker)
                ApplyDeshaker(Script, settings, ref IsLsb, ref IsYV24, multiProcess);
            if (settings.IncreaseFrameRate) {
                DitherPost(Script, ref IsLsb, ref IsYV24, true);
                ApplyInterFrame(Script, settings, CPU);
            }
        }

        private static void DitherPost(AviSynthScriptBuilder Script, ref bool isHD, ref bool isYV24, bool furtherProcessing) {
            //if (isYV24)
            //    Script.AppendLine(@"ConvertToYUV420(chromaresample=""Spline36"")");
            if (isHD)
                Script.AppendLine("ConvertBits(8, dither={0})", furtherProcessing ? "1" : "0");
            isHD = false;
            //isYV24 = false;
        }

        private static void ApplyDegrain(AviSynthScriptBuilder Script, MediaEncoderSettings settings, ref bool isLsb, ref bool isYV24) {
            Script.LoadPluginAvsi("smdegrain.avsi");
            Script.LoadPluginDll("MVTools2.dll");
            Script.LoadPluginDll("MedianBlur2.dll");
            if (settings.DegrainSharp)
                Script.LoadPluginDll("rgtools.dll");
            else
                Script.LoadPluginDll("RemoveGrain.dll");

            if (isYV24) {
                if (isLsb)
                    Script.AppendLine(@"Dither_resize16nr(Width, Height/2, kernel=""Spline36"", csp=""YV12"")");
                else
                    Script.AppendLine("ConvertToYV12()");
                isYV24 = false;
            }

            Script.AppendLine("SMDegrain(thsad={0}, prefilter={1}{2}{3})",
                settings.DegrainStrength * 10,
                settings.DegrainPrefilter == DegrainPrefilters.SD ? 1 : settings.DegrainPrefilter == DegrainPrefilters.HD ? 2 : 4,
                settings.DegrainSharp ? ", contrasharp=true" : "",
                isLsb ? ", lsb_in=true, lsb_out=true" : "");
        }

        private static void ApplyDeshaker(AviSynthScriptBuilder Script, MediaEncoderSettings settings, ref bool isLsb, ref bool isYV24, bool multiProcess) {
            settings.DeshakerSettings.LogFile = Script.GetAsciiPath(settings.DeshakerLog);
            settings.DeshakerSettings.Pass = 2;
            Script.LoadPluginDll(@"VDubFilter.dll");

            Script.AppendLine(@"LoadVirtualDubPlugin(P+""deshaker.vdf"", ""deshaker"", preroll=0)");
            Script.AppendLine(@"Dither_convert_yuv_to_rgb(output=""rgb32"", matrix=""709"", noring=true, lsb_in={0}, mode=6)", isLsb);
            if (multiProcess) {
                Script.AppendLine("### prefetch: 3, 3"); // Keep 3 frames before and after for temporal filters.
                Script.AppendLine("### ###"); // MP_Pipeline starts new process here
            }
            Script.AppendLine(@"deshaker(""{0}"")", settings.DeshakerSettings.ToString());
            Script.AppendLine(@"ConvertToYV12(matrix=""Rec709"")");
            if (multiProcess) {
                Script.AppendLine("### prefetch: {0}, {1}", settings.DeshakerSettings.FillBordersWithFutureFrames ? settings.DeshakerSettings.FillBordersWithFutureFramesCount : 0, settings.DeshakerSettings.FillBordersWithPreviousFrames ? settings.DeshakerSettings.FillBordersWithPreviousFramesCount : 0);
                Script.AppendLine("### ###"); // MP_Pipeline starts another here
            }
            isLsb = false;
            isYV24 = false;
        }

        private static void ApplyInterFrame(AviSynthScriptBuilder Script, MediaEncoderSettings settings, int CPU) {
            Script.LoadPluginDll("FrameRateConverter.dll");
            Script.LoadPluginAvsi("FrameRateConverter.avsi");
            Script.LoadPluginDll("MvTools2.dll");
            Script.LoadPluginDll("RgTools.dll");
            if (settings.IncreaseFrameRateValue == FrameRateModeEnum.Double)
                Script.AppendLine(@"FrameRateConverter(FrameDouble=true)");
            else {
                int NewNum = 0;
                int NewDen = 0;
                if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps30) {
                    NewNum = 30; // 30000;
                    NewDen = 1; //  1001;
                } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps60) {
                    NewNum = 60; // 60000;
                    NewDen = 1; // 1001;
                } else if (settings.IncreaseFrameRateValue == FrameRateModeEnum.fps120) {
                    NewNum = 120; // 120000;
                    NewDen = 1; // 1001;
                }
                Script.AppendLine(@"FrameRateConverter(NewNum={0}, NewDen={1})", NewNum, NewDen);
            }
        }

        /// <summary>
        /// Returns the code for the MatrixIn argument based on SourceColorMatrix.
        /// </summary>
        private static string GetMatrixIn(MediaEncoderSettings settings) {
            string Result = null;
            if (settings.SourceColorMatrix == ColorMatrix.Rec601)
                Result = "Rec601";
            else if (settings.SourceColorMatrix == ColorMatrix.Pc601)
                Result = "Pc601";
            else if (settings.SourceColorMatrix == ColorMatrix.Pc709)
                Result = "Pc709";
            if (Result != null)
                return string.Format(@", MatrixIn=""{0}""", Result);
            else
                return "";
        }

        /// <summary>
        /// Generates a script for Deshaker prescan. This is a much simplified version of the full script that will execute faster.
        /// </summary>
        public static string GenerateDeshakerScript(MediaEncoderSettings settings, string inputFile, int segment, long frameStart, long frameEnd) {
            bool MultiProcesses = false;

            // Calculate encoding and final frame rates.
            double ChangeSpeedValue = settings.ChangeSpeed ? (double)settings.ChangeSpeedValue / 100 : 1;
            double FrameRateBefore = settings.SourceFrameRate.Value * ChangeSpeedValue;

            // Generate video script.
            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.LoadPluginDll("VDubFilter.dll");
            Script.AppendLine(@"LoadVirtualDubPlugin(P+""deshaker.vdf"", ""deshaker"", preroll=0)");
            if (settings.ConvertToAvi || inputFile.ToLower().EndsWith(".avi"))
                Script.OpenAvi(inputFile, false);
            else
                Script.OpenDirect(inputFile, false);
            if (FrameRateBefore != settings.SourceFrameRate)
                Script.AppendLine(CultureInfo.InvariantCulture, "AssumeFPS({0}, true)", FrameRateBefore);
            if (settings.Trim) {
                Script.AppendLine("Trim({0}, {1})",
                    (settings.TrimStart ?? 0) > 0 ? (int)(settings.TrimStart.Value * settings.SourceFrameRate.Value) : 0,
                    (settings.TrimEnd ?? 0) > 0 ? (int)(settings.TrimEnd.Value * settings.SourceFrameRate.Value) : 0);
            }
            if (settings.CropSource.HasValue) {
                Script.AppendLine("Crop({0}, {1}, -{2}, -{3})", settings.CropSource.Left, settings.CropSource.Top, settings.CropSource.Right, settings.CropSource.Bottom);
            }

            // Generate script for a segment.
            if (frameStart > 0 || frameEnd > 0)
                Script.AppendLine("Trim({0}, {1})", frameStart, frameEnd);

            // Resize to the dimention deshaker will run at.
            if (settings.FrameDouble == 1) {
                int FinalWidth = settings.OutputWidth.Value + settings.CropAfter.Left + settings.CropAfter.Right;
                int FinalHeight = settings.OutputHeight + settings.CropAfter.Top + settings.CropAfter.Bottom;
                Script.AppendLine("BicubicResize({0}, {1})", FinalWidth, FinalHeight);
            } else if (settings.FrameDouble > 1) {
                Script.AppendLine("BicubicResize(Width*2, Height*2)");
            }

            // Deshaker requires RGB input.
            Script.AppendLine(@"ConvertToRGB32(matrix=""{0}"")",
                settings.SourceColorMatrix == ColorMatrix.Rec601 ? "Rec601" : settings.SourceColorMatrix == ColorMatrix.Pc709 ? "Pc709" : settings.SourceColorMatrix == ColorMatrix.Pc601 ? "Pc601" : "Rec709");

            // Generate log file
            Script.AppendLine(@"deshaker(""{0}"")", settings.DeshakerSettings.ToString(segment));

            // FFMPEG will generate an AVI file; make it 8x8 to minimize processing.
            Script.AppendLine("ConvertToYV12()");
            Script.AppendLine("PointResize(64,64)");
            if (MultiProcesses) {
                Script.AppendLine("### prefetch: 0, 0");
                Script.AppendLine("### branch: 2");
                Script.AppendLine("### ###");
                Script.ConvertToMultiProcesses(settings.SourceFrameRate.Value);
            }

            return Script.Script;
        }
    }
}
