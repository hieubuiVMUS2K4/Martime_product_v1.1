# KẾ HOẠCH TRIỂN KHAI MODULE SHIP'S DATA

> Ngày tạo: 25/02/2026  
> Module: Ship's Data (Quản lý thông tin tàu)  
> Vị trí: Shore Frontend (`frontend/`)  
> Backend: Shore Backend (`backend/`)

---

## 1. TỔNG QUAN

Module **Ship's Data** cho phép quản lý toàn bộ thông tin chi tiết của tàu, bao gồm 8 tab chính:

| # | Tab | Mô tả |
|---|-----|-------|
| 1 | **Basic Data** | Thông tin cơ bản của tàu |
| 2 | **Dimensions** | Kích thước, trọng tải |
| 3 | **Machinery** | Máy móc, động cơ, thiết bị |
| 4 | **Shipowner** | Chủ tàu, quản lý, vận hành, nhân sự an ninh |
| 5 | **Charterer** | Người thuê tàu |
| 6 | **Class / Flag State** | Đăng kiểm và cờ tàu |
| 7 | **Insurance** | Bảo hiểm |
| 8 | **Radio Communication Equipment** | Thiết bị thông tin liên lạc vô tuyến |
| 9 | **Tanks & Cgo Spaces** | Két chứa và khoang hàng |

**Layout:** Header "SHIP'S DATA" + nút Save (góc phải), navigation tabs ngang phía dưới, nội dung tab hiển thị bên dưới.

---

## 2. CHI TIẾT CÁC TAB

---

### 2.1. Tab: Basic Data ⭐

Thông tin nhận dạng cơ bản của tàu. Các trường có dấu **\*** là bắt buộc.

#### Hàng 1

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | IMO Number * | `string` | Ví dụ: "0000013" |
| 2 | Official Number | `string` | Ví dụ: "232325" |
| 3 | Call Sign | `string` | Ví dụ: "ABCDEFG" |

#### Hàng 2

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 4 | Ship's Name * | `string` | Ví dụ: "VIKING SPIRIT" |
| 5 | Flag * | `dropdown` | Ví dụ: SPAIN (có nút X xóa, ▽ mở) |
| 6 | Port of Registry * | `dropdown` | Ví dụ: ALGECIRAS (có nút X xóa, ▽ mở) |

#### Hàng 3

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 7 | Previous Name | `string` | Ví dụ: "MARLEEN 2" |
| 8 | Previous Flag | `dropdown` | Ví dụ: DENMARK (có nút X xóa, ▽ mở) |
| 9 | *(trống)* | | |
| 10 | MMSI Number | `string` | Ví dụ: "112233445" |

#### Hàng 4

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 11 | Type of vsl | `dropdown` | Ví dụ: "Full container ship/cellular vess" (▽ mở) |
| 12 | Class Notation | `string` | Ví dụ: "+ET66177-S4" |
| 13 | *(trống)* | | |
| 14 | Class Register Number | `string` | Ví dụ: "283761" |

#### Hàng 5

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 15 | Shipyard/Country | `dropdown` | Ví dụ: CANADA (có nút X xóa, ▽ mở) |
| 16 | Shipyard/Name | `string` | Ví dụ: "NUWETT" |
| 17 | Yard No | `string` | Ví dụ: "W-2827" |
| 18 | Company IMO Number | `string` | Ví dụ: "2736966" |
| 19 | Suez Canal ID Number | `string` | Ví dụ: "232324" |

#### Hàng 6

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 20 | Keel Laid Date | `date` | Ví dụ: 14/11/2014 (có date picker 📅) |
| 21 | Year of Built | `number` | Ví dụ: 2015 |
| 22 | Date of Registry | `date` | Ví dụ: 23/09/2015 (có date picker 📅) |
| 23 | Owner IMO Number | `string` | Ví dụ: "4545453" |
| 24 | Panama Canal ID Number | `string` | Ví dụ: "2345" |

#### Hàng 7 (section dưới, cách 1 khoảng trống)

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 25 | Max Persons Allowed O/B | `number` | Ví dụ: 32 |
| 26 | *(trống)* | | |
| 27 | Service speed (kts) | `number` | Ví dụ: 19 |
| 28 | *(trống)* | | |
| 29 | VRP – Vessel Response Plan Number | `string` | Ví dụ: "2219" |
| 30 | VRP Type (Required for US E-NOAID) | `dropdown` | Ví dụ: NONTANK (có nút X xóa, ▽ mở) |

#### Hàng 8

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 31 | No of Crew as per Safe Manning | `number` | Ví dụ: 14 |

#### Hàng 9

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 32 | Max Passengers Allowed O/B | `number` | Ví dụ: 12 |

#### Bố cục tổng thể tab Basic Data

```
┌───────────────────┬───────────────────┬───────────────────┬───────────────────┬───────────────────┐
│ IMO Number *      │ Official Number   │ Call Sign         │                   │                   │
├───────────────────┼───────────────────┼───────────────────┤                   │                   │
│ Ship's Name *     │ Flag * ×▽        │ Port of Registry *│                   │                   │
│                   │                   │ ×▽               │                   │                   │
├───────────────────┼───────────────────┼───────────────────┼───────────────────┤                   │
│ Previous Name     │ Previous Flag ×▽ │                   │ MMSI Number       │                   │
├───────────────────┼───────────────────┼───────────────────┼───────────────────┤                   │
│ Type of vsl ▽    │ Class Notation    │                   │ Class Register No │                   │
├───────────────────┼───────────────────┼───────────────────┼───────────────────┼───────────────────┤
│ Shipyard/Country  │ Shipyard/Name     │ Yard No           │ Company IMO No    │ Suez Canal ID No  │
│ ×▽               │                   │                   │                   │                   │
├───────────────────┼───────────────────┼───────────────────┼───────────────────┼───────────────────┤
│ Keel Laid Date 📅 │ Year of Built     │ Date of Registry📅│ Owner IMO Number  │ Panama Canal ID No│
├───────────────────┴───────────────────┴───────────────────┴───────────────────┴───────────────────┤
│                                                                                                   │
├───────────────────┬───────────────────┬───────────────────┬───────────────────┬───────────────────┤
│ Max Persons O/B   │                   │ Service speed(kts)│ VRP Plan Number   │ VRP Type ×▽      │
├───────────────────┤                   ├───────────────────┤                   │                   │
│ No Crew Safe Man  │                   │                   │                   │                   │
├───────────────────┤                   │                   │                   │                   │
│ Max Passengers O/B│                   │                   │                   │                   │
└───────────────────┴───────────────────┴───────────────────┴───────────────────┴───────────────────┘
```

---

### 2.2. Tab: Dimensions ⭐

Tab phức tạp với nhiều section, tất cả trường `m → ft` đều có auto-convert (nhập m, tự tính ft). Có nút ⊙ info tooltip.

#### 2.2.1. Section trên — Kích thước chính (Grid 6 cột)

**Hàng 1:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | LOA (m → ft) | `number` + `auto-calc` | Ví dụ: 321.76 m → 1055'7" |
| 2 | Depth moulded (m → ft) | `number` + `auto-calc` | Có nút ⊙ info |
| 3 | H – Max Airdraft (m → ft) | `number` + `auto-calc` | Ví dụ: 65.11 m → 213'7". Có nút ⊙ info |
| 4 | Parallel body (Ballast) (m → ft) | `number` + `auto-calc` | Ví dụ: 156.87 m → 514'7" |
| 5 | Parallel body (Loaded) (m → ft) | `number` + `auto-calc` | Ví dụ: 199.33 m → 653'11" |

**Hàng 2:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 6 | LBP (m → ft) | `number` + `auto-calc` | Ví dụ: 301.33 m → 988'7" |
| 7 | Draft moulded (m → ft) | `number` + `auto-calc` | Ví dụ: 12.55 m → 41'2". Có nút ⊙ info |
| 8 | D – Distance (m → ft) | `number` + `auto-calc` | Ví dụ: 88.94 m → 291'9". Có nút ⊙ info |
| 9 | Bridge to Aft (m → ft) | `number` + `auto-calc` | Ví dụ: 103.2 m → 338'6" |
| 10 | Bridge to Bow (m → ft) | `number` + `auto-calc` | Ví dụ: 221.68 m → 727'3" |
| 11 | Bow to Bulbous Bow (m → ft) | `number` + `auto-calc` | Ví dụ: 5.66 m → 18'6" |

**Hàng 3:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 12 | Breadth moulded (m → ft) | `number` + `auto-calc` | Ví dụ: 48.31 m → 158'5". Có nút ⊙ info |
| 13 | Draft Scantling (m → ft) | `number` + `auto-calc` | Ví dụ: 12.77 m → 41'10". Có nút ⊙ info |
| 14 | Airdraft Reduction (Mast Fouled) (m) | `number` | Ví dụ: 3.82 |
| 15 | *(trống)* | | |
| 16 | Light Ship (mt) | `number` | Ví dụ: 10223 |

**Hàng 4:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 17 | *(trống)* | | |
| 18 | Draft Full Ballast (m → ft) | `number` + `auto-calc` | Ví dụ: 8.42 m → 27'7". Có nút ⊙ info |
| 19 | ☐ N/A | `checkbox` | Checkbox N/A (disable block coefficient nếu tích) |
| 20 | Block Coefficient | `number` | Ví dụ: 0.73 |
| 21 | TPC at Summer Draft (mt) | `number` | Ví dụ: 199 |
| 22 | Fresh Water Allowance – FWA (mm) | `number` | Ví dụ: 264 |

#### 2.2.2. LOAD LINES PARTICULARS (Bảng)

Bảng dạng table với 6 load line types:

| Type | Draft (m) | Draft (ft") | Freeboard (m) | Freeboard (ft") | Displacement (mt) | Deadweight (mt) |
|------|-----------|-------------|---------------|-----------------|-------------------|----------------|
| Tropic Fresh – TF | 15.11 | 49'6" | | | 199000 | 209352 |
| Tropic – T | 14.99 | 49'2" | | | 192919 | 194984 |
| Fresh – F | 14.81 | 48'7" | | | 200671 | 203981 |
| Summer – S | 14.42 | 47'3" | 12.00 | 39'4" | 172837 | 188491 |
| Winter – W | 13.89 | 45'6" | | | 167992 | 172001 |
| Winter N.Atl. – WNA | | | | | 155493 | 162937 |

> **Ghi chú:** Draft ft" tự tính từ Draft m. Freeboard chỉ hiển thị cho Summer – S.

