WVDOTNET=..
include ../monorules.mk

all: tests

PKGS += /r:System.Data /r:System.Web
CPPFLAGS += -I..

wvtest.t.exe: wv.t.cs.E wvtest.t.cs.E ../wv.dll

wv.t.exe: \
	wv.t.cs.E wvutils.t.cs.E \
	wvloopsock.t.cs.E \
	wvlog.t.cs.E \
	wvdata.t.cs.E \
	wvstream.t.cs.E wvmoniker.t.cs.E wvurl.t.cs.E \
	../wv.dll

wvhttpserver.t.exe: wv.t.cs.E wvhttpserver.t.cs.E ../wv.dll

tests: wvtest.t.exe wv.t.exe wvhttpserver.t.exe

test: wvtest.t.pass wv.t.pass wvhttpserver.t.pass

clean::
	rm -f *.pass
