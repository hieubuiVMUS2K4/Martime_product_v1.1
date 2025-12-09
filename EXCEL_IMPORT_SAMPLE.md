# File Excel Mẫu cho Import Phiếu Nhập Kho

## Cách sử dụng:

1. Tạo file Excel mới (.xlsx)
2. Copy dữ liệu bên dưới vào Excel (bao gồm header)
3. Lưu file
4. Upload vào hệ thống

## Dữ liệu mẫu (Copy vào Excel):

```
ItemCode	ItemName	Category	Quantity	Unit	UnitCost	PartNumber	Barcode	Manufacturer	Specification	MinStock	MaxStock
MAT-001	Bolt M10 x 50mm	Fasteners	100	pcs	0.5	BLT-M10-50	8901234567890	ABC Fasteners Co.	High tensile steel bolt	50	500
MAT-002	Nut M10	Fasteners	200	pcs	0.3	NUT-M10	8901234567891	ABC Fasteners Co.	Hexagonal nut	100	1000
MAT-003	Washer M10	Fasteners	300	pcs	0.1	WSH-M10	8901234567892	ABC Fasteners Co.	Flat washer	150	1500
MAT-004	Engine Oil SAE 40	Lubricants	50	liters	15.5	OIL-SAE40	8901234567893	Shell Marine	Marine diesel oil	20	200
MAT-005	Hydraulic Oil ISO 68	Lubricants	30	liters	18.0	HYD-68	8901234567894	Castrol Marine	Hydraulic fluid	15	150
MAT-006	Grease Lithium	Lubricants	20	kg	12.5	GRS-LTH	8901234567895	Mobil Marine	Multi-purpose grease	10	100
MAT-007	Paint Red Oxide	Paints	25	liters	25.0	PNT-RO	8901234567896	International Paint	Anti-corrosive paint	10	100
MAT-008	Paint White Enamel	Paints	20	liters	28.0	PNT-WE	8901234567897	International Paint	Enamel finish	10	100
MAT-009	Safety Gloves	Safety Equipment	100	pairs	3.5	GLV-SF	8901234567898	3M Safety	Cut-resistant gloves	50	500
MAT-010	Safety Goggles	Safety Equipment	50	pcs	8.0	GOG-SF	8901234567899	3M Safety	Impact-resistant goggles	25	250
MAT-011	Wire Rope 12mm	Rigging	100	meters	5.5	WR-12	8901234567900	Bridon Marine	Stainless steel wire	50	500
MAT-012	Shackle 10 Ton	Rigging	20	pcs	45.0	SHK-10T	8901234567901	Crosby Marine	Bow shackle	10	100
MAT-013	Fire Extinguisher 6kg	Safety Equipment	10	pcs	85.0	FE-6KG	8901234567902	Kidde Marine	CO2 extinguisher	5	50
MAT-014	Life Jacket Adult	Safety Equipment	50	pcs	35.0	LJ-ADT	8901234567903	Viking Marine	SOLAS approved	25	200
MAT-015	First Aid Kit	Safety Equipment	5	pcs	150.0	FAK-001	8901234567904	Medic Marine	Complete marine kit	3	20
```

## Format các cột:

### Bắt buộc:
- **ItemCode**: Mã vật tư duy nhất (VD: MAT-001)
- **ItemName**: Tên vật tư (VD: Bolt M10 x 50mm)
- **Quantity**: Số lượng nhập (VD: 100)
- **Unit**: Đơn vị tính (VD: pcs, liters, kg, meters)

### Tùy chọn:
- **Category**: Danh mục (VD: Fasteners, Lubricants, Paints)
- **UnitCost**: Đơn giá (VD: 0.5, 15.5)
- **PartNumber**: Mã linh kiện (VD: BLT-M10-50)
- **Barcode**: Mã vạch (VD: 8901234567890)
- **Manufacturer**: Nhà sản xuất (VD: ABC Fasteners Co.)
- **Specification**: Thông số kỹ thuật (VD: High tensile steel bolt)
- **MinStock**: Tồn kho tối thiểu (VD: 50)
- **MaxStock**: Tồn kho tối đa (VD: 500)

## Lưu ý:

1. **Header row** (dòng đầu tiên) phải có tên cột
2. Hệ thống hỗ trợ tên cột cả **tiếng Việt** và **tiếng Anh**:
   - ItemCode / Mã vật tư
   - ItemName / Tên vật tư
   - Category / Danh mục
   - Quantity / Số lượng
   - Unit / Đơn vị
   - UnitCost / Đơn giá

3. Nếu **ItemCode đã tồn tại** trong hệ thống:
   - ✅ Số lượng sẽ được **cộng thêm** vào tồn kho hiện tại
   - ✅ Đơn giá sẽ được **cập nhật** theo giá mới

4. Nếu **ItemCode chưa tồn tại**:
   - ✅ Vật tư mới sẽ được **tạo** với số lượng ban đầu

## Kết quả mẫu:

Khi import file trên, hệ thống sẽ:
- Tạo 15 items mới
- Tổng giá trị phiếu nhập: ~$7,000 USD
- Tự động phân loại theo Category
- Lưu đầy đủ thông tin kỹ thuật và metadata
