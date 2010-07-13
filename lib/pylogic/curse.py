#!/usr/bin/env python
from __future__ import with_statement
import curses, time

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
            if curses.has_colors():
                if curses.can_change_color():
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


class Pos(object):
    __slots__ = ['x','y']
    def __init__(self, x, y):
        self.x = x
        self.y = y

    def __add__(self, pos):
        return Pos(self.x+pos.x, self.y+pos.y)
    def __sub__(self, pos):
        return Pos(self.x-pos.x, self.y-pos.y)
        
    def __iadd__(self, pos):
        self.x += pos.x
        self.y += pos.y
        return self
    def __isub__(self, pos):
        self.x -= pos.x
        self.y -= pos.y
        return self
        
Size = Pos


class View:
    def __init__(self):
        self.w = None

    def setsize(self, size):
        self.size = size
        self.w = curses.newpad(size.y, size.x)

    def render(self, pos, size):
        ipos = Pos(0,0)
        ipos = Pos(-min(pos.x, 0), -min(pos.y, 0))
        pos += ipos
        size -= ipos
        if size.x > self.size.x:
            size.x = self.size.x
        if size.y > self.size.y:
            size.y = self.size.y
        self.w.noutrefresh(ipos.y, ipos.x,
                           pos.y, pos.x, pos.y+size.y-1, pos.x+size.x-1)

    def fill(self, c, at):
        self.w.bkgd(c, at)
        self.w.border()


class Screen:
    def __enter__(self):
        self.w = curses.initscr()
        (ys,xs) = self.w.getmaxyx()
        self.pos = Pos(0,0)
        self.size = Size(xs,ys)
        curses.start_color()
        if curses.can_change_color():
            for (c,nc,(r,g,b),a) in _all_colors:
                curses.init_color(c, r,g,b)
        return self

    def __exit__(self, type,value,traceback):
        curses.endwin()


with Screen() as s:
    w = curses.newpad(s.size.y, s.size.x)
    w.bkgd('.', color(BLACK,xBLUE))
    w.addstr("\n\n   wonko the sane  \n\nbane", color(RED, xBLACK))
    w.addstr("\n\n   wonko the sane  \n\nbane", color(xRED, BLUE, UNDERLINE))
    w.border()

    p = curses.newpad(10,10)
    p.bkgd('!', color(RED, YELLOW))
    w.noutrefresh(0,0, 0,0,s.size.y-1,s.size.x-1)
    p.noutrefresh(0,0, 10,20,10+10-1,20+10-1)

    v = View()
    v.setsize(Size(20,5))
    v.fill('Q', color(xYELLOW,BLACK))
    v.render(Pos(40,2), Size(500,500))

    curses.doupdate()
    
    time.sleep(1)
    
    n = curses.COLOR_PAIRS
    nn = curses.COLORS
    ccc = curses.can_change_color()

print n
print nn
print ccc
