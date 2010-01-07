#!/usr/bin/env python
import sys, re, os
import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.escape
from tornado.web import HTTPError
import mobwrite.daemon.mobwrite_tornado as mobwrite_tornado
mobwrite_core = mobwrite_tornado.mobwrite_core
mobwrite_daemon = mobwrite_tornado.mobwrite_daemon
import schedulator


def get_sched():
    return schedulator.Schedule(open('test.sched'))


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
    def __init__(self, task, root):
        self.task = task
        self.title = task.title
        
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

    def days_and_lateness(self):
        cd = self.task.commitdate
        for d in self.unique_dates:
            dt = schedulator.SDate(d)
            lateness = (cd and dt >= cd) and 'late' or ''
            yield (d[8:], lateness)

    def unique_dates_and_lateness(self):
        cd = self.task.commitdate
        for d in self.unique_dates:
            dt = schedulator.SDate(d)
            lateness = (cd and dt >= cd) and 'late' or ''
            yield (d, lateness)


class GridHandler(tornado.web.RequestHandler):
    def get(self):
        s = get_sched()
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
        projects = list([Project(t, s) for t in s.subtasks])
        #projects.sort(cmp = lambda x,y: cmp(x.task.duedate, y.task.duedate))
        self.render('grids.html',
                    title = 'Schedulator Grid',
                    tasktitler = tasktitler,
                    projects = projects)


if __name__ == "__main__":
    mobwrite_core.CFG.initConfig(mobwrite_daemon.ROOT_DIR
                                 + "lib/mobwrite_config.txt")
    settings = dict(
        static_path = os.path.join(os.path.dirname(__file__), "static"),
        # xsrf_cookies = True, # FIXME?
        debug = 1
    )
    application = tornado.web.Application([
        (r'/', IndexHandler),
        (r'/chunk/edit', EditHandler),
        (r'/chunk/mobwrite', mobwrite_tornado.MobWriteHandler),
        (r'/chunk/sched', SchedHandler),
        (r'/chunk/grid', GridHandler),
        (r'/chunk/user/([^/]+)', SchedHandler),
    ], **settings)

    srv = tornado.httpserver.HTTPServer(application)
    srv.listen(8011)

    print "Listening on port %s" % srv._socket.getsockname()[1]

    loop = tornado.ioloop.IOLoop.instance()
    loop.start()
