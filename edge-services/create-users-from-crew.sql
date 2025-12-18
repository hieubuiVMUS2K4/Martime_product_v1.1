-- =====================================================
-- Tạo users cho tất cả crew members
-- Username = crew_id
-- Password = date_of_birth (format: DDMMYYYY) hoặc "123456"
-- =====================================================

-- Đảm bảo có role USER
INSERT INTO roles (role_name, role_code, description)
VALUES ('User', 'USER', 'Regular crew member')
ON CONFLICT (role_code) DO NOTHING;

-- Tạo users từ crew_members
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
WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = cm.crew_id);

-- Verify
SELECT 
    u.username,
    cm.full_name,
    cm.rank,
    cm.position,
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') as default_password,
    r.role_name,
    u.is_active
FROM users u
JOIN crew_members cm ON u.crew_id = cm.crew_id
LEFT JOIN roles r ON u.role_id = r.id
ORDER BY u.username;
