-- ============================================================
-- SEED DATA: CREW MEMBERS & USERS
-- Based on ANNIE GAS 09 actual organization structure
-- ============================================================

-- Delete existing test data (if any)
-- Must delete users first due to foreign key constraint
DELETE FROM users WHERE username LIKE 'CREW%' OR id BETWEEN 1 AND 20;
DELETE FROM crew_members WHERE crew_id LIKE 'CREW%';

-- ============================================================
-- INSERT ROLES (if not exist)
-- ============================================================
INSERT INTO roles (id, role_code, role_name, description, is_active, created_at)
VALUES 
(1, 'ADMIN', 'Administrator', 'System administrator with full access', true, NOW()),
(2, 'MANAGER', 'Manager', 'Shore-based manager', true, NOW()),
(3, 'MASTER', 'Master', 'Ship Master', true, NOW()),
(4, 'CHIEF_ENGINEER', 'Chief Engineer', 'Chief Engineer', true, NOW()),
(5, 'CREW', 'Crew Member', 'General crew member', true, NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- MANAGEMENT LEVEL (Cấp quản lý)
-- ============================================================

-- 1. Master (Thuyền trưởng)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW001',
    'Nguyễn Văn Thành',
    'Master',
    'MASTER',
    'MANAGEMENT',
    'Vietnam',
    '1975-03-15'::timestamptz,
    '2020-01-01'::timestamptz,
    '2025-10-01'::timestamptz,
    '2026-04-01'::timestamptz,
    true,
    'master@annie-gas09.com',
    '+84901234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 2. Chief Engineer (Máy trưởng)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW002',
    'Trần Minh Tuấn',
    'Chief Engineer',
    'C/E',
    'ENGINE',
    'Vietnam',
    '1978-07-20'::timestamptz,
    '2019-06-01'::timestamptz,
    '2025-09-15'::timestamptz,
    '2026-03-15'::timestamptz,
    true,
    'chief.engineer@annie-gas09.com',
    '+84902234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 3. Chief Officer (Đại phó)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW003',
    'Lê Hoàng Nam',
    'Chief Officer',
    'C/O',
    'DECK',
    'Vietnam',
    '1982-11-10'::timestamptz,
    '2021-03-01'::timestamptz,
    '2025-11-01'::timestamptz,
    '2026-05-01'::timestamptz,
    true,
    'chief.officer@annie-gas09.com',
    '+84903234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- ============================================================
-- ENGINE DEPARTMENT (Bộ phận máy)
-- ============================================================

-- 4. Second Engineer (Máy 2) - KEY PERSON
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW004',
    'Phạm Đức Anh',
    'Second Engineer',
    '2/E',
    'ENGINE',
    'Vietnam',
    '1985-05-25'::timestamptz,
    '2022-01-15'::timestamptz,
    '2025-10-15'::timestamptz,
    '2026-04-15'::timestamptz,
    true,
    '2nd.engineer@annie-gas09.com',
    '+84904234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 5. Third Engineer (Máy 3) - KEY PERSON
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW005',
    'Võ Thanh Tùng',
    'Third Engineer',
    '3/E',
    'ENGINE',
    'Vietnam',
    '1988-09-12'::timestamptz,
    '2023-02-01'::timestamptz,
    '2025-11-10'::timestamptz,
    '2026-05-10'::timestamptz,
    true,
    '3rd.engineer@annie-gas09.com',
    '+84905234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 6. Electrical Officer (Sỹ quan Điện) - KEY PERSON
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW006',
    'Đặng Văn Hải',
    'Electrical Officer',
    'E/O',
    'ENGINE',
    'Vietnam',
    '1987-12-08'::timestamptz,
    '2022-06-01'::timestamptz,
    '2025-10-20'::timestamptz,
    '2026-04-20'::timestamptz,
    true,
    'electrical.officer@annie-gas09.com',
    '+84906234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 7. Fourth Engineer (Máy 4)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW007',
    'Bùi Quang Minh',
    'Fourth Engineer',
    '4/E',
    'ENGINE',
    'Vietnam',
    '1990-06-18'::timestamptz,
    '2024-01-15'::timestamptz,
    '2025-11-20'::timestamptz,
    '2026-05-20'::timestamptz,
    true,
    '4th.engineer@annie-gas09.com',
    '+84907234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 8-9. Fitters (Thợ cơ khí)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES 
(
    'CREW008',
    'Hoàng Văn Đức',
    'Fitter',
    'FITTER',
    'ENGINE',
    'Vietnam',
    '1992-04-22'::timestamptz,
    '2024-03-01'::timestamptz,
    '2025-12-01'::timestamptz,
    '2026-06-01'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
),
(
    'CREW009',
    'Ngô Văn Sơn',
    'Fitter',
    'FITTER',
    'ENGINE',
    'Vietnam',
    '1993-08-30'::timestamptz,
    '2024-04-01'::timestamptz,
    '2025-12-10'::timestamptz,
    '2026-06-10'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 10-11. Oilers (Thợ dầu)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES 
(
    'CREW010',
    'Lý Văn Thắng',
    'Oiler',
    'OILER',
    'ENGINE',
    'Vietnam',
    '1994-02-14'::timestamptz,
    '2024-05-01'::timestamptz,
    '2025-12-15'::timestamptz,
    '2026-06-15'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
),
(
    'CREW011',
    'Phan Văn Tài',
    'Oiler',
    'OILER',
    'ENGINE',
    'Vietnam',
    '1995-10-05'::timestamptz,
    '2024-06-01'::timestamptz,
    '2025-12-20'::timestamptz,
    '2026-06-20'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- ============================================================
-- DECK DEPARTMENT (Bộ phận boong)
-- ============================================================

-- 12. Second Officer (Nhì phó)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW012',
    'Dương Minh Quân',
    'Second Officer',
    '2/O',
    'DECK',
    'Vietnam',
    '1986-03-28'::timestamptz,
    '2023-05-01'::timestamptz,
    '2025-11-05'::timestamptz,
    '2026-05-05'::timestamptz,
    true,
    '2nd.officer@annie-gas09.com',
    '+84912234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 13. Third Officer (Tam phó)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, email_address, phone_number, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW013',
    'Trịnh Văn Hùng',
    'Third Officer',
    '3/O',
    'DECK',
    'Vietnam',
    '1989-07-16'::timestamptz,
    '2024-02-01'::timestamptz,
    '2025-11-15'::timestamptz,
    '2026-05-15'::timestamptz,
    true,
    '3rd.officer@annie-gas09.com',
    '+84913234567',
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 14. Bosun (Thủy thủ trưởng) - KEY PERSON for Ship's Crew
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW014',
    'Vũ Văn Bình',
    'Bosun',
    'BOSUN',
    'DECK',
    'Vietnam',
    '1983-01-20'::timestamptz,
    '2021-08-01'::timestamptz,
    '2025-10-25'::timestamptz,
    '2026-04-25'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 15-17. Able Seamen (Thủy thủ có bằng)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES 
(
    'CREW015',
    'Mai Văn Dũng',
    'Able Seaman',
    'AB',
    'DECK',
    'Vietnam',
    '1991-09-10'::timestamptz,
    '2024-03-15'::timestamptz,
    '2025-12-05'::timestamptz,
    '2026-06-05'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
),
(
    'CREW016',
    'Đinh Văn Lâm',
    'Able Seaman',
    'AB',
    'DECK',
    'Vietnam',
    '1992-11-25'::timestamptz,
    '2024-04-15'::timestamptz,
    '2025-12-08'::timestamptz,
    '2026-06-08'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
),
(
    'CREW017',
    'Hồ Văn Phúc',
    'Able Seaman',
    'AB',
    'DECK',
    'Vietnam',
    '1993-05-30'::timestamptz,
    '2024-05-15'::timestamptz,
    '2025-12-12'::timestamptz,
    '2026-06-12'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- 18-19. Ordinary Seamen (Thủy thủ thường)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES 
(
    'CREW018',
    'Châu Văn Toàn',
    'Ordinary Seaman',
    'OS',
    'DECK',
    'Vietnam',
    '1996-03-12'::timestamptz,
    '2024-08-01'::timestamptz,
    '2025-12-20'::timestamptz,
    '2026-06-20'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
),
(
    'CREW019',
    'Lương Văn Kiên',
    'Ordinary Seaman',
    'OS',
    'DECK',
    'Vietnam',
    '1997-07-08'::timestamptz,
    '2024-09-01'::timestamptz,
    '2025-12-25'::timestamptz,
    '2026-06-25'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- ============================================================
-- CATERING DEPARTMENT (Optional - some ships have)
-- ============================================================

-- 20. Chief Cook (Đầu bếp trưởng)
INSERT INTO crew_members (
    crew_id, full_name, position, rank, department,
    nationality, date_of_birth, join_date, embark_date, contract_end,
    is_onboard, is_synced, origin_node, created_at, updated_at
) VALUES (
    'CREW020',
    'Nguyễn Văn Hải',
    'Chief Cook',
    'COOK',
    'CATERING',
    'Vietnam',
    '1984-06-15'::timestamptz,
    '2023-03-01'::timestamptz,
    '2025-11-01'::timestamptz,
    '2026-05-01'::timestamptz,
    true,
    false,
    'SHIP_01',
    NOW(),
    NOW()
);

-- ============================================================
-- CREATE USERS (Link crew to login accounts)
-- Password default: ddmmyyyy (from DateOfBirth)
-- Hash of "15031975" for example (use bcrypt in production)
-- ============================================================

-- For demo purposes, using simple hash
-- In production: Use BCrypt.Net.BCrypt.HashPassword()

-- All users assigned to role_id = 5 (CREW/USER role)
-- You can update individual role_id later based on your roles table

-- 1-3. Management
INSERT INTO users (id, username, password_hash, role_id, crew_id, is_active, created_at)
VALUES 
(1, 'CREW001', '$2a$11$XYZ...MASTER_HASH...', 5, 'CREW001', true, NOW()),
(2, 'CREW002', '$2a$11$XYZ...CE_HASH...', 5, 'CREW002', true, NOW()),
(3, 'CREW003', '$2a$11$XYZ...CO_HASH...', 5, 'CREW003', true, NOW());

-- 4-7. Engineers
INSERT INTO users (id, username, password_hash, role_id, crew_id, is_active, created_at)
VALUES 
(4, 'CREW004', '$2a$11$XYZ...2E_HASH...', 5, 'CREW004', true, NOW()),
(5, 'CREW005', '$2a$11$XYZ...3E_HASH...', 5, 'CREW005', true, NOW()),
(6, 'CREW006', '$2a$11$XYZ...EO_HASH...', 5, 'CREW006', true, NOW()),
(7, 'CREW007', '$2a$11$XYZ...4E_HASH...', 5, 'CREW007', true, NOW());

-- 8-11. Engine Ratings
INSERT INTO users (id, username, password_hash, role_id, crew_id, is_active, created_at)
VALUES 
(8, 'CREW008', '$2a$11$XYZ...FITTER1_HASH...', 5, 'CREW008', true, NOW()),
(9, 'CREW009', '$2a$11$XYZ...FITTER2_HASH...', 5, 'CREW009', true, NOW()),
(10, 'CREW010', '$2a$11$XYZ...OILER1_HASH...', 5, 'CREW010', true, NOW()),
(11, 'CREW011', '$2a$11$XYZ...OILER2_HASH...', 5, 'CREW011', true, NOW());

-- 12-19. Deck Department
INSERT INTO users (id, username, password_hash, role_id, crew_id, is_active, created_at)
VALUES 
(12, 'CREW012', '$2a$11$XYZ...2O_HASH...', 5, 'CREW012', true, NOW()),
(13, 'CREW013', '$2a$11$XYZ...3O_HASH...', 5, 'CREW013', true, NOW()),
(14, 'CREW014', '$2a$11$XYZ...BOSUN_HASH...', 5, 'CREW014', true, NOW()),
(15, 'CREW015', '$2a$11$XYZ...AB1_HASH...', 5, 'CREW015', true, NOW()),
(16, 'CREW016', '$2a$11$XYZ...AB2_HASH...', 5, 'CREW016', true, NOW()),
(17, 'CREW017', '$2a$11$XYZ...AB3_HASH...', 5, 'CREW017', true, NOW()),
(18, 'CREW018', '$2a$11$XYZ...OS1_HASH...', 5, 'CREW018', true, NOW()),
(19, 'CREW019', '$2a$11$XYZ...OS2_HASH...', 5, 'CREW019', true, NOW());

-- 20. Cook
INSERT INTO users (id, username, password_hash, role_id, crew_id, is_active, created_at)
VALUES (20, 'CREW020', '$2a$11$XYZ...COOK_HASH...', 5, 'CREW020', true, NOW());

-- ============================================================
-- VERIFICATION QUERIES
-- ============================================================

-- Check crew count by department
SELECT 
    department, 
    COUNT(*) as crew_count
FROM crew_members
WHERE crew_id LIKE 'CREW%'
GROUP BY department
ORDER BY department;

-- Check key positions
SELECT 
    crew_id,
    full_name,
    position,
    rank,
    department,
    is_onboard
FROM crew_members
WHERE crew_id LIKE 'CREW%'
  AND rank IN ('MASTER', 'C/E', 'C/O', '2/E', '3/E', 'E/O')
ORDER BY crew_id;

-- Check users linked to crew
SELECT 
    u.username,
    u.crew_id,
    c.full_name,
    c.position,
    c.rank,
    u.is_active
FROM users u
JOIN crew_members c ON u.crew_id = c.crew_id
WHERE u.username LIKE 'CREW%'
ORDER BY u.username;
