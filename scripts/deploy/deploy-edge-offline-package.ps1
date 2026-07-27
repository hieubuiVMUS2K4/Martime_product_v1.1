# ==============================================================================
# EDGE VESSEL OFFLINE PACKAGE BUILDER & DEPLOYMENT SCRIPT
# ==============================================================================
# Usage:
#   powershell -ExecutionPolicy Bypass -File ./scripts/deploy/deploy-edge-offline-package.ps1
# ==============================================================================

param (
    [string]$OutputDir = "./dist/edge-vessel-offline-bundle",
    [string]$Version = "1.1.0"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Building Edge Vessel Offline Deployment Package v$Version" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (Test-Path -Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Step 1: Copy production docker-compose and configs
Write-Host "[1/3] Copying production docker configuration..." -ForegroundColor Green
Copy-Item -Path "./edge_product/production/*" -Destination $OutputDir -Recurse -Force

# Step 2: Build backend binaries
Write-Host "[2/3] Publishing Edge .NET Backend..." -ForegroundColor Green
dotnet publish ./edge_product/edge-services/EdgeCollector.csproj -c Release -o "$OutputDir/backend"

# Step 3: Build frontend dist
Write-Host "[3/3] Building Edge React Frontend..." -ForegroundColor Green
Set-Location ./edge_product/frontend-edge
npm install
npm run build
Set-Location ../..

Copy-Item -Path "./edge_product/frontend-edge/dist" -Destination "$OutputDir/frontend/dist" -Recurse -Force

Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Offline Package Created Successfully at: $OutputDir" -ForegroundColor Green
Write-Host " Copy the contents of $OutputDir to the ship server and run:" -ForegroundColor Yellow
Write-Host "   docker compose up -d" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Green
