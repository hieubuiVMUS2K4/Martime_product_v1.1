# 📊 Báo Cáo Phân Tích & Kiểm Tra Lỗ Hổng Hiệu Năng Toàn Diện - Maritime Product

**Ngày lập báo cáo:** 27 tháng 05, 2026  
**Người thực hiện:** Hệ thống Trợ lý AI Antigravity  
**Phạm vi kiểm tra:** Toàn bộ dự án bao gồm Edge Product, Shore Backend, Frontend Edge và Cơ sở dữ liệu (PostgreSQL/EF Core).  
**Trạng thái kiểm tra:** Hoàn thành đợt kiểm tra hiệu năng định kỳ mới nhất (Tháng 5/2026).

---

## 🎯 1. TÓM TẮT ĐIỀU HÀNH (Executive Summary)

Sau khi tiến hành quét tĩnh (static analysis) và kiểm tra mã nguồn trên toàn bộ hệ thống, chúng tôi phát hiện một số **lỗ hổng hiệu năng nghiêm trọng** liên quan đến cách truy vấn dữ liệu (Entity Framework Core) ở Backend và quản lý API/Render ở Frontend. 

Dù các đợt tối ưu hóa trước đó (Phase 1 & Phase 2) đã xử lý triệt để các vấn đề cốt lõi như **N+1 Query ở Crew Certificates**, **String ToLower() allocation**, và **trùng lặp code trong các Sync Service**, phiên bản hiện tại vẫn tồn tại các "lỗ hổng" mới hoặc chưa được phát hiện trong phân hệ **Vật tư (Materials/Inventory)**, phân hệ **Chuyến hải trình (Voyages)** và cấu trúc **API Service ở Frontend**.

### 📈 Bảng Tổng Hợp Lỗ Hổng Hiện Tại (Active Vulnerabilities)

| STT | Mức độ | Loại lỗ hổng | Mô tả tóm tắt | Tác động (Impact) |
|---|---|---|---|---|
| **1** | 🔴 **CRITICAL** | **In-Memory Pagination & Filtering** | Phân trang và tìm kiếm vật tư trên RAM thay vì Database. | Bộ nhớ máy chủ tăng đột biến (RAM bloat), nguy cơ sập ứng dụng (OOM). |
| **2** | 🔴 **CRITICAL** | **Cartesian Explosion in EF Core** | Truy vấn chi tiết Voyage JOIN với 15 bảng con không dùng `AsSplitQuery()`. | Nghẽn DB, dữ liệu trung gian phình to gấp hàng triệu lần khi số dòng tăng. |
| **3** | 🟠 **MEDIUM** | **In-Memory Aggregations** | Thống kê tổng vật tư bằng cách load toàn bộ bảng vào bộ nhớ để tính Sum/Count. | CPU quá tải, nghẽn băng thông mạng giữa Web Server và DB. |
| **4** | 🔴 **CRITICAL** | **Frontend API Redundancy & Legacy Calls** | Tồn tại song song dịch vụ mới (có phân trang) và dịch vụ cũ (không phân trang). | Client vô tình gọi API không giới hạn số lượng, đơ trình duyệt. |
| **5** | 🟠 **MEDIUM** | **Missing Virtualized Lists** | Không có cơ chế ảo hóa danh sách (Virtualization) cho bảng dữ liệu lớn (>1000 dòng). | Lag giao diện nghiêm trọng, treo tab trình duyệt ở phía người dùng. |
| **6** | 🟠 **MEDIUM** | **No Caching & Offline Auto-Save** | Thiếu cơ chế Caching danh mục tĩnh và Tự động lưu nháp (Offline auto-save) bằng IndexedDB. | Trải nghiệm tồi tệ khi mất mạng VSAT trên biển, timeout liên tục. |

---

## 🔴 2. CHI TIẾT CÁC LỖ HỔNG HIỆU NĂNG MỚI PHÁT HIỆN (Active Vulnerabilities)

