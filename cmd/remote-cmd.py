#!/usr/bin/env python
import sys
from bog import options, repo
from bog.helpers import *

optspec = """
bog remote <url>
"""
o = options.Options('bog remote', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

repo.check_dir()
os.environ['GIT_DIR'] = os.path.join(repo.check_dir(), '.git')
oldurl = repo.remote_url()

if not extra:
    if oldurl:
        print oldurl
    else:
        log('No remote URL defined.\n')
elif len(extra) != 1:
    o.fatal('exactly one argument expected')
else:
    # exactly one argument
    if oldurl:
        log('old remote was: %s\n' % oldurl)
        subprocess.call(['git', 'remote', 'rm', 'origin'])
    sys.exit(subprocess.call(['git', 'remote', 'add', '-f',
                              'origin', extra[0]]))