#### 2.2.3. GROSS TONNAGE

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | International | `number` | Ví dụ: 101837 |
| 2 | Suez Canal | `number` | Ví dụ: 110388 |
| 3 | Panama Canal | `number` | Ví dụ: 94453 |

#### 2.2.4. NETT TONNAGE

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | International | `number` | Ví dụ: 84392 |
| 2 | Suez Canal | `number` | Ví dụ: 92981 |
| 3 | Panama Canal | `number` | Ví dụ: 81772 |

#### 2.2.5. FOR TANKERS, LNG AND LPG ONLY

**Hàng 1:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Manifold to waterline (Ballast) (m) | `number` | Ví dụ: 14.55 |
| 2 | Manifold to waterline (Loaded) (m) | `number` | Ví dụ: 6.23 |
| 3 | Deck to manifold (m) | `number` | Ví dụ: 1.8 |
| 4 | Stern to manifold (m) | `number` | Ví dụ: 215.8 |
| 5 | Shipside to manifold (m) | `number` | Ví dụ: 6.45 |
| 6 | Bow to manifold (m) | `number` | Ví dụ: 95.88 |

**Hàng 2:**

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 7 | Manifold to keel (m) | `number` | Ví dụ: 28.88 |
| 8 | Manifold to Bridge (m) | `number` | Ví dụ: 43.7 |
| 9 | Max loading rate – ship (mq) | `number` | Ví dụ: 800 |
| 10 | Number of lines | `number` | Ví dụ: 2 |
| 11 | Max allowable pressure (psi) | `number` | Ví dụ: 8 |
| 12 | Venting system – ship | `string` | Text tự do |

#### 2.2.6. DATA FOR PILOT CARD

Ba bảng song song:

**MAIN ENGINE ORDER (/MIN)**

| # | Order | Value |
|---|-------|-------|
| 1 | Full Ahead Manoeuvring | `number` |
| 2 | Half Ahead | `number` |
| 3 | Slow Ahead | `number` |
| 4 | Dead Slow Ahead | `number` |
| 5 | Dead Slow Astern | `number` |
| 6 | Slow Astern | `number` |
| 7 | Half Astern | `number` |
| 8 | Full Astern | `number` |

**SPEED LOADED (KTS)** — Cùng các order như trên, giá trị tốc độ khi tải.

**SPEED BALLAST (KTS)** — Cùng các order như trên, giá trị tốc độ khi ballast.

#### Bố cục tổng thể tab Dimensions

