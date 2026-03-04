-- ===================================================================
-- SEED DATA: Drill Types from Excel "Drill Training Schedule (1)"
-- Bulk carrier equipped with Free-Fall Lifeboat
-- Total: 36+ drill types across 5 categories
-- ===================================================================

-- DELETE FROM drill_logs;
-- DELETE FROM drill_schedules;
-- DELETE FROM drill_types;

-- ===================================================================
-- CATEGORY 1: STATION DRILLS (14 items)
-- ===================================================================

-- 1. Abandon ship drill
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", 
    "InstructionContent", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ABANDON_SHIP_MONTHLY', 'Abandon ship drill (SOLAS III 19.3.2 & 19.3.4 / except 19.3.4.1 & 5)', 
 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.4 / except 19.3.4.1 & 5', 'At least once every month', 
 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'MASTER', 1, true, true, true,
 '<p><strong>ABANDON SHIP DRILL REQUIREMENTS (SOLAS III/19):</strong></p>
<ul>
<li>Each lifeboat shall be launched with its assigned crew aboard and maneuvered in the water at least once every three months</li>
<li>Emergency lighting for mustering and abandonment shall be tested at each abandon ship drill</li>
<li>All crew members must report to muster stations with lifejackets on</li>
<li>Check lifeboat equipment, provisions, and emergency position-indicating radio beacon (EPIRB)</li>
</ul>', 
 NOW(), NOW());

-- *1 trigger: Within 24 hours after departure if >25% crew change
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", 
    "InstructionContent", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ABANDON_SHIP_ON_DEPARTURE', 'Abandon ship drill - Within 24 hours after departure *1', 
 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.4 / except 19.3.4.1 & 5', 'Within 24 hours after departure *1', 
 'ON_EVENT', NULL, 'ON_DEPARTURE', 1, 0, 'MASTER', 2, true, true, false,
 '<p><strong>SPECIAL TRIGGER RULE (*1):</strong></p>
<p>If more than 25% of the crew members have turned over their duties (This requirement shall be applied also to the newly delivered vessel)</p>
<p>This drill must be conducted within 24 hours of departure.</p>', 
 NOW(), NOW());

-- 2. Rescue boat drill
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'RESCUE_BOAT_QUARTERLY', 'Rescue boat drill (Launching & maneuvering)', 
 'STATION_DRILLS', 'SOLAS III 19.3.4.6', 'Each month (at least once every three (3) months in all cases)', 
 'MONTHLY', 30, 7, 'CHIEF_OFFICER', 3, true, true, true, NOW(), NOW());

-- 3. Fire-fighting drill
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", 
    "InstructionContent", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FIRE_DRILL_MONTHLY', 'Fire-fighting drill (SOLAS III 19.3.2 & 19.3.2)', 
 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.2', 'At least once every month', 
 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', 4, true, true, true,
 '<p><strong>FIRE DRILL REQUIREMENTS (SOLAS II-2/15.2.2.5):</strong></p>
<ul>
<li>Various emergencies should be trained: fire in engineroom, accommodation, galley, cargohold, paintstore, etc.</li>
<li>Each fire drill should include: reporting to musterlist, starting firepump with at least 2 hoses, checking firemen outfit, practicing actual firefighting</li>
<li>EVERY DRILL REPORT NEEDS TO BE APPROVED BY THE MASTER BEFORE SIGNING OFF</li>
</ul>', 
 NOW(), NOW());

-- Fire drill *1 trigger
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FIRE_DRILL_ON_DEPARTURE', 'Fire-fighting drill - Within 24 hours after departure *1', 
 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.2', 'Within 24 hours after departure *1', 
 'ON_EVENT', NULL, 'ON_DEPARTURE', 1, 0, 'CHIEF_OFFICER', 5, true, true, false, NOW(), NOW());

