#!/usr/bin/env python
import sys, re, datetime

people = ['AP', 'LK']
unitmap = dict(w=40, d=8, h=1, m=1./60, s=1./60/60)

class Task:
    def __init__(self):
        self.parent = None
        self.title = None
        self.owner = None
        self.estimate = None
        self.elapsed = 0
        self.subtasks = []
        self.donedate = None

    def __str__(self):
        s = ''
        if self.parent:
            s += '%s: ' % self.parent.title
        s += self.title
        if self.owner:
            s += ' (owner:%s)' % self.owner
        if self.donedate:
            s += ' (done:%s)' % self.donedate
        if self.estimate != None:
            s += ' (est:%gh)' % self.estimate
        if self.elapsed:
            s += ' (elapsed:%gh)' % self.elapsed
        return s

    def add(self, sub):
        assert(not sub.parent)
        self.subtasks.append(sub)
        sub.parent = self


def read_tasks(prefix, lines):
    out = []
    while lines:
        (pre, text, post) = re.match(r'(\s*)(.*)(\s*)', lines[-1]).groups(0)
        if not text:
            lines.pop()
            continue
        if not pre.startswith(prefix):
            break
        elif len(pre) > len(prefix):
            for t in read_tasks(pre, lines):
                out[-1].add(t)
        else:
            lines.pop()
            t = Task()
            words = text.split()
            while words:
                if words[0] in people:
                    t.owner = words[0]
                elif words[0] == 'DONE':
                    t.donedate = datetime.date.today()
                else:
                    # eg: [5d]
                    # or: [3h/1d]
                    x = re.match(r'\[((\d+(\.\d*)?)([wdhms])/)?(\d+(\.\d*)?)([wdhms])\]', words[0])
                    if x:
                        (junk1, elnum, junk2, elunit, estnum, junk3, estunit) = x.groups()
                        if elnum and elunit:
                            t.elapsed = float(elnum)*unitmap[elunit]
                        if estnum and estunit:
                            t.estimate = float(estnum)*unitmap[estunit]
                    else:
                        break
                words = words[1:]
            t.title = ' '.join(words)
            out.append(t)
    return out


def dump(prefix, t):
    print '%s%s' % (prefix, t)
    for sub in t.subtasks:
        dump(prefix+'    ', sub)

lines = sys.stdin.readlines()
lines.reverse()

tasks = read_tasks('', lines)
for t in tasks:
    dump('', t)
