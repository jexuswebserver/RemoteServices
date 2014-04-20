set EnableNuGetPackageRestore=true
set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
call %MSBuildDir%\msbuild jexusmanager.sln /p:Configuration=Release
@IF %ERRORLEVEL% NEQ 0 PAUSE