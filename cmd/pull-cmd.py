#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog pull
"""
o = options.Options('bog pull', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

repo.check_dir()
repo.commit('Commit (pull)')
os.environ['GIT_DIR'] = os.path.join(repo.check_dir(), '.git')
repo.resolve()
if repo.remote_url():
    sys.exit(subprocess.call(['git', 'pull', '-Xours', 'origin', 'master']))
else:
    log('No remote repository configured.\n')
    sys.exit(0)
