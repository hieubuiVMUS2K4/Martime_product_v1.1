-- ================================================
-- Migration V4: Material Receipts System
-- Description: Tạo hệ thống quản lý phiếu nhập kho
-- Date: 2024-12-04
-- ================================================

-- 1. Tạo bảng MaterialReceipts (Phiếu nhập kho)
CREATE TABLE IF NOT EXISTS "MaterialReceipts" (
    "Id" SERIAL PRIMARY KEY,
    "ReceiptCode" VARCHAR(50) NOT NULL UNIQUE,
    "ReceiptDate" TIMESTAMP NOT NULL,
    "SupplierId" INT,
    "SupplierName" VARCHAR(200),
    "WarehouseLocation" VARCHAR(200),
    "TotalAmount" DECIMAL(18,2),
    "Currency" VARCHAR(10) DEFAULT 'USD',
    "Status" VARCHAR(50) DEFAULT 'Draft',
    "Notes" TEXT,
    "CreatedBy" VARCHAR(100),
    "ApprovedBy" VARCHAR(100),
    "ApprovedDate" TIMESTAMP,
    "ImportSource" VARCHAR(50),
    "ImportFileName" VARCHAR(255),
    "IsActive" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tạo bảng MaterialReceiptItems (Chi tiết phiếu nhập)
CREATE TABLE IF NOT EXISTS "MaterialReceiptItems" (
    "Id" SERIAL PRIMARY KEY,
    "ReceiptId" INT NOT NULL,
    "MaterialItemId" UUID NOT NULL,
    "Quantity" DECIMAL(18,3) NOT NULL,
    "UnitCost" DECIMAL(18,2),
    "TotalCost" DECIMAL(18,2),
    "Currency" VARCHAR(10) DEFAULT 'USD',
    "LineNumber" INT,
    "Notes" TEXT,
    "BatchNumber" VARCHAR(100),
    "ExpiryDate" DATE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Thêm Foreign Keys sau khi đã tạo bảng
DO $$
BEGIN
    -- Thêm FK cho ReceiptId
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_MaterialReceiptItems_Receipt'
    ) THEN
        ALTER TABLE "MaterialReceiptItems"
            ADD CONSTRAINT "FK_MaterialReceiptItems_Receipt" 
            FOREIGN KEY ("ReceiptId") 
            REFERENCES "MaterialReceipts"("Id") 
            ON DELETE CASCADE;
    END IF;
    
    -- Thêm FK cho MaterialItemId (chỉ nếu table MaterialItems tồn tại)
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'MaterialItems') THEN
        IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'FK_MaterialReceiptItems_MaterialItem'
        ) THEN
            ALTER TABLE "MaterialReceiptItems"
                ADD CONSTRAINT "FK_MaterialReceiptItems_MaterialItem" 
                FOREIGN KEY ("MaterialItemId") 
                REFERENCES "MaterialItems"("Id") 
                ON DELETE RESTRICT;
        END IF;
    END IF;
END $$;

-- 4. Tạo Indexes cho performance
-- MaterialReceipts indexes
CREATE INDEX IF NOT EXISTS "idx_receipts_code" ON "MaterialReceipts"("ReceiptCode");
CREATE INDEX IF NOT EXISTS "idx_receipts_date" ON "MaterialReceipts"("ReceiptDate" DESC);
CREATE INDEX IF NOT EXISTS "idx_receipts_status" ON "MaterialReceipts"("Status");
CREATE INDEX IF NOT EXISTS "idx_receipts_created" ON "MaterialReceipts"("CreatedAt" DESC);
CREATE INDEX IF NOT EXISTS "idx_receipts_supplier" ON "MaterialReceipts"("SupplierName");

-- MaterialReceiptItems indexes
CREATE INDEX IF NOT EXISTS "idx_receipt_items_receipt" ON "MaterialReceiptItems"("ReceiptId");
CREATE INDEX IF NOT EXISTS "idx_receipt_items_material" ON "MaterialReceiptItems"("MaterialItemId");
CREATE INDEX IF NOT EXISTS "idx_receipt_items_receipt_material" ON "MaterialReceiptItems"("ReceiptId", "MaterialItemId");
CREATE INDEX IF NOT EXISTS "idx_receipt_items_batch" ON "MaterialReceiptItems"("BatchNumber") WHERE "BatchNumber" IS NOT NULL;
CREATE INDEX IF NOT EXISTS "idx_receipt_items_expiry" ON "MaterialReceiptItems"("ExpiryDate") WHERE "ExpiryDate" IS NOT NULL;

