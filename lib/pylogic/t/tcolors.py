from __future__ import with_statement
from wvtest import *
import curses
import pylogic as pyl

@wvtest
def test_colors_cache():
    with pyl.FakeScreen():
        sys.stderr.write("hello world\n")
        WVPASS(1)
        WVPASSNE(pyl.WHITE, pyl.xWHITE)
        c1a = pyl.color(pyl.RED, pyl.BLUE)
        c1b = pyl.color(pyl.RED, pyl.BLUE)
        c2a = pyl.color(pyl.RED, pyl.BLUE, pyl.BOLD)
        c2b = pyl.color(pyl.RED, pyl.BLUE, pyl.BOLD)
        c3a = pyl.color(pyl.RED, pyl.BLUE, pyl.BOLD, pyl.UNDERLINE)
        c3b = pyl.color(pyl.RED, pyl.BLUE, pyl.BOLD | pyl.UNDERLINE)
        c4a = pyl.color(pyl.RED, pyl.GREEN)
        c4b = pyl.color(pyl.RED, pyl.GREEN)

        WVPASSEQ(c1a, c1b)
        WVPASSEQ(c2a, c2b)
        WVPASSEQ(c3a, c3b)
        if pyl.has_colors():
            WVPASSEQ(curses.pair_number(c1a), curses.pair_number(c1b))
            WVPASSEQ(curses.pair_number(c2a), curses.pair_number(c2b))
            WVPASSEQ(curses.pair_number(c3a), curses.pair_number(c3b))
            WVPASSEQ(curses.pair_number(c4a), curses.pair_number(c4b))
            
            WVPASSEQ(curses.pair_number(c1a), curses.pair_number(c2a))
            WVPASSEQ(curses.pair_number(c1a), curses.pair_number(c3a))
            WVPASSNE(curses.pair_number(c1a), curses.pair_number(c4a))
