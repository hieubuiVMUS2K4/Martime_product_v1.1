using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaritimeLogbooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ballast Water Record Book (BWM Convention compliance)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.ballast_water_record_books (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    operation_date_time timestamp with time zone NOT NULL,
                    operation_code character varying(5) NOT NULL,
                    operation_description text NOT NULL,
                    ballast_tank character varying(100) NOT NULL,
                    volume double precision NOT NULL,
                    start_latitude double precision NOT NULL,
                    start_longitude double precision NOT NULL,
                    start_date_time timestamp with time zone NOT NULL,
                    end_latitude double precision,
                    end_longitude double precision,
                    end_date_time timestamp with time zone,
                    water_depth double precision,
                    distance_from_land double precision,
                    exchange_volume_percentage double precision,
                    exchange_method character varying(30),
                    treatment_system_used boolean,
                    treatment_system_type character varying(200),
                    treatment_successful boolean,
                    treatment_details character varying(500),
                    exceptional_circumstances character varying(500),
                    salinity_before_exchange double precision,
                    salinity_after_exchange double precision,
                    port_name character varying(100),
                    reception_facility character varying(200),
                    receipt_number character varying(100),
                    officer_in_charge character varying(100) NOT NULL,
                    master_signature character varying(100),
                    signed_at timestamp with time zone,
                    remarks text,
                    is_synced boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01'
                );
                CREATE INDEX IF NOT EXISTS idx_ballast_operation_date ON public.ballast_water_record_books(operation_date_time DESC);
                CREATE INDEX IF NOT EXISTS idx_ballast_synced ON public.ballast_water_record_books(is_synced) WHERE is_synced = false;
            ");

            // Deck Log Book (SOLAS Chapter V compliance)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.deck_log_books (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    log_date_time timestamp with time zone NOT NULL,
                    watch_period character varying(10) NOT NULL,
                    officer_on_watch character varying(100) NOT NULL,
                    entry_type character varying(30) NOT NULL,
                    description text NOT NULL,
                    latitude double precision,
                    longitude double precision,
                    course_over_ground double precision,
                    speed_over_ground double precision,
                    heading double precision,
                    wind_direction double precision,
                    wind_speed double precision,
                    sea_state character varying(30),
                    visibility character varying(30),
                    weather_description text,
                    cargo_operations text,
                    ballast_operations text,
                    crew_changes text,
                    drills_conducted text,
                    pilot_on_board boolean,
                    pilot_name character varying(100),
                    tugs_assistance boolean,
                    tug_details character varying(200),
                    port_operations text,
                    anchor_operations text,
                    mooring_operations text,
                    safety_equipment_checks text,
                    pollution_incidents text,
                    master_signature character varying(100),
                    signed_at timestamp with time zone,
                    remarks text,
                    is_synced boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01'
                );
                CREATE INDEX IF NOT EXISTS idx_deck_log_date ON public.deck_log_books(log_date_time DESC);
                CREATE INDEX IF NOT EXISTS idx_deck_log_synced ON public.deck_log_books(is_synced) WHERE is_synced = false;
            ");

            // Engine Log Book (ISM Code requirement)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.engine_log_books (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    log_date_time timestamp with time zone NOT NULL,
                    watch_period character varying(10) NOT NULL,
                    engineer_on_watch character varying(100) NOT NULL,
                    main_engine_hours double precision,
                    auxiliary_engine_1_hours double precision,
                    auxiliary_engine_2_hours double precision,
                    auxiliary_engine_3_hours double precision,
                    boiler_hours double precision,
                    main_engine_rpm double precision,
                    main_engine_power_output double precision,
                    fuel_oil_consumption double precision,
                    diesel_oil_consumption double precision,
                    lubricating_oil_consumption double precision,
                    fresh_water_produced double precision,
                    fresh_water_consumed double precision,
                    fuel_oil_temperature double precision,
                    fuel_oil_pressure double precision,
                    lub_oil_pressure double precision,
                    lub_oil_temperature double precision,
                    cooling_water_temperature double precision,
                    exhaust_gas_temperature double precision,
                    turbocharger_rpm double precision,
                    scavenge_air_pressure double precision,
                    generator_1_kwh double precision,
                    generator_2_kwh double precision,
                    generator_3_kwh double precision,
                    shaft_generator_kwh double precision,
                    boiler_steam_pressure double precision,
                    boiler_water_level double precision,
                    separator_operations text,
                    maintenance_work text,
                    abnormal_conditions text,
                    safety_checks text,
                    pollution_prevention_checks text,
                    chief_engineer_signature character varying(100),
                    signed_at timestamp with time zone,
                    remarks text,
                    is_synced boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01'
                );
                CREATE INDEX IF NOT EXISTS idx_engine_log_date ON public.engine_log_books(log_date_time DESC);
                CREATE INDEX IF NOT EXISTS idx_engine_log_synced ON public.engine_log_books(is_synced) WHERE is_synced = false;
            ");

            // Garbage Record Book (MARPOL Annex V compliance)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.garbage_record_books (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    operation_date_time timestamp with time zone NOT NULL,
                    operation_type character varying(20) NOT NULL,
                    garbage_category character varying(5) NOT NULL,
                    garbage_description text NOT NULL,
                    estimated_amount double precision NOT NULL,
                    unit_of_measurement character varying(10) NOT NULL,
                    discharge_to_sea boolean NOT NULL DEFAULT false,
                    discharge_latitude double precision,
                    discharge_longitude double precision,
                    distance_from_nearest_land double precision,
                    discharge_to_reception_facility boolean NOT NULL DEFAULT false,
                    port_name character varying(100),
                    reception_facility_name character varying(200),
                    receipt_number character varying(100),
                    receipt_date timestamp with time zone,
                    incineration boolean NOT NULL DEFAULT false,
                    incinerator_type character varying(50),
                    comminuted_or_ground boolean NOT NULL DEFAULT false,
                    retained_on_board boolean NOT NULL DEFAULT false,
                    storage_location character varying(100),
                    cargo_residues_category character varying(50),
                    cargo_un_number character varying(20),
                    discharge_method text,
                    exceptional_discharge_circumstances text,
                    officer_in_charge character varying(100) NOT NULL,
                    master_signature character varying(100),
                    signed_at timestamp with time zone,
                    remarks text,
                    is_synced boolean NOT NULL DEFAULT false,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    origin_node character varying(50) NOT NULL DEFAULT 'SHIP_01'
                );
                CREATE INDEX IF NOT EXISTS idx_garbage_operation_date ON public.garbage_record_books(operation_date_time DESC);
                CREATE INDEX IF NOT EXISTS idx_garbage_synced ON public.garbage_record_books(is_synced) WHERE is_synced = false;
                CREATE INDEX IF NOT EXISTS idx_garbage_category ON public.garbage_record_books(garbage_category);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.ballast_water_record_books;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.deck_log_books;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.engine_log_books;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.garbage_record_books;");
        }
    }
}
