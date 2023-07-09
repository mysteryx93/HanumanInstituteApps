##############################################################################
#
#	High bitdepth tools for Avisynth
#       v1.27.1
#	Author: Firesledge
#
#	See the documentation for the function description.
#
#	This program is free software. It comes without any warranty, to
#	the extent permitted by applicable law. You can redistribute it
#	and/or modify it under the terms of the Do What The Fuck You Want
#	To Public License, Version 2, as published by Sam Hocevar. See
#	http://sam.zoy.org/wtfpl/COPYING for more details.
#
##############################################################################



Function Dither1Pre (clip m, string "flt", bool "stacked", float "overlap", bool "tvopt")
{
	m.DitherPre_internal (1, flt, stacked, overlap, tvopt)
}

Function Dither2Pre (clip m, string "flt", bool "stacked", float "overlap", bool "tvopt")
{
	m.DitherPre_internal (2, flt, stacked, overlap, tvopt)
}



Function DitherBuildMask (clip cnew, clip "cold", int "edgelvl", int "radius")
{
	cold_flag = Defined (cold)

	edgelvl = Default (edgelvl, 8)
	radius  = Default (radius,  2)
	cold    = Default (cold, cnew)

	cedgo = cold.mt_edge (
\		"prewitt",
\		thY1=edgelvl, thY2=edgelvl,
\		thC1=edgelvl, thC2=edgelvl,
\		y=3, u=3, v=3
\	)
	cedgn = cnew.mt_edge (
\		"prewitt",
\		thY1=edgelvl, thY2=edgelvl,
\		thC1=edgelvl, thC2=edgelvl,
\		y=3, u=3, v=3
\	)

	cdif = mt_lutxy (cold, cnew, expr="x y == 0 255 ?", y=3, u=3, v=3)

	m = cedgn
	m = (cold_flag) ? mt_logic (m, cedgo, mode="or", y=3, u=3, v=3) : m
	m = (cold_flag) ? mt_logic (m, cdif,  mode="or", y=3, u=3, v=3) : m

	m = (radius > 1) ? m.mt_expand_multi (sw=radius-1, sh=radius-1, y=3, u=3, v=3) : m

	return (m)
}



Function GradFun3 (clip src, float "thr", int "radius", float "elast",
\	int "mask", int "mode", float "ampo", float "ampn", int "pat",
\	bool "dyn", float "dthr", int "smode", float "wmin", float "thr_det",
\	float "thr_edg", int "debug", int "subspl", bool "lsb", bool "lsb_in",
\	bool "staticnoise", float "thrc", int "radiusc", float "elastc",
\	int "y", int "u", int "v", clip "ref", bool "slice")
{
	thr     = Default (thr,       0.35)
	thrc    = Default (thrc,       thr)
	radius  = Default (radius,      12)
	radiusc = Default (radiusc, radius)
	elast   = Default (elast,      3.0)
	elastc  = Default (elastc,   elast)
	mask    = Default (mask,         2)
	smode   = Default (smode,        0)
	wmin    = Default (wmin,       1.0)
	thr_det = Default (thr_det, 2 + Round (Dither_max (thr - 0.35, 0) / 0.3))
	debug   = Default (debug,        0)
	subspl  = Default (subspl,       0)
	lsb     = Default (lsb,      false)
	lsb_in  = Default (lsb_in,   false)
	ref     = Default (ref,        src)

	# Input range check. The other parameters are checked by the plugins.
	Assert (radius  > 0, "GradFun3: "+chr(34)+"radius" +chr(34)+" must be strictly positive.")
	Assert (radiusc > 0, "GradFun3: "+chr(34)+"radiusc"+chr(34)+" must be strictly positive.")
	Assert (thr     > 0, "GradFun3: "+chr(34)+"thr"    +chr(34)+" must be strictly positive.")
	Assert (thrc    > 0, "GradFun3: "+chr(34)+"thrc"   +chr(34)+" must be strictly positive.")
	Assert (thr_det > 0, "GradFun3: "+chr(34)+"thr_det"+chr(34)+" must be strictly positive.")
	Assert (elast  >= 1, "GradFun3: "+chr(34)+"elast"  +chr(34)+" must be greater or equal to 1.")
	Assert (elastc >= 1, "GradFun3: "+chr(34)+"elastc" +chr(34)+" must be greater or equal to 1.")

	src_8  = (lsb_in) ? src.DitherPost (mode=-1) : src
	src_16 = (lsb_in) ? src : src.Dither_convert_8_to_16 ()
	ref_16 = (lsb_in) ? ref : ref.Dither_convert_8_to_16 ()
	yv411_flag = src.Dither_isyv411 ()

	# Main debanding

	yp     = (! Defined (y) || (y == 3)) ? 3 : 1
	up     = (! Defined (u) || (u == 3)) ? 3 : 1
	vp     = (! Defined (v) || (v == 3)) ? 3 : 1

	chroma_flag = (  (thrc != thr || radiusc != radius || elastc != elast)
\	               && yp == 3 && (up == 3 || vp == 3))
	up2    = (chroma_flag) ? 1 : up
	vp2    = (chroma_flag) ? 1 : vp

	src_16
	flt_y  =                 Dither_gf3_smooth (last, src_8, ref_16, smode, radius,  thr,  elast,  lsb_in, wmin, subspl, yp, up2, vp2)
	flt_c  = (chroma_flag) ? Dither_gf3_smooth (last, src_8, ref_16, smode, radiusc, thrc, elastc, lsb_in, wmin, subspl, 1,  up,  vp) : flt_y
	flt    = (chroma_flag) ? flt_y.MergeChroma (flt_c) : flt_y

	# Edge/detail mask

	td_lo  = Dither_max (thr_det * 0.75, 1)
	td_hi  = Dither_max (thr_det,        1)
	mexpr  = Dither_make_expr_gate (td_lo, td_hi)

	dmask  = (mask > 0 && yv411_flag) ? src_8.ConvertToY8 ()       : src_8
	dmask  = (mask > 0) ? dmask.Dither_build_gf3_range_mask (mask) : dmask
	dmask  = (mask > 0) ? dmask.mt_lut (expr=mexpr)                : dmask
	dmask  = (mask > 0) ? dmask.Dither_removegrain_emul (22, -1)   : dmask
	dmask  = (mask > 1) ? dmask.Dither_removegrain_emul (11, -1)   : dmask
	dmask  = (mask > 2) ? dmask.Dither_removegrain_emul (20, -1)   : dmask
	dmask  = (mask > 0 && yv411_flag) ? dmask.ConvertToYV411 ()    : dmask

	res_16 = (mask > 0) ? Dither_merge16_8 (flt, src_16, dmask, luma=true, y=yp, u=up, v=vp) : flt

	# Dithering

	result = (lsb) ? res_16 : res_16.DitherPost (
\		mode=mode, ampo=ampo, ampn=ampn, pat=pat, dyn=dyn,
\		prot=false, thr=dthr, staticnoise=staticnoise,
\		y=yp, u=up, v=vp, slice=slice
\	)
	result = (lsb)
\		? Dither_switch_planes16 (src_16, result, y=y, u=u, v=v)
\		: Dither_switch_planes8  (src_8,  result, y=y, u=u, v=v)

	(debug == 1       ) ? dmask.GreyScale ()        : result
	(debug == 1 && lsb) ? Dither_convert_8_to_16 () : last
}



