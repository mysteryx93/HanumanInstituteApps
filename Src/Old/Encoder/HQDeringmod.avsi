#################################################################################################
###### HQDering mod v1.8         ######      by mawen1250      ######      2014.03.22      ######
#################################################################################################
###### Requirements: masktools v2.0a48, dither v1.25.1, dfttest v1.9.4,                    ######
######               RgTools v0.92.1, MedianBlur2 v0.94, SmoothAdjust v3.00                ######
#################################################################################################
###### applies deringing by using a smart smoother near edges (where ringing occurs) only  ######
#################################################################################################
###### "lsb_in"/"lsb" defines if input/output clip is stacked-16bit, default is False      ######
###### "output" defines colorspace of output clip, for RGB output "lsb" is ignored         ######
#################################################################################################
###### "nrmode" defines kernel of deringing: 1-3 for MinBlur radius, 0 use dfttest instead ######
###### by default, 2 is set for HD(Width>=1280 or Height>=720) and 1 is set for non-HD     ######
#################################################################################################


Function HQDeringmod(clip input, clip "smoothed", clip "ringmask",
\                    int "mrad", int "msmooth", bool "incedge", int "mthr", int "minp",
\                    int "nrmode", int "nrmodec", float "sigma", float "sigma2", int "sbsize", int "sosize",
\                    int "sharp", int "drrep", float "thr", float "elast", float "darkthr",
\                    int "Y", int "U", int "V", bool "lsb_in", bool "lsb", bool "lsb_out", bool "tv_range", int "dither", bool "show")
{
    ###############################################################################################################################
    ###############################################################################################################################
    # Parameters for input/output
    
    Y        = Default(Y,      3       )
    U        = Default(U,      2       )
    V        = Default(V,      2       )
    
    lsb_in   = Default(lsb_in, False   )    # input  clip is 16-bit stacked or not
    lsb      = Default(lsb,    False   )    # smooth clip is 16-bit stacked or not, processing precision is 16-bit or not
    lsb_out  = Default(lsb_out,lsb     )    # output clip is 16-bit stacked or not
    tv_range = Default(tv_range,True   )    # input  clip is TV-range or PC-range
    dither   = tv_range ? Default(dither, 6) : Default(dither, 50)    # dither mode for 16-bit to 8-bit conversion
    show     = Default(show,   False   )    # whether to output mask clip instead of filtered clip
    
    ###############################################################################################################################
    
    Y        = min(Y, 3)
    U        = min(U, 3)
    V        = min(V, 3)
    Yt       = Y == 3
    Ut       = U == 3
    Vt       = V == 3
    Y31      = Yt ? 3 : 1
    U31      = Ut ? 3 : 1
    V31      = Vt ? 3 : 1
    Y4       = Yt ? 4  : -1
    Y11      = Yt ? 11 : -1
    Y20      = Yt ? 20 : -1
    U4       = Ut ? 4  : -1
    U11      = Ut ? 11 : -1
    U20      = Ut ? 20 : -1
    V4       = Vt ? 4  : -1
    V11      = Vt ? 11 : -1
    V20      = Vt ? 20 : -1
    
    ###############################################################################################################################
    ###############################################################################################################################
    
    sw       = input.Width ()
    sh       = input.Height()
    sh       = lsb_in ? sh/2 : sh
    HD       = (sw >= 1280 || sh >= 720) ? True : False
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Parameters for ring mask
    
    mthr     = Default(mthr,   60      )    # threshold of prewitt edge mask, lower  value means more aggressive processing
                                            # but for strong ringing, lower value will treat some ringing as edge, which protects this ringing from being processed
    minp     = Default(minp,   1       )    # inpanding of prewitt edge mask, higher value means more aggressive processing
    mrad     = Default(mrad,   1       )    # expanding of edge mask, higher value means more aggressive processing
    msmooth  = Default(msmooth,1       )    # inflate   of edge mask, smooth boundaries of mask
    incedge  = Default(incedge,False   )    # whether to include edge in ring mask, by default ring mask only include area near edges
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Parameters for deringing kernel
    
    nrmode   = Default(nrmode, HD?2:1  )    # kernel of deringing - 0: dfttest, 1: MinBlur(radius=1), 2: MinBlur(radius=2), 3: MinBlur(radius=3)
    nrmodec  = Default(nrmodec,nrmode  )    # kernel of deringing for chroma
    sigma    = Default(sigma,  128.0   )    # dfttest: sigma for medium frequecies
    sigma2   = Default(sigma2, sigma/16.0)  # dfttest: sigma for low&high frequecies
    sbsize   = Default(sbsize, HD?8:6  )    # dfttest: length of the sides of the spatial window
    sosize   = Default(sosize, HD?6:4  )    # dfttest: spatial overlap amount
    sigma    = string (sigma           )
    sigma2   = string (sigma2          )
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Parameters for post-processing
    
    sharp    = Default(sharp,  1       )    # whether to use contra-sharpening to resharp deringed clip, 1-3 represents radius, 0 means no sharpening
    drrep    = Default(drrep,  nrmode>0?lsb?13:24:0)   # use repair for details retention, recommended values are 24/23(only for lsb=False)/13/12/1
    thr      = Default(thr,    12.0    )    # the same meaning with "thr"   in Dither_limit_dif16, valid value range is [0, 128.0]/[0, 255.0] for lsb=True/False
    elast    = Default(elast,  2.0     )    # the same meaning with "elast" in Dither_limit_dif16, valid value range is [1, inf  )
                                            # PDiff: pixel value diff between processed clip and input clip
                                            # ODiff: pixel value diff between output    clip and input clip
                                            # PDiff, thr and elast is used to calculate ODiff:
                                            # ODiff = PDiff when [PDiff <= thr]
                                            # ODiff gradually smooths from thr to 0 when [thr <= PDiff <= thr * elast]
                                            # for elast>2.0, ODiff reaches maximum when [PDiff == thr * elast / 2]
                                            # ODiff = 0     when [PDiff >= thr * elast]
                                            #
                                            # larger "thr"   will result in more pixels being taken from processed clip
                                            # larger "thr"   will result in less pixels being taken from input     clip
                                            # larger "elast" will result in more pixels being blended from processed&input clip, for smoother merging
    darkthr  = Default(darkthr,thr/4.  )    # threshold for darker area near edges, by default equals to thr/4
                                            # set it lower if you think de-ringing destroys too much lines, etc.
                                            # when "darkthr" is not equal to "thr", "thr" limits darkening while "darkthr" limits brightening.
                                            # 
                                            # example:
                                            # "thr=0,   darkthr=0  " - no limiting
                                            # "thr=255, darkthr=255" - no limiting
                                            # "thr=8,   darkthr=2  " - limit darkening with   8             , brightening is limited to 2
                                            # "thr=8,   darkthr=0  " - limit darkening with   8             , brightening is limited to 0(stays unchanged)
                                            # "thr=255, darkthr=0  " - limit darkening with 255(no limiting), brightening is limited to 0(stays unchanged)
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Pre-Process: Bit Depth Conversion
    
    input8   = lsb_in ? input.HQDeringmod_Down8(tv_range, True, True, True, dither) : input
    input16  = lsb_in ? input                                                       : input.HQDeringmod_U16  (tv_range, True, True, True)
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Kernel: Smoothing
    
    smoothc  = nrmodec <= 0 ? input.dfttest(Y=False, U=Ut, V=Vt,
    \                                       sbsize=sbsize, sosize=sosize, tbsize=1, lsb_in=lsb_in, lsb=lsb,
    \                                       sstring="0.0:"+sigma2+" 0.05:"+sigma+" 0.5:"+sigma+" 0.75:"+sigma2+" 1.0:0.0")
    \                       : input.HQDeringmod_MinBlur(nrmodec, lsb_in=lsb_in, lsb=lsb, tv_range=tv_range, dither=dither, Y=1, U=U, V=V)
    smoothy  = nrmode  <= 0 ? input.dfttest(Y=Yt, U=(nrmode==nrmodec||!(Ut||Vt))?Ut:False, V=(nrmode==nrmodec||!(Ut||Vt))?Vt:False,
    \                                        sbsize=sbsize, sosize=sosize, tbsize=1, lsb_in=lsb_in, lsb=lsb,
    \                                      sstring="0.0:"+sigma2+" 0.05:"+sigma+" 0.5:"+sigma+" 0.75:"+sigma2+" 1.0:0.0")
    \                       : input.HQDeringmod_MinBlur(nrmode, lsb_in=lsb_in, lsb=lsb, tv_range=tv_range, dither=dither,
    \                                                   Y=Y, U=(nrmode==nrmodec||!(Ut||Vt))?U:1, V=(nrmode==nrmodec||!(Ut||Vt))?V:1)
    
    smoothed = Defined(smoothed) ? smoothed
    \                            : nrmode == nrmodec || !(Ut || Vt) ? smoothy : smoothy.MergeChroma(smoothc)
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Post-Process: Contra-Sharpening
    
    pre           = lsb ? smoothed .Dither_removegrain16(Y4, U4, V4)
    \                   : smoothed .         RemoveGrain(Y4, U4, V4)
    method        = sharp <= 1 ? lsb ? pre.Dither_removegrain16(Y11, U11, V11)
    \                                : pre.         RemoveGrain(Y11, U11, V11)
    \             : sharp == 2 ? lsb ? pre.Dither_removegrain16(Y11, U11, V11).Dither_removegrain16(Y20, U20, V20)
    \                                : pre.         RemoveGrain(Y11, U11, V11).         RemoveGrain(Y20, U20, V20)
    \             :              lsb ? pre.Dither_removegrain16(Y11, U11, V11).Dither_removegrain16(Y20, U20, V20).Dither_removegrain16(Y20, U20, V20)
    \                                : pre.         RemoveGrain(Y11, U11, V11).         RemoveGrain(Y20, U20, V20).         RemoveGrain(Y20, U20, V20)
    sharpdiff     = lsb ? pre      .Dither_sub16(method,   y=Y31, u=U31, v=V31, dif=True)
    \                   : pre      . mt_makediff(method,   Y=Y31, U=U31, V=V31)
    allD          = lsb ? input16  .Dither_sub16(smoothed, y=Y31, u=U31, v=V31, dif=True)
    \                   : input8   . mt_makediff(smoothed, Y=Y31, U=U31, V=V31)
    ssDD          = lsb ? sharpdiff.Dither_repair16(allD, Yt ? 1 : -1, Ut ? 1 : -1, Vt ? 1 : -1)
    \                   : sharpdiff.         Repair(allD, Yt ? 1 : -1, Ut ? 1 : -1, Vt ? 1 : -1)
    ssDD          = lsb ? ssDD     .HQDeringmod_limitdiff16(sharpdiff, Y=Y31, U=U31, V=V31)
    \                   : ssDD     .mt_lutxy(sharpdiff, "x 128 - abs y 128 - abs <= x y ?", Y=Y31, U=U31, V=V31)
    sclp          = sharp <= 0 ? smoothed
    \                          : lsb ? smoothed .Dither_add16(ssDD,     y=Y31, u=U31, v=V31, dif=True)
    \                                : smoothed .  mt_adddiff(ssDD,     Y=Y31, U=U31, V=V31)
    
    ###############################################################################################################################
    # Post-Process: Repairing
    
    repclp        = drrep <= 0 ? sclp
    \                          : lsb ? input16.Dither_repair16(sclp, Yt ? drrep : -1, Ut ? drrep : -1, Vt ? drrep : -1)
    \                                : input8 .         Repair(sclp, Yt ? drrep : -1, Ut ? drrep : -1, Vt ? drrep : -1)
    
    ###############################################################################################################################
    # Post-Process: Limiting
    
    limitOFF      = lsb ? (thr <= 0 && darkthr <= 0) || (thr >= 128 && darkthr >= 128)
    \                   : (thr <= 0 && darkthr <= 0) || (thr >= 255 && darkthr >= 255)
    
    limitclp      = limitOFF ? repclp
    \                        : lsb ? repclp.HQDeringmod_limit_dif16(input16, smooth=True, thr=thr, elast=elast,
    \                                                               darkthr=darkthr, Y=Y31, U=U31, V=V31)
    \                              : repclp.HQDeringmod_limit_dif8 (input8 , smooth=True, thr=thr, elast=elast,
    \                                                               darkthr=darkthr, Y=Y31, U=U31, V=V31)
    
    ###############################################################################################################################
    # Post-Process: Ringing Mask Generating
    
    prewittm      = input8.mt_edge("prewitt", mthr, 255, 0, 0, V=1, U=1)
    fmask         = mt_hysteresis(prewittm.RemoveGrain(4, -1), prewittm, U=1, V=1)
    omask         = mrad > 0 ? fmask.HQDeringmod_expand(mrad) : fmask
    omask         = msmooth > 0 ? omask.HQDeringmod_inflate(msmooth) : omask
    imask         = minp > 3 ? fmask.mt_inpand(U=1, V=1).mt_inpand(U=1, V=1)
    \             : minp > 2 ? fmask.mt_inflate(U=1, V=1).mt_inpand(U=1, V=1).mt_inpand(U=1, V=1)
    \             : minp > 1 ? fmask.mt_inpand(U=1, V=1)
    \             : minp > 0 ? fmask.mt_inflate(U=1, V=1).mt_inpand(U=1, V=1)
    \             :            fmask
    ringmaskl     = Defined(ringmask) ? ringmask : incedge ? omask : omask.mt_lutxy(imask, "x 255 y - * 255 /", U=show?-128:1, V=show?-128:1)
    ringmask      = Ut || Vt ? ringmaskl.HQDeringmod_YtoYUV() : ringmaskl
    
    ###############################################################################################################################
    ###############################################################################################################################
    # Mask Merging & Output
    
    return show ? ringmask 
    \           : lsb ? lsb_out ? Dither_merge16_8(input16, limitclp                                                , ringmask, luma=False, y=Y, u=U, v=V)
    \                           : mt_merge        (input8 , limitclp.HQDeringmod_Down8(tv_range, Yt, Ut, Vt, dither), ringmask, luma=False, Y=Y, U=U, V=V)
    \                 : lsb_out ? Dither_merge16_8(input16, limitclp.HQDeringmod_U16  (tv_range, Yt, Ut, Vt)        , ringmask, luma=False, y=Y, u=U, v=V)
    \                           : mt_merge        (input8,  limitclp                                                , ringmask, luma=False, Y=Y, U=U, V=V)
}


