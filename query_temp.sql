SELECT "Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround" FROM position_data WHERE "OriginNode" = '9234567' ORDER BY "Timestamp" DESC LIMIT 5;
