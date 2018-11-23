@echo off
setlocal

set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
set DOTNET_CLI_TELEMETRY_OPTOUT=1

set PROJECT_ROOT="%~dp0"
pushd %PROJECT_ROOT%

if exist "build" (rmdir /s /q "build")
if exist "secure-file\bin" (rmdir "secure-file\bin" /s /q)
if exist "secure-file\obj" (rmdir "secure-file\obj" /s /q)

dotnet restore
dotnet publish secure-file -f netcoreapp2.0 -o ..\build -c Release
dotnet publish secure-file -f net462 -o ..\build -c Release

endlocal
@exit /b %errorlevel%
