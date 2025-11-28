Android build instructions (MonoGame)

Overview
This guide explains how to build and package the MonoGame Android version from this repository on Linux using .NET 8 + Android SDK. It uses ready-to-run scripts placed under scripts/.

What you will get
- A MonoGame Android project (GltronAndroid) generated on-demand
- APK (or AAB) built from the existing MonoGame codebase and content
- HUD with basic FPS/score, background music and SFX place-holders

Prerequisites
1) Install Android Studio and Android SDK
   - Install Android Studio and use the SDK Manager to install:
     • Android SDK Platform (API 34 recommended)
     • Android SDK Platform-Tools
     • Android SDK Build-Tools
   - Set ANDROID_HOME to your SDK location, typically:
     export ANDROID_HOME=$HOME/Android/Sdk
     export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"
   - Alternatively, source the helper script:
     source ./scripts/setup-android-env.sh

2) Install .NET 8 SDK
   - Follow https://learn.microsoft.com/dotnet/core/install/linux
     export DOTNET_ROOT=$HOME/dotnet
     export PATH=$PATH:$HOME/dotnet
   - Verify: dotnet --info
   - Install Android workload for .NET (required for net8.0-android):
     dotnet workload install android

3) Install MonoGame templates and MGCB tool
   dotnet new --install MonoGame.Templates.CSharp
   dotnet tool install -g dotnet-mgcb-editor
   # ensure global tools are on PATH
   export PATH="$HOME/.dotnet/tools:$PATH"

Build steps
Option A: One-shot end-to-end script (recommended)
- Make scripts executable (once):
  chmod +x scripts/*.sh
- Run the app builder (creates Android project if missing, copies code/content, wires content build, builds):
  ./scripts/build-android-app.sh -c Release
- On success, the script prints the APK/AAB path and an adb install command.

Option B: Manual steps (advanced)
1) Create a MonoGame Android project:
   dotnet new mgandroid -n GltronAndroid
2) Copy game code and content from GltronMonoGame into GltronAndroid
   - Files: Game1.cs, GLTronGame.cs, Player.cs, Segment.cs, Vec.cs
   - Folders: Video/*, Sound/*, Content/*
3) Ensure Content/Content.mgcb contains font/audio entries (already configured in this repo) and is built for Android
   - Build manually:
     mgcb -@:"GltronAndroid/Content/Content.mgcb" /platform:Android \
          /outputDir:"GltronAndroid/Content/bin/Android" \
          /intermediateDir:"GltronAndroid/Content/obj/Android"
4) Build APK:
   dotnet build GltronAndroid -c Release -f net8.0-android

Deploying to device/emulator
- Connect a device with USB debugging enabled or start an emulator.
- Install APK (example):
  adb install -r "GltronAndroid/bin/Release/com.companyname.gltronandroid-Signed.apk"
  # Path may differ; use the path printed by the build script.

Troubleshooting
- dotnet: command not found
  • Install .NET 8 SDK and ensure dotnet is on PATH.
- mgcb or mgcb-editor not found
  • Install MGCB editor tool and add ~/.dotnet/tools to PATH.
- ANDROID_HOME not set / sdkmanager/adb not found
  • Set ANDROID_HOME to your SDK path or source scripts/setup-android-env.sh.
- No APK/AAB after build
  • Check build output in GltronAndroid/bin/<Config>/, review dotnet build logs for errors.
- Content not found at runtime
  • Ensure Content.mgcb built for Android and content files exist under GltronAndroid/Content/bin/Android.

Notes
- This repo also contains the original Java Android project under GlTron/. The new MonoGame Android port uses the C# code under GltronMonoGame/.
- Don’t delete the Java project until the MonoGame Android port is fully validated.
- Consider initializing version control (git) if you plan to iterate.

Support
If you want, ask me to run the one-shot builder once your Android SDK and .NET are installed, and I’ll verify the APK generation for you.
