-- =====================================================
-- SEED REPORT TYPES FOR MARITIME REPORTING MODULE
-- IMO/SOLAS/MARPOL Compliant Report Types
-- Run this script in pgAdmin or database client
-- =====================================================

-- First, check if table exists
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT FROM pg_tables WHERE tablename = 'report_types') THEN
        RAISE NOTICE 'Table report_types does not exist. Please run migrations first.';
        RETURN;
    END IF;
END $$;

-- =====================================================
-- INSERT REPORT TYPES
-- =====================================================

INSERT INTO report_types (
    type_code, 
    type_name, 
    category, 
    description, 
    regulation_reference,
    frequency, 
    is_mandatory, 
    requires_master_signature,
    is_active, 
    created_at
)
VALUES 
-- OPERATIONAL REPORTS
(
    'NOON', 
    'Noon Report', 
    'OPERATIONAL',
    'Daily noon position report with weather, fuel consumption, and voyage progress',
    'SOLAS V/28 - Ship Reporting Systems',
    'DAILY',
    true,
    true,
    true,
    NOW()
),
(
    'DEPARTURE', 
    'Departure Report', 
    'OPERATIONAL',
    'Report filed upon departure from port including bunkers, cargo, and crew',
    'SOLAS V/28 - Ship Reporting Systems',
    'VOYAGE',
    true,
    true,
    true,
    NOW()
),
(
    'ARRIVAL', 
    'Arrival Report', 
    'OPERATIONAL',
    'Report filed upon arrival at port including voyage summary and cargo status',
    'SOLAS V/28 - Ship Reporting Systems',
    'VOYAGE',
    true,
    true,
    true,
    NOW()
),
(
    'BUNKER', 
    'Bunker Delivery Report', 
    'OPERATIONAL',
    'Report of bunker fuel delivery with BDN (Bunker Delivery Note) details',
    'MARPOL Annex VI - Regulation 18',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),
(
    'POSITION', 
    'Position Report', 
    'OPERATIONAL',
    'Ship position report for shore monitoring and fleet tracking',
    'SOLAS V/19 - Carriage requirements',
    'EVENT_BASED',
    false,
    false,
    true,
    NOW()
),

-- COMPLIANCE REPORTS
(
    'IOPP', 
    'Oil Record Book Entry', 
    'COMPLIANCE',
    'MARPOL Annex I Oil Record Book entries for oil operations',
    'MARPOL Annex I - Oil Pollution Prevention',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),
(
    'GARBAGE', 
    'Garbage Record Book Entry', 
    'COMPLIANCE',
    'MARPOL Annex V Garbage Record Book entries',
    'MARPOL Annex V - Garbage Pollution Prevention',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),
(
    'BALLAST', 
    'Ballast Water Report', 
    'COMPLIANCE',
    'Ballast water management record as per BWM Convention',
    'BWM Convention - Ballast Water Management',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),

-- SAFETY REPORTS
(
    'SAFETY_DRILL', 
    'Safety Drill Report', 
    'SAFETY',
    'Record of safety drills conducted (fire, abandon ship, etc.)',
    'SOLAS III - Life-saving Appliances',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),
(
    'INCIDENT', 
    'Incident Report', 
    'SAFETY',
    'Report of accidents, near-misses, or safety incidents',
    'ISM Code - Safety Management System',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),
(
    'SECURITY', 
    'Security Report', 
    'SAFETY',
    'Security-related reports as per ISPS Code',
    'ISPS Code - Ship Security',
    'EVENT_BASED',
    true,
    true,
    true,
    NOW()
),

-- ENVIRONMENTAL REPORTS
(
    'EMISSIONS', 
    'Emissions Report', 
    'ENVIRONMENTAL',
    'Air emission data for IMO DCS compliance',
    'MARPOL Annex VI - Air Pollution',
    'ANNUAL',
    true,
    true,
    true,
    NOW()
),
(
    'CII', 
    'CII Report', 
    'ENVIRONMENTAL',
    'Carbon Intensity Indicator report for EEXI/CII compliance',
    'MARPOL Annex VI - EEXI/CII',
    'ANNUAL',
    true,
    true,
    true,
    NOW()
),

-- MAINTENANCE REPORTS
(
    'MAINTENANCE', 
    'Maintenance Report', 
    'MAINTENANCE',
    'Summary of maintenance activities performed',
    'ISM Code - Planned Maintenance System',
    'MONTHLY',
    false,
    false,
    true,
    NOW()
),
(
    'DEFECT', 
    'Defect Report', 
    'MAINTENANCE',
    'Report of equipment defects or failures',
    'ISM Code - Non-conformity Reporting',
    'EVENT_BASED',
    true,
    false,
    true,
    NOW()
)

ON CONFLICT (type_code) DO UPDATE SET
    type_name = EXCLUDED.type_name,
    category = EXCLUDED.category,
    description = EXCLUDED.description,
    regulation_reference = EXCLUDED.regulation_reference,
    frequency = EXCLUDED.frequency,
    is_mandatory = EXCLUDED.is_mandatory,
    requires_master_signature = EXCLUDED.requires_master_signature,
    is_active = EXCLUDED.is_active;

-- =====================================================
-- VERIFY DATA
-- =====================================================

SELECT 
    'report_types' as table_name,
    COUNT(*) as total_records,
    COUNT(*) FILTER (WHERE is_active = true) as active_records,
    COUNT(*) FILTER (WHERE is_mandatory = true) as mandatory_records
FROM report_types;

-- Show all report types
SELECT 
    id,
    type_code,
    type_name,
    category,
    frequency,
    is_mandatory,
    is_active
FROM report_types
ORDER BY category, type_code;
