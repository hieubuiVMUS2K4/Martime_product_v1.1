-- Insert sample TaskTypes for Maritime Maintenance System
-- This script adds common maintenance task types used on ships

-- Engine-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('ENGINE_OVERHAUL', 'Engine Overhaul', 'Complete overhaul of main engine including inspection of all major components', 'ENGINE', 'CRITICAL', 48, 'Marine Engineer License', true, true, NOW()),
('ENGINE_OIL_CHANGE', 'Engine Oil Change', 'Change engine oil and oil filter', 'ENGINE', 'NORMAL', 2, NULL, false, true, NOW()),
('ENGINE_COOLING_CHECK', 'Cooling System Check', 'Inspect cooling system, check coolant level and condition', 'ENGINE', 'NORMAL', 1, NULL, false, true, NOW()),
('ENGINE_FUEL_FILTER', 'Fuel Filter Replacement', 'Replace fuel filters and bleed fuel system', 'ENGINE', 'HIGH', 2, NULL, false, true, NOW()),
('TURBO_INSPECTION', 'Turbocharger Inspection', 'Inspect and clean turbocharger', 'ENGINE', 'HIGH', 4, 'Marine Engineer License', false, true, NOW());

-- Deck-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('DECK_WASH', 'Deck Washing', 'Wash and clean deck area', 'DECK', 'NORMAL', 2, NULL, false, true, NOW()),
('HULL_INSPECTION', 'Hull Inspection', 'Inspect hull for damage, corrosion, and fouling', 'DECK', 'HIGH', 3, 'Diving Certificate', false, true, NOW()),
('MOORING_CHECK', 'Mooring Equipment Check', 'Inspect mooring lines, winches, and fairleads', 'DECK', 'NORMAL', 2, NULL, false, true, NOW()),
('ANCHOR_MAINTENANCE', 'Anchor System Maintenance', 'Inspect and lubricate anchor windlass', 'DECK', 'NORMAL', 3, NULL, false, true, NOW()),
('PAINT_TOUCH_UP', 'Paint Touch-up', 'Touch up paint on deck and superstructure', 'DECK', 'LOW', 4, NULL, false, true, NOW());

-- Safety-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('LIFEBOAT_DRILL', 'Lifeboat Drill', 'Conduct lifeboat launching drill and inspection', 'SAFETY', 'CRITICAL', 2, 'Safety Officer', true, true, NOW()),
('FIRE_EXTINGUISHER_CHECK', 'Fire Extinguisher Inspection', 'Inspect all fire extinguishers for pressure and damage', 'SAFETY', 'CRITICAL', 1, NULL, false, true, NOW()),
('EMERGENCY_LIGHT_TEST', 'Emergency Lighting Test', 'Test emergency lighting system', 'SAFETY', 'HIGH', 1, NULL, false, true, NOW()),
('LIFEJACKET_INSPECTION', 'Life Jacket Inspection', 'Inspect all life jackets for damage and proper storage', 'SAFETY', 'HIGH', 2, NULL, false, true, NOW()),
('SAFETY_EQUIPMENT_CHECK', 'Safety Equipment Check', 'General inspection of all safety equipment', 'SAFETY', 'CRITICAL', 3, 'Safety Officer', false, true, NOW());

-- Electrical-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('GENERATOR_MAINTENANCE', 'Generator Maintenance', 'Service and inspect emergency generator', 'ELECTRICAL', 'HIGH', 4, 'Electrical Officer', false, true, NOW()),
('BATTERY_CHECK', 'Battery Inspection', 'Check battery condition, voltage, and electrolyte level', 'ELECTRICAL', 'NORMAL', 1, NULL, false, true, NOW()),
('LIGHTING_INSPECTION', 'Navigation Light Inspection', 'Test all navigation lights and replace failed bulbs', 'ELECTRICAL', 'CRITICAL', 1, NULL, false, true, NOW()),
('ELECTRICAL_PANEL_CHECK', 'Electrical Panel Inspection', 'Inspect electrical panels for loose connections and corrosion', 'ELECTRICAL', 'HIGH', 2, 'Electrical Officer', false, true, NOW()),
('UPS_TEST', 'UPS System Test', 'Test uninterruptible power supply system', 'ELECTRICAL', 'HIGH', 1, NULL, false, true, NOW());

-- Navigation-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('RADAR_CALIBRATION', 'Radar Calibration', 'Calibrate and test radar equipment', 'NAVIGATION', 'HIGH', 2, 'Navigation Officer', false, true, NOW()),
('GPS_CHECK', 'GPS System Check', 'Verify GPS accuracy and update charts', 'NAVIGATION', 'HIGH', 1, 'Navigation Officer', false, true, NOW()),
('COMPASS_ADJUSTMENT', 'Compass Adjustment', 'Check and adjust magnetic compass', 'NAVIGATION', 'NORMAL', 2, 'Navigation Officer', false, true, NOW()),
('AUTOPILOT_TEST', 'Autopilot System Test', 'Test autopilot functionality and calibration', 'NAVIGATION', 'HIGH', 1, 'Navigation Officer', false, true, NOW()),
('CHART_UPDATE', 'Chart Update', 'Update electronic and paper charts', 'NAVIGATION', 'NORMAL', 2, 'Navigation Officer', false, true, NOW());

-- HVAC-related tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('AC_FILTER_CHANGE', 'AC Filter Replacement', 'Replace air conditioning filters', 'HVAC', 'NORMAL', 1, NULL, false, true, NOW()),
('VENTILATION_CHECK', 'Ventilation System Check', 'Inspect ventilation fans and ducts', 'HVAC', 'NORMAL', 2, NULL, false, true, NOW()),
('REFRIGERATION_SERVICE', 'Refrigeration Service', 'Service refrigeration units and check refrigerant levels', 'HVAC', 'HIGH', 3, 'Refrigeration Technician', false, true, NOW()),
('AC_COMPRESSOR_CHECK', 'AC Compressor Inspection', 'Inspect AC compressor for leaks and performance', 'HVAC', 'NORMAL', 2, NULL, false, true, NOW());

-- General maintenance tasks
INSERT INTO "TaskTypes" ("TypeCode", "TypeName", "Description", "Category", "DefaultPriority", "EstimatedDurationHours", "RequiredCertification", "RequiresApproval", "IsActive", "CreatedAt")
VALUES 
('GENERAL_INSPECTION', 'General Inspection', 'General walkthrough inspection of vessel', 'GENERAL', 'NORMAL', 2, NULL, false, true, NOW()),
('CLEANING', 'General Cleaning', 'General cleaning of assigned areas', 'GENERAL', 'NORMAL', 2, NULL, false, true, NOW()),
('RUST_REMOVAL', 'Rust Removal', 'Remove rust and apply rust inhibitor', 'GENERAL', 'NORMAL', 4, NULL, false, true, NOW()),
('LUBRICATION', 'Equipment Lubrication', 'Lubricate moving parts and equipment', 'GENERAL', 'NORMAL', 1, NULL, false, true, NOW()),
('WASTE_DISPOSAL', 'Waste Disposal', 'Proper disposal of waste materials', 'GENERAL', 'NORMAL', 1, NULL, false, true, NOW());

PRINT 'Successfully inserted 37 TaskTypes into the database';
