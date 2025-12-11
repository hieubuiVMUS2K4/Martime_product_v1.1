-- Create schedule_spare_parts table

CREATE TABLE IF NOT EXISTS schedule_spare_parts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    schedule_id UUID NOT NULL,
    material_item_id UUID NOT NULL,
    quantity_required NUMERIC(10,3) NOT NULL DEFAULT 1,
    is_mandatory BOOLEAN NOT NULL DEFAULT TRUE,
    notes VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_schedule_spare_parts_schedule FOREIGN KEY (schedule_id) 
        REFERENCES maintenance_schedules(id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_schedule_spare_parts_schedule_id ON schedule_spare_parts(schedule_id);
CREATE INDEX IF NOT EXISTS idx_schedule_spare_parts_material_id ON schedule_spare_parts(material_item_id);

-- Verify
SELECT table_name FROM information_schema.tables WHERE table_name = 'schedule_spare_parts';
