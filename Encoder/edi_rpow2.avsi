/* edi_rpow2 v1.0

An improved rpow2 function for nnedi3, nnedi3ocl, eedi3, and eedi2.


##### Requirements #####

Plugins:
  Dither
  eedi2
  eedi3
  FTurn
  MaskTools2
  nnedi3
  nnedi3ocl

Scripts:
  ResizeX

Dither is only required if lsb=true.
FTurn is optional but recommended.
MaskTools2 is only required if eedi3's mclip parameter is used.

Color formats:
  Y8, YV12, YV16, YUY2, YV411, YV24, RGB24
   
Using eedi2 on RGB24 isn't recommended as it can cause discoloration.


##### Common Parameters ######

int rfactorX, int rfactorY (defaults: 2, rfactorX)
--------------------------------------------------
The horizontal and vertical image enlargement factors. Must be a power of 2.
This includes 1, meaning no enlargement in that direction.


string edi (default: "nnedi3")
------------------------------
The edi method to use for image enlargement.
Options are "nnedi3", "nnedi3ocl", "eedi3", and "eedi2".


string cshift (default: "")
---------------------------
The resize kernel to use for correcting the image center shift caused by
image enlargement and for scaling to the final output resolution.
It can be any kernel supported by ResizeX, although kernels exclusive to
Dither_resize16 can only be used if lsb=true.


int fwidth, int fheight (defaults: width*rfactorX, height*rfactorY)
-------------------------------------------------------------------
The final output width and height to scale to after image enlargement.
These settings have no effect unless cshift is specified.


int taps (default: undefined)
-----------------------------
The number of taps to use for the cshift kernel.
Only applies to the Blackman, Blackmanminlobe, Lanczos, Sinc, and Spline kernels.


float a1, float a2 (defaults: undefined)
----------------------------------------
If cshift="Bicubic", these set the b and c values.
If cshift="Gauss", a1 sets the p value.


string cplace (default: "MPEG2")
--------------------------------
Specifies the input's chroma placement. Options are "MPEG1" and "MPEG2".
Only applies to formats with subsampled chroma.
Note that only YV12 should be able to have MPEG1 chroma placement.


bool Y, bool U, bool V (defaults: true)
---------------------------------------
Controls whether the specified plane is processed or not.
These settings have no effect for RGB24 input.


bool lsb (default: false)
-------------------------
When set to true, Dither_resize16 will be used to correct the image
center shift if applicable and the output will be in stack16 format.
If the input is RGB24, the output will be in the RGB48Y format used by Dither.
It can be converted back to RGB24 with the following script:

     DitherPost()
     MergeRGB(SelectEvery(3,0),SelectEvery(3,1),SelectEvery(3,2),"RGB24")


bool bordfix (default: true)
----------------------------
nnedi3, nnedi3ocl, and eedi2 cause lines at the edge of the frame to distort slightly.
When set to true, the distortion is avoided by padding the frame with rows of
duplicate pixels before image enlargement and then cropping the padding off afterward.
Defaults to false when edi="eedi3" because it isn't needed when using eedi3.


bool YV12cfix (default: true)
-----------------------------
Enlarging YV12 vertically causes a small chroma shift that's corrected by
resizing the chroma channels even when not correcting the image center shift.
When set to false, the chroma shift won't be corrected, providing
output that hasn't been resized at all after image enlargement.
Only applies when the input is YV12 and the center shift isn't corrected.


##### nnedi3 Parameters ######

int nsize, int nns, int qual, int etype, int pscrn, int threads, int opt, int fapprox
-------------------------------------------------------------------------------------
See nnedi3's documentation for explanations of these parameters.
Like nnedi3_rpow2, defaults are nsize=0, nns=3, and the rest are the same as nnedi3.


##### nnedi3ocl Parameters ######

int nsize, int nns, int qual, int etype
---------------------------------------
Same as nnedi3's parameters.
As of the 2013.12.08-beta version, only nsize=0 is implemented. Other nsize values are ignored.
Like nnedi3x_rpow2, defaults are nsize=0, nns=3, and the rest are the same as nnedi3.


##### eedi3 Parameters ######

float alpha, float beta, float gamma, int nrad, int mdis, bool hp, bool ucubic, bool cost3, int vcheck,
float vthresh0, float vthresh1, float vthresh2, string sclip, string sclip_params, clip mclip, int threads, int opt
-------------------------------------------------------------------------------------------------------------------
See eedi3's documentation for explanations of these parameters.


string sclip (default: "")
--------------------------
Unlike eedi3, the sclip parameter in edi_rpow2 takes a string rather than a clip.
It specifies what resize kernel or edi method to use to generate the sclip.
All of AviSynth's internal resize kernels, ResizeX's bicubic presets,
"nnedi3", "nnedi3ocl", and "eedi2" are supported.


string sclip_params (default: "")
---------------------------------
Used to pass parameters to ResizeX, nnedi3, nnedi3ocl, or eedi2 when generating the sclip.
If nnedi3 or nnedi3ocl is used, it defaults to "nsize=0, nns=3".


clip mclip (default: undefined)
-------------------------------
The mclip should be the same resolution and colorspace as the input, with the exception of Y8 and RGB24.
If the input is Y8 or RGB24, the mclip can be any YUV format, but only its luma channel will be used.
The mclip is automatically scaled to the appropriate resolution for each iteration of image enlargement.


Other parameters are the same as eedi3.


##### eedi2 Parameters ######

int mthresh, int lthresh, int vthresh, int estr, int dstr, int maxd, int map, int nt, int pp
--------------------------------------------------------------------------------------------
See eedi2's documentation for explanations of these parameters.
Defaults are the same as eedi2.
*/

