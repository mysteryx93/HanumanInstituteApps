########################################################################################
###                                                                                  ###
###                     GradFun2DB MOD : function GradFun2DBmod()                    ###
###                                                                                  ###
###                               v1.5 by "LaTo INV."                                ###
###                                                                                  ###
###                                30 December 2009                                  ###
###                                                                                  ###
########################################################################################
### 
### 
### /!\ Needed filters : Masktools (v2.0a36), Removegrain (v1.0PR),
### -------------------- GradFun2db (v1.0), AddGrainC(v1.4).
### 
###
### USAGE: GradFun2DBmod( thr, thrC, mode,
###                       str, strC, temp, adapt, custom,
###                       mask, radius, range,
###                       show, screenW, screenH ) 
###
###
### +--------+
### | DITHER |
### +--------+
###
### thr [default: 1.2]
### ------------------
### GradFun2DB "thr" parameter
###
### thrC [default: thr]
### -------------------
### GradFun2DB "thrC" parameter
###
### mode [default: 2]
### -----------------
### Mode for the addition of 16 pixels around the image
### 0 = Off                             [same as GradFun2DB]
### 1 = AddBorders  (speed:+ quality:-) [same as GradFunk]
### 2 = PointResize (speed:+ quality:+)
### 3 = Flip/Stack  (speed:- quality:+) [same as GradFunkMirror]
###
###
### +-------+
### | GRAIN |
### +-------+
###
### str [default: 0.8]
### ------------------
### AddGrainC "var" parameter
###
### strC [default: 0.0]
### -------------------
### AddGrainC "uvar" parameter
###
### temp [default: 50]
### ------------------
### Strength for temporal stabilization
### -1  = off
### 0   = nervous grain
### ..
### 100 = calm grain
### 
### adapt [default: 64]
### -------------------
### Threshold for luma-adaptative grain
### -1  = off
### 0   = source
### ..
### 255 = invert
###
### custom [default: "empty"]
### -------------------------
### Use your own grain generator instead of AddGrain
### (temp & adapt parameters are on, set -1 to turn off)
###
###
### +------+
### | MASK |
### +------+
###
### mask [default: true]
### --------------------
### Use adaptative deband mask 
### (dither/grain is only applied to areas with banding's susceptibility)
###
### radius [default: 2]
### -------------------
### Radius for the mask (1 is the fastest, 2-3 are slower)
### 
### range [default: 2]
### ------------------
### Range used in the mask (1-3 are good value)
###
###
### +-------+
### | DEBUG |
### +-------+
###
### show [default: false]
### ---------------------
### Show debug clip & informations
###
### screenW [default: 1280]
### -----------------------
### Screen horizontal resolution (for show clip)
###
### screenH [default: 1024]
### -----------------------
### Screen vertical resolution (for show clip)
###
###
### +-----------+
### | CHANGELOG |
### +-----------+ 
###
### v1.5 : added MOD4 restriction
###        changed script to accept MOD8 clip
###        updated documentation
###
### v1.4 : changed "temp" parameter (-1=off)
###        updated documentation
###
### v1.3 : replaced chroma parameter by thrC & strC
###
### v1.2 : changed int->float for str parameter
###        added custom parameter
###
### v1.1 : added show parameter
###        added small speed improvements
###        added warning for bad settings
###        fixed small bug with chroma=false
###
### v1.0 : first public release
###
########################################################################################

