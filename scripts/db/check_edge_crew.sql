-- Edge DB: check crew_members origin_node
SELECT origin_node, COUNT(*) FROM crew_members GROUP BY origin_node ORDER BY count DESC;
