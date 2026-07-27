# =============================================================
# build-production.ps1
# Build toàn bộ shore_product thành package production/
#
# Usage:
#   cd shore_product
#   .\build-production.ps1              # full build
#   .\build-production.ps1 -SkipNpm    # bỏ qua npm install (đã có node_modules)
# =============================================================
param(
    [switch]$SkipNpm
)

$ErrorActionPreference = "Stop"
$scriptDir    = Split-Path -Parent $MyInvocation.MyCommand.Path
$prodDir      = Join-Path $scriptDir "production"
$backendSrc   = Join-Path $scriptDir "backend"
$frontendSrc  = Join-Path $scriptDir "frontend"
$initScripts  = Join-Path $scriptDir "init-scripts"

function Write-Step($msg) { Write-Host "`n>>> $msg" -ForegroundColor Cyan }
function Write-OK($msg)   { Write-Host "    OK: $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "    FAIL: $msg" -ForegroundColor Red; exit 1 }

# ─── 0. Kiểm tra tools ─────────────────────────────────────
Write-Step "Checking prerequisites..."
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Write-Fail "dotnet not found" }
if (-not (Get-Command npm    -ErrorAction SilentlyContinue)) { Write-Fail "npm not found" }
Write-OK "dotnet + npm found"

# ─── 1. Clean output ───────────────────────────────────────
Write-Step "Cleaning previous build output..."
$cleanPaths = @(
    "$prodDir\backend",
    "$prodDir\frontend\dist"
)
foreach ($p in $cleanPaths) {
    if (Test-Path $p) {
        Remove-Item $p -Recurse -Force
        Write-OK "Removed: $p"
    }
}

# ─── 2. Build .NET backend ─────────────────────────────────
Write-Step "Building .NET 8 backend (dotnet publish)..."
$pubOut = "$prodDir\backend"
dotnet publish "$backendSrc\product-api.csproj" -c Release -o $pubOut --nologo
if ($LASTEXITCODE -ne 0) { Write-Fail "dotnet publish failed (exit $LASTEXITCODE)" }
Write-OK "Backend published → production\backend\"

# ─── 3. Build React frontend ───────────────────────────────
Write-Step "Building React frontend (npm run build)..."
Push-Location $frontendSrc
try {
    if (-not $SkipNpm) {
        Write-Host "    Running npm ci..." -ForegroundColor Gray
        npm ci --silent
        if ($LASTEXITCODE -ne 0) { Write-Fail "npm ci failed" }
    }
    npm run build
    if ($LASTEXITCODE -ne 0) { Write-Fail "npm run build failed" }
} finally {
    Pop-Location
}

# Copy dist → production/frontend/dist/
$distDest = "$prodDir\frontend\dist"
New-Item -ItemType Directory -Path $distDest -Force | Out-Null
Copy-Item "$frontendSrc\dist\*" $distDest -Recurse -Force
Write-OK "Frontend built → production\frontend\dist\"

# ─── 4. Copy init-scripts ──────────────────────────────────
Write-Step "Copying database init scripts..."
$initDest = "$prodDir\init-scripts"
New-Item -ItemType Directory -Path $initDest -Force | Out-Null

# Chỉ copy 00-shore-dump.sql (schema + data đầy đủ)
$dumpFile = "$initScripts\00-shore-dump.sql"
if (-not (Test-Path $dumpFile)) {
    Write-Fail "00-shore-dump.sql not found at: $dumpFile"
}
Copy-Item $dumpFile $initDest -Force
Write-OK "Copied 00-shore-dump.sql ($('{0:N0}' -f ((Get-Item $dumpFile).Length / 1KB)) KB)"

# ─── 5. Kiểm tra file cấu hình ─────────────────────────────
Write-Step "Verifying production package structure..."
$required = @(
    "$prodDir\Dockerfile.backend",
    "$prodDir\Dockerfile.frontend",
    "$prodDir\docker-compose.yml",
    "$prodDir\nginx\default.conf",
    "$prodDir\.env.production",
    "$prodDir\backend\product-api.dll",
    "$prodDir\frontend\dist\index.html",
    "$prodDir\init-scripts\00-shore-dump.sql"
)
$allOk = $true
foreach ($f in $required) {
    if (Test-Path $f) {
        Write-OK (Split-Path $f -Leaf)
    } else {
        Write-Host "    MISSING: $f" -ForegroundColor Red
        $allOk = $false
    }
}
if (-not $allOk) { Write-Fail "Some required files are missing!" }

# ─── 6. Summary ────────────────────────────────────────────
$totalSize = (Get-ChildItem $prodDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "  BUILD COMPLETE" -ForegroundColor Green
Write-Host "  Package size : $([math]::Round($totalSize / 1MB, 1)) MB" -ForegroundColor Green
Write-Host "  Location     : $prodDir" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Các bước tiếp theo:" -ForegroundColor Yellow
Write-Host "  1. Copy thư mục 'production\' lên server (rsync / scp / zip)"
Write-Host "  2. Trên server: cd production"
Write-Host "  3. cp .env.production .env"
Write-Host "  4. Sửa tất cả giá trị CHANGE_ME trong .env"
Write-Host "     (Tạo JWT_KEY: openssl rand -base64 48)"
Write-Host "  5. docker compose up -d --build"
Write-Host ""
Write-Host "  Bật pgAdmin khi cần:"
Write-Host "     docker compose --profile tools up -d pgadmin"
