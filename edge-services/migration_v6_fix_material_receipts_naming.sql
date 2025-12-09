-- ================================================
-- Migration V6: Fix Material Receipts Naming Convention
-- Description: Convert all column names to snake_case to match EF Core convention
-- Date: 2024-12-04
-- ================================================

-- Fix material_receipts columns
ALTER TABLE material_receipts 
    RENAME COLUMN "Id" TO id;

ALTER TABLE material_receipts 
    RENAME COLUMN "ReceiptCode" TO receipt_code;

ALTER TABLE material_receipts 
    RENAME COLUMN "ReceiptDate" TO receipt_date;

ALTER TABLE material_receipts 
    RENAME COLUMN "TotalAmount" TO total_amount;

ALTER TABLE material_receipts 
    RENAME COLUMN "Currency" TO currency;

ALTER TABLE material_receipts 
    RENAME COLUMN "Status" TO status;

ALTER TABLE material_receipts 
    RENAME COLUMN "Notes" TO notes;

ALTER TABLE material_receipts 
    RENAME COLUMN "CreatedBy" TO created_by;

ALTER TABLE material_receipts 
    RENAME COLUMN "ApprovedDate" TO approved_date;

ALTER TABLE material_receipts 
    RENAME COLUMN "ImportSource" TO import_source;

ALTER TABLE material_receipts 
    RENAME COLUMN "ImportFileName" TO import_file_name;

ALTER TABLE material_receipts 
    RENAME COLUMN "IsActive" TO is_active;

ALTER TABLE material_receipts 
    RENAME COLUMN "CreatedAt" TO created_at;

ALTER TABLE material_receipts 
    RENAME COLUMN "UpdatedAt" TO updated_at;

-- Fix material_receipt_items columns
ALTER TABLE material_receipt_items 
    RENAME COLUMN "Id" TO id;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "ReceiptId" TO receipt_id;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "MaterialItemId" TO material_item_id;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "Quantity" TO quantity;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "UnitCost" TO unit_cost;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "TotalCost" TO total_cost;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "Currency" TO currency;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "LineNumber" TO line_number;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "Notes" TO notes;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "BatchNumber" TO batch_number;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "ExpiryDate" TO expiry_date;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "CreatedAt" TO created_at;

ALTER TABLE material_receipt_items 
    RENAME COLUMN "UpdatedAt" TO updated_at;

-- Verify changes
SELECT 'Migration V6 completed successfully!' AS status;
