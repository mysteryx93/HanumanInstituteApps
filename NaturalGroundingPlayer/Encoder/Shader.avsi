# AviSynthShader v1.5.4 (July 15th 2016)
# by Etienne Charland
# 
# Provides a bridge between AviSynth and HLSL pixel shaders for high bit depth processing on the GPU.
# https://github.com/mysteryx93/AviSynthShader
# http://forum.doom9.org/showthread.php?t=172698
# 
# Ex: NNEDI3+SuperRes
# SuperRes(2, .42, 0, """nnedi3_rpow2(2, nns=4, cshift="Spline16Resize", threads=2)""")
# 
# Supported video formats: Y8, YV12, YV24, RGB24 and RGB32.
#
## Documentation of Shader.dll available here
# https://github.com/mysteryx93/AviSynthShader 
#
## Common Parameter
# MatrixIn/MatrixOut: The input and output color matrix (Rec601, Rec709, Pc601, Pc709). This can be used for color matrix conversion. Default="Rec709" for both
# FormatOut: The output format. Default = same as input.
# Convert: Whether to call ConvertToShader and ConvertFromShader within the shader. Default=true
# ConvertYuv: Whether do YUV-RGB color conversion. Default=true unless Convert=true and source is RGB
# lsb_in, lsb_out: Whether the input, result of Upscale and output are to be converted to/from DitherTools' Stack16 format. Default=false
# fKernel, fWidth, fHeight, fB, fC: Allows downscaling the output before reading back from GPU. See ResizeShader.
# PlanarIn, PlanarOut: Whether to transfer frame data as 3 individual planes to reduce bandwidth at the expense of extra processing.
#    Generally, PlanarIn brings no performance benefit while PlanarOut brings a nice performance boost. PlanarIn may bring an advantage with larger frames.
#    Default for SuperRes and SuperResXBR: PlanarIn=false, PlanarOut=true. Default for SuperXBR: PlanarIn=true, PlanarOut=true. Default for ResizeShader: PlanarIn=true, PlanarOut=false.
# Engines: In Avisynth+ with MT_NICE_FILTER, sets the number of DirectX engines that will be shared amongst all threads. Set to 2 if running a single shader function for increased performance. Default=1. Ignored in AviSynth 2.6 running with MT_MULTI_INSTANCE.
# Arguments fKernel, fWidth, fHeight, fB, fC are the same as ResizeShader and allows downscaling the output before reading back from GPU
#
# 
## SuperResXBR(Input, Passes, Str, Soft, XbrStr, XbrSharp, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_out, fKernel, fWidth, fHeight, fB, fC, PlanarIn, PlanarUpscale, PlanarOut)
# Enhances upscaling quality, combining Super-xBR and SuperRes to run in the same command chain, reducing memory transfers and increasing performance.
# 
# Arguments Passes, Str, Soft are the same as SuperRes.
# Arguments XbrStr, XbrSharp are the same as SuperXBR.
# 
# 
## SuperRes(Input, Passes, Str, Soft, Upscale, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_upscale, lsb_out, fKernel, fWidth, fHeight, fB, fC, PlanarIn, PlanarUpscale, PlanarOut)
# Enhances upscaling quality.
# 
# Arguments:
# Passes: How many SuperRes passes to run. Default=1.
# Str: How agressively we want to run SuperRes, between 0 and 1. Default=1.
# Soft: How much smoothness we want to add, between 0 and 1. Default=0.
# Upscale: An upscaling command that must contain offset-correction. Ex: """nnedi3_rpow2(2, cshift="Spline16Resize")"""
# lsb_upscale: Whether the result of Upscale is to be converted from DitherTools' Stack16 format. Default=false
# PlanarUpscale: Whether to read the result of Upscale as planar data. Default=true (slight performance gain)
# 
# 
## SuperXBR(Input, Str, Sharp, MatrixIn, MatrixOut, FormatOut, Convert, lsb_in, lsb_out, fKernel, fWidth, fHeight, fB, fC, PlanarIn, PlanarOut)
# Doubles the size of the image. Produces a sharp result, but with severe ringing.
# 
# Arguments:
# Str: Value between 0 and 5 specifying the strength. Default=1.
# Sharp: Value between 0 and 1.5 specifying the weight. Default=1.
# 
#
## ResizeShader(Input, Width, Height, Kernel, B, C, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_out, PlanarIn, PlanarOut)
# Downscales the image in high quality.
#
# Arguments:
# Width: The width to resize to.
# Height: The height to resize to.
# Str: The algorithm strength to apply between 0 and 1. Default=.5
# Soft: If true, the result will be softer. Default=false
# Kernel: The resize algorithm to use: SSim, Bicubic or ColorMatrix. ColorMatrix performs matrix conversion without resizing. Default=Bicubic.
# B, C: When using SSim, B sets the Strength (0 to 1, default=.5) and C sets whether to use a soft algorithm (0 or 1, default=0)
# B, C: When using Bicubic, sets the B and C values. Default is B=1/3, C=1/3.
# When used as a downscaler in other functions, default is fB=0, fC=.75 (useful for downscaling)
#
#
# Shaders are written by Shiandow and are available here
# https://github.com/zachsaw/MPDN_Extensions/

