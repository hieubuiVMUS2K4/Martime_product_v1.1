-- ============================================================
-- PMS PLANNING SYSTEM - DATABASE MIGRATION
-- ============================================================
-- Add 6 new tables for maintenance planning:
-- 1. equipment_assets - Equipment catalog
-- 2. maintenance_schedules - Periodic maintenance plans
-- 3. schedule_spare_parts - Required spare parts per schedule
-- 4. maintenance_histories - Execution history audit trail
-- 5. equipment_groups - Equipment grouping
-- 6. equipment_group_members - Many-to-many relationship

-- Table 1: Equipment Assets
CREATE TABLE IF NOT EXISTS public.equipment_assets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_code VARCHAR(50) NOT NULL UNIQUE,
    asset_name VARCHAR(200) NOT NULL,
    category VARCHAR(50) NOT NULL,
    manufacturer VARCHAR(200),
    model VARCHAR(100),
    serial_number VARCHAR(100),
    installation_date TIMESTAMP WITH TIME ZONE,
    current_running_hours DOUBLE PRECISION,
    last_running_hours_update TIMESTAMP WITH TIME ZONE,
    equipment_group_id UUID,
    location VARCHAR(100),
    criticality VARCHAR(20) NOT NULL DEFAULT 'NORMAL',
    technical_specs TEXT,
    notes TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_synced BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    origin_node VARCHAR(50) NOT NULL DEFAULT 'SHIP_01'
);

CREATE INDEX idx_equipment_assets_category ON public.equipment_assets(category);
CREATE INDEX idx_equipment_assets_criticality ON public.equipment_assets(criticality);
CREATE INDEX idx_equipment_assets_group_id ON public.equipment_assets(equipment_group_id);

-- Table 2: Maintenance Schedules
CREATE TABLE IF NOT EXISTS public.maintenance_schedules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    schedule_code VARCHAR(50) NOT NULL UNIQUE,
    asset_id UUID NOT NULL,
    task_type_id INTEGER NOT NULL,
    schedule_name VARCHAR(200) NOT NULL,
    interval_type VARCHAR(20) NOT NULL DEFAULT 'CALENDAR',
    interval_hours INTEGER,
    interval_days INTEGER,
    days_before_due INTEGER NOT NULL DEFAULT 7,
    last_executed_at TIMESTAMP WITH TIME ZONE,
    last_executed_running_hours DOUBLE PRECISION,
    next_due_date TIMESTAMP WITH TIME ZONE,
    next_due_running_hours DOUBLE PRECISION,
    priority VARCHAR(20) NOT NULL DEFAULT 'NORMAL',
    estimated_duration_hours DOUBLE PRECISION,
    auto_generate BOOLEAN NOT NULL DEFAULT true,
    instructions TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_synced BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    origin_node VARCHAR(50) NOT NULL DEFAULT 'SHIP_01'
);

CREATE INDEX idx_maintenance_schedules_asset_id ON public.maintenance_schedules(asset_id);
CREATE INDEX idx_maintenance_schedules_task_type_id ON public.maintenance_schedules(task_type_id);
CREATE INDEX idx_maintenance_schedules_next_due ON public.maintenance_schedules(next_due_date);
CREATE INDEX idx_maintenance_schedules_auto_generate ON public.maintenance_schedules(auto_generate) WHERE auto_generate = true;

-- Table 3: Schedule Spare Parts
CREATE TABLE IF NOT EXISTS public.schedule_spare_parts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    schedule_id UUID NOT NULL,
    material_item_id UUID NOT NULL,
    quantity_required DOUBLE PRECISION NOT NULL CHECK (quantity_required > 0),
    is_mandatory BOOLEAN NOT NULL DEFAULT true,
    notes VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_schedule_spare_parts_schedule_id ON public.schedule_spare_parts(schedule_id);
CREATE INDEX idx_schedule_spare_parts_material_id ON public.schedule_spare_parts(material_item_id);
CREATE UNIQUE INDEX idx_schedule_spare_parts_unique ON public.schedule_spare_parts(schedule_id, material_item_id);

-- Table 4: Maintenance History
CREATE TABLE IF NOT EXISTS public.maintenance_histories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    schedule_id UUID NOT NULL,
    task_id UUID NOT NULL,
    executed_at TIMESTAMP WITH TIME ZONE NOT NULL,
    executed_running_hours DOUBLE PRECISION,
    completed_by VARCHAR(100),
    actual_duration_hours DOUBLE PRECISION,
    spare_parts_used TEXT, -- JSON array
    total_spare_parts_cost DECIMAL,
    notes TEXT,
    condition_after VARCHAR(20),
    is_synced BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    origin_node VARCHAR(50) NOT NULL DEFAULT 'SHIP_01'
);

CREATE INDEX idx_maintenance_histories_schedule_id ON public.maintenance_histories(schedule_id);
CREATE INDEX idx_maintenance_histories_task_id ON public.maintenance_histories(task_id);
CREATE INDEX idx_maintenance_histories_executed_at ON public.maintenance_histories(executed_at);

-- Table 5: Equipment Groups
CREATE TABLE IF NOT EXISTS public.equipment_groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_code VARCHAR(50) NOT NULL UNIQUE,
    group_name VARCHAR(200) NOT NULL,
    category VARCHAR(50),
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_synced BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    origin_node VARCHAR(50) NOT NULL DEFAULT 'SHIP_01'
);

CREATE INDEX idx_equipment_groups_category ON public.equipment_groups(category);

-- Table 6: Equipment Group Members
CREATE TABLE IF NOT EXISTS public.equipment_group_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL,
    asset_id UUID NOT NULL,
    sequence_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_equipment_group_members_group_id ON public.equipment_group_members(group_id);
CREATE INDEX idx_equipment_group_members_asset_id ON public.equipment_group_members(asset_id);
CREATE UNIQUE INDEX idx_equipment_group_members_unique ON public.equipment_group_members(group_id, asset_id);

-- Grant permissions
GRANT ALL PRIVILEGES ON TABLE public.equipment_assets TO edge_user;
GRANT ALL PRIVILEGES ON TABLE public.maintenance_schedules TO edge_user;
GRANT ALL PRIVILEGES ON TABLE public.schedule_spare_parts TO edge_user;
GRANT ALL PRIVILEGES ON TABLE public.maintenance_histories TO edge_user;
GRANT ALL PRIVILEGES ON TABLE public.equipment_groups TO edge_user;
GRANT ALL PRIVILEGES ON TABLE public.equipment_group_members TO edge_user;

-- Success message
DO $$
BEGIN
    RAISE NOTICE '✓ PMS Planning System tables created successfully!';
    RAISE NOTICE '  - equipment_assets';
    RAISE NOTICE '  - maintenance_schedules';
    RAISE NOTICE '  - schedule_spare_parts';
    RAISE NOTICE '  - maintenance_histories';
    RAISE NOTICE '  - equipment_groups';
    RAISE NOTICE '  - equipment_group_members';
END $$;
