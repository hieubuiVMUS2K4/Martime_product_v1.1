# SHORE Crew Management Implementation Blueprint

> ### 📊 TỔNG KẾT TIẾN ĐỘ (Cập nhật: 2026-03-08)
>
> **Tiến độ tổng thể: 7/9 pha hoàn thành (~78%)**
>
> - ✅ **Pha 0–2**: Foundation, Onboarding, Document Workflow — HOÀN THÀNH
> - ❌ **Pha 3**: Crew Portal/PWA — CHƯA BẮT ĐẦU
> - ✅ **Pha 4–7**: Compliance, Assignment, External/Travel, Onboard — HOÀN THÀNH
> - ❌ **Pha 8**: Hardening & Rollout — CHƯA BẮT ĐẦU
>
> **Backend:** 10 services, 8 controllers, 5 migrations, 30+ DbSets — ✅ đầy đủ
> **Frontend Shore:** 20+ pages, 9 hooks, 7 API services, routes đầy đủ — ✅ đầy đủ
> **PWA/Crew Portal:** ❌ Chưa có
> **Items cần chú ý (⚠️):** Object storage/signed URL, Email invitation service, Service record auto-update
>
> → Xem chi tiết tại [Mục 24: Kế hoạch triển khai theo pha](#24-kế-hoạch-triển-khai-theo-pha)

## 1. Mục đích tài liệu

Tài liệu này là blueprint triển khai module quản lý thuyền viên trên Shore cho hệ thống Maritime Product. Mục tiêu là đưa ra một kế hoạch đủ chi tiết để đội nghiệp vụ, backend, frontend, mobile/PWA, QA và triển khai có thể dùng làm chuẩn thống nhất trước khi bắt đầu phát triển.

Tài liệu không chỉ mô tả tính năng mà còn định nghĩa:

- Mô hình vận hành mục tiêu giữa Shore, Edge và Crew Portal/PWA.
- Luồng nghiệp vụ chi tiết từ onboarding đến sign-off.
- Chuẩn dữ liệu, chuẩn trạng thái, chuẩn phân quyền và audit.
- Yêu cầu hiệu năng, bảo mật, khả năng mở rộng và độ tin cậy.
- Lộ trình triển khai theo pha, tiêu chí nghiệm thu và rủi ro chính.

## 2. Phạm vi nghiệp vụ

Module Shore Crew Management cần bao phủ 8 nhóm năng lực chính:

1. Crew Master Data và Crew Identity.
2. Onboarding và checklist khởi tạo hồ sơ.
3. Document Lifecycle và verification workflow.
4. Compliance Matrix và eligibility engine.
5. Planning, assignment, timeline và crew confirmation.
6. External Requests và SIU job calls.
7. Travel Management.
8. Onboard access, sign-on/sign-off và tích hợp Edge.

Kênh tương tác người dùng gồm:

- Shore Backoffice cho HR, Coordinator, Compliance, Travel, Fleet Manager.
- Crew Portal/PWA cho thuyền viên.
- Edge cho Master và vận hành thực tế trên tàu.
- External portal hoặc secure link cho agency/travel agent trong phạm vi giới hạn.

## 3. Nguyên tắc kiến trúc tổng thể

### 3.1 Vai trò của Shore, Edge và PWA

- Shore là hệ thống điều phối trung tâm, source of truth cho crew profile, planning, compliance, travel requests, external requests và policy truy cập.
- Edge là hệ thống vận hành tại tàu, chịu trách nhiệm xác nhận factual events như onboard, sign-on, sign-off, thực trạng crew đang có mặt trên tàu.
- Crew Portal/PWA là kênh self-service chuẩn cho thuyền viên để nhận email mời, kích hoạt tài khoản, upload chứng từ, xác nhận assignment, xem itinerary và theo dõi trạng thái hồ sơ.

### 3.2 Nguyên tắc thiết kế cốt lõi

- Workflow-first, không chỉ CRUD-first.
- Event ownership rõ ràng giữa Shore và Edge.
- Tách aggregate nghiệp vụ thay vì nhồi thêm trạng thái vào bảng hiện có.
- Mọi hành động quan trọng phải có audit trail và status history.
- Compliance phải explainable, có thể mô phỏng, có khả năng waive theo quyền.
- Tất cả tính năng phải thiết kế để chạy được trong môi trường mạng không ổn định.

### 3.3 Bounded contexts

Các bounded context nên được tách rõ trong thiết kế domain và service:

1. Crew Master.
2. Onboarding.
3. Document Workflow.
4. Compliance.
5. Planning & Assignment.
6. External Requests.
7. Travel.
8. Onboard Events.
9. Notification & Communication.
10. Audit & Security.

## 4. Chuẩn quốc tế và quy ước bắt buộc

Module cần bám theo các chuẩn và quy ước quốc tế thường gặp trong vận hành thuyền viên:

- STCW 1978 as amended, đặc biệt cho chứng chỉ chuyên môn, an toàn và competency mapping.
- MLC 2006 cho quản lý hợp đồng, welfare, thời hạn làm việc và lịch sử đi tàu.
- SOLAS và ISM Code cho điều kiện vận hành, onboard control và hồ sơ liên quan an toàn.
- ISPS Code cho quyền truy cập, security-sensitive roles và tracking xác nhận onboard.
- FAL Form 5 cho cấu trúc thông tin crew list, nhất là khi liên kết với voyage và port calls.
- IMO Safe Manning Document cho logic minimum manning, rank assignment và equivalent rank constraints.
- ISO 8601 cho mọi định dạng ngày giờ trong API và database contract.
- ISO 3166-1 alpha-2/alpha-3 cho quốc gia và quốc tịch.
- UN/LOCODE cho cảng và điểm đi/đến trong assignment và travel.
- OWASP ASVS level phù hợp cho portal, token flow, upload file và audit trail.
- Nguyên tắc bảo vệ dữ liệu cá nhân tương đương GDPR/PDPA cho PII, giấy tờ tùy thân và hồ sơ y tế.

## 5. Đánh giá hiện trạng hệ thống

### 5.1 Những gì đã có ở Shore

- Shared models trong Maritime.Shared cho CrewMember, CrewCertificate, Rank, Country, ServiceRecord, document models và sync models.
- Shore API cho CRUD thuyền viên, chứng chỉ, tài liệu và compliance cơ bản.
- Sync infrastructure Shore-Edge với outbox, inbox, idempotency, conflict handling và health monitoring.
- Frontend Shore đã có crew list, crew detail, certificate monitor và một phần vessel-centric views.
- Seed data đã có ranks, countries, certificate types và rank-certificate mapping.

### 5.2 Những gì đã có ở Edge

- Crew detail và certificate handling thực dụng hơn.
- Voyage/crew linking và onboard-centric data tốt hơn.
- Mobile app đã có profile, certificates, some offline reading.
- Một số UI pattern hữu ích có thể tái sử dụng cho PWA và Shore detail flow.

### 5.3 Khoảng trống cần lấp

- Chưa có onboarding checklist auto-generated.
- Chưa có document workflow với versioning, reviewer queue, rejection loop, lock after verification.
- Chưa có compliance matrix đa chiều theo nationality, vessel, vessel group, charterer, flag state.
- Chưa có assignment planning engine đúng nghĩa.
- Chưa có external requests và agency collaboration flow.
- Chưa có travel request auto-generation và itinerary distribution.
- Chưa có PWA self-service cho crew.
- Chưa có governance chuẩn cho onboard access và sign-on/sign-off ownership.

## 6. Tầm nhìn vận hành mục tiêu

Trạng thái mục tiêu sau triển khai:

- Mỗi thuyền viên có một hồ sơ master trên Shore, có lifecycle rõ ràng từ draft đến active, inactive, retired.
- Mỗi chứng từ có requirement source, version history, verification history, expiry policy và audit history.
- Mỗi assignment được tính eligibility theo compliance matrix trước khi confirm.
- Mỗi thay đổi assignment có thể kéo theo travel request tự động.
- Mỗi external shortage có thể tạo request ra agency với tracking trạng thái từ sent đến closed.
- Chỉ crew đã onboard hợp lệ mới được cấp quyền vào onboard module tương ứng trên Edge.
- Mọi vai trò có SOP và dashboard riêng, không dùng một màn hình crew chung cho tất cả.

## 7. Vai trò và phân quyền

### 7.1 Vai trò nghiệp vụ

- HR Admin.
- Crew Coordinator.
- Compliance Officer.
- Travel Coordinator.
- Fleet Manager.
- Port Captain.
- Master.
- Crew Member.
- External Agency User.
- Travel Agent User.
- System Admin.

### 7.2 Nguyên tắc phân quyền

- Read, write, verify, approve, waive, confirm, onboard, sign-on, sign-off là các action phân biệt.
- Document verification không mặc định đồng nghĩa với document edit.
- Compliance waiver phải tách quyền riêng, không gộp chung với coordinator edit.
- Master có quyền xác nhận onboard và sign-on/sign-off trong phạm vi tàu được giao.
- External users chỉ thao tác được trên request được cấp quyền, không truy cập hệ thống nội bộ.
- Crew chỉ thấy hồ sơ, assignment, document requirements và itinerary của chính mình.

### 7.3 Nguyên tắc RACI cấp cao

- HR/Crew Coordinator chịu trách nhiệm tạo hồ sơ, chuẩn hóa dữ liệu đầu vào và khởi tạo onboarding.
- Compliance Officer chịu trách nhiệm verify tài liệu, cấu hình matrix và quyết định blocker hoặc waiver.
- Travel Coordinator chịu trách nhiệm xử lý travel request và itinerary.
- Fleet Manager/Port Captain chịu trách nhiệm duyệt logic assignment và shortage handling.
- Master chịu trách nhiệm xác nhận onboard factual events.
- Crew Member chịu trách nhiệm hoàn thiện hồ sơ, upload chứng từ và confirm assignment khi được yêu cầu.

## 8. Mô hình dữ liệu mục tiêu

### 8.1 Nguyên tắc mô hình dữ liệu

- Không phá compatibility với shared models hiện có nếu chưa bắt buộc.
- Dùng aggregate mới cho workflow thay vì cố gắng ép toàn bộ logic vào CrewMember và các bảng document hiện tại.
- Mỗi entity workflow đều cần `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `Status`, `StatusChangedAt` nếu phù hợp.
- Mỗi transition quan trọng cần một bảng history riêng.
- Tất cả thời gian lưu UTC, render theo timezone UI.
- Tất cả file attachment lưu ngoài DB, chỉ lưu metadata trong DB.

### 8.2 Nhóm entity đề xuất

#### Crew Master

- `crew_profiles`
- `crew_identity_accounts`
- `crew_equivalent_ranks`
- `crew_contacts`
- `crew_emergency_contacts`
- `crew_status_history`

#### Onboarding

- `onboarding_cases`
- `onboarding_checklist_items`
- `onboarding_case_events`
- `onboarding_case_comments`

#### Document Workflow

- `document_requirement_sets`
- `document_requirements`
- `crew_document_submissions`
- `crew_document_versions`
- `document_verification_tasks`
- `document_verification_actions`
- `document_comments`
- `document_attachments`

#### Compliance

- `compliance_rule_sets`
- `compliance_rules`
- `compliance_rule_dimensions`
- `compliance_rule_conditions`
- `compliance_waivers`
- `compliance_evaluations`
- `compliance_evaluation_items`

#### Planning & Assignment

- `vessel_manning_standards`
- `vessel_manning_positions`
- `crew_assignments`
- `assignment_candidates`
- `assignment_confirmations`
- `assignment_threads`
- `assignment_conflicts`
- `equivalent_rank_policies`
- `crew_availability_windows`

#### External Requests

- `external_agencies`
- `external_requests`
- `external_request_items`
- `external_candidates`
- `external_request_messages`
- `external_request_attachments`
- `external_request_status_history`

#### Travel

- `travel_requests`
- `travel_request_segments`
- `travel_itineraries`
- `travel_attachments`
- `travel_status_history`
- `travel_vendors`

#### Onboard Events

- `onboard_events`
- `sign_on_events`
- `sign_off_events`
- `crew_access_grants`
- `crew_service_record_updates`

#### Cross-cutting

- `notification_messages`
- `notification_deliveries`
- `one_time_tokens`
- `audit_logs`
- `integration_outbox`

### 8.3 Chuẩn khóa và chỉ mục

- Dùng UUID hoặc GUID cho entity nghiệp vụ chính.
- Các bảng timeline phải có index trên `CrewId`, `VesselId`, `Status`, `StartDate`, `EndDate`.
- Các bảng document phải có composite index trên `CrewId`, `DocumentType`, `IsActiveVersion`.
- Compliance rule tables phải có index theo dimensions thường lọc như `RankId`, `NationalityCode`, `VesselGroupId`, `FlagState`.
- Các queue như verification, notification, sync outbox cần index theo `Status`, `Priority`, `NextRetryAt`.

## 9. Crew Master và Onboarding

### 9.1 Mục tiêu

Cho phép tạo hồ sơ crew chuẩn hóa, sinh checklist tự động và kích hoạt self-service ngay sau khi tạo.

### 9.2 Dữ liệu bắt buộc giai đoạn đầu

#### Thông tin cá nhân

- Họ tên đầy đủ theo hộ chiếu.
- First name, middle name, last name nếu cần cho chứng từ quốc tế.
- Ngày sinh.
- Quốc tịch.
- Giới tính nếu cần cho visa/travel.
- Email.
- Số điện thoại.
- Địa chỉ hiện tại.

#### Thông tin nghề nghiệp

- Crew code hoặc employee code.
- Rank chính.
- Equivalent ranks.
- Department.
- Tình trạng pool.
- Contract duration hoặc expected contract end.
- Last vessel, last sign-off, service summary nếu có.

#### Tham chiếu planning/compliance

- Reference vessel.
- Vessel group.
- Vessel type.
- Flag state nếu có.
- Join port hoặc region dự kiến.

### 9.3 Luồng nghiệp vụ chuẩn

1. Coordinator tạo hồ sơ với bộ trường tối thiểu.
2. Hệ thống kiểm tra trùng theo email, số passport nếu đã có, crew code và họ tên + ngày sinh.
3. Coordinator chọn reference vessel hoặc vessel group.
4. Hệ thống gọi Compliance Matrix Service để xác định requirement set áp dụng.
5. Hệ thống tạo `OnboardingCase` và danh sách `OnboardingChecklistItem`.
6. Hệ thống tạo crew portal account ở trạng thái `PendingActivation`.
7. Hệ thống phát hành token onboarding một lần, có expiry ngắn.
8. Hệ thống gửi email onboarding gồm activation link, hướng dẫn cài PWA và danh sách hành động cần hoàn thành.
9. Crew đăng nhập lần đầu, đổi mật khẩu, xác nhận contact info, cập nhật emergency contact và upload tài liệu.
10. Coordinator và Compliance Officer review checklist đến khi case đạt `ReadyForReview` hoặc `ReadyForActivation`.
11. Khi đủ điều kiện, hồ sơ chuyển sang `Active`.

### 9.4 Trạng thái của OnboardingCase

- `Draft`
- `Invited`
- `InProgress`
- `PendingReview`
- `ReturnedForCompletion`
- `Approved`
- `Activated`
- `Cancelled`

### 9.5 Quy tắc bắt buộc

- Checklist item phải lưu `SourceRuleId` để truy vết vì sao requirement xuất hiện.
- Không được activate crew account nếu thiếu tối thiểu các dữ liệu nhận dạng và contact bắt buộc.
- Hồ sơ có thể được tạo trước khi đủ toàn bộ chứng từ, nhưng không được chuyển sang eligible for assignment nếu còn blocker.

## 10. Document Lifecycle và Verification

### 10.1 Mục tiêu

Biến quản lý giấy tờ từ lưu file đơn thuần thành workflow kiểm soát version, reviewer, trạng thái, renewal và audit.

### 10.2 Nhóm tài liệu tối thiểu

- Passport.
- Seaman book.
- National CoC.
- Flag endorsement.
- Medical fitness certificate.
- Drug and alcohol test nếu policy yêu cầu.
- Yellow fever hoặc vaccination documents theo tuyến hành trình/quốc gia.
- Visa documents.
- Employment contract.
- Appraisal hoặc competency assessment nếu policy yêu cầu.

### 10.3 Trạng thái tài liệu đề xuất

- `Draft`
- `Submitted`
- `SentForVerification`
- `UnderReview`
- `Verified`
- `Rejected`
- `Expired`
- `Archived`
- `Superseded`

### 10.4 Luồng upload và xác minh

1. Crew hoặc coordinator tạo `CrewDocumentSubmission`.
2. Người dùng upload ảnh hoặc PDF.
3. Hệ thống kiểm tra mime type, kích thước, virus scan, metadata tối thiểu.
4. Nếu là ảnh, hệ thống nén và convert sang PDF chuẩn nếu policy bật.
5. Hệ thống tạo `CrewDocumentVersion` đầu tiên.
6. Submission chuyển sang `Submitted`.
7. Coordinator hoặc automation gửi sang verification queue.
8. Verification task được gán cho Compliance Officer theo SLA hoặc work queue.
9. Reviewer có thể verify, reject, request re-upload hoặc add comment.
10. Khi verified, version được lock, submission đánh dấu `Verified`, attachment checksum được cố định.
11. Khi renew, hệ thống tạo version mới, bản cũ chuyển `Superseded` hoặc `Archived`.

### 10.5 Quy tắc dữ liệu quan trọng

- Chỉ một version active được dùng cho compliance tại một thời điểm.
- Version verified không được sửa file gốc hoặc metadata cốt lõi.
- Rejection phải có reason code và free-text note.
- Mỗi document phải có `IssueDate`, `ExpiryDate`, `Issuer`, `DocumentNumber` nếu applicable.
- Các trường y tế phải được gắn sensitivity level cao hơn PII thông thường.

### 10.6 SLA đề xuất

- Submission mới phải xuất hiện ở reviewer queue dưới 1 phút.
- Reviewer SLA mặc định: 24 giờ làm việc cho tài liệu onboarding bình thường, 4 giờ cho urgent assignment.
- Reminder tự động sau 50% và 90% SLA.

### 10.7 Yêu cầu hiệu năng và trải nghiệm upload

- Hỗ trợ upload file tối thiểu 25 MB mỗi file cho PDF và ảnh chất lượng cao.
- Có progress bar, retry và resume ở mức cơ bản.
- Ảnh từ camera phải được nén client-side trước khi gửi để giảm băng thông.
- Các ảnh preview phải sinh thumbnail bất đồng bộ.
- Tài liệu lớn phải phục vụ qua signed URL hoặc streaming endpoint thay vì tải toàn bộ qua payload JSON.

## 11. Crew Portal/PWA

### 11.1 Mục tiêu

Đưa cho thuyền viên một kênh self-service nhẹ, nhanh, cài được trên điện thoại, chịu được mạng yếu và đủ khả năng hoàn tất onboarding mà không cần native app ngay từ đầu.

### 11.2 Luồng activation

1. Crew nhận email mời.
2. Email có activation link một lần và hướng dẫn cài PWA.
3. Crew mở link trên điện thoại.
4. Hệ thống xác nhận token còn hiệu lực.
5. Crew đặt mật khẩu, xác thực contact info, chấp nhận điều khoản nếu cần.
6. Giao diện gợi ý cài PWA.
7. Sau khi cài, crew tiếp tục checklist, upload chứng từ, xem assignment và itinerary.

### 11.3 Tính năng tối thiểu pha đầu

- Activate account.
- Đăng nhập và refresh session an toàn.
- Xem và cập nhật emergency contact.
- Xem onboarding checklist.
- Upload document bằng camera hoặc file picker.
- Xem trạng thái verification và comment.
- Nhận assignment confirmation request.
- Confirm, decline hoặc ask question.
- Xem travel itinerary và file đính kèm.
- Xem notification center.

### 11.4 Yêu cầu kỹ thuật PWA

- Installable trên Android và iOS qua browser.
- Service worker cho static assets, shell caching và limited offline mode.
- Upload queue chịu được mất mạng tạm thời.
- Không cache tài liệu nhạy cảm quá mức trên thiết bị dùng chung.
- Logout phải xóa dữ liệu nhạy cảm ở local storage/cache theo policy.

### 11.5 Yêu cầu UX trong điều kiện mạng yếu

- First meaningful render dưới 3 giây trên mạng 4G bình thường.
- Form submit phải có optimistic feedback hoặc queued state.
- Upload phải hiển thị trạng thái rõ ràng `Queued`, `Uploading`, `Processing`, `Submitted`, `Failed`.
- Mọi lỗi phải diễn đạt rõ hành động tiếp theo cho crew.

## 12. Compliance Matrix và Eligibility Engine

### 12.1 Mục tiêu

Tính toán chính xác crew có đủ điều kiện cho assignment hay không, trên cơ sở rule đa chiều và có thể giải thích được.

### 12.2 Dimensions bắt buộc

- Rank.
- Equivalent rank.
- Nationality.
- Country of issue.
- Vessel.
- Vessel group.
- Vessel type.
- Flag state.
- Charterer hoặc customer profile.
- Assignment type.
- Join country hoặc operating area nếu cần.

### 12.3 Rule semantics

Mỗi rule cần mô tả được:

- Requirement type.
- Document/certificate type cần có.
- Severity: `Blocker`, `Warning`, `Info`.
- Evaluation stage: `Onboarding`, `PreConfirm`, `PreTravel`, `PreOnboard`, `PeriodicReview`.
- Expiry window.
- Renew window.
- Grace period nếu có.
- Equivalent satisfaction logic.
- Waiver allowed hay không.
- Waiver approver role.

### 12.4 Kết quả evaluation bắt buộc

- `Eligible`
- `EligibleWithWarnings`
- `NotEligible`
- `EligibleByWaiver`

Mỗi evaluation item phải trả về:

- Requirement source.
- Matched document/version.
- Days until expiry.
- Severity.
- Message hiển thị cho UI.
- Explainability text.

### 12.5 Hai chế độ sử dụng

- Real-time check khi tạo hoặc cập nhật assignment.
- Simulation mode để coordinator thử crew khác, tàu khác hoặc ngày join khác.

### 12.6 Hiệu năng evaluation

- Evaluation đơn lẻ dưới 1 giây cho hồ sơ crew đã có projection data.
- Bulk evaluation cho 100 crew dưới 10 giây với cache projection hợp lý.
- Compliance snapshots được materialize để tránh join quá nặng trên UI list.

## 13. Planning, Assignment và Timeline

### 13.1 Mục tiêu

Cho phép quản lý crew planning ở mức đội tàu, phát hiện shortage, overlap, gap, equivalent rank fit và điều phối xác nhận assignment.

### 13.2 Thực thể cốt lõi

- Manning Standard.
- Manning Position Requirement.
- Assignment.
- Assignment Candidate.
- Assignment Confirmation.
- Assignment Conflict.
- Crew Availability Window.
- Assignment Thread.

### 13.3 Luồng chuẩn tạo assignment

1. Fleet hoặc coordinator cấu hình standard manning theo vessel hoặc vessel group.
2. Hệ thống tạo planning slots theo crew change dates, template hoặc demand event.
3. Coordinator mở slot và tìm ứng viên.
4. System tính compliance, overlap, gap, rest, travel feasibility và equivalent rank fit.
5. Nếu không có crew nội bộ phù hợp, system gợi ý tạo external request.
6. Khi coordinator chọn được crew, assignment ở trạng thái `Proposed`.
7. Coordinator gửi confirm cho crew hoặc confirm thay theo quyền.
8. Nếu crew đồng ý, assignment sang `Confirmed`.
9. Hệ thống auto-generate travel request nếu policy yêu cầu.
10. Khi master xác nhận onboard, assignment sang `OnBoarded`.
11. Khi sign-off, assignment sang `Completed` và service record cập nhật.

### 13.4 Trạng thái assignment đề xuất

- `Draft`
- `Proposed`
- `PendingCrewConfirmation`
- `Confirmed`
- `TravelInProgress`
- `ReadyToJoin`
- `OnBoarded`
- `Completed`
- `Cancelled`
- `Declined`

### 13.5 Conflict detection bắt buộc

- Date overlap với assignment khác.
- Gap quá ngắn hoặc quá dài so với policy.
- Crew unavailable do medical, contract, leave hoặc hold status.
- Rank mismatch.
- Equivalent rank mismatch theo từng vessel policy.
- Missing compliance blockers.
- Travel infeasibility do lead time không đủ.

### 13.6 Equivalent rank policy

- Không coi equivalent rank là mặc định toàn cục.
- Mỗi vessel hoặc vessel group có policy riêng.
- Equivalent rank có thể cần waiver hoặc approval cấp cao.
- Mỗi lần sử dụng equivalent rank phải được log cho audit và reporting.

### 13.7 Communication trong assignment

- Mỗi assignment có comment thread riêng giữa coordinator, crew và người review liên quan.
- Hệ thống lưu outbound/inbound message timestamps.
- Crew có thể xác nhận, từ chối hoặc đặt câu hỏi qua portal.

## 14. External Requests và SIU Job Calls

### 14.1 Mục tiêu

Chuẩn hóa việc gửi nhu cầu nhân sự ra agency ngoài khi pool nội bộ không đáp ứng, đảm bảo tracking đầy đủ và khả năng liên kết kết quả với assignment.

### 14.2 Luồng chuẩn

1. Coordinator tạo external request từ một assignment shortage hoặc trực tiếp từ planning board.
2. Request có role requirement, dates, vessel, nationality preferences, mandatory documents, response SLA.
3. Hệ thống gửi email template có secure link hoặc account portal giới hạn.
4. Agency mở request, xác nhận đã xem, gửi candidate profile và documents.
5. Compliance engine chạy pre-screening.
6. Coordinator shortlists candidate.
7. Candidate được link vào assignment candidate pool.
8. Khi request hết nhu cầu hoặc slot đã fill, request được đóng.

### 14.3 Trạng thái request

- `Draft`
- `Sent`
- `Viewed`
- `InProgress`
- `CandidateSubmitted`
- `Shortlisted`
- `Closed`
- `Cancelled`

### 14.4 Yêu cầu bảo mật

- Secure link phải có expiry, signature và scope theo request.
- Agency chỉ thấy dữ liệu tối thiểu cần thiết.
- Không cho phép tải xuống dữ liệu nội bộ ngoài phạm vi request.
- Mọi file agency upload phải đi qua cùng pipeline kiểm tra file như crew upload.

## 15. Travel Management

### 15.1 Mục tiêu

Tự động sinh travel request từ assignment, giảm thao tác thủ công và đảm bảo crew nhận đúng itinerary đúng thời điểm.

### 15.2 Triggers sinh travel request

- Assignment chuyển sang `Confirmed`.
- Join date thay đổi đáng kể.
- Sign-off date thay đổi và cần repatriation.
- Vessel hoặc join port thay đổi.

### 15.3 Luồng chuẩn

1. Assignment confirmed.
2. Hệ thống tính travel lead time theo route policy.
3. Sinh `TravelRequest` với thông tin crew, route, port, reporting date, special constraints.
4. Travel coordinator review và gửi cho travel agent.
5. Travel agent phản hồi itinerary.
6. Itinerary được upload hoặc parse vào system.
7. Crew nhận notification trên portal/PWA.
8. Nếu assignment thay đổi, request được re-evaluate hoặc reissued.

### 15.4 Dữ liệu tối thiểu

- Crew.
- Assignment.
- From/to ports.
- Reporting date/time.
- Flight segments.
- Baggage notes nếu cần.
- Hotel/transfer nếu có.
- Visa/travel document blockers.

### 15.5 Yêu cầu hiệu năng và vận hành

- Travel queue phải hỗ trợ batch processing.
- Itinerary attachment phải có preview nhanh, tải lazy và mobile-friendly.
- Notification gửi cho crew trong vòng 1 phút sau khi itinerary được publish.

## 16. Onboard Module và Sign-on/Sign-off

### 16.1 Mục tiêu

Khóa đúng quyền truy cập onboard, đồng thời chuẩn hóa luồng factual event giữa Shore và Edge.

### 16.2 Nguyên tắc ownership

- Shore sở hữu policy, assignment state, compliance state và access intent.
- Edge sở hữu onboard fact, sign-on fact, sign-off fact tại tàu.
- Shore không được overwrite factual onboard event nếu event đã được master xác nhận trên Edge, trừ khi có quy trình correction riêng.

### 16.3 Luồng chuẩn

1. Assignment ở trạng thái `Confirmed` hoặc `ReadyToJoin`.
2. Crew đến tàu.
3. Master hoặc delegated officer trên Edge xác nhận onboard.
4. Edge phát `OnboardEvent` về Shore qua sync.
5. Shore cấp hoặc cập nhật `CrewAccessGrant` cho onboard module tương ứng.
6. Khi chính thức nhận nhiệm vụ, master thực hiện sign-on.
7. Khi rời tàu, master thực hiện sign-off.
8. Edge sync sign-off event về Shore.
9. Shore cập nhật service record, availability window và assignment completion.

### 16.4 Trạng thái access gợi ý

- `NotGranted`
- `PendingSync`
- `Granted`
- `Suspended`
- `Revoked`

### 16.5 Yêu cầu nghiệp vụ quan trọng

- Crew chưa onboard không được truy cập onboard workflows hoặc shipboard-only views.
- Master được phép sign on người khác khi được phân quyền và có audit trail.
- Mọi correction sau sign-on/sign-off phải tạo correction record, không sửa silent.

## 17. State Machines chuẩn hóa

### 17.1 Document submission state machine

- `Draft -> Submitted`
- `Submitted -> SentForVerification`
- `SentForVerification -> UnderReview`
- `UnderReview -> Verified`
- `UnderReview -> Rejected`
- `Verified -> Expired`
- `Verified -> Superseded`
- `Superseded -> Archived`

### 17.2 Assignment state machine

- `Draft -> Proposed`
- `Proposed -> PendingCrewConfirmation`
- `PendingCrewConfirmation -> Confirmed`
- `PendingCrewConfirmation -> Declined`
- `Confirmed -> TravelInProgress`
- `TravelInProgress -> ReadyToJoin`
- `ReadyToJoin -> OnBoarded`
- `OnBoarded -> Completed`
- `Draft/Proposed/Confirmed -> Cancelled`

### 17.3 External request state machine

- `Draft -> Sent`
- `Sent -> Viewed`
- `Viewed -> InProgress`
- `InProgress -> CandidateSubmitted`
- `CandidateSubmitted -> Shortlisted`
- `Shortlisted -> Closed`
- `Draft/Sent/InProgress -> Cancelled`

## 18. Kiến trúc ứng dụng và tích hợp

### 18.1 Backend

- ASP.NET Core + EF Core + PostgreSQL tiếp tục là nền tảng chính.
- Mỗi bounded context có service riêng, command/query tách logic nếu domain đủ phức tạp.
- Dùng outbox cho sync, notification, email và integration events.
- Dùng background workers cho expiry scan, queue processing, document conversion, notification retries.

### 18.2 Frontend Shore

- React + TypeScript + Vite.
- Tách route theo context thay vì dồn vào CrewManagement hiện tại.
- Có dashboard riêng cho coordinator, compliance, travel.
- Crew detail phải là workspace trung tâm, gồm profile, checklist, documents, compliance, assignments, travel, audit.

### 18.3 Portal/PWA

- Có thể nằm cùng repo frontend nhưng tách auth realm và route group.
- UI mobile-first, payload nhỏ, ưu tiên low-bandwidth mode.

### 18.4 Shore-Edge sync

- Assignment, onboard events, service record updates, access grants cần sync theo ownership policy.
- Conflict resolution phải dựa trên entity ownership, timestamp, source priority và correction workflow.

## 19. API và service boundary cấp cao

### 19.1 API groups đề xuất

- `/api/crew-profiles`
- `/api/onboarding-cases`
- `/api/document-requirements`
- `/api/document-submissions`
- `/api/document-verification`
- `/api/compliance/rules`
- `/api/compliance/evaluate`
- `/api/assignments`
- `/api/assignment-confirmations`
- `/api/external-requests`
- `/api/travel-requests`
- `/api/onboard-events`
- `/api/crew-portal`
- `/api/notifications`

### 19.2 Services đề xuất

- `CrewProfileService`
- `OnboardingService`
- `DocumentWorkflowService`
- `DocumentVerificationService`
- `ComplianceMatrixService`
- `ComplianceEvaluationService`
- `AssignmentPlanningService`
- `AssignmentConflictService`
- `ExternalRequestService`
- `TravelService`
- `OnboardAccessService`
- `CrewPortalService`
- `NotificationService`
- `AuditService`

## 20. Yêu cầu hiệu năng và khả năng mở rộng

### 20.1 Mục tiêu hiệu năng

- Crew list filter/search trang đầu: dưới 2 giây với dữ liệu 10.000 crew.
- Crew detail load lần đầu: dưới 2 giây cho metadata, tài liệu preview lazy-load.
- Compliance evaluation đơn lẻ: dưới 1 giây.
- Assignment search candidate: dưới 3 giây cho 500 đến 1.000 ứng viên khả dĩ.
- Notification publish: dưới 60 giây từ event đến crew portal.
- Upload ảnh 5 MB trên mạng di động bình thường phải có phản hồi ngay và hoàn tất xử lý trong giới hạn chấp nhận được.

### 20.2 Chiến lược hiệu năng

- Dùng read projection cho crew compliance summary, assignment candidate summary và document status summary.
- Không render toàn bộ timeline nặng cùng lúc; dùng pagination, virtual scrolling, lazy tabs.
- Dùng cache cho reference data như ranks, countries, certificate types, vessel groups.
- Dùng signed URL và object storage cho file lớn.
- Tách đồng bộ conversion/PDF thumbnail khỏi request upload chính nếu cần.
- Tối ưu query bằng composite index và precomputed snapshots.

### 20.3 Mục tiêu độ tin cậy

- Verification queue, notification queue, sync outbox phải hỗ trợ retry có backoff.
- Các thao tác thay đổi trạng thái quan trọng phải idempotent nếu bị gọi lại.
- Các lệnh từ Edge sync phải có idempotency key.

## 21. Bảo mật, riêng tư và audit

### 21.1 Dữ liệu nhạy cảm

- Passport.
- Visa.
- Medical certificate.
- Drug test.
- Personal contact data.
- Emergency contact.
- Travel itinerary.

### 21.2 Yêu cầu bảo mật tối thiểu

- Token onboarding và secure link là one-time use, short-lived, signed và revocable.
- Fine-grained authorization cho verify, waive, confirm, onboard, sign-on, sign-off.
- Tài liệu tải xuống phải qua permission check và signed access.
- Audit log phải ghi actor, action, entity, before/after summary, timestamp, source channel, correlation id.
- Không log raw secrets, full document bytes hoặc token nhạy cảm.

### 21.3 Retention và purge

- Phải có policy retention cho token, session, temp uploads, archived versions.
- Hồ sơ và tài liệu phải có retention theo luật nội bộ và flag per document class.

## 22. Notification và communication

### 22.1 Kênh thông báo

- Email.
- In-app notification trên Shore.
- In-app notification trên PWA.
- Push notification ở pha sau nếu hạ tầng đủ.

### 22.2 Event cần thông báo

- Onboarding invite sent.
- Document rejected.
- Document verified.
- Assignment sent to confirm.
- Assignment confirmed or declined.
- External request viewed or candidate submitted.
- Travel itinerary published.
- Onboard confirmed.
- Sign-off completed.

## 23. KPI và reporting

### 23.1 KPI vận hành

- Tỷ lệ hồ sơ onboarding hoàn tất trong SLA.
- Tỷ lệ document verified trong SLA.
- Tỷ lệ assignment bị chặn do compliance blocker.
- Tỷ lệ overlap/gap được phát hiện trước khi confirm.
- Tỷ lệ travel request được phát hành đúng lead time.
- Tỷ lệ external request response đúng hạn.
- Tỷ lệ sync thành công Shore-Edge cho onboard events.

### 23.2 Dashboard bắt buộc

- Crew onboarding pipeline.
- Verification queue aging.
- Fleet compliance overview.
- Assignment shortage heatmap.
- Travel request status board.
- External request tracking.
- Onboard/sign-on/sign-off event monitor.

## 24. Kế hoạch triển khai theo pha

> **CẬP NHẬT TIẾN ĐỘ — 2026-03-08**
>
> | Pha | Tên | Trạng thái | Backend | Frontend | Ghi chú |
> |-----|-----|-----------|---------|----------|---------|
> | 0 | Business discovery | ✅ HOÀN THÀNH | — | — | State machines, enums, data dictionary đã chốt |
> | 1 | Foundation & data contracts | ✅ HOÀN THÀNH | ✅ | ✅ | 5 migrations, 30+ DbSets, 10 services, 8 controllers |
> | 2 | Onboarding & document workflow | ✅ HOÀN THÀNH | ✅ | ✅ | OnboardingService, DocumentWorkflowService, UI pages |
> | 3 | Crew portal/PWA | ❌ CHƯA BẮT ĐẦU | — | — | Chưa có PWA, chưa có activation flow |
> | 4 | Compliance matrix & simulation | ✅ HOÀN THÀNH | ✅ | ✅ | ComplianceService, 3 UI pages (Dashboard, RuleSets, Evaluation) |
> | 5 | Planning & assignment | ✅ HOÀN THÀNH | ✅ | ✅ | AssignmentService, 4 UI pages (List, Detail, Form, Planning) |
> | 6 | External requests & travel | ✅ HOÀN THÀNH | ✅ | ✅ | ExternalRequestService, TravelService, 4 UI pages |
> | 7 | Onboard events & Edge integration | ✅ HOÀN THÀNH | ✅ | ✅ | OnboardEventService, SignOn/SignOff, AccessGrant |
> | 8 | Hardening, pilot & rollout | ❌ CHƯA BẮT ĐẦU | — | — | Chưa pilot, chưa có SOP, chưa QA regression |

### Pha 0. Business discovery và rule finalization — ✅ HOÀN THÀNH

Thời lượng: 1 đến 2 tuần.

Kết quả đầu ra:

- ✅ Business glossary — Enums.cs định nghĩa đầy đủ tất cả state machine statuses.
- ✅ State machine definitions — 7 state machines hoàn chỉnh (CrewStatus, OnboardingCase, DocumentSubmission, Assignment, ExternalRequest, TravelRequest, AccessGrant).
- ✅ Rule catalog — RuleSeverity, EvaluationStage, ConflictType, SignOffReason đã chốt.
- ✅ Field-level data dictionary — Models đầy đủ với XML docs, MaxLength, và Required annotations.
- ✅ RACI matrix — Tài liệu phần 7 đã định nghĩa, code phản ánh qua role-based actions.

### Pha 1. Foundation và data contracts — ✅ HOÀN THÀNH

Thời lượng: 2 đến 3 tuần.

Công việc:

- ✅ Mở rộng shared models và DTOs — 7 model files + 5 DTO files trong `shared/Models/CrewManagement/` và `shared/DTOs/CrewManagement/`.
- ✅ Thiết kế migrations — 5 migrations:
  - `20260308101426_AddCrewManagementWorkflow` (Onboarding, Documents, Audit)
  - `20260308112815_AddComplianceMatrix` (ComplianceRuleSet, Rule, Dimension, Waiver, Snapshot)
  - `20260308122700_AddPlanningAssignment` (ManningStandard, Position, Assignment, Confirmation, Conflict, Comment, StatusHistory)
  - `20260308130638_AddExternalRequestsAndTravel` (ExternalRequest, Candidate, Message, TravelRequest, Segment, StatusHistory)
  - `20260308134144_AddOnboardEventsAndSignOnOff` (OnboardEvent, CrewAccessGrant, SignOnRecord, SignOffRecord)
- ✅ Dựng audit — AuditLog model + AuditService.
- ⚠️ Chốt object storage strategy và upload pipeline — Chưa có signed URL / object storage. DocumentVersion chỉ lưu FilePath local.

### Pha 2. Crew onboarding và document workflow — ✅ HOÀN THÀNH

Thời lượng: 4 tuần.

Kết quả:

- ✅ Tạo hồ sơ crew — CrewProfileController + CrewStatusService.
- ✅ Auto-generate checklist theo reference vessel hoặc vessel group — OnboardingService.
- ✅ Upload, verification, rejection, renew, versioning — DocumentWorkflowService (CrewDocumentSubmission + CrewDocumentVersion + DocumentVerificationTask + DocumentVerificationAction).
- ⚠️ Email invitation và first-login flow — Chưa có email service, chưa có token generation. Model OnboardingCase có InvitedAt nhưng chưa có thực thi gửi email.

**Frontend:**
- ✅ OnboardingDashboardPage + OnboardingDetailPage.
- ✅ VerificationQueuePage.
- ✅ CrewDetailPage mở rộng với các tab Onboarding, Doc Workflow, Status History, Audit.
- ✅ TypeScript types (crewManagement.types.ts), API service (crewManagement.service.ts), hooks (useCrewManagement.ts).

### Pha 3. Crew portal/PWA — ❌ CHƯA BẮT ĐẦU

Thời lượng: 3 tuần.

Kết quả:

- ❌ Activate account — Chưa triển khai.
- ❌ Checklist view — Chưa có PWA frontend.
- ❌ Document upload mobile-first — Chưa có.
- ❌ Notification center cơ bản — Chưa có.

**Ghi chú:** Toàn bộ pha này chưa bắt đầu. Không có repo PWA, không có auth realm riêng cho crew, không có service worker.

### Pha 4. Compliance matrix và simulation — ✅ HOÀN THÀNH

Thời lượng: 3 tuần.

Kết quả:

- ✅ Rule configuration — ComplianceRuleSet, ComplianceRule, ComplianceDimension models + ComplianceController CRUD.
- ✅ Evaluation engine — ComplianceService + ComplianceSnapshot.
- ✅ Simulation UI — CrewEvaluationPage (frontend).
- ✅ Waiver flow cơ bản — ComplianceWaiver model + API.

**Frontend:**
- ✅ ComplianceDashboardPage.
- ✅ RuleSetsPage.
- ✅ CrewEvaluationPage.
- ✅ TypeScript types (compliance.types.ts), API service (compliance.service.ts), hooks (useCompliance.ts).

### Pha 5. Planning, assignment và confirmation — ✅ HOÀN THÀNH

Thời lượng: 4 tuần.

Kết quả:

- ✅ Manning standard — VesselManningStandard + ManningPosition models.
- ✅ Planning board — PlanningBoardPage (frontend).
- ✅ Conflict detection — AssignmentConflict model + AssignmentService.
- ✅ Crew confirmation — AssignmentConfirmation model + API.
- ✅ Assignment thread — AssignmentComment model + AssignmentStatusHistory.

**Frontend:**
- ✅ AssignmentListPage.
- ✅ AssignmentDetailPage.
- ✅ AssignmentFormModal.
- ✅ PlanningBoardPage.
- ✅ TypeScript types (assignment.types.ts), API service (assignment.service.ts), hooks (useAssignment.ts).

### Pha 6. External requests và travel — ✅ HOÀN THÀNH

Thời lượng: 3 đến 4 tuần.

Kết quả:

- ✅ Agency request flow — ExternalRequest + ExternalRequestMessage + ExternalRequestService + ExternalRequestController.
- ✅ Candidate submission — ExternalCandidate model.
- ✅ Auto-generate travel requests — TravelRequest + TravelService + TravelController.
- ✅ Itinerary distribution — TravelSegment + TravelStatusHistory.

**Frontend:**
- ✅ ExternalRequestListPage + ExternalRequestDetailPage.
- ✅ TravelListPage + TravelDetailPage.
- ✅ TypeScript types (externalTravel.types.ts), API services (externalRequest.service.ts, travel.service.ts), hooks (useExternalRequest.ts, useTravel.ts).

### Pha 7. Onboard access, sign-on/sign-off và Edge integration — ✅ HOÀN THÀNH

Thời lượng: 3 tuần.

Kết quả:

- ✅ Access grant policy — CrewAccessGrant model + AccessGrantStatus enum.
- ✅ Onboard event flow — OnboardEvent model + OnboardEventService + OnboardEventController.
- ✅ Sign-on/sign-off sync — SignOnRecord + SignOffRecord models.
- ⚠️ Service record auto-update — Models ready, nhưng chưa xác nhận logic tự động cập nhật ServiceRecord khi sign-off.

**Frontend:**
- ✅ OnboardDashboardPage.
- ✅ TypeScript types (OnboardDtos), hooks (useOnboard.ts).

### Pha 8. Hardening, pilot và rollout — ❌ CHƯA BẮT ĐẦU

Thời lượng: 2 đến 3 tuần.

Kết quả:

- ❌ PoC cho 1 đến 2 tàu, 10 đến 20 crew — Chưa pilot.
- ❌ SOP theo vai trò — Chưa viết.
- ❌ Performance tuning — Chưa thực hiện.
- ❌ QA regression và go-live checklist — Chưa có.

## 25. Test strategy và acceptance criteria

### 25.1 End-to-end scenarios bắt buộc

- Tạo crew mới với reference vessel và nationality cụ thể, checklist sinh đúng theo matrix.
- Crew nhận email, activate account, cài PWA, upload passport và medical certificate thành công.
- Reviewer verify passport, hệ thống khóa version đã verified.
- Renew medical certificate tạo version mới, version cũ chuyển superseded.
- Compliance evaluation chặn assignment khi thiếu blocker document.
- Compliance evaluation cho phép warning nếu policy là warning-only.
- Coordinator gửi assignment cho crew, crew confirm trên PWA.
- Nếu thiếu người nội bộ, external request được gửi và agency phản hồi candidate.
- Assignment confirmed sinh travel request, itinerary được publish và crew nhìn thấy trên PWA.
- Master confirm onboard trên Edge, Shore nhận onboard event và cấp quyền truy cập phù hợp.
- Sign-off trên Edge cập nhật service record và assignment completion trên Shore.

### 25.2 Non-functional acceptance criteria

- Hệ thống vẫn usable trên mạng yếu với upload queue và retry.
- Crew list, assignment board, compliance summary đáp ứng SLA hiệu năng đã nêu.
- Audit trail đầy đủ cho verify, reject, renew, confirm, waive, onboard, sign-on, sign-off.
- Không có hành động nhạy cảm nào không qua permission check.

## 26. Rủi ro và biện pháp giảm thiểu

### Rủi ro 1. Rule compliance thay đổi liên tục

Giảm thiểu bằng rule engine cấu hình được và versioned rule sets.

### Rủi ro 2. Thiết kế dữ liệu bị dồn vào CrewMember hiện tại

Giảm thiểu bằng aggregate riêng cho onboarding, documents, assignments, travel.

### Rủi ro 3. UI planning quá nặng ở giai đoạn đầu

Giảm thiểu bằng cách phát hành list/timeline đơn giản trước, chưa đầu tư gantt phức tạp quá sớm.

### Rủi ro 4. Upload trên mạng yếu gây trải nghiệm xấu

Giảm thiểu bằng client-side compression, queueing, async conversion, signed uploads và progressive feedback.

### Rủi ro 5. Sync với Edge làm sai ownership

Giảm thiểu bằng policy ownership rõ ràng và correction workflow riêng cho factual events.

### Rủi ro 6. Dữ liệu cá nhân và y tế bị lộ do phân quyền sai

Giảm thiểu bằng fine-grained authorization, document sensitivity labels, download tokens và audit bắt buộc.

## 27. Khuyến nghị triển khai thực tế

### 27.1 Những gì cần làm đúng ngay từ đầu

- Chốt state machine trước khi làm UI chi tiết.
- Thiết kế document versioning chuẩn ngay từ đầu.
- Tách compliance evaluation thành service testable độc lập.
- Chuẩn hóa assignment event flow gắn với travel và onboard.
- Chốt ownership Shore-Edge cho sign-on/sign-off trước khi viết sync contract.

### 27.2 Những gì không nên làm ở MVP

- Không làm native mobile mới trước khi PWA chứng minh được giá trị.
- Không cố tích hợp OCR/AI extraction ở giai đoạn đầu.
- Không làm external portal full-feature trước khi secure link flow ổn định.
- Không dùng một bảng documents chung thiếu version history chỉ để đi nhanh.

## 28. Điều kiện sẵn sàng trước khi vào sprint build đầu tiên

- Chốt field bắt buộc cho crew profile.
- Chốt document taxonomy.
- Chốt document state machine.
- Chốt assignment state machine.
- Chốt compliance dimensions, severity và waiver policy.
- Chốt equivalent rank policy.
- Chốt travel generation rules.
- Chốt ownership Shore-Edge cho onboard events.
- Chốt SLA cho verification, agency response và travel handling.
- Chốt security model cho portal, secure links và file access.

## 29. Kết    
8. Xây dựng mechanism nhận onboard events từ Edge (SyncInbox handler).

Đợt 3 — Background jobs & Notifications:
9. Background job compliance periodic re-evaluation.
10. Background job SLA breach monitoring (document verification + external request).
11. Notification service (in-app + email templates).