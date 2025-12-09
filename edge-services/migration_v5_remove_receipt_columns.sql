-- ================================================
-- Migration V5: Remove Unused Columns from MaterialReceipts
-- Description: Xóa các cột SupplierId, SupplierName, WarehouseLocation, ApprovedBy
-- Date: 2024-12-04
-- ================================================

-- Xóa các cột không cần thiết
ALTER TABLE "MaterialReceipts" 
  DROP COLUMN IF EXISTS "SupplierId",
  DROP COLUMN IF EXISTS "SupplierName",
  DROP COLUMN IF EXISTS "WarehouseLocation",
  DROP COLUMN IF EXISTS "ApprovedBy";

-- Log kết quả
DO $$
BEGIN
    RAISE NOTICE 'Migration V5 completed: Removed SupplierId, SupplierName, WarehouseLocation, ApprovedBy from MaterialReceipts';
END $$;
