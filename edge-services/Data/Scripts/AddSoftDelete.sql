-- ============================================================
-- Add Soft Delete Support to Maritime Reports
-- IMO Requirement: Retain all reports for minimum 3 years
-- ============================================================

-- Add soft delete columns
ALTER TABLE maritime_reports 
ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP WITH TIME ZONE,
ADD COLUMN IF NOT EXISTS deleted_by VARCHAR(100),
ADD COLUMN IF NOT EXISTS deleted_reason TEXT;

-- Create partial index for deleted reports (space efficient)
CREATE INDEX IF NOT EXISTS idx_report_deleted 
ON maritime_reports(deleted_at) 
WHERE deleted_at IS NOT NULL;

-- Create view for active (non-deleted) reports
CREATE OR REPLACE VIEW active_maritime_reports AS
SELECT * FROM maritime_reports WHERE deleted_at IS NULL;

-- Verify changes
\d maritime_reports

SELECT 'Soft delete support added successfully!' as status;
