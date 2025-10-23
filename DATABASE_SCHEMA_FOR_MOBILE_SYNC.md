# 🗄️ DATABASE SCHEMA - MOBILE APP SYNCHRONIZATION

## 📊 Tổng Quan

Document này mô tả chi tiết các bảng và cột trong PostgreSQL database (Edge Server) cần thiết để đồng bộ với Flutter Mobile App.

---

## 🔑 BẢNG CHÍNH (Core Tables)

### 1. **CrewMembers** - Thông tin thuyền viên

**Mục đích**: Lưu trữ thông tin chi tiết của tất cả thuyền viên trên tàu.

**Mobile App Access**: 
- **READ**: Thuyền viên xem thông tin cá nhân của chính mình
- **UPDATE**: Không cho phép (chỉ Captain/Admin có quyền)

**Table Schema**:
```sql
CREATE TABLE "CrewMembers" (
    -- Primary Key
    "Id" BIGSERIAL PRIMARY KEY,
    
    -- REQUIRED FIELDS (Mobile App MUST have)
    "CrewId" VARCHAR(50) NOT NULL UNIQUE,        -- Crew ID (CM001, CM002, ...) - LOGIN USERNAME
    "FullName" VARCHAR(200) NOT NULL,            -- Tên đầy đủ
    "Position" VARCHAR(100) NOT NULL,            -- Chức vụ (Chief Engineer, Second Officer, ...)
    
    -- Authentication (Mobile App Login)
    "PasswordHash" VARCHAR(255),                 -- ⚠️ CẦN THÊM - Hash của password để login
    "Salt" VARCHAR(100),                         -- ⚠️ CẦN THÊM - Salt cho password hash
    
    -- Personal Information
    "Rank" VARCHAR(50),                          -- Rank (Officer, Rating)
    "Department" VARCHAR(100),                   -- Department (Deck, Engine, Catering)
    "Nationality" VARCHAR(50),                   -- Quốc tịch
    "DateOfBirth" TIMESTAMP,                     -- Ngày sinh
    "PhoneNumber" VARCHAR(50),                   -- Số điện thoại
    "Email" VARCHAR(200),                        -- Email (alternative)
    "EmailAddress" VARCHAR(200),                 -- Email chính
    "Address" VARCHAR(500),                      -- Địa chỉ
    "EmergencyContact" VARCHAR(500),             -- Liên hệ khẩn cấp
    
    -- Employment Information
    "JoinDate" TIMESTAMP,                        -- Ngày gia nhập công ty
    "EmbarkDate" TIMESTAMP,                      -- Ngày lên tàu
    "DisembarkDate" TIMESTAMP,                   -- Ngày xuống tàu
    "ContractEnd" TIMESTAMP,                     -- Ngày kết thúc hợp đồng
    "IsOnboard" BOOLEAN DEFAULT TRUE,            -- Đang trên tàu hay không
    
    -- STCW Certificate (Chứng chỉ STCW - BẮT BUỘC cho hàng hải)
    "CertificateNumber" VARCHAR(100),            -- Số chứng chỉ STCW
    "CertificateIssue" TIMESTAMP,                -- Ngày cấp STCW
    "CertificateExpiry" TIMESTAMP,               -- Ngày hết hạn STCW (MOBILE SHOW ALERT)
    
    -- Medical Certificate (Giấy khám sức khỏe - BẮT BUỘC)
    "MedicalIssue" TIMESTAMP,                    -- Ngày khám sức khỏe
    "MedicalExpiry" TIMESTAMP,                   -- Ngày hết hạn (MOBILE SHOW ALERT)
    
    -- Travel Documents
    "PassportNumber" VARCHAR(50),                -- Số hộ chiếu
    "PassportExpiry" TIMESTAMP,                  -- Hết hạn hộ chiếu (MOBILE SHOW ALERT)
    "VisaNumber" VARCHAR(50),                    -- Số visa
    "VisaExpiry" TIMESTAMP,                      -- Hết hạn visa
    "SeamanBookNumber" VARCHAR(100),             -- Số sổ hàng hải
    
    -- Additional Information
    "Notes" TEXT,                                -- Ghi chú
    
    -- System Fields (QUAN TRỌNG cho sync)
    "IsSynced" BOOLEAN DEFAULT FALSE,            -- Đã đồng bộ với shore server chưa
    "CreatedAt" TIMESTAMP DEFAULT NOW(),         -- Ngày tạo record
    "UpdatedAt" TIMESTAMP,                       -- ⚠️ CẦN THÊM - Ngày cập nhật cuối
    "LastModifiedBy" VARCHAR(100)                -- ⚠️ CẦN THÊM - Ai sửa cuối cùng
);

-- Indexes for performance
CREATE INDEX idx_crewmembers_crewid ON "CrewMembers"("CrewId");
CREATE INDEX idx_crewmembers_isonboard ON "CrewMembers"("IsOnboard");
CREATE INDEX idx_crewmembers_issync ON "CrewMembers"("IsSynced");
```

