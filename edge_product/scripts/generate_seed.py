import uuid
from datetime import datetime, timedelta
import random

# Start date
start_date = datetime(2026, 4, 14, 12, 0, 0)

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
abnormal_indices = [5, 12, 20, 27]

sql += "DO $$\n"
sql += "DECLARE\n"
sql += "    v_report_type_id INT;\n"
sql += "BEGIN\n"
sql += "    -- Get ReportTypeId for NOON report\n"
sql += "    SELECT \"Id\" INTO v_report_type_id FROM \"ReportTypes\" WHERE \"TypeCode\" = 'NOON' LIMIT 1;\n\n"
sql += "    IF v_report_type_id IS NULL THEN\n"
sql += "        INSERT INTO \"ReportTypes\" (\"TypeCode\", \"TypeName\", \"Category\", \"Frequency\", \"IsMandatory\", \"RequiresMasterSignature\", \"IsActive\", \"CreatedAt\")\n"
sql += "        VALUES ('NOON', 'Noon Report', 'OPERATIONAL', 'DAILY', true, true, true, NOW())\n"
sql += "        RETURNING \"Id\" INTO v_report_type_id;\n"
sql += "    END IF;\n\n"

for i in range(30):
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
    
    report_data_json = f'{{"SOG": {sog}, "RPM": {rpm_val}, "Fuel": {fuel}}}'
    
    sql += f"""
    -- Day {i+1}: {report_date.strftime('%Y-%m-%d')}
    INSERT INTO "MaritimeReports" 
    ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "Status", "PreparedBy", "ReportData", "Remarks", "IsTransmitted", "IsSynced", "CreatedAt", "OriginNode")
    VALUES 
    ('{maritime_id}', '{report_number}', v_report_type_id, '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', 'SUBMITTED', 'Captain Jack', '{report_data_json}', '{remarks}', true, true, NOW(), 'SHIP_01');

    INSERT INTO "NoonReports" 
    ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "WeatherConditions", "SeaState", "WindSpeed", "FuelOilConsumed", "MainEngineRPM", "MainEnginePower", "CrewOnBoard", "CreatedAt")
    VALUES 
    ('{noon_id}', '{maritime_id}', '{report_date.strftime('%Y-%m-%d %H:%M:%S')}', {lat_val}, {lon_val}, 45.0, {sog}, {dist}, {5000 - dist}, 'FAIR', 'MODERATE', 15.0, {fuel}, {rpm_val}, {power_val}, 22, NOW());
"""

sql += "END $$;\n"

with open("f:/NCKH/Product/Martime_product_v1.1/edge_product/scripts/seed_noon_reports.sql", "w") as f:
    f.write(sql)

print("SQL generated successfully.")
