# Services/AI — Trợ lý AI & Tự động đánh giá báo cáo

## Mục đích

Thư mục này cung cấp hai tính năng dùng chung một "bộ não" LLM: (1) chatbot cho phép nhân viên Shore hỏi-đáp tự nhiên về dữ liệu vận hành của một tàu (ví dụ "tại sao tiêu hao nhiên liệu tuần này cao hơn?"), và (2) tự động sinh nhận xét AI (tiếng Việt) cho từng Noon Report ngay khi nó được đồng bộ lên từ Edge. Đây là phần "thông minh hóa" dữ liệu vận hành, tách biệt hoàn toàn khỏi cơ chế đồng bộ (`Services/Sync/`) dù được `SyncInboxService` kích hoạt gián tiếp (xem `Services/Background/README.md`).

## Cấu trúc & vai trò

| File / thư mục | Vai trò |
|---|---|
| `IAiChatService.cs` | Interface duy nhất `ChatAsync(AiChatRequest, CancellationToken) : Task<AiChatResponse>` — điểm phụ thuộc chung cho Controller |
| `AiChatService.cs` | Bản triển khai **V1 — hiện là code chết**: không được đăng ký DI ở `Program.cs` (chỉ `AiChatServiceV2` được đăng ký cho `IAiChatService`), không nơi nào khác trong code gọi tới class này. Giữ lại làm tài liệu tham khảo lịch sử |
| `AiChatServiceV2.cs` | Bản triển khai **đang chạy thật** (806 dòng) — chatbot nâng cao: nhận diện ý định câu hỏi, dựng ngữ cảnh 30 ngày + phát hiện ngày bất thường, cache 2 tầng, nhớ hội thoại nhiều lượt |
| `IGeminiEvaluationService.cs` / `GeminiEvaluationService.cs` | Lớp gọi HTTP ra ngoài duy nhất tới nhà cung cấp LLM — xem ghi chú tên gọi bên dưới |
| `Analysis/SemanticAnalysisService.cs` | `ISemanticAnalysisService` — phân loại ý định câu hỏi (12 loại) và tính độ tương đồng câu hỏi, thuần bằng đối chiếu từ khóa (không dùng embedding/ML thật) |
| `Caching/AiMemoryCacheService.cs` | `IAiCacheService` — cache tự viết tay (`ConcurrentDictionary`, KHÔNG dùng `IMemoryCache` có sẵn của ASP.NET Core), đăng ký Singleton, tự dọn dẹp mỗi 30 phút |
| `Conversation/ConversationHistoryService.cs` | `IConversationHistoryService` — lưu lịch sử hội thoại theo `SessionId` trong `Dictionary` thuần (không thread-safe), tối đa 20 tin nhắn/phiên, đăng ký Singleton |

Ba file trong `Analysis/`, `Caching/`, `Conversation/` mỗi thư mục chỉ có 1 file — không tách README riêng vì đơn giản, vai trò của chúng được giải thích đầy đủ trong luồng `AiChatServiceV2` bên dưới.

## Luồng hoạt động chính

**Luồng 1 — Chatbot (`AiChatServiceV2.ChatAsync`)**, gọi bởi `Controllers/ReportEvaluationsController.Chat`:

```
1. ISemanticAnalysisService.DetectIntent(question)     → (intent, confidence) — 12 loại ý định
2. IAiCacheService.GetAsync<AiChatResponse>(hash)       → nếu có cache VÀ confidence > 0.7: trả ngay (IsCached=true), DỪNG
3. (nếu có SessionId) IConversationHistoryService
      .BuildContextFromHistoryAsync(sessionId)          → khối "RECENT USER INTENTS" từ 5 câu hỏi gần nhất
4. Truy vấn AppDbContext: NoonReport 30 ngày gần nhất
   + số liệu PMS quá hạn/đang chạy, alert đang mở, chứng chỉ sắp hết hạn (nếu có VesselId)
5. Dựng/hoặc lấy cache "ContextBundle" (trend 30 ngày + brief ngày bất thường, cache riêng 20 phút)
6. Ghép ENHANCED_SYSTEM_PROMPT (nhiều placeholder: TODAY_DATA, TREND_DATA, ANALYSIS_TYPE...)
7. IGeminiEvaluationService.ChatWithReportsAsync(prompt) → gọi LLM ngoài
8. Hậu xử lý: nếu câu hỏi về "ngày bất thường" mà câu trả lời LLM không nhắc ngày cụ thể
   nào (regex dd/MM) → tự chèn thêm danh sách ngày bất thường tính theo rule (z-score đơn giản)
9. Cache câu trả lời cuối (1 giờ) + lưu cả câu hỏi/trả lời vào ConversationHistory (nếu có SessionId)
```

