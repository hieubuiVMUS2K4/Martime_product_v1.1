-- Fix Equipment Tables Schema to Match C# Models
-- Run this script to align database with the application code

-- =================================================================
-- STEP 1: Drop and recreate equipment_items table with correct schema
-- =================================================================

DROP TABLE IF EXISTS public.equipment_items CASCADE;
DROP TABLE IF EXISTS public.equipment_categories CASCADE;

-- Create equipment_categories table
CREATE TABLE public.equipment_categories (
    id BIGSERIAL PRIMARY KEY,
    category_code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE public.equipment_categories IS 'Equipment categories for classification';

-- Create equipment_items table matching C# model
CREATE TABLE public.equipment_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    category_id BIGINT NOT NULL,
    description TEXT,
    location VARCHAR(100),
    specification TEXT,
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    solas_reference VARCHAR(200),
    quantity DOUBLE PRECISION NOT NULL DEFAULT 1.0,
    status VARCHAR(30) NOT NULL DEFAULT 'OPERATIONAL',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_synced BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node VARCHAR(50) NOT NULL DEFAULT 'SHIP_01',
    
    CONSTRAINT fk_equipment_items_category 
        FOREIGN KEY (category_id) 
        REFERENCES public.equipment_categories(id) 
        ON DELETE RESTRICT
);

-- Create indexes for equipment_items
CREATE INDEX ix_equipment_items_equipment_code ON public.equipment_items(equipment_code);
CREATE INDEX ix_equipment_items_category_id ON public.equipment_items(category_id);
CREATE INDEX ix_equipment_items_status ON public.equipment_items(status);
CREATE INDEX ix_equipment_items_is_active ON public.equipment_items(is_active);

COMMENT ON TABLE public.equipment_items IS 'Individual equipment items tracked for maintenance and compliance';

-- =================================================================
-- STEP 2: Insert sample equipment categories
-- =================================================================

INSERT INTO public.equipment_categories (category_code, name, description, is_active) VALUES
('LSA', 'Life Saving Appliances', 'Lifeboats, liferafts, lifejackets, and other life-saving equipment', true),
('FFA', 'Fire Fighting Appliances', 'Fire extinguishers, fire hoses, fire pumps, and fire detection systems', true),
('NAV', 'Navigation Equipment', 'GPS, radar, echo sounder, gyro compass, and other navigation equipment', true),
('COM', 'Communication Equipment', 'VHF radio, GMDSS equipment, satellite communication systems', true),
('ENG', 'Main Engine Equipment', 'Main engine components, turbochargers, fuel systems', true),
('AUX', 'Auxiliary Machinery', 'Generators, pumps, compressors, purifiers', true),
('DECK', 'Deck Machinery', 'Cranes, winches, windlass, mooring equipment', true),
('SAFE', 'Safety Equipment', 'EPIRB, SART, immersion suits, safety signs', true),
('POLL', 'Pollution Prevention', 'Oil water separators, sewage treatment, garbage management', true),
('ELEC', 'Electrical Systems', 'Switchboards, transformers, batteries, emergency lighting', true);

-- =================================================================
-- STEP 3: Insert sample equipment items
-- =================================================================

