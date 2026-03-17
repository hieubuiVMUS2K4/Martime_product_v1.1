# KẾ HOẠCH TRIỂN KHAI ĐỒNG BỘ QUẢN LÝ HÀNH TRÌNH GIỮA TÀU VÀ BỜ

> Ngày tạo: 15/03/2026  
> Phạm vi: Edge (tàu) <-> Shore (bờ)  
> Mục tiêu: Shore phải nhận đầy đủ dữ liệu voyage đang có trên tàu, đồng thời bổ sung lớp điều hành, phân tích, kiểm soát và tổng hợp mà tàu không nhất thiết phải giữ.

---

## 1. Mục tiêu nghiệp vụ

Phần đồng bộ voyage giữa tàu và bờ không chỉ là copy dữ liệu kỹ thuật. Đây là năng lực để Shore nhìn thấy cùng một bức tranh vận hành mà tàu đang có, sau đó làm giàu thêm dữ liệu để phục vụ điều hành đội tàu, kiểm soát chi phí, hiệu quả chuyến đi, tuân thủ và ra quyết định.

Nguyên tắc cốt lõi:

- Tàu có gì thì bờ phải có cái đó.
- Bờ được phép có nhiều hơn tàu, nhưng không được làm sai dữ kiện thực tế do tàu ghi nhận.
- Dữ liệu factual phát sinh trên tàu phải về bờ theo hướng gần thời gian thực tùy điều kiện mạng.
- Dữ liệu planning, phê duyệt, phân tích, commercial và governance có thể phát sinh ở bờ rồi đồng bộ ngược lại xuống tàu khi cần.
- Mọi bản ghi voyage phải truy vết được nguồn phát sinh, phiên bản đồng bộ, lịch sử thay đổi và trạng thái xử lý xung đột.

---

## 2. Bối cảnh hiện trạng

### 2.1. Những gì Edge đang có

Edge hiện đã có nền tảng voyage tương đối đầy đủ cho vận hành onboard:

- `VoyageRecord` mở rộng với lifecycle, planning summary, vessel snapshot, financial summary.
- `PortCall`, `VoyageCrewAssignment`, `VoyageLogEntry`, `CargoOperation`.
- `VoyagePlanLeg`, `VoyageStatusHistory`, `VoyageCargoPlan`, `VoyageBunkerPlan`, `VoyageCrewChangePlan`.
- Hệ báo cáo thực tế gồm `NoonReport`, `DepartureReport`, `ArrivalReport`, `BunkerReport`, `PositionReport`.
- Nhóm tài chính voyage gồm estimate, expense request, advance, disbursement, actual revenue, settlement.
- Cơ chế sync nền đã có queue, background worker, idempotency, retry và network-aware priority.

### 2.2. Những gì Shore đang có

Shore hiện đã có hạ tầng sync chung và đã nhận tốt nhiều nhóm dữ liệu như crew, ship data và report. Tuy nhiên, phạm vi voyage trên Shore còn chưa tương xứng với Edge:

- `SyncInboxService` hiện mới map mạnh nhóm report và crew-related tables.
- `VoyageRecord` trong `SyncModels` đang ở mức rút gọn, chưa phản ánh đầy đủ model voyage hiện có ở Edge.
- Shore có `Vessel`, `PortCall` và một số màn hình vessel-centric, nhưng chưa có voyage aggregate đồng bộ đầy đủ theo đúng cấu trúc vận hành ở tàu.
- Chưa thấy mapping inbox cho các bảng voyage cốt lõi như `voyage_records`, `voyage_plan_legs`, `voyage_crew_assignments`, `voyage_cost_estimates`, `voyage_expense_requests`, `voyage_disbursements`.

### 2.3. Kết luận hiện trạng

Khoảng trống hiện nay không nằm ở việc thiếu sync framework, mà nằm ở chỗ domain voyage chưa được đưa vào contract sync và model Shore một cách trọn vẹn. Vì vậy, nếu muốn Shore có đầy đủ hơn tàu, cần làm theo hướng mở rộng domain model Shore và cắm domain voyage vào pipeline sync hiện có, thay vì viết một cơ chế sync mới riêng.

---

## 3. Mô hình nghiệp vụ mục tiêu giữa tàu và bờ

### 3.1. Vai trò của Edge

