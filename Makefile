CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------
.PHONY: ui-test
ui-test:
	dotnet test ./form-builder-tests-ui/form-builder-tests-ui.csproj & cd ./src && dotnet run