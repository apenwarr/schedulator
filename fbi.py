#!/usr/bin/env python
import os, errno, time, textwrap, re
import MySQLdb as mysql

m = mysql.Connect(host='fogbugz', user='fogbugz', passwd='scs')
m.select_db('fogbugz5')

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

def mkdir(p):
    try:
        os.mkdir(p)
    except OSError, e:
        if e.errno == errno.EEXIST:
            pass
        else:
            raise

def mkfixfor(f):
    mkdir('p')
    if f.project:
        p = 'p/%s: %s' % (f.project.name, f.name)
    else:
        p = 'p/%s' % f.name
    mkdir(p)
    mkdir('%s/new' % p)
    mkdir('%s/cur' % p)
    mkdir('%s/tmp' % p)
    return p


def fixwrap(s):
    paras = re.split(r'\s*\n\s*\n', s)
    wparas = ['\n'.join(textwrap.wrap(para, width=70)) for para in paras]
    return '\n\n'.join(wparas)


def fixdt(dt):
    return int(dt.strftime('%s'))


def _writemail(f, tm, content):
    f.write("From schedulator %s\n" % time.asctime(time.localtime(tm)))
    f.write(content.replace('\r\n', '\n'))
    f.write('\n\n')


def writemail(f, tm, _from, to, cc, subject, body, headers = []):
    f.write("From schedulator %s\n" % time.asctime(time.localtime(tm)))
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
    body = (body or '').replace('\r\n', '\n')
    f.write(body)
    f.write('\n\n')


for (id, date, title, fixforid, openbyid, assignedid, resolvedid) in \
  query('select ixBug, dtOpened, sTitle, ixFixFor, ' +
        'ixPersonOpenedBy, ixPersonAssignedTo, ixPersonResolvedBy from Bug'):
    fixfor = fixfors[fixforid]
    openby = persons[openbyid]
    assigned = persons[assignedid]
    resolved = persons.get(resolvedid)
    print '%-5s %s' % (id, title)
    subj = '[Bug %s] %s' % (id, title)
    d = mkfixfor(fixfor)
    f = open('%s/cur/%s' % (d, id), 'wb')
    writemail(f, tm=fixdt(date),
              _from=openby.mailname(),
              to=(resolved or assigned).mailname(),
              cc=None,
              subject=subj,
              body=None)
    for (evid, evdate, evismail, verb, evwhoid, evbody, evchanges) in \
      query('select ixBugEvent, dt, fEmail, sVerb, ixPerson, s, sChanges ' +
            ' from BugEvent where ixBug=%s order by dt', id):
        evwho = persons.get(evwhoid)
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
    f.close()
