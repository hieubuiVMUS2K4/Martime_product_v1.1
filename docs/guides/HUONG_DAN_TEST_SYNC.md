# Hướng Dẫn Test Lỗi Đồng Bộ FullName - Bước Cuối

## Tình Huống
User báo: Đã edit "Nguyen Van Minh" thành "Nguyen Van Minh ádasdasd" ở Edge và nhấn Sync Now, nhưng Shore không cập nhật.

## Đã Chuẩn Bị

✅ **Shore backend** đã restart với logging chi tiết (port 5000):
- Log conflict resolution với timestamps
- Log đặc biệt cho FullName changes

✅ **Edge backend** đã restart với logging chi tiết (port 5001):
- Log khi capture thay đổi trong SaveChanges: `[EDGE-SYNC] Queued UPDATE: crew_member/...`
- Log khi gửi tới Shore: `Sending X crew_member updates to Shore`
- Log kết quả: `Shore accepted batch` hoặc error

## Các Bước Test (Quan Trọng - Làm Chính Xác)

### Bước 1: Mở 3 Terminal Windows

**Terminal 1 - Shore Logs:**
```powershell
# Đang chạy tại: shore_product/backend
# Xem log realtime của Shore
```

**Terminal 2 - Edge Logs:**
```powershell
# Đang chạy tại: edge_product/edge-services  
# Xem log realtime của Edge
```

**Terminal 3 - Thực Hiện Test:**
```powershell
# Sẽ chạy commands để test
```

### Bước 2: Test Đồng Bộ

