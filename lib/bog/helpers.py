import sys, os, errno


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
