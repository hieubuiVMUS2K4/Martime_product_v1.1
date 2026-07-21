# Controllers/Safety — An toàn, Tuân thủ, Diễn tập, HSQE & Sổ tay SMS

## Mục đích

Nhóm controller lớn nhất và giàu quy tắc tuân thủ nhất trong toàn bộ Edge backend — bao phủ báo động an toàn, tuân thủ SOLAS/MARPOL (đọc), quy trình xin hoãn bảo trì, diễn tập khẩn cấp (SOLAS/ISPS), và đặc biệt là 2 module đồ sộ **HSQE** (Health-Safety-Quality-Environment) và **SMS** (Safety Management System theo ISM Code) — mô phỏng gần như đầy đủ hệ thống quản lý an toàn giấy tờ của một công ty vận tải biển thực tế.

## Cấu trúc & vai trò

| File | Route | Vai trò |
|---|---|---|
| `AlarmsController.cs` | `api/alarms` | Vòng đời báo động buồng lái/ECR: active/history/acknowledge/resolve/statistics. Có endpoint demo `POST test/generate-sample` chèn 5 alarm mẫu — nằm ngay trong controller nghiệp vụ thật (không phải `Controllers/Testing/`). |
| `ComplianceController.cs` (62 dòng) | `api/compliance` | Rất tối giản — chỉ 2 endpoint đọc: Watchkeeping logs (STCW/SOLAS V) và Oil Record Book (MARPOL Annex I). Chưa có logic ghi/validate riêng trong file này (nghiệp vụ ghi nằm ở `Controllers/Logbooks/`). |
| `DeferralRequestController.cs` | `api/deferral-requests` | Quy trình xin hoãn bảo trì, gồm quy tắc đặc thù cho hạng mục **CMS (Continuous Machinery Survey)**: hoãn &gt; 90 ngày bắt buộc phải có Class Permission Letter. |
| `DrillController.cs` (1020 dòng) | `api/drill` | Quản lý loại diễn tập (SOLAS/ISPS), lịch diễn tập dạng Gantt, log thực hiện với phê duyệt của Master (khoá vĩnh viễn sau khi duyệt), thống kê tỷ lệ tuân thủ. |
| `HsqeController.cs` (1615 dòng — lớn nhất Safety) | `api/hsqe` | Kiểm soát tài liệu SMS (version-bump tự động), sự cố/tai nạn với chuỗi 5-Why + CAPA, đánh giá rủi ro (JHA/JSA), giấy phép làm việc nguy hiểm (Hot/Enclosed/Aloft/Cold) với gas-test interlock và chữ ký PIN 2 cấp. |
| `SmsController.cs` (1052 dòng) | `api/sms` | Cấu trúc Sổ tay SMS đầy đủ theo ISM Code: `IsmElement` (16 chương) → `SmsProcedure` → `SmsFormTemplate` → `SmsFilledRecord`, kèm import file Word/PDF qua Gotenberg + Mammoth. |

## Luồng hoạt động chính

### A. Kiểm soát tài liệu SMS/HSQE — version-bump tự động (`HsqeController.UpdateDocument`)

```
Sửa tài liệu đang Published:
  → Lưu snapshot nội dung cũ vào Revisions, EditCount++
  → EditCount < 3   → bump version NHỎ: "Rev X.1" → "Rev X.2" → ...
  → EditCount == 3  → bump version LỚN: "Rev {X+1}.0", reset EditCount = 0
  → Nếu ≥5 tài liệu category PROCEDURE đã bump lớn (reissuedProcesses)
        → TỰ ĐỘNG bump version luôn tài liệu SMS_HANDBOOK gốc (hiệu ứng dây chuyền)
  → Status quay về "Draft" — chờ DPA (Designated Person Ashore) duyệt lại
```

### B. Giấy phép làm việc nguy hiểm — gas-test interlock + chữ ký PIN 2 cấp (`HsqeController`)

