-- =====================================================
-- COPY & PASTE vào pgAdmin/DataGrip để chạy
-- =====================================================

-- Enable pgcrypto extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Tạo role USER nếu chưa có
INSERT INTO roles (role_name, role_code, description, is_active, created_at)
VALUES ('User', 'USER', 'Regular crew member', true, NOW())
ON CONFLICT (role_code) DO NOTHING;

-- Kiểm tra role_id
SELECT id, role_code, role_name FROM roles WHERE role_code = 'USER';

-- Tạo users từ crew_members
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT 
    cm.crew_id,
    encode(digest(COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456'), 'sha256'), 'base64'),
    (SELECT id FROM roles WHERE role_code = 'USER'),
    cm.crew_id,
    true,
    NOW()
FROM crew_members cm
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id);

-- Hiển thị danh sách users và password
SELECT 
    u.username,
    cm.full_name,
    cm.position,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password
FROM users u
JOIN crew_members cm ON u.crew_id = cm.crew_id
ORDER BY u.username;
