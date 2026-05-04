param(
	[string]$EdgeBaseUrl = 'http://localhost:5001',
	[string]$EdgeLoginUsername = 'admin',
	[string]$EdgeLoginPassword = 'Admin@2026',
	[string]$EdgeDeviceType = 'BRIDGE_PC',
	[string]$EdgeDbContainerName = 'maritime-edge-postgres',
	[string]$EdgeDatabase = 'maritime_edge',
	[string]$EdgeDatabaseUser = 'edge_user',
	[string]$ShoreDbContainerName = 'shore_product-postgres-1',
	[string]$ShoreDatabase = 'productdb',
	[string]$ShoreDatabaseUser = 'product',
	[string]$EdgeServiceContainerName = 'maritime-edge-services',
	[int]$PollTimeoutSeconds = 900,
	[int]$OfflineDurationSeconds = 20,
	[switch]$ApplyNetworkProfile,
	[string]$OutputDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Net.Http

. "$PSScriptRoot\ResearchSync.Common.ps1"

function Get-JsonHeaders {
	param([string]$AccessToken)

	return @{ Authorization = "Bearer $AccessToken" }
}

function Get-ContentTypeForPath {
	param([Parameter(Mandatory = $true)][string]$Path)

	switch ([System.IO.Path]::GetExtension($Path).ToLowerInvariant()) {
		'.pdf' { return 'application/pdf' }
		default { return 'application/octet-stream' }
	}
}

function Invoke-MultipartEdgeRequest {
	param(
		[Parameter(Mandatory = $true)][ValidateSet('POST', 'PUT')][string]$Method,
		[Parameter(Mandatory = $true)][string]$Uri,
		[Parameter(Mandatory = $true)][string]$AccessToken,
		[Parameter(Mandatory = $true)][hashtable]$Fields,
		[string]$FileFieldName,
		[string]$FilePath
	)

	$client = [System.Net.Http.HttpClient]::new()
	try {
		$client.Timeout = [TimeSpan]::FromMinutes(10)
		$client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $AccessToken)

		$content = [System.Net.Http.MultipartFormDataContent]::new()
		foreach ($key in $Fields.Keys) {
			$value = $Fields[$key]
			if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
				continue
			}

			$content.Add([System.Net.Http.StringContent]::new([string]$value), $key)
		}

		if (-not [string]::IsNullOrWhiteSpace($FileFieldName) -and -not [string]::IsNullOrWhiteSpace($FilePath)) {
			$bytes = [System.IO.File]::ReadAllBytes($FilePath)
			$fileContent = [System.Net.Http.ByteArrayContent]::new($bytes)
			$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse((Get-ContentTypeForPath -Path $FilePath))
			$content.Add($fileContent, $FileFieldName, [System.IO.Path]::GetFileName($FilePath))
		}

		$request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::$Method, $Uri)
		$request.Content = $content
		$response = $client.SendAsync($request).GetAwaiter().GetResult()
		$body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
		if (-not $response.IsSuccessStatusCode) {
			throw "HTTP $([int]$response.StatusCode) calling $Uri`n$body"
		}

		if ([string]::IsNullOrWhiteSpace($body)) {
			return $null
		}

		return $body | ConvertFrom-Json
	}
	finally {
		$client.Dispose()
	}
}

function New-LargeBinaryPdfFile {
	param(
		[Parameter(Mandatory = $true)][string]$Path,
		[Parameter(Mandatory = $true)][int]$SizeMb,
		[Parameter(Mandatory = $true)][string]$Label
	)

	$targetBytes = $SizeMb * 1024 * 1024
	$chunkBytes = 1024 * 1024
	$stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
	try {
		$header = [System.Text.Encoding]::ASCII.GetBytes("%PDF-1.4`n% $Label`n")
		$stream.Write($header, 0, $header.Length)

		$written = $header.Length
		$block = New-Object byte[] $chunkBytes
		$rnd = [System.Random]::new($Label.GetHashCode())
		while ($written -lt $targetBytes) {
			$rnd.NextBytes($block)
			$remaining = $targetBytes - $written
			$toWrite = [Math]::Min($chunkBytes, $remaining)
			$stream.Write($block, 0, $toWrite)
			$written += $toWrite
		}
	}
	finally {
		$stream.Dispose()
	}
}

