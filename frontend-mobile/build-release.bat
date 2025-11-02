@echo off
REM Quick Build - Release APK (Requires signing)
echo Building Release APK...
cd /d "%~dp0"

if not exist "android\key.properties" (
    echo.
    echo ERROR: key.properties not found!
    echo.
    echo You need to create a signing key first:
    echo 1. Copy android\key.properties.example to android\key.properties
    echo 2. Run: keytool -genkey -v -keystore android\app\maritime-release-key.jks -keyalg RSA -keysize 2048 -validity 10000 -alias maritime-key
    echo 3. Update key.properties with your passwords
    echo.
    pause
    exit /b 1
)

flutter clean
flutter pub get
flutter pub run build_runner build --delete-conflicting-outputs
flutter build apk --release --split-per-abi

echo.
echo Release APKs ready at: build\app\outputs\flutter-apk\
echo.
dir /b "build\app\outputs\flutter-apk\*-release.apk"
echo.
explorer "build\app\outputs\flutter-apk"
pause