Function HQDeringmod_MinBlur(clip clp, int "r", int "Y", int "U", int "V", bool "lsb_in", bool "lsb", bool "tv_range", int "dither")
{
    r        = Default(r,    1)
    Y        = Default(Y,    3)
    U        = Default(U,    3)
    V        = Default(V,    3)
    
    Y2       = (Y==2) ? 1  :  Y
    Y4       = (Y==3) ? 4  : -1
    Y11      = (Y==3) ? 11 : -1
    Y20      = (Y==3) ? 20 : -1
    Ym2      = (Y==3) ? 2  : -256
    Ym3      = (Y==3) ? 3  : -256
    U2       = (U==2) ? 1  :  U
    U4       = (U==3) ? 4  : -1
    U11      = (U==3) ? 11 : -1
    U20      = (U==3) ? 20 : -1
    Um2      = (U==3) ? 2  : -256
    Um3      = (U==3) ? 3  : -256
    V2       = (V==2) ? 1  :  V
    V4       = (V==3) ? 4  : -1
    V11      = (V==3) ? 11 : -1
    V20      = (V==3) ? 20 : -1
    Vm2      = (V==3) ? 2  : -256
    Vm3      = (V==3) ? 3  : -256
    
    lsb_in   = Default(lsb_in,   False )
    lsb      = Default(lsb,      False )
    tv_range = Default(tv_range, True  )
    dither   = tv_range ? Default(dither, 6) : Default(dither, 50)
    
    clp8     = lsb_in ? clp.HQDeringmod_Down8(tv_range, True, True, True, dither) : clp
    clp16    = lsb_in ? clp                                                       : clp.HQDeringmod_U16  (tv_range, True, True, True)
    
    RG11     = (r<=1) ? lsb ? clp16.Dither_removegrain16(Y11, U11, V11)
    \                       : clp8 .         RemoveGrain(Y11, U11, V11)
    \        : (r==2) ? lsb ? clp16.Dither_removegrain16(Y11, U11, V11).Dither_removegrain16(Y20, U20, V20)
    \                       : clp8 .         RemoveGrain(Y11, U11, V11).         RemoveGrain(Y20, U20, V20)
    \        :          lsb ? clp16.Dither_removegrain16(Y11, U11, V11).Dither_removegrain16(Y20, U20, V20).Dither_removegrain16(Y20, U20, V20)
    \                       : clp8 .         RemoveGrain(Y11, U11, V11).         RemoveGrain(Y20, U20, V20).         RemoveGrain(Y20, U20, V20)
    
    RG4      = (r<=1) ? lsb_in&&lsb ? clp16.Dither_removegrain16(Y4, U4, V4)
    \                               : clp8 .RemoveGrain(Y4, U4, V4)
    \        : (r==2) ? lsb_in&&lsb ? clp16.Dither_median16(2, 2, 0, y=Y2, u=U2, v=V2)
    \                               : clp8 .MedianBlur(Ym2, Um2, Vm2)
    \        :          lsb_in&&lsb ? clp16.Dither_median16(3, 3, 0, y=Y2, u=U2, v=V2)
    \                               : clp8 .MedianBlur(Ym3, Um3, Vm3)
    RG4      = !lsb_in&&lsb ? RG4  .HQDeringmod_U16  (tv_range, Y==3, U==3, V==3) : RG4
    
    RG11D    = lsb ? NOP() : mt_makediff(clp8,  RG11, Y=Y2, U=U2, V=V2)
    RG4D     = lsb ? NOP() : mt_makediff(clp8,  RG4,  Y=Y2, U=U2, V=V2)
             
    DD       = lsb ? NOP() : mt_lutxy(RG11D, RG4D, "x 128 - y 128 - * 0 < 128 x 128 - abs y 128 - abs < x y ? ?", Y=Y2, U=U2, V=V2)
    end      = lsb ? HQDeringmod_min_dif16(RG11, RG4, clp16, Y=Y, U=U, V=V)
    \              : clp8.mt_makediff(DD, Y=Y, U=U, V=V)
    
    return end
}


