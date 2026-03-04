# Database Scripts

## SeedReportTypes.sql

Script để seed dữ liệu mẫu cho bảng `report_types`.

### Cách sử dụng:

#### Option 1: Chạy qua psql (PostgreSQL)
```bash
psql -h localhost -U postgres -d edge_db -f SeedReportTypes.sql
```

#### Option 2: Chạy qua pgAdmin
1. Mở pgAdmin
2. Kết nối đến database `edge_db`
3. Mở Query Tool (Tools > Query Tool)
4. Copy nội dung file `SeedReportTypes.sql` và paste vào
5. Click Execute (F5)

#### Option 3: Chạy qua C# Code (EF Core)
```bash
cd edge-services
dotnet run --seed-data
```

### Dữ liệu được seed:

| Type Code | Type Name | Mandatory | Frequency | Master Signature Required |
|-----------|-----------|-----------|-----------|---------------------------|
| NOON | Noon Report | Yes | DAILY | Yes |
| DEPARTURE | Departure Report | Yes | VOYAGE | Yes |
| ARRIVAL | Arrival Report | Yes | VOYAGE | Yes |
| BUNKER | Bunker Delivery Note Report | Yes | EVENT | Yes |
| POSITION | Position Report | No | EVENT | No |

### Kiểm tra sau khi seed:

```sql
SELECT type_code, type_name, is_mandatory, frequency 
FROM report_types 
ORDER BY type_code;
```

Kết quả mong đợi: 5 rows
