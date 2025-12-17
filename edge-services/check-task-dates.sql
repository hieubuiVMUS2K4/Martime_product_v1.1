-- Kiểm tra next_due_at của các tasks SCHEDULED
SELECT 
    t.task_id,
    t.status,
    t.next_due_at,
    t.next_due_at::date as due_date,
    CURRENT_DATE as today,
    CASE 
        WHEN t.next_due_at::date < CURRENT_DATE THEN 'SHOULD BE OVERDUE'
        WHEN t.next_due_at::date = CURRENT_DATE THEN 'SHOULD BE DUE'
        WHEN t.next_due_at::date > CURRENT_DATE THEN 'CORRECTLY SCHEDULED'
    END as expected_status
FROM maintenance_tasks t
WHERE t.task_id LIKE 'SCHED-%'
  AND t.is_deleted = false
ORDER BY t.next_due_at;
