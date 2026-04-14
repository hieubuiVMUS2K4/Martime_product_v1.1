-- Check rank_certificates mappings
SELECT rc."RankId", r."RankName", rc."CertificateId", c."CertificateName" 
FROM rank_certificates rc 
JOIN ranks r ON r."Id" = rc."RankId" 
JOIN certificates c ON c."Id" = rc."CertificateId" 
ORDER BY rc."RankId", rc."CertificateId";

-- Check a specific crew member's rank
SELECT cm."Id", cm."FullName", cm."RankId", r."RankName" 
FROM crew_members cm 
LEFT JOIN ranks r ON r."Id" = cm."RankId"
WHERE cm."RankId" IS NOT NULL
LIMIT 5;
