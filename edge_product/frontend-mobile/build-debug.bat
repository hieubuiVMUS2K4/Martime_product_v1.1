@echo off
REM Quick Build - Debug APK (No signing required)
echo Building Debug APK...
cd /d "%~dp0"
flutter clean
flutter pub get
flutter build apk --debug
echo.
echo Debug APK ready at: build\app\outputs\flutter-apk\app-debug.apk
explorer "build\app\outputs\flutter-apk"
pause
