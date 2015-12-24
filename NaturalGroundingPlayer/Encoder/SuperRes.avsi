function SuperRes(clip Input, int "Passes", float "Strength", float "Softness", string "UpscaleCommand", string "MatrixIn", string "MatrixOut", bool "Convert", bool "lsb_in", bool "lsb_upscale", bool "lsb_out")
{
	Passes = default(Passes, 1)
	Strength = default(Strength, 1)
	Softness = default(Softness, 0)
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")
	Convert = default(Convert, true)
	lsb_in = default(lsb_in, false)
	lsb_upscale = default(lsb_upscale, false)
	lsb_out = default(lsb_out, false)

	Assert((Passes > 0 && Passes <= 3) ? true : false, "Passes must be between 1 and 3")
	Assert((Strength >= 0 && Strength <= 1) ? true : false, "Strength must be between 0 and 1")
	Assert((Softness >= 0 && Softness <= 1) ? true : false, "Softness must be between 0 and 1")
	Assert(Defined(UpscaleCommand), "You must specify UpscaleCommand")
	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert((!lsb_in && !lsb_upscale && !lsb_out) || Convert, "Convert must be True to use lsb_in, lsb_upscale or lsb_out")

	Input

	PrecisionIn = Convert && !lsb_in ? 1 : 2
	PrecisionUpscale = Convert && !lsb_upscale ? 1 : 2
	PrecisionOut = Convert && !lsb_out ? 1 : 2
	ConvertYuv = convert && !IsRGB()
	sourceFormat = IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	Original = convert ? ConvertToShader(PrecisionIn, lsb=lsb_in) : last
	Eval(UpscaleCommand)
	Input = convert ? ConvertToShader(PrecisionUpscale, lsb=lsb_upscale) : last
	Input.Shader(ConvertYuv && MatrixIn=="601" ? "Yuv601ToLinear.cso" : ConvertYuv ? "YuvToLinear.cso" : "GammaToLinear.cso")
	SuperResPass(last, Input, Original, Strength, Softness, 1, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut)
	Passes > 1 ? SuperResPass(Input, Original, Strength, Softness, 2, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut) : nop
	Passes > 2 ? SuperResPass(Input, Original, Strength, Softness, 3, Passes, ConvertYuv, Convert, PrecisionIn, PrecisionUpscale, MatrixIn, MatrixOut) : nop

	ExecuteShader(last, Input, Original, Precision=3, Clip1Precision=PrecisionUpscale, Clip2Precision=PrecisionIn, OutputPrecision=PrecisionOut)
	convert ? ConvertFromShader(PrecisionOut, format=sourceFormat, lsb=lsb_out) : last
}

function SuperResPass(clip cmd, clip Input, clip Original, float Strength, float Softness, int Pass, int Passes, bool ConvertYuv, bool Convert, int PrecisionIn, int PrecisionUpscale, string MatrixIn, string MatrixOut)
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
		Param3=string(Strength,"%.32f") + "," + string(Softness,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}