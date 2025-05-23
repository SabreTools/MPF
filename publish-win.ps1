# This batch file assumes the following:
# - .NET 9.0 (or newer) SDK is installed and in PATH
# - 7-zip commandline (7z.exe) is installed and in PATH
# - Git for Windows is installed and in PATH
# - The relevant commandline programs are already downloaded
#   and put into their respective folders
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Optional parameters
param(
    [Parameter(Mandatory = $false)]
    [Alias("UseAll")]
    [switch]$USE_ALL,

    [Parameter(Mandatory = $false)]
    [Alias("IncludeDebug")]
    [switch]$INCLUDE_DEBUG,

    [Parameter(Mandatory = $false)]
    [Alias("IncludePrograms")]
    [switch]$INCLUDE_PROGRAMS,

    [Parameter(Mandatory = $false)]
    [Alias("NoBuild")]
    [switch]$NO_BUILD,

    [Parameter(Mandatory = $false)]
    [Alias("NoArchive")]
    [switch]$NO_ARCHIVE
)

# Set the current directory as a variable
$BUILD_FOLDER = $PSScriptRoot

# Set the current commit hash
$COMMIT = git log --pretty=format:"%H" -1

# Output the selected options
Write-Host "Selected Options:"
Write-Host "  Use all frameworks (-UseAll)          $USE_ALL"
Write-Host "  Include debug builds (-IncludeDebug)  $INCLUDE_DEBUG"
Write-Host "  Include programs (-IncludePrograms)   $INCLUDE_PROGRAMS"
Write-Host "  No build (-NoBuild)                   $NO_BUILD"
Write-Host "  No archive (-NoArchive)               $NO_ARCHIVE"
Write-Host " "

