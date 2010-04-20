#!/usr/bin/env python
import sys, subprocess
from bog import options, repo
from bog.helpers import *

optspec = """
bog log
"""
o = options.Options('bog log', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

gitdir = os.path.join(repo.check_dir(), '.git')
os.environ['GIT_DIR'] = gitdir
sys.exit(subprocess.call(['git', 'log']))