**Mobile App Fields (JSON Response)**:
```json
{
  "id": 1,
  "crewId": "CM001",
  "fullName": "Nguyen Van A",
  "position": "Chief Engineer",
  "rank": "Officer",
  "department": "Engine",
  "nationality": "Vietnam",
  "dateOfBirth": "1985-05-15T00:00:00Z",
  "phoneNumber": "+84901234567",
  "email": "nguyenvana@ship.com",
  "address": "123 Hai Ba Trung, Hanoi",
  "emergencyContact": "Mrs. Nguyen Thi B - Wife - +84907654321",
  "joinDate": "2020-01-15T00:00:00Z",
  "embarkDate": "2024-10-01T00:00:00Z",
  "contractEnd": "2025-04-01T00:00:00Z",
  "isOnboard": true,
  "certificateNumber": "STCW-VN-123456",
  "certificateIssue": "2022-06-01T00:00:00Z",
  "certificateExpiry": "2027-06-01T00:00:00Z",
  "medicalIssue": "2024-09-15T00:00:00Z",
  "medicalExpiry": "2025-09-15T00:00:00Z",
  "passportNumber": "N1234567",
  "passportExpiry": "2028-05-20T00:00:00Z",
  "visaNumber": "US-V-987654",
  "visaExpiry": "2025-12-31T00:00:00Z",
  "seamanBookNumber": "VN-SB-789012",
  "notes": "Experienced with Wartsila engines",
  "isSynced": false,
  "createdAt": "2024-01-15T08:00:00Z"
}
```

---

### 2. **MaintenanceTasks** - Công việc bảo dưỡng

**Mục đích**: Lưu trữ các công việc bảo dưỡng thiết bị theo PMS (Planned Maintenance System).

**Mobile App Access**: 
- **READ**: Xem tasks được giao cho mình (`AssignedTo = CrewName`)
- **UPDATE**: Cập nhật status, complete task, add notes

