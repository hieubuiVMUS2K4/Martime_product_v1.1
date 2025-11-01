@echo off
echo ===================================================
echo   Opening Windows Firewall for Edge Server
echo   Port: 5001
echo ===================================================
echo.

REM Check for Administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges!
    echo.
    echo How to run:
    echo 1. Right-click this file
    echo 2. Select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo OK: Running with Administrator privileges
echo.

echo Creating firewall rules...
echo.

REM Create Inbound rule
netsh advfirewall firewall add rule name="Edge Server Port 5001" dir=in action=allow protocol=TCP localport=5001
if %errorLevel% equ 0 (
    echo OK: Inbound rule created successfully!
) else (
    echo ERROR: Failed to create inbound rule!
    pause
    exit /b 1
)

REM Create Outbound rule
netsh advfirewall firewall add rule name="Edge Server Port 5001 (Outbound)" dir=out action=allow protocol=TCP localport=5001
if %errorLevel% equ 0 (
    echo OK: Outbound rule created successfully!
) else (
    echo ERROR: Failed to create outbound rule!
    pause
    exit /b 1
)

echo.
echo ===================================================
echo   SUCCESS!
echo ===================================================
echo.
echo Your mobile device can now connect to:
echo    http://10.20.67.86:5001
echo.
echo Next steps:
echo    1. Make sure Edge Server is running
echo    2. Make sure phone and PC are on same network
echo    3. Try logging in on the app
echo.
pause
