#!/usr/bin/env python
import sys, re, os
import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.escape
from tornado.web import HTTPError
from bog import options, schedulator, repo
from bog.helpers import *


def get_sched(integrate_slips=False):
    # FIXME: this is terrible.  Make a real module for reading email
    # contents...
    lines = []
    lines += open(mainpath).readlines()
    
    lastdir = ['']
    def f(junk, dir, names):
        names.sort()
        #log('dir: %r\n' % dir)
        if dir[-4:] in ['/cur', '/new']:
            for n in names[:10]:
                if n.endswith('~') or n.startswith('.'):
                    continue
                #log('reading: %r\n' % n)
                g = re.split(r'\n--=---{0,2}\n',
                             open(os.path.join(dir, n)).read())
                if g:
                    top = g[0].strip()
                    g2 = re.search(re.compile(r'^Subject: (.*)$', re.M), top)
                    subj = g2 and g2.group(1) or 'Untitled'
                    sch = g[1].strip()
                    #log('%s\n' % sch)
                    folder = dir[len(bogdir):-4]
                    if lastdir[0] != folder:
                        lines.append(folder)
                        lastdir[0] = folder
                    lines.append('\t' + subj)
                    for s in sch.split('\n'):
                        lines.append(re.sub(re.compile(r'^([^#])', re.M),
                                            r'\t\t\1', s))
    os.path.walk(bogdir, f, None)
    #log('\n\n\n')
    #for l in lines:
    #    log('%s\n' % l)
    return schedulator.Schedule(lines,
                                integrate_slips=integrate_slips)


def countify(l):
    last = None
    count = 0
    for i in l:
        if i == last:
            count += 1
        else:
            if count:
                yield (last, count)
            last = i
            count = 1
    if count:
        yield (last, count)


class _Handler(tornado.web.RequestHandler):
    def render(self, template,
               page, subpage=None,
               submenu=[], submenu_title=None, **kwargs):
        menu = [
            ('/bog', 'Bogs'),
            ('/pri', 'Prioritize'),
            ('/sched', 'Schedules'),
            ('/grid', 'TimeGrid'),
        ]
        tornado.web.RequestHandler.render(self, template,
                                          page=page, subpage=subpage,
                                          menu=menu, submenu=submenu,
                                          submenu_title=submenu_title,
                                          **kwargs)


class DashboardHandler(_Handler):
    def get(self):
        self.render('base.html', page='', title='Dashboard')


class BogIndexHandler(_Handler):
    def get(self):
        self.render('unimplemented.html', page='/bog',
                    title='Bog List')


class PriHandler(_Handler):
    def get(self):
        self.render('unimplemented.html', page='/pri',
                    title='Release Prioritization')


class SchedHandler(_Handler):
    def render(self, template, tasks, subpage, **kwargs):
        submenu = [
            ('/sched', '-All-'),
            ('/sched/edit', '-Edit-'),
        ]
        for p in sorted(set(tasks.people.values())):
            if p.time_queued:
                submenu.append(('/sched/user/%s' % p.name, p.name))
        return _Handler.render(self, template,
                               page='/sched',
                               submenu=submenu, subpage=subpage,
                               tasks=tasks,
                               **kwargs)
    
    def get(self, username = None):
        s = get_sched()
        if username:
            username = tornado.escape.url_unescape(username)
        user = username and s.people.get(username.lower()) or None
        tasks = []
        d = dict(tasks = s,
                 subpage = user and ('/sched/user/%s' % user.name) 
                     or '/sched',
                 title = user and ('Schedulator: %s' % user.name) 
                     or 'Schedulator',
                 user = user,
                 render_est = schedulator.render_est)
        taskcount = [0]
        def countup(t):
            if not t.donedate:
                taskcount[0] += 1
            return ''
        def doexpand(t):
            if t.donedate or taskcount[0] > 10 or not user:
                return "precollapsed collapsed"
            return "expanded"
        def render_task(t):
            return self.render_string('task.html', t = t, **d)
        d['doexpand'] = doexpand
        d['render_task'] = render_task
        d['countup'] = countup
        self.render('sched.html', **d)


class EditHandler(SchedHandler):
    def get(self):
        self.render('edit.html', subpage='/sched/edit',
                    title = 'Edit Schedule',
                    tasks = get_sched(),
                    text = open(mainpath).read())

    def post(self):
        t = str(self.request.body)
        open(mainpath, 'wb').write(t)
        self.write('ok')
        print 'Updated schedule (%s).' % repr(t[:40])


