[cmdletbinding()]
param(
    [string]$srcDir = "$PSScriptRoot",
    [string]$dotnetMultiLevelLookup=0,
    [string]$version="9.0.1-dev",
    [string]$defaultNupkgDir="$env:USERPROFILE\.nuget\packages",
    [string]$defaultToolsFolder="$env:USERPROFILE\.dotnet\tools\",
    [string]$defaultToolsStoreFolder="$env:USERPROFILE\.dotnet\tools\.store\",
    [string]$artifactsDir="$PSScriptRoot/artifacts/",
    [string]$nupkgDir="$PSScriptRoot/artifacts/packages/Debug/Shipping/"
)

function Clean{
    Write-Host  "### Clean" -ForegroundColor Red
    DeleteFolder -folder $artifactsDir
}
function Build{
    Write-Host "### Build" -ForegroundColor Red
    # powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -build -restore -pack %*"
    & $srcDir/eng/common/Build.ps1 -build -restore -pack -bl /clp:ErrorsOnly
}

function Install{
    Write-Host "### Install" -ForegroundColor Red
    if (-not (Test-Path $nupkgDir)) {
        Write-Error "Package source directory not found: '$nupkgDir'"
        return
    }
    & dotnet tool install -g Microsoft.dotnet-scaffold --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspire --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspnet --add-source $nupkgDir --version $version
}

function Uninstall{
    Write-Host "### Uninstall" -ForegroundColor Red
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspire
    & dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspnet

    [string[]]$foldersToDelte=@(
        (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold),
        (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold-aspire),
        (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold-aspnet),
        (Join-Path $defaultNupkgDir Microsoft.DotNet.Scaffolding.Internal),
        (Join-Path $defaultNupkgDir Microsoft.DotNet.Scaffolding.Core),
        (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold),
        (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold-aspire),
        (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold-aspnet)
    )
    [string[]]$filesToDelete = @(
        (Join-Path $defaultToolsFolder dotnet-scaffold.exe),
        (Join-Path $defaultToolsFolder dotnet-scaffold-aspire.exe),
        (Join-Path $defaultToolsFolder dotnet-scaffold-aspnet.exe)
    )

    DeleteFolder -folder $foldersToDelte
    DeleteFile -file $filesToDelete
}

function DeleteFolder{
    [cmdletbinding()]
    param(
        [string[]]$folder
    )
    process{
        if($folder){
            foreach($f in $folder){
                if(Test-Path -LiteralPath $f){
                    Remove-Item -LiteralPath $f -Recurse -Force
                }
            }
        }
    }
}

function DeleteFile{
    [cmdletbinding()]
    param(
        [string[]]$file
    )
    process{
        if($file){
            foreach($f in $file){
                if(Test-Path -LiteralPath $f){
                    Remove-Item -LiteralPath $f
                }
            }
        }
    }
}

function KillDotnet{
    Write-Host "### KillDotnet" -ForegroundColor Red
    # call taskkill /f /im dotnet.exe
    $processes = Get-Process -Name dotnet.exe -ErrorAction SilentlyContinue
    if($processes){
        foreach($p in $processes){
            Stop-Process -Id $p.Id -Force
        }
    }
}

function ResetEnv{
    Write-Host "### ResetEnv" -ForegroundColor Red
    $env:DOTNET_MULTILEVEL_LOOKUP = ""
    $env:DOTNET_ROOT=""
    $env:DOTNET_INSTALL_DIR=""
}

### Start script
KillDotnet
Clean
Build
ResetEnv
Uninstall
Install
