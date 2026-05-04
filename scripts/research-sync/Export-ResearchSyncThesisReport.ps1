param(
    [Parameter(Mandatory = $true)][string]$CampaignRoot,
    [string]$OutputPath
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

if (-not (Test-Path -LiteralPath $CampaignRoot)) {
    throw "Campaign root not found: $CampaignRoot"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $CampaignRoot 'thesis-summary.csv'
}

$summaryPath = Join-Path $CampaignRoot 'campaign-summary.csv'
if (-not (Test-Path -LiteralPath $summaryPath)) {
    throw "campaign-summary.csv not found under $CampaignRoot"
}

$rows = Import-Csv -LiteralPath $summaryPath
$grouped = $rows | Group-Object network, record_count
$reportRows = New-Object System.Collections.Generic.List[object]

foreach ($group in $grouped) {
    $items = @($group.Group)
    if ($items.Count -eq 0) {
        continue
    }

    $sample = $items[0]
    $repetitions = $items.Count
    $successCount = @($items | Where-Object { [System.Convert]::ToBoolean($_.success) }).Count
    $queueDrain = @($items | ForEach-Object { [double]$_.queue_drain_seconds })
    $avgTrigger = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.avg_trigger_latency_ms) } | ForEach-Object { [double]$_.avg_trigger_latency_ms })
    $maxTrigger = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.max_trigger_latency_ms) } | ForEach-Object { [double]$_.max_trigger_latency_ms })
    $retryTotals = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.retry_count_total) } | ForEach-Object { [int]$_.retry_count_total })
    $retryCollisions = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.retry_collision_rate_pct) } | ForEach-Object { [double]$_.retry_collision_rate_pct })
    $peakRequestRates = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.peak_request_rate_rps) } | ForEach-Object { [double]$_.peak_request_rate_rps })
    $shoreErrorRates = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.shore_error_rate_pct) } | ForEach-Object { [double]$_.shore_error_rate_pct })
    $cpuValues = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.primary_cpu_percent) } | ForEach-Object { [double]$_.primary_cpu_percent })
    $memoryValues = @($items | Where-Object { -not [string]::IsNullOrWhiteSpace($_.primary_memory_bytes) } | ForEach-Object { [double]$_.primary_memory_bytes })

    $queueDrainStats = Get-ResearchSeriesStatistics -Values $queueDrain
    $avgTriggerStats = Get-ResearchSeriesStatistics -Values $avgTrigger
    $maxTriggerStats = Get-ResearchSeriesStatistics -Values $maxTrigger
    $retryStats = Get-ResearchSeriesStatistics -Values ($retryTotals | ForEach-Object { [double]$_ })
    $retryCollisionStats = Get-ResearchSeriesStatistics -Values $retryCollisions
    $peakRequestRateStats = Get-ResearchSeriesStatistics -Values $peakRequestRates
    $shoreErrorRateStats = Get-ResearchSeriesStatistics -Values $shoreErrorRates
    $cpuStats = Get-ResearchSeriesStatistics -Values $cpuValues
    $memoryStats = Get-ResearchSeriesStatistics -Values ($memoryValues | ForEach-Object { ($_ / 1MB) })

    $reportRows.Add([pscustomobject]@{
        scenario_type = 'record-sync'
        network_profile = $sample.network
        record_count = [int]$sample.record_count
        repetitions = $repetitions
        success_rate_pct = [math]::Round((100.0 * $successCount / $repetitions), 2)
        avg_queue_drain_seconds = $queueDrainStats.mean
        p50_queue_drain_seconds = (Get-ResearchPercentile -Values $queueDrain -Percentile 50)
        p95_queue_drain_seconds = (Get-ResearchPercentile -Values $queueDrain -Percentile 95)
        median_queue_drain_seconds = $queueDrainStats.median
        std_queue_drain_seconds = $queueDrainStats.std_dev
        ci95_queue_drain_seconds = $queueDrainStats.ci95
        min_queue_drain_seconds = $queueDrainStats.min
        max_queue_drain_seconds = $queueDrainStats.max
        avg_trigger_latency_ms = $avgTriggerStats.mean
        median_trigger_latency_ms = $avgTriggerStats.median
        std_trigger_latency_ms = $avgTriggerStats.std_dev
        ci95_trigger_latency_ms = $avgTriggerStats.ci95
        max_trigger_latency_ms = $maxTriggerStats.max
        avg_retry_count_total = $retryStats.mean
        median_retry_count_total = $retryStats.median
        std_retry_count_total = $retryStats.std_dev
        ci95_retry_count_total = $retryStats.ci95
        max_retry_count_total = $retryStats.max
        avg_retry_collision_rate_pct = $retryCollisionStats.mean
        p95_retry_collision_rate_pct = (Get-ResearchPercentile -Values $retryCollisions -Percentile 95)
        avg_peak_request_rate_rps = $peakRequestRateStats.mean
        p95_peak_request_rate_rps = (Get-ResearchPercentile -Values $peakRequestRates -Percentile 95)
        avg_shore_error_rate_pct = $shoreErrorRateStats.mean
        p95_shore_error_rate_pct = (Get-ResearchPercentile -Values $shoreErrorRates -Percentile 95)
        avg_primary_cpu_percent = $cpuStats.mean
        median_primary_cpu_percent = $cpuStats.median
        std_primary_cpu_percent = $cpuStats.std_dev
        ci95_primary_cpu_percent = $cpuStats.ci95
        avg_primary_memory_mb = $memoryStats.mean
        median_primary_memory_mb = $memoryStats.median
        std_primary_memory_mb = $memoryStats.std_dev
        ci95_primary_memory_mb = $memoryStats.ci95
    })
}

