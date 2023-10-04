@echo OFF

REM This batch file assumes the following:
REM - .NET Framework 4.8 SDK is installed and in PATH
REM - .NET 7.0 (or newer) SDK is installed and in PATH
REM - 7-zip commandline (7z.exe) is installed and in PATH
REM - Git for Windows is installed and in PATH
REM - The relevant commandline programs are already downloaded
REM   and put into their respective folders
REM
REM If any of these are not satisfied, the operation may fail
REM in an unpredictable way and result in an incomplete output.

REM Set the current directory as a variable
set BUILD_FOLDER=%~dp0

REM Set the current commit hash
for /f %%i in ('git log --pretty^=%%H -1') do set COMMIT=%%i

REM Restore Nuget packages for all builds
echo Restoring Nuget packages
dotnet restore

REM .NET Framework 4.8 Debug
echo Building .NET Framework 4.8 debug
msbuild MPF\MPF.csproj -target:Publish -property:TargetFramework=net48 -property:RuntimeIdentifiers=win7-x64 -property:VersionSuffix=%COMMIT%
msbuild MPF.Check\MPF.Check.csproj -target:Publish -property:TargetFramework=net48 -property:RuntimeIdentifiers=win7-x64 -property:VersionSuffix=%COMMIT%

REM .NET Framework 4.8 Release
echo Building .NET Framework 4.8 release
msbuild MPF\MPF.csproj -target:Publish -property:TargetFramework=net48 -property:Configuration=Release -property:RuntimeIdentifiers=win7-x64 -property:VersionSuffix=%COMMIT%
msbuild MPF.Check\MPF.Check.csproj -target:Publish -property:TargetFramework=net48  -property:Configuration=Release -property:RuntimeIdentifiers=win7-x64 -property:VersionSuffix=%COMMIT%

REM .NET 6.0 Debug
echo Building .NET 6.0 debug
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r linux-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r osx-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true

REM .NET 6.0 Release
echo Building .NET 6.0 release
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r linux-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r osx-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true

REM .NET 7.0 Debug
echo Building .NET 7.0 debug
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r linux-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r osx-x64 --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true

REM .NET 7.0 Release
echo Building .NET 7.0 release
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r linux-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r osx-x64 -c Release --self-contained true --version-suffix %COMMIT% -p:PublishSingleFile=true

REM Create MPF Debug archives
cd %BUILD_FOLDER%\MPF\bin\Debug\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net48_debug.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net6.0-windows\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win-x64_debug.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net7.0-windows\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win-x64_debug.zip *

REM Create MPF Release archives
cd %BUILD_FOLDER%\MPF\bin\Release\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net48_release.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net6.0-windows\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win-x64_release.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net7.0-windows\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win-x64_release.zip *

REM Create MPF.Check Debug archives
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net48_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win-x64_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_linux-x64_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_osx-x64_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net7.0\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win-x64_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net7.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_linux-x64_debug.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net7.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_osx-x64_debug.zip *

REM Create MPF.Check Release archives
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net48_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win-x64_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_linux-x64_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_osx-x64_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\win-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win-x64_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_linux-x64_release.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_osx-x64_release.zip *