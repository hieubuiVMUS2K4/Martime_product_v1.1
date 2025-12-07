# 👥 CREW MEMBERS & USERS SEED DATA

## 📋 Overview

Script này tạo dữ liệu mẫu cho **20 crew members** dựa trên cấu trúc thực tế của tàu **ANNIE GAS 09**.

## 🚢 Crew Structure

### Management (3 người)
- **CREW001** - Nguyễn Văn Thành (Master)
- **CREW002** - Trần Minh Tuấn (Chief Engineer)  
- **CREW003** - Lê Hoàng Nam (Chief Officer)

### Engine Department (8 người)
- **CREW004** - Phạm Đức Anh (2/E) ⭐ **KEY PERSON**
- **CREW005** - Võ Thanh Tùng (3/E) ⭐ **KEY PERSON**
- **CREW006** - Đặng Văn Hải (E/O) ⭐ **KEY PERSON**
- **CREW007** - Bùi Quang Minh (4/E)
- **CREW008** - Hoàng Văn Đức (Fitter)
- **CREW009** - Ngô Văn Sơn (Fitter)
- **CREW010** - Lý Văn Thắng (Oiler)
- **CREW011** - Phan Văn Tài (Oiler)

### Deck Department (8 người)
- **CREW012** - Dương Minh Quân (2/O)
- **CREW013** - Trịnh Văn Hùng (3/O)
- **CREW014** - Vũ Văn Bình (Bosun) ⭐ **KEY PERSON** (Ship's Crew leader)
- **CREW015** - Mai Văn Dũng (AB)
- **CREW016** - Đinh Văn Lâm (AB)
- **CREW017** - Hồ Văn Phúc (AB)
- **CREW018** - Châu Văn Toàn (OS)
- **CREW019** - Lương Văn Kiên (OS)

### Catering (1 người)
- **CREW020** - Nguyễn Văn Hải (Chief Cook)

---

## 🔐 Login Credentials

**Username**: CREW001, CREW002, ..., CREW020

**Password Format**: `ddmmyyyy` (từ ngày sinh)

**Examples**:
- CREW001 (Master, DOB: 15/03/1975) → Password: `15031975`
- CREW004 (2/E, DOB: 25/05/1985) → Password: `25051985`
- CREW005 (3/E, DOB: 12/09/1988) → Password: `12091988`

---

## 📝 Installation Steps

### Step 1: Run SQL seed script

```bash
cd D:\Martime_product_v1\edge-services

# Connect to PostgreSQL
psql -U postgres -d maritime_edge

# Run seed script
\i seed-crew-and-users.sql
```

### Step 2: Generate BCrypt hashes (Optional - for production)

```powershell
# Install BCrypt module
Install-Module -Name BCrypt.Net-Next -Force

# Generate password hashes
.\generate-crew-passwords.ps1

# This will create: update-crew-passwords.sql
```

### Step 3: Update passwords (if using BCrypt)

```bash
psql -U postgres -d maritime_edge
\i update-crew-passwords.sql
```

---

## ✅ Verification

Run these queries to verify:

```sql
-- Check crew count by department
SELECT 
    "Department", 
    COUNT(*) as "CrewCount"
FROM "CrewMembers"
WHERE "CrewId" LIKE 'CREW%'
GROUP BY "Department";

-- Expected output:
-- MANAGEMENT: 3
-- ENGINE: 8
-- DECK: 8
-- CATERING: 1

-- Check key positions
SELECT 
    "CrewId",
    "FullName",
    "Rank",
    "Department",
    "IsOnboard"
FROM "CrewMembers"
WHERE "Rank" IN ('MASTER', 'C/E', 'C/O', '2/E', '3/E', 'E/O', 'BOSUN')
ORDER BY "CrewId";

-- Check user accounts
SELECT 
    u."Username",
    c."FullName",
    c."Rank",
    u."IsActive"
FROM "Users" u
JOIN "CrewMembers" c ON u."CrewId" = c."CrewId"
WHERE u."Username" LIKE 'CREW%'
ORDER BY u."Username";
```

---

## 🎯 Usage in PMS System

### Task Assignment by Role

Based on actual ship practice (from ANNIE GAS 09 data):

| Equipment Type | Default Assignee | Approver |
|----------------|------------------|----------|
| Main Engine | 2/E (CREW004) | C/E (CREW002) |
| Generators | 3/E (CREW005) | C/E (CREW002) |
| Pumps | 3/E (CREW005) | C/E (CREW002) |
| Electrical | E/O (CREW006) | C/E (CREW002) |
| Cargo Systems | C/O (CREW003) | Master (CREW001) |
| Hull/Deck | BOSUN (CREW014) | C/O (CREW003) |

### API Usage

```typescript
// Get all onboard crew for assignment dropdown
GET /api/crew?isOnboard=true&pageSize=100

// Get crew by rank
GET /api/crew?rank=2/E

// Get crew by department
GET /api/crew?department=ENGINE
```

---

## 🔄 Refresh Data (Clean & Reseed)

```sql
-- Delete all test crew data
DELETE FROM "Users" WHERE "Username" LIKE 'CREW%';
DELETE FROM "CrewMembers" WHERE "CrewId" LIKE 'CREW%';

-- Then re-run seed script
\i seed-crew-and-users.sql
```

---

## 📊 Role Hierarchy

```
┌─────────────────────────┐
│  MASTER (CREW001)       │
└───────┬─────────────────┘
        │
   ┌────┴────┐
   │         │
┌──▼────┐ ┌─▼─────┐
│  C/E  │ │  C/O  │
│ 002   │ │ 003   │
└───┬───┘ └───┬───┘
    │         │
 ┌──┴──┬──┬───┴───┬───┬───┐
 │     │  │       │   │   │
2/E   3/E E/O    2/O 3/O BOSUN
004   005 006    012 013  014
 │     │  │       │   │    │
Engine Dept    Deck Dept
(Fitters,      (ABs,
 Oilers)        OSs)
```

---

## 🚨 Important Notes

1. **IsOnboard Status**: All crew set to `true` for testing. In production, update when crew sign off.

2. **Contract Dates**: Set to realistic future dates. Monitor and update ContractEnd dates.

3. **Roles vs Positions**:
   - **Rank**: Short code (2/E, 3/E, E/O) - Used for task assignment
   - **Position**: Full title (Second Engineer) - For display

4. **Department**: Used for filtering and reporting
   - ENGINE: Machinery and electrical
   - DECK: Navigation and cargo
   - CATERING: Hotel services
   - MANAGEMENT: Master level

5. **Email/Phone**: Sample data - update with real contacts in production

---

## 🔗 Related Documentation

- [PMS_AUTO_GENERATION_ANALYSIS.md](./PMS_AUTO_GENERATION_ANALYSIS.md) - Task assignment logic
- [MAINTENANCE_PLANNING_IMPLEMENTATION.md](../MAINTENANCE_PLANNING_IMPLEMENTATION.md) - PMS workflow

---

## 📞 Support

For questions about crew structure or task assignment logic, refer to actual ship documents:
- TONGHOP MAY... (Engine department assignments)
- DECK... (Deck department assignments)
- Hull construction docs (P.I.C column)