$resumeSummaryPath = Join-Path $CampaignRoot 'resume-summary.csv'
if (Test-Path -LiteralPath $resumeSummaryPath) {
    $resumeRows = Import-Csv -LiteralPath $resumeSummaryPath
    foreach ($row in $resumeRows) {
        $reportRows.Add([pscustomobject]@{
            scenario_type = 'file-resume'
            network_profile = 'VSAT'
            record_count = [int]$row.file_size_mb
            repetitions = 1
            success_rate_pct = 100
            avg_queue_drain_seconds = [double]$row.time_to_recover_seconds
            p50_queue_drain_seconds = [double]$row.time_to_recover_seconds
            p95_queue_drain_seconds = [double]$row.time_to_recover_seconds
            median_queue_drain_seconds = [double]$row.time_to_recover_seconds
            std_queue_drain_seconds = 0
            ci95_queue_drain_seconds = 0
            min_queue_drain_seconds = [double]$row.time_to_recover_seconds
            max_queue_drain_seconds = [double]$row.time_to_recover_seconds
            avg_trigger_latency_ms = $null
            median_trigger_latency_ms = $null
            std_trigger_latency_ms = $null
            ci95_trigger_latency_ms = $null
            max_trigger_latency_ms = $null
            avg_retry_count_total = $null
            median_retry_count_total = $null
            std_retry_count_total = $null
            ci95_retry_count_total = $null
            max_retry_count_total = $null
            avg_retry_collision_rate_pct = $null
            p95_retry_collision_rate_pct = $null
            avg_peak_request_rate_rps = $null
            p95_peak_request_rate_rps = $null
            avg_shore_error_rate_pct = $null
            p95_shore_error_rate_pct = $null
            avg_primary_cpu_percent = $null
            median_primary_cpu_percent = $null
            std_primary_cpu_percent = $null
            ci95_primary_cpu_percent = $null
            avg_primary_memory_mb = $null
            median_primary_memory_mb = $null
            std_primary_memory_mb = $null
            ci95_primary_memory_mb = $null
            resume_efficiency = [double]$row.resume_efficiency
            bytes_retransmitted_ratio_pct = [double]$row.bytes_retransmitted_ratio_pct
            interruption_count = [int]$row.interruption_count
            scenario_name = $row.scenario
        })
    }
}

$reportRows | Sort-Object network_profile, record_count | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding UTF8
Write-ResearchLog -Message "Thesis summary written to $OutputPath"
