# Phase 1 Shore Auth Matrix

## Muc tieu

Tai lieu nay chot auth matrix cho cac nhom Shore API trong Phase 1 theo huong khong pha vo Shore frontend hien tai.

## Nguyen tac

- Shore frontend hien chua co login flow va chua cap Bearer token.
- Vi vay Phase 1 enforce cac nhom API dang truoc day de `AllowAnonymous` bang `InternalAccess`.
- `InternalAccess` duoc cap qua reverse proxy frontend va co the bat bang `Security:RequireInternalAccess=true` khi da cau hinh `INTERNAL_API_KEY`.
- Sau Phase 1, khi Shore co user-auth UI day du, cac nhom nay se duoc chuyen sang business policy dich ben duoi.

## Matrix

| Nhom API | Route prefix | Phase 1 enforcement hien tai | Policy dich sau khi co Shore auth UI |
|---|---|---|---|
| Sync observability/admin | `/api/sync`, `/api/sync/dashboard`, `/api/health/ready` | `InternalAccess` + rate limiting | `InternalAccess` |
| Notifications | `/api/notifications` | `InternalAccess` | `CrewReadOnly` |
| Vessel certificate assignments | `/api/vessels/{vesselId}/certificates` | `InternalAccess` | `ComplianceManagement` |
| Crew CRUD va documents | `/api/crew` | `InternalAccess` | `CrewReadOnly` for GET, `CrewManagement` for write |
| Countries master data | `/api/countries` | `InternalAccess` | `CrewReadOnly` for GET, `CrewManagement` for write |
| Ranks master data | `/api/ranks` | `InternalAccess` | `CrewReadOnly` for GET, `CrewManagement` for write |
| Certificates va crew certificates | `/api/certificates` | `InternalAccess` | `CrewReadOnly` for GET, `ComplianceManagement` for write |
| Compliance engine | `/api/compliance` | `InternalAccess` | `CrewReadOnly` for evaluate/snapshot/fleet, `ComplianceManagement` for rule/waiver/write |
| Document workflow | `/api/document-submissions` | `InternalAccess` | `ComplianceManagement` |
| Onboarding | `/api/onboarding-cases` | `InternalAccess` | `CrewManagement` |
| Assignment management | `/api/assignments` | `InternalAccess` | `FleetManagement` |
| External requests | `/api/external-requests` | `InternalAccess` | `FleetManagement` |
| Travel requests | `/api/travel-requests` | `InternalAccess` | `TravelManagement` |
| Onboard events | `/api/onboard-events` | `InternalAccess` | `OnboardManagement` |
| Crew profile lifecycle | `/api/crew-profiles` | `InternalAccess` | `CrewManagement` |
| Audit logs | `/api/audit-logs` | `InternalAccess` | `InternalAccess` hoac policy audit rieng |

## Dieu kien bat enforcement

1. Frontend Shore phai di qua reverse proxy co inject `X-Internal-Api-Key`.
2. `INTERNAL_API_KEY` cua frontend va `InternalAccess:ApiKey` cua backend phai khop nhau.
3. Neu dung Vite dev proxy, can set `INTERNAL_API_KEY` de proxy gui header noi bo.
4. Sau khi xac nhan cac man hinh Shore hoat dong on dinh, bat `Security:RequireInternalAccess=true`.