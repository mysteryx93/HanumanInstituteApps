# AviSynthShader v1.3.5 (January 7th 2016)
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
## SuperResXBR(Input, Passes, Str, Soft, XbrStr, XbrSharp, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_out, fWidth, fHeight, b, c)
# Enhances upscaling quality, combining Super-xBR and SuperRes to run in the same command chain, reducing memory transfers and increasing performance.
# 
# Arguments are the same as SuperRes and Super-xBR
# fWidth, fHeight: Allows downscaling output with Bicubic before reading back from GPU
# b, c: b and c parameters of Bicubic resize. Default is b=0, c=.75
# 
# 
## SuperRes(Input, Passes, Str, Soft, Upscale, MatrixIn, MatrixOut, FormatOut, Convert, ConvertYuv, lsb_in, lsb_upscale, lsb_out)
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
# 
# 
## Super-xBR(Input, Str, Sharp, FormatOut, Convert, lsb_in, lsb_out)
# Doubles the size of the image. Produces a sharp result, but with severe ringing.
# 
# Arguments:
# Str: Value between 0 and 5 specifying the strength. Default=1.
# Sharp: Value between 0 and 1.5 specifying the weight. Default=1.
# FormatOut: The output format. Default = same as input.
# Convert: Whether to call ConvertToShader and ConvertFromShader within the shader. Default=true
# lsb_in, lsb_out: Whether the input and output are to be converted to/from DitherTools' Stack16 format. Default=false
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

