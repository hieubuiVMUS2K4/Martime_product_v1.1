# Voyage Context & Crew-Voyage Linking

> **Ngày tạo:** 21/02/2026 | **Branch:** feature/tinhhash | **Status:** � Backend hoàn thành

## Vấn đề

- Crew không link với voyage cụ thể
- Không tracking port arrival/departure (chỉ 1 cặp string trên VoyageRecord)
- Thiếu vessel info (IMO, flag state) trên voyage
- Không có Port master data (UN/LOCODE)
- VoyageLogEntry.VoyageId không config FK relationship

## Chuẩn quốc tế

| Chuẩn | Yêu cầu |
|-------|---------|
| **FAL Form 5** | Crew List gắn Voyage No, Port of Arrival, IMO No, Flag State |
| **MLC 2006** | Sea service record: embark/disembark theo voyage |
| **SOLAS Ch.V** | Voyage data recording, crew on board per voyage |
| **UN/LOCODE** | Mã cảng chuẩn quốc tế (VD: VNSGN, SGSIN) |

## Thiết kế

```
VoyageRecord (mở rộng)
  ├── PortCall[]              ← NEW
  ├── VoyageCrewAssignment[]  ← NEW
  ├── VoyageLogEntry[]        ← FIX FK
  └── CargoOperation[]        ← FIX FK

Port (master data)            ← NEW (int PK, UN/LOCODE)
```

## Checklist triển khai

### Backend (edge-services)

- [x] **B1.** Tạo model `Port` — master data cảng, int PK, UN/LOCODE
- [x] **B2.** Tạo model `PortCall` — mỗi lần ghé cảng trong voyage
- [x] **B3.** Tạo model `VoyageCrewAssignment` — crew gắn voyage
- [x] **B4.** Mở rộng `VoyageRecord` — thêm vessel info, navigation properties
- [x] **B5.** Cập nhật `EdgeDbContext` — DbSet, relationships, indexes, seed data
- [x] **B6.** Tạo EF Migration — `VoyageCrewPortLinking`
- [x] **B7.** Tạo DTOs — VoyageDtos.cs (Port, PortCall, CrewAssignment, FAL Form 5)
- [x] **B8.** Tạo `IVoyageManagementService` / `VoyageManagementService`
- [x] **B9.** Tạo `PortController` — CRUD + search by LOCODE
- [x] **B10.** Cập nhật `VoyageController` — port calls, crew assignments, FAL Form 5
- [x] **B11.** Seed data ~80 cảng phổ biến (châu Á, EU, US)
- [x] **B12.** Build & verify — 0 errors

### Frontend (frontend-edge) — Phase 2

- [x] **F1.** Trang Port Management (admin) — `PortManagementPage.tsx`
- [x] **F2.** Voyage detail → tab Port Calls — `VoyagePage.tsx` (PortCallsTab)
- [x] **F3.** Voyage detail → tab Crew Assignments — `VoyagePage.tsx` (CrewAssignmentsTab)
- [x] **F4.** Crew detail → Voyage history — `CrewDetailPage.tsx` (voyage-history tab)

### Bổ sung Frontend

- [x] Tạo `voyage.types.ts` — TypeScript interfaces cho Port, PortCall, VoyageCrewAssignment, VoyageDetail, FAL Form 5
- [x] Tạo `voyage.service.ts` — API client (ports, voyages, portCalls, crewAssignments)
- [x] Thêm route `/ports` → `PortManagementPage` trong App.tsx
- [x] Thêm "Ports" navigation item trong Sidebar.tsx
- [x] Thêm i18n keys `nav.ports` trong en.json / vi.json
- [x] Build verify — 0 TypeScript errors trong các file mới/sửa

## Ghi chú kỹ thuật

- Port dùng `int` PK (hiệu năng JOIN tốt hơn GUID cho master data)
- Giữ backward compatible: VoyageRecord.DeparturePort/ArrivalPort (string) vẫn giữ, thêm mới PortCode
- Composite index: `(VoyageId, Sequence)` cho PortCall, `(VoyageId, CrewMemberId)` cho Assignment
- VoyageRecord snapshot vessel info từ appsettings (không FK) — vì vessel info có thể thay đổi

## Tiến độ

| Ngày | Nội dung | Người thực hiện |
|------|----------|----------------|
| 21/02 | Lên kế hoạch | — |
| 21/02 | Hoàn thành B1-B12 (Backend) | Copilot |
| 21/02 | Hoàn thành F1-F4 + routes/nav/i18n (Frontend) | Copilot |
