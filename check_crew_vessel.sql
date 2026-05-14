-- Xem crew có VesselId trỏ đến vessel nào
SELECT v."IMO", v."Name", COUNT(cm.id) as crew_count 
FROM "Vessels" v 
LEFT JOIN crew_members cm ON v."Id" = cm."VesselId" 
GROUP BY v."IMO", v."Name" 
ORDER BY crew_count DESC;

-- Xem crew của OriginNode 8765432 có VesselId gì
SELECT "OriginNode", "VesselId", COUNT(*) as c 
FROM crew_members 
GROUP BY "OriginNode", "VesselId";

-- Xem sync_logs cho IMO 9412378 - TableName nào
SELECT "TableName", COUNT(*) as cnt 
FROM sync_logs 
WHERE "OriginNode" = '9412378' 
GROUP BY "TableName" 
ORDER BY cnt DESC;
