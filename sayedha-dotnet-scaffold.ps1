$DOTNET_ROOT = "$PSScriptRoot\.dotnet"
$DOTNET_ROOT_x86 = "$PSScriptRoot\.dotnet\x86"

# This tells .NET Core not to go looking for .NET Core in other places
$env:DOTNET_MULTILEVEL_LOOKUP = "0"

# Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
$env:PATH = "$DOTNET_ROOT;$env:PATH"
& dotnet tool list -g
& dotnet scaffold