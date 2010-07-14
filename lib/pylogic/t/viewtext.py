#!/usr/bin/env python
from __future__ import with_statement
import sys, re
sys.path[:0] = ['../..']
from pylogic import *

class TextView(View):
    def __init__(self, lines, minsize=None):
        self.topline = 0
        self.curline = 0
        self.lines = lines
        View.__init__(self, minsize=minsize)

    def do_line(self, y, chunks, at):
        x = 0
        for e in chunks:
            if isinstance(e, tuple):
                (t,tat) = e[0],e[1]
            else:
                (t,tat) = e,at
            if x >= self.size.x:
                break
            t = re.sub('[\x00-\x1f]', ' ', t)
            self.w.insstr(y,x, t, tat)
            x += len(t)
        if x < self.size.x:
            if x < self.size.x-1:
                # workaround for weird but in MacOS X terminal
                self.w.addstr(y,x, ' ', at)
            self.w.insstr(y,x, ' '*(self.size.x - x), at)

    def draw(self):
        self.w.erase()
        for y in range(self.size.y):
            try:
                line = self.lines[self.topline + y].replace('\n', ' ')
            except IndexError:
                pass
            else:
                if self.topline + y == self.curline:
                    at = color(BLACK, xYELLOW)
                    nat = color(BLACK, xYELLOW, BOLD)
                else:
                    at = color(xWHITE, BLACK)
                    nat = color(xWHITE, BLACK, BOLD)
                s = "%d: %s" % (self.topline+y, line)
                self.do_line(y, [('%d: ' % (self.topline+y), nat),
                                 (line, at)],
                             at)


with Screen() as s:
    s.fill(':', color(WHITE, BLACK))
    
    topbar = s.add(View(minsize=Size(0,1)), 'nwe')
    topbar.fill('-', color(xWHITE, CYAN))
    
    botbar = s.add(View(minsize=Size(0,1)), 'swe')
    botbar.fill('-', color(xWHITE, CYAN))

    textview = s.add(TextView(open('/etc/profile').readlines()*20,
                              minsize=Size(30,5)),
                     'ns')
    
    s.layout()

    while 1:
        if s.select():
            k = sys.stdin.read(1)
            if k == chr(27):
                break
            elif k == '+':
                textview.minsize.x += 1
                s.layout()
            elif k == '-':
                textview.minsize.x -= 1
                s.layout()
            else:
                #textview.topline += 1
                textview.curline += 1
                textview.draw()
