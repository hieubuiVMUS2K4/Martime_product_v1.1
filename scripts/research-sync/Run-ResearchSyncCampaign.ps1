param(
    [ValidateSet('Custom', 'Smoke', 'FullMatrix')][string]$Preset = 'Custom',
    [string[]]$Profiles = @('LAN', '4G', 'VSAT', 'HF'),
    [int[]]$RecordCounts = @(100, 1000, 5000, 10000),
    [int]$Repetitions = 10,
    [int]$LanRepetitions = 10,
    [int]$FourGRepetitions = 10,
    [int]$VsatRepetitions = 15,
    [int]$HfRepetitions = 10,
    [string]$EdgeBaseUrl = 'http://localhost:5001',
    [string]$InternalApiKey,
    [string]$EdgeAccessToken,
    [string]$EdgeLoginUsername = 'admin',
    [string]$EdgeLoginPassword = 'Admin@2026',
    [string]$EdgeDeviceType = 'BRIDGE_PC',
    [string]$ArtifactsRoot = '.\artifacts\research-sync',
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

switch ($Preset) {
    'Smoke' {
        $Profiles = @('LAN')
        $RecordCounts = @(10, 100)
        $Repetitions = 1
        $LanRepetitions = 1
        $FourGRepetitions = 1
        $VsatRepetitions = 1
        $HfRepetitions = 1
    }
    'FullMatrix' {
        $Profiles = @('LAN', '4G', 'VSAT', 'HF')
        $RecordCounts = @(100, 1000, 5000, 10000)
        $LanRepetitions = [math]::Max($LanRepetitions, 10)
        $FourGRepetitions = [math]::Max($FourGRepetitions, 10)
        $VsatRepetitions = [math]::Max($VsatRepetitions, 15)
        $HfRepetitions = [math]::Max($HfRepetitions, 10)
    }
}

if ([string]::IsNullOrWhiteSpace($EdgeAccessToken)) {
    Write-ResearchLog -Message "No Edge access token provided. Logging in as $EdgeLoginUsername" -Level WARN
    $EdgeAccessToken = Get-EdgeAccessToken -BaseUrl $EdgeBaseUrl -Username $EdgeLoginUsername -Password $EdgeLoginPassword -DeviceType $EdgeDeviceType
}

$campaignRoot = New-ResearchDirectory -Path (Join-Path $ArtifactsRoot ("campaign-{0}" -f ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss'))))
$aggregate = New-Object System.Collections.Generic.List[object]
$campaignProgressPath = Join-Path $campaignRoot 'campaign-progress.json'
$totalScenarioCount = @($Profiles).Count * @($RecordCounts).Count
$totalRepetitionCount = 0

foreach ($profile in $Profiles) {
    $profileRepetitionCount = switch ($profile) {
        'LAN' { $LanRepetitions }
        '4G' { $FourGRepetitions }
        'VSAT' { $VsatRepetitions }
        'HF' { $HfRepetitions }
        default { $Repetitions }
    }

    $totalRepetitionCount += ($profileRepetitionCount * @($RecordCounts).Count)
}

function Update-CampaignProgressState {
    param(
        [Parameter(Mandatory = $true)][string]$Status,
        [int]$CompletedScenarios,
        [int]$ScenarioIndex,
        [string]$CurrentProfile,
        [int]$CurrentRecordCount,
        [int]$CurrentRepetitions,
        [string]$CurrentScenarioName,
        [string]$CurrentScenarioPath,
        [string]$Note
    )

    $progressPercent = $null
    if ($totalScenarioCount -gt 0) {
        $progressPercent = [math]::Round(($CompletedScenarios / $totalScenarioCount) * 100, 2)
    }

    $state = [ordered]@{
        status = $Status
        preset = $Preset
        campaign_root = $campaignRoot
        total_profiles = @($Profiles).Count
        total_record_counts = @($RecordCounts).Count
        total_scenarios = $totalScenarioCount
        total_repetitions = $totalRepetitionCount
        completed_scenarios = $CompletedScenarios
        current_scenario_index = $ScenarioIndex
        progress_percent = $progressPercent
        current_profile = $CurrentProfile
        current_record_count = $CurrentRecordCount
        current_repetitions = $CurrentRepetitions
        current_scenario_name = $CurrentScenarioName
        current_scenario_path = $CurrentScenarioPath
        updated_at_utc = [DateTime]::UtcNow.ToString('o')
    }

    if (-not [string]::IsNullOrWhiteSpace($Note)) {
        $state.note = $Note
    }

    if (-not [string]::IsNullOrWhiteSpace($CurrentScenarioPath)) {
        $scenarioProgressPath = Join-Path $CurrentScenarioPath 'scenario-progress.json'
        if (Test-Path -LiteralPath $scenarioProgressPath) {
            $state.current_scenario_progress_path = $scenarioProgressPath
        }
    }

    Write-ResearchJsonFile -Path $campaignProgressPath -Data ([pscustomobject]$state)
}

Write-ResearchLog -Message "Campaign root: $campaignRoot"
Update-CampaignProgressState -Status 'starting' -CompletedScenarios 0 -ScenarioIndex 0 -Note 'Campaign directory created.'

$scenarioIndex = 0
foreach ($profile in $Profiles) {
    $profileRepetitions = switch ($profile) {
        'LAN' { $LanRepetitions }
        '4G' { $FourGRepetitions }
        'VSAT' { $VsatRepetitions }
        'HF' { $HfRepetitions }
        default { $Repetitions }
    }

    foreach ($recordCount in $RecordCounts) {
        $scenarioName = '{0}-{1}' -f $profile, $recordCount
        $scenarioIndex++
        Write-ResearchLog -Message "Running scenario $scenarioName with repetitions=$profileRepetitions"
        Update-CampaignProgressState -Status 'running-scenario' -CompletedScenarios ($scenarioIndex - 1) -ScenarioIndex $scenarioIndex -CurrentProfile $profile -CurrentRecordCount $recordCount -CurrentRepetitions $profileRepetitions -CurrentScenarioName $scenarioName -Note 'Scenario execution started.'

        try {
            & "$PSScriptRoot\Invoke-ResearchSyncScenario.ps1" `
                -ScenarioName $scenarioName `
                -NetworkProfile $profile `
                -RecordCount $recordCount `
                -Repetitions $profileRepetitions `
                -EdgeBaseUrl $EdgeBaseUrl `
                -InternalApiKey $InternalApiKey `
                -EdgeAccessToken $EdgeAccessToken `
                -ArtifactsRoot $campaignRoot `
                -ApplyNetworkProfile:$ApplyNetworkProfile.IsPresent `
                -ResetPendingQueue:$ResetPendingQueue.IsPresent `
                -MaxTriggerIterations $MaxTriggerIterations `
                -PollSeconds $PollSeconds `
                -HeartbeatSeconds $HeartbeatSeconds `
                -HeartbeatIterations $HeartbeatIterations `
                -RequireProgressForRetry:$RequireProgressForRetry `
                -NoProgressIterationLimit $NoProgressIterationLimit `
                -NoProgressSecondsLimit $NoProgressSecondsLimit `
                -ScenarioTimeoutSeconds $ScenarioTimeoutSeconds
        }
        catch {
            Update-CampaignProgressState -Status 'failed' -CompletedScenarios ($scenarioIndex - 1) -ScenarioIndex $scenarioIndex -CurrentProfile $profile -CurrentRecordCount $recordCount -CurrentRepetitions $profileRepetitions -CurrentScenarioName $scenarioName -Note $_.Exception.Message
            throw
        }

        $latestScenarioDir = Get-ChildItem -LiteralPath $campaignRoot -Directory | Where-Object { $_.Name -like "$scenarioName-*" } | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
        $latestScenarioPath = if ($null -ne $latestScenarioDir) { $latestScenarioDir.FullName } else { $null }
        Update-CampaignProgressState -Status 'scenario-finished' -CompletedScenarios $scenarioIndex -ScenarioIndex $scenarioIndex -CurrentProfile $profile -CurrentRecordCount $recordCount -CurrentRepetitions $profileRepetitions -CurrentScenarioName $scenarioName -CurrentScenarioPath $latestScenarioPath -Note 'Scenario execution finished. Aggregating results.'
        if ($null -eq $latestScenarioDir) {
            continue
        }

        $summaryPath = Join-Path $latestScenarioDir.FullName 'scenario-summary.csv'
        if (Test-Path -LiteralPath $summaryPath) {
            foreach ($row in @(Import-Csv -LiteralPath $summaryPath)) {
                $aggregate.Add($row)
            }
        }
    }
}

$aggregatePath = Join-Path $campaignRoot 'campaign-summary.csv'
$aggregate | Export-Csv -LiteralPath $aggregatePath -NoTypeInformation -Encoding UTF8
Write-ResearchLog -Message "Campaign summary written to $aggregatePath"
Update-CampaignProgressState -Status 'exporting' -CompletedScenarios $scenarioIndex -ScenarioIndex $scenarioIndex -Note 'Generating thesis report outputs.'

& "$PSScriptRoot\Export-ResearchSyncThesisReport.ps1" -CampaignRoot $campaignRoot
& "$PSScriptRoot\Export-ResearchSyncVietnameseReport.ps1" -CampaignRoot $campaignRoot
Update-CampaignProgressState -Status 'completed' -CompletedScenarios $scenarioIndex -ScenarioIndex $scenarioIndex -Note 'Campaign completed successfully.'