# 📜 CERTIFICATE MANAGEMENT SYSTEM - HƯỚNG DẪN SỬ DỤNG

## 🎯 TỔNG QUAN

Hệ thống quản lý chứng chỉ mới đã thay thế cách lưu trữ certificate cũ trong bảng `crew_members`. Giờ đây:

- ✅ **Không giới hạn số lượng certificate** cho mỗi crew member
- ✅ **Quản lý tập trung** các loại certificate (master data)
- ✅ **Upload file** scan/photo certificate
- ✅ **Tracking trạng thái** VALID/EXPIRED/SUSPENDED
- ✅ **Mở rộng dễ dàng** với các loại certificate mới

---

## 📋 CẤU TRÚC DATABASE

### **1. Bảng `certificates` (Master Data)**

Lưu trữ các **LOẠI certificate** có trong hàng hải.

| Cột | Kiểu | Mô tả | Ví dụ |
|-----|------|-------|-------|
| `id` | UUID | Primary Key | |
| `certificate_code` | VARCHAR(50) | Mã định danh (UNIQUE) | `STCW_II_2`, `MEDICAL` |
| `certificate_name` | VARCHAR(200) | Tên đầy đủ | `Certificate of Competency - Master` |
| `category` | VARCHAR(50) | Nhóm certificate | `COMPETENCY`, `MEDICAL`, `PROFICIENCY` |
| `validity_period_months` | INT | Thời hạn hiệu lực (tháng) | `60` (5 năm), `24` (2 năm) |
| `description` | TEXT | Mô tả chi tiết | |
| `is_mandatory` | BOOLEAN | Bắt buộc hay không | `true`, `false` |
| `is_active` | BOOLEAN | Còn sử dụng không | `true` |

### **2. Bảng `crew_certificates`**

Lưu trữ **certificate cụ thể** của từng crew member.

| Cột | Kiểu | Mô tả | Ví dụ |
|-----|------|-------|-------|
| `id` | UUID | Primary Key | |
| `crew_member_id` | UUID | FK → crew_members | |
| `certificate_id` | UUID | FK → certificates | |
| `certificate_number` | VARCHAR(100) | Số chứng chỉ thực tế (UNIQUE) | `STCW-VN-123456` |
| `issue_date` | TIMESTAMP | Ngày cấp | `2024-01-15` |
| `expiry_date` | TIMESTAMP | Ngày hết hạn | `2029-01-15` |
| `issuing_authority` | VARCHAR(200) | Cơ quan cấp | `Vietnam Maritime Administration` |
| `document_file_path` | VARCHAR(500) | Đường dẫn file scan/photo | `/uploads/certificates/...` |
| `status` | VARCHAR(20) | Trạng thái | `VALID`, `EXPIRED`, `SUSPENDED` |
| `notes` | TEXT | Ghi chú | `Rank: Master 3000GT` |

---

## 🚀 HƯỚNG DẪN TRIỂN KHAI

### **Bước 1: Dừng server (nếu đang chạy)**

```bash
# Nếu chạy bằng docker
docker compose down

# Hoặc Ctrl+C nếu chạy bằng dotnet run
```

### **Bước 2: Chạy migration**

```bash
cd edge-services
dotnet ef database update --context EdgeDbContext
```

Migration sẽ:
- ✅ Xóa 5 cột certificate cũ trong `crew_members`
- ✅ Tạo bảng `certificates` 
- ✅ Tạo bảng `crew_certificates`
- ✅ Tạo indexes và foreign keys

### **Bước 3: Import seed data (Optional)**

```bash
# Kết nối database và chạy file SQL
psql -h localhost -U postgres -d maritime_edge -f SEED_CERTIFICATES.sql

# Hoặc dùng pgAdmin
# 1. Mở pgAdmin
# 2. Connect tới database maritime_edge
# 3. Tools → Query Tool
# 4. Open file SEED_CERTIFICATES.sql
# 5. Execute (F5)
```

