@echo off
setlocal
REM Change to the directory of this script
cd /d "%~dp0"

REM Ensure dependencies are installed
flutter pub get

REM Generate code (e.g., retrofit/hive) and resolve conflicts automatically
flutter pub run build_runner build --delete-conflicting-outputs

REM Run the Windows desktop app
flutter run -d windows
endlocal