function edi_rpow2(clip input, int "rfactorX", int "rfactorY", string "edi", string "cshift", int "fwidth", int "fheight", 
\                  int "taps", float "a1", float "a2", string "cplace", bool "Y", bool "U", bool "V", bool "lsb", bool "bordfix", bool "YV12cfix",
\                  int "nsize", int "nns", int "qual", int "etype", int "pscrn", int "threads", int "opt", int "fapprox",
\                  float "alpha", float "beta", float "gamma", int "nrad", int "mdis", bool "hp", bool "ucubic", bool "cost3",
\                  int "vcheck", float "vthresh0", float "vthresh1", float "vthresh2", string "sclip", string "sclip_params", clip "mclip",
\                  int "mthresh", int "lthresh", int "vthresh", int "estr", int "dstr", int "maxd", int "map", int "nt", int "pp") {

# Row 1-2: Common parameters
# Row 3:   nnedi3 and nnedi3ocl parameters
# Row 4-5: eedi3 parameters (also threads and opt)
# Row 6:   eedi2 parameters

iw = input.Width()
ih = input.Height()

rfactorX  = Default(rfactorX, 2)
rfactorY  = Default(rfactorY, rfactorX)
edi       = Default(edi, "nnedi3")
cshift    = Default(cshift, "")
fwidth    = Default(fwidth, iw*rfactorX)
fheight   = Default(fheight, ih*rfactorY)
cplace    = Default(cplace, "MPEG2")
Y         = Default(Y, true)
U         = Default(U, true)
V         = Default(V, true)
lsb       = Default(lsb, false)
bordfix   = Default(bordfix, edi != "eedi3" ? true : false)
YV12cfix  = Default(YV12cfix, true)
nsize     = Default(nsize, 0)
nns       = Default(nns, 3)

Assert(rfactorX > 0 && BitAnd(rfactorX,rfactorX-1) == 0, "edi_rpow2: rfactorX must be a power of 2")
Assert(rfactorY > 0 && BitAnd(rfactorY,rfactorY-1) == 0, "edi_rpow2: rfactorY must be a power of 2")
Assert(edi == "nnedi3" || edi == "nnedi3ocl" || edi == "eedi3" || edi == "eedi2", "edi_rpow2: invalid edi setting")
Assert(cplace == "MPEG1" || cplace == "MPEG2", "edi_rpow2: cplace must be MPEG1 or MPEG2")
Assert(!mclip.Defined() || mclip.Defined() && mclip.IsYUV(), "edi_rpow2: eedi3 mclip must be YUV")

# Override the fwidth and fheight parameters if not correcting the center shift
fwidth  = cshift == "" ? iw*rfactorX : fwidth
fheight = cshift == "" ? ih*rfactorY : fheight

# Get the input clip's colorspace
csp = input.IsY8()    ? "Y8"
\   : input.IsYV12()  ? "YV12"
\   : input.IsYV16()  ? "YV16"
\   : input.IsYUY2()  ? "YUY2"
\   : input.IsYV411() ? "YV411"
\   : input.IsYV24()  ? "YV24"
\   : input.IsRGB24() ? "RGB24"
\   : input.IsRGB32() ? Assert(false, "edi_rpow2: RGB32 not supported")
\                     : Assert(false, "edi_rpow2: unsupported colorspace")

# Check for subsampled chroma
hssc12 = csp == "YV12"  || csp == "YV16" || csp == "YUY2" ? true : false
hssc14 = csp == "YV411"  ? true : false
vssc12 = csp == "YV12"   ? true : false
alignc = hssc12 || hssc14 ? true : false

Assert(!hssc12 || fwidth%2  == 0, "edi_rpow2: fwidth of "+csp+" must be a multiple of 2")
Assert(!hssc14 || fwidth%4  == 0, "edi_rpow2: fwidth of "+csp+" must be a multiple of 4")
Assert(!vssc12 || fheight%2 == 0, "edi_rpow2: fheight of "+csp+" must be a multiple of 2")

# Input and output chroma width and height determined by subsampling
iw_c      = hssc12 ? iw/2 : hssc14 ? iw/4 : iw
ih_c      = vssc12 ? ih/2 : ih
fwidth_c  = hssc12 ? fwidth/2 : hssc14 ? fwidth/4 : fwidth
fheight_c = vssc12 ? fheight/2 : fheight

# Center shift correction values
cshiftH   = rfactorX == 1    ?  0
\         : hssc12 || hssc14 ? -0.5*(rfactorX-1)
\                            : -0.5
cshiftV   = rfactorY == 1    ?  0
\                            : -0.5
cshiftH_c = hssc12 ? cshiftH/2.0
\         : hssc14 ? cshiftH/4.0
\                  : cshiftH
cshiftV_c = vssc12 && rfactorY > 1 ? cshiftV/2.0-0.25
\                                  : cshiftV

# Add the MPEG1 or MPEG2 chroma shift correction to the cshiftH_c value
no_MPEG2shift_kernel = cshift.LeftStr(5) == "Point" ||
\                      cshift.LeftStr(4) == "Rect"  ||
\                      cshift.LeftStr(3) == "Box"    ? true : false
MPEG1shift = hssc12 ? -0.25*(rfactorX-1)
\          : hssc14 ? -0.375*(rfactorX-1)
\                   : 0
MPEG2shift = hssc12 ? 0.25*(1.0-Float(iw_c*rfactorX)/Float(fwidth_c))
\          : hssc14 ? 0.375*(1.0-Float(iw_c*rfactorX)/Float(fwidth_c))
\                   : 0
cshiftH_c = cplace == "MPEG1"     ? cshiftH_c+MPEG1shift
\         : cplace == "MPEG2"    &&
\           !no_MPEG2shift_kernel ? cshiftH_c+MPEG2shift
\                                 : cshiftH_c

# Pad the frame with rows of duplicate pixels to support mod1 and mod2
# resolutions with eedi3 and eedi2 since they have to work in YV12.
# Also pad at least four rows if bordfix=true.
iw_ismod8 = iw%8 == 0 ? true : false
ih_ismod8 = ih%8 == 0 ? true : false
iw_ismod4 = iw%4 == 0 ? true : false
ih_ismod4 = ih%4 == 0 ? true : false

pad8 = edi == "eedi3" && hssc14 && !iw_ismod8 ||
\      edi == "eedi3" && hssc14 && !ih_ismod8 ||
\      edi == "eedi2" && hssc14 && !iw_ismod8 ||
\      edi == "eedi2" && hssc14 && !ih_ismod8  ? true : false
pad4 = edi == "eedi3" && csp != "YV12" && !iw_ismod4 ||
\      edi == "eedi3" && csp != "YV12" && !ih_ismod4 ||
\      edi == "eedi2" && csp != "YV12" && !iw_ismod4 ||
\      edi == "eedi2" && csp != "YV12" && !ih_ismod4  ? true : false
pad  = pad8 || pad4 ? true : false

padR = pad8 && !iw_ismod8 ? 8-iw%8
\    : pad4 && !iw_ismod4 ? 4-iw%4
\                         : 0
padT = pad8 && !ih_ismod8 ? 8-ih%8
\    : pad4 && !ih_ismod4 ? 4-ih%4
\                         : 0

padR = padR >= 4                       ? padR
\    : bordfix && rfactorX > 1         ? padR+4
\                                      : padR
padT = padT >= 4                       ? padT
\    : bordfix && rfactorY > 1         ? padT+4
\                                      : padT
padL = bordfix && rfactorX > 1 && pad8 ? 8
\    : bordfix && rfactorX > 1         ? 4
\                                      : 0
padB = bordfix && rfactorY > 1 && pad8 ? 8
\    : bordfix && rfactorY > 1         ? 4
\                                      : 0

padL_c = hssc12 ? padL/2 : hssc14 ? padL/4 : padL
padT_c = vssc12 ? padT/2 : padT
padR_c = hssc12 ? padR/2 : hssc14 ? padR/4 : padR
padB_c = vssc12 ? padB/2 : padB

input = pad || bordfix ? input.ResizeX(iw+padL+padR, ih+padT+padB, -padL, -padT, iw+padL+padR, ih+padT+padB,
\                                      kernel="Point", luma=Y, chroma=U||V?true:false)
\                      : input
mclip = mclip.Defined() && pad     ||
\       mclip.Defined() && bordfix  ? mclip.ResizeX(iw+padL+padR, ih+padT+padB, -padL, -padT, iw+padL+padR, ih+padT+padB,
\                                                   kernel="Point", luma=Y, chroma=U||V?true:false)
\                                   : mclip

# nnedi3 image enlargement
    edi == "nnedi3" && csp == "Y8" ?
\   Eval("""
         pow2  = input.edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
         """)
\ : edi == "nnedi3" && csp == "YV12"  ||
\   edi == "nnedi3" && csp == "YV16"  ||
\   edi == "nnedi3" && csp == "YUY2"  ||
\   edi == "nnedi3" && csp == "YV411" ||
\   edi == "nnedi3" && csp == "YV24"   ?
\   Eval("""
         # For nnedi3 and nnedi3ocl, it's faster to split YV12 channels into separate Y8 clips even though they don't need to be
         pow2Y = input.ConvertToY8().edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
         pow2U = input.UToY8().edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
         pow2V = input.VToY8().edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
         """)
\ : edi == "nnedi3" && csp == "RGB24" ?
\   Eval("""
         # If using FTurn, it's faster to split RGB24 channels into an interleaved Y8 clip because FTurn doesn't support RGB24
         try {
             tft  = BlankClip(pixel_type="Y8").FTurnRight()
             pow2 = Interleave(input.ShowRed("Y8"),input.ShowGreen("Y8"),input.ShowBlue("Y8"))
             pow2 = pow2.edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
             splitRGB24 = true
             }
         catch(err_msg) {
             pow2 = input.edi_rpow2_nnedi3(rfactorX,rfactorY,alignc,true,true,true,nsize,nns,qual,etype,pscrn,threads,opt,fapprox)
             splitRGB24 = false
             }
         """)
\ : NOP()

# nnedi3ocl image enlargement
    edi == "nnedi3ocl" && csp == "Y8" ?
\   Eval("""
         pow2  = input.edi_rpow2_nnedi3ocl(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype)
         """)
\ : edi == "nnedi3ocl" && csp == "YV12"  ||
\   edi == "nnedi3ocl" && csp == "YV16"  ||
\   edi == "nnedi3ocl" && csp == "YUY2"  ||
\   edi == "nnedi3ocl" && csp == "YV411" ||
\   edi == "nnedi3ocl" && csp == "YV24"   ?
\   Eval("""
         pow2Y = input.ConvertToY8().edi_rpow2_nnedi3ocl(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype)
         pow2U = input.UToY8().edi_rpow2_nnedi3ocl(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype)
         pow2V = input.VToY8().edi_rpow2_nnedi3ocl(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype)
         """)
\ : edi == "nnedi3ocl" && csp == "RGB24" ?
\   Eval("""
         pow2  = Interleave(input.ShowRed("Y8"),input.ShowGreen("Y8"),input.ShowBlue("Y8"))
         pow2  = pow2.edi_rpow2_nnedi3ocl(rfactorX,rfactorY,alignc,true,false,false,nsize,nns,qual,etype)
         splitRGB24 = true
         """)
\ : NOP()

# Split the eedi3 mclip
    !mclip.Defined() ?
\   Eval("""
         mclipY = Undefined()
         mclipU = Undefined()
         mclipV = Undefined()
         """)
\ : csp == "Y8" ?
\   Eval("""
         mclip  = mclip.ConvertToYV12()
         """)
\ : csp == "RGB24" ?
\   Eval("""
         mclip  = mclip.ConvertToYV12()
         mclip  = mclip.SelectEvery(3,0,0,0,1,1,1,2,2,2)
         """)
\ : Eval("""
         mclipY = mclip.ConvertToYV12()
         mclipU = mclip.UToY8().ConvertToYV12()
         mclipV = mclip.VToY8().ConvertToYV12()
         """)

# eedi3 image enlargement
    edi == "eedi3" && csp == "Y8" ?
\   Eval("""
         pow2  = input.ConvertToYV12()
         \            .edi_rpow2_eedi3(rfactorX,rfactorY,alignc,true,false,false,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                             vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclip,opt)
         \            .ConvertToY8()
         """)
\ : edi == "eedi3" && csp == "YV12" ?
\   Eval("""
         pow2  = input.edi_rpow2_eedi3(rfactorX,rfactorY,alignc,Y,U,V,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                             vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclipY,opt)
         pow2Y = pow2.ConvertToY8()
         pow2U = pow2.UToY8()
         pow2V = pow2.VToY8()
         """)
\ : edi == "eedi3" && csp == "YV16"  ||
\   edi == "eedi3" && csp == "YUY2"  ||
\   edi == "eedi3" && csp == "YV411" ||
\   edi == "eedi3" && csp == "YV24"   ?
\   Eval("""
         pow2Y = input.ConvertToYV12()
         \            .edi_rpow2_eedi3(rfactorX,rfactorY,alignc,true,false,false,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                             vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclipY,opt)
         \            .ConvertToY8()
         pow2U = input.UToY8().ConvertToYV12()
         \            .edi_rpow2_eedi3(rfactorX,rfactorY,alignc,true,false,false,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                             vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclipU,opt)
         \            .ConvertToY8()
         pow2V = input.VToY8().ConvertToYV12()
         \            .edi_rpow2_eedi3(rfactorX,rfactorY,alignc,true,false,false,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                             vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclipV,opt)
         \            .ConvertToY8()
         """)
\ : edi == "eedi3" && csp == "RGB24" ?
\   Eval("""
         # Always split RGB24 so that the sclip and mclip parameters work
         pow2 = Interleave(input.ShowRed("YV12"),input.ShowGreen("YV12"),input.ShowBlue("YV12"))
         pow2 = pow2.edi_rpow2_eedi3(rfactorX,rfactorY,alignc,true,false,false,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
         \                           vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclip,opt).ConvertToY8()
         splitRGB24 = true
         """)
\ : NOP()

# eedi2 image enlargement
    edi == "eedi2" && csp == "Y8" ?
\   Eval("""
         pow2  = input.ConvertToYV12()
         \            .edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp)
         \            .ConvertToY8()
         """)
\ : edi == "eedi2" && csp == "YV12" ?
\   Eval("""
         pow2  = input.edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp)
         pow2Y = pow2.ConvertToY8()
         pow2U = pow2.UToY8()
         pow2V = pow2.VToY8()
         """)
\ : edi == "eedi2" && csp == "YV16"  ||
\   edi == "eedi2" && csp == "YUY2"  ||
\   edi == "eedi2" && csp == "YV411" ||
\   edi == "eedi2" && csp == "YV24"   ?
\   Eval("""
         pow2Y = input.ConvertToYV12()
         \            .edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp)
         \            .ConvertToY8()
         pow2U = input.UToY8().ConvertToYV12()
         \            .edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp)
         \            .ConvertToY8()
         pow2V = input.VToY8().ConvertToYV12()
         \            .edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp)
         \            .ConvertToY8()
         """)
\ : edi == "eedi2" && csp == "RGB24" ?
\   Eval("""
         pow2  = Interleave(input.ShowRed("YV12"),input.ShowGreen("YV12"),input.ShowBlue("YV12"))
         pow2  = pow2.edi_rpow2_eedi2(rfactorX,rfactorY,alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp).ConvertToY8()
         splitRGB24 = true
         """)
\ : NOP()

# Crop off the padding
    pad     && csp == "Y8"    ||
\   bordfix && csp == "Y8"    ||
\   pad     && csp == "RGB24" ||
\   bordfix && csp == "RGB24"  ?
\   Eval("""
         pow2  = pow2.Crop(padL*rfactorX, padT*rfactorY, -padR*rfactorX, -padB*rfactorY)
         """)
\ : pad || bordfix ?
\   Eval("""
         pow2Y = pow2Y.Crop(padL*rfactorX, padT*rfactorY, -padR*rfactorX, -padB*rfactorY)
         pow2U = pow2U.Crop(padL_c*rfactorX, padT_c*rfactorY, -padR_c*rfactorX, -padB_c*rfactorY)
         pow2V = pow2V.Crop(padL_c*rfactorX, padT_c*rfactorY, -padR_c*rfactorX, -padB_c*rfactorY)
         """)
\ : NOP()

# Blank luma and chroma channels for Y/U/V=false
noY  = input.BlankClip(width=fwidth, height=lsb?fheight*2:fheight, pixel_type="Y8", color_yuv=$808080)
noUV = input.BlankClip(width=fwidth_c, height=lsb?fheight_c*2:fheight_c, pixel_type="Y8", color_yuv=$808080)

# Center shift correction
    cshift != "" && csp == "Y8" ?
\   Eval("""
         shift  = Y ? pow2.ResizeX(fwidth,fheight,cshiftH,cshiftV,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb) : noY
         """)
\ : cshift != "" && csp == "YV12"  ||
\   cshift != "" && csp == "YV16"  ||
\   cshift != "" && csp == "YUY2"  ||
\   cshift != "" && csp == "YV411" ||
\   cshift != "" && csp == "YV24"   ?
\   Eval("""
         shiftY = Y ? pow2Y.ResizeX(fwidth,fheight,cshiftH,cshiftV,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb) : noY
         shiftU = U ? pow2U.ResizeX(fwidth_c,fheight_c,cshiftH_c,cshiftV_c,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb) : noUV
         shiftV = V ? pow2V.ResizeX(fwidth_c,fheight_c,cshiftH_c,cshiftV_c,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb) : noUV
         shift  = YToUV(shiftU,shiftV,shiftY)
         shift  = csp == "YUY2" ? shift.ConvertToYUY2() : shift
         """)
\ : cshift != "" && csp == "RGB24" && !splitRGB24 ?
\   Eval("""
         shift  = lsb ? Interleave(pow2.ShowRed("Y8"),pow2.ShowGreen("Y8"),pow2.ShowBlue("Y8")) : pow2
         shift  = shift.ResizeX(fwidth,fheight,cshiftH,cshiftV,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb)
         """)
\ : cshift != "" && csp == "RGB24" && splitRGB24 ?
\   Eval("""
         shift  = pow2.ResizeX(fwidth,fheight,cshiftH,cshiftV,0,0,cshift,taps,a1,a2, chroma=false, lsb=lsb)
         shift  = !lsb ? MergeRGB(shift.SelectEvery(3,0),shift.SelectEvery(3,1),shift.SelectEvery(3,2),"RGB24") : shift
         """)
\ : NOP()

# No center shift correction
    cshift == "" && csp == "Y8" ?
\   Eval("""
         shift  = Y &&  lsb ? pow2.Dither_convert_8_to_16()
         \      : Y && !lsb ? pow2
         \                  : noY
         """)
\ : cshift == "" && csp == "YV12" && rfactorY == 1 ||
\   cshift == "" && csp == "YV12" && !YV12cfix     ||
\   cshift == "" && csp == "YV16"                  ||
\   cshift == "" && csp == "YUY2"                  ||
\   cshift == "" && csp == "YV411"                 ||
\   cshift == "" && csp == "YV24"                   ?
\   Eval("""
         shiftY = Y &&  lsb ? pow2Y.Dither_convert_8_to_16()
         \      : Y && !lsb ? pow2Y
         \                  : noY
         shiftU = U &&  lsb ? pow2U.Dither_convert_8_to_16()
         \      : U && !lsb ? pow2U
         \                  : noUV
         shiftV = V &&  lsb ? pow2V.Dither_convert_8_to_16()
         \      : V && !lsb ? pow2V
         \                  : noUV
         shift  = YToUV(shiftU,shiftV,shiftY)
         shift  = csp == "YUY2" ? shift.ConvertToYUY2() : shift
         """)
\ : cshift == "" && csp == "YV12" ?
\   Eval("""
         # Even if the center shift isn't corrected,
         # doubling the height of YV12 causes a vertical chroma shift that needs to be corrected
         shiftY = Y &&  lsb ? pow2Y.Dither_convert_8_to_16()
         \      : Y && !lsb ? pow2Y
         \                  : noY
         shiftU = U ? pow2U.ResizeX(fwidth_c,fheight_c,0,-0.25,0,0,"Spline36", chroma=false, lsb=lsb) : noUV
         shiftV = V ? pow2V.ResizeX(fwidth_c,fheight_c,0,-0.25,0,0,"Spline36", chroma=false, lsb=lsb) : noUV
         shift  = YToUV(shiftU,shiftV,shiftY)
         """)
\ : cshift == "" && csp == "RGB24" && !splitRGB24 ?
\   Eval("""
         shift  = lsb ? Interleave(pow2.ShowRed("Y8"),pow2.ShowGreen("Y8"),pow2.ShowBlue("Y8")).Dither_convert_8_to_16() : pow2
         """)
\ : cshift == "" && csp == "RGB24" && splitRGB24 ?
\   Eval("""
         shift  = lsb ? pow2.Dither_convert_8_to_16()
         \            : MergeRGB(pow2.SelectEvery(3,0),pow2.SelectEvery(3,1),pow2.SelectEvery(3,2),"RGB24")
         """)
\ : NOP()

return shift
}

