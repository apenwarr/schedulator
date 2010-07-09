#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog commit [-m msg]
--
m,message  Use a non-default commit message
"""
o = options.Options('bog commit', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if extra:
    o.fatal('no arguments expected')

repo.commit(msg = opt.message or 'Commit')

