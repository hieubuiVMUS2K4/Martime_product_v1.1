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
    [int]$PollTimeoutSeconds = 180,
    [string]$OutputDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Net.Http

. "$PSScriptRoot\ResearchSync.Common.ps1"

function Get-JsonHeaders {
    param([string]$AccessToken)

    return @{
        Authorization = "Bearer $AccessToken"
    }
}

function Get-ContentTypeForPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    switch ([System.IO.Path]::GetExtension($Path).ToLowerInvariant()) {
        '.jpg' { return 'image/jpeg' }
        '.jpeg' { return 'image/jpeg' }
        '.png' { return 'image/png' }
        '.gif' { return 'image/gif' }
        '.pdf' { return 'application/pdf' }
        '.txt' { return 'text/plain' }
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
        $client.Timeout = [TimeSpan]::FromMinutes(5)
        $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $AccessToken)

        $content = [System.Net.Http.MultipartFormDataContent]::new()
        foreach ($key in $Fields.Keys) {
            $value = $Fields[$key]
            if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
                continue
            }

            $stringContent = [System.Net.Http.StringContent]::new([string]$value)
            $content.Add($stringContent, $key)
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

function Invoke-ContainerJsonQuery {
    param(
        [Parameter(Mandatory = $true)][string]$ContainerName,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$Sql
    )

    $encodedSql = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($Sql))
    $psqlScript = "printf '%s' '$encodedSql' | base64 -d | psql -U $Username -d $Database -t -A -f -"
    $raw = & docker exec $ContainerName sh -lc $psqlScript
    if ($LASTEXITCODE -ne 0) {
        throw "psql query failed for container $ContainerName"
    }

    $trimmed = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return $null
    }

    return $trimmed | ConvertFrom-Json
}

function Invoke-ContainerScalarQuery {
    param(
        [Parameter(Mandatory = $true)][string]$ContainerName,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$Sql
    )

    $encodedSql = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($Sql))
    $psqlScript = "printf '%s' '$encodedSql' | base64 -d | psql -U $Username -d $Database -t -A -f -"
    $raw = & docker exec $ContainerName sh -lc $psqlScript
    if ($LASTEXITCODE -ne 0) {
        throw "psql scalar query failed for container $ContainerName"
    }

    return ($raw | Out-String).Trim()
}

function Test-UsesShoreQuotedSchema {
    param(
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username
    )

    return $Database -eq 'productdb' -or $Username -eq 'product'
}

function ConvertTo-SqlLiteral {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)

    return $Value.Replace("'", "''")
}

function Get-EdgeCrewSample {
    param([string]$AccessToken, [string]$BaseUrl)

    return Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/crew?page=1&pageSize=1" -Headers (Get-JsonHeaders -AccessToken $AccessToken)
}

function Get-EdgeCertificateSample {
    param([string]$AccessToken, [string]$BaseUrl)

    return Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/certificates" -Headers (Get-JsonHeaders -AccessToken $AccessToken)
}

function Invoke-EdgeTriggerSync {
    param([string]$AccessToken, [string]$BaseUrl)

    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $result = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sync/trigger" -Headers (Get-JsonHeaders -AccessToken $AccessToken)
    $watch.Stop()

    return [pscustomobject]@{
        response = $result
        latencyMs = [math]::Round($watch.Elapsed.TotalMilliseconds, 2)
    }
}

function Wait-ForEdgeQueueIdle {
    param(
        [string]$AccessToken,
        [string]$BaseUrl,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $status = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/sync/status" -Headers (Get-JsonHeaders -AccessToken $AccessToken)
        if ($status.pendingRecords -eq 0) {
            return $status
        }

        Start-Sleep -Seconds 2
    }
    while ((Get-Date) -lt $deadline)

    throw "Edge queue did not drain within $TimeoutSeconds seconds"
}