# Recursive functions for repeated edi image doubling
# When calling these functions, the f and t parameters should always be left at their default values.
# They're used only to control behavior during recursion and changing them will cause a malfunction.
function edi_rpow2_nnedi3(clip input, int "rfactorX", int "rfactorY", bool "alignc",
\                         bool "Y", bool "U", bool "V", int "nsize", int "nns", int "qual",
\                         int "etype", int "pscrn", int "threads", int "opt", int "fapprox",
\                         bool "f", bool "t") {

rfactorX = Default(rfactorX, 2)
rfactorY = Default(rfactorY, rfactorX)
alignc   = Default(alignc, false)
Y        = Default(Y, true)
U        = Default(U, true)
V        = Default(V, true)
nsize    = Default(nsize, 0)
nns      = Default(nns, 3)
f        = Default(f, true)
t        = Default(t, false)

Assert(rfactorX > 0 && BitAnd(rfactorX,rfactorX-1) == 0, "edi_rpow2_nnedi3: rfactorX must be a power of 2")
Assert(rfactorY > 0 && BitAnd(rfactorY,rfactorY-1) == 0, "edi_rpow2_nnedi3: rfactorY must be a power of 2")

# YV16/YUY2/YV411 chroma gets mangled by turning, so don't allow processing it
Assert(!input.IsYV16() && !input.IsYUY2() && !input.IsYV411() || !U && !V,
\      "edi_rpow2_nnedi3: YV16/YUY2/YV411 supported only when U and V are false")

# If alignc=true, always use field=1 for doubling the width
# to maintain alignment of horizontally subsampled chroma.
field2 = f ? 1 : 0
field1 = alignc ? 1 : field2
UVbool = U || V ? true : false

# Only turn right if doubling the width and the input isn't already turned
    !t && rfactorX > 1 ?
\   Eval("""
         dbl = input.TryFTurnRight(UVbool)
         t   = true
         """)
\ : Eval("""dbl = input""")

dbl = rfactorX > 1 ? dbl.nnedi3(field1,true,Y,U,V,nsize,nns,qual,etype,pscrn,threads,opt,fapprox) : dbl

# Only turn left if the height is going to be doubled or after the last iteration of doubling the width.
# This avoids unnecessary turning when only the width is doubled repeatedly.
    t && rfactorY >  1 ||
\   t && rfactorX == 2  ?
\   Eval("""
         dbl = dbl.TryFTurnLeft(UVbool)
         t   = false
         """)
\ : Eval("""dbl = dbl""")

dbl = rfactorY > 1 ? dbl.nnedi3(field2,true,Y,U,V,nsize,nns,qual,etype,pscrn,threads,opt,fapprox) : dbl

return rfactorX > 1 ||
\      rfactorY > 1  ? dbl.edi_rpow2_nnedi3(Max(1,rfactorX/2),Max(1,rfactorY/2),alignc,Y,U,V,
\                                           nsize,nns,qual,etype,pscrn,threads,opt,fapprox,false,t)
\                    : input
}

