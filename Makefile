CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------
.PHONY: ui-test
ui-test:
	cd ./form-builder-tests-ui && dotnet build
	cd ./packages/SpecRun.Runner.*/tools/ && ./SpecRun.exe run Default.srprofile "/baseFolder:$(CUR_DIR)\form-builder-tests-ui\bin\Debug" /log:specrun.log /toolIntegration:TeamCity
