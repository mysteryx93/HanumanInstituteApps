# Frame Rate Converter
# Version: 1.0  (August 8th 2017)
# By Etienne Charland
# Based on Oleg Yushko's YFRC artifact masking,
# johnmeyer's frame interpolation code, and
# raffriff42's "weak mask" and output options.
# Pinterf is the one who spent the most time working on the core libraries, adding features and fixing bugs
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA, or visit
# http:#www.gnu.org/copyleft/gpl.html.

#######################################################################################
### Frame Rate Converter
### Increases the frame rate with interpolation and fine artifact removal.
##
## YV12/YV24/Y8/YUY2
## Requires: FrameRateConverter.dll, MaskTools2, MvTools2 (pinterf), GRunT (for debug only)
##
## @ NewNum      - The new framerate numerator (if FrameDouble = false, default = 60)
##
## @ NewDen      - The new framerate denominator (if FrameDouble = false, default = 1)
##
## @ Preset      - The speed/quality preset [slower|slow|normal|fast]. (default=normal)
##
## @ BlkSize     - The block size. Latest MvTools2.dll version from Pinterf supports 6, 8, 12, 16, 24, 32, 48 and 64.
##                 Defaults for 4/3 video of height:
##                 0-359:  8
##                 360-749: 12
##                 750-1199: 16
##                 1200-1699: 24
##                 1600-2160: 32
## 
## @ BlkSizeV    - The vertical block size. (default = BlkSize)
## 
## @ FrameDouble - Whether to double the frame rate and preserve original frames (default = true)
##
## @ Output      - Output mode [auto|flow|over|none|raw|mask|skip|diff|stripe] (default = auto)
##                 auto=normal artifact masking; flow=interpolation only; over=mask as cyan overlay, stripes mask as yellow; none=ConvertFPS only; raw=raw mask; 
##                 mask=mask only; skip=mask used to Skip; diff=mask where alternate interpolation is better; stripe=mask used to cover stripes
##
## @ Debug       - Whether to display AverageLuma values of Skip, Mask and Raw. (Default = false)
##
## @ Prefilter   - Specifies a prefilter such as RgTools' RemoveGrain(22). Recommended only when not using a denoiser (Default=none)
##
## @ MaskThr     - The threshold where a block is considered bad, between 0 and 255. Smaller = stronger.
##                 0 to disable artifact masking. (Default = 100)
##
## @ MaskOcc     - Occlusion mask threshold, between 0 and 255. 0 to disable occlusion masking. (Default = 105)
##
## @ SkipThr     - The threshold where a block is counted for the skip mask, between 0 and 255. Smaller = stronger.
##                 Must be smaller (stronger) than MaskThr. (Default = 55)
##
## @ BlendOver   - Try fallback block size when artifacts cover more than specified threshold, or 0 to disable.
##                 If it fails again, it will revert to frame blending. (default = 60)
##
## @ SkipOver    - Skip interpolation of frames when artifacts cover more than specified threshold, 
##                 or 0 to disable. (Default = 120)
##                 
## @ Stripes     - How to deal with stripes [none|skip|blend] (default=skip)
##
## @ Dct         - Overrides DCT parameter for MAnalyse (default: Normal=0, Slow/Slower=4, Slowest=1)
##                 
##
## Presets
## Fast:    Basic interpolation
## Normal:  Fast + MRecalculate
## Slow:    Normal + DCT=4
## Slower:  Slow + calculate diff between DCT=4 and DCT=0 to take the best from both
## Slowest: Slower + use DCT=1 instead of 4
##
function FrameRateConverter(clip C, int "NewNum", int "NewDen", string "Preset", int "BlkSize", int "BlkSizeV", bool "FrameDouble", string "Output", bool "Debug",
\ clip "Prefilter", int "MaskThr", int "MaskOcc", int "SkipThr", int "BlendOver", int "SkipOver", string "Stripes", int "Dct")
{
    Preset = Default(Preset, "normal")
    P_SLOWEST = 0  P_SLOWER = 1  P_SLOW = 2  P_NORMAL = 3  P_FAST = 4
    Pset = Preset == "slowest" ? P_SLOWEST : Preset == "slower" ? P_SLOWER : Preset == "slow" ? P_SLOW : Preset == "normal" ? P_NORMAL : Preset == "fast" ? P_FAST : -1
    Assert(Pset != -1, "FrameRateConverter: 'Preset' must be slowest, slower, slow, normal or fast {'" + Preset + "'}")
    Output = Default(Output, "auto")
    O_AUTO = 0  O_FLOW = 1  O_OVER = 2  O_NONE = 3  O_RAW = 4  O_MASK = 5  O_SKIP = 6  O_DIFF = 7  O_STRIPE = 8
    OPut = Output == "auto" ? O_AUTO : Output == "flow" ? O_FLOW : Output == "over" ? O_OVER : Output == "none" ? O_NONE : Output == "raw" ? O_RAW : Output == "mask" ? O_MASK : Output == "skip" ? O_SKIP : Output == "diff" ? O_DIFF : Output == "stripe" ? O_STRIPE : -1
    Assert(OPut != -1, "FrameRateConverter: 'Output' not one of (auto|flow|none|mask|skip|raw|diff|over) {'" + Output + "'}")
    Stripes = Default(Stripes, "blend")
    S_NONE = 0  S_SKIP = 1  S_BLEND = 2
    Stp = Stripes == "none" ? S_NONE : Stripes == "skip" ? S_SKIP : Stripes == "blend" ? S_BLEND : -1
    Assert(Stp != -1, "FrameRateConverter: 'Stripes' not one of (none|skip|blend) {'" + Stripes + "'}")
    
    FrameDouble = Default(FrameDouble, Defined(NewNum) ? false : true)
    NewNum      = FrameDouble ? C.FrameRateNumerator * 2 : Default(NewNum, 60)
    NewDen      = FrameDouble ? C.FrameRateDenominator : Default(NewDen, 1)
    DefH        = Max(C.Height, C.Width/4*3)
    BlkSize     = Default(BlkSize, DefH<360 ? 8 : DefH<750 ? 12 : DefH<1200 ? 16 : DefH<1600 ? 24 : 32)
    BlkSizeV    = Default(BlkSizeV, BlkSize)
    MaskThr     = Default(MaskThr, 100)
    SkipThr     = Default(SkipThr, 55)
    MaskOcc     = MaskThr > 0 ? Default(MaskOcc, 105) : 0
    BlendOver   = Default(BlendOver, 60)
    SkipOver    = Default(SkipOver, 120)
    CalcPrefilter = Defined(Prefilter)
    Prefilter   = Default(Prefilter, C)
    Debug       = Default(Debug, false)
    OutFps      = OPut!=O_MASK && OPut!=O_SKIP && OPut!=O_RAW && OPut!=O_DIFF && OPut!=O_STRIPE  # Whether output has altered frame rate
    Recalculate = PSET <= P_NORMAL
    Dct         = Default(Dct, PSET <= P_SLOWEST ? 1 : PSET <= P_SLOW ? 4 : 0)
    CalcDiff    = Pset <= P_SLOWER

    Assert(MaskThr >= 0 && MaskThr <= 255, String(MaskThr, "FrameRateConverter: MaskThr must be between 0 and 255 {%.f}"))
    Assert(MaskOcc >= 0 && MaskOcc <= 255, String(MaskOcc, "FrameRateConverter: MaskOcc must be between 0 and 255 {%.f}"))
    Assert(SkipThr < MaskThr, "FrameRateConverter: SkipThr must be lower (stronger) than MaskThr")
    Assert(BlendOver >= 0 && BlendOver <= 255, String(BlendOver, "FrameRateConverter: BlendOver must be between 0 and 255 {%.f}"))
    Assert(SkipOver >= 0 && SkipOver <= 255, String(SkipOver, "FrameRateConverter: SkipOver must be between 0 and 255 {%.f}"))
    Assert(BlendOver==0 || SkipOver==0 || SkipOver > BlendOver, "FrameRateConverter: SkipOver must be greater than BlendOver")
    Assert(CalcDiff || OPut!=O_DIFF, "FrameRateConverter: You can only use Output='Diff' when using Preset=slower or slowest")

    ## "B" - Blending, "BHard" - No blending
    B = C.ConvertFps(NewNum, NewDen)
    BHard = C.ChangeFps(NewNum, NewDen)
    AvsPlus = Findstr(VersionString(), "AviSynth+") != 0 && Findstr(VersionString(), "r1576") == 0
    Blank = BlankClip(AvsPlus ? C.ConvertToY() : C.ConvertToY8(), color_yuv=$000000)

    ## Adjust parameters for different block sizes, causing stronger or weaker masks
    blk = Max(BlkSize, BlkSizeV)
    MaskThr = MaskThr + (blk<=4 ? -40 : blk<=6 ? -30 : blk<=8 ? -20 : blk<=12 ? -10 : blk<=16 ? 0 : blk<=24 ? 10 : blk<=32 ? 20 : blk<=48 ? 28 : 35)
    SkipThr = SkipThr + (blk<=4 ? -33 : blk<=6 ? -26 : blk<=8 ? -18 : blk<=12 ? -9 : blk<=16 ? 0 : blk<=24 ? 8 : blk<=32 ? 16 : blk<=48 ? 23 : 30)
    MaskThr = Max(Min(MaskThr, 255), 0)
    SkipThr = Max(Min(SkipThr, 255), 0)
    gam = blk<=4 ? .60 : blk<=6 ? .58 : blk<=8 ? .56 : blk<=12 ? .54 : blk<=16 ? .50 : blk<=24 ? .44 : blk<=32 ? .36 : blk<=48 ? .26 : .14

    ## jm_fps interpolation
    superfilt = MSuper(prefilter, hpad=16, vpad=16) # all levels for MAnalyse
    super = CalcPrefilter ? MSuper(C, hpad=16, vpad=16, levels=1) : superfilt # one level is enough for MRecalculate
    bak = MAnalyse(superfilt, isb=true, blksize=BlkSize, blksizev=BlkSizeV, overlap = BlkSize>4?(BlkSize/4+1)/2*2:0, overlapv = BlkSizeV>4?(BlkSizeV/4+1)/2*2:0, search=3, dct=Dct)
    fwd = MAnalyse(superfilt, isb=false, blksize=BlkSize, blksizev=BlkSizeV, overlap = BlkSize>4?(BlkSize/4+1)/2*2:0, overlapv = BlkSizeV>4?(BlkSizeV/4+1)/2*2:0, search=3, dct=Dct)
    fwd = Recalculate ? MRecalculate(super, fwd, blksize=BlkSize/2, blksizev=BlkSizeV/2, overlap = BlkSize/2>4?(BlkSize/8+1)/2*2:0, overlapv = BlkSizeV/2>4?(BlkSizeV/8+1)/2*2:0, thSAD=100) : fwd
    bak = Recalculate ? MRecalculate(super, bak, blksize=BlkSize/2, blksizev=BlkSizeV/2, overlap = BlkSize/2>4?(BlkSize/8+1)/2*2:0, overlapv = BlkSizeV/2>4?(BlkSizeV/8+1)/2*2:0, thSAD=100) : bak
    Flow = MFlowFps(C, super, bak, fwd, num=NewNum, den=NewDen, blend=false, ml=200, mask=2, thSCD2=255)

    ## "EM" - error or artifact mask
    # Mask: SAD
    EM = MaskThr > 0 ? C.ConvertToY8().MMask(bak, ml=255, kind=1, gamma=1/gam, ysc=255, thSCD2=255) : Blank
    # Mask: Temporal blending
    EMfwd = MaskThr > 0 ? C.ConvertToY8().MMask(fwd, ml=255, kind=1, gamma=1/gam, thSCD2=255) : EM
    EM = MaskThr > 0 ? EM.Overlay(EMfwd, opacity=.6, mode="lighten", pc_range=true) : EM
    # Mask: Occlusion
    EMocc = MaskOcc > 0 ? C.ConvertToY8().MMask(bak, ml=MaskOcc, kind=2, gamma=1/gam, ysc=255, thSCD2=255).mt_inpand() : Blank
    EM = MaskOcc > 0 ? EM.Overlay(EMocc, opacity=.4, mode="lighten", pc_range=true) : EM


    ## For CalcDiff, calculate a 2nd version and create mask to restore from 2nd version the areas that look better
    bak2 = CalcDiff ? MAnalyse(superfilt, isb=true, blksize=BlkSize, blksizev=BlkSizeV, overlap = BlkSize>4?(BlkSize/4+1)/2*2:0, overlapv = BlkSizeV>4?(BlkSizeV/4+1)/2*2:0, search=3, dct=0) : nop
    fwd2 = CalcDiff ? MAnalyse(superfilt, isb=false, blksize=BlkSize, blksizev=BlkSizeV, overlap = BlkSize>4?(BlkSize/4+1)/2*2:0, overlapv = BlkSizeV>4?(BlkSizeV/4+1)/2*2:0, search=3, dct=0) : nop
    fwd2 = CalcDiff ? Recalculate ? MRecalculate(super, fwd2, blksize=BlkSize/2, blksizev=BlkSizeV/2, overlap = BlkSize/2>4?(BlkSize/8+1)/2*2:0, overlapv = BlkSizeV/2>4?(BlkSizeV/8+1)/2*2:0, thSAD=100) : fwd : nop
    bak2 = CalcDiff ? Recalculate ? MRecalculate(super, bak2, blksize=BlkSize/2, blksizev=BlkSizeV/2, overlap = BlkSize/2>4?(BlkSize/8+1)/2*2:0, overlapv = BlkSizeV/2>4?(BlkSizeV/8+1)/2*2:0, thSAD=100) : bak : nop
    Flow2 = CalcDiff ? MFlowFps(C, super, bak2, fwd2, num=NewNum, den=NewDen, blend=false, ml=200, mask=2, thSCD2=255) : nop
    # Get raw mask again
    EM2 = CalcDiff ? MaskThr > 0 ? C.ConvertToY8().MMask(bak2, ml=255, kind=1, gamma=1/gam, ysc=255, thSCD2=255) : Blank : nop
    EMfwd2 = CalcDiff ? MaskThr > 0 ? C.ConvertToY8().MMask(fwd2, ml=255, kind=1, gamma=1/gam, thSCD2=255) : EM2 : nop
    EM2 = CalcDiff ? MaskThr > 0 ? EM2.Overlay(EMfwd2, opacity=.6, mode="lighten", pc_range=true) : EM2 : nop
    EMocc2 = CalcDiff ? MaskOcc > 0 ? C.ConvertToY8().MMask(bak2, ml=MaskOcc, kind=2, gamma=1/gam, ysc=255, thSCD2=255).mt_inpand() : Blank : nop
    EM2 = CalcDiff ? MaskOcc > 0 ? EM2.Overlay(EMocc2, opacity=.4, mode="lighten", pc_range=true) : EM2 : nop
    # Get difference mask between two versions
    EMdiff = CalcDiff ? mt_lutxy(EM, EM2, "x y -")
    \       .BicubicResize(Round(C.Width/BlkSize)*4, Round(C.Height/BlkSizeV)*4)
    \       .mt_expand(mode= mt_circle(zero=true, radius=2))
    \       .mt_binarize(60)
    \       .FRC_GaussianBlur42(1.2)
    \       .BicubicResize(C.Width, C.Height) : nop
    # Apply mask to Flow / EM
    EMdiff = CalcDiff ? OutFps ? EMdiff.ChangeFPS(NewNum, NewDen) : EMdiff : nop
    Flow = CalcDiff ? mt_merge(Flow, Flow2, EMdiff, luma=true, chroma="process") : Flow
    EM = CalcDiff ? mt_merge(EM, EM2, EMdiff, luma=true, chroma="process") : EM

    # Last mask frame is white. Replace with previous frame.
    EM = EM.DeleteFrame(EM.Framecount-1).Loop(2, EM.Framecount-1)

    # Create skip mask
    EMskip = EM.BicubicResize(Round(C.Width/BlkSize/4.0)*4, Round(C.Height/BlkSizeV/4.0)*4)
    \       .mt_expand(mode= mt_circle(zero=true, radius=1))
    \       .mt_binarize(SkipThr)
    OutSkip = EMskip.BicubicResize(C.width, C.Height)
    
    ## Create artifact correction mask
    OutRaw = EM
    EM = EM.BicubicResize(Round(C.Width/BlkSize/4.0)*4, Round(C.Height/BlkSizeV/4.0)*4)
    \       .mt_expand(mode= mt_circle(zero=true, radius=1))
    \       .mt_binarize(MaskThr)
    \       .Blur(.6)
    \       .BicubicResize(C.Width, C.Height)

    # Mask: Stripes
    EMstp = C.StripeMask(blksize=BlkSize, blksizev=BlkSizeV, str=min(SkipThr*2+20, 255), strf=min(SkipThr+10, 255), trh=23)
    \      .BicubicResize(Round(C.Width/BlkSize)*4, Round(C.Height/BlkSizeV)*4)
    \      .ContinuousMask(22)
    EMstp = EMstp.BicubicResize(EMstp.Width/2, EMstp.Height/2)
    \      .mt_binarize(82)
    \      .mt_inpand()
    \      .mt_expand(mode= mt_circle(zero=true, radius= Stp==S_SKIP ? 12 : 8))
    \      .FRC_GaussianBlur42(Stp==S_SKIP ? 8.0 : 2.8)
    \      .BicubicResize(C.Width, C.Height)
    
    ## "M" - Apply artifact removal
    EM = OutFps ? EM.ChangeFPS(NewNum, NewDen) : EM
    EMskip = OutFps ? EMskip.ChangeFPS(NewNum, NewDen) : EMskip
    EMstp = OutFps ? EMstp.ChangeFPS(NewNum, NewDen) : EMstp
    M = OutFps ? mt_merge(Flow, B, EM, luma=true, chroma="process") : Flow
    M = Stp != S_NONE && OutFps ? mt_merge(M, Stp == S_SKIP ? BHard : B, EMstp, luma=true, chroma="process") : M

    ## Apply BlendOver and SkipOver
    M2 = SkipOver > 0 ? ConditionalFilterMT(EMskip, B, BHard, "AverageLuma", "<", string(SkipOver)) : B
    M = BlendOver > 0 ? ConditionalFilterMT(EMskip, M, M2, "AverageLuma", "<", string(BlendOver)) : M

    # Prepare Output=Over: Mask(cyan), Stripes(yellow)
    FlowOver = Flow.Overlay(MergeRGB(Blank, EM, EM), mode="Add", opacity=0.40, pc_range=true)
    FlowOver = Stp != S_NONE ? FlowOver.Overlay(MergeRGB(EMstp, EMstp, Blank), mode="Add", opacity=0.40, pc_range=true) : FlowOver

    # Output modes
    R= (Oput==O_AUTO)                               [** auto: artifact masking     *]
    \     ? (FrameDouble ? Interleave(C, SelectOdd(M)) : M)
    \ : (Oput==O_FLOW)                              [** flow: interpolation only   *]
    \     ? Flow
    \ : (Oput==O_OVER)                              [** over: mask as cyan overlay *]
    \     ? FlowOver
    \ : (Oput==O_NONE)                              [** none: ConvertFPS only      *]
    \     ? B
    \ : (Oput==O_RAW)                               [** raw:  raw mask             *]
    \     ? OutRaw.ColorYUV(levels="PC->TV")
    \ : (Oput==O_MASK)                              [** mask: mask only            *]
    \     ? EM
    \ : (Oput==O_SKIP)                              [** skip: skip mask            *]
    \     ? OutSkip
    \ : (Oput==O_DIFF)                              [** diff: diff mask            *]
    \     ? EMdiff
    \ : (Oput==O_STRIPE)                            [** stripe: stripes mask       *]
    \     ? EMstp
    \ : Assert(false, "FrameRateConverter: 'Output' INTERNAL ERROR")

    # Debug: display AverageLuma values of Skip, Mask and Raw
    ShowRaw = OutFps ? OutRaw.ChangeFPS(NewNum, NewDen) : OutRaw
    R = Debug ? R.GScriptClip("""Skip = EMskip.AverageLuma()
    \           SkipSoft = BlendOver > 0 && Skip >= BlendOver && (Skip < SkipOver || SkipOver == 0)
    \           Subtitle("BlkSize: " + string(BlkSize) + 
    \           (SkipSoft ? " - Blend" : "") +
    \           (SkipOver > 0 && Skip >= SkipOver ? " - Skip" : "") +
    \           "\nSkip:  " + string(Skip, "%.4f") + 
    \           "\nRaw:  " + string(ShowRaw.AverageLuma, "%.4f") + 
    \           "\nMask: " + string(EM.AverageLuma, "%.4f") + 
    \           (CalcDiff ? "\nDiff:    " + string(EMdiff.AverageLuma, "%.4f") : "") + 
    \           (Stp != 0 ? "\nStripes: " + string(EMstp.AverageLuma, "%.4f") : "")
    \           , lsp=0)""", args = "EM,EMskip,EMstp,EMdiff,ShowRaw,BlkSize,SkipOver,BlendOver,CalcDiff,Stp", Local=true) : R
    return R
}