-- 4. Flood-fighting drill (Japanese regulation)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FLOOD_FIGHTING_MONTHLY', 'Flood-fighting drill (Japanese regulation)', 
 'STATION_DRILLS', 'Japanese regulation', 'At least once every month', 
 'MONTHLY', 30, 7, 'CHIEF_OFFICER', 6, true, true, true, NOW(), NOW());

-- 5. Enclosed space entry and rescue drill
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ENCLOSED_SPACE_BIMONTHLY', 'Enclosed space entry and rescue drill', 
 'STATION_DRILLS', 'SOLAS III 19.3.3 & 19.3.6', 'At least once every two (2) months', 
 'BI_MONTHLY', 60, 7, 'CHIEF_OFFICER', 7, true, true, true, NOW(), NOW());

-- 6. Abandon ship drill (Free-Fall or simulated launching)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ABANDON_SHIP_FREEFALL_QUARTERLY', 'Abandon ship drill (Free-Fall or simulated launching)', 
 'STATION_DRILLS', 'SOLAS III 19.3.4.4', 'At least once every three (3) months', 
 'QUARTERLY', 90, 7, 'MASTER', 8, true, true, true, NOW(), NOW());

-- 7. Discharged oil removal drill (SOPEP)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'SOPEP_QUARTERLY', 'Discharged oil removal drill (SOPEP 5.6.2)', 
 'STATION_DRILLS', 'SOPEP 5.6.2', 'At least once every three (3) months', 
 'QUARTERLY', 90, 7, 'CHIEF_ENGINEER', 9, true, true, true, NOW(), NOW());

-- 8. Emergency steering drill
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'EMERGENCY_STEERING_QUARTERLY', 'Emergency steering drill (SOLAS V 26.4)', 
 'STATION_DRILLS', 'SOLAS V 26.4', 'At least once every three (3) months', 
 'QUARTERLY', 90, 7, 'CHIEF_OFFICER', 10, true, true, true, NOW(), NOW());

-- 9. Abandon ship drill (Free-Fall or simulated launching) - Semi-annual
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ABANDON_SHIP_FREEFALL_SEMIANNUAL', 'Abandon ship drill (Free-Fall or simulated launching) - Not more than six (6) months', 
 'STATION_DRILLS', 'SOLAS III 19.3.4.1', 'Not more than six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'MASTER', 11, true, true, true, NOW(), NOW());

-- 10-14. Six-monthly drills
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'RECOVERY_PERSONS_WATER_SEMIANNUAL', 'Recovery of Persons from the Water Drill (MSC.1/Circ.1447)', 
 'STATION_DRILLS', 'MSC.1/Circ.1447', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_OFFICER', 12, true, true, true, NOW(), NOW()),
 
(gen_random_uuid(), 'COLLISION_GROUNDING_SEMIANNUAL', 'Collision, Grounding, Heavy weather damage & Structural failure drill (VIQ 5.13)', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'MASTER', 13, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'SERIOUS_INJURY_SEMIANNUAL', 'Serious injury drill (VIQ 5.13)', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_OFFICER', 14, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'BLACKOUT_SEMIANNUAL', 'Blackout drill (Critical machinery failure/VIQ 5.13))', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_ENGINEER', 15, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'ENGINE_EMERGENCY_SEMIANNUAL', 'Main engine emergency operation drill (Critical machinery failure/VIQ 5.13))', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_ENGINEER', 16, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'EXPLOSION_SEMIANNUAL', 'Explosion drill (VIQ 5.13)', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'MASTER', 17, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'HELICOPTER_SEMIANNUAL', 'Helicopter operation drill (VIQ 5.13)', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_OFFICER', 18, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'EMERGENCY_TOWING_SEMIANNUAL', 'Emergency Towing Equipment drill (VIQ 5.13)', 
 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 
 'SEMI_ANNUAL', 180, 14, 'CHIEF_OFFICER', 19, true, true, true, NOW(), NOW());

-- ===================================================================
-- CATEGORY 2: EXERCISES (2 items)
-- ===================================================================

INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'EMERGENCY_RESPONSE_ANNUAL', 'Emergency response drill (Flight ship)', 
 'EXERCISES', 'N/A', 'Once a year', 
 'ANNUAL', 365, 30, 'MASTER', 20, true, true, true, NOW(), NOW()),

(gen_random_uuid(), 'CYBER_SECURITY_TRIANNUAL', 'Cyber security drill', 
 'EXERCISES', 'Various', 'At least three (3) times each calendar year', 
 'QUARTERLY', 120, 14, 'MASTER', 21, true, true, true, NOW(), NOW());

-- ===================================================================
-- CATEGORY 3: EDUCATION (2 items) - Renamed from "Training" in Excel to avoid confusion
-- ===================================================================

-- Method of using Life-saving and Fire extinguishing equipment
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", 
    "InstructionContent", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'LIFESAVING_FIRE_EQUIPMENT_TRAINING_MONTHLY', 'Method of using Life-saving and Fire extinguishing equipment (SOLAS III 19.4.1)', 
 'EDUCATION', 'SOLAS III 19.4.1', 'At least once every month', 
 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', 22, true, true, true,
 '<p><strong>TRAINING REQUIREMENTS (SOLAS III/19.4):</strong></p>
<ul>
<li>Within 2 weeks after new crew members join the ship</li>
<li>Proper use of lifejackets, immersion suits, lifebuoys</li>
<li>Launching procedures for lifeboats and rescue boats</li>
<li>Use of portable fire extinguishers and fire hoses</li>
</ul>', 
 NOW(), NOW());

-- Trigger: Within 2 weeks after crewmembers join the ship
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'LIFESAVING_FIRE_EQUIPMENT_NEW_CREW', 'Method of using Life-saving and Fire extinguishing equipment - Within 2 weeks after new crew join', 
 'EDUCATION', 'SOLAS III 19.4.1', 'Within 2 weeks after crewmembers join the ship', 
 'ON_EVENT', NULL, 'ON_NEW_CREW', 14, 3, 'CHIEF_OFFICER', 23, true, true, false, NOW(), NOW());

-- Survival at sea
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'SURVIVAL_AT_SEA_MONTHLY', 'Survival at sea (SOLAS III 19.4.1)', 
 'EDUCATION', 'SOLAS III 19.4.1', 'At least once every month', 
 'MONTHLY', 30, 7, 'CHIEF_OFFICER', 24, true, true, true, NOW(), NOW());

-- ===================================================================
-- CATEGORY 4: TRAINING (2 items)
-- ===================================================================

INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FAMILIARIZATION_ON_JOINING', 'Familiarization training (STCW A-VI/1, 1.1-1.7)', 
 'TRAINING', 'STCW A-VI/1, 1.1-1.7', 'Upon joining the ship', 
 'ON_EVENT', NULL, 'ON_NEW_CREW', 1, 0, 'MASTER', 25, true, true, false, NOW(), NOW()),

(gen_random_uuid(), 'LIFESAVING_FIRE_EQUIPMENT_TRAINING_NEW_CREW', 'Method of using lifesaving and fire extinguishing equipment (SOLAS III 19.4.1)', 
 'TRAINING', 'SOLAS III 19.4.1', 'Within 2 weeks after crewmembers join the ship', 
 'ON_EVENT', NULL, 'ON_NEW_CREW', 14, 3, 'CHIEF_OFFICER', 26, true, true, false, NOW(), NOW());

-- Method of using Davit-launched life-raft launching appliances
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'DAVIT_LIFERAFT_BALLAST', 'Method of using Davit-launched life-raft launching appliances (SOLAS III 19.3.3)', 
 'TRAINING', 'SOLAS III 19.3.3', 'Not more than four (4) months', 
 'QUARTERLY', 120, 14, 'CHIEF_OFFICER', 27, true, true, true, NOW(), NOW());

-- ===================================================================
-- CATEGORY 5: CHECKS (3 items)
-- ===================================================================