Function HQDeringmod_limit_dif8(clip filtered, clip original, bool "smooth", float "thr", float "elast", float "darkthr", int "Y", int "U", int "V")
{
    smooth   = Default(smooth, True    )
    thr      = Default(thr,    1.0     )
    elast    = Default(elast,  smooth ? 3.0 : 255./thr)
    darkthr  = Default(darkthr,thr     )
    Y        = Default(Y,      3       )
    U        = Default(U,      3       )
    V        = Default(V,      3       )
    
    Y        = min(Y,     4)
    U        = min(U,     4)
    V        = min(V,     4)
    
    thr      = max(min(    thr, 255.0), 0.0)
    darkthr  = max(min(darkthr, 255.0), 0.0)
	elast    = max(elast, 1.0)
    mode     = thr == 0 && darkthr == 0 ? 4 : thr == 255 && darkthr == 255 ? 2 : 3
    smooth   = elast==1 ? False : smooth
    
    diffstr  = " x y - "
    elaststr = " "+string(elast)+" "
    
    thrstr   = diffstr+" 0 > "+string(darkthr)+" "+string(thr)+" ? "
    alphastr = elaststr+" 1 <= 0 1 "+elaststr+" 1 - "+thrstr+" * / ? "
    betastr  = thrstr+elaststr+" * "
    sexpr    = smooth   ? alphastr+diffstr+" * "+betastr+diffstr+" abs - * y + "
    \                   : thrstr+diffstr+diffstr+" abs / * y + "
    expr     = diffstr+" abs "+thrstr+" <= x "+diffstr+" abs "+betastr+" >= y "+sexpr+" ? ? "
    
    thrstrc  = " "+string(thr)+" "
    alphastrc= elaststr+" 1 <= 0 1 "+elaststr+" 1 - "+thrstrc+" * / ? "
    betastrc = thrstrc+elaststr+" * "
    sexprc   = smooth   ? alphastrc+diffstr+" * "+betastrc+diffstr+" abs - * y + "
    \                   : thrstrc+diffstr+diffstr+" abs / * y + "
    exprc    = diffstr+" abs "+thrstrc+" <= x "+diffstr+" abs "+betastrc+" >= y "+sexprc+" ? ? "
    
    # diff   = filtered - original
    # alpha  = 1 / (thr * (elast - 1))
    # beta   = elast * thr
    # When smooth=True  :
    # output = diff <= thr  ? filtered : \
    #          diff >= beta ? original : \
    #                         original + alpha * diff * (beta - abs(diff))
    # When smooth=False :
    # output = diff <= thr  ? filtered : \
    #          diff >= beta ? original : \
    #                         original + thr * (diff / abs(diff))
	
    return mode == 4 ? original
    \    : mode == 2 ? filtered
    \    :             mt_lutxy(filtered, original, yExpr=expr, uExpr=exprc, vExpr=exprc, Y=Y, U=U, V=V)
}


