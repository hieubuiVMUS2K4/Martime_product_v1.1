-- Migration Script: Update Equipment Tables Schema
-- This script updates equipment tables to match the C# models

-- =================================================================
-- STEP 1: Update equipment_categories table
-- =================================================================

-- Change id from BIGSERIAL to INTEGER
ALTER TABLE public.equipment_categories ALTER COLUMN id TYPE INTEGER;

-- =================================================================
-- STEP 2: Update equipment_items table structure
-- =================================================================

-- First, drop the table and recreate it with correct schema
-- Save existing data first if needed
DROP TABLE IF EXISTS public.equipment_items CASCADE;

-- Recreate equipment_items with correct schema
CREATE TABLE public.equipment_items (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL,
    equipment_code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    manufacturer VARCHAR(200),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    purchase_date TIMESTAMP WITH TIME ZONE,
    purchase_price DECIMAL(18,2),
    location VARCHAR(200),
    status VARCHAR(50) NOT NULL DEFAULT 'operational',
    last_maintenance_date TIMESTAMP WITH TIME ZONE,
    next_maintenance_date TIMESTAMP WITH TIME ZONE,
    maintenance_schedule VARCHAR(200),
    warranty_expiry_date TIMESTAMP WITH TIME ZONE,
    certification_details VARCHAR(500),
    is_critical_equipment BOOLEAN NOT NULL DEFAULT false,
    requires_solas_compliance BOOLEAN NOT NULL DEFAULT false,
    last_inspection_date TIMESTAMP WITH TIME ZONE,
    next_inspection_date TIMESTAMP WITH TIME ZONE,
    inspection_authority VARCHAR(100),
    notes VARCHAR(1000),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_equipment_items_equipment_categories_category_id 
        FOREIGN KEY (category_id) 
        REFERENCES public.equipment_categories(id) 
        ON DELETE RESTRICT
);

-- Create indexes
CREATE INDEX ix_equipment_items_equipment_code ON public.equipment_items(equipment_code);
CREATE INDEX ix_equipment_items_category_id ON public.equipment_items(category_id);
CREATE INDEX ix_equipment_items_status ON public.equipment_items(status);
CREATE INDEX ix_equipment_items_is_active ON public.equipment_items(is_active);
CREATE INDEX ix_equipment_items_is_critical ON public.equipment_items(is_critical_equipment);
CREATE INDEX ix_equipment_items_next_maintenance ON public.equipment_items(next_maintenance_date);

COMMENT ON TABLE public.equipment_items IS 'Individual equipment items tracked for maintenance and compliance';

-- =================================================================
-- STEP 3: Insert sample data
-- =================================================================

