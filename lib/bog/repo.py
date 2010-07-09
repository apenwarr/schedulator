import os, subprocess, glob, tempfile
from bog.helpers import *


def _run(argv):
    rv = subprocess.call(argv)
    if rv:
        raise Exception('%r returned %d' % (argv, rv))


def _grab(argv, stdin=[]):
    p = subprocess.Popen(argv, stdout=subprocess.PIPE, stdin=subprocess.PIPE)
    for i in stdin:
        p.stdin.write(i)
    p.stdin.close()
    data = p.stdout.read().strip()
    rv = p.wait()
    if rv:
        raise Exception('%r returned %d' % (argv, rv))
    return data


def get_dir():
    bd = os.environ.get('BOG_DIR')
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
    for folder in glob.glob('%s/*/.' % d):
        if os.path.exists('%s/cur' % folder):
            mkdirp('%s/new' % folder)
            mkdirp('%s/tmp' % folder)
    return d


def remote_url():
    bogdir = check_dir()
    os.environ['GIT_DIR'] = os.path.join(bogdir, '.git')
    oldurl = _grab(['git', 'config', 'remote.origin.url'])
    return oldurl or None


def head_commitid():
    bogdir = check_dir()
    os.environ['GIT_DIR'] = os.path.join(bogdir, '.git')
    try:
        line = _grab(['git', 'show-ref', '--head', 'HEAD'])
        commit = line.split()[0]
    except Exception:
        commit = None
    return commit or None


def tree_from_commit(cid):
    return _grab(['git', 'rev-parse', '%s:' % cid])


def vcommit(msg, parent):
    bogdir = check_dir()
    os.environ['GIT_DIR'] = os.path.join(bogdir, '.git')
    (tmpfd,tmpname) = tempfile.mkstemp()
    try:
        os.environ['GIT_INDEX_FILE'] = tmpname
        if not parent:
            oldtid = _grab(['git', 'mktree'])  # empty tree
        else:
            oldtid = tree_from_commit(parent)
        _run(['git', 'read-tree', oldtid])
        _run(['git', 'add', '-A', bogdir])
        tid = _grab(['git', 'write-tree'])
        if oldtid != tid:
            cid = _grab(['git', 'commit-tree', tid] +
                        (parent and ['-p', parent] or []),
                        stdin = [msg])
        else:
            cid = parent
        del os.environ['GIT_INDEX_FILE']
        headcommit = head_commitid()
        if headcommit:
            _run(['git', 'reset', '--hard', headcommit])
        return cid
    finally:
        if os.environ.get('GIT_INDEX_FILE') != None:
            del os.environ['GIT_INDEX_FILE']
        os.close(tmpfd)
    

def commit(msg='Checkpoint'):
    cid = vcommit(msg, head_commitid())
    resolve(cid)


def resolve(merge_from='merge-me'):
    gitdir = os.path.join(check_dir(), '.git')
    os.environ['GIT_DIR'] = gitdir
    _run(['git', 'merge', '--', merge_from])
    _run(['git', 'push', gitdir, 'master:merge-me'])


def push():
    os.environ['GIT_DIR'] = os.path.join(check_dir(), '.git')
    _run(['git', 'push', 'origin', 'master:merge-me'])


def pull():
    os.environ['GIT_DIR'] = os.path.join(check_dir(), '.git')
    _run(['git', 'pull', 'origin', 'master'])
    _run(['git', 'pull', 'origin', 'merge-me'])
