####################################################################################################################################
###                                                                                                                              ###
###                                   Motion-Compensated Temporal Denoise: MCTemporalDenoise()                                   ###
###                                                                                                                              ###
###                                                     v1.4.20 by "LaTo INV."                                                   ###
###                                                                                                                              ###
###                                                           2 July 2010                                                        ###
###                                                                                                                              ###
####################################################################################################################################
### 
### 
### 
### /!\ Needed filters: MVTools (v2.5.10), MaskTools (v2.0a43),
### ------------------- LSFmod (v1.9), FFT3Dgpu (v0.8.2.7) or FFT3Dfilter (v2.1.1),
###                     TTempsmooth (v0.9.4), RemoveGrain + Repair (v1.0PR),
###                     Deblock (v1.2), Deblock_QED (25.may.2010), DctFilter (v0.0.1.4),
###                     EEDI2 (v0.9.2), SangNom (v1.0beta),
###                     GradFun2DBmod (v1.5), GradFun2db (v1.0), AddGrainC (v1.5).
###
### 
### 
### USAGE: MCTemporalDenoise(i, radius, sigma, twopass, useTTmpSm, limit, limit2, post, chroma, interlaced,
###                          sharp, adapt, strength, SHmode, SHmethod, Slimit, Sovershoot, Tlimit, Tovershoot,
###                          protect, cutoff, threshold, maxdiff,
###                          AA, useEEDI2, reduc, maxd, AAthr, method,
###                          deblock, useQED, quant1, quant2, 
###                          edgeclean, ECrad, ECthr, ECmode,
###                          stabilize, maxr, TTstr,  
###                          enhance, GFthr, AGstr, bias, temp,
###                          bwbh, owoh, blksize, overlap,
###                          GPU, fixband, bt, ncpu, precision,
###                          thSAD, thSAD2, thSCD1, thSCD2,
###                          truemotion, MVglobal, pel, pelsearch, search, searchparam, MVsharp, DCT,
###                          p, settings,
###                          show, screenW, screenH)
###
###
###
### PARAMETERS:
### -----------
###
### +---------+
### | DENOISE |
### +---------+-----------------------------------------------------------------------------------------------+
### | radius     : Temporal radius [1...6]                                                                    |
### | sigma      : FFT3D sigma for the pre-filtering clip [0=no pre-filtering,1...]                           |
### | twopass    : Do the denoising job in 2 stages (stronger but very slow)                                  |
### | useTTmpSm  : Use MDegrain (faster) or MCompensate+TTempsmooth (stronger)                                |
### | limit      : Limit the effect of the first denoising [-1=auto,0=off,1...255]                            |
### | limit2     : Limit the effect of the second denoising (if twopass=true) [-1=auto,0=off,1...255]         |
### | post       : Sigma value for post-denoising with FFT3D (before all the PP stage) [0=off,...]            |
### | chroma     : Process or not the chroma plane                                                            |
### | interlaced : Input video interlaced or not                                                              |
### +---------------------------------------------------------------------------------------------------------+
###
###
### +-------+
### | SHARP |
### +-------+-----------------------------------------------------------------------------------------------------------------------------+
### | sharp      : Enable Post-Sharpening (Spatial-Sharpening + Motion-compensated temporal limiting)                                     |
### | adapt      : Adaptative sharpening --> sharp only static & detailed areas, prevents grain sharpening & save much bitrate (+-10/15%) |
### | strength   : Sharpening strength ( see LSFmod: strength  )                                                                          |
### | SHmode     : Sharpening mode     ( see LSFmod: Smode     )                                                                          |
### | SHmethod   : Sharpening method   ( see LSFmod: Smethod   )                                                                          |
### | Slimit     : Spatial limiting    ( see LSFmod: Lmode     )                                                                          |
### | Sovershoot : Spatial overshoot   ( see LSFmod: overshoot )                                                                          |
### | Tlimit     : Temporal limiting (Motion compensated, clamp output to Tovershoot)                                                     |
### | Tovershoot : Temporal overshoot, = Maximum sharpening excess allowed (If Tovershoot=0, don't sharp more than the original clip.)    |
### |              /!\ If you set Tovershoot>0, be sure that Slimit!=0 (to not over-enhance edges... Slimit=2 is a good compromise)       |
### +-------------------------------------------------------------------------------------------------------------------------------------+
###
###
### +---------------------------------+
### | STAR & BRIGHT POINTS PROTECTION |
### +---------------------------------+---------------------------------------------------------------------------------------+
### | protect   : Enable star & bright points protection                                                                      |
### | cutoff    : Luma cutoff (process only brighter areas than this value) [0=all,1...255]                                   |
### | threshold : Luma threshold (minimal change needed to be filtered, the higher = the lesser areas are filtered) [0...127] |
### | maxdiff   : Maximum change allowed between source and result (the lesser, the brighter) [0=source,1...255]              |
### +-------------------------------------------------------------------------------------------------------------------------+
###
###
### +--------------+
### | ANTIALIASING |
### +--------------+--------------------------------------------------------------------------------------------------------+
### | AA       : Enable antialiasing (only on edges)                                                                        |
### | useEEDI2 : If true use EEDI2 filtering, else use SANGNOM filtering                                                    |
### | reduc    : MC-soothe reduc parameter, reduc==-1 turn off "MC-antialiasing" [-1=off,0.0...1.0]                         |
### | maxd     : If EEDI2 is used it's his maxd setting, if SANGNOM is used it's his AA setting (see docs of these filters) |
### | AAthr    : Threshold for mask (the higher, the less "small edges" are processed) [0...255]                            |
### | method   : Sharpness for downsize [0=bilinear,1=spline36,2=lanczos4]                                                  |
### +-----------------------------------------------------------------------------------------------------------------------+
###
###
### +---------+
### | DEBLOCK |
### +---------+-----------------------------------------------------------------------------------+
### | deblock : Enable deblocking before the denoising                                            |
### | useQED  : If true, use Deblock_QED, else use Deblock (faster & stronger)                    |
### | quant1  : Deblock_QED "quant1" parameter (Deblock "quant" parameter is "(quant1+quant2)/2") |
### | quant2  : Deblock_QED "quant2" parameter (Deblock "quant" parameter is "(quant1+quant2)/2") |
### +---------------------------------------------------------------------------------------------+
###
###
### +------------------------------+
### | EDGECLEAN: DERING, DEHALO... |
### +------------------------------+-----------------------------------------------------------------------------------------------------+
### | edgeclean : Enable safe edgeclean process after the denoising (only on edges which are in non-detailed areas, so less detail loss) |
### | ECrad     : Radius for mask (the higher, the greater distance from the edge is filtered)                                           |
### | ECthr     : Threshold for mask (the higher, the less "small edges" are process) [0...255]                                          |
### | ECmode    : Script/Filter used for cleaning edges (ex: "dfttest().dehalo_alpha()", don't forget quotes "")                         |
### +------------------------------------------------------------------------------------------------------------------------------------+
###
###
### +-----------+
### | STABILIZE |
### +-----------+------------------------------------------------------------------------------------------------+
### | stabilize : Enable TTempSmooth post processing to stabilize flat areas (background will be less "nervous") |
### | maxr      : Temporal radius (the higher, the more stable image)                                            |
### | TTstr     : Strength (see TTempSmooth docs)                                                                |
### +------------------------------------------------------------------------------------------------------------+
###
###
### +---------+
### | ENHANCE |
### +---------+-----------------------------------------------------------------------------------------------------------------------------------------------------------+
### | enhance : Enable GradFun2DBmod post processing to enhance flat areas (may avoid banding & blocking when encoding too)                                               |
### | GFthr   : Gradfun2db threshold [1.0=off,...]                                                                                                                        |
### | AGstr   : AddGrain strength  [0.0=off,...]                                                                                                                          |
### | bias    : Brightness bias for adaptative grain mask (the higher, the less grain in dark areas & the more grain in bright areas) [-1=off,0=input,1...254,255=invert] |
### | temp    : Temporal stabilization for grain (the higher, the more calm) [0=nervous,...,100=static]                                                                   |
### +---------------------------------------------------------------------------------------------------------------------------------------------------------------------+
###
###
### +---------------------+
### | BLOCKSIZE / OVERLAP |
### +---------------------+----------------+
### | bwbh    : FFT3D blocksize            |
### | owoh    : FFT3D overlap              |
### |             - for speed:   bwbh/4    |
### |             - for quality: bwbh/2    |
### | blksize : MVTools blocksize          |
### | overlap : MVTools overlap            |
### |             - for speed:   blksize/4 |
### |             - for quality: blksize/2 |
### +--------------------------------------+
###
###
### +-------+
### | FFT3D |
### +-------+---------------------------------------+
### | GPU     : Use FFT3Dgpu instead of FFT3Dfilter |
### | fixband : Prevent banding issue with FFT3D    |
### +---------------------------------------+-------+
### | bt        : FFT3D block temporal size |
### | ncpu      : FFT3Dfilter ncpu          |
### | precision : FFT3Dgpu precision        |
### +---------------------------------------+
###
###
### +---------+
### | MVTOOLS |
### +---------+----------------------------------------------------+
### | thSAD  : MVTools thSAD for the first pass                    |
### | thSAD2 : MVTools thSAD for the second pass (if twopass=true) |
### | thSCD1 : MVTools thSCD1                                      |
### | thSCD2 : MVTools thSCD2                                      |
### +-----------------------------------+--------------------------+
### | truemotion  : MVTools truemotion  |
### | MVglobal    : MVTools global      |
### | pel         : MVTools pel         |
### | pelsearch   : MVTools pelsearch   |
### | search      : MVTools search      |
### | searchparam : MVTools searchparam |
### | MVsharp     : MVTools sharp       |
### | DCT         : MVTools DCT         |
### +-----------------------------------+
###
###
### +--------+
### | GLOBAL |
### +--------+-----------------------------------------------------+
### | p        : Set an external prefilter clip                    |
### | settings : Global MCTemporalDenoise settings [default="low"] |
### |             - "very low"                                     |
### |             - "low"                                          |
### |             - "medium"                                       |
### |             - "high"                                         |
### |             - "very high"                                    |
### +--------------------------------------------------------------+
###
###
### +-------+
### | DEBUG |
### +-------+------------------------------------------------+
### | show    : Show debug clip & informations               |
### | screenW : Screen horizontal resolution (for show clip) |
### | screenH : Screen vertical resolution (for show clip)   |
### +--------------------------------------------------------+
###
###
###
### DEFAULTS:
### ---------
###
### +-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+
### | SETTINGS    |      VERY LOW               |      LOW                    |      MEDIUM                 |      HIGH                   |      VERY HIGH              |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | radius      |      1                      |      2                      |      3                      |      2                      |      3                      |
### | sigma       |      2                      |      4                      |      8                      |      12                     |      16                     |
### | twopass     |      false                  |      false                  |      false                  |      true                   |      true                   |
### | useTTmpSm   |      false                  |      false                  |      false                  |      false                  |      false                  |
### | limit       |      -1                     |      -1                     |      -1                     |      -1                     |      0                      |
### | limit2      |      -1                     |      -1                     |      -1                     |      0                      |      0                      |
### | post        |      0                      |      0                      |      0                      |      0                      |      0                      |
### | chroma      |      false                  |      false                  |      true                   |      true                   |      true                   |
### | interlaced  |      false                  |      false                  |      false                  |      false                  |      false                  |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | sharp       |      true                   |      true                   |      true                   |      true                   |      true                   |
### | adapt       |      true                   |      true                   |      true                   |      true                   |      true                   |
### | strength    |      100                    |      100                    |      150                    |      200                    |      200                    |
### | SHmode      |      3                      |      3                      |      3                      |      3                      |      3                      |
### | SHmethod    |      3                      |      3                      |      3                      |      3                      |      3                      |
### | Slimit      |      0                      |      0                      |      0                      |      0                      |      0                      |
### | Sovershoot  |      0                      |      0                      |      0                      |      0                      |      0                      |
### | Tlimit      |      true                   |      true                   |      true                   |      true                   |      true                   |
### | Tovershoot  |      2                      |      2                      |      1                      |      0                      |      0                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | protect     |      false                  |      false                  |      false                  |      false                  |      false                  |
### | cutoff      |      64                     |      64                     |      64                     |      64                     |      64                     |
### | threshold   |      16                     |      16                     |      16                     |      16                     |      16                     |
### | maxdiff     |      4                      |      4                      |      4                      |      4                      |      4                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | AA          |      false                  |      false                  |      false                  |      false                  |      false                  |
### | useEEDI2    |      true                   |      true                   |      false                  |      false                  |      false                  |
### | reduc       |      0.5                    |      0.5                    |      0.5                    |      0.5                    |      0.5                    |
### | maxd        |      24                     |      24                     |      48                     |      48                     |      48                     |
### | AAthr       |      64                     |      32                     |      32                     |      16                     |      16                     |
### | method      |      2                      |      1                      |      1                      |      0                      |      0                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | deblock     |      false                  |      false                  |      false                  |      false                  |      false                  |
### | useQED      |      true                   |      true                   |      true                   |      false                  |      false                  |
### | quant1      |      10                     |      20                     |      30                     |      30                     |      40                     |
### | quant2      |      20                     |      40                     |      60                     |      60                     |      80                     |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | edgeclean   |      false                  |      false                  |      false                  |      false                  |      false                  |
### | ECrad       |      1                      |      2                      |      3                      |      4                      |      5                      |
### | ECthr       |      32                     |      32                     |      32                     |      16                     |      16                     |
### | ECmode      |      "Removegrain(2,0)"     |      "Removegrain(2,0)"     |      "Removegrain(2,2)"     |      "Removegrain(4,4)"     |      "Removegrain(4,4)"     |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | stabilize   |      false                  |      false                  |      false                  |      true                   |      true                   |
### | maxr        |      1                      |      1                      |      2                      |      2                      |      2                      |
### | TTstr       |      1                      |      1                      |      1                      |      2                      |      2                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | enhance     |      true                   |      true                   |      false                  |      false                  |      false                  |
### | GFthr       |      1.1                    |      1.2                    |      1.4                    |      1.6                    |      1.8                    |
### | AGstr       |      0.6                    |      0.8                    |      1.2                    |      1.8                    |      2.4                    |
### | bias        |      64                     |      64                     |      64                     |      64                     |      64                     |
### | temp        |      50                     |      50                     |      50                     |      50                     |      50                     |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | bwbh        |      HD?16:8                |      HD?16:8                |      HD?16:8                |      HD?16:8                |      HD?16:8                |
### | owoh        |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |
### | blksize     |      HD?16:8                |      HD?16:8                |      HD?16:8                |      HD?16:8                |      HD?16:8                |
### | overlap     |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |      HD? 8:4                |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | GPU         |      false                  |      false                  |      false                  |      false                  |      false                  |
### | fixband     |      true                   |      true                   |      true                   |      true                   |      true                   |
### | bt          |      1                      |      3                      |      3                      |      3                      |      4                      |
### | ncpu        |      1                      |      1                      |      1                      |      1                      |      1                      |
### | precision   |      2                      |      2                      |      2                      |      2                      |      2                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | thSAD       |      200                    |      300                    |      400                    |      500                    |      600                    |
### | thSAD2      |      200                    |      300                    |      400                    |      500                    |      600                    |
### | thSCD1      |      200                    |      300                    |      400                    |      500                    |      600                    |
### | thSCD2      |      90                     |      100                    |      100                    |      130                    |      130                    |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | truemotion  |      false                  |      false                  |      false                  |      false                  |      false                  |
### | MVglobal    |      true                   |      true                   |      true                   |      true                   |      true                   |
### | pel         |      1                      |      2                      |      2                      |      2                      |      2                      |
### | pelsearch   |      1                      |      2                      |      2                      |      2                      |      2                      |
### | search      |      2                      |      2                      |      2                      |      2                      |      2                      |
### | searchparam |      2                      |      2                      |      2                      |      2                      |      2                      |
### | MVsharp     |      2                      |      2                      |      2                      |      1                      |      0                      |
### | DCT         |      0                      |      0                      |      0                      |      0                      |      0                      |
### |-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------|
### | show        |      false                  |      false                  |      false                  |      false                  |      false                  |
### | screenW     |      1280                   |      1280                   |      1280                   |      1280                   |      1280                   |
### | screenH     |      1024                   |      1024                   |      1024                   |      1024                   |      1024                   |
### +-------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+-----------------------------+
###
###
###
### MCTemporalDenoise's Post Processor :
### ------------------------------------
### 
### USAGE: MCTemporalDenoisePP( denoised, source,
###                             sharp, adapt, strength, SHmode, SHmethod, Slimit, Sovershoot, Tlimit, Tovershoot,
###                             protect, cutoff, threshold, maxdiff,
###                             AA, useEEDI2, reduc, maxd, AAthr, method,
###                             deblock, useQED, quant1, quant2,
###                             edgeclean, ECrad, ECthr, ECmode,
###                             stabilize, maxr, TTstr,
###                             enhance, GFthr, AGstr, bias, temp,
###                             chroma, settings )
###
### This is the post-processor used internally in MCTemporalDenoise.
### It can be called externally if you want some specific functionality without denoising.
###
### Parameters & defaults are the same as MCTemporalDenoise. 
### By default, all filters are OFF!
###
### "Sharp" & "Protect" options need 2 inputs:
### EX: source=last
###     denoised=source.FFT3Dfilter()
###     MCTemporalDenoisePP(denoised,source,...)
### The others only need "denoised".
###
###
###
### CHANGELOG:
### ----------
###
### v1.4.20 : tweaked some settings
###           updated QED_Deblock version
###
### v1.4.18 : fixed anti-aliasing PP (was broken since v1.4.07)
###           updated MVTools-MaskTools-AddgrainC version
###
### v1.4.16 : added switch to avoid loading of unused filters 
###           fixed a bug in anti-aliasing part
###
### v1.4.08 : fixed MOD16 warning when deblock=true
###
### v1.4.07 : improved masking in PP-stage
###           cleaned code (cosmetic)
###
### v1.4.00 : fixed bug with maxdiff=0
###           fixed speed issue in MCTemporalDenoisePP (+20%)
###           added "deblock" to MCTemporalDenoisePP
###           changed "flat" to "enhance"
###           cleaned and optimized all the code
###           changed some settings
###           updated documentation
###
### v1.3.09 : added MOD4 restriction
###           updated MVTools version
###
### v1.3.00 : added a post-denoising stage (see "post")
###           fixed a bug with fixFFT3D and defined p clip
###           changed "very high" to be less insane
###           renamed fixFFT3D to fixband
###           cleaned some stuff
###           adapted to LSFmod v1.8
###
### v1.2.02 : changed default gpu=true to gpu=false (set gpu=true if you want speed!!!)
###           adapted to LSFmod v1.7
###
### v1.2.00 : adapted for LSFmod v1.6 & GradFun2DBmod v1.3
###           changed sharp int->bool (deleted sharp=1)
###
### v1.1.27 : fixed a bug in MCTemporalDenoisePP with chroma=false
###           adapted for LSFmod v1.5
###
### v1.1.26 : changed "flat" post-processing to use GradFun2DBmod
###           changed int->float for AGstr
###           fixed a small bug in AA
###
### v1.1.24 : fixed a chroma bug with PP
###
### v1.1.23 : adapted script for MvTools v2.3.0 (deleted MT & idx parameters)
###           changed MCTemporalDenoisePP's defaults (now the same as MCTD)
###           deleted safe parameter (slow & useless)
###           changed some minor things
###
### v1.0.03 : fixed a bug in MCTemporalDenoisePP
###
### v1.0.02 : changed defaults according to new LSFmod v1.2
###
### v1.0.01 : fixed a speed issue in MCTemporalDenoisePP (introduced in beta5)
###           changed documentation about MCTemporalDenoisePP
###
### v1.0.00 : first stable version
###           changed sharp=2, need LSFmod v1.0 !
###           fixed a bug introduced in beta5
###
### 1.0beta5: added new sharpening routine & deleted LSF (SHmode,Slimit,Sovershoot,Tlimit,Tovershoot)
###           changed some code (cosmetic)
###
### 1.0beta4: changed some descriptions & blksize default (submitted by Didée)
###           added auto blksize setting (16:HD & 8:SD, if you want speed use 16)
###
### 1.0beta3: all post-processing filters are now usable as a standalone function (MCTemporalDenoisePP)
###           added MC-antialiasing (see reduc parameter)
###
### 1.0beta2: fixed MOD16 restriction (effective only if deblock=true)
###           small speedup/fix with limit>0 and/or protect=true&maxdiff>0
###           added warning about GPU & MT in the documentation
###
### 1.0beta1: changed name Udegrain --> MCTemporalDenoise (to be more specific and avoid confusion with chroma)
###           added the choice to use MDegrain (since the chroma bug in MDegrain is fixed, +50% speed)
###           added twopass option to do the denoising job in two stage (very effective but slow)
###           added MT parameter for MVTools (be careful +- unstable)
###           added fixFFT3D parameter to prevent banding/posterize with limit=-1
###           added stabilize options (stabilize,maxr,TTstr)
###           added deblock options (deblock,useQED,quant1,quant2)
###           added new presets "light"(=low)/"heavy"(=medium) --> 5 presets (very high is 'a bit' extrem)
###           added check/warning for some settings
###           added thSAD2 & limit2 parameters for twopass=true
###           added overrange parameter for controling sharpness (useful for sharpening more than the source)
###           changed +- much code to implement twopass/MDegrain/MT, should be bug free (I hope) 
###           changed sharp->MVsharp & post->sharp
###           changed "light Smode4" --> LimitedSharpenFaster for sharp=2 (strength,Smode,overshoot)
###           reduced max radius to 6
###           optimized chroma in PP stage (+20% with all PP on)
###           fixed a bug with external p clip
###           fixed a bug in Star&Bright Point Protection with maxdiff>0
###           added YV12&MOD16 restriction
###           changed some settings due to new presets
###           restriction when using MT=true: bt=1 in FFT3Dgpu & plane=4 in MDegrainMulti (will be removed when fixed)
###
### v0.9.05a: changed version number: vX.XX branch -> v0.X.XX branch
###           added AA options (AA,useEEDI2,maxd,AAthr,method)
###           changed dering --> edgeclean: you can now use your own script
###           fixed bug with show=true
###           fixed bug with chroma in PP stage
###           fixed omission with chroma=false in MAnalyse
###           changed some settings
###           added changelog in .avsi
### 
### v0.9.01a: added adapt & SS for sharpening
###           added dering option (dering, DRGrad, DRGthr, RGmode)
###           added flat areas processing (flat, AGstr, GFthr, bias)
###           added modX(=blocksize) checking
###           changed some default settings
###           updated documentation in .avsi
### 
### v0.8.08c: fixed bug with chroma=false & interlaced=true
### 
### v0.8.08b: added mocomp contra-sharpening (basic contra-sharpening always usable)
### 
### v0.7.29a: added star & bright points protection
###           changed some default options
###           added more options for FFT3D
###           added YV12 checking
### 
### v0.7.25a: changed to chroma=false for light settings
###           added temporal limiter for sharpening
### 
### v0.7.24a: replaced blankclip by nop
### 
### v0.7.23a: first public release
###
####################################################################################################################################



