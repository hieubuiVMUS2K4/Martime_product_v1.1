-- ============================================================
-- Port Master Data Seed (UN/LOCODE Standard)
-- Major international ports for maritime operations
-- Run after migration: VoyageCrewPortLinking
-- ============================================================

INSERT INTO public.ports (port_code, port_name, country, country_code, latitude, longitude, time_zone, is_active, created_at, updated_at)
VALUES
-- === VIETNAM ===
('VNSGN', 'Ho Chi Minh City (Saigon)', 'Vietnam', 'VN', 10.7769, 106.7009, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),
('VNHPH', 'Hai Phong', 'Vietnam', 'VN', 20.8449, 106.6881, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),
('VNDAD', 'Da Nang', 'Vietnam', 'VN', 16.0544, 108.2022, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),
('VNVUT', 'Vung Tau', 'Vietnam', 'VN', 10.3460, 107.0843, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),
('VNQNH', 'Quy Nhon', 'Vietnam', 'VN', 13.7563, 109.2200, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),
('VNCMR', 'Cam Ranh', 'Vietnam', 'VN', 11.9214, 109.1590, 'Asia/Ho_Chi_Minh', true, NOW(), NOW()),

-- === SINGAPORE ===
('SGSIN', 'Singapore', 'Singapore', 'SG', 1.2644, 103.8222, 'Asia/Singapore', true, NOW(), NOW()),

-- === CHINA ===
('CNSHA', 'Shanghai', 'China', 'CN', 31.2304, 121.4737, 'Asia/Shanghai', true, NOW(), NOW()),
('CNSHE', 'Shenzhen (Shekou)', 'China', 'CN', 22.4814, 113.8671, 'Asia/Shanghai', true, NOW(), NOW()),
('CNNGB', 'Ningbo-Zhoushan', 'China', 'CN', 29.8683, 121.5440, 'Asia/Shanghai', true, NOW(), NOW()),
('CNQIN', 'Qingdao', 'China', 'CN', 36.0671, 120.3826, 'Asia/Shanghai', true, NOW(), NOW()),
('CNTXG', 'Tianjin (Xingang)', 'China', 'CN', 38.9860, 117.7278, 'Asia/Shanghai', true, NOW(), NOW()),
('CNDLC', 'Dalian', 'China', 'CN', 38.9140, 121.6147, 'Asia/Shanghai', true, NOW(), NOW()),
('CNXMN', 'Xiamen', 'China', 'CN', 24.4798, 118.0894, 'Asia/Shanghai', true, NOW(), NOW()),
('HKHKG', 'Hong Kong', 'Hong Kong', 'HK', 22.2855, 114.1577, 'Asia/Hong_Kong', true, NOW(), NOW()),

-- === JAPAN ===
('JPTYO', 'Tokyo', 'Japan', 'JP', 35.6528, 139.8395, 'Asia/Tokyo', true, NOW(), NOW()),
('JPYOK', 'Yokohama', 'Japan', 'JP', 35.4437, 139.6380, 'Asia/Tokyo', true, NOW(), NOW()),
('JPKOB', 'Kobe', 'Japan', 'JP', 34.6901, 135.1956, 'Asia/Tokyo', true, NOW(), NOW()),
('JPNGO', 'Nagoya', 'Japan', 'JP', 35.0880, 136.8815, 'Asia/Tokyo', true, NOW(), NOW()),
('JPOSA', 'Osaka', 'Japan', 'JP', 34.6516, 135.4325, 'Asia/Tokyo', true, NOW(), NOW()),

-- === SOUTH KOREA ===
('KRPUS', 'Busan', 'South Korea', 'KR', 35.1028, 129.0327, 'Asia/Seoul', true, NOW(), NOW()),
('KRINC', 'Incheon', 'South Korea', 'KR', 37.4563, 126.7052, 'Asia/Seoul', true, NOW(), NOW()),

-- === TAIWAN ===
('TWKHH', 'Kaohsiung', 'Taiwan', 'TW', 22.6163, 120.3133, 'Asia/Taipei', true, NOW(), NOW()),
('TWKEL', 'Keelung', 'Taiwan', 'TW', 25.1276, 121.7392, 'Asia/Taipei', true, NOW(), NOW()),

