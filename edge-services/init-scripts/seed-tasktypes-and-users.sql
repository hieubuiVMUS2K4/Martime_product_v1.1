-- =====================================================
-- INSERT SAMPLE TASKTYPES AND CREATE USERS FROM CREW
-- Chạy script này trong pgAdmin hoặc database client
-- =====================================================

-- =====================================================
-- 1. INSERT SAMPLE TASKTYPES
-- =====================================================

INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
-- ENGINE
('ENGINE_OIL_CHANGE', 'Engine Oil Change', 'Change engine oil and oil filter', 'ENGINE', 'NORMAL', 2, NULL, false, true, NOW()),
('ENGINE_COOLING_CHECK', 'Cooling System Check', 'Inspect cooling system, check coolant level and condition', 'ENGINE', 'NORMAL', 1, NULL, false, true, NOW()),
('ENGINE_FUEL_FILTER', 'Fuel Filter Replacement', 'Replace fuel filters and bleed fuel system', 'ENGINE', 'HIGH', 2, NULL, false, true, NOW()),
('TURBO_INSPECTION', 'Turbocharger Inspection', 'Inspect and clean turbocharger', 'ENGINE', 'HIGH', 4, 'Marine Engineer License', false, true, NOW()),
('ENGINE_OVERHAUL', 'Engine Overhaul', 'Complete overhaul of main engine', 'ENGINE', 'CRITICAL', 48, 'Marine Engineer License', true, true, NOW()),

-- DECK
('DECK_WASH', 'Deck Washing', 'Wash and clean deck area', 'DECK', 'NORMAL', 2, NULL, false, true, NOW()),
('HULL_INSPECTION', 'Hull Inspection', 'Inspect hull for damage, corrosion, and fouling', 'DECK', 'HIGH', 3, 'Diving Certificate', false, true, NOW()),
('MOORING_CHECK', 'Mooring Equipment Check', 'Inspect mooring lines, winches, and fairleads', 'DECK', 'NORMAL', 2, NULL, false, true, NOW()),
('ANCHOR_MAINTENANCE', 'Anchor System Maintenance', 'Inspect and lubricate anchor windlass', 'DECK', 'NORMAL', 3, NULL, false, true, NOW()),
('PAINT_TOUCH_UP', 'Paint Touch-up', 'Touch up paint on deck and superstructure', 'DECK', 'LOW', 4, NULL, false, true, NOW()),

-- SAFETY
('LIFEBOAT_DRILL', 'Lifeboat Drill', 'Conduct lifeboat launching drill and inspection', 'SAFETY', 'CRITICAL', 2, 'Safety Officer', true, true, NOW()),
('FIRE_EXTINGUISHER_CHECK', 'Fire Extinguisher Inspection', 'Inspect all fire extinguishers for pressure and damage', 'SAFETY', 'CRITICAL', 1, NULL, false, true, NOW()),
('EMERGENCY_LIGHT_TEST', 'Emergency Lighting Test', 'Test emergency lighting system', 'SAFETY', 'HIGH', 1, NULL, false, true, NOW()),
('LIFEJACKET_INSPECTION', 'Life Jacket Inspection', 'Inspect all life jackets for damage and proper storage', 'SAFETY', 'HIGH', 2, NULL, false, true, NOW()),
('SAFETY_EQUIPMENT_CHECK', 'Safety Equipment Check', 'General inspection of all safety equipment', 'SAFETY', 'CRITICAL', 3, 'Safety Officer', false, true, NOW()),

-- ELECTRICAL
('GENERATOR_MAINTENANCE', 'Generator Maintenance', 'Service and inspect emergency generator', 'ELECTRICAL', 'HIGH', 4, 'Electrical Officer', false, true, NOW()),
('BATTERY_CHECK', 'Battery Inspection', 'Check battery condition, voltage, and electrolyte level', 'ELECTRICAL', 'NORMAL', 1, NULL, false, true, NOW()),
('LIGHTING_INSPECTION', 'Navigation Light Inspection', 'Test all navigation lights and replace failed bulbs', 'ELECTRICAL', 'CRITICAL', 1, NULL, false, true, NOW()),
('ELECTRICAL_PANEL_CHECK', 'Electrical Panel Inspection', 'Inspect electrical panels for loose connections', 'ELECTRICAL', 'HIGH', 2, 'Electrical Officer', false, true, NOW()),
('UPS_TEST', 'UPS System Test', 'Test uninterruptible power supply system', 'ELECTRICAL', 'HIGH', 1, NULL, false, true, NOW()),

