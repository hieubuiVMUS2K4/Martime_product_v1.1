import uuid
from datetime import datetime, timedelta
import random

# Start date
start_date = datetime(2026, 4, 16, 12, 0, 0)

# Base values
latitude = 10.0
longitude = 105.0
speed = 12.0
rpm = 100.0
power = 8000.0
fuel_consumption = 25.0
distance = 288.0

sql = ""

# Abnormal days indices
abnormal_indices = [5, 12, 20, 27] # e.g. 21/4, 28/4, 6/5, 13/5

sql += "DO $$\n"
sql += "DECLARE\n"
sql += "    v_report_type_id INT;\n"
sql += "BEGIN\n"
sql += "    -- Get ReportTypeId for NOON report\n"
sql += "    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = 'NOON' LIMIT 1;\n\n"
sql += "    IF v_report_type_id IS NULL THEN\n"
sql += "        INSERT INTO report_types (type_code, type_name, category, frequency, is_mandatory, requires_master_signature, is_active, created_at)\n"
sql += "        VALUES ('NOON', 'Noon Report', 'OPERATIONAL', 'DAILY', true, true, true, NOW())\n"
sql += "        RETURNING id INTO v_report_type_id;\n"
sql += "    END IF;\n\n"

for i in range(28): # From 16/4 to 13/5 is 28 days
    report_date = start_date + timedelta(days=i)
    
    # Normal variation
    lat_val = latitude + (i * 0.5)
    lon_val = longitude + (i * 0.2)
    sog = round(speed + random.uniform(-0.5, 0.5), 1)
    rpm_val = round(rpm + random.uniform(-2.0, 2.0), 1)
    power_val = round(power + random.uniform(-100, 100), 1)
    fuel = round(fuel_consumption + random.uniform(-1.0, 1.0), 1)
    dist = round(sog * 24, 1)
    
    remarks = "Normal operation."
    
    # Abnormal data injection
    if i in abnormal_indices:
        if i == 5:
            # High fuel consumption
            fuel = 45.5
            remarks = "Abnormal fuel consumption detected."
        elif i == 12:
            # Low speed, high RPM (propeller slip / weather)
            sog = 6.0
            dist = 144.0
            rpm_val = 110.0
            remarks = "Heavy weather, high propeller slip."
        elif i == 20:
            # Engine failure / low power
            power_val = 3000.0
            sog = 4.0
            dist = 96.0
            fuel = 10.0
            remarks = "Main engine issue, reduced power."
        elif i == 27:
            # High fuel, normal speed (sensor issue or hull fouling)
            fuel = 38.0
            remarks = "Suspected hull fouling or sensor drift."

    maritime_id = str(uuid.uuid4())
    noon_id = str(uuid.uuid4())
    report_number = f"RPT-{report_date.strftime('%Y%m%d')}-{i+1000}"
    
    report_data_json = f'{{"SOG": {sog}, "RPM": {rpm_val}, "Fuel": {fuel}, "Remarks": "{remarks}"}}'
    
    sql += f"""
    -- Day {i+1}: {report_date.strftime('%Y-%m-%d')}
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('{maritime_id}', '{report_number}', v_report_type_id, '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', 'SUBMITTED', 'Captain Jack', '{report_data_json}', '{remarks}', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('{noon_id}', '{maritime_id}', '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', {lat_val}, {lon_val}, 45.0, {sog}, {dist}, {5000 - dist}, 'FAIR', 'MODERATE', 15.0, {fuel}, {rpm_val}, {power_val}, 22, NOW());
"""

# Let's add two more reports to make it 30 as requested. (maybe just generate two more in the same date range, or extend to 15/5)
for i in range(28, 30):
    report_date = start_date + timedelta(days=i)
    
    # Normal variation
    lat_val = latitude + (i * 0.5)
    lon_val = longitude + (i * 0.2)
    sog = round(speed + random.uniform(-0.5, 0.5), 1)
    rpm_val = round(rpm + random.uniform(-2.0, 2.0), 1)
    power_val = round(power + random.uniform(-100, 100), 1)
    fuel = round(fuel_consumption + random.uniform(-1.0, 1.0), 1)
    dist = round(sog * 24, 1)
    
    remarks = "Normal operation."

    maritime_id = str(uuid.uuid4())
    noon_id = str(uuid.uuid4())
    report_number = f"RPT-{report_date.strftime('%Y%m%d')}-{i+1000}"
    
    report_data_json = f'{{"SOG": {sog}, "RPM": {rpm_val}, "Fuel": {fuel}, "Remarks": "{remarks}"}}'
    
    sql += f"""
    -- Day {i+1}: {report_date.strftime('%Y-%m-%d')}
    INSERT INTO maritime_reports 
    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)
    VALUES 
    ('{maritime_id}', '{report_number}', v_report_type_id, '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', 'SUBMITTED', 'Captain Jack', '{report_data_json}', '{remarks}', true, true, NOW(), 'SHIP_01');

    INSERT INTO noon_reports 
    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)
    VALUES 
    ('{noon_id}', '{maritime_id}', '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', {lat_val}, {lon_val}, 45.0, {sog}, {dist}, {5000 - dist}, 'FAIR', 'MODERATE', 15.0, {fuel}, {rpm_val}, {power_val}, 22, NOW());
"""


sql += "END $$;\n"

with open("f:/NCKH/Product/Martime_product_v1.1/edge_product/scripts/seed_noon_reports_abnormal.sql", "w") as f:
    f.write(sql)

print("SQL generated successfully.")
