#!/usr/bin/env python
import sys, re, time

unitmap = dict(w=40, d=8, h=1, m=1./60, s=1./60/60)


def _today():
    return int(time.time() / 3600 / 24) * 3600 * 24
today = _today()


def _render_time(t):
    return time.strftime('%Y-%m-%d', time.localtime(t))


def _render_est(e):
    if e >= 16:
        return '%gd' % (e/8.0)
    else:
        return '%gh' % e


class Person:
    def __init__(self, name):
        self.name = name
        self.date = time.mktime(time.strptime('1970-01-01', '%Y-%m-%d'))
        self.time_queued = 0
        self.time_done = 0

    def __str__(self):
        return self.name

    def addtime(self, t):
        self.time_queued += t
        # FIXME: update date

    def addcompleted(self, t):
        # FIXME: this isn't what we'll do eventually
        self.time_done += t

    def remain(self):
        return self.time_queued - self.time_done

nobody = Person('-Unassigned-')
people = {nobody.name: nobody}
people_unique = [nobody]

class Task:
    def __init__(self):
        self.parent = None
        self.title = None
        self.note = None
        self.owner = None
        self.estimate = None
        self.elapsed = 0
        self.subtasks = []
        self.donedate = None

    def __str__(self):
        s = ''
        #if self.parent:
        #    s += '%s: ' % self.parent.title
        s += self.title
        if self.owner:
            s += ' (owner:%s)' % self.owner
        if self.donedate:
            s += ' (done:%s)' % _render_time(self.donedate)
        if self.estimate != None:
            s += ' (est:%s)' % _render_est(self.estimate)
        if self.elapsed:
            s += ' (elapsed:%s)' % _render_est(self.elapsed)
        if self.subtasks:
            s += ' (total:%s)' % _render_est(self.total())
        #if self.note:
        #    s += ' {%s}' % self.note
        return s

    def _fixowners(self, owner):
        for s in self.subtasks:
            if not s.owner:
                s.owner = owner
                s._fixowners(owner)

    def add(self, sub):
        assert(not sub.parent)
        self.subtasks.append(sub)
        sub.parent = self
        if self.owner and not sub.owner:
            sub.owner = self.owner
            sub._fixowners(self.owner)

    def depth(self):
        if self.parent:
            return self.parent.depth() + 1
        return 0

    def linearize(self):
        for t in self.subtasks:
            yield t
            for tt in t.linearize():
                yield tt

    def remain(self):
        return (self.estimate or 0) - self.elapsed

    def total(self):
        tt = self.remain()
        for t in self.subtasks:
            tt += t.total()
        return tt


def read_tasks(prefix, lines):
    out = []
    while lines:
        (dot, pre, text, post) = re.match(r'(\.?)(\s*)(.*)(\s*)', 
                                     lines[-1]).groups(0)
        if not text:
            lines.pop()
            continue
        if not pre.startswith(prefix):
            break
        elif len(pre) > len(prefix):
            subtasks = read_tasks(pre, lines)
            is_real = 0
            for t in subtasks:
                if (t.estimate or t.elapsed or t.subtasks 
                    or t.owner or t.donedate):
                    is_real = 1
                    break
            if is_real:
                for t in subtasks:
                    out[-1].add(t)
            else:
                nl = []
                for t in subtasks:
                    subnote = t.title
                    if t.note:
                        subnote += '\n' + re.sub(re.compile(r'^', re.M),
                                                 '\t', t.note)
                    nl.append(subnote)
                out[-1].note = '\n'.join(nl)
        else:
            lines.pop()
            t = Task()
            words = text.split()
            for (i,word) in enumerate(words):
                # eg: [5d]
                # or: [3h/1d]
                x = re.match(r'\[((\d+(\.\d*)?)([wdhms]?)/)?(\d+(\.\d*)?)([wdhms])\]$', word)
                if x:
                    (j1, elnum, j2, elunit, estnum, j3, estunit) \
                        = x.groups()
                    if not elunit:
                        elunit = estunit
                    if elnum and elunit:
                        t.elapsed = float(elnum)*unitmap[elunit]
                    if estnum and estunit:
                        t.estimate = float(estnum)*unitmap[estunit]
                    words[i] = ''
            while words and not words[0]:
                words = words[1:]
            if words and (words[0] == '.' or words[0] == 'DONE'):
                t.donedate = today
                words = words[1:]
            if dot and not t.donedate:
                t.donedate = today
            isname = words and words[0] and words[0].endswith(':')
            name = isname and words[0][:-1].lower()
            if name and people.get(name):
                t.owner = people.get(name)
                words = words[1:]
            t.title = ' '.join(words).strip()
            if t.elapsed > 0 and t.elapsed == t.estimate and not t.donedate:
                t.donedate = today
            if t.donedate and t.estimate:
                t.elapsed = t.estimate
            out.append(t)
    return out

def dump(prefix, t):
    print '%s%s%s' % (t.donedate and '.' or '', prefix, t)
    if t.note:
        for l in t.note.split('\n'):
            print '%s        %s' % (prefix, l)
    for sub in t.subtasks:
        dump(prefix+'    ', sub)


for line in open('users'):
    names = line.split()
    if names and names[0]:
        p = Person(names[0])
        people_unique.append(p)
        for name in names:
            people[name.lower()] = p

lines = sys.stdin.readlines()
lines.reverse()
root = Task()
tasks = read_tasks('', lines)
for t in tasks:
    root.add(t)

for t in tasks:
    dump('', t)
    print

for t in root.linearize():
    if (t.elapsed or t.estimate) and not t.owner:
        t.owner = nobody
    if t.elapsed:
        t.owner.addcompleted(t.elapsed)
    if t.estimate:
        t.owner.addtime(t.estimate)

print '%-20s %10s %10s %10s' % ('', 'Estimate', 'Elapsed', 'Remain')
mr = 0
mrn = 'None'
for p in sorted(people_unique, cmp = lambda a,b: int(b.remain() - a.remain())):
    if p.remain() > mr:
        mr = p.remain()
        mrn = p.name
    if p.remain() or p.time_queued:
        print '%-20s %9.1fd %9.1fd %9.1fd' % \
            (p.name, p.time_queued/8, p.time_done/8, p.remain()/8)

print '\nCritical path: %s (%.2f days)' % (mrn, mr/8)