function SuperRes(clip Input, int "Passes", float "Str", float "Soft", string "Upscale", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_upscale", bool "lsb_out")
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

	Assert((Passes > 0 && Passes <= 3) ? true : false, "Passes must be between 1 and 3")
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

	Original = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	Eval(Upscale)
	Input = convert ? ConvertToShader(PrecisionUpscale, lsb=lsb_upscale) : last
	Input.Shader(ConvertYuv && MatrixIn=="601" ? "Yuv601ToLinear.cso" : ConvertYuv ? "YuvToLinear.cso" : "GammaToLinear.cso")
	SuperResPass(last, Input, Original, Str, Soft, 1, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut)
	Passes > 1 ? SuperResPass(Input, Original, Str, Soft, 2, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut) : nop
	Passes > 2 ? SuperResPass(Input, Original, Str, Soft, 3, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut) : nop

	ExecuteShader(last, Input, Original, Precision=3, Clip1Precision=PrecisionUpscale, Clip2Precision=PrecisionIn, OutputPrecision=PrecisionOut)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResPass(clip cmd, clip Input, clip Original, float Str, float Soft, int Pass, int Passes, bool ConvertYuv, bool Convert, int PrecisionIn, int PrecisionUpscale, string MatrixIn, string MatrixOut)
{
	InputWidth = Input.Width / PrecisionIn
	OriginalWidth = Original.Width / PrecisionIn

	cmd.Shader("Bicubic.cso", Output=3, \
		Param0=string(OriginalWidth) + "," + string(Original.Height) + "," + string(1./OriginalWidth,"%.32f") + "," + string(1./Original.Height,"%.32f") + "f",\
		Param1=string(InputWidth) + "," + string(Input.Height) + "," + string(1./InputWidth,"%.32f") + "," + string(1./Input.Height,"%.32f") + "f",\
		Param2=string(1/3.,"%.32f") + "," + string(1/3.,"%.32f") + "f",\
		Width=OriginalWidth, Height=Original.Height)

	Shader(ConvertYuv && MatrixIn=="601" ? "SuperResDiff601.cso" : ConvertYuv ? "SuperResDiff709.cso" : "SuperResDiff.cso", Clip1=3, Clip2=2, Output=4)

	Shader(Pass==Passes ? (MatrixOut=="601" ? "SuperResFinal601.cso" : ConvertYuv ? "SuperResFinal709.cso" : "SuperResFinal.cso") : "SuperRes.cso", Clip1=1, Clip2=4, Output=1, \
		Param0=string(InputWidth) + "," + string(Input.Height) + "f", \
		Param1=string(1./InputWidth, "%.32f") + "," + string(1./Input.Height, "%.32f") + "f", \
		Param2=string(OriginalWidth) + "," + string(Original.Height) + "," + string(1./(OriginalWidth),"%.32f") + "," + string(1./Original.Height,"%.32f") + "f", \
		Param3=string(Str,"%.32f") + "," + string(Soft,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}


function SuperResXBR(clip Input, int "Passes", float "Str", float "Soft", float "XbrStr", float "XbrSharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "ConvertYuv", bool "lsb_in", bool "lsb_out", int "fWidth", int "fHeight", float "b", float "c")
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
	fWidth = default(fWidth, 0)
	fHeight = default(fHeight, 0)
	b = default(b, 0)
	c = default(c, .75)

	Assert((Passes > 0 && Passes <= 3) ? true : false, "Passes must be between 1 and 3")
	Assert((Str >= 0 && Str <= 1) ? true : false, "Str must be between 0 and 1")
	Assert((Soft >= 0 && Soft <= 1) ? true : false, "Soft must be between 0 and 1")
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
	
	# Super-xBR
	ConvertYuv ? Shader(MatrixIn=="601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso") : last
	Shader("SuperXBR-pass0.cso", Param2=args_string, Param3=size0_string, Width=2*InputWidth, Height=2*InputHeight)
	Shader("SuperXBR-pass1.cso", Param2=args_string, Param3=size1_string)
	Shader("SuperXBR-pass2.cso", Param2=args_string, Param3=size1_string)
	Shader("GammaToLinear.cso")

	# SuperRes
	SuperResXbrPass(last, Input, Str, Soft, 1, Passes, ConvertYuv, Convert, PrecisionIn, MatrixIn, MatrixOut)
	Passes > 1 ? SuperResXbrPass(Input, Str, Soft, 2, Passes, ConvertYuv, Convert, PrecisionIn, MatrixIn, MatrixOut) : last
	Passes > 2 ? SuperResXbrPass(Input, Str, Soft, 3, Passes, ConvertYuv, Convert, PrecisionIn, MatrixIn, MatrixOut) : last

	# Final Resize
	fWidth > 0 || fHeight > 0 ? SuperResXbrResizeFinal(Input, fWidth, fHeight, b, c, PrecisionIn) : last

	ExecuteShader(last, Input, Precision=3, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResXbrPass(clip cmd, clip Input, float Str, float Soft, int Pass, int Passes, bool ConvertYuv, bool Convert, int PrecisionIn, string MatrixIn, string MatrixOut)
{
	InputWidth = Input.Width / PrecisionIn

	cmd.Shader("Bicubic.cso", Output=3, \
		Param0=string(InputWidth) + "," + string(Input.Height) + "," + string(1./InputWidth,"%.32f") + "," + string(1./Input.Height,"%.32f") + "f",\
		Param1=string(InputWidth*2) + "," + string(Input.Height*2) + "," + string(1./(InputWidth*2),"%.32f") + "," + string(1./(Input.Height*2),"%.32f") + "f",\
		Param2=string(1/3.,"%.32f") + "," + string(1/3.,"%.32f") + "f",\
		Width=InputWidth, Height=Input.Height)

	Shader(ConvertYuv && MatrixIn=="601" ? "SuperResDiff601.cso" : ConvertYuv ? "SuperResDiff709.cso" : "SuperResDiff.cso", Clip1=3, Clip2=2, Output=4)

	Shader(Pass==Passes ? (MatrixOut=="601" ? "SuperResFinal601.cso" : ConvertYuv ? "SuperResFinal709.cso" : "SuperResFinal.cso") : "SuperRes.cso", Clip1=1, Clip2=4, Output=1, \
		Param0=string(InputWidth*2) + "," + string(Input.Height*2) + "f", \
		Param1=string(1./(InputWidth*2), "%.32f") + "," + string(1./(Input.Height*2), "%.32f") + "f", \
		Param2=string(InputWidth) + "," + string(Input.Height) + "," + string(1./InputWidth,"%.32f") + "," + string(1./Input.Height,"%.32f") + "f", \
		Param3=string(Str,"%.32f") + "," + string(Soft,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}

function SuperResXbrResizeFinal(clip cmd, clip Input, int fWidth, int fHeight, float b, float c, PrecisionIn)
{
	InputWidth = Input.Width / PrecisionIn
	fWidth = fWidth > 0 ? fWidth : InputWidth
	fHeight = fHeight > 0 ? fHeight : Input.Height

	return cmd.Shader("Bicubic.cso", \
		Param0=string(fWidth) + "," + string(fHeight) + "," + string(1./fWidth,"%.32f") + "," + string(1./fHeight,"%.32f") + "f",\
		Param1=string(InputWidth*2) + "," + string(Input.Height*2) + "," + string(1./(InputWidth*2),"%.32f") + "," + string(1./(Input.Height*2),"%.32f") + "f",\
		Param2=string(b, "%.32f") + "," + string(c, "%.32f") + "f",\
		Width=fWidth, Height=fHeight)
}


function SuperXBR(clip Input, float "Str", float "Sharp", string "MatrixIn", string "MatrixOut", string "FormatOut", bool "Convert", bool "lsb_in", bool "lsb_out")
{
	Str = default(Str, 1)
	Sharp = default(Sharp, 1)
	Convert = default(Convert, true)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	FormatOut = default(FormatOut, "")
	lsb_in = default(lsb_in, false)
	lsb_out = default(lsb_out, false)

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
	args_string = string(Str,"%.32f") + "," + string(Sharp,"%.32f") + ",0,0f"
	size0_string = string(InputWidth) + "," + string(InputHeight) + "," + string(1./InputWidth,"%.32f") + "," + string(1./InputHeight,"%.32f") + "f"
	size1_string = string(2*InputWidth) + "," + string(2*InputHeight) + "," + string(1./(2*InputWidth),"%.32f") + "," + string(1./(2*InputHeight),"%.32f") + "f"
    
	ConvertYuv ? Shader(MatrixIn=="601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso") : nop

	Shader("SuperXBR-pass0.cso",\
		Param2 = args_string,\
		Param3 = size0_string,\
		Width = 2*InputWidth, Height = 2*InputHeight)

	Shader("SuperXBR-pass1.cso",\
		Param2=args_string,\
		Param3=size1_string)
        
	Shader("SuperXBR-pass2.cso",\
		Param2=args_string,\
		Param3=size1_string)

	ConvertYuv ? Shader(MatrixIn=="601" ? "GammaToYuv601.cso" : "GammaToYuv.cso") : nop
	last.ExecuteShader(Input, Precision=2, Clip1Precision=PrecisionIn, OutputPrecision=PrecisionOut)

	convert ? ConvertFromShader(PrecisionOut, Format=sourceFormat, lsb=lsb_out) : last
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