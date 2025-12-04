# Equipment Import Template

## Required Excel Columns

To import equipment using the Excel import feature, create an Excel file (.xlsx or .xls) with the following columns:

### Required Columns:
- **EquipmentCode** (or Equipment Code, Mã thiết bị, Ma thiet bi, Code): Unique code for the equipment
- **EquipmentName** (or Equipment Name, Tên thiết bị, Ten thiet bi, Name): Name of the equipment
- **Quantity** (or Số lượng, So luong, Qty): Quantity (must be > 0)

### Optional Columns:
- **Category** (or CategoryName, Danh mục, Danh muc): Category name
- **Location** (or Vị trí, Vi tri, Position): Storage location
- **Manufacturer** (or Nhà sản xuất, Nha san xuat, Brand, Maker): Manufacturer name
- **Model** (or Mô hình, Mo hinh): Model number/name
- **SerialNumber** (or Serial Number, Serial, Số serial, So serial, SN): Serial number
- **SolasReference** (or SOLAS Reference, SOLAS, Tham chiếu SOLAS): SOLAS regulation reference
- **Specification** (or Spec, Thông số, Thong so, Specs): Technical specifications
- **Description** (or Mô tả, Mo ta, Desc): Description
- **Notes** (or Ghi chú, Ghi chu, Note): Additional notes

## Sample Data

| EquipmentCode | EquipmentName | Category | Quantity | Location | Manufacturer | Model | SerialNumber |
|--------------|---------------|----------|----------|----------|--------------|-------|--------------|
| EQ-NAV-001 | GPS System | Navigation | 2 | Bridge | Furuno | GP-170 | GPS12345 |
| EQ-COM-002 | VHF Radio | Communication | 3 | Bridge | Icom | IC-M605 | VHF98765 |
| EQ-SAF-003 | Life Jacket | Safety Equipment | 50 | Deck Storage | Viking | Standard | - |
| EQ-ENG-004 | Diesel Generator | Engine | 1 | Engine Room | Caterpillar | C18 | GEN54321 |

## Import Behavior

- **If equipment_code exists**: The system will **UPDATE** the existing equipment by adding the imported quantity
  - Example: If EQ-NAV-001 has 2 units, and you import 3 more, the total becomes 5 units
  
- **If equipment_code doesn't exist**: The system will **CREATE** a new equipment record
  - Example: Importing EQ-NEW-005 for the first time will create a new equipment item

## How to Use

1. Create an Excel file with the columns above
2. Fill in your equipment data (at least EquipmentCode, EquipmentName, Quantity)
3. In the Equipment Management page, click **Import Excel** button
4. Select your Excel file
5. Click **Preview** to see what will be created/updated
6. Review the preview carefully:
   - Green rows = New equipment will be created
   - Yellow rows = Existing equipment will be updated
   - Red rows = Errors (must be fixed before import)
7. Click **Confirm & Import** to execute the import

## Tips

- Use consistent equipment codes (e.g., EQ-NAV-001, EQ-COM-002, etc.)
- Fill in as much detail as possible for better tracking
- Serial numbers help identify specific units
- SOLAS references are important for safety equipment
- The system is case-insensitive and supports both English and Vietnamese column names