**Luồng 2 — Tự động đánh giá Noon Report**: được kích hoạt từ `Services/Sync/SyncInboxService` sau khi xử lý xong một batch chứa `noon_report`, đi qua `Services/Background/ReportEvaluationQueue`/`ReportEvaluationWorker`, cuối cùng gọi `IGeminiEvaluationService.EvaluateReportAsync(jsonData)` để sinh ra bản ghi `ReportEvaluation` (`Models/ReportEvaluation.cs`). Chi tiết đầy đủ của pipeline hàng đợi nằm ở `Services/Background/README.md`.

## Liên kết với phần khác

- **Được gọi bởi**: `Controllers/ReportEvaluationsController.cs` (chat + xem kết quả đánh giá), `Services/Background/ReportEvaluationWorker.cs` (đánh giá tự động).
- **Gọi tới**: `AppDbContext` trực tiếp (đọc `NoonReports`, PMS, alerts, certificates), `IGeminiEvaluationService` (gọi HTTP ra ngoài), và lẫn nhau giữa các service trong chính thư mục này (`AiChatServiceV2` gọi cả `ISemanticAnalysisService` + `IAiCacheService` + `IConversationHistoryService`).
- **DTO liên quan**: `DTOs/AiChatRequest.cs` (họ DTO chat: `AiChatRequest`, `ConversationMessage`, `AiChatResponse`, `GeminiChatResult`, `ResponseMetrics`), `DTOs/AiEvaluationResponse.cs` (`AiEvaluationResponse`, `ShipMetricsDto` — dùng cho đánh giá tự động), `DTOs/ChatRequestDto.cs` (DTO chat đơn giản hơn, vẫn dùng ở một endpoint debug riêng), `DTOs/AiChatEnhancedRequest.cs` (file rỗng — chỉ còn comment ghi chú các DTO cũ đã gộp vào `AiChatRequest.cs`).

## Ghi chú khi đọc/dạy

- **Tên gọi "Gemini" gây hiểu lầm — code thật gọi Groq.** `GeminiEvaluationService` gửi HTTP request tới `https://api.groq.com/openai/v1/chat/completions`, model `llama-3.3-70b-versatile`, đọc API key từ `Groq:ApiKey` (dự phòng `Gemini:ApiKey`). Code parse response còn giữ nhánh dự phòng cho định dạng Gemini gốc (`candidates[0].content.parts[0].text`) — dấu vết rõ ràng của một lần đổi nhà cung cấp mà không đổi tên class/interface. Khi dạy/đọc code, luôn nói "LLM provider (hiện là Groq)" thay vì mặc định đó là Gemini thật.
- **`AiChatService.cs` (V1) là code chết** — không bị xóa nhưng không được đăng ký DI (`Program.cs` chỉ đăng ký `AiChatServiceV2`). Đừng sửa file này mong ảnh hưởng hành vi thật; mọi thay đổi chatbot phải sửa `AiChatServiceV2.cs`.
- **`ChatWithReportsAsync`'s tham số `reportsJson` bị dùng chồng nghĩa**: khi gọi từ `AiChatServiceV2`, tham số này thực chất chứa **toàn bộ prompt đã dựng xong** (nhận diện qua chuỗi đánh dấu `"[DEPTH_CONFIGURED]"`) chứ không phải JSON dữ liệu thô như tên gợi ý.
- **3 tầng cache/lưu trạng thái đều là in-memory, không phải Redis/distributed**: `AiMemoryCacheService` (Singleton, `ConcurrentDictionary`), `ConversationHistoryService` (Singleton, `Dictionary` KHÔNG thread-safe — có nguy cơ race condition nếu cùng session bị ghi đồng thời), và cache 5 giây trong `NetworkDetectionService` (không liên quan AI nhưng cùng pattern). Nếu triển khai nhiều instance Shore (scale-out), mỗi instance có cache/lịch sử hội thoại riêng — cache hit-rate và trí nhớ hội thoại phụ thuộc vào việc load balancer có "sticky session" hay không.
- **`ConversationHistoryService` không có cơ chế tự dọn phiên cũ** — `ClearSessionAsync` tồn tại nhưng không nơi nào trong `AiChatServiceV2` chủ động gọi nó; chạy đủ lâu có thể tích lũy nhiều session trong bộ nhớ.
- **Có 3 lớp timeout/retry chồng nhau** khi gọi ra ngoài: `HttpClient` timeout 30s (`Program.cs`), Polly resilience handler (3 lần thử lại, 2s/lần — cũng ở `Program.cs`), và `CancellationTokenSource` nội bộ trong `GeminiEvaluationService` (20-25s) — khi debug timeout, kiểm tra cả ba nơi.
- Cờ phiên bản prompt (`PromptCacheVersion`/`ContextCacheVersion`, ví dụ `"prompt-v4"`) được nhúng vào cache key — sửa nội dung prompt mà quên tăng version này sẽ khiến câu trả lời cũ (theo prompt cũ) tiếp tục được phục vụ tới 1 giờ.
