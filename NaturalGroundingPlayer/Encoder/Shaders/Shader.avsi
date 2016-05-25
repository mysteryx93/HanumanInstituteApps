# AviSynthShader v1.4.4 (March 25th 2016)
# by Etienne Charland
# 
# Provides a bridge between AviSynth and HLSL pixel shaders for high bit depth processing on the GPU.
# https://github.com/mysteryx93/AviSynthShader
# http://forum.doom9.org/showthread.php?t=172698
# 
# Ex: NNEDI3+SuperRes
# SuperRes(2, .42, 0, """nnedi3_rpow2(2, nns=4, cshift="Spline16Resize", threads=2)""")
# 
# Supported video formats: YV12, YV24, RGB24 and RGB32.
# 
# 
## SuperResXBR(Input, Passes, Str, Soft, XbrStr, XbrSharp, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_out, fKernel, fWidth, fHeight, fB, fC)
# Enhances upscaling quality, combining Super-xBR and SuperRes to run in the same command chain, reducing memory transfers and increasing performance.
# 
# Arguments Passes, Str, Soft are the same as SuperRes.
# Arguments XbrStr, XbrSharp are the same as SuperXBR.
# Arguments fKernel, fWidth, fHeight, fB, fC are the same as ResizeShader and allows downscaling the output before reading back from GPU
# 
# 
## SuperRes(Input, Passes, Str, Soft, Upscale, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_upscale, lsb_out, fKernel, fWidth, fHeight, fB, fC)
# Enhances upscaling quality.
# 
# Arguments:
# Passes: How many SuperRes passes to run. Default=1.
# Str: How agressively we want to run SuperRes, between 0 and 1. Default=1.
# Soft: How much smoothness we want to add, between 0 and 1. Default=0.
# Upscale: An upscaling command that must contain offset-correction. Ex: """nnedi3_rpow2(2, cshift="Spline16Resize")"""
# MatrixIn/MatrixOut: The input and output color matrix (601 or 709). This can be used for color matrix conversion. Default="709" for both
# FormatOut: The output format. Default = same as input.
# Convert: Whether to call ConvertToShader and ConvertFromShader within the shader. Default=true
# ConvertYuv: Whether do YUV-RGB color conversion. Default=true unless Convert=true and source is RGB
# lsb_in, lsb_upscale, lsb_out: Whether the input, result of Upscale and output are to be converted to/from DitherTools' Stack16 format. Default=false
# fKernel, fWidth, fHeight, fB, fC: Allows downscaling the output before reading back from GPU. See ResizeShader.
# 
# 
## SuperXBR(Input, Str, Sharp, FormatOut, Convert, lsb_in, lsb_out, fKernel, fWidth, fHeight, fB, fC)
# Doubles the size of the image. Produces a sharp result, but with severe ringing.
# 
# Arguments:
# Str: Value between 0 and 5 specifying the strength. Default=1.
# Sharp: Value between 0 and 1.5 specifying the weight. Default=1.
# FormatOut: The output format. Default = same as input.
# Convert: Whether to call ConvertToShader and ConvertFromShader within the shader. Default=true
# lsb_in, lsb_out: Whether the input and output are to be converted to/from DitherTools' Stack16 format. Default=false
# fKernel, fWidth, fHeight, fB, fC: Allows downscaling the output before reading back from GPU. See ResizeShader.
# 
#
## ResizeShader(Input, Width, Height, Str, Soft, Kernel, B, C, MatrixIn, MatrixOut, FormatOut, Convert, lsb_in, lsb_out)
# Downscales the image in high quality.
#
# Arguments:
# Width: The width to resize to.
# Height: The height to resize to.
# Str: The algorithm strength to apply between 0 and 1. Default=.5
# Soft: If true, the result will be softer. Default=false
# Kernel: The resize algorithm to use: SSim or Bicubic (default)
# B, C: When using SSim, B sets the Strength (0 to 1, default=.5) and C sets whether to use a soft algorithm (0 or 1, default=0)
# B, C: When using Bicubic, sets the B and C values. Default is B=0, C=.75 (useful for downscaling)
# Other arguments are the same as SuperRes.
#
#
## ColorMatrixShader(input, MatrixIn, MatrixOut, FormatOut)
# Converts the color matrix with 16 bit depth to avoid banding. Source can be YV12, YV24, RGB24 or RGB32.
# 
# Arguments:
# MatrixIn/MatrixOut: The input and output color matrix (601 or 709). Default="709" for both
# FormatOut: The output format. Default = same as input.
# 
# 
# Shaders are written by Shiandow and are available here
# https://github.com/zachsaw/MPDN_Extensions/