Function Dither_convert_yuv_to_rgb (clip src, string "matrix", bool "interlaced",
\	bool "tv_range", string "cplace", string "chromak", float "fh", float "fv",
\	int "taps", float "a1", float "a2", float "a3",
\	bool "lsb_in", int "mode", float "ampn", string "output", float "ampo",
\	bool "staticnoise", bool "noring", bool "slice")
{
	vers = VersionNumber ()

	tv_range   = Default (tv_range,     true)
	cplace     = Default (cplace,    "MPEG2")
	chromak    = Default (chromak, "bicubic")
	interlaced = Default (interlaced,  false)
	lsb_in     = Default (lsb_in,      false)
	output     = Default (output,    "rgb32")
	noring     = Default (noring,      false)

	dithdef    = (Defined (mode) || Defined (ampn))
	mode       = (dithdef) ? mode : Default (mode,   6)
	ampn       = (dithdef) ? ampn : Default (ampn, 0.5)

	Assert ((! interlaced || src.IsFrameBased ()),
\		"Dither_convert_yuv_to_rgb: cannot process fields, only frames."
\		+ "Use Weave() before.")

	src

	# Conversion to 16 bits

	w = Width ()
	h = (lsb_in) ? Height () / 2 : Height ()
	Assert ((! interlaced || (h % 2) == 0),
\	        "Dither_convert_yuv_to_rgb: interlaced stack16 clips must "
\		+ "have a height multiple of 4.")

	(lsb_in) ? last : Dither_convert_8_to_16 ()

	# Coefficient calculation

	matrix    = Default (matrix, (h >= 600) ? "709" : "601")
	mat_i     = Dither_matrix_to_index (matrix)
	Assert ((mat_i >= 0),
\		"Dither_convert_yuv_to_rgb: unexpected matrix " + matrix + ".")

	#                     BT.601   BT.709  SMPTE 240M  FCC    YCgCo  BT.2020
	kr = Select (mat_i,   0.299,   0.2126,   0.212,   0.30,    0.0,  0.2627)
	kg = Select (mat_i,   0.587,   0.7152,   0.701,   0.59,    0.0,  0.6780)
	kb = Select (mat_i,   0.114,   0.0722,   0.087,   0.11,    0.0,  0.0593)

	offset_y = (tv_range) ?          16.0 : 0.0
	scale_y  = (tv_range) ? (255.0 / 219) : 1.0
	scale_uv = (tv_range) ? (255.0 / 112) : 2.0
	scale_uv = (mat_i == 4) ? scale_uv * 0.5 : scale_uv

	# kr/kg/kb matrix:
	# R = Y + V*(1-Kr)
	# G = Y - U*(1-Kb)*Kb/Kg - V*(1-Kr)*Kr/Kg
	# B = Y + U*(1-Kb)

	# YCgCo matrix (Rec. ITU-T H.264 03/2010, p. 393):
	# R = Y - Cg + Co
	# G = Y + Cg
	# B = Y - Cg - Co

	coef_ry = scale_y  * ((mat_i == 4) ?  1 :  1     )
	coef_ru = scale_uv * ((mat_i == 4) ? -1 :  0     )
	coef_rv = scale_uv * ((mat_i == 4) ?  1 :  1 - kr)
	coef_rc = -coef_ry * offset_y - coef_ru * 128 - coef_rv * 128
	kern_r  = "impulse "+String(coef_ry)+" "+String(coef_ru)
\	                +" "+String(coef_rv)+" "+String(coef_rc) + " 0"

	coef_gy = scale_y  * ((mat_i == 4) ?  1 :  1                 )
	coef_gu = scale_uv * ((mat_i == 4) ?  1 : -(1 - kb) * kb / kg)
	coef_gv = scale_uv * ((mat_i == 4) ?  0 : -(1 - kr) * kr / kg)
	coef_gc = -coef_gy * offset_y - coef_gu * 128 - coef_gv * 128
	kern_g  = "impulse "+String(coef_gy)+" "+String(coef_gu)
\	                +" "+String(coef_gv)+" "+String(coef_gc) + " 0"

	coef_by = scale_y  * ((mat_i == 4) ?  1 :  1     )
	coef_bu = scale_uv * ((mat_i == 4) ? -1 :  1 - kb)
	coef_bv = scale_uv * ((mat_i == 4) ? -1 :  0     )
	coef_bc = -coef_by * offset_y - coef_bu * 128 - coef_bv * 128
	kern_b  = "impulse "+String(coef_by)+" "+String(coef_bu)
\	                +" "+String(coef_bv)+" "+String(coef_bc) + " 0"

	# Pixel processing

	# Luma: easy
	y = (vers < 2.60) ? last : last.ConvertToY8 ()

	# Chroma: first, find the right chroma shift to apply when resizing
	# the planes. Interlaced contents adds a bit of complexity…
	chr  = (src.GetParity ()) ? AssumeTFF () : AssumeBFF ()
	chr  = (interlaced) ? chr.SeparateFields () : chr
	idiv = (interlaced) ? 2 : 1
	hf   = h / idiv
	u    =   (vers < 2.60)      ? chr.UToY ()
\	       : src.Dither_isy8 () ? chr.mt_lut (y=-128)
\	       :                      chr.UToY8 ()
	v    =   (vers < 2.60)      ? chr.VToY ()
\	       : src.Dither_isy8 () ? chr.mt_lut (y=-128)
\	       :                      chr.VToY8 ()

	subspl_h = src.Dither_get_chroma_subspl_h ()
	subspl_v = src.Dither_get_chroma_subspl_v () * idiv

	Assert (    u.Width () %  u.Dither_get_chroma_subspl_h ()      == 0
\	        && u.Height () % (u.Dither_get_chroma_subspl_v () * 2) == 0,
\		 "Dither_convert_yuv_to_rgb: supports only mod-"
\		+String(subspl_h*u.Dither_get_chroma_subspl_h ())+" width and mod-"
\		+String(subspl_v*u.Dither_get_chroma_subspl_v ())+" height"
\		+Chr(10)+"for this combination of colorspace, interlacing and Avisynth version.")

	pr           = (chromak == "point")
	cp_src_h     = Dither_get_chroma_placement_h (src, cplace, pr)

	# direction / field / plane
	cp_src_v_t_u = Dither_get_chroma_placement_v (src, cplace, interlaced, true,  true,  pr)
	cp_src_v_t_v = Dither_get_chroma_placement_v (src, cplace, interlaced, true,  false, pr)
	cp_src_v_b_u = Dither_get_chroma_placement_v (src, cplace, interlaced, false, true,  pr)
	cp_src_v_b_v = Dither_get_chroma_placement_v (src, cplace, interlaced, false, false, pr)

	# Converts the chroma placements to chroma shifts
	cs_h     = (0.0 - cp_src_h    ) / subspl_h
	cs_v_t_u = (0.0 - cp_src_v_t_u) / subspl_v
	cs_v_t_v = (0.0 - cp_src_v_t_v) / subspl_v
	cs_v_b_u = (1.0 - cp_src_v_b_u) / subspl_v
	cs_v_b_v = (1.0 - cp_src_v_b_v) / subspl_v

	u = (interlaced)
\	?	u.Dither_convert_yuv_to_rgb_resize_fields (
\			w, hf, cs_h, cs_v_t_u, cs_v_b_u, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	: (subspl_h > 1 || subspl_v > 1)
\	?	u.Dither_convert_yuv_to_rgb_resize_frames (
\			w, hf, cs_h, cs_v_t_u, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	:	u
	v = (interlaced)
\	?	v.Dither_convert_yuv_to_rgb_resize_fields (
\			w, hf, cs_h, cs_v_t_v, cs_v_b_v, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	: (subspl_h > 1 || subspl_v > 1)
\	?	v.Dither_convert_yuv_to_rgb_resize_frames (
\			w, hf, cs_h, cs_v_t_v, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	:	v

	# Finally, done with the chroma planes!

	# Constant multiplier
	c = y.Dither_lut16 ("256", u=1, v=1)

	m0 = Interleave (y,  v ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m1 = Interleave (u,  c ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m  = Interleave (m0, m1).AssumeFieldBased ().AssumeTFF ().Weave ()

	r = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_r, center=false, cnorm=false, u=1, v=1)
	g = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_g, center=false, cnorm=false, u=1, v=1)
	b = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_b, center=false, cnorm=false, u=1, v=1)

	(  output == "rgb32"
\	|| output == "rgb24")      ? Dither_convert_yuv_to_rgb_conv_to_rgb32 (
\		r, g, b,
\		interlaced, output, GetParity (),
\		mode=mode, ampo=ampo, ampn=ampn, staticnoise=staticnoise, slice=slice
\	                             )
\	: (output == "rgb48yv12")  ? Dither_convey_rgb48_on_yv12 (r, g, b)
\	: (output == "rgb48y")     ? Interleave (r, g, b)
\	: Assert (false, "Dither_convert_stack16_to_rgb: unexpected output format.")
}



Function Dither_convert_rgb_to_yuv (clip src, clip "g", clip "b",
\	string "matrix", bool "interlaced", bool "tv_range", string "cplace",
\	string "chromak", float "fh", float "fv", int "taps", float "a1",
\	float "a2", float "a3", bool "lsb", int "mode", float "ampn",
\	string "output", float "ampo", bool "staticnoise", bool "noring",
\	bool "slice")
{
	vers = VersionNumber ()

	tv_range   = Default (tv_range,     true)
	cplace     = Default (cplace,    "MPEG2")
	chromak    = Default (chromak, "bicubic")
	interlaced = Default (interlaced,  false)
	output     = Default (output,     "YV12")
	noring     = Default (noring,      false)

	Assert ((! interlaced || src.IsFrameBased ()),
\		"Dither_convert_yuv_to_rgb: cannot process fields, only frames."
\		+ "Use Weave() before.")

	src

	lsb_in = (Defined (g) && defined (b))
	lsb    = Default (lsb, lsb_in)
	Assert (   (  lsb_in && src.IsPlanar () && g.IsPlanar () && b.IsPlanar ())
\	        || (! lsb_in && src.IsRGB ()),
\	        "Dither_convert_rgb_to_yuv: "
\		+ "Input can only be 1 RGB or 3 planar stack16 clips.")

	w = Width ()
	h = Height ()
	h = (lsb_in) ? h / 2 : h

	# Conversion to 16 bits

	tmp_fmt = (vers < 2.60) ? "YV12" : "Y8"
	r = (lsb_in) ? src : src.ShowRed   (tmp_fmt).Dither_convert_8_to_16 ()
	g = (lsb_in) ?   g : src.ShowGreen (tmp_fmt).Dither_convert_8_to_16 ()
	b = (lsb_in) ?   b : src.ShowBlue  (tmp_fmt).Dither_convert_8_to_16 ()

	# Constant multiplier.
	c = r.Dither_lut16 ("256", u=1, v=1)

	# Coefficient calculation

	matrix    = Default (matrix, (h >= 600) ? "709" : "601")
	mat_i     = Dither_matrix_to_index (matrix)
	Assert ((mat_i >= 0),
\		"Dither_convert_to_rgb: unexpected matrix" + matrix + ".")

	#                     BT.601   BT.709  SMPTE 240M  FCC    YCgCo  BT.2020
	kr = Select (mat_i,   0.299,   0.2126,   0.212,   0.30,    0.0,  0.2627)
	kg = Select (mat_i,   0.587,   0.7152,   0.701,   0.59,    0.0,  0.6780)
	kb = Select (mat_i,   0.114,   0.0722,   0.087,   0.11,    0.0,  0.0593)

	offset_y = (tv_range) ?          16.0 : 0.0
	scale_y  = (tv_range) ? (219 / 255.0) : 1.0
	scale_uv = (tv_range) ? (112 / 255.0) : 0.5
	scale_uv = (mat_i == 4) ? scale_uv * 2 : scale_uv

	# kr/kg/kb matrix:
	# Y =                  R * Kr        + G * Kg        + B * Kb
	# U = (B-Y)/(1-Kb) = - R * Kr/(1-Kb) - G * Kg/(1-Kb) + B
	# V = (R-Y)/(1-Kr) =   R             - G * Kg/(1-Kr) - B * Kb/(1-Kr)

	# YCgCo matrix (Rec. ITU-T H.264 03/2010, p. 393):
	# Y  =  0.25 * R + 0.5  * G + 0.25 * B
	# Cg = -0.25 * R + 0.5  * G - 0.25 * B
	# Co =  0.5  * R            - 0.5  * B

	coef_yr =  scale_y  * ((mat_i == 4) ?  0.25 : kr)
	coef_yg =  scale_y  * ((mat_i == 4) ?  0.5  : kg)
	coef_yb =  scale_y  * ((mat_i == 4) ?  0.25 : kb)
	coef_yc =  offset_y
	kern_y  = "impulse "+String(coef_yr)+" "+String(coef_yg)
\	                +" "+String(coef_yb)+" "+String(coef_yc) + " 0"

	coef_ur = scale_uv * ((mat_i == 4) ? -0.25 : -kr / (1 - kb))
	coef_ug = scale_uv * ((mat_i == 4) ?  0.5  : -kg / (1 - kb))
	coef_ub = scale_uv * ((mat_i == 4) ? -0.25 :  1            )
	coef_uc =  128
	kern_u  = "impulse "+String(coef_ur)+" "+String(coef_ug)
\	                +" "+String(coef_ub)+" "+String(coef_uc) + " 0"

	coef_vr = scale_uv * ((mat_i == 4) ?  0.5  :  1            )
	coef_vg = scale_uv * ((mat_i == 4) ?  0    : -kg / (1 - kr))
	coef_vb = scale_uv * ((mat_i == 4) ? -0.5  : -kb / (1 - kr))
	coef_vc =  128
	kern_v  = "impulse "+String(coef_vr)+" "+String(coef_vg)
\	                +" "+String(coef_vb)+" "+String(coef_vc) + " 0"

	# Pixel processing

	m0 = Interleave (r,  b ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m1 = Interleave (g,  c ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m  = Interleave (m0, m1).AssumeFieldBased ().AssumeTFF ().Weave ()
	m  = (src.GetParity ()) ? m.AssumeTFF () : m.AssumeBFF ()

	y = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_y, center=false, cnorm=false, u=1, v=1)
	u = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_u, center=false, cnorm=false, u=1, v=1)
	v = m.Dither_resize16 (w, h, src_top=2, fv=4, kernel=kern_v, center=false, cnorm=false, u=1, v=1)

	# Luma: nothing to do

	# Chroma: first, find the right chroma shift to apply when resizing
	# the planes Interlaced contents adds a bit of complexity…

	# We don't use the pixels of this clip, it just helps us to know
	# the chroma plane size.
	p =   (vers < 2.60)       ? y
\	    : (output == "YV12")  ? y.ConvertToYV12 ()
\	    : (output == "YV16")  ? y.ConvertToYV16 ()
\	    : (output == "YV24")  ? y.ConvertToYV24 ()
\	    : (output == "YV411") ? y.ConvertToYV411 ()
\	    : (output == "Y8")    ? y.ConvertToY8 ()
\	    : Assert ("Dither_convert_rgb_to_yuv: unsupported output colorspace.")

	wc   = p.Dither_isy8 () ? w : p.UToY ().Width ()
	hc   = p.Dither_isy8 () ? h : (p.UToY ().Height () / 2)
	idiv = (interlaced) ? 2 : 1
	hcf  = hc / idiv

	pr           = (chromak == "point")
	cp_dst_h     = Dither_get_chroma_placement_h (p, cplace, pr)

	# direction / field / plane
	cp_dst_v_t_u = Dither_get_chroma_placement_v (p, cplace, interlaced, true,  true,  pr)
	cp_dst_v_t_v = Dither_get_chroma_placement_v (p, cplace, interlaced, true,  false, pr)
	cp_dst_v_b_u = Dither_get_chroma_placement_v (p, cplace, interlaced, false, true,  pr)
	cp_dst_v_b_v = Dither_get_chroma_placement_v (p, cplace, interlaced, false, false, pr)

	# Converts the chroma placements to chroma shifts
	cs_h     =  cp_dst_h     - 0.0
	cs_v_t_u = (cp_dst_v_t_u - 0.0) / idiv 
	cs_v_t_v = (cp_dst_v_t_v - 0.0) / idiv 
	cs_v_b_u = (cp_dst_v_b_u - 1.0) / idiv 
	cs_v_b_v = (cp_dst_v_b_v - 1.0) / idiv 

	u = (interlaced)
\	?	u.Dither_convert_rgb_to_yuv_resize_fields (
\			wc, hcf, cs_h, cs_v_t_u, cs_v_b_u, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	: (output != "Y8")
\	?	u.Dither_convert_rgb_to_yuv_resize_frames (
\			wc, hcf, cs_h, cs_v_t_u, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	:	u
	v = (interlaced)
\	?	v.Dither_convert_rgb_to_yuv_resize_fields (
\			wc, hcf, cs_h, cs_v_t_v, cs_v_b_v, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	: (output != "Y8")
\	?	v.Dither_convert_rgb_to_yuv_resize_frames (
\			wc, hcf, cs_h, cs_v_t_v, chromak,
\			fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3,
\			noring=noring)
\	:	v

	# Finally, done with the chroma planes!

	# Merges all the planes

	(output == "Y8") ? y : Dither_y_to_uv (u, v, y)
	(lsb) ? last : DitherPost (
\		mode=mode, ampo=ampo, ampn=ampn,
\		interlaced=interlaced, staticnoise=staticnoise, slice=slice
\	)
}



Function Dither_srgb_display (clip src, int "debug",
\	bool "lsb_in", string "matrix", bool "tv_range", string "chromak",
\	int "w", int "h", string "kernel", int "taps",
\	bool "noring", bool "precrop", bool "autopar",
\	float "par", int "parnum", int "parden",
\	float "dar", int "darnum", int "darden",
\	float "src_left", float "src_top", float "src_width", float "src_height",
\	float "gamma", bool "center", bool "lsb", string "cplace",
\	int "bkgcolor", bool "sigmoid", bool "slice", int "dispm")
{
	Assert (src.IsYUV (), "Dither_srgb_display: only YUV or Y input.")
	Assert (   (! Defined (par) && (! Defined (parnum) && ! Defined (parden)))
\	        || (! Defined (par) && (  Defined (parnum) &&   Defined (parden)))
\	        || (  Defined (par) && (! Defined (parnum) && ! Defined (parden))),
\	        "Dither_srgb_display: parnum and parden must be defined simultaneously,"
\	        +Chr(13)+Chr(10)+"and only if par is not defined.")
	Assert (   (! Defined (dar) && (! Defined (darnum) && ! Defined (darden)))
\	        || (! Defined (dar) && (  Defined (darnum) &&   Defined (darden)))
\	        || (  Defined (dar) && (! Defined (darnum) && ! Defined (darden))),
\	        "Dither_srgb_display: darnum and darden must be defined simultaneously,"
\	        +Chr(13)+Chr(10)+"and only if dar is not defined.")

	debug   = Default (debug, 0)
	lsb_in  = Default (lsb_in, false)
	lsb     = Default (lsb, false)
	noring  = Default (noring, true)
	precrop = Default (precrop, false)
	kernel  = Default (kernel, (noring) ? "spline64" : "spline36")
	autopar = Default (autopar, true)
	par     = Default (par, (Defined (parnum) && Defined (parden)) ? Float (parnum) / Float (parden) : par)
	dar     = Default (dar, (Defined (darnum) && Defined (darden)) ? Float (darnum) / Float (darden) : dar)
	gamma   = Default (gamma, 2.2)
	center  = Default (center, true)
	dispm   = Default (dispm, -1)

	dest_siz_def = (Defined (w) && Defined (h))
	dest_par_def = Defined (par)
	dest_dar_def = Defined (dar)
	src_l_def    = Defined (src_left)
	src_t_def    = Defined (src_top )
	src_area_def = (   src_l_def || Defined (src_width )
\	                || src_t_def || Defined (src_height))

	Assert (! dest_siz_def || ! dest_dar_def,
\		"Dither_srgb_display: you cannot set both DAR and complete destination size.")

	src_left = Default (src_left, 0.0)
	src_top  = Default (src_top,  0.0)

	# If we must use YV12 instead of Y8 for temporary clips
	chrss   = (VersionNumber () < 2.60) ? 2 : 1
	Assert (! Defined (w) || (w % chrss) == 0, "Dither_srgb_display: w must be a multiple of 2.")
	Assert (! Defined (h) || (h % chrss) == 0, "Dither_srgb_display: h must be a multiple of 2.")

	swo  = src.Width ()
	sho2 = src.Height ()
	sho  = (lsb_in) ? sho2 / 2 : sho2

	# Identify common resolutions from the original source size
	sd_pal  = (sho >=  568 && sho <= 576 && swo >=  700 && swo <= 720)
	sd_ntsc = (sho >=  472 && sho <= 486 && swo >=  700 && swo <= 720)
	hdtv    = (sho == 1080               && swo == 1440              )

	sw = Float ((precrop && (sd_pal || sd_ntsc)) ? 704 : swo)
	sh = Float (                                         sho)

	src_left = (src_l_def) ? src_left : src_left + (swo - sw) * 0.5

	# Uses the input window size as source size
	sww      = Float (Defined (src_width ) ? src_width  : sw)
	shw      = Float (Defined (src_height) ? src_height : sh)
	src_left = (center && ! src_l_def) ? src_left + (sw - sww) * 0.5 : src_left
	src_top  = (center && ! src_t_def) ? src_top  + (sh - shw) * 0.5 : src_top 
	sw       = sww
	sh       = shw

	# Finds the DAR
	dar    =
\	  (dest_siz_def) ? Float (w)  / Float (h)
\	: (dest_dar_def) ? dar
\	:                  Float (sw) / Float (sh)
	dar169 = ((dar > 1.75 && dar < 1.80) || hdtv)
	dar43  =  (dar > 1.30 && dar < 1.35)

	# Finds the PAR. Takes precedence over DAR.
	parguess =
\	  (dest_siz_def) ? Float (sh) * Float (w) / (Float (sw) * Float (h))
\	: (dest_dar_def) ? Float (sh) * dar       /  Float (sw)
\	:                  1.0
	parguess =
\	  (! autopar && ! dest_par_def) ? parguess
\	: (dar169) ? ((sd_pal) ? 16.0/11.0 : (sd_ntsc) ? 40.0/33.0 : (hdtv) ? 4.0/3.0 : parguess)
\	: (dar43 ) ? ((sd_pal) ? 12.0/11.0 : (sd_ntsc) ? 10.0/11.0 : (hdtv) ? 1.0     : parguess)
\	:            parguess

	par_o = par
	par = Default (par, parguess)

	# If only DAR or PAR has been defined, assigns a partial destination size
	darorparonly = (   ! Defined (w) && ! Defined (h)
\	                && (dest_dar_def || dest_par_def || autopar))
	w = (darorparonly && par <  1.0) ? Round (sw) : w
	h = (darorparonly && par >= 1.0) ? Round (sh) : h

	# Computes partial destination sizes from the other dimension, the PAR
	# and the source size.
	# dw * parden * sh = dh * parnum * sw
	dw_flt = (Defined (h)) ? (Float (h) * Float (sw) * par) /  Float (sh)  : Defined (w) ? Float (w) : w
	dh_flt = (Defined (w)) ? (Float (w) * Float (sh)) / (par * Float (sw)) : Defined (h) ? Float (h) : h

	# Fixed destination size: keep only the fitting rectangle
	dw_flt = (dest_siz_def && dw_flt > w) ? Float (w) : dw_flt
	dh_flt = (dest_siz_def && dh_flt > h) ? Float (h) : dh_flt

	dw     = (Defined (h)) ? Floor (dw_flt + chrss * 0.5) / chrss * chrss : w
	dh     = (Defined (w)) ? Floor (dh_flt + chrss * 0.5) / chrss * chrss : h

	# In case of fixed destination size, we might need to add
	# pillarbox or letterbox because of the PAR
	dw = (dest_siz_def) ? Dither_min (dw, w) : dw
	dh = (dest_siz_def) ? Dither_min (dh, h) : dh

	# Makes dw and dh always defined
	dw = Defined (dw) ? dw : swo
	dh = Defined (dh) ? dh : sho

	# Compensates for rounding errors to get the exact specified PAR
	src_width  =                             sw * Float (dw) / dw_flt
	src_left   = (center) ? src_left + (sw - sw * Float (dw) / dw_flt) * 0.5 : src_left
	src_height =                             sh * Float (dh) / dh_flt
	src_top    = (center) ? src_top  + (sh - sh * Float (dh) / dh_flt) * 0.5 : src_top

	src_width  = Dither_kill_rounding_error (src_width )
	src_left   = Dither_kill_rounding_error (src_left  )
	src_height = Dither_kill_rounding_error (src_height)
	src_top    = Dither_kill_rounding_error (src_top   )

	# Indicates if resizing is needed
	rsz = (   src_left  != 0   || src_width  != swo
\	       || src_top   != 0   || src_height != sho
\	       || par       != 1.0 || dw != swo || dh != sho)

	matrix = Default (matrix, (sho >= 600) ? "709" : "601")

	# Conversion to linear RGB
	gam_flag = (gamma != 2.2 || dispm != 0)
	to_lin   =   (dispm == -1) ? "709"
\	           : (dispm ==  2) ? "1886a"
\	           :                 "1886"
	to_gam   =   (dispm == -1) ? "709" : "sRGB"

	src
	Dither_convert_yuv_to_rgb (matrix=matrix, tv_range=tv_range, output="rgb48y", lsb_in=lsb_in, chromak=chromak, noring=noring, cplace=cplace)
	(rsz || gam_flag) ? Dither_y_gamma_to_linear (tv_range_in=false, tv_range_out=false, u=1, v=1, curve=to_lin, sigmoid=sigmoid) : last

	# Optionnal resizing
	(rsz) ? Dither_resize16nr (dw, dh,
\			src_left=src_left, src_top=src_top,
\			src_width=src_width, src_height=src_height,
\			kernel=kernel, taps=taps, u=1, v=1, noring=noring)
\	: last

	# Conversion to sRGB. Actually it just reencodes to BT.709.
	(rsz || gam_flag) ? Dither_y_linear_to_gamma (tv_range_in=false, tv_range_out=false, u=1, v=1, curve=to_gam, gcor=gamma/2.2, sigmoid=sigmoid) : last

	(lsb) ? last : DitherPost (mode=6, slice=slice, u=1, v=1)
	MergeRGB (SelectEvery (3, 0), SelectEvery (3, 1), SelectEvery (3, 2), pixel_type="RGB32")

	bw_l = (dest_siz_def && center) ? (w - dw) / 2 : 0
	bw_t = (dest_siz_def && center) ? (h - dh) / 2 : 0
	(dest_siz_def && (dw < w || dh < h))
\		? ((lsb) ? Dither_addborders16 (bw_l, bw_t, w - dw - bw_l, h - dh - bw_t, color=bkgcolor)
\		         :          AddBorders (bw_l, bw_t, w - dw - bw_l, h - dh - bw_t, color=bkgcolor))
\		: last

	(debug == 1)
\	? Subtitle ("matrix="+matrix+", sd_pal="+String(sd_pal)+", sd_ntsc="+String(sd_ntsc)+", hdtv="+String(hdtv)
\		+"\npar(original)="+String(par_o)+", par(final)="+String(par)+", autopar="+String(autopar)
\		+((dest_dar_def) ? ("\ndarnum="+String(darnum)+", darden="+String(darden)+", dar="+String(dar)) : "")
\		+"\nsw="+String(sw)+", sh="+String(sh)
\		+((rsz)          ? ("\ndw="+String(dw)+", dh="+String(dh)+", dw_flt="+String(dw_flt)+", dh_flt="+String(dh_flt)) : "")
\		+((rsz)          ? ( "\nsrc_left="+String(src_left)+", src_top="+String(src_top)
\		                    +", src_width="+String(src_width)+", src_height="+String(src_height)) : ""),
\		lsp=1)
\	: last
}



Function Dither_convey_yuv4xxp16_on_yvxx (clip src, bool "bigendian")
{
	src
	Dither_out (bigendian=bigendian)
}



Function Dither_convey_rgb48_on_yv12 (clip r, clip g, clip b, bool "bigendian")
{
	bigendian = Default (bigendian, false)

	Assert (r.IsYUV () && g.IsYUV () && b.IsYUV (),
\		"Dither_convey_rgb48_on_yv12: r, g and b must be YUV.")

	r = r.ConvertToYV12 ()
	g = g.ConvertToYV12 ()
	b = b.ConvertToYV12 ()

	w = r.Width ()
	h = r.Height () / 2

	m0 = Interleave (r,  b ).TurnRight ().AssumeFieldBased ().AssumeTFF ().Weave ()
	m1 = Interleave (g,  b ).TurnRight ().AssumeFieldBased ().AssumeTFF ().Weave ()
	m  = Interleave (m0, m1).AssumeFieldBased ().AssumeTFF ().Weave ()
	m.PointResize (h * 2, w * 3)

	lsb = Crop (0, 0, h, 0)
	msb = Crop (h, 0, 0, 0)
	(bigendian) ? Interleave (msb, lsb) : Interleave (lsb, msb)
	AssumeFieldBased ().AssumeTFF ()
	Weave ()

	TurnLeft ()	# Now we have a 6*w x h pic

	a = Crop (w * 0, 0, w * 2, 0)
	b = Crop (w * 2, 0, w * 2, 0)
	c = Crop (w * 4, 0, w * 2, 0)

	m0 = Interleave (a,  c ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m1 = Interleave (b,  c ).AssumeFieldBased ().AssumeTFF ().Weave ()
	m  = Interleave (m0, m1).AssumeFieldBased ().AssumeTFF ().Weave ()
	m.PointResize (w * 2, h * 3)

	y  = Crop (0, 0,     w * 2, h * 2)
	k1 = Crop (0, h * 2, w    , h    )
	k2 = Crop (w, h * 2, w    , h    )
	Interleave (k1, k2).AssumeFieldBased ().AssumeTFF ().Weave ()

	YToUV (Crop (0, 0, 0, h), Crop (0, h, 0, 0), y)
}



Function Dither_resize16nr (clip src, int width, int height,
\	float  "src_left",
\	float  "src_top",
\	float  "src_width",
\	float  "src_height",
\	string "kernel",
\	float  "fh",
\	float  "fv",
\	int    "taps",
\	float  "a1",
\	float  "a2",
\	float  "a3",
\	int    "kovrspl",
\	bool   "cnorm",
\	bool   "center",
\	string "cplace",
\	int    "y",
\	int    "u",
\	int    "v",
\	string "kernelh",
\	string "kernelv",
\	float  "totalh",
\	float  "totalv",
\	bool   "invks",
\	bool   "invksh",
\	bool   "invksv",
\	int    "invkstaps",
\	string "cplaces",
\	string "cplaced",
\	string "csp",
\	bool   "noring")
{
	noring = Default (noring, true)

	Assert (width > 0 && height > 0, "Dither_resize16nr: width and height must be > 0.")

	sr_h  = Float (width ) / Float (src.width () )
	sr_v  = Float (height) / Float (src.height ())
	sr_up =       Dither_max (sr_h, sr_v)
	sr_dw = 1.0 / Dither_min (sr_h, sr_v)
	sr    = Dither_max (sr_up, sr_dw)
	Assert (sr >= 1.0)

	# Depending on the scale ratio, we may blend or totally disable
	# the ringing cancellation
	thr = 2.5
	nrb = (sr > thr)
	nrf = (sr < thr + 1.0 && noring)
	nrr = (nrb) ? Dither_min (sr - thr, 1.0) : 1.0
	nrv = (nrb) ? Round ((1.0 - nrr) * 255) * $010101 : 0

	main = src.Dither_resize16 (width, height,
\		src_left  =src_left,
\		src_top   =src_top,
\		src_width =src_width,
\		src_height=src_height,
\		kernel    =kernel,
\		fh        =fh,
\		fv        =fv,
\		taps      =taps,
\		a1        =a1,
\		a2        =a2,
\		a3        =a3,
\		kovrspl   =kovrspl,
\		cnorm     =cnorm,
\		center    =center,
\		cplace    =cplace,
\		y         =y,
\		u         =u,
\		v         =v,
\		kernelh   =kernelh,
\		kernelv   =kernelv,
\		totalh    =totalh,
\		totalv    =totalv,
\		invks     =invks,
\		invksh    =invksh,
\		invksv    =invksv,
\		invkstaps =invkstaps,
\		cplaces   =cplaces,
\		cplaced   =cplaced,
\		csp       =csp
\	)

	nrng = (nrf) ? src.Dither_resize16 (width, height,
\		src_left  =src_left,
\		src_top   =src_top,
\		src_width =src_width,
\		src_height=src_height,
\		kernel    ="gauss",
\		a1        =100,
\		center    =center,
\		cplace    =cplace,
\		cplaces   =cplaces,
\		cplaced   =cplaced,
\		csp       =csp,
\		y         =y,
\		u         =u,
\		v         =v
\	) : main

	nrm = (nrb && nrf) ? main.BlankClip (color_yuv=nrv, height=main.Height()/2) : main

	# To do: use a simple frame blending instead of Dither_merge16
	rgm = 1
	rgc = (nrb) ? -1 : 0
	rgy = Defined (y) ? ((y == 3) ? rgm : rgc) : rgm
	rgu = Defined (u) ? ((u == 3) ? rgm : rgc) : rgm
	rgv = Defined (v) ? ((v == 3) ? rgm : rgc) : rgm
	rguv = Dither_max (rgu, rgv)
	(nrf       ) ? main.Dither_repair16 (nrng, rgy, rguv)            : main
	(nrf && nrb) ? Dither_merge16_8 (main, last, nrm, y=y, u=u, v=v) : last
}



Function Dither_add_grain16 (clip src, float "var", float "uvar",
\	float "soft", int "maxts", bool "replace", bool "lsb_in", int "seed")
{
	var     = Default (var,  1.0)
	uvar    = Default (uvar, 0.0)
	replace = Default (replace, false)
	lsb_in  = Default (lsb_in,   true)

	src

	template = (lsb_in) ? Dither_get_msb () : last
	grain = template.Dither_gen_grain_internal (
\		soft=soft, maxts=maxts, var=64, uvar=64, seed=seed)
	grain = grain.Dither_lut8 (
\		expr="x 128 - "+String(sqrt(uvar*64)*4)+" * 32768 +",
\		yexpr="x 128 - "+String(sqrt(var*64)*4)+" * 32768 +",
\		y=3, u=3, v=3
\	)

	(! replace && ! lsb_in) ? Dither_convert_8_to_16 () : last
	(  replace            ) ? grain : Dither_add16 (last, grain, dif=true)
}



Function Dither_quantize (clip src, int "bitdepth", bool "reducerange",
\	int "mode", float "ampo", float "ampn", int "pat", bool "dyn", bool "prot",
\	clip "mask", float "thr", bool "interlaced", int "y", int "u", int "v",
\	bool "staticnoise")
{
	Assert (Defined (bitdepth), "Dither_quantize: you must specify bitdepth.")
	Assert (bitdepth >= 8 && bitdepth <= 14,
\		"Dither_quantize: bitdepth should be in range 8-14.")

	y = Default (y, 3) 
	u = Default (u, 3) 
	v = Default (v, 3) 
	reducerange = Default (reducerange, false)

	yp = (y < 3) ? 1 : y
	up = (u < 3) ? 1 : u
	vp = (v < 3) ? 1 : v

	src
	msb = Dither_get_msb ()
	lsb = Dither_get_lsb ()
	hr = 2

	m = String (Round (Pow (2, 16 - hr - bitdepth)) - 1)
	l = msb.mt_lut ("x     "+m+"   &u", y=yp, u=up, v=vp)
	h = msb.mt_lut ("x 255 "+m+" - &u", y=yp, u=up, v=vp)

	s1 = String (bitdepth - 8)
	ofs = (bitdepth < 14) ? String (Round (Pow (2, 7 - hr))) : "0"
	a = StackVertical (l, lsb)
	a = a.Dither_lut16 ("x "+s1+" <<u "+ofs+" 256 * +", y=yp, u=up, v=vp)

	b = a.DitherPost (mode=mode, ampo=ampo, ampn=ampn, pat=pat, dyn=dyn,
\		prot=prot, mask=mask, thr=thr, interlaced=interlaced,
\		y=yp, u=up, v=vp, staticnoise=staticnoise)

	(reducerange)
\	?	Dither_quantize_out_n (b, h, bitdepth, hr, s1, ofs, m, y=y, u=up, v=vp)
\	:	Dither_quantize_out_16 (b, h, bitdepth, hr, s1, ofs, y=y, u=up, v=vp)

	out_m = SelectEven ()
	out_l = SelectOdd ()
	out_m = (y != 1 || u != 1 || v != 1) ? mt_lutxy (
\		msb, out_m, expr="y",
\		y=(y == 2) ? 2 : (y == 3) ? 4 : (y <= 0) ? y / 256 : y,
\		u=(u == 2) ? 2 : (u == 3) ? 4 : (u <= 0) ? u / 256 : u,
\		v=(v == 2) ? 2 : (v == 3) ? 4 : (v <= 0) ? v / 256 : v
\	) : out_m
	out_l = (y != 1 || u != 1 || v != 1) ? mt_lutxy (
\		lsb, out_l, expr="y",
\		y=(y == 2) ? 2 : (y == 3) ? 4 : (y <= 0) ? y % 256 : y,
\		u=(u == 2) ? 2 : (u == 3) ? 4 : (u <= 0) ? u % 256 : u,
\		v=(v == 2) ? 2 : (v == 3) ? 4 : (v <= 0) ? v % 256 : v
\	) : out_l

	StackVertical (out_m, out_l)
}



Function DitherTestPatterns (int "size", int "mag", int "amp", int "len", int "luma")
{
	size = Default (size, 192)
	mag  = Default (mag,    1)
	amp  = Default (amp,    1)
	len  = Default (len,   24)
	luma = Default (luma, 128)

	sr = (size / (mag * 2)) * 2

	c = BlankClip (length=len, width=sr, height=sr, pixel_type="YV12", fps=24000, fps_denominator=1001)
	c = c.KillAudio ()
	c = mt_lutspa (
\		c,
\		yexpr="  x       2 ^   y       2 ^ + 2    / 0.5 ^ 256 *",
\		uexpr="1 x -     2 ^   y 0.5 * 2 ^ + 1.25 / 0.5 ^ 256 *",
\		vexpr="  x 0.5 * 2 ^ 1 y -     2 ^ + 1.25 * 0.5 ^ 256 *",
\		relative=true, y=3, u=3, v=3
\	)
	lsb = c.mt_lut (expr="x 2 * 256 %", y=3, u=3, v=3)
	msb = c.mt_lut (expr="x 2 * x 2 * 256 % - 256 / 127 +", y=3, u=3, v=3)
	c16 = StackVertical (msb, lsb)

	z00 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=-1)", "None")
	z10 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.9, ampo=0.0, dyn=False)", "0n0.9s")
	z20 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=1.5, ampo=0.0, dyn=False)", "0n1.5s")
	z30 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=2.0, ampo=0.0, dyn=False)", "0n2.0s")
	z01 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.0, ampo=1.0, dyn=False)", "0o1.0s")
	z02 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.0, ampo=1.0, dyn=True )", "0o1.0d")
	z03 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.0, ampo=2.0, dyn=False)", "0o2.0s")
	z11 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.1, ampo=1.0, dyn=False)", "0n0.1o1.0s")
	z22 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.0, ampo=1.0, thr=0.05, dyn=False)", "0o1.0t0.05s")
	z33 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, ampn=0.0, ampo=3.0, thr=0.05, dyn=False)", "0o3.0t0.05s")
	x00 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=1, dyn=False)", "1s")
	x01 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=1, dyn=True )", "1d")
	y10 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=2, dyn=False)", "2p0s")
	y11 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=2, dyn=True )", "2p0d")
	y12 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=1, mode=2, dyn=True )", "2p1d")
	y13 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=2, mode=2, dyn=True )", "2p2d")
	y20 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=3, dyn=False)", "3p0s")
	y21 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=3, dyn=True )", "3p0d")
	y22 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=1, mode=3, dyn=True )", "3p1d")
	y23 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=2, mode=3, dyn=True )", "3p2d")
	y30 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=4, dyn=False)", "4p0s")
	y31 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=4, dyn=True )", "4p0d")
	y32 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=1, mode=4, dyn=True )", "4p1d")
	y33 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=2, mode=4, dyn=True )", "4p2d")
	y40 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=5, dyn=False)", "5p0s")
	y41 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=0, mode=5, dyn=True )", "5p0d")
	y42 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=1, mode=5, dyn=True )", "5p1d")
	y43 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, pat=2, mode=5, dyn=True )", "5p2d")
	e00 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=6, ampo=1.0, ampn=0.0, prot=false, dyn=false)", "FSn0.0")
	e01 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=6, ampo=1.0, ampn=0.8, prot=false, dyn=false)", "FSn0.8")
	e02 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=6, ampo=1.4, ampn=0.0, prot=false, dyn=false)", "FSo1.4")
	e03 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=6, ampo=1.3, ampn=0.2, prot=false, dyn=false)", "FSo1.3n0.2")
	e04 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=7, ampo=1.0, ampn=0.0, prot=false, dyn=false)", "STo0.0")
	e05 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=8, ampo=1.0, ampn=0.0, prot=false, dyn=false)", "ATo0.0")
	e06 = Dither_test_single (c16, mag, amp, luma, "DitherPost (c16, mode=8, ampo=1.6, ampn=0.0, prot=false, dyn=false)", "ATo1.6")

	k0 = StackVertical (z00, z10, z20, z30, e00)
	k1 = StackVertical (x00, z01, z02, z03, e01)
	k2 = StackVertical (x01, z11, z22, z33, e02)
	k3 = StackVertical (y10, y11, y12, y13, e03)
	k4 = StackVertical (y20, y21, y22, y23, e04)
	k5 = StackVertical (y30, y31, y32, y33, e05)
	k6 = StackVertical (y40, y41, y42, y43, e06)

	StackHorizontal (k0, k1, k2, k3, k4, k5, k6)
}



