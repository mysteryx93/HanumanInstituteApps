################################################################################################
###                                                                                          ###
###                           Simple MDegrain Mod - SMDegrain()                              ###
###                                                                                          ###
###                       Mod by Dogway - Original idea by Caroliano                         ###
###                                                                                          ###
###          Special Thanks: Sagekilla, Didée, cretindesalpes, Gavino and MVtools people     ###
###                                                                                          ###
###                       v3.1.2d (Dogway's mod) - 21 July 2015                              ###
###          mod in 2016-06-25 to fix subpixel=3 and add Prefilter auto 16 (lsb) support     ###
###                           and yuy2 support in avs 2.6 with masktools 2.6                 ###
### and medianblur2 now will be used in Minblur instead of Quantile RemoveGrainHD in avs 2.6 ###
###                                    and add TV_range bool                                 ###
###                                                                                          ###
################################################################################################
###
### General purpose simple degrain function. Pure temporal denoiser. Basically a wrapper(function)/frontend of mvtools2+mdegrain
### with some added common related options. Goal is accessibility and quality but not targeted to any specific kind of source.
### The reason behind is to keep it simple so aside masktools2 you will only need MVTools2.
###
### Check documentation for deep explanation on settings and defaults.
### VideoHelp thread: (http://forum.videohelp.com/threads/369142)
###
################################################################################################

