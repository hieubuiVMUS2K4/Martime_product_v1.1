BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Server-safe LSA demo seed.
-- This script does not hard-delete equipment_assets.
-- It upserts by asset_code, then resolves parent_id from the actual DB ids.

CREATE TEMP TABLE tmp_lsa_assets (
    asset_code text PRIMARY KEY,
    name text NOT NULL,
    category text NOT NULL,
    parent_code text NULL,
    location text NULL,
    criticality text NOT NULL,
    status text NOT NULL,
    technical_specs text NULL,
    notes text NULL
) ON COMMIT DROP;

INSERT INTO tmp_lsa_assets (asset_code, name, category, parent_code, location, criticality, status, technical_specs, notes)
VALUES
    ('LSA-SYS', 'Life Saving Appliances (LSA)', 'SYSTEM', NULL, 'Vessel', 'CRITICAL', 'ACTIVE', NULL, 'Root system for life saving appliances'),

    ('LSA-LB', 'Lifeboat', 'SYSTEM', 'LSA-SYS', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Lifeboat system folder'),
    ('LB-01', 'Lifeboat No.1 (Port)', 'SAFETY', 'LSA-LB', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', NULL, 'Port lifeboat'),
    ('LB-01-HULL', 'Outside Hull', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Hull inspection item'),
    ('LB-01-CANOPY', 'Outside Canopy', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Canopy inspection item'),
    ('LB-01-LIFELINE', 'Buoyant Lifeline', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Lifeline item'),
    ('LB-01-DRAIN', 'Drain Valve', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Drain valve item'),
    ('LB-01-HOOK', 'Hook', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', NULL, 'Release hook item'),
    ('LB-01-ENGINE', 'Engine', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', '{"fuelType":"Diesel"}', 'Lifeboat engine'),
    ('LB-01-STEERING', 'Steering System', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Steering system'),
    ('LB-01-FUEL', 'Fuel Tank', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Fuel tank'),
    ('LB-01-BATTERY', 'Battery', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Battery unit'),
    ('LB-01-SEARCHLIGHT', 'Search Light', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Search light'),
    ('LB-01-FIREEXT', 'Fire Extinguisher', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', NULL, 'Fire extinguisher'),
    ('LB-01-WATERSPRAY', 'Water Spray System', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Water spray system'),
    ('LB-01-FOOD', 'Food Ration', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', '{"quantity":10}', 'Food ration stock item'),
    ('LB-01-WATER', 'Water Ration', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', '{"quantity":20}', 'Water ration stock item'),
    ('LB-01-FIRSTAID', 'First Aid Kit', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'First aid kit'),
    ('LB-01-COMPASS', 'Compass', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Compass'),
    ('LB-01-PAINTER', 'Painter', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Painter rope'),
    ('LB-01-RADIO', 'Radio Equipment', 'SAFETY', 'LB-01', 'Boat Deck - Port', 'HIGH', 'ACTIVE', NULL, 'Radio equipment'),

    ('LB-02', 'Lifeboat No.2 (Starboard)', 'SAFETY', 'LSA-LB', 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', NULL, 'Starboard lifeboat'),
    ('LB-02-HULL', 'Outside Hull', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Hull inspection item'),
    ('LB-02-CANOPY', 'Outside Canopy', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Canopy inspection item'),
    ('LB-02-DRAIN', 'Drain Valve', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Drain valve item'),
    ('LB-02-HOOK', 'Hook', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', NULL, 'Release hook item'),
    ('LB-02-ENGINE', 'Engine', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', '{"fuelType":"Diesel"}', 'Lifeboat engine'),
    ('LB-02-BATTERY', 'Battery', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Battery unit'),
    ('LB-02-FOOD', 'Food Ration', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', '{"quantity":10}', 'Food ration stock item'),
    ('LB-02-WATER', 'Water Ration', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', '{"quantity":20}', 'Water ration stock item'),
    ('LB-02-FIRSTAID', 'First Aid Kit', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'First aid kit'),
    ('LB-02-PAINTER', 'Painter', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Painter rope'),
    ('LB-02-RADIO', 'Radio Equipment', 'SAFETY', 'LB-02', 'Boat Deck - Starboard', 'HIGH', 'ACTIVE', NULL, 'Radio equipment'),

    ('LSA-LBA', 'Lifeboat Launching Appliance', 'SYSTEM', 'LSA-SYS', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Launching appliance folder'),
    ('LBA-DAVIT-FRAME', 'Davit Frame', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Davit frame'),
    ('LBA-DAVIT-ARMS', 'Davit Arms', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Davit arms'),
    ('LBA-WINCH', 'Winch', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Winch'),
    ('LBA-BRAKE', 'Brake', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Brake system'),
    ('LBA-WIRE-ROPE', 'Wire Rope', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Wire rope'),
    ('LBA-SHEAVES', 'Sheaves', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Sheaves'),
    ('LBA-HOOK', 'Hook', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Launching hook'),
    ('LBA-HYDRAULIC', 'Hydraulic System', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Hydraulic system'),
    ('LBA-LUBRICATION', 'Lubrication', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Lubrication points'),
    ('LBA-FOUNDATION', 'Foundation', 'SAFETY', 'LSA-LBA', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Foundation'),

    ('LSA-RB', 'Rescue Boat', 'SYSTEM', 'LSA-SYS', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Rescue boat folder'),
    ('RB-01', 'Rescue Boat No.1', 'SAFETY', 'LSA-RB', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Rescue boat'),
    ('RB-01-HULL', 'Outside Hull', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Hull'),
    ('RB-01-LIFELINE', 'Buoyant Lifeline', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Lifeline'),
    ('RB-01-DRAIN', 'Drain Valve', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Drain valve'),
    ('RB-01-ENGINE', 'Engine', 'SAFETY', 'RB-01', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Rescue boat engine'),
    ('RB-01-STEERING', 'Steering', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Steering'),
    ('RB-01-FUEL', 'Fuel Tank', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Fuel tank'),
    ('RB-01-BATTERY', 'Battery', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Battery'),
    ('RB-01-EQUIPMENT', 'Equipment', 'SAFETY', 'RB-01', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Rescue boat equipment'),

    ('LSA-LR', 'Liferaft', 'SYSTEM', 'LSA-SYS', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Liferaft folder'),
    ('LR-01', 'Liferaft No.1', 'SAFETY', 'LSA-LR', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', NULL, 'Liferaft no.1'),
    ('LR-02', 'Liferaft No.2', 'SAFETY', 'LSA-LR', 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', NULL, 'Liferaft no.2'),
    ('LR-03', 'Liferaft No.3', 'SAFETY', 'LSA-LR', 'Boat Deck - Port', 'CRITICAL', 'ACTIVE', NULL, 'Liferaft no.3'),
    ('LR-04', 'Liferaft No.4', 'SAFETY', 'LSA-LR', 'Boat Deck - Starboard', 'CRITICAL', 'ACTIVE', NULL, 'Liferaft no.4'),
    ('LR-HRU', 'Hydrostatic Release Unit', 'SAFETY', 'LSA-LR', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'HRU common item'),
    ('LR-LASHING', 'Lashing', 'SAFETY', 'LSA-LR', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Lashing common item'),
    ('LR-PAINTER', 'Painter', 'SAFETY', 'LSA-LR', 'Boat Deck', 'HIGH', 'ACTIVE', NULL, 'Painter common item'),
    ('LR-HOOK', 'Releasing Hook', 'SAFETY', 'LSA-LR', 'Boat Deck', 'CRITICAL', 'ACTIVE', NULL, 'Releasing hook'),

    ('LSA-ALARM', 'Alarm & Lighting', 'SYSTEM', 'LSA-SYS', 'Vessel', 'CRITICAL', 'ACTIVE', NULL, 'Alarm and emergency lighting folder'),
    ('ALARM-GEA', 'General Emergency Alarm', 'SAFETY', 'LSA-ALARM', 'Vessel', 'CRITICAL', 'ACTIVE', NULL, 'General emergency alarm'),
    ('ALARM-PA', 'Public Address', 'SAFETY', 'LSA-ALARM', 'Vessel', 'CRITICAL', 'ACTIVE', NULL, 'Public address system'),
    ('ALARM-ELIGHT', 'Emergency Lighting', 'SAFETY', 'LSA-ALARM', 'Accommodation / Engine Room / Deck', 'CRITICAL', 'ACTIVE', NULL, 'Emergency lighting');

INSERT INTO public.equipment_assets (
    asset_code,
    name,
    category,
    manufacturer,
    model,
    serial_number,
    installation_date,
    current_running_hours,
    last_running_hours_update,
    equipment_group_id,
    location,
    criticality,
    status,
    default_executor_role,
    approver_role,
    technical_specs,
    notes,
    is_active,
    is_synced,
    created_at,
    updated_at,
    origin_node,
    parent_id
)
SELECT
    asset_code,
    name,
    category,
    NULL,
    NULL,
    NULL,
    NULL,
    0,
    NULL,
    NULL,
    location,
    criticality,
    status,
    NULL,
    NULL,
    technical_specs,
    notes,
    TRUE,
    FALSE,
    NOW(),
    NOW(),
    'SHIP_01',
    NULL
FROM tmp_lsa_assets
ON CONFLICT (asset_code) DO UPDATE SET
    name = EXCLUDED.name,
    category = EXCLUDED.category,
    location = EXCLUDED.location,
    criticality = EXCLUDED.criticality,
    status = EXCLUDED.status,
    technical_specs = EXCLUDED.technical_specs,
    notes = EXCLUDED.notes,
    is_active = TRUE,
    updated_at = NOW();

UPDATE public.equipment_assets child
SET parent_id = parent.id,
    updated_at = NOW()
FROM tmp_lsa_assets child_seed
JOIN tmp_lsa_assets parent_seed ON parent_seed.asset_code = child_seed.parent_code
JOIN public.equipment_assets parent ON parent.asset_code = parent_seed.asset_code
WHERE child.asset_code = child_seed.asset_code;

UPDATE public.equipment_assets root
SET parent_id = NULL,
    updated_at = NOW()
FROM tmp_lsa_assets seed
WHERE root.asset_code = seed.asset_code
  AND seed.parent_code IS NULL;

SELECT setval(
    pg_get_serial_sequence('public.material_categories', 'id'),
    COALESCE((SELECT MAX(id) FROM public.material_categories), 0) + 1,
    false
);

INSERT INTO public.material_categories (
    category_code,
    name,
    description,
    parent_category_id,
    is_active,
    is_synced,
    created_at
)
VALUES (
    'LSA-SPARES',
    'Life Saving Appliance Spares',
    'Spare parts and consumables for life saving appliances',
    NULL,
    TRUE,
    FALSE,
    NOW()
)
ON CONFLICT (category_code) DO UPDATE SET
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

CREATE TEMP TABLE tmp_lsa_materials (
    item_code text PRIMARY KEY,
    name text NOT NULL,
    specification text NULL,
    unit text NOT NULL,
    min_stock double precision NULL,
    max_stock double precision NULL,
    reorder_level double precision NULL,
    reorder_quantity double precision NULL,
    location text NULL,
    part_number text NULL,
    batch_tracked boolean NOT NULL,
    serial_tracked boolean NOT NULL,
    expiry_required boolean NOT NULL,
    unit_cost numeric NULL,
    notes text NULL
) ON COMMIT DROP;

INSERT INTO tmp_lsa_materials (item_code, name, specification, unit, min_stock, max_stock, reorder_level, reorder_quantity, location, part_number, batch_tracked, serial_tracked, expiry_required, unit_cost, notes)
VALUES
    ('LSA-HULL-REPAIR', 'Hull repair kit', 'Fiberglass repair kit for boat hull', 'SET', 1, 4, 1, 1, 'Safety Store', 'HULL-REPAIR', FALSE, FALSE, FALSE, 120, 'For outside hull repairs'),
    ('LSA-CANOPY-PATCH', 'Canopy patch kit', 'Patch kit for lifeboat canopy', 'SET', 1, 4, 1, 1, 'Safety Store', 'CANOPY-PATCH', FALSE, FALSE, FALSE, 45, 'For canopy minor repairs'),
    ('LSA-LIFELINE', 'Buoyant lifeline', 'Floating lifeline for survival craft', 'PCS', 2, 10, 2, 2, 'Safety Store', 'LIFELINE', FALSE, FALSE, FALSE, 35, 'Replacement buoyant lifeline'),
    ('LSA-DRAIN-VALVE', 'Drain valve kit', 'Drain valve spare kit', 'SET', 2, 10, 2, 2, 'Safety Store', 'DRAIN-VALVE', FALSE, FALSE, FALSE, 30, 'Drain valve spare'),
    ('LSA-HOOK-KIT', 'Release hook spare kit', 'Release hook maintenance spare kit', 'SET', 1, 4, 1, 1, 'Safety Store', 'HOOK-KIT', FALSE, TRUE, FALSE, 350, 'Critical hook spare kit'),
    ('LSA-ENGINE-OIL', 'Survival craft engine oil', 'Engine oil for lifeboat/rescue boat engine', 'LTR', 4, 40, 4, 8, 'Engine Store', 'ENGINE-OIL', FALSE, FALSE, FALSE, 15, 'Engine oil reserve'),
    ('LSA-SPARK-PLUG', 'Spark plug', 'Spark plug for rescue/lifeboat engine', 'PCS', 4, 30, 4, 8, 'Engine Store', 'SPARK-PLUG', FALSE, FALSE, FALSE, 9, 'Engine ignition spare'),
    ('LSA-FUEL-FILTER', 'Fuel filter', 'Fuel filter for survival craft engine', 'PCS', 2, 12, 2, 4, 'Engine Store', 'FUEL-FILTER', FALSE, FALSE, FALSE, 18, 'Fuel system spare'),
    ('LSA-BATTERY', 'Battery pack', 'Battery pack for survival craft', 'PCS', 1, 6, 1, 2, 'Safety Store', 'BATTERY', TRUE, TRUE, FALSE, 95, 'Serial tracked battery'),
    ('LSA-SEARCHLIGHT-BULB', 'Search light bulb', 'Replacement bulb for search light', 'PCS', 2, 12, 2, 4, 'Bridge Store', 'SEARCH-BULB', FALSE, FALSE, FALSE, 20, 'Search light spare'),
    ('LSA-FIREEXT', 'Portable fire extinguisher', 'Portable extinguisher for survival craft', 'PCS', 1, 6, 1, 2, 'Safety Store', 'FIREEXT', TRUE, TRUE, TRUE, 65, 'Expiry controlled extinguisher'),
    ('LSA-WATERSPRAY-NOZZLE', 'Water spray nozzle', 'Nozzle for water spray system', 'PCS', 2, 12, 2, 4, 'Safety Store', 'SPRAY-NOZZLE', FALSE, FALSE, FALSE, 12, 'Water spray spare'),
    ('LSA-FOOD-RATION', 'Emergency food ration', 'SOLAS emergency food ration pack', 'PACK', 10, 100, 10, 20, 'Safety Store', 'FOOD-RATION', TRUE, FALSE, TRUE, 12, 'Expiry controlled ration'),
    ('LSA-WATER-RATION', 'Emergency drinking water', 'SOLAS emergency drinking water pack', 'PACK', 20, 140, 20, 30, 'Safety Store', 'WATER-RATION', TRUE, FALSE, TRUE, 5, 'Expiry controlled water ration'),
    ('LSA-FIRSTAID', 'First aid kit', 'SOLAS first aid kit', 'SET', 2, 8, 2, 2, 'Safety Store', 'FIRSTAID', TRUE, FALSE, TRUE, 80, 'Medical kit'),
    ('LSA-COMPASS', 'Boat compass', 'Compass for survival craft', 'PCS', 1, 4, 1, 1, 'Bridge Store', 'COMPASS', FALSE, TRUE, FALSE, 120, 'Compass spare'),
    ('LSA-PAINTER-ROPE', 'Painter rope', 'Painter rope for lifeboat/liferaft', 'PCS', 2, 12, 2, 4, 'Safety Store', 'PAINTER', FALSE, FALSE, FALSE, 45, 'Painter rope'),
    ('LSA-VHF-BATTERY', 'Portable VHF battery pack', 'Rechargeable battery pack for portable VHF', 'PCS', 3, 10, 3, 3, 'Bridge Store', 'VHF-BATT', TRUE, TRUE, FALSE, 75, 'Radio battery'),
    ('LSA-VHF-CHARGER', 'Portable VHF charger', 'Charger cradle for portable VHF', 'PCS', 2, 8, 2, 2, 'Bridge Store', 'VHF-CHARGER', FALSE, FALSE, FALSE, 55, 'Radio charger'),
    ('LSA-WINCH-OIL', 'Winch gear oil', 'Gear oil for launching appliance winch', 'LTR', 4, 30, 4, 8, 'Deck Store', 'WINCH-OIL', FALSE, FALSE, FALSE, 16, 'Winch lubricant'),
    ('LSA-BRAKE-LINING', 'Brake lining set', 'Brake lining set for davit winch', 'SET', 1, 4, 1, 1, 'Deck Store', 'BRAKE-LINING', FALSE, FALSE, FALSE, 180, 'Brake spare'),
    ('LSA-WIRE-ROPE', 'Launching wire rope', 'Wire rope for lifeboat launching appliance', 'PCS', 1, 4, 1, 1, 'Deck Store', 'WIRE-ROPE', FALSE, TRUE, FALSE, 650, 'Critical wire rope'),
    ('LSA-SHEAVE-BEARING', 'Sheave bearing', 'Bearing for launching appliance sheaves', 'PCS', 2, 12, 2, 4, 'Deck Store', 'SHEAVE-BRG', FALSE, FALSE, FALSE, 55, 'Sheave bearing spare'),
    ('LSA-HYD-OIL', 'Hydraulic oil', 'Hydraulic oil for launching appliance', 'LTR', 10, 80, 10, 20, 'Deck Store', 'HYD-OIL', FALSE, FALSE, FALSE, 8, 'Hydraulic oil'),
    ('LSA-GREASE', 'Marine grease', 'Water resistant marine grease', 'KG', 4, 25, 4, 8, 'Deck Store', 'GREASE', FALSE, FALSE, FALSE, 9, 'Lubrication consumable'),
    ('LSA-LR-HRU', 'Hydrostatic release unit', 'HRU for inflatable liferaft', 'PCS', 2, 8, 2, 2, 'Safety Store', 'LR-HRU', TRUE, FALSE, TRUE, 95, 'Expiry controlled HRU'),
    ('LSA-LR-LASHING', 'Liferaft lashing strap', 'Replacement lashing strap', 'PCS', 4, 16, 4, 4, 'Safety Store', 'LR-LASHING', FALSE, FALSE, FALSE, 25, 'Liferaft lashing strap'),
    ('LSA-ALARM-BELL', 'Emergency alarm bell', 'Bell for general emergency alarm', 'PCS', 1, 6, 1, 2, 'Electrical Store', 'ALARM-BELL', FALSE, FALSE, FALSE, 45, 'Alarm spare'),
    ('LSA-PA-SPEAKER', 'Public address speaker', 'PA speaker unit', 'PCS', 2, 10, 2, 4, 'Electrical Store', 'PA-SPEAKER', FALSE, FALSE, FALSE, 35, 'PA speaker spare'),
    ('LSA-EMG-LIGHT', 'Emergency light unit', 'Emergency lighting unit', 'PCS', 4, 20, 4, 6, 'Electrical Store', 'EMG-LIGHT', FALSE, FALSE, FALSE, 40, 'Emergency light spare'),
    ('LSA-EMG-BATTERY', 'Emergency light battery', 'Battery for emergency lighting', 'PCS', 4, 20, 4, 6, 'Electrical Store', 'EMG-BATT', TRUE, TRUE, FALSE, 28, 'Battery spare');

WITH category AS (
    SELECT id
    FROM public.material_categories
    WHERE category_code = 'LSA-SPARES'
)
INSERT INTO public.material_items (
    id,
    item_code,
    name,
    category_id,
    specification,
    unit,
    on_hand_quantity,
    min_stock,
    max_stock,
    reorder_level,
    reorder_quantity,
    location,
    manufacturer,
    supplier,
    part_number,
    barcode,
    batch_tracked,
    serial_tracked,
    expiry_required,
    unit_cost,
    currency,
    notes,
    is_active,
    is_synced,
    created_at,
    updated_at,
    origin_node,
    image_url
)
SELECT
    gen_random_uuid(),
    m.item_code,
    m.name,
    c.id,
    m.specification,
    m.unit,
    0,
    m.min_stock,
    m.max_stock,
    m.reorder_level,
    m.reorder_quantity,
    m.location,
    'Generic Marine',
    'Marine Safety Supplier',
    m.part_number,
    NULL,
    m.batch_tracked,
    m.serial_tracked,
    m.expiry_required,
    m.unit_cost,
    'USD',
    m.notes,
    TRUE,
    FALSE,
    NOW(),
    NOW(),
    'SHIP_01',
    NULL
FROM tmp_lsa_materials m
CROSS JOIN category c
ON CONFLICT (item_code) DO UPDATE SET
    name = EXCLUDED.name,
    category_id = EXCLUDED.category_id,
    specification = EXCLUDED.specification,
    unit = EXCLUDED.unit,
    min_stock = EXCLUDED.min_stock,
    max_stock = EXCLUDED.max_stock,
    reorder_level = EXCLUDED.reorder_level,
    reorder_quantity = EXCLUDED.reorder_quantity,
    location = EXCLUDED.location,
    manufacturer = EXCLUDED.manufacturer,
    supplier = EXCLUDED.supplier,
    part_number = EXCLUDED.part_number,
    batch_tracked = EXCLUDED.batch_tracked,
    serial_tracked = EXCLUDED.serial_tracked,
    expiry_required = EXCLUDED.expiry_required,
    unit_cost = EXCLUDED.unit_cost,
    currency = EXCLUDED.currency,
    notes = EXCLUDED.notes,
    is_active = TRUE,
    updated_at = NOW();

CREATE TEMP TABLE tmp_lsa_material_links (
    asset_code text NOT NULL,
    item_code text NOT NULL,
    quantity_required double precision NOT NULL,
    notes text NULL
) ON COMMIT DROP;

INSERT INTO tmp_lsa_material_links (asset_code, item_code, quantity_required, notes)
VALUES
    ('LB-01-HULL', 'LSA-HULL-REPAIR', 1, 'Hull repair kit for port lifeboat'),
    ('LB-01-CANOPY', 'LSA-CANOPY-PATCH', 1, 'Canopy patch kit'),
    ('LB-01-LIFELINE', 'LSA-LIFELINE', 1, 'Buoyant lifeline'),
    ('LB-01-DRAIN', 'LSA-DRAIN-VALVE', 2, 'Drain valve spare'),
    ('LB-01-HOOK', 'LSA-HOOK-KIT', 1, 'Release hook spare kit'),
    ('LB-01-ENGINE', 'LSA-ENGINE-OIL', 4, 'Engine oil reserve'),
    ('LB-01-ENGINE', 'LSA-SPARK-PLUG', 4, 'Spark plugs'),
    ('LB-01-ENGINE', 'LSA-FUEL-FILTER', 2, 'Fuel filters'),
    ('LB-01-BATTERY', 'LSA-BATTERY', 1, 'Battery pack'),
    ('LB-01-SEARCHLIGHT', 'LSA-SEARCHLIGHT-BULB', 2, 'Search light bulbs'),
    ('LB-01-FIREEXT', 'LSA-FIREEXT', 1, 'Fire extinguisher'),
    ('LB-01-WATERSPRAY', 'LSA-WATERSPRAY-NOZZLE', 2, 'Water spray nozzles'),
    ('LB-01-FOOD', 'LSA-FOOD-RATION', 10, 'Food rations'),
    ('LB-01-WATER', 'LSA-WATER-RATION', 20, 'Water rations'),
    ('LB-01-FIRSTAID', 'LSA-FIRSTAID', 1, 'First aid kit'),
    ('LB-01-COMPASS', 'LSA-COMPASS', 1, 'Compass'),
    ('LB-01-PAINTER', 'LSA-PAINTER-ROPE', 1, 'Painter rope'),
    ('LB-01-RADIO', 'LSA-VHF-BATTERY', 1, 'Radio battery'),
    ('LB-01-RADIO', 'LSA-VHF-CHARGER', 1, 'Radio charger'),
    ('LB-02-HULL', 'LSA-HULL-REPAIR', 1, 'Hull repair kit for starboard lifeboat'),
    ('LB-02-CANOPY', 'LSA-CANOPY-PATCH', 1, 'Canopy patch kit'),
    ('LB-02-DRAIN', 'LSA-DRAIN-VALVE', 2, 'Drain valve spare'),
    ('LB-02-HOOK', 'LSA-HOOK-KIT', 1, 'Release hook spare kit'),
    ('LB-02-ENGINE', 'LSA-ENGINE-OIL', 4, 'Engine oil reserve'),
    ('LB-02-ENGINE', 'LSA-SPARK-PLUG', 4, 'Spark plugs'),
    ('LB-02-ENGINE', 'LSA-FUEL-FILTER', 2, 'Fuel filters'),
    ('LB-02-BATTERY', 'LSA-BATTERY', 1, 'Battery pack'),
    ('LB-02-FOOD', 'LSA-FOOD-RATION', 10, 'Food rations'),
    ('LB-02-WATER', 'LSA-WATER-RATION', 20, 'Water rations'),
    ('LB-02-FIRSTAID', 'LSA-FIRSTAID', 1, 'First aid kit'),
    ('LB-02-PAINTER', 'LSA-PAINTER-ROPE', 1, 'Painter rope'),
    ('LB-02-RADIO', 'LSA-VHF-BATTERY', 1, 'Radio battery'),
    ('LBA-WINCH', 'LSA-WINCH-OIL', 4, 'Winch gear oil'),
    ('LBA-BRAKE', 'LSA-BRAKE-LINING', 1, 'Brake lining set'),
    ('LBA-WIRE-ROPE', 'LSA-WIRE-ROPE', 1, 'Launching wire rope'),
    ('LBA-SHEAVES', 'LSA-SHEAVE-BEARING', 4, 'Sheave bearings'),
    ('LBA-HYDRAULIC', 'LSA-HYD-OIL', 20, 'Hydraulic oil'),
    ('LBA-LUBRICATION', 'LSA-GREASE', 5, 'Marine grease'),
    ('RB-01-HULL', 'LSA-HULL-REPAIR', 1, 'Rescue boat hull repair kit'),
    ('RB-01-LIFELINE', 'LSA-LIFELINE', 1, 'Buoyant lifeline'),
    ('RB-01-DRAIN', 'LSA-DRAIN-VALVE', 1, 'Drain valve spare'),
    ('RB-01-ENGINE', 'LSA-ENGINE-OIL', 4, 'Engine oil'),
    ('RB-01-ENGINE', 'LSA-SPARK-PLUG', 4, 'Spark plugs'),
    ('RB-01-ENGINE', 'LSA-FUEL-FILTER', 2, 'Fuel filters'),
    ('RB-01-BATTERY', 'LSA-BATTERY', 1, 'Battery pack'),
    ('LR-01', 'LSA-LR-HRU', 1, 'HRU for liferaft no.1'),
    ('LR-01', 'LSA-LR-LASHING', 1, 'Lashing strap for liferaft no.1'),
    ('LR-01', 'LSA-PAINTER-ROPE', 1, 'Painter rope for liferaft no.1'),
    ('LR-02', 'LSA-LR-HRU', 1, 'HRU for liferaft no.2'),
    ('LR-02', 'LSA-LR-LASHING', 1, 'Lashing strap for liferaft no.2'),
    ('LR-02', 'LSA-PAINTER-ROPE', 1, 'Painter rope for liferaft no.2'),
    ('LR-03', 'LSA-LR-HRU', 1, 'HRU for liferaft no.3'),
    ('LR-04', 'LSA-LR-HRU', 1, 'HRU for liferaft no.4'),
    ('LR-HRU', 'LSA-LR-HRU', 4, 'Common HRU spare stock'),
    ('ALARM-GEA', 'LSA-ALARM-BELL', 2, 'Emergency alarm bell spare'),
    ('ALARM-PA', 'LSA-PA-SPEAKER', 2, 'PA speaker spare'),
    ('ALARM-ELIGHT', 'LSA-EMG-LIGHT', 6, 'Emergency light spare'),
    ('ALARM-ELIGHT', 'LSA-EMG-BATTERY', 6, 'Emergency light battery');

DELETE FROM public.material_item_equipments mie
USING tmp_lsa_material_links l
JOIN public.material_items m ON m.item_code = l.item_code
JOIN public.equipment_assets e ON e.asset_code = l.asset_code
WHERE mie.material_item_id = m.id
  AND mie.equipment_asset_id = e.id;

INSERT INTO public.material_item_equipments (
    id,
    material_item_id,
    equipment_asset_id,
    notes,
    quantity_required,
    created_at
)
SELECT
    gen_random_uuid(),
    m.id,
    e.id,
    l.notes,
    l.quantity_required,
    NOW()
FROM tmp_lsa_material_links l
JOIN public.material_items m ON m.item_code = l.item_code
JOIN public.equipment_assets e ON e.asset_code = l.asset_code;

COMMIT;
