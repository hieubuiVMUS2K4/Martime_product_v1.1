# hooks/ — Custom Hooks bọc quanh services/

## Mục đích

Mỗi hook domain trong thư mục này làm đúng một việc: gọi một hàm trong `services/`, quản lý `data/loading/error` bằng `useState`, tự `useEffect` fetch khi mount hoặc khi dependency đổi, và trả về `{ data, loading, error, refetch }` (đôi khi thêm `setFilters`, `setData`...). Đây là lớp trung gian giúp page không phải viết `useEffect` gọi API lặp đi lặp lại.

## Cấu trúc & vai trò

| File | Hooks export | Service đứng sau | Dùng chủ yếu ở |
|---|---|---|---|
| `useCrew.ts` | `useCrewList`, `useCrewDetail`, `useCrewCertificates`, `useExpiringCertificates`, `useCompliance`, `useCrewStats`, `useVessels`, `useReferenceData` | `crew.service.ts` | `CrewManagement/*`, `CategoryManagement`, `AssignmentManagement` |
| `useAssignment.ts` | `useManningStandards`, `useManningStandard`, `useAssignments`, `useAssignment`, `useAssignmentConflicts`, `useAssignmentComments`, `useAssignmentHistory`, `useVesselPlanningBoard` | `assignment.service.ts` | `AssignmentManagement/*` |
| `useCompliance.ts` | `useComplianceRuleSets`, `useComplianceRuleSet`, `useComplianceRules`, `useComplianceWaivers`, `useCrewCompliance`, `useFleetCompliance` | `compliance.service.ts` | `ComplianceManagement/*` |
| `useCrewManagement.ts` | `useOnboardingCases`, `useOnboardingCase`, `useCrewOnboarding`, `useCrewDocumentSubmissions`, `useVerificationQueue`, `useCrewStatusHistory`, `useCrewAuditLog` | `crewManagement.service.ts` | `OnboardingManagement/*`, `DocumentWorkflow/*` |
| `useOnboard.ts` | `useOnboardEvents`, `useAccessGrants`, `useSignOns`, `useSignOffs` | `onboard.service.ts` | `OnboardManagement/OnboardDashboardPage` |
| `useTravel.ts` | `useTravelRequests`, `useTravelRequest`, `useTravelHistory` | `travel.service.ts` | `TravelManagement/*` |
| `useExternalRequest.ts` | `useExternalRequests`, `useExternalRequest`, `useCandidates`, `useMessages` | `externalRequest.service.ts` | `ExternalRequestManagement/*` |
| `useDebounce.ts` | `useDebounce<T>(value, delay=300)` | *(không gọi API)* | Debounce ô tìm kiếm — dùng trong `useCrewList` (search/rankName/department/vesselName) và nhiều form filter. |
| `useToggle.ts` | *(không có gì)* | — | **File rỗng.** Không export bất kỳ hook nào; không có nơi nào import. |

Không có hook nào cho: PMS (`pms.service.ts`, `equipment-*`, `maintenance-*` được gọi thẳng từ page/component, không qua hook riêng), Materials, Sync, Voyage — các domain này gọi service trực tiếp trong page bằng `useState`+`useEffect` viết tay.

## Luồng hoạt động chính

```
Page.tsx
  const { data, loading, error, refetch } = useCrewList();
       │
       ▼
hooks/useCrew.ts
  useState + useCallback(fetchCrew) + useEffect(() => { fetchCrew() }, [fetchCrew])
       │  gọi
       ▼
services/crew.service.ts → crewApi.getAll(...)
```

Một số hook có thêm kỹ thuật đáng chú ý:
- `useCrewList`: dùng `useDebounce` cho 4 field lọc, và `AbortController` để hủy request cũ khi filter đổi nhanh (tránh race condition hiển thị kết quả cũ đè lên kết quả mới).
- `useVessels`, `useReferenceData` (trong `useCrew.ts`): dùng biến **module-level** (`let cachedVessels`, `let cachedRanks`...) để cache ngoài React state — cache tồn tại suốt vòng đời tab trình duyệt (không tự invalidate), không phải chỉ trong 1 lần mount component.

## Liên kết với phần khác

- **services/**: mọi hook ở đây chỉ là lớp mỏng bọc quanh đúng 1 file service cùng tên miền — xem `services/README.md` để biết endpoint thật.
- **types/**: mỗi hook import type request/response tương ứng trong `types/*.types.ts`.
- **pages/**: là nơi tiêu thụ chính; nhiều modal (`CrewFormModal`, `AssignShipModal`...) cũng gọi hook trực tiếp.

## Ghi chú khi đọc/dạy

- `useToggle.ts` **rỗng** và **không được dùng ở đâu cả** (đã kiểm tra bằng grep) — đừng mất công tìm chỗ gọi nó; nếu cần toggle boolean, code hiện tại chỉ dùng `useState(false)` thủ công.
- Không có hook nào trong thư mục này dùng `@tanstack/react-query` — tất cả là `useState`/`useEffect` viết tay (xem thêm `store/README.md`). Nếu được giao "tối ưu lại data-fetching", đây chính là các ứng viên đầu tiên để chuyển sang `useQuery`.
- **Bẫy đặt tên dễ nhầm**: nếu tìm hook `useCompliance()`, bản năng sẽ mở file `hooks/useCompliance.ts` — nhưng file đó **không** export hàm nào tên chính xác là `useCompliance` (chỉ có `useComplianceRuleSets`, `useComplianceRuleSet`, `useComplianceRules`, `useComplianceWaivers`, `useCrewCompliance`, `useFleetCompliance`). Hàm thực sự tên `useCompliance()` lại nằm trong `hooks/useCrew.ts`, và gọi `certificateApi.getCompliance()` (từ `crew.service.ts` — báo cáo compliance rút gọn theo crew), khác hẳn nghiệp vụ rule-set/waiver/evaluation engine mà `compliance.service.ts` cung cấp. Luôn kiểm tra import path (`from '../../hooks/useCrew'` hay `from '../../hooks/useCompliance'`) trước khi kết luận đang dùng bản nào.
