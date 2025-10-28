# 🗑️ DANH SÁCH FILE RÁC CẦN XÓA

**Ngày tạo:** 2025-10-28  
**Mục đích:** Cleanup project trước khi làm việc nhóm

---

## ✅ HƯỚNG DẪN SỬ DỤNG

1. **Đọc kỹ từng section**
2. **Kiểm tra file trước khi xóa** (đề phòng)
3. **Xóa theo thứ tự từ trên xuống**
4. **Tick ✓ vào checkbox khi đã xóa**

---

## 🔴 LOẠI 1: FILES RÁC 100% AN TOÀN XÓA

### 📁 ROOT Directory (`D:\Martime_product_v1\`)

```
[ ] vite.config.ts
    ├─ Lý do: File config Vite đặt sai vị trí (nên ở frontend/)
    ├─ Kiểm tra: Mở file, thấy chỉ có 10 dòng config cơ bản
    └─ Xóa: DEL vite.config.ts

[ ] package-lock.json
    ├─ Lý do: Không có package.json tương ứng, packages rỗng
    ├─ Kiểm tra: Mở file, thấy "packages": {}
    └─ Xóa: DEL package-lock.json
```

### 📁 FRONTEND-EDGE (`frontend-edge\`)

```
[ ] cd
    ├─ Lý do: File rỗng do lỗi gõ lệnh terminal
    ├─ Kiểm tra: File size 0 KB
    └─ Xóa: cd frontend-edge && DEL cd

[ ] npm  
    ├─ Lý do: File rỗng do lỗi gõ lệnh terminal
    ├─ Kiểm tra: File size 0 KB
    └─ Xóa: DEL npm
```

### 📁 EDGE-SERVICES (`edge-services\`)

```
[ ] insert-sample-data.sql
    ├─ Lý do: Version cũ, đã được thay thế bởi insert-sample-data-fixed.sql
    ├─ Kiểm tra: So sánh với insert-sample-data-fixed.sql
    └─ Xóa: cd edge-services && DEL insert-sample-data.sql

[ ] insert-sample-data-complete.sql
    ├─ Lý do: Version cũ, schema không khớp với database thực tế
    ├─ Kiểm tra: File có lỗi schema (đã fix ở version fixed)
    └─ Xóa: DEL insert-sample-data-complete.sql

[ ] sample-data-insert.sql
    ├─ Lý do: Trùng lặp với các file insert khác
    ├─ Kiểm tra: Nội dung overlap với fixed version
    └─ Xóa: DEL sample-data-insert.sql

[ ] insert-data.ps1
    ├─ Lý do: Script cũ, đã được thay thế bởi INSERT-SAMPLE-DATA.ps1
    ├─ Kiểm tra: Chức năng giống INSERT-SAMPLE-DATA.ps1
    └─ Xóa: DEL insert-data.ps1
```

**Tổng LOẠI 1: 8 files**

---

## 🟡 LOẠI 2: DOCUMENTATION TRÙNG LẶP (Giữ 1 Xóa 1)

### 📁 ROOT Directory

```
[ ] QUICK_START.md
    ├─ Lý do: Trùng lặp với QUICK_START_GUIDE.md
    ├─ Giữ: QUICK_START_GUIDE.md (chi tiết hơn)
    ├─ Kiểm tra: Mở 2 files so sánh nội dung
    └─ Xóa: DEL QUICK_START.md

[ ] QUICK_REFERENCE.md  
    ├─ Lý do: Trùng lặp với IMPROVEMENTS_QUICK_REFERENCE.md
    ├─ Giữ: IMPROVEMENTS_QUICK_REFERENCE.md (cập nhật hơn)
    ├─ Kiểm tra: So sánh 2 files
    └─ Xóa: DEL QUICK_REFERENCE.md

[ ] SETUP_GUIDE.md
    ├─ Lý do: Trùng lặp với GETTING_STARTED.md
    ├─ Giữ: GETTING_STARTED.md (chuẩn hơn)
    ├─ Kiểm tra: So sánh nội dung
    └─ Xóa: DEL SETUP_GUIDE.md
```

### 📁 FRONTEND-MOBILE (`frontend-mobile\`)

