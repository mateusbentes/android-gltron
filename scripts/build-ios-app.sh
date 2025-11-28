#!/usr/bin/env bash
# Wrapper script for building iOS with Release configuration
# Usage: ./scripts/build-ios-app.sh [-c Debug|Release] [-p iPhone|iPhoneSimulator]

set -euo pipefail

CONFIG="Release"
PLATFORM="iPhone"

while getopts ":c:p:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    p) PLATFORM="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

echo "Building iOS app with configuration: $CONFIG, platform: $PLATFORM"
exec ./scripts/build-ios.sh -c "$CONFIG" -p "$PLATFORM"
