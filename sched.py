#!/usr/bin/env python
import sys, re, time

people = ['AVERY', 'LUKE', 'ZICK', 'EDUARDO',
          'WOOI', 'HUGH', 'RODRIGO', 'BILL']
unitmap = dict(w=40, d=8, h=1, m=1./60, s=1./60/60)


def _today():
    return int(time.time() / 3600 / 24) * 3600 * 24
today = _today()


def _render_time(t):
    return time.strftime('%Y-%m-%d', time.gmtime(t))


def _render_est(e):
    if e >= 16:
        return '%gd' % (e/8.0)
    else:
        return '%gh' % e


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
        if self.note:
            s += ' {%s}' % self.note
        return s

    def add(self, sub):
        assert(not sub.parent)
        self.subtasks.append(sub)
        sub.parent = self
        if self.owner and not sub.owner:
            sub.owner = self.owner

    def depth(self):
        if self.parent:
            return self.parent.depth() + 1
        return 0


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
            subtasks = read_tasks(pre, lines)
            is_real = 0
            for t in subtasks:
                if t.estimate or t.elapsed or t.subtasks or t.owner:
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
                        subnote += '\n' + re.sub(re.compile(r'^', re.M), '\t', t.note)
                    nl.append(subnote)
                out[-1].note = '\n'.join(nl)
        else:
            lines.pop()
            t = Task()
            words = text.split()
            for (i,word) in enumerate(words):
                if word in people:
                    t.owner = words[0]
                    words[i] = ''
                elif word == 'DONE':
                    t.donedate = today
                    words[i] = ''
                else:
                    # eg: [5d]
                    # or: [3h/1d]
                    x = re.match(r'\[((\d+(\.\d*)?)([wdhms])/)?(\d+(\.\d*)?)([wdhms])\]', word)
                    if x:
                        (j1, elnum, j2, elunit, estnum, j3, estunit) \
                            = x.groups()
                        if elnum and elunit:
                            t.elapsed = float(elnum)*unitmap[elunit]
                        if estnum and estunit:
                            t.estimate = float(estnum)*unitmap[estunit]
                        words[i] = ''
            t.title = ' '.join(words).strip()
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
