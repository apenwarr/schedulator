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

class HelloHandler(tornado.web.RequestHandler):
    def get(self):
        s = get_sched()
        userlist = []
        for p in sorted(s.people_unique):
            if p.time_queued:
                userlist.append(p)
        self.render('index.html',
                    title='Schedulator',
                    userlist = userlist)

class UserHandler(tornado.web.RequestHandler):
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

settings = dict(
    static_path = os.path.join(os.path.dirname(__file__), "static"),
    xsrf_cookies = True,
    debug = 1
)
application = tornado.web.Application([
    (r'/', HelloHandler),
    (r'/data/sched', UserHandler),
    (r'/data/user/([^/]+)', UserHandler),
], **settings)

srv = tornado.httpserver.HTTPServer(application)
srv.listen(8011)

print "Listening on port %s" % srv._socket.getsockname()[1]

loop = tornado.ioloop.IOLoop.instance()
loop.start()
