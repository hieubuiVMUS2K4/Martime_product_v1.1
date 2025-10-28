# 🚀 QUY TRÌNH LÀM VIỆC TEAM - MARITIME PROJECT

## 📌 CẤU TRÚC BRANCHES

| Branch | Người phụ trách | Module |
|--------|----------------|--------|
| `master` | **PROTECTED** - Chỉ merge qua Pull Request | Production code |
| `feature/hieu` | hieubuiVMUS2K4 | [Module của bạn] |
| `feature/hoang` | [Teammate 1] | [Module của họ] |
| `feature/fuelanalysis` | [Teammate 2] | Fuel Analysis module |

---

## ✅ QUY TRÌNH HÀNG NGÀY (BẮT BUỘC TUÂN THỦ)

### **1. BẮT ĐẦU NGÀY LÀM VIỆC**

```powershell
# Lấy code mới nhất từ master
git checkout master
git pull origin master

# Quay về branch của mình
git checkout feature/[tên-của-bạn]

# Merge code mới từ master vào branch của mình (để tránh conflict sau này)
git merge master

# Nếu có conflict → Giải quyết ngay → commit
git add .
git commit -m "merge: sync with master"
```

---

### **2. TRONG QUÁ TRÌNH LÀM VIỆC**

```powershell
# Làm việc bình thường trên branch của mình
# Ví dụ: Sửa file, thêm chức năng...

# Commit thường xuyên (mỗi khi hoàn thành 1 tính năng nhỏ)
git add .
git commit -m "feat: thêm chức năng XYZ"

# Push lên GitHub để backup (làm thường xuyên)
git push origin feature/[tên-của-bạn]
```

**💡 Quy tắc commit message:**

| Prefix | Ý nghĩa | Ví dụ |
|--------|---------|-------|
| `feat:` | Thêm tính năng mới | `feat: thêm màn hình đăng nhập` |
| `fix:` | Sửa bug | `fix: sửa lỗi crash khi đồng bộ` |
| `refactor:` | Tái cấu trúc code | `refactor: tối ưu hàm tính toán nhiên liệu` |
| `docs:` | Cập nhật tài liệu | `docs: thêm hướng dẫn API` |
| `style:` | Format code, không thay đổi logic | `style: format code theo convention` |
| `test:` | Thêm/sửa test | `test: thêm unit test cho login` |

---

### **3. KHI HOÀN THÀNH CHỨC NĂNG (SẴN SÀNG MERGE VÀO MASTER)**

#### **Bước 3.1: Chuẩn bị code**

```powershell
# 1. Đảm bảo code của mình đã push hết lên GitHub
git add .
git commit -m "feat: hoàn thành chức năng XYZ"
git push origin feature/[tên-của-bạn]

# 2. Lấy code mới nhất từ master (phòng trường hợp người khác đã merge)
git checkout master
git pull origin master

# 3. Quay về branch và merge master vào (giải quyết conflict nếu có)
git checkout feature/[tên-của-bạn]
git merge master

# 4. Test kỹ sau khi merge (đảm bảo không bị lỗi)
# - Chạy backend: dotnet run
# - Chạy frontend: npm run dev
# - Chạy mobile: flutter run

# 5. Push code đã merge lên
git push origin feature/[tên-của-bạn]
```

#### **Bước 3.2: Tạo Pull Request trên GitHub**

1. Vào: https://github.com/hieubuiVMUS2K4/Martime_product_v1.1
2. Click **Pull requests** → **New pull request**
3. Chọn:
   - Base: `master` ← Compare: `feature/[tên-của-bạn]`
4. Điền tiêu đề rõ ràng:
   - ✅ "feat: Thêm chức năng phân tích nhiên liệu"
   - ❌ "update code"
5. Điền mô tả:
   ```markdown
   ## 📝 Thay đổi
   - Thêm API `/api/fuel/analyze`
   - Thêm màn hình Fuel Analysis trên mobile
   - Cập nhật database schema cho bảng FuelData
   
   ## ✅ Đã test
   - [x] Backend API hoạt động
   - [x] Frontend hiển thị đúng
   - [x] Mobile app sync được data
   
   ## 📸 Screenshots (nếu có)
   [Attach ảnh màn hình]
   ```
6. Click **Create pull request**
7. **Gán reviewer**: Tag 1-2 người review (ví dụ: @hieubuiVMUS2K4)
8. Đợi review và merge

#### **Bước 3.3: Sau khi Pull Request được merge**

