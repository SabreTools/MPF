#! /bin/bash

# This batch file assumes the following:
# - .NET 8.0 (or newer) SDK is installed and in PATH
# - zip is installed and in PATH
# - Git is installed and in PATH
# - The relevant commandline programs are already downloaded
#   and put into their respective folders
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Optional parameters
USE_ALL=false
INCLUDE_PROGRAMS=false
NO_BUILD=false
NO_ARCHIVE=false
while getopts "upba" OPTION
do
    case $OPTION in 
    u)
        USE_ALL=true
        ;;
    p)
        INCLUDE_PROGRAMS=true
        ;;
    b)
        NO_BUILD=true
        ;;
    a)
        NO_ARCHIVE=true
        ;;
    *)
        echo "Invalid option provided"
        exit 1
        ;;
    esac
done

# Set the current directory as a variable
BUILD_FOLDER=$PWD

# Set the current commit hash
COMMIT=`git log --pretty=%H -1`

# Create the build matrix arrays
UI_FRAMEWORKS=("net8.0-windows")
UI_RUNTIMES=("win-x86" "win-x64")
CHECK_FRAMEWORKS=("net8.0")
CHECK_RUNTIMES=("win-x86" "win-x64" "linux-x64" "osx-x64")

# Use expanded lists, if requested
if [ $USE_ALL = true ]
then
    UI_FRAMEWORKS=("net40" "net452" "net462" "net472" "net48" "netcoreapp3.1" "net5.0-windows" "net6.0-windows" "net7.0-windows" "net8.0-windows")
    UI_RUNTIMES=("win-x86" "win-x64")
    CHECK_FRAMEWORKS=("net20" "net35" "net40" "net452" "net462" "net472" "net48" "netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0")
    CHECK_RUNTIMES=("win-x86" "win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64")
fi

# Create the filter arrays
SINGLE_FILE_CAPABLE=("net5.0" "net5.0-windows" "net6.0" "net6.0-windows" "net7.0" "net7.0-windows" "net8.0" "net8.0-windows")
VALID_CROSS_PLATFORM_FRAMEWORKS=("netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0")
VALID_CROSS_PLATFORM_RUNTIMES=("win-arm64" "linux-x64" "linux-arm64" "osx-x64")

# Only build if requested
if [ $NO_BUILD = false ]
then
    # Restore Nuget packages for all builds
    echo "Restoring Nuget packages"
    dotnet restore

    # Build UI
    for FRAMEWORK in "${UI_FRAMEWORKS[@]}"
    do
        for RUNTIME in "${UI_RUNTIMES[@]}"
        do
            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]
            then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]
                then
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]
            then
                dotnet publish MPF/MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                dotnet publish MPF/MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                dotnet publish MPF/MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                dotnet publish MPF/MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done

    # Build Check
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"
    do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"
        do
            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]
            then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]
                then
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]
            then
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done
fi

# Only create archives if requested
if [ $NO_ARCHIVE = false ]
then
    # Create UI archives
    for FRAMEWORK in "${UI_FRAMEWORKS[@]}"
    do
        for RUNTIME in "${UI_RUNTIMES[@]}"
        do
            cd $BUILD_FOLDER/MPF/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
            if [ $INCLUDE_PROGRAMS = true ]
            then
                zip -r $BUILD_FOLDER/MPF_${FRAMEWORK}_${RUNTIME}_debug.zip .
            else
                zip -r $BUILD_FOLDER/MPF_${FRAMEWORK}_${RUNTIME}_debug.zip . -x 'Programs/\*'
            fi
            cd $BUILD_FOLDER/MPF/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            if [ $INCLUDE_PROGRAMS = true ]
            then
                zip -r $BUILD_FOLDER/MPF_${FRAMEWORK}_${RUNTIME}_release.zip .
            else
                zip -r $BUILD_FOLDER/MPF_${FRAMEWORK}_${RUNTIME}_release.zip . -x 'Programs/\*' 
            fi
        done
    done

    # Create Check archives
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"
    do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"
        do
            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]
            then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]
                then
                    continue
                fi
            fi

            cd $BUILD_FOLDER/MPF.Check/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
            zip -r $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_debug.zip .
            cd $BUILD_FOLDER/MPF.Check/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            zip -r $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_release.zip .
        done
    done

    # Reset the directory
    cd $BUILD_FOLDER
fi