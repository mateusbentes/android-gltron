#!/usr/bin/env bash
# Build the solution (Engine + Android Game) for the new architecture
# Usage: ./scripts/build-android.sh [-c Debug|Release] [-p ProjectDir] [-f TargetFramework]
# Defaults: -c Release, -p GltronMobileGame, -f net8.0-android

set -euo pipefail

CONFIG="Release"
PROJ_DIR="GltronMobileGame"
TFM="net8.0-android"

while getopts ":c:p:f:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    p) PROJ_DIR="$OPTARG" ;;
    f) TFM="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

if [ ! -d "$PROJ_DIR" ]; then
  echo "Project directory '$PROJ_DIR' not found. Ensure GltronMobileGame exists."
  exit 1
fi

# Determine which solution file to use
SOLUTION_FILE="GltronMobile.sln"
if [[ "$(uname)" == "Darwin" ]] && [ -f "GltronMobile.Full.sln" ]; then
  SOLUTION_FILE="GltronMobile.Full.sln"
  echo "Using full solution (includes iOS) on macOS"
else
  echo "Using Android-only solution"
fi

# Restore solution
echo "Restoring solution..."
dotnet restore "$SOLUTION_FILE"

# Build content if MGCB present, with change guard and sync to Content/Assets
if [ -f "$PROJ_DIR/Content/Content.mgcb" ]; then
  echo "Checking content changes..."
  CONTENT_FILE="$PROJ_DIR/Content/Content.mgcb"
  OUT_DIR="$PROJ_DIR/Content/bin/Android"
  OBJ_DIR="$PROJ_DIR/Content/obj/Android"
  ASSETS_DIR_SRC="$OUT_DIR/Assets"
  ASSETS_DIR_DST="$PROJ_DIR/Content/Assets"

  # Compute a simple checksum of mgcb and all assets referenced (fallback to timestamp if no sha tool)
  HASH_CMD=""
  if command -v sha256sum >/dev/null 2>&1; then HASH_CMD=sha256sum; elif command -v shasum >/dev/null 2>&1; then HASH_CMD="shasum -a 256"; fi
  STATE_FILE="$PROJ_DIR/Content/.mgcb_android_state"
  CURRENT_SIG="nohash"
  if [ -n "$HASH_CMD" ]; then
    CURRENT_SIG=$( (echo "MGCB:" && $HASH_CMD "$CONTENT_FILE" 2>/dev/null; echo "ASSETS:" && find "$PROJ_DIR/Content/Assets" -type f -printf '%P\n' 2>/dev/null | LC_ALL=C sort | while read -r f; do $HASH_CMD "$PROJ_DIR/Content/Assets/$f" 2>/dev/null || true; done) | $HASH_CMD | awk '{print $1}')
  else
    CURRENT_SIG=$(date -r "$CONTENT_FILE" +%s || stat -f %m "$CONTENT_FILE" 2>/dev/null || echo $RANDOM)
  fi
  PREV_SIG=""
  if [ -f "$STATE_FILE" ]; then PREV_SIG=$(cat "$STATE_FILE" || true); fi

  echo "Content signature (Android):"
  echo "  prev: $PREV_SIG"
  echo "  curr: $CURRENT_SIG"

  # Ensure output bin directory exists
  if [ ! -d "$OUT_DIR/Assets" ]; then
    echo "Output directory $OUT_DIR/Assets missing; forcing content build."
    FORCE_BUILD=1
  else
    FORCE_BUILD=0
  fi

  if [ "$CURRENT_SIG" != "$PREV_SIG" ] || [ "$FORCE_BUILD" -eq 1 ]; then
    echo "Building content (Android platform)..."
    if ! command -v mgcb-editor &>/dev/null && ! command -v mgcb &>/dev/null; then
      echo "MGCB CLI not found. Install with: dotnet tool install -g dotnet-mgcb-editor"
    else
      mkdir -p "$OUT_DIR" "$OBJ_DIR"
      if command -v mgcb &>/dev/null; then
        mgcb /@:"$CONTENT_FILE" /platform:Android /outputDir:"$OUT_DIR" /intermediateDir:"$OBJ_DIR"
      else
        mgcb-editor --build "$CONTENT_FILE" --platform Android --outputDir "$OUT_DIR" --intermediateDir "$OBJ_DIR"
      fi
      # Sync built XNBs into Content/Assets so runtime loads "Assets/..." from RootDirectory=Content
      if command -v rsync >/dev/null 2>&1; then
        echo "Syncing built content to $ASSETS_DIR_DST ..."
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
    echo "No content changes detected and output present; skipping MGCB build."
  fi
fi

# Build via solution (engine + game)
echo "Building solution..."
dotnet build "$SOLUTION_FILE" -c "$CONFIG"

# Optionally also build the game project for a specific TFM if needed
echo "Building Android project (explicit TFM: $TFM)..."
dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM"

echo "Done. To deploy, use your IDE or: adb install -r \"$(find \"$PROJ_DIR/bin/$CONFIG\" -type f -name '*.apk' -o -name '*.aab' 2>/dev/null | head -n1)\""
