-- ================================================
-- Migration V7: Fix material_item_id Type Mismatch
-- Description: Change material_item_id from INTEGER to UUID to match material_items.id
-- Date: 2024-12-04
-- ================================================

-- 1. Drop foreign key constraint if exists
ALTER TABLE material_receipt_items 
    DROP CONSTRAINT IF EXISTS "FK_MaterialReceiptItems_MaterialItem";

-- 2. Drop existing indexes on material_item_id
DROP INDEX IF EXISTS idx_receipt_items_material;
DROP INDEX IF EXISTS idx_receipt_items_receipt_material;

-- 3. Change column type from INTEGER to UUID
ALTER TABLE material_receipt_items 
    ALTER COLUMN material_item_id TYPE UUID USING material_item_id::text::uuid;

-- 4. Re-create indexes
CREATE INDEX idx_receipt_items_material ON material_receipt_items(material_item_id);
CREATE INDEX idx_receipt_items_receipt_material ON material_receipt_items(receipt_id, material_item_id);

-- 5. Add foreign key constraint
ALTER TABLE material_receipt_items 
    ADD CONSTRAINT fk_material_receipt_items_material_item 
    FOREIGN KEY (material_item_id) 
    REFERENCES material_items(id) 
    ON DELETE RESTRICT;

-- Verify changes
SELECT 'Migration V7 completed successfully!' AS status;