function MCTemporalDenoise( clip i, int "radius", int "sigma", bool "twopass", bool "useTTmpSm", int "limit", int "limit2", int "post", bool "chroma", bool "interlaced",
\                           bool "sharp", bool "adapt", int "strength", int "SHmode", int "SHmethod", int "Slimit", int "Sovershoot", bool "Tlimit", int "Tovershoot",
\                           bool "protect", int "cutoff", int "threshold", int "maxdiff",
\                           bool "AA", bool "useEEDI2", float "reduc", int "maxd", int "AAthr", int "method",
\                           bool "deblock", bool "useQED", int "quant1", int "quant2",
\                           bool "edgeclean", int "ECrad", int "ECthr", string "ECmode",
\                           bool "stabilize", int "maxr", int "TTstr",
\                           bool "enhance", float "GFthr", float "AGstr", int "bias", int "temp",
\                           int "bwbh", int "owoh", int "blksize", int "overlap",
\                           bool "GPU", bool "fixband", int "bt", int "ncpu", int "precision",
\                           int "thSAD", int "thSAD2", int "thSCD1", int "thSCD2",
\                           bool "truemotion", bool "MVglobal", int "pel", int "pelsearch", int "search", int "searchparam", int "MVsharp", int "DCT",
\                           clip "p", string "settings",
\                           bool "show", int "screenW", int "screenH" )
{
### VERSION
version = "v1.4.20"
Assert(Is420(i) == True ? true : false, chr(10) + "This is not an YV12 clip ! Please convert color space to YV12 before using MCTemporalDenoise()" + chr(10))


### SETTINGS
settings     = Default(settings, "low")
settings_num =  -1
settings_num = (settings == "very low")  ? 0 : settings_num 
settings_num = (settings == "low")       ? 1 : settings_num
settings_num = (settings == "medium")    ? 2 : settings_num
settings_num = (settings == "high")      ? 3 : settings_num
settings_num = (settings == "very high") ? 4 : settings_num
Assert(settings_num >=0 ? true : false, chr(10) + "These settings do not exist !" + chr(10))

   function MCTD_Defaults(clip clp, int "settings_num", string "setting")
   {
   HD = (clp.width > 1024 || clp.height > 576) ? true : false

   ### DEFAULTS                     #          VERY LOW           #             LOW             #            MEDIUM           #            HIGH             #          VERY HIGH          #
   radius      = Select(settings_num,      1                      ,      2                      ,      3                      ,      2                      ,      3                      )
   sigma       = Select(settings_num,      2                      ,      4                      ,      8                      ,      12                     ,      16                     )
   twopass     = Select(settings_num,      false                  ,      false                  ,      false                  ,      true                   ,      true                   )
   useTTmpSm   = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   limit       = Select(settings_num,      -1                     ,      -1                     ,      -1                     ,      -1                     ,      0                      )
   limit2      = Select(settings_num,      -1                     ,      -1                     ,      -1                     ,      0                      ,      0                      )
   post        = Select(settings_num,      0                      ,      0                      ,      0                      ,      0                      ,      0                      )
   chroma      = Select(settings_num,      false                  ,      false                  ,      true                   ,      true                   ,      true                   )
   interlaced  = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )

   sharp       = Select(settings_num,      true                   ,      true                   ,      true                   ,      true                   ,      true                   )
   adapt       = Select(settings_num,      true                   ,      true                   ,      true                   ,      true                   ,      true                   )
   strength    = Select(settings_num,      100                    ,      100                    ,      150                    ,      200                    ,      200                    )
   SHmode      = Select(settings_num,      3                      ,      3                      ,      3                      ,      3                      ,      3                      )
   SHmethod    = Select(settings_num,      3                      ,      3                      ,      3                      ,      3                      ,      3                      )
   Slimit      = Select(settings_num,      0                      ,      0                      ,      0                      ,      0                      ,      0                      )
   Sovershoot  = Select(settings_num,      0                      ,      0                      ,      0                      ,      0                      ,      0                      )
   Tlimit      = Select(settings_num,      true                   ,      true                   ,      true                   ,      true                   ,      true                   )
   Tovershoot  = Select(settings_num,      2                      ,      2                      ,      1                      ,      0                      ,      0                      )

   protect     = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   cutoff      = Select(settings_num,      64                     ,      64                     ,      64                     ,      64                     ,      64                     )
   threshold   = Select(settings_num,      16                     ,      16                     ,      16                     ,      16                     ,      16                     )
   maxdiff     = Select(settings_num,      4                      ,      4                      ,      4                      ,      4                      ,      4                      )

   AA          = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   useEEDI2    = Select(settings_num,      true                   ,      true                   ,      false                  ,      false                  ,      false                  )
   reduc       = Select(settings_num,      0.5                    ,      0.5                    ,      0.5                    ,      0.5                    ,      0.5                    )
   maxd        = Select(settings_num,      24                     ,      24                     ,      48                     ,      48                     ,      48                     )
   AAthr       = Select(settings_num,      64                     ,      32                     ,      32                     ,      16                     ,      16                     )
   method      = Select(settings_num,      2                      ,      1                      ,      1                      ,      0                      ,      0                      )

   deblock     = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   useQED      = Select(settings_num,      true                   ,      true                   ,      true                   ,      false                  ,      false                  )
   quant1      = Select(settings_num,      10                     ,      20                     ,      30                     ,      30                     ,      40                     )
   quant2      = Select(settings_num,      20                     ,      40                     ,      60                     ,      60                     ,      80                     )

   edgeclean   = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   ECrad       = Select(settings_num,      1                      ,      2                      ,      3                      ,      4                      ,      5                      )
   ECthr       = Select(settings_num,      64                     ,      32                     ,      32                     ,      16                     ,      16                     )
   ECmode      = Select(settings_num,      "Removegrain(2,0)"     ,      "Removegrain(2,0)"     ,      "Removegrain(2,2)"     ,      "Removegrain(4,4)"     ,      "Removegrain(4,4)"     )

   stabilize   = Select(settings_num,      false                  ,      false                  ,      false                  ,      true                   ,      true                   )
   maxr        = Select(settings_num,      1                      ,      1                      ,      2                      ,      2                      ,      2                      )
   TTstr       = Select(settings_num,      1                      ,      1                      ,      1                      ,      2                      ,      2                      )

   enhance     = Select(settings_num,      true                   ,      true                   ,      false                  ,      false                  ,      false                  )
   GFthr       = Select(settings_num,      1.1                    ,      1.2                    ,      1.4                    ,      1.6                    ,      1.8                    )
   AGstr       = Select(settings_num,      0.6                    ,      0.8                    ,      1.2                    ,      1.8                    ,      2.4                    )
   bias        = Select(settings_num,      64                     ,      64                     ,      64                     ,      64                     ,      64                     )
   temp        = Select(settings_num,      50                     ,      50                     ,      50                     ,      50                     ,      50                     )

   bwbh        = Select(settings_num,      HD?16:8                ,      HD?16:8                ,      HD?16:8                ,      HD?16:8                ,      HD?16:8                )
   owoh        = Select(settings_num,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                )
   blksize     = Select(settings_num,      HD?16:8                ,      HD?16:8                ,      HD?16:8                ,      HD?16:8                ,      HD?16:8                )
   overlap     = Select(settings_num,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                ,      HD? 8:4                )

   GPU         = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   fixband     = Select(settings_num,      true                   ,      true                   ,      true                   ,      true                   ,      true                   )
   bt          = Select(settings_num,      1                      ,      3                      ,      3                      ,      3                      ,      4                      )
   ncpu        = Select(settings_num,      1                      ,      1                      ,      1                      ,      1                      ,      1                      )
   precision   = Select(settings_num,      2                      ,      2                      ,      2                      ,      2                      ,      2                      )

   thSAD       = Select(settings_num,      200                    ,      300                    ,      400                    ,      500                    ,      600                    )
   thSAD2      = Select(settings_num,      200                    ,      300                    ,      400                    ,      500                    ,      600                    )
   thSCD1      = Select(settings_num,      200                    ,      300                    ,      400                    ,      500                    ,      600                    )
   thSCD2      = Select(settings_num,      90                     ,      100                    ,      100                    ,      130                    ,      130                    )

   truemotion  = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   MVglobal    = Select(settings_num,      true                   ,      true                   ,      true                   ,      true                   ,      true                   )
   pel         = Select(settings_num,      1                      ,      2                      ,      2                      ,      2                      ,      2                      )
   pelsearch   = Select(settings_num,      1                      ,      2                      ,      2                      ,      2                      ,      2                      )
   search      = Select(settings_num,      2                      ,      2                      ,      2                      ,      2                      ,      2                      )
   searchparam = Select(settings_num,      2                      ,      2                      ,      2                      ,      2                      ,      2                      )
   MVsharp     = Select(settings_num,      2                      ,      2                      ,      2                      ,      1                      ,      0                      )
   DCT         = Select(settings_num,      0                      ,      0                      ,      0                      ,      0                      ,      0                      )

   show        = Select(settings_num,      false                  ,      false                  ,      false                  ,      false                  ,      false                  )
   screenW     = Select(settings_num,      1280                   ,      1280                   ,      1280                   ,      1280                   ,      1280                   )
   screenH     = Select(settings_num,      1024                   ,      1024                   ,      1024                   ,      1024                   ,      1024                   )

   return Eval(setting)
   }

