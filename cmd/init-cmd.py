#!/usr/bin/env python
import sys, os, subprocess
from bog import options
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

oldd = get_bog_dir()
if oldd and oldd != os.path.abspath('.'):
    fatal('BOG_DIR already initialized: %r' % oldd)

subprocess.call(['git', 'init'])
open('.bogroot', 'wb').close()
log('Initialized Bog repository.\n')
