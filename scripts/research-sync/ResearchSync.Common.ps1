Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-ResearchLog {
    param(
        [Parameter(Mandatory = $true)][string]$Message,
        [ValidateSet('INFO', 'WARN', 'ERROR')][string]$Level = 'INFO'
    )

    $timestamp = [DateTime]::UtcNow.ToString('o')
    Write-Host "[$timestamp] [$Level] $Message"
}

function New-ResearchDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        $null = New-Item -ItemType Directory -Path $Path -Force
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Write-ResearchJsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][object]$Data
    )

    $parentPath = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parentPath) -and -not (Test-Path -LiteralPath $parentPath)) {
        $null = New-Item -ItemType Directory -Path $parentPath -Force
    }

    $Data | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Invoke-PostgresSqlFile {
    param(
        [Parameter(Mandatory = $true)][string]$ContainerName,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$SqlFilePath
    )

    if (-not (Test-Path -LiteralPath $SqlFilePath)) {
        throw "SQL file not found: $SqlFilePath"
    }

    $content = Get-Content -LiteralPath $SqlFilePath -Raw
    $content | docker exec -i $ContainerName psql -v ON_ERROR_STOP=1 -U $Username -d $Database | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "psql execution failed for file: $SqlFilePath"
    }
}

function Get-DockerContainerState {
    param([Parameter(Mandatory = $true)][string]$ContainerName)

    $status = docker ps -a --filter "name=^/$ContainerName$" --format '{{.Status}}'
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($status)) {
        return $null
    }

    $trimmed = $status.Trim()
    if ($trimmed.StartsWith('Up', [System.StringComparison]::OrdinalIgnoreCase)) {
        return 'running'
    }

    if ($trimmed.StartsWith('Exited', [System.StringComparison]::OrdinalIgnoreCase)) {
        return 'exited'
    }

    if ($trimmed.StartsWith('Created', [System.StringComparison]::OrdinalIgnoreCase)) {
        return 'created'
    }

    return $trimmed
}

function Assert-DockerContainerRunning {
    param([Parameter(Mandatory = $true)][string]$ContainerName)

    $state = Get-DockerContainerState -ContainerName $ContainerName
    if ($state -ne 'running') {
        throw "Docker container '$ContainerName' is not running. Current state: $state"
    }
}

function New-InternalApiHeaders {
    param(
        [string]$InternalApiKey,
        [string]$AccessToken
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($InternalApiKey)) {
        $headers['X-Internal-Api-Key'] = $InternalApiKey
    }

    if (-not [string]::IsNullOrWhiteSpace($AccessToken)) {
        $headers['Authorization'] = "Bearer $AccessToken"
    }

    return $headers
}

function Invoke-EdgeApi {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('GET', 'POST')][string]$Method,
        [Parameter(Mandatory = $true)][string]$BaseUrl,
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$InternalApiKey,
        [string]$AccessToken,
        [object]$Body
    )

    $uri = ([Uri]::new($BaseUrl.TrimEnd('/') + '/' + $Path.TrimStart('/'))).AbsoluteUri
    $headers = New-InternalApiHeaders -InternalApiKey $InternalApiKey -AccessToken $AccessToken

    if ($Method -eq 'GET') {
        return Invoke-RestMethod -Method Get -Uri $uri -Headers $headers -TimeoutSec 60
    }

    if ($null -eq $Body) {
        return Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -TimeoutSec 300
    }

    $json = $Body | ConvertTo-Json -Depth 10
    return Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -TimeoutSec 300 -Body $json -ContentType 'application/json'
}

function Get-EdgeSyncStatus {
    param(
        [Parameter(Mandatory = $true)][string]$BaseUrl,
        [string]$InternalApiKey,
        [string]$AccessToken
    )

    return Invoke-EdgeApi -Method GET -BaseUrl $BaseUrl -Path 'api/sync/status' -InternalApiKey $InternalApiKey -AccessToken $AccessToken
}

function Get-EdgeAccessToken {
    param(
        [Parameter(Mandatory = $true)][string]$BaseUrl,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$Password,
        [string]$DeviceType = 'BRIDGE_PC'
    )

    $body = @{
        username = $Username
        password = $Password
        deviceType = $DeviceType
    }

    $response = Invoke-RestMethod -Method Post -Uri ([Uri]::new($BaseUrl.TrimEnd('/') + '/api/auth/login')).AbsoluteUri -TimeoutSec 60 -Body ($body | ConvertTo-Json -Depth 5) -ContentType 'application/json'
    if ($null -eq $response -or [string]::IsNullOrWhiteSpace($response.accessToken)) {
        throw 'Unable to obtain Edge access token from /api/auth/login.'
    }

    return [string]$response.accessToken
}

function Invoke-PostgresJsonQuery {
    param(
        [Parameter(Mandatory = $true)][string]$ContainerName,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$Sql
    )

    Assert-DockerContainerRunning -ContainerName $ContainerName

    $raw = $Sql | docker exec -i $ContainerName psql -t -A -v ON_ERROR_STOP=1 -U $Username -d $Database
    if ($LASTEXITCODE -ne 0) {
        throw 'psql JSON query execution failed.'
    }

    $trimmed = $raw.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return $null
    }

    return $trimmed | ConvertFrom-Json
}

