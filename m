#!/bin/bash
_mutt()
{
    mutt -e "set folder=$PWD/p" \
    	 -e 'set hdr_format="%3C%Z %{%y/%m/%d} %-15.15F %s' \
    	 "$@"
}

if [ -n "$1" ]; then
	for d in p/*/cur/$1*; do
		echo >&2 "Using: $d"
		_mutt -f "$d"
		break
	done
else
	_mutt -f /dev/null -e "push c?"
fi