function SMDegrain (clip input   , int "tr"      , int "thSAD"    , int  "thSADC" , bool "RefineMotion",  val  "contrasharp", clip "CClip"      , bool "interlaced" ,  int   "plane" ,  int   "Globals",                                  \
                     int "pel"   , int "subpixel", val "prefilter", clip "mfilter", int  "blksize"     , int  "overlap"     ,  int  "search"    , bool "truemotion" , int  "limit"   ,  int   "limitc" ,  int   "thSCD1" , int "thSCD2",  \
                    bool "chroma", int "hpad"    , int "vpad"     , bool "lsb"    , bool "lsb_in"      ,  bool "lsb_out"    , int  "mode"       , bool  "slices"    , val  "Show"    ,  float "Str"    ,  float "Amp" , bool "TV_range"){



# Defaults & Conditionals


tr           = default( tr,       2 )
thSAD        = default( thSAD,  300 )
thSAD2=int(thSAD/2)
thSADC       = default( thSADC, thSAD2)
RefineMotion = default( RefineMotion,false)

lsb_in       = default( lsb_in,  false)
lsb_out      = default( lsb_out, false)
lsb          = default( lsb    , lsb_in || lsb_out)
mode         = default( mode, 0 )
mode         = mode != 9 ? mode : -1
ampn         = mode != 9 ? 0.0  : 1.0
slices       = default(slices, true)

Globals      = default( Globals,  0 )
GlobalR = Globals == 1        ? true : false
GlobalO = Globals == 3        ? true : false
if1     = Defined(CClip)      ? true : false

Contrasharp  = default( Contrasharp, !GlobalO && if1 ? true : false)
Interlaced   = default( Interlaced,  false)

w       = input.width ()
h       = input.height()
sShow   = IsString(show)      ? true : false
preclip = IsClip(prefilter)   ? true : false
planar  = IsYUY2(input)       ? true : false
ifC     = IsBool(Contrasharp) ? true : false
if0     = ifC ? (Contrasharp  ? true : false) : (Contrasharp > 0.0 ? true : false)
if2     = if0 && lsb && !GlobalO                                    ? true : false
if3     = tr > 3                                                    ? true : false
if4     = (w > 1099 ||  h > (lsb_in ? 1199 : 599))                  ? true : false
if5     = interlaced ? (GetParity(input)                            ? true : false) : nop()
avs26   = VersionNumber() < 2.60 ? false : true
yuy25   = !avs26 && planar


pel          = default( pel, if4 ? 1 : 2 )
subpixel     = default( subpixel,  2 )
prefilter    = default( prefilter, -1 )
MFilterB     = defined(mfilter) ? true : false
pelclip      = pel>1 && subpixel == 3 ? true : false
Str          = default( Str, 1.0 )
Amp          = default( Amp, 0.0625 )
TV_range     = default( TV_range, true)

blksize      = default( blksize,  if4 ? 16 : 8 )
blk2=blksize/2
overlap      = default( overlap,  blk2 )
ovl2=overlap/2
search       = default( search,   4 )
truemotion   = default( truemotion, !if4)
thSCD1       = default( thSCD1,  int(pow((blksize*2.5),2)))
thSCD2       = default( thSCD2,  130 )


plane        = default( plane,    4 )
chroma       = default( chroma,    true)
Chr          = chroma ? 3 : 1
Chr2         = chroma ? 3 : (prefilter==3 ? 2 : 1)
plane0       = plane!=0        ? true : false
U            = plane0 && plane!=2 ? 3 : 2
V            = plane0 && plane!=1 ? 3 : 2
Uin          = lsb_in ? 3 : u
Vin          = lsb_in ? 3 : v


hpad         = blksize
vpad         = blksize
limit        = default( limit,  255 )
limitc       = default( limitc,limit)

Show         = default( Show,  false)




# Error Report

                  Assert(blksize==4 || blksize==8 || blksize==16 || blksize==32,"MAnalyse: Block's size must be 4x4, 8x4, 8x8, 16x2, 16x8, 16x16, 32x16, 32x32")
                  Assert(overlap <= blk2,              "Overlap must be at least half blksize or less")
                  Assert(overlap == overlap-overlap%2, "MAnalyse: overlap must be an even figure")
                  Assert(IsBool(Show) || sShow,        "'Show' only accepts bool inputs (true,false), or string modes ('Speed','Memory',Quality')")
                  Assert(preclip || IsInt(prefilter),  "'prefilter' only accepts integers and clip inputs")
                  Assert(ifC     || IsInt(Contrasharp),"'Contrasharp' only accepts integers and bool inputs")
!chroma         ? Assert(plane==0,                     "Denoising chroma with luma only vectors is bugged in mvtools and thus unsupported") : nop()
MFilterB        ? Assert(IsClip(mfilter),              "'mfilter' only accepts clip inputs")                        : nop()
interlaced      ? Assert(h%4==0,                       "Interlaced sources require mod 4 height sizes")             : nop()
lsb_in          ? Assert(!interlaced,                  "Interlaced 16 bit stacked clip is not a recognized format") : nop()
planar && if0   ? Assert(ifC,                          "LSFmod doesn't support YUY2 sources")                       : nop()
RefineMotion    ? Assert(blksize > 7,                  "For RefineMotion you need a blksize of at least 8")         : nop()
lsb_in          ? Assert(lsb,                          "lsb_in  requires: lsb=true")                                : nop()
lsb_out         ? Assert(lsb,                          "lsb_out requires: lsb=true")                                : nop()



# RefineMotion Variables

halfblksize  = RefineMotion ?  blk2                                     : nop() # MRecalculate works with half block size
halfoverlap  = RefineMotion ? (overlap == 2 ? overlap : ovl2+ovl2%2)    : nop() # Halve the overlap to suit the halved block size
halfthSAD    = RefineMotion || if3 ?     thSAD2                         : nop() # MRecalculate uses a more strict thSAD, which defaults to 150 (half of function's default of 300)
halfthSADC   =                 if3 ? int(thSADC/2)                      : nop() # For MDegrainN()






# Input preparation for: LSB_IN, Interlacing, Planar and MSuper optimization when pel=2

inputY  = planar      ? (lsb_in   ? Dither_YUY2toPlanar16(input)          : Interleaved2planar(input))                          : input

inputP  = !interlaced ? (pel == 2 ? inputY.AssumeFrameBased()             : inputY)                                             : \
                        (if5      ? inputY.AssumeTFF().SeparateFields()   : inputY.AssumeBFF().SeparateFields())

input8h = lsb_in      ?             inputP. Ditherpost(mode=6, slice=slices)                                                    : nop()
input8y = planar      ? (lsb_in   ? input8h.Dither_YUY2toInterleaved()    : inputP)                                             : inputP
input8  = lsb_in      ? (planar   ? input8y.Interleaved2planar()                                                                : \
                                    input8h)                                                                                    : input8y


# Prefilter & Motion Filter

Mfilter = MFilterB ? Mfilter : input8

bug_wa = interlaced && planar && chroma && !avs26 ? 2 : Chr # bug: crash prevention workaround (http://forum.doom9.org/showthread.php?p=1546684#post1546684)

# The mt_merge() line for prefilter=3 should be swapped with a high bitdepth variant (Dither_merge16_8() ?) due to a 1 point limited range
# in both range ends, but then it won't work with planar sources. This isn't as critical since we are only trying to find motion vectors.
pref   = !GlobalR ? ( preclip        ?  prefilter                                                                  : \
                    ((prefilter==-1) ?  inputP                                                                     : \
                     (prefilter==0)  ?  input8.MinBlur(0,Chr,planar)                                               : \
                     (prefilter==1)  ?  input8.MinBlur(1,Chr,planar)                                               : \
                     (prefilter==2)  ?  input8.MinBlur(2,Chr,planar)                                               : \
                     (prefilter==3)  ?  (!planar && lsb ? Dither_merge16_8(inputP.Dfttest(sstring="0.0:4.0 0.2:9.0 1.0:15.0",tbsize=1,u=chroma,v=chroma,lsb=true,lsb_in=lsb_in,quiet=true), lsb_in?inputP:inputP.Dither_convert_8_to_16(),                                \
                                                                   lsb_in?inputP.Dither_lut16("x 4096 < 65535 x 19200 > 0 65535 x 4096 - 65535 19200 4096 - / * - ? ?",u=1,v=1).Ditherpost(mode=6, slice=slices, u=Chr,   v=Chr)                                           \
                                                                         :inputP.mt_lut(      "x 16 < 255 x 75 > 0 255 x 16 - 255 75 16 - / * - ? ?",u=1,v=1), luma=chroma,                       u=Chr2,  v=Chr2)                                                       : \
                                 avs26 && planar && lsb ? Dither_merge16_8(inputP.Dfttest(sstring="0.0:4.0 0.2:9.0 1.0:15.0",tbsize=1,u=chroma,v=chroma,lsb=true,lsb_in=lsb_in,quiet=true), lsb_in?inputP:inputP.Dither_convert_8_to_16(),                                \
                                                                   lsb_in?inputP.Dither_lut16("x 4096 < 65535 x 19200 > 0 65535 x 4096 - 65535 19200 4096 - / * - ? ?",u=1,v=1).Ditherpost(mode=6, slice=slices, u=Chr,   v=Chr)                                           \
                                                                         :inputP.Planar2Interleaved().ConvertToYV16().mt_lut(      "x 16 < 255 x 75 > 0 255 x 16 - 255 75 16 - / * - ? ?",u=1,v=1), luma=chroma,                       u=Chr2,  v=Chr2).ConvertToYUY2().Interleaved2planar()                                                       : \
                                           planar && avs26 ? mt_merge(           Dfttest(!lsb_in?inputP.Planar2Interleaved(!chroma):input8y,sstring="0.0:4.0 0.2:9.0 1.0:15.0",tbsize=1,u=chroma,v=chroma,dither=1).ConvertToYV16(),!lsb_in?input8y.Planar2Interleaved().ConvertToYV16():input8y.ConvertToYV16(),                                                   \
                                                                          input8y.Planar2Interleaved().ConvertToYV16().mt_lut(      "x 16 < 255 x 75 > 0 255 x 16 - 255 75 16 - / * - ? ?",u=1,v=1), luma=planar?false:chroma,          u=bug_wa,v=bug_wa).ConvertToYUY2().Interleaved2planar()                                                    : \
                                                             mt_merge( planar ?  Dfttest(!lsb_in?inputP.Planar2Interleaved(!chroma):input8y,sstring="0.0:4.0 0.2:9.0 1.0:15.0",tbsize=1,u=chroma,v=chroma,dither=1).Interleaved2planar(!chroma)                          : \
                                                                                 Dfttest(        inputP,                                    sstring="0.0:4.0 0.2:9.0 1.0:15.0",tbsize=1,u=chroma,v=chroma,dither=1),!lsb_in?input8y:input8y.Interleaved2planar(!chroma),   \
                                                                          input8y.mt_lut(      "x 16 < 255 x 75 > 0 255 x 16 - 255 75 16 - / * - ? ?",u=1,v=1), luma=planar?false:chroma,          u=bug_wa,v=bug_wa))                                                   : \
                     (prefilter==4)  ?  eval("""
                                              # In a more lucid state I could probably have laid out this block much better... or not...
                                              NL_in   = !planar ? (lsb_in ? inputP : input8.Dither_convert_8_to_16()) : Interlaced ? (if5 ? input.AssumeTFF().SeparateFields().Dither_convert_8_to_16() : input.AssumeBFF().SeparateFields().Dither_convert_8_to_16()) : (lsb_in ? input : input.Dither_convert_8_to_16())
                                              NL_W    = width(NL_in)
                                              U8      = chroma || planar ? UToY8(NL_in) : nop()
                                              V8      = chroma || planar ? VToY8(NL_in) : nop()
                                              NL_lsb  = chroma ? (!planar ?           StackVertical(StackVertical(Dither_get_msb(u8),Dither_get_msb(v8)),\
                                                                                      StackVertical(Dither_get_lsb(u8),Dither_get_lsb(v8)))              :\
                                                                                      StackHorizontal(u8.ConvertToYV12(),v8.ConvertToYV12())) : nop()

                                              Y8      = chroma ? (!planar ? StackHorizontal(ConvertToY8(NL_in),NL_lsb) : \
                                                                            StackHorizontal(StackVertical(Dither_get_msb(ConvertToY8(NL_in)),Dither_get_lsb(ConvertToY8(NL_in))).ConvertToYV12(),NL_lsb)) : \
                                                                 (!planar ? NL_in : NL_in.converttoY8())

                                              Y8      = Y8.KNLMeansCL(D=1, A=1, h=7.0,lsb_inout=true,device_type="GPU")

                                              my      = planar ? Y8.Dither_get_msb().crop(0,0,chroma?-NL_W:0,0) : nop()
                                              ly      = planar ? Y8.Dither_get_lsb().crop(0,0,chroma?-NL_W:0,0) : nop()

                                              chroma ? eval ("
                                              u_h = u8.height()/2
                                              v_h = v8.height()/2

                                              u16 = !planar ? StackVertical(Dither_get_msb(Y8).crop(NL_W,0,0,-v_h),Dither_get_lsb(Y8).crop(NL_W,0,0,-v_h))                : \
                                                              StackVertical(Dither_get_msb(Y8).crop(NL_W,0,-NL_W/2 ,0),Dither_get_lsb(Y8).crop(NL_W,0,-NL_W/2 ,0))
                                              v16 = !planar ? StackVertical(Dither_get_msb(Y8).crop(NL_W,u_h, 0,0),Dither_get_lsb(Y8).crop(NL_W,u_h, 0,0))                : \
                                                              StackVertical(Dither_get_msb(Y8).crop(NL_W+NL_W/2,0,0,0),Dither_get_lsb(Y8).crop(NL_W+NL_W/2,0,0,0))

                                              !planar ? YToUV(u16,v16,Y8.crop(0,0,-u8.width(),0)) : avs26 ? YToUV(u16, v16, StackVertical(my, ly)) : YToUV(u16, v16, StackVertical(StackVertical (my,my), StackVertical (ly,ly))) ") : \
                                            ( !planar || avs26 ? Y8 : YToUV (u8, v8, StackVertical (StackVertical (my, my), StackVertical (ly, ly)))) """)                                   : \
                      Assert(false,    "Mode must be between -1~3: "+string(prefilter))))                                                                                           : \
                      input8





# Default Auto-Prefilter - Luma expansion TV->PC (up to 16% more values for motion estimation)

pref   = avs26 && planar && ((lsb_in && prefilter==-1) || prefilter==4) ? pref.Dither_YUY2toInterleaved16().Interleaved2planar() : pref
pref16 = (Height(pref)) == (Height(input8)*2) ? true : false
pref   = !GlobalR ? (!yuy25 && (lsb || (!preclip ? prefilter==4 : false) ) ? ((!preclip ? ((lsb_in && prefilter==-1) || prefilter==3 || prefilter==4 ) : false)          ? \
                     !TV_range ? pref.Ditherpost(mode=6,ampn=0.0,slice=slices,staticnoise=false,u=Chr2,v=Chr2)                                      : \
                     pref.Dither_Luma_Rebuild(S0=Str,c=Amp,slice=slices,lsb_in=true,uv=Chr2)                                      : \
                     !TV_range ? pref16 ? pref.Ditherpost(mode=6,ampn=0.0,slice=slices,staticnoise=false,u=Chr2,v=Chr2) : pref                                      : \
                     pref.Dither_Luma_Rebuild(S0=Str,c=Amp,slice=slices,lsb   =true,lsb_in=pref16,uv=Chr2))                       : \
                                      ((!preclip ?  ((lsb_in && prefilter==-1) || prefilter==4)                                   : false)                                 ? \
                     !TV_range ? pref.Ditherpost(mode=6,slice=slices,u=Chr,v=Chr).Dither_YUY2toInterleaved().Interleaved2planar(!chroma)   : \
                     pref.Ditherpost(mode=6,slice=slices,u=Chr,v=Chr).Dither_YUY2toInterleaved().Interleaved2planar(!chroma).Dither_Luma_Rebuild(S0=Str,c=Amp,uv=Chr   )   : \
                     !TV_range ? pref16 ? pref.Dither_YUY2toPlanar16().Ditherpost(mode=6,slice=slices,u=bug_wa,v=bug_wa).Dither_YUY2toInterleaved().Interleaved2planar(!chroma) : pref                                  : \
                     pref.                                                    Dither_Luma_Rebuild(S0=Str,lsb_in=pref16,slice=slices,c=Amp,uv=bug_wa)))                                  : \
                     pref





(sShow ? false : !Show) ? eval("""


# Subpixel 3

pclip  = pelclip              ?    pel==4 ? pref.nnedi3_rpow2(rfactor=2,nns=4).nnedi3_rpow2(rfactor=2,nns=4) :   pref.nnedi3_rpow2(rfactor=2,nns=4)  : nop()
pclip2 = pelclip && !GlobalR  ?  pel==4 ? input8.nnedi3_rpow2(rfactor=2,nns=4).nnedi3_rpow2(rfactor=2,nns=4) : input8.nnedi3_rpow2(rfactor=2,nns=4)  : nop()




# Motion vectors search


super_search =                             pelclip ? MSuper(pref,     pel=pel,               chroma=chroma, hpad=hpad, vpad=vpad,          pelclip=pclip,  rfilter=4, planar=planar) : \
                                                     MSuper(pref,     pel=pel,sharp=subpixel,chroma=chroma, hpad=hpad, vpad=vpad                        ,  rfilter=4, planar=planar)
super_render = !GlobalR ?                 (pelclip ? MSuper(input8,   pel=pel,               chroma=plane0, hpad=hpad, vpad=vpad,levels=1, pelclip=pclip2           , planar=planar) : \
                                                     MSuper(input8,   pel=pel,sharp=subpixel,chroma=plane0, hpad=hpad, vpad=vpad,levels=1                           , planar=planar)): super_search
Recalculate  = !GlobalR && RefineMotion ? (pelclip ? MSuper(pref,     pel=pel,               chroma=chroma, hpad=hpad, vpad=vpad,levels=1, pelclip=pclip            , planar=planar) : \
                                                     MSuper(pref,     pel=pel,sharp=subpixel,chroma=chroma, hpad=hpad, vpad=vpad,levels=1                           , planar=planar)): nop()
!if3  ? eval("
bv6  =   interlaced && (tr > 2)                  ? (!GlobalR  ? super_search.MAnalyse(isb = true, delta = 6,  overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         bv6) :  nop()
bv6  =   interlaced && (tr > 2) && RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, bv6,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  bv6
bv4  =   interlaced && (tr > 1)                  ? (!GlobalR  ? super_search.MAnalyse(isb = true, delta = 4,  overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         bv4) :  nop()
bv4  =   interlaced && (tr > 1) && RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, bv4,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  bv4
bv3  =  !interlaced ? ((tr > 2)                  ? (!GlobalR  ? super_search.MAnalyse(isb = true, delta = 3,  overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         bv3) :  nop()) : nop()
bv3  =  !interlaced ? ((tr > 2  && RefineMotion &&  !GlobalR) ?   MRecalculate           (Recalculate, bv3,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  bv3               )   : nop()
bv2  =  (interlaced ||  tr > 1)                  ? (!GlobalR  ? super_search.MAnalyse(isb = true, delta = 2,  overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         bv2) :  nop()
bv2  = ((interlaced ||  tr > 1) && RefineMotion &&  !GlobalR) ?   MRecalculate           (Recalculate, bv2,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  bv2
bv1  =  !interlaced ?                              (!GlobalR  ? super_search.MAnalyse(isb = true, delta = 1,  overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         bv1        ) : nop()
bv1  =  !interlaced ?            ( RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, bv1,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  bv1               ) : nop()

fv1  =  !interlaced ?                              (!GlobalR  ? super_search.MAnalyse(isb = false, delta = 1, overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         fv1        ) : nop()
fv1  =  !interlaced ?            ( RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, fv1,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  fv1               ) : nop()
fv2  =  (interlaced ||  tr > 1)                  ? (!GlobalR  ? super_search.MAnalyse(isb = false, delta = 2, overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         fv2) :  nop()
fv2  = ((interlaced ||  tr > 1) && RefineMotion &&  !GlobalR) ?   MRecalculate           (Recalculate, fv2,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  fv2
fv3  =  !interlaced ? ((tr > 2)                  ? (!GlobalR  ? super_search.MAnalyse(isb = false, delta = 3, overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         fv3) :  nop()) : nop()
fv3  =  !interlaced ? ((tr > 2  && RefineMotion &&  !GlobalR) ?   MRecalculate           (Recalculate, fv3,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  fv3               )   : nop()
fv4  =   interlaced && (tr > 1)                  ? (!GlobalR  ? super_search.MAnalyse(isb = false, delta = 4, overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         fv4) :  nop()
fv4  =   interlaced && (tr > 1) && RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, fv4,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  fv4
fv6  =   interlaced && (tr > 2)                  ? (!GlobalR  ? super_search.MAnalyse(isb = false, delta = 6, overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma, truemotion=truemotion) :         fv6) :  nop()
fv6  =   interlaced && (tr > 2) && RefineMotion &&  !GlobalR  ?   MRecalculate           (Recalculate, fv6,   overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma, truemotion=truemotion) :  fv6 " )     : \
 \
eval("
tr2    = tr*2
vmulti =                   !GlobalR ? super_search.MAnalyse( multi=true,overlap=    overlap,blksize=    blksize,   search=search,chroma=chroma,truemotion=truemotion, delta = interlaced ? tr2 : tr) : vmulti
vmulti = RefineMotion   ? (!GlobalR ? MRecalculate (Recalculate, vmulti,overlap=halfoverlap,blksize=halfblksize, thSAD=halfthSAD,chroma=chroma,truemotion=truemotion,    tr = interlaced ? tr2 : tr) : vmulti) : vmulti
vmulti =                   !GlobalR ? (interlaced ? vmulti.SelectEvery (4, 2, 3) : vmulti) : vmulti.SelectRangeEvery (Rtr*2, tr2, 0, false)")



# Finally, MDegrain


output  = !GlobalO ? (interlaced ?
 \        ((lsb ?     ( if3  ? mfilter.MDegrainN(super_render, vmulti, tr, thSAD2=halfthSAD, thSADC2=halfthSADC, thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                 (tr == 3) ? mfilter.MDegrain3(super_render, bv2, fv2, bv4, fv4, bv6, fv6,                     thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                 (tr == 2) ? mfilter.MDegrain2(super_render, bv2, fv2, bv4, fv4,                               thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                             mfilter.MDegrain1(super_render, bv2, fv2,                                         thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true)):
 \                    ( if3  ? mfilter.MDegrainN(super_render, vmulti, tr, thSAD2=halfthSAD, thSADC2=halfthSADC, thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=false):
 \                 (tr == 3) ? mfilter.MDegrain3(super_render, bv2, fv2, bv4, fv4, bv6, fv6,                     thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar)          :
 \                 (tr == 2) ? mfilter.MDegrain2(super_render, bv2, fv2, bv4, fv4,                               thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar)          :
 \                             mfilter.MDegrain1(super_render, bv2, fv2,                                         thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar))))       :
 \         (lsb ?     ( if3  ? mfilter.MDegrainN(super_render, vmulti, tr, thSAD2=halfthSAD, thSADC2=halfthSADC, thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                 (tr == 3) ? mfilter.MDegrain3(super_render, bv1, fv1, bv2, fv2, bv3, fv3,                     thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                 (tr == 2) ? mfilter.MDegrain2(super_render, bv1, fv1, bv2, fv2,                               thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true) :
 \                             mfilter.MDegrain1(super_render, bv1, fv1,                                         thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=true)):
 \                    ( if3  ? mfilter.MDegrainN(super_render, vmulti, tr, thSAD2=halfthSAD, thSADC2=halfthSADC, thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar,lsb=false):
 \                 (tr == 3) ? mfilter.MDegrain3(super_render, bv1, fv1, bv2, fv2, bv3, fv3,                     thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar)          :
 \                 (tr == 2) ? mfilter.MDegrain2(super_render, bv1, fv1, bv2, fv2,                               thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar)          :
 \                             mfilter.MDegrain1(super_render, bv1, fv1,                                         thSAD=thSAD, thSADC=thSADC, thSCD1=thSCD1, thSCD2=thSCD2, limit=limit, limitc=limitc, plane=plane, planar=planar))))       : nop()


# LSB_IN merging

outputI= !GlobalO && planar && (lsb || ifC) ? output.Planar2Interleaved()                                                                                             : nop()
!GlobalO && lsb   ? eval("
blnkclp= lsb_in  || (if2 && !planar) ? BlankClip (lsb_in?input8h:input8, pixel_type="+Chr(34)+"YV12"+Chr(34)+", color_yuv=0)                                          : nop()
output = lsb && planar               ? Dither_YUY2toPlanar16(outputI)                                                                                                 : output
output = lsb_in                      ? inputY.Dither_add16 (Dither_sub16(output, planar?inputY:input8.Dither_convert_8_to_16 (), dif=true,u=u,v=v), dif=true,u=u,v=v) : output") : nop()




# Contrasharp (only sharpens luma)

ifC && if0 && if4 ? eval("
global Super           = super_render
global cb1             = !if3 ? (interlaced ? bv2 : bv1) : vmulti.SelectEvery (tr2, 0)
global cf1             = !if3 ? (interlaced ? fv2 : fv1) : vmulti.SelectEvery (tr2, 1)") : nop()

!GlobalO && if0   ? eval("

CCh   =       if1 ? CClip.height() : nop()
ref8  =       if1 ? (h == CCh*2)   : nop()
ref16 =       if1 ? (h == CCh/2)   : nop()

interlaced && if1 ? Assert(!ref16, "+Chr(34)+"Interlaced 16 bit stacked CClip is not a recognized format"+Chr(34)+")         : nop()

CClip = planar ?  (if1 ?  (lsb_in ? (ref8 ? CClip.Interleaved2planar(true) : CClip.Dither_YUY2toPlanar16().DitherPost(mode=-1,u=1,v=1).Dither_YUY2toInterleaved().Interleaved2planar(true))                         : \
                                     (interlaced ? (if5 ? CClip.AssumeTFF().SeparateFields().Interleaved2planar(true) : CClip.AssumeBFF().SeparateFields().Interleaved2planar(true))                                : \
                                     (ref16 ? CClip.Dither_YUY2toPlanar16().DitherPost(mode=-1,u=1,v=1).Dither_YUY2toInterleaved().Interleaved2planar(true) : CClip.Interleaved2planar(true))))                     : \
                          (lsb_in ?  inputY.DitherPost(mode=-1,u=1,v=1).Dither_YUY2toInterleaved().Interleaved2planar(true)                                                                                         : \
                                     inputP))                                                                                                                                                                       : \
                  (if1 ?  (lsb_in ? (ref8 ? CClip : CClip.DitherPost(mode=-1,u=1,v=1))                                                                                                                              : \
                                     (interlaced ? (if5 ? CClip.AssumeTFF().SeparateFields()                          : CClip.AssumeBFF().SeparateFields()) : (ref16 ? CClip.DitherPost(mode=-1,u=1,v=1) : CClip))) : \
                          (lsb_in ?  input.DitherPost(mode=-1,u=1,v=1)                                                                                                                                              : \
                                     inputP))")                                                                                                                                                                     : nop()


!GlobalO && if2   ? eval("
OutTO8  = output.DitherPost(mode=-1,u=1,v=1)
OutTO8P = planar  ? OutTO8.Dither_YUY2toInterleaved()                                                                                               : nop()
ctr16   = ifC     ? ContrasharpeningHD (planar ? OutTO8P.Interleaved2planar(true):OutTO8, CClip, planar=planar, HD=if4)                             : \
                    LSFmod(OutTO8,source=CClip,defaults="+Chr(34)+"slow"+Chr(34)+",strength=Contrasharp,soothe=false,Lmode=0,soft=-2,keep=0)


ctr16P  = planar  ? ctr16.Planar2Interleaved(true)  : ctr16
ctr16   = StackVertical (ctr16P, planar ? BlankClip  (ctr16P, pixel_type="+Chr(34)+"YUY2"+Chr(34)+", color_yuv=0,length=1) : blnkclp )
ctr16   = Dither_add16 (output,mt_makediff(!planar?ctr16P:ctr16P.Dither_YUY2toPlanar(), OutTO8,u=2,v=2).Dither_convert_8_to_16(), dif=true,u=2,v=2)
ctr16   = MergeChroma(ctr16, output)                                                                                                     ")         : nop()



# Globals Output

GlobalO || Globals == 2  ? (!if3 ? eval("
global Super           = super_render
global bv6             = bv6
global bv4             = bv4
global bv3             = bv3
global bv2             = bv2
global bv1             = bv1

global fv1             = fv1
global fv2             = fv2
global fv3             = fv3
global fv4             = fv4
global fv6             = fv6
") : eval("
global bv1             =                                            vmulti.SelectEvery (tr2, 0)
global bv2             = interlaced ? bv1                         : vmulti.SelectEvery (tr2, 2)
global bv3             =                                            vmulti.SelectEvery (tr2, 4)
global bv4             = interlaced ? vmulti.SelectEvery (tr2, 2) : nop()
global bv6             = interlaced ? bv3                         : nop()

global fv1             =                                            vmulti.SelectEvery (tr2, 1)
global fv2             = interlaced ? fv1                         : vmulti.SelectEvery (tr2, 3)
global fv3             =                                            vmulti.SelectEvery (tr2, 5)
global fv4             = interlaced ? vmulti.SelectEvery (tr2, 3) : nop()
global fv6             = interlaced ? fv3                         : nop()
global Super           = super_render
global vmulti          = vmulti
global Rtr             = tr")) : nop()



# Output

!GlobalO ? (lsb_out ? (if0 ? ctr16 : output)  : \
                      (if0 ? (lsb ?  (interlaced ?  ctr16.weave().DitherPost(u=u,v=v,mode=6,slice=slices,interlaced=true)               :  ctr16.DitherPost(mode=mode,slice=slices,ampn=ampn,u=Uin,v=Vin))                                              : \
                                     (interlaced ? (ifC ? ContraSharpeningHD(output, CClip, planar=planar, HD=if4).weave()              : LSFmod(output,source=CClip,defaults="slow",strength=Contrasharp,soothe=false,Lmode=0,soft=-2,keep=0).weave()) : \
                                                   (ifC ? ContraSharpeningHD(output, CClip, planar=planar, HD=if4)                      : LSFmod(output,source=CClip,defaults="slow",strength=Contrasharp,soothe=false,Lmode=0,soft=-2,keep=0))))       : \
                             (lsb ?  (interlaced ? output.weave().DitherPost(u=u,v=v,mode=6,slice=slices,interlaced=true)               : output.DitherPost(mode=mode,slice=slices,ampn=ampn,u=Uin,v=Vin)) : (interlaced ? output.weave() : output))))    \
         :  input

!GlobalO ? (planar ? (lsb ? Dither_YUY2toInterleaved() : (if0 ? MergeChroma(Planar2Interleaved(true), interlaced ? outputI.weave() : outputI) : Planar2Interleaved())) : last) : last """) : \
 \
 \
 \
 \
 \
eval("""

##############
# SHOW PANEL #
##############


  # Common Variables

h     = lsb_in ? h/2 : h
w2    = w*1.0
h2    = h*1.0
hi    = interlaced ? round(h/2.0) : h
AR    = w2/h2
lsp   = interlaced?15:30
lsp   = AR < 1.9 ? lsp : floor(AR < 1.9 ? lsp : ((AR>2.4) ? 0 : lsp-((AR-1.9)*(lsp/(2.4-1.9)))))
ys    = round(lsb_in ? h2/6 : (interlaced?h2/15:h2/6))
s     = floor(((interlaced?w2/1.6:w2)/720.0)*16.0)
type  = planar?"YUY2":"YV12"

Wadd  = w2/40
Wadd2 = Wadd/2
w1    = round(w2+Wadd+Wadd2)
w3    = (w2/3)+Wadd/2.1
w2    = round(w1+w3)
w3    = round(w1+(w3+Wadd2)*1.96)
w3sq  = string(round(w3*1.01))
w3s   = string(w3)

  # Panel Separator

blkclp = BlankClip(interlaced?assumeframebased(pref):pref,length=1,pixel_type=type,color=$161616)
         \.crop(0,interlaced?h/2%8:h%8,0,0).separatefields().assumeframebased().separatefields()

Interleave(selecteven(blkclp).addborders(2,0,0,0,color=$7f7f7f).crop(0,0,-2,0),selectodd(blkclp))

WOut=weave().assumefieldbased().weave()
sep=round((hi-height(WOut))/2.0)

StackHorizontal(planar? pref.Planar2Interleaved() : pref,WOut.addborders(0,sep-sep%2,0,hi-(height(WOut)+(sep-sep%2)),color=$161616))



  # Color Guide Squares

a=interlaced?h*0.37:h*0.74
b=interlaced?h*0.40:h*0.80
c=interlaced?h*0.43:h*0.86

!planar ? mt_merge(mt_lut(yexpr="127",uexpr="183",vexpr="47" ,u=3,v=3) ,mt_lutspa(mode="absolute", yexpr="x "+w3s+" >= x "+w3sq+" <= & y "+string(a)+" >= y "+string(interlaced?h*0.3835:h*0.767)+" <= & & 255 0 ?").trim(0,-1).FreezeFrame(0, FrameCount(pref)-1, 0),luma=true) : nop()
!planar ? mt_merge(mt_lut(yexpr="81" ,uexpr="91" ,vexpr="240",u=3,v=3) ,mt_lutspa(mode="absolute", yexpr="x "+w3s+" >= x "+w3sq+" <= & y "+string(b)+" >= y "+string(interlaced?h*0.4135:h*0.827)+" <= & & 255 0 ?").trim(0,-1).FreezeFrame(0, FrameCount(pref)-1, 0),luma=true) : nop()
!planar ? mt_merge(mt_lut(yexpr="210",uexpr="16" ,vexpr="146",u=3,v=3) ,mt_lutspa(mode="absolute", yexpr="x "+w3s+" >= x "+w3sq+" <= & y "+string(c)+" >= y "+string(interlaced?h*0.4435:h*0.887)+" <= & & 255 0 ?").trim(0,-1).FreezeFrame(0, FrameCount(pref)-1, 0),luma=true) : nop()


  # Parameters List

blue  = sShow ? (show=="Speed"   ? true : false) : false
red   = sShow ? (show=="Memory"  ? true : false) : false
yellow= sShow ? (show=="Quality" ? true : false) : false

Color = sShow ? (blue   ? $00aef0 : \
                 red    ? $ff0000 : \
                 yellow ? $ffff00 : \
                 Assert(false,    "Revise typo for possible color guide modes: 'Speed','Memory','Quality'")) : nop()



varn = "\n\n\n\n\n"
sp   = !GlobalR && str==1.0 ? "  " : ""

Subtitle("SMDegrain Mod v3.1.2",text_color=red||blue||yellow?Color:$7f7f7f,halo_color=$000000,font="VERDANA",size=s*2,x=w1,y=lsb_in?h/40:(interlaced?h/50:h/20))

Subtitle("BASIC"   ,lsp=lsp,text_color=$ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle(            "\n\ntr           = " + string(tr)                                                                              + \
         varn+varn+      "chroma       = " + string(chroma)                                                                          ,lsp=lsp,text_color=red||blue               ?Color:$ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle(          "\n\n\nthSAD(C)     = " + string( GlobalO?"bypass":string(thSAD)     + "(" + string(plane0?thSADC:"bypass") + ")")+ \
         varn+           "CClip        = " + string( GlobalO?"bypass":(defined(CClip)?"defined":"undefined"))                        + \
                     "\n\nInterlaced   = " + string(interlaced)                                                                      ,lsp=lsp,text_color=                               $ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle(varn+           "RefineMotion = " + string( GlobalO?"bypass":RefineMotion)                                                  ,lsp=lsp,text_color=yellow                  ?Color:$ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle(varn+       "\n\nContrasharp  = " + string( GlobalO?"bypass":contrasharp)                                                   ,lsp=lsp,text_color=yellow&&!GlobalO        ?Color:$ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle(varn+varn+    "\nplane        = " + string( GlobalO?"bypass":(plane==0?"0 (Luma)":plane==1?"1 (U plane)":plane==2?"2 (V plane)":plane==3?"3 (Chroma)":"4(Luma+Chroma)")),lsp=lsp,text_color=(red||blue)&&!GlobalO?Color:$ffffff,font="COURIER NEW",size=s,x=w1,y=ys)
Subtitle("<-- "+string(IsClip(prefilter)||prefilter!=-1?"Prefiltered clip converted\n    to PC levels":"Clip converted to PC levels\n   ")+" for the motion search",lsp=lsp,text_color=$ffffff,font="COURIER NEW",size=s,x=w1-10,y=(interlaced?h/2:h)-int(ys/1.5))

Subtitle("ADVANCED",lsp=lsp,text_color=$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(              "\n\npel        = " + string(pel)                                                                             ,lsp=lsp,text_color=red                     ?Color:$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(            "\n\n\nsubpixel   = " + string( pel==1 ?"bypass":subpixel)                                                      ,lsp=lsp,text_color=(yellow||blue)&&pel!=1  ?Color:$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(varn+varn+  "\n\n\ntruemotion = " + string( GlobalR?"bypass":truemotion)                                                    ,lsp=lsp,text_color=(yellow||blue)&&!GlobalR?Color:$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(varn+             "mfilter    = " + string( GlobalR?"bypass":MFilterB?"Clip"+sp+" Variable":"OFF")                          ,lsp=lsp,text_color=                               $7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(varn+         "\n\nStr        = " + (GlobalR?string("bypass"):string(Str,"%1.2f"))+sp+" Brightness"                         + \
         varn+             "search     = " + string( GlobalR?"bypass":search)                                                        ,lsp=lsp,text_color=yellow&&!GlobalR        ?Color:$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(varn+varn+        "blksize    = " + string( GlobalR?"bypass":blksize)                                                       + \
                         "\noverlap    = " + string( GlobalR?"bypass":overlap)                                                       ,lsp=lsp,text_color=(red||blue)&&!GlobalR   ?Color:$7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)
Subtitle(varn+           "\nprefilter  = " + (GlobalR?string("bypass"):(preclip?"Clip"+sp+" Variable":(prefilter==0?"0"+sp+"    MinBlur(0)":prefilter==1?"1"+sp+"    MinBlur(1)":prefilter==2?"2"+sp+"    MinBlur(2)":prefilter==3?"3"+sp+"    Dfttest":prefilter==4?"4"+sp+"    KNLmeans":"OFF"+sp+"    (-1)"))) + \
                       "\n\nAmp        = " + (GlobalR||str==1.0?string("bypass"):string(Amp,"%1.2f"))+" Amplitude"                   + \
         varn+           "\nlimit(C)   = " + (GlobalO?string("bypass"):string(limit)     + "(" + string(limitc) + ")")               ,lsp=lsp,text_color=                               $7f7f7f,font="COURIER NEW",size=s,x=w2,y=ys)

Subtitle("OTHERS"  ,lsp=lsp,text_color=$494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(             "\n\n\nthSCD1(2) = " + (GlobalO?string("bypass"):string(thSCD1) + "(" + string(thSCD2) + ")")                  + \
                   varn+"\n\nGlobals   = " + string(Globals==0 ? "0 (OFF)" : Globals==1 ? "1 (Import)" : Globals==2 ? "2 (Export)" : "3 (E.Only)"),lsp=lsp,text_color=                  $494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(          varn+"\n\nslices    = " + string(slices)                                                                          ,lsp=lsp,text_color=blue                    ?Color:$494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(               "\n\nhpad(v)   = " + string(hpad)   + "(" + string(vpad)   + ")"                                             ,lsp=lsp,text_color=red||blue               ?Color:$494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(varn+              "lsb       = " + string(lsb)                                                                             ,lsp=lsp,text_color=yellow                  ?Color:$494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(varn+            "\nlsb_in/out= " + string(lsb_in) +"/"+ string(lsb_out)                                                    ,lsp=lsp,text_color=red&&lsb_in             ?Color:$494949,font="COURIER NEW",size=s,x=w3,y=ys)
Subtitle(varn+        "\n\n\nmode      = " + (GlobalO||(lsb_out||!lsb)?string("bypass"):interlaced?"6 (locked)":string(mode))        ,lsp=lsp,text_color=yellow&&!GlobalO&&lsb   ?Color:$494949,font="COURIER NEW",size=s,x=w3,y=ys)

Subtitle("   Speed"   ,lsp=interlaced?15:30,text_color=planar?$00aef0:$7f7f7f,font="COURIER NEW",size=s,x=w3,y=int(a-1))
Subtitle("   Memory"  ,lsp=interlaced?17:34,text_color=planar?$ff0000:$7f7f7f,font="COURIER NEW",size=s,x=w3,y=int(b-1))
Subtitle("   Quality" ,lsp=interlaced?19:38,text_color=planar?$ffff00:$7f7f7f,font="COURIER NEW",size=s,x=w3,y=int(c-1))

lsb_out ? StackVertical (last, BlankClip (last, pixel_type=type, color_yuv=0,length=1)) : last""")}





