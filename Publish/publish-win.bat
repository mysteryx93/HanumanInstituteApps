@echo off
set zipApp="C:\Program Files\7-Zip\7z.exe"
if not exist %zipApp% (
  @echo %zipApp% not found.
  exit /b 1
)
if [%1]==[] (
  @echo Syntax: publish-win AppName [runtime] ^(eg: publish-win Player432hz win-x86^)
  exit /b 1
)

set runtime=%2
if [%2]==[] set runtime=win-x64
set sufix=%runtime%
if "%runtime%"=="win-x64" (set sufix=Win_x64)
if "%runtime%"=="win-x86" (set sufix=Win_x86)

set folder=..\%1
set proj=%folder%\%1.csproj
if not exist "%proj%" (
  echo Could not find %proj%
  exit /b 1
)

:: Read AssemblyVersion from csproj
setlocal enableextensions disabledelayedexpansion
set "build="
for /f "tokens=3 delims=<>" %%a in (
    'find /i "<AssemblyVersion>" ^< "%proj%"'
) do set "version=%%a"

:: Display information
set publishPath=%folder%\bin\Publish\%runtime%
set output=%version%\%1-%version%_%sufix%.zip

echo Runtime = %runtime%
echo AppName = %1
echo Version = %version%
echo PublishPath = %publishPath%
echo Output = Build\%output%

:: Start publishing
@echo on
rd /s /q %publishPath%
dotnet publish %folder% -r %runtime% -c Release --self-contained true -p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -o %publishPath%
del /s %publishPath%\*.xml
del /q %output%
%zipApp% a -tzip %output% %publishPath%/*
