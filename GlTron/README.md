# Legacy Android Java Project

This folder contains the original Android Java/OpenGL ES implementation of GLTron. It is kept for reference and parity checks while the MonoGame Android port (C#) is developed.

## Status
- Not used by the new Android build scripts.
- Active Android build path is the MonoGame port.

## Where to go now (MonoGame Android)
- Quick start and top-level notes: README_ANDROID_PORT.md (repo root)
- Detailed Android instructions: GltronMonoGame/README_ANDROID.md
- Build scripts:
  - scripts/build-android-app.sh (one-shot scaffold + build)
  - scripts/setup-android-env.sh (optional SDK env helper)

## When to use this project
- Review original gameplay logic, rendering, HUD, and audio behavior implemented in Java/OpenGL ES.
- Build a legacy APK with Android Studio for comparison or debugging.

## When not to use this project
- For the new APK build workflow. Use the MonoGame-based scripts and project instead.

## Note
Keep this folder until the MonoGame port reaches feature parity and is fully validated on Android. After that, consider archiving or removing it to reduce repository size.