Function HQDeringmod_limit_dif16(clip filtered, clip original, bool "smooth", float "thr", float "elast", float "darkthr", int "Y", int "U", int "V")
{
    smooth   = Default(smooth, True    )
    thr      = Default(thr,    1.0     )
    elast    = Default(elast,  smooth ? 3.0 : 128./thr)
    darkthr  = Default(darkthr,thr     )
    Y        = Default(Y,      3       )
    U        = Default(U,      3       )
    V        = Default(V,      3       )
    
    Y        = min(Y,     4)
    U        = min(U,     4)
    V        = min(V,     4)
    Yt       = Y == 3
    Ut       = U == 3
    Vt       = V == 3
    Y31      = Yt ? 3 : 1
    U31      = Ut ? 3 : 1
    V31      = Vt ? 3 : 1
    
    thr      = max(min(    thr, 128.0), 0.0)
    darkthr  = max(min(darkthr, 128.0), 0.0)
	elast    = max(elast, 1.0)
    mode     = thr == 0 && darkthr == 0 ? 4 : thr == 128 && darkthr == 128 ? 2 : 3
    smooth   = elast==1 ? False : smooth
    thr      = thr * 256.
    darkthr  = darkthr * 256.
    
    diffstr  = " x 32768 - "
    elaststr = " "+string(elast)+" "
    
    thrstr   = diffstr+" 0 > "+string(darkthr)+" "+string(thr)+" ? "
    alphastr = elaststr+" 1 <= 0 1 "+elaststr+" 1 - "+thrstr+" * / ? "
    betastr  = thrstr+elaststr+" * "
    sexpr    = smooth   ? alphastr+diffstr+" * "+betastr+diffstr+" abs - * 32768 + "
    \                   : thrstr+diffstr+diffstr" abs / * 32768 + "
    expr     = diffstr+" abs "+thrstr+" <= x "+diffstr+" abs "+betastr+" >= 32768 "+sexpr+" ? ? "
    
    thrstrc  = " "+string(thr)+" "
    alphastrc= elaststr+" 1 <= 0 1 "+elaststr+" 1 - "+thrstrc+" * / ? "
    betastrc = thrstrc+elaststr+" * "
    sexprc   = smooth   ? alphastrc+diffstr+" * "+betastrc+diffstr+" abs - * 32768 + "
    \                   : thrstrc+diffstr+diffstr" abs / * 32768 + "
    exprc    = diffstr+" abs "+thrstrc+" <= x "+diffstr+" abs "+betastrc+" >= 32768 "+sexprc+" ? ? "
    
    # diff   = filtered - original
    # alpha  = 1 / (thr * (elast - 1))
    # beta   = elast * thr
    # When smooth=True  :
    # output = diff <= thr  ? filtered : \
    #          diff >= beta ? original : \
    #                         original + alpha * diff * (beta - abs(diff))
    # When smooth=False :
    # output = diff <= thr  ? filtered : \
    #          diff >= beta ? original : \
    #                         original + thr * (diff / abs(diff))
	
    diff     = Dither_sub16(filtered, original, y=Y31, u=U31, v=V31, dif=True)
    ldiff    = Dither_lut16(diff, yexpr=expr, uexpr=exprc, vexpr=exprc, y=Y31, u=U31, v=V31)
    merged   = Dither_add16(original, ldiff   , y=Y31, u=U31, v=V31, dif=True)
    merged   = Y==2 || U==2 || V==2 || Y==4 || U==4 || V==4 ? mt_lutxyz(filtered, original, merged, Y=Y==3?5:Y, U=U==3?5:U, V=V==3?5:V) : merged
    
    return mode == 4 ? original
    \    : mode == 2 ? filtered
    \    :             merged
}


