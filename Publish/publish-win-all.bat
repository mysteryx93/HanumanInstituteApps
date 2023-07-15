
call publish-win Player432Hz win-x64 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win Player432Hz win-x86 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win Player432Hz win-x64 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win Player432Hz win-x86 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win Converter432Hz win-x64 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win Converter432Hz win-x86 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win Converter432Hz win-x64 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win Converter432Hz win-x86 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win PowerliminalsPlayer win-x64 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win PowerliminalsPlayer win-x86 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win PowerliminalsPlayer win-x64 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win PowerliminalsPlayer win-x86 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win YangDownloader win-x64 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win YangDownloader win-x86 zip
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win YangDownloader win-x64 setup
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
::call publish-win YangDownloader win-x86 setup
