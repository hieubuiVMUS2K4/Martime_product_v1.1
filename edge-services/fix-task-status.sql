-- =====================================================
-- FIX TASK STATUS - Chuyển SCHEDULED → DUE/OVERDUE
-- =====================================================

-- Chuyển tất cả tasks SCHEDULED đã đến hạn sang DUE/OVERDUE
UPDATE maintenance_tasks
SET 
    status = CASE 
        WHEN next_due_at::date < CURRENT_DATE THEN 'OVERDUE'
        WHEN next_due_at::date = CURRENT_DATE THEN 'DUE'
        ELSE status
    END,
    updated_at = NOW()
WHERE status = 'SCHEDULED'
  AND next_due_at::date <= CURRENT_DATE
  AND is_deleted = false;

-- Verify kết quả
SELECT 
    status,
    COUNT(*) as count,
    string_agg(task_id, ', ') as sample_tasks
FROM maintenance_tasks
WHERE is_deleted = false
  AND task_id LIKE 'SCHED-%'
GROUP BY status
ORDER BY status;