function SuperRes(clip Input, int "Passes", float "Str", float "Soft", string "Upscale", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_upscale", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC", bool "PlanarIn", bool "PlanarUpscale", bool "PlanarOut", int "Engines")
{
	Passes = default(Passes, 1)
	Str = default(Str, 1)
	Soft = default(Soft, 0)
	MatrixIn = default(MatrixIn, "Rec709")
	MatrixOut = default(MatrixOut, "Rec709")
	FormatOut = default(FormatOut, "")
	Convert = default(Convert, true)
	ConvertYuv = default(ConvertYuv, Convert ? !Input.IsRGB() : true)
	lsb_in = default(lsb_in, false)
	lsb_upscale = default(lsb_upscale, lsb_in)
	lsb_out = default(lsb_out, false)
	fKernel = default(fKernel, "Bicubic")
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	fB = default(fB, fKernel == "SSim" ? .5 : 0)
	fC = default(fC, fKernel == "SSim" ? 0 : .75)
	PlanarIn = default(PlanarIn, false) # no performance benefit
	PlanarUpscale = default(PlanarUpscale, true) # slight performance gain, but slightly different output with Precision=1
	PlanarOut = default(PlanarOut, true)

	Assert((Passes > 0 && Passes <= 5) ? true : false, "Passes must be between 1 and 5")
	Assert((Str >= 0 && Str <= 1) ? true : false, "Str must be between 0 and 1")
	Assert((Soft >= 0 && Soft <= 1) ? true : false, "Soft must be between 0 and 1")
	Assert(Defined(Upscale), "You must specify Upscale")
	Assert(MatrixIn == "Rec601" || MatrixIn == "Rec709" || MatrixIn == "Pc601" || MatrixIn == "Pc709", "MatrixIn must be Rec601, Rec709, Pc601 or Pc709")
	Assert(MatrixOut == "Rec601" || MatrixOut == "Rec709" || MatrixOut == "Pc601" || MatrixOut == "Pc709", "MatrixOut must be Rec601, Rec709, Pc601 or Pc709")
	Assert((!lsb_in && !lsb_upscale && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input

	PrecisionIn = lsb_in || !Convert ? 2 : 1
	PrecisionInW = lsb_in || Convert ? 1 : 2
	PrecisionInH = lsb_in ? 2 : 1
	PrecisionUpscale = lsb_upscale || !Convert ? 2 : 1
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	SmallWidth = Input.Width / PrecisionInW
	SmallHeight = Input.Height / PrecisionInH

	PlanarIn = PlanarIn && Convert
	PlanarUpscale = PlanarUpscale && Convert

	# Upscale
	Original = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in, Planar=PlanarUpscale) : last
	Eval(Upscale)
	LargeWidth = Width
	LargeHeight = lsb_upscale ? Height / 2 : Height

	Input = convert ? ConvertToShader(PrecisionUpscale, lsb=lsb_upscale, Planar=PlanarIn) : last

	VariantIn = ConvertYuv ? MatrixIn : ""
	Shader(ConvertYuv ? (PlanarIn ? "YV" : "Yuv") + "ToLinear" + VariantIn + ".cso" : "GammaToLinear.cso")
	PlanarUpscale ? Shader("YVToYuv.cso", Clip1=3, Output=3) : last

	# Downscale (optional)
	fWidth > 0 || fHeight > 0 ? ResizeInternal(Input, false, LargeWidth, LargeHeight, fKernel, fWidth, fHeight, fB, fC) : last
	fWidth = fWidth > 0 ? fWidth : LargeWidth
	fHeight = fHeight > 0 ? fHeight : LargeHeight

	# SuperRes
	SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 1, Passes, ConvertYuv, MatrixIn, MatrixOut)
	Passes > 1 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 2, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 2 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 3, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 3 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 4, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 4 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 5, Passes, ConvertYuv, MatrixIn, MatrixOut) : last

	ExecuteShader(last, Input, Clip3=Original, Precision=3, Clip1Precision=PrecisionUpscale, Clip2Precision=PrecisionIn, OutputPrecision=PrecisionOut, PlanarOut=PlanarOut, Engines=Engines, Resource=true)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResXBR(clip Input, int "Passes", float "Str", float "Soft", float "XbrStr", float "XbrSharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC", bool "PlanarIn", bool "PlanarOut", int "Engines")
{
	Passes = default(Passes, 1)
	Str = default(Str, 1)
	Soft = default(Soft, 0)
	XbrStr = default(XbrStr, 1)
	XbrSharp = default(XbrSharp, 1)
	MatrixIn = default(MatrixIn, "Rec709")
	MatrixOut = default(MatrixOut, "Rec709")
	FormatOut = default(FormatOut, "")
	Convert = default(Convert, true)
	ConvertYuv = default(ConvertYuv, Convert ? !Input.IsRGB() : true)
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)
	fKernel = default(fKernel, "Bicubic")
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	fB = default(fB, fKernel == "SSim" ? .5 : 0)
	fC = default(fC, fKernel == "SSim" ? 0 : .75)
	PlanarIn = default(PlanarIn, false) # no performance benefit
	PlanarOut = default(PlanarOut, true)

	Assert(Passes > 0 && Passes <= 5, "Passes must be between 1 and 5")
	Assert(Str >= 0 && Str <= 1, "Str must be between 0 and 1")
	Assert(Soft >= 0 && Soft <= 1, "Soft must be between 0 and 1")
	Assert(XbrStr >= 0 && XbrStr <= 5, "XbrStr must be between 0 and 5")
	Assert(XbrSharp >= 0 && XbrSharp <= 1.5, "XbrSharp must be between 0 and 1.5")
	Assert(MatrixIn == "Rec601" || MatrixIn == "Rec709" || MatrixIn == "Pc601" || MatrixIn == "Pc709", "MatrixIn must be Rec601, Rec709, Pc601 or Pc709")
	Assert(MatrixOut == "Rec601" || MatrixOut == "Rec709" || MatrixOut == "Pc601" || MatrixOut == "Pc709", "MatrixOut must be Rec601, Rec709, Pc601 or Pc709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in or lsb_out")

	Input

	PrecisionIn = lsb_in || !Convert ? 2 : 1
	PrecisionInW = lsb_in || Convert ? 1 : 2
	PrecisionInH = lsb_in ? 2 : 1
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	SmallWidth = Input.Width / PrecisionInW
	SmallHeight = Input.Height / PrecisionInH
	LargeWidth = SmallWidth * 2
	LargeHeight = SmallHeight * 2
	args_string = string(XbrStr,"%.32f") + "," + string(XbrSharp,"%.32f") + ",0,0f"
	size0_string = CreateParamFloat4(SmallWidth, SmallHeight)
	size1_string = CreateParamFloat4(LargeWidth, LargeHeight)

	PlanarIn = PlanarIn && Convert
	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in, Planar=PlanarIn) : last

	Shader(PlanarIn ? "YVToYuv.cso" : "", Output=3)
	
	# Upscale with Super-xBR
	ConvertYuv ? Shader((PlanarIn ? "YV" : "Yuv") + "ToGamma" + MatrixIn + ".cso") : last
	Shader("SuperXBR-pass0.cso", Param2=args_string, Param3=size0_string, Width=LargeWidth, Height=LargeHeight)
	Shader("SuperXBR-pass1.cso", Param2=args_string, Param3=size1_string)
	Shader("SuperXBR-pass2.cso", Param2=args_string, Param3=size1_string)
	Shader("GammaToLinear.cso")

	# Downscale (optional)
	fWidth > 0 || fHeight > 0 ? ResizeInternal(Input, false, LargeWidth, LargeHeight, fKernel, fWidth, fHeight, fB, fC) : last
	fWidth = fWidth > 0 ? fWidth : LargeWidth
	fHeight = fHeight > 0 ? fHeight : LargeHeight

	# SuperRes
	SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 1, Passes, ConvertYuv, MatrixIn, MatrixOut)
	Passes > 1 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 2, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 2 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 3, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 3 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 4, Passes, ConvertYuv, MatrixIn, MatrixOut) : last
	Passes > 4 ? SuperResPass(SmallWidth, SmallHeight, fWidth, fHeight, Str, Soft, 5, Passes, ConvertYuv, MatrixIn, MatrixOut) : last

	ExecuteShader(last, Input, Precision=3, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut, PlanarOut=PlanarOut, Engines=Engines, Resource=true)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResPass(clip cmd, int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight, float Str, float Soft, int Pass, int Passes, bool ConvertYuv, string MatrixIn, string MatrixOut)
{
	cmd.Shader("SSimDownscalerX.cso", Output=2, \
		Param2=CreateParamFloat4(LargeWidth, LargeHeight), \
		Width=SmallWidth, Height=LargeHeight)

	Shader("SuperResDownscaleAndDiff" + (ConvertYuv ? MatrixIn : "") + ".cso", \
		Clip1=2, Clip2=3, Output=2, \
		Param2=CreateParamFloat4(SmallWidth, LargeHeight), \
		Width=SmallWidth, Height=SmallHeight)

	VariantOut = Pass==Passes ? "Final" + (ConvertYuv ? MatrixOut : "") : Soft==0 ? "SkipSoft" : ""
	Shader("SuperRes" + VariantOut + ".cso", \
		Clip1=1, Clip2=2, Output=1, \
		Param2=CreateParamFloat4(SmallWidth, SmallHeight), \
		Param3=CreateParamFloat4(LargeWidth, LargeHeight), \
		Param4=string(Str,"%.32f") + "," + string(Soft,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}

function SuperXBR(clip Input, float "Str", float "Sharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC", bool "PlanarIn", bool "PlanarOut", int "Engines")
{
	Str = default(Str, 1)
	Sharp = default(Sharp, 1)
	Convert = default(Convert, true)
	ConvertYuv = default(ConvertYuv, Convert ? !Input.IsRGB() : Input.IsY8() ? false : true)
	MatrixIn = default(MatrixIn, "Rec709")
	MatrixOut = default(MatrixOut, "Rec709")
	FormatOut = default(FormatOut, "")
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)
	fKernel = default(fKernel, "Bicubic")
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	fB = default(fB, fKernel == "SSim" ? .5 : 0)
	fC = default(fC, fKernel == "SSim" ? 0 : .75)
	PlanarIn = default(PlanarIn, Convert) # Very slight performance gain, slightly lower memory usage
	PlanarOut = default(PlanarOut, true)

	Assert(Str >= 0 && Str <= 5, "Str must be between 0 and 5")
	Assert(Sharp >= 0 && Sharp <= 1.5, "Sharp must be between 0 and 1.5")
	Assert(MatrixIn == "Rec601" || MatrixIn == "Rec709" || MatrixIn == "Pc601" || MatrixIn == "Pc709", "MatrixIn must be Rec601, Rec709, Pc601 or Pc709")
	Assert(MatrixOut == "Rec601" || MatrixOut == "Rec709" || MatrixOut == "Pc601" || MatrixOut == "Pc709", "MatrixOut must be Rec601, Rec709, Pc601 or Pc709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input
	PrecisionIn = IsY8 ? 0 : lsb_in || !Convert ? 2 : 1
	PrecisionInW = lsb_in || Convert ? 1 : 2
	PrecisionInH = lsb_in ? 2 : 1
	PrecisionOut = IsY8 ? 0 : Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsY8 ? "Y8" : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	SmallWidth = Input.Width / PrecisionInW
	SmallHeight = Input.Height / PrecisionInH
	LargeWidth = SmallWidth * 2
	LargeHeight = SmallHeight * 2
	args_string = string(Str,"%.32f") + "," + string(Sharp,"%.32f") + "f"
	size0_string = CreateParamFloat4(SmallWidth, SmallHeight)
	size1_string = CreateParamFloat4(LargeWidth, LargeHeight)

	PlanarIn = PlanarIn && Convert

	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in, Planar=PlanarIn) : last
	Input
	
	ConvertYuv ? Shader((PlanarIn ? "YV" : "Yuv") + "ToGamma" + MatrixIn + ".cso") : last
	Shader("SuperXBR-pass0.cso", Param2=args_string, Param3=size0_string, Width=LargeWidth, Height=LargeHeight)
	Shader("SuperXBR-pass1.cso", Param2=args_string, Param3=size1_string)
	Shader("SuperXBR-pass2.cso", Param2=args_string, Param3=size1_string)

	# Final Resize
	fWidth > 0 || fHeight > 0 ? ResizeInternal(Input, false, LargeWidth, LargeHeight, fKernel, fWidth, fHeight, fB, fC) : last

	ConvertYuv ? Shader("GammaToYuv" + MatrixOut + ".cso") : last
	last.ExecuteShader(Input, Precision=2, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut, PlanarOut=PlanarOut, Engines=Engines, Resource=true)

	convert ? ConvertFromShader(PrecisionOut, Format=sourceFormat, lsb=lsb_out) : last
}

function ResizeShader(clip Input, int "Width", int "Height", string "Kernel", float "B", float "C", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_out", bool "PlanarIn", bool "PlanarOut", int "Engines")
{
	Width = default(Width, Input.Width)
	Height = default(Height, Input.Height)
	Convert = default(Convert, true)
	ConvertYuv = default(ConvertYuv, Convert ? !Input.IsRGB() : Input.IsY8() ? false : true)
	MatrixIn = default(MatrixIn, "Rec709")
	MatrixOut = default(MatrixOut, "Rec709")
	FormatOut = default(FormatOut, "")
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)
	Kernel = default(Kernel, "Bicubic")
	B = default(B, Kernel == "SSim" ? .5 : 1./3)
	C = default(C, Kernel == "SSim" ? 0 : 1./3)
	PlanarIn = default(PlanarIn, true)
	PlanarOut = default(PlanarOut, false)
	PlanarIn=false

	Assert(Width > 0, "Width must be greater than 0")
	Assert(Height > 0, "Height must be greater than 0")
	Assert(MatrixIn == "Rec601" || MatrixIn == "Rec709" || MatrixIn == "Pc601" || MatrixIn == "Pc709", "MatrixIn must be Rec601, Rec709, Pc601 or Pc709")
	Assert(MatrixOut == "Rec601" || MatrixOut == "Rec709" || MatrixOut == "Pc601" || MatrixOut == "Pc709", "MatrixOut must be Rec601, Rec709, Pc601 or Pc709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input
	PrecisionIn = IsY8 ? 0 : lsb_in || !Convert ? 2 : 1
	PrecisionInW = lsb_in || Convert ? 1 : 2
	PrecisionInH = lsb_in ? 2 : 1
	PrecisionOut = IsY8 ? 0 : Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsY8 ? "Y8" : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be Y8, YV12, YV24, RGB24 or RGB32" + chr(10))

	InputWidth = Input.Width / PrecisionInW
	InputHeight = Input.Height / PrecisionInH

	PlanarIn = PlanarIn && convert

	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in, Planar=PlanarIn) : last
	
	ConvertYuv ? Shader((PlanarIn ? "YV" : "Yuv") + "ToLinear" + MatrixIn + ".cso") : last

	ResizeInternal(Input, true, InputWidth, InputHeight, Kernel, Width, Height, B, C)

	ConvertYuv ? Shader("LinearToYuv" + MatrixOut + ".cso") : Shader("LinearToGamma.cso")
	last.ExecuteShader(Input, Precision=Kernel=="SSim"?3:2, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut, PlanarOut=PlanarOut, Engines=Engines, Resource=true)

	convert ? ConvertFromShader(PrecisionOut, Format=sourceFormat, lsb=lsb_out) : last
}

function ResizeInternal(clip cmd, clip Input, bool SimpleArgs, int InputWidth, int InputHeight, string Kernel, int W, int H, float B, float C)
{
	Prefix = SimpleArgs ? "" : "f" # Functions other than ShaderResize have downscaling arguments starting with 'f'
	Assert(Kernel == "SSim" || Kernel == "Bicubic" || Kernel == "ColorMatrix", Prefix + "Kernel must be SSim, Bicubic or ColorMatrix")
	Assert(W >= 0 && H >= 0, Prefix + "Width and " + Prefix + "Height must be 0 or above")
	Assert(Kernel != "SSim" || (B >= 0 && B <= 1), Prefix + "B (SSim Str) must be between 0 and 1")
	Assert(Kernel != "SSim" || (C == 0 || C == 1), Prefix + "C (SSim Soft) must be 0 (false) or 1 (true)")

	W = W > 0 ? W : InputWidth
	H = H > 0 ? H : InputHeight
	cmd
	SSim = Kernel=="SSim"
	Bicubic = Kernel=="Bicubic"

	Soft = C==1 ? "SSimSoft" : "SSim"
	SSim ? Shader(Soft + "DownscalerX.cso", \
		Clip1=1, Output=10,\
		Param2=CreateParamFloat4(InputWidth, InputHeight),\
		Width=W, Height=InputHeight) : last
	SSim ? Shader(Soft + "DownscalerY.cso", \
		Clip1=10, Output=11,\
		Param2=CreateParamFloat4(W, InputHeight),\
		Width=W, Height=H) : last
	SSim ? Shader("SSimDownscaledVarI.cso",\
		Clip1=1, Clip2=10, Output=12,\
		Param2=CreateParamFloat4(InputWidth, InputHeight),\
		Width=W, Height=InputHeight) : last
	SSim ? Shader("SSimDownscaledVarII.cso",\
		Clip1=12, Clip2=10, Clip3=11, Output=13,\
		Param2=CreateParamFloat4(W, InputHeight),\
		Width=W, Height=H) : last
	SSim ? Shader("SSimSinglePassConvolver.cso",\
		Clip1=11, Output=14,\
		Width=W, Height=H) : last
	SSim ? Shader("SSimCalcR.cso",\
		Clip1=11, Clip2=14, Clip3=13, Output=15,\
		Width=W, Height=H) : last
	SSim ? Shader("SSimCalc.cso",\
		Clip1=11, Clip2=14, Clip3=15, Output=1,\
		Param3=String(B,"%.32f")+"f",\
		Width=W,Height=H) : last

	Bicubic ? Shader("Bicubic.cso",\
		Param0=CreateParamFloat4(W, H),\
		Param1=CreateParamFloat4(InputWidth, InputHeight),\
		Param2=String(B,"%.32f")+","+String(C,"%.32f")+"f",\
		Width=W, Height=H) : last

	return last
}

function CreateParamXY(int width, int height) {
	return string(width) + "," + string(height) + "f"
}

function CreateParamDivXY(int width, int height) {
	return string(1./width, "%.32f") + "," + string(1./height, "%.32f") + "f"
}

function CreateParamFloat4(int width, int height) {
	return string(width) + "," + string(height) + "," + string(1./width, "%.32f") + "," + string(1./height, "%.32f") + "f"
}
