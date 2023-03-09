
call publish-win Player432hz win-x64
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%
call publish-win Player432hz win-x86
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%

call publish-win Converter432hz win-x64
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%
call publish-win Converter432hz win-x86
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%

call publish-win PowerliminalsPlayer win-x64
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%
call publish-win PowerliminalsPlayer win-x86
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%

call publish-win YangDownloader win-x64
if %ERRORLEVEL% > 0 exit /b %ERRORLEVEL%
call publish-win YangDownloader win-x86