### Tools for 16-bit clips ###################################################



Function Dither_get_msb (clip src)
{
	src
	w = Width ()
	h = Height () / 2
	Crop (0, 0, w, h)
}



Function Dither_get_lsb (clip src)
{
	src
	w = Width ()
	h = Height () / 2
	Crop (0, h, w, h)
}



Function Dither_gen_null_lsb (clip src)
{
	vers = VersionNumber ()
	p_t = (vers < 2.60) ? "YV12" : Dither_undef ()
	BlankClip (src, pixel_type=p_t, color_yuv=0)
}



Function Dither_convert_8_to_16 (clip src)
{
	StackVertical (src, src.Dither_gen_null_lsb ())
}



Function Dither_crop16 (clip src, int "left", int "top",
\	int "width", int "height", bool "align")
{
	left   = Default (left,   0)
	top    = Default (top,    0)
	width  = Default (width,  0)
	height = Default (height, 0)

	src
	w = Width ()
	h = Height () / 2

	width  = (width  <= 0) ? w - left + width  : width
	height = (height <= 0) ? h - top  + height : height
	Assert (width  > 0, "Dither_crop16: resulting width is negative or null.")
	Assert (height > 0, "Dither_crop16: resulting height is negative or null.")

	msb = Crop (left, top,     width, height, align)
	lsb = Crop (left, top + h, width, height, align)

	(top == 0 && height == h)
\	?	src.Crop (left, 0, width, 0)
\	:	StackVertical (msb, lsb)
}



