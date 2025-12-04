-- Create Equipment Tables
-- Run this SQL script directly in PostgreSQL to create equipment tables

-- Create equipment_categories table
CREATE TABLE IF NOT EXISTS public.equipment_categories (
    id BIGSERIAL PRIMARY KEY,
    category_code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS ix_equipment_categories_category_code ON public.equipment_categories(category_code);
CREATE INDEX IF NOT EXISTS ix_equipment_categories_is_active ON public.equipment_categories(is_active);

-- Create equipment_items table
CREATE TABLE IF NOT EXISTS public.equipment_items (
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
    CONSTRAINT fk_equipment_items_equipment_categories_category_id 
        FOREIGN KEY (category_id) 
        REFERENCES public.equipment_categories(id) 
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_equipment_items_equipment_code ON public.equipment_items(equipment_code);
CREATE INDEX IF NOT EXISTS ix_equipment_items_category_id ON public.equipment_items(category_id);
CREATE INDEX IF NOT EXISTS ix_equipment_items_status ON public.equipment_items(status);
CREATE INDEX IF NOT EXISTS ix_equipment_items_is_active ON public.equipment_items(is_active);
CREATE INDEX IF NOT EXISTS ix_equipment_items_is_synced ON public.equipment_items(is_synced);

-- Insert sample equipment categories
INSERT INTO public.equipment_categories (category_code, name, description) VALUES
('FIRE', 'Fire Fighting Equipment', 'Fire extinguishers, fire pumps, fire detection systems'),
('SAFETY', 'Safety Equipment', 'Life jackets, life rafts, emergency equipment'),
('SCBA', 'Self-Contained Breathing Apparatus', 'Breathing apparatus for emergency use'),
('CO2', 'CO2 Fire Suppression', 'CO2 cylinders and systems'),
('FOAM', 'Foam Fire Suppression', 'Foam concentrate and equipment'),
('LIFEBOAT', 'Lifeboats & Rescue Boats', 'Lifeboats, rescue boats, davits'),
('NAVIGATION', 'Navigation Equipment', 'Radar, GPS, ECDIS, AIS'),
('COMMUNICATION', 'Communication Equipment', 'VHF, GMDSS, satellite communication'),
('ENGINE', 'Engine Systems', 'Main engines, auxiliary engines, pumps'),
('ELECTRICAL', 'Electrical Systems', 'Generators, switchboards, batteries')
ON CONFLICT (category_code) DO NOTHING;

-- Insert sample equipment items
INSERT INTO public.equipment_items (equipment_code, name, category_id, location, quantity, status, specification) VALUES
('FSS.01', 'Portable Fire Extinguisher 5kg CO2', (SELECT id FROM public.equipment_categories WHERE category_code = 'FIRE'), 'Engine Room', 4, 'OPERATIONAL', '5kg CO2, Pressure: 15 bar'),
('FSS.02', 'Portable Fire Extinguisher 9kg Powder', (SELECT id FROM public.equipment_categories WHERE category_code = 'FIRE'), 'Main Deck', 6, 'OPERATIONAL', '9kg ABC Powder'),
('SE.01', 'Life Jacket Adult', (SELECT id FROM public.equipment_categories WHERE category_code = 'SAFETY'), 'Bridge', 50, 'OPERATIONAL', 'SOLAS approved, 150N'),
('LB.01', 'Port Side Lifeboat', (SELECT id FROM public.equipment_categories WHERE category_code = 'LIFEBOAT'), 'Port Side Main Deck', 1, 'OPERATIONAL', 'Capacity: 65 persons'),
('LB.02', 'Starboard Side Lifeboat', (SELECT id FROM public.equipment_categories WHERE category_code = 'LIFEBOAT'), 'Starboard Side Main Deck', 1, 'OPERATIONAL', 'Capacity: 65 persons'),
('SCBA.01', 'SCBA Set with 6L Cylinder', (SELECT id FROM public.equipment_categories WHERE category_code = 'SCBA'), 'Fire Station', 4, 'OPERATIONAL', '6 Liter, 300 bar, 30 min duration')
ON CONFLICT (equipment_code) DO NOTHING;

COMMENT ON TABLE public.equipment_categories IS 'Equipment categories for maritime safety and operational equipment';
COMMENT ON TABLE public.equipment_items IS 'Individual equipment items tracked for maintenance and compliance';
