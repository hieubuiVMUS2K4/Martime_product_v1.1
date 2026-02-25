# Abstract Log - Kế hoạch triển khai

## Kiến trúc: 3 bảng DB

| Bảng | Mô tả |
|------|--------|
| `AbstractLogVoyage` | Header — 1 record/voyage, chứa admin info + time summary + fuel ROB reconciliation |
| `AbstractLogLeg` | Leg header — N records/voyage (dynamic multi-leg), chứa port/draft/totals cho mỗi chặng |
| `AbstractLogDailyEntry` | Daily rows — N records/leg, chứa noon position, weather, hours, distance, FOC matrix |

## Dữ liệu tự động kéo vs. nhập tay

**Auto-fill từ hệ thống hiện có:**
- Vessel info (appsettings), Voyage Number, Master/CE (từ CrewAssignment)
- Ports, Draft (từ DepartureReport/ArrivalReport/PortCall)
- Noon position, Weather, Distance, Speed, RPM (từ NoonReport)
- FOC tổng M/E, D/E, Boiler (từ EngineLogBook)
- ROB FO, DO, LO, FW (từ NoonReport/DepartureReport/ArrivalReport)

**Cần nhập tay (trường mới):**
- Hours breakdown: Propelling / Drifting / Anchor / Port per day
- FOC matrix: 3 equipment × 3 fuel types × 3 periods
- Distance Prop / Distance Log (tách từ OG)
- Slip %, Shaft Revolutions
- Cylinder/System/Generator Oil (tách từ LubOil)
- Propeller Pitch, Date of Last Docking

## API Endpoints

```
GET    /api/abstract-log?voyageId=         — List all abstract logs (filter by voyage)
GET    /api/abstract-log/{id}              — Get full abstract log with legs + daily entries
POST   /api/abstract-log                   — Create (auto-fill from voyage data)
PUT    /api/abstract-log/{id}              — Update SUM sheet fields
POST   /api/abstract-log/{id}/legs         — Add new leg (auto-numbered)
PUT    /api/abstract-log/legs/{legId}      — Update leg header
DELETE /api/abstract-log/legs/{legId}      — Delete leg + re-sequence remaining
POST   /api/abstract-log/legs/{legId}/entries — Add daily entry
PUT    /api/abstract-log/entries/{entryId} — Update daily entry
DELETE /api/abstract-log/entries/{entryId} — Delete daily entry
POST   /api/abstract-log/{id}/auto-fill    — Re-pull data from NoonReport/EngineLog
```

## Frontend: Route `/logbooks/abstract`

Dynamic tabs: Summary + N leg tabs (sorted by sequence) + "+" add leg button
- Summary: Form-based summary with fuel reconciliation table
- Leg tabs: Auto-labeled (e.g. "Hai Phong → Singapore"), editable data grid (daily entries), delete leg button
- Auto-fill button pulls from existing reports, auto-generates leg labels from port calls
