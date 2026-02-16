# ===================================================================
# Script Export Toàn Bộ Database
# ===================================================================
# Script này sẽ export toàn bộ dữ liệu từ cả 2 databases:
# 1. Shore Database (productdb) - Port 5432
# 2. Edge Database (maritime_edge) - Port 5433
#
# Tạo file backup với timestamp để dễ quản lý
# ===================================================================

param(
    [string]$BackupFolder = "database-backups",
    [switch]$IncludeSchema = $true,
    [switch]$IncludeData = $true
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$scriptDir = $PSScriptRoot

# Tạo thư mục backup nếu chưa có
$backupPath = Join-Path $scriptDir $BackupFolder
if (-not (Test-Path $backupPath)) {
    New-Item -Path $backupPath -ItemType Directory | Out-Null
    Write-Host "✅ Đã tạo thư mục backup: $backupPath" -ForegroundColor Green
}

# Màu sắc cho output
function Write-Info { param($msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param($msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     EXPORT TOÀN BỘ DATABASES - MARITIME PRODUCT v1.1        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ===================================================================
# 1. EXPORT SHORE DATABASE (productdb)
# ===================================================================

Write-Info "Đang export Shore Database (productdb)..."

$shoreBackupFile = Join-Path $backupPath "shore_database_backup_$timestamp.sql"
$shoreDbConfig = @{
    Host = "localhost"
    Port = "5432"
    Database = "productdb"
    Username = "product"
    Password = "productpwd"
}

# Set password environment variable
$env:PGPASSWORD = $shoreDbConfig.Password

try {
    # Export với pg_dump
    $pgDumpArgs = @(
        "-h", $shoreDbConfig.Host,
        "-p", $shoreDbConfig.Port,
        "-U", $shoreDbConfig.Username,
        "-d", $shoreDbConfig.Database,
        "-F", "p",  # Plain text format
        "--no-owner",  # Không include owner
        "--no-privileges",  # Không include privileges
        "-f", $shoreBackupFile
    )

    if ($IncludeSchema -and $IncludeData) {
        # Export cả schema và data (mặc định)
        $pgDumpArgs += "--clean"  # Add DROP statements
        $pgDumpArgs += "--if-exists"  # Add IF EXISTS to DROP statements
    } elseif ($IncludeData -and -not $IncludeSchema) {
        # Chỉ export data
        $pgDumpArgs += "--data-only"
    } elseif ($IncludeSchema -and -not $IncludeData) {
        # Chỉ export schema
        $pgDumpArgs += "--schema-only"
    }

    & pg_dump @pgDumpArgs 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0) {
        $fileSize = (Get-Item $shoreBackupFile).Length / 1KB
        Write-Success "Shore Database đã được export: $shoreBackupFile ($('{0:N2}' -f $fileSize) KB)"
    } else {
        Write-Error "Lỗi khi export Shore Database"
        exit 1
    }
} catch {
    Write-Error "Exception khi export Shore Database: $_"
    exit 1
}

# ===================================================================
# 2. EXPORT EDGE DATABASE (maritime_edge)
# ===================================================================

Write-Info "`nĐang export Edge Database (maritime_edge)..."

$edgeBackupFile = Join-Path $backupPath "edge_database_backup_$timestamp.sql"
$edgeDbConfig = @{
    Host = "localhost"
    Port = "5433"
    Database = "maritime_edge"
    Username = "edge_user"
    Password = "ChangeMe_EdgePassword123!"
}

# Set password environment variable
$env:PGPASSWORD = $edgeDbConfig.Password

try {
    # Export với pg_dump
    $pgDumpArgs = @(
        "-h", $edgeDbConfig.Host,
        "-p", $edgeDbConfig.Port,
        "-U", $edgeDbConfig.Username,
        "-d", $edgeDbConfig.Database,
        "-F", "p",  # Plain text format
        "--no-owner",
        "--no-privileges",
        "-f", $edgeBackupFile
    )

    if ($IncludeSchema -and $IncludeData) {
        $pgDumpArgs += "--clean"
        $pgDumpArgs += "--if-exists"
    } elseif ($IncludeData -and -not $IncludeSchema) {
        $pgDumpArgs += "--data-only"
    } elseif ($IncludeSchema -and -not $IncludeData) {
        $pgDumpArgs += "--schema-only"
    }

    & pg_dump @pgDumpArgs 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0) {
        $fileSize = (Get-Item $edgeBackupFile).Length / 1KB
        Write-Success "Edge Database đã được export: $edgeBackupFile ($('{0:N2}' -f $fileSize) KB)"
    } else {
        Write-Error "Lỗi khi export Edge Database"
        exit 1
    }
} catch {
    Write-Error "Exception khi export Edge Database: $_"
    exit 1
}

# ===================================================================
# 3. TẠO FILE METADATA
# ===================================================================

Write-Info "`nĐang tạo file metadata..."

$metadataFile = Join-Path $backupPath "backup_metadata_$timestamp.json"
$metadata = @{
    BackupDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    BackupBy = $env:USERNAME
    ComputerName = $env:COMPUTERNAME
    Databases = @{
        Shore = @{
            File = "shore_database_backup_$timestamp.sql"
            Host = $shoreDbConfig.Host
            Port = $shoreDbConfig.Port
            Database = $shoreDbConfig.Database
            Size = (Get-Item $shoreBackupFile).Length
        }
        Edge = @{
            File = "edge_database_backup_$timestamp.sql"
            Host = $edgeDbConfig.Host
            Port = $edgeDbConfig.Port
            Database = $edgeDbConfig.Database
            Size = (Get-Item $edgeBackupFile).Length
        }
    }
    Options = @{
        IncludeSchema = $IncludeSchema
        IncludeData = $IncludeData
    }
}

$metadata | ConvertTo-Json -Depth 10 | Out-File -FilePath $metadataFile -Encoding UTF8
Write-Success "Metadata đã được tạo: $metadataFile"

# ===================================================================
# 4. TẠO PACKAGE ZIP (OPTIONAL)
# ===================================================================

Write-Info "`nĐang tạo file ZIP..."

$zipFile = Join-Path $scriptDir "database_backup_$timestamp.zip"
try {
    Compress-Archive -Path $shoreBackupFile, $edgeBackupFile, $metadataFile -DestinationPath $zipFile -Force
    $zipSize = (Get-Item $zipFile).Length / 1MB
    Write-Success "File ZIP đã được tạo: $zipFile ($('{0:N2}' -f $zipSize) MB)"
} catch {
    Write-Warning "Không thể tạo file ZIP: $_"
}

# ===================================================================
# SUMMARY
# ===================================================================

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                  EXPORT HOÀN THÀNH                           ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n📦 Files đã tạo:" -ForegroundColor Cyan
Write-Host "   1. Shore DB: $shoreBackupFile" -ForegroundColor White
Write-Host "   2. Edge DB:  $edgeBackupFile" -ForegroundColor White
Write-Host "   3. Metadata: $metadataFile" -ForegroundColor White
if (Test-Path $zipFile) {
    Write-Host "   4. ZIP:      $zipFile" -ForegroundColor White
}

Write-Host "`n📋 Hướng dẫn sử dụng:" -ForegroundColor Yellow
Write-Host "   - Để import lại database, chạy script: import-all-databases.ps1" -ForegroundColor White
Write-Host "   - Hoặc sử dụng file ZIP để chia sẻ với team" -ForegroundColor White
Write-Host ""

# Clean up
$env:PGPASSWORD = $null
