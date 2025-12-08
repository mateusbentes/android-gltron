#!/bin/bash
# Verify FNA native libraries are correct architecture and compatible

set -e

echo "🔍 Verifying FNA native libraries..."

# Change to project root
cd "$(dirname "$0")/.."

# Function to check ELF architecture
check_elf_arch() {
    local file="$1"
    local expected_arch="$2"
    
    if [ ! -f "$file" ]; then
        echo "❌ File not found: $file"
        return 1
    fi
    
    # Use file command to check architecture
    local file_info=$(file "$file")
    local size=$(stat -c%s "$file")
    
    echo "📁 Checking: $file ($size bytes)"
    echo "   File info: $file_info"
    
    # Check if it's an ELF file
    if [[ "$file_info" != *"ELF"* ]]; then
        echo "   ❌ Not an ELF file"
        return 1
    fi
    
    # Check architecture
    case "$expected_arch" in
        "arm64-v8a")
            if [[ "$file_info" == *"aarch64"* ]] || [[ "$file_info" == *"ARM aarch64"* ]]; then
                echo "   ✅ Correct ARM64 architecture"
                return 0
            else
                echo "   ❌ Wrong architecture for ARM64"
                return 1
            fi
            ;;
        "armeabi-v7a")
            if [[ "$file_info" == *"ARM"* ]] && [[ "$file_info" != *"aarch64"* ]]; then
                echo "   ✅ Correct ARMv7 architecture"
                return 0
            else
                echo "   ❌ Wrong architecture for ARMv7"
                return 1
            fi
            ;;
        "x86_64")
            if [[ "$file_info" == *"x86-64"* ]] || [[ "$file_info" == *"x86_64"* ]]; then
                echo "   ✅ Correct x86_64 architecture"
                return 0
            else
                echo "   ❌ Wrong architecture for x86_64"
                return 1
            fi
            ;;
        *)
            echo "   ❌ Unknown expected architecture: $expected_arch"
            return 1
            ;;
    esac
}

# Function to check library symbols
check_library_symbols() {
    local file="$1"
    local lib_type="$2"
    
    if [ ! -f "$file" ]; then
        return 1
    fi
    
    # Use nm or objdump to check symbols (if available)
    if command -v nm >/dev/null 2>&1; then
        echo "   🔍 Checking symbols..."
        case "$lib_type" in
            "SDL2")
                if nm -D "$file" 2>/dev/null | grep -q "SDL_Init"; then
                    echo "   ✅ SDL2 symbols found"
                else
                    echo "   ⚠️  SDL2 symbols not found (may be stripped)"
                fi
                ;;
            "OpenAL")
                if nm -D "$file" 2>/dev/null | grep -q "alGetError"; then
                    echo "   ✅ OpenAL symbols found"
                else
                    echo "   ⚠️  OpenAL symbols not found (may be stripped)"
                fi
                ;;
        esac
    fi
}

# Check all architectures
OVERALL_SUCCESS=true

for abi in arm64-v8a armeabi-v7a x86_64; do
    echo ""
    echo "🏗️  Checking $abi libraries..."
    
    SDL2_LIB="GltronMobileGame/lib/$abi/libSDL2.so"
    OPENAL_LIB="GltronMobileGame/lib/$abi/libopenal.so"
    
    # Check SDL2
    if check_elf_arch "$SDL2_LIB" "$abi"; then
        check_library_symbols "$SDL2_LIB" "SDL2"
    else
        OVERALL_SUCCESS=false
    fi
    
    # Check OpenAL
    if check_elf_arch "$OPENAL_LIB" "$abi"; then
        check_library_symbols "$OPENAL_LIB" "OpenAL"
    else
        OVERALL_SUCCESS=false
    fi
done

echo ""
echo "📋 Library verification summary:"

# Check Android project configuration
echo ""
echo "🔧 Checking Android project configuration..."

CSPROJ_FILE="GltronMobileGame/GltronAndroid.csproj"
if [ -f "$CSPROJ_FILE" ]; then
    if grep -q "AndroidNativeLibrary.*libSDL2.so" "$CSPROJ_FILE"; then
        echo "✅ SDL2 libraries configured in Android project"
    else
        echo "⚠️  SDL2 libraries not found in Android project configuration"
    fi
    
    if grep -q "AndroidNativeLibrary.*libopenal.so" "$CSPROJ_FILE"; then
        echo "✅ OpenAL libraries configured in Android project"
    else
        echo "⚠️  OpenAL libraries not found in Android project configuration"
    fi
else
    echo "❌ Android project file not found: $CSPROJ_FILE"
    OVERALL_SUCCESS=false
fi

# Check Android manifest
MANIFEST_FILE="GltronMobileGame/AndroidManifest.xml"
if [ -f "$MANIFEST_FILE" ]; then
    if grep -q "glEsVersion" "$MANIFEST_FILE"; then
        echo "✅ OpenGL ES version specified in manifest"
    else
        echo "⚠️  OpenGL ES version not specified in manifest"
    fi
else
    echo "⚠️  Android manifest not found: $MANIFEST_FILE"
fi

echo ""
if [ "$OVERALL_SUCCESS" = true ]; then
    echo "🎉 All native libraries verified successfully!"
    echo "✅ FNA should initialize without architecture mismatch errors"
    echo ""
    echo "📝 Ready to build:"
    echo "   dotnet clean GltronMobileGame/GltronAndroid.csproj"
    echo "   dotnet publish GltronMobileGame/GltronAndroid.csproj -c Release"
else
    echo "❌ Some libraries have issues"
    echo "🔧 Run ./scripts/download-real-fna-libs.sh to fix library issues"
    exit 1
fi