########################################################
#                                                      #
# HELPER FUNCTIONS: For Contrasharp and YUY2 handling  #
#                                                      #
########################################################

########################################
## Didée's functions:

 # contra-sharpening: sharpen the denoised clip, but don't add more to any pixel than what was removed previously.
 # script function from Didée, at the VERY GRAINY thread (http://forum.doom9.org/showthread.php?p=1076491#post1076491)

 # In final version 2.0d ContraHD() was merged, to allow proper HD sharpening.
 # In this case global variables of (before denoising) source MSuper and forward and backward
 # compensated motion vectors are necessary as: Super, cb1 and cf1, if used as a stand alone function.
 # Don't know who made (mod) it, so I can't give proper credits, sorry.

FUNCTION ContraSharpeningHD(clip denoised, clip original, bool "HD", bool "planar", int "overshoot"){
HD        = default(HD    ,false)
planar    = default(planar,false)
overshoot = default(overshoot,0)
avs26 = VersionNumber() < 2.60 ? false : true
yuy26 = avs26 && Isyuy2(denoised) && planar

HD ? eval("""
cb1=original.MCompensate(Super, cb1, planar=planar)
cf1=original.MCompensate(Super, cf1, planar=planar)
pmax = yuy26 ? original.Planar2Interleaved().ConvertToYV16().mt_logic(cb1.Planar2Interleaved().ConvertToYV16(), "max").mt_logic(cf1.Planar2Interleaved().ConvertToYV16(), "max") : original.mt_logic(cb1, "max").mt_logic(cf1, "max")
pmin = yuy26 ? original.Planar2Interleaved().ConvertToYV16().mt_logic(cb1.Planar2Interleaved().ConvertToYV16(), "min").mt_logic(cf1.Planar2Interleaved().ConvertToYV16(), "min") : original.mt_logic(cb1, "min").mt_logic(cf1, "min")""") : nop()

s    = denoised.MinBlur(HD?2:1,1,planar=planar)                                   # Damp down remaining spots of the denoised clip.
s    = yuy26 ? s.Planar2Interleaved().ConvertToYV16() : s
original = yuy26 ? original.Planar2Interleaved().ConvertToYV16() : original
denoised = yuy26 ? denoised.Planar2Interleaved().ConvertToYV16() : denoised
allD = mt_makediff(original,denoised)                                             # The difference achieved by the denoising.
ssD  = mt_makediff(s,HD?s.removegrain(20,-1,planar=planar).\
                          removegrain(20,-1,planar=planar):\
                        s.removegrain(11,-1,planar=planar))                       # The difference of a simple kernel blur.
allD = yuy26 ? allD.ConvertToYUY2().Interleaved2planar() : allD
ssD  = yuy26 ? ssD.ConvertToYUY2().Interleaved2planar() : ssD
ssDD = ssD.repair(HD?ssD.repair(allD,1,planar=planar):allD,HD?12:1,planar=planar) # Limit the difference to the max of what the denoising removed locally.
ssDD = yuy26 ? ssDD.Planar2Interleaved().ConvertToYV16() : ssDD
ssD  = yuy26 ? ssD.Planar2Interleaved().ConvertToYV16() : ssD
ssDD = SSDD.mt_lutxy(ssD,"x 128 - abs y 128 - abs < x y ?")                       # abs(diff) after limiting may not be bigger than before.

denoised.mt_adddiff(ssDD,U=2,V=2)                                                 # Apply the limited difference. (Sharpening is just inverse blurring)
HD ? mt_clamp(last,pmax,pmin,overshoot,overshoot,chroma="copy first") : last
yuy26 ? last.ConvertToYUY2().Interleaved2planar() : last
}

 # MinBlur   by Didée (http://avisynth.org/mediawiki/MinBlur)
 # Nifty Gauss/Median combination (Modified for Planar input)

