# components/editor — Rich Text Editor (tiptap)

## Mục đích

Cung cấp **một** trình soạn thảo văn bản định dạng (rich text/WYSIWYG) dùng chung cho toàn dashboard, xây
trên thư viện **tiptap 3** (`@tiptap/react`, `@tiptap/starter-kit`...). Đây là thư viện mới được thêm vào
dự án (`package.json` liệt kê `@tiptap/*` là nhóm dependency riêng) để thay thế việc nhập tài liệu SMS/ISM
bằng textarea thuần — cho phép định dạng tiêu đề, danh sách, căn lề, tô nổi bật... giống Word, rồi lưu lại
dưới dạng chuỗi HTML.

## Cấu trúc & vai trò

| File | Export | Vai trò |
|---|---|---|
| `RichTextEditor.tsx` | `RichTextEditor` | Component soạn thảo đầy đủ: toolbar (Bold/Italic/Underline/Strike/Highlight, Heading 1-3, danh sách, trích dẫn, căn lề, undo/redo) + vùng soạn thảo |
| | `RichContentViewer` | Component **chỉ đọc** — render lại HTML đã lưu (dùng `dangerouslySetInnerHTML`), không có toolbar |
| `editor.css` | — | CSS riêng cho vùng soạn thảo (import thẳng ở `main.tsx`, áp dụng toàn cục chứ không phải CSS module cục bộ) |

Extension tiptap đang bật: `StarterKit` (heading cấp 1-3, đoạn văn, danh sách, blockquote, in đậm/nghiêng...
mặc định), `Underline`, `TextAlign` (áp dụng cho heading + paragraph), `Highlight` (tô nổi bật, không multicolor),
`Placeholder` (chữ mờ khi ô trống).

## Luồng hoạt động chính

```
Component cha (vd. SmsDocumentPage khi soạn SOP)
   │  const [content, setContent] = useState(initialHtml)
   ▼
<RichTextEditor content={content} onChange={setContent} placeholder="..." />
   │  useEditor({ extensions: [...], content, onUpdate: ({editor}) => onChange(editor.getHTML()) })
   ▼
Người dùng gõ/định dạng → tiptap phát sự kiện onUpdate → onChange(html) → component cha cập nhật state
   ▼
Khi lưu: component cha gửi chuỗi HTML này lên service (vd. smsService.bumpVersion({ newContent: content }))
   ▼
Khi hiển thị lại (không cho sửa): <RichContentViewer html={savedHtml} />  (không tạo instance editor, nhẹ hơn)
```

Có một `useEffect` đồng bộ 2 chiều đáng chú ý: nếu `content` (prop) đổi từ bên ngoài (vd. người dùng chuyển
sang xem phiên bản cũ hơn) **và** editor hiện không có focus, `RichTextEditor` tự gọi
`editor.commands.setContent(content, { emitUpdate: false })` để nạp lại nội dung mà không kích hoạt vòng lặp
`onChange` ngược lại.

## Liên kết với phần khác

- **`pages/HSQE/components/SmsDocumentPage.tsx`**: dùng `RichTextEditor` để soạn nội dung thủ tục (SOP) và
  xem lịch sử phiên bản; đồng thời tự viết hàm `tokenizeHtml`/`diffHtml` để so sánh 2 phiên bản HTML (không
  phải một phần của `components/editor`, nhưng thao tác trên cùng chuỗi HTML mà editor này tạo ra).
- **`pages/HSQE/components/DocumentLibrary/tabs/DocumentTab.tsx`,
  `ChapterVersionsTab.tsx`**: cũng dùng `RichTextEditor` cho module Document Library (module này hiện chưa
  được route — xem `pages/HSQE/README.md`).
- **`main.tsx`**: import `editor.css` ở cấp cao nhất (`import './components/editor/editor.css'`) — nghĩa là
  style của editor có hiệu lực toàn cục ngay từ khi app khởi động, không đợi tới khi component được mount.

## Ghi chú khi đọc/dạy

- Đây là thư viện **rich text duy nhất** trong dự án — nếu thấy nhu cầu soạn thảo văn bản định dạng ở tính
  năng mới (không chỉ HSQE), nên tái sử dụng `RichTextEditor` thay vì thêm thư viện editor khác, để tránh
  phình bundle.
- Nội dung được lưu dưới dạng **HTML thô** (không phải JSON/Markdown) — khi hiển thị lại luôn phải qua
  `RichContentViewer` (hoặc `dangerouslySetInnerHTML` tương đương) chứ không thể hiển thị như text thường.
  Cần lưu ý rủi ro XSS về lý thuyết nếu HTML này có thể bị chèn từ nguồn không tin cậy — hiện nội dung chỉ do
  người dùng nội bộ (thuyền viên/sĩ quan đã đăng nhập) soạn, không nhận HTML từ bên ngoài.
- `showToolbar={false}` cho phép dùng editor ở chế độ chỉ-nội-dung (không toolbar) nhưng **vẫn** cho sửa nếu
  `editable=true` — khác với `RichContentViewer` (hoàn toàn không có instance editor, chỉ render tĩnh). Khi
  cần "chỉ xem", nên dùng `RichContentViewer` để nhẹ hơn thay vì `RichTextEditor` với `editable={false}`.
