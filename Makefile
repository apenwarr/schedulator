
default: all

all: cmds bog # Documentation/all

%/all:
	$(MAKE) -C $* all
	
%/clean:
	$(MAKE) -C $* clean

runtests: all runtests-python runtests-cmdline

runtests-python:
	./wvtest.py $(wildcard t/t*.py)
	
runtests-cmdline: all
	t/test.sh
	
stupid:
	PATH=/bin:/usr/bin $(MAKE) test
	
test: all
	./wvtestrun $(MAKE) runtests

bog: main.py
	rm -f $@
	ln -s $< $@
	
cmds: $(patsubst cmd/%-cmd.py,cmd/bog-%,$(wildcard cmd/*-cmd.py))

cmd/bog-%: cmd/%-cmd.py
	rm -f $@
	ln -s $*-cmd.py $@
	
%: %.py
	rm -f $@
	ln -s $< $@
	
bog-%: cmd-%.sh
	rm -f $@
	ln -s $< $@
	
clean: # Documentation/clean
	rm -f .*~ *~ */*~ */*/*~ \
		*.pyc */*.pyc */*/*.pyc\
		bog cmd/bog-*