```
[ ] QUICKSTART.md
    ├─ Lý do: Trùng lặp với QUICK_START.md (chỉ khác chữ hoa)
    ├─ Giữ: QUICK_START.md (naming convention chuẩn)
    ├─ Kiểm tra: So sánh 2 files
    └─ Xóa: cd frontend-mobile && DEL QUICKSTART.md

[ ] QUICK_TEST_NOW.md
    ├─ Lý do: Overlap với QUICK_TEST_SERVER_CONFIG.md
    ├─ Giữ: QUICK_TEST_SERVER_CONFIG.md (chi tiết hơn)
    ├─ Kiểm tra: So sánh nội dung về server testing
    └─ Xóa: DEL QUICK_TEST_NOW.md

[ ] INTEGRATION_COMPLETE.md
    ├─ Lý do: Trùng lặp với SERVER_CONFIG_COMPLETE.md
    ├─ Giữ: SERVER_CONFIG_COMPLETE.md (mới hơn, chi tiết hơn)
    ├─ Kiểm tra: Cả 2 đều là completion summary
    └─ Xóa: DEL INTEGRATION_COMPLETE.md

[ ] DEVELOPMENT_SUMMARY.md
    ├─ Lý do: Trùng lặp với PROJECT_SUMMARY.md
    ├─ Giữ: PROJECT_SUMMARY.md (tổng quan hơn)
    ├─ Kiểm tra: So sánh 2 files summary
    └─ Xóa: DEL DEVELOPMENT_SUMMARY.md

[ ] COMPLETE_GUIDE.md
    ├─ Lý do: Nhiều overlap với SERVER_CONFIGURATION_GUIDE.md
    ├─ Giữ: SERVER_CONFIGURATION_GUIDE.md (focused hơn)
    ├─ Kiểm tra: So sánh phần server config
    └─ Xóa (optional): DEL COMPLETE_GUIDE.md
```

### 📁 FRONTEND-EDGE (`frontend-edge\`)

```
[ ] QUICK_TEST_CREW.md
    ├─ Lý do: Trùng lặp với QUICK_TEST_GUIDE.md
    ├─ Giữ: QUICK_TEST_GUIDE.md (tổng quát hơn)
    ├─ Kiểm tra: So sánh nội dung testing
    └─ Xóa: cd frontend-edge && DEL QUICK_TEST_CREW.md
```

### 📁 EDGE-SERVICES (`edge-services\`)

```
[ ] API_SETUP_COMPLETE.md
    ├─ Lý do: Completion doc, có thể merge vào README.md
    ├─ Giữ (tạm): Để merge nội dung vào README trước
    ├─ Kiểm tra: Đọc nội dung, copy phần quan trọng vào README
    └─ Xóa (sau khi merge): DEL API_SETUP_COMPLETE.md

[ ] TROUBLESHOOTING.md
    ├─ Lý do: Có thể merge vào README.md
    ├─ Giữ (tạm): Để merge nội dung vào README trước
    ├─ Kiểm tra: Copy troubleshooting section vào README
    └─ Xóa (sau khi merge): DEL TROUBLESHOOTING.md
```

**Tổng LOẠI 2: 11 files**

---

## 🟠 LOẠI 3: CẦN KIỂM TRA KỸ

### 📁 BACKEND (`backend\`)

```
[ ] TOÀN BỘ FOLDER backend\
    ├─ Lý do: Dự án đang dùng edge-services\ làm backend chính
    ├─ Kiểm tra: 
    │   1. Xem có file nào được reference trong edge-services không
    │   2. Xem có dependency nào không
    │   3. Hỏi team có đang dùng backend\ không
    ├─ Nếu KHÔNG dùng:
    │   └─ Có thể xóa hoặc rename thành backend.OLD\
    └─ Nếu CÓ dùng:
        └─ GIỮ LẠI
```

**Kiểm tra backend folder:**
```powershell
# Xem có reference đến backend\ trong edge-services không
cd edge-services
findstr /s /i "backend" *.cs *.json

# Nếu không có kết quả → An toàn xóa
# Nếu có kết quả → Xem xét kỹ
```

---

## 📊 THỐNG KÊ TỔNG HỢP

```
┌─────────────────────────────────────┐
│ TỔNG QUAN FILES CẦN XÓA             │
├─────────────────────────────────────┤
│ Loại 1: An toàn xóa        8 files  │
│ Loại 2: Docs trùng        11 files  │
│ Loại 3: Cần kiểm tra       1 folder │
├─────────────────────────────────────┤
│ TỔNG CỘNG:               ~19 files  │
└─────────────────────────────────────┘
```

**Dung lượng tiết kiệm:** ~500KB - 1MB  
**Thời gian cleanup:** 10-15 phút

---

## 🔧 LỆNH XÓA NHANH (PowerShell)

### **OPTION A: Xóa từng file (An toàn)**

```powershell
# CD vào thư mục project
cd D:\Martime_product_v1

# === LOẠI 1: An toàn xóa ===
Remove-Item "vite.config.ts" -Force
Remove-Item "package-lock.json" -Force

cd frontend-edge
Remove-Item "cd" -Force
Remove-Item "npm" -Force
cd ..

cd edge-services
Remove-Item "insert-sample-data.sql" -Force
Remove-Item "insert-sample-data-complete.sql" -Force
Remove-Item "sample-data-insert.sql" -Force
Remove-Item "insert-data.ps1" -Force
cd ..

# === LOẠI 2: Docs trùng ===
# (Review trước rồi xóa)
Remove-Item "QUICK_START.md" -Force
Remove-Item "QUICK_REFERENCE.md" -Force
Remove-Item "SETUP_GUIDE.md" -Force

cd frontend-mobile
Remove-Item "QUICKSTART.md" -Force
Remove-Item "QUICK_TEST_NOW.md" -Force
Remove-Item "INTEGRATION_COMPLETE.md" -Force
Remove-Item "DEVELOPMENT_SUMMARY.md" -Force
# Remove-Item "COMPLETE_GUIDE.md" -Force  # Optional
cd ..

cd frontend-edge
Remove-Item "QUICK_TEST_CREW.md" -Force
cd ..
```

