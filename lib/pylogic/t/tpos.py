from wvtest import *
from pylogic import Pos, Size, Area

@wvtest
def test_pos():
    p1 = Pos(0,0)
    p2 = Pos(-5,-10)
    p3 = Pos(1,1)
    p4 = Pos(1,0)
    s1 = Size(3,4)
    s2 = Size(300,400)
    s3 = Size(-5,-10)
    a1 = Area(p1, s1)
    a2 = Area(p2, s2)
    a3 = Area(p2, s3)

    WVPASSNE(p1, p2)
    WVPASSEQ(p1, p1)
    WVPASSNE(s1, p3+p4)
    WVPASSEQ(s1, p3+p3+p3+p3-p4)
    WVPASSEQ(s3, p2)

    WVPASSEQ(a1.coords(), (0,0,2,3))
    WVPASSEQ(a2.coords(), (-5,-10,294, 389))
    WVPASSEQ(a3.coords(), (-5,-10,-6,-11)) # areas can't have negative size

    WVPASSEQ(a1.intersect(a2).coords(), (0,0,2,3))
    WVPASSEQ(a1.intersect(a3).coords(), (0,0,-1,-1))
    WVPASSEQ(a1.intersect(a3).size(), Size(0,0))
    WVPASSEQ(a2.intersect(a3).coords(), (-5,-10,-6,-11))
    
    WVPASSEQ(a1.intersect(a2), a2.intersect(a1))
    WVPASSEQ(a2.intersect(a3), a3.intersect(a2))

    # arguably, points should be immutable so this sort of thing isn't
    # possible, and therefore isn't confusing.
    p2x = p2
    WVPASS(p2 is p2x)
    p2 += p2 + p3
    WVPASSEQ(p2, Pos(-9,-19))
    WVPASS(p2 is p2x)
    WVPASSEQ(p2x, Pos(-9, -19)) # this makes sense, but is probably non-obvious
