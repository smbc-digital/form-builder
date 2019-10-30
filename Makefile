CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------
.PHONY: ui-test
ui-test:
	cd ./form-builder-tests-ui && dotnet build
	cd ./packages/specrun.runner/*/tools/net45/ && ./SpecRun.exe run ../../../form-builder-tests-ui/Default1.srprofile "/baseFolder:$(CUR_DIR)\form-builder-tests-ui\bin\Debug" /log:specrun.log /toolIntegration:TeamCity
