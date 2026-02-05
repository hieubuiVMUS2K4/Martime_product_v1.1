-- Refresh users table from crew_members
-- - Deletes existing users
-- - Recreates one user per crew_members.crew_id
-- - Default password rule: ddMMyyyy from date_of_birth, fallback: 123456
--
-- Run (PowerShell) from edge-services folder:
--   Get-Content -Raw .\Scripts\refresh-users-from-crew.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge

BEGIN;

-- Needed for digest()
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Ensure role USER exists (roles.role_code is UNIQUE)
INSERT INTO roles (role_code, role_name, description, is_active, created_at)
VALUES ('USER', 'User', 'Regular crew member', true, NOW())
ON CONFLICT (role_code) DO UPDATE
SET role_name = EXCLUDED.role_name,
    description = EXCLUDED.description,
    is_active = true;

-- Remove old users (no other tables FK->users in this DB)
DELETE FROM users;

-- Create users from crew list
INSERT INTO users (username, password_hash, role_id, crew_id, is_active, created_at)
SELECT
    cm.crew_id,
    encode(
        digest(
            COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456'),
            'sha256'
        ),
        'base64'
    ) AS password_hash,
    (SELECT id FROM roles WHERE role_code = 'USER') AS role_id,
    cm.crew_id,
    true,
    NOW()
FROM crew_members cm;

COMMIT;

-- Verify
SELECT
    u.username,
    cm.full_name,
    cm.position,
    r.role_code,
    u.is_active,
    u.created_at
FROM users u
LEFT JOIN crew_members cm ON cm.crew_id = u.crew_id
LEFT JOIN roles r ON r.id = u.role_id
ORDER BY u.username;
