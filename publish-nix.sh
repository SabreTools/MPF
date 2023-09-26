#! /bin/bash

# This batch file assumes the following:
# - .NET 7.0 (or newer) SDK is installed and in PATH
# - zip is installed and in PATH
# - The relevant commandline programs are already downloaded
#   and put into their respective folders
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# TODO: Re-enable MPF building after figuring out how to build Windows desktop applications on Linux
# This may require an additional package to be installed?

# Set the current directory as a variable
BUILD_FOLDER=$PWD

# Restore Nuget packages for all builds
echo "Restoring Nuget packages"
dotnet restore

# .NET 6.0
echo "Building .NET 6.0 releases"
#dotnet publish MPF/MPF.csproj --framework net6.0-windows --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net6.0-windows --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net6.0-windows --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net6.0-windows --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net6.0 --runtime osx-x64 --self-contained true -p:PublishSingleFile=true

# .NET 7.0
echo "Building .NET 6.0 releases"
#dotnet publish MPF/MPF.csproj --framework net7.0-windows --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net7.0-windows --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net7.0-windows --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
#dotnet publish MPF/MPF.csproj --framework net7.0-windows --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime win7-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime win8-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime win81-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime win10-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish MPF.Check/MPF.Check.csproj --framework net7.0 --runtime osx-x64 --self-contained true -p:PublishSingleFile=true

# Create MPF archives
#cd $BUILD_FOLDER/MPF/bin/Debug/net6.0-windows/win7-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net6.0_win7-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net6.0-windows/win8-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net6.0_win8-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net6.0-windows/win81-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net6.0_win81-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net6.0-windows/win10-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net6.0_win10-x64.zip .

#cd $BUILD_FOLDER/MPF/bin/Debug/net7.0-windows/win7-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net7.0_win7-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net7.0-windows/win8-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net7.0_win8-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net7.0-windows/win81-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net7.0_win81-x64.zip .
#cd $BUILD_FOLDER/MPF/bin/Debug/net7.0-windows/win10-x64/publish/
#zip -r $BUILD_FOLDER/MPF_net7.0_win10-x64.zip .

# Create MPF.Check archives
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/win7-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_win7-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/win8-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_win8-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/win81-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_win81-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/win10-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_win10-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/linux-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_linux-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net6.0/osx-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net6.0_osx-x64.zip .

cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/win7-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_win7-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/win8-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_win8-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/win81-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_win81-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/win10-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_win10-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/linux-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_linux-x64.zip .
cd $BUILD_FOLDER/MPF.Check/bin/Debug/net7.0/osx-x64/publish/
zip -r $BUILD_FOLDER/MPF.Check_net7.0_osx-x64.zip .