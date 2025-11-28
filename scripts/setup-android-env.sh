#!/usr/bin/env bash
# Setup Android SDK environment variables for building .NET for Android / MonoGame
# Usage: source ./scripts/setup-android-env.sh

set -euo pipefail

# Default SDK path used by Android Studio on Linux
export ANDROID_HOME="${ANDROID_HOME:-$HOME/Android/Sdk}"
export ANDROID_SDK_ROOT="$ANDROID_HOME"

if [ ! -d "$ANDROID_HOME" ]; then
  echo "WARNING: ANDROID_HOME ($ANDROID_HOME) does not exist. Install Android Studio SDKs first."
fi

export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/tools:$ANDROID_HOME/tools/bin:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"

echo "ANDROID_HOME=$ANDROID_HOME"
command -v adb >/dev/null 2>&1 && echo "adb found: $(command -v adb)" || echo "adb not found in PATH"
command -v sdkmanager >/dev/null 2>&1 && echo "sdkmanager found: $(command -v sdkmanager)" || echo "sdkmanager not found in PATH"