Edge là nơi ghi nhận sự thật vận hành onboard:

- Voyage nào đang diễn ra.
- Tàu đang ở đâu, vào cảng nào, rời cảng nào.
- Crew nào đang gắn với voyage.
- Báo cáo thực tế, logbook, position, bunker, arrival/departure.
- Phát sinh thực tế về cargo operation, route execution, tiêu hao nhiên liệu.
- Chi phí hoặc yêu cầu thanh toán phát sinh trong chuyến đi nếu onboard có nhập.

### 3.2. Vai trò của Shore

Shore là nơi điều hành, giám sát và làm giàu dữ liệu voyage:

- Nhận bản sao đầy đủ dữ liệu factual từ tàu.
- Hợp nhất dữ liệu của nhiều tàu để tạo fleet view.
- Bổ sung planning, phê duyệt, estimate, commercial context, dashboard, cảnh báo và phân tích.
- Đối chiếu plan vs actual giữa lịch trình, nhiên liệu, port call, cargo và chi phí.
- Quản lý ngoại lệ, conflict, missing sync, delayed sync, duplicate sync.
- Trở thành nguồn báo cáo quản trị cho ban điều hành.

### 3.3. Quy tắc ownership dữ liệu

Đây là nguyên tắc quan trọng nhất để tránh sync sai:

| Nhóm dữ liệu | Chủ sở hữu chính | Hướng đồng bộ chính | Ghi chú |
|---|---|---|---|
| Voyage thực tế đang chạy | Edge | Edge -> Shore | Shore mirror đầy đủ |
| Port call thực tế | Edge | Edge -> Shore | Shore không được ghi đè thời gian factual nếu không có cơ chế override rõ ràng |
| Crew assignment thực tế onboard | Edge | Edge -> Shore | Shore dùng để kiểm soát, không được làm sai thực trạng |
| Noon/arrival/departure/bunker/position reports | Edge | Edge -> Shore | Dữ liệu factual và auditable |
| Voyage planning, estimates, budget, commercial metadata | Shore hoặc Hybrid | Shore -> Edge hoặc hai chiều | Tùy phân hệ cụ thể |
| Cost control, settlement, profitability | Shore | Shore là chính, có nhận actual từ Edge | Shore có thể nhiều hơn Edge |
| Dashboard, analytics, alerting, compliance cross-check | Shore | Không bắt buộc sync ngược | Thuần lớp làm giàu dữ liệu |

### 3.4. Quy tắc “bờ có nhiều hơn tàu”

Shore được phép có thêm các lớp dữ liệu sau mà không phá nguyên tắc source of truth:

- Mapping voyage với vessel, fleet, charter party, customer, operator.
- Các cột tổng hợp plan/actual, KPI, exception status, sync health status.
- Approval workflow cho estimate, disbursement, settlement.
- Dashboard tổng hợp multi-vessel và cảnh báo deviation.
- Audit, version lineage, reconciliation status.
- Dữ liệu BI hoặc materialized view phục vụ báo cáo.

---

## 4. Phạm vi dữ liệu cần đồng bộ trong giai đoạn voyage sync

### 4.1. Bắt buộc đồng bộ từ Edge về Shore

Pha đầu phải bao phủ tối thiểu các bảng voyage vận hành cốt lõi:

- `voyage_records`
- `voyage_plan_legs`
- `voyage_status_histories`
- `ports`
- `port_calls`
- `voyage_crew_assignments`
- `voyage_log_entries`
- `cargo_operations`
- `maritime_reports`
- `noon_report`
- `departure_report`
- `arrival_report`
- `bunker_report`
- `position_report`

### 4.2. Đồng bộ mở rộng sau khi vận hành ổn định

- `voyage_cargo_plans`
- `voyage_bunker_plans`
- `voyage_crew_change_plans`
- `voyage_cost_estimates`
- `voyage_revenue_estimates`
- `voyage_expense_requests`
- `voyage_advance_payments`
- `voyage_disbursements`
- `voyage_actual_revenues`
- `voyage_settlements`

### 4.3. Dữ liệu Shore bổ sung thêm

Shore nên có thêm các bảng hoặc view phục vụ quản trị:

