call vsclean
set VERSION=1.0.33.1
dotnet restore
dotnet build --configuration Release --no-restore
dotnet pack --configuration Release --no-build
dotnet nuget push bin/Release/MikeNakis.Kit.%VERSION%.nupkg --source https://api.nuget.org/v3/index.json --api-key %NUGET_API_KEY%