```powershell
# 1. Cập nhật master về máy
git checkout master
git pull origin master

# 2. Xóa branch cũ (đã merge rồi)
git branch -d feature/[tên-cũ]

# 3. Tạo branch mới cho chức năng tiếp theo
git checkout -b feature/[chức-năng-mới]
git push -u origin feature/[chức-năng-mới]
```

---

## 🚨 XỬ LÝ CONFLICT

### **Khi nào xảy ra conflict?**
- 2 người cùng sửa 1 file
- Khi merge master vào branch của mình

### **Cách giải quyết:**

```powershell
# 1. Merge master vào branch
git checkout feature/[tên-của-bạn]
git merge master

# Nếu có conflict, Git sẽ báo:
# CONFLICT (content): Merge conflict in [file-name]

# 2. Mở file bị conflict trong VS Code
# Tìm đoạn code như này:
# <<<<<<< HEAD
# [Code của bạn]
# =======
# [Code từ master]
# >>>>>>> master

# 3. Chọn code nào giữ lại (hoặc kết hợp cả 2)
# - Click "Accept Current Change" (giữ code của bạn)
# - Click "Accept Incoming Change" (giữ code từ master)
# - Click "Accept Both Changes" (giữ cả 2)
# - Hoặc sửa tay

# 4. Sau khi sửa xong tất cả conflicts
git add .
git commit -m "merge: resolve conflicts with master"
git push origin feature/[tên-của-bạn]
```

---

## 📞 LIÊN HỆ KHI CÓ VẤN ĐỀ

| Vấn đề | Hỏi ai | Cách liên hệ |
|--------|--------|--------------|
| Conflict không giải quyết được | Team Lead | Zalo/Telegram/Discord |
| Code không chạy sau khi merge | Người viết code đó | GitHub PR comment |
| Không biết merge hay không | Team Lead | Trước khi tạo PR |

---

## ⚠️ QUY TẮC NGHIÊM CẤM

❌ **KHÔNG BAO GIỜ:**
1. Push trực tiếp vào `master` (git push origin master)
2. Force push (git push --force) trên branch chung
3. Commit code chưa test
4. Xóa branch của người khác
5. Merge Pull Request của mình (phải có người khác review)

✅ **LUÔN LUÔN:**
1. Pull master trước khi bắt đầu làm việc
2. Commit thường xuyên với message rõ ràng
3. Test kỹ trước khi tạo Pull Request
4. Review code của người khác nghiêm túc
5. Hỏi khi không chắc chắn

---

## 📊 WORKFLOW DIAGRAM

```
master (protected)
  ↓
  ├─→ feature/hieu ──→ [Code] ──→ Pull Request ──→ Review ──→ Merge vào master
  ├─→ feature/hoang ──→ [Code] ──→ Pull Request ──→ Review ──→ Merge vào master
  └─→ feature/fuelanalysis ──→ [Code] ──→ Pull Request ──→ Review ──→ Merge vào master
```

---

## 🎯 CHECKLIST HÀNG NGÀY

### **Sáng (bắt đầu làm việc):**
- [ ] `git checkout master && git pull origin master`
- [ ] `git checkout feature/[tên-của-tôi]`
- [ ] `git merge master` (sync code mới)

### **Trong ngày:**
- [ ] Commit thường xuyên
- [ ] Push backup lên GitHub
- [ ] Test code trước khi commit

### **Tối (kết thúc làm việc):**
- [ ] `git add . && git commit -m "..."`
- [ ] `git push origin feature/[tên-của-tôi]`
- [ ] Update progress trên Trello/Notion (nếu có)

### **Khi hoàn thành chức năng:**
- [ ] Merge master vào branch
- [ ] Test kỹ lưỡng
- [ ] Tạo Pull Request
- [ ] Gán reviewer
- [ ] Đợi review và merge

---

## 🆘 TROUBLESHOOTING

### **Lỗi: "Permission denied" khi push**
→ Xem file `TEAM_WORKFLOW.md` phần xác thực Git

### **Lỗi: "Your branch is behind 'origin/master'"**
```powershell
git pull origin master
```

### **Lỗi: "fatal: refusing to merge unrelated histories"**
```powershell
git pull origin master --allow-unrelated-histories
```

### **Muốn hủy commit vừa làm (chưa push)**
```powershell
git reset --soft HEAD~1  # Giữ lại thay đổi
# hoặc
git reset --hard HEAD~1  # Xóa hết thay đổi
```

### **Muốn xóa branch đã push nhầm**
```powershell
git push origin --delete feature/[tên-branch]
```

---

**📅 Cập nhật lần cuối:** 28/10/2025
**👨‍💻 Team Lead:** hieubuiVMUS2K4
