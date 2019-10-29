@pushd %~dp0

dotnet msbuild "form-builder-tests-ui.csproj"

@if ERRORLEVEL 1 goto end

@cd ..\..\packages\SpecRun.Runner.*\tools

SpecRun.exe run Default.srprofile "/baseFolder:%~dp0\bin\Debug" /log:specrun.log /toolIntegration:TeamCity

:end

@popd