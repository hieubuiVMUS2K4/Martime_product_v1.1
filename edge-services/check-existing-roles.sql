-- Check what roles exist in the database
SELECT * FROM roles ORDER BY id;

-- If roles table is empty, insert basic roles:
INSERT INTO roles (id, role_code, role_name, description, is_active, created_at)
VALUES 
(1, 'ADMIN', 'Administrator', 'System administrator with full access', true, NOW()),
(2, 'MANAGER', 'Manager', 'Shore-based manager', true, NOW()),
(3, 'MASTER', 'Master', 'Ship Master', true, NOW()),
(4, 'CHIEF_ENGINEER', 'Chief Engineer', 'Chief Engineer', true, NOW()),
(5, 'CREW', 'Crew Member', 'General crew member', true, NOW())
ON CONFLICT (id) DO NOTHING;

-- Verify roles were inserted
SELECT * FROM roles ORDER BY id;