function MinBlur(clip clp, int "r", int "uv", bool "planar"){

r      = default(r,1)
uv     = default(uv,3)
planar = default(planar,false)
avs26 = VersionNumber() < 2.60 ? false : true
yuy26 = avs26 && Isyuy2(clp) && planar

uv2    = (uv==2) ? 1  : uv
rg4    = (uv==3) ? 4  : -1
rg11   = (uv==3) ? 11 : -1
rg20   = (uv==3) ? 20 : -1
uvm2   = r==2 ? (uv==3?2:uv==2?0:-1) : nop()
uvm3   = r==3 ? (uv==3?3:uv==2?0:-1) : nop()
medf   = (uv==3) ? 1 : -200

RG11D = (r==0) ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.sbr(planar=planar).Planar2Interleaved().ConvertToYV16(),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.sbr(planar=planar),U=uv2,V=uv2)
 \    : (r==1) ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.removegrain(11,rg11,planar=planar).Planar2Interleaved().ConvertToYV16(),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.removegrain(11,rg11,planar=planar),U=uv2,V=uv2)
 \    : (r==2) ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.removegrain(11,rg11,planar=planar).removegrain(20,rg20,planar=planar).Planar2Interleaved().ConvertToYV16(),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.removegrain(11,rg11,planar=planar).removegrain(20,rg20,planar=planar),U=uv2,V=uv2)
 \    :          yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.removegrain(11,rg11,planar=planar).removegrain(20,rg20,planar=planar).removegrain(20,rg20,planar=planar).Planar2Interleaved().ConvertToYV16(),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.removegrain(11,rg11,planar=planar).removegrain(20,rg20,planar=planar).removegrain(20,rg20,planar=planar),U=uv2,V=uv2)