Function HQDeringmod_limitdiff16(clip diff1, clip diff2, int "Y", int "U", int "V")
{
    Y          = Default(Y,        3)
    U          = Default(U,        3)
    V          = Default(V,        3)
    
    sh         = diff1.Height()/2
    
    nulldiff   = diff1.HQDeringmod_gen_null_diff(lsb_in=True, lsb=True)
    maxdif     = Dither_max_dif16(diff1, diff2, nulldiff, y=Y==3?3:1, u=U==3?3:1, v=V==3?3:1)
    bin_stack  = mt_lutxy(diff1, maxdif, "x y == 255 0 ?", Y=Y==3?3:1, U=U==3?3:1, V=V==3?3:1)
    bin_msb    = bin_stack.Crop(0, 0, 0, sh)
    bin_lsb    = bin_stack.Crop(0, sh, 0, 0)
    bin        = mt_logic(bin_msb, bin_lsb, "min", Y=Y==3?3:1, U=U==3?3:1, V=V==3?3:1)
    
    return Dither_merge16_8(diff1, diff2, bin, luma=False, y=Y, u=U, v=V)
}


Function HQDeringmod_min_dif16(clip src1, clip src2, clip ref, int "Y", int "U", int "V")
{
    Y          = Default(Y,        3)
    U          = Default(U,        3)
    V          = Default(V,        3)
    
    sh         = ref.Height()/2
    
    maxdif     = Dither_max_dif16(src1, src2, ref, y=Y==3?3:1, u=U==3?3:1, v=V==3?3:1)
    bin_stack  = mt_lutxy(src1, maxdif, "x y == 255 0 ?", Y=Y==3?3:1, U=U==3?3:1, V=V==3?3:1)
    bin_msb    = bin_stack.Crop(0, 0, 0, sh)
    bin_lsb    = bin_stack.Crop(0, sh, 0, 0)
    bin        = mt_logic(bin_msb, bin_lsb, "min", Y=Y==3?3:1, U=U==3?3:1, V=V==3?3:1)
    
    return Dither_merge16_8(src1, src2, bin, luma=False, y=Y, u=U, v=V)
}


