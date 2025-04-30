#!/bin/bash

# This batch file assumes the following:
# - .NET 9.0 (or newer) SDK is installed and in PATH
# - zip is installed and in PATH
# - wget is installed an in PATH
# - Git is installed and in PATH
# - The relevant commandline programs are already downloaded
#   and put into their respective folders
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Optional parameters
USE_ALL=false
INCLUDE_DEBUG=false
INCLUDE_PROGRAMS=false
NO_BUILD=false
NO_ARCHIVE=false
while getopts "udpba" OPTION; do
    case $OPTION in
    u)
        USE_ALL=true
        ;;
    d)
        INCLUDE_DEBUG=true
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
COMMIT=$(git log --pretty=%H -1)

# Output the selected options
echo "Selected Options:"
echo "  Use all frameworks (-u)               $USE_ALL"
echo "  Include debug builds (-d)             $INCLUDE_DEBUG"
echo "  Include programs (-p)                 $INCLUDE_PROGRAMS"
echo "  No build (-b)                         $NO_BUILD"
echo "  No archive (-a)                       $NO_ARCHIVE"
echo " "

# Create the build matrix arrays
UI_FRAMEWORKS=("net9.0-windows")
UI_RUNTIMES=("win-x86" "win-x64")
CHECK_FRAMEWORKS=("net9.0")
CHECK_RUNTIMES=("win-x86" "win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")

# Use expanded framework lists, if requested
if [ $USE_ALL = true ]; then
    UI_FRAMEWORKS=("net40" "net452" "net462" "net472" "net48" "netcoreapp3.1" "net5.0-windows" "net6.0-windows" "net7.0-windows" "net8.0-windows" "net9.0-windows")
    CHECK_FRAMEWORKS=("net20" "net35" "net40" "net452" "net462" "net472" "net48" "netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0" "net9.0")
fi

# Create the filter arrays
SINGLE_FILE_CAPABLE=("net5.0" "net5.0-windows" "net6.0" "net6.0-windows" "net7.0" "net7.0-windows" "net8.0" "net8.0-windows" "net9.0" "net9.0-windows")
VALID_APPLE_FRAMEWORKS=("net6.0" "net7.0" "net8.0" "net9.0")
VALID_CROSS_PLATFORM_FRAMEWORKS=("netcoreapp3.1" "net5.0" "net6.0" "net7.0" "net8.0" "net9.0")
VALID_CROSS_PLATFORM_RUNTIMES=("win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")

# Only build if requested
if [ $NO_BUILD = false ]; then
    # Restore Nuget packages for all builds
    echo "Restoring Nuget packages"
    dotnet restore

    # Create Nuget Packages
    dotnet pack MPF.ExecutionContexts/MPF.ExecutionContexts.csproj --output $BUILD_FOLDER
    dotnet pack MPF.Processors/MPF.Processors.csproj --output $BUILD_FOLDER

    # Build UI
    for FRAMEWORK in "${UI_FRAMEWORKS[@]}"; do
        for RUNTIME in "${UI_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Build UI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]; then
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                fi
                dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                fi
                dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done

    # Build CLI
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"; do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Build CLI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]; then
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                fi
                dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                fi
                dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done

    # Build Check
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"; do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Build Check - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only .NET 5 and above can publish to a single file
            if [[ $(echo ${SINGLE_FILE_CAPABLE[@]} | fgrep -w $FRAMEWORK) ]]; then
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                fi
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            else
                # Only include Debug if set
                if [ $INCLUDE_DEBUG = true ]; then
                    dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                fi
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            fi
        done
    done
fi

