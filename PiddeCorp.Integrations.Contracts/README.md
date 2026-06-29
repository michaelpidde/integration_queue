## Build
```
dotnet pack -c Release -o ./nupkg
```

### Add local Nuget directory
```
dotnet nuget add source "C:\LocalNuget" --name "Local"
```

### Copy package there
```
copy nupkg\*.nupkg C:\LocalNuget
```

### Use package in other project
```
dotnet add package PiddeCorp.Integrations.Contracts
```

### Nuget cache-busting
```
dotnet nuget locals all --clear
```