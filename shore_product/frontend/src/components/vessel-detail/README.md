# components/vessel-detail/ — Các tab dữ liệu kỹ thuật tàu

## Mục đích

12 component, mỗi component là **một tab** trong `pages/VesselManagement/VesselDetailPage.tsx` — trang hồ sơ chi tiết một con tàu. Tách nhỏ theo nhóm dữ liệu (kích thước, máy móc, chủ tàu, bảo hiểm...) để `VesselDetailPage.tsx` không phình thành một file khổng lồ.

## Cấu trúc & vai trò

| File | Tab hiển thị | Kiểu field (từ `components/vessel/VesselDataFields`) | Nguồn dữ liệu |
|---|---|---|---|
| `VesselOverviewTab.tsx` | Tổng quan | *(tự viết UI riêng, không dùng VesselDataFields)* | Gọi thẳng API: alerts an toàn (`SafetyAlert`), sự kiện máy (`EngineEventItem`), tổng hợp `AlertsSummary` — 24h gần nhất, engine start/stop. |
| `BasicDataTab.tsx` | Dữ liệu cơ bản (IMO, official number, call sign, MMSI, cờ, tên cũ, port of registry...) | `SectionCard`, `ReadOnlyField` | Field nhận qua prop `vessel` (dữ liệu đã fetch sẵn ở `VesselDetailPage`). |
| `DimensionsTab.tsx` | Kích thước (LOA, LBP, breadth, depth, draft, block coefficient, tonnage...) | `SectionCard`, `ReadOnlyField`, `MetricFieldReadOnly` | như trên — nhiều field dùng `MetricFieldReadOnly` để hiện cả mét lẫn ft/in. |
| `MachineryTab.tsx` | Máy móc (anchor chain, harbour generator, azimuth engine...) | `SectionCard`, `ReadOnlyField`, `PowerFieldReadOnly` | như trên. |
| `ShipownerTab.tsx` | Chủ tàu (tên, địa chỉ, liên hệ, managing owner) | `SectionCard`, `EditableField` | như trên — dùng `EditableField` (không chỉ read-only) vì đây là **dữ liệu thương mại thuộc quyền Shore** (xem README gốc mục 2.3/6.4). |
| `ChartererTab.tsx` | Người thuê tàu (charterer, bareboat charterer) | `SectionCard`, `EditableField` | như trên. |
| `ClassFlagStateTab.tsx` | Đăng kiểm & quốc gia treo cờ (class society, flag state) | `SectionCard`, `ReadOnlyField` | như trên. |
| `InsuranceTab.tsx` | Bảo hiểm (P&I Club, H&M Club) | `SectionCard`, `EditableField` | như trên. |
| `RadioCommTab.tsx` | Thông tin liên lạc vô tuyến (Inmarsat, GSM, sea area A1-A4, AIS) | `SectionCard`, `ReadOnlyField`, `CheckboxDisplay` | như trên. |
| `TanksCargoTab.tsx` | Két/hầm hàng (HFO/MDO/nước ngọt/ballast theo m³, TEU on/under deck, grain/bale capacity) | `SectionCard`, `ReadOnlyField` | như trên. |
| `VesselCrewTab.tsx` | Thuyền viên đang trên tàu | *(tự fetch riêng)* | Gọi `crewApi` (`services/crew.service.ts`) lọc theo `vesselId`; tái dùng CSS của `CrewListPage.css`. |
| `VesselCertificateTab.tsx` | Chứng chỉ tàu (vessel certificate — khác certificate của crew) | *(tự fetch riêng)* | Gọi thẳng `${ENV.API_BASE_URL}/vessels/.../certificates`-kiểu endpoint; tái dùng CSS của `VesselsPage.css`. |

## Luồng hoạt động chính

```
pages/VesselManagement/VesselDetailPage.tsx
   │  fetch 1 lần: GET /vessels/:id  → toàn bộ field kỹ thuật của tàu (object "Vessel" rất lớn, 80+ field)
   │  state: activeTab
   ▼
   switch(activeTab) { case 'basic': <BasicDataTab vessel={vessel}/> ... }
        │
        ▼  hầu hết tab CHỈ NHẬN "vessel" QUA PROP — không tự gọi API
   components/vessel-detail/BasicDataTab.tsx (v.v.)
        │  render <SectionCard><ReadOnlyField label="IMO" value={vessel.imo}/>...</SectionCard>
```

Ngoại lệ: `VesselOverviewTab`, `VesselCrewTab`, `VesselCertificateTab` **tự fetch dữ liệu riêng** (không chỉ nhận qua prop `vessel`), vì dữ liệu của chúng (alerts, crew list, certificate list) không nằm trong response `GET /vessels/:id` ban đầu.

`VesselDetailPage.tsx` còn nhúng thêm 6 trang PMS/Materials khác làm "tab" (không nằm trong thư mục này, nhưng cùng cơ chế tab): `AssetsPage`, `WorkPlanningPage`, `MaterialPage`, `MaterialRequestPage`, `StockReceiptPage`, `InventoryPage` — biến trang chi tiết tàu thành một "workspace" thu nhỏ cho toàn bộ nghiệp vụ PMS/Materials **lọc theo đúng con tàu đó**.

## Liên kết với phần khác

- **components/vessel/VesselDataFields.tsx**: cung cấp `SectionCard`/`ReadOnlyField`/`EditableField`/`MetricFieldReadOnly`/`PowerFieldReadOnly`/`CheckboxDisplay` cho 8/12 tab ở đây (xem `components/vessel/README.md`).
- **pages/VesselManagement/VesselDetailPage.tsx**: nơi duy nhất render các tab này, quyết định `vessel` object truyền xuống.
- **pages/PMS/**, **pages/Materials/**: được nhúng làm tab bổ sung trong cùng trang chi tiết tàu.
- **services/crew.service.ts**: hậu thuẫn `VesselCrewTab`.

## Ghi chú khi đọc/dạy

- Đa số tab (8/12) là "form hiển thị dữ liệu" thuần tuý — không có logic phức tạp, chỉ khai báo field nào hiển thị dưới field nào. Đọc 1 tab (vd `DimensionsTab.tsx`) là đủ hiểu format chung của cả nhóm.
- 3 tab còn lại (`Overview`, `Crew`, `Certificate`) **khác hẳn về kiến trúc** — tự quản lý `loading`/`useEffect`/gọi API riêng, giống một trang con nhỏ hơn là một "tab hiển thị field". Đừng áp khuôn mẫu của `BasicDataTab` khi đọc 3 tab này.
- Phân chia `EditableField` (Shipowner/Charterer/Insurance — dữ liệu **thương mại**) vs `ReadOnlyField` (Basic/Dimensions/Machinery/ClassFlagState/RadioComm/TanksCargo — phần lớn là dữ liệu **kỹ thuật**) phản ánh đúng nguyên tắc "quyền sở hữu dữ liệu Hybrid" cho Vessel đã nêu ở README gốc (mục 6.4: Edge sở hữu kỹ thuật, Shore sở hữu thương mại) — dù vậy đây là quy ước theo tên field trong code, không phải một cơ chế phân quyền field-level chính thức được backend enforce.
- Tên `VesselCertificateTab` dễ nhầm với chứng chỉ **thuyền viên** (`crew certificate`, quản lý ở `pages/CrewManagement/CertificateMonitorPage.tsx`) — đây là chứng chỉ của **con tàu** (class, statutory certificates...), hoàn toàn khác đối tượng.
