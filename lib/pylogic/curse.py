#!/usr/bin/env python
from __future__ import with_statement
import sys, os, curses, time, weakref, select

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

    def __repr__(self):
        return 'Pos(%d,%d)' % (self.x, self.y)
        
    def __str__(self):
        return '(%d,%d)' % (self.x, self.y)

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


class Area(object):
    __slots__ = ['x1', 'y1', 'x2', 'y2']

    def __init__(self, pos, size):
        (self.x1, self.y1) = (pos.x, pos.y)
        size = Pos(max(size.x, 0), max(size.y, 0))
        (self.x2, self.y2) = (pos.x + size.x - 1, pos.y + size.y - 1)

    def __repr__(self):
        return 'Area(%d,%d,%d,%d)' % (self.x1, self.y1, self.x2, self.y2)

    def size(self):
        return Size(self.x2 - self.x1 + 1, self.y2 - self.y1 + 1)

    def intersect(self, area):
        topleft = Pos(max(self.x1, area.x1), max(self.y1, area.y1))
        botright = Pos(min(self.x2, area.x2), min(self.y2, area.y2))
        return Area(topleft, botright - topleft + Pos(1,1))


class _Child(object):
    __slots__ = ['child', 'anchor', 'pos', 'size']

    def __init__(self, child, anchor, pos, size):
        (self.child, self.anchor, self.pos, self.size) \
            = (child,anchor,pos,size)

    def __repr__(self):
        return 'C(%r,%r,%r,%r)' % (self.child, self.anchor, self.pos, self.size)


class View:
    def __init__(self):
        self.parent = None
        self.children = []
        self.w = self.pos = self.size = None
        self._needs_layout = True

    def add(self, child, anchor=None, pos=None, size=None):
        if anchor:
            assert(not pos)
        if not anchor:
            if not pos:
                pos = child.pos
            if not size:
                size = child.size
            assert(pos)
            assert(size)
        child._setparent(self)
        self.children.append(_Child(child, anchor, pos, size))
        if size:
            child._setsize(size)
            child._setpos(Pos(0,0))
        self._needs_layout = True
        return child

    def remove(self, child):
        for i,c in enumerate(self.children):
            if c.child == child:
                del self.children[i]
                c.child._setparent(None)
                break

    def _setparent(self, parent):
        p = self.parent and self.parent() or None
        if p != parent:
            if parent:
                self.parent = weakref.ref(parent)
            else:
                self.parent = None
            self._needs_layout = True

    def _setpos(self, pos):
        if self.pos != pos:
            self.pos = pos

    def _setsize(self, size):
        if self.size != size:
            self.size = size
            self.w = curses.newpad(size.y, size.x)
            self._needs_layout = True

    def area(self):
        return Area(self.pos, self.size)

    def gpos(self):
        if self.parent:
            return self.parent().gpos() + self.pos
        else:
            return self.pos

    def garea(self):
        if self.parent:
            return Area(self.gpos(), self.size)
        else:
            return Area(self.pos, self.size)

    def _do_layout(self):
        for c in self.children:
            c.child._setsize(c.size)
            c.child._setpos(c.pos)
        self._needs_layout = False

    def _render_me(self):
        area = self.garea()
        parea = self.parent().garea()
        remain = area.intersect(parea)
        if remain.size().x and remain.size().y:
            offset = Pos(remain.x1 - area.x1, remain.y1 - area.y1)
            try:
                self.w.noutrefresh(offset.y, offset.x,
                                   remain.y1, remain.x1, remain.y2, remain.x2)
            except:
                raise Exception('refresh error: %r' %
                                ((area, parea, remain, offset),))

    def _render(self):
        if self._needs_layout:
            self._do_layout()

        self._render_me()

        # do this *after* our own display, because the children should be
        # drawn on top.
        for c in self.children:
            c.child._render()
        
    def fill(self, c, at):
        self.w.bkgd(c, at)
        self.w.border()

    def setcursor(self, pos):
        if self.parent:
            if pos:
                self.parent().setcursor(self.pos + pos)
            else:
                self.parent().setcursor(None)


class Screen(View):
    def __init__(self):
        View.__init__(self)
        self.w = None
        self.cursorpos = None
    
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

    def _render_me(self):
        self.w.redrawwin()
        self.w.noutrefresh()
        pass

    def setcursor(self, pos):
        self.cursorpos = pos

    def refresh(self):
        self._render()
        if self.cursorpos:
            curses.setsyx(max(0, self.cursorpos.y), max(0, self.cursorpos.x))
            try:
                curses.curs_set(1)
            except curses.error:
                pass
        else:
            curses.setsyx(self.size.y-1, self.size.x-1)
            try:
                curses.curs_set(0)
            except curses.error:
                pass
        curses.doupdate()

    def select(self, timeout=None):
        self.refresh()
        (r,w,x) = select.select([sys.stdin.fileno()], [], [], timeout)
        if r:
            return r

    def runonce(self, timeout=None):
        r = self.select(timeout)
        if r:
            os.read(sys.stdin.fileno(), 4096)  # *up to* 4096 bytes


if 1:
    with Screen() as s:
        w = s.add(View(), size=s.size, pos=Pos(0,0))
        w.fill('.', color(BLACK,xBLUE))
        w.w.addstr("\n\n   wonko the sane  \n\nbane",
                   color(RED, xBLACK))
        w.w.addstr("\n\n   wonko the sane  \n\nbane",
                   color(xRED, BLUE, UNDERLINE))

        p = s.add(View(), size=Size(10,10), pos=Pos(20,10))
        p.fill('!', color(RED, YELLOW))

        v = s.add(View(), pos=Pos(-5,6), size=Size(30,600))
        v.fill('Q', color(xYELLOW,BLACK))

        v4 = s.add(View(), pos=Pos(10,8), size=Size(30,5))
        v4.fill('R', color(xBLUE,BLACK))

        s.runonce()

        v.setcursor(Pos(1,1))
        s.runonce()

        v.setcursor(None)
        s.runonce()

        s.remove(v)
        s.runonce()

        s.add(v)
        s.runonce()

        for i in range(s.size.y):
            s.remove(v)
            s.add(v, pos=Pos(i*10,i))
            s.runonce(0.05)

            n = curses.COLOR_PAIRS
            nn = curses.COLORS
            ccc = curses.can_change_color()

            print n
            print nn
            print ccc

if 1:
    with Screen() as s:
        pass
