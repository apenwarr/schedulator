#!/usr/bin/env python
import sys, re
import tornado.httpserver
import tornado.ioloop
import tornado.web
from tornado.web import HTTPError
import schedulator

class HelloHandler(tornado.web.RequestHandler):
    def get(self):
        self.write("hello world")

class UserHandler(tornado.web.RequestHandler):
    def get(self, username):
        s = schedulator.Schedule(open('test.sched'))
        self.render('sched.html',
                    title='Schedulator for %s' % username,
                    tasks=s,
                    render_est=schedulator.render_est)

application = tornado.web.Application([
    (r'/', HelloHandler),
    (r'/user/(\w+)', UserHandler),
], debug=1)

srv = tornado.httpserver.HTTPServer(application)
srv.listen(8011)

print "Listening on port %s" % srv._socket.getsockname()[1]

loop = tornado.ioloop.IOLoop.instance()
loop.start()
