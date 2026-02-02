-- ========================================================
-- CLEAR DATA FROM CERTIFICATE TABLES
-- Xóa tất cả dữ liệu trong 5 bảng: 
-- countries, country_certificates, certificates, 
-- crew_certificates, crew_members
-- ========================================================

-- Tắt foreign key constraint check tạm thời (nếu cần)
-- SET session_replication_role = 'replica';

BEGIN;

-- 1. Xóa dữ liệu từ bảng crew_certificates (phải xóa trước vì có FK đến certificates và crew_members)
DELETE FROM crew_certificates;
TRUNCATE TABLE crew_certificates RESTART IDENTITY CASCADE;

-- 2. Xóa dữ liệu từ bảng country_certificates (phải xóa trước vì có FK đến countries và certificates)
DELETE FROM country_certificates;
TRUNCATE TABLE country_certificates RESTART IDENTITY CASCADE;

-- 3. Xóa dữ liệu từ bảng crew_members
DELETE FROM crew_members;
-- Note: crew_members có UUID nên không RESTART IDENTITY

-- 4. Xóa dữ liệu từ bảng certificates
DELETE FROM certificates;
TRUNCATE TABLE certificates RESTART IDENTITY CASCADE;

-- 5. Xóa dữ liệu từ bảng countries
DELETE FROM countries;
TRUNCATE TABLE countries RESTART IDENTITY CASCADE;

COMMIT;

-- Bật lại foreign key constraint check
-- SET session_replication_role = 'origin';

-- Kiểm tra kết quả
SELECT 'crew_certificates' as table_name, COUNT(*) as count FROM crew_certificates
UNION ALL
SELECT 'country_certificates', COUNT(*) FROM country_certificates
UNION ALL
SELECT 'crew_members', COUNT(*) FROM crew_members
UNION ALL
SELECT 'certificates', COUNT(*) FROM certificates
UNION ALL
SELECT 'countries', COUNT(*) FROM countries;

-- Hiển thị thông báo thành công
SELECT 'All data cleared successfully!' as status;
