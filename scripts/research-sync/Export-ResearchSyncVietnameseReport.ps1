param(
    [Parameter(Mandatory = $true)][string]$CampaignRoot,
    [string]$CsvOutputPath,
    [string]$MarkdownOutputPath
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

if (-not (Test-Path -LiteralPath $CampaignRoot)) {
    throw "Campaign root not found: $CampaignRoot"
}

$sourcePath = Join-Path $CampaignRoot 'thesis-summary.csv'
if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "thesis-summary.csv not found under $CampaignRoot"
}

if ([string]::IsNullOrWhiteSpace($CsvOutputPath)) {
    $CsvOutputPath = Join-Path $CampaignRoot 'bao-cao-luan-van.csv'
}

if ([string]::IsNullOrWhiteSpace($MarkdownOutputPath)) {
    $MarkdownOutputPath = Join-Path $CampaignRoot 'bao-cao-luan-van.md'
}

$rows = Import-Csv -LiteralPath $sourcePath
$vnRows = New-Object System.Collections.Generic.List[object]
$mdLines = New-Object System.Collections.Generic.List[string]

$mdLines.Add('## Mo ta phuong phap gia lap mang')
$mdLines.Add('')
$mdLines.Add('Trong dot thuc nghiem nay, he thong su dung Toxiproxy de gia lap dieu kien mang giua Edge va Shore. Mot proxy `shore` duoc mo tai `localhost:8666` va chuyen tiep den Shore backend tai `host.docker.internal:5000`. Edge backend duoc cau hinh gui request dong bo den dia chi proxy nay de toan bo heartbeat, push sync va pull sync deu di qua lop mo phong mang truoc khi den Shore.')
$mdLines.Add('')
$mdLines.Add('Profile `LAN` duoc xem la baseline, tuc proxy khong ap them toxic nao. Cac profile `4G`, `VSAT` va `HF` duoc mo phong bang cach them `latency`, `jitter`, `bandwidth` hoac `timeout` tren Toxiproxy. Vi vay, cac so lieu duoi day phan anh hieu nang dong bo cua he thong trong moi truong mo phong co kiem soat, phu hop cho muc dich danh gia nghien cuu trong repo hien tai.')
$mdLines.Add('')
$mdLines.Add('Luu y: day la lop harness nghien cuu toi thieu co the chay tren may Windows/local workspace. Ket qua co gia tri so sanh giua cac profile mang trong moi truong noi bo, nhung khong thay the cho do dac hien truong hay mo phong vat ly day du.')
$mdLines.Add('')
$mdLines.Add('## Bang cau hinh profile mang da dung')
$mdLines.Add('')
$mdLines.Add('| Profile | Cong cu gia lap | Cach ap dung | Thong so chinh |')
$mdLines.Add('|---|---|---|---|')
$mdLines.Add('| LAN | Toxiproxy | Khong them toxic | Duong truyen baseline, khong gioi han bo sung |')
$mdLines.Add('| 4G | Toxiproxy | `latency` hai chieu | Upstream: 23 ms, jitter 6 ms; Downstream: 22 ms, jitter 6 ms |')
$mdLines.Add('| VSAT | Toxiproxy | `latency` + `bandwidth` hai chieu | Upstream: 345 ms, jitter 43 ms, rate 250; Downstream: 345 ms, jitter 42 ms, rate 250 |')
$mdLines.Add('| HF | Toxiproxy | `latency` + `bandwidth` hai chieu | Upstream: 1200 ms, jitter 250 ms, rate 8; Downstream: 1200 ms, jitter 250 ms, rate 8 |')
$mdLines.Add('| OFFLINE | Toxiproxy | `timeout` | Cat ket noi downstream trong 60000 ms |')
$mdLines.Add('')
$mdLines.Add('## Bang tong hop ket qua')
$mdLines.Add('')
$mdLines.Add('| Kich ban | Mang | So ban ghi | So lan lap | Ti le thanh cong (%) | Queue drain TB (s) | Queue drain Median (s) | Queue drain SD | Queue drain 95% CI | Trigger TB (ms) | Trigger Median (ms) | Trigger SD | Trigger 95% CI | Retry TB | Retry Median | Retry SD | Retry 95% CI | CPU TB (%) | RAM TB (MB) |')
$mdLines.Add('|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|')

