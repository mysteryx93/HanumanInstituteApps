# mClean spatio/temporal denoiser
# Version: 3.2 (01 March 2018)
# By burfadel

#  +++ Description +++
# Typical spatial filters work by removing large variations in the image on a small scale, reducing noise but also making the image less
# sharp or temporally stable. mClean removes noise whilst retaining as much detail as possible, as well as provide optional image enhancement

# mClean works primarily in the temporal domain, although there is some spatial limiting
# Chroma is processed a little differently to luma for optimal results
# Input must be 8-bit Planar type (YV12, YV16, YV24) or their equivalents in 10, 12, 14, or 16 bits
# Chroma processing can be disabled with chroma=false

#  +++ Artifacts +++
# Spatial picture artifacts may remain as removing them is a fine balance between removing the unwanted artifact whilst not removing detail
# Additional dering/dehalo/deblock filters may be required, but should ONLY be uses if required due the detail loss/artifact removal balance

#  +++ Sharpening +++
# Applies a modified unsharp mask to edges and major detected detail. Range of normal sharpening is 0-20, the default 10. There are 4 additional
# settings, 21-24 that provide 'overboost' sharpening. Overboost sharpening is only suitable typically for high definition, high quality sources.
# Actual sharpening calculation is scaled based on resolution.

# +++ ReNoise +++
# ReNoise adds back some of the removed luma noise. Re-adding original noise would be counterproductive, therefore ReNoise modifies this noise
# both spatially and temporally. The result of this modification is the noise becomes much nicer and it's impact on compressibility is greatly
# reduced. It is not applied on areas where the sharpening occurs as that would be counterproductive. Settings range from 1 to 20, default
# value is 14. The strength of renoise is affected by the the amount of original noise removed and how this noise varies between frames. It's
# main purpose is to reduce the 'flatness' that occurs with any form of effective denoising.

# +++ Deband +++
# This will perceptibly improve the quality of the image by reducing banding effect and adding a small amount of temporally stabilised grain to
# both luma and chroma. The settings are not adjustable as the default settings are suitable for most cases without having a large effect on
# compressibility. Auto balance uses Autoadjust, it calculates statistics of the clip, stabilises temporally and adjusts luminance gain & colour
# balance of the noise reduced clip.
# 0=disabled, 1=deband only, 2=auto balance only, 3=both deband and auto balance, 4=deband and veed, 5=all

# +++ Depth +++
# This applies a modified warp sharpening on the image that may be useful for certain things, and can improve the perception of image depth. Default
# is 0 (disabled), and ranges up to 5. This function will distort the image, for animation a setting of 1 or 2 can be beneficial to improve lines. The
# effect

# +++ Strength +++
# The strength of the denoising effect can be adjusted using this parameter. It ranges from 20 percent denoising effect with strength 1, up to the
# 100 percent of the denoising with strength 20 (default). This function works by blending a scaled percentage of the original image with the processed
# image.

# +++ Outbits +++
# Specifies the bits per component (bpc) for the output for processing by additional filters. It will also be the bpc that mClean will process.
# By default, mClean processes as 12 bits if the input is 8 bit, and converts back to 8 bit. If the input is 10 bits or higher no conversion is
# done unless outbits is specified and is different to the input bpc. If you output at a higher bpc keep in mind that there may be limitations
# to what subsequent filters and the encoder may support.

#  +++ Required plugins +++
# Latest RGTools, MVTools2, Masktools2, f3kdb, Modplus, AutoAdjust
# Refer to https://forum.doom9.org/showpost.php?p=1834698&postcount=334

