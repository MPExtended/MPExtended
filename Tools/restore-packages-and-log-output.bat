@ECHO OFF

echo Please wait while packages are restored.

echo ^>^> Package restore started in %CD% at %DATE% %TIME% > restore-packages.log
echo. >> restore-packages.log
@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" restore-packages.targets >> restore-packages.log
echo. >> restore-packages.log
echo ^>^> Package restore finished at %DATE% %TIME% >> restore-packages.log