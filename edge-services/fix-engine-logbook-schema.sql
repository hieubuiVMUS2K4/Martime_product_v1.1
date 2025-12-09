-- Fix missing columns in engine_log_books table

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_status VARCHAR(50);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_rpm DECIMAL(6,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_load DECIMAL(5,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_coolant_temp DECIMAL(5,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_exhaust_temp DECIMAL(6,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_lube_oil_pressure DECIMAL(5,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_lube_oil_temp DECIMAL(5,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS main_engine_running_hours DECIMAL(10,2);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_oil_consumed_me DECIMAL(10,3);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_oil_consumed_ae DECIMAL(10,3);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_oil_consumed_boiler DECIMAL(10,3);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS lube_oil_consumed DECIMAL(10,3);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_unit VARCHAR(10) DEFAULT 'MT';

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine1_running BOOLEAN;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine1_running_hours DECIMAL(10,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine1_load DECIMAL(5,2);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine2_running BOOLEAN;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine2_running_hours DECIMAL(10,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine2_load DECIMAL(5,2);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine3_running BOOLEAN;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine3_running_hours DECIMAL(10,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS aux_engine3_load DECIMAL(5,2);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS boiler_in_operation BOOLEAN;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS boiler_pressure DECIMAL(5,2);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS boiler_water_level DECIMAL(5,2);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_oil_rob DECIMAL(10,3);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS fuel_oil_transfers VARCHAR(200);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS has_alarms BOOLEAN DEFAULT FALSE;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS alarms_description VARCHAR(500);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS maintenance_activities VARCHAR(500);

ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS chief_engineer_remarks TEXT;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS chief_engineer_signature VARCHAR(100);
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS signed_at TIMESTAMP WITH TIME ZONE;
ALTER TABLE engine_log_books ADD COLUMN IF NOT EXISTS remarks TEXT;

-- Also check DeckLogBook columns just in case
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS latitude DECIMAL(10,7);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS longitude DECIMAL(10,7);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS course_over_ground DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS speed_over_ground DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS heading DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS wind_direction VARCHAR(20);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS wind_speed DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS sea_state VARCHAR(20);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS visibility VARCHAR(30);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS barometric_pressure DECIMAL(7,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS air_temperature DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS sea_temperature DECIMAL(5,2);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS drill_type VARCHAR(50);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS drill_successful BOOLEAN;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS crew_on_board INTEGER;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS crew_changes VARCHAR(200);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS port_name VARCHAR(100);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS port_arrival_time TIMESTAMP WITH TIME ZONE;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS port_departure_time TIMESTAMP WITH TIME ZONE;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS pilot_name VARCHAR(100);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS pilot_on_board TIMESTAMP WITH TIME ZONE;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS pilot_off_board TIMESTAMP WITH TIME ZONE;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS master_signature VARCHAR(100);
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS signed_at TIMESTAMP WITH TIME ZONE;
ALTER TABLE deck_log_books ADD COLUMN IF NOT EXISTS remarks TEXT;
