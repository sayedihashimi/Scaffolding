:: set VERSION=9.0.1-dev
:: set DEFAULT_NUPKG_PATH=%userprofile%/.nuget/packages
:: SET SRC_DIR=%~dp0
:: set NUPKG=artifacts/packages/Debug/Shipping/

call taskkill /f /im dotnet.exe

echo DEFAULT_NUPKG_PATH: '%DEFAULT_NUPKG_PATH%'
echo SRC_DIR: '%SRC_DIR%'
echo NUPKG: '%NUPKG%'
echo VERSION: '%VERSION%'


ECHO ### START UNINSTALL
call rd /Q /S artifacts
call build.cmd
call dotnet tool uninstall -g Microsoft.dotnet-scaffold
call dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspire
call dotnet tool uninstall -g Microsoft.dotnet-scaffold-aspnet
ECHO ### END UNINSTALL

ECHO ### START DELETE PACKAGES
call cd %DEFAULT_NUPKG_PATH%
call rd /Q /S Microsoft.dotnet-scaffold
call rd /Q /S Microsoft.dotnet-scaffold-aspire
call rd /Q /S Microsoft.dotnet-scaffold-aspnet
call rd /Q /S Microsoft.DotNet.Scaffolding.Internal
call rd /Q /S Microsoft.DotNet.Scaffolding.Core
ECHO ### END DELETE PACKAGES


ECHO ### START INSTALL PACKAGES
echo NUPKG: '%NUPKG%'
call cd %NUPKG%
call dotnet tool install -g Microsoft.dotnet-scaffold --add-source %NUPKG% --version %VERSION%
call dotnet tool install -g Microsoft.dotnet-scaffold-aspire --add-source %NUPKG% --version %VERSION%
call dotnet tool install -g Microsoft.dotnet-scaffold-aspnet --add-source %NUPKG% --version %VERSION%
call cd %SRC_DIR%
ECHO ### END INSTALL PACKAGES

echo DEFAULT_NUPKG_PATH: '%DEFAULT_NUPKG_PATH%'
echo SRC_DIR: '%SRC_DIR%'
echo NUPKG: '%NUPKG%'
echo VERSION: '%VERSION%'
