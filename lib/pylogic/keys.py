TAB = 9
SHIFT_TAB = -999
ESC = 27
BACKSPACE = 127
ENTER = 10

UP = -1
DOWN = -2
LEFT = -3
RIGHT = -4
SHIFT_UP = -5
SHIFT_DOWN = -6
SHIFT_LEFT = -7
SHIFT_RIGHT = -8
CTRL_UP = -9
CTRL_DOWN = -10
CTRL_LEFT = -11
CTRL_RIGHT = -12

PGUP = -21
PGDN = -22
SHIFT_PGUP = -23
SHIFT_PGDN = -24
CTRL_PGUP = -25
CTRL_PGDN = -26

HOME = -31
END = -32
SHIFT_HOME = -33
SHIFT_END = -34
CTRL_HOME = -35
CTRL_END = -36

INSERT = -41
DELETE = -42
SHIFT_INSERT = -43
SHIFT_DELETE = -44
CTRL_INSERT = -45
CTRL_DELETE = -46

F1 = -101
F2 = -102
F3 = -103
F4 = -104
F5 = -105
F6 = -106
F7 = -107
F8 = -108
F9 = -109
F10 = -110
F11 = -111
F12 = -112
SHIFT_F1 = -201
SHIFT_F2 = -202
SHIFT_F3 = -203
SHIFT_F4 = -204
SHIFT_F5 = -205
SHIFT_F6 = -206
SHIFT_F7 = -207
SHIFT_F8 = -208
SHIFT_F9 = -209
SHIFT_F10 = -210
SHIFT_F11 = -211
SHIFT_F12 = -212
CTRL_F1 = -301
CTRL_F2 = -302
CTRL_F3 = -303
CTRL_F4 = -304
CTRL_F5 = -305
CTRL_F6 = -306
CTRL_F7 = -307
CTRL_F8 = -308
CTRL_F9 = -309
CTRL_F10 = -310
CTRL_F11 = -311
CTRL_F12 = -312