```
┌──────────────┬──────────────┬──────────────┬──────────────┬──────────────┬──────────────┐
│ LOA m|ft     │ Depth m|ft ⊙ │ H-MaxAir m|ft│ Para.Bal m|ft│ Para.Load m|ft│              │
├──────────────┼──────────────┼──────────────┼──────────────┼──────────────┼──────────────┤
│ LBP m|ft     │ Draft mld ⊙  │ D-Distance ⊙ │ Bridge2Aft   │ Bridge2Bow   │ Bow2Bulbous  │
├──────────────┼──────────────┼──────────────┤              ├──────────────┤              │
│ Breadth ⊙    │ Draft Scant ⊙│ Airdraft Red │              │ Light Ship   │              │
│              ├──────────────┤              │ ☐ N/A        ├──────────────┤              │
│              │ DraftFullBal ⊙│             │ Block Coeff  │ TPC Summer   │ FWA (mm)     │
├──────────────┴──────────────┴──────────────┴──────────────┴──────────────┴──────────────┤
│ LOAD LINES PARTICULARS                      │ GROSS TONNAGE                             │
│ ┌──────┬───────┬──────┬────────┬──────────┐ │ [International] [Suez Canal] [Panama Canal]│
│ │ Type │Draft m│Free m│Displ mt│DWT mt    │ ├───────────────────────────────────────────┤
│ │ TF   │ 15.11 │      │ 199000 │ 209352   │ │ NETT TONNAGE                              │
│ │ T    │ 14.99 │      │ 192919 │ 194984   │ │ [International] [Suez Canal] [Panama Canal]│
│ │ F    │ 14.81 │      │ 200671 │ 203981   │ │                                           │
│ │ S    │ 14.42 │12.00 │ 172837 │ 188491   │ │                                           │
│ │ W    │ 13.89 │      │ 167992 │ 172001   │ │                                           │
│ │ WNA  │       │      │ 155493 │ 162937   │ │                                           │
│ └──────┴───────┴──────┴────────┴──────────┘ │                                           │
├─────────────────────────────────────────────┴───────────────────────────────────────────┤
│ FOR TANKERS, LNG AND LPG ONLY                                                          │
│ [Man2WL Bal] [Man2WL Load] [Deck2Man] [Stern2Man] [Ship2Man] [Bow2Man]                 │
│ [Man2Keel]   [Man2Bridge]  [MaxLoad]  [NoLines]   [MaxPres]  [Venting]                 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ DATA FOR PILOT CARD                                                                    │
│ ┌─────────────────────┬─────────────────────┬─────────────────────┐                     │
│ │ MAIN ENGINE ORDER   │ SPEED LOADED (KTS)  │ SPEED BALLAST (KTS) │                     │
│ │ Full Ahead Man.     │ Full Ahead Man.     │ Full Ahead Man.     │                     │
│ │ Half Ahead          │ Half Ahead          │ Half Ahead          │                     │
│ │ Slow Ahead          │ Slow Ahead          │ Slow Ahead          │                     │
│ │ Dead Slow Ahead     │ Dead Slow Ahead     │ Dead Slow Ahead     │                     │
│ │ Dead Slow Astern    │ Dead Slow Astern    │ Dead Slow Astern    │                     │
│ │ Slow Astern         │ Slow Astern         │ Slow Astern         │                     │
│ │ Half Astern         │ Half Astern         │ Half Astern         │                     │
│ │ Full Astern         │ Full Astern         │ Full Astern         │                     │
│ └─────────────────────┴─────────────────────┴─────────────────────┘                     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 2.3. Tab: Machinery ⭐

Tab phức tạp nhất, chứa nhiều section với khả năng thêm/xóa nhiều bản ghi.

#### 2.3.1. MAIN ENGINE(S)

> Có thể thêm nhiều Main Engine (nút **+** xanh). Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | ME Type | `string` | Ví dụ: "MAN B&W 11G95ME-" |
| 2 | ME Fuel Grade | `dropdown` | HFO, MDO, MGO, LNG, v.v. (có nút X xóa, ▽ mở) |
| 3 | ME Power (kW → HP) | `number` + `auto-calc` | Nhập kW, tự tính HP. Ví dụ: 75570 kW → 101341 HP |
| 4 | MCR (kW) | `number` | Maximum Continuous Rating. Ví dụ: 75570 |

**Layout:** Mỗi ME entry = 1 row: `[ME Type] [ME Fuel Grade ×▽] [ME Power kW | HP] [MCR kW] [🗑]`

---

#### 2.3.2. AUXILIARY ENGINE(S)

> Có thể thêm nhiều Auxiliary Engine (nút **+** xanh). Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | AE Type | `string` | Ví dụ: "Yanmar 6EY26L" |
| 2 | AE Fuel Grade | `dropdown` | MDO, HFO, v.v. (có nút X xóa, ▽ mở) |
| 3 | AE Power (kW → HP) | `number` + `auto-calc` | Nhập kW, tự tính HP. Ví dụ: 2700 kW → 3621 HP |

**Layout:** Mỗi AE entry = 1 row: `[AE Type] [AE Fuel Grade ×▽] [AE Power kW | HP] [🗑]`  
**Ảnh mẫu:** 3 entry Yanmar 6EY26L, MDO, 2700/3621.

---

#### 2.3.3. PROPELLER(S)

> Có thể thêm nhiều Propeller (nút **+** xanh). Có nút **❓** (help). Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Type | `dropdown` | (FPP) Fixed Pitch Propeller, (CPP) Controllable Pitch Propeller, v.v. (có nút X xóa, ▽ mở) |
| 2 | Number of Blades | `number + dropdown` | Ví dụ: 4 (có nút X xóa, ▽ mở) |
| 3 | Rotation | `radio` | ○ Clockwise / ○ Counter-Clockwise |
| 4 | Diam. (mm) | `number` | Đường kính. Ví dụ: 9500 |
| 5 | Propeller pitch (Geometric) (mm) | `number` | Bước chân vịt. Ví dụ: 7500 |
| 6 | Pitch Ratio | `number` + `auto-calc` | Tự tính = Pitch / Diameter. Ví dụ: 0.7895. Có nút ⊙ auto-calc |

**Layout:** 1 row: `[Type ×▽] [Blades ×▽] [○CW ○CCW] [Diam.] [Pitch] [Pitch Ratio ⊙] [🗑]`

---

#### 2.3.4. ANCHOR(S) CHAIN

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Port (shackles) | `number` | Mạn trái. Ví dụ: 13 |
| 2 | Starboard (shackles) | `number` | Mạn phải. Ví dụ: 13 |
| 3 | Stern (shackles) | `number` | Đuôi tàu |
| 4 | N/A | `checkbox` | Nếu tích → disable trường Stern |

**Layout:** 1 row: `[Port] [Starboard] [Stern] [☑ N/A]`

---

#### 2.3.5. BOWTHRUSTER(S)

> Có checkbox **N/A** và nút **+** thêm. Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | N/A | `checkbox` | Nếu tích → ẩn/disable toàn bộ entries |
| 2 | Power (kW → HP) | `number` + `auto-calc` | Nhập kW, tự tính HP. Ví dụ: 2500 kW → 3353 HP |

**Layout:** Header: `BOWTHRUSTER(S) [☐ N/A] [+]`, mỗi entry: `[Power kW | HP] [🗑]`

---

#### 2.3.6. STERNTHRUSTER(S)

> Có checkbox **N/A** và nút **+** thêm. Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | N/A | `checkbox` | Nếu tích → ẩn/disable toàn bộ entries |
| 2 | Power (kW → HP) | `number` + `auto-calc` | Nhập kW, tự tính HP |

**Layout:** Header: `STERNTHRUSTER(S) [☑ N/A] [+]`, mỗi entry: `[Power kW | HP] [🗑]`

---

#### 2.3.7. RUDDER(S)

> Có thể thêm nhiều Rudder (nút **+** xanh). Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Rudder Type | `dropdown` | Semi-balanced, Spade, Flap, v.v. (có nút X xóa, ▽ mở) |

**Layout:** Mỗi entry: `[Rudder Type ×▽] [🗑]`

---

#### 2.3.8. SHAFTS GENERATOR(S)

> Có checkbox **N/A** và nút **+** thêm. Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | N/A | `checkbox` | Nếu tích → ẩn/disable toàn bộ entries |
| 2 | Max Power (kW → HP) | `number` + `auto-calc` | Nhập kW, tự tính HP |

**Layout:** Header: `SHAFTS GENERATOR(S) [☑ N/A] [+]`, mỗi entry: `[Max Power kW | HP] [🗑]`

---

#### 2.3.9. BOILER(S)

> Có thể thêm nhiều Boiler (nút **+** xanh). Mỗi entry có nút **🗑** xóa (đỏ).

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Type | `string` | Ví dụ: "Exhaust Gas Boiler" |
| 2 | Model | `string` | Ví dụ: "Aalborg XS-2V" |

**Layout:** Mỗi entry: `[Type] [Model] [🗑]`

---

#### 2.3.10. HARBOUR / EMERGENCY GENERATOR

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Maker | `string` | Ví dụ: "Caterpillar" |
| 2 | Max Power (kW → HP) | `number` + `auto-calc` | Ví dụ: 800 kW → 1073 HP |

**Layout:** 1 row: `[Maker] [Max Power kW | HP]`

---

#### 2.3.11. AZIMUTH ENGINE

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | No of Azimuth Eng. FWD | `number` | Số lượng phía trước |
| 2 | Max Power FWD (kW → HP) | `number` + `auto-calc` | Công suất FWD |
| 3 | No of Azimuth Eng. AFT | `number` | Số lượng phía sau. Ví dụ: 0 |
| 4 | Max Power AFT (kW → HP) | `number` + `auto-calc` | Ví dụ: 1200 kW → 1609 HP |

**Layout:** 2 cột:  
- Cột trái: `[No FWD] [Max Power FWD kW | HP]`  
- Cột phải: `[No AFT] [Max Power AFT kW | HP]`

---

#### Bố cục tổng thể tab Machinery

```
┌─────────────────────────────────────┬─────────────────────────────────────┐
│ MAIN ENGINE(S)              [+]     │ AUXILIARY ENGINE(S)           [+]   │
│ ┌─────────────────────────────┐     │ ┌─────────────────────────────┐     │
│ │ ME entry 1              [🗑]│     │ │ AE entry 1              [🗑]│     │
│ └─────────────────────────────┘     │ │ AE entry 2              [🗑]│     │
│                                     │ │ AE entry 3              [🗑]│     │
│                                     │ └─────────────────────────────┘     │
├─────────────────────────────────────┴─────────────────────────────────────┤
│ PROPELLER(S)                                                  [❓] [+]   │
│ ┌───────────────────────────────────────────────────────────────────┐     │
│ │ Type | Blades | Rotation | Diam. | Pitch | Pitch Ratio      [🗑]│     │
│ └───────────────────────────────────────────────────────────────────┘     │
├───────────────────────────────────────────────────────────────────────────┤
│ ANCHOR(S) CHAIN                                                          │
│ [Port] [Starboard] [Stern] [☑ N/A]                                      │
├─────────────────────┬──────────────────────┬──────────────────────────────┤
│ BOWTHRUSTER(S)      │ STERNTHRUSTER(S)     │ RUDDER(S)              [+]  │
│ [☐ N/A] [+]        │ [☑ N/A] [+]         │                              │
│ [Power kW|HP] [🗑]  │ [Power kW|HP] [🗑]   │ [Rudder Type ×▽]      [🗑]  │
├─────────────────────┼──────────────────────┼──────────────────────────────┤
│ SHAFTS GENERATOR(S) │ BOILER(S)      [+]   │ HARBOUR / EMERGENCY GEN.    │
│ [☑ N/A] [+]        │                      │                              │
│ [Max Pwr kW|HP] [🗑]│ [Type] [Model] [🗑]  │ [Maker] [Max Pwr kW | HP]   │
├─────────────────────┴──────────────────────┴──────────────────────────────┤
│ [No Azimuth FWD] [Max Power kW|HP]  │ [No Azimuth AFT] [Max Power kW|HP]│
└─────────────────────────────────────┴─────────────────────────────────────┘
```

---

### 2.4. Tab: Shipowner ⭐

#### 2.4.1. Hàng 1 — Ba thực thể công ty (3 cột ngang)

##### SHIPOWNER

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Oceanic Lines Holding" |
| 2 | Street | `string` | Ví dụ: "21 Portside Avenue" |
| 3 | Country | `dropdown` | Ví dụ: SINGAPORE (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "119962" |
| 5 | City | `string` | Ví dụ: "Singapore" |
| 6 | Phone | `string` | Ví dụ: "+65-6123-8877" |
| 7 | Fax | `string` | Ví dụ: "+65-6123-8878" |
| 8 | Tlx | `string` | Telex |
| 9 | E-mail | `string` | Ví dụ: "admin@oceanic-lines.sg" |
| 10 | Contact Person | `string` | Ví dụ: "Michelle Tan" |

##### MANAGING OWNER

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Oceanic Maritime Pte. Ltd." |
| 2 | Street | `string` | Ví dụ: "15 Harbourfront Tower" |
| 3 | Country | `dropdown` | Ví dụ: AUSTRIA (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "098632" |
| 5 | City | `string` | Ví dụ: "Vienna" |
| 6 | Phone | `string` | Ví dụ: "+43-6789-1200" |
| 7 | Fax | `string` | Ví dụ: "+43-6789-1201" |
| 8 | Tlx | `string` | Telex |
| 9 | E-mail | `string` | Ví dụ: "ops@oceanicmaritime.sg" |
| 10 | Contact Person | `string` | Ví dụ: "Raymond Smith" |

##### OPERATOR

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Eastern Fleet Management Ltd." |
| 2 | Street | `string` | Ví dụ: "9 Marine Boulevard" |
| 3 | Country | `dropdown` | Ví dụ: GREECE (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "18545" |
| 5 | City | `string` | Ví dụ: "Piraeus" |
| 6 | Phone | `string` | Ví dụ: "+30-210-7788990" |
| 7 | Fax | `string` | Ví dụ: "+30-210-7788991" |
| 8 | Tlx | `string` | Telex |
| 9 | E-mail | `string` | Ví dụ: "fleet@easternfleet.gr" |
| 10 | Contact Person | `string` | Ví dụ: "Nikolas Papadakis" |

#### 2.4.2. Hàng 2 — Bốn nhân sự liên hệ (4 cột ngang)

##### COMPANY SECURITY OFFICER – CSO

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Title | `dropdown` | Mr. / Ms. / Mrs. / Capt. (có nút X xóa, ▽ mở) |
| 2 | First Name | `string` | Ví dụ: "Robert" |
| 3 | Last Name | `string` | Ví dụ: "Voss" |
| 4 | Street | `string` | Ví dụ: "42 Anchor Road" |
| 5 | Country | `dropdown` | Ví dụ: NORWAY (có nút X xóa, ▽ mở) |
| 6 | ZIP | `string` | Ví dụ: "5014" |
| 7 | City | `string` | Ví dụ: "Bergen" |
| 8 | Phone – 24hrs | `string` | Ví dụ: "+47-5501-2233" |
| 9 | Fax | `string` | Ví dụ: "+47-5501-2234" |
| 10 | Tlx | `string` | Telex |
| 11 | E-mail | `string` | Ví dụ: "cso@oceanic-lines.sg" |

##### DESIGNATED PERSON ASHORE – DPA

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Title | `dropdown` | Mr. / Ms. / Mrs. / Capt. |
| 2 | First Name | `string` | Ví dụ: "Anna" |
| 3 | Last Name | `string` | Ví dụ: "Fischer" |
| 4 | Street | `string` | Ví dụ: "12 Dockstrasse" |
| 5 | Country | `dropdown` | Ví dụ: GERMANY |
| 6 | ZIP | `string` | Ví dụ: "20457" |
| 7 | City | `string` | Ví dụ: "Hamburg" |
| 8 | Phone – 24hrs | `string` | Ví dụ: "+49-40-123456" |
| 9 | Fax | `string` | Ví dụ: "+49-40-123457" |
| 10 | Tlx | `string` | |
| 11 | E-mail | `string` | Ví dụ: "dpa@oceanicmaritime." |

##### QUALIFIED INDIVIDUAL USA – QI

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Title | `dropdown` | Mr. / Ms. / Mrs. / Capt. |
| 2 | First Name | `string` | Ví dụ: "Eric" |
| 3 | Last Name | `string` | Ví dụ: "Mitchell" |
| 4 | Street | `string` | Ví dụ: "88 Portway Lane" |
| 5 | Country | `dropdown` | Ví dụ: UNITED STATES |
| 6 | ZIP | `string` | Ví dụ: "77058" |
| 7 | City | `string` | Ví dụ: "Houston" |
| 8 | Phone – 24hrs | `string` | Ví dụ: "+1-832-555-9900" |
| 9 | Fax | `string` | Ví dụ: "+1-832-555-9901" |
| 10 | Tlx | `string` | |
| 11 | E-mail | `string` | Ví dụ: "qi-usa@oceanic-lines.sg" |

##### QUALIFIED INDIVIDUAL PANAMA CANAL – QI

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Title | `dropdown` | Mr. / Ms. / Mrs. / Capt. |
| 2 | First Name | `string` | Ví dụ: "Luis" |
| 3 | Last Name | `string` | Ví dụ: "Mendoza" |
| 4 | Street | `string` | Ví dụ: "33 Canal Drive" |
| 5 | Country | `dropdown` | Ví dụ: PANAMA |
| 6 | ZIP | `string` | Ví dụ: "0816-07218" |
| 7 | City | `string` | Ví dụ: "Balboa" |
| 8 | Phone – 24hrs | `string` | Ví dụ: "+507-300-4500" |
| 9 | Fax | `string` | Ví dụ: "+507-300-4501" |
| 10 | Tlx | `string` | |
| 11 | E-mail | `string` | Ví dụ: "qi-panama@oceanic-lin..." |

#### Bố cục tổng thể tab Shipowner

```
┌──────────────────────┬──────────────────────┬──────────────────────┐
│     SHIPOWNER        │   MANAGING OWNER     │     OPERATOR         │
│                      │                      │                      │
│ Name                 │ Name                 │ Name                 │
│ Street    | Country  │ Street    | Country  │ Street    | Country  │
│ ZIP       | City     │ ZIP       | City     │ ZIP       | City     │
│ Phone     | Fax      │ Phone     | Fax      │ Phone     | Fax      │
│ Tlx       | E-mail   │ Tlx       | E-mail   │ Tlx       | E-mail   │
│ Contact Person       │ Contact Person       │ Contact Person       │
├──────────┬───────────┼───────────┬──────────┼──────────────────────┤
│  CSO     │   DPA     │  QI USA   │ QI PANAMA│                      │
│          │           │           │ CANAL    │                      │
│ Title|FN|LN│Title|FN|LN│Title|FN|LN│Title|FN|LN                   │
│ Street|Country│Street|Country│Street|Country│Street|Country        │
│ ZIP  |City│ ZIP  |City│ ZIP  |City│ ZIP  |City                     │
│ Ph24h|Fax │ Ph24h|Fax │ Ph24h|Fax │ Ph24h|Fax                     │
│ Tlx|Email │ Tlx|Email │ Tlx|Email │ Tlx|Email                     │
└──────────┴───────────┴───────────┴──────────┴──────────────────────┘
```

---

### 2.5. Tab: Charterer ⭐

Hai card ngang song song.

#### CHARTERER

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Atlantic Container Leasing S.A." |
| 2 | Street | `string` | Ví dụ: "Rua do Porto 12" |
| 3 | Country | `dropdown` | Ví dụ: PORTUGAL (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "1350-092" |
| 5 | City | `string` | Ví dụ: "Lisbon" |
| 6 | Phone | `string` | Ví dụ: "+351-212-556-899" |
| 7 | Fax | `string` | Ví dụ: "+351-212-556-900" |
| 8 | Tlx | `string` | Ví dụ: "45678 ACL P" |
| 9 | E-mail | `string` | Ví dụ: "info@atlantic-charter.pt" |
| 10 | Contact Person | `string` | Ví dụ: "Ana Rodrigues" |

#### BAREBOAT CHARTERER

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Mariposa Marine Inc." |
| 2 | Street | `string` | Ví dụ: "81 Coral Bay Blvd" |
| 3 | Country | `dropdown` | Ví dụ: PANAMA (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "0819-00324" |
| 5 | City | `string` | Ví dụ: "Panama City" |
| 6 | Phone | `string` | Ví dụ: "+507-300-7788" |
| 7 | Fax | `string` | Ví dụ: "+507-300-7789" |
| 8 | Tlx | `string` | Ví dụ: "98765 MMI PN" |
| 9 | E-mail | `string` | Ví dụ: "ops@mariposa-marine.pa" |
| 10 | Contact Person | `string` | Ví dụ: "Carlos Ibáñez" |

#### Bố cục tổng thể tab Charterer

```
┌────────────────────────────────┬────────────────────────────────┐
│        CHARTERER               │      BAREBOAT CHARTERER        │
│                                │                                │
│ Name                           │ Name                           │
│ Street         | Country ×▽   │ Street         | Country ×▽   │
│ ZIP            | City          │ ZIP            | City          │
│ Phone          | Fax           │ Phone          | Fax           │
│ Tlx            | E-mail        │ Tlx            | E-mail        │
│ Contact Person                 │ Contact Person                 │
└────────────────────────────────┴────────────────────────────────┘
```

---

### 2.6. Tab: Class / Flag State ⭐

Hai card ngang song song.

#### CLASSIFICATION SOCIETY

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `dropdown` | Bureau Veritas, Lloyd's Register, DNV, ABS, v.v. (có nút X xóa, ▽ mở) |
| 2 | Street | `string` | Ví dụ: "Kalvebod Brygge 45" |
| 3 | Country | `dropdown` | Ví dụ: DENMARK (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "1560" |
| 5 | City | `string` | Ví dụ: "Copenhagen" |
| 6 | Phone | `string` | Ví dụ: "+45-3344-1100" |
| 7 | Fax | `string` | Ví dụ: "+45-3344-1101" |
| 8 | Tlx | `string` | Ví dụ: "45678 BV DK" |
| 9 | E-mail | `string` | Ví dụ: "cph@class.bureauveritas.com" |
| 10 | Contact Person | `string` | Ví dụ: "Lars Kristensen" |

#### FLAG STATE

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "Department of Merchant Shipping" |
| 2 | Street | `string` | Ví dụ: "23 Spyrou Araouzou St." |
| 3 | Country | `dropdown` | Ví dụ: CYPRUS (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "3036" |
| 5 | City | `string` | Ví dụ: "Limassol" |
| 6 | Phone | `string` | Ví dụ: "+357-2584-6000" |
| 7 | Fax | `string` | Ví dụ: "+357-2584-6001" |
| 8 | Tlx | `string` | Ví dụ: "60654 MTS CY" |
| 9 | E-mail | `string` | Ví dụ: "flagstate@dms.mcw.gov.cy" |
| 10 | Contact Person | `string` | Ví dụ: "Eleni Charalambous" |

#### Bố cục tổng thể tab Class / Flag State

```
┌────────────────────────────────┬────────────────────────────────┐
│   CLASSIFICATION SOCIETY       │         FLAG STATE             │
│                                │                                │
│ Name              ×▽          │ Name                           │
│ Street         | Country ×▽   │ Street         | Country ×▽   │
│ ZIP            | City          │ ZIP            | City          │
│ Phone          | Fax           │ Phone          | Fax           │
│ Tlx            | E-mail        │ Tlx            | E-mail        │
│ Contact Person                 │ Contact Person                 │
└────────────────────────────────┴────────────────────────────────┘
```

---

### 2.7. Tab: Insurance ⭐

Hai card ngang song song, cùng cấu trúc như Charterer / Class / Flag State.

#### INSURANCE – P&I CLUB

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "West of England P&I Club" |
| 2 | Street | `string` | Ví dụ: "4 Lombard Street" |
| 3 | Country | `dropdown` | Ví dụ: UNITED KINGDOM (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "EC3V 9AA" |
| 5 | City | `string` | Ví dụ: "London" |
| 6 | Phone | `string` | Ví dụ: "+44-20-7283-6789" |
| 7 | Fax | `string` | Ví dụ: "+44-20-7283-6790" |
| 8 | Tlx | `string` | Ví dụ: "12345 WESTUK G" |
| 9 | E-mail | `string` | Ví dụ: "underwriting@westpandi.com" |
| 10 | Contact Person | `string` | Ví dụ: "Sarah Middleton" |

#### INSURANCE – H&M CLUB

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Name | `string` | Ví dụ: "North Maritime H&M Club" |
| 2 | Street | `string` | Ví dụ: "Nyhavn 27" |
| 3 | Country | `dropdown` | Ví dụ: DENMARK (có nút X xóa, ▽ mở) |
| 4 | ZIP | `string` | Ví dụ: "1051" |
| 5 | City | `string` | Ví dụ: "Copenhagen" |
| 6 | Phone | `string` | Ví dụ: "+45-3312-8888" |
| 7 | Fax | `string` | Ví dụ: "45-3312-8889" |
| 8 | Tlx | `string` | Ví dụ: "67890 NORTHDK H" |
| 9 | E-mail | `string` | Ví dụ: "claims@north-hm.dk" |
| 10 | Contact Person | `string` | Ví dụ: "Henrik Sørensen" |

#### Bố cục tổng thể tab Insurance

```
┌────────────────────────────────┬────────────────────────────────┐
│     INSURANCE – P&I CLUB       │     INSURANCE – H&M CLUB       │
│                                │                                │
│ Name                           │ Name                           │
│ Street         | Country ×▽   │ Street         | Country ×▽   │
│ ZIP            | City          │ ZIP            | City          │
│ Phone          | Fax           │ Phone          | Fax           │
│ Tlx            | E-mail        │ Tlx            | E-mail        │
│ Contact Person                 │ Contact Person                 │
└────────────────────────────────┴────────────────────────────────┘
```

---

### 2.8. Tab: Radio Communication Equipment ⭐

#### 2.8.1. INMARSAT

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | INMARSAT Telex (1) | `string` | Ví dụ: "+870-765432100" |
| 2 | INMARSAT Telex (2) | `string` | Ví dụ: "+870-765432322" |
| 3 | INMARSAT Phone (1) | `string` | Ví dụ: "+870-765432101" |
| 4 | INMARSAT Phone (2) | `string` | Ví dụ: "+870-765432303" |
| 5 | INMARSAT Fax (1) | `string` | Ví dụ: "+870-765432102" |
| 6 | INMARSAT Fax (2) | `string` | Ví dụ: "+870-765432262" |
| 7 | E-mail address (1) | `string` | Ví dụ: "master@viking-spirit.com" |
| 8 | E-mail address (2) | `string` | Ví dụ: "master2@viking-spirit.com" |
| 9 | GSM Phone | `string` | Ví dụ: "+87-4444444444" |

**Layout:** Grid 2 cột (trường 1 bên trái, trường 2 bên phải):

```
[INMARSAT Telex (1)]     [INMARSAT Telex (2)]
[INMARSAT Phone (1)]     [INMARSAT Phone (2)]
[INMARSAT Fax (1)]       [INMARSAT Fax (2)]
[E-mail address (1)]     [E-mail address (2)]
[GSM Phone]
```

#### 2.8.2. Sea Areas (Regulation IV/2)

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | A1 | `checkbox` | ☑ |
| 2 | A2 | `checkbox` | ☑ |
| 3 | A3 | `checkbox` | ☑ |
| 4 | A4 | `checkbox` | ☐ |

**Label:** "Sea areas in which ship is certified to operate (regulation IV/2)"  
**Layout:** 4 checkbox ngang: `[☑ A1] [☑ A2] [☑ A3] [☐ A4]`

#### 2.8.3. SELECT IF O/B (On Board)

Bảng checkbox matrix:

| | HF | MF | VHF | Khác |
|---|:---:|:---:|:---:|------|
| **DSC** | ☑ | ☑ | ☑ | NAVTEX ☑, AIS ☑ |
| **Radiotelephone** | ☑ | ☑ | ☑ | SART transponder ☑ |
| **Radiotelegraph** | ☑ | ☑ | ☑ | Radiotelex ☐ |

| # | Trường | Kiểu dữ liệu |
|---|--------|--------------|
| 1 | DSC – HF | `checkbox` |
| 2 | DSC – MF | `checkbox` |
| 3 | DSC – VHF | `checkbox` |
| 4 | Radiotelephone – HF | `checkbox` |
| 5 | Radiotelephone – MF | `checkbox` |
| 6 | Radiotelephone – VHF | `checkbox` |
| 7 | Radiotelegraph – HF | `checkbox` |
| 8 | Radiotelegraph – MF | `checkbox` |
| 9 | Radiotelegraph – VHF | `checkbox` |
| 10 | NAVTEX | `checkbox` |
| 11 | AIS | `checkbox` |
| 12 | SART transponder | `checkbox` |
| 13 | Radiotelex | `checkbox` |
| 14 | Other radio equipment | `string` | Ví dụ: "LRIT terminal integrated with AIS-B" |

**Layout:**
```
┌─────────────────────────────────────────────────────────────────┐
│ SELECT IF O/B                                                   │
│                                                                 │
│         DSC:  [☑ HF] [☑ MF] [☑ VHF]    [☑ NAVTEX]  [☑ AIS]   │
│ Radiotelephone: [☑ HF] [☑ MF] [☑ VHF]  [☑ SART transponder]  │
│ Radiotelegraph: [☑ HF] [☑ MF] [☑ VHF]  [☐ Radiotelex]        │
│                                                                 │
│ Other radio equipment:                                          │
│ [LRIT terminal integrated with AIS-B                         ]  │
└─────────────────────────────────────────────────────────────────┘
```

#### 2.8.4. EPIRB'S DETAILS

| # | Trường | Kiểu dữ liệu | Ghi chú |
|---|--------|--------------|---------|
| 1 | Number | `string` | Ví dụ: "3342223" |
| 2 | Operating system | `string` | Ví dụ: "GLONAS" |
| 3 | Maker | `string` | Ví dụ: "FURUNO" |
| 4 | Model | `string` | Ví dụ: "M38K8" |
| 5 | Frequency | `string` | Ví dụ: "121" |

**Layout:** 1 row: `[Number] [Operating system] [Maker] [Model] [Frequency]`

#### Bố cục tổng thể tab Radio Communication Equipment

```
┌─────────────────────────────────────────────────────────────────┐
│ [INMARSAT Telex (1)]          [INMARSAT Telex (2)]             │
│ [INMARSAT Phone (1)]          [INMARSAT Phone (2)]             │
│ [INMARSAT Fax (1)]            [INMARSAT Fax (2)]               │
│ [E-mail address (1)]          [E-mail address (2)]             │
│ [GSM Phone]                                                     │
│                                                                 │
│ Sea areas certified (reg IV/2): [☑A1] [☑A2] [☑A3] [☐A4]      │
├─────────────────────────────────────────────────────────────────┤
│ SELECT IF O/B                                                   │
│         DSC:  [☑HF] [☑MF] [☑VHF]    [☑NAVTEX]  [☑AIS]        │
│ Radiotelephone: [☑HF] [☑MF] [☑VHF]  [☑SART transponder]      │
│ Radiotelegraph: [☑HF] [☑MF] [☑VHF]  [☐Radiotelex]            │
│                                                                 │
│ Other radio equipment:                                          │
│ [_______________________________________________]               │
├─────────────────────────────────────────────────────────────────┤
│ EPIRB'S DETAILS                                                 │
│ [Number] [Operating system] [Maker] [Model] [Frequency]        │
└─────────────────────────────────────────────────────────────────┘
```

---

### 2.9. Tab: Tanks & Cgo Spaces ⭐

Hai card ngang song song.

#### TANKS CAPACITY (100%)

| # | Trường | Kiểu dữ liệu | Đơn vị | Ghi chú |
|---|--------|--------------|--------|---------|
| 1 | HFO (cbm) | `number` | cbm | Ví dụ: 11231 |
| 2 | MDO (cbm) | `number` | cbm | Ví dụ: 788 |
| 3 | Lub. Oil (cbm) | `number` | cbm | Ví dụ: 25 |
| 4 | Sludge (cbm) | `number` | cbm | Ví dụ: 97 |
| 5 | Bilge Water (cbm) | `number` | cbm | Ví dụ: 17 |
| 6 | Sewage (cbm) | `number` | cbm | Ví dụ: 142 |
| 7 | Fresh Water (cbm) | `number` | cbm | Ví dụ: 452 |
| 8 | Ballast Water (cbm) | `number` | cbm | Ví dụ: 12839 |
| 9 | No of Ballast Tanks | `number` | - | Ví dụ: 15 |

**Layout:** Grid 2 cột:
```
[HFO (cbm)]           [MDO (cbm)]
[Lub. Oil (cbm)]      [Sludge (cbm)]
[Bilge Water (cbm)]   [Sewage (cbm)]
[Fresh Water (cbm)]   [Ballast Water (cbm)]
[No of Ballast Tanks]
```

#### CARGO CAPACITY

| # | Trường | Kiểu dữ liệu | Đơn vị | Ghi chú |
|---|--------|--------------|--------|---------|
| 1 | TEU Total | `number` | - | Ví dụ: 9910 |
| 2 | TEU on Deck | `number` | - | Ví dụ: 5488 |
| 3 | TEU Under Deck | `number` | - | Ví dụ: 4422 |
| 4 | Grain (cbm) | `number` | cbm | Ví dụ: 9999 |
| 5 | Grain CBM to CB Feet (cb feet) | `number` + `auto-calc` | cb feet | Ví dụ: 353111.35 (auto = cbm × 35.3147) |
| 6 | Bales (cbm) | `number` | cbm | Ví dụ: 9999 |
| 7 | Bales CBM to CB Feet (cb feet) | `number` + `auto-calc` | cb feet | Ví dụ: 353111.35 (auto = cbm × 35.3147) |
| 8 | No of Cargo Holds | `number` | - | Ví dụ: 8 |
| 9 | No of Hatches | `number` | - | Ví dụ: 26 |

**Layout:**
```
[TEU Total                                  ]
[TEU on Deck                                ]
[TEU Under Deck                             ]
[Grain (cbm)]          [Grain CBM → CB Feet ]
[Bales (cbm)]          [Bales CBM → CB Feet ]
[No of Cargo Holds]    [No of Hatches       ]
```

#### Bố cục tổng thể tab Tanks & Cgo Spaces

```
┌────────────────────────────────┬────────────────────────────────┐
│   TANKS CAPACITY (100%)        │       CARGO CAPACITY           │
│                                │                                │
│ [HFO cbm]     [MDO cbm]       │ [TEU Total                   ] │
│ [Lub.Oil cbm] [Sludge cbm]    │ [TEU on Deck                 ] │
│ [Bilge cbm]   [Sewage cbm]    │ [TEU Under Deck              ] │
│ [FreshW cbm]  [BallastW cbm]  │ [Grain cbm] [Grain→CB Feet   ] │
│ [No Ballast Tanks]             │ [Bales cbm] [Bales→CB Feet   ] │
│                                │ [No Cargo Holds] [No Hatches ] │
└────────────────────────────────┴────────────────────────────────┘
```

---

## 3. THIẾT KẾ DATABASE

### 3.1. Bảng chính

```sql
-- Bảng chính Ship's Data
CREATE TABLE ShipData (
    Id INT PRIMARY KEY IDENTITY,
    ShipId INT NOT NULL FOREIGN KEY REFERENCES Ships(Id),
    
    -- Basic Data
    ImoNumber NVARCHAR(20),                -- IMO Number *
    OfficialNumber NVARCHAR(50),           -- Official Number
    CallSign NVARCHAR(20),                 -- Call Sign
    ShipName NVARCHAR(200),                -- Ship's Name *
    Flag NVARCHAR(100),                    -- Flag *
    PortOfRegistry NVARCHAR(200),          -- Port of Registry *
    PreviousName NVARCHAR(200),            -- Previous Name
    PreviousFlag NVARCHAR(100),            -- Previous Flag
    MMSI NVARCHAR(20),                     -- MMSI Number
    TypeOfVessel NVARCHAR(200),            -- Type of vsl (dropdown)
    ClassNotation NVARCHAR(200),           -- Class Notation
    ClassRegisterNumber NVARCHAR(50),      -- Class Register Number
    ShipyardCountry NVARCHAR(100),         -- Shipyard/Country
    ShipyardName NVARCHAR(200),            -- Shipyard/Name
    YardNo NVARCHAR(50),                   -- Yard No
    CompanyImoNumber NVARCHAR(50),         -- Company IMO Number
    SuezCanalIdNumber NVARCHAR(50),        -- Suez Canal ID Number
    KeelLaidDate DATE,                     -- Keel Laid Date
    YearBuilt INT,                         -- Year of Built
    DateOfRegistry DATE,                   -- Date of Registry
    OwnerImoNumber NVARCHAR(50),           -- Owner IMO Number
    PanamaCanalIdNumber NVARCHAR(50),      -- Panama Canal ID Number
    MaxPersonsAllowedOB INT,               -- Max Persons Allowed O/B
    ServiceSpeedKts DECIMAL(6,2),          -- Service speed (kts)
    VrpNumber NVARCHAR(50),                -- VRP - Vessel Response Plan Number
    VrpType NVARCHAR(50),                  -- VRP Type (Required for US E-NOAID)
    NoOfCrewSafeManning INT,               -- No of Crew as per Safe Manning
    MaxPassengersAllowedOB INT,            -- Max Passengers Allowed O/B
    
    -- Dimensions - Section trên
    LOA DECIMAL(10,2),                          -- LOA (m)
    DepthMoulded DECIMAL(10,2),                 -- Depth moulded (m)
    HMaxAirdraft DECIMAL(10,2),                 -- H - Max Airdraft (m)
    ParallelBodyBallast DECIMAL(10,2),          -- Parallel body (Ballast) (m)
    ParallelBodyLoaded DECIMAL(10,2),           -- Parallel body (Loaded) (m)
    LBP DECIMAL(10,2),                          -- LBP (m)
    DraftMoulded DECIMAL(10,2),                 -- Draft moulded (m)
    DDistance DECIMAL(10,2),                     -- D - Distance (m)
    BridgeToAft DECIMAL(10,2),                  -- Bridge to Aft (m)
    BridgeToBow DECIMAL(10,2),                  -- Bridge to Bow (m)
    BowToBulbousBow DECIMAL(10,2),              -- Bow to Bulbous Bow (m)
    BreadthMoulded DECIMAL(10,2),               -- Breadth moulded (m)
    DraftScantling DECIMAL(10,2),               -- Draft Scantling (m)
    AirdraftReductionMastFouled DECIMAL(10,2),  -- Airdraft Reduction (Mast Fouled) (m)
    LightShip DECIMAL(12,2),                    -- Light Ship (mt)
    DraftFullBallast DECIMAL(10,2),             -- Draft Full Ballast (m)
    BlockCoefficientNA BIT DEFAULT 0,           -- N/A checkbox
    BlockCoefficient DECIMAL(6,4),              -- Block Coefficient
    TPCAtSummerDraft DECIMAL(10,2),             -- TPC at Summer Draft (mt)
    FreshWaterAllowanceFWA DECIMAL(8,2),        -- Fresh Water Allowance - FWA (mm)
    
    -- Gross Tonnage
    GrossTonnageInternational DECIMAL(12,2),    -- GT International
    GrossTonnageSuezCanal DECIMAL(12,2),        -- GT Suez Canal
    GrossTonnagePanamaCanal DECIMAL(12,2),      -- GT Panama Canal
    
    -- Nett Tonnage
    NettTonnageInternational DECIMAL(12,2),     -- NT International
    NettTonnageSuezCanal DECIMAL(12,2),         -- NT Suez Canal
    NettTonnagePanamaCanal DECIMAL(12,2),       -- NT Panama Canal
    
    -- For Tankers, LNG and LPG Only
    ManifoldToWaterlineBallast DECIMAL(10,2),
    ManifoldToWaterlineLoaded DECIMAL(10,2),
    DeckToManifold DECIMAL(10,2),
    SternToManifold DECIMAL(10,2),
    ShipsideToManifold DECIMAL(10,2),
    BowToManifold DECIMAL(10,2),
    ManifoldToKeel DECIMAL(10,2),
    ManifoldToBridge DECIMAL(10,2),
    MaxLoadingRateShip DECIMAL(10,2),
    NumberOfLines INT,
    MaxAllowablePressurePsi DECIMAL(10,2),
    VentingSystemShip NVARCHAR(200),
    
    -- Anchor Chain
    AnchorChainPort INT,
    AnchorChainStarboard INT,
    AnchorChainStern INT,
    AnchorChainSternNA BIT DEFAULT 0,
    
    -- Bowthruster
    BowthrusterNA BIT DEFAULT 0,
    
    -- Sternthruster
    SternthrusterNA BIT DEFAULT 0,
    
    -- Shaft Generator
    ShaftGeneratorNA BIT DEFAULT 0,
    
    -- Harbour / Emergency Generator
    HarbourGeneratorMaker NVARCHAR(200),
    HarbourGeneratorMaxPowerKW DECIMAL(10,2),
    
    -- Azimuth Engine
    AzimuthEngFwdCount INT,
    AzimuthEngFwdMaxPowerKW DECIMAL(10,2),
    AzimuthEngAftCount INT,
    AzimuthEngAftMaxPowerKW DECIMAL(10,2),
    
    -- Shipowner
    ShipownerName NVARCHAR(300),
    ShipownerStreet NVARCHAR(300),
    ShipownerCountry NVARCHAR(100),
    ShipownerZip NVARCHAR(20),
    ShipownerCity NVARCHAR(100),
    ShipownerPhone NVARCHAR(50),
    ShipownerFax NVARCHAR(50),
    ShipownerTlx NVARCHAR(50),
    ShipownerEmail NVARCHAR(200),
    ShipownerContactPerson NVARCHAR(200),
    
    -- Managing Owner
    ManagingOwnerName NVARCHAR(300),
    ManagingOwnerStreet NVARCHAR(300),
    ManagingOwnerCountry NVARCHAR(100),
    ManagingOwnerZip NVARCHAR(20),
    ManagingOwnerCity NVARCHAR(100),
    ManagingOwnerPhone NVARCHAR(50),
    ManagingOwnerFax NVARCHAR(50),
    ManagingOwnerTlx NVARCHAR(50),
    ManagingOwnerEmail NVARCHAR(200),
    ManagingOwnerContactPerson NVARCHAR(200),
    
    -- Operator
    OperatorName NVARCHAR(300),
    OperatorStreet NVARCHAR(300),
    OperatorCountry NVARCHAR(100),
    OperatorZip NVARCHAR(20),
    OperatorCity NVARCHAR(100),
    OperatorPhone NVARCHAR(50),
    OperatorFax NVARCHAR(50),
    OperatorTlx NVARCHAR(50),
    OperatorEmail NVARCHAR(200),
    OperatorContactPerson NVARCHAR(200),
    
    -- CSO
    CsoTitle NVARCHAR(20),
    CsoFirstName NVARCHAR(100),
    CsoLastName NVARCHAR(100),
    CsoStreet NVARCHAR(300),
    CsoCountry NVARCHAR(100),
    CsoZip NVARCHAR(20),
    CsoCity NVARCHAR(100),
    CsoPhone24h NVARCHAR(50),
    CsoFax NVARCHAR(50),
    CsoTlx NVARCHAR(50),
    CsoEmail NVARCHAR(200),
    
    -- DPA
    DpaTitle NVARCHAR(20),
    DpaFirstName NVARCHAR(100),
    DpaLastName NVARCHAR(100),
    DpaStreet NVARCHAR(300),
    DpaCountry NVARCHAR(100),
    DpaZip NVARCHAR(20),
    DpaCity NVARCHAR(100),
    DpaPhone24h NVARCHAR(50),
    DpaFax NVARCHAR(50),
    DpaTlx NVARCHAR(50),
    DpaEmail NVARCHAR(200),
    
    -- QI USA
    QiUsaTitle NVARCHAR(20),
    QiUsaFirstName NVARCHAR(100),
    QiUsaLastName NVARCHAR(100),
    QiUsaStreet NVARCHAR(300),
    QiUsaCountry NVARCHAR(100),
    QiUsaZip NVARCHAR(20),
    QiUsaCity NVARCHAR(100),
    QiUsaPhone24h NVARCHAR(50),
    QiUsaFax NVARCHAR(50),
    QiUsaTlx NVARCHAR(50),
    QiUsaEmail NVARCHAR(200),
    
    -- QI Panama Canal
    QiPanamaTitle NVARCHAR(20),
    QiPanamaFirstName NVARCHAR(100),
    QiPanamaLastName NVARCHAR(100),
    QiPanamaStreet NVARCHAR(300),
    QiPanamaCountry NVARCHAR(100),
    QiPanamaZip NVARCHAR(20),
    QiPanamaCity NVARCHAR(100),
    QiPanamaPhone24h NVARCHAR(50),
    QiPanamaFax NVARCHAR(50),
    QiPanamaTlx NVARCHAR(50),
    QiPanamaEmail NVARCHAR(200),
    
    -- Charterer
    ChartererName NVARCHAR(300),
    ChartererStreet NVARCHAR(300),
    ChartererCountry NVARCHAR(100),
    ChartererZip NVARCHAR(20),
    ChartererCity NVARCHAR(100),
    ChartererPhone NVARCHAR(50),
    ChartererFax NVARCHAR(50),
    ChartererTlx NVARCHAR(50),
    ChartererEmail NVARCHAR(200),
    ChartererContactPerson NVARCHAR(200),
    
    -- Bareboat Charterer
    BareboatChartererName NVARCHAR(300),
    BareboatChartererStreet NVARCHAR(300),
    BareboatChartererCountry NVARCHAR(100),
    BareboatChartererZip NVARCHAR(20),
    BareboatChartererCity NVARCHAR(100),
    BareboatChartererPhone NVARCHAR(50),
    BareboatChartererFax NVARCHAR(50),
    BareboatChartererTlx NVARCHAR(50),
    BareboatChartererEmail NVARCHAR(200),
    BareboatChartererContactPerson NVARCHAR(200),
    
    -- Classification Society (detail)
    ClassSocietyName NVARCHAR(300),
    ClassSocietyStreet NVARCHAR(300),
    ClassSocietyCountry NVARCHAR(100),
    ClassSocietyZip NVARCHAR(20),
    ClassSocietyCity NVARCHAR(100),
    ClassSocietyPhone NVARCHAR(50),
    ClassSocietyFax NVARCHAR(50),
    ClassSocietyTlx NVARCHAR(50),
    ClassSocietyEmail NVARCHAR(200),
    ClassSocietyContactPerson NVARCHAR(200),
    
    -- Flag State
    FlagStateName NVARCHAR(300),
    FlagStateStreet NVARCHAR(300),
    FlagStateCountry NVARCHAR(100),
    FlagStateZip NVARCHAR(20),
    FlagStateCity NVARCHAR(100),
    FlagStatePhone NVARCHAR(50),
    FlagStateFax NVARCHAR(50),
    FlagStateTlx NVARCHAR(50),
    FlagStateEmail NVARCHAR(200),
    FlagStateContactPerson NVARCHAR(200),
    
    -- Insurance - P&I Club
    PiClubName NVARCHAR(300),
    PiClubStreet NVARCHAR(300),
    PiClubCountry NVARCHAR(100),
    PiClubZip NVARCHAR(20),
    PiClubCity NVARCHAR(100),
    PiClubPhone NVARCHAR(50),
    PiClubFax NVARCHAR(50),
    PiClubTlx NVARCHAR(50),
    PiClubEmail NVARCHAR(200),
    PiClubContactPerson NVARCHAR(200),
    
    -- Insurance - H&M Club
    HmClubName NVARCHAR(300),
    HmClubStreet NVARCHAR(300),
    HmClubCountry NVARCHAR(100),
    HmClubZip NVARCHAR(20),
    HmClubCity NVARCHAR(100),
    HmClubPhone NVARCHAR(50),
    HmClubFax NVARCHAR(50),
    HmClubTlx NVARCHAR(50),
    HmClubEmail NVARCHAR(200),
    HmClubContactPerson NVARCHAR(200),
    
    -- Radio Communication Equipment - INMARSAT
    InmarsatTelex1 NVARCHAR(50),
    InmarsatTelex2 NVARCHAR(50),
    InmarsatPhone1 NVARCHAR(50),
    InmarsatPhone2 NVARCHAR(50),
    InmarsatFax1 NVARCHAR(50),
    InmarsatFax2 NVARCHAR(50),
    EmailAddress1 NVARCHAR(200),
    EmailAddress2 NVARCHAR(200),
    GsmPhone NVARCHAR(50),
    
    -- Sea Areas
    SeaAreaA1 BIT DEFAULT 0,
    SeaAreaA2 BIT DEFAULT 0,
    SeaAreaA3 BIT DEFAULT 0,
    SeaAreaA4 BIT DEFAULT 0,
    
    -- Radio Equipment O/B
    DscHF BIT DEFAULT 0,
    DscMF BIT DEFAULT 0,
    DscVHF BIT DEFAULT 0,
    RadiotelephoneHF BIT DEFAULT 0,
    RadiotelephoneMF BIT DEFAULT 0,
    RadiotelephoneVHF BIT DEFAULT 0,
    RadiotelegraphHF BIT DEFAULT 0,
    RadiotelegraphMF BIT DEFAULT 0,
    RadiotelegraphVHF BIT DEFAULT 0,
    Navtex BIT DEFAULT 0,
    Ais BIT DEFAULT 0,
    SartTransponder BIT DEFAULT 0,
    Radiotelex BIT DEFAULT 0,
    OtherRadioEquipment NVARCHAR(500),
    
    -- EPIRB
    EpirbNumber NVARCHAR(50),
    EpirbOperatingSystem NVARCHAR(50),
    EpirbMaker NVARCHAR(100),
    EpirbModel NVARCHAR(100),
    EpirbFrequency NVARCHAR(50),
    
    -- Tanks Capacity (100%)
    HfoCbm DECIMAL(12,2),              -- HFO (cbm)
    MdoCbm DECIMAL(12,2),              -- MDO (cbm)
    LubOilCbm DECIMAL(12,2),           -- Lub. Oil (cbm)
    SludgeCbm DECIMAL(12,2),           -- Sludge (cbm)
    BilgeWaterCbm DECIMAL(12,2),       -- Bilge Water (cbm)
    SewageCbm DECIMAL(12,2),           -- Sewage (cbm)
    FreshWaterCbm DECIMAL(12,2),       -- Fresh Water (cbm)
    BallastWaterCbm DECIMAL(12,2),     -- Ballast Water (cbm)
    NoOfBallastTanks INT,              -- No of Ballast Tanks
    
    -- Cargo Capacity
    TeuTotal INT,                      -- TEU Total
    TeuOnDeck INT,                     -- TEU on Deck
    TeuUnderDeck INT,                  -- TEU Under Deck
    GrainCbm DECIMAL(12,2),            -- Grain (cbm)
    BalesCbm DECIMAL(12,2),            -- Bales (cbm)
    NoOfCargoHolds INT,                -- No of Cargo Holds
    NoOfHatches INT,                   -- No of Hatches
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(100),
    UpdatedBy NVARCHAR(100)
);
```

### 3.2. Bảng con (1-to-Many)

```sql
-- Main Engines (nhiều bản ghi)
CREATE TABLE ShipMainEngines (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    MeType NVARCHAR(200),          -- "MAN B&W 11G95ME-"
    MeFuelGrade NVARCHAR(50),      -- "HFO"
    MePowerKW DECIMAL(10,2),       -- 75570
    McrKW DECIMAL(10,2),           -- 75570
    SortOrder INT DEFAULT 0
);