**A. Vào Edge UI** (http://localhost:3001 hoặc frontend của Edge):

1. Đăng nhập
2. Vào trang Crew Management
3. Tìm crew member "Nguyen Van Minh" (hoặc bất kỳ crew nào)
4. Click Edit / Chỉnh sửa
5. **Thay đổi FullName** thành "Nguyen Van Minh TEST123"
6. **Click Save/Lưu**
7. **Quan Sát Terminal 2 (Edge Logs)** - phải thấy:
   ```
   [EDGE-SYNC] Queued UPDATE: crew_member/{guid} with FullName='Nguyen Van Minh TEST123'
   ```
   
   ✅ Nếu thấy dòng này → Edge ĐÃ capture thay đổi
   
   ❌ Nếu KHÔNG thấy → **DỪNG LẠI** - Vấn đề là Edge không detect change. Nguyên nhân có thể:
      - Chưa click Save
      - Frontend không gọi API
      - EF Change Tracking bị lỗi

**B. Nhấn Sync Now:**

8. Trong Edge UI, tìm nút "Sync Now" hoặc "Đồng bộ ngay"
9. Click vào nút đó
10. **Quan Sát Terminal 2 (Edge Logs)** - phải thấy:
    ```
    Manual sync triggered via API
    Sending 1 crew_member updates to Shore:
      - crew_member/{guid} UPDATE
    Posting batch of X items to http://localhost:5000/api/sync
    Shore accepted batch: X items synced
    ```
    
    ✅ Nếu thấy "Shore accepted batch" → Edge ĐÃ GỬI thành công
    
    ❌ Nếu thấy error "Cannot reach shore API" → Shore không chạy hoặc sai URL
    
    ❌ Nếu không thấy "Sending X crew_member updates" → SyncQueue rỗng hoặc item đã synced

**C. Kiểm Tra Shore:**

11. **Quan Sát Terminal 1 (Shore Logs)** - phải thấy:
    ```
    [INF] CrewMember conflict resolution: origin=9876543, existingUpdated=..., incomingUpdated=..., incomingIsNewer=True
    [INF] CrewMember FullName change: 'Nguyen Van Minh' → 'Nguyen Van Minh TEST123', shouldApply=True
    ```
    
    ✅ Nếu thấy `shouldApply=True` → Shore ĐÃ APPLY thay đổi
    
    ❌ Nếu thấy `shouldApply=False` → Conflict resolution reject (check timestamp logic)
    
    ❌ Nếu không thấy log này → Shore không nhận request hoặc table không phải crew_member

12. **Vào Shore UI** (http://localhost:3000):
    - Refresh trang
    - Tìm crew member "Nguyen Van Minh"
    - Xem FullName có phải "Nguyen Van Minh TEST123" không
    
    ✅ Nếu đúng → HOÀN TẤT! Sync đã work
    
    ❌ Nếu vẫn "Nguyen Van Minh" → Database không save

### Bước 3: Nếu Vẫn Lỗi - Query Database Trực Tiếp

**Option A: Nếu có psql trong PATH:**
```powershell
# Shore database
$env:PGPASSWORD='maritime'
psql -U maritime -d shore_product -c "SELECT \"Id\", \"FullName\", \"UpdatedAt\", \"IsSynced\", \"OriginNode\", \"SyncVersion\" FROM crew_members WHERE \"FullName\" LIKE '%Nguyen Van Minh%';"

# Edge database  
$env:PGPASSWORD='maritime'
psql -U maritime -d maritime_edge -c "SELECT id, full_name, updated_at, is_synced, origin_node FROM crew_member WHERE full_name LIKE '%Nguyen Van Minh%';"
```

**Option B: Nếu không có psql - Dùng API:**
```powershell
# Shore: Get sync logs
Invoke-WebRequest -Uri "http://localhost:5000/api/sync/status" -UseBasicParsing | ConvertFrom-Json | Select-Object -ExpandProperty recentLogs | Where-Object { $_.tableName -eq 'crew_member' } | Format-Table

# Edge: Cần auth - skip hoặc login trước
```

## Các Trường Hợp Lỗi Và Giải Pháp

### Case 1: Edge Không Capture Change
**Triệu chứng:** Terminal 2 không thấy `[EDGE-SYNC] Queued UPDATE`

**Nguyên nhân:**
1. Frontend không gọi PUT /api/crew/{id}
2. User edit nhưng không Save
3. EF Change Tracking không detect (FullName value giống value cũ)

**Kiểm tra:**
```powershell
# Check Edge logs có thấy "Updated crew member: {Id} - {FullName}" không
# Nếu thấy dòng này từ CrewController → Backend đã nhận request
# Nhưng không thấy [EDGE-SYNC] → ProcessSyncQueue skip

# Tình huống: User edit "Nguyen Van Minh" → "Nguyen Van Minh" (giống nhau)
# → EF không detect change → không queue sync
```

**Giải pháp:** Đảm bảo edit FullName thành giá trị KHÁC hẳn

### Case 2: Edge Gửi Nhưng Shore Không Nhận
**Triệu chứng:** Terminal 2 thấy "Posting batch" nhưng Terminal 1 không có log

**Nguyên nhân:**
1. Shore backend không chạy (port 5000)
2. Firewall block
3. Edge config sai BaseUrl

**Kiểm tra:**
```powershell
# Test Shore API
Invoke-WebRequest -Uri "http://localhost:5000/api/sync/status" -UseBasicParsing

# Xem Edge config
Get-Content edge_product/edge-services/appsettings.json | Select-String -Pattern "ShoreAPI" -Context 5
```

**Giải pháp:** 
- Restart Shore backend
- Fix ShoreAPI:BaseUrl trong appsettings.json
- Check firewall

### Case 3: Shore Nhận Nhưng Reject
**Triệu chứng:** Terminal 1 thấy conflict resolution log với `shouldApply=False`

**Nguyên nhân:** 
- Edge UpdatedAt timestamp cũ hơn Shore
- Conflict resolution logic bug

**Kiểm tra:**
```powershell
# Xem log conflict resolution chi tiết tại Terminal 1
# Check dòng: existingUpdated=... vs incomingUpdated=...
```

**Giải pháp:**
- Đảm bảo Edge có timestamp mới hơn
- Fix conflict resolution logic nếu cần

### Case 4: Shore Apply Nhưng Database Không Update
**Triệu chứng:** Terminal 1 thấy `shouldApply=True` nhưng UI không đổi

**Nguyên nhân:**
- SaveChangesAsync throw exception
- Transaction rollback
- Database connection issue

**Kiểm tra:**
```powershell
# Xem Terminal 1 có error sau log conflict resolution không
# Tìm dòng: "Error", "Exception", "Failed"
```

**Giải pháp:**
- Check database connection
- Check constraints, foreign keys
- Check terminal logs đầy đủ

## Test Thành Công - Kết Quả Mong Đợi

Sau khi làm đúng các bước trên, bạn sẽ thấy:

**Terminal 2 (Edge):**
```
[EDGE-SYNC] Queued UPDATE: crew_member/a1b2c3d4-... with FullName='Nguyen Van Minh TEST123'
[INF] Manual sync triggered via API
[INF] Sending 1 crew_member updates to Shore:
[INF]   - crew_member/a1b2c3d4-... UPDATE
[INF] Posting batch of 5 items to http://localhost:5000/api/sync
[INF] Shore accepted batch: 5 items synced
[INF] Manual sync done. Synced: 5, Remaining: 0
```

**Terminal 1 (Shore):**
```
[INF] CrewMember conflict resolution: origin=9876543, existingUpdated=2026-03-06 10:00:00, incomingUpdated=2026-03-06 10:05:00, incomingIsNewer=True
[INF] CrewMember FullName change: 'Nguyen Van Minh' → 'Nguyen Van Minh TEST123', shouldApply=True
```

**Shore UI:**
- Crew member "Nguyen Van Minh TEST123" xuất hiện
- UpdatedAt timestamp mới
- OriginNode = "9876543" (từ Edge)

## Nếu Vẫn Không Work - Báo Cáo

Nếu sau khi test đúng các bước trên vẫn không work, hãy:

1. **Copy toàn bộ logs** từ Terminal 1 và 2
2. **Chụp màn hình** Edge UI (trang edit crew) và Shore UI (sau khi sync)
3. **Ghi lại chính xác:**
   - Crew member nào đã edit
   - FullName cũ và mới
   - Thời gian test
   - Logs nào thấy, logs nào không thấy
4. **Gửi cho dev với đầy đủ thông tin trên**

## Files Liên Quan

- [DEBUG_FULLNAME_SYNC.md](DEBUG_FULLNAME_SYNC.md) - Tổng quan vấn đề
- [test-crew-fullname-sync.ps1](test-crew-fullname-sync.ps1) - Script test API trực tiếp

- **Shore backend:** shore_product/backend/Services/Sync/ConflictResolverService.cs (đã có logging)
- **Edge backend:** edge_product/edge-services/Data/EdgeDbContext.cs (đã có logging)
- **Edge sync:** edge_product/edge-services/Services/Core/SyncService.cs (đã có logging)