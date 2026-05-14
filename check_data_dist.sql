-- Check crew members origin nodes
SELECT "OriginNode", COUNT(*) as crew_count FROM "CrewMembers" GROUP BY "OriginNode" ORDER BY crew_count DESC;

-- Check positions origin by vessel id
SELECT v."IMO", v."Name", COUNT(p."VesselId") as pos_count 
FROM "Vessels" v 
LEFT JOIN "VesselPositions" p ON v."Id" = p."VesselId" 
GROUP BY v."IMO", v."Name" 
ORDER BY pos_count DESC;

-- Check sync logs origin
SELECT "OriginNode", COUNT(*) as sync_count FROM "SyncLogs" GROUP BY "OriginNode" ORDER BY sync_count DESC LIMIT 10;
