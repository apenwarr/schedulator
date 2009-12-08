import sys, re, time

unitmap = dict(w=40, d=8, h=1, m=1./60, s=1./60/60)
DAY = 24.0*60*60
WEEK = 7.0*DAY

def _parse_date(s):
    if isinstance(s, int) or isinstance(s, float):
        return s
    else:
        return time.mktime(time.strptime(str(s), '%Y-%m-%d'))


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
        self.vacations = []
        self.date = _parse_date(start)

    def __str__(self):
        return render_time(self.date)

    def __repr__(self):
        return 'SDate(%s)' % str(self)

    def __cmp__(x, y):
        return cmp(x.date, y.date)

    def _is_vacation_day(self):
        while self.vacations and self.vacations[0][1] < self:
            self.vacations = self.vacations[1:]
        return self.vacations and self.vacations[0][0] <= self

    def _fixdate(self):
        while 1:
            (y,m,d,h,m,s,wday,yday,isdst) = time.localtime(self.date)
            if wday == 5 or wday == 6:
                self.date += DAY
            elif self._is_vacation_day():
                self.date += DAY
            else:
                break

    def add(self, days):
        # print 'adding %g days to %s' % (days, self)
        assert(days < 10000)
        assert(days >= 0)
        self._fixdate()
        #while days > 40:  # fast forward a week at a time
        #    self.date += WEEK
        #    days -= 40
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


def _today():
    lt = time.localtime(time.time())
    return time.mktime((lt[0],lt[1],lt[2],0,0,0,0,0,0))
today = _today()
stoday = SDate(today)


class Person:
    def __init__(self, name):
        self.name = name
        self.loadfactor = 1.0
        self.date = SDate('1970-01-01')
        self.date_set_explicitly = 0
        self.time_queued = 0
        self.time_done = 0

    def __str__(self):
        return self.name

    def __cmp__(self, y):
        return cmp(self.name, y and y.name or '')

    def __hash__(self):
        return hash(self.name)

    def addtime(self, hestimate, helapsed):
        lf = self.loadfactor
        self.time_queued += hestimate or 0
        remain = (hestimate or 0)*lf - (helapsed or 0)*lf
        self.date.add(remain/8.0)

    def add_elapsed(self, helapsed):
        lf = self.loadfactor
        self.time_done += helapsed or 0
        self.date.add((helapsed or 0)*lf/8.0)

    def remain(self):
        return self.time_queued - self.time_done


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

    def flat_title(self):
        if self.parent and self.parent.title:
            return '%s: %s' % (self.parent.flat_title(), self.title)
        return self.title

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

    def total_children(self, user):
        c = 0
        for p in self.subtasks:
            if p.contains_user(user):
                c += 1 + p.total_children(user)
        return c

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

    def calc_duedate(self):
        mindate = self.owner and self.owner.date or SDate('1970-01-01')
        for t in self.subtasks:
            if t.duedate > mindate:
                mindate = t.duedate
        return mindate.copy()

    def late(self):
        return (self.duedate and (self.duedate.date < today)
                and not self.donedate);

    def contains_user(self, user):
        if not user:
            return 1
        if self.owner == user:
            return 1
        for t in self.subtasks:
            c = t.contains_user(user)
            if c: return 1
        return 0


def _expand(tabtext):
    out = ''
    for c in tabtext:
        if c == '\t':
            nextstop = (len(out)+8)/8*8
            out += ' ' * (nextstop-len(out))
        else:
            out += c
    return out


