default: all

FORCE:
	@

define make_subdir
	@echo
	@echo "--> Making $(if $2,$2 in )$(if $1,$1,$@)..."
	@+$(MAKE) -C $(if $1,$1,$@) --no-print-directory $3 $2
endef

%: %/Makefile
	$(make_subdir)

%/clean: %/Makefile
	$(call make_subdir,$*,clean)

%/test: %/Makefile
	$(call make_subdir,$*,test)

%/tests: %/Makefile
	$(call make_subdir,$*,tests)

%/all: %/Makefile
	$(call make_subdir,$*,all)