function edi_rpow2_nnedi3ocl(clip input, int "rfactorX", int "rfactorY", bool "alignc",
\                            bool "Y", bool "U", bool "V", int "nsize", int "nns", int "qual", int "etype",
\                            bool "f", bool "t") {

rfactorX = Default(rfactorX, 2)
rfactorY = Default(rfactorY, rfactorX)
alignc   = Default(alignc, false)
Y        = Default(Y, true)
U        = Default(U, true)
V        = Default(V, true)
nsize    = Default(nsize, 0)
nns      = Default(nns, 3)
f        = Default(f, true)
t        = Default(t, false)

Assert(rfactorX > 0 && BitAnd(rfactorX,rfactorX-1) == 0, "edi_rpow2_nnedi3ocl: rfactorX must be a power of 2")
Assert(rfactorY > 0 && BitAnd(rfactorY,rfactorY-1) == 0, "edi_rpow2_nnedi3ocl: rfactorY must be a power of 2")
Assert(!input.IsYV16() && !input.IsYV411() || !U && !V,
\      "edi_rpow2_nnedi3ocl: YV16/YV411 supported only when U and V are false")

field2 = f ? 1 : 0
field1 = alignc ? 1 : field2
dw     = rfactorX > 1 ? field1 : -1
UVbool = U || V ? true : false

# Doubling the width and height together is faster than turning,
# but doubling only the width causes artifacts because the clip still gets deinterlaced vertically,
# so the input is turned and the height doubled in that case.

dbl1 = input.nnedi3ocl(field2,true,Y,U,V,nsize,nns,qual,etype,dw)

    !t && rfactorX > 1 && rfactorY < 2 ?
\   Eval("""
         dbl2 = input.TryFTurnRight(UVbool)
         t    = true
         """)
\ : Eval("""dbl2 = input""")

dbl2 = rfactorX > 1 ? dbl2.nnedi3ocl(field1,true,Y,U,V,nsize,nns,qual,etype,-1) : dbl2

    t && rfactorX == 2 ?
\   Eval("""
         dbl2 = dbl2.TryFTurnLeft(UVbool)
         t    = false
         """)
\ : Eval("""dbl2 = dbl2""")

dbl = rfactorX > 1 && rfactorY < 2 ? dbl2 : dbl1

return rfactorX > 1 ||
\      rfactorY > 1  ? dbl.edi_rpow2_nnedi3ocl(Max(1,rfactorX/2),Max(1,rfactorY/2),alignc,Y,U,V,nsize,nns,qual,etype,false,t)
\                    : input
}