RG4D  = (r<=1) ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.removegrain(4,rg4,planar=planar).Planar2Interleaved().ConvertToYV16(),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.removegrain(4,rg4,planar=planar),U=uv2,V=uv2)
 \    : (r==2) ? avs26 ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.Planar2Interleaved().ConvertToYV16().medianblur(2,2*medf,2*medf),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.medianblur(2,2*medf,2*medf),U=uv2,V=uv2) : mt_makediff(clp,clp.Quantile(radius_y=2, radius_u=uvm2, radius_v=uvm2, planar=planar), U=uv2,V=uv2)
 \    :          avs26 ? yuy26 ? mt_makediff(clp.Planar2Interleaved().ConvertToYV16(),clp.Planar2Interleaved().ConvertToYV16().medianblur(3,3*medf,3*medf),U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_makediff(clp,clp.medianblur(3,3*medf,3*medf),U=uv2,V=uv2) : mt_makediff(clp,clp.Quantile(radius_y=3, radius_u=uvm3, radius_v=uvm3, planar=planar), U=uv2,V=uv2)
DD    = yuy26 ? mt_lutxy(RG11D.Planar2Interleaved().ConvertToYV16(),RG4D.Planar2Interleaved().ConvertToYV16(),"x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?",U=uv2,V=uv2).ConvertToYUY2().Interleaved2planar() : mt_lutxy(RG11D,RG4D,"x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?",U=uv2,V=uv2)
return (yuy26 ? clp.Planar2Interleaved().ConvertToYV16().mt_makediff(DD.Planar2Interleaved().ConvertToYV16(),U=uv,V=uv).ConvertToYUY2().Interleaved2planar() : clp.mt_makediff(DD,U=uv,V=uv)) }


 # helper function: make a highpass on a blur's difference (well, kind of that)
 # Highpass of spatial r=1 Gaussian (like blur(1.0)) (Modified for Planar input)

