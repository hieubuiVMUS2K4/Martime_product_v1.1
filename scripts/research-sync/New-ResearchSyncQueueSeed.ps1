param(
    [int]$RecordCount = 100,
    [int]$Priority = 2,
    [string]$RunId = ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')),
    [string]$OutputSqlPath = '.\artifacts\research-sync\seed.sql',
    [string]$EdgeContainerName = 'maritime-edge-postgres',
    [string]$EdgeDatabase = 'maritime_edge',
    [string]$EdgeUsername = 'edge_user',
    [switch]$Apply,
    [switch]$ResetPendingQueue,
    [switch]$SeedCrewIfEmpty
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

if ($RecordCount -le 0) {
    throw 'RecordCount must be greater than zero.'
}

$outputParent = Split-Path -Path $OutputSqlPath -Parent
if ([string]::IsNullOrWhiteSpace($outputParent)) {
    $outputParent = '.'
}

$resolvedOutputDir = New-ResearchDirectory -Path $outputParent
$resolvedSqlPath = Join-Path $resolvedOutputDir ([System.IO.Path]::GetFileName($OutputSqlPath))

$resetSql = ''
if ($ResetPendingQueue.IsPresent) {
    $resetSql = "DELETE FROM sync_queue WHERE synced_at IS NULL;"
}

$sql = @"
BEGIN;

$resetSql

DO `$$
BEGIN
    IF (SELECT COUNT(*) FROM crew_members) = 0 THEN
        RAISE EXCEPTION 'crew_members table is empty. Seed crew data first.';
    END IF;
END
`$$;

WITH expanded AS (
    SELECT
        gs AS seq,
        selected.id,
        selected.crew_id,
        selected.full_name,
        COALESCE(selected.origin_node, 'EDGE') AS origin_node,
        format(
            '%s%s',
            left(selected.full_name, GREATEST(1, 200 - length(format(' BENCH-$RunId-%s', lpad(gs::text, 6, '0'))))),
            format(' BENCH-$RunId-%s', lpad(gs::text, 6, '0'))
        ) AS bench_name,
        NOW() + make_interval(secs => (gs % 300)) + (gs * interval '1 millisecond') AS bench_updated_at
    FROM generate_series(1, $RecordCount) AS gs
    JOIN LATERAL (
        SELECT id, crew_id, full_name, origin_node
        FROM crew_members
        ORDER BY id
        OFFSET ((gs - 1) % (SELECT COUNT(*) FROM crew_members))
        LIMIT 1
    ) AS selected ON TRUE
),
latest_per_crew AS (
    SELECT DISTINCT ON (id)
        id,
        bench_name,
        bench_updated_at
    FROM expanded
    ORDER BY id, seq DESC
),
updated AS (
    UPDATE crew_members c
    SET full_name = l.bench_name,
        updated_at = l.bench_updated_at,
        is_synced = false
    FROM latest_per_crew l
    WHERE c.id = l.id
    RETURNING c.id
)
INSERT INTO sync_queue (
    table_name,
    record_key,
    action_type,
    payload,
    priority,
    retry_count,
    max_retries,
    next_retry_at,
    last_error,
    created_at,
    synced_at
)
SELECT
    'crew_member',
    e.id::text,
    1,
    json_build_object(
        'Id', e.id,
        'CrewId', e.crew_id,
        'FullName', e.bench_name,
        'UpdatedAt', to_char(e.bench_updated_at AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'IsSynced', false,
        'OriginNode', e.origin_node
    )::text,
    $Priority,
    0,
    5,
    NULL,
    NULL,
    NOW() + (e.seq * interval '1 millisecond'),
    NULL
FROM expanded e
ORDER BY e.seq;

COMMIT;
"@

[System.IO.File]::WriteAllText($resolvedSqlPath, $sql, [System.Text.Encoding]::UTF8)
Write-ResearchLog -Message "Seed SQL written to $resolvedSqlPath"

if ($Apply.IsPresent) {
    Assert-DockerContainerRunning -ContainerName $EdgeContainerName

    if ($SeedCrewIfEmpty.IsPresent) {
        $crewCountSql = Join-Path $env:TEMP "research-crew-count-$RunId.sql"
        [System.IO.File]::WriteAllText($crewCountSql, "SELECT COUNT(*) AS crew_count FROM crew_members;", [System.Text.Encoding]::UTF8)
        $crewCountRaw = Get-Content -LiteralPath $crewCountSql -Raw | docker exec -i $EdgeContainerName psql -t -A -v ON_ERROR_STOP=1 -U $EdgeUsername -d $EdgeDatabase
        Remove-Item -LiteralPath $crewCountSql -Force -ErrorAction SilentlyContinue

        if ($LASTEXITCODE -ne 0) {
            throw 'Unable to query crew_members count.'
        }

        $crewCount = [int]($crewCountRaw.Trim())
        if ($crewCount -eq 0) {
            $seedCrewSql = Join-Path (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\edge_product\scripts\seed-crew-edge.sql')).Path
            Write-ResearchLog -Message "crew_members is empty. Applying existing seed file: $seedCrewSql" -Level WARN
            Invoke-PostgresSqlFile -ContainerName $EdgeContainerName -Database $EdgeDatabase -Username $EdgeUsername -SqlFilePath $seedCrewSql
        }
    }

    Invoke-PostgresSqlFile -ContainerName $EdgeContainerName -Database $EdgeDatabase -Username $EdgeUsername -SqlFilePath $resolvedSqlPath
    Write-ResearchLog -Message "Applied benchmark sync_queue seed into $EdgeDatabase"
}