#!/usr/bin/env bash
# Build a macOS .app for MPF.UI.Avalonia.
#   ./build-macos-app.sh              -> universal (arm64 + x64), runs natively on both Mac types
#   ./build-macos-app.sh universal    -> same as above
#   ./build-macos-app.sh osx-arm64    -> Apple Silicon only
#   ./build-macos-app.sh osx-x64      -> Intel only
set -euo pipefail

MODE="${1:-universal}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# Use the repo-local SDK if present (developer machines); otherwise fall back to the
# dotnet already on PATH (e.g. CI runners that installed it via actions/setup-dotnet).
if [ -d "$ROOT/.dotnet" ]; then
    export DOTNET_ROOT="$ROOT/.dotnet"; export PATH="$ROOT/.dotnet:$PATH"
fi
PROJ="$ROOT/MPF.UI.Avalonia/MPF.UI.Avalonia.csproj"
PUBROOT="$ROOT/MPF.UI.Avalonia/bin/publish"
APP="$ROOT/MPF.UI.Avalonia/bin/MPF.app"

publish() { # $1 = RID
    dotnet publish "$PROJ" -c Release -f net10.0 -r "$1" --self-contained true \
        -p:PublishSingleFile=false -o "$PUBROOT/$1"
}

is_macho() { file "$1" 2>/dev/null | grep -q "Mach-O"; }

if [ "$MODE" = "universal" ]; then
    publish "osx-arm64"
    publish "osx-x64"
    MERGED="$PUBROOT/universal"
    rm -rf "$MERGED"; cp -R "$PUBROOT/osx-arm64" "$MERGED"
    # Managed assemblies are architecture-neutral; only the native files differ.
    # Fuse each native Mach-O with its x64 counterpart into a universal binary.
    while IFS= read -r -d '' f; do
        rel="${f#"$MERGED"/}"
        x64f="$PUBROOT/osx-x64/$rel"
        if is_macho "$f" && [ -f "$x64f" ]; then
            if lipo -create "$f" "$x64f" -output "$f.uni" 2>/dev/null; then
                mv "$f.uni" "$f"
            fi
        fi
    done < <(find "$MERGED" -type f -print0)
    SRC="$MERGED"
else
    publish "$MODE"
    SRC="$PUBROOT/$MODE"
fi

rm -rf "$APP"; mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$SRC/." "$APP/Contents/MacOS/"

# App/dock icon
if [ -f "$ROOT/MPF.UI.Avalonia/Images/AppIcon.icns" ]; then
    cp "$ROOT/MPF.UI.Avalonia/Images/AppIcon.icns" "$APP/Contents/Resources/AppIcon.icns"
fi

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
  <key>CFBundleIconFile</key><string>AppIcon</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
  <key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST

# lipo strips signatures; ad-hoc re-sign the whole bundle (--deep signs the apphost and every
# nested Mach-O and writes the CodeResources seal) so the app runs on a signed-only OS and
# verifies cleanly.
codesign --force --deep --sign - "$APP" 2>/dev/null || true

# Zip the bundle next to the .app so the release-upload step on CI just consumes this artifact.
# Naming matches the cross-platform zips emitted by publish-nix.sh.
ZIP_NAME="MPF.UI.Avalonia_macos_${MODE}_release.zip"
ZIP_PATH="$ROOT/$ZIP_NAME"
rm -f "$ZIP_PATH"
( cd "$(dirname "$APP")" && zip -ry "$ZIP_PATH" "$(basename "$APP")" >/dev/null )

echo "Built $APP ($MODE)"
echo "Zipped $ZIP_PATH"
file "$APP/Contents/MacOS/MPF.Avalonia"