function sbr(clip c, bool "planar") {

planar = default(planar,false)
avs26 = VersionNumber() < 2.60 ? false : true
yuy26 = avs26 && Isyuy2(c) && planar

rg11   = c.removegrain(11,-1,planar=planar)
rg11D  = yuy26 ? mt_makediff(c.Planar2Interleaved().ConvertToYV16(),rg11.Planar2Interleaved().ConvertToYV16()).ConvertToYUY2().Interleaved2planar() : mt_makediff(c,rg11)
rg11DD = yuy26 ? mt_makediff(rg11D.Planar2Interleaved().ConvertToYV16(),rg11D.removegrain(11,-1,planar=planar).Planar2Interleaved().ConvertToYV16()).mt_lutxy(rg11D.Planar2Interleaved().ConvertToYV16(),"x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?").ConvertToYUY2().Interleaved2planar() : mt_makediff(rg11D,rg11D.removegrain(11,-1,planar=planar)).mt_lutxy(rg11D,"x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?")
yuy26 ? c.Planar2Interleaved().ConvertToYV16().mt_makediff(rg11DD.Planar2Interleaved().ConvertToYV16(),U=2,V=2).ConvertToYUY2().Interleaved2planar() : c.mt_makediff(rg11DD,U=2,V=2) }

########################################
## cretindesalpes' functions: (http://forum.doom9.org/showthread.php?p=1505297#post1505297)

