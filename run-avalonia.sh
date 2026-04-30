#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

env DOTNET_CLI_HOME=/tmp/dotnet-home \
  dotnet run --project "$SCRIPT_DIR/MPF.Avalonia/MPF.Avalonia.csproj"
