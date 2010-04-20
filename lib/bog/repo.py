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
