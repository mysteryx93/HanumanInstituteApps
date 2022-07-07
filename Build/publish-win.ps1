Write-Host "abc"

[CmdletBinding()]
Param(
      [switch] $app,
      [switch] $r
)

$zipApp="C:\Program Files\7-Zip\7z.exe"
$e = Test-Path -Path "$zipApp"
Write-Host "abc"
if (Test-Path -Path "$zipApp") {
  Write-Output "$zipApp not found."
  Exit
}

<#
if [%1]==[] goto usage
if [%2]==[] goto usage
set folder=%1
set version=%2
set win64=0
set win86=0
set osx64=0
if [%3]==[] (
  set win64=1
  set win86=1
  set osx64=1
)
if "%3"=="win-x64" (set win64=1)
if "%3"=="win-x86" (set win86=1)
if "%3"=="osx-x64" (set osx64=1)

if %win64%==1 call :publish win-x64 Win_x64 true true true
if %win86%==1 call :publish win-x86 Win_x86 true true true
if %osx64%==1 call :publish osx-x64 MacOS_x64 false true false

if %win64%==0 if %win86%==0 if %osx64%==0 @echo Optional 3rd parameter valid values: win-x64, win-x86 or osx-x64

goto :eof

:publish
:: Function parameters: platform zip-sufix self-contained single-file trim-assembly
set publishPath=%folder%\bin\Publish\%~1
set zipPath=Publish\%version%\%folder%-%version%_%~2.zip
@echo PublishPath = %publishPath%
@echo ZipPath = %zipPath%
@echo on
rd /s /q %publishPath%
dotnet publish %folder% -r %~1 -c Release --self-contained %~3 -p:DebugType=None -p:DebugSymbols=false -p:PublishSingleFile=%~4 -p:PublishTrimmed=%~5 -p:TrimMode=link -o %publishPath%
del /s %folder%\bin\Publish\$~1\*.xml
del /q %zipPath%
%zipApp% a -tzip %zipPath% ./%publishPath%/*
@echo off
exit /B 0

:usage
@echo Syntax: publish AppName Version [OS] (eg: publish Player432hz 2.1 osx-64)
#>