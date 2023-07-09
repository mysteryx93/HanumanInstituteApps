# FineDehalo
# 1.1 mod7.5
#
# This program is free software. It comes without any warranty, to
# the extent permitted by applicable law. You can redistribute it
# and/or modify it under the terms of the Do What The Fuck You Want
# To Public License, Version 2, as published by Sam Hocevar.

Function FineDehalo (clip src, float "rx", float "ry", float "thmi", float "thma", float "thlimi", float "thlima", float "darkstr", float "brightstr", int "showmask", float "contra", bool "excl", float "edgeproc", val "exdehalo", clip "exedgesm")
{
	rx        = Default (rx,          2)
	ry        = Default (ry,         rx)
	thmi      = Default (thmi,       80)
	thma      = Default (thma,      128)
	thlimi    = Default (thlimi,     50)
	thlima    = Default (thlima,    100)
	darkstr   = Default (darkstr,   1.0)
	brightstr = Default (brightstr, 1.0)
	showmask  = Default (showmask,    0)
	contra    = Default (contra,    0.0)
	excl      = Default (excl,     true)
	edgeproc  = Default (edgeproc,  0.0)

	rx_i = Round (rx)
	ry_i = Round (ry)

	src


	### Dehaloing ###

	dehaloed = defined(exdehalo) ? isclip(exdehalo) ? exdehalo : eval("last." + exdehalo) : DeHalo_alpha (rx=rx, ry=ry, darkstr=darkstr, brightstr=brightstr)

	# Contrasharpening
	dehaloed =   (contra > 0)
\	           ? dehaloed.FineDehalo_contrasharp (src, contra)
\	           : dehaloed

	edgesm = defined(exedgesm) ? exedgesm : FineDehaloedges(rx=rx, ry=ry, thmi=thmi, thma=thma, thlimi=thlimi, thlima=thlima, showmask=showmask, excl=excl, edgeproc=edgeproc)

	### Masking ###

		  (showmask != 0) ? edgesm
	\	:                   mt_merge (last, dehaloed, edgesm, y=3, u=2, v=2)
}

# level == 1.0 : normal contrasharp
Function FineDehalo_contrasharp (clip dehaloed, clip src, float level)
{
	bb  = dehaloed.RemoveGrain (11, -1)
	bb2 = bb.Repair (bb.Repair (bb.Medianblur (2, -256, -256), 1), 1)
	xd  = mt_makediff (bb, bb2)
	xd  = VersionNumber() < 2.6 ? xd.mt_lut ("x 128 - 2.49 * "+String(level)+" * 128 +")
              \                     : xd.mt_lut ("x range_half - 2.49 * "+String(level)+" * range_half +")
	xdd = mt_lutxy (
\		xd,
\		mt_makediff (src, dehaloed),
\		VersionNumber() < 2.6 ? "x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?"
\		                      : "x range_half - y range_half - * 0 < range_half x range_half - abs y range_half - abs < x y ? ?"
\	)

	dehaloed.mt_adddiff (xdd, y=3, u=2, v=2)
}

