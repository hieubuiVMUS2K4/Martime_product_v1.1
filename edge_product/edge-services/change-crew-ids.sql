BEGIN;

ALTER TABLE users DROP CONSTRAINT IF EXISTS "FK_users_crew_members_crew_id";
ALTER TABLE crew_certificates DROP CONSTRAINT IF EXISTS "f_k_crew_certificates__crew_members_crew_member_id";
ALTER TABLE travel_documents DROP CONSTRAINT IF EXISTS "f_k_travel_documents_crew_members_crew_member_id";
ALTER TABLE employment_documents DROP CONSTRAINT IF EXISTS "f_k_employment_documents_crew_members_crew_member_id";
ALTER TABLE health_documents DROP CONSTRAINT IF EXISTS "f_k_health_documents_crew_members_crew_member_id";
ALTER TABLE seafarer_documents DROP CONSTRAINT IF EXISTS "f_k_seafarer_documents_crew_members_crew_member_id";
ALTER TABLE voyage_crew_assignments DROP CONSTRAINT IF EXISTS "f_k_voyage_crew_assignments_crew_members_crew_member_id";
ALTER TABLE drill_schedules DROP CONSTRAINT IF EXISTS "f_k_drill_schedules_crew_members_assigned_to_crew_id";
ALTER TABLE drill_logs DROP CONSTRAINT IF EXISTS "f_k_drill_logs_crew_members_conducted_by_crew_id";
ALTER TABLE drill_logs DROP CONSTRAINT IF EXISTS "f_k_drill_logs_crew_members_verified_by_crew_id";
ALTER TABLE service_records DROP CONSTRAINT IF EXISTS "f_k_service_records_crew_members_crew_member_id";
ALTER TABLE voyage_crew_change_plans DROP CONSTRAINT IF EXISTS "f_k_voyage_crew_change_plans_crew_members_crew_member_id";

CREATE TEMP TABLE crew_id_map AS
SELECT 
    id AS old_id,
    ('bd000001-0001-4000-a000-' || SUBSTRING(id::text FROM 25))::uuid AS new_id,
    crew_id AS old_crew_id,
    REPLACE(crew_id, 'CREW-', 'BD-') AS new_crew_id
FROM crew_members;

UPDATE crew_certificates SET crew_member_id = m.new_id FROM crew_id_map m WHERE crew_certificates.crew_member_id = m.old_id;
UPDATE voyage_crew_assignments SET crew_member_id = m.new_id FROM crew_id_map m WHERE voyage_crew_assignments.crew_member_id = m.old_id;
UPDATE voyage_crew_change_plans SET crew_member_id = m.new_id FROM crew_id_map m WHERE voyage_crew_change_plans.crew_member_id = m.old_id;
UPDATE service_records SET crew_member_id = m.new_id FROM crew_id_map m WHERE service_records.crew_member_id = m.old_id;
UPDATE users SET crew_id = m.new_id::text FROM crew_id_map m WHERE users.crew_id = m.old_id::text;
UPDATE drill_schedules SET assigned_to_crew_id = m.new_id FROM crew_id_map m WHERE drill_schedules.assigned_to_crew_id = m.old_id;
UPDATE drill_logs SET conducted_by_crew_id = m.new_id FROM crew_id_map m WHERE drill_logs.conducted_by_crew_id = m.old_id;
UPDATE drill_logs SET verified_by_crew_id = m.new_id FROM crew_id_map m WHERE drill_logs.verified_by_crew_id = m.old_id;
UPDATE travel_documents SET crew_member_id = m.new_id FROM crew_id_map m WHERE travel_documents.crew_member_id = m.old_id;
UPDATE employment_documents SET crew_member_id = m.new_id FROM crew_id_map m WHERE employment_documents.crew_member_id = m.old_id;
UPDATE health_documents SET crew_member_id = m.new_id FROM crew_id_map m WHERE health_documents.crew_member_id = m.old_id;
UPDATE seafarer_documents SET crew_member_id = m.new_id FROM crew_id_map m WHERE seafarer_documents.crew_member_id = m.old_id;

UPDATE crew_members SET 
    id = m.new_id,
    crew_id = m.new_crew_id,
    is_synced = false,
    updated_at = NOW()
FROM crew_id_map m
WHERE crew_members.id = m.old_id;

-- Note: FK_users_crew_members_crew_id NOT re-added (users.crew_id is varchar, crew_members.id is uuid - incompatible types)
ALTER TABLE crew_certificates ADD CONSTRAINT "f_k_crew_certificates__crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE travel_documents ADD CONSTRAINT "f_k_travel_documents_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE employment_documents ADD CONSTRAINT "f_k_employment_documents_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE health_documents ADD CONSTRAINT "f_k_health_documents_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE seafarer_documents ADD CONSTRAINT "f_k_seafarer_documents_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE voyage_crew_assignments ADD CONSTRAINT "f_k_voyage_crew_assignments_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE drill_schedules ADD CONSTRAINT "f_k_drill_schedules_crew_members_assigned_to_crew_id" FOREIGN KEY (assigned_to_crew_id) REFERENCES crew_members(id);
ALTER TABLE drill_logs ADD CONSTRAINT "f_k_drill_logs_crew_members_conducted_by_crew_id" FOREIGN KEY (conducted_by_crew_id) REFERENCES crew_members(id);
ALTER TABLE drill_logs ADD CONSTRAINT "f_k_drill_logs_crew_members_verified_by_crew_id" FOREIGN KEY (verified_by_crew_id) REFERENCES crew_members(id);
ALTER TABLE service_records ADD CONSTRAINT "f_k_service_records_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);
ALTER TABLE voyage_crew_change_plans ADD CONSTRAINT "f_k_voyage_crew_change_plans_crew_members_crew_member_id" FOREIGN KEY (crew_member_id) REFERENCES crew_members(id);

DELETE FROM sync_queue;
UPDATE crew_certificates SET is_synced = false, updated_at = NOW();

SELECT id, crew_id, full_name FROM crew_members ORDER BY crew_id LIMIT 5;

DROP TABLE crew_id_map;

COMMIT;
