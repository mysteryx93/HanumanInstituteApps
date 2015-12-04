function SuperRes(clip Input, int "Passes", float "Strength", float "Softness", string "UpscaleCommand", bool "SrcMatrix601", int "PrecisionIn")
{
	Passes = default(Passes, 1)
	Strength = default(Strength, 1)
	Softness = default(Softness, 0)
	SrcMatrix601 = default(SrcMatrix601, false)
	PrecisionIn = default(PrecisionIn, 0)
	convert = PrecisionIn==0

	Assert((Passes > 0 && Passes <= 3) ? true : false, "Passes must be between 1 and 3")
	Assert((Strength >= 0 && Strength <= 1) ? true : false, "Strength must be between 0 and 1")
	Assert((Softness >= 0 && Softness <= 1) ? true : false, "Softness must be between 0 and 1")
	Assert(Defined(UpscaleCommand), "You must specify UpscaleCommand")
	Assert(PrecisionIn >= 0 && PrecisionIn <= 3, "PrecisionIn must be between 0 and 3")

	Input

	ConvertYuv = convert && !IsRGB()
	sourceFormat = IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	Original = convert ? ConvertToShader(precision=1) : last
	Eval(UpscaleCommand)
	Input = convert ? ConvertToShader(precision=1) : last
	Input.Shader(ConvertYuv && SrcMatrix601 ? "Yuv601ToLinear.cso" : ConvertYuv ? "YuvToLinear.cso" : "GammaToLinear.cso")
	SuperResPass(last, Input, Original, Strength, Softness, 1, Passes, ConvertYuv, SrcMatrix601, PrecisionIn)
	Passes > 1 ? SuperResPass(Input, Original, Strength, Softness, 2, Passes, ConvertYuv, SrcMatrix601, PrecisionIn) : nop
	Passes > 2 ? SuperResPass(Input, Original, Strength, Softness, 3, Passes, ConvertYuv, SrcMatrix601, PrecisionIn) : nop
	!convert && SrcMatrix601 ? Shader("Yuv601ToGamma.cso").Shader("GammaToYuv.cso") : last

	ExecuteShader(last, Input, Original, precision=3, PrecisionIn=convert?1:PrecisionIn, precisionOut=convert?1:PrecisionIn)
	convert ? ConvertFromShader(format=sourceFormat, precision=1) : last
}

function SuperResPass(clip cmd, clip Input, clip Original, float Strength, float Softness, int Pass, int Passes, bool ConvertYuv, bool SrcMatrix601, int PrecisionIn)
{
	InputWidth = Input.Width / ((PrecisionIn==0 || PrecisionIn==1) ? 1 : 2)
	OriginalWidth = Original.Width / ((PrecisionIn==0 || PrecisionIn==1) ? 1 : 2)

	cmd.Shader("Bicubic.cso", Output=3, \
		Param0=string(OriginalWidth) + "," + string(Original.Height) + "," + string(1./OriginalWidth,"%.32f") + "," + string(1./Original.Height,"%.32f") + "f",\
		Param1=string(InputWidth) + "," + string(Input.Height) + "," + string(1./InputWidth,"%.32f") + "," + string(1./Input.Height,"%.32f") + "f",\
		Param2=string(1/3.,"%.32f") + "," + string(1/3.,"%.32f") + "f",\
		Width=OriginalWidth, Height=Original.Height)

	Shader(ConvertYuv && SrcMatrix601 ? "SuperResDiff601.cso" : ConvertYuv ? "SuperResDiff709.cso" : "SuperResDiff.cso", Clip1=3, Clip2=2, Output=4)

	Shader(Pass==Passes ? (ConvertYuv ? "SuperResFinal709.cso" : "SuperResFinal.cso") : "SuperRes.cso", Clip1=1, Clip2=4, Output=1, \
		Param0=string(InputWidth) + "," + string(Input.Height) + "f", \
		Param1=string(1./InputWidth, "%.32f") + "," + string(1./Input.Height, "%.32f") + "f", \
		Param2=string(OriginalWidth) + "," + string(Original.Height) + "," + string(1./(OriginalWidth),"%.32f") + "," + string(1./Original.Height,"%.32f") + "f", \
		Param3=string(Strength,"%.32f") + "," + string(Softness,"%.32f") + "," + string(Pass) + "," + string(Passes) + "f")
}