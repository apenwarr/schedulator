#!/usr/bin/env python
import sys, re, os
import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.escape
from tornado.web import HTTPError
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
        for p in sorted(s.people_unique):
            if p.time_queued:
                userlist.append(p)
        self.render('index.html',
                    title='Schedulator',
                    userlist = userlist)


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
    def __init__(self, task, nobody):
        self.task = task
        self.title = task.title
        
        datetasks = {}
        owners = {}
        for t in task.linearize():
            if t.estimate:
                date = str(t.duedate)
                datetasks[date] = datetasks.get(date, []) + [t]
                owner = t.owner or nobody
                owners[owner.name] = owner
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

    def days(self):
        for d in self.unique_dates:
            yield d[8:]


class GridHandler(tornado.web.RequestHandler):
    def get(self):
        s = get_sched()
        def tasktitler(t):
            l = []
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
        projects = list([Project(t, s.nobody) for t in s.subtasks])
        projects.sort(cmp = lambda x,y: cmp(x.task.duedate, y.task.duedate))
        self.render('grids.html',
                    title = 'Schedulator Grid',
                    tasktitler = tasktitler,
                    projects = projects)


if __name__ == "__main__":
    settings = dict(
        static_path = os.path.join(os.path.dirname(__file__), "static"),
        xsrf_cookies = True,
        debug = 1
    )
    application = tornado.web.Application([
        (r'/', IndexHandler),
        (r'/chunk/sched', SchedHandler),
        (r'/chunk/grid', GridHandler),
        (r'/chunk/user/([^/]+)', SchedHandler),
    ], **settings)

    srv = tornado.httpserver.HTTPServer(application)
    srv.listen(8011)

    print "Listening on port %s" % srv._socket.getsockname()[1]

    loop = tornado.ioloop.IOLoop.instance()
    loop.start()