radius      = default(radius,       MCTD_Defaults( i, settings_num, "radius"      ) )
sigma       = default(sigma,        MCTD_Defaults( i, settings_num, "sigma"       ) )
twopass     = default(twopass,      MCTD_Defaults( i, settings_num, "twopass"     ) )
useTTmpSm   = default(useTTmpSm,    MCTD_Defaults( i, settings_num, "useTTmpSm"   ) )
limit       = default(limit,        MCTD_Defaults( i, settings_num, "limit"       ) )
limit2      = default(limit2,       MCTD_Defaults( i, settings_num, "limit2"      ) )
post        = default(post,         MCTD_Defaults( i, settings_num, "post"        ) )
chroma      = default(chroma,       MCTD_Defaults( i, settings_num, "chroma"      ) )
interlaced  = default(interlaced,   MCTD_Defaults( i, settings_num, "interlaced"  ) )

sharp       = default(sharp,        MCTD_Defaults( i, settings_num, "sharp"       ) )
adapt       = default(adapt,        MCTD_Defaults( i, settings_num, "adapt"       ) )
strength    = default(strength,     MCTD_Defaults( i, settings_num, "strength"    ) )
SHmode      = default(SHmode,       MCTD_Defaults( i, settings_num, "SHmode"      ) )
SHmethod    = default(SHmethod,     MCTD_Defaults( i, settings_num, "SHmethod"    ) )
Slimit      = default(Slimit,       MCTD_Defaults( i, settings_num, "Slimit"      ) )
Sovershoot  = default(Sovershoot,   MCTD_Defaults( i, settings_num, "Sovershoot"  ) )
Tlimit      = default(Tlimit,       MCTD_Defaults( i, settings_num, "Tlimit"      ) )
Tovershoot  = default(Tovershoot,   MCTD_Defaults( i, settings_num, "Tovershoot"  ) )