# moved FineDehalo edges mask in stand-alone Function
Function FineDehaloedges (clip src, float "rx", float "ry", float "thmi", float "thma", float "thlimi", float "thlima", int "showmask", bool "excl", float "edgeproc", clip "edgemask")
{
	rx        = Default (rx,          2)
	ry        = Default (ry,         rx)
	thmi      = Default (thmi,       80)
	thma      = Default (thma,      128)
	thlimi    = Default (thlimi,     50)
	thlima    = Default (thlima,    100)
	showmask  = Default (showmask,    0)
	excl      = Default (excl,     true)
	edgeproc  = Default (edgeproc,  0.0)

	rx_i = Round (rx)
	ry_i = Round (ry)

	src

	### Main edges ###

	# Basic edge detection, thresholding will be applied later.
	edges = defined(edgemask) ? edgemask : mt_edge (mode="prewitt", thY1=0, thY2=255)

	# Keeps only the sharpest edges (line edges)
	strong = VersionNumber() < 2.6 ? edges.mt_lut (expr="x "+String(thmi)+" - "+String(thma-thmi)+" / 255 *")
                 \                     : edges.mt_lut (expr="clamp_f_i8 x "+String(thmi)+" scalef - "+String(thma-thmi)+" scalef / range_max *")

	# Extends them to include the potential halos
	large = strong.mt_expand_multi (sw=rx_i, sh=ry_i)


	### Exclusion zones ###

	# When two edges are close from each other (both edges of a single
	# line or multiple parallel color bands), the halo removal
	# oversmoothes them or makes seriously bleed the bands, producing
	# annoying artifacts. Therefore we have to produce a mask to exclude
	# these zones from the halo removal.

	# Includes more edges than previously, but ignores simple details
	light = VersionNumber() < 2.6 ? edges.mt_lut (expr="x "+String(thlimi)+" - "+String(thlima-thlimi)+" / 255 *")
                \                     : edges.mt_lut (expr="clamp_f_i8  x "+String(thlimi)+" scalef - "+String(thlima-thlimi)+" scalef / range_max *")

	# To build the exclusion zone, we make grow the edge mask, then shrink
	# it to its original shape. During the growing stage, close adjacent
	# edge masks will join and merge, forming a solid area, which will
	# remain solid even after the shrinking stage.

	# Mask growing
	shrink = light.mt_expand_multi (sw=rx_i, sh=ry_i, mode="ellipse")

	# At this point, because the mask was made of a shades of grey, we may
	# end up with large areas of dark grey after shrinking. To avoid this,
	# we amplify and saturate the mask here (actually we could even
	# binarize it).
	shrink = VersionNumber() < 2.6 ? shrink.mt_lut ("x 4 *")
                 \                     : shrink.mt_lut ("clamp_f x 4 *")

	# Mask shrinking
	shrink = shrink.mt_inpand_multi (sw=rx_i, sh=ry_i, mode="ellipse")

	# This mask is almost binary, which will produce distinct
	# discontinuities once applied. Then we have to smooth it.
	shrink = shrink.RemoveGrain (20, -1)
	shrink = shrink.RemoveGrain (20, -1)


	### Final mask building ###

	# Previous mask may be a bit weak on the pure edge side, so we ensure
	# that the main edges are really excluded. We do not want them to be
	# smoothed by the halo removal.
	shr_med = (excl) ? mt_logic (strong, shrink, mode="max") : strong

	# Substracts masks and amplifies the difference to be sure we get 255
	# on the areas to be processed.
	outside = VersionNumber() < 2.6 ? mt_lutxy (large, shr_med, "x y - 2 *")
                  \                     : mt_lutxy (large, shr_med, "clamp_f x y - 2 *")

	# If edge processing is required, adds the edgemask
	ep_str  = VersionNumber() < 2.6 ? "x y "+String(edgeproc * 0.66)+" * +" : "clamp_f x y "+String(edgeproc * 0.66)+" * +"
	outside = (edgeproc > 0) ? mt_lutxy (outside, strong, ep_str) : outside

	# Smooth again and amplify to grow the mask a bit, otherwise the halo
	# parts sticking to the edges could be missed.
	VersionNumber() < 2.6 ? outside.RemoveGrain (20, -1).mt_lut ("x 2 *")
        \                     : outside.RemoveGrain (20, -1).mt_lut ("clamp_f x 2 *")

	  (showmask == 1) ? outside.GreyScale ()
\	: (showmask == 2) ? shrink.GreyScale ()
\	: (showmask == 3) ? edges.GreyScale ()
\	: (showmask == 4) ? strong.GreyScale ()
\	:                   last
}



