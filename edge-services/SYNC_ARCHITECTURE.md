# 🔄 Data Synchronization Architecture - Ship-to-Shore

## 📡 Tổng Quan Kiến Trúc Đồng Bộ

```
┌─────────────────────────────────────────────────────────────────┐
│                    TÀU (EDGE SERVER)                             │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Thuyền viên │  │   NMEA       │  │   Modbus     │          │
│  │  Mobile App  │  │   Sensors    │  │   Engines    │          │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │
│         │                  │                  │                  │
│         ▼                  ▼                  ▼                  │
│  ┌───────────────────────────────────────────────────┐          │
│  │         PostgreSQL Edge Database                  │          │
│  │  - 12 tables (position, engine, fuel, etc.)      │          │
│  │  - is_synced flag on every record                 │          │
│  │  - Local storage: 30-90 days retention            │          │
│  └───────────────────┬───────────────────────────────┘          │
│                      │                                           │
│  ┌───────────────────▼───────────────────────────────┐          │
│  │          SYNC SERVICE (Background Service)        │          │
│  │                                                     │          │
│  │  1. Collect unsync records (is_synced = false)    │          │
│  │  2. Group by priority & table                      │          │
│  │  3. Compress (gzip) → Batch size: 100-1000 rows   │          │
│  │  4. HTTPS POST to shore API                        │          │
│  │  5. Mark as synced (is_synced = true)             │          │
│  │                                                     │          │
│  │  Retry Logic:                                      │          │
│  │  - Exponential backoff: 1m → 5m → 15m → 1h        │          │
│  │  - Max retries: 5 attempts                         │          │
│  │  - Store in sync_queue table                       │          │
│  └───────────────────┬───────────────────────────────┘          │
│                      │                                           │
│                      │ HTTPS/TLS 1.3                            │
│                      │ JWT Authentication                        │
│                      │ Compressed JSON/Protobuf                  │
└──────────────────────┼───────────────────────────────────────────┘
                       │
                       │ 📡 Kết nối:
                       │ - VSAT (64-512 kbps)
                       │ - 4G/LTE (coastal)
                       │ - Starlink (50-150 Mbps)
                       │ - Iridium (backup, 2.4 kbps)
                       │
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│                  BỜ (SHORE BACKEND)                              │
│                                                                   │
│  ┌────────────────────────────────────────────────────┐          │
│  │         API Gateway (ASP.NET Core)                 │          │
│  │  - /api/sync/position                              │          │
│  │  - /api/sync/engine-data                           │          │
│  │  - /api/sync/fuel-consumption                      │          │
│  │  - /api/sync/alarms                                │          │
│  │                                                     │          │
│  │  Validate:                                         │          │
│  │  - JWT token (identify ship: MMSI/IMO)            │          │
│  │  - Data integrity (checksum)                       │          │
│  │  - Duplicate detection (timestamp + vessel_id)     │          │
│  └────────────────────┬───────────────────────────────┘          │
│                       │                                           │
│                       ▼                                           │
│  ┌────────────────────────────────────────────────────┐          │
│  │      PostgreSQL Central Database                   │          │
│  │                                                     │          │
│  │  Tables (với vessel_id FK):                        │          │
│  │  - position_data (vessel_id, timestamp, lat, lng)  │          │
│  │  - engine_data (vessel_id, engine_id, rpm, ...)   │          │
│  │  - fuel_consumption (vessel_id, fuel_type, ...)   │          │
│  │  - safety_alarms (vessel_id, alarm_type, ...)     │          │
│  │  - voyage_records (vessel_id, voyage_number, ...) │          │
│  │                                                     │          │
│  │  Schema khác biệt:                                 │          │
│  │  + vessel_id (reference to vessels table)          │          │
│  │  + received_at (timestamp khi nhận từ tàu)         │          │
│  │  + data_source (EDGE/MANUAL/API)                   │          │
│  └────────────────────┬───────────────────────────────┘          │
│                       │                                           │
│                       ▼                                           │
│  ┌────────────────────────────────────────────────────┐          │
│  │         Web Dashboard (React)                      │          │
│  │  - Fleet Overview (all ships)                      │          │
│  │  - Real-time Monitoring                            │          │
│  │  - Reports & Analytics                             │          │
│  │  - Alerts Dashboard                                │          │
│  └────────────────────────────────────────────────────┘          │
│         ▲                    ▲                    ▲              │
│         │                    │                    │              │
│  ┌──────┴─────┐      ┌──────┴──────┐     ┌──────┴──────┐       │
│  │   Admin    │      │ Head Office │     │   Manager   │       │
│  │  (All Data)│      │ (Fleet View)│     │(Single Ship)│       │
│  └────────────┘      └─────────────┘     └─────────────┘       │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Cơ Chế Đồng Bộ (Sync Mechanism)

### **1. Store-and-Forward Pattern**

#### **Bước 1: Thu thập dữ liệu chưa sync**
```csharp
// Edge Server - SyncService.cs
public async Task<List<PositionData>> GetUnsyncedPositions(int batchSize = 500)
{
    return await _context.PositionData
        .Where(p => !p.IsSynced)
        .OrderBy(p => p.Timestamp)  // Oldest first
        .Take(batchSize)
        .ToListAsync();
}
```

#### **Bước 2: Nén và gửi theo batch**
```csharp
public async Task<bool> SyncPositionData()
{
    var unsyncedData = await GetUnsyncedPositions(500);
    if (!unsyncedData.Any()) return true;
    
    // Serialize to JSON
    var json = JsonSerializer.Serialize(new {
        vessel_mmsi = _config.VesselMMSI,
        vessel_imo = _config.VesselIMO,
        data = unsyncedData,
        checksum = CalculateChecksum(unsyncedData)
    });
    
    // Compress with gzip (70-90% reduction)
    var compressed = await CompressAsync(json);
    
    // HTTP POST to shore API
    var response = await _httpClient.PostAsync(
        $"{_shoreApiUrl}/api/sync/position",
        new ByteArrayContent(compressed)
    );
    
    if (response.IsSuccessStatusCode)
    {
        // Mark as synced
        foreach (var item in unsyncedData)
            item.IsSynced = true;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Synced {unsyncedData.Count} position records");
        return true;
    }
    else
    {
        // Add to retry queue
        await AddToRetryQueue("position_data", unsyncedData);
        return false;
    }
}
```

#### **Bước 3: Shore API nhận và lưu**
```csharp
// Shore Backend - SyncController.cs
[HttpPost("api/sync/position")]
[Authorize] // JWT validation
public async Task<IActionResult> ReceivePositionData()
{
    // 1. Decompress
    var json = await DecompressRequestBody();
    
    // 2. Parse JSON
    var payload = JsonSerializer.Deserialize<SyncPayload>(json);
    
    // 3. Validate checksum
    if (!ValidateChecksum(payload.Data, payload.Checksum))
        return BadRequest("Checksum mismatch");
    
    // 4. Get vessel from JWT token
    var vesselId = GetVesselIdFromToken(User);
    
    // 5. Bulk insert to central database
    var records = payload.Data.Select(p => new PositionData
    {
        VesselId = vesselId,  // Add vessel FK
        Timestamp = p.Timestamp,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        // ... other fields
        ReceivedAt = DateTime.UtcNow,  // Track when received
        DataSource = "EDGE"
    }).ToList();
    
    await _context.PositionData.AddRangeAsync(records);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation($"Received {records.Count} position records from vessel {vesselId}");
    
    return Ok(new { 
        received = records.Count,
        timestamp = DateTime.UtcNow 
    });
}
```

---

## 📊 Sync Priority & Frequency

| Data Type | Priority | Batch Size | Sync Interval | Bandwidth Usage |
|-----------|----------|----------|---------------|-----------------|
| **Safety Alarms** | 1 (CRITICAL) | 1-10 | Immediate | ~2-5 KB |
| **Position (GPS)** | 2 (HIGH) | 100-500 | Every 5 min | ~50-100 KB |
| **Engine Data** | 3 (HIGH) | 100-500 | Every 15 min | ~100-200 KB |
| **Fuel Consumption** | 4 (MEDIUM) | 50-100 | Daily | ~10-20 KB |
| **AIS Data** | 5 (MEDIUM) | 100-200 | Every 10 min | ~30-60 KB |
| **Environmental** | 6 (LOW) | 50-100 | Hourly | ~5-10 KB |
| **Tank Levels** | 7 (LOW) | 50-100 | Every 6 hours | ~5-10 KB |

**Tổng băng thông ước tính:** ~500 KB/hour (~12 MB/day)

---

## 🔁 Retry Logic với Exponential Backoff

```csharp
public class SyncRetryPolicy
{
    private static readonly int[] RetryDelays = { 1, 5, 15, 60, 240 }; // minutes
    