function Get-CrewSample {
	param([string]$AccessToken)

	$response = Invoke-RestMethod -Method Get -Uri "$EdgeBaseUrl/api/crew?page=1&pageSize=1" -Headers (Get-JsonHeaders -AccessToken $AccessToken)
	return $response.data | Select-Object -First 1
}

function Get-ResumeMetrics {
	param([Parameter(Mandatory = $true)][string]$RecordKey)

	$sql = @"
SELECT row_to_json(t)
FROM (
	WITH sessions AS (
		SELECT
			COUNT(*)::int AS session_count,
			COALESCE(SUM("CommittedBytes"), 0)::bigint AS committed_bytes_total,
			COALESCE(MAX("SizeBytes"), 0)::bigint AS file_size_bytes,
			COALESCE(MAX("NextChunkIndex"), 0)::int AS max_next_chunk_index
		FROM sync_file_chunk_sessions
		WHERE "TableName" = 'travel_document'
		  AND "RecordKey" = '$RecordKey'
		  AND "Direction" = 'upload'
	)
	SELECT
		session_count,
		committed_bytes_total,
		file_size_bytes,
		max_next_chunk_index,
		CASE
			WHEN file_size_bytes = 0 THEN 0
			ELSE GREATEST(0, committed_bytes_total - file_size_bytes)
		END AS retransmitted_bytes,
		CASE
			WHEN file_size_bytes = 0 THEN 0
			ELSE ROUND((GREATEST(0, committed_bytes_total - file_size_bytes)::numeric / file_size_bytes::numeric) * 100, 3)
		END AS retransmitted_ratio_pct,
		CASE
			WHEN file_size_bytes = 0 THEN 1
			ELSE ROUND(1 - (GREATEST(0, committed_bytes_total - file_size_bytes)::numeric / file_size_bytes::numeric), 6)
		END AS resume_efficiency
	FROM sessions
) t;
"@

	return Invoke-PostgresJsonQuery -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -Sql $sql
}

function Wait-ForTravelDocumentSynced {
	param([Parameter(Mandatory = $true)][string]$RecordKey)

	$deadline = (Get-Date).AddSeconds($PollTimeoutSeconds)
	do {
		$status = Invoke-RestMethod -Method Get -Uri "$EdgeBaseUrl/api/sync/status" -Headers (Get-JsonHeaders -AccessToken $accessToken)
		if ([int]$status.pendingRecords -eq 0) {
			$metrics = Get-ResumeMetrics -RecordKey $RecordKey
			if ($null -ne $metrics -and [int]$metrics.max_next_chunk_index -gt 0) {
				return $metrics
			}
		}

		Start-Sleep -Seconds 2
	}
	while ((Get-Date) -lt $deadline)

	throw "Travel document $RecordKey was not fully synced within timeout"
}

function Invoke-TriggerSync {
	return Invoke-RestMethod -Method Post -Uri "$EdgeBaseUrl/api/sync/trigger" -Headers (Get-JsonHeaders -AccessToken $accessToken)
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
	$OutputDir = ".\artifacts\research-sync\file-resume-runs\resume-" + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
}

$resolvedOutputDir = New-ResearchDirectory -Path $OutputDir
$generatedDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'generated-files')

Write-ResearchLog -Message "Logging in to Edge at $EdgeBaseUrl as $EdgeLoginUsername"
$accessToken = Get-EdgeAccessToken -BaseUrl $EdgeBaseUrl -Username $EdgeLoginUsername -Password $EdgeLoginPassword -DeviceType $EdgeDeviceType
$crew = Get-CrewSample -AccessToken $accessToken
if ($null -eq $crew) {
	throw 'No crew records available on Edge for resume campaign.'
}

$runTag = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')
$summaryRows = New-Object System.Collections.Generic.List[object]