Function GradFun2DBmod( clip input,
\                       float "thr", float "thrC", int "mode",
\                       float "str", float "strC", int "temp", int "adapt", string "custom",
\                       bool "mask", int "radius", int "range",
\                       bool "show", int "screenW", int "screenH" ) 
{
version = "v1.5"


### CHECKS
w    = input.width()
h    = input.height()
mod4 = ( ( round(w/4.0)*4==w ) && ( round(h/4.0)*4==h ) ) ? true : false

Assert(isYV12(input), chr(10) + "This is not an YV12 clip! Please use ConvertToYV12() before using GradFun2DBmod()" + chr(10))
Assert(mod4,          chr(10) + "This is not a MOD4 clip! Please use Crop() or AddBorders() before using GradFun2DBmod()" + chr(10))


### DEFAULTS
thr     = default( thr,             1.2 )
thrC    = default( thrC,            thr )
mode    = default( mode,              2 )

str     = default( str,             0.8 )
strC    = default( strC,            0.0 )
temp    = default( temp,             50 )
adapt   = default( adapt,            64 )
custom  = default( custom,      "empty" )

mask    = default( mask,           true )
radius  = default( radius,            2 )
range   = default( range,             2 )

show    = default( show,          false )
screenW = default( screenW,        1280 )
screenH = default( screenH,        1024 )

Assert( ( thr >= 1.0 )                   ? true : false, chr(10) + "'thr' have not a correct value! [>=1.0]" + chr(10))
Assert( ( thrC >= 1.0 )                  ? true : false, chr(10) + "'thrC' have not a correct value! [>=1.0]" + chr(10))
Assert( ( mode >= 0 && mode <= 3 )       ? true : false, chr(10) + "'mode' have not a correct value! [0,1,2,3]" + chr(10))
Assert( ( str >= 0.0 )                   ? true : false, chr(10) + "'str' have not a correct value! [>=0.0]" + chr(10))
Assert( ( strC >= 0.0 )                  ? true : false, chr(10) + "'strC' have not a correct value! [>=0.0]" + chr(10))
Assert( ( temp >= -1 && temp <= 100 )    ? true : false, chr(10) + "'temp' have not a correct value! [-1,0...100]" + chr(10))
Assert( ( adapt >= -1 && adapt <= 255 )  ? true : false, chr(10) + "'adapt' have not a correct value! [-1,0...255]" + chr(10))
Assert( ( radius >= 1 )                  ? true : false, chr(10) + "'radius' have not a correct value! [>=1]" + chr(10))
Assert( ( range >= 0 && range <= 255 )   ? true : false, chr(10) + "'range' have not a correct value! [0...255]" + chr(10))

chr    = strC > 0.0 ? true : false
chroma = thrC > 1.0 || strC > 0.0 ? true : false


### DITHER
   Function GF_ModeOff(clip clp, float thr, float thrC)
   {
   w = CLP.width()
   p = (ceil(w/8.0)*8-w)/2

   RDY = CLP.addborders(p,0,p,0)

   LUM = RDY.gradfun2db(thr).Crop(p,0,-p,-0)
   CHR = RDY.gradfun2db(thrC).Crop(p,0,-p,-0)

   GFO = thr==1.0 && thrC==1.0 ? CLP
   \   : thr==thrC             ? LUM
   \   : thr!=1.0 && thrC==1.0 ? LUM.mergechroma(CLP)
   \   : thr==1.0 && thrC!=1.0 ? CLP.mergechroma(CHR)
   \   :                         LUM.mergechroma(CHR)

   Return GFO
   }

   Function GF_Borders(clip clp, float thr, float thrC)
   {
   w = CLP.width()
   p = 16 + (ceil(w/8.0)*8-w)/2

   RDY = CLP.addborders(p,16,p,16)

   LUM = RDY.gradfun2db(thr).Crop(p,16,-p,-16)
   CHR = RDY.gradfun2db(thrC).Crop(p,16,-p,-16)

   GFK = thr==1.0 && thrC==1.0 ? CLP
   \   : thr==thrC             ? LUM
   \   : thr!=1.0 && thrC==1.0 ? LUM.mergechroma(CLP)
   \   : thr==1.0 && thrC!=1.0 ? CLP.mergechroma(CHR)
   \   :                         LUM.mergechroma(CHR)

   Return GFK
   }
   
   Function GF_Padding(clip clp, float thr, float thrC)
   {
   w = CLP.width()
   h = CLP.height()
   p = 32 + (ceil(w/8.0)*8-w)
   
   RDY = CLP.pointresize(w+p,h+32,-p/2,-16,w+p,h+32)

   LUM = RDY.gradfun2db(thr).Crop(p/2,16,-p/2,-16)
   CHR = RDY.gradfun2db(thrC).Crop(p/2,16,-p/2,-16)

   GFP = thr==1.0 && thrC==1.0 ? CLP
   \   : thr==thrC             ? LUM
   \   : thr!=1.0 && thrC==1.0 ? LUM.mergechroma(CLP)
   \   : thr==1.0 && thrC!=1.0 ? CLP.mergechroma(CHR)
   \   :                         LUM.mergechroma(CHR)

   Return GFP
   }

   Function GF_Mirrors(clip clp, float thr, float thrC)
   {
   w = CLP.width()
   h = CLP.height()
   p = 16 + (ceil(w/8.0)*8-w)/2
   
   TM = CLP.crop(0,0,0,-h+16).FlipVertical()
   TL = TM.crop(0,0,-w+p,0).FlipHorizontal()
   TR = TM.crop(w-p,0,0,0).FlipHorizontal()
   
   MM = CLP
   ML = MM.crop(0,0,-w+p,0).FlipHorizontal()
   MR = MM.crop(w-p,0,0,0).FlipHorizontal()
   
   BM = CLP.crop(0,h-16,0,0).FlipVertical()
   BL = BM.crop(0,0,-w+p,0).FlipHorizontal()
   BR = BM.crop(w-p,0,0,0).FlipHorizontal()
   
   TOP = StackHorizontal(TL,TM,TR)
   MID = StackHorizontal(ML,MM,MR)
   BOT = StackHorizontal(BL,BM,BR)
   
   RDY = StackVertical(TOP,MID,BOT)

   LUM = RDY.gradfun2db(thr).Crop(p,16,-p,-16)
   CHR = RDY.gradfun2db(thrC).Crop(p,16,-p,-16)

   GFM = thr==1.0 && thrC==1.0 ? CLP
   \   : thr==thrC             ? LUM
   \   : thr!=1.0 && thrC==1.0 ? LUM.mergechroma(CLP)
   \   : thr==1.0 && thrC!=1.0 ? CLP.mergechroma(CHR)
   \   :                         LUM.mergechroma(CHR)

   Return GFM
   }

dither  = mode==1  ? input.GF_Borders(thr,thrC)
\       : mode==2  ? input.GF_Padding(thr,thrC)
\       : mode==3  ? input.GF_Mirrors(thr,thrC)
\       :            input.GF_ModeOff(thr,thrC)


### GRAIN
grain  = custom=="empty" ? dither.addgrainC(str,strC,0,0)
\      :                   Eval("dither." + custom)
diff   = custom=="empty" ? blankclip(dither,color_yuv=$808080).addgrainC(str,strC,0,0)
\      :                   Eval("blankclip(dither,color_yuv=$808080)." + custom)

grain  = temp<=0   ? grain
\      : temp==100 ? mt_makediff(dither,diff.temporalsoften(1,255,chr?255:0,255,2),u=chr?3:2,v=chr?3:2)
\      :             mt_makediff(dither,diff.mergeluma(diff.temporalsoften(1,255,chr?255:0,255,2),temp/100.0),u=chr?3:2,v=chr?3:2)

AGmask = adapt==0   ? input.removegrain(19,-1)
\      : adapt==255 ? input.invert().removegrain(19,-1)
\      :              input.mt_lut("x "+string(adapt)+" - abs 255 * "+string(adapt)+" 128 - abs 128 + /",u=1,v=1).removegrain(19,-1)

deband = str==0.0 && strC==0.0 ? dither
\      : adapt==-1             ? grain
\      :                         mt_merge(grain,dither,AGmask,luma=chr?true:false,u=chr?3:4,v=chr?3:4)


### MASK
GFmask = radius==1 ? input.mt_edge(mode="min/max",thY1=0,thY2=255,u=1,v=1).mt_lut(expr="255 x 1 "+string(range)+" / * 2 ^ /",u=1,v=1).removegrain(19,-1)
\      :             mt_luts(input,input,mode="range",pixels=mt_square(radius),expr="y",u=1,v=1).mt_lut(expr="255 x 1 "+string(range)+" / * 2 ^ /",u=1,v=1).removegrain(19,-1)

output = mask==true   ? mt_merge(input,deband,GFmask,luma=chroma?true:false,u=chroma?3:2,v=chroma?3:2)
\      : chroma==true ? deband
\      :                deband.mergechroma(input)


### SHOW
   function GFMOD_Show(clip input, clip output, string "version",
   \                   float "thr", float "thrC", int "mode",
   \                   float "str", float "strC", int "temp", int "adapt", string "custom",
   \                   bool "mask", int "radius", int "range",
   \                   bool "show", int "screenW", int "screenH")
   {
   custom   = StrLen(custom) <= 15 ? custom
   \        :                        LeftStr(custom,12) + "..."
   strength = LeftStr(string(str),4) + " | " + LeftStr(string(strC),4)

   screen_x = screenW
   screen_y = screenH - 120
   src_x    = input.width()
   src_y    = input.height()
   dest_x   = round( screen_x / 8 ) * 4
   dest_y   = round( (dest_x * src_y) / (4 * src_x) ) * 4
   scale_x  = screen_x - ( 2 * dest_x )
   scale_y  = screen_y - ( 2 * dest_y )

   a      = input.spline36resize(dest_x,dest_y)
   b      = output.spline36resize(dest_x,dest_y)
   mkdiff = mt_makediff(a, b, chroma="process")
   c      = mkdiff.ColorYUV(analyze=true)
   d      = mkdiff.Levels(112, 1.0, 144, 0, 255, coring=false)

   a = scale_x > 0 ? a.addborders(scale_x/4,0,scale_x/4,0)
   \ : scale_x < 0 ? a.crop(-scale_x/4,0,scale_x/4,0)
   \ :               a
   a = scale_y > 0 ? a.addborders(0,scale_y/4,0,scale_y/4)
   \ : scale_y < 0 ? a.crop(0,-scale_y/4,0,scale_y/4)
   \ :               a

   b = scale_x > 0 ? b.addborders(scale_x/4,0,scale_x/4,0)
   \ : scale_x < 0 ? b.crop(-scale_x/4,0,scale_x/4,0)
   \ :               b
   b = scale_y > 0 ? b.addborders(0,scale_y/4,0,scale_y/4)
   \ : scale_y < 0 ? b.crop(0,-scale_y/4,0,scale_y/4)
   \ :               b

   c = scale_x > 0 ? c.addborders(scale_x/4,0,scale_x/4,0)
   \ : scale_x < 0 ? c.crop(-scale_x/4,0,scale_x/4,0)
   \ :               c
   c = scale_y > 0 ? c.addborders(0,scale_y/4,0,scale_y/4)
   \ : scale_y < 0 ? c.crop(0,-scale_y/4,0,scale_y/4)
   \ :               c

   d = scale_x > 0 ? d.addborders(scale_x/4,0,scale_x/4,0)
   \ : scale_x < 0 ? d.crop(-scale_x/4,0,scale_x/4,0)
   \ :               d
   d = scale_y > 0 ? d.addborders(0,scale_y/4,0,scale_y/4)
   \ : scale_y < 0 ? d.crop(0,-scale_y/4,0,scale_y/4)
   \ :               d

   e = blankclip(input, width=screen_x, height=120)
   e = e.SubTitle("GradFun2DBmod " + string(version)           ,text_color=$FFFFFF,font="COURIER NEW",size=24,x=20,y=40)

   e = e.SubTitle("DITHER:"                                    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=320,y=20)
   e = e.SubTitle("-------"                                    ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=320,y=30)
   e = e.SubTitle("thr          = " + string(thr)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=320,y=50)
   e = e.SubTitle("thrC         = " + string(thrC)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=320,y=70)
   e = e.SubTitle("mode         = " + string(mode)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=320,y=90)

   e = e.SubTitle("GRAIN:"                                     ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=20)
   e = e.SubTitle("------"                                     ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=30)
   e = custom=="empty"
   \ ? e.SubTitle("str | strC   = " + string(strength)         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=50)
   \ : e.SubTitle("custom       = " + string(custom)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=50)
   e = e.SubTitle("temp         = " + string(temp)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=70)
   e = e.SubTitle("adapt        = " + string(adapt)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=520,y=90)

   e = e.SubTitle("MASK:"                                      ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=720,y=20)
   e = e.SubTitle("-----"                                      ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=720,y=30)
   e = e.SubTitle("mask         = " + string(mask)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=720,y=50)
   e = e.SubTitle("radius       = " + string(radius)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=720,y=70)
   e = e.SubTitle("range        = " + string(range)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=720,y=90)

   e = e.SubTitle("DEBUG:"                                     ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=920,y=20)
   e = e.SubTitle("------"                                     ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=920,y=30)
   e = e.SubTitle("show         = " + string(show)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=920,y=50)
   e = e.SubTitle("screenW      = " + string(screenW)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=920,y=70)
   e = e.SubTitle("screenH      = " + string(screenH)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=920,y=90)

   debug = stackvertical( e
   \                      ,
   \                      stackhorizontal( a.subtitle("Input",text_color=$FFFFFF,size=20,x=20,y=20)  ,
   \                                       b.subtitle("Output",text_color=$FFFFFF,size=20,x=20,y=20) )
   \                      ,
   \                      stackhorizontal( c.subtitle("Differences",text_color=$FFFFFF,size=20,x=20,y=20) ,
   \                                       d.subtitle("Amplified",text_color=$FFFFFF,size=20,x=20,y=20)   )
   \                      )

   return debug
   }

output = show==false ? output
   \                 : GFMOD_Show(input, output, version=version,
   \                              thr=thr, thrC=thrC, mode=mode,
   \                              str=str, strC=strC, temp=temp, adapt=adapt, custom=custom,
   \                              mask=mask, radius=radius, range=range,
   \                              show=show, screenW=screenW, screenH=screenH)

return output
}