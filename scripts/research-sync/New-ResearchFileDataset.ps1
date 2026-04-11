param(
    [string]$OutputDir = '.\artifacts\research-sync\file-dataset',
    [int]$TotalFiles = 300,
    [int]$DuplicateCount = 60,
    [int]$MinPaddingBytes = 2048,
    [int]$MaxPaddingBytes = 262144,
    [int]$RandomSeed = 9412378
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

if ($DuplicateCount -ge $TotalFiles) {
    throw 'DuplicateCount must be smaller than TotalFiles.'
}

$resolvedOutputDir = New-ResearchDirectory -Path $OutputDir
$imagesDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'images')
$pdfDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'pdf')
$scanDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'scan')
$duplicateDir = New-ResearchDirectory -Path (Join-Path $resolvedOutputDir 'duplicates')

$random = [System.Random]::new($RandomSeed)

$jpegBase = [Convert]::FromBase64String('/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxAQEBAQEA8PEA8PDw8PDw8PDw8PDw8QFREWFhURFRUYHSggGBolGxUVITEhJSkrLi4uFx8zODMsNygtLisBCgoKDg0OGhAQGy0lICYtLS8tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLf/AABEIAAEAAgMBIgACEQEDEQH/xAAXAAADAQAAAAAAAAAAAAAAAAAAAQID/8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEAMQAAAB6A//xAAXEAEAAwAAAAAAAAAAAAAAAAAAAREh/9oACAEBAAEFApL/xAAVEQEBAAAAAAAAAAAAAAAAAAAAEf/aAAgBAwEBPwGn/8QAFBEBAAAAAAAAAAAAAAAAAAAAEP/aAAgBAgEBPwCf/8QAFBABAAAAAAAAAAAAAAAAAAAAEP/aAAgBAQAGPwJf/8QAFhABAQEAAAAAAAAAAAAAAAAAABEh/9oACAEBAAE/IRmP/9k=')
$pngBase = [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y9l9wAAAABJRU5ErkJggg==')
$pdfBase = [System.Text.Encoding]::ASCII.GetBytes("%PDF-1.4`n1 0 obj`n<< /Type /Catalog /Pages 2 0 R >>`nendobj`n2 0 obj`n<< /Type /Pages /Count 1 /Kids [3 0 R] >>`nendobj`n3 0 obj`n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 200] /Contents 4 0 R >>`nendobj`n4 0 obj`n<< /Length 44 >>`nstream`nBT /F1 18 Tf 36 120 Td (Research Sync Dataset) Tj ET`nendstream`nendobj`nxref`n0 5`n0000000000 65535 f `n0000000010 00000 n `n0000000060 00000 n `n0000000117 00000 n `n0000000207 00000 n `ntrailer`n<< /Root 1 0 R /Size 5 >>`nstartxref`n305`n%%EOF`n")

function New-PaddedBytes {
    param(
        [byte[]]$BaseBytes,
        [System.Random]$Generator,
        [int]$MinimumPadding,
        [int]$MaximumPadding
    )

    $paddingLength = $Generator.Next($MinimumPadding, $MaximumPadding)
    $padding = New-Object byte[] $paddingLength
    $Generator.NextBytes($padding)

    $output = New-Object byte[] ($BaseBytes.Length + $padding.Length)
    [Array]::Copy($BaseBytes, 0, $output, 0, $BaseBytes.Length)
    [Array]::Copy($padding, 0, $output, $BaseBytes.Length, $padding.Length)
    return $output
}

$uniqueCount = $TotalFiles - $DuplicateCount
$typePattern = @('image-jpg', 'image-png', 'pdf', 'scan-jpg', 'scan-pdf')
$manifest = New-Object System.Collections.Generic.List[object]
$uniqueFiles = New-Object System.Collections.Generic.List[object]

for ($index = 1; $index -le $uniqueCount; $index++) {
    $typeName = $typePattern[($index - 1) % $typePattern.Count]
    switch ($typeName) {
        'image-jpg' {
            $extension = 'jpg'
            $subDir = $imagesDir
            $baseBytes = $jpegBase
        }
        'image-png' {
            $extension = 'png'
            $subDir = $imagesDir
            $baseBytes = $pngBase
        }
        'pdf' {
            $extension = 'pdf'
            $subDir = $pdfDir
            $baseBytes = $pdfBase
        }
        'scan-jpg' {
            $extension = 'jpg'
            $subDir = $scanDir
            $baseBytes = $jpegBase
        }
        'scan-pdf' {
            $extension = 'pdf'
            $subDir = $scanDir
            $baseBytes = $pdfBase
        }
        default {
            throw "Unsupported type: $typeName"
        }
    }

    $bytes = New-PaddedBytes -BaseBytes $baseBytes -Generator $random -MinimumPadding $MinPaddingBytes -MaximumPadding $MaxPaddingBytes
    $fileName = ('research-{0:d4}.{1}' -f $index, $extension)
    $filePath = Join-Path $subDir $fileName
    [System.IO.File]::WriteAllBytes($filePath, $bytes)

    $hash = Get-Sha256Hex -Bytes $bytes
    $relativePath = Get-RelativePathPortable -BasePath $resolvedOutputDir -TargetPath $filePath

    $entry = [pscustomobject]@{
        file_name = $fileName
        file_type = $typeName
        size_bytes = $bytes.Length
        sha256 = $hash
        duplicate_of = $null
        relative_path = $relativePath
    }

    $manifest.Add($entry)
    $uniqueFiles.Add([pscustomobject]@{
        file_path = $filePath
        file_name = $fileName
        file_type = $typeName
        sha256 = $hash
        relative_path = $relativePath
    })
}

for ($duplicateIndex = 1; $duplicateIndex -le $DuplicateCount; $duplicateIndex++) {
    $source = $uniqueFiles[($duplicateIndex - 1) % $uniqueFiles.Count]
    $extension = [System.IO.Path]::GetExtension($source.file_name)
    $targetFileName = ('duplicate-{0:d4}{1}' -f $duplicateIndex, $extension)
    $targetPath = Join-Path $duplicateDir $targetFileName
    Copy-Item -LiteralPath $source.file_path -Destination $targetPath -Force

    $relativePath = Get-RelativePathPortable -BasePath $resolvedOutputDir -TargetPath $targetPath
    $sizeBytes = (Get-Item -LiteralPath $targetPath).Length
    $manifest.Add([pscustomobject]@{
        file_name = $targetFileName
        file_type = $source.file_type
        size_bytes = $sizeBytes
        sha256 = $source.sha256
        duplicate_of = $source.file_name
        relative_path = $relativePath
    })
}

$manifestPath = Join-Path $resolvedOutputDir 'manifest.csv'
$manifest | Sort-Object file_name | Export-Csv -LiteralPath $manifestPath -NoTypeInformation -Encoding UTF8

Write-ResearchLog -Message "Created research file dataset at $resolvedOutputDir"
Write-ResearchLog -Message "Manifest written to $manifestPath"
Write-ResearchLog -Message "Unique files: $uniqueCount | Duplicates: $DuplicateCount | Total: $TotalFiles"