# It's perfectly okay for more than one sequence to map to the same key.
# In fact, it's a necessity if we want to support all possible terminals.
# And curses doesn't allow it, which is what's wrong with the curses
# input module.
#
# However, it's *not* okay to have the same sequence map to more than one key.
# That wouldn't make any sense.  It also hasn't ever been a problem so far.
_codes = [
    ('\x1b', ESC),
    (chr(9), TAB),
    ('\x1b[Z', SHIFT_TAB),
    (chr(10), ENTER),
    (chr(13), ENTER),

    # Some terminals, like Windows cmd.exe, map the Delete key to 127.  That's
    # just unforgivably retarded and we won't try to save people like that
    # from themselves.  They will just have to live without a "delete forward"
    # key.  Old versions of XFree86 used to map both Delete *and* Backspace to
    # 127.  Thankfully, those versions are now very dead.
    (chr(8), BACKSPACE),
    (chr(127), BACKSPACE),

    # rxvt-unicode
    
    ('\x1b[A', UP),
    ('\x1b[B', DOWN),
    ('\x1b[D', LEFT),
    ('\x1b[C', RIGHT),
    ('\x1b[a', SHIFT_UP),
    ('\x1b[b', SHIFT_DOWN),
    ('\x1b[d', SHIFT_LEFT),
    ('\x1b[c', SHIFT_RIGHT),
    ('\x1b[Oa', CTRL_UP),
    ('\x1b[Ob', CTRL_DOWN),
    ('\x1b[Od', CTRL_LEFT),
    ('\x1b[Oc', CTRL_RIGHT),

    ('\x1b[5~', PGUP),
    ('\x1b[6~', PGDN),
    ('\x1b[5$', SHIFT_PGUP),
    ('\x1b[6$', SHIFT_PGDN),
    ('\x1b[5^', CTRL_PGUP),
    ('\x1b[6^', CTRL_PGDN),

    ('\x1b[7~', HOME),
    ('\x1b[8~', END),
    ('\x1b[7$', SHIFT_HOME),
    ('\x1b[8$', SHIFT_END),
    ('\x1b[7^', CTRL_HOME),
    ('\x1b[8^', CTRL_END),

    ('\x1b[2~', INSERT),
    ('\x1b[3~', DELETE),
    ('\x1b[2$', SHIFT_INSERT),
    ('\x1b[3$', SHIFT_DELETE),
    ('\x1b[2^', CTRL_INSERT),
    ('\x1b[3^', CTRL_DELETE),

    ('\x1b[11~', F1),
    ('\x1b[12~', F2),
    ('\x1b[13~', F3),
    ('\x1b[14~', F4),
    ('\x1b[15~', F5),
    ('\x1b[17~', F6),
    ('\x1b[18~', F7),
    ('\x1b[19~', F8),
    ('\x1b[20~', F9),
    ('\x1b[21~', F10),
    ('\x1b[23~', F11),
    ('\x1b[24~', F12),
    ('\x1b[23~', SHIFT_F1),
    ('\x1b[24~', SHIFT_F2),
    ('\x1b[25~', SHIFT_F3),
    ('\x1b[26~', SHIFT_F4),
    ('\x1b[28~', SHIFT_F5),
    ('\x1b[29~', SHIFT_F6),
    ('\x1b[31~', SHIFT_F7),
    ('\x1b[32~', SHIFT_F8),
    ('\x1b[33~', SHIFT_F9),
    ('\x1b[34~', SHIFT_F10),
    ('\x1b[23$', SHIFT_F11),
    ('\x1b[24$', SHIFT_F12),
    ('\x1b[11^', CTRL_F1),
    ('\x1b[12^', CTRL_F2),
    ('\x1b[13^', CTRL_F3),
    ('\x1b[14^', CTRL_F4),
    ('\x1b[15^', CTRL_F5),
    ('\x1b[17^', CTRL_F6),
    ('\x1b[18^', CTRL_F7),
    ('\x1b[19^', CTRL_F8),
    ('\x1b[20^', CTRL_F9),
    ('\x1b[21^', CTRL_F10),
    ('\x1b[23^', CTRL_F11),
    ('\x1b[24^', CTRL_F12),

    # xterm (Debian)
              
    ('\x1b[1;2A', SHIFT_UP),
    ('\x1b[1;2B', SHIFT_DOWN),
    ('\x1b[1;2D', SHIFT_LEFT),
    ('\x1b[1;2C', SHIFT_RIGHT),
    ('\x1b[1;5A', CTRL_UP),
    ('\x1b[1;5B', CTRL_DOWN),
    ('\x1b[1;5D', CTRL_LEFT),
    ('\x1b[1;5C', CTRL_RIGHT),
    
    ('\x1b[5;5~', CTRL_PGUP),
    ('\x1b[6;5~', CTRL_PGDN),

    ('\x1b[H', HOME),
    ('\x1b[F', END),
    ('\x1b[1;2H', SHIFT_HOME),
    ('\x1b[1;2F', SHIFT_END),
    ('\x1b[1;5H', CTRL_HOME),
    ('\x1b[1;5F', CTRL_END),

    ('\x1b[2;2~', SHIFT_INSERT),
    ('\x1b[3;2~', SHIFT_DELETE),
    ('\x1b[2;5~', CTRL_INSERT),
    ('\x1b[3;5~', CTRL_DELETE),

    ('\x1b[OP^', F1),
    ('\x1b[OQ^', F2),
    ('\x1b[OR^', F3),
    ('\x1b[OS^', F4),
    ('\x1b[1;2P^', SHIFT_F1),
    ('\x1b[1;2Q^', SHIFT_F2),
    ('\x1b[1;2R^', SHIFT_F3),
    ('\x1b[1;2S^', SHIFT_F4),
    ('\x1b[15;2~', SHIFT_F5),
    ('\x1b[17;2~', SHIFT_F6),
    ('\x1b[18;2~', SHIFT_F7),
    ('\x1b[19;2~', SHIFT_F8),
    ('\x1b[20;2~', SHIFT_F9),
    ('\x1b[21;2~', SHIFT_F10),
    ('\x1b[23;2$', SHIFT_F11),
    ('\x1b[24;2$', SHIFT_F12),
    ('\x1b[1;5P^', CTRL_F1),
    ('\x1b[1;5Q^', CTRL_F2),
    ('\x1b[1;5R^', CTRL_F3),
    ('\x1b[1;5S^', CTRL_F4),
    ('\x1b[15;5~', CTRL_F5),
    ('\x1b[17;5~', CTRL_F6),
    ('\x1b[18;5~', CTRL_F7),
    ('\x1b[19;5~', CTRL_F8),
    ('\x1b[20;5~', CTRL_F9),
    ('\x1b[21;5~', CTRL_F10),
    ('\x1b[23;5$', CTRL_F11),
    ('\x1b[24;5$', CTRL_F12),
              
    # gnome-terminal (Debian)

    ('\x1bOH', HOME),
    ('\x1bOF', END),

    # konsole (Debian)

    ('\x1b[O2P^', SHIFT_F1),
    ('\x1b[O2Q^', SHIFT_F2),
    ('\x1b[O2R^', SHIFT_F3),
    ('\x1b[O2S^', SHIFT_F4),
    ('\x1b[O5P^', CTRL_F1),
    ('\x1b[O5Q^', CTRL_F2),
    ('\x1b[O5R^', CTRL_F3),
    ('\x1b[O5S^', CTRL_F4),

    # pterm (Debian - based on putty)
    
    ('\x1b[1~', HOME),
    ('\x1b[4~', END),
]


_tree = None


def _build_tree():
    global _tree
    _tree = {}
    for seq,key in _codes:
        d = _tree
        for c in seq:
            sub = d.get(c)
            if not sub:
                sub = d[c] = {}
            d = sub
        d[None] = key


def match(s, timeout=0):
    if not _tree:
        _build_tree()

    d = _tree
    for i,c in enumerate(s):
        sub = d.get(c)
        if not sub:
            if d.get(None):
                # the sequence is definitely over, and there's an exact match
                return d[None], s[i:]
            else:
                # the sequence tree we were searching has no options left;
                # it's not a sequence, just a regular char.
                return s[0],s[1:]
        d = sub
    # we ran out of chars
    if d.get(None) and (len(d.keys()) == 1 or timeout):
        # there's only one possible match
        return d.get(None),''
    elif timeout and s:
        # timed out waiting for the end of a sequence
        return s[0],s[1:]
    else:
        # we haven't yet eliminated all the options; wait for more bytes
        return None,s


class Processor:
    def __init__(self):
        self.queue = ''
        self.time = 0

    def add(self, s, timestamp):
        if s:
            self.queue += s
            self.time = timestamp

    def next(self, timestamp):
        timeout = (timestamp and timestamp-self.time > 0.25)
        (k,self.queue) = match(self.queue, timeout=timeout)
        return k

    def iter(self, timestamp):
        while 1:
            k = self.next(timestamp)
            if k:
                yield k
            else:
                break
