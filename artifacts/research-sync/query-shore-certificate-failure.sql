SELECT "Id", "Direction", "OriginNode", "TableName", "RecordKey", "ActionType", "Status", "ConflictDetail", "ProcessedAt"
FROM sync_logs
WHERE "TableName" = 'crew_certificate'
  AND "RecordKey" = '-2147482647'
ORDER BY "ProcessedAt" DESC
LIMIT 10;

SELECT "Id", "CrewMemberId", "DocumentNumber", "CertificateName", "IssueDate", "ExpiryDate", "FilePath", "CreatedAt"
FROM crew_certificates
WHERE "DocumentNumber" LIKE 'RS-LARGE-%'
ORDER BY "CreatedAt" DESC
LIMIT 5;
