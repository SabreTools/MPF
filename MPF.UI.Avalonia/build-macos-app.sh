#!/usr/bin/env bash
set -euo pipefail
RID="${1:-osx-arm64}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export DOTNET_ROOT="$ROOT/.dotnet"; export PATH="$ROOT/.dotnet:$PATH"
OUT="$ROOT/MPF.UI.Avalonia/bin/publish/$RID"
dotnet publish "$ROOT/MPF.UI.Avalonia/MPF.UI.Avalonia.csproj" \
  -c Release -f net10.0 -r "$RID" --self-contained true \
  -p:PublishSingleFile=false -o "$OUT"
APP="$ROOT/MPF.UI.Avalonia/bin/MPF.app"
rm -rf "$APP"; mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$OUT/." "$APP/Contents/MacOS/"
cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>CFBundleName</key><string>MPF</string>
  <key>CFBundleDisplayName</key><string>Media Preservation Frontend</string>
  <key>CFBundleIdentifier</key><string>com.sabretools.mpf</string>
  <key>CFBundleVersion</key><string>3.7.1</string>
  <key>CFBundleShortVersionString</key><string>3.7.1</string>
  <key>CFBundleExecutable</key><string>MPF.Avalonia</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
  <key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST
echo "Built $APP"
