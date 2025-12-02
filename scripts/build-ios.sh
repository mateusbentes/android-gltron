#!/usr/bin/env bash
# Build the iOS project for macOS
# Usage: ./scripts/build-ios.sh [-c Debug|Release] [-p Platform] [-t Target]
# Defaults: -c Release, -p iPhone, -t GltronMobileGame.iOS
# Platform options: iPhone (device), iPhoneSimulator (simulator)

set -euo pipefail

CONFIG="Release"
PLATFORM="iPhone"
PROJ_DIR="GltronMobileGame.iOS"
TFM="net8.0-ios"

while getopts ":c:p:t:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    p) PLATFORM="$OPTARG" ;;
    t) PROJ_DIR="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

# Validate platform
if [[ "$PLATFORM" != "iPhone" && "$PLATFORM" != "iPhoneSimulator" ]]; then
  echo "Error: Platform must be 'iPhone' or 'iPhoneSimulator'"
  exit 1
fi

# Check if running on macOS
if [[ "$(uname)" != "Darwin" ]]; then
  echo "Error: iOS builds require macOS"
  exit 1
fi

# Check if iOS project exists
if [ ! -d "$PROJ_DIR" ]; then
  echo "Error: iOS project directory '$PROJ_DIR' not found."
  exit 1
fi

# Check if iOS workload is installed
echo "Checking iOS workload..."
if ! dotnet workload list | grep -q ios; then
  echo "Error: iOS workload not installed. Install with: dotnet workload install ios"
  exit 1
fi

echo "Building iOS project..."
echo "  Configuration: $CONFIG"
echo "  Platform: $PLATFORM"
echo "  Project: $PROJ_DIR"

# Use full solution that includes iOS project
SOLUTION_FILE="GltronMobile.Full.sln"
if [ ! -f "$SOLUTION_FILE" ]; then
  echo "Error: Full solution file '$SOLUTION_FILE' not found."
  echo "This file should include both Android and iOS projects."
  exit 1
fi

# Restore solution
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# Build content if MGCB present, with change guard and sync to Content/Assets
CONTENT_DIR="GltronMobileGame/Content"
if [ -f "$CONTENT_DIR/Content.mgcb" ]; then
  echo "Checking content changes for iOS..."
  CONTENT_FILE="$CONTENT_DIR/Content.mgcb"
  OUT_DIR="$CONTENT_DIR/bin/iOS"
  OBJ_DIR="$CONTENT_DIR/obj/iOS"
  ASSETS_DIR_SRC="$OUT_DIR/Assets"
  ASSETS_DIR_DST="$CONTENT_DIR/Assets"

  # Compute checksum of mgcb and assets; fallback to timestamp if sha not available
  HASH_CMD=""
  if command -v sha256sum >/dev/null 2>&1; then HASH_CMD=sha256sum; elif command -v shasum >/dev/null 2>&1; then HASH_CMD="shasum -a 256"; fi
  STATE_FILE="$CONTENT_DIR/.mgcb_ios_state"
  CURRENT_SIG="nohash"
  if [ -n "$HASH_CMD" ]; then
    CURRENT_SIG=$( (echo "MGCB:" && $HASH_CMD "$CONTENT_FILE" 2>/dev/null; echo "ASSETS:" && find "$CONTENT_DIR/Assets" -type f -printf '%P\n' 2>/dev/null | LC_ALL=C sort | while read -r f; do $HASH_CMD "$CONTENT_DIR/Assets/$f" 2>/dev/null || true; done) | $HASH_CMD | awk '{print $1}')
  else
    CURRENT_SIG=$(date -r "$CONTENT_FILE" +%s 2>/dev/null || stat -f %m "$CONTENT_FILE" 2>/dev/null || echo $RANDOM)
  fi
  PREV_SIG=""
  if [ -f "$STATE_FILE" ]; then PREV_SIG=$(cat "$STATE_FILE" || true); fi

  echo "Content signature (iOS):"
  echo "  prev: $PREV_SIG"
  echo "  curr: $CURRENT_SIG"

  # Ensure output bin directory exists
  if [ ! -d "$OUT_DIR/Assets" ]; then
    echo "Output directory $OUT_DIR/Assets missing; forcing iOS content build."
    FORCE_BUILD=1
  else
    FORCE_BUILD=0
  fi

  if [ "$CURRENT_SIG" != "$PREV_SIG" ] || [ "$FORCE_BUILD" -eq 1 ]; then
    echo "Building content (iOS platform)..."
    if ! command -v mgcb-editor &>/dev/null && ! command -v mgcb &>/dev/null; then
      echo "Warning: MGCB CLI not found. Install with: dotnet tool install -g dotnet-mgcb-editor"
      echo "Attempting to build without explicit content build..."
    else
      mkdir -p "$OUT_DIR" "$OBJ_DIR"
      if command -v mgcb &>/dev/null; then
        mgcb /@:"$CONTENT_FILE" /platform:iOS /outputDir:"$OUT_DIR" /intermediateDir:"$OBJ_DIR"
      else
        mgcb-editor --build "$CONTENT_FILE" --platform iOS --outputDir "$OUT_DIR" --intermediateDir "$OBJ_DIR"
      fi
      # Sync built XNBs into Content/Assets so runtime loads "Assets/..." from RootDirectory=Content
      if command -v rsync >/dev/null 2>&1; then
        echo "Syncing built iOS content to $ASSETS_DIR_DST ..."
        mkdir -p "$ASSETS_DIR_DST"
        rsync -a --delete "$ASSETS_DIR_SRC/" "$ASSETS_DIR_DST/" || true
      else
        echo "rsync not found, using cp -ru"
        mkdir -p "$ASSETS_DIR_DST"
        cp -ru "$ASSETS_DIR_SRC/"* "$ASSETS_DIR_DST/" 2>/dev/null || true
      fi
      echo "$CURRENT_SIG" > "$STATE_FILE" || true
    fi
  else
    echo "No iOS content changes detected and output present; skipping MGCB build."
  fi
