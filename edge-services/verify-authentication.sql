-- =====================================================
-- VERIFY AUTHENTICATION SYSTEM
-- Script để kiểm tra và test hệ thống đăng nhập
-- =====================================================

-- =====================================================
-- 1. Kiểm tra Roles
-- =====================================================
SELECT 
    id,
    role_code,
    role_name,
    description,
    is_active,
    created_at
FROM roles
ORDER BY id;

-- =====================================================
-- 2. Kiểm tra Users
-- =====================================================
SELECT 
    u.id,
    u.username,
    r.role_name as role,
    u.crew_id,
    cm.full_name,
    cm.position,
    u.is_active,
    u.last_login_at,
    u.created_at
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
ORDER BY u.id;

-- =====================================================
-- 3. Danh sách password mặc định (để test)
-- =====================================================
SELECT 
    u.username,
    cm.full_name,
    cm.position,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password,
    r.role_name
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
ORDER BY u.username;

-- =====================================================
-- 4. Thống kê Users theo Role
-- =====================================================
SELECT 
    r.role_name,
    COUNT(u.id) as total_users,
    COUNT(CASE WHEN u.is_active = true THEN 1 END) as active_users,
    COUNT(CASE WHEN u.last_login_at IS NOT NULL THEN 1 END) as users_logged_in
FROM roles r
LEFT JOIN users u ON r.id = u.role_id
GROUP BY r.id, r.role_name
ORDER BY r.id;

-- =====================================================
-- 5. Crew Members chưa có tài khoản
-- =====================================================
SELECT 
    cm.crew_id,
    cm.full_name,
    cm.position,
    cm.is_onboard,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password
FROM crew_members cm
WHERE NOT EXISTS (
    SELECT 1 FROM users u WHERE u.crew_id = cm.crew_id
)
AND cm.is_onboard = true
ORDER BY cm.crew_id;

-- =====================================================
-- 6. Tạo user cho crew members chưa có tài khoản
-- =====================================================
-- Run này nếu muốn tạo user cho tất cả crew members
/*
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    cm.crew_id,
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
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id)
AND cm.is_onboard = true;
*/

-- =====================================================
-- 7. Test password hash
-- =====================================================
-- Ví dụ: Hash password "15051990" (ngày sinh 15/05/1990)
SELECT 
    '15051990' as original_password,
    encode(digest('15051990', 'sha256'), 'base64') as password_hash;

-- Ví dụ: Hash password "admin123"
SELECT 
    'admin123' as original_password,
    encode(digest('admin123', 'sha256'), 'base64') as password_hash;

-- =====================================================
-- 8. Kiểm tra user login history (recent logins)
-- =====================================================
SELECT 
    u.username,
    cm.full_name,
    r.role_name,
    u.last_login_at,
    EXTRACT(EPOCH FROM (NOW() - u.last_login_at)) / 3600 as hours_since_login
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
WHERE u.last_login_at IS NOT NULL
ORDER BY u.last_login_at DESC
LIMIT 20;

-- =====================================================
-- 9. Active users (logged in last 24 hours)
-- =====================================================
SELECT 
    u.username,
    cm.full_name,
    r.role_name,
    u.last_login_at
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
WHERE u.last_login_at > NOW() - INTERVAL '24 hours'
ORDER BY u.last_login_at DESC;

-- =====================================================
-- 10. Reset password về mặc định cho một user
-- =====================================================
-- Ví dụ: Reset password cho user "CM001"
/*
UPDATE users u
SET 
    password_hash = (
        SELECT encode(digest(
            COALESCE(
                to_char(cm.date_of_birth, 'DDMMYYYY'),
                '123456'
            ), 'sha256'
        ), 'base64')
        FROM crew_members cm
        WHERE cm.crew_id = u.crew_id
    ),
    updated_at = NOW()
WHERE u.username = 'CM001';
*/

-- =====================================================
-- 11. Vô hiệu hóa/kích hoạt user
-- =====================================================
-- Vô hiệu hóa user
-- UPDATE users SET is_active = false, updated_at = NOW() WHERE username = 'CM001';

-- Kích hoạt user
-- UPDATE users SET is_active = true, updated_at = NOW() WHERE username = 'CM001';

-- =====================================================
-- 12. Tìm users không active
-- =====================================================
SELECT 
    u.id,
    u.username,
    cm.full_name,
    r.role_name,
    u.is_active,
    u.created_at,
    u.last_login_at
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN crew_members cm ON u.crew_id = cm.crew_id
WHERE u.is_active = false
ORDER BY u.username;

-- =====================================================
-- NOTES
-- =====================================================
-- 
-- TEST LOGIN với CURL:
-- 
-- curl -X POST http://localhost:5001/api/auth/login \
--   -H "Content-Type: application/json" \
--   -d '{"username":"CM001","password":"15051990"}'
--
-- curl -X POST http://localhost:5001/api/auth/login \
--   -H "Content-Type: application/json" \
--   -d '{"username":"admin","password":"admin123"}'
--
-- TEST CHANGE PASSWORD:
--
-- curl -X POST http://localhost:5001/api/auth/change-password \
--   -H "Content-Type: application/json" \
--   -d '{
--     "userId": 1,
--     "oldPassword": "15051990",
--     "newPassword": "newpass123",
--     "confirmPassword": "newpass123"
--   }'
--
-- TEST RESET PASSWORD:
--
-- curl -X POST http://localhost:5001/api/auth/reset-password \
--   -H "Content-Type: application/json" \
--   -d '{"username":"CM001"}'
--
-- =====================================================

