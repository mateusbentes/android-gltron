#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
PROJECT="GltronMobileGame/GltronAndroid.csproj"

echo "Building Android content via MSBuild (MonoGame.Content.Builder.Task)"
dotnet build "$PROJECT" -c Debug

CONTENT_OUT="GltronMobileGame/Content/bin/Android/Content"

if [ ! -d "$CONTENT_OUT" ]; then
  echo "ERROR: Expected content output not found: $CONTENT_OUT"
  exit 1
fi

# Check a couple of expected xnb outputs
check_file() {
  local stem="$1"
  if [ ! -f "$CONTENT_OUT/$stem.xnb" ]; then
    echo "ERROR: Missing built content: $CONTENT_OUT/$stem.xnb"
    exit 1
  fi
  echo "OK: $stem.xnb present"
}

check_file Fonts/Default
check_file Assets/game_engine
check_file Assets/game_crash
check_file Assets/song_revenge_of_cats

echo "All checks passed."