-- Emergency Fire Pump check
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FIRE_PUMP_BALLAST_CHECK', 'Emergency Fire Pump (Vacuum Pump) (SOLAS II-2,14.2.2.1)', 
 'CHECKS', 'SOLAS II-2,14.2.2.1', 'At least once on vessel under the ballast passage', 
 'ON_EVENT', NULL, 'ON_BALLAST', 7, 3, 'CHIEF_ENGINEER', 28, true, true, false, NOW(), NOW());

-- Vacuum Pump overhaul (5 years)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'VACUUM_PUMP_OVERHAUL_5YEAR', 'Overhaul inspections of Vacuum Pump (NK Guidance B3.2.3)', 
 'CHECKS', 'NK Guidance B3.2.3', 'Every 5 years', 
 'FIVE_YEARS', 1825, 90, 'CHIEF_ENGINEER', 29, true, true, true, NOW(), NOW());

-- Fire Line and Fire Hoses check
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'FIRE_LINE_HOSES_ANNUAL', 'Fire Line and Fire Hoses (SOLAS II-2,14.2.2.1)', 
 'CHECKS', 'SOLAS II-2,14.2.2.1', 'Once a year', 
 'ANNUAL', 365, 30, 'CHIEF_ENGINEER', 30, true, true, true, NOW(), NOW());

-- ===================================================================
-- CATEGORY 6: ISPS (2 items)
-- ===================================================================

-- Security Drill (3 times per year)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", 
    "InstructionContent", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ISPS_SECURITY_DRILL_TRIANNUAL', 'Security Drill (ISPS Code Part B 13.6)', 
 'ISPS', 'ISPS Code Part B 13.6', 'At least three (3) times each calendar year', 
 'QUARTERLY', 120, 'ROUTINE', NULL, 14, 'MASTER', 31, true, true, true,
 '<p><strong>ISPS SECURITY DRILL REQUIREMENTS:</strong></p>
<ul>
<li>At least three times each calendar year with intervals not exceeding 18 months</li>
<li>Drill to be held by company without any advanced notice</li>
<li>Test ship security alert system</li>
<li>Check restricted area access control</li>
</ul>', 
 NOW(), NOW());

-- Trigger: Within one week of the change
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "TriggerCondition", "TriggerWithinDays", "WarningDaysBefore", 
    "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ISPS_SECURITY_DRILL_NEW_CREW', 'Security Drill - Within one week of the change *1', 
 'ISPS', 'ISPS Code Part B 13.6', 'Within one week of the change *1', 
 'ON_EVENT', NULL, 'ON_NEW_CREW', 7, 1, 'MASTER', 32, true, true, false, NOW(), NOW());

-- Security Exercise (annual, no more than 18 months interval)
INSERT INTO "DrillTypes" ("Id", "DrillCode", "DrillName", "Category", "RegulationSource", "RegulationPeriod", 
    "FrequencyType", "FrequencyDays", "WarningDaysBefore", "AssignedToRole", "DisplayOrder", "IsActive", "IsMandatory", "IsFixedInterval", "CreatedAt", "UpdatedAt")
VALUES 
(gen_random_uuid(), 'ISPS_SECURITY_EXERCISE_ANNUAL', 'Exercise (ISPS Code Part B 13.7)', 
 'ISPS', 'ISPS Code Part B 13.7', 'At least each calendar year with no more than 18 months', 
 'ANNUAL', 365, 60, 'MASTER', 33, true, true, true, NOW(), NOW());

-- ===================================================================
-- VERIFICATION QUERY
-- ===================================================================

SELECT 
    "Category",
    COUNT(*) as "DrillCount"
FROM "DrillTypes"
GROUP BY "Category"
ORDER BY "Category";

-- Expected results:
-- STATION_DRILLS: 18 (14 + 4 triggers)
-- EXERCISES: 2
-- EDUCATION: 3 (2 + 1 trigger)
-- TRAINING: 3
-- CHECKS: 3
-- ISPS: 3 (2 + 1 trigger)
-- TOTAL: 32 drill types