-- Auxiliary Engines (nhiều bản ghi)
CREATE TABLE ShipAuxiliaryEngines (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    AeType NVARCHAR(200),          -- "Yanmar 6EY26L"
    AeFuelGrade NVARCHAR(50),      -- "MDO"
    AePowerKW DECIMAL(10,2),       -- 2700
    SortOrder INT DEFAULT 0
);

-- Propellers (nhiều bản ghi)
CREATE TABLE ShipPropellers (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    PropellerType NVARCHAR(100),           -- "(FPP) Fixed Pitch Propeller"
    NumberOfBlades INT,                     -- 4
    Rotation NVARCHAR(20),                 -- "Clockwise" / "Counter-Clockwise"
    DiameterMm DECIMAL(10,2),             -- 9500
    PropellerPitchGeometricMm DECIMAL(10,2), -- 7500
    PitchRatio DECIMAL(8,4),              -- 0.7895 (auto-calc)
    SortOrder INT DEFAULT 0
);

-- Bowthrusters (nhiều bản ghi)
CREATE TABLE ShipBowthrusters (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    PowerKW DECIMAL(10,2),         -- 2500
    SortOrder INT DEFAULT 0
);

-- Sternthrusters (nhiều bản ghi)
CREATE TABLE ShipSternthrusters (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    PowerKW DECIMAL(10,2),
    SortOrder INT DEFAULT 0
);