-- Insert sample equipment items with new schema
INSERT INTO public.equipment_items (
    equipment_code, 
    name, 
    category_id, 
    location, 
    status,
    description,
    manufacturer,
    model,
    serial_number,
    purchase_date,
    purchase_price,
    is_critical_equipment,
    requires_solas_compliance,
    last_maintenance_date,
    next_maintenance_date,
    maintenance_schedule,
    warranty_expiry_date,
    certification_details,
    last_inspection_date,
    next_inspection_date,
    inspection_authority,
    notes,
    is_active
) VALUES
(
    'FSS.01', 
    'Portable Fire Extinguisher 5kg CO2', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'FIRE' LIMIT 1), 
    'Engine Room', 
    'operational',
    '5kg CO2 fire extinguisher for electrical fires',
    'Tyco',
    'CO2-5KG-STD',
    'TYC-2023-001234',
    '2023-01-15',
    450.00,
    true,
    true,
    '2024-10-01',
    '2025-04-01',
    'Every 6 months',
    '2028-01-15',
    'SOLAS Ch.II-2, Reg.10',
    '2024-10-01',
    '2025-04-01',
    'DNV GL',
    'Located near electrical panel',
    true
),
(
    'FSS.02', 
    'Portable Fire Extinguisher 9kg Powder', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'FIRE' LIMIT 1), 
    'Main Deck', 
    'operational',
    '9kg ABC Powder for general fires',
    'Kidde',
    'ABC-9KG-PRO',
    'KID-2023-005678',
    '2023-02-20',
    380.00,
    true,
    true,
    '2024-09-15',
    '2025-03-15',
    'Every 6 months',
    '2028-02-20',
    'SOLAS Ch.II-2, Reg.10',
    '2024-09-15',
    '2025-03-15',
    'Lloyd''s Register',
    'Main deck forward section',
    true
),
(
    'SE.01', 
    'Life Jacket Adult SOLAS', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'SAFETY' LIMIT 1), 
    'Bridge', 
    'operational',
    'Adult life jacket 150N buoyancy',
    'Viking',
    'LJ-150N-ADT',
    'VIK-2022-112233',
    '2022-06-10',
    85.00,
    true,
    true,
    '2024-11-01',
    '2025-05-01',
    'Annual inspection',
    '2027-06-10',
    'SOLAS Ch.III, Reg.7.2',
    '2024-11-01',
    '2025-05-01',
    'Bureau Veritas',
    '50 units available',
    true
),
(
    'LB.01', 
    'Port Side Totally Enclosed Lifeboat', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'LIFEBOAT' LIMIT 1), 
    'Port Side Main Deck', 
    'operational',
    'Totally enclosed motor propelled lifeboat',
    'Norsafe',
    'GES-65P-TE',
    'NOR-2020-445566',
    '2020-03-15',
    125000.00,
    true,
    true,
    '2024-08-20',
    '2025-02-20',
    'Every 6 months',
    '2030-03-15',
    'SOLAS Ch.III, Reg.31',
    '2024-08-20',
    '2025-02-20',
    'ABS',
    'Capacity: 65 persons. Regular load test required',
    true
),
(
    'LB.02', 
    'Starboard Side Totally Enclosed Lifeboat', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'LIFEBOAT' LIMIT 1), 
    'Starboard Side Main Deck', 
    'operational',
    'Totally enclosed motor propelled lifeboat',
    'Norsafe',
    'GES-65S-TE',
    'NOR-2020-445567',
    '2020-03-15',
    125000.00,
    true,
    true,
    '2024-08-20',
    '2025-02-20',
    'Every 6 months',
    '2030-03-15',
    'SOLAS Ch.III, Reg.31',
    '2024-08-20',
    '2025-02-20',
    'ABS',
    'Capacity: 65 persons. Regular load test required',
    true
),
(
    'SCBA.01', 
    'Self-Contained Breathing Apparatus Set', 
    (SELECT id FROM public.equipment_categories WHERE category_code = 'SCBA' LIMIT 1), 
    'Fire Station', 
    'operational',
    'SCBA with 6L cylinder, 300 bar pressure',
    'Dräger',
    'PSS-BG-4PLUS',
    'DRA-2021-778899',
    '2021-05-20',
    3500.00,
    true,
    true,
    '2024-10-10',
    '2025-04-10',
    'Every 6 months + annual cylinder test',
    '2026-05-20',
    'SOLAS Ch.II-2, Reg.10.10',
    '2024-10-10',
    '2025-04-10',
    'DNV GL',
    '4 sets available. Cylinder test due annually',
    true
)
ON CONFLICT (equipment_code) DO NOTHING;

-- =================================================================
-- VERIFICATION QUERIES
-- =================================================================

-- Check equipment_categories structure
SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
  AND table_name = 'equipment_categories'
ORDER BY ordinal_position;

-- Check equipment_items structure
SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    numeric_precision,
    numeric_scale,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
  AND table_name = 'equipment_items'
ORDER BY ordinal_position;

-- Count records
SELECT 
    'Categories' as table_name,
    COUNT(*) as record_count
FROM public.equipment_categories
UNION ALL
SELECT 
    'Items' as table_name,
    COUNT(*) as record_count
FROM public.equipment_items;

-- Show sample data
SELECT * FROM public.equipment_categories ORDER BY category_code;
SELECT * FROM public.equipment_items ORDER BY equipment_code;
