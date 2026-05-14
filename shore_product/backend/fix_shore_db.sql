-- Shore fix script
DO $$ 
DECLARE
    v_target_id uuid;
    v_old_id uuid;
BEGIN
    SELECT "Id" INTO v_target_id FROM "Vessels" WHERE "IMO" = '8765432';
    SELECT "Id" INTO v_old_id FROM "Vessels" WHERE "IMO" = '9412378';

    IF v_target_id IS NOT NULL AND v_old_id IS NOT NULL THEN
        UPDATE crew_members SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;
        UPDATE "VesselPositions" SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;
        UPDATE "FuelConsumptions" SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;
        UPDATE "PortCalls" SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;
        UPDATE "VesselAlerts" SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;
        UPDATE vessel_certificate_assignments SET "VesselId" = v_target_id WHERE "VesselId" = v_old_id;

        DELETE FROM "Vessels" WHERE "Id" = v_old_id;
    END IF;

    UPDATE crew_members SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE crew_certificates SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE service_records SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE maritime_reports SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE position_data SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE sync_logs SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE sync_idempotency_records SET "OriginNode" = '8765432' WHERE "OriginNode" = '9412378';
    UPDATE sync_idempotency_records SET "IdempotencyKey" = REPLACE("IdempotencyKey", '9412378', '8765432') WHERE "IdempotencyKey" LIKE '%9412378%';

    UPDATE "Vessels" SET "Name" = 'MV MEKONG SPIRIT', "CallSign" = '3WXY8', "VesselType" = 'Bulk Carrier' WHERE "IMO" = '8765432';
END $$;