INSERT INTO public.equipment_items (
    equipment_code, name, category_id, description, location, 
    manufacturer, model, serial_number, quantity, status, origin_node
) VALUES
-- Life Saving Appliances
('LSA-001', 'Lifeboat Port Side', 1, 'Port side rescue lifeboat - 50 person capacity', 'Port Boat Deck', 'Viking', 'LSB-50', 'VK-LSB-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('LSA-002', 'Lifeboat Starboard Side', 1, 'Starboard side rescue lifeboat - 50 person capacity', 'Starboard Boat Deck', 'Viking', 'LSB-50', 'VK-LSB-2018-002', 1, 'OPERATIONAL', 'SHIP_01'),
('LSA-003', 'Life Raft 25P Port', 1, 'Inflatable life raft - 25 person capacity', 'Port Main Deck', 'Survitec', 'SR-25', 'SV-SR-2019-101', 1, 'OPERATIONAL', 'SHIP_01'),
('LSA-004', 'Life Raft 25P Starboard', 1, 'Inflatable life raft - 25 person capacity', 'Starboard Main Deck', 'Survitec', 'SR-25', 'SV-SR-2019-102', 1, 'OPERATIONAL', 'SHIP_01'),
('LSA-005', 'Lifejacket Adult', 1, 'SOLAS approved adult lifejacket', 'Various cabins', 'Baltic', 'LJ-150', 'Multiple', 60, 'OPERATIONAL', 'SHIP_01'),

-- Fire Fighting Appliances
('FFA-001', 'CO2 Fire Extinguisher 5kg', 2, 'Portable CO2 fire extinguisher', 'Engine Room', 'Kidde', 'KFE-5CO2', 'KD-CO2-2020-001', 12, 'OPERATIONAL', 'SHIP_01'),
('FFA-002', 'Foam Fire Extinguisher 9L', 2, 'AFFF foam extinguisher', 'Accommodation', 'Minimax', 'MX-9F', 'MM-FOAM-2020-012', 8, 'OPERATIONAL', 'SHIP_01'),
('FFA-003', 'Fire Hose with Nozzle', 2, 'Fire hose 20m with nozzle', 'Fire Station', 'Parker', 'PH-20', 'PK-FH-2019-101', 6, 'OPERATIONAL', 'SHIP_01'),
('FFA-004', 'Fire Pump Main', 2, 'Main fire fighting pump', 'Engine Room', 'Svanehoj', 'FP-500', 'SV-FP-2018-M01', 1, 'OPERATIONAL', 'SHIP_01'),
('FFA-005', 'Fire Detection System', 2, 'Automatic fire detection and alarm system', 'Throughout vessel', 'Autronica', 'AFS-100', 'AT-FDS-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),

-- Navigation Equipment
('NAV-001', 'GPS Navigator', 3, 'GNSS receiver with chart plotter', 'Wheelhouse', 'Furuno', 'GP-170', 'FU-GPS-2019-001', 1, 'OPERATIONAL', 'SHIP_01'),
('NAV-002', 'Radar X-Band', 3, 'X-band navigation radar', 'Wheelhouse', 'Furuno', 'FR-8252', 'FU-RAD-2019-X01', 1, 'OPERATIONAL', 'SHIP_01'),
('NAV-003', 'Gyro Compass', 3, 'Master gyro compass', 'Wheelhouse', 'Tokimec', 'TG-8000', 'TK-GYR-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('NAV-004', 'Echo Sounder', 3, 'Dual frequency echo sounder', 'Wheelhouse', 'Furuno', 'FE-700', 'FU-ECH-2019-001', 1, 'OPERATIONAL', 'SHIP_01'),
('NAV-005', 'AIS Transponder', 3, 'Class A AIS transponder', 'Wheelhouse', 'Furuno', 'FA-170', 'FU-AIS-2019-001', 1, 'OPERATIONAL', 'SHIP_01'),

-- Communication Equipment
('COM-001', 'VHF Radio Main', 4, 'Main VHF DSC radio', 'Wheelhouse', 'Icom', 'IC-M506', 'IC-VHF-2019-M01', 1, 'OPERATIONAL', 'SHIP_01'),
('COM-002', 'VHF Radio Portable', 4, 'Portable VHF handheld', 'Bridge Wing', 'Icom', 'IC-M73', 'IC-VHF-2019-P01', 4, 'OPERATIONAL', 'SHIP_01'),
('COM-003', 'EPIRB 406MHz', 4, 'Emergency Position Indicating Radio Beacon', 'Bridge Top', 'McMurdo', 'E6', 'MC-EPB-2020-001', 1, 'OPERATIONAL', 'SHIP_01'),
('COM-004', 'SART Radar Transponder', 4, 'Search and Rescue Transponder', 'Lifeboat', 'McMurdo', 'S4', 'MC-SRT-2020-001', 2, 'OPERATIONAL', 'SHIP_01'),
('COM-005', 'Satellite Phone', 4, 'Inmarsat Fleet Broadband terminal', 'Communications Room', 'Cobham', 'FB500', 'CB-SAT-2019-001', 1, 'OPERATIONAL', 'SHIP_01'),

-- Main Engine Equipment
('ENG-001', 'Main Engine', 5, 'MAN B&W 6S50MC-C diesel engine', 'Engine Room', 'MAN', '6S50MC-C', 'MAN-ME-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('ENG-002', 'Turbocharger', 5, 'Main engine turbocharger', 'Engine Room', 'ABB', 'TPS57-F14', 'ABB-TC-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('ENG-003', 'Fuel Oil Pump Main', 5, 'Main engine fuel oil supply pump', 'Engine Room', 'Alfa Laval', 'S-RO-15', 'AL-FOP-2018-M01', 1, 'OPERATIONAL', 'SHIP_01'),
('ENG-004', 'Cooling Water Pump', 5, 'Main engine cooling water circulating pump', 'Engine Room', 'Grundfos', 'CR-150', 'GF-CWP-2018-001', 2, 'OPERATIONAL', 'SHIP_01'),

-- Auxiliary Machinery
('AUX-001', 'Diesel Generator No.1', 6, 'Auxiliary diesel generator 500 kW', 'Engine Room', 'Wartsila', '6L20', 'WA-DG-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('AUX-002', 'Diesel Generator No.2', 6, 'Auxiliary diesel generator 500 kW', 'Engine Room', 'Wartsila', '6L20', 'WA-DG-2018-002', 1, 'OPERATIONAL', 'SHIP_01'),
('AUX-003', 'Air Compressor Main', 6, 'Main starting air compressor', 'Engine Room', 'Sperre', 'VC-500', 'SP-AC-2018-M01', 1, 'OPERATIONAL', 'SHIP_01'),
('AUX-004', 'Fuel Oil Purifier', 6, 'Centrifugal fuel oil separator', 'Engine Room', 'Alfa Laval', 'S-900', 'AL-FOP-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('AUX-005', 'Lube Oil Purifier', 6, 'Centrifugal lube oil separator', 'Engine Room', 'Alfa Laval', 'S-800', 'AL-LOP-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),

-- Deck Machinery
('DECK-001', 'Cargo Crane No.1', 7, 'Electro-hydraulic deck crane 40T', 'Main Deck Forward', 'Liebherr', 'CBB-400', 'LH-CRN-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('DECK-002', 'Cargo Crane No.2', 7, 'Electro-hydraulic deck crane 40T', 'Main Deck Aft', 'Liebherr', 'CBB-400', 'LH-CRN-2018-002', 1, 'OPERATIONAL', 'SHIP_01'),
('DECK-003', 'Windlass', 7, 'Electric anchor windlass', 'Forecastle Deck', 'MacGregor', 'WL-50', 'MG-WDL-2018-001', 1, 'OPERATIONAL', 'SHIP_01'),
('DECK-004', 'Mooring Winch Port', 7, 'Electric mooring winch', 'Port Side', 'MacGregor', 'MW-30', 'MG-MWP-2018-001', 2, 'OPERATIONAL', 'SHIP_01'),
('DECK-005', 'Mooring Winch Starboard', 7, 'Electric mooring winch', 'Starboard Side', 'MacGregor', 'MW-30', 'MG-MWS-2018-002', 2, 'OPERATIONAL', 'SHIP_01');

-- =================================================================
-- STEP 4: Verify installation
-- =================================================================

-- Check categories
SELECT 'Equipment Categories Created:' as status, COUNT(*) as count FROM public.equipment_categories;

-- Check items
SELECT 'Equipment Items Created:' as status, COUNT(*) as count FROM public.equipment_items;

-- Show sample data
SELECT 
    ei.equipment_code,
    ei.name,
    ec.name as category_name,
    ei.status,
    ei.quantity
FROM public.equipment_items ei
JOIN public.equipment_categories ec ON ei.category_id = ec.id
ORDER BY ei.equipment_code
LIMIT 10;

COMMENT ON COLUMN public.equipment_items.id IS 'UUID primary key';
COMMENT ON COLUMN public.equipment_items.category_id IS 'Foreign key to equipment_categories';
COMMENT ON COLUMN public.equipment_items.quantity IS 'Number of units of this equipment';
COMMENT ON COLUMN public.equipment_items.solas_reference IS 'SOLAS regulation reference if applicable';
COMMENT ON COLUMN public.equipment_items.is_synced IS 'Sync status with shore database';
COMMENT ON COLUMN public.equipment_items.origin_node IS 'Source vessel/location identifier';
