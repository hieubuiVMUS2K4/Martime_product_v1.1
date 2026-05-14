-- Xem vessel 9412378 và 8765432 chi tiết
SELECT "Id", "IMO", "Name", "CreatedAt" FROM "Vessels" WHERE "IMO" IN ('9412378', '8765432', '9191919');

-- Crew data cho 9412378 (via OriginNode trong sync_logs = 9412378)
SELECT "OriginNode", "VesselId", COUNT(*) FROM crew_members WHERE "OriginNode" = '9412378' GROUP BY "OriginNode", "VesselId";

-- Crew data cho 8765432 với VesselId mapping
SELECT cm."OriginNode", v."IMO", v."Name", COUNT(cm."Id") as crew 
FROM crew_members cm 
LEFT JOIN "Vessels" v ON cm."VesselId" = v."Id"
GROUP BY cm."OriginNode", v."IMO", v."Name"
ORDER BY crew DESC;

-- Xem ID của vessel 9412378 và 8765432
SELECT "Id", "IMO", "Name" FROM "Vessels" WHERE "IMO" IN ('8765432', '9412378');
