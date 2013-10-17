@ECHO OFF
if not exist output ( mkdir output )
..\Packages\xunit.runners.1.9.2\tools\xunit.console.clr4.x86 .\mpextended.xunit
pause>nul