-- Rudders (nhiều bản ghi)
CREATE TABLE ShipRudders (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    RudderType NVARCHAR(100),      -- "Semi-balanced, Spade"
    SortOrder INT DEFAULT 0
);

-- Shaft Generators (nhiều bản ghi)
CREATE TABLE ShipShaftGenerators (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    MaxPowerKW DECIMAL(10,2),
    SortOrder INT DEFAULT 0
);

-- Boilers (nhiều bản ghi)
CREATE TABLE ShipBoilers (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    BoilerType NVARCHAR(200),      -- "Exhaust Gas Boiler"
    Model NVARCHAR(200),           -- "Aalborg XS-2V"
    SortOrder INT DEFAULT 0
);

-- Load Lines Particulars (bảng con cho Dimensions)
CREATE TABLE ShipLoadLines (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    LoadLineType NVARCHAR(50),         -- "Tropic Fresh - TF", "Tropic - T", "Fresh - F", "Summer - S", "Winter - W", "Winter N.Atl. - WNA"
    DraftM DECIMAL(10,2),              -- Draft (m)
    FreeboardM DECIMAL(10,2),          -- Freeboard (m) — chỉ có ở Summer
    DisplacementMt DECIMAL(12,2),      -- Displacement (mt)
    DeadweightMt DECIMAL(12,2),        -- Deadweight (mt)
    SortOrder INT DEFAULT 0
);