-- === SOUTHEAST ASIA ===
('MYPKG', 'Port Klang', 'Malaysia', 'MY', 3.0000, 101.3800, 'Asia/Kuala_Lumpur', true, NOW(), NOW()),
('MYTPP', 'Tanjung Pelepas', 'Malaysia', 'MY', 1.3667, 103.5500, 'Asia/Kuala_Lumpur', true, NOW(), NOW()),
('THBKK', 'Bangkok (Laem Chabang)', 'Thailand', 'TH', 13.0778, 100.8842, 'Asia/Bangkok', true, NOW(), NOW()),
('THLCH', 'Laem Chabang', 'Thailand', 'TH', 13.0819, 100.8931, 'Asia/Bangkok', true, NOW(), NOW()),
('IDJKT', 'Jakarta (Tanjung Priok)', 'Indonesia', 'ID', -6.1048, 106.8800, 'Asia/Jakarta', true, NOW(), NOW()),
('IDBLW', 'Belawan', 'Indonesia', 'ID', 3.7833, 98.6833, 'Asia/Jakarta', true, NOW(), NOW()),
('PHMNL', 'Manila', 'Philippines', 'PH', 14.5833, 120.9667, 'Asia/Manila', true, NOW(), NOW()),
('MMRGN', 'Yangon', 'Myanmar', 'MM', 16.8500, 96.1667, 'Asia/Yangon', true, NOW(), NOW()),
('KHPNH', 'Phnom Penh', 'Cambodia', 'KH', 11.5564, 104.9282, 'Asia/Phnom_Penh', true, NOW(), NOW()),

-- === SOUTH ASIA & MIDDLE EAST ===
('INMAA', 'Chennai (Madras)', 'India', 'IN', 13.0827, 80.2707, 'Asia/Kolkata', true, NOW(), NOW()),
('INNSA', 'Nhava Sheva (Mumbai)', 'India', 'IN', 18.9500, 72.9500, 'Asia/Kolkata', true, NOW(), NOW()),
('LKCMB', 'Colombo', 'Sri Lanka', 'LK', 6.9497, 79.8428, 'Asia/Colombo', true, NOW(), NOW()),
('AEJEA', 'Jebel Ali (Dubai)', 'UAE', 'AE', 25.0174, 55.0628, 'Asia/Dubai', true, NOW(), NOW()),
('SAJED', 'Jeddah', 'Saudi Arabia', 'SA', 21.4858, 39.1925, 'Asia/Riyadh', true, NOW(), NOW()),
('OMSLL', 'Salalah', 'Oman', 'OM', 16.9434, 54.0025, 'Asia/Muscat', true, NOW(), NOW()),

-- === EUROPE ===
('NLRTM', 'Rotterdam', 'Netherlands', 'NL', 51.9244, 4.4777, 'Europe/Amsterdam', true, NOW(), NOW()),
('BEANR', 'Antwerp', 'Belgium', 'BE', 51.2194, 4.4025, 'Europe/Brussels', true, NOW(), NOW()),
('DEHAM', 'Hamburg', 'Germany', 'DE', 53.5461, 9.9669, 'Europe/Berlin', true, NOW(), NOW()),
('DEBRV', 'Bremerhaven', 'Germany', 'DE', 53.5396, 8.5809, 'Europe/Berlin', true, NOW(), NOW()),
('GBFXT', 'Felixstowe', 'United Kingdom', 'GB', 51.9536, 1.3511, 'Europe/London', true, NOW(), NOW()),
('GBLGP', 'London Gateway', 'United Kingdom', 'GB', 51.5050, 0.4583, 'Europe/London', true, NOW(), NOW()),
('FRLEH', 'Le Havre', 'France', 'FR', 49.4944, 0.1079, 'Europe/Paris', true, NOW(), NOW()),
('ESALG', 'Algeciras', 'Spain', 'ES', 36.1309, -5.4433, 'Europe/Madrid', true, NOW(), NOW()),
('ESVLC', 'Valencia', 'Spain', 'ES', 39.4453, -0.3240, 'Europe/Madrid', true, NOW(), NOW()),
('ITGOA', 'Genoa', 'Italy', 'IT', 44.4056, 8.9463, 'Europe/Rome', true, NOW(), NOW()),
('GRPIR', 'Piraeus', 'Greece', 'GR', 37.9475, 23.6370, 'Europe/Athens', true, NOW(), NOW()),
('TRIST', 'Istanbul (Ambarli)', 'Turkey', 'TR', 40.9667, 28.6833, 'Europe/Istanbul', true, NOW(), NOW()),

-- === MEDITERRANEAN & SUEZ ===
('EGPSD', 'Port Said', 'Egypt', 'EG', 31.2565, 32.2841, 'Africa/Cairo', true, NOW(), NOW()),
('EGSKH', 'Suez (El-Sukhna)', 'Egypt', 'EG', 29.6000, 32.3167, 'Africa/Cairo', true, NOW(), NOW()),
('MAPTM', 'Tanger Med', 'Morocco', 'MA', 35.8889, -5.5000, 'Africa/Casablanca', true, NOW(), NOW()),

