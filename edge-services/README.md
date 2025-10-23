# 🚢 Maritime Edge Server - Ship-Based Data Collection System

## 📋 Overview

**Maritime Edge Server** là hệ thống thu thập dữ liệu trên tàu (ship-based edge computing), tuân thủ các tiêu chuẩn quốc tế:
- ✅ **SOLAS Chapter V** - Navigation & Safety requirements
- ✅ **IMO Resolution A.1106(29)** - Revised performance standards for ECDIS
- ✅ **NMEA 0183/2000** - Marine electronics data interchange
- ✅ **AIS Class A** - Automatic Identification System
- ✅ **IMO DCS** - Data Collection System for fuel consumption
- ✅ **EU MRV** - Monitoring, Reporting, Verification of CO2 emissions

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        SHIP EDGE SERVER                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ NMEA Parser  │  │ AIS Decoder  │  │ Modbus RTU   │          │
│  │ GPS/Gyro/Log │  │ VDM/VDO Msgs │  │ Engine Data  │          │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │
│         │                  │                  │                  │
│         └──────────────────┴──────────────────┘                  │
│                            │                                     │
│                  ┌─────────▼─────────┐                          │
│                  │  Data Processor   │                          │
│                  │  - Validation     │                          │
│                  │  - Aggregation    │                          │
│                  │  - Calculations   │                          │
│                  └─────────┬─────────┘                          │
│                            │                                     │
│         ┌──────────────────┼──────────────────┐                 │
│         │                  │                  │                 │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐         │
│  │   SQLite     │  │  Alert       │  │  MQTT        │         │
│  │   Local DB   │  │  Generator   │  │  Publisher   │         │
│  │  (Offline)   │  │              │  │  (Internal)  │         │
│  └──────┬───────┘  └──────────────┘  └──────────────┘         │
│         │                                                        │
│  ┌──────▼───────────────────────────────────┐                  │
│  │     Store-and-Forward Sync Service       │                  │
│  │  - Batch Upload (VSAT/4G LTE/Starlink)  │                  │
│  │  - Retry Logic with Exponential Backoff  │                  │
│  │  - Data Compression (gzip)                │                  │
│  │  - Bandwidth Optimization                 │                  │
│  └──────────────────┬────────────────────────┘                  │
└─────────────────────┼──────────────────────────────────────────┘
                      │
            ┌─────────▼──────────┐
            │   HTTPS/MQTT/TLS   │
            │   Shore Backend    │
            │  (Cloud/On-Premise)│
            └────────────────────┘
```

---

## 📊 Edge Database Schema (SQLite)

### **Why SQLite?**
- ✅ **Embedded**: No separate DB server required
- ✅ **Reliable**: ACID transactions, crash recovery
- ✅ **Offline-first**: Works without internet connection
- ✅ **Low resource**: Perfect for edge devices
- ✅ **File-based**: Easy backup and transfer

### **Database Structure**

```sql
-- 1. NMEA Raw Sentences (для отладки и аудита)
CREATE TABLE nmea_raw_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    sentence_type TEXT NOT NULL,           -- GGA, RMC, VTG, etc.
    raw_sentence TEXT NOT NULL,            -- Full NMEA sentence
    checksum_valid BOOLEAN,
    device_source TEXT,                    -- COM1, COM2, etc.
    is_synced BOOLEAN DEFAULT 0
);

