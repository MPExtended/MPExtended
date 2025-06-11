@ECHO OFF

echo Please wait while packages are restored.

FOR %%p IN ("%PROGRAMFILES(x86)%" "%PROGRAMFILES%") DO (
  FOR %%s IN (2019 2022) DO (
    FOR %%e IN (Community Professional Enterprise BuildTools) DO (
      SET PF=%%p
      SET PF=!PF:"=!
      SET MSBUILD_PATH="!PF!\Microsoft Visual Studio\%%s\%%e\MSBuild\Current\Bin\MSBuild.exe"
      IF EXIST "!MSBUILD_PATH!" GOTO :BUILD
    )
  )
)

:BUILD

echo ^>^> Package restore started in %CD% at %DATE% %TIME% > restore-packages.log
echo. >> restore-packages.log
%MSBUILD_PATH% restore-packages.targets >> restore-packages.log
echo. >> restore-packages.log
echo ^>^> Package restore finished at %DATE% %TIME% >> restore-packages.log