protect     = default(protect,      MCTD_Defaults( i, settings_num, "protect"     ) )
cutoff      = default(cutoff,       MCTD_Defaults( i, settings_num, "cutoff"      ) )
threshold   = default(threshold,    MCTD_Defaults( i, settings_num, "threshold"   ) )
maxdiff     = default(maxdiff,      MCTD_Defaults( i, settings_num, "maxdiff"     ) )

AA          = default(AA,           MCTD_Defaults( i, settings_num, "AA"          ) )
useEEDI2    = default(useEEDI2,     MCTD_Defaults( i, settings_num, "useEEDI2"    ) )
reduc       = default(reduc,        MCTD_Defaults( i, settings_num, "reduc"       ) )
maxd        = default(maxd,         MCTD_Defaults( i, settings_num, "maxd"        ) )
AAthr       = default(AAthr,        MCTD_Defaults( i, settings_num, "AAthr"       ) )
method      = default(method,       MCTD_Defaults( i, settings_num, "method"      ) )

deblock     = default(deblock,      MCTD_Defaults( i, settings_num, "deblock"     ) )
useQED      = default(useQED,       MCTD_Defaults( i, settings_num, "useQED"      ) )
quant1      = default(quant1,       MCTD_Defaults( i, settings_num, "quant1"      ) )
quant2      = default(quant2,       MCTD_Defaults( i, settings_num, "quant2"      ) )

edgeclean   = default(edgeclean,    MCTD_Defaults( i, settings_num, "edgeclean"   ) )
ECrad       = default(ECrad,        MCTD_Defaults( i, settings_num, "ECrad"       ) )
ECthr       = default(ECthr,        MCTD_Defaults( i, settings_num, "ECthr"       ) )
ECmode      = default(ECmode,       MCTD_Defaults( i, settings_num, "ECmode"      ) )

stabilize   = default(stabilize,    MCTD_Defaults( i, settings_num, "stabilize"   ) )
maxr        = default(maxr,         MCTD_Defaults( i, settings_num, "maxr"        ) )
TTstr       = default(TTstr,        MCTD_Defaults( i, settings_num, "TTstr"       ) )

enhance     = default(enhance,      MCTD_Defaults( i, settings_num, "enhance"     ) )
GFthr       = default(GFthr,        MCTD_Defaults( i, settings_num, "GFthr"       ) )
AGstr       = default(AGstr,        MCTD_Defaults( i, settings_num, "AGstr"       ) )
bias        = default(bias,         MCTD_Defaults( i, settings_num, "bias"        ) )
temp        = default(temp,         MCTD_Defaults( i, settings_num, "temp"        ) )

bwbh        = default(bwbh,         MCTD_Defaults( i, settings_num, "bwbh"        ) )
owoh        = default(owoh,         MCTD_Defaults( i, settings_num, "owoh"        ) )
blksize     = default(blksize,      MCTD_Defaults( i, settings_num, "blksize"     ) )
overlap     = default(overlap,      MCTD_Defaults( i, settings_num, "overlap"     ) )

GPU         = default(GPU,          MCTD_Defaults( i, settings_num, "GPU"         ) )
fixband     = default(fixband,      MCTD_Defaults( i, settings_num, "fixband"     ) )
bt          = default(bt,           MCTD_Defaults( i, settings_num, "bt"          ) )
ncpu        = default(ncpu,         MCTD_Defaults( i, settings_num, "ncpu"        ) )
precision   = default(precision,    MCTD_Defaults( i, settings_num, "precision"   ) )

thSAD       = default(thSAD,        MCTD_Defaults( i, settings_num, "thSAD"       ) )
thSAD2      = default(thSAD2,       MCTD_Defaults( i, settings_num, "thSAD2"      ) )
thSCD1      = default(thSCD1,       MCTD_Defaults( i, settings_num, "thSCD1"      ) )
thSCD2      = default(thSCD2,       MCTD_Defaults( i, settings_num, "thSCD2"      ) )

truemotion  = default(truemotion,   MCTD_Defaults( i, settings_num, "truemotion"  ) )
MVglobal    = default(MVglobal,     MCTD_Defaults( i, settings_num, "MVglobal"    ) )
pel         = default(pel,          MCTD_Defaults( i, settings_num, "pel"         ) )
pelsearch   = default(pelsearch,    MCTD_Defaults( i, settings_num, "pelsearch"   ) )
search      = default(search,       MCTD_Defaults( i, settings_num, "search"      ) )
searchparam = default(searchparam,  MCTD_Defaults( i, settings_num, "searchparam" ) )
MVsharp     = default(MVsharp,      MCTD_Defaults( i, settings_num, "MVsharp"     ) )
DCT         = default(DCT,          MCTD_Defaults( i, settings_num, "DCT"         ) )

show        = default(show,         MCTD_Defaults( i, settings_num, "show"        ) )
screenW     = default(screenW,      MCTD_Defaults( i, settings_num, "screenW"     ) )
screenH     = default(screenH,      MCTD_Defaults( i, settings_num, "screenH"     ) )

show_p      = defined(p) ? "external" : "internal"


### WARNING
Assert((radius>=1&&radius<=6)         ? true : false, chr(10) + "'radius' have not a correct value! [0-6]" + chr(10))
Assert((limit>=-1&&limit<=255)        ? true : false, chr(10) + "'limit' have not a correct value! [-1,0,...255]" + chr(10))
Assert((limit2>=-1&&limit2<=255)      ? true : false, chr(10) + "'limit' have not a correct value! [-1,0,...255]" + chr(10))
Assert((Tovershoot>=0)                ? true : false, chr(10) + "'Tovershoot' have not a correct value! [>=0]" + chr(10))
Assert((cutoff>=0&&cutoff<=255)       ? true : false, chr(10) + "'cutoff' have not a correct value! [0,...,255]" + chr(10))
Assert((threshold>=0&&threshold<=127) ? true : false, chr(10) + "'threshold' have not a correct value! [0,...,127]" + chr(10))
Assert((maxdiff>=0&&maxdiff<=255)     ? true : false, chr(10) + "'maxdiff' have not a correct value! [0,...,255]" + chr(10))
Assert((reduc>=-1&&reduc<=1.0)        ? true : false, chr(10) + "'reduc' have not a correct value! [-1,0.0,...,1.0]" + chr(10))
Assert((AAthr>=0&&AAthr<=255)         ? true : false, chr(10) + "'AAthr' have not a correct value! [0,...,255]" + chr(10))
Assert((method>=0&&method<=2)         ? true : false, chr(10) + "'method' have not a correct value! [0,1,2]" + chr(10))
Assert((ECrad>=1)                     ? true : false, chr(10) + "'ECrad' have not a correct value! [>=1]" + chr(10))
Assert((ECthr>=0&&ECthr<=255)         ? true : false, chr(10) + "'ECthr' have not a correct value! [0,...,255]" + chr(10))