-- NAVIGATION
('RADAR_CALIBRATION', 'Radar Calibration', 'Calibrate and test radar equipment', 'NAVIGATION', 'HIGH', 2, 'Navigation Officer', false, true, NOW()),
('GPS_CHECK', 'GPS System Check', 'Verify GPS accuracy and update charts', 'NAVIGATION', 'HIGH', 1, 'Navigation Officer', false, true, NOW()),
('COMPASS_ADJUSTMENT', 'Compass Adjustment', 'Check and adjust magnetic compass', 'NAVIGATION', 'NORMAL', 2, 'Navigation Officer', false, true, NOW()),
('AUTOPILOT_TEST', 'Autopilot System Test', 'Test autopilot functionality and calibration', 'NAVIGATION', 'HIGH', 1, 'Navigation Officer', false, true, NOW()),
('CHART_UPDATE', 'Chart Update', 'Update electronic and paper charts', 'NAVIGATION', 'NORMAL', 2, 'Navigation Officer', false, true, NOW()),

-- HVAC
('AC_FILTER_CHANGE', 'AC Filter Replacement', 'Replace air conditioning filters', 'HVAC', 'NORMAL', 1, NULL, false, true, NOW()),
('VENTILATION_CHECK', 'Ventilation System Check', 'Inspect ventilation fans and ducts', 'HVAC', 'NORMAL', 2, NULL, false, true, NOW()),
('REFRIGERATION_SERVICE', 'Refrigeration Service', 'Service refrigeration units and check refrigerant', 'HVAC', 'HIGH', 3, 'Refrigeration Technician', false, true, NOW()),
('AC_COMPRESSOR_CHECK', 'AC Compressor Inspection', 'Inspect AC compressor for leaks and performance', 'HVAC', 'NORMAL', 2, NULL, false, true, NOW()),

-- GENERAL
('GENERAL_INSPECTION', 'General Inspection', 'General walkthrough inspection of vessel', 'GENERAL', 'NORMAL', 2, NULL, false, true, NOW()),
('CLEANING', 'General Cleaning', 'General cleaning of assigned areas', 'GENERAL', 'NORMAL', 2, NULL, false, true, NOW()),
('RUST_REMOVAL', 'Rust Removal', 'Remove rust and apply rust inhibitor', 'GENERAL', 'NORMAL', 4, NULL, false, true, NOW()),
('LUBRICATION', 'Equipment Lubrication', 'Lubricate moving parts and equipment', 'GENERAL', 'NORMAL', 1, NULL, false, true, NOW()),
('WASTE_DISPOSAL', 'Waste Disposal', 'Proper disposal of waste materials', 'GENERAL', 'NORMAL', 1, NULL, false, true, NOW())

ON CONFLICT ("TypeCode") DO NOTHING;

-- =====================================================
-- 2. CREATE USERS FROM CREW_MEMBERS (if not exists)
-- =====================================================

-- Tạo user cho TẤT CẢ crew members trong bảng crew_members
-- Username = crew_id
-- Password = date_of_birth (format: ddMMyyyy) hoặc "123456" nếu không có date_of_birth
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    cm.crew_id,
    encode(digest(
        COALESCE(
            to_char(cm.date_of_birth, 'DDMMYYYY'),
            '123456'
        ), 'sha256'
    ), 'base64'),
    (SELECT id FROM roles WHERE role_code = 'USER'),
    cm.crew_id,
    true,
    NOW()
FROM crew_members cm
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id);

-- =====================================================
-- 3. VERIFY DATA
-- =====================================================

-- Đếm TaskTypes
SELECT 
    'TaskTypes' as table_name,
    COUNT(*) as total_records,
    COUNT(*) FILTER (WHERE "IsActive" = true) as active_records
FROM "TaskTypes";

-- Đếm Users
SELECT 
    'Users' as table_name,
    COUNT(*) as total_records,
    COUNT(*) FILTER (WHERE is_active = true) as active_records
FROM users;

-- Hiển thị TaskTypes theo Category
SELECT 
    "Category",
    COUNT(*) as count
FROM "TaskTypes"
WHERE "IsActive" = true
GROUP BY "Category"
ORDER BY "Category";

-- Hiển thị Users và default password
SELECT 
    u.username,
    cm.full_name,
    cm.rank,
    cm.position,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password,
    r.role_name,
    u.is_active
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
WHERE u.username != 'admin'
ORDER BY u.username;

-- =====================================================
-- COMPLETED!
-- =====================================================
-- TaskTypes: 33 records inserted
-- Users: Created from all crew_members
-- 
-- NEXT STEPS:
-- 1. Copy script này vào pgAdmin 4
-- 2. Chạy script
-- 3. Restart backend (dotnet run)
-- 4. Test frontend Add Task modal
-- =====================================================