Function HQDeringmod_gen_null_diff(clip input, bool "lsb_in", bool "lsb")
{
    lsb_in     = Default(lsb_in, True  )
    lsb        = Default(lsb,    lsb_in)
    
    sh         = lsb_in ? input.Height()/2 : input.Height()
    output     = input.BlankClip(height=sh, color_yuv=$808080)
    output     = lsb    ? StackVertical(output, input.BlankClip(height=sh, color_yuv=$000000)) : output
    
    return output
}


Function HQDeringmod_YtoYUV(clip inputl, string "colorspace")
{
    sw          = inputl.Width()
    sh          = inputl.Height()
    wmod4       = sw/4*4 == sw ? True : False
    hmod4       = sh/4*4 == sh ? True : False
   
    icolorspace = inputl.YtoYUV_GetCSP()
    ocolorspace = Defined(colorspace) ? colorspace : icolorspace
    
    try {
        inputp = inputl.ConvertToY8()
        inputc = ocolorspace == "YV24" ? inputp
        \      : ocolorspace == "YV16" ? inputp.BilinearResize(sw/2, sh  , -0.50)
        \      :                         inputp.BilinearResize(sw/2, sh/2, -0.50)
        
        output = YtoUV(inputc, inputc, inputp)
    } catch (error_msg) {
        inputp = wmod4&&hmod4 ? inputl : inputl.PointResize(wmod4?sw:sw+2, hmod4?sh:sh+2, 0, 0, wmod4?sw:sw+2, hmod4?sh:sh+2)
        inputc = inputp.BilinearResize(wmod4?sw/2:sw/2+1, hmod4?sh/2:sh/2+1, -0.50)
        
        output = YtoUV(inputc, inputc, inputp)
        output = wmod4&&hmod4 ? output : output.Crop(0, 0, wmod4?0:-2, hmod4?0:-2)
    }
    
    return output
    
    Function YtoYUV_GetCSP(clip c)
    {
        return c.IsPlanar ? c.IsYV12 ? "YV12" :
        \                   c.IsYV16 ? "YV16" :
        \                   c.IsYV24 ? "YV24" : c.GetCSP_Y8_YV411() :
        \      c.IsYUY2   ? "YUY2"   :
        \      c.IsRGB32  ? "RGB32"  :
        \      c.IsRGB24  ? "RGB24"  : "Unknown"
        
        Function GetCSP_Y8_YV411(clip c) {
            try {
                c.UtoY
                csp = "YV411"
            } catch (error_msg) {
                csp = "Y8"
            }
            return csp
        }
    }
}


