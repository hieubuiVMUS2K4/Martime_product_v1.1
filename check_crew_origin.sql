SELECT "OriginNode", COUNT(*) as crew_count FROM crew_members GROUP BY "OriginNode" ORDER BY crew_count DESC;
