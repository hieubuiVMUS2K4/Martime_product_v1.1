-- Maritime Edge Database Initialization Script
-- This script creates all tables for fresh database setup
-- For teammates: docker-compose will auto-run this on first startup

-- Drop existing tables if any (for clean reinstall)
DROP TABLE IF EXISTS __efmigrationshistory CASCADE;

-- Core System Tables
CREATE TABLE IF NOT EXISTS public.ais_data (
    id uuid NOT NULL,
    timestamp timestamp with time zone NOT NULL,
    mmsi character varying(9) NOT NULL,
    message_type integer NOT NULL,
    navigation_status integer,
    rate_of_turn numeric(6,2),
    speed_over_ground numeric(5,2),
    position_accuracy boolean,
    latitude numeric(10,7),
    longitude numeric(10,7),
    course_over_ground numeric(5,2),
    true_heading integer,
    imo_number character varying(7),
    call_sign character varying(7),
    ship_name character varying(120),
    ship_type integer,
    dimension_bow integer,
    dimension_stern integer,
    dimension_port integer,
    dimension_starboard integer,
    eta_month integer,
    eta_day integer,
    eta_hour integer,
    eta_minute integer,
    draught numeric(4,2),
    destination character varying(120),
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_ais_data PRIMARY KEY (id)
);

-- Equipment Assets
CREATE TABLE IF NOT EXISTS public.equipment_assets (
    id uuid NOT NULL,
    asset_code character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    category character varying(100),
    manufacturer character varying(200),
    model character varying(200),
    serial_number character varying(100),
    installation_date timestamp with time zone,
    location character varying(200),
    status character varying(50) NOT NULL DEFAULT 'Active',
    running_hours_at_installation numeric(10,2) DEFAULT 0,
    current_running_hours numeric(10,2) DEFAULT 0,
    notes text,
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_equipment_assets PRIMARY KEY (id),
    CONSTRAINT uk_equipment_assets_asset_code UNIQUE (asset_code)
);

-- Equipment Groups (PMS Planning)
CREATE TABLE IF NOT EXISTS public.equipment_groups (
    id uuid NOT NULL,
    group_code character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    category character varying(100),
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_equipment_groups PRIMARY KEY (id),
    CONSTRAINT uk_equipment_groups_group_code UNIQUE (group_code)
);

-- Equipment Group Members
CREATE TABLE IF NOT EXISTS public.equipment_group_members (
    id uuid NOT NULL,
    group_id uuid NOT NULL,
    asset_id uuid NOT NULL,
    sequence_order integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_equipment_group_members PRIMARY KEY (id),
    CONSTRAINT fk_equipment_group_members_groups FOREIGN KEY (group_id) REFERENCES public.equipment_groups(id) ON DELETE CASCADE,
    CONSTRAINT fk_equipment_group_members_assets FOREIGN KEY (asset_id) REFERENCES public.equipment_assets(id) ON DELETE CASCADE
);

-- Maintenance Schedules
CREATE TABLE IF NOT EXISTS public.maintenance_schedules (
    id uuid NOT NULL,
    schedule_name character varying(200) NOT NULL,
    equipment_group_id uuid NOT NULL,
    task_type_id uuid NOT NULL,
    interval_type character varying(50) NOT NULL,
    interval_value integer NOT NULL,
    interval_unit character varying(50),
    last_maintenance_date timestamp with time zone,
    last_running_hours numeric(10,2),
    next_due_date timestamp with time zone,
    next_due_running_hours numeric(10,2),
    priority character varying(50) NOT NULL DEFAULT 'Medium',
    estimated_duration_hours numeric(5,2),
    responsible_person character varying(200),
    is_active boolean NOT NULL DEFAULT true,
    notes text,
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_maintenance_schedules PRIMARY KEY (id),
    CONSTRAINT fk_maintenance_schedules_equipment_groups FOREIGN KEY (equipment_group_id) REFERENCES public.equipment_groups(id) ON DELETE CASCADE
);

-- Maintenance Tasks
CREATE TABLE IF NOT EXISTS public.maintenance_tasks (
    id uuid NOT NULL,
    task_code character varying(50) NOT NULL,
    schedule_id uuid,
    equipment_group_id uuid NOT NULL,
    task_name character varying(200) NOT NULL,
    description text,
    priority character varying(50) NOT NULL DEFAULT 'Medium',
    status character varying(50) NOT NULL DEFAULT 'Pending',
    assigned_to character varying(200),
    planned_start_date timestamp with time zone,
    planned_end_date timestamp with time zone,
    actual_start_date timestamp with time zone,
    actual_completion_date timestamp with time zone,
    estimated_duration_hours numeric(5,2),
    actual_duration_hours numeric(5,2),
    completion_notes text,
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_maintenance_tasks PRIMARY KEY (id),
    CONSTRAINT uk_maintenance_tasks_task_code UNIQUE (task_code),
    CONSTRAINT fk_maintenance_tasks_schedules FOREIGN KEY (schedule_id) REFERENCES public.maintenance_schedules(id) ON DELETE SET NULL,
    CONSTRAINT fk_maintenance_tasks_equipment_groups FOREIGN KEY (equipment_group_id) REFERENCES public.equipment_groups(id) ON DELETE CASCADE
);

