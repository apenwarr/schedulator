#!/usr/bin/env python
import os, errno, time, textwrap, re, subprocess
import MySQLdb as mysql
from bog import options
from bog.helpers import *

optspec = """
bog fogbugz [-h server] [-u user] [-p passwd] <outputdir>
--
s,server=   Fogbugz MySQL server hostname
u,user=     Fogbugz MySQL server login username
p,passwd=   Fogbugz MySQL server password
d,dbname=   Fogbugz MysQL database name
"""
o = options.Options('bog fogbugz', optspec)
(opt, flags, extra) = o.parse(sys.argv[1:])

if not opt.server or not opt.user or not opt.passwd or not opt.dbname:
    o.fatal('you must provide -h, -u, -p, and -d options')

if len(extra) != 1:
    o.fatal('you must provide exactly one <outputdir> parameter')

m = mysql.Connect(host=opt.server, user=opt.user, passwd=opt.passwd)
m.select_db(opt.dbname)

topdir = extra[0]

if os.path.exists(topdir):
    log('Deleting %r...\n' % topdir)
    subprocess.call(['rm', '-rf', topdir])
log('Exporting...\n')


def query(q, *args):
    c = m.cursor()
    c.execute(q, *args)
    return c


class Person:
    def __init__(self, id, name, email):
        self.id = id
        self.name = name
        self.email = email
    def __repr__(self):
        return 'Person(%d,%s <%s>)' % (self.id, self.name, self.email)
    def mailname(self):
        # FIXME: handle quoting
        return '%s <%s>' % (self.name, self.email)
    def user(self):
        return re.sub(r'@.*', '', self.email)
persons = {}

for (id, name, email) in \
  query('select ixPerson, sFullName, sEmail from Person'):
    persons[id] = Person(id, name, email)



class Project:
    def __init__(self, id, name):
        self.id = id
        self.name = name
    def __repr__(self):
        return 'Project(%d,%s)' % (self.id, self.name)
projects = {}

for (id, name) in query('select ixProject, sProject from Project'):
    projects[id] = Project(id, name)



class FixFor:
    def __init__(self, id, name, project):
        self.id = id
        self.name = name
        self.project = project
    def __repr__(self):
        return 'FixFor(%d,%s.%s)' % (self.id, self.project, self.name)
fixfors = {}

for (id, name, projectid) in \
  query('select ixFixFor, sFixFor, ixProject from FixFor'):
    fixfors[id] = FixFor(id, name, projects.get(projectid))

def mkfixfor(f):
    if f.project:
        p = '%s-%s' % (f.project.name, f.name)
    else:
        p = '%s' % f.name
    p = os.path.join(topdir, re.sub(r'[\s:/_]', '-', p))
    mkdirp('%s/new' % p)
    mkdirp('%s/cur' % p)
    mkdirp('%s/tmp' % p)
    return p


def fixwrap(s):
    l = []
    for line in s.replace('\r\n', '\n').split('\n'):
        if not re.match(r'\s', line):  # leading whitespace = preformatted
            if len(line) > 80:
                l += textwrap.wrap(line, width=72)
                continue
        l.append(line)
    return '\n'.join(l)


def fixdt(dt):
    return int(dt.strftime('%s'))


def _writemail(f, tm, content):
    #f.write("From schedulator %s\n" % time.asctime(time.localtime(tm)))
    f.write(content.replace('\r\n', '\n'))
    f.write('\n\n')


def writemail(f, tm, _from, to, cc, subject, body, headers = []):
    #f.write("From schedulator %s\n" % time.asctime(time.localtime(tm)))
    #f.write("Message-Id: <%d@fbi>\n" % id)
    if _from:
        f.write("From: %s\n" % _from)
    if to:
        f.write("To: %s\n" % to)
    if cc:
        f.write("Cc: %s\n" % cc)
    if subject:
        f.write("Subject: %s\n" % subject)
    for k,v in headers:
        f.write("%s: %s\n" % (k, v))
    f.write("\n")
    body = (body or '').replace('\r\n', '\n').rstrip()
    if body:
        f.write(body + '\n')


for (id, date, isopen, status, title, fixforid, 
     openbyid, assignedid, resolvedid) in \
  query('select ixBug, dtOpened, fOpen, ixStatus, sTitle, ixFixFor, ' +
        'ixPersonOpenedBy, ixPersonAssignedTo, ixPersonResolvedBy from Bug'):
    isresolved = (status != 1)
    fixfor = fixfors[fixforid]
    openby = persons[openbyid]
    assigned = persons[assignedid]
    resolved = persons.get(resolvedid)
    print '%-5s %s' % (id, title)
    subj = '[%s] %s' % (id, title)
    d = mkfixfor(fixfor)
    flags = 'S'
    if isresolved:
        flags += 'R'   # R is for "replied", but we'll use it for resolved
    if not isopen:
        flags += 'F'   # F is for "flagged", but we'll use it for closed
    f = open('%s/cur/%s:2,%s' % (d, id, flags), 'wb')
    f.write("From schedulator %s\n" 
            % time.asctime(time.localtime(fixdt(date))))
    msgid = [('MIME-Version', '1.0'),
             ('Content-Type', 'multipart/mixed; boundary="=--"'),
             ]
    assignee = (resolved or assigned)
    writemail(f, tm=fixdt(date),
              _from=openby.mailname(),
              to=assignee.mailname(),
              cc=None,
              subject=subj,
              body=None,
              headers=msgid)
    f.write('\n--=--\n\n')
    f.write(('# Schedulator tasks\n' +
            '%s: Implementation [1h]\n' +
            '%s: Test\n\n') % (assignee.user(), assignee.user()))
    for (evid, evdate, evismail, verb, evwhoid, evbody, evchanges) in \
      query('select ixBugEvent, dt, fEmail, sVerb, ixPerson, s, sChanges ' +
            ' from BugEvent where ixBug=%s order by dt', id):
        evwho = persons.get(evwhoid)
        f.write('\n--=--\nContent-Type: message/rfc822\n\n')
        if evismail:
            _writemail(f, tm=fixdt(evdate), content=evbody)
        else:
            sl = []
            headers = []
            if verb:
                headers.append(('X-Action', verb))
            if evchanges: sl.append(evchanges.strip())
            if evbody: sl.append(fixwrap(evbody))
            s = '\n\n'.join(sl)
            writemail(f, tm=fixdt(evdate),
                      _from=evwho and evwho.mailname(),
                      to=None,
                      cc=None,
                      subject=None,
                      body=s,
                      headers=headers)
    f.write('\n--=----\n\n')
    f.close()
