param(
    [string]$CampaignRoot,
    [string]$ArtifactsRoot = '.\artifacts\research-sync',
    [switch]$Watch,
    [int]$RefreshSeconds = 5
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

function Get-LatestCampaignRoot {
    param([Parameter(Mandatory = $true)][string]$BasePath)

    if (-not (Test-Path -LiteralPath $BasePath)) {
        throw "Artifacts root not found: $BasePath"
    }

    $latestCampaign = Get-ChildItem -LiteralPath $BasePath -Directory |
        Where-Object { $_.Name -like 'campaign-*' } |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if ($null -eq $latestCampaign) {
        throw "No campaign directories found under $BasePath"
    }

    return $latestCampaign.FullName
}

function Get-LatestScenarioRoot {
    param([Parameter(Mandatory = $true)][string]$BasePath)

    if (-not (Test-Path -LiteralPath $BasePath)) {
        throw "Artifacts root not found: $BasePath"
    }

    $latestScenario = Get-ChildItem -LiteralPath $BasePath -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'scenario-progress.json') } |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if ($null -eq $latestScenario) {
        throw "No scenario directories found under $BasePath"
    }

    return $latestScenario.FullName
}

function Resolve-ProgressRoot {
    param([string]$InputPath, [string]$BaseArtifactsRoot)

    $candidatePath = $InputPath
    if ([string]::IsNullOrWhiteSpace($candidatePath)) {
        $candidatePath = $BaseArtifactsRoot
    }

    $resolvedPath = (Resolve-Path -LiteralPath $candidatePath).Path

    if (Test-Path -LiteralPath (Join-Path $resolvedPath 'campaign-progress.json')) {
        return [pscustomobject]@{ kind = 'campaign'; path = $resolvedPath }
    }

    if (Test-Path -LiteralPath (Join-Path $resolvedPath 'scenario-progress.json')) {
        return [pscustomobject]@{ kind = 'scenario'; path = $resolvedPath }
    }

    try {
        return [pscustomobject]@{ kind = 'campaign'; path = (Get-LatestCampaignRoot -BasePath $resolvedPath) }
    }
    catch {
    }

    return [pscustomobject]@{ kind = 'scenario'; path = (Get-LatestScenarioRoot -BasePath $resolvedPath) }
}

function Show-ScenarioProgress {
    param([Parameter(Mandatory = $true)][string]$ScenarioRoot)

    $scenarioProgressPath = Join-Path $ScenarioRoot 'scenario-progress.json'
    if (-not (Test-Path -LiteralPath $scenarioProgressPath)) {
        Write-Host "Scenario root: $ScenarioRoot"
        Write-Host 'Status file not found yet: scenario-progress.json'
        return
    }

    $scenario = Get-Content -LiteralPath $scenarioProgressPath -Raw | ConvertFrom-Json
    Write-Host ("Scenario: {0}" -f $ScenarioRoot)
    Write-Host ("Status: {0}" -f $scenario.status)
    Write-Host ("Network: {0} | Records: {1}" -f $scenario.network, $scenario.record_count)
    Write-Host ("Repetition: {0}/{1}" -f $scenario.current_repetition, $scenario.repetitions)

    if (($scenario.PSObject.Properties['pending_before'] -and $null -ne $scenario.pending_before) -or ($scenario.PSObject.Properties['pending_current'] -and $null -ne $scenario.pending_current)) {
        Write-Host ("Queue: {0} -> {1} ({2}%)" -f $scenario.pending_before, $scenario.pending_current, $scenario.progress_percent)
    }

    if ($scenario.PSObject.Properties['iteration']) {
        Write-Host ("Iterations: {0}/{1}" -f $scenario.iteration, $scenario.max_trigger_iterations)
    }

    if ($scenario.PSObject.Properties['elapsed_seconds']) {
        Write-Host ("Elapsed seconds: {0}" -f $scenario.elapsed_seconds)
    }

    if ($scenario.PSObject.Properties['last_trigger_latency_ms']) {
        Write-Host ("Last trigger latency ms: {0}" -f $scenario.last_trigger_latency_ms)
    }

    if ($scenario.PSObject.Properties['last_total_synced']) {
        Write-Host ("Last totalSynced: {0}" -f $scenario.last_total_synced)
    }

    if ($scenario.PSObject.Properties['note'] -and -not [string]::IsNullOrWhiteSpace($scenario.note)) {
        Write-Host ("Scenario note: {0}" -f $scenario.note)
    }

    if ($scenario.PSObject.Properties['failure_reason'] -and -not [string]::IsNullOrWhiteSpace($scenario.failure_reason)) {
        Write-Host ("Failure reason: {0}" -f $scenario.failure_reason)
    }

    if ($scenario.PSObject.Properties['updated_at_utc'] -and -not [string]::IsNullOrWhiteSpace($scenario.updated_at_utc)) {
        Write-Host ("Updated: {0}" -f $scenario.updated_at_utc)
    }
}

