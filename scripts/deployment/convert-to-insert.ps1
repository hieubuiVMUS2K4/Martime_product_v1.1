$file = "edge_pms_materials_warehouse_export.sql"
$lines = Get-Content $file -Encoding UTF8
$newLines = @()
$inCopy = $false
$tableName = ""
$columns = ""

foreach ($line in $lines) {
    if ($line.StartsWith("COPY")) {
        # Format: COPY public.equipment_assets (id, asset_code, ...) FROM stdin;
        if ($line -match "^COPY\s+(public\.[a-zA-Z_0-9]+)\s+\(([^)]+)\)\s+FROM\s+stdin;") {
            $tableName = $matches[1]
            $columns = $matches[2]
            $inCopy = $true
            $newLines += "-- Insert data for $tableName"
        } else {
            $newLines += $line
        }
    } elseif ($line.Trim() -eq "\.") {
        if ($inCopy) {
            $inCopy = $false
            $newLines += ""
        } else {
            $newLines += $line
        }
    } elseif ($inCopy) {
        $fields = $line.Split("`t")
        $values = @()
        foreach ($field in $fields) {
            if ($field -eq "\N") {
                $values += "NULL"
            } elseif ($field -eq "t") {
                $values += "true"
            } elseif ($field -eq "f") {
                $values += "false"
            } else {
                # Escape single quotes
                $escaped = $field -replace "'", "''"
                $values += "'$escaped'"
            }
        }
        $valString = $values -join ", "
        $newLines += "INSERT INTO $tableName ($columns) VALUES ($valString);"
    } else {
        $newLines += $line
    }
}

[System.IO.File]::WriteAllLines((Join-Path (Get-Location) $file), $newLines, [System.Text.Encoding]::UTF8)
Write-Host "Done converting to INSERT"
