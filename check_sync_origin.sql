SELECT "OriginNode", COUNT(*) as sync_count FROM sync_logs GROUP BY "OriginNode" ORDER BY sync_count DESC LIMIT 15;
