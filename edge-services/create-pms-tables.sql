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