    public async Task<bool> ExecuteWithRetry(Func<Task<bool>> syncAction, 
        string tableName, long recordId)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var success = await syncAction();
                if (success) return true;
                
                // Wait before retry
                var delayMinutes = RetryDelays[attempt];
                _logger.LogWarning(
                    $"Sync failed for {tableName}:{recordId}. " +
                    $"Retry {attempt + 1}/5 in {delayMinutes} minutes"
                );
                
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Network error during sync: {ex.Message}");
                
                if (attempt == 4) // Last attempt
                {
                    // Store in dead-letter queue for manual review
                    await AddToDeadLetterQueue(tableName, recordId, ex.Message);
                    return false;
                }
            }
        }
        
        return false;
    }
}
```

---

## 🗄️ Sync Queue Table Design

### **Edge Database: sync_queue**
```sql
CREATE TABLE sync_queue (
    id BIGSERIAL PRIMARY KEY,
    table_name VARCHAR(50) NOT NULL,      -- 'position_data', 'engine_data', etc.
    record_id BIGINT NOT NULL,             -- FK to actual table
    payload TEXT NOT NULL,                 -- JSON snapshot of data
    priority INTEGER NOT NULL DEFAULT 5,   -- 1=highest, 10=lowest
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 5,
    next_retry_at TIMESTAMPTZ,             -- When to retry next
    last_error VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    synced_at TIMESTAMPTZ                  -- NULL = not synced yet
);

