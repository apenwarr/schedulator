#!/usr/bin/env python
from __future__ import with_statement
import sys, os, curses, time, weakref, select, signal
sys.path[:0] = ['../..']
from pylogic import *

if 1:
    print 'test 1'
    sys.stdout.flush()
    with Screen() as s:
        w = s.add(View(), size=s.size, pos=Pos(0,0))
        w.fill('.', color(BLACK,xBLUE))
        w.w.addstr("\n\n   wonko the sane  \n\nbane",
                   color(RED, xBLACK))
        w.w.addstr("\n\n   wonko the sane  \n\nbane",
                   color(xRED, BLUE, UNDERLINE))
        w.border()

        p = s.add(View(), size=Size(10,10), pos=Pos(20,10))
        p.fill('!', color(RED, YELLOW))
        p.border()

        v = s.add(View(), pos=Pos(-5,6), size=Size(30,600))
        v.fill('Q', color(xYELLOW,BLACK))
        v.border()

        v4 = s.add(View(), pos=Pos(10,8), size=Size(30,5))
        v4.fill('R', color(xBLUE,BLACK))
        v4.border()

        s.runonce(0.125)

        v.setcursor(Pos(1,1))
        s.runonce(0.125)

        v.setcursor(None)
        s.runonce(0.125)

        s.remove(v)
        s.runonce(0.25)

        s.add(v, size=Size(30,600), pos=Pos(-5,6))
        s.runonce(0.25)

        while not s.runonce(0.025):
            s.remove(v)
            s.add(v, size=Size(30,600), pos=Pos(10,6))

        for i in range(s.size.y):
            s.remove(v)
            s.add(v, size=Size(30,600), pos=Pos(i*2,i))
            s.runonce(0.025)

if 1:
    print 'test 2'
    sys.stdout.flush()
    with Screen() as s:
        topbar = s.add(View(minsize=Size(3,1)), 'ne')
        topbar2 = s.add(View(minsize=Size(3,1)), 'nw')
        botbar = s.add(View(minsize=Size(3,2)), 'swe')
        leftbar = s.add(View(minsize=Size(10,3)), 'wns')
        rightbar = s.add(View(minsize=Size(5,3)), 'e')
        content = s.add(View(), 'nsew')
        centre = s.add(View(minsize=Size(8,2)), '')
        s.layout()

        topbar.fill(' ', color(xWHITE, CYAN))
        topbar2.fill(' ', color(xWHITE, xCYAN))
        botbar.fill(' ', color(xWHITE, BLUE))
        leftbar.fill('x', color(RED, BLACK))
        rightbar.fill('y', color(RED, BLACK))
        centre.fill('C', color(xYELLOW, BLUE))
        content.fill(':', color(WHITE, BLACK))

        while not s.runonce(1):
            pass

        topbar2.minsize.x += 10
        botbar.minsize.y = 0
        leftbar.minsize.x *= 2
        s.layout()

        while not s.runonce(1):
            pass
