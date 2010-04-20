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
rv = repo.commit('Commit (pull)')
rv += repo.resolve()
if repo.remote_url():
    rv += repo.pull()
else:
    log('No remote repository configured.\n')
sys.exit(rv)

