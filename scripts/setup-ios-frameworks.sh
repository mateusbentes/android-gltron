#!/bin/bash
# Setup FNA frameworks for iOS
# Downloads or provides guidance for iOS framework setup

set -e

echo "🍎 Setting up FNA frameworks for iOS..."

# Check if we're on macOS
if [[ "$OSTYPE" != "darwin"* ]]; then
    echo "❌ iOS development requires macOS"
    echo "   This script can only run on macOS with Xcode installed"
    exit 1
fi

# Change to project root
cd "$(dirname "$0")/.."

# Create frameworks directory
mkdir -p GltronMobileGame.iOS/Frameworks

echo "📁 Frameworks directory: GltronMobileGame.iOS/Frameworks"

# Check for existing frameworks
SDL2_EXISTS=false
OPENAL_EXISTS=false

if [ -d "GltronMobileGame.iOS/Frameworks/SDL2.framework" ]; then
    SDL2_EXISTS=true
    echo "✅ SDL2.framework already exists"
else
    echo "❌ SDL2.framework missing"
fi

if [ -d "GltronMobileGame.iOS/Frameworks/OpenAL.framework" ]; then
    OPENAL_EXISTS=true
    echo "✅ OpenAL.framework already exists"
else
    echo "❌ OpenAL.framework missing"
fi

# If frameworks are missing, provide download instructions
if [ "$SDL2_EXISTS" = false ] || [ "$OPENAL_EXISTS" = false ]; then
    echo ""
    echo "📋 To get FNA iOS frameworks:"
    echo ""
    
    if [ "$SDL2_EXISTS" = false ]; then
        echo "🔽 SDL2.framework:"
        echo "1. Download SDL2 iOS framework from:"
        echo "   https://github.com/libsdl-org/SDL/releases"
        echo "2. Look for 'SDL2-[version].dmg' in release assets"
        echo "3. Mount the DMG and copy SDL2.framework to:"
        echo "   GltronMobileGame.iOS/Frameworks/SDL2.framework"
        echo ""
    fi
    
    if [ "$OPENAL_EXISTS" = false ]; then
        echo "🔽 OpenAL.framework:"
        echo "1. OpenAL is provided by iOS system frameworks"
        echo "2. Or download OpenAL Soft iOS framework from:"
        echo "   https://github.com/kcat/openal-soft/releases"
        echo "3. Copy to: GltronMobileGame.iOS/Frameworks/OpenAL.framework"
        echo ""
    fi
    
    echo "📝 Alternative: Use system frameworks"
    echo "   iOS provides built-in OpenAL support"
    echo "   SDL2 can be built from source for iOS"
    echo ""
    
    echo "⚠️  Without proper frameworks:"
    echo "   • App may not build or run correctly"
    echo "   • Limited audio/graphics functionality"
    echo "   • App Store submission may fail"
    echo ""
fi

# Check Xcode and iOS SDK
echo "🔧 Checking iOS development environment..."

if command -v xcodebuild >/dev/null 2>&1; then
    XCODE_VERSION=$(xcodebuild -version | head -1)
    echo "✅ $XCODE_VERSION"
    
    # List available iOS SDKs
    IOS_SDKS=$(xcodebuild -showsdks | grep iphoneos | tail -1)
    if [ -n "$IOS_SDKS" ]; then
        echo "✅ $IOS_SDKS"
    else
        echo "⚠️  No iOS SDKs found"
    fi
else
    echo "❌ Xcode not found or not in PATH"
    echo "   Install Xcode from the App Store"
fi

# Check for iOS deployment target compatibility
echo ""
echo "📱 iOS Deployment Target: 12.0+"
echo "   This ensures compatibility with FNA requirements"

# Summary
echo ""
if [ "$SDL2_EXISTS" = true ] && [ "$OPENAL_EXISTS" = true ]; then
    echo "🎉 All FNA iOS frameworks are ready!"
    echo ""
    echo "📝 Next steps:"
    echo "1. Build: dotnet build GltronMobileGame.iOS/GltronMobileGame.iOS.csproj"
    echo "2. Deploy to simulator or device using Xcode"
    echo "3. Test FNA functionality"
else
    echo "⚠️  Some frameworks are missing"
    echo "   The project will build with warnings but may have limited functionality"
    echo ""
    echo "📝 To complete setup:"
    echo "1. Follow the download instructions above"
    echo "2. Re-run this script to verify"
    echo "3. Build and test the project"
fi

echo ""
echo "✅ iOS framework setup completed!"