fi

# Build via solution first (engine + shared dependencies)
echo "Building solution..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG"

# Build iOS project with specific platform
echo "Building iOS project for $PLATFORM..."
if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhoneSimulator
else
  dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM" /p:Platform=iPhone
fi

# Find and report the output
echo ""
echo "Build complete!"

# Look for IPA or app bundle
OUTPUT_DIR="$PROJ_DIR/bin/$CONFIG/$TFM"
if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
  OUTPUT_DIR="$OUTPUT_DIR-iossimulator"
else
  OUTPUT_DIR="$OUTPUT_DIR-ios"
fi

if [ -d "$OUTPUT_DIR" ]; then
  echo "Output directory: $OUTPUT_DIR"
  
  # Look for .app bundle
  APP_BUNDLE=$(find "$OUTPUT_DIR" -name "*.app" -type d 2>/dev/null | head -n1)
  if [ -n "$APP_BUNDLE" ]; then
    echo "App bundle: $APP_BUNDLE"
  fi
  
  # Look for .ipa file
  IPA_FILE=$(find "$OUTPUT_DIR" -name "*.ipa" -type f 2>/dev/null | head -n1)
  if [ -n "$IPA_FILE" ]; then
    echo "IPA file: $IPA_FILE"
  fi
  
  if [[ "$PLATFORM" == "iPhoneSimulator" ]]; then
    echo ""
    echo "To run in simulator:"
    echo "  1. Open Xcode and start a simulator"
    echo "  2. Drag the .app bundle to the simulator"
    echo "  3. Or use: xcrun simctl install booted \"$APP_BUNDLE\""
  else
    echo ""
    echo "To deploy to device:"
    echo "  1. Connect your iOS device"
    echo "  2. Use Xcode to deploy the app bundle"
    echo "  3. Or use deployment tools with proper provisioning"
  fi
else
  echo "Warning: Expected output directory not found: $OUTPUT_DIR"
  echo "Check build output above for errors."
fi

echo ""
echo "Note: iOS deployment requires proper Apple Developer certificates and provisioning profiles."