**Table Schema**:
```sql
CREATE TABLE "MaintenanceTasks" (
    -- Primary Key
    "Id" BIGSERIAL PRIMARY KEY,
    
    -- REQUIRED FIELDS
    "TaskId" VARCHAR(50) NOT NULL UNIQUE,        -- Task ID (MNT-001, MNT-002, ...)
    "EquipmentId" VARCHAR(100) NOT NULL,         -- Equipment ID (MAIN_ENGINE, GEN_1, ...)
    "EquipmentName" VARCHAR(200) NOT NULL,       -- Tên thiết bị (Main Engine Wartsila 8L20)
    "TaskType" VARCHAR(50) NOT NULL,             -- RUNNING_HOURS | CALENDAR | CONDITION
    "TaskDescription" TEXT NOT NULL,             -- Mô tả chi tiết công việc
    
    -- Schedule Information
    "IntervalHours" DOUBLE PRECISION,            -- Chu kỳ theo giờ chạy (500, 1000, ...)
    "IntervalDays" INTEGER,                      -- Chu kỳ theo ngày (30, 60, 90, ...)
    "LastDoneAt" TIMESTAMP,                      -- Lần bảo dưỡng cuối cùng
    "NextDueAt" TIMESTAMP NOT NULL,              -- Ngày đến hạn tiếp theo (MOBILE ALERT)
    "RunningHoursAtLastDone" DOUBLE PRECISION,   -- Giờ chạy khi bảo dưỡng cuối
    
    -- Priority & Status
    "Priority" VARCHAR(20) DEFAULT 'NORMAL',     -- CRITICAL | HIGH | NORMAL | LOW
    "Status" VARCHAR(20) DEFAULT 'PENDING',      -- PENDING | IN_PROGRESS | COMPLETED | OVERDUE
    
    -- Assignment (QUAN TRỌNG cho Mobile App)
    "AssignedTo" VARCHAR(100),                   -- Tên crew được giao việc
    "AssignedToCrewId" VARCHAR(50),              -- ⚠️ CẦN THÊM - CrewId để filter trong Mobile
    
    -- Completion Information (Mobile App UPDATE)
    "CompletedAt" TIMESTAMP,                     -- Thời điểm hoàn thành
    "CompletedBy" VARCHAR(100),                  -- Người hoàn thành (từ Mobile App)
    "CompletedByCrewId" VARCHAR(50),             -- ⚠️ CẦN THÊM - CrewId người hoàn thành
    "Notes" TEXT,                                -- Ghi chú khi hoàn thành (từ Mobile)
    "SparePartsUsed" VARCHAR(500),               -- Phụ tùng sử dụng (từ Mobile)
    "RunningHoursAtCompletion" DOUBLE PRECISION, -- ⚠️ CẦN THÊM - Giờ chạy khi hoàn thành
    
    -- Attachments (Optional - Future)
    "PhotoUrls" TEXT,                            -- ⚠️ CẦN THÊM - JSON array của photo URLs
    
    -- System Fields
    "IsSynced" BOOLEAN DEFAULT FALSE,            -- Đã sync với shore
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP,                       -- ⚠️ CẦN THÊM
    "LastModifiedBy" VARCHAR(100)                -- ⚠️ CẦN THÊM
);

-- Indexes
CREATE INDEX idx_maintenance_assignedto ON "MaintenanceTasks"("AssignedToCrewId");
CREATE INDEX idx_maintenance_status ON "MaintenanceTasks"("Status");
CREATE INDEX idx_maintenance_nextdue ON "MaintenanceTasks"("NextDueAt");
CREATE INDEX idx_maintenance_issync ON "MaintenanceTasks"("IsSynced");
```

**Mobile App Fields (My Tasks Response)**:
```json
{
  "id": 1,
  "taskId": "MNT-001",
  "equipmentId": "MAIN_ENGINE",
  "equipmentName": "Main Engine Wartsila 8L20",
  "taskType": "RUNNING_HOURS",
  "taskDescription": "Change engine oil and oil filter. Check cooling system.",
  "intervalHours": 500.0,
  "intervalDays": null,
  "lastDoneAt": "2024-09-15T10:00:00Z",
  "nextDueAt": "2024-11-05T00:00:00Z",
  "runningHoursAtLastDone": 12500.0,
  "priority": "HIGH",
  "status": "PENDING",
  "assignedTo": "Nguyen Van A",
  "assignedToCrewId": "CM001",
  "completedAt": null,
  "completedBy": null,
  "notes": null,
  "sparePartsUsed": null,
  "isSynced": false,
  "createdAt": "2024-10-01T08:00:00Z",
  
  // Computed fields (Mobile App calculate)
  "daysUntilDue": 15,
  "isOverdue": false,
  "isDueSoon": true
}
```

---

### 3. **SyncQueue** - Hàng đợi đồng bộ

**Mục đích**: Lưu trữ các thao tác cần đồng bộ với shore server (không phải Mobile → Edge Server).

**Mobile App Access**: KHÔNG (internal system only)

**Table Schema**:
```sql
CREATE TABLE "SyncQueue" (
    "Id" BIGSERIAL PRIMARY KEY,
    "TableName" VARCHAR(100) NOT NULL,           -- Tên bảng cần sync
    "RecordId" BIGINT NOT NULL,                  -- ID của record
    "Operation" VARCHAR(20) NOT NULL,            -- INSERT | UPDATE | DELETE
    "Payload" TEXT,                              -- JSON data của record
    "Status" VARCHAR(20) DEFAULT 'PENDING',      -- PENDING | IN_PROGRESS | COMPLETED | FAILED
    "RetryCount" INTEGER DEFAULT 0,
    "LastError" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "ProcessedAt" TIMESTAMP
);

CREATE INDEX idx_syncqueue_status ON "SyncQueue"("Status");
```

---

### 4. **Users** - Tài khoản đăng nhập (CẦN TẠO MỚI)