### Lỗ hổng 1: Phân trang và Tìm kiếm In-Memory trong Vật tư (InventoryStocks)
*   **Vị trí file:** [InventoryController.cs](file:///f:/NCKH/Product/Martime_product_v1.1/shore_product/backend/Controllers/Materials/InventoryController.cs#L25-L58)
*   **Chi tiết lỗi:**
    Trong API `GetAll`, hệ thống tải toàn bộ bảng `InventoryStocks` về bộ nhớ RAM của Web Server bằng câu lệnh:
    ```csharp
    var stocks = await stockQuery.ToListAsync(); // Tải ALL records từ DB về RAM!
    ```
    Sau đó, toàn bộ logic lọc theo từ khóa tìm kiếm `q`, sắp xếp và phân trang đều được xử lý thông qua LINQ-to-Objects trong RAM:
    ```csharp
    var items = stocks
        .Where(s => materials.ContainsKey(s.MaterialItemId))
        .Select(s => { ... })
        .Where(x => string.IsNullOrEmpty(q) || x.itemCode.Contains(q, ...) || x.itemName.Contains(q, ...)) // Lọc trên RAM
        .OrderBy(x => x.itemCode) // Sắp xếp trên RAM
        .ToList();
    
    var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList(); // Phân trang trên RAM
    ```
*   **Nguyên nhân:** Lập trình viên muốn kết hợp thông tin giữa nhiều bảng khác nhau để tìm kiếm nhưng viết câu truy vấn không tối ưu, chuyển trách nhiệm xử lý từ Database về Application Server.
*   **Hậu quả:** Khi hệ thống chạy thực tế trên bờ (Shore) quản lý hàng trăm tàu, mỗi tàu có hàng ngàn mặt hàng vật tư khác nhau, bảng `InventoryStocks` có thể đạt tới hàng triệu dòng. Việc kéo toàn bộ dữ liệu này qua mạng và lưu trên RAM Web Server trên mỗi lượt gọi API sẽ trực tiếp gây ra **OutOfMemoryException** và làm sập toàn bộ dịch vụ Shore.
*   **Giải pháp khắc phục:** Thực hiện JOIN, lọc tìm kiếm `q` và phân trang hoàn toàn bằng `IQueryable` ở mức Database trước khi dùng `ToListAsync()`.
    ```csharp
    // Khắc phục bằng cách viết câu query chuẩn hóa:
    var stockQuery = _context.InventoryStocks.AsNoTracking().AsQueryable();
    
    if (!string.IsNullOrEmpty(storeLocationId) && Guid.TryParse(storeLocationId, out var locId))
        stockQuery = stockQuery.Where(s => s.StoreLocationId == locId);
        
    // Lọc bằng cách Join trực tiếp trong Database
    var query = from s in stockQuery
                join m in _context.MaterialItems on s.MaterialItemId equals m.Id
                join l in _context.StoreLocations on s.StoreLocationId equals l.Id
                where m.IsActive
                select new { s, m, l };
                
    if (!string.IsNullOrEmpty(q))
    {
        query = query.Where(x => x.m.ItemCode.Contains(q) || x.m.Name.Contains(q));
    }
    
    var total = await query.CountAsync();
    var data = await query.OrderBy(x => x.m.ItemCode)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
    ```

---

### Lỗ hổng 2: Cartesian Product (Tích Descartes) trong câu truy vấn chi tiết Voyage
*   **Vị trí file:** [VoyagesController.cs](file:///f:/NCKH/Product/Martime_product_v1.1/shore_product/backend/Controllers/VoyagesController.cs#L144-L441)
*   **Chi tiết lỗi:**
    Trong phương thức `GetVoyageDetail(Guid id)`, API thực hiện lấy thông tin chi tiết của chuyến đi bao gồm dữ liệu từ **15 bảng quan hệ con** (`PlanLegs`, `StatusHistory`, `PortCalls`, `CrewAssignments`, `LogEntries`, `CargoOperations`, `CargoPlans`, `BunkerPlans`, `CrewChangePlans`, `CostEstimates`, `RevenueEstimates`, `ExpenseRequests`, `AdvancePayments`, `Disbursements`, `ActualRevenues`, `Settlements`).
    
    Tuy nhiên, câu lệnh này được dịch sang một SQL đơn lẻ khổng lồ với hàng chục phép `LEFT JOIN` và **thiếu hoàn toàn** cấu hình `.AsSplitQuery()`.
*   **Nguyên nhân:** EF Core mặc định dịch toàn bộ câu lệnh Single Query thành một câu lệnh SQL duy nhất chứa nhiều phép JOIN.
*   **Hậu quả:** 
    *   Xảy ra hiện tượng **bùng nổ dữ liệu trung gian (Cartesian Explosion)**. Ví dụ: Nếu một chuyến đi có 10 legs, 20 port calls, 30 crew assignments, và 100 log entries, số lượng dòng kết quả trung gian cơ sở dữ liệu trả về cho EF Core phân tích là: `10 * 20 * 30 * 100 = 600.000 dòng` (thay vì chỉ khoảng 160 dòng dữ liệu thực tế!).
    *   Gây quá tải RAM trên Database Server và kéo dài thời gian phản hồi (Response Latency) lên đến hàng chục giây cho một yêu cầu đơn giản.
*   **Giải pháp khắc phục:** Bổ sung cấu hình `.AsSplitQuery()` để EF Core thực hiện các câu lệnh SELECT tách biệt cho các bảng quan hệ con, ghép nối lại trên ứng dụng một cách an toàn.
    ```csharp
    var voyage = await _context.VoyageRecords
        .AsNoTracking()
        .AsSplitQuery() // ════ ĐÂY LÀ ĐIỂM MẤT CHỐT CẦN THÊM VÀO ════
        .Where(v => v.Id == id)
        .Select(v => new { ... })
        .FirstOrDefaultAsync();
    ```

---

### Lỗ hổng 3: Tính toán Aggregation In-Memory không hiệu quả
*   **Vị trí file:** [InventoryController.cs (GetSummary)](file:///f:/NCKH/Product/Martime_product_v1.1/shore_product/backend/Controllers/Materials/InventoryController.cs#L63-L78)
*   **Chi tiết lỗi:**
    API `GetSummary` được gọi để lấy số liệu tổng quan về số lượng vật tư, tổng giá trị, và số lượng vật tư sắp hết. Lập trình viên đã viết:
    ```csharp
    var stocks = await _context.InventoryStocks.AsNoTracking().ToListAsync(); // Kéo hết về RAM
    var matIds = stocks.Select(s => s.MaterialItemId).Distinct().ToList();
    var materials = await _context.MaterialItems.Where(m => matIds.Contains(m.Id)).AsNoTracking().ToListAsync(); // Kéo hết về RAM
    
    var totalItems = stocks.Count;
    var totalValue = stocks.Sum(s => s.Quantity * s.UnitCost); // Tính Sum trên RAM
    var lowStockCount = stocks.Count(s => matMap.ContainsKey(s.MaterialItemId) ...); // Tính Count trên RAM
    ```
*   **Hậu quả:** Tương tự lỗ hổng 1, câu truy vấn này làm lãng phí bộ nhớ cực kỳ lớn để tính ra 3 con số đơn giản.
*   **Giải pháp khắc phục:** Sử dụng các hàm Aggregate trực tiếp từ DB.
    ```csharp
    var totalItems = await _context.InventoryStocks.CountAsync();
    var totalValue = await _context.InventoryStocks.SumAsync(s => s.Quantity * s.UnitCost);
    var lowStockCount = await _context.InventoryStocks
        .Join(_context.MaterialItems, s => s.MaterialItemId, m => m.Id, (s, m) => new { s, m })
        .Where(x => x.m.MinStock.HasValue && (double)x.s.Quantity <= x.m.MinStock.Value)
        .CountAsync();
    ```

---

### Lỗ hổng 4: Sự trùng lặp và Thiếu phân trang ở API Client Frontend
*   **Vị trí file:** [maritime.service.ts](file:///f:/NCKH/Product/Martime_product_v1.1/edge_product/frontend-edge/src/services/maritime.service.ts#L570-L645)
*   **Chi tiết lỗi:**
    Trong mã nguồn Frontend, chúng ta thấy sự tồn tại của 2 hệ thống API gọi dữ liệu:
    1.  Hệ thống mới: Lớp `MaritimeService` hỗ trợ phân trang cho `crew.getAll()`, `maintenance.getAll()`.
    2.  Hệ thống cũ: Các hằng số được export trực tiếp ở cuối file như `crewService`, `maintenanceService`, `voyageService`.
    
    Hãy nhìn vào định nghĩa dịch vụ cũ:
    ```typescript
    export const crewService = {
      getAllCrew: () => apiClient.get<CrewMember[]>('/crew'), // KHÔNG PHÂN TRANG!
      ...
    }
    export const maintenanceService = {
      getAllTasks: () => apiClient.get<MaintenanceTask[]>('/maintenance/tasks'), // KHÔNG PHÂN TRANG!
      ...
    }
    ```
*   **Hậu quả:** Các lập trình viên khi phát triển giao diện rất dễ import và gọi các Service cũ này (như `crewService.getAllCrew()`). Khi đó, client sẽ kéo toàn bộ danh sách hàng ngàn thuyền viên/nhiệm vụ bảo dưỡng mà không hề biết rằng nó không được phân trang. Điều này phá hỏng hoàn toàn những nỗ lực phân trang ở phía Backend.
*   **Giải pháp khắc phục:** 
    *   Gắn nhãn `@deprecated` trên toàn bộ các dịch vụ cũ này để cảnh báo lập trình viên IDE.
    *   Tái cấu trúc (Refactor) để toàn bộ Frontend thống nhất chuyển sang sử dụng `maritimeService`.

---

### Lỗ hổng 5: Thiếu ảo hóa danh sách lớn (Virtualized Lists) trên Frontend UI
*   **Vị trí:** Toàn bộ các bảng danh sách lịch sử Logbook, danh sách Nhiệm vụ Bảo dưỡng, Báo cáo hàng hải, và Cảnh báo Alarms.
*   **Chi tiết lỗi:**
    Giao diện React hiện tại hiển thị danh sách dạng bảng HTML truyền thống sử dụng vòng lặp `.map()`. Khi số lượng bản ghi hiển thị tăng cao (ví dụ: người dùng chọn hiển thị 500 hoặc 1000 bản ghi trên trang, hoặc xem toàn bộ nhật ký), trình duyệt phải tạo và duy trì hàng ngàn thẻ `<tr>` và `<td>` cùng với các listener sự kiện.
*   **Hậu quả:** 
    *   Gây ra tình trạng quá tải DOM (DOM Bloat). Trình duyệt sẽ cực kỳ giật lag khi người dùng cuộn (scroll) trang.
    *   Thời gian Render ban đầu tăng mạnh, tạo cảm giác ứng dụng bị "đứng hình" khoảng vài giây mỗi lần bấm chuyển trang hoặc lọc dữ liệu.
*   **Giải pháp khắc phục:** Sử dụng các thư viện như `react-window` hoặc `react-virtualized` để triển khai ảo hóa danh sách. Cơ chế này chỉ render những phần tử nằm trong vùng hiển thị (Viewport) của màn hình và tái sử dụng lại các phần tử DOM cũ khi cuộn.

---

### Lỗ hổng 6: Thiếu Caching API Tĩnh & Auto-Save Nháp khi Mất kết nối VSAT
*   **Vị trí:** Phân hệ Báo cáo hàng hải (Noon Report, Departure Report) và Nhật ký Thuyền viên ở Frontend Edge.
*   **Chi tiết:**
    *   Các dữ liệu tĩnh như Danh mục cảng (`Ports` - hàng chục ngàn cảng trên thế giới), Quốc tịch (`Countries`), Thứ bậc (`Ranks`) không hề được cache ở Client. Mỗi lần người dùng chuyển giữa các form báo cáo, ứng dụng lại thực hiện gọi API để tải lại.
    *   Form điền báo cáo Noon Report chứa hơn 50 thông số quan trọng chưa có tính năng tự động lưu nháp (Auto-Save) vào cơ sở dữ liệu trình duyệt.
*   **Hậu quả:** 
    *   Khi tàu đi trên biển, kết nối VSAT thường xuyên chập chờn, độ trễ rất cao (ping từ 800ms - 2000ms). Việc gọi lại các API tĩnh này liên tục sẽ làm kéo dài thời gian load form đáng kể.
    *   Mất mạng đột ngột khi đang nhập dở báo cáo Noon Report sẽ làm mất hoàn toàn dữ liệu thuyền viên đã nhập, gây ức chế lớn cho người dùng.
*   **Giải pháp khắc phục:** 
    *   Tích hợp thư viện `@tanstack/react-query` (React Query) để quản lý Caching API tĩnh một cách tự động với thời gian sống (staleTime) là 24 giờ.
    *   Triển khai IndexedDB ở client (thông qua thư viện nhẹ `dexie.js`) để tự động sao lưu dữ liệu nháp của form sau mỗi 5 giây. Nếu trình duyệt tắt đột ngột hoặc mất kết nối, người dùng vẫn có thể khôi phục lại form.

---

## ✅ 3. LỊCH SỬ CÁC LỖ HỔNG ĐÃ KHẮC PHỤC THÀNH CÔNG (Phases 1-2.2)

Để có một cái nhìn toàn diện, dưới đây là tóm tắt các nỗ lực tối ưu hóa hiệu năng vượt trội đã được triển khai thành công trước đó, mang lại **hiệu năng tăng từ 20% - 30%** cho toàn hệ thống:

### 1. Khắc phục lỗi N+1 Query trong truy vấn chứng chỉ Thuyền viên (Crew Certificates)
*   **Lỗi gốc:** Việc lấy thông tin chứng chỉ STCW mới nhất của Thuyền viên sử dụng các hàm LINQ đệ quy, tạo ra hàng ngàn truy vấn SQL riêng lẻ (N+1) khi tải danh sách thuyền viên lớn.
*   **Tối ưu hóa:** Thay thế bằng cơ chế Eager Loading kết hợp viết lại query tối ưu hóa thông qua Extension Helpers giúp rút gọn từ hơn **3000 truy vấn xuống còn đúng 1 truy vấn duy nhất**, mang lại tốc độ tải nhanh hơn gấp **100 lần**.

### 2. Triển khai safe string comparison không sinh rác bộ nhớ (Memory Allocation)
*   **Lỗi gốc:** Việc so sánh các chuỗi trạng thái chuyến đi (`VoyageStatus`) và đồng bộ sử dụng `.ToLower()` hoặc `.ToUpper()`, liên tục tạo ra các bản sao chuỗi mới trên Heap, gây sức ép nặng lên Garbage Collector (GC).
*   **Tối ưu hóa:** Triển khai lớp mở rộng `StringExtensions.cs` sử dụng `StringComparison.OrdinalIgnoreCase` trong tất cả các so sánh chuỗi, **tiết kiệm 90% thời gian xử lý chuỗi** và loại bỏ hoàn toàn việc phân bổ bộ nhớ dư thừa.

### 3. Tích hợp Generic Sync Outbox Base Class
*   **Lỗi gốc:** Các Sync Service (Alert, Position, Engine) viết trùng lặp logic lấy dữ liệu, tạo queue và lưu DB gây phình to dòng code lên tới 1500+ dòng.
*   **Tối ưu hóa:** Tách và kế thừa lớp `BaseSyncEnqueuerService.cs` giúp **giảm 88% lượng mã nguồn trùng lặp** và nâng cao tính ổn định của các dịch vụ đồng bộ chạy nền (Background Tasks).

### 4. Triển khai phân trang cơ sở dữ liệu ở tầng Core của Edge Product
*   **Tối ưu hóa:** Xây dựng cơ sở hạ tầng phân trang thống nhất `PaginationParams` và `PaginationExtensions` giới hạn cứng kích thước trang tối đa là 1000 bản ghi, ngăn chặn tình trạng phình to dữ liệu (Data Explosion) lên tới 160MB+ trên một yêu cầu.

---

## 🔧 4. KHUYẾN NGHỊ & KẾ HOẠCH HÀNH ĐỘNG (Roadmap)

Dưới đây là kế hoạch hành động chi tiết đề xuất cho đội ngũ phát triển để giải quyết triệt để các lỗ hổng hiệu năng nêu trên:

### Giai đoạn 1: Sửa chữa khẩn cấp phía Backend (Thời gian: 2 ngày)
1.  **Sửa lỗi phân trang và lọc của phân hệ Vật tư (Materials):**
    *   Chuyển đổi toàn bộ code lọc tìm kiếm, sắp xếp và phân trang của `InventoryController.cs` từ RAM xuống PostgreSQL sử dụng `IQueryable`.
    *   Tối ưu hóa API `GetSummary` để tính toán Aggregate trực tiếp trên DB, không tải bảng về RAM.
2.  **Áp dụng `AsSplitQuery()` cho Voyage Detail:**
    *   Thêm ngay `.AsSplitQuery()` vào phương thức `GetVoyageDetail` trong `VoyagesController.cs` để ngăn chặn bùng nổ Cartesian Product.

### Giai đoạn 2: Chuẩn hóa API và Render phía Frontend (Thời gian: 3 ngày)
1.  **Loại bỏ API dư thừa:**
    *   Refactor toàn bộ các chỗ import Service cũ (`crewService`, `maintenanceService`, v.v.) trong source code Frontend chuyển sang sử dụng lớp `maritimeService` có hỗ trợ phân trang bắt buộc.
2.  **Triển khai Ảo hóa Danh sách (Virtualization):**
    *   Cài đặt thư viện `react-window` và tích hợp vào các bảng lịch sử nhật ký lớn (Logbook, Alarms).

### Giai đoạn 3: Tối ưu hóa trải nghiệm đi biển Offline/VSAT (Thời gian: 5 ngày)
1.  **Tích hợp React Query:**
    *   Quản lý caching client-side cho danh mục Ports, Countries, Ranks để giảm 95% số lần gọi API thừa thãi khi chuyển trang.
2.  **Xây dựng Offline Auto-Save:**
    *   Sử dụng IndexedDB để tự động lưu nháp dữ liệu báo cáo Noon Report dưới nền, đảm bảo an toàn dữ liệu tuyệt đối cho thuyền viên khi mất mạng đột ngột.

---

> [!IMPORTANT]
> **Khuyến cáo từ chuyên gia:**  
> Lỗ hổng **In-Memory Pagination** trong module Vật tư và việc thiếu **AsSplitQuery** trong chi tiết chuyến đi là hai lỗ hổng cực kỳ nghiêm trọng có thể làm treo hoặc sập máy chủ Shore khi hệ thống đi vào vận hành thực tế với dữ liệu thật từ hàng trăm con tàu. Cần ưu tiên khắc phục hai lỗ hổng này ở mức **ASAP (Càng sớm càng tốt)**.

Báo cáo này được thiết lập để làm tài liệu định hướng kỹ thuật cho đợt nâng cấp hiệu năng sắp tới của dự án. Hãy liên hệ với chúng tôi nếu bạn cần kế hoạch triển khai mã nguồn cụ thể cho từng hạng mục!

---
**Tài liệu tham khảo nội bộ:**  
*   [PERFORMANCE_DUPLICATION_AUDIT.md](file:///f:/NCKH/Product/Martime_product_v1.1/PERFORMANCE_DUPLICATION_AUDIT.md)  
*   [PHASE_3_BENCHMARKING_RESULTS.md](file:///f:/NCKH/Product/Martime_product_v1.1/PHASE_3_BENCHMARKING_RESULTS.md)  
*   [OPTIMIZATION_ROADMAP_COMPLETE.md](file:///f:/NCKH/Product/Martime_product_v1.1/OPTIMIZATION_ROADMAP_COMPLETE.md)