Function HQDeringmod_Spline36Resize(clip input, int "target_width", int "target_height", float "src_left", float "src_top", float "src_width", float "src_height", bool "chroma")
{
    w             = input.Width()
    h             = input.Height()
     
    target_width  = Default(target_width,      w)
    target_height = Default(target_height,     h)
    src_left      = Default(src_left,          0)
    src_top       = Default(src_top,           0)
    src_width     = Default(src_width,         w)
    src_height    = Default(src_height,        h)
    chroma        = Default(chroma,         True)
    
    try {
        inputp = chroma ? input  : input .ConvertToY8()
        resize = inputp.Spline36Resize(target_width, target_height, src_left, src_top, src_width, src_height)
        resize = chroma ? resize : resize.ConvertToYV12()
    } catch (error_msg) {
        resize = input.Spline36Resize(target_width, target_height, src_left, src_top, src_width, src_height)
    }
    
    return resize
}


Function HQDeringmod_expand(clip input, int "radius")
{
    return radius<=0 ? input : input.mt_expand(U=1, V=1).HQDeringmod_expand(radius-1)
}


Function HQDeringmod_inflate(clip input, int "radius")
{
    return radius<=0 ? input : input.mt_inflate(U=1, V=1).HQDeringmod_inflate(radius-1)
}