function edi_rpow2_eedi3(clip input, int "rfactorX", int "rfactorY", bool "alignc",
\                        bool "Y", bool "U", bool "V", float "alpha", float "beta", float "gamma",
\                        int "nrad", int "mdis", bool "hp", bool "ucubic", bool "cost3",
\                        int "vcheck", float "vthresh0", float "vthresh1", float "vthresh2",
\                        string "sclip", string "sclip_params", int "threads", clip "mclip", int "opt",
\                        bool "f", bool "t") {

rfactorX     = Default(rfactorX, 2)
rfactorY     = Default(rfactorY, rfactorX)
alignc       = Default(alignc, false)
Y            = Default(Y, true)
U            = Default(U, true)
V            = Default(V, true)
sclip        = Default(sclip, "")
sclip_params = Default(sclip_params, sclip == "nnedi3" || sclip == "nnedi3ocl" ? "nsize=0, nns=3" : "")
f            = Default(f, true)
t            = Default(t, false)

Assert(rfactorX > 0 && BitAnd(rfactorX,rfactorX-1) == 0, "edi_rpow2_eedi3: rfactorX must be a power of 2")
Assert(rfactorY > 0 && BitAnd(rfactorY,rfactorY-1) == 0, "edi_rpow2_eedi3: rfactorY must be a power of 2")
Assert(!input.IsYUY2() || !U && !V, "edi_rpow2_eedi3: YUY2 supported only when U and V are false")

# If the sclip_params string doesn't start with a comma, add one to avoid a syntax error
sclip_params = sclip_params == "" || sclip_params.LeftStr(1) == "," ? sclip_params : ","+sclip_params

field2  = f ? 1 : 0
field1  = alignc ? 1 : field2
UVbool  = U || V ? true : false
Yint    = Y ? 3 : 1
Uint    = U ? 3 : 1
Vint    = V ? 3 : 1

# Use a src_left or src_top shift dependent on the field when resizing
# for the sclip or mclip to keep it in alignment with the eedi3 clip.
# Additionally, the vertical shift for YV12 chroma is doubled,
# so the chroma must be resized separately when doubling the height.
rshift1 = field1 == 1 ? 0.25 : -0.25
rshift2 = field2 == 1 ? 0.25 : -0.25

    !t && rfactorX > 1 ?
\   Eval("""
         dbl      = input.TryFTurnRight(UVbool)
         t        = true
         """)
\ : Eval("""
         dbl      = input
         """)

sclip2 = sclip == ""          ? Undefined()
\      : sclip == "nnedi3"    ? Eval(""" rfactorX > 1    ? dbl.nnedi3(field1,true,Y=Y,U=U,V=V"""+sclip_params+""") : Undefined() """)
\      : sclip == "nnedi3ocl" ? Eval(""" rfactorX > 1    ? dbl.nnedi3ocl(field1,true,Y=Y,U=U,V=V"""+sclip_params+""") : Undefined() """)
\      : sclip == "eedi2"     ? Eval(""" rfactorX > 1    ? dbl.eedi2(field=field1"""+sclip_params+""") : Undefined() """)
\                             : Eval(""" rfactorX > 1   &&
                                     \   UVbool         &&
                                     \   input.IsYV12()  ? dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift1, kernel=sclip,
                                     \                                 luma=Y, chroma=false"""+sclip_params+""")
                                     \                        .MergeChroma(dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift1*2.0, kernel=sclip,
                                     \                                                 luma=false, chroma=true"""+sclip_params+"""))
                                     \ : rfactorX > 1    ? dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift1, kernel=sclip,
                                     \                                 luma=Y, chroma=UVbool"""+sclip_params+""")
                                     \                   : Undefined() """)
mclip1 = mclip.Defined() && rfactorX > 1 ? mclip.TryFTurnRight(UVbool) : mclip
dbl    = rfactorX > 1 ? dbl.eedi3(field1,true,Y,U,V,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
\                                 vcheck,vthresh0,vthresh1,vthresh2,sclip2,threads,mclip1,opt) : dbl

# No need to resize YV12 chroma separately here because it's the width being doubled
mclip  = mclip.Defined() && rfactorX > 1 ? mclip.ResizeX(mclip.Width()*2,mclip.Height(),rshift1,0, luma=Y, chroma=UVbool)
\                                               .mt_binarize(128, Y=Yint, U=Uint, V=Vint)
\                                        : mclip

    t && rfactorY >  1 ||
\   t && rfactorX == 2  ?
\   Eval("""
         dbl = dbl.TryFTurnLeft(UVbool)
         t   = false
         """)
\ : Eval("""dbl = dbl""")

sclip2 = sclip == ""          ? Undefined()
\      : sclip == "nnedi3"    ? Eval(""" rfactorY > 1    ? dbl.nnedi3(field2,true,Y=Y,U=U,V=V"""+sclip_params+""") : Undefined() """)
\      : sclip == "nnedi3ocl" ? Eval(""" rfactorY > 1    ? dbl.nnedi3ocl(field2,true,Y=Y,U=U,V=V"""+sclip_params+""") : Undefined() """)
\      : sclip == "eedi2"     ? Eval(""" rfactorY > 1    ? dbl.eedi2(field=field2"""+sclip_params+""") : Undefined() """)
\                             : Eval(""" rfactorY > 1   &&
                                     \   UVbool         &&
                                     \   input.IsYV12()  ? dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift2, kernel=sclip,
                                     \                                 luma=Y, chroma=false"""+sclip_params+""")
                                     \                        .MergeChroma(dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift2*2.0, kernel=sclip,
                                     \                                                 luma=false, chroma=true"""+sclip_params+"""))
                                     \ : rfactorY > 1    ? dbl.ResizeX(dbl.Width(),dbl.Height()*2,0,rshift2, kernel=sclip,
                                     \                                 luma=Y, chroma=UVbool"""+sclip_params+""")
                                     \                   : Undefined() """)
dbl    = rfactorY > 1 ? dbl.eedi3(field2,true,Y,U,V,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
\                                 vcheck,vthresh0,vthresh1,vthresh2,sclip2,threads,mclip,opt) : dbl
mclip  = mclip.Defined() && rfactorY > 1 &&
\        input.IsYV12()  && UVbool        ? mclip.ResizeX(mclip.Width(),mclip.Height()*2,0,rshift2, luma=Y, chroma=false)
\                                                .MergeChroma(mclip.ResizeX(mclip.Width(),mclip.Height()*2,0,rshift2*2.0, luma=false, chroma=true))
\      : mclip.Defined() && rfactorY > 1  ? mclip.ResizeX(mclip.Width(),mclip.Height()*2,0,rshift2, luma=Y, chroma=UVbool)
\                                                .mt_binarize(128, Y=Yint, U=Uint, V=Vint)
\                                         : mclip

return rfactorX > 1 ||
\      rfactorY > 1  ? dbl.edi_rpow2_eedi3(Max(1,rfactorX/2),Max(1,rfactorY/2),alignc,Y,U,V,alpha,beta,gamma,nrad,mdis,hp,ucubic,cost3,
\                                          vcheck,vthresh0,vthresh1,vthresh2,sclip,sclip_params,threads,mclip,opt,false,t)
\                    : input
}