### **OPTION B: Script tự động (Tạo backup trước)**

```powershell
# Tạo folder backup
New-Item -ItemType Directory -Path ".\BACKUP_$(Get-Date -Format 'yyyyMMdd')" -Force

# Move files vào backup thay vì xóa
Move-Item "vite.config.ts" -Destination ".\BACKUP_*\" -Force
# ... (tương tự cho các file khác)

# Nếu OK sau 1 tuần → Xóa folder BACKUP
```

---

## ⚠️ LƯU Ý QUAN TRỌNG

### **TRƯỚC KHI XÓA:**

1. ✅ **Commit code hiện tại:**
   ```bash
   git add .
   git commit -m "chore: commit before cleanup"
   git push origin master
   ```

2. ✅ **Tạo backup:**
   ```bash
   # Tạo branch backup
   git checkout -b backup-before-cleanup
   git push origin backup-before-cleanup
   git checkout master
   ```

3. ✅ **Thông báo team:**
   - Báo team bạn sắp cleanup
   - Hỏi có file nào đang dùng không
   - Chờ confirm trước khi xóa

### **SAU KHI XÓA:**

1. ✅ **Test lại project:**
   ```bash
   # Test Edge Server
   cd edge-services
   dotnet build
   dotnet run
   
   # Test Mobile App
   cd frontend-mobile
   flutter pub get
   flutter run -d windows
   
   # Test Frontend Web
   cd frontend-edge
   npm install
   npm run dev
   ```

2. ✅ **Commit cleanup:**
   ```bash
   git add .
   git commit -m "chore: cleanup unused files"
   git push origin master
   ```

3. ✅ **Update documentation:**
   - Cập nhật README.md nếu cần
   - Thông báo team đã cleanup xong

---

## 📋 CHECKLIST CLEANUP

### **PHASE 1: Preparation (5 phút)**
- [ ] Đọc kỹ danh sách files
- [ ] Commit code hiện tại
- [ ] Tạo backup branch
- [ ] Thông báo team

### **PHASE 2: Delete Files (5 phút)**
- [ ] Xóa LOẠI 1 (8 files rác)
- [ ] Review và xóa LOẠI 2 (11 docs trùng)
- [ ] Kiểm tra LOẠI 3 (backend folder)

### **PHASE 3: Verification (5 phút)**
- [ ] Test Edge Server build
- [ ] Test Mobile App build
- [ ] Test Frontend build
- [ ] Kiểm tra không có broken links

### **PHASE 4: Finalize (2 phút)**
- [ ] Commit cleanup changes
- [ ] Push to master
- [ ] Update documentation
- [ ] Thông báo team hoàn thành

---

## 🎯 GỢI Ý THỰC HIỆN

### **CÁCH 1: Xóa từng loại (An toàn nhất)**
```
Day 1: Xóa LOẠI 1 (files rác) → Test → Commit
Day 2: Xóa LOẠI 2 (docs trùng) → Test → Commit
Day 3: Kiểm tra LOẠI 3 (backend) → Quyết định
```

### **CÁCH 2: Xóa tất cả (Nhanh hơn)**
```
Morning: Backup → Xóa tất cả LOẠI 1 & 2
Afternoon: Test toàn bộ → Fix nếu có lỗi → Commit
```

---

## 📞 HỖ TRỢ

**Nếu xóa nhầm file:**
```bash
# Restore từ git
git checkout HEAD -- <file_name>

# Hoặc restore từ backup branch
git checkout backup-before-cleanup -- <file_name>
```

**Nếu project lỗi sau khi cleanup:**
```bash
# Rollback toàn bộ
git reset --hard HEAD~1

# Hoặc restore từ backup
git checkout backup-before-cleanup
git checkout -b master-new
# Fix issues
git push -f origin master
```

---

## ✅ KẾT LUẬN

**Bạn có thể bắt đầu cleanup theo thứ tự:**

1. ✅ **LOẠI 1 trước** (8 files - 100% an toàn)
2. ✅ **LOẠI 2 sau** (11 files - review trước)
3. ✅ **LOẠI 3 cuối** (backend folder - hỏi team)

**Ước tính thời gian:**
- Xóa LOẠI 1: 5 phút
- Review + Xóa LOẠI 2: 10 phút
- Kiểm tra LOẠI 3: 5 phút
- **Tổng:** 20 phút

---

**File này sẽ tự động bị xóa sau khi cleanup xong! 😄**

**Ready to cleanup? Let's go! 🚀**