class Schedule(Task):
    def __init__(self, f):
        Task.__init__(self)
        self.nobody = Person('-Unassigned-')
        self.people = {self.nobody.name.lower(): self.nobody}
        self.vacations = []

        self.doneroot = Task()
        self.doneroot.title = 'Elapsed Time'
        self.add(self.doneroot)

        self.vacationroot = Task()
        self.vacationroot.title = 'Vacations'
        self.add(self.vacationroot)

        lines = f.readlines()
        lines.reverse()
        tasks = self.read_tasks('', lines)
        for t in tasks:
            self.add(t)

        self.vacations.sort()
        for v in self.vacations:
            if v[2]:
                pl = [v[2]]
            else:
                pl = set(self.people.values()) - set([self.nobody])
            for p in pl:
                vt = Task()
                vt.title = 'Vacation: %s to %s (%s)' % (v[0], v[1], p.name)
                vt.owner = p
                if v[1] <= stoday:
                    vt.duedate = vt.donedate = v[1]
                else:
                    vt.duedate = v[0]
                p.date.vacations.append(v)
                self.vacationroot.add(vt)

        self.schedule_tasks()

    def make_person(self, name):
        p = self.people.get(name.lower())
        if not p:
            p = Person(name)
            self.people[name.lower()] = p
        return p

    def add_vacation(self, user, startdate, enddate):
        assert(startdate)
        p = user and self.make_person(user) or None
        if not enddate: enddate = startdate
        self.vacations.append((SDate(startdate), SDate(enddate), p))

    def read_tasks(self, prefix, lines):
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
                        p = self.make_person(user)
                        if p:
                            p.date.fastforward(sdate)
                            p.date_set_explicitly = 1
                    else:
                        for p in set(self.people.values()):
                            if not p.date_set_explicitly:
                                p.date.fastforward(sdate)
                                p.date_set_explicitly = 1
                    lines.pop()
                    continue
                g = re.match(r'#vacation\s+(\d\d\d\d-\d\d-\d\d)(\s+(\d\d\d\d-\d\d-\d\d))?', text)
                if g:
                    (d1, junk, d2) = g.groups()
                    self.add_vacation(None, d1, d2)
                    lines.pop()
                    continue
                g = re.match(r'#vacation(\s+(\S+))?\s+(\d\d\d\d-\d\d-\d\d)(\s+(\d\d\d\d-\d\d-\d\d))', text)
                if g:
                    (junk, user, d1, junk2, d2) = g.groups()
                    self.add_vacation(user, d1, d2)
                    lines.pop()
                    continue
                g = re.match(r'#loadfactor(\s+(\S+))?\s+(\d+\.?\d*)', text)
                if g:
                    (junk, user, loadfactor_s) = g.groups()
                    loadfactor = float(loadfactor_s)
                    if user:
                        p = self.make_person(user)
                        if p: p.loadfactor = loadfactor
                    else:
                        for p in set(self.people.values()):
                            p.loadfactor = loadfactor
                    lines.pop()
                    continue
                g = re.match(r'#alias\s+(.*)', text)
                if g:
                    names = g.groups()[0].split()
                    if names and names[0]:
                        p = self.make_person(names[0])
                        for name in names:
                            self.people[name.lower()] = p
                    lines.pop()
                    continue
            if not pre.startswith(prefix):
                break
            elif len(pre) > len(prefix):
                subtasks = self.read_tasks(pre, lines)
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
                if name and self.people.get(name):
                    t.owner = self.people.get(name)
                    words = words[1:]
                t.title = ' '.join(words).strip()
                if t.elapsed > 0 and t.elapsed == t.estimate and not t.donedate:
                    t.donedate = today
                if t.donedate and t.estimate:
                    t.elapsed = t.estimate
                out.append(t)
        return out

    def schedule_tasks(self):
        # allocate time for all completed tasks first
        for t in self.linearize(parent_after_children=1):
            if t.donedate:
                if t.owner and t.elapsed:
                    t.owner.add_elapsed(t.elapsed)
                # FIXME: it would be smarter to record the *actual*
                # completion date, but we don't yet.
                t.donedate = t.duedate = t.calc_duedate()

        # allocate time for all elapsed-but-incomplete tasks
        for t in self.linearize(parent_after_children=1):
            if t.elapsed and t.owner and not t.donedate:
                nt = Task()
                ttl = t.flat_title()
                nt.title = 'Partially done: ' + ttl
                nt.owner = t.owner
                nt.estimate = nt.elapsed = t.elapsed
                nt.owner.add_elapsed(nt.elapsed)
                nt.donedate = nt.duedate = nt.calc_duedate()
                self.doneroot.add(nt)
                
        # calculate due dates for incomplete tasks
        for t in self.linearize(parent_after_children=1):
            if t.duedate: # already set
                continue
            if (t.elapsed or t.estimate) and not t.owner:
                t.owner = self.nobody
            if t.owner:
                t.owner.addtime(t.estimate, t.elapsed)
            if not t.donedate:
                t.duedate = t.calc_duedate()
