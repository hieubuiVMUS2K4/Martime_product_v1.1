$edgePath = '.\edge_categories_export.sql'
$shorePath = '.\shore_ports_export.sql'

$raw = Get-Content -Path $edgePath -Raw
$lines = $raw -split "`r?`n" | Where-Object { $_ -match '^INSERT INTO public\.ports VALUES \(' }

$converted = foreach ($line in $lines) {
    if ($line -match "^INSERT INTO public\.ports VALUES \((.+), (.+), (.+), (.+), (.+), (.+), (.+), (.+), (true|false), (.+), (.+), (true|false), (.+), (.+)\);$") {
        $id = $matches[1]
        $portCode = $matches[2]
        $portName = $matches[3]
        $country = $matches[4]
        $countryCode = $matches[5]
        $lat = $matches[6]
        $lng = $matches[7]
        $tz = $matches[8]
        $isActive = $matches[9]
        $createdAt = $matches[10]
        $updatedAt = $matches[11]
        $isSynced = $matches[12]
        $originNode = $matches[13]
        $syncVersion = $matches[14]

        "INSERT INTO public.ports VALUES ($id, $portCode, $portName, $country, $countryCode, $lat, $lng, $tz, $isActive, $isSynced, $syncVersion, $createdAt, $updatedAt, $originNode);"
    }
}

$content = @(
    "-- Shore ports export updated from edge_categories_export.sql",
    "BEGIN;",
    "",
    "DELETE FROM public.ports;",
    ""
) + $converted + @(
    "",
    'SELECT pg_catalog.setval(pg_get_serial_sequence(''public.ports'',''Id''), (SELECT COALESCE(MAX("Id"), 1) FROM public.ports), true);',
    "",
    "COMMIT;"
)

Set-Content -Path $shorePath -Value $content -Encoding utf8
Write-Host "Converted rows: $($converted.Count)"
