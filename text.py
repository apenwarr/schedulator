#!/usr/bin/env python
import sys
import schedulator

def dump(prefix, t):
    print '%s%s%s' % (t.donedate and '.' or '', prefix, t)
    if t.note:
        for l in t.note.split('\n'):
            print '%s        %s' % (prefix, l)
    for sub in t.subtasks:
        dump(prefix+'    ', sub)


def print_pretty(tasks):
    for t in tasks:
        dump('', t)
        print


def print_pretty_totals():
    print '%-20s %10s %10s %10s  %-10s' % ('', 'Estimate',
                                         'Elapsed', 'Remain', 'Date')
    mr = 0
    mrn = 'None'
    for p in sorted(schedulator.people_unique,
                    cmp = lambda a,b: int(b.remain() - a.remain())):
        if p.remain() > mr:
            mr = p.remain()
            mrn = p.name
        if p.remain() or p.time_queued:
            print '%-20s %9.1fw %9.1fw %9.1fw  %10s' % \
                (p.name, p.time_queued/40.0, p.time_done/40.0, p.remain()/40.0,
                 p.date)

    print '\nCritical path: %s (%.2f days)' % (mrn, mr/8)


sched = schedulator.Schedule(sys.stdin)

print_pretty(sched.subtasks)
print_pretty_totals()
