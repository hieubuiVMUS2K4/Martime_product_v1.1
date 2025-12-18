-- =====================================================
-- RUN AFTER BACKEND RESTART
-- Chạy sau khi backend đã generate tasks
-- Chuyển tất cả tasks sang status DUE/OVERDUE
-- =====================================================

-- Kiểm tra tasks đã được tạo chưa
SELECT 
    'Tasks created' as info,
    COUNT(*) as count 
FROM maintenance_tasks 
WHERE task_id LIKE 'SCHED-%' AND is_deleted = false;

-- Update tất cả tasks SCHEDULED → DUE/OVERDUE
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
    string_agg(SUBSTRING(task_id FROM 1 FOR 30), ', ') as sample_tasks
FROM maintenance_tasks
WHERE is_deleted = false AND task_id LIKE 'SCHED-%'
GROUP BY status
ORDER BY status;

-- Chi tiết các tasks DUE/OVERDUE
SELECT 
    task_id,
    priority,
    status,
    next_due_at::date as due_date,
    assigned_to,
    CASE 
        WHEN next_due_at::date < CURRENT_DATE THEN 'OVERDUE'
        WHEN next_due_at::date = CURRENT_DATE THEN 'DUE TODAY'
        ELSE 'SCHEDULED'
    END as expected_status
FROM maintenance_tasks
WHERE is_deleted = false 
  AND task_id LIKE 'SCHED-%'
  AND status IN ('DUE', 'OVERDUE', 'SCHEDULED')
ORDER BY 
    CASE status 
        WHEN 'OVERDUE' THEN 1 
        WHEN 'DUE' THEN 2 
        ELSE 3 
    END,
    priority DESC,
    next_due_at;
