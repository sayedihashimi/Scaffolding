[cmdletbinding()]
param(
    [string]$srcDir = "$PSScriptRoot",
    [string]$dotnetRoot="$PSScriptRoot/.dotnet",
    [string]$dotnetRootX86="$PSScriptRoot/.dotnet/x86",
    [string]$dotnetMultiLevelLookup=0,
    [string]$version="9.0.1-dev",
    [string]$defaultNupkgDir="$env:USERPROFILE\.nuget\packages",
    [string]$defaultToolsFolder="$env:USERPROFILE\.dotnet\tools\",
    [string]$defaultToolsStoreFolder="$env:USERPROFILE\.dotnet\tools\.store\",
    [string]$artifactsDir="$PSScriptRoot/artifacts/",
    [string]$nupkgDir="$PSScriptRoot/artifacts/packages/Debug/Shipping/"
)

function Clean(){
    Write-Host  "### Clean" -ForegroundColor Red
    # delete artifacts dir
    # if(test-path -Path $artifactsDir){
    #     Remove-Item -Path $artifactsDir -Recurse -Force
    # }
    DeleteFolder -f $artifactsDir
#     ECHO ### START DELETE PACKAGES
# call cd %DEFAULT_NUPKG_PATH%
# call rd /Q /S Microsoft.dotnet-scaffold
# call rd /Q /S Microsoft.dotnet-scaffold-aspire
# call rd /Q /S Microsoft.dotnet-scaffold-aspnet
# call rd /Q /S Microsoft.DotNet.Scaffolding.Internal
# call rd /Q /S Microsoft.DotNet.Scaffolding.Core
# ECHO ### END DELETE PACKAGES




    # C:\Users\sayedha\.dotnet\tools\.store\microsoft.dotnet-scaffold-aspire\9.0.1-dev\microsoft.dotnet-scaffold-aspire\9.0.1-dev
}
function Build(){
    Write-Host "### Build" -ForegroundColor Red
    # powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -build -restore -pack %*"
    & $srcDir/eng/common/Build.ps1 -build -restore -pack -bl
}

function Install(){
    Write-Host "### Install" -ForegroundColor Red
    & dotnet tool install -g Microsoft.dotnet-scaffold --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspire --add-source $nupkgDir --version $version
    & dotnet tool install -g Microsoft.dotnet-scaffold-aspnet --add-source $nupkgDir --version $version
}

function Uninstall(){
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

    # Remove-Item -Path (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold-aspire) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultNupkgDir Microsoft.dotnet-scaffold-aspnet) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultNupkgDir Microsoft.DotNet.Scaffolding.Internal) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultNupkgDir Microsoft.DotNet.Scaffolding.Core) -Recurse -Force


    # Remove-Item -Path (Join-Path $defaultToolsFolder dotnet-scaffold.exe)
    # Remove-Item -Path (Join-Path $defaultToolsFolder dotnet-scaffold-aspire.exe)
    # Remove-Item -Path (Join-Path $defaultToolsFolder dotnet-scaffold-aspnet.exe)

    # Remove-Item -Path (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold-aspire) -Recurse -Force
    # Remove-Item -Path (Join-Path $defaultToolsStoreFolder Microsoft.dotnet-scaffold-aspnet) -Recurse -Force
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
                    #'removing item {0}' -f $f | Write-Host -ForegroundColor Green
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
                    #'removing item {0}' -f $f | Write-Host -ForegroundColor Green
                }
            }
        }
    }
}

function Initalize(){
    Write-Host "### Initalize" -ForegroundColor Red
    if ($env:PATH -notmatch [regex]::Escape($DOTNET_ROOT)) {
        $env:PATH = "$DOTNET_ROOT;$env:PATH"
    }
    $env:DOTNET_MULTILEVEL_LOOKUP = $dotnetMultiLevelLookup
    $env:DOTNET_ROOT=$dotnetRoot
}
function ResetEnv(){
    Write-Host "### ResetEnv" -ForegroundColor Red
    $env:PATH = ($env:PATH -split ';' | Where-Object { $_ -ne $DOTNET_ROOT }) -join ';'
    $env:DOTNET_MULTILEVEL_LOOKUP = ""
    $env:DOTNET_ROOT=""
    $env:DOTNET_INSTALL_DIR=""
}

### Start script
Clean
Build
ResetEnv
Uninstall
Install
