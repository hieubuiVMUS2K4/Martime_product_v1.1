# ============================================
# SCRIPT CLEANUP GIT REPOSITORY
# Xóa node_modules và build files khỏi Git history
# ============================================

Write-Host "🚨 CẢNH BÁO: Script này sẽ xóa node_modules/ và build files khỏi Git" -ForegroundColor Red
Write-Host "Repository hiện tại đang track 19,957 files (quá nhiều!)" -ForegroundColor Yellow
Write-Host ""
Write-Host "📋 Sẽ xóa các file/folder sau khỏi Git (nhưng GIỮ LẠI trên máy):" -ForegroundColor Cyan
Write-Host "  - frontend-edge/node_modules/" -ForegroundColor White
Write-Host "  - frontend/node_modules/ (nếu có)" -ForegroundColor White
Write-Host "  - backend/bin/" -ForegroundColor White
Write-Host "  - backend/obj/" -ForegroundColor White
Write-Host "  - edge-services/bin/" -ForegroundColor White
Write-Host "  - edge-services/obj/" -ForegroundColor White
Write-Host "  - frontend-mobile/build/" -ForegroundColor White
Write-Host "  - frontend-mobile/.dart_tool/" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  LƯU Ý:" -ForegroundColor Yellow
Write-Host "  - File .gitignore ĐÃ CÓ sẵn, không cần lo" -ForegroundColor Green
Write-Host "  - Sau khi xóa khỏi Git, các file vẫn còn trên máy" -ForegroundColor Green
Write-Host "  - Teammates cần pull lại để đồng bộ" -ForegroundColor Green
Write-Host ""

$confirm = Read-Host "Bạn có chắc muốn tiếp tục? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "🔧 Bắt đầu cleanup..." -ForegroundColor Cyan

# 1. Xóa node_modules khỏi Git (nhưng giữ lại trên máy)
Write-Host ""
Write-Host "📦 [1/8] Xóa frontend-edge/node_modules/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached frontend-edge/node_modules 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa frontend-edge/node_modules/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

Write-Host ""
Write-Host "📦 [2/8] Xóa frontend/node_modules/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached frontend/node_modules 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa frontend/node_modules/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

# 2. Xóa .NET build files
Write-Host ""
Write-Host "🔨 [3/8] Xóa backend/bin/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached backend/bin 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa backend/bin/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔨 [4/8] Xóa backend/obj/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached backend/obj 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa backend/obj/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔨 [5/8] Xóa edge-services/bin/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached edge-services/bin 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa edge-services/bin/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🔨 [6/8] Xóa edge-services/obj/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached edge-services/obj 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa edge-services/obj/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

# 3. Xóa Flutter build files
Write-Host ""
Write-Host "📱 [7/8] Xóa frontend-mobile/build/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached frontend-mobile/build 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa frontend-mobile/build/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

Write-Host ""
Write-Host "📱 [8/8] Xóa frontend-mobile/.dart_tool/ khỏi Git..." -ForegroundColor Yellow
git rm -r --cached frontend-mobile/.dart_tool 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Đã xóa frontend-mobile/.dart_tool/" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  Không tìm thấy hoặc đã xóa rồi" -ForegroundColor Gray
}

# 4. Commit thay đổi
Write-Host ""
Write-Host "💾 Commit thay đổi..." -ForegroundColor Cyan
git add .gitignore
git add backend/.gitignore
git add edge-services/.gitignore
git add frontend/.gitignore
git add frontend-edge/.gitignore
git add frontend-mobile/.gitignore

git commit -m "chore: remove node_modules and build files from Git

- Remove frontend-edge/node_modules/ (thousands of files)
- Remove frontend/node_modules/
- Remove backend/bin/ and backend/obj/
- Remove edge-services/bin/ and edge-services/obj/
- Remove frontend-mobile/build/ and .dart_tool/
- Keep .gitignore files to prevent future commits

This reduces repository size significantly.
Teammates need to run 'npm install' or 'flutter pub get' after pulling."

Write-Host ""
Write-Host "✅ HOÀN THÀNH!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Kiểm tra số file còn lại:" -ForegroundColor Cyan
$fileCount = (git ls-files | Measure-Object -Line).Lines
Write-Host "   Số file đang track: $fileCount" -ForegroundColor White
Write-Host "   (Trước đây: 19,957 files)" -ForegroundColor Gray
Write-Host ""
Write-Host "🚀 Bước tiếp theo:" -ForegroundColor Cyan
Write-Host "   1. Push lên GitHub: git push origin master" -ForegroundColor Yellow
Write-Host "   2. Gửi tin nhắn cho teammates:" -ForegroundColor Yellow
Write-Host ""
Write-Host '      "Team ơi, tôi vừa cleanup Git repository (xóa node_modules/)."' -ForegroundColor White
Write-Host '      "Các bạn cần làm:"' -ForegroundColor White
Write-Host '      "  1. git pull origin master"' -ForegroundColor White
Write-Host '      "  2. cd frontend-edge && npm install"' -ForegroundColor White
Write-Host '      "  3. cd frontend-mobile && flutter pub get"' -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Nếu teammates đang làm việc, hãy BÁO TRƯỚC khi push!" -ForegroundColor Red
