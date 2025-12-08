#!/usr/bin/env bash
# GLTron Mobile FNA - Production Build Script
# Creates optimized, signed APKs and AABs for Google Play Store release
# Usage: ./scripts/build-production-fna.sh [-t apk|aab|both] [-k keystore_path] [-a alias]

set -euo pipefail

# Configuration
CONFIG="Release"
PROJ_DIR="GltronMobileGame"
TFM="net9.0-android36.0"
BUILD_TYPE="both"  # apk, aab, or both
KEYSTORE_PATH=""
KEY_ALIAS=""
STORE_PASS=""
KEY_PASS=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parse command line arguments
while getopts ":t:k:a:h" opt; do
  case $opt in
    t) BUILD_TYPE="$OPTARG" ;;
    k) KEYSTORE_PATH="$OPTARG" ;;
    a) KEY_ALIAS="$OPTARG" ;;
    h) 
      echo "Usage: $0 [-t apk|aab|both] [-k keystore_path] [-a alias]"
      echo "  -t: Build type (apk, aab, or both) - default: both"
      echo "  -k: Path to keystore file"
      echo "  -a: Key alias in keystore"
      echo "  -h: Show this help"
      exit 0
      ;;
    *) echo "Unknown option -$OPTARG. Use -h for help." ; exit 1 ;;
  esac
done

echo -e "${BLUE}🚀 GLTron Mobile FNA - Production Build${NC}"
echo "=========================================="
echo ""

# Validate build type
if [[ "$BUILD_TYPE" != "apk" && "$BUILD_TYPE" != "aab" && "$BUILD_TYPE" != "both" ]]; then
  echo -e "${RED}❌ ERROR: Invalid build type '$BUILD_TYPE'. Use 'apk', 'aab', or 'both'${NC}"
  exit 1
fi

# Check if project directory exists
if [ ! -d "$PROJ_DIR" ]; then
  echo -e "${RED}❌ ERROR: Project directory '$PROJ_DIR' not found${NC}"
  exit 1
fi

# Check for keystore if signing is requested
if [ -n "$KEYSTORE_PATH" ]; then
  if [ ! -f "$KEYSTORE_PATH" ]; then
    echo -e "${RED}❌ ERROR: Keystore file not found: $KEYSTORE_PATH${NC}"
    exit 1
  fi
  
  if [ -z "$KEY_ALIAS" ]; then
    echo -e "${RED}❌ ERROR: Key alias required when using keystore${NC}"
    exit 1
  fi
  
  # Prompt for passwords
  echo -e "${YELLOW}🔐 Keystore Authentication${NC}"
  read -s -p "Enter keystore password: " STORE_PASS
  echo ""
  read -s -p "Enter key password (or press Enter if same as keystore): " KEY_PASS
  echo ""
  
  if [ -z "$KEY_PASS" ]; then
    KEY_PASS="$STORE_PASS"
  fi
fi

echo -e "${CYAN}📋 Build Configuration:${NC}"
echo "  Project: $PROJ_DIR"
echo "  Configuration: $CONFIG"
echo "  Target Framework: $TFM"
echo "  Build Type: $BUILD_TYPE"
echo "  Keystore: ${KEYSTORE_PATH:-"None (unsigned)"}"
echo "  Key Alias: ${KEY_ALIAS:-"N/A"}"
echo ""

# Clean previous builds
echo -e "${YELLOW}🧹 Cleaning previous builds...${NC}"
dotnet clean "$PROJ_DIR" -c "$CONFIG" -f "$TFM" --verbosity quiet

# Ensure FNA dependencies are available
echo -e "${YELLOW}🔧 Checking FNA dependencies...${NC}"
if [ ! -f "GltronMobileGame/FNA/lib/SDL2-CS/src/SDL2.cs" ]; then
  echo "FNA dependencies missing, setting up..."
  ./scripts/setup-fna-deps.sh
fi

# Restore packages
echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
dotnet restore GltronMobile.sln --verbosity quiet

# Build the solution first
echo -e "${YELLOW}🔨 Building FNA solution...${NC}"
dotnet build GltronMobile.sln -c "$CONFIG" --no-restore --verbosity quiet

