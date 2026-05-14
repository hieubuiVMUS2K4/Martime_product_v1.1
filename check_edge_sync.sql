-- Edge DB: check sync_queue
SELECT table_name, origin_node, COUNT(*) FROM sync_queue GROUP BY table_name, origin_node ORDER BY count DESC LIMIT 20;

-- Check position_data origin
SELECT origin_node, COUNT(*) FROM position_data GROUP BY origin_node ORDER BY count DESC;

-- Check ship_data IMO  
SELECT imo_number, ship_name FROM ship_data LIMIT 5;
