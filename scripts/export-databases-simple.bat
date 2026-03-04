@echo off
echo.
echo ===============================================
echo Maritime Product - Database Export
echo ===============================================
echo.

set TIMESTAMP=%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

if not exist "team-database-backups" mkdir team-database-backups

echo [1/2] Exporting Edge Database (maritime_edge)...
docker exec e83c9374df19 pg_dump -U edge_user -d maritime_edge --clean --if-exists --no-owner --no-privileges > team-database-backups\edge_%TIMESTAMP%.sql
if %ERRORLEVEL% EQU 0 (
    echo SUCCESS - Edge database exported
    dir team-database-backups\edge_%TIMESTAMP%.sql | findstr /C:"edge_"
) else (
    echo FAILED - Could not export edge database
)

echo.
echo [2/2] Exporting Shore Database (productdb)...
echo Trying Docker container...
docker exec e83c9374df19 pg_dump -U product -d productdb --clean --if-exists --no-owner --no-privileges > team-database-backups\shore_%TIMESTAMP%.sql 2>nul
if %ERRORLEVEL% EQU 0 (
    echo SUCCESS - Shore database exported from Docker
    dir team-database-backups\shore_%TIMESTAMP%.sql | findstr /C:"shore_"
) else (
    echo Shore database not in same container
    echo.
    echo NOTE: If you have PostgreSQL installed locally on port 5432,
    echo you can manually export using:
    echo   pg_dump -h localhost -p 5432 -U product -d productdb -f team-database-backups\shore_%TIMESTAMP%.sql
    echo.
    echo Or use pgAdmin to export the database.
)

echo.
echo ===============================================
echo Export completed!
echo.
echo Files are in: team-database-backups\
echo.
pause
