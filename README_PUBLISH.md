# GLTron Mobile - Signing Scripts

This folder contains scripts for creating keystores and signing Android App Bundles (AAB) for Google Play Store distribution.

## 🔐 Scripts Overview

### 1. `create-keystore.sh`
Creates a new Android keystore for app signing.

**Usage:**
```bash
./scripts/create-keystore.sh
```

**What it does:**
- Creates `gltron-release.keystore` in project root
- Sets up proper key alias and security settings
- Adds keystore to .gitignore for security
- Provides security reminders and best practices

### 2. `sign-aab.sh`
Builds and signs an Android App Bundle (AAB) for Google Play Store.

**Usage:**
```bash
./scripts/sign-aab.sh
```

**What it does:**
- Builds the app in Release configuration
- Signs the AAB with your keystore
- Creates optimized bundle for Google Play Store
- Verifies the signed AAB
- Provides upload instructions

## 🚀 Quick Start

### First Time Setup:
```bash
# 1. Create your keystore
./scripts/create-keystore.sh

# 2. Build and sign your AAB
./scripts/sign-aab.sh
```

### Regular Builds:
```bash
# Just run the signing script
./scripts/sign-aab.sh
```

## 📋 Requirements

- **Java Development Kit (JDK)** - For keytool
- **.NET 8 SDK** - For building the app
- **Android SDK** - For Android development

### Install Requirements:

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install openjdk-11-jdk
```

**macOS:**
```bash
brew install openjdk@11
```

**Windows:**
- Install JDK from Oracle or OpenJDK
- Install .NET 8 SDK from Microsoft

## 🔒 Security Best Practices

1. **Keep your keystore safe:**
   - Back it up to multiple secure locations
   - Never commit it to version control
   - Store passwords in a secure password manager

2. **Keystore passwords:**
   - Use strong, unique passwords
   - Remember them - they cannot be recovered
   - Consider using the same password for keystore and key

3. **File permissions:**
   - Keep keystore file permissions restrictive
   - Only you should be able to read it

## 📱 Google Play Store Upload

After running `sign-aab.sh`:

1. **Go to Google Play Console:** https://play.google.com/console
2. **Upload the AAB file** (location shown in script output)
3. **Fill in store listing details**
4. **Submit for review**

## 🔧 Troubleshooting

### "keytool not found"
- Install Java Development Kit (JDK)
- Make sure `keytool` is in your PATH

### "dotnet not found"
- Install .NET 8 SDK
- Restart your terminal after installation

### "Build failed"
- Check Android SDK is installed
- Run `dotnet clean` and try again
- Verify keystore password is correct

### "Keystore not found"
- Run `./scripts/create-keystore.sh` first
- Make sure you're in the project root directory

## 📁 Output Locations

- **Keystore:** `gltron-release.keystore` (project root)
- **Signed AAB:** `GltronMobileGame/bin/Release/net8.0-android/publish/*.aab`

## 🆘 Need Help?

1. Check the script output for specific error messages
2. Verify all requirements are installed
3. Make sure you're running from the project root directory
4. Check file permissions on keystore

## 📝 Notes

- **AAB vs APK:** AAB (Android App Bundle) is preferred for Google Play Store
- **Keystore lifetime:** Keep your keystore forever - you need it for app updates
- **Security:** Never share your keystore or passwords
- **Backup:** Always backup your keystore securely
