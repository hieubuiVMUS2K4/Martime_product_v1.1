CREATE TABLE IF NOT EXISTS vessel_certificate_assignments (
    "Id" SERIAL PRIMARY KEY,
    "CertificateId" INTEGER NOT NULL,
    "VesselId" UUID NOT NULL,
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "IsSynced" BOOLEAN NOT NULL DEFAULT FALSE,
    "LastSyncedAt" TIMESTAMP WITH TIME ZONE NULL,
    UNIQUE("VesselId", "CertificateId"),
    FOREIGN KEY ("CertificateId") REFERENCES certificates("Id") ON DELETE CASCADE,
    FOREIGN KEY ("VesselId") REFERENCES "Vessels"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_vca_vessel_id ON vessel_certificate_assignments("VesselId");
CREATE INDEX IF NOT EXISTS idx_vca_is_synced ON vessel_certificate_assignments("IsSynced");

-- Seed: assign ALL active certificates to the only vessel currently in the database
INSERT INTO vessel_certificate_assignments ("CertificateId", "VesselId", "AssignedAt", "IsSynced")
SELECT c."Id", v."Id", NOW(), FALSE
FROM certificates c
CROSS JOIN "Vessels" v
WHERE c."IsActive" = TRUE
ON CONFLICT ("VesselId", "CertificateId") DO NOTHING;