### INPUT
i   = (interlaced==true) ? i.separatefields() : i

mod = (bwbh>=blksize) ? bwbh : blksize
xi  = i.width()
xf  = ceil(xi/float(mod))*mod - xi + mod
xn  = int(xi+xf)
yi  = i.height()
yf  = ceil(yi/float(mod))*mod - yi + mod
yn  = int(yi+yf)
i   = i.pointresize(xn,yn,-xf/2,-yf/2,xn,yn)

m4 = ((int(xi/4.0)*4==xi)&&(int(yi/4.0)*4==yi)) ? true : false
Assert( m4 ? true : false, chr(10) + "This is not a MOD4 clip! Please use crop() or addborders() before using MCTemporalDenoise()" + chr(10))

m16 = (deblock==false) ? true : ((int(xi/16.0)*16==xi)&&(int(yi/16.0)*16==yi)) ? true : false
Assert( m16 ? true : false, chr(10) + "This is not a MOD16 clip! Please use crop() or addborders() before using MCTemporalDenoise() with deblock=true" + chr(10))


### PREFILTERING
p = defined(p)                   ? p.pointresize(xn,yn,-xf/2,-yf/2,xn,yn)
\ : (sigma==0)                   ? i
\ : (GPU==false&&fixband==false) ? i.FFT3Dfilter(sigma=sigma*0.8,sigma2=sigma*0.6,sigma3=sigma*0.4,sigma4=sigma*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,ncpu=ncpu)
\ : (GPU==true &&fixband==false) ? i.FFT3Dgpu(sigma=sigma*0.8,sigma2=sigma*0.6,sigma3=sigma*0.4,sigma4=sigma*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,precision=precision)
\ : (GPU==false&&fixband==true ) ? i.FFT3Dfilter(sigma=sigma*0.8,sigma2=sigma*0.6,sigma3=sigma*0.4,sigma4=sigma*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,ncpu=ncpu).gradfun2db(thr=1.0+sigma*0.05)
\ :                                i.FFT3Dgpu(sigma=sigma*0.8,sigma2=sigma*0.6,sigma3=sigma*0.4,sigma4=sigma*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,precision=precision).gradfun2db(thr=1.0+sigma*0.05)


### DEBLOCKING
d  = (deblock==false) ? i
\  : (useQED==true)   ? i.crop(xf/2,yf/2,-xf/2,-yf/2).deblock_QED(quant1=quant1,quant2=quant2,uv=chroma?3:2).pointresize(xn,yn,-xf/2,-yf/2,xn,yn)
\  :                    i.crop(xf/2,yf/2,-xf/2,-yf/2).deblock(quant=int(quant1*0.5+quant2*0.5)).pointresize(xn,yn,-xf/2,-yf/2,xn,yn)

### PREPARING
nullclip = Blankclip()

pMVS = p.MSuper(hpad=0,vpad=0,pel=pel,sharp=MVsharp,chroma=chroma)