**Mục đích**: Quản lý authentication cho Mobile App và Web Dashboard.

**Mobile App Access**: 
- **POST /api/auth/login**: Login với CrewId + Password

**Table Schema**:
```sql
CREATE TABLE "Users" (
    "Id" BIGSERIAL PRIMARY KEY,
    "CrewId" VARCHAR(50) NOT NULL UNIQUE,        -- Link to CrewMembers.CrewId
    "Username" VARCHAR(100) NOT NULL UNIQUE,     -- Username (có thể = CrewId)
    "PasswordHash" VARCHAR(255) NOT NULL,        -- BCrypt hash
    "Salt" VARCHAR(100) NOT NULL,                -- Salt
    "Role" VARCHAR(50) DEFAULT 'CREW',           -- CREW | OFFICER | CAPTAIN | ADMIN
    "IsActive" BOOLEAN DEFAULT TRUE,
    "LastLoginAt" TIMESTAMP,
    "FailedLoginAttempts" INTEGER DEFAULT 0,
    "LockedUntil" TIMESTAMP,                     -- Account lockout
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

-- Link to CrewMembers
ALTER TABLE "Users" 
    ADD CONSTRAINT fk_users_crewmembers 
    FOREIGN KEY ("CrewId") REFERENCES "CrewMembers"("CrewId");

CREATE INDEX idx_users_crewid ON "Users"("CrewId");
CREATE INDEX idx_users_username ON "Users"("Username");
```

---

### 5. **RefreshTokens** - JWT Refresh Tokens (CẦN TẠO MỚI)

**Mục đích**: Lưu trữ refresh tokens để renew access token khi expire.

**Mobile App Access**: 
- **POST /api/auth/refresh**: Refresh access token

**Table Schema**:
```sql
CREATE TABLE "RefreshTokens" (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" BIGINT NOT NULL,                    -- Link to Users.Id
    "Token" VARCHAR(500) NOT NULL UNIQUE,        -- Refresh token string
    "DeviceId" VARCHAR(200),                     -- Device identifier (Mobile)
    "DeviceName" VARCHAR(100),                   -- Device name
    "ExpiresAt" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT NOW(),
    "RevokedAt" TIMESTAMP,
    "IsRevoked" BOOLEAN DEFAULT FALSE
);

ALTER TABLE "RefreshTokens" 
    ADD CONSTRAINT fk_refreshtokens_users 
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id");

CREATE INDEX idx_refreshtokens_token ON "RefreshTokens"("Token");
CREATE INDEX idx_refreshtokens_userid ON "RefreshTokens"("UserId");
```

---

### 6. **AuditLogs** - Nhật ký thao tác (CẦN TẠO MỚI)

**Mục đích**: Ghi lại tất cả thao tác từ Mobile App để audit.

**Mobile App Access**: KHÔNG (internal only)

**Table Schema**:
```sql
CREATE TABLE "AuditLogs" (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" BIGINT,                             -- User thực hiện
    "CrewId" VARCHAR(50),                        -- CrewId
    "Action" VARCHAR(100) NOT NULL,              -- LOGIN | COMPLETE_TASK | START_TASK | ...
    "TableName" VARCHAR(100),                    -- Bảng bị tác động
    "RecordId" BIGINT,                           -- ID record bị tác động
    "OldValue" TEXT,                             -- JSON của giá trị cũ
    "NewValue" TEXT,                             -- JSON của giá trị mới
    "IpAddress" VARCHAR(50),                     -- IP address
    "DeviceInfo" TEXT,                           -- Device info from Mobile
    "Timestamp" TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_auditlogs_crewid ON "AuditLogs"("CrewId");
CREATE INDEX idx_auditlogs_action ON "AuditLogs"("Action");
CREATE INDEX idx_auditlogs_timestamp ON "AuditLogs"("Timestamp");
```

---

## 🔄 CÁC TRƯỜNG BẮT BUỘC CHO SYNC

### Mobile App → Edge Server