- `voyage_sync_states`
- `voyage_reconciliation_results`
- `voyage_kpi_snapshots`
- `voyage_alerts`
- `voyage_profitability_views`
- `voyage_timeline_views`

---

## 5. Yêu cầu phi chức năng

- Hỗ trợ mạng chập chờn, băng thông thấp, retry nhiều lần.
- Idempotent theo `table + recordKey + syncVersion`.
- Có khả năng resume theo cursor hoặc watermark.
- Phân priority để Iridium chỉ đẩy dữ liệu quan trọng, WiFi/4G mới đẩy full payload.
- Có conflict policy rõ ràng theo entity, không dùng một rule chung cho mọi bảng.
- Có khả năng backfill snapshot cho tàu mới hoặc dữ liệu lịch sử.
- Có dashboard theo dõi node sync, độ trễ sync, batch lỗi, backlog queue.

---

## 6. Kế hoạch triển khai theo phase

## Phase 0. Chốt nghiệp vụ và contract dữ liệu

### Mục tiêu

Chốt phạm vi domain voyage cần sync, định nghĩa ownership, conflict rule và thứ tự ưu tiên triển khai.

### Công việc chính

- Liệt kê toàn bộ voyage entities hiện có ở Edge và phân loại theo factual, planning, financial, analytics.
- Chốt entity nào Shore phải mirror 1:1, entity nào Shore được enrich thêm.
- Chốt naming contract giữa edge snake_case, JSON payload và model Shore.
- Chốt sync priority theo network cho từng nhóm entity voyage.
- Chốt chính sách delete, soft delete, archive và snapshot.

### Kết quả bàn giao

- Data contract matrix cho voyage sync.
- Ownership matrix Edge/Shore.
- Conflict resolution matrix theo entity.
- Danh sách bảng vào scope phase 1 và phase 2.

### Tiêu chí nghiệm thu

- Tất cả team backend, frontend, QA cùng dùng chung một danh sách entity voyage sync.
- Không còn tranh cãi Shore sửa được gì và Edge giữ quyền ở đâu.

## Phase 1. Chuẩn hóa domain model Shore cho voyage mirror

### Mục tiêu

Làm Shore đủ khả năng lưu đầy đủ dữ liệu voyage đang có ở Edge.

### Công việc chính

- Tạo hoặc mở rộng model Shore cho `VoyageRecord` tương thích với Edge Phase 1.
- Bổ sung các bảng Shore còn thiếu: `VoyagePlanLeg`, `VoyageStatusHistory`, `VoyageCrewAssignment`, `VoyageLogEntry`, `CargoOperation` và các quan hệ liên quan.
- Chuẩn hóa foreign key, index, origin node, sync version, updated at.
- Thiết kế migration để không phá dữ liệu Shore đang có.
- Rà soát reuse model nào thuộc `MaritimeModels`, model nào cần tách riêng bounded context voyage.

### Kết quả bàn giao

- Shore schema mirror được tập core voyage từ Edge.
- Migrations chạy sạch trên môi trường dev/test.
- Seed hoặc backfill mapping vessel-port cơ bản nếu cần.

### Tiêu chí nghiệm thu

- Shore có thể lưu đầy đủ payload thực tế từ Edge mà không mất cột quan trọng.
- Quan hệ voyage -> legs -> port calls -> crew assignments -> reports truy vấn được ổn định.

## Phase 2. Cắm voyage vào pipeline sync hiện tại

### Mục tiêu

Đưa toàn bộ nhóm voyage core vào inbox/outbox hiện có, không tạo cơ chế riêng lẻ.

### Công việc chính

- Mở rộng `SyncInboxService` để nhận các bảng voyage core từ Edge.
- Nếu cần, mở rộng `SyncService` phía Edge để queue đúng các bảng voyage chưa được enqueue.
- Chuẩn hóa deserialization, UTC conversion, payload normalization cho các entity voyage.
- Cấu hình idempotency và dedup cho nhóm voyage.
- Bổ sung sync log chi tiết cho voyage batches.

### Kết quả bàn giao

- Edge tạo queue được cho voyage core entities.
- Shore nhận và upsert được voyage core entities.
- Có log lỗi đủ sâu để truy vết batch và record.

### Tiêu chí nghiệm thu

