-- =============================================================
-- auto-crew-user-trigger.sql
-- Tự động tạo / đồng bộ tài khoản đăng nhập mobile từ crew_members
-- =============================================================
--
-- Cách dùng (PowerShell từ thư mục edge-services):
--   Get-Content -Raw .\Scripts\auto-crew-user-trigger.sql |
--     docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge
--
-- Sau khi chạy một lần:
--   - Trigger tự kích hoạt mỗi khi INSERT / UPDATE vào crew_members
--   - Backfill tạo ngay tài khoản cho tất cả crew_members hiện có
--
-- Quy tắc mật khẩu mặc định:
--   - ddMMYYYY (ngày tháng năm sinh), nếu không có DOB → "123456"
--   - Hash SHA-256 không có salt (legacy mode)
--   - App tự migrate sang PBKDF2 khi user đăng nhập lần đầu thành công
--   - must_change_password = TRUE → bắt buộc đổi mật khẩu sau lần đăng nhập đầu
-- =============================================================

BEGIN;

-- Extension cần thiết cho SHA-256
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Đảm bảo role USER tồn tại
INSERT INTO roles (role_code, role_name, description, is_active, created_at)
VALUES ('USER', 'User', 'Regular crew member', true, NOW())
ON CONFLICT (role_code) DO NOTHING;

-- =============================================================
-- Trigger function
-- =============================================================
CREATE OR REPLACE FUNCTION trg_auto_crew_user_fn()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_role_id  INT;
    v_pwd_raw  TEXT;
    v_pwd_hash TEXT;
BEGIN
    -- Lấy role_id của USER
    SELECT id INTO v_role_id
    FROM roles
    WHERE role_code = 'USER'
    LIMIT 1;

    IF v_role_id IS NULL THEN
        RAISE EXCEPTION 'Role USER not found in roles table';
    END IF;

    -- ---- INSERT: tạo tài khoản cho crew member mới ----
    IF TG_OP = 'INSERT' THEN
        v_pwd_raw  := COALESCE(to_char(NEW.date_of_birth, 'DDMMYYYY'), '123456');
        v_pwd_hash := encode(digest(v_pwd_raw, 'sha256'), 'base64');

        INSERT INTO users (
            username,
            password_hash,
            password_salt,
            role_id,
            crew_id,
            is_active,
            must_change_password,
            failed_login_attempts,
            created_at,
            updated_at
        )
        VALUES (
            NEW.crew_id,
            v_pwd_hash,
            NULL,       -- không có salt → legacy mode; app tự upgrade PBKDF2 khi login đầu
            v_role_id,
            NEW.crew_id,
            true,       -- tài khoản active ngay
            true,       -- bắt buộc đổi mật khẩu sau lần đăng nhập đầu tiên
            0,
            NOW(),
            NOW()
        )
        ON CONFLICT (username) DO NOTHING; -- bỏ qua nếu username đã tồn tại

    -- ---- UPDATE: đồng bộ trạng thái active theo is_onboard ----
    ELSIF TG_OP = 'UPDATE' THEN
        -- Khi trạng thái onboard thay đổi → bật/tắt tài khoản tương ứng
        IF OLD.is_onboard IS DISTINCT FROM NEW.is_onboard THEN
            UPDATE users
            SET    is_active  = NEW.is_onboard,
                   updated_at = NOW()
            WHERE  crew_id = NEW.crew_id;
        END IF;
    END IF;

    RETURN NEW;
END;
$$;

-- Xóa trigger cũ nếu có, sau đó tạo lại
DROP TRIGGER IF EXISTS trg_auto_crew_user ON crew_members;

CREATE TRIGGER trg_auto_crew_user
    AFTER INSERT OR UPDATE ON crew_members
    FOR EACH ROW
    EXECUTE FUNCTION trg_auto_crew_user_fn();

COMMIT;

-- =============================================================
-- Backfill: tạo tài khoản cho tất cả crew_members hiện có
-- (trigger không áp dụng hồi tố cho dữ liệu cũ)
-- =============================================================
INSERT INTO users (
    username,
    password_hash,
    password_salt,
    role_id,
    crew_id,
    is_active,
    must_change_password,
    failed_login_attempts,
    created_at,
    updated_at
)
SELECT
    cm.crew_id,
    encode(
        digest(COALESCE(to_char(cm.date_of_birth, 'DDMMYYYY'), '123456'), 'sha256'),
        'base64'
    ),
    NULL,
    (SELECT id FROM roles WHERE role_code = 'USER'),
    cm.crew_id,
    true,
    true,
    0,
    NOW(),
    NOW()
FROM crew_members cm
WHERE NOT EXISTS (
    SELECT 1 FROM users u WHERE u.crew_id = cm.crew_id
);

-- =============================================================
-- Kiểm tra kết quả
-- =============================================================
SELECT
    u.username,
    cm.full_name,
    r.role_code,
    u.is_active,
    u.must_change_password,
    u.created_at
FROM users u
LEFT JOIN crew_members cm ON cm.crew_id = u.crew_id
LEFT JOIN roles r ON r.id = u.role_id
WHERE r.role_code = 'USER'
ORDER BY u.username;
