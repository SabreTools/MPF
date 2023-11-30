# This batch file assumes the following:
# - .NET 8.0 (or newer) SDK is installed and in PATH
# - 7-zip commandline (7z.exe) is installed and in PATH
# - Git for Windows is installed and in PATH
# - The relevant commandline programs are already downloaded
#   and put into their respective folders
#
# If any of these are not satisfied, the operation may fail
# in an unpredictable way and result in an incomplete output.

# Set the current directory as a variable
$BUILD_FOLDER = $PSScriptRoot

# Set the current commit hash
$COMMIT = git log --pretty=format:"%H" -1

# Create the build matrix arrays
$UI_FRAMEWORKS = @('net6.0-windows', 'net8.0-windows') #@('net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0-windows', 'net6.0-windows', 'net7.0-windows', 'net8.0-windows')
$UI_RUNTIMES = @('win-x64') #@('win-x86', 'win-x64')
$CHECK_FRAMEWORKS = @('net6.0', 'net8.0') #@('net20', 'net35', 'net40', 'net452', 'net462', 'net472', 'net48', 'netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0')
$CHECK_RUNTIMES = @('win-x64', 'linux-x64', 'osx-x64') #@('win-x86', 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')

# Restore Nuget packages for all builds
Write-Host "Restoring Nuget packages"
dotnet restore

# Build UI
foreach ($FRAMEWORK in $UI_FRAMEWORKS)
{
	foreach ($RUNTIME in $UI_RUNTIMES)
	{
		dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
		dotnet publish MPF\MPF.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
	}
}

# Build Check
foreach ($FRAMEWORK in $CHECK_FRAMEWORKS)
{
	foreach ($RUNTIME in $CHECK_RUNTIMES)
	{
		dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Debug --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true
		dotnet publish MPF.Check\MPF.Check.csproj -f $FRAMEWORK -r $RUNTIME -c Release --self-contained true --version-suffix $COMMIT -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
	}
}

# Create UI archives
foreach ($FRAMEWORK in $UI_FRAMEWORKS)
{
	for ($j = 0; $j -le ($UI_RUNTIMES.length - 1); $j += 1)
	{
		Set-Location -Path $BUILD_FOLDER\MPF\bin\Debug\$FRAMEWORK\$RUNTIME\publish\
		7z a -tzip $BUILD_FOLDER\MPF_$FRAMEWORK_$RUNTIME_debug.zip *
		Set-Location -Path $BUILD_FOLDER\MPF\bin\Release\$FRAMEWORK\$RUNTIME\publish\
		7z a -tzip $BUILD_FOLDER\MPF_$FRAMEWORK_$RUNTIME_release.zip *
	}
}

# Create Check archives
foreach ($FRAMEWORK in $CHECK_FRAMEWORKS)
{
	for ($j = 0; $j -le ($CHECK_RUNTIMES.length - 1); $j += 1)
	{
		Set-Location -Path $BUILD_FOLDER\MPF.Check\bin\Debug\$FRAMEWORK\$RUNTIME\publish\
		7z a -tzip $BUILD_FOLDER\MPF.Check_$FRAMEWORK_$RUNTIME_debug.zip *
		Set-Location -Path $BUILD_FOLDER\MPF.Check\bin\Release\$FRAMEWORK\$RUNTIME\publish\
		7z a -tzip $BUILD_FOLDER\MPF.Check_$FRAMEWORK_$RUNTIME_release.zip *
	}
}
