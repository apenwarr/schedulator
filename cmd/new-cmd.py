#!/usr/bin/env python
import sys, os, glob, uuid, time, re, pwd, socket, hashlib
from bog import options, repo
from bog.helpers import *

optspec = """
bog new [milestone]
"""
o = options.Options('bog new', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

bogdir = repo.check_dir()
(exedir,junk) = os.path.split(sys.argv[0])

if len(extra) > 1:
    o.fatal('no more than one argument expected')

fixfor = extra and extra[0] or 'Misc'

p = os.path.join(bogdir, re.sub(r'[\s:/_]', '-', fixfor))
mkdirp('%s/new' % p)
mkdirp('%s/cur' % p)
mkdirp('%s/tmp' % p)

bogid = uuid.uuid4()
pwent = pwd.getpwuid(os.getuid())
username = pwent.pw_name
fullname = pwent.pw_gecos.split(',')[0]
hostname = socket.gethostname()

tfn = '%s/tmp/%s:2,S' % (p, bogid) 
fn = '%s/cur/%s:2,S' % (p, bogid)
f = open(tfn, 'wb')
f.write("From schedulator %s\n" 
        % time.asctime(time.localtime(time.time())))
f.write(
"""From: %s <%s@%s>
To: %s <%s@%s>
Subject: <FIXME ENTER SUBJECT HERE>
MIME-Version: 1.0
Content-type: multipart/mixed; boundary="=--"

--=--

%s: Implementation
\tSubtask 1 [1h]
\tSubtask 2 [1h]
%s: Testing [1h]

--=----
""" % (fullname, username, hostname,
       fullname, username, hostname,
       username, username))
f.close()

sum1 = hashlib.sha1(open(tfn).read()).digest()
editor(tfn, offset=4)
sum2 = hashlib.sha1(open(tfn).read()).digest()
if sum1 != sum2:
    os.rename(tfn, fn)
    print bogid
    sys.exit(0)
else:
    unlink(fn)
    log('bog not changed; aborting.\n')
    sys.exit(1)