-- 2. Position Data (GPS/GNSS)
CREATE TABLE position_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    latitude REAL NOT NULL,                -- Decimal degrees
    longitude REAL NOT NULL,               -- Decimal degrees
    altitude REAL,                         -- Meters above MSL
    speed_over_ground REAL,                -- Knots
    course_over_ground REAL,               -- Degrees true
    magnetic_variation REAL,               -- Degrees
    fix_quality INTEGER,                   -- 0=invalid, 1=GPS, 2=DGPS, etc.
    satellites_used INTEGER,
    hdop REAL,                             -- Horizontal dilution of precision
    source TEXT DEFAULT 'GPS',             -- GPS, DGPS, GLONASS, etc.
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 3. AIS Data (Automatic Identification System)
CREATE TABLE ais_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    mmsi TEXT NOT NULL,                    -- Maritime Mobile Service Identity
    message_type INTEGER NOT NULL,         -- 1,2,3,5,18,19,21,24
    navigation_status INTEGER,             -- 0-15 (underway, at anchor, etc.)
    rate_of_turn REAL,                     -- Degrees per minute
    speed_over_ground REAL,                -- Knots
    position_accuracy BOOLEAN,             -- High/Low accuracy
    latitude REAL,
    longitude REAL,
    course_over_ground REAL,               -- Degrees
    true_heading INTEGER,                  -- Degrees
    -- Static data (Message Type 5)
    imo_number TEXT,
    call_sign TEXT,
    ship_name TEXT,
    ship_type INTEGER,
    dimension_bow INTEGER,
    dimension_stern INTEGER,
    dimension_port INTEGER,
    dimension_starboard INTEGER,
    eta_month INTEGER,
    eta_day INTEGER,
    eta_hour INTEGER,
    eta_minute INTEGER,
    draught REAL,                          -- Meters
    destination TEXT,
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 4. Heading & Navigation
CREATE TABLE navigation_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    heading_true REAL,                     -- Degrees true
    heading_magnetic REAL,                 -- Degrees magnetic
    rate_of_turn REAL,                     -- Degrees per minute
    pitch REAL,                            -- Degrees
    roll REAL,                             -- Degrees
    speed_through_water REAL,              -- Knots (from log)
    depth REAL,                            -- Meters
    wind_speed_relative REAL,              -- Knots
    wind_direction_relative REAL,          -- Degrees
    wind_speed_true REAL,                  -- Knots
    wind_direction_true REAL,              -- Degrees true
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 5. Engine Telemetry (Modbus RTU/TCP)
CREATE TABLE engine_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    engine_id TEXT NOT NULL,               -- MAIN_ENGINE, AUX_ENGINE_1, etc.
    rpm REAL,
    load_percent REAL,
    coolant_temp REAL,                     -- Celsius
    exhaust_temp REAL,                     -- Celsius
    lube_oil_pressure REAL,                -- Bar
    lube_oil_temp REAL,                    -- Celsius
    fuel_pressure REAL,                    -- Bar
    fuel_rate REAL,                        -- Liters/hour
    running_hours REAL,
    start_count INTEGER,
    alarm_status INTEGER,                  -- Bitmap of active alarms
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 6. Fuel Consumption (IMO DCS Compliance)
CREATE TABLE fuel_consumption (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    fuel_type TEXT NOT NULL,               -- HFO, MGO, LNG, MDO, etc.
    consumed_volume REAL,                  -- Liters
    consumed_mass REAL,                    -- Metric Tons
    tank_id TEXT,
    density REAL,                          -- kg/m³
    -- IMO DCS fields
    distance_traveled REAL,                -- Nautical miles
    time_underway REAL,                    -- Hours
    cargo_weight REAL,                     -- Metric Tons
    co2_emissions REAL,                    -- Metric Tons CO2
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 7. Tank Levels (Fuel, Fresh Water, Ballast)
CREATE TABLE tank_levels (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    tank_id TEXT NOT NULL,                 -- FO_1, FW_2, BALLAST_1, etc.
    tank_type TEXT NOT NULL,               -- FUEL, FRESHWATER, BALLAST, LUBE_OIL
    level_percent REAL,                    -- 0-100%
    volume_liters REAL,
    temperature REAL,                      -- Celsius
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 8. Generator Status
CREATE TABLE generator_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    generator_id TEXT NOT NULL,            -- GEN_1, GEN_2, EMER_GEN
    is_running BOOLEAN,
    voltage REAL,                          -- Volts
    frequency REAL,                        -- Hz
    current REAL,                          -- Amperes
    active_power REAL,                     -- kW
    power_factor REAL,
    running_hours REAL,
    load_percent REAL,
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 9. Environmental Sensors
CREATE TABLE environmental_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    air_temperature REAL,                  -- Celsius
    barometric_pressure REAL,              -- hPa
    humidity REAL,                         -- Percent
    sea_temperature REAL,                  -- Celsius
    wind_speed REAL,                       -- Knots
    wind_direction REAL,                   -- Degrees true
    wave_height REAL,                      -- Meters
    visibility REAL,                       -- Nautical miles
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 10. Safety & Alarms
CREATE TABLE safety_alarms (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp DATETIME NOT NULL,
    alarm_type TEXT NOT NULL,              -- FIRE, BILGE, ENGINE, NAVIGATION
    alarm_code TEXT,
    severity TEXT NOT NULL,                -- CRITICAL, WARNING, INFO
    location TEXT,                         -- ENGINE_ROOM, BRIDGE, etc.
    description TEXT,
    is_acknowledged BOOLEAN DEFAULT 0,
    acknowledged_at DATETIME,
    acknowledged_by TEXT,
    is_resolved BOOLEAN DEFAULT 0,
    resolved_at DATETIME,
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 11. Voyage Data
CREATE TABLE voyage_records (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    voyage_number TEXT UNIQUE,
    departure_port TEXT,
    departure_time DATETIME,
    arrival_port TEXT,
    arrival_time DATETIME,
    cargo_type TEXT,
    cargo_weight REAL,                     -- Metric Tons
    distance_traveled REAL,                -- Nautical miles
    fuel_consumed REAL,                    -- Metric Tons
    average_speed REAL,                    -- Knots
    voyage_status TEXT,                    -- PLANNING, UNDERWAY, COMPLETED
    is_synced BOOLEAN DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 12. Sync Queue (Store-and-Forward)
CREATE TABLE sync_queue (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    table_name TEXT NOT NULL,
    record_id INTEGER NOT NULL,
    payload TEXT NOT NULL,                 -- JSON data
    priority INTEGER DEFAULT 5,            -- 1=highest, 10=lowest
    retry_count INTEGER DEFAULT 0,
    max_retries INTEGER DEFAULT 5,
    next_retry_at DATETIME,
    last_error TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    synced_at DATETIME
);

-- Indexes for performance
CREATE INDEX idx_position_timestamp ON position_data(timestamp DESC);
CREATE INDEX idx_position_synced ON position_data(is_synced);
CREATE INDEX idx_ais_mmsi_timestamp ON ais_data(mmsi, timestamp DESC);
CREATE INDEX idx_engine_timestamp ON engine_data(engine_id, timestamp DESC);
CREATE INDEX idx_fuel_timestamp ON fuel_consumption(timestamp DESC);
CREATE INDEX idx_sync_queue_priority ON sync_queue(priority, next_retry_at);
CREATE INDEX idx_alarms_unresolved ON safety_alarms(is_resolved, severity);
```

---

## 🔧 Service Components

### **1. NMEA Collector Service**
- Serial port communication (RS232/RS485)
- NMEA 0183 sentence parsing
- Real-time data validation
- Store to SQLite

### **2. AIS Decoder Service**
- Decode AIS VDM/VDO messages
- Track nearby vessels
- Collision avoidance data

### **3. Modbus Collector Service**
- Modbus RTU (serial) for engine sensors
- Modbus TCP for modern equipment
- Register mapping configuration

### **4. Data Aggregation Service**
- Calculate derived values (fuel efficiency, CII, etc.)
- Hourly/daily summaries
- IMO DCS report generation

### **5. Store-and-Forward Sync Service**
- Batch upload to shore backend
- Priority queue (alerts > routine data)
- Bandwidth optimization
- Retry with exponential backoff

### **6. Alert Engine**
- Real-time anomaly detection
- Safety threshold monitoring
- SOLAS compliance checks

---

## 📡 Communication Protocols

### **Satellite/Cellular Connectivity**
- **VSAT**: Primary satellite connection (64-512 kbps)
- **Iridium/Inmarsat**: Backup for critical alerts
- **4G LTE**: When in coastal areas
- **Starlink**: High-bandwidth option (50-150 Mbps)

### **Data Compression**
- gzip compression (70-90% reduction)
- Delta encoding for time-series data
- Binary protocol (Protocol Buffers/MessagePack)

---

## 🔐 Security Features

- X.509 certificate-based authentication
- TLS 1.3 encryption for all transmissions
- Local data encryption (SQLCipher)
- Tamper detection and audit logs

---

## 📦 Deployment

### **Hardware Requirements**
- **CPU**: Intel Atom / ARM Cortex-A53 or better
- **RAM**: 2GB minimum, 4GB recommended
- **Storage**: 32GB SSD minimum (for 30 days offline buffer)
- **Serial Ports**: 2-4 COM ports (RS232/RS485)
- **Network**: Ethernet + 4G modem + Satellite modem
- **Power**: 12-24V DC, <15W consumption

### **Operating Systems**
- Windows 10/11 IoT Enterprise
- Linux (Debian, Ubuntu, Raspberry Pi OS)
- Docker containerized deployment

---

## 📊 Data Retention Policy

| Data Type | Local Retention | Upload Frequency | Priority |
|-----------|----------------|------------------|----------|
| Position (GPS) | 7 days | Every 5 minutes | HIGH |
| AIS Data | 3 days | Every 10 minutes | MEDIUM |
| Engine Data | 30 days | Every 15 minutes | HIGH |
| Fuel Consumption | 90 days | Daily | HIGH |
| Alarms | 90 days | Immediate | CRITICAL |
| Environmental | 7 days | Hourly | LOW |

---

## 🎯 Compliance & Standards

✅ **IMO SOLAS** Chapter V - Navigation safety
✅ **IMO DCS** - Fuel consumption reporting
✅ **EU MRV** - CO2 emissions monitoring
✅ **MARPOL Annex VI** - Air pollution prevention
✅ **IEC 61162-1** - NMEA 0183 standard
✅ **ITU-R M.1371-5** - AIS technical characteristics

---

*Maritime Edge Server - Enabling Smart, Connected, and Compliant Ship Operations* 🚢⚓