**1. Task Completion (Complete Task)**
```json
POST /api/maintenance/tasks/{id}/complete
{
  "completedBy": "Nguyen Van A",           // Tên crew
  "completedByCrewId": "CM001",            // CrewId (from JWT)
  "completedAt": "2024-10-20T10:30:00Z",   // Client timestamp
  "runningHoursAtCompletion": 13000.5,     // Giờ chạy thiết bị
  "sparePartsUsed": "Oil Filter x2, Engine Oil 20L", // Phụ tùng
  "notes": "Replaced oil filter. Cooling system OK. No leaks found.", // Ghi chú
  "photoUrls": ["photo1.jpg", "photo2.jpg"] // Optional: Photos
}
```

**2. Task Start**
```json
POST /api/maintenance/tasks/{id}/start
{
  "startedBy": "Nguyen Van A",
  "startedByCrewId": "CM001",
  "startedAt": "2024-10-20T08:00:00Z"
}
```

**3. Offline Sync Queue Item**
```json
{
  "id": "uuid-1234-5678",
  "type": "TASK_COMPLETE",
  "data": { /* completion data */ },
  "createdAt": "2024-10-20T10:30:00Z",
  "retryCount": 0
}
```

---

## 📱 MOBILE APP CACHE STRUCTURE

Mobile app nên cache các data sau (Hive/SharedPreferences):

### 1. User Session
```dart
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "refresh_token_here",
  "userId": 1,
  "crewId": "CM001",
  "fullName": "Nguyen Van A",
  "position": "Chief Engineer",
  "expiresAt": "2024-10-20T12:00:00Z"
}
```

### 2. My Profile (Cache 1 hour)
```dart
{
  "crewMember": { /* full CrewMember object */ },
  "cachedAt": "2024-10-20T10:00:00Z"
}
```

### 3. My Tasks (Cache 30 minutes)
```dart
{
  "tasks": [ /* array of MaintenanceTask */ ],
  "cachedAt": "2024-10-20T10:00:00Z"
}
```

### 4. Sync Queue (Persistent - no expiry)
```dart
[
  {
    "id": "uuid-1",
    "type": "TASK_COMPLETE",
    "taskId": 1,
    "data": { /* completion data */ },
    "timestamp": "2024-10-20T10:30:00Z",
    "retryCount": 0
  }
]
```

---

## 🔐 JWT TOKEN PAYLOAD

Access Token payload (Mobile App nhận từ login):

```json
{
  "sub": "1",                              // UserId
  "crewId": "CM001",                       // CrewId
  "username": "CM001",                     // Username
  "fullName": "Nguyen Van A",              // Display name
  "position": "Chief Engineer",            // Position
  "role": "CREW",                          // Role
  "iat": 1697800000,                       // Issued at
  "exp": 1697806000,                       // Expires at (1 hour)
  "iss": "maritime-edge-server",           // Issuer
  "aud": "maritime-mobile-app"             // Audience
}
```

---

## 🛠️ MIGRATION SCRIPTS CẦN CHẠY

### 1. Add Authentication Fields to CrewMembers
```sql
ALTER TABLE "CrewMembers"
    ADD COLUMN "UpdatedAt" TIMESTAMP,
    ADD COLUMN "LastModifiedBy" VARCHAR(100);
```

### 2. Add New Fields to MaintenanceTasks
```sql
ALTER TABLE "MaintenanceTasks"
    ADD COLUMN "AssignedToCrewId" VARCHAR(50),
    ADD COLUMN "CompletedByCrewId" VARCHAR(50),
    ADD COLUMN "RunningHoursAtCompletion" DOUBLE PRECISION,
    ADD COLUMN "PhotoUrls" TEXT,
    ADD COLUMN "UpdatedAt" TIMESTAMP,
    ADD COLUMN "LastModifiedBy" VARCHAR(100);

-- Add foreign key
ALTER TABLE "MaintenanceTasks"
    ADD CONSTRAINT fk_maintenance_assignedto 
    FOREIGN KEY ("AssignedToCrewId") REFERENCES "CrewMembers"("CrewId");
```

### 3. Create Users Table
```sql
-- See schema above
```

### 4. Create RefreshTokens Table
```sql
-- See schema above
```

### 5. Create AuditLogs Table
```sql
-- See schema above
```

### 6. Insert Sample Users (for testing)
```sql
-- Password: "password123" (BCrypt hash)
INSERT INTO "Users" ("CrewId", "Username", "PasswordHash", "Salt", "Role")
VALUES 
    ('CM001', 'CM001', '$2a$10$...hash...', 'salt1', 'CREW'),
    ('CM002', 'CM002', '$2a$10$...hash...', 'salt2', 'CREW'),
    ('CAP001', 'CAP001', '$2a$10$...hash...', 'salt3', 'CAPTAIN');
```

