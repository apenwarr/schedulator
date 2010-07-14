import sys, os, curses, weakref, select, signal
import colors

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

    def __cmp__(self, pos):
        if not pos:
            return 1
        return cmp((self.x,self.y), (pos.x,pos.y))


class Size(Pos):
    __slots__ = ['x','y']
        
    def __repr__(self):
        return 'Size(%d,%d)' % (self.x, self.y)
        

class Area(object):
    __slots__ = ['x1', 'y1', 'x2', 'y2']

    def __init__(self, pos, size):
        (self.x1, self.y1) = (pos.x, pos.y)
        size = Pos(max(size.x, 0), max(size.y, 0))
        (self.x2, self.y2) = (pos.x + size.x - 1, pos.y + size.y - 1)

    def __repr__(self):
        return 'Area(%d,%d,%d,%d)' % self.coords()

    def size(self):
        return Size(self.x2 - self.x1 + 1, self.y2 - self.y1 + 1)

    def coords(self):
        return (self.x1, self.y1, self.x2, self.y2)

    def intersect(self, area):
        topleft = Pos(max(self.x1, area.x1), max(self.y1, area.y1))
        botright = Pos(min(self.x2, area.x2), min(self.y2, area.y2))
        return Area(topleft, botright - topleft + Pos(1,1))

    def __cmp__(self, area):
        if not area:
            return 1
        return cmp(self.coords(), area.coords())


class _Child(object):
    __slots__ = ['child', 'anchor', 'pos', 'size']

    def __init__(self, child, anchor, pos, size):
        (self.child, self.anchor, self.pos, self.size) \
            = (child,anchor,pos,size)

    def __repr__(self):
        return 'C(%r,%r,%r,%r)' % (self.child, self.anchor, self.pos, self.size)


class View:
    def __init__(self, minsize=None):
        self.parent = None
        self.children = []
        self.w = self.pos = self.size = self._autofiller = None
        self.minsize = minsize or Size(0,0)
        self._needs_layout = True

    def add(self, child, anchor=None, pos=None, size=None):
        anchor = anchor or ''
        if anchor:
            assert(not pos)
            assert(not size)
        if pos:
            assert(size)
        child._setparent(self)
        self.children.append(_Child(child, anchor, pos, size))
        if size:
            child._setsize(size)
            child._setpos(Pos(0,0))
        self._needs_layout = True
        return child

    def remove(self, child):
        self._needs_layout = True
        for i,c in enumerate(self.children):
            if c.child == child:
                del self.children[i]
                c.child._setparent(None)
                return child

    def _setparent(self, parent):
        p = self.parent and self.parent() or None
        if p != parent:
            if parent:
                self.parent = weakref.ref(parent)
            else:
                self.parent = None

    def _setpos(self, pos):
        if self.pos != pos:
            self.pos = pos

    def draw(self):
        pass

    def _setsize(self, size):
        if self.size != size:
            self.size = size
            self.w = curses.newpad(max(size.y, 1), max(size.x, 1))
            if self._autofiller:
                self._autofiller()
            self.draw()
            self._needs_layout = True

    def area(self):
        assert(self.pos)
        assert(self.size)
        return Area(self.pos, self.size)

    def gpos(self):
        assert(self.pos)
        if self.parent:
            return self.parent().gpos() + self.pos
        else:
            return self.pos

    def garea(self):
        assert(self.pos)
        assert(self.size)
        if self.parent:
            return Area(self.gpos(), self.size)
        else:
            return Area(self.pos, self.size)

    def _do_layout(self):
        remain = Area(Pos(0,0), self.size)
        for c in self.children:
            if c.pos:
                assert(c.size)
                size = c.size
                pos = c.pos
            else:
                a = c.anchor
                wantsize = c.child.minsize
                stretch_x = 'w' in c.anchor and 'e' in c.anchor
                stretch_y = 'n' in c.anchor and 's' in c.anchor
                uses_x = not stretch_x and ('w' in c.anchor or 'e' in c.anchor)
                uses_y = not stretch_y and ('n' in c.anchor or 's' in c.anchor)

                if stretch_x:
                    x1 = remain.x1
                    x2 = remain.x2
                elif 'w' in c.anchor:
                    x1 = remain.x1
                    x2 = remain.x1 + wantsize.x - 1
                    if not uses_y:
                        remain.x1 += wantsize.x
                elif 'e' in c.anchor:
                    x2 = remain.x2
                    x1 = remain.x2 - wantsize.x + 1
                    if not uses_y:
                        remain.x2 -= wantsize.x
                else:
                    x1 = remain.x1 + (remain.x2 - remain.x1 + 1 - wantsize.x)/2
                    x2 = x1 + wantsize.x - 1

                if stretch_y:
                    y1 = remain.y1
                    y2 = remain.y2
                elif 'n' in c.anchor:
                    y1 = remain.y1
                    y2 = remain.y1 + wantsize.y - 1
                    remain.y1 += wantsize.y
                elif 's' in c.anchor:
                    y2 = remain.y2
                    y1 = remain.y2 - wantsize.y + 1
                    remain.y2 -= wantsize.y
                else:
                    y1 = remain.y1 + (remain.y2 - remain.y1 + 1 - wantsize.y)/2
                    y2 = y1 + wantsize.y - 1

                size = Size(x2-x1+1, y2-y1+1)
                pos = Pos(x1, y1)

            c.child._setsize(size)
            c.child._setpos(pos)
        self._needs_layout = False

    def layout(self):
        self._do_layout()
        for c in self.children:
            c.child.layout()

    def _render_me(self):
        area = self.garea()
        parea = self.parent and self.parent().garea() or area
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
        self._autofiller = lambda: self.w.bkgd(c, at)
        if self.w:
            self._autofiller()

    def border(self):
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
        self.root = None
        self.w = None
        self.cursorpos = None
        self.oldhandler = None
    
    def __enter__(self):
        # avoid a bug in python 2.6's curses implementation when
        # curses.initscr() is called more than once.  The workaround is to
        # use _curses.initscr() instead, although this makes the ACS_*
        # constants unavailable in curses, only in _curses.  But that's ok.
        #
        # See: http://bugs.python.org/issue7567
        #
        # Once it's clear which versions of python do/don't have this bug,
        # it might make sense to adjust the hack to only affect those
        # versions.
        if 1:
            import _curses
            self.root = _curses.initscr()
        else:
            self.root = curses.initscr()
        try:
            (ys,xs) = self.root.getmaxyx()
            self._setsize(Size(xs,ys))
            self._setpos(Pos(0,0))
            curses.start_color()
            if colors._can_change_color():
                for (c,nc,(r,g,b),a) in colors._all_colors:
                    curses.init_color(c, r,g,b)
            def resize_handler(sig, frame):
                self.resize()
            self.oldhandler = signal.signal(signal.SIGWINCH, resize_handler)
            return self
        except:
            curses.endwin()
            raise

    def __exit__(self, type,value,traceback):
        signal.signal(signal.SIGWINCH, self.oldhandler)
        try:
            curses.endwin()
        except:
            # this pointlessly returns ERR (but initscr doesn't??) if stdout
            # is not a tty.  Surely there's no way to recover if we can't
            # *stop* curses, so let's just ignore the exception and pray for
            # the best.  (In the case of 'make test', where there is no tty,
            # that seems to be fine.)
            pass

    def resize(self):
        self.__exit__(None,None,None)
        self.__enter__()
        self.layout()
        self.refresh()

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
        try:
            (r,w,x) = select.select([sys.stdin.fileno()], [], [], timeout)
        except select.error:
            return None
        else:
            return r

    def runonce(self, timeout=None):
        r = self.select(timeout)
        if r:
            os.read(sys.stdin.fileno(), 4096)  # *up to* 4096 bytes
        return r and True or False


