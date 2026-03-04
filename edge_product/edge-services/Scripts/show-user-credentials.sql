-- Show login credentials for all users
-- Default password: ddMMyyyy from date_of_birth, or '123456' if no date_of_birth
--
-- Run (PowerShell):
--   Get-Content -Raw .\Scripts\show-user-credentials.sql | docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge

\echo '================================================'
\echo 'USER LOGIN CREDENTIALS'
\echo '================================================'
\echo ''

SELECT
    u.username AS "Username",
    cm.full_name AS "Full Name",
    cm.position AS "Position",
    COALESCE(to_char(cm.date_of_birth, 'DD/MM/YYYY'), '(no date_of_birth)') AS "Date of Birth",
    COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456') AS "Default Password",
    r.role_name AS "Role",
    CASE WHEN u.is_active THEN '✓' ELSE '✗' END AS "Active"
FROM users u
LEFT JOIN crew_members cm ON cm.crew_id = u.crew_id
LEFT JOIN roles r ON r.id = u.role_id
ORDER BY u.username;

\echo ''
\echo '================================================'
\echo 'Password Rule:'
\echo '  - If crew has date_of_birth: use DDMMYYYY format'
\echo '  - Otherwise: use default "123456"'
\echo '================================================'
