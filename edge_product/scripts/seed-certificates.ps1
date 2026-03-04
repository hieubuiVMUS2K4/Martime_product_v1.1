# Seed Certificates PowerShell Script
Write-Host "Seeding certificate data..." -ForegroundColor Cyan

# Read SQL file
$sqlContent = Get-Content "SEED_CERTIFICATES.sql" -Raw

# Execute via docker
$sqlContent | docker exec -i maritime psql -U postgres -d edge_database

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Successfully seeded certificates!" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to seed certificates" -ForegroundColor Red
}