function Wait-ForEdgeScenarioQueueDrain {
    param(
        [Parameter(Mandatory = $true)][string]$ContainerName,
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Username,
        [Parameter(Mandatory = $true)][string]$TableName,
        [Parameter(Mandatory = $true)][string[]]$RecordKeys,
        [int]$TimeoutSeconds
    )

    $safeTableName = ConvertTo-SqlLiteral -Value $TableName
    $safeRecordKeys = @($RecordKeys | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object {
        "'$(ConvertTo-SqlLiteral -Value $_)'"
    })
    if ($safeRecordKeys.Count -eq 0) {
        throw 'No record keys supplied to Wait-ForEdgeScenarioQueueDrain.'
    }

    $recordList = $safeRecordKeys -join ', '
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $pending = [int](Invoke-ContainerScalarQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql @"
select count(*)
from sync_queue
where table_name = '$safeTableName'
  and record_key in ($recordList)
  and synced_at is null;
"@)

        if ($pending -eq 0) {
            return [pscustomobject]@{
                trackedTable = $TableName
                trackedRecordKeys = @($RecordKeys)
                pendingScenarioRecords = 0
            }
        }

        Start-Sleep -Seconds 2
    }
    while ((Get-Date) -lt $deadline)

    throw "Edge scenario queue did not drain within $TimeoutSeconds seconds for $TableName"
}

