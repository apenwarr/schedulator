#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog remote <url>
"""
o = options.Options('bog remote', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if len(extra) != 1:
    o.fatal('exactly one argument expected')

repo.check_dir()
os.environ['GIT_DIR'] = os.path.join(repo.check_dir(), '.git')

oldurl = repo.remote_url()
if oldurl:
    log('old remote was: %s\n' % oldurl)
    subprocess.call(['git', 'remote', 'rm', 'origin'])
sys.exit(subprocess.call(['git', 'remote', 'add', '-f', 'origin', extra[0]]))