Function Dither_addborders16 (clip src, int "left", int "top",
\	int "right", int "bottom", int "color_yuv", int "color")
{
	left      = Default (left,            0)
	top       = Default (top,             0)
	right     = Default (right,           0)
	bottom    = Default (bottom,          0)

	Assert ((left >= 0 && top >= 0 && right >= 0 && bottom >= 0),
\		"Dither_addborders16: borders must be positive.")

	subspl_h = src.Dither_get_chroma_subspl_h ()
	subspl_v = src.Dither_get_chroma_subspl_v ()
	err_str  =   "Dither_addborders16: borders must be" + chr (10)
\		   + "multiple of the chroma subsampling."
	Assert (((left   % subspl_h) == 0), err_str)
	Assert (((top    % subspl_v) == 0), err_str)
	Assert (((right  % subspl_h) == 0), err_str)
	Assert (((bottom % subspl_v) == 0), err_str)

	vers = VersionNumber ()
	p_t  =   (src.IsRGB24 ()) ? "RGB24"
\	       : (src.IsRGB32 ()) ? "RGB32"
\	       : (src.IsYUY2 ()) ?  "YUY2"
\	       : (vers < 2.60   ) ? "YV12"
\	       :                    Dither_undef ()

	cx_yuv    = (src.IsRGB ()) ? Dither_undef () : Default (color_yuv, color)
	cx        = (src.IsRGB ()) ? Default (color, color_yuv) : Dither_undef ()
	c0_yuv    = (src.IsRGB ()) ? Dither_undef () : 0
	c0        = (src.IsRGB ()) ? 0 : Dither_undef ()

	src.Dither_get_msb ()
	(left   > 0) ? StackHorizontal (BlankClip (last, width=left, pixel_type=p_t, color_yuv=cx_yuv, color=cx), last)  : last
	(right  > 0) ? StackHorizontal (last, BlankClip (last, width=right, pixel_type=p_t, color_yuv=cx_yuv, color=cx)) : last
	(top    > 0) ? StackVertical (BlankClip (last, height=top, pixel_type=p_t, color_yuv=cx_yuv, color=cx), last)    : last
	(bottom > 0) ? StackVertical (last, BlankClip (last, height=bottom, pixel_type=p_t, color_yuv=cx_yuv, color=cx)) : last
	msb = last

	src.Dither_get_lsb ()
	(left   > 0) ? StackHorizontal (BlankClip (last, width=left, pixel_type=p_t, color_yuv=c0_yuv, color=c0), last)  : last
	(right  > 0) ? StackHorizontal (last, BlankClip (last, width=right, pixel_type=p_t, color_yuv=c0_yuv, color=c0)) : last
	(top    > 0) ? StackVertical (BlankClip (last, height=top, pixel_type=p_t, color_yuv=c0_yuv, color=c0), last)    : last
	(bottom > 0) ? StackVertical (last, BlankClip (last, height=bottom, pixel_type=p_t, color_yuv=c0_yuv, color=c0)) : last
	lsb = last

	StackVertical (msb, lsb)
}



