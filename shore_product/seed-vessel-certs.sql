INSERT INTO vessel_certificate_assignments ("CertificateId", "VesselId", "AssignedAt", "IsSynced")
SELECT c."Id", v."Id", NOW(), FALSE
FROM certificates c
CROSS JOIN "Vessels" v
WHERE c."IsActive" = TRUE
ON CONFLICT ("VesselId", "CertificateId") DO NOTHING;
