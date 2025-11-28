#!/usr/bin/env bash
# End-to-end script to scaffold and build a MonoGame Android APK from the existing GltronMonoGame codebase.
# Usage: ./scripts/build-android-app.sh [-c Debug|Release] [-n ProjectName] [-t net8.0-android]
# Defaults: -c Debug, -n GltronAndroid, -t net8.0-android

set -euo pipefail

CONFIG="Debug"
APP_NAME="GltronAndroid"
TFM="net8.0-android"
SRC_DIR="GltronMonoGame"

while getopts ":c:n:t:" opt; do
  case $opt in
    c) CONFIG="$OPTARG" ;;
    n) APP_NAME="$OPTARG" ;;
    t) TFM="$OPTARG" ;;
    *) echo "Unknown option -$OPTARG" ; exit 1 ;;
  esac
done

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

# 1) Pre-flight checks
if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet SDK not found. Please install .NET 8 SDK and try again."
  exit 1
fi

ANDROID_HOME_DEFAULT="$HOME/Android/Sdk"
export ANDROID_HOME="${ANDROID_HOME:-$ANDROID_HOME_DEFAULT}"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
if [ ! -d "$ANDROID_HOME" ]; then
  echo "WARNING: ANDROID_HOME ($ANDROID_HOME) not found. Ensure Android Studio SDK is installed and ANDROID_HOME is set."
fi

# 2) Ensure MonoGame templates and MGCB tools (soft check)
if ! dotnet new --list | grep -q "mgandroid"; then
  echo "MonoGame templates not found. Installing..."
  dotnet new --install MonoGame.Templates.CSharp || true
fi

if ! command -v mgcb >/dev/null 2>&1 && ! command -v mgcb-editor >/dev/null 2>&1; then
  echo "MGCB CLI not found. Installing dotnet-mgcb-editor tool (provides mgcb-editor)."
  dotnet tool install -g dotnet-mgcb-editor || true
  export PATH="$HOME/.dotnet/tools:$PATH"
fi

# 3) Create Android project if missing
if [ ! -d "$APP_NAME" ]; then
  echo "Creating MonoGame Android project: $APP_NAME"
  dotnet new mgandroid -n "$APP_NAME"
fi

# 4) Copy game source files into the Android project
# Create target directories
mkdir -p "$APP_NAME/Video" "$APP_NAME/Sound" "$APP_NAME/Content"

copy_file() {
  local src="$1" dest="$2"
  if [ -f "$src" ]; then
    mkdir -p "$(dirname "$dest")"
    cp -f "$src" "$dest"
  fi
}

copy_file "$SRC_DIR/Game1.cs" "$APP_NAME/Game1.cs"
copy_file "$SRC_DIR/GLTronGame.cs" "$APP_NAME/GLTronGame.cs"
copy_file "$SRC_DIR/Player.cs" "$APP_NAME/Player.cs"
copy_file "$SRC_DIR/Segment.cs" "$APP_NAME/Segment.cs"
copy_file "$SRC_DIR/Vec.cs" "$APP_NAME/Vec.cs"
copy_file "$SRC_DIR/Video/HUD.cs" "$APP_NAME/Video/HUD.cs"
copy_file "$SRC_DIR/Video/ModelLoader.cs" "$APP_NAME/Video/ModelLoader.cs"
copy_file "$SRC_DIR/Video/SimpleModel.cs" "$APP_NAME/Video/SimpleModel.cs"
copy_file "$SRC_DIR/Sound/SoundManager.cs" "$APP_NAME/Sound/SoundManager.cs"

# 5) Copy Content including mgcb and assets
rsync -a --delete "$SRC_DIR/Content/" "$APP_NAME/Content/"

# Ensure Content.mgcb is configured for Android platform (we'll build with /platform:Android)

# 6) Wire MGCB build into Android .csproj if not present
CSPROJ_FILE="$APP_NAME/$APP_NAME.csproj"
if ! grep -q "MonoGame.Content.Builder" "$CSPROJ_FILE" 2>/dev/null; then
  echo "Wiring MGCB content build into $CSPROJ_FILE"
  awk '1; /<Project Sdk=/ && !x { x=1 }' "$CSPROJ_FILE" >/dev/null
  # Append standard MGCB targets
  cat >> "$CSPROJ_FILE" <<'EOF'

  <ItemGroup>
    <MonoGameContentReference Include="Content\Content.mgcb" />
  </ItemGroup>

  <Target Name="BuildContent" BeforeTargets="Build">
    <Message Text="Building MGCB content..." Importance="high" />
    <Exec Command="mgcb -@:&quot;$(MSBuildProjectDirectory)/Content/Content.mgcb&quot; /platform:Android /outputDir:&quot;$(MSBuildProjectDirectory)/Content/bin/Android&quot; /intermediateDir:&quot;$(MSBuildProjectDirectory)/Content/obj/Android&quot;" ContinueOnError="false" />
  </Target>
EOF
fi

# 7) Build the APK
echo "Building $APP_NAME ($CONFIG, $TFM) ..."
dotnet build "$APP_NAME" -c "$CONFIG" -f "$TFM"

# 8) Locate APK or AAB
APK_PATH=$(find "$APP_NAME/bin/$CONFIG" -type f -name "*.apk" | head -n1 || true)
AAB_PATH=$(find "$APP_NAME/bin/$CONFIG" -type f -name "*.aab" | head -n1 || true)

if [ -n "$APK_PATH" ]; then
  echo "APK built: $APK_PATH"
  echo "Install with: adb install -r \"$APK_PATH\""
elif [ -n "$AAB_PATH" ]; then
  echo "App bundle built: $AAB_PATH"
  echo "You can use bundletool to generate an APK set."
else
  echo "Build finished but no APK/AAB found. Check the build output directories under $APP_NAME/bin/$CONFIG."
fi
