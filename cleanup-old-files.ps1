# =====================================================
# Clean up old/unused files from workspace
# Created: 2025-12-11
# =====================================================

$rootPath = "f:\NCKH\Product\Martime_product_v1.1"
$deletedCount = 0
$failedCount = 0
$deletedFiles = @()
$failedFiles = @()

Write-Host "================================" -ForegroundColor Cyan
Write-Host "MARITIME WORKSPACE CLEANUP" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# List of files to delete
$filesToDelete = @(
    # === BACKUP SQL FILES (5) ===
    "backend\productdb_backup_20251126_135933.sql",
    "edge-services\backup_before_migration_test_20251207_160349.sql",
    "edge-services\backup_full_20251207_160427.sql",
    "edge-services\database-schema-backup.sql",
    "edge-services\maritime_edge_full_backup.sql",
    
    # === MIGRATION/FIX SQL FILES (20) ===
    "edge-services\migration_v2_sync_update.sql",
    "edge-services\migration_v3_sync_queue.sql",
    "edge-services\migration_v4_material_receipts.sql",
    "edge-services\migration_v4_material_receipts_snake_case.sql",
    "edge-services\migration_v5_remove_receipt_columns.sql",
    "edge-services\migration_v6_fix_material_receipts_naming.sql",
    "edge-services\migration_v7_fix_material_item_id_type.sql",
    "edge-services\fix-deck-logbook-schema.sql",
    "edge-services\fix-engine-logbook-schema.sql",
    "edge-services\fix-equipment-assets-columns.sql",
    "edge-services\fix-missing-columns.sql",
    "edge-services\fix-missing-columns-v2.sql",
    "edge-services\fix-missing-columns-v3.sql",
    "edge-services\fix-oil-record-book-schema.sql",
    "edge-services\fix-task-type-id-column.sql",
    "edge-services\migrate-schedule-to-groups.sql",
    "edge-services\add-equipment-status-management.sql",
    "edge-services\add-performance-indexes.sql",
    "edge-services\add-missing-indexes.sql",
    "edge-services\sync-database-schema.sql",
    "edge-services\create-pms-tables.sql",
    "edge-services\create-schedule-spare-parts-table.sql",
    "edge-services\add-pms-planning-tables.sql",
    "edge-services\add-maintenance-schedules.sql",
    "edge-services\add-watchkeeping-stcw-columns.sql",
    "edge-services\add-crew-role-code.sql",
    "backend\migrate-shore-optimization.sql",
    
    # === SEED/TEST DATA FILES (12) ===
    "edge-services\insert-sample-data-fixed.sql",
    "edge-services\insert-10-more-data.sql",
    "edge-services\insert-task-system-sample.sql",
    "edge-services\insert-test-schedules.sql",
    "edge-services\seed-pms-complete.sql",
    "edge-services\seed-pms-minimal.sql",
    "edge-services\seed-pms-sample-data.sql",
    "edge-services\seed-equipment-groups.sql",
    "edge-services\reset-maintenance-workflow.sql",
    "edge-services\revalidate-existing-tasks.sql",
    "edge-services\insert-roles-and-users.sql",
    "edge-services\seed-crew-and-users.sql",
    
    # === CHECK/UTILITY FILES (10) ===
    "edge-services\check-checklist-table.sql",
    "edge-services\check-existing-roles.sql",
    "edge-services\check-logbooks.sql",
    "edge-services\check-roles.sql",
    "edge-services\check-tables.sql",
    "edge-services\list-all-tables.sql",
    "edge-services\database-health-check.sql",
    "edge-services\database-audit-fixed.sql",
    "check-schedule-templates.ps1",
    
    # === POWERSHELL SCRIPTS (7) ===
    "edge-services\restore-database.ps1",
    "edge-services\restore-database-fixed.ps1",
    "edge-services\reset-and-test-workflow.ps1",
    "edge-services\validate-database-sync.ps1",
    "edge-services\generate-crew-passwords.ps1",
    "frontend-edge\optimize-form-css.ps1",
    "frontend-mobile\test-i18n.ps1"
)

Write-Host "Total files to delete: $($filesToDelete.Count)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Starting cleanup..." -ForegroundColor Green
Write-Host ""

foreach ($file in $filesToDelete) {
    $fullPath = Join-Path $rootPath $file
    
    if (Test-Path $fullPath) {
        try {
            # Get file size before deletion
            $fileSize = (Get-Item $fullPath).Length
            $fileSizeMB = [math]::Round($fileSize / 1MB, 2)
            
            Remove-Item $fullPath -Force
            $deletedCount++
            $deletedFiles += $file
            
            Write-Host "[OK] Deleted: $file " -ForegroundColor Green -NoNewline
            if ($fileSizeMB -gt 0) {
                Write-Host "($fileSizeMB MB)" -ForegroundColor Gray
            } else {
                Write-Host ""
            }
        }
        catch {
            $failedCount++
            $failedFiles += $file
            Write-Host "[FAIL] Failed: $file - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "[SKIP] Not found: $file" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "CLEANUP SUMMARY" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Total files processed: $($filesToDelete.Count)" -ForegroundColor White
Write-Host "Successfully deleted: $deletedCount" -ForegroundColor Green
Write-Host "Failed to delete: $failedCount" -ForegroundColor Red
Write-Host "Not found: $($filesToDelete.Count - $deletedCount - $failedCount)" -ForegroundColor Gray
Write-Host ""

if ($deletedCount -gt 0) {
    # Calculate total space saved
    Write-Host "Files cleaned up:" -ForegroundColor Green
    foreach ($file in $deletedFiles) {
        Write-Host "  - $file" -ForegroundColor DarkGreen
    }
}

if ($failedCount -gt 0) {
    Write-Host ""
    Write-Host "Failed to delete:" -ForegroundColor Red
    foreach ($file in $failedFiles) {
        Write-Host "  - $file" -ForegroundColor DarkRed
    }
}

Write-Host ""
Write-Host "Cleanup completed!" -ForegroundColor Cyan
Write-Host ""
