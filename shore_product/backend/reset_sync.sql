UPDATE sync_queue SET synced_at = NULL, retry_count = 0, last_error = NULL WHERE table_name LIKE '%report%';