function mClean(clip c, int "thSAD", bool "chroma", int "sharp", int "rn", int "deband", int "depth", float "strength", int "outbits")
{

    defH        = Max (C.Height, C.Width/4*3)   # Resolution calculation for auto blksize settings
    thSAD       = Default (thSAD, 400)   # Denoising threshold
    chroma      = Default (chroma, true)   # Process chroma
    sharp       = Default (sharp, 10)   # Sharp multiplier
    rn          = Default (rn, 14)   # Luma ReNoise strength from 0 (disabled) to 20
    deband      = Default (deband, 4)   # Apply deband/veed and/or auto balance
    depth       = Default (depth, 0)   # Depth enhancement
    strength    = Default (strength, 20)   # Strength of denoising.
    outbits     = Default (outbits, BitsPerComponent(c))   # Output bits, default input depth
    calcbits    = BitsPerComponent(c) == 8 ? 12 : outbits

    Assert(isYUV(c)==true, """mClean: Supports only YUV formats (YV12, YV16, YV24)""")
    Assert(isYUY2(c)==false, """mClean: Supports only YUV formats (YV12, YV16, YV24)""")
    Assert(isYV411(c)==false, """mClean: Supports only YUV formats (YV12, YV16, YV24)""")
    Assert(sharp>=0 && sharp<=24, """mClean: "sharp" ranges from 0 to 24""")
    Assert(rn>=0 && rn<=20, """mClean: "rn" ranges from 0 to 20""")
    Assert(deband>=0 && deband<=5, """mClean: deband options 0 (disabled) to 5. Refer to description""")
    Assert(depth>=0 && depth<=5, """mClean: depth ranges from 0 (disabled) to 5""")
    Assert(strength>0 && depth<=20, """mClean: strength ranges from 1 (20%) to 20 (100%, default)""")
    Assert(outbits>=8 && outbits<=16, """mClean: "outbits" ranges from 8 to 16""")

padX       =  c.width%8 == 0 ? 0 : (16 - c.width%8)
padY       =  c.height%8 == 0 ? 0 : (16 - c.height%8)
c          =  padX+padY<>0 ? c.addborders(0, 0, padX, padY) : c
cy         =  ExtractY(c)
sc         =  defH>2800 ? 8 : defH>1400 ? 4 : defH>720 ? 2 : 1
blksize    =  sc==8 ? 8 : ((defH/sc)/360)>1.5 ? 16 : ((defH/sc)/360)>0.8 ? 12 : 8
overlap    =  blksize>=12 ? 6 : 2
lambda     =  775*(blksize*blksize)/64
sharp      =  sharp>20 ? sharp+30 : DefH<=2600 ? 16+round(defH*(34/2600)*sharp/20) : 50
depth      =  depth*2
depth2     =  -(depth+(depth/2))


# Denoise preparation
c           =  chroma ? Median (c, yy=false, uu=true, vv=true) : c

# Temporal luma noise filter
fvec1       =  bitspercomponent(c)>8 ? convertbits(c, 8) : undefined()
bvec1       =  bitspercomponent(cy)>8 ? convertbits(cy, 8) : undefined()
super       =  MSuper (BicubicResize(chroma ? defined(fvec1) ? fvec1 : c : defined(bvec1) ? bvec1 : cy, c.Width/sc, c.Height/sc),
            \  hpad=16/sc, vpad=16/sc, rfilter=4)
super2      =  MSuper (chroma ? defined(fvec1) ? fvec1 : c : defined(bvec1) ? bvec1 : cy, hpad=16, vpad=16, levels=1)

# --> Analysis
bvec4       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = true, delta = 4, blksize=blksize, overlap=overlap), sc),
            \  blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)
bvec3       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = true, delta = 3, blksize=blksize, overlap=overlap), sc),
            \  blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)
bvec2       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = true, delta = 2, blksize=blksize, overlap=overlap,
            \  badSAD=1100, lsad=1120), sc), searchparam=3, blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)
bvec1       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = true, delta = 1, blksize=blksize, overlap=overlap, badSAD=1500, badrange=27,
            \  search=5, lsad=980), sc), blksize=blksize, overlap=overlap, search=5, searchparam=3, lambda=lambda, thSAD=180)
fvec1       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = false, delta = 1, blksize=blksize, overlap=overlap, badSAD=1500, badrange=27,
            \  search=5, lsad=980), sc), blksize=blksize, overlap=overlap, search=5, searchparam=3, lambda=lambda, thSAD=180)
fvec2       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = false, delta = 2, blksize=blksize, overlap=overlap,
            \  badSAD=1100, lsad=1120), sc), searchparam=3, blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)
fvec3       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = false, delta = 3, blksize=blksize, overlap=overlap), sc),
            \  blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)
fvec4       =  MRecalculate(super2, MscaleVect (MAnalyse (super, isb = false, delta = 4, blksize=blksize, overlap=overlap), sc),
            \  blksize=blksize, overlap=overlap, lambda=lambda, thSAD=180)

# --> Bit depth conversion
c           =  chroma ? calcbits != BitsPerComponent(c) ? ConvertBits(c, calcbits) : c : c
super2      =  calcbits != BitsPerComponent(super2) ? ConvertBits(super2, calcbits) : super2
cy          =  calcbits != BitsPerComponent(cy) ? ConvertBits(cy, calcbits) : cy

# --> Applying cleaning
clean       =  MDegrain4(chroma ? c : cy, super2, bvec1, fvec1, bvec2, fvec2, bvec3, fvec3, bvec4, fvec4, thSAD=thSAD)
u           =  chroma ? ExtractU(clean) : nop ()
v           =  chroma ? ExtractV(clean) : nop ()
filt_chroma =  chroma ? CombinePlanes(c, mt_adddiff(u, clense(mt_makediff(ExtractU(c), u), reduceflicker=true)), mt_adddiff(v,
            \  clense(mt_makediff(ExtractV(c), v), reduceflicker=true)), planes="yuv", source_planes="yyy", sample_clip=c) : c