-- Data for Pilot Card (bảng con cho Dimensions)
CREATE TABLE ShipPilotCardData (
    Id INT PRIMARY KEY IDENTITY,
    ShipDataId INT NOT NULL FOREIGN KEY REFERENCES ShipData(Id) ON DELETE CASCADE,
    EngineOrder NVARCHAR(50),          -- "Full Ahead Manoeuvring", "Half Ahead", "Slow Ahead", v.v.
    MainEngineRPM DECIMAL(8,2),        -- Main Engine Order (/MIN)
    SpeedLoadedKts DECIMAL(6,2),       -- Speed Loaded (KTS)
    SpeedBallastKts DECIMAL(6,2),      -- Speed Ballast (KTS)
    SortOrder INT DEFAULT 0
);
```

---

## 4. API ENDPOINTS

### Shore Backend (`backend/Controllers/ShipDataController.cs`)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| `GET` | `/api/ships/{shipId}/data` | Lấy toàn bộ Ship's Data (gồm tất cả tab) |
| `PUT` | `/api/ships/{shipId}/data` | Cập nhật toàn bộ Ship's Data |
| `PATCH` | `/api/ships/{shipId}/data/{tabName}` | Cập nhật từng tab riêng lẻ |
| **Main Engines** | | |
| `GET` | `/api/ships/{shipId}/data/main-engines` | Lấy danh sách Main Engines |
| `POST` | `/api/ships/{shipId}/data/main-engines` | Thêm Main Engine |
| `PUT` | `/api/ships/{shipId}/data/main-engines/{id}` | Cập nhật Main Engine |
| `DELETE` | `/api/ships/{shipId}/data/main-engines/{id}` | Xóa Main Engine |
| **Auxiliary Engines** | | |
| `GET` | `/api/ships/{shipId}/data/auxiliary-engines` | Lấy danh sách AE |
| `POST` | `/api/ships/{shipId}/data/auxiliary-engines` | Thêm AE |
| `PUT` | `/api/ships/{shipId}/data/auxiliary-engines/{id}` | Cập nhật AE |
| `DELETE` | `/api/ships/{shipId}/data/auxiliary-engines/{id}` | Xóa AE |
| **Propellers** | | |
| `GET` | `/api/ships/{shipId}/data/propellers` | Lấy danh sách Propeller |
| `POST` | `/api/ships/{shipId}/data/propellers` | Thêm Propeller |
| `PUT` | `/api/ships/{shipId}/data/propellers/{id}` | Cập nhật Propeller |
| `DELETE` | `/api/ships/{shipId}/data/propellers/{id}` | Xóa Propeller |
| **Bowthrusters** | | |
| `POST` | `/api/ships/{shipId}/data/bowthrusters` | Thêm |
| `DELETE` | `/api/ships/{shipId}/data/bowthrusters/{id}` | Xóa |
| **Sternthrusters** | | |
| `POST` | `/api/ships/{shipId}/data/sternthrusters` | Thêm |
| `DELETE` | `/api/ships/{shipId}/data/sternthrusters/{id}` | Xóa |
| **Rudders** | | |
| `POST` | `/api/ships/{shipId}/data/rudders` | Thêm |
| `DELETE` | `/api/ships/{shipId}/data/rudders/{id}` | Xóa |
| **Shaft Generators** | | |
| `POST` | `/api/ships/{shipId}/data/shaft-generators` | Thêm |
| `DELETE` | `/api/ships/{shipId}/data/shaft-generators/{id}` | Xóa |
| **Boilers** | | |
| `POST` | `/api/ships/{shipId}/data/boilers` | Thêm |
| `PUT` | `/api/ships/{shipId}/data/boilers/{id}` | Cập nhật |
| `DELETE` | `/api/ships/{shipId}/data/boilers/{id}` | Xóa |

---

## 5. FRONTEND COMPONENT STRUCTURE

### Thư mục: `frontend/src/pages/ShipData/`

```
ShipData/
├── ShipDataPage.tsx                    # Trang chính, chứa tab navigation
├── ShipDataPage.css                    # Style chung cho module
├── components/
│   ├── ShipDataHeader.tsx              # Header "SHIP'S DATA" + nút Save
│   ├── ShipDataTabs.tsx                # Tab navigation bar
│   ├── BasicDataTab.tsx                # Tab 1: Basic Data
│   ├── DimensionsTab.tsx               # Tab 2: Dimensions
│   ├── MachineryTab/
│   │   ├── MachineryTab.tsx            # Tab 3: Container chính
│   │   ├── MainEngineSection.tsx       # Section Main Engine(s)
│   │   ├── AuxiliaryEngineSection.tsx  # Section Auxiliary Engine(s)
│   │   ├── PropellerSection.tsx        # Section Propeller(s)
│   │   ├── AnchorChainSection.tsx      # Section Anchor Chain
│   │   ├── BowthrusterSection.tsx      # Section Bowthruster(s)
│   │   ├── SternthrusterSection.tsx    # Section Sternthruster(s)
│   │   ├── RudderSection.tsx           # Section Rudder(s)
│   │   ├── ShaftGeneratorSection.tsx   # Section Shaft Generator(s)
│   │   ├── BoilerSection.tsx           # Section Boiler(s)
│   │   ├── HarbourGeneratorSection.tsx # Section Harbour/Emergency Gen.
│   │   └── AzimuthEngineSection.tsx    # Section Azimuth Engine
│   ├── ShipownerTab/
│   │   ├── ShipownerTab.tsx            # Tab 4: Container chính
│   │   ├── CompanyCard.tsx             # Reusable card: Shipowner/ManagingOwner/Operator
│   │   └── PersonCard.tsx              # Reusable card: CSO/DPA/QI USA/QI Panama
│   ├── ChartererTab.tsx                # Tab 5: Charterer
│   ├── ClassFlagStateTab.tsx           # Tab 6: Class / Flag State
│   ├── InsuranceTab.tsx                # Tab 7: Insurance
│   ├── RadioCommTab/
│   │   ├── RadioCommTab.tsx            # Tab 8: Container chính
│   │   ├── InmarsatSection.tsx         # INMARSAT fields
│   │   ├── SeaAreasSection.tsx         # Sea areas checkboxes
│   │   ├── RadioEquipmentSection.tsx   # SELECT IF O/B matrix
│   │   └── EpirbSection.tsx            # EPIRB's Details
│   └── TanksCgoSpacesTab.tsx           # Tab 9: Tanks & Cargo Spaces
├── shared/
│   ├── PowerField.tsx                  # Component kW → HP auto-convert
│   ├── MetricField.tsx                 # Component m → ft'in" auto-convert
│   ├── CbmToCbFeetField.tsx            # Component cbm → cb feet auto-convert
│   ├── CountryDropdown.tsx             # Dropdown chọn quốc gia
│   ├── AddDeleteButtons.tsx            # Nút [+] thêm và [🗑] xóa
│   └── SectionCard.tsx                 # Card wrapper cho mỗi section
├── hooks/
│   ├── useShipData.ts                  # Hook lấy/lưu ship data
│   ├── useKwToHp.ts                    # Hook tính kW → HP (× 1.34102)
│   └── useMeterToFeet.ts               # Hook tính m → ft'in" (× 3.28084)
├── services/
│   └── shipDataService.ts             # API service calls
└── types/
    └── shipData.types.ts              # TypeScript interfaces