function Wait-ForShoreManifest {
    param(
        [string]$ContainerName,
        [string]$Database,
        [string]$Username,
        [string]$TableName,
        [string]$RecordKey,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
        $quotedSchema = Test-UsesShoreQuotedSchema -Database $Database -Username $Username
        $safeTableName = ConvertTo-SqlLiteral -Value $TableName
        $safeRecordKey = ConvertTo-SqlLiteral -Value $RecordKey
        do {
                if ($quotedSchema) {
                        $sql = @"
select row_to_json(t)
from (
    select "Id" as id, "TableName" as table_name, "RecordKey" as record_key, "FileName" as file_name,
                 "SizeBytes" as size_bytes, "OriginalSizeBytes" as original_size_bytes, "Sha256" as sha256,
                 "TransportEncoding" as transport_encoding, "IsPreprocessed" as is_preprocessed,
                 "PreprocessProfile" as preprocess_profile, "TransferStatus" as transfer_status,
                 "StoragePath" as storage_path, "VerifiedAtUtc" as verified_at_utc,
                 "CreatedAt" as created_at, "UpdatedAt" as updated_at
    from sync_file_manifests
    where "TableName" = '$safeTableName' and "RecordKey" = '$safeRecordKey'
    order by "CreatedAt" desc
    limit 1
) t;
"@
                }
                else {
                        $sql = @"
select row_to_json(t)
from (
    select id, table_name, record_key, file_name, size_bytes, original_size_bytes, sha256,
                 transport_encoding, is_preprocessed, preprocess_profile, transfer_status,
                 storage_path, verified_at_utc, created_at, updated_at
    from sync_file_manifests
    where table_name = '$safeTableName' and record_key = '$safeRecordKey'
    order by created_at desc
    limit 1
) t;
"@
                }
        $manifest = Invoke-ContainerJsonQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql $sql
        if ($null -ne $manifest -and $null -ne $manifest.verified_at_utc) {
            return $manifest
        }

        Start-Sleep -Seconds 2
    }
    while ((Get-Date) -lt $deadline)

    throw "Shore manifest $TableName/$RecordKey was not verified within $TimeoutSeconds seconds"
}

function Get-LatestManifestMetrics {
    param(
        [string]$ContainerName,
        [string]$Database,
        [string]$Username,
        [string]$TableName,
        [string]$RecordKey
    )

        $quotedSchema = Test-UsesShoreQuotedSchema -Database $Database -Username $Username
        $safeTableName = ConvertTo-SqlLiteral -Value $TableName
        $safeRecordKey = ConvertTo-SqlLiteral -Value $RecordKey
        if ($quotedSchema) {
                $sql = @"
select row_to_json(t)
from (
    select "Id" as id, "OwnerNodeId" as owner_node_id, "ReceiverNodeId" as receiver_node_id,
                 "TableName" as table_name, "RecordKey" as record_key, "FileRole" as file_role,
                 "FileName" as file_name, "SizeBytes" as size_bytes,
                 "OriginalSizeBytes" as original_size_bytes, "Sha256" as sha256,
                 "TransportEncoding" as transport_encoding, "IsPreprocessed" as is_preprocessed,
                 "PreprocessProfile" as preprocess_profile, "TransferPriority" as transfer_priority,
                 "TransferStatus" as transfer_status, "StoragePath" as storage_path,
                 "VerifiedAtUtc" as verified_at_utc, "CreatedAt" as created_at,
                 "UpdatedAt" as updated_at
    from sync_file_manifests
    where "TableName" = '$safeTableName' and "RecordKey" = '$safeRecordKey'
    order by "CreatedAt" desc
    limit 1
) t;
"@
        }
        else {
                $sql = @"
select row_to_json(t)
from (
    select id, owner_node_id, receiver_node_id, table_name, record_key, file_role, file_name,
                 size_bytes, original_size_bytes, sha256, transport_encoding, is_preprocessed,
                 preprocess_profile, transfer_priority, transfer_status, storage_path,
                 verified_at_utc, created_at, updated_at
    from sync_file_manifests
    where table_name = '$safeTableName' and record_key = '$safeRecordKey'
    order by created_at desc
    limit 1
) t;
"@
        }
    return Invoke-ContainerJsonQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql $sql
}

function Get-LatestTransferRequestMetrics {
    param(
        [string]$ContainerName,
        [string]$Database,
        [string]$Username,
        [string]$TableName,
        [string]$RecordKey
    )

        $quotedSchema = Test-UsesShoreQuotedSchema -Database $Database -Username $Username
        $safeTableName = ConvertTo-SqlLiteral -Value $TableName
        $safeRecordKey = ConvertTo-SqlLiteral -Value $RecordKey
        if ($quotedSchema) {
                $sql = @"
select row_to_json(t)
from (
    select r."Id" as id, r."ManifestId" as manifest_id,
                 r."RequesterNodeId" as requester_node_id, r."SupplierNodeId" as supplier_node_id,
                 r."Status" as status, r."RetryCount" as retry_count,
                 r."PreferDeltaTransfer" as prefer_delta_transfer,
                 r."DeltaBlockSizeBytes" as delta_block_size_bytes,
                 r."ReceiverBaseSha256" as receiver_base_sha256,
                 r."ReceiverBlockHashesJson" as receiver_block_hashes_json,
                 r."RequestedAtUtc" as requested_at_utc, r."CompletedAtUtc" as completed_at_utc
    from sync_file_transfer_requests r
    join sync_file_manifests m on m."Id" = r."ManifestId"
    where m."TableName" = '$safeTableName' and m."RecordKey" = '$safeRecordKey'
    order by r."RequestedAtUtc" desc
    limit 1
) t;
"@
        }
        else {
                $sql = @"
select row_to_json(t)
from (
    select r.id, r.manifest_id, r.requester_node_id, r.supplier_node_id, r.status,
                 r.retry_count, r.prefer_delta_transfer, r.delta_block_size_bytes,
                 r.receiver_base_sha256, r.receiver_block_hashes_json,
                 r.requested_at_utc, r.completed_at_utc
    from sync_file_transfer_requests r
    join sync_file_manifests m on m.id = r.manifest_id
    where m.table_name = '$safeTableName' and m.record_key = '$safeRecordKey'
    order by r.requested_at_utc desc
    limit 1
) t;
"@
        }
    return Invoke-ContainerJsonQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql $sql
}

function Get-LatestChunkSessionMetrics {
    param(
        [string]$ContainerName,
        [string]$Database,
        [string]$Username,
        [string]$TableName,
        [string]$RecordKey
    )

        $quotedSchema = Test-UsesShoreQuotedSchema -Database $Database -Username $Username
        $safeTableName = ConvertTo-SqlLiteral -Value $TableName
        $safeRecordKey = ConvertTo-SqlLiteral -Value $RecordKey
        if ($quotedSchema) {
                $sql = @"
select row_to_json(t)
from (
    select "Id" as id, "RequestId" as request_id, "ManifestId" as manifest_id,
                 "Direction" as direction, "TableName" as table_name, "RecordKey" as record_key,
                 "FileName" as file_name, "SizeBytes" as size_bytes,
                 "ChunkSizeBytes" as chunk_size_bytes, "TotalChunks" as total_chunks,
                 "NextChunkIndex" as next_chunk_index, "CommittedBytes" as committed_bytes,
                 "IsDeltaSession" as is_delta_session,
                 "RequestedChunkIndexesJson" as requested_chunk_indexes_json,
                 "ReceiverBaseSha256" as receiver_base_sha256, "Status" as status,
                 "CreatedAtUtc" as created_at_utc, "CompletedAtUtc" as completed_at_utc
    from sync_file_chunk_sessions
    where "TableName" = '$safeTableName' and "RecordKey" = '$safeRecordKey' and "Direction" = 'upload'
    order by "CreatedAtUtc" desc
    limit 1
) t;
"@
        }
        else {
                $sql = @"
select row_to_json(t)
from (
    select id, request_id, manifest_id, direction, table_name, record_key, file_name,
                 size_bytes, chunk_size_bytes, total_chunks, next_chunk_index, committed_bytes,
                 is_delta_session, requested_chunk_indexes_json, receiver_base_sha256,
                 status, created_at_utc, completed_at_utc
    from sync_file_chunk_sessions
    where table_name = '$safeTableName' and record_key = '$safeRecordKey' and direction = 'upload'
    order by created_at_utc desc
    limit 1
) t;
"@
        }
    return Invoke-ContainerJsonQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql $sql
}

function Get-ChunkSessionCount {
    param(
        [string]$ContainerName,
        [string]$Database,
        [string]$Username,
        [string]$TableName,
        [string[]]$RecordKeys
    )

    if ($RecordKeys.Count -eq 0) {
        return 0
    }

    $quotedSchema = Test-UsesShoreQuotedSchema -Database $Database -Username $Username
    $safeTableName = ConvertTo-SqlLiteral -Value $TableName
    $inList = ($RecordKeys | ForEach-Object { "'$(ConvertTo-SqlLiteral -Value $_)'" }) -join ', '
    if ($quotedSchema) {
        $sql = @"
select count(*)
from sync_file_chunk_sessions
where "TableName" = '$safeTableName' and "RecordKey" in ($inList) and "Direction" = 'upload';
"@
    }
    else {
        $sql = @"
select count(*)
from sync_file_chunk_sessions
where table_name = '$safeTableName' and record_key in ($inList) and direction = 'upload';
"@
    }
    return [int](Invoke-ContainerScalarQuery -ContainerName $ContainerName -Database $Database -Username $Username -Sql $sql)
}

function New-SmallPdfFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label,
        [int]$PaddingBytes = 16384
    )

    $header = [System.Text.Encoding]::ASCII.GetBytes("%PDF-1.4`n1 0 obj`n<< /Type /Catalog /Pages 2 0 R >>`nendobj`n2 0 obj`n<< /Type /Pages /Count 1 /Kids [3 0 R] >>`nendobj`n3 0 obj`n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R >>`nendobj`n4 0 obj`n<< /Length 64 >>`nstream`nBT /F1 14 Tf 36 120 Td ($Label) Tj ET`nendstream`nendobj`n%%EOF`n")
    $padding = New-Object byte[] $PaddingBytes
    [System.Random]::new($Label.GetHashCode()).NextBytes($padding)
    $bytes = New-Object byte[] ($header.Length + $padding.Length)
    [Array]::Copy($header, 0, $bytes, 0, $header.Length)
    [Array]::Copy($padding, 0, $bytes, $header.Length, $padding.Length)
    [System.IO.File]::WriteAllBytes($Path, $bytes)
}

