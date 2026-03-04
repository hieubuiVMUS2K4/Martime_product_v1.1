-- ============================================================
-- MARITIME REPORTING SYSTEM - REPORT TYPES SEED DATA
-- IMO/SOLAS/MARPOL Compliant Report Types
-- ============================================================

-- Clear existing data (optional - uncomment if needed)
-- DELETE FROM report_types;

-- Insert 5 standard maritime report types
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
) VALUES 
-- 1. NOON REPORT (Most important daily report)
(
    'NOON',
    'Noon Report',
    'OPERATIONAL',
    'Daily position and performance report submitted at 12:00 Local Time. Reports vessel position, weather conditions, fuel consumption, speed, distance covered, and engine performance. Required for DCS (Data Collection System) compliance.',
    'SOLAS Chapter V Regulation 19',
    'DAILY',
    true,
    true,
    true,
    CURRENT_TIMESTAMP
),

-- 2. DEPARTURE REPORT
(
    'DEPARTURE',
    'Departure Report',
    'VOYAGE',
    'Report submitted when vessel departs from port. Includes departure time, bunker quantities (ROB), crew list, cargo details, draft readings, and voyage plan to next port. Required for SOLAS compliance and company monitoring.',
    'SOLAS Chapter V Regulation 28',
    'VOYAGE_BASED',
    true,
    true,
    true,
    CURRENT_TIMESTAMP
),

-- 3. ARRIVAL REPORT
(
    'ARRIVAL',
    'Arrival Report',
    'VOYAGE',
    'Report submitted when vessel arrives at port. Includes arrival time, voyage statistics (total distance, average speed), fuel consumption summary, ROB on arrival, and any incidents during voyage. Completes the voyage cycle.',
    'SOLAS Chapter V Regulation 28',
    'VOYAGE_BASED',
    true,
    true,
    true,
    CURRENT_TIMESTAMP
),

-- 4. BUNKER DELIVERY NOTE (BDN) REPORT
(
    'BUNKER',
    'Bunker Delivery Note Report',
    'COMPLIANCE',
    'Report submitted when vessel receives bunker fuel. Records fuel type, quantity, supplier, sulfur content, and BDN details. Critical for MARPOL Annex VI compliance (fuel sulfur limits) and IMO DCS (fuel consumption reporting). Must retain BDN onboard for 3 years.',
    'MARPOL Annex VI Regulation 18',
    'EVENT_BASED',
    true,
    true,
    true,
    CURRENT_TIMESTAMP
),

-- 5. POSITION REPORT
(
    'POSITION',
    'Position Report',
    'OPERATIONAL',
    'Interim position report submitted at regular intervals or upon request. Contains vessel position, course, speed, weather, and ETA to next port. Used for vessel tracking, piracy monitoring, and emergency response coordination.',
    'SOLAS Chapter V Regulation 19',
    'EVENT_BASED',
    false,
    false,
    true,
    CURRENT_TIMESTAMP
);

-- Verification query
SELECT 
    id,
    type_code,
    type_name,
    category,
    frequency,
    is_mandatory,
    requires_master_signature,
    created_at
FROM report_types
ORDER BY id;

-- Count total report types
SELECT COUNT(*) as total_report_types FROM report_types;