Function Dither_merge16_8 (clip src1, clip src2, clip mask, bool "luma", int "y", int "u", int "v")
{
	mask16 = StackVertical (mask, mask)
	Dither_merge16 (src1, src2, mask16, luma=luma, y=y, u=u, v=v)
}



# src is a stacked 16-bit clip
# ?expr is a mt_lut-like expression, using x as variable in range [0 ; 65535]
Function Dither_lut16 (clip src,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	s_msb = Dither_get_msb (src)
	s_lsb = Dither_get_lsb (src)

	d_msb = Dither_lut16_msb (s_msb, s_lsb,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
	d_lsb = Dither_lut16_lsb (s_msb, s_lsb,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)

	StackVertical (d_msb, d_lsb)
}

Function Dither_lut16_msb (clip msb, clip lsb,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	expr  = Dither_lut16_expr (expr,  True)
	yexpr = Dither_lut16_expr (yexpr, True)
	uexpr = Dither_lut16_expr (uexpr, True)
	vexpr = Dither_lut16_expr (vexpr, True)

	y = (Defined (y) && y < 0) ? y / 256 : y
	u = (Defined (u) && u < 0) ? u / 256 : u
	v = (Defined (v) && v < 0) ? v / 256 : v

	mt_lutxy (msb, lsb,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}

Function Dither_lut16_lsb (clip msb, clip lsb,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	expr  = Dither_lut16_expr (expr,  False)
	yexpr = Dither_lut16_expr (yexpr, False)
	uexpr = Dither_lut16_expr (uexpr, False)
	vexpr = Dither_lut16_expr (vexpr, False)

	y = (Defined (y) && y < 0) ? y % 256 : y
	u = (Defined (u) && u < 0) ? u % 256 : u
	v = (Defined (v) && v < 0) ? v % 256 : v

	mt_lutxy (lsb, msb,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}

Function Dither_lut16_expr (string expr, bool gmsb)
{
	# We use spaces to enclose " x" because we don't want operators
	# like "max" or "exp" to have their "x" replaced...
	repstr = (gmsb) ? " x 256 * y + " : " y 256 * x + "
	expr = Dither_replace_string (" " + expr, " x", repstr)
	part = Dither_expr_part (gmsb)
	expr = expr + part

	return (expr)
}



Function Dither_lut8 (clip src,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	d_msb = Dither_lut8_msb (src,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
	d_lsb = Dither_lut8_lsb (src,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)

	StackVertical (d_msb, d_lsb)
}

Function Dither_lut8_msb (clip src,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (true)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y) && y < 0) ? y / 256 : y
	u = (Defined (u) && u < 0) ? u / 256 : u
	v = (Defined (v) && v < 0) ? v / 256 : v

	mt_lut (src, expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}

Function Dither_lut8_lsb (clip src,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (false)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y)) ? ((y==2 || y==4 || y==5) ? 0 : (y < 0) ? y % 256 : y) : y
	u = (Defined (u)) ? ((u==2 || u==4 || u==5) ? 0 : (u < 0) ? u % 256 : u) : u
	v = (Defined (v)) ? ((v==2 || v==4 || v==5) ? 0 : (v < 0) ? v % 256 : v) : v

	mt_lut (src, expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}



Function Dither_lutxy8 (clip src1, clip src2,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	d_msb = Dither_lutxy8_msb (src1, src2,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
	d_lsb = Dither_lutxy8_lsb (src1, src2,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)

	StackVertical (d_msb, d_lsb)
}

Function Dither_lutxy8_msb (clip src1, clip src2,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (true)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y) && y < 0) ? y / 256 : y
	u = (Defined (u) && u < 0) ? u / 256 : u
	v = (Defined (v) && v < 0) ? v / 256 : v

	mt_lutxy (src1, src2,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}

Function Dither_lutxy8_lsb (clip src1, clip src2,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (false)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y)) ? ((y==2 || y==4 || y==5) ? 0 : (y < 0) ? y % 256 : y) : y
	u = (Defined (u)) ? ((u==2 || u==4 || u==5) ? 0 : (u < 0) ? u % 256 : u) : u
	v = (Defined (v)) ? ((v==2 || v==4 || v==5) ? 0 : (v < 0) ? v % 256 : v) : v

	mt_lutxy (src1, src2, expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr,
\		y=y, u=u, v=v)
}



