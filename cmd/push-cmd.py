#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog push
"""
o = options.Options('bog push', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

repo.check_dir()
repo.commit('Commit (push)')
gitdir = os.path.join(repo.check_dir(), '.git')
os.environ['GIT_DIR'] = gitdir
subprocess.call(['git', 'push', gitdir, 'master:merge-me'])
sys.exit(subprocess.call(['git', 'push', 'origin', 'master:merge-me']))