# Converts 16 bit interleaved YUY2 to 16bit planar YUY2 suitable for Ditherpost. (Use right before Ditherpost; this format is not suitable for filtering)

function Dither_YUY2toPlanar16(c){c
yuy26   = !(VersionNumber() < 2.60) && isyuy2(c)
my = !yuy26 ? Dither_get_msb().ConvertToYV12() : nop()
ly = !yuy26 ? Dither_get_lsb().ConvertToYV12() : nop()
u = !yuy26 ? UToY ().ConvertToYV12 () : nop()
v = !yuy26 ? VToY ().ConvertToYV12 () : nop()
yuy26 ? ConvertToYV16() : YToUV (u, v, StackVertical (StackVertical (my, my), StackVertical (ly, ly)))}


# Converts 8 bit interleaved YUY2 to 8 bit planar YUY2 ready for either 8 or 16 bit processing also suitable for Ditherpost

function Dither_YUY2toPlanar(c){c
yuy26   = !(VersionNumber() < 2.60) && isyuy2(c)
y = !yuy26 ? ConvertToYV12 () : nop()
u = !yuy26 ? UToY ().ConvertToYV12 () : nop()
v = !yuy26 ? VToY ().ConvertToYV12 () : nop()
yuy26 ? ConvertToYV16() : YToUV (u, v, StackVertical (y, y))}