CREATE INDEX idx_sync_priority_retry 
    ON sync_queue(priority, next_retry_at) 
    WHERE synced_at IS NULL;

CREATE INDEX idx_sync_table_record 
    ON sync_queue(table_name, record_id);
```

### **Background Service Logic**
```csharp
public class SyncBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Sync high priority items first
                await SyncByPriority(1, stoppingToken); // Alarms
                await SyncByPriority(2, stoppingToken); // Position
                await SyncByPriority(3, stoppingToken); // Engine
                
                // 2. Process retry queue
                await ProcessRetryQueue(stoppingToken);
                
                // 3. Wait before next cycle
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync service error");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
    
    private async Task SyncByPriority(int priority, CancellationToken ct)
    {
        // Get items to sync for this priority
        var items = await _context.SyncQueue
            .Where(q => q.Priority == priority && 
                       q.SyncedAt == null &&
                       q.RetryCount < q.MaxRetries &&
                       (q.NextRetryAt == null || q.NextRetryAt <= DateTime.UtcNow))
            .OrderBy(q => q.CreatedAt)
            .Take(100)
            .ToListAsync(ct);
        
        foreach (var item in items)
        {
            var success = await SyncItem(item, ct);
            
            if (success)
            {
                item.SyncedAt = DateTime.UtcNow;
            }
            else
            {
                item.RetryCount++;
                item.NextRetryAt = DateTime.UtcNow.AddMinutes(
                    Math.Pow(2, item.RetryCount) // Exponential: 2, 4, 8, 16, 32 minutes
                );
            }
            
            await _context.SaveChangesAsync(ct);
        }
    }
}
```

---

## 🔐 Authentication & Security

### **1. JWT Token cho tàu**
```csharp
// Edge Server startup - Get JWT token once per voyage
public async Task<string> AuthenticateVessel()
{
    var credentials = new {
        vessel_mmsi = _config.VesselMMSI,
        vessel_imo = _config.VesselIMO,
        api_key = _config.ApiKey
    };
    
    var response = await _httpClient.PostAsJsonAsync(
        $"{_shoreApiUrl}/api/auth/vessel-login",
        credentials
    );
    
    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
    
    // Store token for 30 days
    _jwtToken = result.Token;
    _tokenExpiry = result.ExpiresAt;
    
    return _jwtToken;
}

// Attach to every sync request
_httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", _jwtToken);
```

### **2. Data Integrity - Checksum**
```csharp
public string CalculateChecksum(List<PositionData> data)
{
    var json = JsonSerializer.Serialize(data);
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
    return Convert.ToBase64String(hash);
}
```

### **3. TLS 1.3 Encryption**
```csharp
// Always use HTTPS in production
var handler = new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls13,
    ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateHandler // Dev only!
};