Seed data sẽ thêm **23 loại certificate** phổ biến:
- Master, Chief Engineer, Chief Mate, etc.
- Medical Certificate
- Basic Safety, Advanced Fire Fighting
- GMDSS, Tanker Certificates
- Radar, ECDIS, etc.

### **Bước 4: Khởi động server**

```bash
dotnet run
# hoặc
docker compose up -d
```

---

## 📝 CÁC THAO TÁC THƯỜNG DÙNG

### **1. Lấy tất cả certificates của một crew member**

```sql
SELECT 
    cc.certificate_number,
    c.certificate_name,
    c.category,
    cc.issue_date,
    cc.expiry_date,
    cc.status,
    cc.issuing_authority
FROM crew_certificates cc
JOIN certificates c ON cc.certificate_id = c.id
WHERE cc.crew_member_id = '<crew-member-id>'
ORDER BY cc.expiry_date;
```

### **2. Tìm certificates sắp hết hạn (trong 90 ngày)**

```sql
SELECT 
    cm.full_name,
    cm.position,
    c.certificate_name,
    cc.certificate_number,
    cc.expiry_date,
    (cc.expiry_date - CURRENT_DATE) as days_remaining
FROM crew_certificates cc
JOIN crew_members cm ON cc.crew_member_id = cm.id
JOIN certificates c ON cc.certificate_id = c.id
WHERE cc.expiry_date BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '90 days'
  AND cc.status = 'VALID'
ORDER BY cc.expiry_date;
```

### **3. Tìm crew thiếu certificate bắt buộc**

```sql
SELECT 
    cm.id,
    cm.full_name,
    cm.position,
    c.certificate_name as missing_certificate
FROM crew_members cm
CROSS JOIN certificates c
WHERE c.is_mandatory = true
  AND NOT EXISTS (
    SELECT 1 FROM crew_certificates cc
    WHERE cc.crew_member_id = cm.id
      AND cc.certificate_id = c.id
      AND cc.status = 'VALID'
  )
ORDER BY cm.full_name, c.certificate_name;
```

### **4. Thêm certificate mới cho crew**

```sql
INSERT INTO crew_certificates (
    id,
    crew_member_id,
    certificate_id,
    certificate_number,
    issue_date,
    expiry_date,
    issuing_authority,
    status,
    notes
) VALUES (
    gen_random_uuid(),
    '<crew-member-id>',
    (SELECT id FROM certificates WHERE certificate_code = 'STCW_II_2'),
    'VN-MASTER-789456',
    '2025-01-01',
    '2030-01-01',
    'Vietnam Maritime Administration',
    'VALID',
    'Rank: Master unlimited. No restrictions.'
);
```

### **5. Cập nhật trạng thái certificate (khi hết hạn)**

```sql
-- Tự động update status EXPIRED cho certificates đã hết hạn
UPDATE crew_certificates
SET status = 'EXPIRED',
    updated_at = CURRENT_TIMESTAMP
WHERE expiry_date < CURRENT_DATE
  AND status = 'VALID';
```

### **6. Thêm loại certificate mới**

```sql
INSERT INTO certificates (
    id,
    certificate_code,
    certificate_name,
    category,
    validity_period_months,
    description,
    is_mandatory,
    is_active
) VALUES (
    gen_random_uuid(),
    'POLAR_WATER',
    'Polar Water Operations',
    'PROFICIENCY',
    60,
    'Basic training for ships operating in polar waters (STCW V/4)',
    false,
    true
);
```

---

## 🔄 MIGRATE DỮ LIỆU CŨ (Nếu có)

Nếu bạn có dữ liệu certificate cũ trong `crew_members`, chạy script này **TRƯỚC KHI** chạy migration:

