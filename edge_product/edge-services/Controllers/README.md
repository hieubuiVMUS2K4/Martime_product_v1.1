# 📁 Edge Services Controllers Structure

## 🎯 Tổng Quan

Controllers được tổ chức theo **domain-driven design**, phân chia rõ ràng theo nghiệp vụ để dễ dàng bảo trì và mở rộng.

---

## 📂 Cấu Trúc Thư Mục

```
Controllers/
├── Core/                    🔵 Hệ thống cốt lõi
├── Safety/                  🚨 An toàn & Tuân thủ
├── Crew/                    👥 Quản lý thuyền viên
├── Maintenance/             🔧 Bảo trì & Thiết bị
├── Inventory/               📦 Vật tư & Nhiên liệu
├── Voyage/                  🗺️ Hành trình & Telemetry
├── Logbooks/                📓 Sổ nhật ký
├── Reporting/               📊 Báo cáo
└── Testing/                 🧪 Kiểm thử
```

---

## 📋 Chi Tiết Từng Module

### 🔵 **Core/** - Hệ Thống Cốt Lõi
**Namespace:** `MaritimeEdge.Controllers.Core`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `AuthController` | `/api/auth` | Xác thực & phân quyền |
| `DashboardController` | `/api/dashboard` | Dashboard tổng quan |
| `SyncController` | `/api/sync` | Đồng bộ dữ liệu với shore |
| `HealthController` | `/api/health` | Health check |
| `SystemController` | `/api/system` | Cấu hình hệ thống |

---

### 🚨 **Safety/** - An Toàn & Tuân Thủ
**Namespace:** `MaritimeEdge.Controllers.Safety`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `AlarmsController` | `/api/alarms` | Cảnh báo an toàn |
| `ComplianceController` | `/api/compliance` | Tuân thủ quy định IMO |
| `DeferralRequestController` | `/api/deferral-requests` | Yêu cầu hoãn bảo trì |

**Tiêu chuẩn:** SOLAS, MARPOL, ISM Code

---

### 👥 **Crew/** - Quản Lý Thuyền Viên
**Namespace:** `MaritimeEdge.Controllers.Crew`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `CrewController` | `/api/crew` | Quản lý thuyền viên |
| `CertificatesController` | `/api/certificates` | Chứng chỉ hành nghề |
| `CountriesController` | `/api/countries` | Danh sách quốc gia |
| `CountryCertificatesController` | `/api/country-certificates` | Chứng chỉ theo quốc gia |

**Tuân thủ:** STCW Convention

---

### 🔧 **Maintenance/** - Bảo Trì & Thiết Bị
**Namespace:** `MaritimeEdge.Controllers.Maintenance`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `MaintenanceController` | `/api/maintenance` | Quản lý công việc bảo trì |
| `MaintenanceScheduleController` | `/api/maintenance-schedules` | Lịch bảo trì định kỳ |
| `EquipmentAssetController` | `/api/equipment-assets` | Danh mục thiết bị |
| `EquipmentGroupController` | `/api/equipment-groups` | Nhóm thiết bị |
| `TaskWorkflowController` | `/api/maintenance/workflows` | Quy trình công việc |
| `TaskChecklistItemsController` | `/api/maintenance/tasks/{id}/checklist` | Checklist công việc |
| `TaskRealTimeUpdatesController` | `/api/maintenance/tasks/{id}` | Cập nhật realtime |

**Hệ thống:** PMS (Planned Maintenance System)

---

### 📦 **Inventory/** - Vật Tư & Nhiên Liệu
**Namespace:** `MaritimeEdge.Controllers.Inventory`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `MaterialController` | `/api/materials` | Quản lý vật tư |
| `MaterialReceiptsController` | `/api/material-receipts` | Phiếu nhập/xuất |
| `FuelAnalyticsController` | `/api/fuel/analytics` | Phân tích nhiên liệu |

**Tính năng:** Stock management, Fuel optimization

---

### 🗺️ **Voyage/** - Hành Trình & Telemetry
**Namespace:** `MaritimeEdge.Controllers.Voyage`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `VoyageController` | `/api/voyages` | Quản lý chuyến đi |
| `VoyageLogController` | `/api/voyage-logs` | Nhật ký hành trình |
| `TelemetryController` | `/api/telemetry` | Dữ liệu cảm biến realtime |

**Tích hợp:** GPS, AIS, Engine sensors

---

### 📓 **Logbooks/** - Sổ Nhật Ký
**Namespace:** `MaritimeEdge.Controllers.Logbooks`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `DeckLogbookController` | `/api/logbooks/deck` | Sổ boong (SOLAS V) |
| `EngineLogbookController` | `/api/logbooks/engine` | Sổ máy |
| `BallastWaterController` | `/api/logbooks/ballast` | Sổ nước dằn |
| `OilRecordController` | `/api/logbooks/oil` | Sổ dầu (MARPOL) |
| `GarbageRecordController` | `/api/logbooks/garbage` | Sổ rác thải |
| `WatchkeepingController` | `/api/logbooks/watchkeeping` | Sổ trực ca |

**Tuân thủ:** SOLAS Chapter V, MARPOL Annex I/V

---

### 📊 **Reporting/** - Báo Cáo
**Namespace:** `MaritimeEdge.Controllers.Reporting`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `ReportingController` | `/api/reports` | Tạo báo cáo tùy chỉnh |
| `AggregateReportController` | `/api/reports/aggregate` | Báo cáo tổng hợp |

**Export:** PDF, Excel, CSV

---

### 🧪 **Testing/** - Kiểm Thử & Development
**Namespace:** `MaritimeEdge.Controllers.Testing`

| Controller | Route | Mô tả |
|-----------|-------|-------|
| `TestDataController` | `/api/test-data` | Seed dữ liệu mẫu |
| `SignalKTestController` | `/api/signalk/test` | Test SignalK integration |

**⚠️ Chỉ dùng trong môi trường Development**

---

## 🔄 Migration Guide

### Trước Refactoring
```csharp
namespace MaritimeEdge.Controllers;
```

### Sau Refactoring
```csharp
namespace MaritimeEdge.Controllers.Core;      // Core controllers
namespace MaritimeEdge.Controllers.Safety;    // Safety controllers
namespace MaritimeEdge.Controllers.Crew;      // Crew controllers
// ...etc
```

---

## ✅ Lợi Ích

1. **Tìm kiếm nhanh** - Dễ dàng locate controller theo domain
2. **Bảo trì tốt** - Thay đổi 1 module không ảnh hưởng module khác
3. **Onboarding dễ** - Developer mới hiểu structure ngay lập tức
4. **Scalable** - Dễ thêm module mới
5. **Code review** - Review theo từng domain riêng biệt

---

## 📌 Quy Tắc Đặt Tên

- **Controller name:** `{Entity}Controller.cs` (VD: `CrewController.cs`)
- **Route prefix:** `/api/{entity-plural}` (VD: `/api/crew`, `/api/materials`)
- **Namespace:** `MaritimeEdge.Controllers.{ModuleName}`

---

## 🚀 Next Steps

- [ ] Thêm XML documentation cho tất cả endpoints
- [ ] Implement API versioning
- [ ] Thêm integration tests theo module
- [ ] Setup Swagger groups theo module

---