function Show-ResearchSyncProgress {
    param([Parameter(Mandatory = $true)][string]$ResolvedCampaignRoot)

    $campaignProgressPath = Join-Path $ResolvedCampaignRoot 'campaign-progress.json'
    if (-not (Test-Path -LiteralPath $campaignProgressPath)) {
        Write-Host "Campaign root: $ResolvedCampaignRoot"
        Write-Host 'Status file not found yet: campaign-progress.json'
        return
    }

    $campaign = Get-Content -LiteralPath $campaignProgressPath -Raw | ConvertFrom-Json
    Write-Host ("Campaign: {0}" -f $ResolvedCampaignRoot)
    Write-Host ("Status: {0}" -f $campaign.status)
    Write-Host ("Progress: {0}/{1} scenarios ({2}%)" -f $campaign.completed_scenarios, $campaign.total_scenarios, $campaign.progress_percent)

    if ($campaign.PSObject.Properties['current_scenario_name'] -and -not [string]::IsNullOrWhiteSpace($campaign.current_scenario_name)) {
        Write-Host ("Current scenario: {0} [{1}] records={2} repetitions={3}" -f $campaign.current_scenario_name, $campaign.current_profile, $campaign.current_record_count, $campaign.current_repetitions)
    }

    if ($campaign.PSObject.Properties['note'] -and -not [string]::IsNullOrWhiteSpace($campaign.note)) {
        Write-Host ("Campaign note: {0}" -f $campaign.note)
    }

    if ($campaign.PSObject.Properties['updated_at_utc'] -and -not [string]::IsNullOrWhiteSpace($campaign.updated_at_utc)) {
        Write-Host ("Updated: {0}" -f $campaign.updated_at_utc)
    }

    $scenarioProgressPath = $null
    if ($campaign.PSObject.Properties['current_scenario_progress_path'] -and -not [string]::IsNullOrWhiteSpace($campaign.current_scenario_progress_path) -and (Test-Path -LiteralPath $campaign.current_scenario_progress_path)) {
        $scenarioProgressPath = $campaign.current_scenario_progress_path
    }
    elseif ($campaign.PSObject.Properties['current_scenario_path'] -and -not [string]::IsNullOrWhiteSpace($campaign.current_scenario_path)) {
        $candidatePath = Join-Path $campaign.current_scenario_path 'scenario-progress.json'
        if (Test-Path -LiteralPath $candidatePath) {
            $scenarioProgressPath = $candidatePath
        }
    }

    if ($null -ne $scenarioProgressPath) {
        $scenario = Get-Content -LiteralPath $scenarioProgressPath -Raw | ConvertFrom-Json
        Write-Host ''
        Write-Host ("Scenario status: {0}" -f $scenario.status)
        Write-Host ("Repetition: {0}/{1}" -f $scenario.current_repetition, $scenario.repetitions)

        if (($scenario.PSObject.Properties['pending_before'] -and $null -ne $scenario.pending_before) -or ($scenario.PSObject.Properties['pending_current'] -and $null -ne $scenario.pending_current)) {
            Write-Host ("Queue: {0} -> {1} ({2}%)" -f $scenario.pending_before, $scenario.pending_current, $scenario.progress_percent)
        }

        if ($scenario.PSObject.Properties['iteration']) {
            Write-Host ("Iterations: {0}/{1}" -f $scenario.iteration, $scenario.max_trigger_iterations)
        }

        if ($scenario.PSObject.Properties['elapsed_seconds']) {
            Write-Host ("Elapsed seconds: {0}" -f $scenario.elapsed_seconds)
        }

        if ($scenario.PSObject.Properties['last_trigger_latency_ms']) {
            Write-Host ("Last trigger latency ms: {0}" -f $scenario.last_trigger_latency_ms)
        }

        if ($scenario.PSObject.Properties['last_total_synced']) {
            Write-Host ("Last totalSynced: {0}" -f $scenario.last_total_synced)
        }

        if ($scenario.PSObject.Properties['note'] -and -not [string]::IsNullOrWhiteSpace($scenario.note)) {
            Write-Host ("Scenario note: {0}" -f $scenario.note)
        }

        if ($scenario.PSObject.Properties['failure_reason'] -and -not [string]::IsNullOrWhiteSpace($scenario.failure_reason)) {
            Write-Host ("Failure reason: {0}" -f $scenario.failure_reason)
        }

        if ($scenario.PSObject.Properties['updated_at_utc'] -and -not [string]::IsNullOrWhiteSpace($scenario.updated_at_utc)) {
            Write-Host ("Scenario updated: {0}" -f $scenario.updated_at_utc)
        }
    }
}

$progressTarget = Resolve-ProgressRoot -InputPath $CampaignRoot -BaseArtifactsRoot $ArtifactsRoot

if (-not $Watch.IsPresent) {
    if ($progressTarget.kind -eq 'campaign') {
        Show-ResearchSyncProgress -ResolvedCampaignRoot $progressTarget.path
    }
    else {
        Show-ScenarioProgress -ScenarioRoot $progressTarget.path
    }
    return
}

while ($true) {
    Clear-Host
    if ($progressTarget.kind -eq 'campaign') {
        Show-ResearchSyncProgress -ResolvedCampaignRoot $progressTarget.path
    }
    else {
        Show-ScenarioProgress -ScenarioRoot $progressTarget.path
    }
    Start-Sleep -Seconds $RefreshSeconds
}