-- ============================================================
-- Create Audit Trail for Maritime Report Workflow
-- Track all status changes for compliance and accountability
-- ============================================================

-- Create workflow history table
CREATE TABLE IF NOT EXISTS report_workflow_history (
    id BIGSERIAL PRIMARY KEY,
    maritime_report_id BIGINT NOT NULL,
    from_status VARCHAR(20) NOT NULL,
    to_status VARCHAR(20) NOT NULL,
    changed_by VARCHAR(100) NOT NULL,
    changed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    remarks TEXT,
    ip_address VARCHAR(50),
    user_agent VARCHAR(200),
    
    -- Foreign key constraint
    CONSTRAINT fk_workflow_report 
        FOREIGN KEY (maritime_report_id) 
        REFERENCES maritime_reports(id) 
        ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_workflow_history_report 
ON report_workflow_history(maritime_report_id);

CREATE INDEX IF NOT EXISTS idx_workflow_history_datetime 
ON report_workflow_history(changed_at DESC);

CREATE INDEX IF NOT EXISTS idx_workflow_history_status 
ON report_workflow_history(to_status);

-- Create view for recent workflow changes
CREATE OR REPLACE VIEW recent_workflow_changes AS
SELECT 
    rwh.id,
    rwh.maritime_report_id,
    mr.report_number,
    rwh.from_status,
    rwh.to_status,
    rwh.changed_by,
    rwh.changed_at,
    rwh.remarks
FROM report_workflow_history rwh
JOIN maritime_reports mr ON rwh.maritime_report_id = mr.id
ORDER BY rwh.changed_at DESC
LIMIT 100;

-- Verify
SELECT 'Audit trail created successfully!' as status;
SELECT COUNT(*) as workflow_history_count FROM report_workflow_history;
