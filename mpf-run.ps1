param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $AppArguments
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir 'MPF.Avalonia\MPF.Avalonia.csproj'
$nugetConfigPath = Join-Path $scriptDir 'NuGet.Config'
$dotnetHome = Join-Path $scriptDir '.dotnet-home'
New-Item -ItemType Directory -Force -Path $dotnetHome | Out-Null
$env:DOTNET_CLI_HOME = $dotnetHome
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:APPDATA = Join-Path $dotnetHome 'AppData\Roaming'
$env:LOCALAPPDATA = Join-Path $dotnetHome 'AppData\Local'
$env:NUGET_PACKAGES = Join-Path $scriptDir '.nuget\packages'
New-Item -ItemType Directory -Force -Path $env:APPDATA | Out-Null
New-Item -ItemType Directory -Force -Path $env:LOCALAPPDATA | Out-Null
New-Item -ItemType Directory -Force -Path $env:NUGET_PACKAGES | Out-Null

$machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
$env:Path = @($machinePath, $userPath, $env:Path) -join ';'

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnet = if ($dotnetCommand) { $dotnetCommand.Source } else { $null }
$commonDotnetPaths = @(
    'C:\Program Files\dotnet\dotnet.exe',
    'C:\Program Files (x86)\dotnet\dotnet.exe',
    (Join-Path $env:USERPROFILE '.dotnet\dotnet.exe'),
    (Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe')
)

if (-not $dotnet) {
    $dotnet = $commonDotnetPaths | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if (-not $dotnet) {
    Write-Error 'The .NET SDK was not found on PATH. Install the .NET 10 SDK, then run mpf-run again.'
    exit 1
}

if (-not (Test-Path -LiteralPath $projectPath)) {
    Write-Error "Could not find the Avalonia project at: $projectPath"
    exit 1
}

$installedSdks = & $dotnet --list-sdks
if ($LASTEXITCODE -ne 0 -or -not $installedSdks) {
    Write-Error @'
dotnet is installed, but no .NET SDKs were found.

This project is pinned to .NET SDK 10.0.203 in global.json. Install the .NET 10 SDK, then open a new terminal and run mpf-run again.
'@
    exit 1
}

& $dotnet restore $projectPath --configfile $nugetConfigPath -p:TargetFramework=net10.0 '-p:NoWarn=NU1510%3BNU1902%3BNU1903'
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& $dotnet build $projectPath --no-restore -p:TargetFramework=net10.0 '-p:NoWarn=NU1510%3BNU1902%3BNU1903'
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& $dotnet run --no-build --no-restore --project $projectPath -- @AppArguments
exit $LASTEXITCODE
