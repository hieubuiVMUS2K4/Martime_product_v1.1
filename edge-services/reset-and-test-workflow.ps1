#!/usr/bin/env pwsh
# Reset and test group-based maintenance workflow

$env:PGPASSWORD = 'ChangeMe_EdgePassword123!'
$pgHost = 'localhost'
$pgPort = '5433'
$pgUser = 'edge_user'
$pgDb = 'maritime_edge'

Write-Host "🔄 Starting maintenance workflow reset..." -ForegroundColor Cyan

# Step 1: Clean up old data
Write-Host "`n1️⃣ Cleaning up old tasks..." -ForegroundColor Yellow
$cleanupSql = @"
-- Delete all old maintenance tasks and related data
DELETE FROM task_checklist_items;
DELETE FROM maintenance_task_details;
DELETE FROM maintenance_tasks;

-- Reset schedules
UPDATE maintenance_schedules 
SET 
    last_executed_at = NULL,
    next_due_date = CURRENT_TIMESTAMP + INTERVAL '7 days',
    next_due_running_hours = NULL,
    last_executed_running_hours = NULL;
"@

psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $cleanupSql

# Step 2: Show cleanup results
Write-Host "`n2️⃣ Verifying cleanup..." -ForegroundColor Yellow
$verifySql = @"
SELECT 'Maintenance Tasks' as table_name, COUNT(*) as record_count FROM maintenance_tasks
UNION ALL
SELECT 'Task Checklist Items', COUNT(*) FROM task_checklist_items
UNION ALL
SELECT 'Maintenance Schedules', COUNT(*) FROM maintenance_schedules;
"@

psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $verifySql

# Step 3: Create test schedules
Write-Host "`n3️⃣ Creating test schedules (overdue for immediate testing)..." -ForegroundColor Yellow
$schedulesSql = @"
-- Get first equipment group for testing
DO `$`$
DECLARE
    test_group_id uuid;
BEGIN
    -- Find any equipment group with members
    SELECT eg.id INTO test_group_id
    FROM equipment_groups eg
    INNER JOIN equipment_group_members egm ON eg.id = egm.group_id
    WHERE eg.is_active = true
    GROUP BY eg.id
    HAVING COUNT(egm.id) > 0
    LIMIT 1;

    -- If group found, create test schedule
    IF test_group_id IS NOT NULL THEN
        -- Delete old test schedules
        DELETE FROM maintenance_schedules WHERE schedule_code LIKE 'TEST-%';
        
        -- Insert test schedule (OVERDUE for immediate trigger)
        INSERT INTO maintenance_schedules (
            schedule_code,
            schedule_name,
            equipment_group_id,
            interval_type,
            interval_days,
            priority,
            instructions,
            is_active,
            next_due_date,
            created_at
        ) VALUES (
            'TEST-GROUP-MAINT',
            'Test Group-Based Maintenance',
            test_group_id,
            'CALENDAR',
            7, -- Weekly
            'HIGH',
            'This is a test schedule for group-based maintenance workflow',
            true,
            CURRENT_TIMESTAMP - INTERVAL '1 hour', -- OVERDUE - will trigger immediately
            CURRENT_TIMESTAMP
        );
        
        RAISE NOTICE 'Test schedule created successfully';
    ELSE
        RAISE NOTICE 'No equipment groups with members found. Please create equipment groups first.';
    END IF;
END
`$`$;
"@

psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $schedulesSql

# Step 4: Show created schedules
Write-Host "`n4️⃣ Active schedules ready for testing..." -ForegroundColor Yellow
$showSchedulesSql = @"
SELECT 
    ms.schedule_code,
    ms.schedule_name,
    eg.group_name,
    eg.department,
    COUNT(egm.id) as asset_count,
    ms.priority,
    ms.next_due_date,
    CASE 
        WHEN ms.next_due_date < CURRENT_TIMESTAMP THEN '🔴 OVERDUE'
        WHEN ms.next_due_date < CURRENT_TIMESTAMP + INTERVAL '3 days' THEN '🟡 DUE SOON'
        ELSE '🟢 SCHEDULED'
    END as status
FROM maintenance_schedules ms
INNER JOIN equipment_groups eg ON ms.equipment_group_id = eg.id
LEFT JOIN equipment_group_members egm ON eg.id = egm.group_id
WHERE ms.is_active = true
GROUP BY ms.id, ms.schedule_code, ms.schedule_name, eg.group_name, 
         eg.department, ms.priority, ms.next_due_date
ORDER BY ms.next_due_date;
"@

psql -h $pgHost -p $pgPort -U $pgUser -d $pgDb -c $showSchedulesSql

Write-Host "`n✅ Reset complete! Backend service will auto-generate group tasks for overdue schedules." -ForegroundColor Green
Write-Host "   Run: cd edge-services && dotnet run" -ForegroundColor Cyan
