#!/usr/bin/env python
import sys, re, os
import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.escape
from tornado.web import HTTPError
import schedulator
from bog import options


def get_sched(integrate_slips=False):
    return schedulator.Schedule(open('test.sched'),
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


class IndexHandler(tornado.web.RequestHandler):
    def get(self):
        s = get_sched()
        userlist = []
        for p in sorted(set(s.people.values())):
            if p.time_queued:
                userlist.append(p)
        self.render('index.html',
                    title='Schedulator',
                    userlist = userlist)


class EditHandler(tornado.web.RequestHandler):
    def get(self):
        if os.path.exists('.schedid'):
            id = open('.schedid').read().strip()
        else:
            id = 'x' + open('/dev/urandom').read(32).encode('hex')
            open('.schedid', 'w').write(id)
        self.render('edit.html',
                    hexcode = id,
                    text = open('test.sched').read())

    def post(self):
        t = str(self.request.body)
        open('test.sched', 'w').write(t)
        self.write('ok')
        print 'Updated schedule (%s).' % repr(t[:40])


class SchedHandler(tornado.web.RequestHandler):
    def get(self, username = None):
        s = get_sched()
        if username:
            username = tornado.escape.url_unescape(username)
        user = username and s.people.get(username.lower()) or None
        tasks = []
        self.render('sched.html',
                    tasks = s,
                    user = user,
                    render_est = schedulator.render_est)


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


class GridHandler(tornado.web.RequestHandler):
    def get(self, integrate_slips=False,
            title='Schedulator Grid'):
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
                    title = title,
                    tasktitler = tasktitler,
                    projects = projects)

class SlipGridHandler(GridHandler):
    def get(self):
        return GridHandler.get(self, integrate_slips=True,
                               title = 'Schedulator Slipgrid')


optspec = """
bog instaweb [-p port]
--
p,port=     Port number to listen on for http
"""
o = options.Options('bog instaweb', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

pwd = os.path.abspath('.')

settings = dict(
    static_path = os.path.join(pwd, "static"),
    template_path = pwd,
    # xsrf_cookies = True, # FIXME?
    debug = 1
)
application = tornado.web.Application([
    (r'/', IndexHandler),
    (r'/chunk/edit', EditHandler),
    (r'/chunk/sched', SchedHandler),
    (r'/chunk/grid', GridHandler),
    (r'/chunk/slipgrid', SlipGridHandler),
    (r'/chunk/user/([^/]+)', SchedHandler),
], **settings)

srv = tornado.httpserver.HTTPServer(application)
srv.listen(opt.port or 8011)

print "Listening on port %s" % srv._socket.getsockname()[1]

loop = tornado.ioloop.IOLoop.instance()
loop.start()