##################################
### Emulate [[VariableBlur/GaussianBlur]] 
##  For YUV, effective chroma blur varies depending on source 
##  color subsampling - YUV444 has *more* chroma blur, others less.
##
## @ var - works like GaussianBlur's varY
## @ rad - blur radius (<var> squared); overrides <var>
## @ vvar, vrad - vertical var & rad; default same as horizontal
## @ p  - final [[GaussResize]] sharpness. Default 19
##        (if > 25, blockiness; if < 15, loss of contrast)
##
## version 2013-10-23 raffriff42 
## version 2014-05-31 discrete hor. and vert. args
## version 2017-05-21 bugfix: blockiness
##
function FRC_GaussianBlur42(clip C, 
\               float "var", float "rad",
\               float "vvar", float "vrad", float "p")
{
    var = Max(0.0, Float(Default(var, 1.0)))
    rad = Max(1.0, Float(Default(rad, Pow(var, 0.5))))
    var = Pow(Min(Max(0.0, rad), 60.0), 1.9) ## arbitrary max radius = 60

    vvar = Max(0.0, Float(Default(vvar, var)))
    vrad = Max(1.0, Float(Default(vrad, Pow(vvar, 0.5))))
    vvar = Pow(Min(Max(0.0, vrad), 60.0), 1.9)
    p    = Default(p, 19)

    w0 = C.Width
    h0 = C.Height
    w1 = Round(w0/rad)
    h1 = Round(h0/vrad) 

    B = C.BilinearResize(
    \         Min(Max(4, w1 + (w1 % 2)), w0), 
    \         Min(Max(4, h1 + (h1 % 2)), h0))

    B = B.Blur(1.0).Blur(1.0)
    
    return (var<0.01 && vvar<0.01)   ? C 
    \    : (B.Width>8 && B.Height>8) ? B.GaussResize(w0, h0, p=p)
    \                                : B.BilinearResize(w0, h0)
}