$scenarios = @(
	[pscustomobject]@{ Name = 'R1-50MB-one-cut'; FileSizeMb = 50; Cuts = 1; RestartEdge = $false },
	[pscustomobject]@{ Name = 'R2-200MB-multi-cut'; FileSizeMb = 200; Cuts = 3; RestartEdge = $false },
	[pscustomobject]@{ Name = 'R3-50MB-edge-restart'; FileSizeMb = 50; Cuts = 1; RestartEdge = $true }
)

foreach ($scenario in $scenarios) {
	Write-ResearchLog -Message "Starting $($scenario.Name)"
	$filePath = Join-Path $generatedDir ("$($scenario.Name)-$runTag.pdf")
	New-LargeBinaryPdfFile -Path $filePath -SizeMb $scenario.FileSizeMb -Label $scenario.Name

	$doc = Invoke-MultipartEdgeRequest -Method POST -Uri "$EdgeBaseUrl/api/crew/$($crew.id)/identity-documents" -AccessToken $accessToken -Fields @{
		TargetTable = 'travel_documents'
		DocumentType = 'ResearchResume'
		DocumentNumber = "RS-RESUME-$($scenario.Name)-$runTag"
		Notes = "Resume benchmark $($scenario.Name)"
	} -FileFieldName 'File' -FilePath $filePath

	$recordKey = [string]$doc.id
	$start = [DateTime]::UtcNow

	if ($ApplyNetworkProfile.IsPresent) {
		& "$PSScriptRoot\Set-ToxiproxyProfile.ps1" -Profile VSAT
	}

	Invoke-TriggerSync | Out-Null

	for ($i = 0; $i -lt $scenario.Cuts; $i++) {
		if ($ApplyNetworkProfile.IsPresent) {
			& "$PSScriptRoot\Set-ToxiproxyProfile.ps1" -Profile OFFLINE
		}

		if ($scenario.RestartEdge -and $i -eq 0) {
			Write-ResearchLog -Message "Restarting edge container $EdgeServiceContainerName for resume validation" -Level WARN
			& docker restart $EdgeServiceContainerName | Out-Null
			Start-Sleep -Seconds 12
			$accessToken = Get-EdgeAccessToken -BaseUrl $EdgeBaseUrl -Username $EdgeLoginUsername -Password $EdgeLoginPassword -DeviceType $EdgeDeviceType
		}
		else {
			Start-Sleep -Seconds $OfflineDurationSeconds
		}

		if ($ApplyNetworkProfile.IsPresent) {
			& "$PSScriptRoot\Set-ToxiproxyProfile.ps1" -Profile VSAT
		}

		Invoke-TriggerSync | Out-Null
	}

	$metrics = Wait-ForTravelDocumentSynced -RecordKey $recordKey
	$elapsed = [math]::Round(([DateTime]::UtcNow - $start).TotalSeconds, 3)

	$summaryRows.Add([pscustomobject]@{
		scenario = $scenario.Name
		file_size_mb = $scenario.FileSizeMb
		interruption_count = $scenario.Cuts
		restarted_edge = $scenario.RestartEdge
		record_key = $recordKey
		time_to_recover_seconds = $elapsed
		committed_bytes_total = $metrics.committed_bytes_total
		file_size_bytes = $metrics.file_size_bytes
		retransmitted_bytes = $metrics.retransmitted_bytes
		bytes_retransmitted_ratio_pct = $metrics.retransmitted_ratio_pct
		resume_efficiency = $metrics.resume_efficiency
		next_chunk_index_final = $metrics.max_next_chunk_index
		completed_at_utc = [DateTime]::UtcNow.ToString('o')
	})
}

$summaryCsv = Join-Path $resolvedOutputDir 'resume-summary.csv'
$summaryJson = Join-Path $resolvedOutputDir 'resume-summary.json'
$summaryRows | Export-Csv -LiteralPath $summaryCsv -NoTypeInformation -Encoding UTF8
Write-ResearchJsonFile -Path $summaryJson -Data $summaryRows

if ($ApplyNetworkProfile.IsPresent) {
	& "$PSScriptRoot\Set-ToxiproxyProfile.ps1" -Profile LAN
}

Write-ResearchLog -Message "Resume campaign complete. CSV: $summaryCsv"
Write-Output ($summaryRows | ConvertTo-Json -Depth 10)
