###########################################################################
# DO NOT OPEN THIS DOCUMENT IN GOOGLE TRANSLATE OR BAD THINGS WILL HAPPEN #
###########################################################################
# General info:
#    1. Do not include core filters in the list
#    2. Specify the lowest mode that produces correct output with any settings. 
#        For example if something works with MT_MULTI_INSTANCE on some parameters and MT_SERIALIZED with other, MT_SERIALIZED should be used
#        You can of course specify this in comments so people who do want to get MAXIMUM PERFORMANCE can modify the script themselves.
#    3. Do not specify MT modes for runtime filters. Behaviour of runtime filtering with mt is not very well known yet.
#    4. In case it isn't obvious, NICE_FILTER - mt mode 1, MULTI_INSTANCE - mt mode 2, SERIALIZED - mt mode 3. Variables are defined by avs+ core.
##################################################################################
#Another Lists
#https://gist.github.com/tp7/8899021
##################################################################################
# Official avs+ filters. Newer versions will register themself so you should never force any of these modes
############################################
#rgtools
SetFilterMTMode("DEFAULT_MT_MODE",     MT_MULTI_INSTANCE)

SetFilterMTMode("removegrain",         MT_NICE_FILTER)
SetFilterMTMode("repair",              MT_NICE_FILTER)
SetFilterMTMode("verticalcleaner",     MT_NICE_FILTER)
SetFilterMTMode("clense",              MT_NICE_FILTER)
#medianblur   
SetFilterMTMode("medianblur",          MT_MULTI_INSTANCE)
SetFilterMTMode("medianblurtemporal",  MT_MULTI_INSTANCE)
#masktools   
SetFilterMTMode("mt_invert",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_binarize",         MT_MULTI_INSTANCE)
SetFilterMTMode("mt_inflate",          MT_MULTI_INSTANCE)
SetFilterMTMode("mt_deflate",          MT_MULTI_INSTANCE)
SetFilterMTMode("mt_inpand",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_expand",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lut",              MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lutxy",            MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lutxyz",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_luts",             MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lutf",             MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lutsx",            MT_MULTI_INSTANCE)
SetFilterMTMode("mt_lutspa",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_merge",            MT_MULTI_INSTANCE)
SetFilterMTMode("mt_logic",            MT_MULTI_INSTANCE)
SetFilterMTMode("mt_convolution",      MT_MULTI_INSTANCE)
SetFilterMTMode("mt_mappedblur",       MT_MULTI_INSTANCE)
SetFilterMTMode("mt_makediff",         MT_MULTI_INSTANCE)
SetFilterMTMode("mt_average",          MT_MULTI_INSTANCE)
SetFilterMTMode("mt_adddiff",          MT_MULTI_INSTANCE)
SetFilterMTMode("mt_clamp",            MT_MULTI_INSTANCE)
SetFilterMTMode("mt_motion",           MT_MULTI_INSTANCE)
SetFilterMTMode("mt_edge",             MT_MULTI_INSTANCE)
SetFilterMTMode("mt_hysteresis",       MT_MULTI_INSTANCE)
SetFilterMTMode("sangnom2",            MT_SERIALIZED) #can run with 2 but you should specify threads=1 then, otherwise too many threads will get created without any benefit
SetFilterMTMode("average",             MT_NICE_FILTER)
SetFilterMTMode("TMaskCleaner",        MT_MULTI_INSTANCE)
SetFilterMTMode("checkmate",           MT_NICE_FILTER)
SetFilterMTMode("Deblock",             MT_NICE_FILTER)
SetFilterMTMode("msharpen",            MT_MULTI_INSTANCE)
SetFilterMTMode("TColorMask",          MT_NICE_FILTER)
SetFilterMTMode("Vinverse",            MT_MULTI_INSTANCE)
SetFilterMTMode("Vinverse2",           MT_MULTI_INSTANCE)

#tp7's update: removed sangnom2 and vinverse lines since they cannot run with mode 1 correctly (updated versions can but those will register themselves anyway), also removed core filters
 
#########################################
# Core filters (because putting it here is easier than modifying the source and firing a PR)
SetFilterMTMode("LanczosResize",       MT_SERIALIZED) #Tested On 2016/03/06
##################################
 
 
#########################################
# External filters (guessed, might not be very stable)
##################################
SetFilterMTMode("dfttest",             MT_SERIALIZED)   #same story as with sangnom2
SetFilterMTMode("AddGrainC",           MT_MULTI_INSTANCE) 
SetFilterMTMode("FFT3dGPU",            MT_SERIALIZED)

#############################################
# External Filters (tested by Reel.Deal)
#############################################
# Testing info:
#    1. All processing plugins have been tested with ColorBars(pixel_type="YV12") as the source. Plugins that do not support YV12 have been tested accordingly.
#    2. AVSMeter is used to check optimal performance and VDub/MPC-HC is used to check correct output.
#############################################
# Source plugins
SetFilterMTMode("DGSource",            MT_SERIALIZED)     #DGDecNV           2046    2014/01/01 - GPU decoding
SetFilterMTMode("DSS2",                MT_SERIALIZED)     #DSS2          2.0.0.13   2014/11/10
SetFilterMTMode("FFAudioSource",       MT_SERIALIZED)     #FFMS2            V2.22   2015/10/12
SetFilterMTMode("FFImageSource",       MT_SERIALIZED)     #FFMS2            V2.22   2015/10/12
SetFilterMTMode("FFFFmpegSource2",     MT_SERIALIZED)     #FFMS2            V2.22   2015/10/12
SetFilterMTMode("FFMS2",               MT_SERIALIZED)     #FFMS2            V2.22   2015/10/12
SetFilterMTMode("FFVideoSource",       MT_SERIALIZED)     #FFMS2            V2.22   2015/10/12
SetFilterMTMode("FRIMSource",          MT_SERIALIZED)     #FRIMSource       V1.26   2016/01/16
SetFilterMTMode("JpegSource",          MT_SERIALIZED)     #JpegSource       --->    2014/04/19
SetFilterMTMode("LSMASHAudioSource",   MT_SERIALIZED)     #LSMASHSource     r877    2016/03/09
SetFilterMTMode("LSMASHVideoSource",   MT_SERIALIZED)     #LSMASHSource     r877    2016/03/09
SetFilterMTMode("LWLibavAudioSource",  MT_SERIALIZED)     #LSMASHSource     r877    2016/03/09
SetFilterMTMode("LWLibavVideoSource",  MT_SERIALIZED)     #LSMASHSource     r877    2016/03/09
SetFilterMTMode("RawSource",           MT_SERIALIZED)     #RawSource26      2046    2013/08/26 
 
# Processing plugins
SetFilterMTMode("aBlur",               MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Odd, with certain Prefetch values mode 1 is slightly better with other values mode 2 is slightly better???
SetFilterMTMode("aSobel",              MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Same as aBlur
SetFilterMTMode("aWarp",               MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Same as aBlur
SetFilterMTMode("aWarp4",              MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Mode 1/2/3 perform very similar and have no real inprovement over single threaded. 
SetFilterMTMode("aWarpSharp",          MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Same as aBlur
SetFilterMTMode("aWarpSharp2",         MT_NICE_FILTER)    #aWarpSharp2       --->    2012/03/28 - Same as aBlur
SetFilterMTMode("DeCross",             MT_SERIALIZED)     #DeCross           v0.0.02 2004/02/01 - Mode 1/2 hang up and crash. Mode 3 with Prefetch >1 is much less effecient than single threaded.
SetFilterMTMode("frfun7",              MT_MULTI_INSTANCE) #frfun7            rev6    2006/05/10 - With mode 1 the output is corrupted. This plugin works well by itself but it's really slow with other filters. (2013 version seems to work as mode 1)
SetFilterMTMode("gradfun2db",          MT_MULTI_INSTANCE) #gradfun2db        v1.0    2006/03/15 - With mode 1 the output is corrupted, random artifacts when playing and/or seeking.
SetFilterMTMode("MosquitoNR",          MT_SERIALIZED)     #MosquitoNR        v0.10   2013/03/14 - It also works with mode 2, but you have to set MosquitoNR(threads=1) so it does not create any unnecessary threads.
SetFilterMTMode("StaticNoiseC",        MT_NICE_FILTER)    #StaticNoiseC      --->    2011/01/08 - Mode 1/2 performance is almost identical. 
SetFilterMTMode("SVSuper",             MT_MULTI_INSTANCE)    #SVPflow       4.0.0.128   2016/02/12 - Documentation states "all filters now compatible with MT mode 1"
SetFilterMTMode("SVAnalyse",           MT_MULTI_INSTANCE)    #SVPflow       4.0.0.128   2016/02/12 - Documentation states "all filters now compatible with MT mode 1"
SetFilterMTMode("SVSmoothFps",         MT_MULTI_INSTANCE)    #SVPflow       4.0.0.128   2016/02/12 - Documentation states "all filters now compatible with MT mode 1"
SetFilterMTMode("tcanny",              MT_MULTI_INSTANCE) #tcanny            v1.0    2009/06/03 - With mode 1 the output is corrupted, random artifacts when playing and/or seeking.
SetFilterMTMode("tdtrans",             MT_MULTI_INSTANCE) #tcanny            v1.0    2009/06/03 - Same as tcanny
SetFilterMTMode("TCannyMod_TCannyMod", MT_NICE_FILTER)    #TCannyMod         v1.1.1  2016/04/10
SetFilterMTMode("TCannyMod_GBlur",     MT_NICE_FILTER)    #TCannyMod         v1.1.1  2016/04/10
SetFilterMTMode("TCannyMod_EMask",     MT_NICE_FILTER)    #TCannyMod         v1.1.1  2016/04/10
SetFilterMTMode("TEdgeMask",           MT_MULTI_INSTANCE) #TEdgeMask         v0.9    2005/01/20 - Same as tcanny
SetFilterMTMode("TEMmod",              MT_MULTI_INSTANCE) #TEMmod            v0.2.0  2013/08/12 - Same as tcanny
SetFilterMTMode("TMM",                 MT_MULTI_INSTANCE) #TMM               v1.0    2007/04/06 - Mode 1 randomly crashes and mode 2/3 are not very efficient.
SetFilterMTMode("TNLMeans",            MT_MULTI_INSTANCE) #TNLMeans          v1.0.3  2007/08/28 - Mode 1 is extremely slow
SetFilterMTMode("UnsharpHQ",           MT_MULTI_INSTANCE) #UnsharpHQ         v0.5    2014/01/19 - Mode 1 creates a few extra threads without any benefit.
SetFilterMTMode("VideoScope",          MT_NICE_FILTER)    #VideoScope        v1.2    2004/05/01 - Mode 1/2 performance is almost identical.
SetFilterMTMode("flash3kyuu_deband",   MT_MULTI_INSTANCE) #flash3kyuu_deband v1.5.1  2012/04/07 - Also works with mode 1 if set mt=False
# flash3kyuu_deband 2.0pre2 2012/12/03 - Crashes with mode 1 even with mt=False
#########################################
# Filters from Dither 1.25.0. Tested by Firesledge (not extensively though)
SetFilterMTMode("DitherPost",          MT_NICE_FILTER)
SetFilterMTMode("SmoothGrad",          MT_NICE_FILTER)
SetFilterMTMode("Dither_box_filter16", MT_NICE_FILTER)
SetFilterMTMode("Dither_bilateral16",  MT_NICE_FILTER)
SetFilterMTMode("Dither_limit_dif16",  MT_NICE_FILTER)
SetFilterMTMode("Dither_resize16",     MT_NICE_FILTER)
SetFilterMTMode("Dither_out",          MT_NICE_FILTER)
SetFilterMTMode("Dither_removegrain16",MT_NICE_FILTER)
SetFilterMTMode("Dither_repair16",     MT_NICE_FILTER)
SetFilterMTMode("Dither_median16",     MT_NICE_FILTER)
SetFilterMTMode("Dither_add16",        MT_NICE_FILTER)
SetFilterMTMode("Dither_sub16",        MT_NICE_FILTER)
SetFilterMTMode("Dither_max_dif16",    MT_NICE_FILTER)
SetFilterMTMode("Dither_merge16",      MT_NICE_FILTER)
#########################################
SetFilterMTMode("DGDecode_mpeg2source", MT_SERIALIZED)    #seems to work fine as 1 although MT_SERIALIZED is recommended for almost all source filters
SetFilterMTMode("ColorMatrix",          MT_NICE_FILTER)    #read note2
SetFilterMTMode("NNEDI3",               MT_MULTI_INSTANCE)
SetFilterMTMode("eDeen",                MT_SERIALIZED)     #1 and 2 gave errors
SetFilterMTMode("Deen",                 MT_SERIALIZED)     #2 randomly gave errors, or corrupted output
SetFilterMTMode("VagueDenoiser",        MT_MULTI_INSTANCE)
SetFilterMTMode("TFM",                  MT_MULTI_INSTANCE) #2 is faster. 1 crashes randomly.
SetFilterMTMode("TDecimate",            MT_SERIALIZED)     #1 gave error, 2 was slower than 3
SetFilterMTMode("FTurnLeft",            MT_NICE_FILTER)    #FTurn is deprecated since it's already included in the avs+ core
SetFilterMTMode("FTurnRight",           MT_NICE_FILTER)    #FTurn is deprecated since it's already included in the avs+ core
SetFilterMTMode("nnedi3ocl",            MT_MULTI_INSTANCE)
SetFilterMTMode("Yadif",                MT_MULTI_INSTANCE)
#note1: Can't use mode 1 or 2 on any filters if encoding 1080i files(480i works perfectly fine).
#FFVideoSource runs a few seconds, however, it always crashes.
#As far as I know, the only workaround seems to be running a deinterlacer right after the video source and running it as "MT_SERIALIZED",
#than running any desired filters after. Filters after the deinterlacer, seem to be able use any mode.
#note2: tried multiple files, seems to corrupt video even when it is the only filter in a script.
#tried mode 1, 2, and 3, none worked. however it works fine if MT isn't enabled.
#should ColorMatrix still be in the list? as a notice? or should it be removed?
#########################################
SetFilterMTMode("FRFun3d",             MT_NICE_FILTER)
SetFilterMTMode("ContinuityFixer",     MT_NICE_FILTER)
SetFilterMTMode("ReferenceFixer",      MT_NICE_FILTER)
SetFilterMTMode("SmoothLevels",        MT_NICE_FILTER)
SetFilterMTMode("SmoothTweak",         MT_NICE_FILTER)
SetFilterMTMode("SmoothCurve",         MT_NICE_FILTER)
SetFilterMTMode("SmoothCustom",        MT_NICE_FILTER)
SetFilterMTMode("SmoothTools",         MT_NICE_FILTER)
SetFilterMTMode("SmoothLevels16",      MT_NICE_FILTER)
SetFilterMTMode("SmoothTweak16",       MT_NICE_FILTER)
SetFilterMTMode("SmoothCurve16",       MT_NICE_FILTER)
SetFilterMTMode("SmoothCustom16",      MT_NICE_FILTER)
SetFilterMTMode("SmoothTools16",       MT_NICE_FILTER)
SetFilterMTMode("TDeint",              MT_MULTI_INSTANCE) # Mode 1 creates artifacts
SetFilterMTMode("yadifmod",            MT_NICE_FILTER)
SetFilterMTMode("yadifmod2",           MT_NICE_FILTER)

#########################################
# Filters from MVTools v2.6.0.5 (from http://forum.doom9.org/showthread.php?p=1386559#post1386559 ) Tested On 2016/03/06
SetFilterMTMode("MSuper",              MT_MULTI_INSTANCE)
SetFilterMTMode("MAnalyse",            MT_MULTI_INSTANCE)
SetFilterMTMode("MFlowFps",            MT_MULTI_INSTANCE)
SetFilterMTMode("MRecalculate",        MT_MULTI_INSTANCE)
#########################################
#Filters from EEDI2 v0.9.2 (from http://forum.doom9.org/showthread.php?t=173238 ) Tested On 2016/03/06
SetFilterMTMode("EEDI2",               MT_MULTI_INSTANCE)

SetFilterMTMode("KNLMeansCL",          MT_SERIALIZED)

#keep it the last line please
if (FunctionExists("avstp_set_threads")) {
    # this isn't actually optimal, because it will also disable avstp threads if running 
    # on a single-threaded filter chain
    avstp_set_threads(0, 1)   #disable threading in dither
}