function New-LargeBinaryPdfFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Label,
        [int]$BlockSizeBytes = 262144,
        [int]$BlockCount = 6,
        [int[]]$MutatedBlocks = @()
    )

    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try {
        $header = [System.Text.Encoding]::ASCII.GetBytes("%PDF-1.4`n% $Label`n")
        $stream.Write($header, 0, $header.Length)
        for ($index = 0; $index -lt $BlockCount; $index++) {
            $buffer = New-Object byte[] $BlockSizeBytes
            [System.Random]::new(($Label + ':' + $index).GetHashCode()).NextBytes($buffer)
            if ($MutatedBlocks -contains $index) {
                for ($offset = 0; $offset -lt [Math]::Min(4096, $buffer.Length); $offset++) {
                    $buffer[$offset] = [byte](255 - $buffer[$offset])
                }
            }

            $stream.Write($buffer, 0, $buffer.Length)
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Get-RequestedChunkCount {
    param($ChunkSession)

    if ($null -eq $ChunkSession -or [string]::IsNullOrWhiteSpace([string]$ChunkSession.requested_chunk_indexes_json)) {
        return 0
    }

    return @(($ChunkSession.requested_chunk_indexes_json | ConvertFrom-Json)).Count
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
    $OutputDir = ".\artifacts\research-sync\file-flow-runs\file-flow-$timestamp"
}

$resolvedOutputDir = New-ResearchDirectory -Path $OutputDir
$generatedDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'generated-files')

Write-ResearchLog -Message "Logging in to Edge at $EdgeBaseUrl as $EdgeLoginUsername"
$accessToken = Get-EdgeAccessToken -BaseUrl $EdgeBaseUrl -Username $EdgeLoginUsername -Password $EdgeLoginPassword -DeviceType $EdgeDeviceType

$crewResponse = Get-EdgeCrewSample -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$crew = $crewResponse.data | Select-Object -First 1
if ($null -eq $crew) {
    throw 'No crew records available on Edge for file-flow scenarios.'
}

$certificates = Get-EdgeCertificateSample -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$certificate = $certificates | Select-Object -First 1
if ($null -eq $certificate) {
    throw 'No certificate definitions available on Edge for file-flow scenarios.'
}

$runTag = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')

$largeImageSource = Join-Path (Resolve-Path -LiteralPath '.').Path 'artifacts\research-sync\metadata-first-benchmark-1000\generated-corpus\crew_certificate\0015_cert_31_20260225172956.png'
if (-not (Test-Path -LiteralPath $largeImageSource)) {
    throw "Large image source not found: $largeImageSource"
}

$largeImagePath = Join-Path $generatedDir "large-image-$runTag.png"
Copy-Item -LiteralPath $largeImageSource -Destination $largeImagePath -Force

$smallFilePaths = @()
for ($index = 1; $index -le 3; $index++) {
    $path = Join-Path $generatedDir ("small-bundle-$runTag-$index.pdf")
    New-SmallPdfFile -Path $path -Label "bundle-$runTag-$index" -PaddingBytes (12288 + ($index * 2048))
    $smallFilePaths += $path
}

$deltaV1Path = Join-Path $generatedDir "delta-$runTag-v1.pdf"
$deltaV2Path = Join-Path $generatedDir "delta-$runTag-v2.pdf"
New-LargeBinaryPdfFile -Path $deltaV1Path -Label "delta-$runTag" -BlockCount 6
New-LargeBinaryPdfFile -Path $deltaV2Path -Label "delta-$runTag" -BlockCount 6 -MutatedBlocks @(2)

Write-ResearchLog -Message "Using crew $($crew.id) ($($crew.fullName)) and certificate $($certificate.id) ($($certificate.certificateCode))"

$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    outputDirectory = $resolvedOutputDir
    crew = $crew
    certificate = $certificate
    scenarios = @{}
}

