-- =====================================================
-- SAMPLE DATA FOR TASK MANAGEMENT SYSTEM
-- Hệ thống quản lý công việc chuyên nghiệp
-- =====================================================

-- =====================================================
-- 1. INSERT TASK TYPES (Các loại công việc)
-- =====================================================

INSERT INTO task_types (type_code, type_name, description, category, default_priority, estimated_duration_hours, required_certification, requires_approval, is_active, created_at) VALUES
-- ENGINE TASKS
('ENGINE_OVERHAUL', 'Đại tu động cơ chính', 'Đại tu toàn bộ động cơ chính, kiểm tra và thay thế các bộ phận', 'ENGINE', 'CRITICAL', 48, 'Engine Certificate', true, true, NOW()),
('ENGINE_OIL_CHANGE', 'Thay dầu động cơ', 'Thay dầu và lọc dầu động cơ chính', 'ENGINE', 'HIGH', 4, NULL, false, true, NOW()),
('ENGINE_INSPECTION', 'Kiểm tra động cơ định kỳ', 'Kiểm tra tổng thể tình trạng động cơ', 'ENGINE', 'NORMAL', 2, NULL, false, true, NOW()),

-- SAFETY TASKS
('SAFETY_DRILL', 'Diễn tập an toàn', 'Diễn tập cứu hỏa, cứu sinh, abandon ship', 'SAFETY', 'CRITICAL', 2, 'Safety Officer', false, true, NOW()),
('FIRE_EQUIPMENT_CHECK', 'Kiểm tra thiết bị PCCC', 'Kiểm tra bình cứu hỏa, vòi rồng, báo cháy', 'SAFETY', 'HIGH', 1, NULL, false, true, NOW()),
('LIFEBOAT_INSPECTION', 'Kiểm tra xuồng cứu sinh', 'Kiểm tra tình trạng xuồng, động cơ, thiết bị', 'SAFETY', 'HIGH', 2, 'Lifeboat Certificate', false, true, NOW()),

-- DECK TASKS
('DECK_CLEANING', 'Vệ sinh boong', 'Vệ sinh tổng thể boong tàu', 'DECK', 'NORMAL', 4, NULL, false, true, NOW()),
('MOORING_EQUIPMENT_CHECK', 'Kiểm tra thiết bị neo buộc', 'Kiểm tra dây neo, dây buộc, tời neo', 'DECK', 'HIGH', 2, NULL, false, true, NOW()),
('CARGO_HOLD_INSPECTION', 'Kiểm tra khoang hàng', 'Kiểm tra tình trạng khoang hàng, hệ thống thông gió', 'DECK', 'NORMAL', 3, NULL, false, true, NOW()),

-- ELECTRICAL TASKS
('GENERATOR_MAINTENANCE', 'Bảo dưỡng máy phát điện', 'Kiểm tra và bảo dưỡng máy phát điện phụ', 'ELECTRICAL', 'HIGH', 6, 'Electrical Certificate', false, true, NOW()),
('BATTERY_CHECK', 'Kiểm tra ắc quy', 'Kiểm tra điện áp, mức nước cất ắc quy', 'ELECTRICAL', 'NORMAL', 1, NULL, false, true, NOW()),
('LIGHTING_INSPECTION', 'Kiểm tra hệ thống chiếu sáng', 'Kiểm tra đèn tín hiệu, đèn boong, đèn cabin', 'ELECTRICAL', 'NORMAL', 2, NULL, false, true, NOW())
ON CONFLICT (type_code) DO NOTHING;

-- =====================================================
-- 2. INSERT TASK DETAILS (Chi tiết từng loại công việc)
-- =====================================================

-- Task Details cho ENGINE_OIL_CHANGE
INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Kiểm tra mức dầu cũ',
    'Kiểm tra và ghi nhận mức dầu hiện tại trước khi thay',
    1,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    false,
    false,
    'Sử dụng que thăm dầu để kiểm tra mức dầu',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'ENGINE_OIL_CHANGE'
ON CONFLICT DO NOTHING;

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Xả dầu cũ',
    'Mở van xả và xả hoàn toàn dầu cũ ra',
    2,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Đảm bảo thu gom dầu cũ đúng quy định MARPOL',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'ENGINE_OIL_CHANGE';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Thay lọc dầu',
    'Tháo lọc dầu cũ và lắp lọc mới',
    3,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Kiểm tra O-ring của lọc mới, bôi dầu trước khi lắp',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'ENGINE_OIL_CHANGE';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Đổ dầu mới',
    'Đổ dầu mới theo đúng quy cách và số lượng',
    4,
    'MEASUREMENT',
    true,
    'liter',
    100,
    150,
    false,
    false,
    'Sử dụng dầu động cơ SAE 40, ghi nhận số lượng chính xác',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'ENGINE_OIL_CHANGE';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Chạy thử và kiểm tra',
    'Khởi động động cơ, kiểm tra áp suất dầu',
    5,
    'MEASUREMENT',
    true,
    'bar',
    3.5,
    5.0,
    false,
    true,
    'Áp suất dầu phải đạt 3.5-5.0 bar khi động cơ ở nhiệt độ vận hành',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'ENGINE_OIL_CHANGE';

