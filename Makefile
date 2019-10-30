NPM_PROXY=http://172.16.0.126:8080/
CUR_DIR = $(CURDIR)

.PHONY: help
help:
	@cat ./MakefileHelp

# Project automation targets
# ---------------------------------------------------------------------------------------

.PHONY: setup
setup:
	@echo Setting up application
	make npm-install
	cd ../../
	make js-build
	cd ../../
	make styleguide-pull
	cd ../../
	make sass-compile
	cd ../../
	make dotnet-restore

.PHONY: dotnet-restore
dotnet-restore:
	@echo Restoring application dependancies
	dotnet restore

.PHONY: dotnet-test
dotnet-test:
	@echo Running dotnet tests
	cd ./tests/form-builder-tests-unit && dotnet test

.PHONY: dotnet-run-uitests-profile
dotnet-run-uitests-profile:
	@echo Running frontend with UiTests profile
	cd ./src/form-builder && dotnet run --launch-profile UiTests	

.PHONY: dotnet-integration-test
dotnet-integration-test:
	@echo Starting Integration tests
	cd ./tests/form-builder-tests-integration && dotnet test
	
.PHONY: npm-install
npm-install:
	@echo Restoring NPM dependancies
	npm config set proxy $(NPM_PROXY)
	npm config set https-proxy $(NPM_PROXY)
	cd ./src/form-builder && npm install

.PHONY: js-build
js-build:
	@echo Installing, cleaning and building JavaScript files
	make npm-install
	cd ./src/form-builder && npm run js:clean 
	cd ./src/form-builder && npm run js:compile

.PHONY: js-test
js-test:
	@echo Testing application JavaScript
	cd ./tests/form-builder-tests-javascript && npm install
	cd ./tests/form-builder-tests-javascript && npm run jstest

.PHONY: sass-compile
sass-compile:
	@echo Building and minifying Sass into Css
	make npm-install
	cd ./src/form-builder && npm run sass:compile

.PHONY: sass-compile-prod
sass-compile-prod:
	@echo Building and minifying Sass into Css
	make npm-install
	cd ./src/form-builder && npm run sass:compile-prod

.PHONY: sass-compile-semantic-prod
sass-compile-semantic-prod:
	@echo Building and minifying Sass into Css
	make npm-install
	cd ./src/form-builder && npm run sass:compile-semantic-prod

.PHONY: styleguide-pull
styleguide-pull:
	@echo Pulling artifacts from Styleguide
	make npm-install
	cd ./src/form-builder && npm run styleguide:pull

.PHONY: styleguide-pull-from-tag
styleguide-pull-from-tag:
	@echo Pulling artifacts from Styleguide
	make npm-install
	cd ./src/form-builder && npm run styleguide:pull-from-tag

.PHONY: dotnet-build
dotnet-build:
	@echo Building application
	cd ./src/form-builder && dotnet build

.PHONY: dotnet-run
dotnet-run:
	@echo Starting application
	cd ./src/form-builder && dotnet run --launch-profile 'Frontend'

.PHONY: dotnet-publish
dotnet-publish:
	@echo Publishing application
	cd ./src/form-builder && dotnet publish -c Release -o publish

.PHONY: ui-test
ui-test:
	cd ./tests/form-builder-tests-ui && dotnet build
	cd ./packages/SpecRun.Runner.*/tools/ && ./SpecRun.exe run Default.srprofile "/baseFolder:$(CUR_DIR)\tests\form-builder-tests-ui\bin\Debug" /log:specrun.log /toolIntegration:TeamCity

# Develpoment automation scripts
# ---------------------------------------------------------------------------------------

.PHONY: sass-watch
sass-watch:
	@echo Watching for changes in ~/wwwroot/assets/sass
	cd ./src/form-builder && npm run sass:watch