# Converts back the above special planar formats to normal interleaved 16 bit YUY2

function Dither_YUY2toInterleaved16(c){c
yuy26   = !(VersionNumber() < 2.60)
y = !yuy26 ? StackVertical (Crop (0, 0, 0, (Height () /(4))), Crop (0, (Height () /(2))+(Height () /(4)), 0, 0)).ConvertToYUY2 () : nop()
u = !yuy26 ? UToY ().ConvertToYUY2 () : nop()
v = !yuy26 ? VToY ().ConvertToYUY2 () : nop()
yuy26 ? ConvertToYUY2() : YToUV (u, v, y)}

# Converts back the above special planar formats to normal interleaved YUY2

function Dither_YUY2toInterleaved(c){c
yuy26   = !(VersionNumber() < 2.60)
y = !yuy26 ? Crop (0, 0, 0, Height () /(2)).ConvertToYUY2 () : nop()
u = !yuy26 ? UToY ().ConvertToYUY2 () : nop()
v = !yuy26 ? VToY ().ConvertToYUY2 () : nop()
yuy26 ? ConvertToYUY2() : YToUV (u, v, y)}



# Converts luma (and chroma) to PC levels, and optionally allows tweaking for pumping up the darks. (for the clip to be fed to motion search only)
# By courtesy of cretindesalpes. (http://forum.doom9.org/showthread.php?p=1548318#post1548318)

function Dither_Luma_Rebuild (clip src, float "s0", float "c",int "uv", bool "lsb", bool "lsb_in", bool "lsb_out", int "mode", float "ampn", bool "slice"){
lsb_in  = Default( lsb_in  ,false)
lsb_out = Default( lsb_out ,false)
lsb     = Default( lsb     ,lsb_in || lsb_out)
mode    = Default( mode ,6)
uv      = Default(uv,    3)
s0      = Default(s0,  2.0)
c       = Default(c,  1.0/16)
ampn    = Default(ampn, 0.0)
slice   = Default(slice, true)
yuy26   = !(VersionNumber() < 2.60) && isyuy2(src)
src     = yuy26 ? src.Planar2Interleaved().ConvertToYV16() : src

    k = (s0 - 1) * c
    t = lsb_in ? "x 4096 - 56064 / 0 1 clip" : "x 16 - 219 / 0 1 clip"
    e = String(k)+" "+String(1+c)+" "+String((1+c)*c)+" "+t+" "+String(c)
\       +" + / - * "+t+" 1 "+String(k)+" - * + "+String(lsb?65536:255)+" *"
src
lsb ? (lsb_in ? Dither_lut16 (yexpr=e,expr="x 32768 - 32768 * 28672 / 32768 +",y=3, u=uv, v=uv)   : \
                Dither_lut8  (yexpr=e,expr="x 128 - 32768 * 112 / 32768 +"    ,y=3, u=uv, v=uv))  : \
                mt_lut       (yexpr=e,expr="x 128 - 128 * 112 / 128 +"        ,y=3, u=uv, v=uv)
lsb_out ? last : (lsb ? Ditherpost(mode=mode,ampn=ampn,slice=slice,staticnoise=ampn!=0.0?true:false,u=uv,v=uv) : last)
return yuy26 ? last.ConvertToYUY2().Interleaved2planar() : last
}