# Create the build matrix arrays
$UI_FRAMEWORKS = @('net9.0-windows')
$UI_RUNTIMES = @('win-x86', 'win-x64')
$CHECK_FRAMEWORKS = @('net9.0')
$CHECK_RUNTIMES = @('win-x86', 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')

# Use expanded framework lists, if requested
if ($USE_ALL.IsPresent) {
    $UI_FRAMEWORKS = @('net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0-windows', 'net6.0-windows', 'net7.0-windows', 'net8.0-windows', 'net9.0-windows')
    $CHECK_FRAMEWORKS = @('net20', 'net35', 'net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0')
}

# Create the filter arrays
$SINGLE_FILE_CAPABLE = @('net5.0', 'net5.0-windows', 'net6.0', 'net6.0-windows', 'net7.0', 'net7.0-windows', 'net8.0', 'net8.0-windows', 'net9.0', 'net9.0-windows')
$VALID_APPLE_FRAMEWORKS = @('net6.0', 'net7.0', 'net8.0', 'net9.0')
$VALID_CROSS_PLATFORM_FRAMEWORKS = @('netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0')
$VALID_CROSS_PLATFORM_RUNTIMES = @('win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')

# Download programs step
function Download-Programs {
    # Define download constants
    $DL_PREFIXES = ("Aaru", "Creator", "Redumper")
    $DL_MAP = @{
        # Aaru
        "Aaru_linux-arm64"     = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_linux_arm64.tar.gz"
        "Aaru_linux-x64"       = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_linux_amd64.tar.gz"
        "Aaru_osx-arm64"       = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_macos.zip"
        "Aaru_osx-x64"         = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_macos.zip"
        "Aaru_win-arm64"       = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_aarch64.zip"
        "Aaru_win-x86"         = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_x86.zip"
        "Aaru_win-x64"         = "https://github.com/aaru-dps/Aaru/releases/download/v5.3.2/aaru-5.3.2_windows_x64.zip"

        # DiscImageCreator
        "Creator_linux-arm64"  = ""
        "Creator_linux-x64"    = "https://github.com/user-attachments/files/20000183/DiscImageCreator_20250501.tar.gz"
        "Creator_osx-arm64"    = "https://github.com/user-attachments/files/20000184/DiscImageCreator_20250501.zip"
        "Creator_osx-x64"      = "https://github.com/user-attachments/files/20000184/DiscImageCreator_20250501.zip"
        "Creator_win-arm64"    = ""
        "Creator_win-x86"      = "https://github.com/user-attachments/files/20000181/DiscImageCreator_20250501.zip"
        "Creator_win-x64"      = "https://github.com/user-attachments/files/20000181/DiscImageCreator_20250501.zip"

        # Redumper
        "Redumper_linux-arm64" = ""
        "Redumper_linux-x64"   = "https://github.com/superg/redumper/releases/download/build_585/redumper-2025.05.21_build585-Linux64.zip"
        "Redumper_osx-arm64"   = "https://github.com/superg/redumper/releases/download/build_585/redumper-2025.05.21_build585-Darwin64.zip"
        "Redumper_osx-x64"     = "https://github.com/superg/redumper/releases/download/build_585/redumper-2025.05.21_build585-Darwin64.zip"
        "Redumper_win-arm64"   = ""
        "Redumper_win-x86"     = "https://github.com/superg/redumper/releases/download/build_585/redumper-2025.05.21_build585-Windows32.zip"
        "Redumper_win-x64"     = "https://github.com/superg/redumper/releases/download/build_585/redumper-2025.05.21_build585-Windows64.zip"
    }

    # Download and extract files
    Write-Host "===== Downloading Required Programs ====="
    foreach ($PREFIX in $DL_PREFIXES) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            # Check for a valid URL
            $DL_KEY = $PREFIX + "_" + $RUNTIME
            $URL = $DL_MAP[$DL_KEY]
            if ( [string]::IsNullOrEmpty($URL) ) {
                continue
            }

            # Download the file to a predictable local file
            $EXT = [System.IO.Path]::GetExtension($URL)
            $OUTNAME = $PREFIX + "_" + $RUNTIME + $EXT
            Invoke-WebRequest -Uri $URL -OutFile $OUTNAME

            $TEMPDIR = $PREFIX + "_" + $RUNTIME + "-temp"
            $OUTDIR = $PREFIX + "_" + $RUNTIME + "-dir"

            # Handle gzipped files separately
            if ($EXT -eq ".gz") {
                mkdir $TEMPDIR
                tar -xvf $OUTNAME -C $TEMPDIR
            }
            else {
                Expand-Archive -LiteralPath $OUTNAME -DestinationPath "$TEMPDIR"
            }

            # Create the proper structure
            Move-Item -Path "$BUILD_FOLDER/$TEMPDIR/*" -Destination "$BUILD_FOLDER/$OUTDIR"
            Remove-Item -Path "$TEMPDIR" -Recurse -Force

            if ([System.IO.Directory]::Exists("$OUTDIR/bin")) {
                Move-Item -Path "$BUILD_FOLDER/$OUTDIR/bin/*" -Destination "$BUILD_FOLDER/$OUTDIR"
            }

            # Remove empty subdirectories
            $EMPTY = Get-ChildItem $OUTDIR -directory -recurse | Where-Object { (Get-ChildItem $_.fullName).count -eq 0 } | Select-Object -expandproperty FullName
            $EMPTY | Foreach-Object { Remove-Item $_ }
        }
    }

    # Create UI directories and copy data
    foreach ($FRAMEWORK in $UI_FRAMEWORKS) {
        foreach ($RUNTIME in $UI_RUNTIMES) {
            foreach ($PREFIX in $DL_PREFIXES) {
                $OUTDIR = $BUILD_FOLDER + "/" + $PREFIX + "_" + $RUNTIME + "-dir"
                if (![System.IO.Directory]::Exists($PREFIX + "_" + $RUNTIME + "-dir")) {
                    continue
                }

                if ($INCLUDE_DEBUG.IsPresent) {
                    New-Item -Name "MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX" -Type Directory -ErrorAction SilentlyContinue
                    Copy-Item -Path "$OUTDIR/*" -Destination "MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX/" -Recurse -Force
                }

                New-Item -Name "MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX" -Type Directory -ErrorAction SilentlyContinue
                Copy-Item -Path "$OUTDIR/*" -Destination "MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX/" -Recurse -Force

            }
        }
    }

    # Create CLI directories and copy data
    foreach ($FRAMEWORK in $CHECK_FRAMEWORKS) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            foreach ($PREFIX in $DL_PREFIXES) {
                $OUTDIR = $BUILD_FOLDER + "/" + $PREFIX + "_" + $RUNTIME + "-dir"
                if (![System.IO.Directory]::Exists($OUTDIR)) {
                    continue
                }

                if ($INCLUDE_DEBUG.IsPresent) {
                    New-Item -Name "MPF.CLI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX" -Type Directory -ErrorAction SilentlyContinue
                    Copy-Item -Path "$OUTDIR/*" -Destination "MPF.CLI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX/" -Recurse -Force
                }

                New-Item -Name "MPF.CLI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX" -Type Directory -ErrorAction SilentlyContinue
                Copy-Item -Path "$OUTDIR/*" -Destination "MPF.CLI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/Programs/$PREFIX/" -Recurse -Force

            }
        }
    }

    # Clean up the downloaded files and directories
    foreach ($PREFIX in $DL_PREFIXES) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            $DL_KEY = $PREFIX + "_" + $RUNTIME
            $URL = $DL_MAP[$DL_KEY]
            if ( [string]::IsNullOrEmpty($URL) ) {
                continue
            }

            $EXT = [System.IO.Path]::GetExtension($URL)
            $OUTNAME = $PREFIX + "_" + $RUNTIME + $EXT
            $OUTDIR = $PREFIX + "_" + $RUNTIME + "-dir"

            Remove-Item -Path $OUTNAME
            Remove-Item -Path $OUTDIR -Recurse -Force
        }
    }
}

