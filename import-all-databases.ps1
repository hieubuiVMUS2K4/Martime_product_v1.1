# ===================================================================
# Script Import Toàn Bộ Database
# ===================================================================
# Script này sẽ import dữ liệu vào cả 2 databases:
# 1. Shore Database (productdb) - Port 5432
# 2. Edge Database (maritime_edge) - Port 5433
#
# CẢNH BÁO: Script này sẽ XÓA toàn bộ dữ liệu hiện tại!
# ===================================================================

param(
    [string]$BackupFolder = "database-backups",
    [string]$Timestamp = "",
    [switch]$SkipConfirmation = $false,
    [switch]$DropDatabase = $false,
    [switch]$CreateDatabase = $false
)

$scriptDir = $PSScriptRoot
$backupPath = Join-Path $scriptDir $BackupFolder

# Màu sắc cho output
function Write-Info { param($msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param($msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║     IMPORT TOÀN BỘ DATABASES - MARITIME PRODUCT v1.1        ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Yellow

# Kiểm tra thư mục backup
if (-not (Test-Path $backupPath)) {
    Write-Error "Không tìm thấy thư mục backup: $backupPath"
    exit 1
}

# Tìm file backup mới nhất nếu không chỉ định timestamp
if ([string]::IsNullOrEmpty($Timestamp)) {
    Write-Info "Đang tìm backup mới nhất..."
    $latestShoreBackup = Get-ChildItem -Path $backupPath -Filter "shore_database_backup_*.sql" | 
                         Sort-Object LastWriteTime -Descending | 
                         Select-Object -First 1
    
    if ($null -eq $latestShoreBackup) {
        Write-Error "Không tìm thấy file backup nào trong $backupPath"
        exit 1
    }
    
    # Extract timestamp từ filename
    if ($latestShoreBackup.Name -match "shore_database_backup_(\d{8}_\d{6})\.sql") {
        $Timestamp = $Matches[1]
        Write-Success "Tìm thấy backup: $Timestamp"
    } else {
        Write-Error "Không thể xác định timestamp từ file: $($latestShoreBackup.Name)"
        exit 1
    }
}

# Định nghĩa đường dẫn file
$shoreBackupFile = Join-Path $backupPath "shore_database_backup_$Timestamp.sql"
$edgeBackupFile = Join-Path $backupPath "edge_database_backup_$Timestamp.sql"
$metadataFile = Join-Path $backupPath "backup_metadata_$Timestamp.json"

# Kiểm tra file tồn tại
if (-not (Test-Path $shoreBackupFile)) {
    Write-Error "Không tìm thấy file Shore backup: $shoreBackupFile"
    exit 1
}

if (-not (Test-Path $edgeBackupFile)) {
    Write-Error "Không tìm thấy file Edge backup: $edgeBackupFile"
    exit 1
}

# Hiển thị thông tin backup
if (Test-Path $metadataFile) {
    Write-Info "Thông tin backup:"
    $metadata = Get-Content $metadataFile | ConvertFrom-Json
    Write-Host "   - Ngày backup: $($metadata.BackupDate)" -ForegroundColor White
    Write-Host "   - Người backup: $($metadata.BackupBy)" -ForegroundColor White
    Write-Host "   - Máy tính: $($metadata.ComputerName)" -ForegroundColor White
}

# Xác nhận trước khi import
if (-not $SkipConfirmation) {
    Write-Warning "`n⚠️  CẢNH BÁO: Script này sẽ XÓA toàn bộ dữ liệu hiện tại và import dữ liệu mới!"
    Write-Host "`nBạn có chắc chắn muốn tiếp tục? (yes/no): " -NoNewline -ForegroundColor Yellow
    $confirmation = Read-Host
    
    if ($confirmation -ne "yes") {
        Write-Info "Đã hủy import"
        exit 0
    }
}

Write-Host ""

# Database configs
$shoreDbConfig = @{
    Host = "localhost"
    Port = "5432"
    Database = "productdb"
    Username = "product"
    Password = "productpwd"
}

$edgeDbConfig = @{
    Host = "localhost"
    Port = "5433"
    Database = "maritime_edge"
    Username = "edge_user"
    Password = "ChangeMe_EdgePassword123!"
}

# ===================================================================
# FUNCTION: Import Database
# ===================================================================

function Import-Database {
    param(
        [string]$Name,
        [hashtable]$Config,
        [string]$BackupFile
    )
    
    Write-Info "Đang import $Name Database..."
    
    # Set password
    $env:PGPASSWORD = $Config.Password
    
    try {
        # Nếu cần drop database
        if ($DropDatabase) {
            Write-Warning "Đang drop database $($Config.Database)..."
            & psql -h $Config.Host -p $Config.Port -U $Config.Username -d postgres -c "DROP DATABASE IF EXISTS $($Config.Database);" 2>&1 | Out-Null
        }
        
        # Nếu cần create database
        if ($CreateDatabase) {
            Write-Info "Đang tạo database $($Config.Database)..."
            & psql -h $Config.Host -p $Config.Port -U $Config.Username -d postgres -c "CREATE DATABASE $($Config.Database);" 2>&1 | Out-Null
        }
        
        # Import data
        Write-Info "Đang restore dữ liệu..."
        $psqlArgs = @(
            "-h", $Config.Host,
            "-p", $Config.Port,
            "-U", $Config.Username,
            "-d", $Config.Database,
            "-f", $BackupFile
        )
        
        $output = & psql @psqlArgs 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "$Name Database đã được import thành công"
            return $true
        } else {
            Write-Error "Lỗi khi import $Name Database"
            Write-Host $output -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Error "Exception khi import $Name Database: $_"
        return $false
    } finally {
        $env:PGPASSWORD = $null
    }
}

# ===================================================================
# 1. IMPORT SHORE DATABASE
# ===================================================================

$shoreSuccess = Import-Database -Name "Shore" -Config $shoreDbConfig -BackupFile $shoreBackupFile

# ===================================================================
# 2. IMPORT EDGE DATABASE
# ===================================================================

$edgeSuccess = Import-Database -Name "Edge" -Config $edgeDbConfig -BackupFile $edgeBackupFile

# ===================================================================
# SUMMARY
# ===================================================================

Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                  KẾT QUẢ IMPORT                              ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Host "`n📊 Kết quả:" -ForegroundColor Cyan
Write-Host "   - Shore Database: " -NoNewline -ForegroundColor White
if ($shoreSuccess) {
    Write-Host "✅ Thành công" -ForegroundColor Green
} else {
    Write-Host "❌ Thất bại" -ForegroundColor Red
}

Write-Host "   - Edge Database:  " -NoNewline -ForegroundColor White
if ($edgeSuccess) {
    Write-Host "✅ Thành công" -ForegroundColor Green
} else {
    Write-Host "❌ Thất bại" -ForegroundColor Red
}

if ($shoreSuccess -and $edgeSuccess) {
    Write-Host "`n🎉 Import hoàn thành thành công!" -ForegroundColor Green
    Write-Host "   Bạn có thể khởi động lại ứng dụng để sử dụng dữ liệu mới" -ForegroundColor White
} else {
    Write-Host "`n⚠️  Import hoàn thành với lỗi" -ForegroundColor Yellow
    Write-Host "   Vui lòng kiểm tra log để biết thêm chi tiết" -ForegroundColor White
}

Write-Host ""