```
POST /api/hsqe/permits/{id}/gas-test
  Ngưỡng an toàn CỨNG: O2 ≥ 20.9%, LEL < 1%, CO < 25ppm, H2S ≤ 0.0
  → Vi phạm BẤT KỲ ngưỡng nào → TỰ ĐỘNG THU HỒI mọi chữ ký đã ký trước đó
                                 (ChiefOfficerSigned/CaptainApproved = false)
                                 + trả lỗi "Khí độc hoặc nồng độ Oxy Vượt Ngưỡng An Toàn!"

POST /api/hsqe/permits/{id}/sign  { role, pin }
  role = ChiefOfficer → chấp nhận PIN 4 số bất kỳ
  role = Captain      → BẮT BUỘC ChiefOfficerSigned == true trước
                       → BẮT BUỘC PIN đúng "1111" (giá trị demo cứng)
                       → với Hot/Enclosed: RE-VALIDATE kết quả gas-test gần nhất vẫn an toàn
                         (không thể lách qua bằng cách ký nhanh trước khi gas-test cập nhật xấu)
```

`SmsController.ApproveRecord` dùng đúng pattern PIN cứng `"1111"` tương tự cho cấp phê duyệt cuối (Captain/DPA).

### C. Sự cố → 5-Why → CAPA → tự đóng (`HsqeController`)

```
CreateIncident (phân loại theo "Kim tự tháp Heinrich": major/minor/near-miss dựa trên Severity+Type)
  → InvestigateIncident: ghi Why1..Why5 + RootCause → Status = "CAPA_Open"
  → AddCapa (nhiều action Corrective/Preventive)
  → ToggleCapa: khi TẤT CẢ CAPA của incident hoàn tất → tự động Status = "Closed"
```

### D. Diễn tập — khoá vĩnh viễn sau khi Master duyệt (`DrillController`)

Log diễn tập (`DrillLog`) sau khi được `PUT .../approve` (Master) sẽ `IsLocked = true` — không thể sửa lại. Riêng bản ghi có `IsSecureHistory = true` còn được chặn **xoá** tường minh, viện dẫn "SOLAS, ISM Code" ngay trong message lỗi.

## Liên kết với phần khác

- **`Controllers/Logbooks/README.md`** — `ComplianceController` chỉ đọc lại dữ liệu Watchkeeping/Oil Record mà `Services/Logbooks/` ghi ra.
- **`Constants/TaskStatus.cs`** — trạng thái task (`SCHEDULED/DUE/OVERDUE/MISSING_*`) mà `DeferralRequestController` kiểm tra trước khi cho phép tạo yêu cầu hoãn.
- **`Models/DrillModels.cs`, `Models/HsqeModels.cs`, `Models/SmsModels.cs`** — entity nền cho cả 3 controller `DrillController`/`HsqeController`/`SmsController`.
- **`appsettings.json → DocumentConversion:GotenbergUrl`** — `SmsController.ImportDocx` gọi dịch vụ Gotenberg/LibreOffice HTTP (mặc định `http://localhost:3200`) để chuyển .docx sang PDF; dùng thư viện **Mammoth** để trích text thuần — cùng thư viện được `Helpers/DocxHeaderFooterExtractor.cs` dùng cho mục đích khác (trích header/footer).

## Ghi chú khi đọc/dạy

- **PIN `"1111"` hardcode và `DigitalSignature` là chuỗi giả** (`SIG-DIR-{initials}-2026-{shortGuid}`) ở cả `HsqeController` và `SmsController` — đây rõ ràng là mức bảo mật **demo/prototype**, KHÔNG phải cơ chế chữ ký số mã hoá thật. Cả 2 controller cũng **không có `[Authorize]`**. Khi dạy, cần nói rõ ranh giới giữa "workflow nghiệp vụ được mô phỏng đúng" và "cơ chế bảo mật chỉ mang tính minh hoạ" — đừng để sinh viên hiểu nhầm đây là chuẩn production.
- **`AlarmsController` có endpoint sinh dữ liệu mẫu** (`POST test/generate-sample`) nằm lẫn trong controller nghiệp vụ thật — khác với quy ước "endpoint test nên nằm ở `Controllers/Testing/`" mà chính dự án áp dụng ở nơi khác.
- **Gas-test interlock** (mục B) là ví dụ dạy tốt về "an toàn theo thiết kế" (safety-by-design) trong phần mềm: hệ thống tự động vô hiệu hoá chữ ký cũ khi có dữ liệu đo mới cho thấy nguy hiểm, thay vì chỉ dựa vào con người nhớ để huỷ.
- **`HsqeController`/`SmsController` là 2 trong số các controller dài nhất repo** (1615 và 1052 dòng) — nên đọc theo từng nhóm endpoint nhỏ (Document Control / Incidents / Risk & Permits ở Hsqe; Tree-Procedure-Template-Record ở Sms) thay vì đọc tuyến tính từ đầu đến cuối.