function SuperRes(clip Input, int "Passes", float "Str", float "Soft", string "Upscale", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_upscale", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC")
{
	Passes = default(Passes, 1)
	Str = default(Str, 1)
	Soft = default(Soft, 0)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	FormatOut = default(FormatOut, "")
	Convert = default(Convert, true)
	ConvertYuv = default(ConvertYuv, Convert ? !Input.IsRGB() : true)
	lsb_in = default(lsb_in, false)
	lsb_upscale = default(lsb_upscale, false)
	lsb_out = default(lsb_out, false)
	fKernel = default(fKernel, "Bicubic")
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	fB = default(fB, fKernel == "SSim" ? .5 : 0)
	fC = default(fC, fKernel == "SSim" ? 0 : .75)

	Assert((Passes > 0 && Passes <= 5) ? true : false, "Passes must be between 1 and 5")
	Assert((Str >= 0 && Str <= 1) ? true : false, "Str must be between 0 and 1")
	Assert((Soft >= 0 && Soft <= 1) ? true : false, "Soft must be between 0 and 1")
	Assert(Defined(Upscale), "You must specify Upscale")
	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert((!lsb_in && !lsb_upscale && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input

	PrecisionIn = Convert && !lsb_in ? 1 : 2
	PrecisionUpscale = Convert && !lsb_upscale ? 1 : 2
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	# Upscale
	Original = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	Eval(Upscale)
	Input = convert ? ConvertToShader(PrecisionUpscale, lsb=lsb_upscale) : last
	Input.Shader(ConvertYuv && MatrixIn=="601" ? "Yuv601ToLinear.cso" : ConvertYuv ? "YuvToLinear.cso" : "GammaToLinear.cso")

	SmallWidth = Original.Width / PrecisionIn
	SmallHeight = Original.Height
	LargeWidth = Input.Width / PrecisionIn
	LargeHeight = Input.Height

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

	ExecuteShader(last, Input, Original, Precision=3, Clip1Precision=PrecisionUpscale, Clip2Precision=PrecisionIn, OutputPrecision=PrecisionOut)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResXBR(clip Input, int "Passes", float "Str", float "Soft", float "XbrStr", float "XbrSharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC")
{
	Passes = default(Passes, 1)
	Str = default(Str, 1)
	Soft = default(Soft, 0)
	XbrStr = default(XbrStr, 1)
	XbrSharp = default(XbrSharp, 1)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
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

	Assert(Passes > 0 && Passes <= 5, "Passes must be between 1 and 5")
	Assert(Str >= 0 && Str <= 1, "Str must be between 0 and 1")
	Assert(Soft >= 0 && Soft <= 1, "Soft must be between 0 and 1")
	Assert(XbrStr >= 0 && XbrStr <= 5, "XbrStr must be between 0 and 5")
	Assert(XbrSharp >= 0 && XbrSharp <= 1.5, "XbrSharp must be between 0 and 1.5")
	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in or lsb_out")

	Input

	PrecisionIn = Convert && !lsb_in ? 1 : 2
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	InputWidth = Width / PrecisionIn
	InputHeight = Height
	args_string = string(XbrStr,"%.32f") + "," + string(XbrSharp,"%.32f") + ",0,0f"
	size0_string = string(InputWidth) + "," + string(InputHeight) + "," + string(1./InputWidth,"%.32f") + "," + string(1./InputHeight,"%.32f") + "f"
	size1_string = string(2*InputWidth) + "," + string(2*InputHeight) + "," + string(1./(2*InputWidth),"%.32f") + "," + string(1./(2*InputHeight),"%.32f") + "f"

	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	Shader(Output=2)
	
	# Upscale with Super-xBR
	ConvertYuv ? Shader(MatrixIn=="601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso") : last
	Shader("SuperXBR-pass0.cso", Param2=args_string, Param3=size0_string, Width=2*InputWidth, Height=2*InputHeight)
	Shader("SuperXBR-pass1.cso", Param2=args_string, Param3=size1_string)
	Shader("SuperXBR-pass2.cso", Param2=args_string, Param3=size1_string)
	Shader("GammaToLinear.cso")

	SmallWidth = Input.Width / PrecisionIn
	SmallHeight = Input.Height
	LargeWidth = Input.Width / PrecisionIn * 2
	LargeHeight = Input.Height * 2

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

	ExecuteShader(last, Input, Precision=3, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResPass(clip cmd, int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight, float Str, float Soft, int Pass, int Passes, bool ConvertYuv, string MatrixIn, string MatrixOut)
{
	ScriptVariantIn = ConvertYuv && MatrixIn=="601" ? "601.cso" : ConvertYuv && MatrixIn=="709" ? "709.cso" : ".cso"
	ScriptVariantOut = ConvertYuv && MatrixOut=="601" ? "601.cso" : ConvertYuv && MatrixOut=="709" ? "709.cso" : ".cso"

	cmd.Shader("SuperResDownscaler.cso", Output=3, \
		Param2=CreateParamFloat4(LargeWidth, LargeHeight), \
		Width=SmallWidth, Height=LargeHeight)

	Shader("SuperResDownscaleAndDiff" + ScriptVariantIn, \
		Clip1=3, Clip2=2, Output=3, \
		Param2=CreateParamFloat4(SmallWidth, LargeHeight), \
		Width=SmallWidth, Height=SmallHeight)

	Shader(Pass==Passes ? "SuperResFinal" + ScriptVariantOut : \
		(Soft==0 ? "SuperResSkipSoftening.cso" : "SuperRes.cso"), \
		Clip1=1, Clip2=3, Output=1, \
		Param2=CreateParamFloat4(SmallWidth, SmallHeight), \
		Param3=CreateParamFloat4(LargeWidth, LargeHeight), \
		Param4=string(Str,"%.32f") + "," + string(Soft,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}

function SuperXBR(clip Input, float "Str", float "Sharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "lsb_in", bool "lsb_out", string "fKernel", int "fWidth", int "fHeight", float "fB", float "fC")
{
	Str = default(Str, 1)
	Sharp = default(Sharp, 1)
	Convert = default(Convert, true)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	FormatOut = default(FormatOut, "")
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)
	fKernel = default(fKernel, "Bicubic")
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	fB = default(fB, fKernel == "SSim" ? .5 : 0)
	fC = default(fC, fKernel == "SSim" ? 0 : .75)

	Assert(Str >= 0 && Str <= 5, "Str must be between 0 and 5")
	Assert(Sharp >= 0 && Sharp <= 1.5, "Sharp must be between 0 and 1.5")
	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input
	ConvertYuv = convert && !IsRGB()
	PrecisionIn = Convert && !lsb_in ? 1 : 2
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : "RGB32"

	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	
	Input
	InputWidth = Width / PrecisionIn
	InputHeight = Height
	args_string = string(Str,"%.32f") + "," + string(Sharp,"%.32f") + "f"
	size0_string = CreateParamFloat4(InputWidth, InputHeight)
	size1_string = CreateParamFloat4(2*InputWidth, 2*InputHeight)

	ConvertYuv ? Shader(MatrixIn=="601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso") : last

	Shader("SuperXBR-pass0.cso", Param2=args_string, Param3=size0_string, Width=2*InputWidth, Height=2*InputHeight)
	Shader("SuperXBR-pass1.cso", Param2=args_string, Param3=size1_string)
	Shader("SuperXBR-pass2.cso", Param2=args_string, Param3=size1_string)

	# Final Resize
	fWidth > 0 || fHeight > 0 ? ResizeInternal(Input, false, 2*InputWidth, 2*InputHeight, fKernel, fWidth, fHeight, fB, fC) : last

	ConvertYuv ? Shader(MatrixOut=="601" ? "GammaToYuv601.cso" : "GammaToYuv.cso") : last
	last.ExecuteShader(Input, Precision=2, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut)

	convert ? ConvertFromShader(PrecisionOut, Format=sourceFormat, lsb=lsb_out) : last
}

function ResizeShader(clip Input, int "Width", int "Height", float "B", float "C", string "Kernel", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "lsb_in", bool "lsb_out")
{
	Width = default(Width, Input.Width)
	Height = default(Height, Input.Height)
	Convert = default(Convert, true)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	FormatOut = default(FormatOut, "")
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)
	B = default(B, Kernel == "SSim" ? .5 : 0)
	C = default(C, Kernel == "SSim" ? 0 : .75)
	Kernel = default(Kernel, "SSim")

	Assert(Width > 0, "Width must be greater than 0")
	Assert(Height > 0, "Height must be greater than 0")
	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert((!lsb_in && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input
	ConvertYuv = convert && !IsRGB()
	PrecisionIn = Convert && !lsb_in ? 1 : 2
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : "RGB32"

	Input = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	
	Input
	InputWidth = Width / PrecisionIn    
	Shader(ConvertYuv && MatrixIn=="601" ? "Yuv601ToLinear.cso" : ConvertYuv ? "YuvToLinear.cso" : "GammaToLinear.cso")

	ResizeInternal(Input, true, Input.Width / PrecisionIn, Input.Height, Kernel, Width, Height, B, C)

	Shader(ConvertYuv && MatrixOut=="601" ? "LinearToYuv601.cso" : ConvertYuv ? "LinearToYuv.cso" : "LinearToGamma.cso")
	last.ExecuteShader(Input, Precision=3, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut)

	convert ? ConvertFromShader(PrecisionOut, Format=sourceFormat, lsb=lsb_out) : last
}

function ResizeInternal(clip cmd, clip Input, bool SimpleArgs, int InputWidth, int InputHeight, string Kernel, int W, int H, float B, float C)
{
	Prefix = SimpleArgs ? "" : "f" # Functions other than ShaderResize have downscaling arguments starting with 'f'
	Assert(Kernel == "SSim" || Kernel == "Bicubic", Prefix + "Kernel must be SSim or Bicubic")
	Assert(W >= 0 && H >= 0, Prefix + "Width and " + Prefix + "Height must be 0 or above")
	Assert(Kernel != "SSim" || (B >= 0 && B <= 1), Prefix + "B (SSim Str) must be between 0 and 1")
	Assert(Kernel != "SSim" || (C == 0 || C == 1), Prefix + "C (SSim Soft) must be 0 (false) or 1 (true)")

	W = W > 0 ? W : InputWidth
	H = H > 0 ? H : InputHeight
	cmd
	SSim = Kernel == "SSim"

	SSim ? Shader(C==1 ? "SSimSoftDownscalerX.cso" : "SSimDownscalerX.cso",\
		Clip1=1, Output=3,\
		Param2=CreateParamFloat4(InputWidth, InputHeight),\
		Width=W, Height=InputHeight) : last
	SSim ? Shader(C==1 ? "SSimSoftDownscalerY.cso" : "SSimDownscalerY.cso",\
		Clip1=3, Output=4,\
		Param2=CreateParamFloat4(W, InputHeight),\
		Width=W, Height=H) : last
	SSim ? Shader("SSimDownscaledVarI.cso",\
		Clip1=1, Clip2=3, Output=5,\
		Param2=CreateParamFloat4(InputWidth, InputHeight),\
		Width=W, Height=InputHeight) : last
	SSim ? Shader("SSimDownscaledVarII.cso",\
		Clip1=5, Clip2=3, Clip3=4, Output=6,\
		Param2=CreateParamFloat4(W, InputHeight),\
		Width=W, Height=H) : last
	SSim ? Shader("SSimSinglePassConvolver.cso",\
		Clip1=4, Output=7,\
		Width=W, Height=H) : last
	SSim ? Shader("SSimCalcR.cso",\
		Clip1=4, Clip2=7, Clip3=6, Output=8,\
		Width=W, Height=H) : last
	SSim ? Shader("SSimCalc.cso",\
		Clip1=4, Clip2=7, Clip3=8, Output=1,\
		Param3=String(B,"%.32f")+"f",\
		Width=W,Height=H) : last

	!SSim ? Shader("Bicubic.cso",\
		Param0=CreateParamFloat4(W, H),\
		Param1=CreateParamFloat4(InputWidth, InputHeight),\
		Param2=String(B,"%.32f")+","+String(C,"%.32f")+"f",\
		Width=W, Height=H) : last

	return last
}

# Performs color matrix conversion with 16 bit depth to avoid banding
function ColorMatrixShader(clip input, string "MatrixIn", string "MatrixOut", string "FormatOut") {
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	FormatOut = default(FormatOut, "")

	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert(MatrixIn != MatrixOut, "MatrixIn and MatrixOut must be different")

	input
	sourceFormat = FormatOut != "" ? FormatOut : IsYV12 ? "YV12" : IsYV24 ? "YV24" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12 or YV24" + chr(10))

	input = ConvertToShader(1)
	Shader(MatrixIn == "601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso")
	Shader(MatrixOut == "601" ? "GammaToYuv601.cso" : "GammaToYuv.cso")
	ExecuteShader(last, input, Precision=2, Clip1Precision=1, OutputPrecision=1)
	ConvertFromShader(1, format=sourceFormat)
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