Function Dither_lutxyz8 (clip src1, clip src2, clip src3,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	d_msb = Dither_lutxyz8_msb (src1, src2, src3,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
	d_lsb = Dither_lutxyz8_lsb (src1, src2, src3,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)

	StackVertical (d_msb, d_lsb)
}

Function Dither_lutxyz8_msb (clip src1, clip src2, clip src3,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (true)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y) && y < 0) ? y / 256 : y
	u = (Defined (u) && u < 0) ? u / 256 : u
	v = (Defined (v) && v < 0) ? v / 256 : v

	mt_lutxyz (src1, src2, src3,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}

Function Dither_lutxyz8_lsb (clip src1, clip src2, clip src3,
\	string "expr", string "yexpr", string "uexpr", string "vexpr",
\	int "y", int "u", int "v")
{
	expr  = Default (expr,  "x")
	yexpr = Default (yexpr, expr)
	uexpr = Default (uexpr, expr)
	vexpr = Default (vexpr, expr)
	part  = Dither_expr_part (false)
	expr  = expr  + part
	yexpr = yexpr + part
	uexpr = uexpr + part
	vexpr = vexpr + part
	y = (Defined (y)) ? ((y==2 || y==4 || y==5) ? 0 : (y < 0) ? y % 256 : y) : y
	u = (Defined (u)) ? ((u==2 || u==4 || u==5) ? 0 : (u < 0) ? u % 256 : u) : u
	v = (Defined (v)) ? ((v==2 || v==4 || v==5) ? 0 : (v < 0) ? v % 256 : v) : v

	mt_lutxyz (src1, src2, src3,
\		expr=expr, yexpr=yexpr, uexpr=uexpr, vexpr=vexpr, y=y, u=u, v=v)
}



Function Dither_y_gamma_to_linear (clip src,
\	bool "tv_range_in", bool "tv_range_out", string "curve", int "u", int "v",
\	float "gcor", bool "sigmoid", float "thr", float "cont")
{
	src
	Dither_linear_and_gamma (false,
\		tv_range_in, tv_range_out, curve, u, v,
\		gcor, sigmoid, thr, cont
\	)
}

Function Dither_y_linear_to_gamma (clip src,
\	bool "tv_range_in", bool "tv_range_out", string "curve", int "u", int "v",
\	float "gcor", bool "sigmoid", float "thr", float "cont")
{
	src
	Dither_linear_and_gamma (true,
\		tv_range_in, tv_range_out, curve, u, v,
\		gcor, sigmoid, thr, cont
\	)
}

Function Dither_linear_and_gamma (clip src, bool l2g_flag,
\	bool "tv_range_in", bool "tv_range_out", string "curve", int "u", int "v",
\	float "gcor", bool "sigmoid", float "thr", float "cont")
{
	tv_range_in  = Default (tv_range_in,  true)
	tv_range_out = Default (tv_range_out, true)
	curve        = Default (curve,      "srgb")
	u            = Default (u,               2)
	v            = Default (v,               2)
	gcor         = Default (gcor,          1.0)
	sigmoid      = Default (sigmoid,     false)

	c_num =
\	  (curve == "srgb" ) ? 0
\	: (curve == "709"  ) ? 1
\	: (curve == "601"  ) ? 1
\	: (curve == "170"  ) ? 1
\	: (curve == "240"  ) ? 2
\	: (curve == "2020" ) ? 3
\	: (curve == "1886" ) ? 4
\	: (curve == "1886a") ? 5
\	: Assert (false, "Dither_linear_and_gamma: wrong curve value.")

	#                                  BT-709/601
	#                         sRGB     SMPTE 170M  SMPTE 240M    BT-2020    BT-1886    BT-1886a
	k0    = Select (c_num, " 0.04045", " 0.081  ", " 0.0912 ", " 0.08145", "0      ", "0.35   ")
	phi   = Select (c_num, "12.92   ", " 4.5    ", " 4.0    ", " 4.5    ", "4.5    ", "0.65709357")	# 0.35 ^ (3.0-2.6)
	gam2  = Select (c_num, " 1      ", " 1      ", " 1      ", " 1      ", "1      ", "3.0    ")
	alpha = Select (c_num, " 0.055  ", " 0.099  ", " 0.1115 ", " 0.0993 ", "0      ", "0      ")
	gamma = Select (c_num, " 2.4    ", " 2.22222", " 2.22222", " 2.22222", "2.4    ", "2.6    ")

	expr = (tv_range_in) ? "x 4096 - 56064 /" : "x 65536 /"

	# E = (E' <= k0)   ?   (E' ^ gam2) / phi   :   ((E' + alpha) / (1 + alpha)) ^ gamma
	g2l = expr
	g2l =   g2l + " " + k0 +" <= "
\	      + g2l + " " + gam2 + " ^ " + phi +" / "
\	      + g2l + " " + alpha + " + 1 " + alpha + " + / " + gamma + " ^   ?"
	g2l = (gcor != 1.0) ? g2l + " 0 >=   " + g2l + " " + String (gcor) + " ^   " + g2l + "   ?" : g2l
	g2l = (sigmoid) ? Dither_build_sigmoid_expr (g2l , true , thr, cont) : g2l

	l2g = (sigmoid) ? Dither_build_sigmoid_expr (expr, false, thr, cont) : expr
	l2g = (gcor != 1.0) ? l2g + " 0 >=   " + l2g + " " + String (gcor) + " ^   " + l2g + "   ?" : l2g
	# E' = (E <= k0 / phi)   ?   (E * phi) ^ (1 / gam2)   :   (E ^ (1 / gamma)) * (alpha + 1) - alpha
	l2g =   l2g + " " + k0 + " " + phi + " / <= "
\	      + l2g + " " + phi + " * 1 " + gam2 + " / ^ "
\	      + l2g + " 1 " + gamma + " / ^ " + alpha + " 1 + * " + alpha + " -   ?"

	expr = (l2g_flag) ? l2g : g2l
	expr = expr + ((tv_range_out) ? " 56064 * 4096 +" : " 65536 *")
	src.Dither_lut16 (expr=expr, y=3, u=u, v=v)
}



Function Dither_sigmoid_direct (clip src, float "thr", float "cont", int "u", int "v")
{
	u = Default (u, 2)
	v = Default (v, 2)

	expr = "x 65536 /"
	expr = Dither_build_sigmoid_expr (expr, false, thr, cont)
	expr = expr + " 65536 *"
	src.Dither_lut16 (yexpr=expr, y=3, u=u, v=v)
}



Function Dither_sigmoid_inverse (clip src, float "thr", float "cont", int "u", int "v")
{
	u = Default (u, 2)
	v = Default (v, 2)

	expr = "x 65536 /"
	expr = Dither_build_sigmoid_expr (expr, true , thr, cont)
	expr = expr + " 65536 *"
	src.Dither_lut16 (yexpr=expr, y=3, u=u, v=v)
}



# Sigmoidal functions:
# x0 = 1 / (1 + exp (cont *  thr     ))
# x1 = 1 / (1 + exp (cont * (thr - 1)))
# y  = (1 / (1 + exp (cont * (thr - x))) - x0) / (x1 - x0)
# x  = thr - log (1 / (y * (x1 - x0) + x0) - 1) / cont
Function Dither_build_sigmoid_expr (string in, bool inv, float "thr", float "cont")
{
	thr   = Default (thr,  0.5)
	cont  = Default (cont, 6.5)
	Assert ((cont > 0), "Dither_build_sigmoid_expr: cont must be strictly positive.")
	Assert ((thr >= 0 && thr <= 1), "Dither_build_sigmoid_expr: thr must be in the 0-1 range.")
	x0v = 1 / (1 + exp (cont *  thr     ))
	x1v = 1 / (1 + exp (cont * (thr - 1)))

	x0   = String (      x0v)
	x1m0 = String (x1v - x0v)
	cont = String (cont)
	thr  = String (thr)

	expr = (inv)
\	? thr + " 1 " + in + " " + x1m0 + " * " + x0 + " + 0.000001 max / 1 - 0.000001 max log " + cont + " / -"
\	: "1 1 " + cont + " " + thr + " " + in + " - * exp + / " + x0 + " - " + x1m0 + " /"

	return (expr)
}



Function Dither_expr_part (bool gmsb)
{
	part = (gmsb) ? "8 >>u "    : "255 &u "
	expr = " 0 65535 clip " + part

	return (expr)
}


### Internal utility functions ###############################################



Function Dither_min (val a, val b)
{
	return ((a < b) ? a : b)
}

Function Dither_max (val a, val b)
{
	return ((b < a) ? a : b)
}



Function DitherPre_internal (clip m, int bitd, string "flt", bool "stacked", float "overlap", bool "tvopt")
{
	flt      = Default (flt, "dfttest (sigma=16.0 * "+String (Pow (4, bitd))+")")
	stacked  = Default (stacked, true)
	tvopt    = Default (tvopt,  false)
	overlap  = Default (overlap, 0.5)

	n_base   = Int (Round (Pow (2, bitd)))

	n_add_mi = (tvopt) ? 0 : 1
	n_add    = Dither_max (Int (Round (n_base * 2 * overlap)) - 1, n_add_mi)
	Assert (n_add < n_base, "DitherXPre: overlap must be <= 0.5")

	n        = n_base + n_add

	DitherPre_internal_rec (m, m, flt, 0, bitd, tvopt, n)

	msb = SelectEven ()
	lsb = SelectOdd ()
	(stacked) ? StackVertical (msb, lsb) : last
}

Function DitherPre_internal_rec (clip msblsbi, clip m, string flt, int k, int bitd, bool tvopt, int n)
{
	msblsbi
	(k == 0) ? DitherPre_internal_first (flt, bitd, tvopt)
\	         : DitherPre_internal_next (m, flt, k, bitd, tvopt, n)

	(k < n - 1) ? DitherPre_internal_rec (m, flt, k + 1, bitd, tvopt, n) : last
}

Function DitherPre_internal_first (clip m, string flt, int bitd, bool tvopt)
{
	p     = Int (Round (Pow (2, bitd)))
	bitds = String (bitd)
	ds    = String (8 - bitd)
	of    = (tvopt) ?  16 :   0
	rbs   = String (of * p)

	m0 = m.mt_lut (expr="x "+bitds+" <<u "+rbs+" -", y=3, u=3, v=3)
	m0 = Eval ("m0." + flt)
	msb = m0.mt_lut (expr="x "+rbs+" + "+bitds+" >>u", y=3, u=3, v=3)
	lsb = m0.mt_lut (expr="x "+rbs+" + "+ds+" <<u 255 &u", y=3, u=3, v=3)
	Interleave (msb, lsb)
}

