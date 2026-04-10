$tables = @(
  "equipment_assets", "equipment_groups", "equipment_group_members",
  "maintenance_schedules", "maintenance_tasks", "material_categories",
  "material_items", "schedule_checklist_templates", "schedule_spare_parts"
)
$content = ""
$inBlock = $false

Write-Host "Reading file..."
$rawText = [System.IO.File]::ReadAllText("edge_database_export_20260317_184602.sql", [System.Text.Encoding]::GetEncoding(437))
$fullText = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::GetEncoding(437).GetBytes($rawText))
$lines = $fullText -split "`r?`n"

Write-Host "Parsing..."
foreach ($line in $lines) {
    if ($line.Trim() -eq "\.") {
        if ($inBlock) {
            $content += "\.`n`n"
            $inBlock = $false
        }
    }
    
    if ($line -match "^COPY public\.([a-z_]+)\s+\(") {
        $tbl = $matches[1]
        if ($tables -contains $tbl) {
            $inBlock = $true
            $content += "--`n-- Data for Name: $tbl; Type: TABLE DATA; Schema: public; Owner: edge_user`n--`n`n"
        }
    }
    
    if ($inBlock) {
        $content += "$line`n"
    }
}

Write-Host "Writing file..."
[System.IO.File]::WriteAllText("edge_pms_materials_warehouse_export.sql", $content, [System.Text.Encoding]::UTF8)
Write-Host "Done"
