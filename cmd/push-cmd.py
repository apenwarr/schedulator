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
rv = repo.commit('Commit (push)')
rv += repo.resolve()
rv += repo.push()
sys.exit(rv)