Function DitherPre_internal_next (clip msblsbi, clip m, string flt, int k, int bitd, bool tvopt, int n)
{
	msb   = msblsbi.SelectEven ()
	lsb   = msblsbi.SelectOdd ()

	p     = Int (Round (Pow (2, bitd)))
	pms   = String (p * 256 - 1)
	of    = (tvopt) ?  16 :   0
	sz    = (tvopt) ? 224 : 256

	bitds = String (bitd)
	ds    = String (8 - bitd)
	rb    = (of * p) + Ceil ((sz * p - 256.0) * k / (n - 1))	# Range begin, in N-bit scale
	rbs   = String (rb)
	ovlap = 256.0 - (sz * p - 256.0) / (n - 1)	# Overlap, in N-bit scale
	thr   = Round (ovlap / 2)			# Relative to rb
	thrs  = String (thr)

	mk = m.mt_lut (expr="x "+bitds+" <<u "+rbs+" -", y=3, u=3, v=3)
	mk = Eval ("mk." + flt)

	msb = mt_lutxy (
\		mk, msb,
\		expr="x "+thrs+" < y x "+rbs+" + "+pms+" min "+bitds+" >>u ?",
\		y=3, u=3, v=3
\	)
	lsb = mt_lutxy (
\		mk, lsb,
\		expr="x "+thrs+" < y x "+rbs+" + "+pms+" min "+ds+" <<u 255 &u ?",
\		y=3, u=3, v=3
\	)

	Interleave (msb, lsb)
}



Function Dither_replace_string (string str, string fstr, string rstr)
{
	strl  = LCase (str)
	fstrl = LCase (fstr)
	lenf  = StrLen (fstr)

	pos = FindStr (strl, fstrl)
	pos2 = (pos <= 0) ? 1 : pos
	rep  = (pos <= 0) ? "" : LeftStr (str, pos-1) + rstr + Dither_replace_string (MidStr (str, pos + lenf), fstr, rstr)

	return ((pos <= 0) ? str : rep)
}



Function Dither_undef () {}



Function Dither_test_single (clip c16,
\	int mag, int amp, int luma, String ditherfunc, String txt)
{
	Eval (ditherfunc)
	mt_lut (expr="x 128 - " + String (amp) + " * 128 +", y=3, u=3, v=3)
	mt_lut (expr="x " + String (luma - 128) + " +", y=3, u=2, v=2)
	PointResize (Width () * mag, Height () * mag)
	Subtitle (txt, align=2)
}



Function Dither_switch_planes8 (clip c_before, clip c_after, int "y", int "u", int "v")
{
	y = (! Defined (y) || y == 3) ? 4 : y
	u = (! Defined (u) || u == 3) ? 4 : u
	v = (! Defined (v) || v == 3) ? 4 : v

	# Just used for plane switching & filling, not for actual work
	  (y == 2 && u == 2 && v == 2) ? c_before
\	: (y == 4 && u == 4 && v == 4) ? c_after
\	: mt_makediff (c_before, c_after, y=y, u=u, v=v)
}

Function Dither_switch_planes16 (clip c_before, clip c_after, int "y", int "u", int "v")
{
	y = (! Defined (y) || y == 3) ? 4 : y
	u = (! Defined (u) || u == 3) ? 4 : u
	v = (! Defined (v) || v == 3) ? 4 : v

	# Just used for plane switching & filling, not for actual work
	  (y == 2 && u == 2 && v == 2) ? c_before
\	: (y == 4 && u == 4 && v == 4) ? c_after
\	: Dither_add16 (c_before, c_after, y=y, u=u, v=v)
}



Function Dither_gf3_smooth (clip src_16, clip src_8, clip ref_16, int smode, int radius, float thr, float elast, bool lsb_in, float wmin, int subspl, int yp, int up, int vp)
{
	src_16
	  (smode == 0) ? Dither_gf3_smoothgrad_multistage (ref_16, radius, thr, elast, yp, up, vp)
\	: (smode == 1) ? Dither_gf3_dfttest (src_8, ref_16, radius, thr, elast, lsb_in, yp, up, vp)
\	: (smode == 2) ? Dither_gf3_bilateral_multistage (ref_16, radius, thr, elast, wmin, subspl, yp, up, vp)
\	: (smode == 3) ? Dither_gf3_smoothgrad_multistage_3 (radius, thr, elast, yp, up, vp)
\	:                Assert (false, "GradFun3: wrong smode value.") 
}



# Valid values for y, u and v: 1 and 3
Function Dither_gf3_smoothgrad_multistage (clip src, clip ref,
\	int radius, float thr, float elast, int y, int u, int v)
{
	Assert (radius <=  68, "GradFun3: max "+chr(34)+"radius" +chr(34)+" value is 68 when smode = 0.")

	ela_2 = Dither_max (elast * 0.83, 1)
	ela_3 = Dither_max (elast * 0.67, 1)
	r2       = radius * 2 / 3
	r3       = radius * 3 / 3
	r4       = radius * 4 / 3

	src

	(r2 >= 1) ? SmoothGrad (radius=r2, thr=thr,      elast=elast, ref=ref, y=y, u=u, v=v) : last
	(r3 >= 1) ? SmoothGrad (radius=r3, thr=thr*0.7,  elast=ela_2, ref=ref, y=y, u=u, v=v) : last
	(r4 >= 1) ? SmoothGrad (radius=r4, thr=thr*0.46, elast=ela_3, ref=ref, y=y, u=u, v=v) : last
}



# Valid values for y, u and v: 1 and 3
Function Dither_gf3_smoothgrad_multistage_3 (clip src,
\	int radius, float thr, float elast,
\	int y, int u, int v)
{
	src

	ref = SmoothGrad (radius=radius/3, thr=thr*0.8, elast=elast)

	Dither_box_filter16 (radius=radius, y=y, u=u, v=v)
	Dither_box_filter16 (radius=radius, y=y, u=u, v=v)

	Dither_limit_dif16 (last, src, thr=thr*0.6, elast=elast, ref=ref, y=y, u=u, v=v)
}



# Valid values for y, u and v: 1 and 3
Function Dither_gf3_dfttest (clip src, clip src_8, clip ref,
\	int radius, float thr, float elast, bool lsb_in,
\	int y, int u, int v)
{
	Assert (radius <= 128, "GradFun3: max "+chr(34)+"radius" +chr(34)+" value is 128 when smode = 1.")

	hrad = Dither_max (radius * 3 / 4, 1)

	(lsb_in) ? src : src_8

	dfttest (
\		sigma=hrad*thr*thr*32, tbsize=1,
\		sbsize=hrad*4, sosize=hrad*3,
\		lsb=true, lsb_in=lsb_in,
\		Y=(y==3), U=(u==3), V=(v==3)
\	)

	Dither_limit_dif16 (last, ref, thr=thr, elast=elast, y=y, u=u, v=v)
}



# Valid values for y, u and v: 1 and 3
Function Dither_gf3_bilateral_multistage (clip src, clip ref,
\	int radius, float thr, float elast, float wmin, int subspl,
\	int y, int u, int v)
{
	thr_1    = Dither_max (thr * 4.5, 1.25)
	thr_2    = Dither_max (thr * 9,   5.0)
	subspl_2 = (subspl == 1) ? 1 : subspl / 2
	r4       = Dither_max (radius * 4 / 3, 4)
	r2       = Dither_max (radius * 2 / 3, 3)
	r1       = Dither_max (radius * 1 / 3, 2)

	src
	(r4 >= 2) ? Dither_bilateral16 (radius=r4, thr=thr_1, flat=0.75, wmin=wmin, ref=ref, subspl=subspl  , y=y, u=u, v=v) : last
	(r2 >= 2) ? Dither_bilateral16 (radius=r2, thr=thr_2, flat=0.25, wmin=wmin, ref=ref, subspl=subspl_2, y=y, u=u, v=v) : last
	(r1 >= 2) ? Dither_bilateral16 (radius=r1, thr=thr_2, flat=0.50, wmin=wmin, ref=ref, subspl=subspl_2, y=y, u=u, v=v) : last

	Dither_limit_dif16 (last, src, ref=ref, thr=thr, elast=elast, y=y, u=u, v=v)
}



Function Dither_build_gf3_range_mask (clip src, int radius)
{
	src
	ma  = (radius >  1) ? mt_expand_multi (sw=radius, sh=radius, mode="ellipse") : last
	mi  = (radius >  1) ? mt_inpand_multi (sw=radius, sh=radius, mode="ellipse") : last

	(radius >  1) ? mt_lutxy (ma, mi, "x y -")
\	              : mt_edge (mode="min/max", thY1=0, thY2=255)
}



# pass if x >= thr_hi
# cut  if x <= thr_lo
Function Dither_make_expr_gate (float thr_lo, float thr_hi)
{
	Assert (thr_lo <= thr_hi, "Dither_make_expr_gate: thr_lo must be less or equal to thr_hi")

	# 255 * (x - tl) / (th - tl)
	tl = String (thr_lo - 0.0001)
	th = String (thr_hi + 0.0001)

	return ("x "+tl+" - "+th+" "+tl+" - / 0 1 clip 255 *")
}



Function Dither_convert_yuv_to_rgb_resize_frames (clip src, int w, int h,
\	float cs_h, float cs_v, string chromak, float "fh", float "fv",
\	int "taps", float "a1", float "a2", float "a3", bool "noring")
{
	src
	Dither_resize16nr (w, h, src_left=cs_h, src_top=cs_v, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\		u=1, v=1, noring=noring)
}



Function Dither_convert_yuv_to_rgb_resize_fields (clip src, int w, int h,
\	float cs_h, float cs_v_t, float cs_v_b, string chromak, float "fh",
\	float "fv", int "taps", float "a1", float "a2", float "a3", bool "noring")
{
	src
	p = GetParity ()
	ofs = (p) ? 0 : 1
	t = SelectEvery (2,     ofs)
	b = SelectEvery (2, 1 - ofs)
	t = t.Dither_resize16nr (w, h, src_left=cs_h, src_top=cs_v_t, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\		u=1, v=1, noring=noring)
	b = b.Dither_resize16nr (w, h, src_left=cs_h, src_top=cs_v_b, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\		u=1, v=1, noring=noring)
	(p) ? Interleave (t, b).AssumeFieldBased ().AssumeTFF ()
\	    : Interleave (b, t).AssumeFieldBased ().AssumeBFF ()
	Weave ()
}



Function Dither_convert_yuv_to_rgb_conv_to_rgb32 (clip r, clip g, clip b,
\	bool interlaced, string pixel_type, bool parity, int "mode",
\	float "ampn", float "ampo", bool "staticnoise", bool "slice")
{
	Interleave (r, g, b)
	DitherPost (
\		mode=mode, ampo=ampo, ampn=ampn, interlaced=interlaced,
\		staticnoise=staticnoise, slice=slice, u=1, v=1
\	)
	MergeRGB (
\		SelectEvery (3, 0),
\		SelectEvery (3, 1),
\		SelectEvery (3, 2),
\		pixel_type=pixel_type
\	)
	AssumeFrameBased ()
	(! interlaced) ? last : (parity) ? AssumeTFF () : AssumeBFF ()
}



Function Dither_convert_rgb_to_yuv_resize_frames (clip src, int w, int h,
\	float cs_h, float cs_v, string chromak, float "fh", float "fv",
\	int "taps", float "a1", float "a2", float "a3", bool "noring")
{
	src
	Dither_resize16nr (w, h, src_left=cs_h, src_top=cs_v, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\		u=1, v=1, noring=noring)
}