class FakeScreen(Screen):
    """A viewscreen that redirects to /dev/null.  Useful for testing.
    Theoretically curses.setupterm() could let you do this, but it doesn't
    seem to work at all.
    """
    
    def __enter__(self):
        # the default stdin/out/err objects refer to fd 0,1,2.  curses doesn't
        # use those - it uses 0,1,2 directly.  So we'll change 0,1,2 to point
        # at /dev/null, and stdin/stdout/stderr to point at a new set of fds.
        # That way python's I/O will continue to work as expected.
        self._oldterm = os.environ.get('TERM')
        os.environ['TERM'] = 'xterm'  # should exist on all systems
        self._newfiles = (os.fdopen(os.dup(0), 'rb'),
                          os.fdopen(os.dup(1), 'wb'),
                          os.fdopen(os.dup(2), 'wb'))
        self._oldfiles = (sys.stdin, sys.stdout, sys.stderr)
        (sys.stdin, sys.stdout, sys.stderr) = self._newfiles

        nullf = open('/dev/null', 'w+b')
        os.dup2(nullf.fileno(), 0)
        os.dup2(nullf.fileno(), 1)
        os.dup2(nullf.fileno(), 2)
        try:
            return Screen.__enter__(self)
        except:
            self._restore()
            raise

    def _restore(self):
        os.environ['TERM'] = self._oldterm
        os.dup2(self._newfiles[0].fileno(), 0)
        os.dup2(self._newfiles[1].fileno(), 1)
        os.dup2(self._newfiles[2].fileno(), 2)
        (sys.stdin, sys.stdout, sys.stderr) = self._oldfiles
        self._newfiles[0].close()
        self._newfiles[1].close()
        self._newfiles[2].close()
        del self._newfiles
        del self._oldfiles
        del self._oldterm

    def __exit__(self, type,value,traceback):
        try:
            Screen.__exit__(self, type,value,traceback)
        finally:
            self._restore()
