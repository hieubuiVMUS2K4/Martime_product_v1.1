-- ═══════════════════════════════════════════════════════════════
-- Migration: Add PMS/Alarm summary columns to noon_reports
-- Run on BOTH edge and shore PostgreSQL databases
-- Safe to re-run (uses IF NOT EXISTS pattern)
-- ═══════════════════════════════════════════════════════════════

DO $$
BEGIN
    -- MaintenanceSummaryJson - JSON snapshot of PMS summary at transmit time
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'noon_reports' AND column_name = 'MaintenanceSummaryJson'
    ) THEN
        ALTER TABLE noon_reports ADD COLUMN "MaintenanceSummaryJson" TEXT NULL;
    END IF;

    -- AlarmSummaryJson - JSON snapshot of alarm summary at transmit time
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'noon_reports' AND column_name = 'AlarmSummaryJson'
    ) THEN
        ALTER TABLE noon_reports ADD COLUMN "AlarmSummaryJson" TEXT NULL;
    END IF;

    -- CertificatesExpiringSoon - crew certificates expiring within 30 days
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'noon_reports' AND column_name = 'CertificatesExpiringSoon'
    ) THEN
        ALTER TABLE noon_reports ADD COLUMN "CertificatesExpiringSoon" INTEGER NULL;
    END IF;
END $$;

-- Verify
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'noon_reports' 
  AND column_name IN ('MaintenanceSummaryJson', 'AlarmSummaryJson', 'CertificatesExpiringSoon');
