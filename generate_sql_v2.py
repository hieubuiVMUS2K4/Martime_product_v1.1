import uuid
from datetime import datetime, timedelta

start_date = datetime(2026, 6, 30, 12, 0, 0)
sql_statements = []
sql_statements.append('DO $$')
sql_statements.append('DECLARE')
sql_statements.append('    v_report_type_id INT;')
sql_statements.append('BEGIN')
sql_statements.append('    -- Get ReportTypeId for NOON report')
sql_statements.append('    SELECT id INTO v_report_type_id FROM report_types WHERE type_code = \'NOON\' LIMIT 1;')
sql_statements.append('')

for i in range(20):
    current_date = start_date + timedelta(days=i)
    date_str = current_date.strftime('%Y-%m-%d %H:%M:%S')
    report_num_date = current_date.strftime('%Y%m%d')
    
    maritime_id = str(uuid.uuid4())
    noon_id = str(uuid.uuid4())
    
    # Defaults
    sog = 12.0
    rpm = 100.0
    fuel = 25.0
    remarks = 'Normal operation.'
    
    # Abnormal days
    if i == 4: # 2026-07-04
        sog = 11.8
        rpm = 102.5
        fuel = 48.5
        remarks = 'Abnormal fuel consumption detected.'
    elif i == 11: # 2026-07-11
        sog = 5.5
        rpm = 112.0
        fuel = 26.0
        remarks = 'Heavy weather, high propeller slip.'
    elif i == 17: # 2026-07-17
        sog = 12.1
        rpm = 101.0
        fuel = 42.0
        remarks = 'Suspected hull fouling or sensor drift.'
        
    report_data = f'{{"SOG": {sog}, "RPM": {rpm}, "Fuel": {fuel}}}'
    
    # IsSynced = false, IsTransmitted = false so it pushes to shore
    sql_statements.append(f'    -- Day {i+1}: {current_date.strftime("%Y-%m-%d")}')
    sql_statements.append(f'    INSERT INTO maritime_reports ')
    sql_statements.append(f'    (id, report_number, report_type_id, report_date_time, status, prepared_by, report_data, remarks, is_transmitted, is_synced, created_at, origin_node)')
    sql_statements.append(f'    VALUES ')
    sql_statements.append(f'    (\'{maritime_id}\', \'RPT-{report_num_date}-200{i}\', v_report_type_id, \'{date_str}\', \'SUBMITTED\', \'Captain Jack\', \'{report_data}\', \'{remarks}\', false, false, NOW(), \'SHIP_01\');')
    sql_statements.append('')
    
    # For NoonReports table, calculate distance based on SOG approx (24 hours)
    distance = round(sog * 24, 1)
    dist_to_go = max(0, 4000 - (i * 280))
    power = round(rpm * 80, 1)
    
    sql_statements.append(f'    INSERT INTO noon_reports ')
    sql_statements.append(f'    (id, maritime_report_id, report_date, latitude, longitude, course_over_ground, speed_over_ground, distance_traveled, distance_to_go, weather_conditions, sea_state, wind_speed, fuel_oil_consumed, main_engine_r_p_m, main_engine_power, crew_on_board, created_at)')
    sql_statements.append(f'    VALUES ')
    sql_statements.append(f'    (\'{noon_id}\', \'{maritime_id}\', \'{date_str}\', 15.0, 110.0, 45.0, {sog}, {distance}, {dist_to_go}, \'FAIR\', \'MODERATE\', 15.0, {fuel}, {rpm}, {power}, 22, NOW());')
    sql_statements.append('')

sql_statements.append('END $$;')

with open('f:/NCKH/Product/Martime_product_v1.1/insert_20_reports.sql', 'w', encoding='utf-8') as f:
    f.write('\n'.join(sql_statements))

print("SQL generated at f:/NCKH/Product/Martime_product_v1.1/insert_20_reports.sql")
