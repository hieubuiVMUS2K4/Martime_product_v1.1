# ============================================================
# Generate BCrypt password hashes for crew members
# Password format: ddmmyyyy (from DateOfBirth)
# ============================================================

# Install BCrypt if not exists
# Install-Module -Name BCrypt.Net-Next -Force

# Function to generate BCrypt hash
function Get-BCryptHash {
    param(
        [string]$Password
    )
    return [BCrypt.Net.BCrypt]::HashPassword($Password, 11)
}

Write-Host "Generating BCrypt hashes for crew passwords..." -ForegroundColor Green
Write-Host ""

# Crew data: CrewId, Name, DateOfBirth
$crewData = @(
    @{Id="CREW001"; Name="Nguyễn Văn Thành (Master)"; DOB="15031975"},
    @{Id="CREW002"; Name="Trần Minh Tuấn (C/E)"; DOB="20071978"},
    @{Id="CREW003"; Name="Lê Hoàng Nam (C/O)"; DOB="10111982"},
    @{Id="CREW004"; Name="Phạm Đức Anh (2/E)"; DOB="25051985"},
    @{Id="CREW005"; Name="Võ Thanh Tùng (3/E)"; DOB="12091988"},
    @{Id="CREW006"; Name="Đặng Văn Hải (E/O)"; DOB="08121987"},
    @{Id="CREW007"; Name="Bùi Quang Minh (4/E)"; DOB="18061990"},
    @{Id="CREW008"; Name="Hoàng Văn Đức (Fitter)"; DOB="22041992"},
    @{Id="CREW009"; Name="Ngô Văn Sơn (Fitter)"; DOB="30081993"},
    @{Id="CREW010"; Name="Lý Văn Thắng (Oiler)"; DOB="14021994"},
    @{Id="CREW011"; Name="Phan Văn Tài (Oiler)"; DOB="05101995"},
    @{Id="CREW012"; Name="Dương Minh Quân (2/O)"; DOB="28031986"},
    @{Id="CREW013"; Name="Trịnh Văn Hùng (3/O)"; DOB="16071989"},
    @{Id="CREW014"; Name="Vũ Văn Bình (Bosun)"; DOB="20011983"},
    @{Id="CREW015"; Name="Mai Văn Dũng (AB)"; DOB="10091991"},
    @{Id="CREW016"; Name="Đinh Văn Lâm (AB)"; DOB="25111992"},
    @{Id="CREW017"; Name="Hồ Văn Phúc (AB)"; DOB="30051993"},
    @{Id="CREW018"; Name="Châu Văn Toàn (OS)"; DOB="12031996"},
    @{Id="CREW019"; Name="Lương Văn Kiên (OS)"; DOB="08071997"},
    @{Id="CREW020"; Name="Nguyễn Văn Hải (Cook)"; DOB="15061984"}
)

Write-Host "Crew ID`tPassword`tBCrypt Hash" -ForegroundColor Cyan
Write-Host "-------`t--------`t-----------" -ForegroundColor Cyan

$sqlUpdates = @()

foreach ($crew in $crewData) {
    $password = $crew.DOB
    $hash = Get-BCryptHash -Password $password
    
    Write-Host "$($crew.Id)`t$password`t$hash" -ForegroundColor Yellow
    
    $sqlUpdate = "UPDATE `"Users`" SET `"PasswordHash`" = '$hash' WHERE `"Username`" = '$($crew.Id)';"
    $sqlUpdates += $sqlUpdate
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "SQL UPDATE STATEMENTS:" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

foreach ($sql in $sqlUpdates) {
    Write-Host $sql
}

Write-Host ""
Write-Host "Save these UPDATE statements to update-crew-passwords.sql" -ForegroundColor Cyan
Write-Host "Or run them directly after running seed-crew-and-users.sql" -ForegroundColor Cyan

# Export to file
$sqlUpdates | Out-File -FilePath "update-crew-passwords.sql" -Encoding UTF8
Write-Host ""
Write-Host "✅ Exported to: update-crew-passwords.sql" -ForegroundColor Green