clean       =  chroma ? ExtractY(clean) : clean

# Post clean, pre-process deband
filt_chroma_bits =  BitsPerComponent(filt_chroma)
clean2           =  deband==0 ? nop() : ConvertBits(clean, 8)
noise_diff       =  deband==0 ? nop() : BitsPerComponent(c)==8 ? nop() : mt_makediff(convertbits(clean2, calcbits), clean)
depth_calc       =  deband==0 ? nop() : CombinePlanes (clean2, filt_chroma_bits>8 ? ConvertBits(filt_chroma, 8) : filt_chroma, planes="YUV",
                 \  source_planes="YUV", pixel_type="YV12")
depth_calc       =  deband==0 ? nop() : deband>1 ? deband==4 ? depth_calc : AutoAdjust (depth_calc, auto_gain=true, bright_limit=1.09, dark_limit=1.11,
                 \  gamma_limit=1.045, auto_balance=true, chroma_limit=1.13, chroma_process=115, balance_str=0.85) : depth_calc
depth_calc       =  deband==0 ? undefined() : deband<>2 ? f3kdb (depth_calc, preset=chroma?"high":"luma", range=16, grainY=38*(defH/540),
                 \  grainC=chroma?37*(defH/540):0) :depth_calc
clean            =  deband==0 ? clean : BitsPerComponent(c)==8 ? ExtractY (depth_calc) : mt_adddiff(ConvertBits(ExtractY
                 \  (depth_calc), calcbits), noise_diff)
depth_calc       =  deband==0 ? nop() : BitsPerComponent(depth_calc)<>filt_chroma_bits ? ConvertBits(depth_calc, filt_chroma_bits) : depth_calc
filt_chroma      =  deband==0 ? filt_chroma : deband>4 ? veed(depth_calc) : depth_calc

# Spatial luma denoising
clean2      =  removegrain(clean, 18)

# Unsharp filter for spatial detail enhancement
clsharp     =  sharp>0 ? sharp>=51<=54 ? mt_makediff(clean, gblur(clean2, (sharp-50), sd=3)) :
            \  mt_makediff(clean, blur(clean2, 1.58*(0.03+(0.97/50)*sharp))) : nop()
clsharp     =  mt_adddiff(clean2, repair(clense(clsharp), clsharp, 12))

# If selected, combining ReNoise
noise_diff  =  mt_makediff (clean2, cy)
clean2      =  rn>0<=20 ? mt_merge(clean2, mergeluma (clean2, mt_adddiff(clean2, tweak(clense(noise_diff, reduceflicker=true), cont=1.008+(0.0032*(rn/20)))),
            \  0.3+(rn*0.035)), mt_lut (overlay(clean, invert(clean), mode="darken"), "x 32 scaleb < 0 x 45 scaleb > range_max 0 x 35 scaleb - range_max 32
            \  scaleb 65 scaleb - / * - ? ?")) : clean2

# Combining spatial detail enhancement with spatial noise reduction using prepared mask
noise_diff  =  mt_invert(mt_binarize(noise_diff))
clean2      =  sharp>0 ? mt_merge (clean2, clsharp, overlay(noise_diff, mt_edge(clean, "prewitt"), mode="lighten")) :
            \  mt_merge (clean2, clean, overlay(noise_diff, mt_edge(clean, "prewitt"), mode="lighten"))

# Converting bits per channel and luma format
filt_chroma =  outbits < BitsPerComponent(filt_chroma) ? ConvertBits(filt_chroma, outbits, dither=1) : ConvertBits(filt_chroma, outbits)
clean2      =  outbits < BitsPerComponent(clean2) ? ConvertBits(clean2, outbits, dither=1) : ConvertBits(clean2, outbits)
c           =  BitsPerComponent(c) <> BitsPerComponent(clean2) ? ConvertBits(c, BitsPerComponent(clean2)) : c

# Combining result of luma and chroma cleaning
output      =  CombinePlanes(clean2, filt_chroma, planes="YUV", source_planes="YUV", sample_clip=c)
output      =  strength<20 ? Merge(c, output, 0.2+(0.04*strength)) : output
depth_calc  =  depth>0 ? defh>640 ? bicubicresize(output, 720, 480) : output : nop()
output      =  depth>0 ? mt_adddiff(output, spline36resize(mt_makediff(awarpsharp2(depth_calc, depth=depth2, blur=3),
            \  awarpsharp2(depth_calc, depth=depth, blur=2)), output.width, output.height)) : output
output      =  padX+padY<>0 ? output.crop(0, 0, -padX, -padY) : output

return output
}