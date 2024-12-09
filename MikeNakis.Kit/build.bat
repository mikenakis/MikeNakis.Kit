call vsclean
set VERSION=1.0.331
dotnet restore
dotnet build --configuration Release --no-restore
dotnet pack --configuration Release --no-build --version-suffix=10.1
dotnet nuget push bin/Release/MikeNakis.Kit.%VERSION%.nupkg --source https://api.nuget.org/v3/index.json --api-key %NUGET_API_KEY%
