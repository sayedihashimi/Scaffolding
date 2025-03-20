$DOTNET_ROOT = "$PSScriptRoot/.dotnet"
$DOTNET_ROOT_x86 = "$PSScriptRoot/.dotnet/x86"
#set NUPKG=artifacts/packages/Debug/Shipping/
$NUPKG="$PSScriptRoot/artifacts/packages/Debug/Shipping/"
$VERSION="9.0.1-dev"
$SRC_DIR="$PSScriptRoot/"
$DEFAULT_NUPKG_DIR="$env:USERPROFILE\.nuget\packages"

$env:SRC_DIR=$SRC_DIR
$env:DOTNET_ROOT=$DOTNET_ROOT
$env:DEFAULT_NUPKG_PATH = $DEFAULT_NUPKG_DIR
# This tells .NET Core not to go looking for .NET Core in other places
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
# Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
$env:PATH = "$DOTNET_ROOT;$env:PATH"
$env:NUPKG=$NUPKG
$env:VERSION=$VERSION


