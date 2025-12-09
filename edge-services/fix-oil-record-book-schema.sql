-- Fix missing columns in oil_record_books table

ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS location_lat DECIMAL(10,7);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS location_lon DECIMAL(10,7);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS quantity DECIMAL(10,3);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS quantity_unit VARCHAR(20) DEFAULT 'm³';
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS tank_from VARCHAR(50);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS tank_to VARCHAR(50);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS officer_in_charge VARCHAR(100);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS master_signature VARCHAR(200);
ALTER TABLE oil_record_books ADD COLUMN IF NOT EXISTS remarks TEXT;
