import os, subprocess
from bog.helpers import *


def get_dir():
    bd = os.getenv('BOG_DIR')
    if bd:
        return os.path.join(bd, '')  # ensure terminating /


def check_dir():
    d = get_dir()
    try:
        if not d:
            raise BogError('BOG_DIR not set.  Try "bog init"')
        if not os.path.exists(os.path.join(d, '.')):
            raise BogError('BOG_DIR %r does not exist' % d)
    except BogError, e:
        fatal(e)
    return d


def remote_url():
    bogdir = check_dir()
    os.environ['GIT_DIR'] = os.path.join(bogdir, '.git')
    p = subprocess.Popen(['git', 'config', 'remote.origin.url'],
                         stdout=subprocess.PIPE)
    oldurl = p.stdout.read().strip()
    p.wait()
    return oldurl or None


def commit(msg='Checkpoint'):
    bogdir = check_dir()
    os.environ['GIT_DIR'] = os.path.join(bogdir, '.git')
    subprocess.call(['git', 'add', bogdir])
    rv = subprocess.call(['git', 'commit', '-m', msg])
    subprocess.call(['git', 'branch', 'merge-me'],
                    stderr=open('/dev/null', 'w'))
    return rv


def resolve():
    os.environ['GIT_DIR'] = os.path.join(check_dir(), '.git')
    return subprocess.call(['git', 'merge', '-Xours', 'merge-me'])
