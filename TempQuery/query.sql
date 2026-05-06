SELECT "Timestamp", "Latitude", "Longitude", "OriginNode" FROM position_data WHERE "Latitude" < 1 OR "Longitude" < 1 LIMIT 5;