-- 4. Tạo trigger để tự động cập nhật UpdatedAt
CREATE OR REPLACE FUNCTION update_material_receipts_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_material_receipts_updated_at
    BEFORE UPDATE ON "MaterialReceipts"
    FOR EACH ROW
    EXECUTE FUNCTION update_material_receipts_updated_at();

CREATE TRIGGER trigger_material_receipt_items_updated_at
    BEFORE UPDATE ON "MaterialReceiptItems"
    FOR EACH ROW
    EXECUTE FUNCTION update_material_receipts_updated_at();

-- 5. Tạo function để generate ReceiptCode tự động
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
    SELECT COALESCE(MAX(CAST(SUBSTRING("ReceiptCode" FROM 14) AS INT)), 0) + 1
    INTO sequence_num
    FROM "MaterialReceipts"
    WHERE "ReceiptCode" LIKE 'PN-' || date_part || '-%';
    
    new_code := 'PN-' || date_part || '-' || LPAD(sequence_num::TEXT, 3, '0');
    
    RETURN new_code;
END;
$$ LANGUAGE plpgsql;

-- 6. Comment mô tả các bảng
COMMENT ON TABLE "MaterialReceipts" IS 'Quản lý phiếu nhập kho vật tư';
COMMENT ON TABLE "MaterialReceiptItems" IS 'Chi tiết các vật tư trong mỗi phiếu nhập';

COMMENT ON COLUMN "MaterialReceipts"."ReceiptCode" IS 'Mã phiếu nhập duy nhất (PN-YYYYMMDD-XXX)';
COMMENT ON COLUMN "MaterialReceipts"."Status" IS 'Trạng thái: Draft, Approved, Completed, Cancelled';
COMMENT ON COLUMN "MaterialReceipts"."ImportSource" IS 'Nguồn: Manual, Excel, API';

COMMENT ON COLUMN "MaterialReceiptItems"."Quantity" IS 'Số lượng nhập trong lần này';
COMMENT ON COLUMN "MaterialReceiptItems"."UnitCost" IS 'Đơn giá tại thời điểm nhập (snapshot)';
COMMENT ON COLUMN "MaterialReceiptItems"."BatchNumber" IS 'Số lô hàng (nếu có)';
COMMENT ON COLUMN "MaterialReceiptItems"."ExpiryDate" IS 'Hạn sử dụng (nếu có)';

-- 7. Insert sample data (optional - for testing)
-- Uncomment để test
/*
-- Sample receipt
INSERT INTO "MaterialReceipts" (
    "ReceiptCode", "ReceiptDate", "SupplierName", "WarehouseLocation", 
    "Status", "ImportSource", "CreatedBy"
) VALUES (
    generate_receipt_code(), 
    CURRENT_TIMESTAMP, 
    'ABC Marine Supplies Co.', 
    'Main Warehouse',
    'Completed', 
    'Manual',
    'admin'
);

-- Sample receipt items (assuming MaterialItems with Id 1, 2, 3 exist)
INSERT INTO "MaterialReceiptItems" (
    "ReceiptId", "MaterialItemId", "Quantity", "UnitCost", "TotalCost", "LineNumber"
) VALUES 
    (1, 1, 100.000, 5.50, 550.00, 1),
    (1, 2, 50.000, 12.00, 600.00, 2),
    (1, 3, 200.000, 0.75, 150.00, 3);

-- Update receipt total amount
UPDATE "MaterialReceipts" 
SET "TotalAmount" = (
    SELECT SUM("TotalCost") 
    FROM "MaterialReceiptItems" 
    WHERE "ReceiptId" = 1
)
WHERE "Id" = 1;
*/

-- 8. Verification queries
-- Check if tables created successfully
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'MaterialReceipts') THEN
        RAISE NOTICE 'Table MaterialReceipts created successfully';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'MaterialReceiptItems') THEN
        RAISE NOTICE 'Table MaterialReceiptItems created successfully';
    END IF;
END $$;

-- Migration completed
SELECT 'Migration V4: Material Receipts System - Completed Successfully!' AS status;
