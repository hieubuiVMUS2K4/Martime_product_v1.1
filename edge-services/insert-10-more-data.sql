-- ==================================================
-- INSERT 10 DỮ LIỆU BỔ SUNG CHO MAINTENANCE VÀ MATERIALS
-- Date: 2025-10-29
-- Cập nhật: Thêm maintenance_tasks, material_categories, material_items
-- ==================================================

BEGIN;

-- ==================================================
-- THÊM 5 MATERIAL CATEGORIES (Danh mục vật tư)
-- ==================================================

INSERT INTO material_categories (
    category_code, name, description,
    parent_category_id, is_active, is_synced, created_at
) VALUES

-- 1. Engine Components (Top level)
('CAT-001', 'Engine Components', 'Main and auxiliary engine parts and components',
 NULL, true, false, NOW()),

-- 2. Deck Equipment (Top level)
('CAT-002', 'Deck Equipment', 'Deck machinery, cargo handling, and mooring equipment',
 NULL, true, false, NOW()),

-- 3. Electrical Equipment (Top level)
('CAT-003', 'Electrical Equipment', 'Electrical systems, automation, and control equipment',
 NULL, true, false, NOW()),

-- 4. Safety & Life-Saving (Top level)
('CAT-004', 'Safety & Life-Saving Equipment', 'SOLAS-required safety equipment and LSA',
 NULL, true, false, NOW()),

