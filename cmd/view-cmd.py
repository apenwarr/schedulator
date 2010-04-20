#!/usr/bin/env python
import sys, os, glob
from bog import options, repo
from bog.helpers import *

optspec = """
bog view [bugid]
"""
o = options.Options('bog init', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

bogdir = repo.check_dir()
(exedir,junk) = os.path.split(sys.argv[0])

if len(extra) > 1:
    o.fatal('at most one argument expected')


def mutt(*l):
    os.execvp('mutt',
              ['mutt',
               '-e', 'set folder="%s"' % bogdir,
               '-e', 'source "%s/muttx"' % exedir] + list(l))


if not extra:
    cwd = os.path.abspath('.')
    dirbase = os.path.join(bogdir, '')
    if cwd != dirbase and cwd.startswith(dirbase):
        if cwd[-4:] in ['/cur', '/new', '/tmp']:
            cwd = cwd[:-4]
        mutt('-f', cwd,
             '-e', 'macro index q "Q"')
    else:
        mutt('-f', '/dev/null',
             '-e', 'push q')
else:
    bugid = extra[0]
    for n in (glob.glob('%s/*/cur/%s' % (bogdir, bugid)) +
              glob.glob('%s/*/cur/%s:*' % (bogdir, bugid))):
        log('Viewing: %r\n' % n)
        mutt('-f', n,
              '-e', 'macro pager q "<exit>Q"',
              '-e', 'push \\n')
        break
    fatal('bog #%r not found' % bugid)
