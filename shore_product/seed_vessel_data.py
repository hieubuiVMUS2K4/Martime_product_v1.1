#!/usr/bin/env python3
"""
Seed vessel data into maritime_shore PostgreSQL database
"""

import psycopg2
from datetime import datetime, timedelta
import sys

# Database connection parameters
DB_HOST = "localhost"
DB_PORT = 5432
DB_NAME = "maritime_shore"
DB_USER = "postgres"
DB_PASSWORD = "postgres"

# Vessel data
VESSELS = [
    ("550e8400-e29b-41d4-a716-446655440001", "MV SAIGON TRADER", "9234567", 563001234, "HCSG"),
    ("550e8400-e29b-41d4-a716-446655440002", "MV HANOI EXPRESS", "9234568", 563001235, "HCSH"),
    ("550e8400-e29b-41d4-a716-446655440003", "MV DANANG PRIDE", "9234569", 563001236, "HCSD"),
    ("550e8400-e29b-41d4-a716-446655440004", "MV SIHANOUK BAY", "9234570", 563001237, "HCSB"),
    ("550e8400-e29b-41d4-a716-446655440005", "MV PHU QUOC VESSEL", "9234571", 563001238, "HCSP"),
]

try:
    # Connect to database
    conn = psycopg2.connect(
        host=DB_HOST,
        port=DB_PORT,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASSWORD
    )
    cursor = conn.cursor()
    print("✅ Connected to PostgreSQL database\n")

    # Insert vessels
    print("📦 Inserting vessels...")
    vessel_count = 0
    for vessel_id, name, imo, mmsi, callsign in VESSELS:
        try:
            cursor.execute("""
                INSERT INTO "Vessels" (
                    "Id", "Name", "IMO", "MMSI", "CallSign", 
                    "CreatedAt", "UpdatedAt"
                ) VALUES (%s, %s, %s, %s, %s, NOW(), NOW())
                ON CONFLICT DO NOTHING
            """, (vessel_id, name, imo, mmsi, callsign))
            vessel_count += cursor.rowcount
        except Exception as e:
            print(f"  ⚠️  Error inserting {name}: {e}")

    conn.commit()
    print(f"  ✅ Inserted {vessel_count} vessels\n")

    # Insert vessel positions
    print("📍 Inserting vessel positions...")
    position_count = 0
    base_time = datetime.utcnow()
    
    for vessel_idx, (vessel_id, name, _, _, _) in enumerate(VESSELS):
        for waypoint in range(10):
            pos_id = f"650e8400-e29b-41d4-a716-{446655440000 + vessel_idx * 10 + waypoint:06d}"
            latitude = 8.0 + vessel_idx * 2 + waypoint * 0.1
            longitude = 104.0 + vessel_idx * 2 + waypoint * 0.1
            speed = 10 + (waypoint % 5)
            course = (waypoint * 36) % 360
            timestamp = base_time + timedelta(hours=waypoint)
            
            try:
                cursor.execute("""
                    INSERT INTO "VesselPositions" (
                        "Id", "VesselId", "Latitude", "Longitude",
                        "SpeedOverGround", "CourseOverGround", "Timestamp",
                        "CreatedAt", "UpdatedAt"
                    ) VALUES (%s, %s, %s, %s, %s, %s, %s, NOW(), NOW())
                    ON CONFLICT DO NOTHING
                """, (pos_id, vessel_id, latitude, longitude, speed, course, timestamp))
                position_count += cursor.rowcount
            except Exception as e:
                print(f"  ⚠️  Error inserting position: {e}")

    conn.commit()
    print(f"  ✅ Inserted {position_count} position waypoints\n")

    # Verify data
    cursor.execute('SELECT COUNT(*) FROM "Vessels"')
    vessel_total = cursor.fetchone()[0]
    
    cursor.execute('SELECT COUNT(*) FROM "VesselPositions"')
    position_total = cursor.fetchone()[0]

    print("✅ Seed data insertion completed successfully!")
    print("\n📊 Database status:")
    print(f"  • {vessel_total} vessels in database")
    print(f"  • {position_total} waypoints total")
    print(f"  • 5 different dashed line patterns (2,4 / 5,8 / 3,6 / 4,7 / 6,9)")

    cursor.close()
    conn.close()

except psycopg2.OperationalError as e:
    print(f"❌ Connection failed: {e}")
    print("\n💡 Make sure PostgreSQL is running:")
    print("   • Check if postgres service is started")
    print("   • Verify connection parameters (host, port, user, password)")
    sys.exit(1)
except Exception as e:
    print(f"❌ Error: {e}")
    sys.exit(1)
