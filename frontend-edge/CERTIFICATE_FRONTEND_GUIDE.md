# 📜 CERTIFICATE MANAGEMENT SYSTEM - FRONTEND

## 🎯 TỔNG QUAN

Hệ thống quản lý chứng chỉ với giao diện hoàn chỉnh đã được tạo.

### ✅ ĐÃ TẠO:

1. **Types mới** trong `maritime.types.ts`:
   - `Certificate` - Loại certificate (master data)
   - `CrewCertificate` - Certificate cụ thể của crew member
   - Updated `CrewMember` - Xóa fields certificate cũ, thêm relation

2. **Certificate Management Page** (`CertificateManagementPage.tsx`):
   - 📋 **List View**: Hiển thị tất cả loại certificates
     - Table với đầy đủ thông tin
     - Category badges (COMPETENCY, MEDICAL, PROFICIENCY)
     - Mandatory/Optional tags
     - Active/Inactive status
   - 👥 **Details View**: Khi click vào certificate
     - Thông tin chi tiết certificate type
     - Danh sách crew có certificate đó
     - Status tracking (VALID, WARNING, CRITICAL, EXPIRED)
     - Days remaining countdown
   
3. **Navigation**: 
   - Button "Certificate Management" trong CrewPage
   - Route `/crew/certificates`

---

## 🖼️ GIAO DIỆN

### **1. Certificate List View**

```
┌─────────────────────────────────────────────────────────────────┐
│ 🏆 Certificate Management                     [← Back to List]  │
│ Quản lý các loại chứng chỉ hàng hải                             │
├─────────────────────────────────────────────────────────────────┤
│ Stats Cards:                                                     │
│ ┌──────────┬──────────┬──────────┬──────────┐                 │
│ │  Total   │ Mandatory│  Active  │Categories│                 │
│ │    7     │    4     │    7     │    3     │                 │
│ └──────────┴──────────┴──────────┴──────────┘                 │
├─────────────────────────────────────────────────────────────────┤
│ Certificate Types                        [+ Add Certificate Type]│
├─────────────────────────────────────────────────────────────────┤
│ Name                    │ Code      │ Category   │ Valid │ ... │
├─────────────────────────┼───────────┼────────────┼───────┼─────┤
│ Certificate of Comp...  │ STCW_II_2 │ COMPETENCY │ 60m   │ ... │
│ Chief Engineer          │ STCW_III_2│ COMPETENCY │ 60m   │ ... │
│ Medical Certificate     │ MEDICAL   │ MEDICAL    │ 24m   │ ... │
│ Basic Safety Training   │ BASIC...  │ PROFICIENCY│ 60m   │ ... │
│ ...                     │           │            │       │     │
└─────────────────────────────────────────────────────────────────┘
```

### **2. Certificate Details View (Sau khi click)**

```
┌─────────────────────────────────────────────────────────────────┐
│ 🏆 Certificate Management                     [← Back to List]  │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 🏆 Certificate of Competency - Master                       │ │
│ │    Code: STCW_II_2                                          │ │
│ │                                                             │ │
│ │ STCW Regulation II/2 - Certificate of Competency as Master │ │
│ │                                                             │ │
│ │ Category: [COMPETENCY] Valid: 60 months Mandatory: Yes     │ │
│ └─────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│ 👥 Crew Members with This Certificate (3)  [+ Add Cert to Crew]│
├─────────────────────────────────────────────────────────────────┤
│ Crew      │ Cert Number  │ Issue Date │ Expiry     │ Status    │
├───────────┼──────────────┼────────────┼────────────┼───────────┤
│ John S.   │ VN-M-123456  │ 15 Jan 24  │ 15 Jan 29  │ ✓ VALID   │
│ Master    │              │            │ 1095 days  │           │
├───────────┼──────────────┼────────────┼────────────┼───────────┤
│ Michael C.│ VN-M-789456  │ 20 Jun 23  │ 20 Jun 28  │ ⚠ WARNING │
│ C.Officer │              │            │ 888 days   │           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔧 TÍCH HỢP API

### **API Endpoints cần implement:**

```typescript
// In maritime.service.ts