Function Dither_convert_rgb_to_yuv_resize_fields (clip src, int w, int h,
\	float cs_h, float cs_v_t, float cs_v_b, string chromak,
\	float "fh", float "fv", int "taps", float "a1", float "a2", float "a3",
\	bool "noring")
{
	src
	cssh = Dither_get_chroma_subspl_h ()
	cssv = Dither_get_chroma_subspl_v ()
	p    = GetParity ()
	ofs  = (p) ? 0 : 1
	SeparateFields ()
	t    = SelectEvery (2,     ofs)
	b    = SelectEvery (2, 1 - ofs)

	w2   = Int (w + cssh - 1) / cssh * cssh
	h2   = Int (h + cssh - 1) / cssv * cssv
	sz_w = Float (t.Width ()     ) * w2 / w
	sz_h = Float (t.Height () / 2) * h2 / h

	t = t.Dither_resize16nr (w2, h2, src_left=cs_h, src_top=cs_v_t,
\		src_width=sz_w, src_height=sz_h, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\	        u=1, v=1, noring=noring)
	b = b.Dither_resize16nr (w2, h2, src_left=cs_h, src_top=cs_v_b,
\		src_width=sz_w, src_height=sz_h, kernel=chromak,
\		fh=fh, fv=fv, taps=taps, a1=a1, a2=a2, a3=a3, center=false,
\		u=1, v=1, noring=noring)
	(p) ? Interleave (t, b).AssumeFieldBased ().AssumeTFF ()
\	    : Interleave (b, t).AssumeFieldBased ().AssumeBFF ()
	Weave ()
}



Function Dither_matrix_to_index (string matrix)
{
	return (
\	  (matrix == "601")   ?  0
\	: (matrix == "709")   ?  1
\	: (matrix == "240")   ?  2
\	: (matrix == "FCC")   ?  3
\	: (matrix == "YCgCo") ?  4
\	: (matrix == "2020")  ?  5
\	:                       -1)
}



# http://www.mir.com/DMG/chroma.html
# Relative to the frame origin in luma scale.
Function Dither_get_chroma_placement_h (clip src, string cplace, bool pr)
{
	wl = src.Width ()
	wc = src.Dither_isy8 () ? wl : src.UToY ().Width ()
	subspl = wl / wc

	pos = (cplace == "MPEG2" || cplace == "DV") ? 0 : 0.5
	pos = (pr) ? 0 : pos
	
	return (pos)
}



# When interlaced == true, chroma placement is relative to the frame origin,
# not the field origin
Function Dither_get_chroma_placement_v (clip src,
\	string cplace, bool interlaced, bool parity, bool u, bool pr)
{
	hl = src.Height ()
	hc = src.Dither_isy8 () ? hl : src.UToY ().Height ()
	subspl = hl / hc

	# No vertical subsampling
	pos_ss1      = (! interlaced) ? 0.0 : (parity) ? 0.0 : 1.0

	# 2x vertical subsampling
	pos_ss2_mpeg = (! interlaced) ? 0.5 : (parity) ? 0.5 : 2.5
	pos_ss2_dv_u = (! interlaced) ? 1.0 : (parity) ? 2.0 : 3.0
	pos_ss2_dv_v = (! interlaced) ? 0.0 : (parity) ? 0.0 : 1.0
	pos_ss2_dv   = (u) ? pos_ss2_dv_u : pos_ss2_dv_v
	pos_ss2      = (cplace == "DV") ? pos_ss2_dv : pos_ss2_mpeg

	pos          = (subspl == 2) ? pos_ss2 : pos_ss1
	pos          = (pr) ? 0 : pos

	return (pos)
}



Function Dither_isy8 (clip src)
{
	vers = VersionNumber ()

	res = (     vers >= 2.6
\	       &&   src.IsPlanar ()
\	       && ! src.IsYV12 ()
\	       && ! src.IsYV16 ()
\	       && ! src.IsYV24 ())

	try
	{
		src.UtoY ()
		res = false
	}
	catch (e)
	{
		# Keeps the previous result
	}

	return (res)
}



Function Dither_isyv411 (clip src)
{
	vers = VersionNumber ()

	res = (     vers >= 2.6
\	       &&   src.IsPlanar ()
\	       && ! src.IsYV12 ()
\	       && ! src.IsYV16 ()
\	       && ! src.IsYV24 ())

	try
	{
		src.UtoY ()
		# Keeps the previous result
	}
	catch (e)
	{
		res = false
	}

	return (res)
}



Function Dither_get_chroma_subspl_h (src)
{
	vers = VersionNumber ()
	s =   (src.IsYUY2 ())      ? 2
\	    : (! src.IsPlanar ())  ? 1
\	    : (vers < 2.6)         ? 2	/* Assuming YV12 */
\	    : (src.IsYV12 ())      ? 2
\	    : (src.IsYV16 ())      ? 2
\	    : (src.IsYV24 ())      ? 1
\	    : (src.Dither_isy8 ()) ? 1
\	    :                        4

	return (s)
}



Function Dither_get_chroma_subspl_v (src)
{
	vers = VersionNumber ()
	s =   (! src.IsPlanar ())  ? 1
\	    : (vers < 2.6)         ? 2	/* Assuming YV12 */
\	    : (src.IsYV12 ())      ? 2
\	    : (src.IsYV16 ())      ? 1
\	    : (src.IsYV24 ())      ? 1
\	    : (src.Dither_isy8 ()) ? 1
\	    :                        1

	return (s)
}



Function Dither_y_to_uv (clip u, clip v, clip y)
{
	w     = y.Width ()
	h     = y.Height ()
	uvw   = u.Width ()
	uvh   = u.Height ()
	cssh  = y.Dither_get_chroma_subspl_h ()
	cssv  = y.Dither_get_chroma_subspl_v ()
	dif_h = uvw * cssh - w
	dif_v = uvh * cssv - h
	fix   = (dif_h > 0 || dif_v > 0)

	y
	(fix) ? AddBorders (0, 0, dif_h, dif_v) : last
	YToUV (u, v, last)
	(fix) ? Crop (0, 0, w, h) : last
}



Function Dither_removegrain_emul (clip src, int ml, int "mc")
{
	mc = Default (mc, ml)

	src

	  IsYV12 ()      ? RemoveGrain (ml, mc)
\	: Dither_isy8 () ? ConvertToYV12 ().RemoveGrain (ml, -1).ConvertToY8 ()
\	:                  YToUV (
\		UToY ().ConvertToYV12 ().RemoveGrain (mc, -1),
\		VToY ().ConvertToYV12 ().RemoveGrain (mc, -1),
\		        ConvertToYV12 ().RemoveGrain (ml, -1)
\	                   )
}



# soft = noise regen period, in frames (low-pass cutoff period)
# soft >= 100: static noise.
Function Dither_gen_grain_internal (clip template,
\	float "soft", int "maxts", float "var", float "uvar", int "seed")
{
	soft    = Default (soft, 1.0)
	maxts   = Default (maxts,  5)
	var     = Default (var,  1.0)
	uvar    = Default (uvar, 0.0)
	Assert (soft  >= 1.0)
	Assert (maxts >= 0)

	thr_aft  = 10.0
	constant = (soft >= 100)

	ts_rad   = (constant)   ? 0 : Ceil ((soft - 1) * 0.5)
	ts_rad   = (ts_rad > maxts) ? maxts : ts_rad

	rate_mul = (constant)   ? 1 : soft / Float (ts_rad * 2 + 1)
	decim    = (rate_mul < 2.0/3.0) ? 2.0 : 1.0

	amp_comp = pow (ts_rad * 2 + 1, 0.7)

	pa_l_max = 128.0 /  var
	pa_c_max = 128.0 / uvar
	a_l_pre  = (amp_comp > pa_l_max) ? pa_l_max : amp_comp
	a_c_pre  = (amp_comp > pa_c_max) ? pa_c_max : amp_comp
	a_l_post = amp_comp / a_l_pre
	a_c_post = amp_comp / a_c_pre
	a_l_str  = "x 128 - "+String(a_l_post)+" * 128 +"
	a_c_str  = "x 128 - "+String(a_c_post)+" * 128 +"

	p_l      = (a_l_post == 1.0 &&  var <= 0.01) ? 2 : 3
	p_c      = (a_c_post == 1.0 && uvar <= 0.01) ? 2 : 3
	amp_bef  = (ts_rad > 0 && (p_l == 3 || p_c == 3) && var >  thr_aft)
	amp_aft  = (ts_rad > 0 && (p_l == 3 || p_c == 3) && var <= thr_aft)

	len      = template.FrameCount ()
	base_len = Ceil (len / rate_mul) + ts_rad * 2
	template.BlankClip (color_yuv=$808080, length=base_len)

	AddGrainC (var=var*a_l_pre, uvar=uvar*a_c_pre, constant=constant, seed=seed)

	(! constant && ts_rad > 0) ? TemporalSoften (ts_rad, 255, 255, mode=2)                 : last
	(! constant && ts_rad > 0) ? Trim (ts_rad, 0)                                          : last
	(! constant &&    amp_bef) ? mt_lut (yexpr=a_l_str, expr=a_c_str, y=p_l, u=p_c, v=p_c) : last

	(! constant &&  decim > 1) ? SelectEven ()                                             : last
	(! constant              ) ? ConvertFPS (FrameRate () * rate_mul * decim)              : last
	(! constant &&    amp_aft) ? mt_lut (yexpr=a_l_str, expr=a_c_str, y=p_l, u=p_c, v=p_c) : last

	AssumeFPS (template.FrameRateNumerator (), template.FrameRateDenominator ())
	Assert (FrameCount () >= len)
	Trim (0, -len)

#	Subtitle ("ts_rad="+String(ts_rad)+" rate_mul="+String(rate_mul)+" decim="+String(decim)
#\	+ "\nvar="+String(var)+" uvar="+String(uvar)+" amp_comp="+String(amp_comp)
#\	+ "\na_l_pre="+String(a_l_pre)+" a_l_post="+String(a_l_post)
#\	+ "\na_c_pre="+String(a_c_pre)+" a_c_post="+String(a_c_post)
#\	+ "\namp_bef="+String(amp_bef)+" amp_aft="+String(amp_aft), lsp=1)
}



Function Dither_quantize_out_16 (clip b, clip h, int n, int hr, string s1, string ofs, int "y", int "u", int "v")
{
	s2 = String (16 - n)
	ofs2 = String (Round (Pow (2, 15 - hr - n)))
	b_msb = b.mt_lut ("x "+s1+" >>u 255 &u "+ofs2+" - 128 +", y=y, u=u, v=v)
	b_lsb = b.mt_lut ("x "+ofs+" - "+s2+" <<u 255 &u", y=y, u=u, v=v)

	c = mt_adddiff (h, b_msb, y=y, u=u, v=v)

	Interleave (c, b_lsb)
}



Function Dither_quantize_out_n (clip b, clip h, int n, int hr, string s1, string ofs, string m, int "y", int "u", int "v")
{
	ofs2 = (n < 14) ? String (Round (Pow (2, 7 - (n-8) - hr))) : "0"
	b2 = b.mt_lut ("x "+s1+" >>u "+ofs2+" - 128 + 255 "+m+" - &u", y=y, u=u, v=v)
	h2 = mt_adddiff (h, b2, y=y, u=u, v=v)
	msb = h2.mt_lut ("x "+String(16-n)+" >>u", y=y, u=u, v=v)

	m2 = String (Round (Pow (2, 8 - hr)) - 1)
	lsb = mt_lutxy (h2, b, "x "+s1+" <<u 255 "+m2+" - &u y "+ofs+" - "+m2+" &u +", y=y, u=u, v=v)

	Interleave (msb, lsb)
}



Function Dither_kill_rounding_error (float x, float "e")
{
	e   = Default (e, 0.00001)
	tol = e * Dither_max (Abs (x), 1.0)
	rx  = Round (x)
	dif = Abs (x - rx)
	x   = (dif < tol) ? rx : x

	Return (x)
}

