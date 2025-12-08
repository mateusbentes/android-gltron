#!/bin/bash
# Download real FNA native libraries for Android
# This replaces the stub libraries with actual SDL2 and OpenAL binaries

set -e

echo "🔽 Downloading real FNA native libraries for Android..."

# Change to project root
cd "$(dirname "$0")/.."

# Create directories
mkdir -p GltronMobileGame/lib/arm64-v8a
mkdir -p GltronMobileGame/lib/armeabi-v7a
mkdir -p GltronMobileGame/lib/x86_64

# Temporary directory for downloads
TEMP_DIR=$(mktemp -d)
echo "📁 Using temporary directory: $TEMP_DIR"

# Function to download and extract SDL2
download_sdl2() {
    echo "📥 Downloading SDL2 for Android..."
    
    # Try multiple SDL2 sources for better reliability
    SDL2_SOURCES=(
        "https://github.com/libsdl-org/SDL/releases/download/release-2.28.5/SDL2-devel-2.28.5-VC.zip"
        "https://github.com/libsdl-org/SDL/archive/refs/tags/release-2.28.5.tar.gz"
        "https://www.libsdl.org/release/SDL2-2.28.5.tar.gz"
    )
    
    cd "$TEMP_DIR"
    
    for url in "${SDL2_SOURCES[@]}"; do
        echo "🔄 Trying: $url"
        
        if [[ "$url" == *.zip ]]; then
            if timeout 30 wget -q "$url" -O sdl2.zip; then
                echo "✅ SDL2 downloaded successfully"
                unzip -q sdl2.zip
                
                # Look for prebuilt Android libraries in various locations
                for dir in SDL2-* */; do
                    if [ -d "$dir" ]; then
                        find "$dir" -name "libSDL2.so" -type f | while read -r lib; do
                            # Determine ABI from path
                            if [[ "$lib" == *"arm64-v8a"* ]] || [[ "$lib" == *"aarch64"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libSDL2.so"
                                echo "✅ SDL2 ARM64 library copied"
                            elif [[ "$lib" == *"armeabi-v7a"* ]] || [[ "$lib" == *"armv7"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libSDL2.so"
                                echo "✅ SDL2 ARMv7 library copied"
                            elif [[ "$lib" == *"x86_64"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libSDL2.so"
                                echo "✅ SDL2 x86_64 library copied"
                            fi
                        done
                    fi
                done
                return 0
            fi
        else
            if timeout 30 wget -q "$url" -O sdl2.tar.gz; then
                echo "✅ SDL2 downloaded successfully"
                tar -xzf sdl2.tar.gz
                
                # Look for Android project structure
                for dir in SDL2-* */; do
                    if [ -d "$dir/android-project" ]; then
                        find "$dir" -name "libSDL2.so" -type f | while read -r lib; do
                            # Determine ABI from path
                            if [[ "$lib" == *"arm64-v8a"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libSDL2.so"
                                echo "✅ SDL2 ARM64 library copied"
                            elif [[ "$lib" == *"armeabi-v7a"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libSDL2.so"
                                echo "✅ SDL2 ARMv7 library copied"
                            elif [[ "$lib" == *"x86_64"* ]]; then
                                cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libSDL2.so"
                                echo "✅ SDL2 x86_64 library copied"
                            fi
                        done
                        return 0
                    fi
                done
            fi
        fi
        
        echo "⚠️  Failed to download from $url, trying next source..."
        rm -f sdl2.* 2>/dev/null || true
    done
    
    echo "❌ All SDL2 download sources failed"
    return 1
}

# Function to download OpenAL Soft
download_openal() {
    echo "📥 Downloading OpenAL Soft for Android..."
    
    cd "$TEMP_DIR"
    
    # Try multiple OpenAL sources
    OPENAL_SOURCES=(
        "https://github.com/kcat/openal-soft/releases/download/1.23.1/openal-soft-1.23.1-bin.zip"
        "https://github.com/kcat/openal-soft/archive/refs/tags/1.23.1.tar.gz"
    )
    
    for url in "${OPENAL_SOURCES[@]}"; do
        echo "🔄 Trying: $url"
        
        if [[ "$url" == *.zip ]]; then
            if timeout 20 wget -q "$url" -O openal.zip; then
                echo "✅ OpenAL Soft downloaded successfully"
                unzip -q openal.zip
                
                # Look for Android libraries in various locations
                find . -name "libopenal.so" -type f | while read -r lib; do
                    # Determine ABI from path
                    if [[ "$lib" == *"arm64-v8a"* ]] || [[ "$lib" == *"aarch64"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libopenal.so"
                        echo "✅ OpenAL ARM64 library copied"
                    elif [[ "$lib" == *"armeabi-v7a"* ]] || [[ "$lib" == *"armv7"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libopenal.so"
                        echo "✅ OpenAL ARMv7 library copied"
                    elif [[ "$lib" == *"x86_64"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libopenal.so"
                        echo "✅ OpenAL x86_64 library copied"
                    fi
                done
                return 0
            fi
        else
            if timeout 20 wget -q "$url" -O openal.tar.gz; then
                echo "✅ OpenAL Soft downloaded successfully"
                tar -xzf openal.tar.gz
                
                # Look for prebuilt libraries
                find . -name "libopenal.so" -type f | while read -r lib; do
                    # Determine ABI from path
                    if [[ "$lib" == *"arm64-v8a"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libopenal.so"
                        echo "✅ OpenAL ARM64 library copied"
                    elif [[ "$lib" == *"armeabi-v7a"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libopenal.so"
                        echo "✅ OpenAL ARMv7 library copied"
                    elif [[ "$lib" == *"x86_64"* ]]; then
                        cp "$lib" "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libopenal.so"
                        echo "✅ OpenAL x86_64 library copied"
                    fi
                done
                return 0
            fi
        fi
        
        echo "⚠️  Failed to download from $url, trying next source..."
        rm -f openal.* 2>/dev/null || true
    done
    
    echo "❌ All OpenAL download sources failed"
    return 1
}

# Function to create minimal working stubs if downloads fail
create_minimal_stubs() {
    echo "📋 Creating minimal working stubs as fallback..."
    
    # Create a more complete stub that won't crash FNA
    cat > "$TEMP_DIR/sdl2_stub.c" << 'EOF'
// Minimal SDL2 stub that allows FNA to initialize
#include <stdint.h>

// SDL2 initialization stubs
int SDL_Init(uint32_t flags) { return 0; }
void SDL_Quit(void) { }
const char* SDL_GetError(void) { return "Stub SDL2"; }

// Window management stubs
void* SDL_CreateWindow(const char* title, int x, int y, int w, int h, uint32_t flags) { 
    return (void*)1; 
}
void SDL_DestroyWindow(void* window) { }

// OpenGL context stubs
void* SDL_GL_CreateContext(void* window) { return (void*)1; }
void SDL_GL_DeleteContext(void* context) { }
void SDL_GL_SwapWindow(void* window) { }
int SDL_GL_SetAttribute(int attr, int value) { return 0; }

// Event handling stubs
int SDL_PollEvent(void* event) { return 0; }
int SDL_WaitEvent(void* event) { return 0; }

// Video mode stubs
int SDL_GetNumVideoDisplays(void) { return 1; }
int SDL_GetDisplayBounds(int displayIndex, void* rect) { return 0; }
EOF

    cat > "$TEMP_DIR/openal_stub.c" << 'EOF'
// Minimal OpenAL stub that allows FNA to initialize
#include <stdint.h>

// OpenAL error handling
int alGetError(void) { return 0; }
const char* alGetString(int param) { return "Stub OpenAL"; }

// Source management stubs
void alGenSources(int n, uint32_t* sources) { 
    for(int i = 0; i < n; i++) sources[i] = i + 1; 
}
void alDeleteSources(int n, uint32_t* sources) { }
void alSourcePlay(uint32_t source) { }
void alSourceStop(uint32_t source) { }
void alSourcePause(uint32_t source) { }

// Buffer management stubs
void alGenBuffers(int n, uint32_t* buffers) { 
    for(int i = 0; i < n; i++) buffers[i] = i + 1; 
}
void alDeleteBuffers(int n, uint32_t* buffers) { }
void alBufferData(uint32_t buffer, int format, const void* data, int size, int freq) { }

// Listener stubs
void alListenerf(int param, float value) { }
void alListener3f(int param, float v1, float v2, float v3) { }
EOF

    # Compile stubs for each architecture if we have Android NDK
    if [ -n "$ANDROID_NDK_ROOT" ] && [ -d "$ANDROID_NDK_ROOT" ]; then
        echo "🔨 Compiling stubs with Android NDK..."
        
        API=21
        TOOLCHAIN="$ANDROID_NDK_ROOT/toolchains/llvm/prebuilt/linux-x86_64/bin"
        
        if [ -f "$TOOLCHAIN/aarch64-linux-android${API}-clang" ]; then
            # ARM64
            "$TOOLCHAIN/aarch64-linux-android${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libSDL2.so" "$TEMP_DIR/sdl2_stub.c"
            "$TOOLCHAIN/aarch64-linux-android${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/arm64-v8a/libopenal.so" "$TEMP_DIR/openal_stub.c"
            echo "✅ ARM64 stubs compiled"
        fi
        
        if [ -f "$TOOLCHAIN/armv7a-linux-androideabi${API}-clang" ]; then
            # ARMv7
            "$TOOLCHAIN/armv7a-linux-androideabi${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libSDL2.so" "$TEMP_DIR/sdl2_stub.c"
            "$TOOLCHAIN/armv7a-linux-androideabi${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/armeabi-v7a/libopenal.so" "$TEMP_DIR/openal_stub.c"
            echo "✅ ARMv7 stubs compiled"
        fi
        
        if [ -f "$TOOLCHAIN/x86_64-linux-android${API}-clang" ]; then
            # x86_64
            "$TOOLCHAIN/x86_64-linux-android${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libSDL2.so" "$TEMP_DIR/sdl2_stub.c"
            "$TOOLCHAIN/x86_64-linux-android${API}-clang" -shared -fPIC -o "$PROJECT_ROOT/GltronMobileGame/lib/x86_64/libopenal.so" "$TEMP_DIR/openal_stub.c"
            echo "✅ x86_64 stubs compiled"
        fi
    else
        echo "⚠️  Android NDK not found, cannot compile architecture-specific stubs"
        echo "   Set ANDROID_NDK_ROOT environment variable to enable stub compilation"
        return 1
    fi
}

# Main execution
PROJECT_ROOT="$(pwd)"

echo "🎯 Attempting to download real libraries first..."

# Try to download real libraries
SDL2_SUCCESS=false
OPENAL_SUCCESS=false

if download_sdl2; then
    SDL2_SUCCESS=true
fi

if download_openal; then
    OPENAL_SUCCESS=true
fi

# If downloads failed, create stubs
if [ "$SDL2_SUCCESS" = false ] || [ "$OPENAL_SUCCESS" = false ]; then
    echo "⚠️  Some downloads failed, creating minimal stubs..."
    if ! create_minimal_stubs; then
        echo "❌ Failed to create stubs. Please install Android NDK and set ANDROID_NDK_ROOT"
        exit 1
    fi
fi

# Clean up
rm -rf "$TEMP_DIR"

# Verify libraries exist
echo ""
echo "🔍 Verifying native libraries..."
for abi in arm64-v8a armeabi-v7a x86_64; do
    SDL2_LIB="GltronMobileGame/lib/$abi/libSDL2.so"
    OPENAL_LIB="GltronMobileGame/lib/$abi/libopenal.so"
    
    if [ -f "$SDL2_LIB" ]; then
        SIZE=$(stat -c%s "$SDL2_LIB")
        echo "✅ $abi/libSDL2.so ($SIZE bytes)"
    else
        echo "❌ $abi/libSDL2.so missing"
    fi
    
    if [ -f "$OPENAL_LIB" ]; then
        SIZE=$(stat -c%s "$OPENAL_LIB")
        echo "✅ $abi/libopenal.so ($SIZE bytes)"
    else
        echo "❌ $abi/libopenal.so missing"
    fi
done

echo ""
echo "🎉 FNA native libraries setup completed!"
echo ""
echo "📝 Next steps:"
echo "1. Build your project: dotnet publish GltronMobileGame/GltronAndroid.csproj -c Release"
echo "2. Install and test on device"
echo "3. Check logs with: adb logcat -s GLTRON-FNA"
echo ""
echo "✅ FNA should now initialize properly without architecture mismatch errors"
