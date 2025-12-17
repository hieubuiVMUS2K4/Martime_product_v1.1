-- =====================================================
-- FIX MISSING CHECKLIST TEMPLATES
-- Add checklist items to all schedules to prevent MISSING_CHECKLIST status
-- =====================================================

-- Delete existing checklist templates (clean slate)
DELETE FROM schedule_checklist_templates;

-- Add checklist for Weekly Generator Service (SCH-GEN-WEEKLY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check fuel system and filter for leaks', 1, false),
    ('Inspect coolant level and condition', 2, false),
    ('Test battery voltage and electrolyte', 3, true),
    ('Check oil level and quality', 4, false),
    ('Inspect belts for wear and tension', 5, false),
    ('Test generator output voltage and frequency', 6, true),
    ('Record running hours', 7, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-GEN-WEEKLY';

-- Add checklist for Daily Generator Inspection (SCH-GEN-DAILY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check oil level', 1, false),
    ('Check coolant level', 2, false),
    ('Check fuel level', 3, false),
    ('Inspect for leaks or abnormal noise', 4, false),
    ('Record running hours and parameters', 5, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-GEN-DAILY';

-- Add checklist for Battery Inspection (SCH-BATT-DAILY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Check battery terminals for corrosion', 1, false),
    ('Measure battery voltage', 2, true),
    ('Check electrolyte level (if applicable)', 3, false),
    ('Inspect battery casing for cracks or damage', 4, false),
    ('Test charging system', 5, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-BATT-DAILY';

-- Add checklist for Monthly Turbocharger Service (SCH-TURBO-MONTHLY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect turbocharger for oil leaks', 1, false),
    ('Clean air filter and intake system', 2, false),
    ('Check exhaust gas temperature', 3, true),
    ('Inspect compressor and turbine blades', 4, false),
    ('Check bearing clearances', 5, false),
    ('Lubricate turbocharger bearings', 6, false),
    ('Test boost pressure', 7, true)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-TURBO-MONTHLY';

-- Add checklist for Weekly Lifeboat Drill (SCH-LB-WEEKLY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect lifeboat hull and equipment', 1, false),
    ('Check mooring lines and fittings', 2, false),
    ('Conduct lifeboat drill with crew', 3, false),
    ('Test engine start and running', 4, false),
    ('Check emergency equipment (flares, first aid)', 5, false),
    ('Verify communication equipment', 6, false),
    ('Update drill log and attendance', 7, false)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-LB-WEEKLY';

-- Add checklist for Monthly Fire Fighting Equipment (SCH-FIRE-MONTHLY)
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
CROSS JOIN (VALUES
    ('Inspect all fire extinguishers', 1, false),
    ('Check fire hose and nozzles', 2, false),
    ('Test fire alarm system', 3, false),
    ('Inspect fire doors and seals', 4, false),
    ('Check emergency lighting', 5, false),
    ('Test fire pump operation', 6, false),
    ('Update fire equipment inspection log', 7, false)
) AS item(description, seq, needs_reading)
WHERE s.schedule_code = 'SCH-FIRE-MONTHLY';

-- Add generic checklist for any schedules without specific templates
INSERT INTO schedule_checklist_templates (id, schedule_id, checkpoint_description, sequence_order, requires_reading, created_at)
SELECT 
    gen_random_uuid(),
    s.id,
    item.description,
    item.seq,
    item.needs_reading,
    NOW()
FROM maintenance_schedules s
LEFT JOIN schedule_checklist_templates sct ON s.id = sct.schedule_id
CROSS JOIN (VALUES
    ('Perform visual inspection', 1, false),
    ('Check for leaks or damage', 2, false),
    ('Test operation and functionality', 3, false),
    ('Clean and lubricate as needed', 4, false),
    ('Record readings and observations', 5, true)
) AS item(description, seq, needs_reading)
WHERE sct.schedule_id IS NULL;

-- =====================================================
-- UPDATE EXISTING TASKS FROM MISSING_CHECKLIST TO SCHEDULED/DUE
-- =====================================================

-- Update tasks that now have checklists
UPDATE maintenance_tasks t
SET 
    status = CASE 
        WHEN t.next_due_at::date <= CURRENT_DATE THEN 'DUE'
        WHEN t.next_due_at::date < CURRENT_DATE THEN 'OVERDUE'
        ELSE 'SCHEDULED'
    END,
    updated_at = NOW()
FROM maintenance_schedules s
WHERE t.schedule_id = s.id
  AND t.status IN ('MISSING_CHECKLIST', 'MISSING_PIC', 'MISSING_BOTH')
  AND t.assigned_to IS NOT NULL  -- Has PIC
  AND EXISTS (
      SELECT 1 FROM schedule_checklist_templates sct 
      WHERE sct.schedule_id = s.id
  );

-- =====================================================
-- VERIFICATION
-- =====================================================

-- Count checklist items per schedule
SELECT 
    s.schedule_code,
    s.schedule_name,
    COUNT(sct.id) as checklist_items
FROM maintenance_schedules s
LEFT JOIN schedule_checklist_templates sct ON s.id = sct.schedule_id
GROUP BY s.id, s.schedule_code, s.schedule_name
ORDER BY s.schedule_code;

-- Verify task statuses after update
SELECT 
    status,
    COUNT(*) as task_count
FROM maintenance_tasks
GROUP BY status
ORDER BY status;

-- Tasks that still have MISSING_* status (need attention)
SELECT 
    t.task_id,
    t.task_description,
    t.status,
    t.assigned_to,
    s.schedule_code
FROM maintenance_tasks t
LEFT JOIN maintenance_schedules s ON t.schedule_id = s.id
WHERE t.status IN ('MISSING_CHECKLIST', 'MISSING_PIC', 'MISSING_BOTH')
ORDER BY t.status, t.task_id;
