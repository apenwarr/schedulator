#!/usr/bin/env python
import sys, re
import schedulator

root= schedulator.Schedule(sys.stdin)

print 'Person,Task,,,,,,,,,,Remain (days),,,,,,,,,'
for t in root.linearize():
    depth = t.depth()-1
    before = ['']*depth
    after = ['']*(9-depth)
    title = re.sub(',', ';', t.title)
    print ','.join([t.owner and t.owner.name or ''] +
                   before + [title] + after + 
                   before + ['%.1f' % (t.total()/8.0)] + after)
