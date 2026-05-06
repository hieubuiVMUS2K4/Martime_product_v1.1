param(
    [Parameter(Mandatory = $true)][string]$ScenarioName,
    [Parameter(Mandatory = $true)][ValidateSet('LAN', '4G', 'VSAT', 'LEO', 'HF')][string]$NetworkProfile,
    [Parameter(Mandatory = $true)][int]$RecordCount,
    [ValidateRange(1, 200)][int]$SimulatedNodeCount = 1,
    [int]$Repetitions = 1,
    [string]$EdgeBaseUrl = 'http://localhost:5001',
    [string]$InternalApiKey,
    [string]$EdgeAccessToken,
    [string]$EdgeLoginUsername = 'admin',
    [string]$EdgeLoginPassword = 'Admin@2026',
    [string]$EdgeDeviceType = 'BRIDGE_PC',
    [string]$ArtifactsRoot = '.\artifacts\research-sync',
    [string]$EdgeContainerName = 'maritime-edge-postgres',
    [string]$EdgeDatabase = 'maritime_edge',
    [string]$EdgeDatabaseUser = 'edge_user',
    [string[]]$StatsContainers = @('maritime-edge-postgres', 'maritime-edge-collector', 'shore_product-postgres-1', 'shore_product-backend-1'),
    [switch]$ApplyNetworkProfile,
    [switch]$ResetPendingQueue,
    [int]$MaxTriggerIterations = 300,
    [int]$PollSeconds = 2,
    [int]$HeartbeatSeconds = 15,
    [int]$HeartbeatIterations = 10,
    [bool]$RequireProgressForRetry = $true,
    [int]$NoProgressIterationLimit = 25,
    [int]$NoProgressSecondsLimit = 180,
    [int]$ScenarioTimeoutSeconds = 1800
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

if ($Repetitions -le 0) {
    throw 'Repetitions must be greater than zero.'
}

$scenarioRoot = New-ResearchDirectory -Path (Join-Path $ArtifactsRoot ("{0}-{1}-{2}" -f $ScenarioName, $NetworkProfile, ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss'))))
$summaryRows = New-Object System.Collections.Generic.List[object]
$scenarioProgressPath = Join-Path $scenarioRoot 'scenario-progress.json'

function Update-ScenarioProgressState {
    param(
        [Parameter(Mandatory = $true)][string]$Status,
        [int]$Repetition,
        [string]$RepetitionDir,
        [object]$PendingBefore,
        [object]$PendingCurrent,
        [int]$Iteration = 0,
        [double]$ElapsedSeconds = 0,
        [double]$LastTriggerLatencyMs,
        [object]$TriggerResponse,
        [string]$Note,
        [string]$FailureReason
    )

    $progressPercent = $null
    if ($PendingBefore -is [int] -and $PendingBefore -gt 0 -and $PendingCurrent -is [int]) {
        $progressPercent = [math]::Round((($PendingBefore - $PendingCurrent) / $PendingBefore) * 100, 2)
        if ($progressPercent -lt 0) {
            $progressPercent = 0
        }
    }

    $state = [ordered]@{
        scenario = $ScenarioName
        network = $NetworkProfile
        record_count = $RecordCount
        simulated_node_count = $SimulatedNodeCount
        repetitions = $Repetitions
        current_repetition = $Repetition
        status = $Status
        iteration = $Iteration
        max_trigger_iterations = $MaxTriggerIterations
        require_progress_for_retry = $RequireProgressForRetry
        no_progress_iteration_limit = $NoProgressIterationLimit
        no_progress_seconds_limit = $NoProgressSecondsLimit
        poll_seconds = $PollSeconds
        heartbeat_seconds = $HeartbeatSeconds
        scenario_timeout_seconds = $ScenarioTimeoutSeconds
        pending_before = $PendingBefore
        pending_current = $PendingCurrent
        progress_percent = $progressPercent
        elapsed_seconds = [math]::Round($ElapsedSeconds, 3)
        last_trigger_latency_ms = $LastTriggerLatencyMs
        scenario_root = $scenarioRoot
        repetition_dir = $RepetitionDir
        updated_at_utc = [DateTime]::UtcNow.ToString('o')
    }

    if (-not [string]::IsNullOrWhiteSpace($Note)) {
        $state.note = $Note
    }

    if (-not [string]::IsNullOrWhiteSpace($FailureReason)) {
        $state.failure_reason = $FailureReason
    }

    if ($null -ne $TriggerResponse) {
        if ($TriggerResponse.PSObject.Properties['totalSynced']) {
            $state.last_total_synced = $TriggerResponse.totalSynced
        }

        if ($TriggerResponse.PSObject.Properties['pendingRecords']) {
            $state.last_reported_pending_records = $TriggerResponse.pendingRecords
        }

        if ($TriggerResponse.PSObject.Properties['message']) {
            $state.last_message = $TriggerResponse.message
        }
    }

    $progressObject = [pscustomobject]$state
    Write-ResearchJsonFile -Path $scenarioProgressPath -Data $progressObject

    if (-not [string]::IsNullOrWhiteSpace($RepetitionDir)) {
        Write-ResearchJsonFile -Path (Join-Path $RepetitionDir 'progress.json') -Data $progressObject
    }
}

function Get-BenchmarkSyncMetrics {
    param([Parameter(Mandatory = $true)][string]$BenchmarkRunId)

    $benchmarkSql = @"
SELECT row_to_json(t)
FROM (
    SELECT
        COUNT(*)::int AS queued_records,
        COUNT(*) FILTER (WHERE synced_at IS NOT NULL)::int AS synced_records,
        COUNT(*) FILTER (WHERE synced_at IS NULL)::int AS pending_records,
        COUNT(*) FILTER (WHERE retry_count > 0)::int AS retried_records,
        COALESCE(SUM(retry_count), 0)::int AS retry_count_total
    FROM sync_queue
    WHERE payload LIKE '%BENCH-$BenchmarkRunId-%'
) t;
"@

    return Invoke-PostgresJsonQuery -ContainerName $EdgeContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -Sql $benchmarkSql
}

function Get-BenchmarkRetryCollisionMetrics {
    param([Parameter(Mandatory = $true)][string]$BenchmarkRunId)

    $collisionSql = @"
WITH retry_rows AS (
    SELECT date_trunc('second', COALESCE(next_retry_at, created_at)) AS retry_second
    FROM sync_queue
    WHERE payload LIKE '%BENCH-$BenchmarkRunId-%'
      AND retry_count > 0
),
buckets AS (
    SELECT retry_second, COUNT(*)::int AS bucket_count
    FROM retry_rows
    GROUP BY retry_second
),
aggregated AS (
    SELECT
        COALESCE((SELECT COUNT(*) FROM retry_rows), 0)::int AS retried_records,
        COALESCE((SELECT SUM(bucket_count) FROM buckets WHERE bucket_count > 1), 0)::int AS collided_records,
        COALESCE((SELECT MAX(bucket_count) FROM buckets), 0)::int AS peak_retry_bucket
)
SELECT row_to_json(t)
FROM (
    SELECT
        retried_records,
        collided_records,
        peak_retry_bucket,
        CASE
            WHEN retried_records = 0 THEN 0
            ELSE ROUND((collided_records::numeric / retried_records::numeric) * 100, 3)
        END AS retry_collision_rate_pct
    FROM aggregated
) t;
"@

    return Invoke-PostgresJsonQuery -ContainerName $EdgeContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -Sql $collisionSql
}

function Get-PeakRequestRate {
    param([Parameter(Mandatory = $true)][object[]]$DetailRows)

    if ($DetailRows.Count -eq 0) {
        return 0
    }

    $buckets = @{}
    foreach ($row in $DetailRows) {
        $key = ([DateTime]$row.timestamp_utc).ToString('yyyy-MM-ddTHH:mm:ss')
        if (-not $buckets.ContainsKey($key)) {
            $buckets[$key] = 0
        }

        $buckets[$key] += 1
    }

    return ($buckets.Values | Measure-Object -Maximum).Maximum
}

Write-ResearchLog -Message "Scenario root: $scenarioRoot"
Update-ScenarioProgressState -Status 'initializing' -Repetition 0 -PendingBefore $null -PendingCurrent $null -Note 'Scenario created.'

if ([string]::IsNullOrWhiteSpace($EdgeAccessToken)) {
    Write-ResearchLog -Message "No Edge access token provided. Logging in as $EdgeLoginUsername" -Level WARN
    $EdgeAccessToken = Get-EdgeAccessToken -BaseUrl $EdgeBaseUrl -Username $EdgeLoginUsername -Password $EdgeLoginPassword -DeviceType $EdgeDeviceType
}

Update-ScenarioProgressState -Status 'authenticated' -Repetition 0 -PendingBefore $null -PendingCurrent $null -Note 'Edge access token is ready.'

try {
    $null = Get-EdgeSyncStatus -BaseUrl $EdgeBaseUrl -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
}
catch {
    throw "Unable to reach Edge sync status endpoint at $EdgeBaseUrl/api/sync/status. $_"
}

if ($ApplyNetworkProfile.IsPresent) {
    & "$PSScriptRoot\Set-ToxiproxyProfile.ps1" -Profile $NetworkProfile
}

for ($rep = 1; $rep -le $Repetitions; $rep++) {
    $runId = '{0}-{1:d2}' -f ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')), $rep
    $repetitionDir = New-ResearchDirectory -Path (Join-Path $scenarioRoot ("run-$rep"))
    $sqlPath = Join-Path $repetitionDir 'seed.sql'
    $detailRows = New-Object System.Collections.Generic.List[object]

    Write-ResearchLog -Message "Starting repetition $rep/$Repetitions for $ScenarioName"
    Update-ScenarioProgressState -Status 'seeding' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $null -PendingCurrent $null -Note 'Generating and applying benchmark seed.'

    & "$PSScriptRoot\New-ResearchSyncQueueSeed.ps1" `
        -RecordCount ($RecordCount * $SimulatedNodeCount) `
        -Priority 2 `
        -RunId $runId `
        -OutputSqlPath $sqlPath `
        -EdgeContainerName $EdgeContainerName `
        -EdgeDatabase $EdgeDatabase `
        -EdgeUsername $EdgeDatabaseUser `
        -Apply `
        -SeedCrewIfEmpty `
        -ResetPendingQueue:$ResetPendingQueue.IsPresent

    $initialStatus = Get-EdgeSyncStatus -BaseUrl $EdgeBaseUrl -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
    $initialBenchmarkMetrics = Get-BenchmarkSyncMetrics -BenchmarkRunId $runId
    $globalPendingBefore = [int]$initialStatus.pendingRecords
    $pendingBefore = [int]$initialBenchmarkMetrics.pending_records
    $startUtc = [DateTime]::UtcNow
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $iteration = 0
    $pendingNow = $pendingBefore
    $benchmarkSyncedNow = [int]$initialBenchmarkMetrics.synced_records
    $lastHeartbeatUtc = [DateTime]::UtcNow
    $lastTriggerLatencyMs = $null
    $lastTriggerResponse = $null
    $repetitionFailureReason = $null
    $repetitionFailureNote = $null
    $bestPendingSeen = $pendingBefore
    $bestSyncedSeen = $benchmarkSyncedNow
    $lastProgressUtc = [DateTime]::UtcNow
    $iterationsSinceProgress = 0
    Update-ScenarioProgressState -Status 'draining' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingBefore -Iteration 0 -ElapsedSeconds 0 -Note 'Seed applied. Waiting for queue to drain.'

    while ($true) {
        if ($stopwatch.Elapsed.TotalSeconds -ge $ScenarioTimeoutSeconds) {
            Write-ResearchLog -Level WARN -Message "Scenario timeout reached for repetition $rep"
            Update-ScenarioProgressState -Status 'timeout' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingNow -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $lastTriggerResponse -FailureReason 'scenario-timeout' -Note 'Scenario timed out before queue drained.'
            break
        }

        $currentStatus = Get-EdgeSyncStatus -BaseUrl $EdgeBaseUrl -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
        $currentBenchmarkMetrics = Get-BenchmarkSyncMetrics -BenchmarkRunId $runId
        $pendingNow = [int]$currentBenchmarkMetrics.pending_records
        $benchmarkSyncedNow = [int]$currentBenchmarkMetrics.synced_records
        $globalPendingNow = [int]$currentStatus.pendingRecords
        if ($pendingNow -le 0) {
            Update-ScenarioProgressState -Status 'drained' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingNow -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $lastTriggerResponse -Note 'Queue drained successfully.'
            break
        }

        $iteration++
        if ($iteration -gt $MaxTriggerIterations) {
            Write-ResearchLog -Level WARN -Message "MaxTriggerIterations reached for repetition $rep"
            Update-ScenarioProgressState -Status 'max-iterations' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingNow -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $lastTriggerResponse -FailureReason 'max-trigger-iterations' -Note 'Trigger loop hit MaxTriggerIterations.'
            break
        }

        try {
            $triggerWatch = [System.Diagnostics.Stopwatch]::StartNew()
            $triggerResponse = Invoke-EdgeApi -Method POST -BaseUrl $EdgeBaseUrl -Path 'api/sync/trigger' -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
            $triggerWatch.Stop()
            $lastTriggerLatencyMs = [math]::Round($triggerWatch.Elapsed.TotalMilliseconds, 3)
            $lastTriggerResponse = $triggerResponse

            Start-Sleep -Seconds $PollSeconds
            $statusAfterTrigger = Get-EdgeSyncStatus -BaseUrl $EdgeBaseUrl -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
            $benchmarkMetricsAfterTrigger = Get-BenchmarkSyncMetrics -BenchmarkRunId $runId
            $pendingAfter = [int]$benchmarkMetricsAfterTrigger.pending_records
            $benchmarkSyncedAfter = [int]$benchmarkMetricsAfterTrigger.synced_records
            $globalPendingAfter = [int]$statusAfterTrigger.pendingRecords
        }
        catch {
            $lastTriggerLatencyMs = if ($null -ne $triggerWatch) { [math]::Round($triggerWatch.Elapsed.TotalMilliseconds, 3) } else { $null }
            $repetitionFailureReason = 'trigger-or-status-error'
            $repetitionFailureNote = $_.Exception.Message
            Write-ResearchLog -Level WARN -Message "Trigger/status failed for repetition ${rep}: $repetitionFailureNote"
            Update-ScenarioProgressState -Status 'trigger-error' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingNow -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $lastTriggerResponse -FailureReason $repetitionFailureReason -Note $repetitionFailureNote
            break
        }

        $madeProgress = $false
        if ($pendingAfter -lt $bestPendingSeen) {
            $bestPendingSeen = $pendingAfter
            $madeProgress = $true
        }

        if ($benchmarkSyncedAfter -gt $bestSyncedSeen) {
            $bestSyncedSeen = $benchmarkSyncedAfter
            $madeProgress = $true
        }

        if ($null -ne $triggerResponse -and $triggerResponse.PSObject.Properties['totalSynced'] -and [int]$triggerResponse.totalSynced -gt 0) {
            $madeProgress = $true
        }

        if ($madeProgress) {
            $lastProgressUtc = [DateTime]::UtcNow
            $iterationsSinceProgress = 0
        }
        else {
            $iterationsSinceProgress++
        }

        $detailRows.Add([pscustomobject]@{
            scenario = $ScenarioName
            repetition = $rep
            iteration = $iteration
            timestamp_utc = [DateTime]::UtcNow.ToString('o')
            pending_before = $pendingNow
            pending_after = $pendingAfter
            benchmark_synced_before = $benchmarkSyncedNow
            benchmark_synced_after = $benchmarkSyncedAfter
            global_pending_before = $globalPendingNow
            global_pending_after = $globalPendingAfter
            progress_detected = $madeProgress
            trigger_latency_ms = $lastTriggerLatencyMs
            response_json = ($triggerResponse | ConvertTo-Json -Depth 10 -Compress)
        })

        if ($RequireProgressForRetry -and -not $madeProgress) {
            Write-ResearchLog -Level WARN -Message "Retry stopped because benchmark queue made no progress for repetition $rep"
            Update-ScenarioProgressState -Status 'stalled' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingAfter -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $triggerResponse -FailureReason 'no-benchmark-progress' -Note 'Previous trigger did not reduce benchmark pending queue, so no further retry was attempted.'
            break
        }

        if ($NoProgressIterationLimit -gt 0 -and $iterationsSinceProgress -ge $NoProgressIterationLimit) {
            Write-ResearchLog -Level WARN -Message "No progress iteration limit reached for repetition $rep"
            Update-ScenarioProgressState -Status 'stalled' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingAfter -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $triggerResponse -FailureReason 'no-progress-iterations' -Note 'Queue did not show measurable progress for too many trigger loops.'
            break
        }

        if ($NoProgressSecondsLimit -gt 0 -and (([DateTime]::UtcNow - $lastProgressUtc).TotalSeconds -ge $NoProgressSecondsLimit)) {
            Write-ResearchLog -Level WARN -Message "No progress time limit reached for repetition $rep"
            Update-ScenarioProgressState -Status 'stalled' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingAfter -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $triggerResponse -FailureReason 'no-progress-time' -Note 'Queue did not show measurable progress for too much wall time.'
            break
        }

        $shouldWriteHeartbeat = $false
        if ($HeartbeatIterations -gt 0 -and (($iteration % $HeartbeatIterations) -eq 0)) {
            $shouldWriteHeartbeat = $true
        }

        if (([DateTime]::UtcNow - $lastHeartbeatUtc).TotalSeconds -ge $HeartbeatSeconds) {
            $shouldWriteHeartbeat = $true
        }

        if ($shouldWriteHeartbeat) {
            Update-ScenarioProgressState -Status 'draining' -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingAfter -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $lastTriggerLatencyMs -TriggerResponse $triggerResponse -Note 'Queue draining in progress.'
            Write-ResearchLog -Message ("Progress {0} rep {1}/{2}: iteration={3}/{4} benchmark_pending={5}->{6} benchmark_synced={7}->{8} global_pending={9}->{10} elapsed_s={11} last_trigger_ms={12}" -f $ScenarioName, $rep, $Repetitions, $iteration, $MaxTriggerIterations, $pendingNow, $pendingAfter, $benchmarkSyncedNow, $benchmarkSyncedAfter, $globalPendingNow, $globalPendingAfter, [math]::Round($stopwatch.Elapsed.TotalSeconds, 1), $lastTriggerLatencyMs)
            $lastHeartbeatUtc = [DateTime]::UtcNow
        }

        $benchmarkSyncedNow = $benchmarkSyncedAfter
    }

    $stopwatch.Stop()
    $finalStatus = Get-EdgeSyncStatus -BaseUrl $EdgeBaseUrl -InternalApiKey $InternalApiKey -AccessToken $EdgeAccessToken
    $benchmarkMetrics = Get-BenchmarkSyncMetrics -BenchmarkRunId $runId
    $collisionMetrics = Get-BenchmarkRetryCollisionMetrics -BenchmarkRunId $runId
    $pendingFinal = [int]$benchmarkMetrics.pending_records
    $statsSnapshot = Get-DockerStatsSnapshot -ContainerNames $StatsContainers
    $statsMetrics = Convert-DockerStatsSnapshotToMetrics -StatsSnapshot $statsSnapshot
    $globalPendingFinal = [int]$finalStatus.pendingRecords
    $avgTriggerLatency = $null
    $maxTriggerLatency = $null
    if ($detailRows.Count -gt 0) {
        $avgTriggerLatency = [math]::Round((($detailRows | Measure-Object -Property trigger_latency_ms -Average).Average), 3)
        $maxTriggerLatency = [math]::Round((($detailRows | Measure-Object -Property trigger_latency_ms -Maximum).Maximum), 3)
    }
    $peakRequestRateRps = Get-PeakRequestRate -DetailRows ($detailRows.ToArray())
    $shoreErrorRatePct = 0
    if ([int]$benchmarkMetrics.queued_records -gt 0) {
        $shoreErrorRatePct = [math]::Round((([int]$benchmarkMetrics.queued_records - [int]$benchmarkMetrics.synced_records) / [double][int]$benchmarkMetrics.queued_records) * 100, 3)
        if ($shoreErrorRatePct -lt 0) {
            $shoreErrorRatePct = 0
        }
    }

    $primaryContainerMetrics = $statsMetrics | Where-Object {
        $metricName = $null
        if ($_.PSObject.Properties['name']) {
            $metricName = $_.name
        }
        elseif ($_.PSObject.Properties['Name']) {
            $metricName = $_.Name
        }

        $metricName -eq $EdgeContainerName
    } | Select-Object -First 1
    if ($null -eq $primaryContainerMetrics) {
        $primaryContainerMetrics = $statsMetrics | Select-Object -First 1
    }

    $detailPath = Join-Path $repetitionDir 'trigger-timeline.csv'
    $detailRows | Export-Csv -LiteralPath $detailPath -NoTypeInformation -Encoding UTF8

    $statsPath = Join-Path $repetitionDir 'docker-stats.json'
    $statsSnapshot | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $statsPath -Encoding UTF8

    $summary = [pscustomobject]@{
        scenario = $ScenarioName
        network = $NetworkProfile
        repetition = $rep
        record_count = $RecordCount
        simulated_node_count = $SimulatedNodeCount
        pending_before = $pendingBefore
        pending_after = $pendingFinal
        global_pending_before = $globalPendingBefore
        global_pending_after = $globalPendingFinal
        trigger_iterations = $iteration
        queue_drain_seconds = [math]::Round($stopwatch.Elapsed.TotalSeconds, 3)
        avg_trigger_latency_ms = $avgTriggerLatency
        max_trigger_latency_ms = $maxTriggerLatency
        queued_records = $benchmarkMetrics.queued_records
        synced_records = $benchmarkMetrics.synced_records
        benchmark_pending_records = $benchmarkMetrics.pending_records
        retried_records = $benchmarkMetrics.retried_records
        retry_count_total = $benchmarkMetrics.retry_count_total
        retry_collision_rate_pct = if ($null -ne $collisionMetrics) { $collisionMetrics.retry_collision_rate_pct } else { 0 }
        peak_retry_bucket = if ($null -ne $collisionMetrics) { $collisionMetrics.peak_retry_bucket } else { 0 }
        peak_request_rate_rps = $peakRequestRateRps
        shore_error_rate_pct = $shoreErrorRatePct
        primary_cpu_percent = if ($null -ne $primaryContainerMetrics) { $primaryContainerMetrics.cpu_percent } else { $null }
        primary_memory_bytes = if ($null -ne $primaryContainerMetrics) { $primaryContainerMetrics.memory_usage_bytes } else { $null }
        success = ($pendingFinal -eq 0)
        failure_reason = $repetitionFailureReason
        failure_note = $repetitionFailureNote
        started_at_utc = $startUtc.ToString('o')
        finished_at_utc = [DateTime]::UtcNow.ToString('o')
    }

    $summaryRows.Add($summary)
    ($summary | ConvertTo-Json -Depth 10) | Set-Content -LiteralPath (Join-Path $repetitionDir 'summary.json') -Encoding UTF8
    $finalStatusName = if ($summary.success) { 'completed' } else { 'failed' }
    $finalNote = if ($summary.success) { 'Repetition completed successfully.' } elseif (-not [string]::IsNullOrWhiteSpace($repetitionFailureNote)) { $repetitionFailureNote } else { 'Repetition ended with pending records remaining.' }
    Update-ScenarioProgressState -Status $finalStatusName -Repetition $rep -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingFinal -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $avgTriggerLatency -TriggerResponse $lastTriggerResponse -FailureReason $repetitionFailureReason -Note $finalNote
    Write-ResearchLog -Message "Finished repetition $rep/$Repetitions. pending_after=$pendingFinal drain_s=$($summary.queue_drain_seconds)"
}

$summaryPath = Join-Path $scenarioRoot 'scenario-summary.csv'
$summaryRows | Export-Csv -LiteralPath $summaryPath -NoTypeInformation -Encoding UTF8
Update-ScenarioProgressState -Status 'scenario-complete' -Repetition $Repetitions -RepetitionDir $repetitionDir -PendingBefore $pendingBefore -PendingCurrent $pendingFinal -Iteration $iteration -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds -LastTriggerLatencyMs $avgTriggerLatency -TriggerResponse $lastTriggerResponse -Note 'Scenario summary file written.'
Write-ResearchLog -Message "Scenario summary written to $summaryPath"