function Convert-DockerPercentString {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $match = [regex]::Match($Value, '([0-9]+(?:\.[0-9]+)?)')
    if (-not $match.Success) {
        return $null
    }

    return [double]$match.Groups[1].Value
}

function Convert-DockerSizeToBytes {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $match = [regex]::Match($Value.Trim(), '^([0-9]+(?:\.[0-9]+)?)\s*([KMGTP]?i?B)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        return $null
    }

    $number = [double]$match.Groups[1].Value
    $unit = $match.Groups[2].Value.ToUpperInvariant()
    $factor = switch ($unit) {
        'B' { 1 }
        'KB' { 1000 }
        'MB' { 1000 * 1000 }
        'GB' { 1000 * 1000 * 1000 }
        'TB' { 1000 * 1000 * 1000 * 1000 }
        'KIB' { 1024 }
        'MIB' { 1024 * 1024 }
        'GIB' { 1024 * 1024 * 1024 }
        'TIB' { 1024 * 1024 * 1024 * 1024 }
        default { $null }
    }

    if ($null -eq $factor) {
        return $null
    }

    return [math]::Round($number * $factor, 0)
}

function Convert-DockerStatsSnapshotToMetrics {
    param([object[]]$StatsSnapshot)

    $metrics = @()
    foreach ($item in @($StatsSnapshot)) {
        if ($null -eq $item) {
            continue
        }

        $memoryUsageText = $null
        if (-not [string]::IsNullOrWhiteSpace($item.MemUsage)) {
            $memoryUsageText = ($item.MemUsage -split '/')[0].Trim()
        }

        $metrics += [pscustomobject]@{
            name = $item.Name
            cpu_percent = Convert-DockerPercentString -Value $item.CPUPerc
            memory_usage_text = $memoryUsageText
            memory_usage_bytes = Convert-DockerSizeToBytes -Value $memoryUsageText
        }
    }

    return $metrics
}

function Get-DockerStatsSnapshot {
    param([string[]]$ContainerNames)

    $stats = @()
    foreach ($containerName in $ContainerNames) {
        if ([string]::IsNullOrWhiteSpace($containerName)) {
            continue
        }

        if ((Get-DockerContainerState -ContainerName $containerName) -ne 'running') {
            continue
        }

        $raw = docker stats $containerName --no-stream --format '{{json .}}' 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($raw)) {
            continue
        }

        try {
            $stats += ($raw | ConvertFrom-Json)
        }
        catch {
            Write-ResearchLog -Level WARN -Message "Unable to parse docker stats for $containerName"
        }
    }

    return $stats
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha256.ComputeHash($Bytes)
    }
    finally {
        $sha256.Dispose()
    }

    return ([System.BitConverter]::ToString($hash).Replace('-', '')).ToLowerInvariant()
}

function Get-RelativePathPortable {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $normalizedBase = (Resolve-Path -LiteralPath $BasePath).Path
    $normalizedTarget = (Resolve-Path -LiteralPath $TargetPath).Path

    if (-not $normalizedBase.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $normalizedBase += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = [Uri]$normalizedBase
    $targetUri = [Uri]$normalizedTarget
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    return [Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

function Get-ResearchSeriesStatistics {
    param([double[]]$Values)

    $series = @($Values | Where-Object { $null -ne $_ })
    if ($series.Count -eq 0) {
        return [pscustomobject]@{
            count = 0
            mean = $null
            median = $null
            min = $null
            max = $null
            std_dev = $null
            ci95 = $null
        }
    }

    $sorted = @($series | Sort-Object)
    $count = $sorted.Count
    $mean = ($sorted | Measure-Object -Average).Average
    $min = ($sorted | Measure-Object -Minimum).Minimum
    $max = ($sorted | Measure-Object -Maximum).Maximum

    if (($count % 2) -eq 1) {
        $median = $sorted[[int][math]::Floor($count / 2)]
    }
    else {
        $upperIndex = [int]($count / 2)
        $lowerIndex = $upperIndex - 1
        $median = ($sorted[$lowerIndex] + $sorted[$upperIndex]) / 2.0
    }

    $sumSquares = 0.0
    foreach ($value in $sorted) {
        $sumSquares += [math]::Pow(($value - $mean), 2)
    }

    $stdDev = 0.0
    $ci95 = 0.0
    if ($count -gt 1) {
        $variance = $sumSquares / ($count - 1)
        $stdDev = [math]::Sqrt($variance)
        $ci95 = 1.96 * ($stdDev / [math]::Sqrt($count))
    }

    return [pscustomobject]@{
        count = $count
        mean = [math]::Round($mean, 3)
        median = [math]::Round($median, 3)
        min = [math]::Round($min, 3)
        max = [math]::Round($max, 3)
        std_dev = [math]::Round($stdDev, 3)
        ci95 = [math]::Round($ci95, 3)
    }
}