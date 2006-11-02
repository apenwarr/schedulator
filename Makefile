
CSFLAGS=-warn:4 -debug
#CSFLAGS += -warnaserror
PKGS=-pkg:nunit -r:System.Data -r:System.Web

all: schedulator.exe webtest.exe

LIBFILES= \
	wvutils.cs wvtest.cs wvweb.cs wvdbi.cs wvini.cs \
	wvtest.t.cs.E wvutils.t.cs.E \

webtest.exe: webtest.cs $(LIBFILES)

schedulator.exe: schedulator.cs webui.cs $(LIBFILES) \
	source.cs person.cs project.cs fixfor.cs task.cs dateslider.cs \
	testsource.cs stringsource.cs logsource.cs fogbugz.cs mantis.cs \
	resultsource.cs \
	$(addsuffix .E,$(wildcard *.t.cs))

test: schedulator.exe
	nunit-console /nologo /labels $^

# you're supposed to use addsuffix() here, but it doesn't really
# matter: if the .E file doesn't exist, we don't care that it
# depends on a .h file.
$(wildcard *.cs.E): $(wildcard *.h)

%: %.exe
	rm -f $@ && ln -sf $< $@
	
%.cs.E: %.cs
	@rm -f $@
	cpp -C -dI $< \
		| expand -8 \
		| sed -e 's,^#include,//#include,' \
		| grep -v '^# [0-9]' \
		>$@

define mcs_go
	mcs $(PKGS) $(CSFLAGS) -out:$@ $(filter %.E %.cs,$^)
endef

%.exe: %.cs.E
	$(mcs_go)

%.exe: %.cs
	$(mcs_go)

clean:
	rm -f *~ *.E *.exe *.dll TestResult.xml *.mdb
