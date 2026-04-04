DO $$
DECLARE
    v_tag text := '20260322231345';
    v_doc_id uuid := '0b4f9715-4a18-4879-a39d-02a0034eaf8d';
    v_doc_no text := 'RS-REPRO-20260322231345';
    v_notes text := 'research repro 20260322231345';
BEGIN
    INSERT INTO health_documents (
        id,
        crew_member_id,
        document_type,
        document_number,
        issue_date,
        expiry_date,
        file_url,
        notes,
        created_at,
        updated_at
    ) VALUES (
        v_doc_id,
        'bd000001-0001-4000-a000-000000000016',
        'ResearchRepro',
        v_doc_no,
        CURRENT_DATE,
        CURRENT_DATE + INTERVAL '30 days',
        NULL,
        v_notes,
        NOW(),
        NOW()
    );

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
    ) VALUES (
        'health_document',
        v_doc_id::text,
        0,
        json_build_object(
            'Id', v_doc_id::text,
            'CrewMemberId', 'bd000001-0001-4000-a000-000000000016',
            'CrewId', 'BD-016',
            'CrewFullName', 'Ahmad Hidayat BENCH',
            'CrewIdCardNumber', '13431414',
            'CrewDateOfBirth', '1980-02-07T00:00:00Z',
            'DocumentType', 'ResearchRepro',
            'DocumentNumber', v_doc_no,
            'IssueDate', '2026-03-22T00:00:00Z',
            'ExpiryDate', '2026-04-21T00:00:00Z',
            'Notes', v_notes,
            'IsActive', true,
            'UpdatedAt', '2026-03-22T23:13:45.000Z'
        )::text,
        2,
        0,
        5,
        NULL,
        NULL,
        NOW(),
        NULL
    );

    RAISE NOTICE 'Inserted repro tag=% doc_id=%', v_tag, v_doc_id;
END $$;