-- Maintenance Task Details
CREATE TABLE IF NOT EXISTS public.maintenance_task_details (
    id uuid NOT NULL,
    task_id uuid NOT NULL,
    asset_id uuid NOT NULL,
    status character varying(50) NOT NULL DEFAULT 'Pending',
    assigned_to character varying(200),
    actual_start_date timestamp with time zone,
    actual_completion_date timestamp with time zone,
    completion_notes text,
    running_hours_before numeric(10,2),
    running_hours_after numeric(10,2),
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_maintenance_task_details PRIMARY KEY (id),
    CONSTRAINT fk_maintenance_task_details_tasks FOREIGN KEY (task_id) REFERENCES public.maintenance_tasks(id) ON DELETE CASCADE,
    CONSTRAINT fk_maintenance_task_details_assets FOREIGN KEY (asset_id) REFERENCES public.equipment_assets(id) ON DELETE CASCADE
);

-- Task Types
CREATE TABLE IF NOT EXISTS public.task_types (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    category character varying(100),
    default_duration_hours numeric(5,2),
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_task_types PRIMARY KEY (id),
    CONSTRAINT uk_task_types_name UNIQUE (name)
);

-- Spare Parts
CREATE TABLE IF NOT EXISTS public.spare_parts (
    id uuid NOT NULL,
    part_number character varying(100) NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    category character varying(100),
    unit character varying(50),
    quantity_in_stock integer NOT NULL DEFAULT 0,
    minimum_stock_level integer DEFAULT 0,
    unit_price numeric(10,2),
    location character varying(200),
    supplier character varying(200),
    is_synced boolean NOT NULL DEFAULT false,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01',
    CONSTRAINT pk_spare_parts PRIMARY KEY (id),
    CONSTRAINT uk_spare_parts_part_number UNIQUE (part_number)
);

-- Schedule Spare Parts
CREATE TABLE IF NOT EXISTS public.schedule_spare_parts (
    id uuid NOT NULL,
    schedule_id uuid NOT NULL,
    spare_part_id uuid NOT NULL,
    quantity_required integer NOT NULL DEFAULT 1,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_schedule_spare_parts PRIMARY KEY (id),
    CONSTRAINT fk_schedule_spare_parts_schedules FOREIGN KEY (schedule_id) REFERENCES public.maintenance_schedules(id) ON DELETE CASCADE,
    CONSTRAINT fk_schedule_spare_parts_spare_parts FOREIGN KEY (spare_part_id) REFERENCES public.spare_parts(id) ON DELETE CASCADE
);

-- Sync Queue
CREATE TABLE IF NOT EXISTS public.sync_queue (
    id uuid NOT NULL,
    entity_type character varying(100) NOT NULL,
    entity_id uuid NOT NULL,
    operation character varying(50) NOT NULL,
    payload jsonb,
    status character varying(50) NOT NULL DEFAULT 'Pending',
    retry_count integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_sync_queue PRIMARY KEY (id)
);

-- Users
CREATE TABLE IF NOT EXISTS public.users (
    id uuid NOT NULL,
    username character varying(100) NOT NULL,
    password_hash character varying(500) NOT NULL,
    full_name character varying(200),
    email character varying(200),
    role character varying(50) NOT NULL DEFAULT 'User',
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_users PRIMARY KEY (id),
    CONSTRAINT uk_users_username UNIQUE (username)
);

-- Mark migration as applied (so EF knows schema is up-to-date)
CREATE TABLE IF NOT EXISTS __efmigrationshistory (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___efmigrationshistory PRIMARY KEY (migration_id)
);

INSERT INTO __efmigrationshistory (migration_id, product_version) 
VALUES ('20251207094407_InitialCreate', '9.0.10')
ON CONFLICT (migration_id) DO NOTHING;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_equipment_group_members_group_id ON public.equipment_group_members(group_id);
CREATE INDEX IF NOT EXISTS idx_equipment_group_members_asset_id ON public.equipment_group_members(asset_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_schedules_equipment_group_id ON public.maintenance_schedules(equipment_group_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_tasks_schedule_id ON public.maintenance_tasks(schedule_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_tasks_equipment_group_id ON public.maintenance_tasks(equipment_group_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_task_details_task_id ON public.maintenance_task_details(task_id);
CREATE INDEX IF NOT EXISTS idx_maintenance_task_details_asset_id ON public.maintenance_task_details(asset_id);
CREATE INDEX IF NOT EXISTS idx_sync_queue_status ON public.sync_queue(status);
