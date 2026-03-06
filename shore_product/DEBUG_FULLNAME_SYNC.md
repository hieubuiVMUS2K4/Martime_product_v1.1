# Hướng Dẫn Debug Lỗi Đồng Bộ FullName

## Vấn Đề
User báo: "tôi nhớ đợt trước tôi có gửi tên Tran Duc Thanh 9999 gì đó nhưng vào shore thì chỉ là Tran Duc Thanh thoi"

## Những Gì Đã Làm
1. ✅ **Đã thêm logging chi tiết** vào `ConflictResolverService.cs`:
   - Log tất cả conflict resolution với timestamps
   - Log đặc biệt cho FullName changes (giá trị cũ → giá trị mới)
   - Log `shouldApply` flag để xem có apply hay không

2. ✅ **Đã restart backend** với code mới có logging

3. ✅ **Đã verify logic conflict resolution**:
   - `FullName` KHÔNG có trong `_shoreAuthoritativeCrewProps` (chỉ có SocialInsuranceNumber, TaxIdNumber)
   - Vậy khi Edge gửi UPDATE, logic sẽ set `shouldApply = true`
   - FullName từ Edge **NÊN LUÔN được apply**

##Cách Kiểm Tra

### Option 1: Test Với Script (KHUYẾN NGHỊ)
```powershell
# 1. Tìm ID của crew member "Tran Duc Thanh"
psql -U maritime -d shore_product -c "SELECT \"Id\", \"CrewId\", \"FullName\", \"UpdatedAt\" FROM crew_members WHERE \"FullName\" LIKE '%Tran Duc Thanh%';"

# 2. Copy ID từ kết quả (ví dụ: a1b2c3d4-e5f6-7890-abcd-ef1234567890)

# 3. Edit file test-crew-fullname-sync.ps1, thay "00000000-0000-0000-0000-000000000001" bằng ID thực

# 4. Chạy test script
.\shore_product\test-crew-fullname-sync.ps1

# 5. Check backend logs - tìm dòng:
#    - "CrewMember conflict resolution: origin=9876543..."
#    - "CrewMember FullName change: 'Tran Duc Thanh' → 'Tran Duc Thanh 9999', shouldApply=True"

# 6. Query lại database xem có update không
psql -U maritime -d shore_product -c "SELECT \"FullName\", \"UpdatedAt\" FROM crew_members WHERE \"Id\" = 'YOUR_CREW_ID';"
```

### Option 2: Test Từ Edge (Thực Tế)
```powershell
# 1. Vào Edge system, tìm crew member "Tran Duc Thanh"

# 2. Edit FullName thành "Tran Duc Thanh 9999"

# 3. Save

# 4. Đợi 60 giây (Edge sync mỗi 60s) hoặc restart Edge backend để force sync ngay

# 5. Vào Shore backend logs, tìm các dòng:
#    [INF] CrewMember conflict resolution: origin=9876543, existingUpdated=..., incomingUpdated=..., incomingIsNewer=True
#    [INF] CrewMember FullName change: 'Tran Duc Thanh' → 'Tran Duc Thanh 9999', shouldApply=True

# 6. Vào Shore UI, refresh trang và xem crew member có FullName = "Tran Duc Thanh 9999" không
```

### Option 3: Check Sync Logs
```powershell
# Query sync_logs để xem lịch sử sync crew_member
psql -U maritime -d shore_product -c "
    SELECT 
        origin_node,
        table_name,
        record_key,
        action_type,
        status,
        conflict_detail,
        processed_at 
    FROM sync_logs 
    WHERE table_name = 'crew_member' 
    ORDER BY processed_at DESC 
    LIMIT 20;
"

# Nếu thấy status = 'CONFLICT' hoặc 'FAILED' → có lỗi
# Nếu thấy status = 'SUCCESS' → đã sync thành công, cần kiểm tra database
```

## Các Nguyên Nhân Có Thể

### 1. Edge Không Gửi FullName Trong Payload
**Triệu chứng**: Backend log không thấy "CrewMember FullName change"

**Giải pháp**: Check Edge code ở `EdgeDbContext.SaveChangesAsync()`:
- Khi UPDATE, Edge chỉ gửi changed properties
- Nếu FullName không changed, sẽ không có trong payload
- Verify Edge có detect FullName change không

### 2. User Xem Nhầm Crew Member
**Triệu chứng**: Database có nhiều crew member tên "Tran Duc Thanh"

**Giải pháp**: Query database:
```sql
SELECT "Id", "CrewId", "FullName", "DateOfBirth", "CrewId", "UpdatedAt" 
FROM crew_members 
WHERE "FullName" LIKE '%Tran Duc Thanh%' 
ORDER BY "UpdatedAt" DESC;
```
Xem có bao nhiêu records. User có thể edit record A nhưng xem record B.

### 3. Frontend Display Bị Truncate
**Triệu chứng**: Database có "Tran Duc Thanh 9999" nhưng UI chỉ hiện "Tran Duc Thanh"

**Giải pháp**: 
- Check CSS của FullName column có `text-overflow: ellipsis` không
- Check table column width có bị limit không
- F12 DevTools → inspect cell → xem full content

### 4. Race Condition (Ít Có Thể)
**Triệu chứng**: 
- Edge gửi "Tran Duc Thanh 9999" lên Shore
- Shore apply thành công
- Sau đó Shore tự đẩy ngược lại "Tran Duc Thanh" về Edge
- Edge apply và overwrite

**Giải pháp**: Check logs xem có pattern:
```
[1] EDGE_TO_SHORE: crew_member UPDATE FullName → shore
[2] SHORE_TO_EDGE: crew_member UPDATE FullName → edge (sau vài giây)
```

Nhưng tôi đã verify Shore KHÔNG tự động broadcast sau khi nhận sync, nên case này ít có thể.

### 5. Database Column Truncation (Đã Loại Trừ)
**Triệu chứng**: Database column FullName có limit quá ngắn

**Kết quả**: Migration cho thấy `FullName VARCHAR(200)` → đủ cho "Tran Duc Thanh 9999"

## Checklist Troubleshooting

- [ ] Backend đang chạy với code mới (có logging chi tiết)
- [ ] Đã identify đúng crew member ID (không nhầm record khác)
- [ ] Test sync với script hoặc từ Edge
- [ ] Check backend logs thấy "CrewMember FullName change" log
- [ ] Check `shouldApply=True` trong log
- [ ] Query database verify FullName đã update
- [ ] Check sync_logs table thấy status='SUCCESS'
- [ ] Check frontend UI không bị CSS truncate

## Kết Luận Tạm Thời

Dựa trên code analysis:
- Conflict resolution logic **ĐÚNG** - FullName từ Edge luôn được apply
- Có thể là:
  1. **Edge bug** - không gửi FullName trong UPDATE payload
  2. **User confusion** - nhầm record hoặc nhớ sai (đợt trước có bug đã fix?)
  3. **Frontend display issue** - database đúng nhưng UI hiển thị sai

Cần user test lại với logging mới để xác định chính xác nguyên nhân.

## Files Đã Thay Đổi

1. **shore_product/backend/Services/Sync/ConflictResolverService.cs**
   - Thêm log conflict resolution với timestamps
   - Thêm log đặc biệt cho FullName changes

2. **shore_product/test-crew-fullname-sync.ps1** (MỚI)
   - Test script để simulate Edge → Shore sync
   - Gửi POST /api/sync với crew_member UPDATE payload

## Backend Status

✅ Backend đang chạy trên http://localhost:5000
✅ Logging đã được enhance
✅ Ship 9876543 online, đã nhận 20,595 sync items
✅ Sẵn sàng để test
