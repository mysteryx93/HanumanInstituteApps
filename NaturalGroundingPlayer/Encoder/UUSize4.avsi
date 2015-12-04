##################################
### symmetrically crop or expand a clip to ensure it is a certain size, 
### with size forced to mod-4, mod-8 etc
##
## @ wid, hgt - new desired width & height (no mod requirement); default C.Width, Height
## @ mod      - mod value; one of (4|8|16|32); default 4
## @ option   - expand method -1:extend edges, -2:mirror edges, 
##              else border (option=border color); default 0 (black border)
## @ debug    - show internal variables
##
function UUSize4(clip C, int "wid", int "hgt", int "mod", int "option", bool "debug")
{
    wid      = Default(wid, C.Width)
    hgt      = Default(hgt, C.Height)
    mod     = Default(mod, 4)
    option  = Default(option, 0)
    dbug    = Default(debug, false)

    Assert((mod==4||mod==8||mod==16||mod==32), 
    \  "UUSize4: 'mod' argument not one of (4|8|16|32)")

    color  = Min(Max(0, option), $ffffff)
    option = (option==-1) ? 1 : (option==-2) ? 2 : 0

    out_wid = wid - (wid%mod)
    out_hgt = hgt - (hgt%mod)

    C ## Last==C
    bdrX  = out_wid - Width
    bdrY  = out_hgt - Height
    bdrLt = Ceil(Float(bdrX) / 2.0)
    bdrTp = Ceil(Float(bdrY) / 2.0)
    bdrLt = bdrLt - (bdrLt % 2)
    bdrTp = bdrTp - (bdrTp % 2)
    bdrRt = (bdrX - bdrLt) 
    bdrBt = (bdrY - bdrTp)

    dbg="UUSize4:"                                  +String(mod, "       mod= %0.0f")
    dbg=dbg+"\n"+String(C.Width, "in_wid= %0.0f")   +String(C.Height, "    in_hgt= %0.0f")
    dbg=dbg+"\n"+String(wid, "arg_wid= %0.0f")      +String(hgt, "    arg_hgt= %0.0f")
    dbg=dbg+"\n"+String(bdrX, "bdrX= %+0.0f")       +String(bdrY, "       bdrY= %+0.0f")
    dbg=dbg+"\n"+String(bdrLt, "bdrLt= %+0.0f")     +String(bdrTp, "      bdrTp= %+0.0f")
    dbg=dbg+"\n"+String(bdrRt, "bdrRt= %+0.0f")     +String(bdrBt, "      bdrBt= %+0.0f")
    dbg=dbg+"\n"+String(out_wid, "out_wid= %0.0f")  +String(out_hgt, "    out_hgt= %0.0f")

    Select(option, 
    \   AddBorders(
    \       Max(0, bdrLt), Max(0, bdrTp), 
    \       Max(0, bdrRt), Max(0, bdrBt), color), 
    \   padding(
    \       Max(0, bdrLt), Max(0, bdrTp), 
    \       Max(0, bdrRt), Max(0, bdrBt)), 
    \   MirrorBorders(
    \       Max(0, bdrLt), Max(0, bdrTp), 
    \       Max(0, bdrRt), Max(0, bdrBt))
    \ )

    Crop(
    \   ((bdrLt < 0) ? -bdrLt : 0), 
    \   ((bdrTp < 0) ? -bdrTp : 0), 
    \   ((bdrRt < 0) ?  bdrRt : out_wid), 
    \   ((bdrBt < 0) ?  bdrBt : out_hgt))

    return (!dbug) ? Last : Subtitle(dbg, align=5, lsp=0)
}

# http://forum.doom9.org/showthread.php?p=1596804#post1596804
##################################
function Padding(clip C, int left, int top, int right, int bottom)
{
    left   = Max(0, left)
    top    = Max(0, top)
    right  = Max(0, right)
    bottom = Max(0, bottom)

    w = C.Width()
    h = C.Height()
    C.PointResize(
    \   w+left+right, h+top+bottom, 
    \   -left, -top, 
    \   w+left+right, h+top+bottom)
}

##################################
function MirrorBorders(clip C, int left, int top, int right, int bottom)
{
    bdrLt = Max(0, left)
    bdrTp = Max(0, top)
    bdrRt = Max(0, right)
    bdrBt = Max(0, bottom)

    C ## Last==c
    
    (bdrLt==0) ? Last : StackHorizontal(Crop(0, 0, bdrLt, 0).FlipHorizontal, Last)
    (bdrRt==0) ? Last : StackHorizontal(Last, Crop(Width-bdrRt, 0, bdrRt, 0).FlipHorizontal)

    (bdrTp==0) ? Last : StackVertical(Crop(0, 0, 0, bdrTp).FlipVertical, Last)
    (bdrBt==0) ? Last : StackVertical(Last, Crop(0, Height-bdrBt, 0, bdrBt).FlipVertical)
    
    return Last
}