# components/Reports/ — Trợ lý AI cho báo cáo tàu

## Mục đích

2 component tích hợp AI (LLM) vào luồng xem báo cáo hải trình — nằm trong `pages/Report/*`, không phải một "page" độc lập.

## Cấu trúc & vai trò

| File | Export | Dùng ở | Vai trò |
|---|---|---|---|
| `AIInsights.tsx` | `AIInsights` (props: `reportId`) | `pages/Report/ReportDetailPage.tsx` | Banner phân tích tự động cho **một báo cáo cụ thể**: gọi `GET {ENV.API_BASE_URL}/reports/:reportId/ai-insights`, nhận `{ status: 'Normal'|'Warning'|'Critical', contentVi }`, hiển thị màu/icon tương ứng (xanh/vàng/đỏ) + nội dung tiếng Việt. Skeleton loading khi đang chờ; ẩn hẳn nếu API lỗi (không hiển thị thông báo lỗi). |
| `AIChatWidget.tsx` | `AIChatWidget` (default export, props: `vesselId`) | `pages/Report/VesselReportDetailPage.tsx` | Chat widget dạng nút nổi (floating action button) góc màn hình, mở ra cửa sổ chat hỏi-đáp về **hiệu suất/an toàn/bảo trì của một con tàu**, có sẵn 6 nhóm câu hỏi mẫu (nhiên liệu, máy/hải hành, ngày bất thường, ETA/thời tiết, an toàn/tuân thủ, bảo trì/vật tư). Gọi `POST {ENV.API_BASE_URL}/reports/chat/vessels/:vesselId` với `{ message, vesselId, sessionId }`. |

## Luồng hoạt động chính

```
AIChatWidget:
  mount → tạo/khôi phục sessionId từ localStorage (`ai_chat_history_{vesselId}_session`)
        → khôi phục lịch sử chat từ localStorage (`ai_chat_history_{vesselId}_messages`)
  user gõ câu hỏi (hoặc bấm câu hỏi mẫu) → POST /reports/chat/vessels/:vesselId
        → AbortController timeout 60s (mô hình AI có thể chậm)
        → xử lý riêng lỗi 429 (rate limit, đọc Retry-After) và 503 (model quá tải)
        → append câu trả lời (kèm "sources") vào lịch sử chat, lưu lại localStorage

AIInsights:
  mount → GET /reports/:reportId/ai-insights → hiển thị banner Normal/Warning/Critical
```

Cả 2 component gọi API **trực tiếp bằng `fetch`**, không qua một file trong `services/` — đây là 2 ngoại lệ so với quy ước chung của thư mục (xem `services/README.md`).

## Liên kết với phần khác

- **pages/Report/ReportDetailPage.tsx**: render `AIInsights` cho report con (`report.childReport.id`).
- **pages/Report/VesselReportDetailPage.tsx**: render `AIChatWidget` khi có `vesselId`.
- **config/env.ts**: cả 2 component dùng `ENV.API_BASE_URL` để build URL gọi API AI (backend Shore, không gọi thẳng OpenAI/Anthropic từ trình duyệt — key AI được giữ ở phía Backend).
- **localStorage**: `AIChatWidget` là nơi hiếm hoi trong Shore Frontend dùng `localStorage` để lưu **nội dung nghiệp vụ** (lịch sử chat), không chỉ token/preference như các nơi khác (`authToken`, `shore_selected_vessel`, `hold_notifications_last_seen`).

## Ghi chú khi đọc/dạy

- Đây là 2 trong số rất ít nơi trong Shore Frontend gọi endpoint **không thuộc CRUD chuẩn** (`/reports/:id/ai-insights`, `/reports/chat/vessels/:id`) — nếu cần tìm hiểu tích hợp AI của dự án, đây là điểm bắt đầu đúng, phần Backend tương ứng nằm trong `shore_product/backend` (controller Reports, không nằm trong phạm vi tài liệu này).
- `AIChatWidget` lưu lịch sử chat trong `localStorage` theo từng `vesselId` — nghĩa là lịch sử **không đồng bộ giữa các máy/trình duyệt khác nhau** và sẽ mất nếu người dùng xoá dữ liệu trình duyệt; đây không phải bug, mà là thiết kế hiện tại (chưa lưu lịch sử chat phía server).
- Timeout 60 giây cho chat là chủ đích (comment trong code: *"60 seconds (optimized data = faster response)"*) — nếu thấy request chat bị huỷ giữa chừng khi debug mạng chậm, kiểm tra `AbortController` này trước.
