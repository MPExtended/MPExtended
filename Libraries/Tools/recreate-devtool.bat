@ECHO OFF

REM Requires 64-bit Windows

"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /target:exe /v4 /out:DevTool.exe "..\..\Applications\MPExtended.Applications.Development.DevTool\bin\Debug\MPExtended.Applications.Development.DevTool.exe" "..\..\Applications\MPExtended.Applications.Development.DevTool\bin\Debug\MPExtended.Libraries.Service.dll"