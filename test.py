import sys
import clr

sys.path.Add('/home/apenwarr/src/vx-lin/wvdotnet')
clr.AddReference('wv.dll')
from Wv import *

class TestyRow:
    r = None
    
    def __init__(self, dbirow):
        self.r = dbirow
        
    def __repr__(self):
        return self.r.data.__repr__()
        
    def __getitem__(self, x):
        return self.r[x].inner
        
    def __getattr__(self, x):
        return self.__getitem__(x)

class Testy:
    dbi = None

    def __init__(self, moniker):
        self.dbi = WvDbi.create(moniker)
    
    def select(self, q, *args):
        return [TestyRow(r) for r in self.dbi.select(q, *args)]

d = Testy('mssql://sa:scs@pwc-averyp/avery-ams')

for r in d.select('select top 5 * from Names where NameType=@col0', 'COUNTRY'):
    print r.NameIdCode, r.LastName
    print list(r.r)
