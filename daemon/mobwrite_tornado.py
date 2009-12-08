#!/usr/bin/env python
import sys, re, os
import tornado.httpserver
import tornado.ioloop
import tornado.web
import tornado.escape
from tornado.web import HTTPError
import mobwrite_daemon
mobwrite_core = mobwrite_daemon.mobwrite_core


ioloop_registered = 0

class MobWriteHandler(tornado.web.RequestHandler):
    def _register_ioloop(self):
        global ioloop_registered
        if not ioloop_registered:
            # Occasionally do timeouts and cleanup
            tornado.ioloop.ioloop = tornado.ioloop  # bug in tornado 0.2
            tornado.ioloop.PeriodicCallback(mobwrite_daemon.do_cleanup,
                                    60*1000).start()
            ioloop_registered = 1

    def _query(self):
        self._register_ioloop()
        mw = mobwrite_daemon.DaemonMobWrite()
        p = str(self.get_argument('p', '', strip=False))
        q = str(self.get_argument('q', '', strip=False))
        if q:   # normal postdata
            r = mw.handleRequest(q)
            self.write(r)
        elif p:  # jsonp response requested
            r = mw.handleRequest(p)
            self.write('mobwrite.callback(%s)' % tornado.escape.json_encode(r))
        else:
            self.send_error(500)
        
    def post(self):
        print 'POST'
        return self._query()
        
    def get(self):
        print 'GET'
        return self._query()


class HelloHandler(tornado.web.RequestHandler):
    def get(self):
        self.write('<html><body>' +
                   'Use <tt>/mobwrite</tt> for the mobwrite API.' +
                   '</body></html>')


def main():
    mobwrite_core.CFG.initConfig(mobwrite_daemon.ROOT_DIR
                                 + "lib/mobwrite_config.txt")
    port = 8091
    settings = dict(
        static_path = os.path.join(os.path.dirname(__file__), "static"),
        debug = 1
    )
    application = tornado.web.Application([
        (r'/', HelloHandler),
        (r'/mobwrite', MobWriteHandler),
    ], **settings)

    srv = tornado.httpserver.HTTPServer(application)
    srv.listen(port)
    mobwrite_core.LOG.info("Listening on port %d..." 
                           % srv._socket.getsockname()[1])
    loop = tornado.ioloop.IOLoop.instance()
    try:
        loop.start()
    finally:
        mobwrite_core.LOG.info("Shutting down.")
        mobwrite_daemon.do_cleanup()


if __name__ == "__main__":
    mobwrite_core.logging.basicConfig()
    main()
    mobwrite_core.logging.shutdown()