- Tạo mới hoặc sửa một voyage trên Edge thì Shore nhận đúng trong chu kỳ sync.
- Tạo port call, crew assignment, log entry và report trên Edge thì Shore có đầy đủ bản ghi liên quan.
- Sync lặp lại cùng version không tạo duplicate.

## Phase 3. Hoàn thiện ownership và conflict resolution cho voyage

### Mục tiêu

Đảm bảo dữ liệu factual không bị ghi đè sai, đồng thời Shore vẫn có thể enrich phần dữ liệu của mình.

### Công việc chính

- Viết conflict policy riêng cho `VoyageRecord` và từng bảng con.
- Tách field factual, field planning, field financial, field computed.
- Quy định field nào Edge thắng tuyệt đối, field nào Shore thắng, field nào so theo timestamp.
- Thiết kế cơ chế marker cho manual override ở Shore nếu nghiệp vụ cần.
- Bổ sung audit trail khi có conflict hoặc override.

### Kết quả bàn giao

- Conflict resolver matrix cho voyage.
- Audit log cho conflict và manual reconciliation.

### Tiêu chí nghiệm thu

- Port call factual từ tàu không bị Shore vô tình ghi đè.
- Estimate hoặc enrich field từ Shore không bị Edge delta cũ ghi mất.

## Phase 4. Snapshot, backfill và đồng bộ ban đầu

### Mục tiêu

Cho phép một tàu hoặc một Shore node mới lấy đủ lịch sử voyage mà không cần đợi delta dần dần.

### Công việc chính

- Thiết kế full snapshot cho voyage core theo tàu và theo khoảng thời gian.
- Hỗ trợ backfill lịch sử từ Edge sang Shore cho voyages đang mở và voyages gần đây.
- Bổ sung cursor hoặc watermark riêng cho snapshot batches lớn.
- Kiểm soát chunk size theo loại mạng.

### Kết quả bàn giao

- API hoặc job snapshot/backfill cho voyage.
- Quy trình bootstrap khi onboard tàu mới.

### Tiêu chí nghiệm thu

- Một Shore môi trường mới có thể nhận lại lịch sử voyage gần đây từ Edge.
- Snapshot chạy lặp lại không tạo duplicate và không làm hỏng delta pipeline.

## Phase 5. Shore enrich: điều hành, đối soát và dashboard

### Mục tiêu

Biến Shore từ nơi chỉ nhận dữ liệu thành nơi quản trị voyage cấp đội tàu.

### Công việc chính

- Tạo voyage aggregate view theo tàu, theo fleet, theo trạng thái hành trình.
- Xây dashboard plan vs actual: route, ETA, fuel, cargo, port call, crew, reports.
- Tạo cảnh báo cho missing report, delayed sync, route deviation, fuel deviation.
- Tạo timeline voyage hợp nhất từ log, report, port call, status history.
- Tạo reconciliation status cho từng voyage.

### Kết quả bàn giao

- Shore voyage overview page.
- Vessel/voyage detail page với timeline và sync health.
- Các view tổng hợp phục vụ vận hành.

### Tiêu chí nghiệm thu

- Người dùng Shore nhìn được cùng dữ liệu như tàu và thêm lớp phân tích cần thiết.
- Có thể phát hiện chuyến đi nào mất đồng bộ hoặc sai lệch plan/actual.

## Phase 6. Mở rộng sang planning và financial sync hai chiều

### Mục tiêu

Đưa các thực thể planning và financial vào cơ chế hybrid sync mà vẫn giữ rõ ownership.

### Công việc chính

- Đồng bộ `voyage_cargo_plans`, `voyage_bunker_plans`, `voyage_crew_change_plans`.
- Đồng bộ estimate và actual financial entities theo rule riêng.
- Cho phép Shore tạo hoặc duyệt dữ liệu planning/financial rồi sync ngược xuống Edge nếu cần hiển thị onboard.
- Bổ sung trạng thái approval, lock và reopen nếu nghiệp vụ yêu cầu.

### Kết quả bàn giao

- Planning và financial flows giữa tàu-bờ hoạt động thống nhất.
- Shore trở thành hệ điều phối cho cost control và profitability.

### Tiêu chí nghiệm thu

- Tàu nhận được planning hoặc approval state cần thiết từ bờ.
- Shore vẫn giữ được actual phát sinh từ tàu để đối soát.

