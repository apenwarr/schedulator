include wvdotnet/rules.mk
include wvdotnet/monorules.mk

PKGS=-r:System.Data -r:System.Web
CPPFLAGS=-Iwvdotnet
DOTDIR=../$(shell basename "$$PWD")

all: wvdotnet/all basecamp/all schedulator.exe webtest.exe

webtest.exe: webtest.cs $(DOTDIR)/wvdotnet/wv.dll

SRC=schedulator.cs \
	source.cs person.cs project.cs fixfor.cs task.cs dateslider.cs \
	testsource.cs stringsource.cs logsource.cs fogbugz.cs mantis.cs \
	resultsource.cs googlecode.cs \
	$(DOTDIR)/wvdotnet/wv.dll $(DOTDIR)/basecamp/basecamp.dll

schedulator.exe: webui.cs $(SRC)

schedulator.t.exe: $(SRC) \
	$(addsuffix .E,$(wildcard *.t.cs)) \
	
clean:: wvdotnet/clean basecamp/clean

tests: all schedulator.t.exe

test: tests
	wvdotnet/wvtestrunner.pl mono --debug ./schedulator.t.exe
