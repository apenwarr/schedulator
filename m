#!/bin/bash
_mutt()
{
    mutt -e "set folder=$PWD/p" -e 'source muttx' "$@"
}

if [ -n "$1" ]; then
	for d in p/*/cur/$1*; do
		echo >&2 "Using: $d"
		_mutt -f "$d"
		break
	done
else
	_mutt -f /dev/null -e "push q"
fi
