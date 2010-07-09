#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog resolve
"""
o = options.Options('bog resolve', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

repo.check_dir()
repo.commit('Commit (resolve)')
repo.resolve()
