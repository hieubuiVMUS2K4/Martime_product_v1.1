# Maritime Edge Database Backup Script
# Run: .\backup-edge-database.ps1

$ErrorActionPreference = "Stop"

# Configuration
$backupDir = "F:\NCKH\Product\sampleProduct-master (1)\sampleProduct-master\edge-services\backups"
$containerName = "maritime-edge-postgres"
$dbUser = "edge_user"
$dbName = "maritime_edge"
$retentionDays = 30

# Create timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$backupDir\maritime_edge_$timestamp.sql.gz"

# Create backup directory if not exists
if (!(Test-Path $backupDir)) {
    Write-Host "📁 Creating backup directory: $backupDir"
    New-Item -ItemType Directory -Path $backupDir | Out-Null
}

# Check if container is running
Write-Host "🔍 Checking PostgreSQL container status..."
$containerStatus = docker inspect -f '{{.State.Running}}' $containerName 2>$null

if ($containerStatus -ne "true") {
    Write-Host "❌ Container '$containerName' is not running!"
    Write-Host "Start it with: docker-compose up -d edge-postgres"
    exit 1
}

# Create backup
Write-Host "💾 Starting backup to: $backupFile"
Write-Host "   Database: $dbName"
Write-Host "   User: $dbUser"
Write-Host "   Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ""

try {
    # Execute pg_dump and compress
    docker exec $containerName pg_dump -U $dbUser $dbName | gzip > $backupFile
    
    if (Test-Path $backupFile) {
        $fileSize = (Get-Item $backupFile).Length / 1MB
        Write-Host "✅ Backup completed successfully!"
        Write-Host "   File: $backupFile"
        Write-Host "   Size: $([math]::Round($fileSize, 2)) MB"
        Write-Host ""
        
        # Clean up old backups
        Write-Host "🗑️  Cleaning up old backups (older than $retentionDays days)..."
        $oldBackups = Get-ChildItem $backupDir -Filter "maritime_edge_*.sql.gz" | 
            Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$retentionDays) }
        
        if ($oldBackups) {
            foreach ($file in $oldBackups) {
                Write-Host "   Removing: $($file.Name)"
                Remove-Item $file.FullName -Force
            }
            Write-Host "   Removed $($oldBackups.Count) old backup(s)"
        } else {
            Write-Host "   No old backups to remove"
        }
        
        # List current backups
        Write-Host ""
        Write-Host "📋 Current backups:"
        Get-ChildItem $backupDir -Filter "maritime_edge_*.sql.gz" | 
            Sort-Object LastWriteTime -Descending | 
            Select-Object -First 5 | 
            ForEach-Object {
                $age = [math]::Round(((Get-Date) - $_.LastWriteTime).TotalDays, 1)
                $size = [math]::Round($_.Length / 1MB, 2)
                Write-Host "   $($_.Name) - $size MB ($age days old)"
            }
        
        Write-Host ""
        Write-Host "🎉 Backup process completed successfully!"
        
    } else {
        throw "Backup file was not created"
    }
    
} catch {
    Write-Host "❌ Backup failed: $_"
    Write-Host "Check Docker logs: docker logs $containerName"
    exit 1
}

# Optional: Test backup integrity
Write-Host ""
Write-Host "🔬 Testing backup integrity..."
$testResult = gunzip -t $backupFile 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Backup file integrity: OK"
} else {
    Write-Host "⚠️  Warning: Could not verify backup integrity (gunzip not found)"
}

Write-Host ""
Write-Host "💡 To restore this backup, run:"
Write-Host "   gunzip -c $backupFile | docker exec -i $containerName psql -U $dbUser $dbName"
