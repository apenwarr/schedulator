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
repo.resolve()
repo.push()