_httpClient = new HttpClient(handler)
{
    BaseAddress = new Uri(_config.ShoreApiUrl),
    Timeout = TimeSpan.FromMinutes(5)
};
```

---

## 📉 Bandwidth Optimization

### **1. Compression (gzip)**
```csharp
public async Task<byte[]> CompressAsync(string data)
{
    var bytes = Encoding.UTF8.GetBytes(data);
    
    using var output = new MemoryStream();
    using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
    {
        await gzip.WriteAsync(bytes, 0, bytes.Length);
    }
    
    var compressed = output.ToArray();
    
    _logger.LogInformation(
        $"Compressed {bytes.Length} bytes → {compressed.Length} bytes " +
        $"(ratio: {(1 - (double)compressed.Length / bytes.Length):P1})"
    );
    
    return compressed;
}
```

### **2. Delta Encoding (cho time-series)**
```csharp
// Chỉ gửi thay đổi so với giá trị trước
public class DeltaEncoder
{
    public List<EngineDeltaData> EncodeDelta(List<EngineData> data)
    {
        var result = new List<EngineDeltaData>();
        EngineData? previous = null;
        
        foreach (var current in data)
        {
            if (previous == null)
            {
                // First record: send full data
                result.Add(new EngineDeltaData(current, isFullRecord: true));
            }
            else
            {
                // Subsequent records: send only changes
                result.Add(new EngineDeltaData
                {
                    Timestamp = current.Timestamp,
                    RpmDelta = current.Rpm - previous.Rpm,
                    LoadDelta = current.LoadPercent - previous.LoadPercent,
                    // ... only fields that changed significantly
                });
            }
            
            previous = current;
        }
        
        return result;
    }
}
```

### **3. Batch Aggregation**
```csharp
// Aggregate 1-second samples to 1-minute averages
public class DataAggregator
{
    public async Task<List<EngineData>> AggregateToMinuteAverages()
    {
        return await _context.EngineData
            .Where(e => !e.IsSynced)
            .GroupBy(e => new {
                e.EngineId,
                Minute = new DateTime(e.Timestamp.Year, e.Timestamp.Month, 
                                     e.Timestamp.Day, e.Timestamp.Hour, 
                                     e.Timestamp.Minute, 0)
            })
            .Select(g => new EngineData
            {
                EngineId = g.Key.EngineId,
                Timestamp = g.Key.Minute,
                Rpm = g.Average(e => e.Rpm),
                LoadPercent = g.Average(e => e.LoadPercent),
                CoolantTemp = g.Average(e => e.CoolantTemp),
                // ...
            })
            .ToListAsync();
    }
}
```

---

## 🚨 Handling Network Failures

### **Scenario 1: Tàu mất mạng 3 ngày**
1. Edge Server tiếp tục thu thập dữ liệu → PostgreSQL local
2. Records có `is_synced = false` tích lũy
3. Khi có mạng trở lại:
   - SyncService tự động detect kết nối
   - Sync theo priority: Alarms → Position → Engine → Fuel
   - Batch size tự động tăng (500 → 1000 records)
   - Process ~72 hours of data in ~2-4 hours

### **Scenario 2: API Shore bị down**
1. Edge Server retry với exponential backoff
2. Sau 5 attempts failed → move to sync_queue
3. Admin nhận alert email
4. Khi API recovery → automatic resume sync

### **Scenario 3: Băng thông thấp (<10 kbps)**
1. Giảm batch size: 500 → 100 records
2. Tăng compression level
3. Chỉ sync critical data (alarms, position)
4. Delay low-priority data (environmental, tank levels)

---

## 📊 Monitoring & Metrics

### **Dashboard Metrics**
```csharp
public class SyncMetrics
{
    public int RecordsPendingSync { get; set; }
    public int RecordsSyncedToday { get; set; }
    public int FailedSyncAttempts { get; set; }
    public double AverageSyncLatency { get; set; } // seconds
    public long TotalBytesSentToday { get; set; }
    public DateTime? LastSuccessfulSync { get; set; }
    public string ConnectionStatus { get; set; } // ONLINE/OFFLINE/SLOW
}

// Expose via API endpoint
[HttpGet("api/sync/metrics")]
public async Task<SyncMetrics> GetSyncMetrics()
{
    return new SyncMetrics
    {
        RecordsPendingSync = await _context.PositionData
            .CountAsync(p => !p.IsSynced),
        RecordsSyncedToday = await _context.SyncQueue
            .CountAsync(q => q.SyncedAt >= DateTime.Today),
        // ...
    };
}
```

---

## ✅ Best Practices

1. **Always use `is_synced` flag** trên mọi table có data cần đồng bộ
2. **Implement idempotent APIs** - Shore API phải handle duplicate records
3. **Checksum validation** - Detect data corruption during transmission
4. **Compress before send** - Save 70-90% bandwidth
5. **Priority queue** - Critical alarms sync immediately
6. **Exponential backoff** - Don't overwhelm network on failures
7. **Monitor sync lag** - Alert if >2 hours behind
8. **Local retention** - Keep 30-90 days on edge for audit
9. **Dead-letter queue** - Handle permanently failed records
10. **JWT rotation** - Refresh tokens every 24 hours

---

*Maritime Edge-to-Shore Synchronization - Reliable, Efficient, Secure* 🚢📡🌊