# Function to build APK
build_apk() {
  echo -e "${BLUE}📱 Building Production APK...${NC}"
  
  local publish_args=(
    "$PROJ_DIR"
    -c "$CONFIG"
    -f "$TFM"
    --no-restore
    --no-build
    -p:AndroidPackageFormat=apk
    -p:AndroidUseAapt2=true
    -p:AndroidCreatePackagePerAbi=false
    -p:EmbedAssembliesIntoApk=true
    -p:AndroidLinkMode=SdkOnly
    -p:AndroidEnableProguard=true
    -p:AndroidUseSharedRuntime=false
    -p:AndroidStripILAfterAOT=true
    -p:PublishTrimmed=true
    -p:TrimMode=partial
  )
  
  # Add signing parameters if keystore is provided
  if [ -n "$KEYSTORE_PATH" ]; then
    publish_args+=(
      -p:AndroidSigningKeyStore="$KEYSTORE_PATH"
      -p:AndroidSigningKeyAlias="$KEY_ALIAS"
      -p:AndroidSigningKeyPass="$KEY_PASS"
      -p:AndroidSigningStorePass="$STORE_PASS"
    )
  fi
  
  dotnet publish "${publish_args[@]}"
  
  # Find and report APK
  local apk_path=$(find "$PROJ_DIR/bin/$CONFIG/$TFM" -name "*.apk" -type f | head -n1)
  if [ -n "$apk_path" ]; then
    local apk_size=$(du -h "$apk_path" | cut -f1)
    echo -e "${GREEN}✅ APK created: $apk_path ($apk_size)${NC}"
    return 0
  else
    echo -e "${RED}❌ ERROR: APK not found after build${NC}"
    return 1
  fi
}

# Function to build AAB
build_aab() {
  echo -e "${BLUE}📦 Building Production AAB (Android App Bundle)...${NC}"
  
  local publish_args=(
    "$PROJ_DIR"
    -c "$CONFIG"
    -f "$TFM"
    --no-restore
    --no-build
    -p:AndroidPackageFormat=aab
    -p:AndroidUseAapt2=true
    -p:AndroidCreatePackagePerAbi=false
    -p:EmbedAssembliesIntoApk=true
    -p:AndroidLinkMode=SdkOnly
    -p:AndroidEnableProguard=true
    -p:AndroidUseSharedRuntime=false
    -p:AndroidStripILAfterAOT=true
    -p:PublishTrimmed=true
    -p:TrimMode=partial
  )
  
  # Add signing parameters if keystore is provided
  if [ -n "$KEYSTORE_PATH" ]; then
    publish_args+=(
      -p:AndroidSigningKeyStore="$KEYSTORE_PATH"
      -p:AndroidSigningKeyAlias="$KEY_ALIAS"
      -p:AndroidSigningKeyPass="$KEY_PASS"
      -p:AndroidSigningStorePass="$STORE_PASS"
    )
  fi
  
  dotnet publish "${publish_args[@]}"
  
  # Find and report AAB
  local aab_path=$(find "$PROJ_DIR/bin/$CONFIG/$TFM" -name "*.aab" -type f | head -n1)
  if [ -n "$aab_path" ]; then
    local aab_size=$(du -h "$aab_path" | cut -f1)
    echo -e "${GREEN}✅ AAB created: $aab_path ($aab_size)${NC}"
    return 0
  else
    echo -e "${RED}❌ ERROR: AAB not found after build${NC}"
    return 1
  fi
}

# Execute builds based on type
case "$BUILD_TYPE" in
  "apk")
    build_apk
    ;;
  "aab")
    build_aab
    ;;
  "both")
    build_apk
    echo ""
    build_aab
    ;;
esac

echo ""
echo -e "${GREEN}🎉 FNA Production Build Completed!${NC}"
echo ""

# Show build artifacts
echo -e "${CYAN}📁 Build Artifacts:${NC}"
find "$PROJ_DIR/bin/$CONFIG/$TFM" -name "*.apk" -o -name "*.aab" | while read -r file; do
  local size=$(du -h "$file" | cut -f1)
  local type=$(echo "$file" | grep -o '\.(apk\|aab)$' | tr '[:lower:]' '[:upper:]')
  echo "  $type: $file ($size)"
done

echo ""
echo -e "${BLUE}📋 Production Build Features:${NC}"
echo "  ✅ FNA Framework (XNA4 compatible)"
echo "  ✅ Native SDL2/OpenAL integration"
echo "  ✅ Raw content loading (no XNB files)"
echo "  ✅ Optimized release build"
echo "  ✅ ProGuard code shrinking"
echo "  ✅ IL trimming for smaller size"
echo "  ✅ Single APK/AAB (all architectures)"
if [ -n "$KEYSTORE_PATH" ]; then
  echo "  ✅ Digitally signed for Play Store"
else
  echo "  ⚠️  Unsigned (development only)"
fi

echo ""
if [ -n "$KEYSTORE_PATH" ]; then
  echo -e "${GREEN}🚀 Ready for Google Play Store upload!${NC}"
else
  echo -e "${YELLOW}⚠️  To create signed builds for Play Store:${NC}"
  echo "   ./scripts/create-keystore.sh  # Create keystore (one time)"
  echo "   $0 -k gltron-release.keystore -a gltron-release-key"
fi

echo ""
echo -e "${CYAN}🔗 Next Steps:${NC}"
echo "1. Test the APK on various Android devices"
echo "2. Upload AAB to Google Play Console"
echo "3. Configure Play Store listing and screenshots"
echo "4. Submit for review and publication"