# Scenario 1: large image preprocessing/compression path
$certificateNumber = "RS-LARGE-$runTag"
$certificateCreate = Invoke-RestMethod -Method Post -Uri "$EdgeBaseUrl/api/certificates/crew-certificates" -Headers (Get-JsonHeaders -AccessToken $accessToken) -ContentType 'application/json' -Body (@{
    certificateId = $certificate.id
    crewMemberId = $crew.id
    certificateNumber = $certificateNumber
    issueDate = [DateTime]::UtcNow.ToString('o')
    expiryDate = [DateTime]::UtcNow.AddYears(5).ToString('o')
    issuingAuthority = 'Research Harness'
    certificateOfCompetency = 'File Flow Validation'
    status = 'VALID'
    notes = 'Large image preprocess scenario'
} | ConvertTo-Json -Depth 6)
$certificateId = [int]$certificateCreate.id
Invoke-MultipartEdgeRequest -Method PUT -Uri "$EdgeBaseUrl/api/certificates/crew-certificates/$certificateId/file" -AccessToken $accessToken -Fields @{} -FileFieldName 'file' -FilePath $largeImagePath | Out-Null

$largeTrigger = Invoke-EdgeTriggerSync -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$largeQueue = Wait-ForEdgeScenarioQueueDrain -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'crew_certificate' -RecordKeys @($certificateNumber) -TimeoutSeconds $PollTimeoutSeconds
$largeShoreManifest = Wait-ForShoreManifest -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'crew_certificate' -RecordKey $certificateNumber -TimeoutSeconds $PollTimeoutSeconds
$summary.scenarios.largeImage = [ordered]@{
    certificateId = $certificateId
    certificateNumber = $certificateNumber
    triggerLatencyMs = $largeTrigger.latencyMs
    edgeQueueAfter = $largeQueue
    edgeManifest = Get-LatestManifestMetrics -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'crew_certificate' -RecordKey $certificateNumber
    shoreManifest = $largeShoreManifest
}

