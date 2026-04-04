BEGIN;

DELETE FROM sync_queue WHERE synced_at IS NULL;

DO $$
BEGIN
    IF (SELECT COUNT(*) FROM crew_members) = 0 THEN
        RAISE EXCEPTION 'crew_members table is empty. Seed crew data first.';
    END IF;
END
$$;

WITH expanded AS (
    SELECT
        gs AS seq,
        selected.id,
        selected.crew_id,
        selected.full_name,
        COALESCE(selected.origin_node, 'EDGE') AS origin_node,
        format(
            '%s%s',
            left(selected.full_name, GREATEST(1, 200 - length(format(' BENCH-20260322-055055-01-%s', lpad(gs::text, 6, '0'))))),
            format(' BENCH-20260322-055055-01-%s', lpad(gs::text, 6, '0'))
        ) AS bench_name,
        NOW() + make_interval(secs => (gs % 300)) + (gs * interval '1 millisecond') AS bench_updated_at
    FROM generate_series(1, 100) AS gs
    JOIN LATERAL (
        SELECT id, crew_id, full_name, origin_node
        FROM crew_members
        ORDER BY id
        OFFSET ((gs - 1) % (SELECT COUNT(*) FROM crew_members))
        LIMIT 1
    ) AS selected ON TRUE
),
latest_per_crew AS (
    SELECT DISTINCT ON (id)
        id,
        bench_name,
        bench_updated_at
    FROM expanded
    ORDER BY id, seq DESC
),
updated AS (
    UPDATE crew_members c
    SET full_name = l.bench_name,
        updated_at = l.bench_updated_at,
        is_synced = false
    FROM latest_per_crew l
    WHERE c.id = l.id
    RETURNING c.id
)
INSERT INTO sync_queue (
    table_name,
    record_key,
    action_type,
    payload,
    priority,
    retry_count,
    max_retries,
    next_retry_at,
    last_error,
    created_at,
    synced_at
)
SELECT
    'crew_member',
    e.id::text,
    1,
    json_build_object(
        'Id', e.id,
        'CrewId', e.crew_id,
        'FullName', e.bench_name,
        'UpdatedAt', to_char(e.bench_updated_at AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"'),
        'IsSynced', false,
        'OriginNode', e.origin_node
    )::text,
    2,
    0,
    5,
    NULL,
    NULL,
    NOW() + (e.seq * interval '1 millisecond'),
    NULL
FROM expanded e
ORDER BY e.seq;

COMMIT;