```

---

## 6. SHARED COMPONENTS

### 6.1. PowerField (kW → HP)

Component hiển thị 2 ô: nhập kW (editable), hiển thị HP (auto-calc, read-only, nền xám).

**Công thức:** `HP = kW × 1.34102`

```
┌──────────┬──────────┐
│ kW value │ HP value │  ← HP tự tính, background xám
└──────────┴──────────┘
```

### 6.2. MetricField (m → ft'in")

Component hiển thị 2 ô: nhập m (editable), hiển thị ft'in" (auto-calc, read-only, nền xám). Có nút ⊙ info tooltip (tùy chọn).

**Công thức:** 
- Tổng feet = `m × 3.28084`
- Phần nguyên = `floor(Tổng feet)` ft
- Phần inch = `round((Tổng feet - floor) × 12)` in
- Hiển thị: `ft' in"`

```
┌──────────┬──────────┐
│ 321.76 m │ 1055'7" │  ← auto-calc, background xám
└──────────┴──────────┘
```

### 6.3. CbmToCbFeetField (cbm → cb feet)

Component hiển thị 2 ô: nhập cbm (editable), hiển thị cb feet (auto-calc, read-only, nền xám).

**Công thức:** `cb feet = cbm × 35.3147`

### 6.4. CountryDropdown

