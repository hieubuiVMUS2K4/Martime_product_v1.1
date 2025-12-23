-- Fix Past Due Dates for Maintenance Schedules
-- Run this script to update all schedules with past NextDueDate to future dates
-- Date: 2025-12-21

USE [EdgeDatabase]; -- Change to your database name
GO

DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);
DECLARE @UpdatedCount INT = 0;

PRINT '====================================';
PRINT 'Fixing Past Due Dates in Schedules';
PRINT '====================================';
PRINT '';
PRINT 'Today: ' + CAST(@Today AS VARCHAR(20));
PRINT '';

-- Update schedules with CALENDAR intervals
UPDATE ms
SET 
    NextDueDate = DATEADD(DAY, 
        -- Calculate how many intervals to skip
        (DATEDIFF(DAY, ms.NextDueDate, @Today) / ms.IntervalDays + 1) * ms.IntervalDays,
        ms.NextDueDate
    ),
    UpdatedAt = GETUTCDATE()
FROM MaintenanceSchedules ms
WHERE 
    ms.IntervalType = 'CALENDAR'
    AND ms.IntervalDays IS NOT NULL
    AND ms.NextDueDate < @Today
    AND ms.IsAutoGenerate = 1;

SET @UpdatedCount = @@ROWCOUNT;

PRINT 'Updated ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' CALENDAR schedules';
PRINT '';

-- Show updated schedules
SELECT 
    ScheduleCode,
    ScheduleName,
    IntervalType,
    IntervalDays,
    CAST(NextDueDate AS DATE) AS NewNextDueDate,
    DATEDIFF(DAY, @Today, NextDueDate) AS DaysUntilDue,
    Priority
FROM MaintenanceSchedules
WHERE 
    IntervalType = 'CALENDAR'
    AND UpdatedAt >= DATEADD(SECOND, -5, GETUTCDATE())
ORDER BY NextDueDate;

PRINT '';
PRINT '✅ All past due schedules have been updated to future dates!';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Restart backend service to trigger scheduler';
PRINT '2. Scheduler will create new tasks with correct future dates';
PRINT '3. Old OVERDUE tasks should be reviewed/cancelled manually if needed';
GO
