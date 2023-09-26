@echo OFF

REM This batch file assumes the following:
REM - .NET Framework 4.8 SDK is installed and in PATH
REM - .NET 7.0 (or newer) SDK is installed and in PATH
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
msbuild MPF\MPF.csproj -target:Publish -property:TargetFramework=net48 -property:Configuration=Release -property:RuntimeIdentifiers=win7-x64
msbuild MPF.Check\MPF.Check.csproj -target:Publish -property:TargetFramework=net48  -property:Configuration=Release -property:RuntimeIdentifiers=win7-x64

REM .NET 6.0 Debug
echo Building .NET 6.0 debug releases
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win7-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win8-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win81-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net6.0-windows -r win10-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win7-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win8-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win81-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r win10-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net6.0 -r osx-x64 -c Release --self-contained true -p:PublishSingleFile=true

REM .NET 7.0 Debug
echo Building .NET 7.0 debug releases
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win7-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win8-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win81-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF\MPF.csproj -f net7.0-windows -r win10-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win7-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win8-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win81-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r win10-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check\MPF.Check.csproj -f net7.0 -r osx-x64 -c Release --self-contained true -p:PublishSingleFile=true

REM Create MPF Debug archives
cd %BUILD_FOLDER%\MPF\bin\Release\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net48.zip *

cd %BUILD_FOLDER%\MPF\bin\Release\net6.0-windows\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net6.0-windows\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net6.0-windows\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net6.0-windows\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net6.0_win10-x64.zip *

cd %BUILD_FOLDER%\MPF\bin\Release\net7.0-windows\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net7.0-windows\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net7.0-windows\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF\bin\Release\net7.0-windows\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF_net7.0_win10-x64.zip *

REM Create MPF.Check Debug archives
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net48\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net48.zip *

cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_win10-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_linux-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net6.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net6.0_osx-x64.zip *

cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\win7-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win7-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\win8-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win8-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\win81-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win81-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\win10-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_win10-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\linux-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_linux-x64.zip *
cd %BUILD_FOLDER%\MPF.Check\bin\Release\net7.0\osx-x64\publish\
7z a -tzip %BUILD_FOLDER%\MPF.Check_net7.0_osx-x64.zip *