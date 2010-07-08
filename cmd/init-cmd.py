#!/usr/bin/env python
import sys, os, subprocess
from bog import options, repo
from bog.helpers import *

optspec = """
bog init
"""
o = options.Options('bog init', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

if os.path.exists('.bogroot'):
    log('.bogroot already exists in this directory.\n')

oldd = repo.get_dir()
if oldd and os.path.abspath(oldd) != os.path.abspath('.'):
    fatal('BOG_DIR already initialized: %r' % oldd)
os.environ['BOG_DIR'] = os.path.abspath('.')

subprocess.call(['git', 'init'])
open('.bogroot', 'wb').close()
if not os.path.exists('.gitignore'):
    open('.gitignore', 'wb').write("*~\nnohup.out\n*.swp\n*.bak\n")
if not os.path.exists('.gitattributes'):
    open('.gitattributes', 'wb').write('*  merge=union\n')
mkdirp('Undecided/cur')
mkdirp('Undecided/new')
mkdirp('Undecided/tmp')
if not os.path.exists('.git/index'):
    repo.commit(msg='Initial commit')
log('Initialized Bog repository.\n')
