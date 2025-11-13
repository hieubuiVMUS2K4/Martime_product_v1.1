# PowerShell script to optimize CSS in NoonReportForm.tsx
$filePath = "f:\NCKH\Product\Martime_product_v1.1\frontend-edge\src\pages\Reporting\NoonReportForm.tsx"
$content = Get-Content -Path $filePath -Raw

# Replace CSS classes for compact design
$replacements = @{
    # Padding adjustments
    'px-3 py-2 border' = 'px-3 py-1.5 text-sm border'
    
    # Label sizing
    'text-sm font-medium text-gray-700 mb-2' = 'text-xs font-medium text-gray-700 mb-1'
    
    # Header sizing  
    'text-lg font-semibold text-gray-900 mb-4' = 'text-base font-semibold text-gray-900 mb-3'
    
    # Section padding
    'shadow-sm p-6 border' = 'shadow-sm p-4 border'
    
    # Grid gaps
    'gap-4">' = 'gap-3">'
    
    # Help text
    'text-xs text-gray-500 mt-1' = 'text-[10px] text-gray-500 mt-0.5'
    
    # Icon sizes
    'h-5 w-5 text' = 'h-4 w-4 text'
}

foreach ($key in $replacements.Keys) {
    $content = $content -replace [regex]::Escape($key), $replacements[$key]
}

# Write back
Set-Content -Path $filePath -Value $content -NoNewline
Write-Host "✅ CSS optimized successfully!" -ForegroundColor Green