# Try to remove 2nd order halos.
Function FineDehalo2 (clip src, string "hconv", string "vconv", int "showmask")
{
	hconv    = Default (hconv, "-1 -2 0 0 40 0 0 -2 -1")
	vconv    = Default (vconv, "-2 -1 0 0 40 0 0 -1 -2")
	showmask = Default (showmask, 0)

	src
	fix_h = mt_convolution (horizontal="1", vertical=vconv, y=3, u=2, v=2)
	fix_v = mt_convolution (horizontal=hconv, vertical="1", y=3, u=2, v=2)
	edges_h = mt_edge (mode="1 2 1 0 0 0 -1 -2 -1", thY1=0, thY2=255)
	edges_v = mt_edge (mode="1 0 -1 2 0 -2 1 0 -1", thY1=0, thY2=255)
	mask_h = edges_h	#.mt_lut (expr="x 2 *")
	mask_v = edges_v	#.mt_lut (expr="x 2 *")
	temp_h = VersionNumber() < 2.6 ? mt_lutxy (mask_h, mask_v, expr="x 3 * y -")
                 \	               : mt_lutxy (mask_h, mask_v, expr="clamp_f x 3 * y -")
	temp_v = VersionNumber() < 2.6 ? mt_lutxy (mask_v, mask_h, expr="x 3 * y -")
                 \	               : mt_lutxy (mask_v, mask_h, expr="clamp_f x 3 * y -")
	mask_h = temp_h
	mask_v = temp_v

	mask_h = mask_h.FineDehalo2_grow_mask ("vertical")
	mask_v = mask_v.FineDehalo2_grow_mask ("horizontal")

	src
	mt_merge (last, fix_h, mask_h, y=3, u=2, v=2)
	mt_merge (last, fix_v, mask_v, y=3, u=2, v=2)

	  (showmask == 1) ? mt_logic (mask_h, mask_v, mode="max").GreyScale ()
\	:                   last
}

Function FineDehalo2_grow_mask (clip mask, string mode)
{
	Assert ((mode == "horizontal" || mode == "vertical"), "Wrong mode")

	mask
	mt_expand (mode=mode).mt_inpand (mode=mode)
	mask_1 = mt_expand (mode=mode)
	mask_2 = mask_1.mt_expand (mode=mode).mt_expand (mode=mode)
	VersionNumber() < 2.6 ? mt_lutxy (mask_2, mask_1, expr="x y -")
        \		      : mt_lutxy (mask_2, mask_1, expr="clamp_f x y -")
	VersionNumber() < 2.6 ? RemoveGrain (12, -1).mt_lut (expr="x 1.8 *")
        \		      : RemoveGrain (12, -1).mt_lut (expr="clamp_f x 1.8 *")
}


# new Functions added to the mod ver.


Function FineDehaloanalog (clip src, int "dhhradius", float "arx", float "ary", string "exdehalo", clip "fexedgesm", clip "linesm", bool "remasked", float "darkstr", float "brightstr")
{
rx       = Default (arx,          2) # DeHalo_alpha rx
ry       = Default (ary,         rx) # DeHalo_alpha ry
edgm     = Defined(linesm) ? linesm : src.slinesm()
rad      = Default(dhhradius,      2)
remasked = Default(remasked, true)
blurmix  = remasked ? 0.5 : 0.8
	rx_i = Round (rx)
	ry_i = Round (ry)
exdehalo = Default(exdehalo, "VHSHaloremover("+String(rx_i)+","+String(ry_i)+",200,100,"+String(blurmix)+")")
useldh   = !defined(darkstr) && !defined(brightstr) && !defined(arx) && !defined(ary)

src
lightdh = DeHalo_alpha_mt(rx=rx_i,ry=ry_i,darkstr=0.2,brightstr=0.8)
FDedges = Defined(fexedgesm) ? fexedgesm : lightdh.FineDehaloedges(rx=rx_i,ry=ry_i)
alphadh = remasked ? lightdh : \ 
                     FineDehalo(rx=rx,ry=ry,exdehalo=useldh ? lightdh : \
                                                              undefined(),darkstr=darkstr,brightstr=brightstr,exedgesm=FDedges)

hfil = alphadh.FineDehalo(rx=rx,ry=ry,exdehalo=DeHaloHmod(rad,exdehalo=exdehalo,extmask=edgm), exedgesm=FDedges)
RM2  = remasked ? DR_Radius_dhh(edgm.mt_inflate(),rad,0).mt_inflate() : nop()

DeRinging = mt_Merge(hfil, src, edgm.mt_inflate(155,155),u=remasked ? 1 : 4,v=remasked ? 1 : 4)
remasked ? mt_Merge(src, DeRinging, RM2, u=2, v=2) : DeRinging
}

