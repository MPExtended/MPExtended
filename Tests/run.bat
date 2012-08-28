@ECHO OFF
if not exist output ( mkdir output )
..\Libraries\Tests\xunit.console.clr4 .\mpextended.xunit
pause>nul