foreach ($row in $rows) {
    $scenarioLabel = "Dong bo ban ghi"
    $vnRows.Add([pscustomobject]@{
        'Kich ban' = $scenarioLabel
        'Loai mang' = $row.network_profile
        'So ban ghi' = [int]$row.record_count
        'So lan lap' = [int]$row.repetitions
        'Ti le thanh cong (%)' = [double]$row.success_rate_pct
        'Thoi gian giai phong hang doi trung binh (s)' = [double]$row.avg_queue_drain_seconds
        'Thoi gian giai phong hang doi trung vi (s)' = if ([string]::IsNullOrWhiteSpace($row.median_queue_drain_seconds)) { $null } else { [double]$row.median_queue_drain_seconds }
        'Do lech chuan giai phong hang doi (s)' = if ([string]::IsNullOrWhiteSpace($row.std_queue_drain_seconds)) { $null } else { [double]$row.std_queue_drain_seconds }
        'Khoang tin cay 95% giai phong hang doi (s)' = if ([string]::IsNullOrWhiteSpace($row.ci95_queue_drain_seconds)) { $null } else { [double]$row.ci95_queue_drain_seconds }
        'Thoi gian giai phong hang doi nho nhat (s)' = [double]$row.min_queue_drain_seconds
        'Thoi gian giai phong hang doi lon nhat (s)' = [double]$row.max_queue_drain_seconds
        'Do tre trigger trung binh (ms)' = if ([string]::IsNullOrWhiteSpace($row.avg_trigger_latency_ms)) { $null } else { [double]$row.avg_trigger_latency_ms }
        'Do tre trigger trung vi (ms)' = if ([string]::IsNullOrWhiteSpace($row.median_trigger_latency_ms)) { $null } else { [double]$row.median_trigger_latency_ms }
        'Do lech chuan trigger (ms)' = if ([string]::IsNullOrWhiteSpace($row.std_trigger_latency_ms)) { $null } else { [double]$row.std_trigger_latency_ms }
        'Khoang tin cay 95% trigger (ms)' = if ([string]::IsNullOrWhiteSpace($row.ci95_trigger_latency_ms)) { $null } else { [double]$row.ci95_trigger_latency_ms }
        'Do tre trigger lon nhat (ms)' = if ([string]::IsNullOrWhiteSpace($row.max_trigger_latency_ms)) { $null } else { [double]$row.max_trigger_latency_ms }
        'So retry trung binh' = if ([string]::IsNullOrWhiteSpace($row.avg_retry_count_total)) { $null } else { [double]$row.avg_retry_count_total }
        'So retry trung vi' = if ([string]::IsNullOrWhiteSpace($row.median_retry_count_total)) { $null } else { [double]$row.median_retry_count_total }
        'Do lech chuan retry' = if ([string]::IsNullOrWhiteSpace($row.std_retry_count_total)) { $null } else { [double]$row.std_retry_count_total }
        'Khoang tin cay 95% retry' = if ([string]::IsNullOrWhiteSpace($row.ci95_retry_count_total)) { $null } else { [double]$row.ci95_retry_count_total }
        'So retry lon nhat' = if ([string]::IsNullOrWhiteSpace($row.max_retry_count_total)) { $null } else { [double]$row.max_retry_count_total }
        'CPU trung binh (%)' = if ([string]::IsNullOrWhiteSpace($row.avg_primary_cpu_percent)) { $null } else { [double]$row.avg_primary_cpu_percent }
        'CPU trung vi (%)' = if ([string]::IsNullOrWhiteSpace($row.median_primary_cpu_percent)) { $null } else { [double]$row.median_primary_cpu_percent }
        'Do lech chuan CPU (%)' = if ([string]::IsNullOrWhiteSpace($row.std_primary_cpu_percent)) { $null } else { [double]$row.std_primary_cpu_percent }
        'Khoang tin cay 95% CPU (%)' = if ([string]::IsNullOrWhiteSpace($row.ci95_primary_cpu_percent)) { $null } else { [double]$row.ci95_primary_cpu_percent }
        'RAM trung binh (MB)' = if ([string]::IsNullOrWhiteSpace($row.avg_primary_memory_mb)) { $null } else { [double]$row.avg_primary_memory_mb }
        'RAM trung vi (MB)' = if ([string]::IsNullOrWhiteSpace($row.median_primary_memory_mb)) { $null } else { [double]$row.median_primary_memory_mb }
        'Do lech chuan RAM (MB)' = if ([string]::IsNullOrWhiteSpace($row.std_primary_memory_mb)) { $null } else { [double]$row.std_primary_memory_mb }
        'Khoang tin cay 95% RAM (MB)' = if ([string]::IsNullOrWhiteSpace($row.ci95_primary_memory_mb)) { $null } else { [double]$row.ci95_primary_memory_mb }
    })

    $mdLines.Add(("| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} | {13} | {14} | {15} | {16} | {17} | {18} |" -f `
        $scenarioLabel, `
        $row.network_profile, `
        $row.record_count, `
        $row.repetitions, `
        $row.success_rate_pct, `
        $row.avg_queue_drain_seconds, `
        $row.median_queue_drain_seconds, `
        $row.std_queue_drain_seconds, `
        $row.ci95_queue_drain_seconds, `
        $row.avg_trigger_latency_ms, `
        $row.median_trigger_latency_ms, `
        $row.std_trigger_latency_ms, `
        $row.ci95_trigger_latency_ms, `
        $row.avg_retry_count_total, `
        $row.median_retry_count_total, `
        $row.std_retry_count_total, `
        $row.ci95_retry_count_total, `
        $row.avg_primary_cpu_percent, `
        $row.avg_primary_memory_mb))
}

$vnRows | Export-Csv -LiteralPath $CsvOutputPath -NoTypeInformation -Encoding UTF8
$mdLines | Set-Content -LiteralPath $MarkdownOutputPath -Encoding UTF8

Write-ResearchLog -Message "Vietnamese thesis CSV written to $CsvOutputPath"
Write-ResearchLog -Message "Vietnamese thesis Markdown written to $MarkdownOutputPath"