-- 5. Hydraulic Systems (Sub-category under Engine Components)
('CAT-001-01', 'Hydraulic Systems', 'Hydraulic pumps, valves, cylinders, and hoses',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-001'), true, false, NOW());

-- ==================================================
-- THÊM 10 MATERIAL ITEMS (Vật tư cụ thể)
-- ==================================================

INSERT INTO material_items (
    item_code, name, category_id, specification,
    unit, on_hand_quantity, min_stock, max_stock,
    reorder_level, reorder_quantity, location,
    manufacturer, supplier, part_number, barcode,
    batch_tracked, serial_tracked, expiry_required,
    unit_cost, currency, notes, is_active, is_synced, created_at
) VALUES

-- 1. Main Engine Piston Rings
('MIT-001', 'Main Engine Piston Ring Set',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-001'),
 'Chrome-plated compression and oil control rings for MAN B&W 6S50MC',
 'SET', 4, 2, 10, 3, 6, 'Engine Spares Store - Shelf A12',
 'MAN Energy Solutions', 'Marine Spares International', 'ME-PR-6S50-001', 'BAR001',
 false, true, false, 850.00, 'USD',
 'Critical spare - order lead time 45 days', true, false, NOW()),

-- 2. Fuel Injection Valves
('MIT-002', 'Fuel Injection Valve Assembly',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-001'),
 'High-pressure fuel injection valve for main engine',
 'PCS', 8, 6, 18, 8, 12, 'Engine Spares Store - Shelf A15',
 'Bosch Marine', 'Marine Spares International', 'BM-FIV-6S50', 'BAR002',
 false, true, false, 650.00, 'USD',
 'Replace during major overhaul', true, false, NOW()),

-- 3. Hydraulic Pump Overhaul Kit
('MIT-003', 'Hydraulic Pump Overhaul Kit',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-001-01'),
 'Complete overhaul kit for steering gear hydraulic pump including seals and bearings',
 'KIT', 1, 1, 3, 1, 2, 'Engine Spares Store - Shelf B08',
 'Vickers', 'Hydraulic Solutions Inc', 'VK-PUMP-KIT-SG', 'BAR003',
 true, false, false, 8500.00, 'USD',
 'Scheduled overhaul every 5 years', true, false, NOW()),

-- 4. Hydraulic Hoses HP
('MIT-004', 'Hydraulic Hose High Pressure 1"',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-001-01'),
 'Reinforced rubber hydraulic hose rated 350 bar',
 'MTR', 45, 20, 100, 25, 50, 'Engine Spares Store - Shelf B12',
 'Parker Hannifin', 'Hydraulic Solutions Inc', 'PKR-HP-25', 'BAR004',
 false, false, false, 85.00, 'USD',
 'Cut to required length', true, false, NOW()),

-- 5. Emergency Generator AVR
('MIT-005', 'Automatic Voltage Regulator',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-003'),
 'AVR for Caterpillar 3306 emergency generator',
 'PCS', 1, 1, 2, 1, 2, 'Electrical Store - Cabinet E3',
 'Caterpillar', 'ElectroMarine Ltd', 'CAT-AVR-3306', 'BAR005',
 false, true, false, 2200.00, 'USD',
 'Critical spare - no substitute', true, false, NOW()),

-- 6. Circuit Breakers 63A
('MIT-006', 'Circuit Breaker 3-Phase 63A',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-003'),
 'Molded case circuit breaker 63A 440V AC 3-phase',
 'PCS', 8, 5, 15, 6, 10, 'Electrical Store - Cabinet E7',
 'ABB', 'ElectroMarine Ltd', 'ABB-CB-63A-440', 'BAR006',
 false, true, false, 180.00, 'USD',
 'For switchboard maintenance', true, false, NOW()),

-- 7. Manila Rope 24mm
('MIT-007', 'Manila Rope 24mm 3-Strand',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-002'),
 'Natural fiber manila rope 24mm diameter, breaking load 5.5 tons',
 'MTR', 350, 100, 500, 150, 300, 'Deck Store - Compartment D4',
 'Robline', 'Maritime Supply Co.', 'MAN-24-3S', 'BAR007',
 false, false, false, 8.50, 'USD',
 'For mooring and towing operations', true, false, NOW()),

-- 8. Steel Wire Rope 22mm
('MIT-008', 'Steel Wire Rope 22mm 6x36 IWRC',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-002'),
 'Galvanized steel wire rope, independent wire rope core',
 'MTR', 250, 150, 500, 200, 300, 'Deck Store - Compartment D8',
 'Chant Engineering', 'Maritime Supply Co.', 'SWR-22-636', 'BAR008',
 false, false, false, 18.00, 'USD',
 'For cargo crane operations', true, false, NOW()),

-- 9. Lifeboat Davit Wire
('MIT-009', 'Lifeboat Davit Wire Rope 16mm',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-004'),
 'Stainless steel wire rope for lifeboat davit, 6x19 construction',
 'MTR', 70, 50, 150, 60, 100, 'Safety Equipment Store - Locker S2',
 'Viking Life-Saving Equipment', 'Safety Marine Equipment', 'VK-LB-WIRE-16', 'BAR009',
 false, false, false, 25.00, 'USD',
 'SOLAS requirement - annual inspection', true, false, NOW()),

-- 10. Fire Hose 50mm
('MIT-010', 'Fire Fighting Hose 50mm x 20m',
 (SELECT id FROM material_categories WHERE category_code = 'CAT-004'),
 'Synthetic rubber-lined fire hose with couplings',
 'PCS', 12, 10, 20, 12, 15, 'Safety Equipment Store - Locker S5',
 'Angus Fire', 'Emergency Marine Supply', 'AF-HOSE-50', 'BAR010',
 false, false, false, 250.00, 'USD',
 'Pressure tested annually per SOLAS', true, false, NOW());

-- ==================================================
-- THÊM 10 MAINTENANCE TASKS (Nhiệm vụ bảo trì)
-- ==================================================

INSERT INTO maintenance_tasks (
    task_id, equipment_id, equipment_name,
    task_type, task_description,
    interval_hours, interval_days, last_done_at, next_due_at,
    running_hours_at_last_done, priority, status, assigned_to,
    started_at, completed_at, completed_by,
    notes, spare_parts_used, is_synced, created_at
) VALUES

-- 1. Main Engine Cylinder Head Inspection
('MTK-001', 'ME-6S50MC-001', 'Main Engine - MAN 6S50MC',
 'RUNNING_HOURS', 'Cylinder head inspection and valve clearance adjustment',
 2000, NULL, NOW() - INTERVAL '1850 hours', NOW() + INTERVAL '150 hours',
 18500, 'HIGH', 'PENDING', 'John Smith - Chief Engineer',
 NULL, NULL, NULL,
 'Schedule during port stay. Check for cracks and carbon deposits.',
 NULL, false, NOW()),

-- 2. Steering Gear Monthly Test (SOLAS)
('MTK-002', 'SG-HYD-001', 'Steering Gear - Hydraulic System',
 'CALENDAR', 'Monthly steering gear test per SOLAS Chapter V',
 NULL, 30, NOW() - INTERVAL '28 days', NOW() + INTERVAL '2 days',
 NULL, 'CRITICAL', 'PENDING', 'Carlos Garcia - Second Engineer',
 NULL, NULL, NULL,
 'Mandatory SOLAS requirement. Test both pumps and emergency system.',
 NULL, false, NOW()),

-- 3. Emergency Generator Load Test
('MTK-003', 'EG-CAT-001', 'Emergency Generator - Caterpillar 3306',
 'CALENDAR', 'Weekly load test 100% capacity for 1 hour',
 NULL, 7, NOW() - INTERVAL '6 days', NOW() + INTERVAL '1 day',
 NULL, 'HIGH', 'PENDING', 'Mike Johnson - Electrician',
 NULL, NULL, NULL,
 'SOLAS requirement. Check voltage, frequency, and temperature.',
 NULL, false, NOW()),

-- 4. Main Engine Fuel Pump Overhaul
('MTK-004', 'ME-FP-001', 'Main Engine Fuel Injection Pump',
 'RUNNING_HOURS', 'Fuel pump complete overhaul and calibration',
 4000, NULL, NOW() - INTERVAL '3950 hours', NOW() + INTERVAL '50 hours',
 36000, 'NORMAL', 'IN_PROGRESS', 'Ahmed Hassan - Third Engineer',
 NOW() - INTERVAL '2 days', NULL, NULL,
 'Maker''s recommended interval. Send calibration report to classification society.',
 'Fuel pump overhaul kit, calibration shims', false, NOW()),

-- 5. Air Compressor Safety Valve Test
('MTK-005', 'AC-001', 'Main Air Compressor #1',
 'CALENDAR', 'Safety valve pressure test and certification',
 NULL, 180, NOW() - INTERVAL '178 days', NOW() + INTERVAL '2 days',
 NULL, 'HIGH', 'PENDING', 'Carlos Garcia - Second Engineer',
 NULL, NULL, NULL,
 'Mandatory 6-month test. Must be witnessed by Chief Engineer.',
 NULL, false, NOW()),

-- 6. Fresh Water Generator Descaling
('MTK-006', 'FWG-001', 'Fresh Water Generator',
 'RUNNING_HOURS', 'Evaporator descaling and gasket replacement',
 1500, NULL, NOW() - INTERVAL '1480 hours', NOW() + INTERVAL '20 hours',
 12000, 'NORMAL', 'PENDING', 'David Wilson - Fourth Engineer',
 NULL, NULL, NULL,
 'Heavy scale buildup observed. Check TDS settings after cleaning.',
 'Descaling chemical, evaporator gaskets', false, NOW()),

-- 7. Lifeboat Engine Monthly Start
('MTK-007', 'LB-ENG-001', 'Lifeboat #1 Engine - Yanmar',
 'CALENDAR', 'Monthly lifeboat engine start test per SOLAS',
 NULL, 30, NOW() - INTERVAL '29 days', NOW() + INTERVAL '1 day',
 NULL, 'CRITICAL', 'PENDING', 'Robert Brown - Chief Officer',
 NULL, NULL, NULL,
 'SOLAS mandatory requirement. Run for minimum 3 minutes. Log in ORB.',
 NULL, false, NOW()),

-- 8. Cargo Crane Wire Rope Inspection
('MTK-008', 'CR-001', 'Cargo Crane #1',
 'CALENDAR', 'Wire rope inspection and lubrication',
 NULL, 90, NOW() - INTERVAL '85 days', NOW() + INTERVAL '5 days',
 NULL, 'NORMAL', 'PENDING', 'Robert Brown - Chief Officer',
 NULL, NULL, NULL,
 'Quarterly inspection per class requirements. Check for broken wires.',
 'Wire rope lubricant', false, NOW()),

-- 9. Incinerator Refractory Repair (CONDITION-BASED)
('MTK-009', 'INC-001', 'Incinerator',
 'CONDITION', 'Refractory lining repair due to cracks found',
 NULL, NULL, NULL, NOW() + INTERVAL '3 days',
 NULL, 'NORMAL', 'OVERDUE', 'Carlos Garcia - Second Engineer',
 NULL, NULL, NULL,
 'Cracks found during inspection. Schedule during next port stay.',
 'Refractory cement 25kg, refractory bricks', false, NOW() - INTERVAL '2 days'),

-- 10. Main Engine Turbocharger Cleaning
('MTK-010', 'ME-TC-001', 'Main Engine Turbocharger',
 'RUNNING_HOURS', 'Turbocharger air side cleaning and inspection',
 1000, NULL, NOW() - INTERVAL '950 hours', NOW() + INTERVAL '50 hours',
 37000, 'HIGH', 'PENDING', 'John Smith - Chief Engineer',
 NULL, NULL, NULL,
 'Exhaust temperature deviation indicates fouling. Clean compressor and turbine blades.',
 'Turbo cleaning chemical, gaskets', false, NOW());

COMMIT;

-- ==================================================
-- VERIFICATION QUERIES (Kiểm tra dữ liệu)
-- ==================================================

-- 1. Kiểm tra số lượng material categories
SELECT COUNT(*) as total_material_categories FROM material_categories;

-- 2. Kiểm tra số lượng material items
SELECT COUNT(*) as total_material_items FROM material_items;

-- 3. Kiểm tra số lượng maintenance tasks
SELECT COUNT(*) as total_maintenance_tasks FROM maintenance_tasks;

-- 4. Hiển thị danh sách material categories
SELECT category_code, name, description, is_active
FROM material_categories
ORDER BY category_code;

-- 5. Hiển thị danh sách material items
SELECT item_code, name, unit, on_hand_quantity, min_stock, location
FROM material_items
ORDER BY item_code;

-- 6. Hiển thị danh sách maintenance tasks mới
SELECT task_id, equipment_name, task_type, status, priority, next_due_at
FROM maintenance_tasks
WHERE task_id LIKE 'MTK-%'
ORDER BY next_due_at;

-- 7. Kiểm tra material items theo category
SELECT 
    mc.category_code,
    mc.name as category_name,
    COUNT(mi.id) as item_count
FROM material_categories mc
LEFT JOIN material_items mi ON mc.id = mi.category_id
GROUP BY mc.id, mc.category_code, mc.name
ORDER BY mc.category_code;

-- 8. Kiểm tra maintenance tasks theo status
SELECT status, COUNT(*) as task_count
FROM maintenance_tasks
WHERE task_id LIKE 'MTK-%'
GROUP BY status
ORDER BY status;

-- 9. Danh sách maintenance tasks sắp đến hạn (trong 7 ngày tới)
SELECT task_id, equipment_name, task_description, 
       next_due_at, status, priority, assigned_to
FROM maintenance_tasks
WHERE task_id LIKE 'MTK-%'
  AND next_due_at BETWEEN NOW() AND NOW() + INTERVAL '7 days'
ORDER BY next_due_at;

-- 10. Danh sách material items cần order (dưới mức tối thiểu)
SELECT item_code, name, on_hand_quantity, min_stock, 
       (min_stock - on_hand_quantity) as shortage,
       location, supplier
FROM material_items
WHERE on_hand_quantity < min_stock
ORDER BY (min_stock - on_hand_quantity) DESC;