---

## 📊 INDEX RECOMMENDATIONS

Để tối ưu performance cho Mobile App queries:

```sql
-- CrewMembers
CREATE INDEX idx_crewmembers_crewid ON "CrewMembers"("CrewId");
CREATE INDEX idx_crewmembers_certificate_expiry ON "CrewMembers"("CertificateExpiry");
CREATE INDEX idx_crewmembers_medical_expiry ON "CrewMembers"("MedicalExpiry");

-- MaintenanceTasks (QUAN TRỌNG cho Mobile)
CREATE INDEX idx_maintenance_assignedto_crewid ON "MaintenanceTasks"("AssignedToCrewId");
CREATE INDEX idx_maintenance_status_crewid ON "MaintenanceTasks"("Status", "AssignedToCrewId");
CREATE INDEX idx_maintenance_nextdue ON "MaintenanceTasks"("NextDueAt");
CREATE INDEX idx_maintenance_priority ON "MaintenanceTasks"("Priority");

-- Users
CREATE INDEX idx_users_crewid ON "Users"("CrewId");
CREATE INDEX idx_users_username ON "Users"("Username");
CREATE INDEX idx_users_isactive ON "Users"("IsActive");

-- AuditLogs
CREATE INDEX idx_auditlogs_crewid_timestamp ON "AuditLogs"("CrewId", "Timestamp");
```

---

## 🚀 BACKEND API ENDPOINTS CẦN TẠO

Dựa trên database schema, cần tạo các endpoints:

### Authentication
```
POST   /api/auth/login              - Login with CrewId + Password
POST   /api/auth/refresh            - Refresh access token
POST   /api/auth/logout             - Logout & revoke token
POST   /api/auth/change-password    - Change password
```

### Crew Profile
```
GET    /api/crew/me                 - Get my profile
GET    /api/crew/me/certificates    - Get my certificates status
```

### Maintenance Tasks
```
GET    /api/maintenance/tasks/my-tasks              - Get tasks assigned to me
GET    /api/maintenance/tasks/{id}                  - Get task detail
POST   /api/maintenance/tasks/{id}/start            - Start task
POST   /api/maintenance/tasks/{id}/complete         - Complete task
GET    /api/maintenance/tasks/upcoming              - Get upcoming tasks
GET    /api/maintenance/schedule/my-schedule        - Get my schedule
```

### Sync
```
GET    /api/sync/status             - Get sync queue status
POST   /api/sync/check              - Check if device needs sync
```

---

## ✅ CHECKLIST IMPLEMENTATION

### Phase 1: Database Migration
- [ ] Add UpdatedAt, LastModifiedBy to CrewMembers
- [ ] Add new fields to MaintenanceTasks
- [ ] Create Users table
- [ ] Create RefreshTokens table
- [ ] Create AuditLogs table
- [ ] Create indexes
- [ ] Insert sample users for testing

### Phase 2: Backend API
- [ ] Implement JWT authentication
- [ ] Create AuthController (login, refresh, logout)
- [ ] Update MaintenanceController (my-tasks, start, complete)
- [ ] Create CrewController (me, certificates)
- [ ] Add audit logging middleware
- [ ] Test all endpoints with Postman

### Phase 3: Mobile App
- [ ] Implement Dart models matching database schema
- [ ] Implement API client with JWT interceptor
- [ ] Implement authentication flow
- [ ] Implement task CRUD with offline support
- [ ] Implement sync queue
- [ ] Test offline scenarios

---

## 📝 NOTES

1. **Password Hash**: Sử dụng BCrypt với salt để hash password
2. **JWT**: Access token expire sau 1 hour, refresh token sau 7 days
3. **Audit**: Mọi thao tác từ Mobile đều ghi vào AuditLogs
4. **Timezone**: Tất cả timestamps dùng UTC
5. **Sync**: IsSynced = true khi đã đồng bộ với shore server (không phải Mobile)
6. **Photos**: PhotoUrls lưu JSON array của file paths/URLs

---

**🚢 This schema ensures complete synchronization between Mobile App and Edge Server! ⚓**
