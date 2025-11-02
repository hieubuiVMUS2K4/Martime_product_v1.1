@echo off
REM Maritime Crew App - Build APK Script
REM =====================================

echo.
echo ========================================
echo Maritime Crew App - APK Builder
echo ========================================
echo.

cd /d "%~dp0"

echo [1/5] Cleaning previous builds...
call flutter clean
if errorlevel 1 goto error

echo.
echo [2/5] Getting dependencies...
call flutter pub get
if errorlevel 1 goto error

echo.
echo [3/5] Running code generation...
call flutter pub run build_runner build --delete-conflicting-outputs
if errorlevel 1 goto error

echo.
echo [4/5] Building APK (Release)...
echo.
echo Choose build type:
echo   1. Universal APK (larger, works on all devices)
echo   2. Split APKs (smaller, optimized for each CPU)
echo   3. Debug APK (for testing only)
echo.
set /p choice="Enter choice (1-3): "

if "%choice%"=="1" (
    echo Building universal release APK...
    call flutter build apk --release
) else if "%choice%"=="2" (
    echo Building split release APKs...
    call flutter build apk --release --split-per-abi
) else if "%choice%"=="3" (
    echo Building debug APK...
    call flutter build apk --debug
) else (
    echo Invalid choice. Building universal APK...
    call flutter build apk --release
)

if errorlevel 1 goto error

echo.
echo [5/5] Build completed successfully!
echo.
echo ========================================
echo APK files location:
echo ========================================
dir /b "build\app\outputs\flutter-apk\*.apk"
echo.
echo Full path: %cd%\build\app\outputs\flutter-apk\
echo.
echo ========================================
echo Done! You can now install the APK on Android devices.
echo ========================================
echo.

explorer "build\app\outputs\flutter-apk"
goto end

:error
echo.
echo ========================================
echo ERROR: Build failed!
echo ========================================
echo.
echo Please check the error messages above.
echo Common issues:
echo   - Android SDK not installed
echo   - Missing key.properties file (for release builds)
echo   - Network issues during dependency download
echo.
echo Run 'flutter doctor' to check your setup.
echo.
pause
exit /b 1

:end
pause
