-- ================================================
-- Migration V4: Material Receipts System (snake_case)
-- Description: Tạo hệ thống quản lý phiếu nhập kho với snake_case naming
-- Date: 2024-12-09
-- ================================================

-- 1. Tạo bảng material_receipts (Phiếu nhập kho)
CREATE TABLE IF NOT EXISTS material_receipts (
    id SERIAL PRIMARY KEY,
    receipt_code VARCHAR(50) NOT NULL UNIQUE,
    receipt_date TIMESTAMP NOT NULL,
    total_amount DECIMAL(18,2),
    currency VARCHAR(10) DEFAULT 'USD',
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

-- 2. Tạo bảng material_receipt_items (Chi tiết phiếu nhập)
CREATE TABLE IF NOT EXISTS material_receipt_items (
    id SERIAL PRIMARY KEY,
    receipt_id INT NOT NULL,
    material_item_id UUID NOT NULL,
    quantity DECIMAL(18,3) NOT NULL,
    unit_cost DECIMAL(18,2),
    total_cost DECIMAL(18,2),
    currency VARCHAR(10) DEFAULT 'USD',
    line_number INT,
    notes TEXT,
    batch_number VARCHAR(100),
    expiry_date DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Thêm Foreign Keys
ALTER TABLE material_receipt_items
    ADD CONSTRAINT fk_material_receipt_items_receipt 
    FOREIGN KEY (receipt_id) 
    REFERENCES material_receipts(id) 
    ON DELETE CASCADE;

ALTER TABLE material_receipt_items
    ADD CONSTRAINT fk_material_receipt_items_material_item 
    FOREIGN KEY (material_item_id) 
    REFERENCES material_items(id) 
    ON DELETE RESTRICT;

-- 4. Tạo Indexes cho performance
CREATE INDEX idx_receipts_code ON material_receipts(receipt_code);
CREATE INDEX idx_receipts_date ON material_receipts(receipt_date DESC);
CREATE INDEX idx_receipts_status ON material_receipts(status);
CREATE INDEX idx_receipts_created ON material_receipts(created_at DESC);

CREATE INDEX idx_receipt_items_receipt ON material_receipt_items(receipt_id);
CREATE INDEX idx_receipt_items_material ON material_receipt_items(material_item_id);
CREATE INDEX idx_receipt_items_receipt_material ON material_receipt_items(receipt_id, material_item_id);
CREATE INDEX idx_receipt_items_batch ON material_receipt_items(batch_number) WHERE batch_number IS NOT NULL;
CREATE INDEX idx_receipt_items_expiry ON material_receipt_items(expiry_date) WHERE expiry_date IS NOT NULL;

-- 5. Tạo trigger để tự động cập nhật updated_at
CREATE OR REPLACE FUNCTION update_material_receipts_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_material_receipts_updated_at
    BEFORE UPDATE ON material_receipts
    FOR EACH ROW
    EXECUTE FUNCTION update_material_receipts_updated_at();

CREATE TRIGGER trigger_material_receipt_items_updated_at
    BEFORE UPDATE ON material_receipt_items
    FOR EACH ROW
    EXECUTE FUNCTION update_material_receipts_updated_at();

-- 6. Tạo function để generate receipt_code tự động
CREATE OR REPLACE FUNCTION generate_receipt_code()
RETURNS VARCHAR(50) AS $$
DECLARE
    new_code VARCHAR(50);
    date_part VARCHAR(8);
    sequence_num INT;
BEGIN
    -- Format: PN-YYYYMMDD-XXX
    date_part := TO_CHAR(CURRENT_DATE, 'YYYYMMDD');
    
    -- Lấy số sequence tiếp theo trong ngày
    SELECT COALESCE(MAX(CAST(SUBSTRING(receipt_code FROM 14) AS INT)), 0) + 1
    INTO sequence_num
    FROM material_receipts
    WHERE receipt_code LIKE 'PN-' || date_part || '-%';
    
    new_code := 'PN-' || date_part || '-' || LPAD(sequence_num::TEXT, 3, '0');
    
    RETURN new_code;
END;
$$ LANGUAGE plpgsql;

-- 7. Comment mô tả các bảng
COMMENT ON TABLE material_receipts IS 'Quản lý phiếu nhập kho vật tư';
COMMENT ON TABLE material_receipt_items IS 'Chi tiết các vật tư trong mỗi phiếu nhập';

COMMENT ON COLUMN material_receipts.receipt_code IS 'Mã phiếu nhập duy nhất (PN-YYYYMMDD-XXX)';
COMMENT ON COLUMN material_receipts.status IS 'Trạng thái: Draft, Approved, Completed, Cancelled';
COMMENT ON COLUMN material_receipts.import_source IS 'Nguồn: Manual, Excel, API';

COMMENT ON COLUMN material_receipt_items.quantity IS 'Số lượng nhập trong lần này';
COMMENT ON COLUMN material_receipt_items.unit_cost IS 'Đơn giá tại thời điểm nhập (snapshot)';
COMMENT ON COLUMN material_receipt_items.batch_number IS 'Số lô hàng (nếu có)';
COMMENT ON COLUMN material_receipt_items.expiry_date IS 'Hạn sử dụng (nếu có)';

-- Verification
SELECT 'Migration V4 (snake_case): Material Receipts System - Completed Successfully!' AS status;
