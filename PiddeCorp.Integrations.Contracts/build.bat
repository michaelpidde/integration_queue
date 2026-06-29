@echo off

dotnet pack -c Release -o ./nupkg
copy nupkg\*.nupkg C:\LocalNuget
dotnet nuget locals all --clear
