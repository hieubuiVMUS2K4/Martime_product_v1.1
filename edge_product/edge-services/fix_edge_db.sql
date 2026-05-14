-- Edge fix script
DO $$ 
BEGIN
    UPDATE crew_members SET origin_node = '8765432' WHERE origin_node IN ('9412378', '9191919');
    UPDATE crew_certificates SET origin_node = '8765432' WHERE origin_node IN ('9412378', '9191919');
    UPDATE service_records SET origin_node = '8765432' WHERE origin_node IN ('9412378', '9191919');
    UPDATE maritime_reports SET origin_node = '8765432' WHERE origin_node IN ('9412378', '9191919');
    UPDATE position_data SET origin_node = '8765432' WHERE origin_node IN ('9412378', '9191919');
    UPDATE ship_data SET imo_number = '8765432', ship_name = 'MV MEKONG SPIRIT', call_sign = '3WXY8' WHERE imo_number IN ('9412378', '9191919');
    
    UPDATE sync_queue SET payload = REPLACE(payload, '9412378', '8765432') WHERE payload LIKE '%9412378%';
    UPDATE sync_queue SET payload = REPLACE(payload, '9191919', '8765432') WHERE payload LIKE '%9191919%';
END $$;
