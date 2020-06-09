CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------
.PHONY: ui-test
ui-test:
	 cd ./src && ENVIRONMENT=uitest dotnet run & dotnet test MSBUILDSINGLELOADCONTEXT=1 ./form-builder-tests-ui/form-builder-tests-ui.csproj && trap "kill 0" EXIT 

.PHONY: ui-test-feature
ui-test-feature:
	dotnet test form-builder-tests-ui/form-builder-tests-ui.csproj --filter $(FEATURE)
