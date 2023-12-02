# This batch file assumes the following:
# - .NET 8.0 (or newer) SDK is installed and in PATH
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

# Create the build matrix arrays
$UI_FRAMEWORKS = @('net6.0-windows', 'net8.0-windows')
$UI_RUNTIMES = @('win-x64')
$CHECK_FRAMEWORKS = @('net6.0', 'net8.0')
$CHECK_RUNTIMES = @('win-x64', 'linux-x64', 'osx-x64')

# Use expanded lists, if requested
if ($USE_ALL.IsPresent)
{
    $UI_FRAMEWORKS = @('net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0-windows', 'net6.0-windows', 'net7.0-windows', 'net8.0-windows')
    $UI_RUNTIMES = @('win-x86', 'win-x64')
    $CHECK_FRAMEWORKS = @('net20', 'net35', 'net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0')
    $CHECK_RUNTIMES = @('win-x86', 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64')
}

# Create the filter arrays
$SINGLE_FILE_CAPABLE = @('net5.0', 'net5.0-windows', 'net6.0', 'net6.0-windows', 'net7.0', 'net7.0-windows', 'net8.0', 'net8.0-windows')
$VALID_CROSS_PLATFORM_FRAMEWORKS = @('netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0')
$VALID_CROSS_PLATFORM_RUNTIMES = @('win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64')

# Only build if requested
if (!$NO_BUILD.IsPresent)
{
	# Restore Nuget packages for all builds
	Write-Host "Restoring Nuget packages"
	dotnet restore

	# Build UI
	foreach ($FRAMEWORK in $UI_FRAMEWORKS)
	{
		foreach ($RUNTIME in $UI_RUNTIMES)
		{
			# Only .NET 5 and above can publish to a single file
			if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK)
			{
				dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
				dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
			}
			else
			{
				dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
				dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
			}
		}
	}

	# Build Check
	foreach ($FRAMEWORK in $CHECK_FRAMEWORKS)
	{
		foreach ($RUNTIME in $CHECK_RUNTIMES)
		{
			# If we have an invalid combination of framework and runtime
			if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME)
			{
				continue
			}

			# Only .NET 5 and above can publish to a single file
			if ($SINGLE_FILE_CAPABLE -contains $FRAMEWORK)
			{
				dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
				dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
			}
			else
			{
				dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT
				dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:DebugType=None -p:DebugSymbols=false
			}
		}
	}
}

# Only create archives if requested
if (!$NO_ARCHIVE.IsPresent)
{
	# Create UI archives
	foreach ($FRAMEWORK in $UI_FRAMEWORKS)
	{
		foreach ($RUNTIME in $UI_RUNTIMES)
		{
			Set-Location -Path $BUILD_FOLDER\MPF\bin\Debug\$FRAMEWORK\$RUNTIME\publish\
			if ($INCLUDE_PROGRAMS.IsPresent)
			{
				7z a -tzip $BUILD_FOLDER\MPF_${FRAMEWORK}_${RUNTIME}_debug.zip *
			}
			else
			{
				7z a -tzip -x!Programs\* $BUILD_FOLDER\MPF_${FRAMEWORK}_${RUNTIME}_debug.zip *
			}
			Set-Location -Path $BUILD_FOLDER\MPF\bin\Release\$FRAMEWORK\$RUNTIME\publish\
			if ($INCLUDE_PROGRAMS.IsPresent)
			{
				7z a -tzip $BUILD_FOLDER\MPF_${FRAMEWORK}_${RUNTIME}_release.zip *
			}
			else
			{
				7z a -tzip -x!Programs\* $BUILD_FOLDER\MPF_${FRAMEWORK}_${RUNTIME}_release.zip *
			}
		}
	}

	# Create Check archives
	foreach ($FRAMEWORK in $CHECK_FRAMEWORKS)
	{
		foreach ($RUNTIME in $CHECK_RUNTIMES)
		{
			# If we have an invalid combination of framework and runtime
			if ($VALID_CROSS_PLATFORM_FRAMEWORKS -notcontains $FRAMEWORK -and $VALID_CROSS_PLATFORM_RUNTIMES -contains $RUNTIME)
			{
				continue
			}

			Set-Location -Path $BUILD_FOLDER\MPF.Check\bin\Debug\$FRAMEWORK\$RUNTIME\publish\
			7z a -tzip $BUILD_FOLDER\MPF.Check_${FRAMEWORK}_${RUNTIME}_debug.zip *
			Set-Location -Path $BUILD_FOLDER\MPF.Check\bin\Release\$FRAMEWORK\${RUNTIME}\publish\
			7z a -tzip $BUILD_FOLDER\MPF.Check_${FRAMEWORK}_${RUNTIME}_release.zip *
		}
	}

	# Reset the directory
	Set-Location -Path $PSScriptRoot
}