class Project:
    def __init__(self, task, root, integrate_slips):
        self.root = root
        self.task = task
        self.title = task.title
        self.integrate_slips = integrate_slips
        
        datetasks = {}
        owners = {}
#        for t in root.linearize():
#            datetasks[str(t.duedate)] = []
        for t in task.linearize():
            if t.estimate or not t.subtasks:
                date = str(t.duedate)
                datetasks[date] = datetasks.get(date, []) + [t]
                owner = t.owner or root.nobody
                owners[owner.name] = owner
        for p in set(root.people.values()):
            owners[p.name] = p
        self.unique_dates = sorted(datetasks.keys())
        self.date_tasks = datetasks
        self.users = sorted(owners.values())

    def years(self):
        years = list([d[0:4] for d in self.unique_dates])
        return countify(years)

    def months(self):
        monthnames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
                      'Jul', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec']
        periods = list([d[0:7] for d in self.unique_dates])
        for (period,count) in countify(periods):
            yield (monthnames[int(period[5:])-1],count)

    def _lateness(self, dt):
        cd = self.task.commitdate
        today = schedulator.today
        if cd:
            wdate = (cd.date-today)/self.root.slipfactor + today \
                         - schedulator.DAY
            if dt >= cd:
                return 'late'
            elif (not self.integrate_slips) and dt.date >= wdate:
                return 'warning'
        if dt.date < today:
            return 'old'
        return ''

    def days_and_lateness(self):
        cd = self.task.commitdate
        for d in self.unique_dates:
            dt = schedulator.SDate(d)
            yield (str(dt)[8:], self._lateness(dt))

    def unique_dates_and_lateness(self):
        cd = self.task.commitdate
        for d in self.unique_dates:
            dt = schedulator.SDate(d)
            yield (d, self._lateness(dt))


class GridHandler(_Handler):
    def get(self, integrate_slips=False, title='Grid',
            subpage="/grid"):
        submenu = [
            ('/grid', 'Normal'),
            ('/grid/slip', 'Slip'),
        ]
                
        s = get_sched(integrate_slips=integrate_slips)
        def tasktitler(t):
            l = [str(t.duedate)]
            tt = t
            while tt:
                l.append(tt.title)
                tt = tt.parent
            l.pop()
            l.pop()
            l.reverse()
            l2 = []
            for i,e in enumerate(l):
                l2.append(('&nbsp;'*(i*4)) + e)
            title = '\n'.join(l2)
            return re.sub('"', "'", title)
        projects = list([Project(t, s, integrate_slips) for t in s.subtasks])
        #projects.sort(cmp = lambda x,y: cmp(x.task.duedate, y.task.duedate))
        self.render('grids.html',
                    title=title,
                    page='/grid', subpage=subpage,
                    submenu=submenu, submenu_title='Grid type:',
                    tasktitler=tasktitler,
                    projects=projects)


class SlipGridHandler(GridHandler):
    def get(self):
        return GridHandler.get(self, integrate_slips=True,
                               title = 'Slipgrid',
                               subpage = '/grid/slip' )


optspec = """
bog instaweb [-p port]
--
p,port=     Port number to listen on for http
"""
o = options.Options('bog instaweb', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

(exedir,junk) = os.path.split(sys.argv[0])
pwd = os.path.abspath(os.path.join(exedir, '..'))
bogdir = repo.check_dir()
mainpath = os.path.join(bogdir, 'main.sched')

if not os.path.exists(mainpath):
    f = open(mainpath, 'wb')
    f.write('\n# Main schedule file\n')
    f.close()

settings = dict(
    static_path = os.path.join(pwd, "static"),
    template_path = os.path.join(pwd, "templates"),
    # xsrf_cookies = True, # FIXME?
    debug = 1
)
application = tornado.web.Application([
    (r'/', DashboardHandler),
    (r'/bog', BogIndexHandler),
    (r'/pri', PriHandler),
    (r'/sched', SchedHandler),
    (r'/sched/edit', EditHandler),
    (r'/sched/user/([^/]+)', SchedHandler),
    (r'/grid', GridHandler),
    (r'/grid/slip', SlipGridHandler),
], **settings)

srv = tornado.httpserver.HTTPServer(application)
srv.listen(opt.port or 8011)

print "Listening on port %s" % srv._socket.getsockname()[1]

loop = tornado.ioloop.IOLoop.instance()
loop.start()
