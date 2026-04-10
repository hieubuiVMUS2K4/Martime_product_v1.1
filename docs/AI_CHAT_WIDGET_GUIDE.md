# AI Chat Widget - Hướng Dẫn Công Năng

## Tổng Quan

Tính năng AI Chat Widget cho phép bạn **trò chuyện với AI để phân tích báo cáo của tàu** mà không cần phải vào chi tiết từng báo cáo. Tất cả các dữ liệu báo cáo sẽ được tiền xử lý, chuẩn hóa, và đưa cho Gemini API để phân tích.

## Cách Sử Dụng

### 1. Trên Giao Diện Danh Sách Báo Cáo
Khi bạn vào xem danh sách báo cáo của một tàu, bạn sẽ thấy:
- **Nút tròn với biểu tượng hộp sơ đồ** ở góc dưới bên phải màn hình (FAB - Floating Action Button)
- Click nút này để mở cửa sổ AI Chat

### 2. Trong Cửa Sổ Chat
- **Khởi đầu**: AI sẽ chào hỏi bạn và thông báo nó có thể phân tích dữ liệu báo cáo
- **Nhập câu hỏi**: Gõ câu hỏi của bạn vào ô input phía dưới
  - Ví dụ: "Phân tích báo cáo hôm nay"
  - Ví dụ: "Tiêu hao nhiên liệu có bình thường không?"
  - Ví dụ: "Hãy so sánh RPM giữa hôm nay và hôm qua"
- **Gửi yêu cầu**: Nhấn nút "Gửi" hoặc bấm phím `Enter`

### 3. AI Sẽ Trả Lời
AI sẽ:
1. Thu thập dữ liệu báo cáo từ 30 ngày gần nhất
2. Chuẩn hóa dữ liệu theo định dạng JSON
3. Gửi đến Gemini API kèm theo câu hỏi của bạn
4. Trả lại câu trả lời chi tiết bằng tiếng Việt

## Ví Dụ Câu Hỏi Efektif

```
✅ Phân tích tình trạng máy trong 7 ngày qua
✅ Nhiên liệu tiêu hao có tăng so với bình thường không?
✅ Tàu gặp phải bất thường nào trong tuần vừa rồi?
✅ RPM và tốc độ có liên hệ gì không?
✅ Hãy đánh giá hiệu suất vận hành tàu
```

## Backend API Endpoint

### Endpoint
```
POST /api/reports/chat/vessels/{vesselId}
```

### Request Body
```json
{
  "message": "Phân tích báo cáo hôm nay",
  "vesselId": "guid-của-tàu"
}
```

### Response
```json
{
  "answer": "Phân tích chi tiết từ AI...",
  "sources": "Phân tích dựa trên 25 báo cáo từ 15/03/2026 đến 08/04/2026",
  "success": true
}
```

## Cấu Hình

### 1. API Key Gemini
Đảm bảo bạn đã thêm Gemini API Key:

**Cách 1: User Secrets (Dev)**
```bash
cd shore_product/backend
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY"
```

**Cách 2: appsettings.Development.json**
```json
{
  "Gemini": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

**Cách 3: Environment Variable (Production)**
```bash
export Gemini__ApiKey=YOUR_API_KEY
```

### 2. Thời Gian Query Mặc Định
- Hiện tại AI query dữ liệu từ **30 ngày gần nhất**
- Bạn có thể thay đổi trong `AiChatService.cs`:
```csharp
FromDate = DateTime.UtcNow.AddDays(-30),  // ← Thay đổi số ngày ở đây
ToDate = DateTime.UtcNow
```

## Các File Được Tạo/Sửa

### Backend
- `Services/AI/IAiChatService.cs` - Interface service chat
- `Services/AI/AiChatService.cs` - Implementation service chat
- `DTOs/AiChatRequest.cs` - DTO request
- `DTOs/ChatRequestDto.cs` - DTO request (legacy)
- `Controllers/ReportEvaluationsController.cs` - API endpoint
- `Program.cs` - Dependency injection

### Frontend
- `components/Reports/AIChatWidget.tsx` - React component chat
- `components/Reports/AIChatWidget.css` - Styling
- `pages/Report/VesselReportDetailPage.tsx` - Integration point

## Troubleshooting

### API trả lỗi "API Key missing"
→ Kiểm tra xem Gemini API Key đã được cấu hình chưa

### Chat không nhận được câu trả lời
→ Kiểm tra:
1. Backend có chạy không: `dotnet run` ở thư mục `backend`
2. Có dữ liệu báo cáo trong database không
3. Network connection đến Gemini API có hợp lệ không

### Frontend không hiển thị chat widget
→ Đảm bảo:
1. `AIChatWidget` được import đúng: `import AIChatWidget from '../../components/Reports/AIChatWidget'`
2. Component được render trong `VesselReportDetailPage`
3. `vesselId` prop không phải undefined

## Hiệu Suất & Hạn Chế

- **Rate Limit Gemini**: 15 request/phút (API free tier)
- **Timeout**: 30 giây cho mỗi yêu cầu
- **Max Records**: Lấy tối đa 30 báo cáo gần nhất
- **Max Prompt Size**: ~4096 tokens (tuỳ thuộc vào dữ liệu)

## Phát Triển Tiếp Theo

Có thể mở rộng thêm:
- [ ] Lưu lịch sử chat vào database
- [ ] Multi-turn conversation (nhớ context)
- [ ] Export chat transcript
- [ ] Real-time streaming response từ AI
- [ ] Support other LLMs (Claude, GPT-4, v.v.)
