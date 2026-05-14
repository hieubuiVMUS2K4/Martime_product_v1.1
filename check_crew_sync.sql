-- Check crew members origin nodes (lowercase table name)
SELECT origin_node, COUNT(*) as crew_count FROM crew_members GROUP BY origin_node ORDER BY crew_count DESC;

-- Check sync logs origin 
SELECT origin_node, COUNT(*) as sync_count FROM sync_logs GROUP BY origin_node ORDER BY sync_count DESC LIMIT 10;
