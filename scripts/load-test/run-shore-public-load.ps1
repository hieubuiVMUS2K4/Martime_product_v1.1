param(
    [ValidateSet('100', '150', '200', '300', '500', 'custom')]
    [string]$LoadProfile = '100',

    [int]$Vus = 0,

    [string]$BaseUrl = 'https://shcdvmu.site',

    [string]$RampUp = '2m',

    [string]$Hold = '10m',

    [string]$RampDown = '1m',

    [int]$MinThinkMs = 800,

    [int]$MaxThinkMs = 2500,

    [string]$AccessToken = '',

    [string]$OutputRoot = 'artifacts/load-test'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

function Resolve-TargetVus {
    param(
        [string]$SelectedProfile,
        [int]$CustomVus
    )

    switch ($SelectedProfile) {
        '100' { return 100 }
        '150' { return 150 }
        '200' { return 200 }
        '300' { return 300 }
        '500' { return 500 }
        'custom' {
            if ($CustomVus -lt 1) {
                throw 'Khi dung -Profile custom, can truyen -Vus > 0.'
            }

            return $CustomVus
        }
        default {
            throw "Profile khong hop le: $SelectedProfile"
        }
    }
}

$k6Command = Get-Command k6 -ErrorAction SilentlyContinue
if (-not $k6Command) {
    $fallbackPaths = @(
        (Join-Path $env:ProgramFiles 'k6\k6.exe'),
        (Join-Path $env:ProgramFiles 'GrafanaLabs\k6\k6.exe')
    )

    $fallbackPath = $fallbackPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
    if ($fallbackPath) {
        $k6Command = [pscustomobject]@{ Source = $fallbackPath }
    } else {
        throw @"
Khong tim thay k6 trong PATH.

Cai dat nhanh tren Windows:
  winget install --id GrafanaLabs.k6 -e

Sau do mo terminal moi va chay lai script nay.
"@
    }
}

$targetVus = Resolve-TargetVus -SelectedProfile $LoadProfile -CustomVus $Vus
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runName = "shore-public-$timestamp-${targetVus}vu"
$workspaceRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$outputDir = Join-Path $workspaceRoot $OutputRoot
$runDir = Join-Path $outputDir $runName
$scriptPath = Join-Path $PSScriptRoot 'shore-public-load.js'
$summaryPath = Join-Path $runDir 'summary.json'
$consolePath = Join-Path $runDir 'console.log'
$configPath = Join-Path $runDir 'run-config.json'

New-Item -ItemType Directory -Path $runDir -Force | Out-Null

$runConfig = [ordered]@{
    baseUrl = $BaseUrl
    targetVus = $targetVus
    rampUp = $RampUp
    hold = $Hold
    rampDown = $RampDown
    minThinkMs = $MinThinkMs
    maxThinkMs = $MaxThinkMs
    accessTokenConfigured = -not [string]::IsNullOrWhiteSpace($AccessToken)
    startedAt = (Get-Date).ToString('o')
    scriptPath = $scriptPath
}

$runConfig | ConvertTo-Json -Depth 5 | Set-Content -Path $configPath

$env:BASE_URL = $BaseUrl
$env:TARGET_VUS = "$targetVus"
$env:RAMP_UP = $RampUp
$env:HOLD = $Hold
$env:RAMP_DOWN = $RampDown
$env:MIN_THINK_MS = "$MinThinkMs"
$env:MAX_THINK_MS = "$MaxThinkMs"
$env:ACCESS_TOKEN = $AccessToken
$env:K6_WEB_DASHBOARD = 'false'

Write-Host "Running shore public load test"
Write-Host "  Base URL : $BaseUrl"
Write-Host "  VUs      : $targetVus"
Write-Host "  Auth     : $(if ([string]::IsNullOrWhiteSpace($AccessToken)) { 'public' } else { 'bearer token configured' })"
Write-Host "  Output   : $runDir"
Write-Host ''

& $k6Command.Source run --summary-export $summaryPath $scriptPath 2>&1 | Tee-Object -FilePath $consolePath

Write-Host ''
Write-Host 'Completed.'
Write-Host "  Summary : $summaryPath"
Write-Host "  Console : $consolePath"
Write-Host "  Config  : $configPath"