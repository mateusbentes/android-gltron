#!/usr/bin/env bash
# Wrapper script for building Android with Release configuration
# Usage: ./scripts/build-android-app.sh [-c Debug|Release]

set -euo pipefail

CONFIG="Release"

while getopts ":c:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

echo "Building Android app with configuration: $CONFIG"
exec ./scripts/build-android.sh -c "$CONFIG"