# Scenario 2: small-file bundle path
$bundleRecordKeys = New-Object System.Collections.Generic.List[string]
for ($index = 0; $index -lt $smallFilePaths.Count; $index++) {
    $document = Invoke-MultipartEdgeRequest -Method POST -Uri "$EdgeBaseUrl/api/crew/$($crew.id)/identity-documents" -AccessToken $accessToken -Fields @{
        TargetTable = 'health_documents'
        DocumentType = "ResearchBundle$($index + 1)"
        DocumentNumber = "RS-BUNDLE-$runTag-$($index + 1)"
        Notes = 'Small file bundling scenario'
    } -FileFieldName 'File' -FilePath $smallFilePaths[$index]
    $bundleRecordKeys.Add([string]$document.id)
}

$bundleTrigger = Invoke-EdgeTriggerSync -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$bundleQueue = Wait-ForEdgeScenarioQueueDrain -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'health_document' -RecordKeys @($bundleRecordKeys) -TimeoutSeconds $PollTimeoutSeconds
$bundleShoreManifests = @()
foreach ($recordKey in $bundleRecordKeys) {
    $bundleShoreManifests += Wait-ForShoreManifest -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'health_document' -RecordKey $recordKey -TimeoutSeconds $PollTimeoutSeconds
}

$bundleEdgeManifests = @()
foreach ($recordKey in $bundleRecordKeys) {
    $bundleEdgeManifests += Get-LatestManifestMetrics -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'health_document' -RecordKey $recordKey
}

