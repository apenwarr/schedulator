import curses

BOLD = curses.A_BOLD
UNDERLINE = curses.A_UNDERLINE
REVERSE = curses.A_REVERSE

BLACK    = (0x10, curses.COLOR_BLACK,    (0,0,0),       0)
RED      = (0x11, curses.COLOR_RED,      (600,0,0),     0)
GREEN    = (0x12, curses.COLOR_GREEN,    (0,600,0),     0)
YELLOW   = (0x13, curses.COLOR_YELLOW,   (600,600,0),   0)
BLUE     = (0x14, curses.COLOR_BLUE,     (250,0,900),   0)
MAGENTA  = (0x15, curses.COLOR_MAGENTA,  (600,0,600),   0)
CYAN     = (0x16, curses.COLOR_CYAN,     (0,600,600),   0)
WHITE    = (0x17, curses.COLOR_WHITE,    (600,600,600), 0)

xBLACK   = (0x18, curses.COLOR_BLACK,    (200,200,200),    BOLD)
xRED     = (0x19, curses.COLOR_RED,      (1000,0,350),     BOLD)
xGREEN   = (0x1a, curses.COLOR_GREEN,    (0,1000,350),     BOLD)
xYELLOW  = (0x1b, curses.COLOR_YELLOW,   (1000,1000,350),  BOLD)
xBLUE    = (0x1c, curses.COLOR_BLUE,     (375,0,1000),     BOLD)
xMAGENTA = (0x1d, curses.COLOR_MAGENTA,  (1000,0,1000),    BOLD)
xCYAN    = (0x1e, curses.COLOR_CYAN,     (0,1000,1000),    BOLD)
xWHITE   = (0x1f, curses.COLOR_WHITE,    (1000,1000,1000), BOLD)

_all_colors = [BLACK, RED, GREEN, YELLOW, BLUE, MAGENTA, CYAN, WHITE,
               xBLACK, xRED, xGREEN, xYELLOW, xBLUE, xMAGENTA, xCYAN, xWHITE]

def _can_change_color():
    return curses.can_change_color() and curses.COLORS >= 32

def has_colors():
    return curses.has_colors()

_colorcache = {}
_colornext = 1
def color(fg, bg, *attrs):
    try:
        # fast path
        return _colorcache[fg,bg,attrs]
    except KeyError:
        ccv = _colorcache.get((fg,bg))
        if ccv == None:
            global _colornext
            pairid = _colornext
            _colornext += 1
            if has_colors():
                if _can_change_color():
                    av = 0
                    curses.init_pair(pairid, fg[0], bg[0])
                else:
                    av = fg[3]
                    curses.init_pair(pairid, fg[1], bg[1])
                pv = curses.color_pair(pairid)
            else:
                # terrible black-and-white terminal like vt100
                av = fg[3]
                if bg[0] >= xRED[0] or bg[0] == WHITE[0]:
                    av |= REVERSE
                pv = 0
            _colorcache[fg,bg] = pv,av
        else:
            (pv,av) = ccv
        for a in attrs:
            av |= a
        _colorcache[fg,bg,attrs] = av | pv
        return av | pv