Function HQDeringmod_U16(clip input, bool "tv_range", bool "Y", bool "U", bool "V", int "dither", int "interp", bool "HQ")
{ 
    tv_range = Default(tv_range, True   ) # define if input clip is of tv range(limited range)
    interp   = Default(interp,   0      ) # use interp or not for SmoothCurve/SmoothCurve16
    HQ       = Default(HQ,       False  ) # enable high quality interpolation (slower)
    dither   = Default(dither,   -1     ) # -1 for no dither, 0 for ordered-dither, 1-100 for noise strength
    Y        = Default(Y,        True   )
    U        = Default(U,        True   )
    V        = Default(V,        True   )
    
    
    Assert(dither>=-1 && dither<=100 , """HQDeringmod_U16: "dither" ranges from -1 to 100!""")
    
    oCceil   = (255-128) / (255.5-128) * (65535.5-32768) + 32768
    
    Yexpr    = "0-0  ;                  255-65535             ;65535-65535          "
    Cexpr    = "0-0.5;0.5-0.5;128-32768;255-"+String(oCceil)+";65535-"+String(oCceil)
    DfExpr   = "0-0;65535-65535"
    Yexpr    = Y ? Yexpr : DfExpr
    Uexpr    = U ? Cexpr : DfExpr
    Vexpr    = V ? Cexpr : DfExpr
      
    up       =  tv_range ? input.Dither_convert_8_to_16()
    \                    : Y||U||V ? StackVertical(input.Dither_gen_null_lsb(), input)
	\                               .SmoothCurve16(Ycurve=Yexpr, Ucurve=Uexpr, Vcurve=Vexpr, mode=0, interp=interp, HQ=HQ,
    \                                              dither=dither, limiter=False, TVrange=0)
    \                              : StackVertical(input.Dither_gen_null_lsb(), input)
    
    
    return up
}


Function HQDeringmod_Down8(clip input, bool "tv_range", bool "Y", bool "U", bool "V", int "dither", int "interp", bool "HQ")
{
    tv_range = Default(tv_range, True   ) # define if input clip is of tv range(limited range)
    interp   = Default(interp,   0      ) # use interp or not for SmoothCurve/SmoothCurve16
    HQ       = Default(HQ,       False  ) # enable high quality interpolation (slower)
    dither   = tv_range ? Default(dither, 6) : Default(dither, 50) # dither mode
    Y        = Default(Y,        True   )
    U        = Default(U,        True   )
    V        = Default(V,        True   )
    
    
    Assert(dither>=-1 && dither<=100 , """HQDeringmod_Down8: "dither" ranges from -1 to 100!""")
    
    iCceil   = (255-128) / (255.5-128) * (65535.5-32768) + 32768
    
    Yexpr    = "0-0;                                           65535-255"
    Cexpr    = "0-0.5;0.5-0.5;32768-128;"+String(iCceil)+"-255;65535-255"
    DfExpr   = "0-0;65535-65535"
    Yexpr    = Y ? Yexpr : DfExpr
    Uexpr    = U ? Cexpr : DfExpr
    Vexpr    = V ? Cexpr : DfExpr
    
    sDown    = tv_range ? NOP()
    \                   : input.SmoothCurve16(Ycurve=Yexpr, Ucurve=Uexpr, Vcurve=Vexpr, mode=0, interp=interp, HQ=HQ,
    \                                         dither=dither, limiter=False, TVrange=0)
    
    down     = tv_range ? input.DitherPost(mode=dither, y=Y?3:1, u=U?3:1, v=V?3:1)
    \                   : Y||U||V ? sDown.Dither_get_lsb()
    \                             : input.Dither_get_msb()
    
    
    return down
}