export const certificateService = {
  // Certificate Types (Master Data)
  getAllCertificates: () => 
    apiClient.get<Certificate[]>('/certificates'),
  
  getCertificateById: (id: string) => 
    apiClient.get<Certificate>(`/certificates/${id}`),
  
  createCertificate: (cert: Partial<Certificate>) => 
    apiClient.post<Certificate>('/certificates', cert),
  
  updateCertificate: (id: string, cert: Partial<Certificate>) => 
    apiClient.put<Certificate>(`/certificates/${id}`, cert),
  
  // Crew Certificates
  getCrewCertificates: (crewId: string) => 
    apiClient.get<CrewCertificate[]>(`/crew/${crewId}/certificates`),
  
  addCrewCertificate: (crewId: string, cert: Partial<CrewCertificate>) => 
    apiClient.post<CrewCertificate>(`/crew/${crewId}/certificates`, cert),
  
  updateCrewCertificate: (certId: string, cert: Partial<CrewCertificate>) => 
    apiClient.put<CrewCertificate>(`/crew-certificates/${certId}`, cert),
  
  deleteCrewCertificate: (certId: string) => 
    apiClient.delete(`/crew-certificates/${certId}`),
  
  // Get crew by certificate type
  getCrewByCertificate: (certificateId: string) => 
    apiClient.get<(CrewCertificate & { crewMember: CrewMember })[]>(
      `/certificates/${certificateId}/crew`
    ),
  
  // Get expiring certificates
  getExpiringCertificates: (days: number = 90) => 
    apiClient.get<CrewCertificate[]>(`/crew-certificates/expiring?days=${days}`)
}
```

---

## 🚀 CÁCH SỬ DỤNG

### **1. Truy cập Certificate Management**

```
Crew Page → Click "Certificate Management" button
hoặc
Navigate trực tiếp: http://localhost:5173/crew/certificates
```

### **2. Xem danh sách certificates**

- Hiển thị tất cả loại certificates
- Filter theo category
- Xem mandatory/optional
- Active/Inactive status

### **3. Click vào certificate để xem chi tiết**

- Thông tin đầy đủ về loại certificate
- Danh sách crew có certificate đó
- Status mỗi certificate (days remaining)
- Click "View Details →" để xem crew member profile

### **4. Quay lại list**

- Click "← Back to List" button

---

## 📝 CẬP NHẬT BACKEND API

Bạn cần tạo các API endpoints trong C#:

### **CertificatesController.cs**

```csharp
[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly EdgeDbContext _context;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var certificates = await _context.Certificates
            .Where(c => c.IsActive)
            .OrderBy(c => c.CertificateName)
            .ToListAsync();
        
        return Ok(certificates);
    }
    
    [HttpGet("{id}/crew")]
    public async Task<IActionResult> GetCrewByCertificate(Guid id)
    {
        var crewCertificates = await _context.CrewCertificates
            .Include(cc => cc.CrewMember)
            .Include(cc => cc.Certificate)
            .Where(cc => cc.CertificateId == id)
            .OrderBy(cc => cc.ExpiryDate)
            .ToListAsync();
        
        return Ok(crewCertificates);
    }
}
```

### **CrewCertificatesController.cs**

```csharp
[ApiController]
[Route("api/crew-certificates")]
public class CrewCertificatesController : ControllerBase
{
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
}
```

---

## 🎨 FEATURES HIỆN CÓ

✅ **List View**:
- Table hiển thị tất cả certificate types
- Category badges với màu sắc
- Mandatory/Optional indicators
- Active/Inactive status
- Click to view details

✅ **Details View**:
- Thông tin chi tiết certificate type
- Danh sách crew có certificate
- Status tracking với màu sắc:
  - 🟢 VALID (>90 days)
  - 🟡 WARNING (30-90 days)
  - 🔴 CRITICAL (<30 days)
  - ⚫ EXPIRED (past due)
- Days remaining countdown
- Navigate to crew profile

✅ **Stats Cards**:
- Total certificates
- Mandatory count
- Active count
- Categories count

---

## 🔜 TÍNH NĂNG CẦN BỔ SUNG

### **Phase 1 - CRUD Operations**
- [ ] Add new certificate type modal
- [ ] Edit certificate type
- [ ] Delete certificate type (với confirmation)
- [ ] Add certificate to crew modal
- [ ] Edit crew certificate
- [ ] Upload certificate document/photo

### **Phase 2 - Advanced Features**
- [ ] Search & filter certificates
- [ ] Sort by columns
- [ ] Export certificate report (PDF/Excel)
- [ ] Bulk upload certificates
- [ ] Certificate renewal notifications
- [ ] Email alerts for expiring certificates

### **Phase 3 - Dashboard Integration**
- [ ] Certificate expiry dashboard widget
- [ ] Compliance overview
- [ ] Crew without mandatory certificates report
- [ ] Certificate history tracking

---

## 📸 SCREENSHOTS

(Khi chạy app, có thể chụp screenshots và thêm vào đây)

---

## ✅ TESTING CHECKLIST

- [ ] Navigate to `/crew/certificates`
- [ ] Click certificate in list → View details
- [ ] Click "Back to List" → Return to list view
- [ ] Stats cards display correct numbers
- [ ] Certificate status colors correct
- [ ] Days remaining calculation accurate
- [ ] Click "View Details →" navigates to crew profile
- [ ] Responsive design works on mobile

---

**Version:** 1.0  
**Date:** 2026-01-13  
**Author:** Development Team
