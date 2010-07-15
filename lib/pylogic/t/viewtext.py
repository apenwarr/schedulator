#!/usr/bin/env python
from __future__ import with_statement
import sys, os, re, time
sys.path[:0] = ['../..']
from pylogic import *

class TextView(View):
    def __init__(self, lines, minsize=None):
        self.topline = 0
        self.curline = 0
        self.lines = lines
        self.search_regex = None
        View.__init__(self, minsize=minsize)

    def search(self, regex):
        self.search_regex = regex
        self.draw()

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
        if self.curline < 0:
            self.curline = 0
        if self.curline >= len(self.lines):
            self.curline = len(self.lines) - 1
        if self.curline < self.topline:
            self.topline = self.curline
        if self.curline >= self.topline + self.size.y:
            self.topline = self.curline - self.size.y + 1
        for y in range(self.size.y):
            try:
                line = self.lines[self.topline + y].replace('\n', ' ')
            except IndexError:
                pass
            else:
                if self.topline + y == self.curline:
                    at = color(BLACK, xYELLOW)
                    nat = color(BLACK, xYELLOW, BOLD)
                    rat = color(GREEN, xYELLOW, BOLD, UNDERLINE)
                else:
                    at = color(xWHITE, BLACK)
                    nat = color(xWHITE, BLACK, BOLD)
                    rat = color(BLACK, GREEN, BOLD, UNDERLINE)
                s = "%d: %s" % (self.topline+y, line)
                chunks = []
                chunks.append(('%d: ' % (self.topline+y), nat))
                ss = 0
                if self.search_regex:
                    try:
                        it = re.finditer(self.search_regex, line, re.I)
                    except:
                        # don't die if the user enters an invalid regex
                        chunks.append((line, at))
                    else:
                        for i in it:
                            s,e = i.span()
                            chunks.append((line[ss:s], at))
                            chunks.append((line[s:e], rat))
                            ss = e
                        chunks.append((line[ss:], at))
                else:
                    chunks.append((line, at))
                self.do_line(y, chunks, at)


def draw_searchbar(s, searchbar, searchtext):
    searchbar.minsize.x = 8 + len(searchtext) + 5
    if searchtext:
        searchbar.minsize.y = 1
        s.layout()
        searchbar.w.erase()
        searchbar.w.insstr(0,0, "Search: ", color(xWHITE, BLACK))
        searchbar.w.insstr(0,8, searchtext)
        searchbar.setcursor(Pos(8+len(searchtext), 0))
    else:
        searchbar.setcursor(None)
        searchbar.minsize.y = 0
        s.layout()
    textview.search(searchtext)


with Screen() as s:
    s.fill(':', color(WHITE, BLACK))
    
    topbar = s.add(View(minsize=Size(0,1)), 'nwe')
    topbar.fill('-', color(xWHITE, CYAN))
    
    botbar = s.add(View(minsize=Size(0,1)), 'swe')
    botbar.fill('-', color(xWHITE, CYAN))

    searchbar = s.add(View(minsize=Size(0,0)), 'swe')
    searchbar.fill(' ', color(WHITE, BLACK))

    textview = s.add(TextView(open('/etc/profile').readlines()*20,
                              minsize=Size(30,5)),
                     'ns')
    
    s.layout()

    p = keys.Processor()
    done = 0
    searchtext = ''
    while not done:
        if s.select(0.26):
            p.add(os.read(sys.stdin.fileno(), 4096), time.time())
        for k in p.iter(time.time()):
            if k == keys.ESC:
                if searchtext:
                    searchtext = ''
                    draw_searchbar(s, searchbar, searchtext)
                else:
                    done = 1
            elif k == keys.RIGHT:
                textview.minsize.x += 1
                s.layout()
            elif k == keys.LEFT:
                textview.minsize.x -= 1
                s.layout()
            elif k == keys.DOWN:
                textview.curline += 1
                textview.draw()
            elif k == keys.UP:
                textview.curline -= 1
                textview.draw()
            elif k == keys.PGDN:
                if textview.curline < textview.topline + textview.size.y - 1:
                    textview.curline = textview.topline + textview.size.y - 1
                else:
                    textview.curline += textview.size.y
                textview.draw()
            elif k == keys.PGUP:
                if textview.curline > textview.topline:
                    textview.curline = textview.topline
                else:
                    textview.curline -= textview.size.y
                textview.draw()
            elif k == keys.HOME:
                textview.curline = 0
                textview.draw()
            elif k == keys.END:
                textview.curline = len(textview.lines)-1
                textview.draw()
            elif k == keys.BACKSPACE:
                if searchtext:
                    searchtext = searchtext[:-1]
                draw_searchbar(s, searchbar, searchtext)
            elif k == keys.ENTER:
                if searchtext:
                    for y in xrange(textview.curline+1, len(textview.lines)):
                        if re.search(searchtext, textview.lines[y], re.I):
                            textview.curline = y
                            break
                    else:
                        for y in xrange(0, textview.curline):
                            if re.search(searchtext, textview.lines[y], re.I):
                                textview.curline = y
                                break
                    textview.draw()
            else:
                if isinstance(k, basestring):
                    searchtext += k
                draw_searchbar(s, searchbar, searchtext)
