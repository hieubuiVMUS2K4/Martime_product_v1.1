-- ================================================
-- Migration V8: Equipment Receipts System
-- Description: Tạo hệ thống quản lý phiếu nhập thiết bị
-- Date: 2024-12-04
-- Note: Equipment receipts KHÔNG quản lý giá trị tài chính,
--       chỉ tracking việc nhập thiết bị vào kho
-- ================================================

-- 1. Tạo bảng equipment_receipts (Phiếu nhập thiết bị)
CREATE TABLE IF NOT EXISTS equipment_receipts (
    id SERIAL PRIMARY KEY,
    receipt_code VARCHAR(50) NOT NULL UNIQUE,
    receipt_date TIMESTAMP NOT NULL,
    status VARCHAR(50) DEFAULT 'Draft',
    notes TEXT,
    created_by VARCHAR(100),
    approved_date TIMESTAMP,
    import_source VARCHAR(50),
    import_file_name VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tạo bảng equipment_receipt_items (Chi tiết thiết bị trong phiếu)
CREATE TABLE IF NOT EXISTS equipment_receipt_items (
    id SERIAL PRIMARY KEY,
    receipt_id INT NOT NULL,
    equipment_item_id UUID NOT NULL,
    line_number INT,
    
    -- Snapshot data (chụp lại thông tin tại thời điểm nhập)
    equipment_code VARCHAR(50),
    equipment_name VARCHAR(200),
    category_name VARCHAR(200),
    
    -- Thông tin nhập kho
    quantity DOUBLE PRECISION NOT NULL,
    location VARCHAR(100),
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    solas_reference VARCHAR(200),
    specification TEXT,
    description TEXT,
    
    -- Metadata
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Thêm Foreign Keys
ALTER TABLE equipment_receipt_items
    ADD CONSTRAINT fk_equipment_receipt_items_receipt 
    FOREIGN KEY (receipt_id) 
    REFERENCES equipment_receipts(id) 
    ON DELETE CASCADE;

ALTER TABLE equipment_receipt_items
    ADD CONSTRAINT fk_equipment_receipt_items_equipment 
    FOREIGN KEY (equipment_item_id) 
    REFERENCES equipment_items(id) 
    ON DELETE RESTRICT;

-- 4. Tạo Indexes cho performance
-- equipment_receipts indexes
CREATE INDEX IF NOT EXISTS idx_equipment_receipts_code ON equipment_receipts(receipt_code);
CREATE INDEX IF NOT EXISTS idx_equipment_receipts_date ON equipment_receipts(receipt_date DESC);
CREATE INDEX IF NOT EXISTS idx_equipment_receipts_status ON equipment_receipts(status);
CREATE INDEX IF NOT EXISTS idx_equipment_receipts_created ON equipment_receipts(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_equipment_receipts_created_by ON equipment_receipts(created_by);

-- equipment_receipt_items indexes
CREATE INDEX IF NOT EXISTS idx_equipment_receipt_items_receipt ON equipment_receipt_items(receipt_id);
CREATE INDEX IF NOT EXISTS idx_equipment_receipt_items_equipment ON equipment_receipt_items(equipment_item_id);
CREATE INDEX IF NOT EXISTS idx_equipment_receipt_items_receipt_equipment ON equipment_receipt_items(receipt_id, equipment_item_id);
CREATE INDEX IF NOT EXISTS idx_equipment_receipt_items_serial ON equipment_receipt_items(serial_number) WHERE serial_number IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_equipment_receipt_items_code ON equipment_receipt_items(equipment_code);

-- 5. Tạo trigger để tự động cập nhật updated_at
CREATE OR REPLACE FUNCTION update_equipment_receipts_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_equipment_receipts_updated_at
    BEFORE UPDATE ON equipment_receipts
    FOR EACH ROW
    EXECUTE FUNCTION update_equipment_receipts_updated_at();

CREATE TRIGGER trigger_equipment_receipt_items_updated_at
    BEFORE UPDATE ON equipment_receipt_items
    FOR EACH ROW
    EXECUTE FUNCTION update_equipment_receipts_updated_at();

-- 6. Tạo function để generate ReceiptCode tự động
CREATE OR REPLACE FUNCTION generate_equipment_receipt_code()
RETURNS VARCHAR(50) AS $$
DECLARE
    new_code VARCHAR(50);
    date_part VARCHAR(8);
    sequence_num INT;
BEGIN
    -- Format: EQ-YYYYMMDD-XXX
    date_part := TO_CHAR(CURRENT_DATE, 'YYYYMMDD');
    
    -- Lấy số sequence tiếp theo trong ngày
    SELECT COALESCE(MAX(CAST(SUBSTRING(receipt_code FROM 14) AS INT)), 0) + 1
    INTO sequence_num
    FROM equipment_receipts
    WHERE receipt_code LIKE 'EQ-' || date_part || '-%';
    
    new_code := 'EQ-' || date_part || '-' || LPAD(sequence_num::TEXT, 3, '0');
    
    RETURN new_code;
END;
$$ LANGUAGE plpgsql;

-- 7. Comment mô tả các bảng
COMMENT ON TABLE equipment_receipts IS 'Quản lý phiếu nhập thiết bị - KHÔNG quản lý giá trị tài chính';
COMMENT ON TABLE equipment_receipt_items IS 'Chi tiết các thiết bị trong mỗi phiếu nhập';

COMMENT ON COLUMN equipment_receipts.receipt_code IS 'Mã phiếu nhập duy nhất (EQ-YYYYMMDD-XXX)';
COMMENT ON COLUMN equipment_receipts.status IS 'Trạng thái: Draft, Approved, Completed, Cancelled';
COMMENT ON COLUMN equipment_receipts.import_source IS 'Nguồn: Manual, Excel, API';

COMMENT ON COLUMN equipment_receipt_items.quantity IS 'Số lượng thiết bị nhập trong lần này (thêm vào kho)';
COMMENT ON COLUMN equipment_receipt_items.equipment_code IS 'Snapshot: Mã thiết bị tại thời điểm nhập';
COMMENT ON COLUMN equipment_receipt_items.equipment_name IS 'Snapshot: Tên thiết bị tại thời điểm nhập';
COMMENT ON COLUMN equipment_receipt_items.category_name IS 'Snapshot: Tên danh mục tại thời điểm nhập';
COMMENT ON COLUMN equipment_receipt_items.serial_number IS 'Serial number của thiết bị cụ thể (nếu có)';
COMMENT ON COLUMN equipment_receipt_items.solas_reference IS 'Tham chiếu SOLAS cho thiết bị an toàn';

-- 8. Insert sample data (for testing)
-- Sample receipt
INSERT INTO equipment_receipts (
    receipt_code, receipt_date, status, notes, created_by, import_source
) VALUES (
    'EQ-20241204-001',
    CURRENT_TIMESTAMP,
    'Draft',
    'Nhập thiết bị an toàn SOLAS mới',
    'admin',
    'Manual'
);

-- Sample receipt items
INSERT INTO equipment_receipt_items (
    receipt_id, equipment_item_id, line_number,
    equipment_code, equipment_name, category_name,
    quantity, location, manufacturer, model, serial_number, solas_reference,
    notes
) VALUES (
    1,
    (SELECT id FROM equipment_items LIMIT 1),
    1,
    'FIRE-EXT-001',
    'Fire Extinguisher CO2 5kg',
    'Fire Safety Equipment',
    5.0,
    'Main Deck - Port Side',
    'Kidde',
    'ProPlus 5',
    'KDE-2024-12345',
    'SOLAS Chapter II-2, Regulation 10',
    'Thay thế bình cũ đã hết hạn kiểm định'
);

-- 9. Verification query
SELECT 
    'equipment_receipts' as table_name,
    COUNT(*) as record_count
FROM equipment_receipts
UNION ALL
SELECT 
    'equipment_receipt_items',
    COUNT(*)
FROM equipment_receipt_items;

COMMENT ON DATABASE maritime_edge IS 'Maritime Edge Database - Updated with Equipment Receipts System (V8)';
