#!/bin/bash
# Clean FNA build artifacts and force rebuild

echo "🧹 Cleaning FNA build artifacts..."

# Change to project root
cd "$(dirname "$0")/.."

# Clean all bin and obj directories
echo "Cleaning bin/obj directories..."
find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Clean FNA specific directories
echo "Cleaning FNA build cache..."
rm -rf GltronMobileGame/FNA/bin 2>/dev/null || true
rm -rf GltronMobileGame/FNA/obj 2>/dev/null || true
rm -rf GltronMobileGame/FNA/obj_netstandard 2>/dev/null || true

# Clean NuGet cache
echo "Cleaning NuGet cache..."
dotnet nuget locals all --clear

# Clean Android build cache
echo "Cleaning Android build cache..."
rm -rf GltronMobileGame/obj 2>/dev/null || true
rm -rf GltronMobileEngine/obj 2>/dev/null || true

echo "✅ Clean completed!"
echo ""
echo "Next steps:"
echo "1. Run: dotnet restore GltronMobileGame/FNA/FNA.NetStandard.csproj"
echo "2. Run: dotnet restore GltronMobileEngine/GltronMobileEngine.csproj"
echo "3. Run: dotnet restore GltronMobileGame/GltronAndroid.csproj"
echo "4. Run: dotnet build GltronMobileGame/GltronAndroid.csproj -c Release"
