SELECT "Id", "DocumentType", "DocumentNumber", "CrewMemberId", "CreatedAt"
FROM health_documents
WHERE "DocumentNumber" = 'RS-REPRO-20260322231345';

SELECT "OriginNode", "TableName", "RecordKey", "Status", "ErrorMessage", "ProcessedAt"
FROM sync_logs
WHERE "TableName" = 'health_document'
  AND "RecordKey" = '0b4f9715-4a18-4879-a39d-02a0034eaf8d'
ORDER BY "ProcessedAt" DESC
LIMIT 5;
