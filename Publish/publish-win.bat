@echo off
set zipApp="C:\Program Files\7-Zip\7z.exe"
if not exist %zipApp% (
  @echo %zipApp% not found.
  exit /b 1
)
if [%1]==[] (
  echo Syntax: publish-win AppName [runtime] [kind]
  echo Runtime: win-x64 or win-x86 ^(default=win-x64^)
  echo Kind: zip^|setup ^(default=zip^)

  exit /b 1
)

set runtime=%2
if [%2]==[] set runtime=win-x64
set sufix=%runtime%
if "%runtime%"=="win-x64" set sufix=Win_x64
if "%runtime%"=="win-x86" set sufix=Win_x86

set kind=%3
if [%3]==[] set kind=zip
if "%kind%"=="zip" set ext=.zip
if "%kind%"=="setup" set ext=_Setup.exe

set projShared=..\Src\Directory.Build.props
set folder=..\Src\App.%1\%1.Desktop
set proj=%folder%\%1.Desktop.csproj
if not exist "%proj%" (
  echo Could not find %proj%
  exit /b 1
)

:: Read AssemblyVersion from Directory.Build.props and csproj
setlocal enableextensions disabledelayedexpansion
set "build="
for /f "tokens=3 delims=<>" %%a in (
    'find /i "<AssemblyVersion>" ^< "%projShared%"'
) do set "version=%%a"
if [%version%]==[] (
  for /f "tokens=3 delims=<>" %%a in (
      'find /i "<AssemblyVersion>" ^< "%proj%"'
  ) do set "version=%%a"
  if [%version%]==[] (
    echo AssemblyVersion is missing from project file.
    exit /b 1
  )
)


:: Display information
set publishPath=%folder%\bin\Publish\%runtime%
set outFile=%1-%version%_%sufix%%ext%
set output=%version%\%outFile%

echo Runtime = %runtime%
echo AppName = %1
echo Version = %version%
echo PublishPath = %publishPath%
echo Output = Publish\%output%

:: Start publishing
@echo on
rd /s /q %publishPath%

:: dotnet publish %proj% -r %runtime% -c Release --self-contained=true -p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=true -o %publishPath%

pupnet "..\Src\App.%1\Deploy\pupnet.conf" -y -r %runtime% -v %version% -o "..\..\..\Publish\%version%\%outFile%" -k %kind%


:: if ERRORLEVEL 1 exit /b !ERRORLEVEL!

:: del /s %publishPath%\*.xml
:: del /q %output%
:: %zipApp% a -tzip %output% %publishPath%/*