-- === AMERICAS ===
('USLAX', 'Los Angeles / Long Beach', 'United States', 'US', 33.7406, -118.2712, 'America/Los_Angeles', true, NOW(), NOW()),
('USNYC', 'New York / New Jersey', 'United States', 'US', 40.6693, -74.0446, 'America/New_York', true, NOW(), NOW()),
('USSAV', 'Savannah', 'United States', 'US', 32.0809, -81.0912, 'America/New_York', true, NOW(), NOW()),
('USHOU', 'Houston', 'United States', 'US', 29.7260, -95.2690, 'America/Chicago', true, NOW(), NOW()),
('CAHAL', 'Halifax', 'Canada', 'CA', 44.6488, -63.5752, 'America/Halifax', true, NOW(), NOW()),
('CAVAN', 'Vancouver', 'Canada', 'CA', 49.2890, -123.1115, 'America/Vancouver', true, NOW(), NOW()),
('BRSSZ', 'Santos', 'Brazil', 'BR', -23.9608, -46.3340, 'America/Sao_Paulo', true, NOW(), NOW()),
('PAPCN', 'Panama Canal (Colon)', 'Panama', 'PA', 9.3545, -79.9019, 'America/Panama', true, NOW(), NOW()),

-- === OCEANIA ===
('AUSYD', 'Sydney', 'Australia', 'AU', -33.8587, 151.2140, 'Australia/Sydney', true, NOW(), NOW()),
('AUMEL', 'Melbourne', 'Australia', 'AU', -37.8275, 144.9250, 'Australia/Melbourne', true, NOW(), NOW()),
('NZAKL', 'Auckland', 'New Zealand', 'NZ', -36.8404, 174.7400, 'Pacific/Auckland', true, NOW(), NOW()),

-- === AFRICA ===
('ZADUR', 'Durban', 'South Africa', 'ZA', -29.8706, 31.0447, 'Africa/Johannesburg', true, NOW(), NOW()),
('DJJIB', 'Djibouti', 'Djibouti', 'DJ', 11.5951, 43.1458, 'Africa/Djibouti', true, NOW(), NOW()),

-- === ADDITIONAL GLOBAL HUBS ===
('AEAUH', 'Abu Dhabi', 'UAE', 'AE', 24.4539, 54.3773, 'Asia/Dubai', true, NOW(), NOW()),
('QAHMD', 'Hamad Port', 'Qatar', 'QA', 25.0150, 51.6100, 'Asia/Qatar', true, NOW(), NOW()),
('KWIQE', 'Shuaiba', 'Kuwait', 'KW', 29.0392, 48.1460, 'Asia/Kuwait', true, NOW(), NOW()),
('PKKHI', 'Karachi', 'Pakistan', 'PK', 24.8607, 67.0011, 'Asia/Karachi', true, NOW(), NOW()),
('BDCGP', 'Chattogram', 'Bangladesh', 'BD', 22.3350, 91.8325, 'Asia/Dhaka', true, NOW(), NOW()),
('INMUN', 'Mundra', 'India', 'IN', 22.8390, 69.7210, 'Asia/Kolkata', true, NOW(), NOW()),
('USOAK', 'Oakland', 'United States', 'US', 37.8044, -122.2712, 'America/Los_Angeles', true, NOW(), NOW()),
('USSEA', 'Seattle', 'United States', 'US', 47.6062, -122.3321, 'America/Los_Angeles', true, NOW(), NOW()),
('COCTG', 'Cartagena', 'Colombia', 'CO', 10.3910, -75.4794, 'America/Bogota', true, NOW(), NOW()),
('CLVAP', 'Valparaiso', 'Chile', 'CL', -33.0472, -71.6127, 'America/Santiago', true, NOW(), NOW()),
('PECLL', 'Callao', 'Peru', 'PE', -12.0464, -77.1428, 'America/Lima', true, NOW(), NOW()),
('ARBUE', 'Buenos Aires', 'Argentina', 'AR', -34.6037, -58.3816, 'America/Argentina/Buenos_Aires', true, NOW(), NOW()),
('MXZLO', 'Manzanillo', 'Mexico', 'MX', 19.0501, -104.3188, 'America/Mexico_City', true, NOW(), NOW())

ON CONFLICT (port_code) DO NOTHING;

-- Verify
SELECT COUNT(*) AS total_ports FROM public.ports;
