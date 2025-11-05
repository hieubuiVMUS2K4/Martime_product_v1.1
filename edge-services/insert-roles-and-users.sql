-- =====================================================
-- INSERT ROLES AND USERS FOR AUTHENTICATION SYSTEM
-- Thêm dữ liệu phân quyền và tài khoản người dùng
-- =====================================================

-- =====================================================
-- 1. INSERT ROLES (Bảng phân quyền)
-- =====================================================
INSERT INTO roles (role_code, role_name, description, is_active, created_at) VALUES
('ADMIN', 'Quản trị viên', 'Toàn quyền quản lý hệ thống', true, NOW()),
('USER', 'Người dùng', 'Quyền xem và cập nhật dữ liệu cơ bản', true, NOW())
ON CONFLICT (role_code) DO NOTHING;

-- =====================================================
-- 2. CREATE ADMIN USER
-- Password mặc định: admin123
-- SHA256 hash của "admin123"
-- =====================================================
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    'admin',
    'YWRtaW4xMjM=', -- Base64 của SHA256 hash sẽ được tạo bởi application
    (SELECT id FROM roles WHERE role_code = 'ADMIN'),
    NULL,
    true,
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin');

-- =====================================================
-- 3. CREATE USERS FOR ALL CREW MEMBERS
-- Username = crew_id
-- Password = date_of_birth (format: ddMMyyyy)
-- Ví dụ: Ngày sinh 15/05/1990 -> password: 15051990
-- =====================================================

-- Tạo user cho TẤT CẢ thuyền viên trong database
-- Password sẽ được hash từ ngày sinh (nếu có), nếu không có ngày sinh thì dùng "123456"
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    cm.crew_id,
    -- SHA256 hash của password sẽ được tính trong application
    -- Đây là placeholder, cần chạy qua API để tạo đúng hash
    encode(digest(
        COALESCE(
            to_char(cm.date_of_birth, 'DDMMYYYY'),
            '123456'
        ), 'sha256'
    ), 'base64'),
    (SELECT id FROM roles WHERE role_code = 'USER'),
    cm.crew_id,
    true,
    NOW()
FROM crew_members cm
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id);

-- =====================================================
-- 4. VERIFY DATA
-- =====================================================

-- Kiểm tra roles đã insert
SELECT 
    id,
    role_code,
    role_name,
    description,
    is_active,
    created_at
FROM roles
ORDER BY id;

-- Kiểm tra users đã tạo
SELECT 
    u.id,
    u.username,
    r.role_name,
    u.crew_id,
    cm.full_name,
    cm.position,
    cm.date_of_birth,
    u.is_active,
    u.created_at
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
ORDER BY u.id;

-- Thống kê user theo role
SELECT 
    r.role_name,
    COUNT(u.id) as user_count
FROM roles r
LEFT JOIN users u ON r.id = u.role_id AND u.is_active = true
GROUP BY r.id, r.role_name
ORDER BY r.id;

-- Hiển thị password mặc định cho từng user (để test)
SELECT 
    u.username,
    cm.full_name,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password,
    r.role_name
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
ORDER BY u.username;

-- =====================================================
-- NOTES - HƯỚNG DẪN SỬ DỤNG
-- =====================================================
-- 
-- 1. ROLES:
--    - ADMIN: Quản trị viên (username: admin, password: admin123)
--    - USER: Người dùng thường
--
-- 2. TẠO USER TỰ ĐỘNG:
--    - Username = crew_id (mã thuyền viên)
--    - Password = ngày sinh (format: ddMMyyyy)
--    - Ví dụ: Crew CM001 có ngày sinh 15/05/1990 
--      -> username: CM001
--      -> password: 15051990
--    - Nếu không có ngày sinh: password mặc định là "123456"
--
-- 3. ĐĂNG NHẬP:
--    POST /api/auth/login
--    {
--      "username": "CM001",
--      "password": "15051990"
--    }
--
-- 4. ĐỔI MẬT KHẨU:
--    POST /api/auth/change-password
--    {
--      "userId": 1,
--      "oldPassword": "15051990",
--      "newPassword": "newpassword123",
--      "confirmPassword": "newpassword123"
--    }
--
-- 5. RESET MẬT KHẨU VỀ MẶC ĐỊNH (từ ngày sinh):
--    POST /api/auth/reset-password
--    {
--      "username": "CM001"
--    }
--
-- 6. TẠO USER MỚI CHO CREW MEMBER:
--    POST /api/auth/create-user
--    {
--      "crewId": "CM001",
--      "roleId": 2
--    }
--
-- =====================================================
