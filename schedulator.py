import sys, re, time

unitmap = dict(w=40, d=8, h=1, m=1./60, s=1./60/60)
DAY = 24.0*60*60
WEEK = 7.0*DAY

def _parse_date(s):
    return time.mktime(time.strptime(str(s), '%Y-%m-%d'))


def _today():
    lt = time.localtime(time.time())
    return time.mktime((lt[0],lt[1],lt[2],0,0,0,0,0,0))
today = _today()


def render_time(t):
    return time.strftime('%Y-%m-%d', time.localtime(t))


def render_est(e):
    if not e:
        return ''
    if e >= 60:
        return '%.1fw' % (e/40.0)
    if e >= 16:
        return '%.1fd' % (e/8.0)
    else:
        return '%.1fh' % e


class SDate:
    def __init__(self, start):
        self.date = _parse_date(start)
        self._fixdate()

    def __str__(self):
        return render_time(self.date)

    def __cmp__(x, y):
        return cmp(x.date, y.date)

    def _fixdate(self):
        while 1:
            (y,m,d,h,m,s,wday,yday,isdst) = time.localtime(self.date)
            if wday == 5 or wday == 6:
                self.date += DAY
            else:
                break

    def add(self, days):
        # print 'adding %g days to %s' % (days, self)
        assert(days < 10000)
        assert(days >= 0)
        while days > 40:  # fast forward a week at a time
            self.date += WEEK
            days -= 40
        while days >= 1:
            self.date += DAY
            self._fixdate()
            days -= 1
        self.date += days*DAY
        self._fixdate()
        # print '  result: %s' % self

    def copy(self):
        return SDate(self)

    def fastforward(self, sdate):
        if self < sdate:
            self.date = sdate.date
            self._fixdate()


class Person:
    def __init__(self, name):
        self.name = name
        self.loadfactor = 1.0
        self.date = SDate('1970-01-01')
        self.time_queued = 0
        self.time_done = 0

    def __str__(self):
        return self.name

    def addtime(self, hestimate, helapsed):
        lf = self.loadfactor
        self.time_queued += hestimate or 0
        self.time_done += helapsed or 0
        remain = (hestimate or 0)*lf - (helapsed or 0)*lf
        self.date.add(remain/8.0)

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
        self.duedate = None

    def __str__(self):
        s = ''
        #if self.parent:
        #    s += '%s: ' % self.parent.title
        s += self.title
        if self.owner:
            s += ' (owner:%s)' % self.owner
        if self.donedate:
            s += ' (done:%s)' % render_time(self.donedate)
        if self.estimate != None:
            s += ' (est:%s)' % render_est(self.estimate)
        if self.elapsed:
            s += ' (elapsed:%s)' % render_est(self.elapsed)
        if self.subtasks:
            s += ' (total:%s)' % render_est(self.total_remain())
        if self.duedate:
            s += ' (due:%s)' % self.duedate
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

    def linearize(self, parent_after_children = 0):
        for t in self.subtasks:
            if not parent_after_children:
                yield t
            for tt in t.linearize(parent_after_children):
                yield tt
            if parent_after_children:
                yield t

    def remain(self):
        return (self.estimate or 0) - self.elapsed

    def total_estimate(self):
        tt = self.estimate or 0
        for t in self.subtasks:
            tt += t.total_estimate()
        return tt

    def total_elapsed(self):
        tt = self.elapsed or 0
        for t in self.subtasks:
            tt += t.total_elapsed()
        return tt

    def total_remain(self):
        tt = self.remain()
        for t in self.subtasks:
            tt += t.total_remain()
        return tt

    def set_duedate(self):
        mindate = self.owner and self.owner.date or SDate('1970-01-01')
        for t in self.subtasks:
            if t.duedate > mindate:
                mindate = t.duedate
        self.duedate = mindate.copy()

def _expand(tabtext):
    out = ''
    for c in tabtext:
        if c == '\t':
            nextstop = (len(out)+8)/8*8
            out += ' ' * (nextstop-len(out))
        else:
            out += c
    return out

def read_tasks(prefix, lines):
    out = []
    while lines:
        (dot, pre, text, post) = re.match(r'(\.?)(\s*)(.*)(\s*)', 
                                     lines[-1]).groups()
        pre = _expand(pre)
        if not text:
            lines.pop()
            continue
        if text.startswith('#'):
            # FIXME: we should be doing the duedate calculation at parse
            # time, so that the following directives are interpreted at the
            # right times.  Or maybe store the directives inline as tasks?
            g = re.match(r'#date(\s+(\S+))?\s+(\d\d\d\d-\d\d-\d\d)', text)
            if g:
                (junk, user, date) = g.groups()
                sdate = SDate(date)
                if user:
                    p = people.get(user.lower())
                    if p: p.date.fastforward(sdate)
                else:
                    for p in people_unique:
                        p.date.fastforward(sdate)
                lines.pop()
                continue
            g = re.match(r'#loadfactor(\s+(\S+))?\s+(\d+\.?\d*)', text)
            if g:
                (junk, user, loadfactor_s) = g.groups()
                loadfactor = float(loadfactor_s)
                if user:
                    p = people.get(user.lower())
                    if p: p.loadfactor = loadfactor
                else:
                    for p in people_unique:
                        p.loadfactor = loadfactor
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


for line in open('users'):
    names = line.split()
    if names and names[0]:
        p = Person(names[0])
        people_unique.append(p)
        for name in names:
            people[name.lower()] = p

class Schedule(Task):
    def __init__(self, f):
        Task.__init__(self)
        lines = f.readlines()
        lines.reverse()
        tasks = read_tasks('', lines)
        for t in tasks:
            self.add(t)

        for t in self.linearize(parent_after_children=1):
            if (t.elapsed or t.estimate) and not t.owner:
                t.owner = nobody
            if t.owner:
                t.owner.addtime(t.estimate, t.elapsed)
            t.set_duedate()
