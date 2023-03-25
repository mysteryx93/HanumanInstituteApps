
call publish-win Player432Hz win-x64
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win Player432Hz win-x86
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win Converter432Hz win-x64
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win Converter432Hz win-x86
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win PowerliminalsPlayer win-x64
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win PowerliminalsPlayer win-x86
if ERRORLEVEL 1 exit /b !ERRORLEVEL!

call publish-win YangDownloader win-x64
if ERRORLEVEL 1 exit /b !ERRORLEVEL!
call publish-win YangDownloader win-x86
