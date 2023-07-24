@echo OFF

REM This batch file assumes the following:
REM - .NET Framework 4.8 SDK is installed and in PATH
REM - .NET 6.0 (or newer) SDK is installed and in PATH
REM - 7-zip commandline (7z.exe) is installed and in PATH
REM - The relevant commandline programs are already downloaded
REM   and put into their respective folders
REM
REM If any of these are not satisfied, the operation may fail
REM in an unpredictable way and result in an incomplete output.

REM Set the current directory as a variable
set BUILD_FOLDER=%~dp0

REM Restore Nuget packages for all builds
echo Restoring Nuget packages
dotnet restore

REM .NET Framework 4.8
echo Building .NET Framework 4.8 releases
msbuild MPF\MPF.csproj -target:Publish -property:TargetFramework=net48 -property:RuntimeIdentifiers=win7-x64
msbuild MPF.Check\MPF.Check.csproj -target:Publish -property:TargetFramework=net48 -property:RuntimeIdentifiers=win7-x64

REM .NET 6.0
echo Building .NET 6.0 releases
dotnet publish MPF\MPF.csproj --framework net6.0-windows --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj --framework net6.0-windows --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj --framework net6.0-windows --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj --framework net6.0-windows --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj --framework net6.0 --runtime osx-x64 --self-contained true -p:PublishSingleFile=true

REM Create MPF archives
cd %BUILD_FOLDER%\MPF\bin\Debug\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net48.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net6.0-windows\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net6.0-windows\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net6.0-windows\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Debug\net6.0-windows\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win10-x64.zip *

REM Create MPF.Check archives
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net48.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win10-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_linux-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Debug\net6.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_osx-x64.zip *