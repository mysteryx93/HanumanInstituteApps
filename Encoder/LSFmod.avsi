################################################################################################
###                                                                                          ###
###                       LimitedSharpenFaster MOD : function LSFmod()                       ###
###                                                                                          ###
###                                Modded Version by LaTo INV.                               ###
###                                                                                          ###
###                                  v1.9 - 05 October 2009                                  ###
###                                                                                          ###
###                            another Modded Version by A.SONY.                             ###
###                                                                                          ###
###                                  v2.1 - 13 July 2017                                     ###
###                                                                                          ###
################################################################################################
###
### +-----------+
### | CHANGELOG |
### +-----------+
###
### v2.1 : - HBD in avs+
###
### v1.9 : - tweaked settings
###        - default preset is now defaults="fast" /!\
###
### v1.8 : - changed preblur to allow more tweaking (bool->string)
###        - tweaked settings
###        - cleaned the code
###        - updated documentation
###
### v1.7 : - changed Smethod=4 to "source"
###
### v1.6 : - added preblur option
###        - added new Smethod=4
###
### v1.5 : - fixed LUT expression (thanks to Did�e)
###        - changed Smethod to Smethod+secure
###
### v1.4 : - changed defaults="new" to defaults="slow" & defaults="fast"
###        - added show parameter
###        - cleaned a little the code
###
### v1.3 : - changed a little Smethod=3&5 (same effect, but more precise)
###        - added new calculation for soft (soft=-2) [default on]
###        - added warning about bad settings (no more silent)
###        - updated the documentation
###
### v1.2 : - added new Lmode<0 (limit with repair)
###        - added 2 new Smode (unsharp masking)
###        - changed Smode order: now old Smode3-4 is new Smode3-4 to avoid mistake
###
### v1.1 : - fixed a bug with dest_x!=ox or dest_y!=oy
###        - replaced Lfactor by over/undershoot2
###
### v1.0 : - deleted old Smode(1-4), added new Smode(1-3) & Smethod(1-5)
###        - added parameters for nonlinear sharpening (S2zp,S2pwr,S2dmpLo,S2dmpHi)
###        - corrected the nonlinear formula
###        - added new Lmode 2 & 4 + fixed Lmode 0
###        - added faster edgemask
###        - added soothe temporal stabilization, 2 parameters: soothe & keep
###        - replaced lanczosresize by spline36resize
###        - moved "strength" parameter (first place)
###        - deleted wide, special and exborder
###        - changed some code (cosmetic)
###        - added "defaults" parameter (to switch between original and modded version)
###        - added documentation
###
###
###
### +--------------+
### | DEPENDENCIES |
### +--------------+
###
### -> Masktools    (v2a36)
### -> Removegrain  (v1.0pre)
### -> Warpsharp    (2003.11.03) [for Smode=1]
### -> Variableblur (v0.30)      [for Smode=2]
###
###
###
### +-------+
### | USAGE |
### +-------+
###
### LSFmod( strength, Smode, Smethod, kernel, 
###         preblur, secure, source,
###         Szrp, Spwr, SdmpLo, SdmpHi,
###         Lmode, overshoot, undershoot, overshoot2, undershoot2,
###         soft, soothe, keep,
###         edgemode, edgemaskHQ,
###         ss_x, ss_y, dest_x, dest_y,
###         show, screenW, screenH,
###         defaults )
###
###
###
### +---------+
### | GENERAL |
### +---------+
###
### strength [int]
### --------------
### Strength of the sharpening
###
### Smode [int: 1,2,3,4,5]
### ----------------------
### Sharpen mode:
###    =1 : Unsharp masking (from warpsharp)
###    =2 : Unsharp masking (from variableblur) 
###    =3 : Range sharpening
###    =4 : Nonlinear sharpening (original  version)
###    =5 : Nonlinear sharpening (corrected version)
###
### Smethod [int: 1,2,3]
### --------------------
### Sharpen method: (only used in Smode=3,4,5)
###    =1 : 3x3 kernel
###    =2 : Min/Max
###    =3 : Min/Max + 3x3 kernel
###
### kernel [int: 11,12,19,20]
### -------------------------
### Kernel used in Smethod=1&3
### In strength order: + 19 > 12 >> 20 > 11 -
###
###
###
### +---------+
### | SPECIAL |
### +---------+
###
### preblur [string: "ON","OFF",...]
### --------------------------------
### Mode to avoid noise sharpening & ringing (only used in Smode=3,4,5)
### "ON" is sufficient to prevent ringing, but to prevent noise sharpening you should set your own denoiser
###    Usage:   LSFmod(preblur="YourFavoriteDenoiser()")
###    Example: LSFmod(preblur="FFT3Dfilter(sigma=4,plane=0)")
###
###
### secure [bool]
### -------------
### Mode to avoid banding & oil painting (or face wax) effect of sharpening
### (only used in Smode=3,4,5)
###
### source [clip]
### -------------
### If source is defined, LSFmod doesn't sharp more a denoised clip than this source clip
### In this mode, you can safely set Lmode=0 & PP=off
###    Usage:   denoised.LSFmod(source=source)
###    Example: last.FFT3Dfilter().LSFmod(source=last,Lmode=0,soft=0)
###
###
###
### +----------------------+
### | NONLINEAR SHARPENING |
### +----------------------+
###
### Szrp [int]
### ----------
### Zero Point:
###    - differences below Szrp are amplified (overdrive sharpening)
###    - differences above Szrp are reduced   (reduced sharpening)
###
### Spwr [int]
### ----------
### Power: exponent for sharpener
###
### SdmpLo [int]
### ------------
### Damp Low: reduce sharpening for small changes [0:disable]
###
### SdmpHi [int]
### ------------
### Damp High: reduce sharpening for big changes [0:disable]
###
###
###
### +----------+
### | LIMITING |
### +----------+
###
### Lmode [int: ...,0,1,2,3,4]
### --------------------------
### Limit mode:
###    <0 : Limit with repair (ex: Lmode=-1 --> repair(1), Lmode=-5 --> repair(5)...)
###    =0 : No limit
###    =1 : Limit to over/undershoot
###    =2 : Limit to over/undershoot on edges and no limit on not-edges
###    =3 : Limit to zero on edges and to over/undershoot on not-edges
###    =4 : Limit to over/undershoot on edges and to over/undershoot2 on not-edges
###
### overshoot [int]
### ---------------
### Limit for pixels that get brighter during sharpening
###
### undershoot [int]
### ----------------
### Limit for pixels that get darker during sharpening
###
### overshoot2 [int]
### ----------------
### Same as overshoot, only for Lmode=4
###
### undershoot2 [int]
### -----------------
### Same as undershoot, only for Lmode=4
###
###
###
### +-----------------+
### | POST-PROCESSING |
### +-----------------+
###
### soft [int: -2,-1,0...100]
### -------------------------
### Soft the sharpening effect (-1 = old autocalculate, -2 = new autocalculate)
###
### soothe [bool]
### -------------
###    =true  : Enable soothe temporal stabilization
###    =false : Disable soothe temporal stabilization
###
### keep [int: 0...100]
### -------------------
### Minimum percent of the original sharpening to keep (only with soothe=true)
###
###
###
### +-------+
### | EDGES |
### +-------+
###
### edgemode [int: -1,0,1,2]
### ------------------------
###    =-1 : Show edgemask
###    = 0 : Sharpening all
###    = 1 : Sharpening only edges
###    = 2 : Sharpening only not-edges
###
### edgemaskHQ [bool]
### -----------------
###    =true  : Original edgemask
###    =false : Faster edgemask
###
###
###
### +------------+
### | UPSAMPLING |
### +------------+
###
### ss_x ; ss_y [float]
### -------------------
### Supersampling factor (reduce aliasing on edges)
###
### dest_x ; dest_y [int]
### ---------------------
### Output resolution after sharpening (avoid a resizing step)
###
###
###
### +-------+
### | DEBUG |
### +-------+
###
### show [bool]
### -----------
### Show debug clip & informations
###
### screenW ; screenH [int]
### -----------------------
### Screen resolution (for show clip)
###
###
###
### +----------+
### | SETTINGS |
### +----------+
###
### defaults [string: "old" or "slow" or "fast"]
### --------------------------------------------
###    = "old"  : Reset settings to original version (output will be THE SAME AS LSF)
###    = "slow" : Enable SLOW modded version settings
###    = "fast" : Enable FAST modded version settings
###  --> /!\ [default:"fast"]
###
###
### defaults="old" :  - strength    = 100
### ----------------  - Smode       = 3
###                   - Smethod     = Smode==3?2:1
###                   - kernel      = 11
###
###                   - preblur     = "OFF"
###                   - secure      = false
###                   - source      = undefined
###
###                   - Szrp        = 16
###                   - Spwr        = 2
###                   - SdmpLo      = strength/25
###                   - SdmpHi      = 0
###
###                   - Lmode       = 1
###                   - overshoot   = 1
###                   - undershoot  = overshoot
###                   - overshoot2  = overshoot*2
###                   - undershoot2 = overshoot2
###
###                   - soft        = 0
###                   - soothe      = false
###                   - keep        = 25
###
###                   - edgemode    = 0
###                   - edgemaskHQ  = true
###
###                   - ss_x        = Smode==3?1.50:1.25
###                   - ss_y        = ss_x
###                   - dest_x      = ox
###                   - dest_y      = oy
###
###                   - show        = false
###                   - screenW     = 1280
###                   - screenH     = 1024
###
###
### defaults="slow" : - strength    = 100
### ----------------- - Smode       = 5
###                   - Smethod     = 3
###                   - kernel      = 11
###
###                   - preblur     = "OFF"
###                   - secure      = true
###                   - source      = undefined
###
###                   - Szrp        = 16
###                   - Spwr        = 4
###                   - SdmpLo      = 4
###                   - SdmpHi      = 48
###
###                   - Lmode       = 4
###                   - overshoot   = strength/100
###                   - undershoot  = overshoot
###                   - overshoot2  = overshoot*2
###                   - undershoot2 = overshoot2
###
###                   - soft        = -2
###                   - soothe      = true
###                   - keep        = 20
###
###                   - edgemode    = 0
###                   - edgemaskHQ  = true
###
###                   - ss_x        = 1.50
###                   - ss_y        = ss_x
###                   - dest_x      = ox
###                   - dest_y      = oy
###
###                   - show        = false
###                   - screenW     = 1280
###                   - screenH     = 1024
###
###
### defaults="fast" : - strength    = 100
### ----------------- - Smode       = 3
###                   - Smethod     = 2
###                   - kernel      = 11
###
###                   - preblur     = "OFF"
###                   - secure      = true
###                   - source      = undefined
###
###                   - Szrp        = 16
###                   - Spwr        = 4
###                   - SdmpLo      = 4
###                   - SdmpHi      = 48
###
###                   - Lmode       = 1
###                   - overshoot   = strength/100
###                   - undershoot  = overshoot
###                   - overshoot2  = overshoot*2
###                   - undershoot2 = overshoot2
###
###                   - soft        = 0
###                   - soothe      = true
###                   - keep        = 20
###
###                   - edgemode    = 0
###                   - edgemaskHQ  = false
###
###                   - ss_x        = 1.25
###                   - ss_y        = ss_x
###                   - dest_x      = ox
###                   - dest_y      = oy
###
###                   - show        = false
###                   - screenW     = 1280
###                   - screenH     = 1024
###
################################################################################################