Dropdown với khả năng search, hiển thị tên country viết hoa. Có nút X để xóa selection và ▽ để mở.

### 6.5. AddDeleteButtons

- Nút **+** (xanh dương): Thêm entry mới
- Nút **🗑** (đỏ): Xóa entry, có confirm dialog

### 6.6. SectionCard

Wrapper với tiêu đề in hoa, đường viền, nền trắng, padding.

---

## 7. KẾ HOẠCH TRIỂN KHAI

### Phase 1: Backend (3-4 ngày)

| # | Task | Thời gian |
|---|------|-----------|
| 1 | Tạo Model classes (ShipData + 8 bảng con) | 0.5 ngày |
| 2 | Tạo DbContext, Migration | 0.5 ngày |
| 3 | Tạo DTOs (Request/Response) | 0.5 ngày |
| 4 | Tạo Repository + Service layer | 1 ngày |
| 5 | Tạo ShipDataController + endpoints | 1 ngày |
| 6 | Unit tests | 0.5 ngày |

### Phase 2: Frontend — Shared Components (1-2 ngày)

| # | Task | Thời gian |
|---|------|-----------|
| 1 | PowerField component | 0.25 ngày |
| 2 | CountryDropdown component | 0.5 ngày |
| 3 | AddDeleteButtons component | 0.25 ngày |
| 4 | SectionCard component | 0.25 ngày |
| 5 | shipDataService (API calls) | 0.5 ngày |
| 6 | TypeScript types/interfaces | 0.25 ngày |

### Phase 3: Frontend — Các Tab (5-7 ngày)

| # | Tab | Thời gian | Độ phức tạp |
|---|-----|-----------|-------------|
| 1 | ShipDataPage + Header + Tab Navigation | 0.5 ngày | Thấp |
| 2 | Basic Data Tab | 0.5 ngày | Thấp |
| 3 | **Dimensions Tab** (Load Lines table + Tanker section + Pilot Card) | **1.5 ngày** | **Trung bình - Cao** |
| 4 | **Machinery Tab** (11 sections, dynamic add/delete) | **2 ngày** | **Cao** |
| 5 | **Shipowner Tab** (7 cards) | **1 ngày** | Trung bình |
| 6 | Charterer Tab | 0.5 ngày | Thấp |
| 7 | Class / Flag State Tab | 0.5 ngày | Thấp |
| 8 | Insurance Tab | 0.5 ngày | Thấp |
| 9 | **Radio Communication Equipment Tab** | **1 ngày** | Trung bình |
| 10 | Tanks & Cgo Spaces Tab | 0.5 ngày | Thấp |

### Phase 4: Tích hợp & Testing (2-3 ngày)

| # | Task | Thời gian |
|---|------|-----------|
| 1 | Kết nối Frontend ↔ Backend API | 1 ngày |
| 2 | Test toàn bộ CRUD | 0.5 ngày |
| 3 | Validation (required fields, format) | 0.5 ngày |
| 4 | Responsive design / UI polish | 0.5 ngày |
| 5 | Route registration + Sidebar menu | 0.25 ngày |
| 6 | Bug fixes | 0.5 ngày |

### **Tổng: ~14-18 ngày làm việc**

---

## 8. GHI CHÚ QUAN TRỌNG

### 8.1. Auto-calculate Fields

| Trường | Công thức |
|--------|-----------|
| **Tab Dimensions** | |
| LOA ft | `= LOA m × 3.28084` (hiển thị dạng ft'in") |
| LBP ft | `= LBP m × 3.28084` |
| Breadth moulded ft | `= Breadth m × 3.28084` |
| Depth moulded ft | `= Depth m × 3.28084` |
| Draft moulded ft | `= Draft m × 3.28084` |
| Draft Scantling ft | `= Draft m × 3.28084` |
| Draft Full Ballast ft | `= Draft m × 3.28084` |
| H – Max Airdraft ft | `= H m × 3.28084` |
| D – Distance ft | `= D m × 3.28084` |
| Bridge to Aft ft | `= m × 3.28084` |
| Bridge to Bow ft | `= m × 3.28084` |
| Bow to Bulbous Bow ft | `= m × 3.28084` |
| Parallel body (Ballast) ft | `= m × 3.28084` |
| Parallel body (Loaded) ft | `= m × 3.28084` |
| Load Lines Draft ft" | `= Draft m × 3.28084` |
| Grain CBM to CB Feet | `= Grain cbm × 35.3147` |
| Bales CBM to CB Feet | `= Bales cbm × 35.3147` |
| **Tab Machinery** | |
| ME Power HP | `= ME Power kW × 1.34102` |
| AE Power HP | `= AE Power kW × 1.34102` |
| Bowthruster HP | `= Power kW × 1.34102` |
| Sternthruster HP | `= Power kW × 1.34102` |
| Shaft Generator HP | `= Max Power kW × 1.34102` |
| Harbour Generator HP | `= Max Power kW × 1.34102` |
| Azimuth FWD HP | `= Max Power kW × 1.34102` |
| Azimuth AFT HP | `= Max Power kW × 1.34102` |
| Pitch Ratio | `= Propeller Pitch (mm) / Diameter (mm)` |

### 8.2. Dynamic Lists (Add/Delete)

Các section cho phép thêm/xóa nhiều bản ghi:
- Main Engine(s)
- Auxiliary Engine(s)
- Propeller(s)
- Bowthruster(s) (có N/A toggle)
- Sternthruster(s) (có N/A toggle)
- Rudder(s)
- Shaft Generator(s) (có N/A toggle)
- Boiler(s)

### 8.3. N/A Toggle Behavior

Khi tick checkbox **N/A**:
- Ẩn hoặc disable tất cả entries trong section
- Không cho thêm entry mới
- Lưu trạng thái N/A vào database

### 8.4. Save Behavior

- Nút **Save** (💾 xanh dương, góc trên phải) lưu toàn bộ dữ liệu của tất cả tabs
- Hiển thị loading indicator khi đang lưu
- Toast notification thành công / thất bại
- Validation trước khi lưu (highlight lỗi trên tab có lỗi)

### 8.5. Dropdown Values

| Dropdown | Giá trị |
|----------|---------|
| Type of vsl | Full container ship/cellular vessel, Bulk Carrier, Oil Tanker, Chemical Tanker, General Cargo, LNG Carrier, LPG Carrier, Ro-Ro, Passenger, Tug, Offshore Supply, FPSO |
| Flag / Previous Flag | (Danh sách ISO 3166-1 countries) |
| Port of Registry | (Danh sách cảng, có thể search) |
| Shipyard/Country | (Danh sách ISO 3166-1 countries) |
| VRP Type | NONTANK, TANK |
| Fuel Grade | HFO, MDO, MGO, VLSFO, ULSFO, LNG, Methanol, Ethanol |
| Propeller Type | (FPP) Fixed Pitch Propeller, (CPP) Controllable Pitch Propeller |
| Rotation | Clockwise, Counter-Clockwise |
| Rudder Type | Semi-balanced Spade, Balanced, Unbalanced, Flap Rudder, Schilling Rudder, Becker Rudder |
| Title | Mr., Ms., Mrs., Capt., Dr. |
| Classification Society | Bureau Veritas, Lloyd's Register, DNV, ABS, ClassNK, RINA, CCS, Korean Register, Indian Register |
| Country | (Danh sách ISO 3166-1, reuse từ module Crew) |