-- Task Details cho FIRE_EQUIPMENT_CHECK
INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Kiểm tra bình cứu hỏa CO2',
    'Kiểm tra áp suất và trọng lượng bình CO2',
    1,
    'MEASUREMENT',
    true,
    'bar',
    50,
    65,
    true,
    false,
    'Áp suất bình CO2 phải trong khoảng 50-65 bar',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'FIRE_EQUIPMENT_CHECK';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Kiểm tra bình bọt',
    'Kiểm tra áp suất và hạn sử dụng bình bọt',
    2,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Kiểm tra chỉ báo áp suất trong vùng xanh, hạn sử dụng còn hiệu lực',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'FIRE_EQUIPMENT_CHECK';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Kiểm tra vòi rồng',
    'Kiểm tra tình trạng vòi, vòi phun, van',
    3,
    'INSPECTION',
    true,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Kiểm tra không rò rỉ, vòi không bị hư, nước phun mạnh',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'FIRE_EQUIPMENT_CHECK';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Kiểm tra báo cháy',
    'Test hệ thống báo cháy tự động',
    4,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    false,
    true,
    'Nhấn nút test trên đầu báo, đảm bảo chuông kêu',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'FIRE_EQUIPMENT_CHECK';

-- Task Details cho SAFETY_DRILL
INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Tập hợp đầy đủ thuyền viên',
    'Kiểm tra sĩ số tại điểm tập trung',
    1,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    false,
    false,
    'Điểm danh đầy đủ theo danh sách muster list',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'SAFETY_DRILL';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Mặc áo phao và kiểm tra',
    'Tất cả mặc áo phao đúng cách',
    2,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Kiểm tra mỗi người mặc đúng cách, còn nguyên seal CO2',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'SAFETY_DRILL';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Hạ thả xuồng cứu sinh',
    'Thực hiện hạ thả xuồng (không cần hạ xuống nước)',
    3,
    'CHECKLIST',
    false,
    NULL,
    NULL,
    NULL,
    true,
    false,
    'Hạ xuống 1-2m để kiểm tra hệ thống tời',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'SAFETY_DRILL';

INSERT INTO task_details (task_type_id, detail_name, description, order_index, detail_type, is_mandatory, unit, min_value, max_value, requires_photo, requires_signature, instructions, is_active, created_at)
SELECT 
    tt.id,
    'Ghi nhận và ký tên',
    'Thuyền trưởng/Chief Officer ký xác nhận',
    4,
    'CHECKLIST',
    true,
    NULL,
    NULL,
    NULL,
    false,
    true,
    'Ghi nhận vào Safety Drill Log, ký tên xác nhận',
    true,
    NOW()
FROM task_types tt WHERE tt.type_code = 'SAFETY_DRILL';

-- =====================================================
-- 3. VERIFY DATA
-- =====================================================

-- Xem danh sách Task Types
SELECT 
    id,
    type_code,
    type_name,
    category,
    default_priority,
    estimated_duration_hours,
    is_active
FROM task_types
ORDER BY category, id;

-- Xem Task Details theo từng Task Type
SELECT 
    tt.type_name,
    td.order_index,
    td.detail_name,
    td.detail_type,
    td.is_mandatory,
    td.requires_photo,
    td.requires_signature,
    td.unit,
    td.min_value,
    td.max_value
FROM task_details td
JOIN task_types tt ON td.task_type_id = tt.id
ORDER BY tt.type_name, td.order_index;

-- Thống kê số lượng details theo từng type
SELECT 
    tt.type_name,
    COUNT(td.id) as detail_count,
    SUM(CASE WHEN td.is_mandatory THEN 1 ELSE 0 END) as mandatory_count,
    SUM(CASE WHEN td.requires_photo THEN 1 ELSE 0 END) as photo_required_count,
    SUM(CASE WHEN td.requires_signature THEN 1 ELSE 0 END) as signature_required_count
FROM task_types tt
LEFT JOIN task_details td ON tt.id = td.task_type_id
GROUP BY tt.id, tt.type_name
ORDER BY tt.type_name;

-- =====================================================
-- NOTES - HƯỚNG DẪN SỬ DỤNG
-- =====================================================
--
-- CẤU TRÚC HỆ THỐNG:
-- 1. TaskTypes: Định nghĩa các loại công việc (template)
-- 2. TaskDetails: Các bước/checklist cụ thể cho từng loại
-- 3. MaintenanceTasks: Công việc thực tế được giao (tham chiếu TaskType)
-- 4. MaintenanceTaskDetails: Kết quả thực hiện từng bước (N-N)
--
-- QUY TRÌNH:
-- 1. Admin tạo TaskTypes (các loại công việc)
-- 2. Admin định nghĩa TaskDetails cho từng TaskType
-- 3. Khi giao việc, Admin tạo MaintenanceTask và chọn TaskType
-- 4. Hệ thống tự động tạo MaintenanceTaskDetails từ TaskDetails
-- 5. Thuyền viên thực hiện và cập nhật kết quả từng bước
-- 6. Admin theo dõi tiến độ và duyệt kết quả
--
-- =====================================================