# Only build if requested
if (!$NO_BUILD.IsPresent) {
    # Restore Nuget packages for all builds
    Write-Host "Restoring Nuget packages"
    dotnet restore

    # Create Nuget Packages
    dotnet pack MPF.ExecutionContexts/MPF.ExecutionContexts.csproj --output $BUILD_FOLDER
    dotnet pack MPF.Processors/MPF.Processors.csproj --output $BUILD_FOLDER

    # Build UI
    foreach ($FRAMEWORK in $UI_FRAMEWORKS) {
        foreach ($RUNTIME in $UI_RUNTIMES) {
            # Output the current build
            Write-Host "===== Build UI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only .NET 5 and above can publish to a single file
            if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK) {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                }
                dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            }
            else {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                }
                dotnet publish MPF.UI/MPF.UI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            }
        }
    }

    # Build CLI
    foreach ($FRAMEWORK in $CHECK_FRAMEWORKS) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            # Output the current build
            Write-Host "===== Build CLI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only .NET 5 and above can publish to a single file
            if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK) {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                }
                dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            }
            else {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                }
                dotnet publish MPF.CLI/MPF.CLI.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            }
        }
    }

    # Build Check
    foreach ($FRAMEWORK in $CHECK_FRAMEWORKS) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            # Output the current build
            Write-Host "===== Build Check - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only .NET 5 and above can publish to a single file
            if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK) {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
                }
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
            }
            else {
                # Only include Debug if set
                if ($INCLUDE_DEBUG.IsPresent) {
                    dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
                }
                dotnet publish MPF.Check/MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
            }
        }
    }
}

# Only create archives if requested
if (!$NO_ARCHIVE.IsPresent) {
    # Download and extract, if needed
    if ($INCLUDE_PROGRAMS.IsPresent) {
        Download-Programs
    }

    # Create UI archives
    foreach ($FRAMEWORK in $UI_FRAMEWORKS) {
        foreach ($RUNTIME in $UI_RUNTIMES) {
            # Output the current build
            Write-Host "===== Archive UI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only include Debug if set
            if ($INCLUDE_DEBUG.IsPresent) {
                Set-Location -Path $BUILD_FOLDER/MPF.UI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                if ($INCLUDE_PROGRAMS.IsPresent) {
                    7z a -tzip $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_debug.zip *
                }
                else {
                    7z a -tzip -x!Programs/* $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_debug.zip *
                }
            }
            
            Set-Location -Path $BUILD_FOLDER/MPF.UI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            if ($INCLUDE_PROGRAMS.IsPresent) {
                7z a -tzip $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_release.zip *
            }
            else {
                7z a -tzip -x!Programs/* $BUILD_FOLDER/MPF.UI_${FRAMEWORK}_${RUNTIME}_release.zip *
            }
        }
    }

    # Create CLI archives
    foreach ($FRAMEWORK in $CHECK_FRAMEWORKS) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            # Output the current build
            Write-Host "===== Archive CLI - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only include Debug if set
            if ($INCLUDE_DEBUG.IsPresent) {
                Set-Location -Path $BUILD_FOLDER/MPF.CLI/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                if ($INCLUDE_PROGRAMS.IsPresent) {
                    7z a -tzip $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_debug.zip *
                }
                else {
                    7z a -tzip -x!Programs/* $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_debug.zip *
                }
            }
            
            Set-Location -Path $BUILD_FOLDER/MPF.CLI/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            if ($INCLUDE_PROGRAMS.IsPresent) {
                7z a -tzip $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_release.zip *
            }
            else {
                7z a -tzip -x!Programs/* $BUILD_FOLDER/MPF.CLI_${FRAMEWORK}_${RUNTIME}_release.zip *
            }
        }
    }

    # Create Check archives
    foreach ($FRAMEWORK in $CHECK_FRAMEWORKS) {
        foreach ($RUNTIME in $CHECK_RUNTIMES) {
            # Output the current build
            Write-Host "===== Archive Check - $FRAMEWORK, $RUNTIME ====="

            # If we have an invalid combination of framework and runtime
            if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME) {
                Write-Host "Skipped due to invalid combination"
                continue
            }

            # If we have Apple silicon but an unsupported framework
            if ($VALID_APPLE_FRAMEWORKS -notcontains $FRAMEWORK -and $RUNTIME -eq 'osx-arm64') {
                Write-Host "Skipped due to no Apple Silicon support"
                continue
            }

            # Only include Debug if set
            if ($INCLUDE_DEBUG.IsPresent) {
                Set-Location -Path $BUILD_FOLDER/MPF.Check/bin/Debug/${FRAMEWORK}/${RUNTIME}/publish/
                7z a -tzip $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_debug.zip *
            }
            Set-Location -Path $BUILD_FOLDER/MPF.Check/bin/Release/${FRAMEWORK}/${RUNTIME}/publish/
            7z a -tzip $BUILD_FOLDER/MPF.Check_${FRAMEWORK}_${RUNTIME}_release.zip *
        }
    }

    # Reset the directory
    Set-Location -Path $PSScriptRoot
}
