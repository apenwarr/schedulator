#!/usr/bin/env python
import sys, os, subprocess, signal

argv = sys.argv
exe = argv[0]
exepath = os.path.split(exe)[0] or '.'

# fix the PYTHONPATH to include our lib dir
libpath = os.path.join(exepath, 'lib')
cmdpath = os.path.join(exepath, 'cmd')
sys.path[:0] = [libpath]
os.environ['PYTHONPATH'] = libpath + ':' + os.environ.get('PYTHONPATH', '')

from bog.helpers import *


def find_bog_dir():
    if not os.environ.get('BOG_DIR'):
        parts = os.path.abspath('.').split('/')
        while len(parts) >= 1:
            p = '/'.join(parts)
            if os.path.exists(os.path.join(p, '.bogroot')):
                os.environ['BOG_DIR'] = p
                return p
            parts.pop()

            
def columnate(l, prefix):
    l = l[:]
    clen = max(len(s) for s in l)
    ncols = (78 - len(prefix)) / (clen + 2)
    if ncols <= 1:
        ncols = 1
        clen = 0
    cols = []
    while len(l) % ncols:
        l.append('')
    rows = len(l)/ncols
    for s in range(0, len(l), rows):
        cols.append(l[s:s+rows])
    for row in zip(*cols):
        print prefix + ''.join(('%-*s' % (clen+2, s)) for s in row)


def usage():
    log('Usage: bog <command> <options...>\n\n')
    common = dict()

    if common:
        log('Common commands:\n')
        for cmd,synopsis in sorted(common.items()):
            print '    %-10s %s' % (cmd, synopsis)
        log('\n')
    
    cmds = []
    for c in sorted(os.listdir(cmdpath) + os.listdir(exepath)):
        if c.startswith('bog-') and c.find('.') < 0:
            cname = c[4:]
            if cname not in common:
                cmds.append(c[4:])
    if cmds:
        log('Other available commands:\n')
        columnate(cmds, '    ')
        log('\n')

    #log("See 'bog help <command>' for more information on " +
    #    "a specific command.\n")
    sys.exit(99)


if len(argv) == 1 or atoi(argv[1]) != 0:
    argv[1:1] = ['view']  # default to 'view' if just a bug number is given
elif not argv[1] or argv[1][0] == '-':
    usage()
subcmd = argv[1]

def subpath(s):
    sp = os.path.join(exepath, 'bog-%s' % s)
    if not os.path.exists(sp):
        sp = os.path.join(cmdpath, 'bog-%s' % s)
    return sp

if not os.path.exists(subpath(subcmd)):
    log('error: unknown command "%s"\n' % subcmd)
    usage()

bog_dir = find_bog_dir()
#log('BOG_DIR is %r\n' % find_bog_dir())

ret = 95
try:
    os.execv(subpath(subcmd), [subpath(subcmd)] + argv[2:])
except OSError, e:
    log('%s: %s\n' % (subpath(subcmd), e))
    ret = 98
except KeyboardInterrupt, e:
    ret = 94
sys.exit(ret)