## Phase 7. Hardening, monitoring và rollout production

### Mục tiêu

Đảm bảo solution chạy ổn định ngoài thực địa với nhiều tàu và nhiều kiểu mạng.

### Công việc chính

- Load test cho batch sync voyage.
- Kiểm thử duplicate, out-of-order, partial failure, retry exhaustion.
- Thêm dashboard vận hành sync theo node, backlog, failure reason, conflict counts.
- Viết SOP xử lý lỗi sync cho đội triển khai và support.
- Triển khai pilot 1-2 tàu trước khi rollout fleet-wide.

### Kết quả bàn giao

- Bộ test scenario cho ship-shore sync.
- Monitoring dashboard và cảnh báo production.
- Tài liệu runbook xử lý sự cố.

### Tiêu chí nghiệm thu

- Hệ thống giữ được tính nhất quán khi mất mạng, sync lại, hoặc sync trùng.
- Rollout production có chỉ số theo dõi rõ ràng và quy trình support cụ thể.

---

## 7. Backlog kỹ thuật ưu tiên cao

Các hạng mục nên làm sớm ngay từ đầu vì ảnh hưởng toàn cục:

1. Đồng nhất `VoyageRecord` Shore với Edge thay vì giữ model rút gọn.
2. Thêm mapping inbox cho toàn bộ voyage core tables.
3. Đưa `OriginNode`, `SyncVersion`, `UpdatedAt`, `CreatedAt` vào mọi entity voyage syncable phía Shore.
4. Tạo test contract cho payload snake_case -> C# model.
5. Thiết kế reconciliation job kiểm tra thiếu record theo voyage.
6. Tạo dashboard sync health theo từng tàu.

---

## 8. Rủi ro chính và cách giảm thiểu

### Rủi ro 1. Shore model không theo kịp Edge model

Nếu Shore tiếp tục giữ model voyage rút gọn, payload từ Edge sẽ bị mất nghĩa hoặc phải map thủ công nhiều lớp.

Giảm thiểu: dùng mirror schema cho core voyage trước, enrich bằng bảng/view riêng sau.

### Rủi ro 2. Không chốt ownership từ đầu

Nếu không chốt field nào Edge thắng, field nào Shore thắng, hệ thống sẽ sync đúng kỹ thuật nhưng sai nghiệp vụ.

Giảm thiểu: phải có conflict matrix theo entity trước khi mở sync hai chiều.

### Rủi ro 3. Payload voyage quá lớn khi mạng yếu

Nhóm reports, logs, port calls, position có thể tạo lưu lượng cao.

Giảm thiểu: chia priority, batch size, snapshot riêng, delta riêng, compression nếu cần.

### Rủi ro 4. Thiếu công cụ đối soát

Nếu chỉ có sync mà không có reconciliation, đội vận hành khó biết tàu nào đang thiếu dữ liệu.

Giảm thiểu: phase 5 phải có sync health và reconciliation dashboard.

---

## 9. Định nghĩa hoàn thành mức business

Hệ thống voyage sync ship-shore được coi là đạt mục tiêu khi:

- Mọi voyage factual trên tàu đều xuất hiện ở Shore với đủ context liên quan.
- Shore xem được hành trình, port calls, crew assignments, reports, logs của tàu theo cùng một timeline.
- Shore có thêm lớp planning, monitoring, KPI, financial control và reconciliation mà không làm sai dữ liệu thực tế từ tàu.
- Khi mất mạng và đồng bộ lại, dữ liệu vẫn nhất quán, không mất bản ghi, không nhân đôi bản ghi.

---

## 10. Khuyến nghị thứ tự thực hiện thực tế

Nếu triển khai ngay trong repo hiện tại, nên đi theo thứ tự ngắn nhất để có giá trị sớm:

1. Phase 0
2. Phase 1
3. Phase 2
4. Phase 3
5. Phase 5
6. Phase 4
7. Phase 6
8. Phase 7

Lý do:

- Cần Shore mirror được voyage core trước khi nói tới dashboard hay financial sync.
- Cần chạy được delta sync core sớm để team nghiệp vụ nhìn thấy dữ liệu thật.
- Snapshot/backfill có thể làm sau khi delta pipeline đã ổn định, tránh triển khai quá rộng ngay từ đầu.