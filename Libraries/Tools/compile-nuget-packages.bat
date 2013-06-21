@ECHO OFF

for %%s in (Common MediaAccessService TVAccessService StreamingService UserSessionService MetaService) do (
   echo Building NuGet package for MPExtended.Services.%%s.Interfaces
   ..\..\.nuget\nuget.exe pack ..\..\Services\MPExtended.Services.%%s.Interfaces\MPExtended.Services.%%s.Interfaces.csproj -OutputDirectory ..\.. -Build -Properties Configuration=Release
   echo.
)

echo NuGet packages have been placed in solution directory