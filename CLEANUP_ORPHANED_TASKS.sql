-- 🔧 CLEANUP ORPHANED TASKS (Tasks từ schedules đã bị xóa)
-- Chạy script này để xóa các maintenance tasks không còn schedule

-- 1. XEM TRƯỚC các tasks sẽ bị xóa
SELECT 
    t.id,
    t.task_code,
    t.task_name,
    t.status,
    t.schedule_id,
    t.created_at
FROM maintenance_tasks t
WHERE t.schedule_id IS NULL  -- Tasks không còn schedule
ORDER BY t.created_at DESC;

-- 2. XÓA các tasks orphaned (không còn schedule)
-- UNCOMMENT dòng dưới đây để xóa
-- DELETE FROM maintenance_tasks WHERE schedule_id IS NULL;

-- 3. Verify kết quả
-- SELECT COUNT(*) as remaining_orphaned_tasks 
-- FROM maintenance_tasks 
-- WHERE schedule_id IS NULL;
