[cmdletbinding()]
param(
    [string]$srcDir = "$PSScriptRoot",
    [string]$dotnetRoot="$PSScriptRoot/.dotnet",
    [string]$dotnetRootX86="$PSScriptRoot/.dotnet/x86",
    [string]$dotnetMultiLevelLookup=0,
    [string]$version="9.0.1-dev",
    [string]$defaultNupkgDir="$env:USERPROFILE\.nuget\packages",
    [string]$artifactsDir="$PSScriptRoot/artifacts/",
    [string]$nupkgDir="$PSScriptRoot/artifacts/packages/Debug/Shipping/"
)

function Clean(){
    Write-Output  "### Clean" -ForegroundColor Red
    # delete artifacts dir
    Remove-Item -Path $artifactsDir -Recurse -Force
}
function Build(){
    Write-Output "### Build" -ForegroundColor Red
    # powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -build -restore -pack %*"
    & $srcDir/eng/common/Build.ps1 -build -restore -pack
}

function Install(){
    Write-Output "### Install" -ForegroundColor Red
#     echo NUPKG: '%NUPKG%'
# call cd %NUPKG%
# call dotnet tool install -g Microsoft.dotnet-scaffold --add-source %NUPKG% --version %VERSION%
# call dotnet tool install -g Microsoft.dotnet-scaffold-aspire --add-source %NUPKG% --version %VERSION%
# call dotnet tool install -g Microsoft.dotnet-scaffold-aspnet --add-source %NUPKG% --version %VERSION%
# call cd %SRC_DIR%
    & dotnet tool install -g Microsoft.dotnet-scaffold --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspire --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspnet --add-source $nupkgDir --version $version
}

function Uninstall(){
    Write-Output "### Uninstall" -ForegroundColor Red
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspire
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspnet
}

function Initalize(){
    Write-Output "### Initalize" -ForegroundColor Red
    if ($env:PATH -notmatch [regex]::Escape($DOTNET_ROOT)) {
        $env:PATH = "$DOTNET_ROOT;$env:PATH"
    }
    $env:DOTNET_MULTILEVEL_LOOKUP = $dotnetMultiLevelLookup
    $env:DOTNET_ROOT=$dotnetRoot
}
function ResetEnv(){
    Write-Output "### ResetEnv" -ForegroundColor Red
    $env:PATH = ($env:PATH -split ';' | Where-Object { $_ -ne $DOTNET_ROOT }) -join ';'
    $env:DOTNET_MULTILEVEL_LOOKUP = ""
    $env:DOTNET_ROOT=""
    $env:DOTNET_INSTALL_DIR=""
}



### Start script
Clean
Uninstall
Build
ResetEnv
Install