f1v = (radius<1) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=1, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
f2v = (radius<2) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=2, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
f3v = (radius<3) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=3, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
f4v = (radius<4) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=4, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
f5v = (radius<5) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=5, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
f6v = (radius<6) ? nullclip
\   :              pMVS.MAnalyse(isb=false, delta=6, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b1v = (radius<1) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=1, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b2v = (radius<2) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=2, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b3v = (radius<3) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=3, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b4v = (radius<4) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=4, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b5v = (radius<5) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=5, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)
b6v = (radius<6) ? nullclip
\   :              pMVS.MAnalyse(isb=true, delta=6, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=chroma)

   function MCTD_TTSM(clip "i", clip "iMVS", clip "SAD_f1m", clip "SAD_b1m", clip "f1c", clip "b1c",
   \                  clip "f1v", clip "f2v", clip "f3v", clip "f4v", clip "f5v", clip "f6v", clip "b1v", clip "b2v", clip "b3v", clip "b4v", clip "b5v", clip "b6v",
   \                  int "radius", bool "chroma", int "thSAD", int "thSCD1", int "thSCD2")
   {
   nullclip = BlankClip()

   SAD_f2m = (radius>=2) ? i.MMask(f2v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_f3m = (radius>=3) ? i.MMask(f3v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_f4m = (radius>=4) ? i.MMask(f4v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_f5m = (radius>=5) ? i.MMask(f5v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_f6m = (radius>=6) ? i.MMask(f6v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_b2m = (radius>=2) ? i.MMask(b2v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_b3m = (radius>=3) ? i.MMask(b3v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_b4m = (radius>=4) ? i.MMask(b4v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_b5m = (radius>=5) ? i.MMask(b5v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip
   SAD_b6m = (radius>=6) ? i.MMask(b6v, kind=1, ml=thSAD, gamma=0.999, Ysc=255) : nullclip

   b     = BlankClip(i,color_yuv=$008080)
   SAD_m = (radius==6) ? interleave(SAD_f6m, SAD_f5m, SAD_f4m, SAD_f3m, SAD_f2m, SAD_f1m, b, SAD_b1m, SAD_b2m, SAD_b3m, SAD_b4m, SAD_b5m, SAD_b6m)
   \     : (radius==5) ? interleave(         SAD_f5m, SAD_f4m, SAD_f3m, SAD_f2m, SAD_f1m, b, SAD_b1m, SAD_b2m, SAD_b3m, SAD_b4m, SAD_b5m         )
   \     : (radius==4) ? interleave(                  SAD_f4m, SAD_f3m, SAD_f2m, SAD_f1m, b, SAD_b1m, SAD_b2m, SAD_b3m, SAD_b4m                  )
   \     : (radius==3) ? interleave(                           SAD_f3m, SAD_f2m, SAD_f1m, b, SAD_b1m, SAD_b2m, SAD_b3m                           )
   \     : (radius==2) ? interleave(                                    SAD_f2m, SAD_f1m, b, SAD_b1m, SAD_b2m                                    )
   \     : (radius==1) ? interleave(                                             SAD_f1m, b, SAD_b1m                                             )
   \     : nullclip

   b2c = (radius>=2)  ? i.MCompensate(iMVS, b2v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   b3c = (radius>=3)  ? i.MCompensate(iMVS, b3v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   b4c = (radius>=4)  ? i.MCompensate(iMVS, b4v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   b5c = (radius>=5)  ? i.MCompensate(iMVS, b5v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   b6c = (radius>=6)  ? i.MCompensate(iMVS, b6v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   f2c = (radius>=2)  ? i.MCompensate(iMVS, f2v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   f3c = (radius>=3)  ? i.MCompensate(iMVS, f3v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   f4c = (radius>=4)  ? i.MCompensate(iMVS, f4v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   f5c = (radius>=5)  ? i.MCompensate(iMVS, f5v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   f6c = (radius>=6)  ? i.MCompensate(iMVS, f6v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2) : nullclip
   
   c = (radius==6) ? interleave(f6c, f5c, f4c, f3c, f2c, f1c, i, b1c, b2c, b3c, b4c, b5c, b6c)
   \ : (radius==5) ? interleave(     f5c, f4c, f3c, f2c, f1c, i, b1c, b2c, b3c, b4c, b5c     )
   \ : (radius==4) ? interleave(          f4c, f3c, f2c, f1c, i, b1c, b2c, b3c, b4c          )
   \ : (radius==3) ? interleave(               f3c, f2c, f1c, i, b1c, b2c, b3c               )
   \ : (radius==2) ? interleave(                    f2c, f1c, i, b1c, b2c                    )
   \ : (radius==1) ? interleave(                         f1c, i, b1c                         )
   \ : nullclip
return c
   
   sm = c.TTempSmooth(radius,255,255,1,1,strength=radius+1,pfclip=SAD_m,fp=false,scthresh=99.9)
   sm = sm.SelectEvery(1+radius*2,radius)
   
   return sm
   }

   function MCTD_MVD(clip "i", clip "iMVS",
   \                 clip "f1v", clip "f2v", clip "f3v", clip "f4v", clip "f5v", clip "f6v", clip "b1v", clip "b2v", clip "b3v", clip "b4v", clip "b5v", clip "b6v",
   \                 int "radius", bool "chroma", int "thSAD", int "thSCD1", int "thSCD2")
   {
   nullclip = BlankClip()

   mv12  = (radius==4) ? i.MDegrain2(iMVS, b1v, f1v, b2v, f2v,           thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0) : nullclip
   mv34  = (radius==4) ? i.MDegrain2(iMVS, b3v, f3v, b4v, f4v,           thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0) : nullclip
   mv45  = (radius==5) ? i.MDegrain2(iMVS, b4v, f4v, b5v, f5v,           thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0) : nullclip
   mv123 = (radius>=5) ? i.MDegrain3(iMVS, b1v, f1v, b2v, f2v, b3v, f3v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0) : nullclip
   mv456 = (radius==6) ? i.MDegrain3(iMVS, b4v, f4v, b5v, f5v, b6v, f6v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0) : nullclip
   
   sm = (radius==1) ? i.MDegrain1(iMVS, b1v, f1v,                     thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0)
   \  : (radius==2) ? i.MDegrain2(iMVS, b1v, f1v, b2v, f2v,           thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0)
   \  : (radius==3) ? i.MDegrain3(iMVS, b1v, f1v, b2v, f2v, b3v, f3v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=chroma?4:0)
   \  : (radius==4) ? Merge(mv12,  mv34,  0.4444)
   \  : (radius==5) ? Merge(mv123, mv45,  0.4545)
   \  :               Merge(mv123, mv456, 0.4615)
   
   return sm
   }

dMVS    = d.MSuper(hpad=0,vpad=0,pel=pel,sharp=MVsharp,chroma=chroma)
SAD_f1m = d.MMask(f1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)
SAD_b1m = d.MMask(b1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)
f1c     = d.MCompensate(dMVS, f1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
b1c     = d.MCompensate(dMVS, b1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)


### DENOISING: FIRST PASS
sm = (useTTmpSm==true) ? MCTD_TTSM(i=d, iMVS=dMVS, SAD_f1m=SAD_f1m, SAD_b1m=SAD_b1m, f1c=f1c, b1c=b1c,
\                                  f1v=f1v, f2v=f2v, f3v=f3v, f4v=f4v, f5v=f5v, f6v=f6v, b1v=b1v, b2v=b2v, b3v=b3v, b4v=b4v, b5v=b5v, b6v=b6v,
\                                  radius=radius, chroma=chroma, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
\  :                     MCTD_MVD(i=d, iMVS=dMVS,
\                                 f1v=f1v, f2v=f2v, f3v=f3v, f4v=f4v, f5v=f5v, f6v=f6v, b1v=b1v, b2v=b2v, b3v=b3v, b4v=b4v, b5v=b5v, b6v=b6v,
\                                 radius=radius, chroma=chroma, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)

chr41 = chroma?4:1
chr31 = chroma?3:1
chr21 = chroma?2:1
pD    = mt_makediff(i,p,U=chr31,V=chr31)

smD = mt_makediff(i,sm,U=chr31,V=chr31)
DD  = (limit==-1) ? mt_lutxy(pD,smD,"x 128 - abs y 128 - abs < x y ?",U=chr31,V=chr31)
\   : (limit>0)   ? mt_lutxy(sm,i,"x y - abs "+string(limit)+" <= x x y - 0 < y "+string(limit)+" - y "+string(limit)+" + ? ?",U=chr31,V=chr31)
\   :               sm
smL = (limit==-1) ? i.mt_makediff(DD,U=chr31,V=chr31) : DD


### DENOISING: SECOND PASS
smLMVS = (twopass==true) ? smL.MSuper(hpad=0,vpad=0,pel=pel,sharp=MVsharp,chroma=chroma) : nullclip

sm = (twopass==false)  ? sm
\  : (useTTmpSm==true) ? MCTD_TTSM(i=smL, iMVS=smLMVS, SAD_f1m=SAD_f1m, SAD_b1m=SAD_b1m, f1c=f1c, b1c=b1c,
\                                  f1v=f1v, f2v=f2v, f3v=f3v, f4v=f4v, f5v=f5v, f6v=f6v, b1v=b1v, b2v=b2v, b3v=b3v, b4v=b4v, b5v=b5v, b6v=b6v,
\                                  radius=radius, chroma=chroma, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
\  :                     MCTD_MVD(i=smL, iMVS=smLMVS,
\                                 f1v=f1v, f2v=f2v, f3v=f3v, f4v=f4v, f5v=f5v, f6v=f6v, b1v=b1v, b2v=b2v, b3v=b3v, b4v=b4v, b5v=b5v, b6v=b6v,
\                                 radius=radius, chroma=chroma, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)

smD = mt_makediff(i,sm,U=chr31,V=chr31)
DD  = (twopass==true&&limit2==-1) ? mt_lutxy(pD,smD,"x 128 - abs y 128 - abs < x y ?",U=chr31,V=chr31)
\   : (twopass==true&&limit2>0)   ? mt_lutxy(sm,i,"x y - abs "+string(limit2)+" <= x x y - 0 < y "+string(limit2)+" - y "+string(limit2)+" + ? ?",U=chr31,V=chr31)
\   :                               sm
smL = (twopass==false) ? smL
\   : (limit2==-1)     ? i.mt_makediff(DD,U=chr31,V=chr31)
\   :                    DD


### POST-DENOISING: FFT3D
smP = (post==0)    ? smL
\   : (GPU==false) ? smL.FFT3Dfilter(sigma=post*0.8,sigma2=post*0.6,sigma3=post*0.4,sigma4=post*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,ncpu=ncpu)
\   :                smL.FFT3Dgpu(sigma=post*0.8,sigma2=post*0.6,sigma3=post*0.4,sigma4=post*0.2,bw=bwbh,bh=bwbh,ow=owoh,oh=owoh,plane=chroma?4:0,bt=bt,precision=precision)
smP = (post==0)        ? smP
\   : (fixband==false) ? smP
\   :                    mt_makediff(smL,mt_makediff(smL,smP,U=chr31,V=chr31).gradfun2DB(thr=1.0+post*0.5),U=chr31,V=chr31)


### POST-PROCESSING
   function MCTemporalDenoisePP( clip denoised, clip "source",
   \                             bool "sharp", bool "adapt", int "strength", int "SHmode", int "SHmethod", int "Slimit", int "Sovershoot", bool "Tlimit", int "Tovershoot",
   \                             bool "protect", int "cutoff", int "threshold", int "maxdiff",
   \                             bool "AA", bool "useEEDI2", float "reduc", int "maxd", int "AAthr", int "method",
   \                             bool "deblock", bool "useQED", int "quant1", int "quant2",          
   \                             bool "edgeclean", int "ECrad", int "ECthr", string "ECmode",
   \                             bool "stabilize", int "maxr", int "TTstr",
   \                             bool "enhance", float "GFthr", float "AGstr", int "bias", int "temp",
   \                             bool "chroma", string "settings",
   \                             clip "f1v", clip "b1v", clip "f1c", clip "b1c", clip "SAD_f1m", clip "SAD_b1m", bool "internal" )
   {
   ### SETTINGS
   settings     = Default(settings, "low")
   settings_num =  -1
   settings_num = (settings == "very low")  ? 0 : settings_num 
   settings_num = (settings == "low")       ? 1 : settings_num
   settings_num = (settings == "medium")    ? 2 : settings_num
   settings_num = (settings == "high")      ? 3 : settings_num
   settings_num = (settings == "very high") ? 4 : settings_num
   Assert(settings_num >=0 ? true : false, chr(10) + "These settings do not exist !" + chr(10))

   sharp       = default(sharp,        false                                                  )
   adapt       = default(adapt,        MCTD_Defaults( denoised, settings_num, "adapt"       ) )
   strength    = default(strength,     MCTD_Defaults( denoised, settings_num, "strength"    ) )
   SHmode      = default(SHmode,       MCTD_Defaults( denoised, settings_num, "SHmode"      ) )
   SHmethod    = default(SHmethod,     MCTD_Defaults( denoised, settings_num, "SHmethod"    ) )
   Slimit      = default(Slimit,       MCTD_Defaults( denoised, settings_num, "Slimit"      ) )
   Sovershoot  = default(Sovershoot,   MCTD_Defaults( denoised, settings_num, "Sovershoot"  ) )
   Tlimit      = default(Tlimit,       MCTD_Defaults( denoised, settings_num, "Tlimit"      ) )
   Tovershoot  = default(Tovershoot,   MCTD_Defaults( denoised, settings_num, "Tovershoot"  ) )

   protect     = default(protect,      false                                                  )
   cutoff      = default(cutoff,       MCTD_Defaults( denoised, settings_num, "cutoff"      ) )
   threshold   = default(threshold,    MCTD_Defaults( denoised, settings_num, "threshold"   ) )
   maxdiff     = default(maxdiff,      MCTD_Defaults( denoised, settings_num, "maxdiff"     ) )

   AA          = default(AA,           false                                                  )
   useEEDI2    = default(useEEDI2,     MCTD_Defaults( denoised, settings_num, "useEEDI2"    ) )
   reduc       = default(reduc,        MCTD_Defaults( denoised, settings_num, "reduc"       ) )
   maxd        = default(maxd,         MCTD_Defaults( denoised, settings_num, "maxd"        ) )
   AAthr       = default(AAthr,        MCTD_Defaults( denoised, settings_num, "AAthr"       ) )
   method      = default(method,       MCTD_Defaults( denoised, settings_num, "method"      ) )

   deblock     = default(deblock,      false                                                  )
   useQED      = default(useQED,       MCTD_Defaults( denoised, settings_num, "useQED"      ) )
   quant1      = default(quant1,       MCTD_Defaults( denoised, settings_num, "quant1"      ) )
   quant2      = default(quant2,       MCTD_Defaults( denoised, settings_num, "quant2"      ) )

   edgeclean   = default(edgeclean,    false                                                  )
   ECrad       = default(ECrad,        MCTD_Defaults( denoised, settings_num, "ECrad"       ) )
   ECthr       = default(ECthr,        MCTD_Defaults( denoised, settings_num, "ECthr"       ) )
   ECmode      = default(ECmode,       MCTD_Defaults( denoised, settings_num, "ECmode"      ) )

   stabilize   = default(stabilize,    false                                                  )
   maxr        = default(maxr,         MCTD_Defaults( denoised, settings_num, "maxr"        ) )
   TTstr       = default(TTstr,        MCTD_Defaults( denoised, settings_num, "TTstr"       ) )

   enhance     = default(enhance,      false                                                  )
   GFthr       = default(GFthr,        MCTD_Defaults( denoised, settings_num, "GFthr"       ) )
   AGstr       = default(AGstr,        MCTD_Defaults( denoised, settings_num, "AGstr"       ) )
   bias        = default(bias,         MCTD_Defaults( denoised, settings_num, "bias"        ) )
   temp        = default(temp,         MCTD_Defaults( denoised, settings_num, "temp"        ) )

   chroma      = default(chroma,       MCTD_Defaults( denoised, settings_num, "chroma"      ) )
   internal    = default(internal,     false                                                  )

   Assert((Tovershoot>=0)                ? true : false, chr(10) + "'Tovershoot' have not a correct value! [>=0]" + chr(10))
   Assert((cutoff>=0&&cutoff<=255)       ? true : false, chr(10) + "'cutoff' have not a correct value! [0,...,255]" + chr(10))
   Assert((threshold>=0&&threshold<=127) ? true : false, chr(10) + "'threshold' have not a correct value! [0,...,127]" + chr(10))
   Assert((maxdiff>=0&&maxdiff<=255)     ? true : false, chr(10) + "'maxdiff' have not a correct value! [0,...,255]" + chr(10))
   Assert((reduc>=-1&&reduc<=1.0)        ? true : false, chr(10) + "'reduc' have not a correct value! [-1,0.0,...,1.0]" + chr(10))
   Assert((AAthr>=0&&AAthr<=255)         ? true : false, chr(10) + "'AAthr' have not a correct value! [0,...,255]" + chr(10))
   Assert((method>=0&&method<=2)         ? true : false, chr(10) + "'method' have not a correct value! [0,1,2]" + chr(10))
   Assert((ECrad>=1)                     ? true : false, chr(10) + "'ECrad' have not a correct value! [>=1]" + chr(10))
   Assert((ECthr>=0&&ECthr<=255)         ? true : false, chr(10) + "'ECthr' have not a correct value! [0,...,255]" + chr(10))

   Assert((sharp==true||protect==true)&&defined(source)==false ? false : true, chr(10) + "Sharp & Protect need 'source' input in MCTemporalDenoisePP !" + chr(10))


   ### WARNING
   Assert(is420(denoised) == True ? true : false, chr(10) + "This is not an YV12 clip ! Please convert color space to YV12 before using MCTemporalDenoisePP()" + chr(10))

   w = denoised.width()
   h = denoised.height()

   m4 = (internal==true) ? true : ((int(w/4.0)*4==w)&&(int(h/4.0)*4==h)) ? true : false
   Assert( m4 ? true : false, chr(10) + "This is not a MOD4 clip! Please use crop() or addborders() before using MCTemporalDenoisePP()" + chr(10))

   m16 = (internal==true) ? true : (deblock==false) ? true : ((int(w/16.0)*16==w)&&(int(h/16.0)*16==h)) ? true : false
   Assert( m16 ? true : false, chr(10) + "This is not a MOD16 clip! Please use crop() or addborders() before using MCTemporalDenoisePP() with deblock=true" + chr(10))


   ### EXTERNAL
   blksize     = MCTD_Defaults( denoised, settings_num, "blksize"     )
   overlap     = MCTD_Defaults( denoised, settings_num, "overlap"     )
   truemotion  = MCTD_Defaults( denoised, settings_num, "truemotion"  )
   MVglobal    = MCTD_Defaults( denoised, settings_num, "MVglobal"    )
   pel         = MCTD_Defaults( denoised, settings_num, "pel"         )
   pelsearch   = MCTD_Defaults( denoised, settings_num, "pelsearch"   )
   search      = MCTD_Defaults( denoised, settings_num, "search"      )
   searchparam = MCTD_Defaults( denoised, settings_num, "searchparam" )
   MVsharp     = MCTD_Defaults( denoised, settings_num, "MVsharp"     )
   DCT         = MCTD_Defaults( denoised, settings_num, "DCT"         )
   thSAD       = MCTD_Defaults( denoised, settings_num, "thSAD"       )
   thSCD1      = MCTD_Defaults( denoised, settings_num, "thSCD1"      )
   thSCD2      = MCTD_Defaults( denoised, settings_num, "thSCD2"      )

   denoisedMVS = (internal==true) ? denoised : denoised.MSuper(hpad=0,vpad=0,pel=pel,sharp=MVsharp,chroma=false)
   f1v         = (internal==true) ? f1v : MAnalyse(denoisedMVS, isb=false, delta=1, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=false)
   b1v         = (internal==true) ? b1v : MAnalyse(denoisedMVS, isb=true,  delta=1, truemotion=truemotion, blksize=blksize, overlap=overlap, pelsearch=pelsearch, search=search, searchparam=searchparam, DCT=DCT, global=MVglobal, chroma=false)
   f1c         = (internal==true) ? f1c
   \           : defined(source)  ? MCompensate(source  , denoisedMVS, f1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
   \           :                    MCompensate(denoised, denoisedMVS, f1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
   b1c         = (internal==true) ? b1c
   \           : defined(source)  ? MCompensate(source  , denoisedMVS, b1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
   \           :                    MCompensate(denoised, denoisedMVS, b1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2)
   SAD_f1m     = (internal==true) ? SAD_f1m 
   \           : defined(source)  ? MMask(source,   f1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)
   \           :                    MMask(denoised, f1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)
   SAD_b1m     = (internal==true) ? SAD_b1m 
   \           : defined(source)  ? MMask(source,   b1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)
   \           :                    MMask(denoised, b1v, kind=1, ml=thSAD, gamma=0.999, Ysc=255)

   denoised = (internal==true) ? denoised
   \        : (deblock==false) ? denoised
   \        : (useQED==true)   ? denoised.deblock_QED(quant1=quant1,quant2=quant2,uv=chroma?3:2)
   \        :                    denoised.deblock(quant=int(quant1*0.5+quant2*0.5))


   ### INITIATING
   nullclip = BlankClip()

   i   = defined(source) ? source : denoised
   smP = denoised

   xn = smP.width()
   yn = smP.height()

   chr41 = chroma?4:1
   chr31 = chroma?3:1
   chr21 = chroma?2:1


   ### PROTECTING
   rg17 = (protect==true) ? smP.removegrain(17,-1) : NOP()
   iOB  = (protect==true) ? i.mt_lut("x "+string(cutoff)+" >= x 0 ?",U=1,V=1) : NOP()
   mB   = (protect==true) ? mt_makediff(iOB,rg17,U=1,V=1).mt_binarize(128+threshold,upper=false,U=1,V=1).removegrain(5,-1) : NOP()
   lB   = (protect==true) ? mt_lutxy(smP,i,"x y - abs "+string(maxdiff)+" <= x x y - 0 < y "+string(maxdiff)+" - x ? ?",U=1,V=1) : NOP()
   smB  = (protect==true) ? mt_merge(smP,lB,mB,U=chr21,V=chr21) : smP


   ### ANTI-ALIASING
   cAA = (AA==false)      ? nullclip
   \   : (useEEDI2==true) ? smB.Turnleft().EEDI2(field=1,maxd=maxd).Turnright().EEDI2(field=1,maxd=maxd)
   \   :                    smB.Pointresize(xn*2,yn*2).Turnleft().Sangnom(order=1,aa=maxd).Turnright().Sangnom(order=1,aa=maxd)
   cAA = (AA==false)      ? nullclip
   \   : (method==0)      ? cAA.Bilinearresize(xn,yn,0.5,-0.5,xn*2,yn*2)
   \   : (method==1)      ? cAA.Spline36resize(xn,yn,0.5,-0.5,xn*2,yn*2)
   \   :                    cAA.Lanczos4resize(xn,yn,0.5,-0.5,xn*2,yn*2)
   mA  = (AA==true) ? smB.mt_edge("prewitt",0,255,0,0,V=1,U=1).mt_lut("x "+string(AAthr)+" <= 0 x 1.4 ^ ?",U=1,V=1).removegrain(4,-1).mt_inflate(U=1,V=1) : NOP()
   smA = (AA==true) ? mt_merge(smB,cAA,mA,luma=chroma?true:false,U=chr31,V=chr31) : NOP()

   aaD1    = (AA==true&&reduc!=-1) ? mt_makediff(smB, smA, U=1, V=1) : NOP()
   aaD1MVS = (AA==true&&reduc!=-1) ? aaD1.MSuper(hpad=0,vpad=0,pel=pel,sharp=MVsharp,chroma=false) : NOP()              
   aaD2    = (AA==true&&reduc!=-1) ? aaD1.MDegrain1(aaD1MVS, b1v, f1v, thSAD=thSAD, thSCD1=thSCD1, thSCD2=thSCD2, plane=0) : NOP()
   aaD2    = (AA==true&&reduc!=-1) ? mt_lutxy(aaD1, aaD2, "x 128 - abs y 128 - abs < x y ?", U=1, V=1).mergeluma(aaD2, 1.0-reduc) : NOP()

   smA     = (AA==false) ? smB
   \       : (reduc==-1) ? smA
   \       :               mt_makediff(smB, aaD2, U=2, V=2)


   ### EDGECLEANING
   mP  = (edgeclean==true) ? mt_edge(smA,"prewitt",0,255,0,0,V=1,U=1) : NOP()
   mS  = (edgeclean==true) ? mP.mt_expand(mode=mt_square(radius=ECrad),U=1,V=1).mt_inflate(U=1,V=1) : NOP()
   mD  = (edgeclean==true) ? mt_lutxy(mS,mP.mt_inflate(U=1,V=1),"x y - "+string(ECthr)+" <= 0 x y - ?",U=1,V=1).mt_inflate(U=1,V=1).removegrain(20,-1) : NOP()
   smE = (edgeclean==true) ? mt_merge(smA,Eval("smA." + ECmode),mD,luma=chroma?true:false,U=chr31,V=chr31) : smA


   ### MASKING
   mM = (sharp==true||enhance==true||stabilize==true) ? mt_average(SAD_f1m,SAD_b1m,U=1,V=1).mt_lut(expr="x 1.6 ^",U=1,V=1) : NOP()
   mE = (sharp==true||enhance==true||stabilize==true) ? mt_edge(smE,"prewitt",0,255,0,0,V=1,U=1).mt_lut(expr="x 1.8 ^",U=1,V=1).removegrain(4,-1).mt_inflate(U=1,V=1) : NOP()
   mL = (sharp==true||enhance==true||stabilize==true) ? mt_logic(mM.invert(),mE,"min",U=1,V=1).removegrain(20,-1) : NOP()
   mF = (sharp==true||enhance==true||stabilize==true) ? mt_logic(mM,mE,"max",U=1,V=1).removegrain(20,-1) : NOP()


   ### SHARPENING
   Sclp = (sharp == true) ? smE.LSFmod(strength=strength,Smode=SHmode,Smethod=SHmethod,Lmode=Slimit,overshoot=Sovershoot,preblur="ON",secure=true,edgemode=0,soft=0,soothe=false,ss_x=1.00,ss_y=1.00) : NOP()
   Tmax = (sharp == true) ? i.mt_logic(f1c,"max",U=1,V=1).mt_logic(b1c,"max",U=1,V=1) : NOP()
   Tmin = (sharp == true) ? i.mt_logic(f1c,"min",U=1,V=1).mt_logic(b1c,"min",U=1,V=1) : NOP()

   shrp = (sharp==false) ? smE
   \    : (Tlimit==true) ? Sclp.mt_clamp(Tmax, Tmin, Tovershoot, Tovershoot, U=1, V=1)
   \    :                  Sclp
   sL   = (sharp==true&&adapt==true) ? mt_merge(smE,shrp,mL,U=chr21,V=chr21)
   \    :                              shrp.mergechroma(smE)


   ### STABILIZING & ENHANCING
   TTc = (stabilize==true) ? sL.TTempSmoothF(maxr=maxr,strength=TTstr)
   \   :                     sL
   GFc = (enhance==true) ? TTc.GradFun2DBmod(thr=GFthr,thrC=chroma?GFthr:1.0,mode=0,str=AGstr,strC=0.0,temp=temp,adapt=bias,mask=false,show=false)
   \   :                   TTc
   FSc = (enhance==true||stabilize==true) ? mt_merge(GFc,sL,mF,luma=chroma?true:false,U=chr31,V=chr31)
   \   :                                    GFc
   PP  = (chroma==false) ? FSc.mergechroma(internal?i:smP)
   \   :                   FSc

   return PP
   }

PP = MCTemporalDenoisePP( smP, i,
   \                      sharp, adapt, strength, SHmode, SHmethod, Slimit, Sovershoot, Tlimit, Tovershoot,
   \                      protect, cutoff, threshold, maxdiff,
   \                      AA, useEEDI2, reduc, maxd, AAthr, method,
   \                      deblock, useQED, quant1, quant2,
   \                      edgeclean, ECrad, ECthr, ECmode,
   \                      stabilize, maxr, TTstr,
   \                      enhance, GFthr, AGstr, bias, temp,
   \                      chroma, settings,
   \                      f1v, b1v, f1c, b1c, SAD_f1m, SAD_b1m, true )


### OUTPUT
o = PP.crop(xf/2,yf/2,-xf/2,-yf/2)
o = (interlaced==true) ? o.weave() : o


### SHOW
   function MCTD_Show( clip i, clip p, clip o, int "xf", int "yf", string "version", string "settings", string "show_p",
   \                   int "radius", int "sigma", bool "twopass", bool "useTTmpSm", int "limit", int "limit2", int "post", bool "chroma", bool "interlaced",
   \                   bool "sharp", bool "adapt", int "strength", int "SHmode", int "SHmethod", int "Slimit", int "Sovershoot", bool "Tlimit", int "Tovershoot",
   \                   bool "protect", int "cutoff", int "threshold", int "maxdiff",
   \                   bool "AA", bool "useEEDI2", float "reduc", int "maxd", int "AAthr", int "method",
   \                   bool "deblock", bool "useQED", int "quant1", int "quant2",
   \                   bool "edgeclean", int "ECrad", int "ECthr", string "ECmode",
   \                   bool "stabilize", int "maxr", int "TTstr",
   \                   bool "enhance", float "GFthr", float "AGstr", int "bias", int "temp",
   \                   int "bwbh", int "owoh", int "blksize", int "overlap",
   \                   bool "GPU", bool "fixband", int "bt", int "ncpu", int "precision",
   \                   int "thSAD", int "thSAD2", int "thSCD1", int "thSCD2",
   \                   bool "truemotion", bool "MVglobal", int "pel", int "pelsearch", int "search", int "searchparam", int "MVsharp", int "DCT",
   \                   int "screenW", int "screenH" )
   {
   i  = i.crop(xf/2,yf/2,-xf/2,-yf/2)
   p  = p.crop(xf/2,yf/2,-xf/2,-yf/2)
   cW = screenW/2
   cH = screenH/2
   iW = i.width()
   iH = i.height()

   iR = round((iW*cH)/(4*iH))*4
   iC = (iR-cW)/2

   i = i.spline36resize(iR,cH)
   p = p.spline36resize(iR,cH)
   o = o.spline36resize(iR,cH)

   i = (iC>0) ? i.crop(iC,0,-iC,0) : i
   p = (iC>0) ? p.crop(iC,0,-iC,0) : p
   o = (iC>0) ? o.crop(iC,0,-iC,0) : o

   i = (iC<0) ? i.addborders(-iC,0,-iC,0) : i
   p = (iC<0) ? p.addborders(-iC,0,-iC,0) : p
   o = (iC<0) ? o.addborders(-iC,0,-iC,0) : o

   b = i.blankclip()

   b = b.SubTitle("MCTemporalDenoise    " + string(version)             ,text_color=$FFFFFF,font="COURIER NEW",size=24,x=40,y=20)
   b = b.SubTitle("SETTINGS           = " + string(settings)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=60)
   b = b.SubTitle("'p' clip           = " + string(show_p)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=70)

   b = b.SubTitle("radius             = " + string(radius)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=90)
   b = b.SubTitle("sigma              = " + string(sigma)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=100)
   b = b.SubTitle("twopass            = " + string(twopass)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=110)
   b = b.SubTitle("useTTmpSm          = " + string(useTTmpSm)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=120)
   b = b.SubTitle("limit              = " + string(limit)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=130)
   b = b.SubTitle("limit2             = " + string(limit2)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=140)
   b = b.SubTitle("post               = " + string(post)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=150)
   b = b.SubTitle("chroma             = " + string(chroma)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=160)
   b = b.SubTitle("interlaced         = " + string(interlaced)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=170)

   b = b.SubTitle("sharp              = " + string(sharp)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=190)
   b = b.SubTitle("adapt              = " + string(adapt)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=200)
   b = b.SubTitle("strength           = " + string(strength)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=210)
   b = b.SubTitle("SHmode             = " + string(SHmode)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=220)
   b = b.SubTitle("SHmethod           = " + string(SHmethod)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=230)
   b = b.SubTitle("Slimit             = " + string(Slimit)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=240)
   b = b.SubTitle("Sovershoot         = " + string(Sovershoot)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=250)
   b = b.SubTitle("Tlimit             = " + string(Tlimit)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=260)
   b = b.SubTitle("Tovershoot         = " + string(Tovershoot)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=270)

   b = b.SubTitle("protect            = " + string(protect)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=290)
   b = b.SubTitle("cutoff             = " + string(cutoff)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=300)
   b = b.SubTitle("threshold          = " + string(threshold)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=310)
   b = b.SubTitle("maxdiff            = " + string(maxdiff)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=320)

   b = b.SubTitle("AA                 = " + string(AA)                  ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=340)
   b = b.SubTitle("useEEDI2           = " + string(useEEDI2)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=350)
   b = b.SubTitle("reduc              = " + string(reduc)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=360)
   b = b.SubTitle("maxd               = " + string(maxd)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=370)
   b = b.SubTitle("AAthr              = " + string(AAthr)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=380)
   b = b.SubTitle("method             = " + string(method)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=390)

   b = b.SubTitle("deblock            = " + string(deblock)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=410)
   b = b.SubTitle("useQED             = " + string(useQED)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=420)
   b = b.SubTitle("quant1             = " + string(quant1)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=430)
   b = b.SubTitle("quant2             = " + string(quant2)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=60,y=440)

   b = b.SubTitle("edgeclean          = " + string(edgeclean)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=60)
   b = b.SubTitle("ECrad              = " + string(ECrad)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=70)
   b = b.SubTitle("ECthr              = " + string(ECthr)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=80)
   b = b.SubTitle("ECmode             = " + string(ECmode)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=90)

   b = b.SubTitle("stabilize          = " + string(stabilize)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=110)
   b = b.SubTitle("maxr               = " + string(maxr)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=120)
   b = b.SubTitle("TTstr              = " + string(TTstr)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=130)

   b = b.SubTitle("enhance            = " + string(enhance)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=150)
   b = b.SubTitle("GFthr              = " + string(GFthr)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=160)
   b = b.SubTitle("AGstr              = " + string(AGstr)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=170)
   b = b.SubTitle("bias               = " + string(bias)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=180)
   b = b.SubTitle("temp               = " + string(temp)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=190)

   b = b.SubTitle("bwbh               = " + string(bwbh)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=210)
   b = b.SubTitle("owoh               = " + string(owoh)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=220)
   b = b.SubTitle("blksize            = " + string(blksize)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=230)
   b = b.SubTitle("overlap            = " + string(overlap)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=240)

   b = b.SubTitle("GPU                = " + string(GPU)                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=260)
   b = b.SubTitle("fixband            = " + string(fixband)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=270)
   b = b.SubTitle("bt                 = " + string(bt)                  ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=280)
   b = b.SubTitle("ncpu               = " + string(ncpu)                ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=290)
   b = b.SubTitle("precision          = " + string(precision)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=300)

   b = b.SubTitle("thSAD              = " + string(thSAD)               ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=320)
   b = b.SubTitle("thSAD2             = " + string(thSAD2)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=330)
   b = b.SubTitle("thSCD1             = " + string(thSCD1)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=340)
   b = b.SubTitle("thSCD2             = " + string(thSCD2)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=350)

   b = b.SubTitle("truemotion         = " + string(truemotion)          ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=370)
   b = b.SubTitle("MVglobal           = " + string(MVglobal)            ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=380)
   b = b.SubTitle("pel                = " + string(pel)                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=390)
   b = b.SubTitle("pelsearch          = " + string(pelsearch)           ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=400)
   b = b.SubTitle("search             = " + string(search)              ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=410)
   b = b.SubTitle("searchparam        = " + string(searchparam)         ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=420)
   b = b.SubTitle("MVsharp            = " + string(MVsharp)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=430)
   b = b.SubTitle("DCT                = " + string(DCT)                 ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=440)

   #b = b.SubTitle("screenW            = " + string(screenW)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=460)
   #b = b.SubTitle("screenH            = " + string(screenH)             ,text_color=$FFFFFF,font="COURIER NEW",size=12,x=360,y=470)

   i = i.subtitle("Input",text_color=$FFFFFF,size=20,x=20,y=20)
   p = p.subtitle("Prefilter",text_color=$FFFFFF,size=20,x=20,y=20)
   o = o.subtitle("Output",text_color=$FFFFFF,size=20,x=20,y=20)

   TOP = stackhorizontal(b,p)
   BOT = stackhorizontal(i,o)

   return stackvertical(TOP,BOT)
   }

o = (show==false) ? o : MCTD_Show(i,p,o,xf,yf,version,settings,show_p,
\                                 radius,sigma,twopass,useTTmpSm,limit,limit2,post,chroma,interlaced,
\                                 sharp,adapt,strength,SHmode,SHmethod,Slimit,Sovershoot,Tlimit,Tovershoot,
\                                 protect,cutoff,threshold,maxdiff,
\                                 AA,useEEDI2,reduc,maxd,AAthr,method,
\                                 deblock,useQED,quant1,quant2, 
\                                 edgeclean,ECrad,ECthr,ECmode,
\                                 stabilize,maxr,TTstr,  
\                                 enhance,GFthr,AGstr,bias,temp,
\                                 bwbh,owoh,blksize,overlap,
\                                 GPU,fixband,bt,ncpu,precision,
\                                 thSAD,thSAD2,thSCD1,thSCD2,
\                                 truemotion,MVglobal,pel,pelsearch,search,searchparam,MVsharp,DCT,
\                                 screenW,screenH)

return o
}
