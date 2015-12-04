# Performs color matrix conversion with 16 bit depth to avoid banding
function ColorMatrix601to709(clip input)
{
	input
	srcYuv = !IsRGB
	sourceFormat = IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	input = ConvertToShader(precision=1)
	Shader(srcYuv ? "Yuv601ToGamma.cso" : "GammaToYuv601.cso")
	Shader(srcYuv ? "GammaToYuv.cso" : "YuvToGamma.cso")
	ExecuteShader(last, input, precision=2, precisionIn=1, precisionOut=1)
	ConvertFromShader(format=sourceFormat, precision=1)
}

function ColorMatrix709to601(clip input)
{
	input
	srcYuv = !IsRGB
	sourceFormat = IsYV12 ? "YV12" : IsYV24 ? "YV24" : IsRGB24 ? "RGB24" : IsRGB32 ? "RGB32" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12, YV24, RGB24 or RGB32" + chr(10))

	input = ConvertToShader(precision=1)
	Shader(srcYuv ? "YuvToGamma.cso" : "GammaToYuv.cso")
	Shader(srcYuv ? "GammaToYuv601.cso" : "Yuv601ToGamma.cso")
	ExecuteShader(last, input, precision=2, precisionIn=1, precisionOut=1)
	ConvertFromShader(format=sourceFormat, precision=1)
}