INSERT INTO sync_queue (table_name, record_key, action_type, payload, priority, created_at, synced_at, retry_count, next_retry_at, max_retries)
VALUES 
('crew_member', '9f96602d-85c8-4c56-9f1b-ad7abedb7cc4', 1, '{"OnboardStatus":"Approved","OnboardStatusChangedAt":"2026-03-18T07:57:00.565664Z","OnboardStatusChangedBy":"Captain","IsOnboard":true,"EdgeChangesViewed":false}', 0, now(), NULL, 0, NULL, 5),
('crew_member', '2f6bad79-e2a4-401a-b95c-d697c3c9007d', 1, '{"OnboardStatus":"Approved","OnboardStatusChangedAt":"2026-03-18T07:57:00Z","OnboardStatusChangedBy":"Captain","IsOnboard":true,"EdgeChangesViewed":false}', 0, now(), NULL, 0, NULL, 5);