# Only create archives if requested
if [ $NO_ARCHIVE = false ]; then
    # Download and extract, if needed
    if [ $INCLUDE_PROGRAMS = true ]; then
        echo "===== Downloading Required Programs ====="

        # Aaru
        # --- Skipped for now ---

        # DiscImageCreator
        wget https://github.com/user-attachments/files/18287520/DiscImageCreator_20250101.zip
        unzip -u DiscImageCreator_20250101.zip

        # Redumper
        wget https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Windows64.zip
        unzip redumper-2025.04.15_build549-Windows64.zip

        # Create directories and copy data
        for FRAMEWORK in "${UI_FRAMEWORKS[@]}"; do
            for RUNTIME in "${UI_RUNTIMES[@]}"; do
                if [ $INCLUDE_DEBUG = true ]; then
                    mkdir -p MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/Creator
                    cp -rfp Release_ANSI/* MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/Creator/

                    mkdir -p MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/Redumper
                    cp -rfp redumper-2025.04.15_build549-Windows64/bin/redumper.exe MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/Redumper/
                fi

                mkdir -p MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/Creator
                cp -rfp Release_ANSI/* MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/Creator/

                mkdir -p MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/Redumper
                cp -rfp redumper-2025.04.15_build549-Windows64/bin/redumper.exe MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/Redumper/
            done
        done

        # Clean up the downloaded files and directories
        rm DiscImageCreator_20250101.zip
        rm -r Release_ANSI
        rm redumper-2025.04.15_build549-Windows64.zip
        rm -r redumper-2025.04.15_build549-Windows64
    fi

    # Create UI archives
    for FRAMEWORK in "${UI_FRAMEWORKS[@]}"; do
        for RUNTIME in "${UI_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Archive UI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only include Debug if set
            if [ $INCLUDE_DEBUG = true ]; then
                cd $BUILD_FOLDER/MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                if [ $INCLUDE_PROGRAMS = true ]; then
                    zip -r $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_debug.zip .
                else
                    zip -r $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_debug.zip . -x 'Programs/*'
                fi
            fi
            cd $BUILD_FOLDER/MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            if [ $INCLUDE_PROGRAMS = true ]; then
                zip -r $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_release.zip .
            else
                zip -r $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_release.zip . -x 'Programs/*'
            fi
        done
    done

    # Create CLI archives
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"; do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Archive CLI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only include Debug if set
            if [ $INCLUDE_DEBUG = true ]; then
                cd $BUILD_FOLDER/MPF.CLI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                zip -r $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_debug.zip .
            fi
            cd $BUILD_FOLDER/MPF.CLI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            zip -r $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_release.zip .
        done
    done

    # Create Check archives
    for FRAMEWORK in "${CHECK_FRAMEWORKS[@]}"; do
        for RUNTIME in "${CHECK_RUNTIMES[@]}"; do
            # Output the current build
            echo "===== Archive Check - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if [[ ! $(echo ${VALID_CROSS_PLATFORM_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [[ $(echo ${VALID_CROSS_PLATFORM_RUNTIMES[@]} | fgrep -w $RUNTIME) ]]; then
                    echo "Skipped due to invalid combination"
                    continue
                fi
            fi

            # If we have Apple silicon but an unsupported framework
            if [[ ! $(echo ${VALID_APPLE_FRAMEWORKS[@]} | fgrep -w $FRAMEWORK) ]]; then
                if [ $RUNTIME = "osx-arm64" ]; then
                    echo "Skipped due to no Apple Silicon support"
                    continue
                fi
            fi

            # Only include Debug if set
            if [ $INCLUDE_DEBUG = true ]; then
                cd $BUILD_FOLDER/MPF.Check/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                zip -r $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_debug.zip .
            fi
            cd $BUILD_FOLDER/MPF.Check/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            zip -r $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_release.zip .
        done
    done

    # Reset the directory
    cd $BUILD_FOLDER
fi

# Function to download programs
download_programs() {
    # Aaru - Skipped for now
    AARU_LINUX_ARM64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_linux_arm64.tar.gz"
    AARU_LINUX_ARM64_LOCAL="aaru_linux-arm64"
    AARU_LINUX_X64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_linux_amd64.tar.gz"
    AARU_LINUX_X64_LOCAL="aaru_linux-amd64"

    AARU_MACOS_ARM64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_macos.zip"
    AARU_MACOS_ARM64_LOCAL="aaru_macos-arm64"
    AARU_MACOS_X64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_macos.zip"
    AARU_MACOS_X64_LOCAL="aaru_macos-x64"

    AARU_WIN_ARM64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_aarch64.zip"
    AARU_WIN_ARM64_LOCAL="aaru_win-arm64"
    AARU_WIN_X86_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_x86.zip"
    AARU_WIN_X86_LOCAL="aaru_win-x86"
    AARU_WIN_X64_URL="https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_x64.zip"
    AARU_WIN_X64_LOCAL="aaru_win-x64"

    #wget $AARU_LINUX_ARM64_URL -O $AARU_LINUX_ARM64_LOCAL.tar.gz
    #tar -xvf $AARU_LINUX_ARM64_LOCAL.tar.gz -C $AARU_LINUX_ARM64_LOCAL
    #wget $AARU_LINUX_X64_URL -O $AARU_LINUX_X64_LOCAL.tar.gz
    #tar -xvf $AARU_LINUX_X64_LOCAL.tar.gz -C $AARU_LINUX_X64_LOCAL
    #wget $AARU_MACOS_ARM64_URL -O $AARU_MACOS_ARM64_LOCAL.zip
    #unzip -u $AARU_MACOS_ARM64_LOCAL.zip -d $AARU_MACOS_ARM64_LOCAL
    #wget $AARU_MACOS_X64_URL -O $AARU_MACOS_X64_LOCAL.zip
    #unzip -u $AARU_MACOS_X64_LOCAL.zip -d $AARU_MACOS_X64_LOCAL
    #wget $AARU_WIN_ARM64_URL -O $AARU_WIN_ARM64_LOCAL.zip
    #unzip -u $AARU_WIN_ARM64_LOCAL.zip -d $AARU_WIN_ARM64_LOCAL
    #wget $AARU_WIN_X86_URL -O $AARU_WIN_X86_LOCAL.zip
    #unzip -u $AARU_WIN_X86_LOCAL.zip -d $AARU_WIN_X86_LOCAL
    #wget $AARU_WIN_X64_URL -O $AARU_WIN_X64_LOCAL.zip
    #unzip -u $AARU_WIN_X64_LOCAL.zip -d $AARU_WIN_X64_LOCAL

    # DiscImageCreator
    DIC_LINUX_ARM64_URL=""
    DIC_LINUX_ARM64_LOCAL="creator_linux-arm64"
    DIC_LINUX_X64_URL="https://github.com/user-attachments/files/18285720/DiscImageCreator_20250101.tar.gz"
    DIC_LINUX_X64_LOCAL="creator_linux-x64"

    DIC_MACOS_ARM64_URL="https://github.com/user-attachments/files/18285727/DiscImageCreator_20250101.zip"
    DIC_MACOS_ARM64_LOCAL="creator_macos-arm64"
    DIC_MACOS_X64_URL="https://github.com/user-attachments/files/18285727/DiscImageCreator_20250101.zip"
    DIC_MACOS_X64_LOCAL="creator_macos-x64"

    DIC_WIN_ARM64_URL=""
    DIC_WIN_ARM64_LOCAL="creator_win-arm64"
    DIC_WIN_X86_URL="https://github.com/user-attachments/files/18287520/DiscImageCreator_20250101.zip"
    DIC_WIN_X86_LOCAL="creator_win-x86"
    DIC_WIN_X64_URL="https://github.com/user-attachments/files/18287520/DiscImageCreator_20250101.zip"
    DIC_WIN_X64_LOCAL="creator_win-x64"

    #wget $DIC_LINUX_ARM64_URL -O $DIC_LINUX_ARM64_LOCAL.tar.gz
    #tar -xvf $DIC_LINUX_ARM64_LOCAL.tar.gz -C $DIC_LINUX_ARM64_LOCAL
    wget $DIC_LINUX_X64_URL -O $DIC_LINUX_X64_LOCAL.tar.gz
    tar -xvf $DIC_LINUX_X64_LOCAL.tar.gz -C $DIC_LINUX_X64_LOCAL
    wget $DIC_MACOS_ARM64_URL -O $DIC_MACOS_ARM64_LOCAL.zip
    unzip -u $DIC_MACOS_ARM64_LOCAL.zip -d $DIC_MACOS_ARM64_LOCAL
    wget $DIC_MACOS_X64_URL -O $DIC_MACOS_X64_LOCAL.zip
    unzip -u $DIC_MACOS_X64_LOCAL.zip -d $DIC_MACOS_X64_LOCAL
    #wget $DIC_WIN_ARM64_URL -O $DIC_WIN_ARM64_LOCAL.zip
    #unzip -u $DIC_WIN_ARM64_LOCAL.zip -d $DIC_WIN_ARM64_LOCAL
    wget $DIC_WIN_X86_URL -O $DIC_WIN_X86_LOCAL.zip
    unzip -u $DIC_WIN_X86_LOCAL.zip -d $DIC_WIN_X86_LOCAL
    wget $DIC_WIN_X64_URL -O $DIC_WIN_X64_LOCAL.zip
    unzip -u $DIC_WIN_X64_LOCAL.zip -d $DIC_WIN_X64_LOCAL

    # Redumper
    REDUMPER_LINUX_ARM64_URL=""
    REDUMPER_LINUX_ARM64_LOCAL="redumper_linux-arm64"
    REDUMPER_LINUX_X64_URL="https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Linux64.zip"
    REDUMPER_LINUX_X64_LOCAL="redumper_linux-x64"

    REDUMPER_MACOS_ARM64_URL="https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Darwin64.zip"
    REDUMPER_MACOS_ARM64_LOCAL="redumper_macos-arm64"
    REDUMPER_MACOS_X64_URL="https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Darwin64.zip"
    REDUMPER_MACOS_X64_LOCAL="redumper_macos-x64"

    REDUMPER_WIN_ARM64_URL=""
    REDUMPER_WIN_ARM64_LOCAL="redumper_win-arm64"
    REDUMPER_WIN_X86_URL="https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Windows32.zip"
    REDUMPER_WIN_X86_LOCAL="redumper_win-x86"
    REDUMPER_WIN_X64_URL="https://github.com/superg/redumper/releases/download/build_549/redumper-2025.04.15_build549-Windows64.zip"
    REDUMPER_WIN_X64_LOCAL="redumper_win-x64"

    #wget $REDUMPER_LINUX_ARM64_URL -O $REDUMPER_LINUX_ARM64_LOCAL.zip
    #unzip -u $REDUMPER_LINUX_ARM64_LOCAL.zip -d $REDUMPER_LINUX_ARM64_LOCAL
    wget $REDUMPER_LINUX_X64_URL -O $REDUMPER_LINUX_X64_LOCAL.zip
    unzip -u $REDUMPER_LINUX_X64_LOCAL.zip -d $REDUMPER_LINUX_X64_LOCAL
    wget $REDUMPER_MACOS_ARM64_URL -O $REDUMPER_MACOS_ARM64_LOCAL.zip
    unzip -u $REDUMPER_MACOS_ARM64_LOCAL.zip -d $REDUMPER_MACOS_ARM64_LOCAL
    wget $REDUMPER_MACOS_X64_URL -O $REDUMPER_MACOS_X64_LOCAL.zip
    unzip -u $REDUMPER_MACOS_X64_LOCAL.zip -d $REDUMPER_MACOS_X64_LOCAL
    #wget $REDUMPER_WIN_ARM64_URL -O $REDUMPER_WIN_ARM64_LOCAL.zip
    #unzip -u $REDUMPER_WIN_ARM64_LOCAL.zip -d $REDUMPER_WIN_ARM64_LOCAL
    wget $REDUMPER_WIN_X86_URL -O $REDUMPER_WIN_X86_LOCAL.zip
    unzip -u $REDUMPER_WIN_X86_LOCAL.zip -d $REDUMPER_WIN_X86_LOCAL
    wget $REDUMPER_WIN_X64_URL -O $REDUMPER_WIN_X64_LOCAL.zip
    unzip -u $REDUMPER_WIN_X64_LOCAL.zip -d $REDUMPER_WIN_X64_LOCAL
}