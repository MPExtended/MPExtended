@ECHO OFF

cd %1

set ORIGPATH=%PATH%
PATH=C:\Program Files (x86)\Git\bin;C:\Program Files\Git\bin;%PATH%

for /f "delims=" %%a in ('git rev-parse HEAD') do set rev=%%a

echo // WARNING: This file is automatically generated on build. DO NOT EDIT! > %2
echo using MPExtended.Libraries.Service.Internal; >> %2
echo [assembly: AssemblyGitVersion("%rev%")] >> %2
echo. >> %2

PATH=%ORIGPATH%

exit /b 0