```sql
-- Migrate STCW certificates
INSERT INTO crew_certificates (
    id,
    crew_member_id,
    certificate_id,
    certificate_number,
    issue_date,
    expiry_date,
    status
)
SELECT 
    gen_random_uuid(),
    cm.id,
    (SELECT id FROM certificates WHERE certificate_code = 'STCW_II_2' LIMIT 1),
    cm.certificate_number,
    COALESCE(cm.certificate_issue, CURRENT_DATE - INTERVAL '1 year'),
    cm.certificate_expiry,
    CASE 
        WHEN cm.certificate_expiry < CURRENT_DATE THEN 'EXPIRED'
        ELSE 'VALID'
    END
FROM crew_members cm
WHERE cm.certificate_number IS NOT NULL
  AND cm.certificate_expiry IS NOT NULL;

-- Migrate Medical certificates
INSERT INTO crew_certificates (
    id,
    crew_member_id,
    certificate_id,
    certificate_number,
    issue_date,
    expiry_date,
    status
)
SELECT 
    gen_random_uuid(),
    cm.id,
    (SELECT id FROM certificates WHERE certificate_code = 'MEDICAL' LIMIT 1),
    'MED-' || cm.crew_id,  -- Generate certificate number
    COALESCE(cm.medical_issue, CURRENT_DATE - INTERVAL '1 year'),
    cm.medical_expiry,
    CASE 
        WHEN cm.medical_expiry < CURRENT_DATE THEN 'EXPIRED'
        ELSE 'VALID'
    END
FROM crew_members cm
WHERE cm.medical_expiry IS NOT NULL;
```

---

## 🎨 UPDATE FRONTEND/API

### **API Endpoints cần cập nhật:**

#### **GET /api/crew/{id}/certificates**
```csharp
[HttpGet("{id}/certificates")]
public async Task<IActionResult> GetCrewCertificates(Guid id)
{
    var certificates = await _context.CrewCertificates
        .Include(cc => cc.Certificate)
        .Where(cc => cc.CrewMemberId == id)
        .OrderBy(cc => cc.ExpiryDate)
        .ToListAsync();
    
    return Ok(certificates);
}
```

#### **POST /api/crew/{id}/certificates**
```csharp
[HttpPost("{id}/certificates")]
public async Task<IActionResult> AddCrewCertificate(Guid id, [FromBody] CrewCertificate certificate)
{
    certificate.CrewMemberId = id;
    _context.CrewCertificates.Add(certificate);
    await _context.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetCrewCertificates), new { id }, certificate);
}
```

#### **GET /api/certificates/expiring**
```csharp
[HttpGet("expiring")]
public async Task<IActionResult> GetExpiringCertificates([FromQuery] int days = 90)
{
    var expiringDate = DateTime.UtcNow.AddDays(days);
    
    var certificates = await _context.CrewCertificates
        .Include(cc => cc.CrewMember)
        .Include(cc => cc.Certificate)
        .Where(cc => cc.ExpiryDate <= expiringDate && cc.Status == "VALID")
        .OrderBy(cc => cc.ExpiryDate)
        .ToListAsync();
    
    return Ok(certificates);
}
```

---

## ✅ CHECKLIST SAU KHI MIGRATE

- [ ] Migration chạy thành công
- [ ] Dữ liệu certificate cũ đã được migrate (nếu có)
- [ ] Seed data đã được import
- [ ] API endpoints đã được cập nhật
- [ ] Frontend đã được cập nhật để hiển thị nhiều certificates
- [ ] Test thêm/sửa/xóa certificate
- [ ] Test upload file certificate
- [ ] Test queries tìm kiếm certificate sắp hết hạn

---

## 🆘 TROUBLESHOOTING

### **Lỗi: Foreign key constraint**
```
ERROR: insert or update on table "crew_certificates" violates foreign key constraint
```
**Giải pháp**: Đảm bảo `crew_member_id` và `certificate_id` tồn tại trong database.

### **Lỗi: Unique constraint violation**
```
ERROR: duplicate key value violates unique constraint "idx_crew_cert_number_unique"
```
**Giải pháp**: Số certificate đã tồn tại. Sử dụng số certificate khác.

### **Rollback migration**
```bash
dotnet ef database update <previous-migration-name> --context EdgeDbContext
```

---

## 📞 LIÊN HỆ

Nếu có vấn đề, liên hệ team development hoặc tạo issue trên GitHub.

---

**Version:** 1.0  
**Date:** 2026-01-13  
**Author:** Development Team
