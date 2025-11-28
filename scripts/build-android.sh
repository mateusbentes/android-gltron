#!/usr/bin/env bash
# Build MonoGame Android project
# Usage: ./scripts/build-android.sh [-c Debug|Release] [-p ProjectDir] [-f TargetFramework]
# Defaults: -c Debug, -p GltronAndroid, -f net8.0-android

set -euo pipefail

CONFIG="Debug"
PROJ_DIR="GltronAndroid"
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
  echo "Project directory '$PROJ_DIR' not found. Create a MonoGame Android project first (dotnet new mgandroid -n GltronAndroid)."
  exit 1
fi

# Build content if MGCB present
if [ -f "$PROJ_DIR/Content/Content.mgcb" ]; then
  echo "Building content..."
  if ! command -v mgcb-editor &>/dev/null && ! command -v mgcb &>/dev/null; then
    echo "MGCB CLI not found. Install with: dotnet tool install -g dotnet-mgcb-editor"
  else
    # Prefer mgcb if available
    if command -v mgcb &>/dev/null; then
      mgcb /@:$PROJ_DIR/Content/Content.mgcb /platform:Android /outputDir:$PROJ_DIR/Content/bin/Android /intermediateDir:$PROJ_DIR/Content/obj/Android
    else
      mgcb-editor --build "$PROJ_DIR/Content/Content.mgcb" --platform Android
    fi
  fi
fi

echo "Building Android project..."
dotnet build "$PROJ_DIR" -c "$CONFIG" -f "$TFM"

echo "Done. To deploy, use your IDE or: adb install -r \"$(find \"$PROJ_DIR/bin/$CONFIG\" -type f -name '*.apk' -o -name '*.aab' 2>/dev/null | head -n1)\""