# filtering with black borders by A.SONY
Function filtering_wbb (clip src, string "filter", int "oneborder", clip "linesm", clip "clip4lines", int "mmy", bool "mmLuma", int "mmuv", bool "minflate")
{
oneborder = Default (oneborder,   2)
mmLuma    = Default (mmLuma,  false)
minflate  = Default (minflate, true)
mmuv      = Default (mmuv, mmLuma ? 3 : 2)
mmy       = Default (mmy,         3)
filter    = Default (filter, defined(linesm) ? "yahr3.TBilateral(5,5,0.9,0.9,5,5,0.7,chroma=false)" : \
                                             """FineDehaloanalog(exdehalo="VHSHaloremover(2,2,200,100,0.5).yahr2(32)")""")
minf = defined(linesm) ? minflate ? linesm.mt_inflate(155,155,u=mmLuma || mmuv!=3 ? 1 : 3 ,v=mmLuma || mmuv!=3 ? 1 : 3) : linesm : nop()

src
AddBorders(oneborder,oneborder,oneborder,oneborder)
eval(filter)
Crop(oneborder, oneborder, -oneborder, -oneborder, align=true)

defined(linesm) ? mt_Merge(last, defined(clip4lines) ? clip4lines : src, minf,y=mmy,luma=mmLuma,u=mmuv,v=mmuv) : last
}

# black lines mask by A.SONY
Function slinesm (clip i, int "trh", float "trh2", bool "analog", bool "autogain", val "edgesm", int "useMedianBlur", bool "noedges")
{
analog        = Default(analog,   true)
autogain      = Default(autogain, true)
noedges       = Default(noedges,  false)
trh           = Default(trh,  255)
trh2          = Default(trh2, 170)
useMedianBlur = default(useMedianBlur, -1)

i          = autogain ? i.ColorYUV(autogain=true) : i

edgesm     = !noedges ? defined(edgesm) ? isclip(edgesm) ? edgesm : eval("i." + edgesm) : i.Camembert_dhh(useMedianBlur) : nop()

LineDarkenclip = trh==0 ? i : i.FastLineDarkenMOD3_dhh(trh).blur(0.5).FastLineDarkenMOD3_dhh(250,1,250,-2)

lut4darkex = analog ? VersionNumber() < 2.6 ? "x "+String(trh2/2.46)+" < 255 x "+String(trh2)+" > 0 255 x "+String(trh2/2.46)+" - 255 "+String(trh2)+" "+String(trh2/2.46)+" - / * - ? ?"
                      \                     : "x "+String(trh2/2.46)+" scalef < range_max x "+String(trh2)+" scalef > 0 range_max x "+String(trh2/2.46)+" scalef - range_max "+String(trh2)+" scalef "+String(trh2/2.46)+" scalef - / * - ? ?" : ""
lut4dark   = analog ? LineDarkenclip.mt_lut(lut4darkex,u=1,v=1) : 
                    \ LineDarkenclip.mt_binarize(70, mode="0 255")

noedges ? lut4dark : 
        \ mt_merge(i.mt_edge(mode="min/max", thY1=255, thY2=255), edgesm, lut4dark)
}

# tophf lines mask
function t_linemask( clip c, int "blur", float "thresh", float "str" )
{
	blur=default(blur,5)
	thresh=default(thresh,4)
	str=default(str,16)
	VersionNumber() < 2.6 ? mt_lutxy(c,c.binomialblur(blur,u=1,v=1),"x "+string(thresh)+" + y < y x - "+string(str)+" * 0 ?")
        \                     : mt_lutxy(c,c.binomialblur(blur,u=1,v=1),"clamp_f_i8 x "+string(thresh)+" scalef + y < y x - "+string(str)+" * 0 ?")
}