$summary.scenarios.smallBundle = [ordered]@{
    recordKeys = @($bundleRecordKeys)
    triggerLatencyMs = $bundleTrigger.latencyMs
    edgeQueueAfter = $bundleQueue
    edgeManifests = $bundleEdgeManifests
    shoreManifests = $bundleShoreManifests
    edgeUploadChunkSessionCount = Get-ChunkSessionCount -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'health_document' -RecordKeys @($bundleRecordKeys)
    shoreUploadChunkSessionCount = Get-ChunkSessionCount -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'health_document' -RecordKeys @($bundleRecordKeys)
}

# Scenario 3: delta transfer path
$deltaDocument = Invoke-MultipartEdgeRequest -Method POST -Uri "$EdgeBaseUrl/api/crew/$($crew.id)/identity-documents" -AccessToken $accessToken -Fields @{
    TargetTable = 'travel_documents'
    DocumentType = 'ResearchDelta'
    DocumentNumber = "RS-DELTA-$runTag"
    Notes = 'Delta transfer scenario'
} -FileFieldName 'File' -FilePath $deltaV1Path
$deltaRecordKey = [string]$deltaDocument.id

$deltaBaselineTrigger = Invoke-EdgeTriggerSync -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$deltaBaselineQueue = Wait-ForEdgeScenarioQueueDrain -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'travel_document' -RecordKeys @($deltaRecordKey) -TimeoutSeconds $PollTimeoutSeconds
$null = Wait-ForShoreManifest -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey -TimeoutSeconds $PollTimeoutSeconds
$baselineChunkSession = Get-LatestChunkSessionMetrics -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey

Invoke-MultipartEdgeRequest -Method PUT -Uri "$EdgeBaseUrl/api/crew/identity-documents/$deltaRecordKey/file" -AccessToken $accessToken -Fields @{ TargetTable = 'travel_documents' } -FileFieldName 'File' -FilePath $deltaV2Path | Out-Null

$deltaTrigger = Invoke-EdgeTriggerSync -AccessToken $accessToken -BaseUrl $EdgeBaseUrl
$deltaQueue = Wait-ForEdgeScenarioQueueDrain -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'travel_document' -RecordKeys @($deltaRecordKey) -TimeoutSeconds $PollTimeoutSeconds
$deltaShoreManifest = Wait-ForShoreManifest -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey -TimeoutSeconds $PollTimeoutSeconds
$deltaRequest = Get-LatestTransferRequestMetrics -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey
$deltaChunkSession = Get-LatestChunkSessionMetrics -ContainerName $ShoreDbContainerName -Database $ShoreDatabase -Username $ShoreDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey

$summary.scenarios.deltaTransfer = [ordered]@{
    recordKey = $deltaRecordKey
    baselineTriggerLatencyMs = $deltaBaselineTrigger.latencyMs
    baselineQueueAfter = $deltaBaselineQueue
    baselineChunkSession = $baselineChunkSession
    modifiedTriggerLatencyMs = $deltaTrigger.latencyMs
    modifiedQueueAfter = $deltaQueue
    edgeManifest = Get-LatestManifestMetrics -ContainerName $EdgeDbContainerName -Database $EdgeDatabase -Username $EdgeDatabaseUser -TableName 'travel_document' -RecordKey $deltaRecordKey
    shoreManifest = $deltaShoreManifest
    shoreTransferRequest = $deltaRequest
    shoreChunkSession = $deltaChunkSession
    changedChunkCount = Get-RequestedChunkCount -ChunkSession $deltaChunkSession
}

$summaryPath = Join-Path $resolvedOutputDir 'summary.json'
Write-ResearchJsonFile -Path $summaryPath -Data $summary
Write-ResearchLog -Message "File-flow scenario summary written to $summaryPath"
Write-Output ($summary | ConvertTo-Json -Depth 10)