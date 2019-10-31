CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------
.PHONY: ui-test
ui-test:
	cd ./src && dotnet run &
	dotnet test ./form-builder-tests-ui/form-builder-tests-ui.csproj