function LSFmod( clip inclip, float "strength", int "Smode", int "Smethod", int "kernel",
\                string "preblur", bool "secure", clip "source",
\                int "Szrp", int "Spwr", float "SdmpLo", float "SdmpHi",
\                int "Lmode", int "overshoot", int "undershoot", int "overshoot2", int "undershoot2",
\                int "soft", bool "soothe", int "keep",
\                int "edgemode", bool "edgemaskHQ",
\                float "ss_x", float "ss_y", int "dest_x", int "dest_y",
\                bool "show", int "screenW", int "screenH",
\                string "defaults" )
{

ssispmt   = Findstr(VersionString(), "AviSynth+") != 0 && Findstr(VersionString(), "r1576") == 0
 
sislumaonly = ssispmt ? inclip.isy() : VersionNumber() < 2.6 ? true : inclip.isy8()
 
input = sislumaonly ? inclip : ssispmt ? inclip.converttoy() : inclip.converttoy8()

input = defined(source) ? input.pointresize(width(input),height(input)+8,0,-4,width(input),height(input)+8.0001) : input

source = defined(source) ? sislumaonly ? source : ssispmt ? source.converttoy() : source.converttoy8() : source

source = defined(source) ? source.pointresize(width(source),height(source)+8,0,-4,width(source),height(source)+8.0001) : source

### DEFAULTS
version  = "v2.1"
defaults = default(defaults,"fast")
num      = defaults=="old" ? 0 : defaults=="slow" ? 1 : defaults=="fast" ? 2 : 3

ox = input.width
oy = input.height

Assert(!isYUY2(input), chr(10) + "not work with an YUY2 clip !" + chr(10))
Assert(num         != 3      ? true : false, chr(10) + """Defaults must be "old" or "slow" or "fast" !""" + chr(10))

strength    = default( strength,    Select(num,                100 ,                100 ,                100 ) )
Smode       = default( Smode,       Select(num,                  3 ,                  5 ,                  3 ) )
Smethod     = default( Smethod,     Select(num,       Smode==3?2:1 ,                  3 ,                  2 ) )
kernel      = default( kernel,      Select(num,                 11 ,                 11 ,                 11 ) )

preblur     = default( preblur,     Select(num,              "OFF" ,              "OFF" ,              "OFF" ) )
secure      = default( secure,      Select(num,              false ,               true ,               true ) )

Szrp        = default( Szrp,        Select(num,                 16 ,                 16 ,                 16 ) )
Spwr        = default( Spwr,        Select(num,                  2 ,                  4 ,                  4 ) )
SdmpLo      = default( SdmpLo,      Select(num,        strength/25 ,                  4 ,                  4 ) )
SdmpHi      = default( SdmpHi,      Select(num,                  0 ,                 48 ,                 48 ) )

Lmode       = default( Lmode,       Select(num,                  1 ,                  4 ,                  1 ) )
overshoot   = default( overshoot,   Select(num,                  1 ,       strength/100 ,       strength/100 ) )
undershoot  = default( undershoot,  Select(num,          overshoot ,          overshoot ,          overshoot ) )
overshoot2  = default( overshoot2,  Select(num,        overshoot*2 ,        overshoot*2 ,        overshoot*2 ) )
undershoot2 = default( undershoot2, Select(num,         overshoot2 ,         overshoot2 ,         overshoot2 ) )

soft        = default( soft,        Select(num,                  0 ,                 -2 ,                  0 ) )
soothe      = default( soothe,      Select(num,              false ,               true ,               true ) )
keep        = default( keep,        Select(num,                 25 ,                 20 ,                 20 ) )

edgemode    = default( edgemode,    Select(num,                  0 ,                  0 ,                  0 ) )
edgemaskHQ  = default( edgemaskHQ,  Select(num,               true ,               true ,              false ) )

ss_x        = default( ss_x,        Select(num, Smode==3?1.50:1.25 ,               1.50 ,               1.25 ) )
ss_y        = default( ss_y,        Select(num,               ss_x ,               ss_x ,               ss_x ) )
dest_x      = default( dest_x,      Select(num,                 ox ,                 ox ,                 ox ) )
dest_y      = default( dest_y,      Select(num,                 oy ,                 oy ,                 oy ) )

show        = default( show,        Select(num,              false ,              false ,              false ) )
screenW     = default( screenW,     Select(num,               1280 ,               1280 ,               1280 ) )
screenH     = default( screenH,     Select(num,               1024 ,               1024 ,               1024 ) )

Assert( ( strength >= 0 )                                                ? true : false, chr(10) + "'strength' have not a correct value! [>=0]" + chr(10))
Assert( ( Smode >= 1 && Smode <= 5 )                                     ? true : false, chr(10) + "'Smode' have not a correct value! [1,2,3,4,5]" + chr(10))
Assert( ( Smethod >= 1 && Smethod <= 4 )                                 ? true : false, chr(10) + "'Smethod' have not a correct value! [1,2,3,4]" + chr(10))
Assert( ( kernel == 19 || kernel == 20 || kernel == 11 || kernel == 12 ) ? true : false, chr(10) + "'kernel' have not a correct value! [19,20,11,12]" + chr(10))
Assert( ( Szrp >= 1 && Szrp <= 255 )                                     ? true : false, chr(10) + "'Szrp' have not a correct value! [1...255]" + chr(10))
Assert( ( Spwr >= 1 )                                                    ? true : false, chr(10) + "'Spwr' have not a correct value! [>=1]" + chr(10))
Assert( ( SdmpLo >= 0 && SdmpLo <= 255 )                                 ? true : false, chr(10) + "'SdmpLo' have not a correct value! [0...255]" + chr(10))
Assert( ( SdmpHi >= 0 && SdmpHi <= 255 )                                 ? true : false, chr(10) + "'SdmpHi' have not a correct value! [0...255]" + chr(10))
Assert( ( Lmode <= 4 )                                                   ? true : false, chr(10) + "'Lmode' have not a correct value! [...-1,0,1,2,3,4]" + chr(10))
Assert( ( overshoot >= 0 && overshoot <= 255 )                           ? true : false, chr(10) + "'overshoot' have not a correct value! [0...255]" + chr(10))
Assert( ( undershoot >= 0 && undershoot <= 255 )                         ? true : false, chr(10) + "'undershoot' have not a correct value! [0...255]" + chr(10))
Assert( ( overshoot2 >= 0 && overshoot2 <= 255 )                         ? true : false, chr(10) + "'overshoot2' have not a correct value! [0...255]" + chr(10))
Assert( ( undershoot2 >= 0 && undershoot2 <= 255 )                       ? true : false, chr(10) + "'undershoot2' have not a correct value! [0...255]" + chr(10))
Assert( ( soft >= -2 && soft <= 100 )                                    ? true : false, chr(10) + "'soft' have not a correct value! [-2,-1,0,1...100]" + chr(10))
Assert( ( keep >= 0 && keep <= 100 )                                     ? true : false, chr(10) + "'keep' have not a correct value! [0...100]" + chr(10))
Assert( ( edgemode >= -1 && edgemode <= 2 )                              ? true : false, chr(10) + "'edgemode' have not a correct value! [-1,0,1,2]" + chr(10))
Assert( ( ss_x >= 1.0 )                                                  ? true : false, chr(10) + "'ss_x' have not a correct value! [>=1.0]" + chr(10))
Assert( ( ss_y >= 1.0 )                                                  ? true : false, chr(10) + "'ss_y' have not a correct value! [>=1.0]" + chr(10))

soft = soft!=-1  ? soft : sqrt( (((ss_x+ss_y)/2.0-1.0)*100.0) ) * 10
soft = soft!=-2  ? soft : int( (1.0+(2.0/(ss_x+ss_y))) * sqrt(strength) )
soft = soft<=100 ? soft : 100

xxs  = round(ox*ss_x/8)*8
yys  = round(oy*ss_y/8)*8

str  = float(strength)/100.0


### SHARP
tmp  = ss_x > 1.0 || ss_y > 1.0 ? input.spline36resize(xxs,yys) : input
pre  = preblur=="OFF" ? tmp
\    : preblur=="ON"  ? VersionNumber() < 2.6 ? tmp.mt_makediff( mt_lutxy(mt_makediff(tmp,tmp.removegrain(11,-1),U=1,V=1)
\                                       			,mt_makediff(tmp,tmp.removegrain(4,-1),U=1,V=1)
\                                       			,"x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?",U=1,V=1) ,U=1,V=1) \
					      : tmp.mt_makediff( mt_lutxy(mt_makediff(tmp,tmp.removegrain(11,-1),U=1,V=1)
\                                       			,mt_makediff(tmp,tmp.removegrain(4,-1),U=1,V=1)
\                                       			,"x range_half - y range_half - * 0 < range_half x range_half - abs y range_half - abs < x y ? ?",U=1,V=1) ,U=1,V=1)
\    : eval("tmp."+preblur)

dark_limit   = pre.mt_inpand(U=1,V=1)
bright_limit = pre.mt_expand(U=1,V=1)
minmaxavg    = mt_average(dark_limit,bright_limit,U=1,V=1)

method = Smethod==1   ? pre.removegrain(kernel,-1)
\      : Smethod==2   ? minmaxavg
\      :                minmaxavg.removegrain(kernel,-1)

method = secure==true ? VersionNumber() < 2.6 ? method.mt_lutxy(pre,"x y < x 1 + x y > x 1 - x ? ?",U=1,V=1) \
					      : method.mt_lutxy(pre,"x y < x 1 scalef + x y > x 1 scalef - x ? ?",U=1,V=1)
\      :                method

method = preblur=="OFF" ? method : tmp.mt_makediff(mt_makediff(pre,method,U=1,V=1),U=1,V=1)

normsharp = Smode==1 ? tmp.unsharpmask(strength=int(str*100),radius=round(1.0+((ss_x+ss_y)/2.0)),threshold=0)
\         : Smode==2 ? tmp.unsharp(strength=str,vary=((ss_x+ss_y)/2.0),varc=1,border=3,U=1,V=1)
\         : Smode==3 ? VersionNumber() < 2.6 ? mt_lutxy(tmp, method, yexpr="x x y - "+string(str)+" * +",U=1,V=1) \
					     : mt_lutxy(tmp, method, yexpr="clamp_f_i8 x x y - "+string(str)+" * +",U=1,V=1)
\         : Smode==4 ? VersionNumber() < 2.6 ? mt_lutxy(tmp, method, yexpr="x y == x x x y - abs "+string(Szrp)+" / 1 "+string(Spwr)+" / ^ "+string(Szrp)+" * "+string(str)+" * x y - 2 ^ x y - 2 ^ "
\                                                   			    +string(SdmpLo)+" + / * x y - x y - abs / * 1 "+string(SdmpHi)+" 0 == 0 x y - abs "+string(SdmpHi)+" / 4 ^ ? + / + ?",U=1,V=1) \
					     : mt_lutxy(tmp, method, yexpr="clamp_f_i8 x y == x x x y - abs "+string(Szrp)+" / 1 "+string(Spwr)+" / ^ "+string(Szrp)+" * "+string(str)+" * x y - 2 ^ x y - 2 ^ "
\                                                   			    +string(SdmpLo)+" scalef + / * x y - x y - abs / * 1 "+string(SdmpHi)+" 0 == 0 x y - abs "+string(SdmpHi)+" / 4 ^ ? + / + ?",U=1,V=1)
\         :            VersionNumber() < 2.6 ? mt_lutxy(tmp, method, yexpr="x y == x x x y - abs "+string(Szrp)+" / 1 "+string(Spwr)+" / ^ "+string(Szrp)+" * "+string(str)+" * x y - x y - abs / * x y - 2 ^ "
\                                                   			    +string(Szrp)+" 2 ^ "+string(SdmpLo)+" + * x y - 2 ^ "+string(SdmpLo)+" + "+string(Szrp)+" 2 ^ * / * 1 "
\                                                   			    +string(SdmpHi)+" 0 == 0 "+string(Szrp)+" "+string(SdmpHi)+" / 4 ^ ? + 1 "+string(SdmpHi)+" 0 == 0 x y - abs "+string(SdmpHi)+" / 4 ^ ? + / * + ?",U=1,V=1) \
					     : mt_lutxy(tmp, method, yexpr="clamp_f_i8 x y == x x x y - abs "+string(Szrp)+" / 1 "+string(Spwr)+" / ^ "+string(Szrp)+" * "+string(str)+" * x y - x y - abs / * x y - 2 ^ "
\                                                   			    +string(Szrp)+" 2 ^ "+string(SdmpLo)+" scalef + * x y - 2 ^ "+string(SdmpLo)+" scalef + "+string(Szrp)+" 2 ^ * / * 1 scalef "
\                                                   			    +string(SdmpHi)+" 0 == 0 "+string(Szrp)+" scalef "+string(SdmpHi)+" scalef / 4 ^ ? + 1 scalef "+string(SdmpHi)+" scalef 0 == 0 x y - abs "+string(SdmpHi)+" scalef / 4 ^ ? + / * + ?",U=1,V=1)


### LIMIT
normal = mt_clamp(normsharp, bright_limit, dark_limit, overshoot,  undershoot,  U=1, V=1)
second = mt_clamp(normsharp, bright_limit, dark_limit, overshoot2, undershoot2, U=1, V=1)
zero   = mt_clamp(normsharp, bright_limit, dark_limit, 0,          0,           U=1, V=1)

edge  = edgemaskHQ ? mt_logic( tmp.mt_edge(thY1=0,thY2=255,"8 16 8 0 0 0 -8 -16 -8 4",U=1,V=1)
\                             ,tmp.mt_edge(thY1=0,thY2=255,"8 0 -8 16 0 -16 8 0 -8 4",U=1,V=1)
\                             ,"max",U=1,V=1)
\     :              tmp.mt_edge(thY1=0,thY2=255,mode="min/max",U=1,V=1)

edge  = edgemaskHQ ? VersionNumber() < 2.6 ? edge.mt_lut("x 128 / 0.86 ^ 255 *",U=1,V=1) \
					   : edge.mt_lut("clamp_f_i8 x range_half / 0.86 ^ range_max *",U=1,V=1)
\     :              VersionNumber() < 2.6 ? edge.mt_lut("x 32 / 0.86 ^ range_max *",U=1,V=1) \
					   : edge.mt_lut("clamp_f_i8 x 32 scalef / 0.86 ^ range_max *",U=1,V=1)

limit1 = Lmode<0  ? normsharp.repair(tmp,abs(Lmode),-1)
\      : Lmode==0 ? normsharp
\      : Lmode==1 ? normal
\      : Lmode==2 ? mt_merge(normsharp, normal, edge.mt_inflate(U=1,V=1), U=1, V=1)
\      : Lmode==3 ? mt_merge(normal,    zero,   edge.mt_inflate(U=1,V=1), U=1, V=1) 
\      :            mt_merge(second,    normal, edge.mt_inflate(U=1,V=1), U=1, V=1)

limit2 = edgemode==0 ? limit1
\      : edgemode==1 ? mt_merge(tmp,limit1,edge.mt_inflate(U=1,V=1).mt_inflate(U=1,V=1).removegrain(11,-1),U=1,V=1)
\      :               mt_merge(limit1,tmp,edge.mt_inflate(U=1,V=1).mt_inflate(U=1,V=1).removegrain(11,-1),U=1,V=1)


### SOFT
sharpdiff = mt_makediff(tmp,limit2,U=1,V=1)
sharpdiff = VersionNumber() < 2.6 ? mt_lutxy(sharpdiff,sharpdiff.removegrain(19,-1),
\                    			     "x 128 - abs y 128 - abs > y "+string(soft)+" * x "+string(100-soft)+" * + 100 / x ?",U=1,V=1) \
				  : mt_lutxy(sharpdiff,sharpdiff.removegrain(19,-1),
\                    			     "clamp_f_i8 x range_half - abs y range_half - abs > y "+string(soft)+" * x "+string(100-soft)+" * + 100 / x ?",U=1,V=1)

PP1 = soft==0 ? limit2 : mt_makediff(tmp,sharpdiff,U=1,V=1)


### SOOTHE
diff  = mt_makediff(tmp,PP1,U=1,V=1)
diff2 = diff.temporalsoften(1,255,0,32,2)
diff3 = VersionNumber() < 2.6 ? mt_lutxy(diff,diff2,"x 128 - y 128 - * 0 < x 128 - 100 / "+string(keep)+
\                           			    " * 128 + x 128 - abs y 128 - abs > x "+string(keep)+" * y 100 "+string(keep)+" - * + 100 / x ? ?",U=1,V=1) \
			      : mt_lutxy(diff,diff2,"clamp_f_i8 x range_half - y range_half - * 0 < x range_half - 100 / "+string(keep)+
\                           			    " * range_half + x range_half - abs y range_half - abs > x "+string(keep)+" * y 100 "+string(keep)+" - * + 100 / x ? ?",U=1,V=1)

PP2 = soothe ? mt_makediff(tmp,diff3,U=1,V=1) : PP1


### OUTPUT
out = (dest_x != ox || dest_y != oy) ? tmp.mergeluma(PP2).spline36resize(dest_x,dest_y)
\   : (ss_x > 1.0 || ss_y > 1.0)     ? input.mergeluma(PP2.spline36resize(dest_x,dest_y))
\   :                                  input.mergeluma(PP2)

src = defined(source)==false         ? blankclip
\   : (dest_x != ox || dest_y != oy) ? source.spline36resize(dest_x,dest_y)
\   :                                  source
in  = (dest_x != ox || dest_y != oy) ? input.spline36resize(dest_x,dest_y)
\   :                                  input

shrpD  = mt_makediff(in,out,U=1,V=1)
shrpL  = VersionNumber() < 2.6 ? shrpD.repair(mt_makediff(in,src,U=1,V=1),1,-1,-1).mt_lutxy(shrpD,"x 128 - abs y 128 - abs < x y ?",U=1,V=1) \
			       : shrpD.repair(mt_makediff(in,src,U=1,V=1),1,-1,-1).mt_lutxy(shrpD,"x range_half - abs y range_half - abs < x y ?",U=1,V=1)
output = defined(source) ? mt_makediff(in,shrpL,U=2,V=2)
\      :                   out

output = edgemode!=-1    ? output
\      :                   edge.spline36resize(dest_x,dest_y).greyscale()


### SHOW
   function LSFM_Show(clip input, clip output, string "version", string "defaults",
   \                  float "strength", int "Smode", int "Smethod", int "kernel",
   \                  string "preblur", bool "secure", string "source",
   \                  int "Szrp", int "Spwr", float "SdmpLo", float "SdmpHi",
   \                  int "Lmode", int "overshoot", int "undershoot", int "overshoot2", int "undershoot2",
   \                  int "soft", bool "soothe", int "keep",
   \                  int "edgemode", bool "edgemaskHQ",
   \                  float "ss_x", float "ss_y", int "dest_x", int "dest_y",
   \                  bool "show", int "screenW", int "screenH")
   {
   resoW = screenW - 384
   resoH = screenH

   inW   = input.width()
   inH   = input.height()
   outW  = round( resoW / 4 ) * 4
   outH  = round( (outW * inH) / (4 * inW) ) * 4
   scale = resoH - ( 2 * outH )

   i = input.spline36resize(outW,outH)
   i = scale > 0 ? i.addborders(0,scale/4,0,scale/4) : i
   i = scale < 0 ? i.crop(0,-scale/4,0,scale/4) : i

   o = output.spline36resize(outW,outH)
   o = scale > 0 ? o.addborders(0,scale/4,0,scale/4) : o
   o = scale < 0 ? o.crop(0,-scale/4,0,scale/4) : o

   e = blankclip(input, width=384, height=resoH)
   e = e.SubTitle("LSFmod " + string(version)                ,text_color=$FFFFFF,font="COURIER NEW",size=24,x=40,y=40)
   e = e.SubTitle("DEFAULTS      = " + string(defaults)      ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=100)

   e = e.SubTitle("GENERAL:"                                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=150)
   e = e.SubTitle("--------"                                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=160)
   e = e.SubTitle("strength      = " + string(strength)      ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=180)
   e = e.SubTitle("Smode         = " + string(Smode)         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=190)
   e = e.SubTitle("Smethod       = " + string(Smethod)       ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=200)
   e = e.SubTitle("kernel        = " + string(kernel)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=210)

   e = e.SubTitle("SPECIAL:"                                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=250)
   e = e.SubTitle("--------"                                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=260)
   e = e.SubTitle("preblur       = " + string(preblur)       ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=280)
   e = e.SubTitle("secure        = " + string(secure)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=290)
   e = e.SubTitle("source        = " + string(source)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=300)

   e = e.SubTitle("NONLINEAR SHARPENING:"                    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=340)
   e = e.SubTitle("---------------------"                    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=350)
   e = e.SubTitle("Szrp          = " + string(Szrp)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=370)
   e = e.SubTitle("Spwr          = " + string(Spwr)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=380)
   e = e.SubTitle("SdmpLo        = " + string(SdmpLo)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=390)
   e = e.SubTitle("SdmpHi        = " + string(SdmpHi)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=400)

   e = e.SubTitle("LIMITING:"                                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=440)
   e = e.SubTitle("---------"                                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=450)
   e = e.SubTitle("Lmode         = " + string(Lmode)         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=470)
   e = e.SubTitle("overshoot     = " + string(overshoot)     ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=480)
   e = e.SubTitle("undershoot    = " + string(undershoot)    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=490)
   e = e.SubTitle("overshoot2    = " + string(overshoot2)    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=500)
   e = e.SubTitle("undershoot2   = " + string(undershoot2)   ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=510)

   e = e.SubTitle("POST-PROCESSING:"                         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=550)
   e = e.SubTitle("----------------"                         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=560)
   e = e.SubTitle("soft          = " + string(soft)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=580)
   e = e.SubTitle("soothe        = " + string(soothe)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=590)
   e = e.SubTitle("keep          = " + string(keep)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=600)

   e = e.SubTitle("EDGES:"                                   ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=640)
   e = e.SubTitle("------"                                   ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=650)
   e = e.SubTitle("edgemode      = " + string(edgemode)      ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=670)
   e = e.SubTitle("edgemaskHQ    = " + string(edgemaskHQ)    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=680)

   e = e.SubTitle("UPSAMPLING:"                              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=720)
   e = e.SubTitle("-----------"                              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=730)
   e = e.SubTitle("ss_x          = " + string(ss_x)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=750)
   e = e.SubTitle("ss_y          = " + string(ss_y)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=760)
   e = e.SubTitle("dest_x        = " + string(dest_x)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=770)
   e = e.SubTitle("dest_y        = " + string(dest_y)        ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=780)

   e = e.SubTitle("DEBUG:"                                   ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=820)
   e = e.SubTitle("------"                                   ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=830)
   e = e.SubTitle("show          = " + string(show)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=850)
   e = e.SubTitle("screenW       = " + string(screenW)       ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=860)
   e = e.SubTitle("screenH       = " + string(screenH)       ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=40,y=870)

   debug = stackhorizontal( e ,
   \                        stackvertical( i.subtitle("Input",text_color=$FFFFFF,size=20,x=40,y=20) ,
   \                                       o.subtitle("Output",text_color=$FFFFFF,size=20,x=40,y=20) )
   \                      )

   return debug
   }

output = defined(source) ? output.crop(0,4,-0,-4) : output

output = sislumaonly ? output : ssispmt ? CombinePlanes(output,inclip,planes="YUV",sample_clip=inclip) : ytouv(inclip.utoy8(),inclip.vtoy8(),output)

output = show==false ? output
\                    : LSFM_Show(inclip, output, version, defaults,
\                                strength, Smode, Smethod, kernel,
\                                preblur, secure, defined(source)?"defined":"undefined",
\                                Szrp, Spwr, SdmpLo, SdmpHi,
\                                Lmode, overshoot, undershoot, overshoot2, undershoot2,
\                                soft, soothe, keep,
\                                edgemode, edgemaskHQ,
\                                ss_x, ss_y, dest_x, dest_y,
\                                show, screenW, screenH)

return output
}