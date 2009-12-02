#!/usr/bin/env python
import sys, re
import schedulator

root= schedulator.Schedule(sys.stdin)

print 'Person,Task,,,,,,,,,,Due,,,,,,,,,'
for t in root.linearize():
    depth = t.depth()-1
    before = ['']*depth
    after = ['']*(9-depth)
    title = re.sub(',', ';', t.title)
    print ','.join([t.owner and t.owner.name or ''] +
                   before + [title] + after + 
                   before + [str(t.duedate)] + after)
