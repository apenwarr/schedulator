import sys, os, errno, subprocess


def log(s):
    sys.stderr.write(s)


def mkdirp(d):
    try:
        os.makedirs(d)
    except OSError, e:
        if e.errno == errno.EEXIST:
            pass
        else:
            raise


def next(it):
    try:
        return it.next()
    except StopIteration:
        return None
    
    
def unlink(f):
    try:
        os.unlink(f)
    except OSError, e:
        if e.errno == errno.ENOENT:
            pass  # it doesn't exist, that's what you asked for


def atoi(s):
    try:
        return int(s or '0')
    except ValueError:
        return 0


class BogError(Exception):
    pass


def fatal(s):
    log('error: %s\n' % s)
    sys.exit(90)


def _try_editors(progs, fn, offset):
    for prog in progs:
        try:
            return subprocess.Popen([prog, '+%d' % offset, fn]).wait()
        except OSError, e:
            pass


def editor(fn, offset = 0):
    return _try_editors([os.getenv('EDITOR'),
                         'sensible-editor',
                         'editor',
                         'nano',
                         'vi'], fn, offset)
    
