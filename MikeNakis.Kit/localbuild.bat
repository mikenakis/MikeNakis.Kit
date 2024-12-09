@echo off

:: This batch file is used for troubleshooting stuff locally, without involving github or nuget.

if "%SUFFIX%"=="" (
  set SUFFIX=1
) else (
  set /A SUFFIX=SUFFIX+1
)
set RELEASE_VERSION=1.0.%SUFFIX%
call vsclean
rd /S/Q %USERPROFILE%\Personal\Dev\Dotnet\Main\LocalNugetSource
mkdir %USERPROFILE%\Personal\Dev\Dotnet\Main\LocalNugetSource
dotnet pack
dotnet nuget push bin/Release/MikeNakis.Kit.%RELEASE_VERSION%.nupkg --source %USERPROFILE%\Personal\Dev\Dotnet\Main\LocalNugetSource
