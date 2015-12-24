# Performs color matrix conversion with 16 bit depth to avoid banding
function ColorMatrixShader(clip input, string "MatrixIn", string "MatrixOut") {
	MatrixIn = default(MatrixIn, "709")
	MatrixOut = default(MatrixOut, "709")

	Assert(MatrixIn == "601" || MatrixIn == "709", "MatrixIn must be 601 or 709")
	Assert(MatrixOut == "601" || MatrixOut == "709", "MatrixOut must be 601 or 709")
	Assert(MatrixIn != MatrixOut, "MatrixIn and MatrixOut must be different")

	input
	sourceFormat = IsYV12 ? "YV12" : IsYV24 ? "YV24" : ""
	Assert(sourceFormat != "", chr(10) + "Source must be YV12 or YV24" + chr(10))

	input = ConvertToShader(1)
	Shader(MatrixIn == "601" ? "Yuv601ToGamma.cso" : "YuvToGamma.cso")
	Shader(MatrixOut == "601" ? "GammaToYuv601.cso" : "GammaToYuv.cso")
	ExecuteShader(last, input, Precision=2, Clip1Precision=1, OutputPrecision=1)
	ConvertFromShader(1, format=sourceFormat)
}