function edi_rpow2_eedi2(clip input, int "rfactorX", int "rfactorY", bool "alignc",
\                        int "mthresh", int "lthresh", int "vthresh", int "estr",
\                        int "dstr", int "maxd", int "map", int "nt", int "pp",
\                        bool "f", bool "t") {

rfactorX = Default(rfactorX, 2)
rfactorY = Default(rfactorY, rfactorX)
alignc   = Default(alignc, false)
f        = Default(f, true)
t        = Default(t, false)

Assert(rfactorX > 0 && BitAnd(rfactorX,rfactorX-1) == 0, "edi_rpow2_eedi2: rfactorX must be a power of 2")
Assert(rfactorY > 0 && BitAnd(rfactorY,rfactorY-1) == 0, "edi_rpow2_eedi2: rfactorY must be a power of 2")
Assert(!input.IsYUY2(), "edi_rpow2_eedi2: YUY2 not supported")

field2 = f ? 1 : 0
field1 = alignc ? 1 : field2

    !t && rfactorX > 1 ?
\   Eval("""
         dbl = input.TryFTurnRight()
         t   = true
         """)
\ : Eval("""dbl = input""")

dbl = rfactorX > 1 ? dbl.eedi2(mthresh,lthresh,vthresh,estr,dstr,maxd,field1,map,nt,pp) : dbl

    t && rfactorY >  1 ||
\   t && rfactorX == 2  ?
\   Eval("""
         dbl = dbl.TryFTurnLeft()
         t   = false
         """)
\ : Eval("""dbl = dbl""")

dbl = rfactorY > 1 ? dbl.eedi2(mthresh,lthresh,vthresh,estr,dstr,maxd,field2,map,nt,pp) : dbl

return rfactorX > 1 ||
\      rfactorY > 1  ? dbl.edi_rpow2_eedi2(Max(1,rfactorX/2),Max(1,rfactorY/2),alignc,mthresh,lthresh,vthresh,estr,dstr,maxd,map,nt,pp,false,t)
\                    : input
}

# Functions to use FTurn if available or else fall back to the internal turn functions
function TryFTurnLeft(clip c, bool "chroma") {
try { 
    return c.FTurnLeft(chroma=chroma)
    }
catch(err_msg) {
    return c.TurnLeft()
    }
}

function TryFTurnRight(clip c, bool "chroma") {
try { 
    return c.FTurnRight(chroma=chroma)
    }
catch(err_msg) {
    return c.TurnRight()
    }
}

function TryFTurn180(clip c, bool "chroma") {
try { 
    return c.FTurn180(chroma=chroma)
    }
catch(err_msg) {
    return c.Turn180()
    }
}