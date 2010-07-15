from wvtest import *
from pylogic import keys

@wvtest
def test_keys():
    WVPASSEQ(keys.match(chr(8)), (keys.BACKSPACE, ''))

    s = 'abca\x1b[OQ^\x1b[Abob\x7f\x1b[\x1b['
    (k1,s) = keys.match(s)
    (k2,s) = keys.match(s)
    (k3,s) = keys.match(s)
    (k4,s) = keys.match(s)
    (k5,s) = keys.match(s)
    (k6,s) = keys.match(s)
    (k7,s) = keys.match(s)
    (k8,s) = keys.match(s)
    (k9,s) = keys.match(s)
    (k10,s) = keys.match(s)
    (k11,s) = keys.match(s)
    (k12,s) = keys.match(s)
    (k13,s) = keys.match(s)
    (k14,s) = keys.match(s)
    (k15,s) = keys.match(s, timeout=True)
    (k16,s) = keys.match(s, timeout=True)
    (k17,s) = keys.match(s, timeout=True)
    WVPASSEQ(k1, 'a')
    WVPASSEQ(k2, 'b')
    WVPASSEQ(k3, 'c')
    WVPASSEQ(k4, 'a')
    WVPASSEQ(k5, keys.F2)
    WVPASSEQ(k6, keys.UP)
    WVPASSEQ(k7, 'b')
    WVPASSEQ(k8, 'o')
    WVPASSEQ(k9, 'b')
    WVPASSEQ(k10, keys.BACKSPACE)
    WVPASSEQ(k11, '\x1b')
    WVPASSEQ(k12, '[')
    WVPASSEQ(k13, None)
    WVPASSEQ(k14, None)
    WVPASSEQ(k15, '\x1b')
    WVPASSEQ(k16, '[')
    WVPASSEQ(k17, None)


@wvtest
def test_key_processor():
    p = keys.Processor()
    p.add('ab\x7f\x1b', 1)
    WVPASSEQ(p.next(1), 'a')
    WVPASSEQ(p.next(1), 'b')
    WVPASSEQ(p.next(1), keys.BACKSPACE)
    WVPASSEQ(p.next(1), None)
    WVPASSEQ(p.next(5), keys.ESC)
