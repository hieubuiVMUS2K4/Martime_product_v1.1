export interface PrintSmsDocumentOptions {
  title: string;
  subtitle?: string;
  code?: string;
  version?: string;
  date?: string;
  contentHtml?: string;
  watermark?: string;
  isForm?: boolean;
  metadata?: Record<string, string>;
  signatures?: Array<{
    name?: string;
    rank?: string;
    sigCode?: string;
    timestamp?: string;
  }>;
}

/**
 * Opens an isolated print window and prints a standardized, high-fidelity A4 document.
 */
export function printSmsDocument(options: PrintSmsDocumentOptions) {
  const {
    title,
    subtitle = 'HỆ THỐNG QUẢN LÝ AN TOÀN HÀNG HẢI - SMS',
    code = '',
    version = '',
    date = '',
    contentHtml = '',
    watermark = '',
    isForm = false,
    metadata = {},
    signatures = []
  } = options;

  const printWindow = window.open('', '_blank');
  if (!printWindow) {
    alert('Không thể mở cửa sổ in. Vui lòng tắt trình chặn pop-up của trình duyệt.');
    return;
  }

  // Format date cleanly
  const formattedDate = date ? date.split('T')[0] : new Date().toISOString().split('T')[0];

  // 1. Build metadata table (only for procedures, or non-form headers)
  let metaHtml = '';
  if (!isForm && (code || version || formattedDate)) {
    metaHtml = `
      <div class="metadata-grid">
        <div class="metadata-item">
          <span class="metadata-label">Mã tài liệu:</span>
          <span class="metadata-value">${code || 'N/A'}</span>
        </div>
        <div class="metadata-item">
          <span class="metadata-label">Phiên bản:</span>
          <span class="metadata-value">Rev ${version || '1.0'}</span>
        </div>
        <div class="metadata-item">
          <span class="metadata-label">Ngày ban hành:</span>
          <span class="metadata-value">${formattedDate}</span>
        </div>
        <div class="metadata-item">
          <span class="metadata-label">Phạm vi áp dụng:</span>
          <span class="metadata-value">Đội tàu & Shore Office</span>
        </div>
      </div>
    `;
  }

  // 2. Build structured form layout if it is a form record
  let bodyContent = contentHtml;
  if (isForm) {
    if (code === 'TL-02-01') {
      const shipName = metadata['shipName'] || metadata['Tàu thực thi'] || 'N/A';
      const shipType = metadata['shipType'] || 'Bulk Carrier';
      const masterName = metadata['masterName'] || metadata['Người lập'] || 'N/A';
      const reviewDate = metadata['reviewDate'] || formattedDate;

      const items = [
        {
          num: '1',
          en: 'Are personnel aware of and understand the Company policies? Are there any areas of policy where staff consider that improvement could be made?',
          vi: 'Nhận thức của thuyền viên về các chính sách của Công ty? Những phần nào của chính sách cần được chú trọng nâng cao cho thuyền viên?',
          val: metadata['reviewItem1'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '2',
          en: 'Is the SLMS easily and readily accessible to all relevant staff? Do the officers and crew have a relevant understanding of the procedures contained in SLMS in relation to safety and their responsibilities onboard?',
          vi: 'Thuyền viên có thể dễ dàng tiếp cận với tài liệu QLAT&LĐHH? Thuyền viên và sỹ quan có hiểu được các quy trình và nhiệm vụ liên quan đến mình?',
          val: metadata['reviewItem2'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '3',
          en: 'Are records, filing and checklists being completed as required? What improvements would you recommend in relation to these areas of the SLMS?',
          vi: 'Các báo cáo, danh mục kiểm tra và cặp hồ sơ lưu được thực hiện theo yêu cầu? Những khuyến nghị để công tác này được thực hiện tốt hơn?',
          val: metadata['reviewItem3'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '4',
          en: 'Summarise any significant internal and external audit findings since last review and comment on any issues that may have come about as a result and corrective actions.',
          vi: 'Tóm tắt những phát hiện quan trọng trong đánh giá nội bộ và của bên ngoài kể từ lần soát xét trước, những nhận xét về các vấn đề liên quan đến việc thực hiện các hành động khắc phục.',
          val: metadata['reviewItem4'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '5',
          en: 'Briefly summarise any significant findings or defects raised by any third party since last review such as Port State and comment on steps taken to avoid recurrence.',
          vi: 'Tóm tắt những phát hiện, lỗi quan trọng trong các cuộc kiểm tra của PSC kể từ lần soát xét trước và những khuyến nghị về các hành động cần thiết để tránh lặp lại lỗi đó.',
          val: metadata['reviewItem5'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '6',
          en: 'Summarise accidents/ incidents since last review and comment on steps taken to avoid recurrence.',
          vi: 'Tóm tắt những tai nạn/ sự cố kể từ lần soát xét và những khuyến nghị về các hành động cần thiết để tránh lặp lại sự cố đó.',
          val: metadata['reviewItem6'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '7',
          en: 'Any customer (i.e. Owner and/or Charterers) feedback regarding satisfaction or complaints.',
          vi: 'Nhận xét, khiếu nại của khách hàng hoặc người thuê tàu.',
          val: metadata['reviewItem7'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '8',
          en: 'What general improvements do you consider could be made to the SLMS?',
          vi: 'Những cải tiến cần thiết đối với hệ thống QLAT&LĐHH?',
          val: metadata['reviewItem8'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '9',
          en: 'Discuss any training carried out during command and comment on its effectiveness. What areas of training would you consider that could be improved or made more beneficial?',
          vi: 'Các cuộc huấn luyện/đào tạo đã thực hiện và hiệu quả của chúng, những điểm cần cải tiến?',
          val: metadata['reviewItem9'] || '--- Không có thông tin / No comment ---'
        },
        {
          num: '10',
          en: 'Document Review. Please recommend any significant changes which you consider should be made in order to improve the effectiveness of the SLMS.',
          vi: 'Soát xét hệ thống tài liệu. Nêu những sửa đổi cần thiết nâng cao hiệu lực của Sổ tay QLAT&LĐHH.',
          val: metadata['reviewItem10'] || '--- Không có thông tin / No comment ---'
        }
      ];

      bodyContent = `
        <div style="font-family: Arial, sans-serif; margin-bottom: 20px;">
          <!-- Header Grid Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; text-align: center;">
            <tr>
              <td style="width: 25%; border-right: 1px solid #cbd5e1; padding: 10px;">
                <div style="font-size: 24px; margin-bottom: 3px;">⚓</div>
                <div style="font-size: 10px; font-weight: bold; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.5px;">HP SHIPPING</div>
              </td>
              <td style="width: 50%; border-right: 1px solid #cbd5e1; padding: 10px; background-color: #f8fafc;">
                <h3 style="font-size: 12px; font-weight: bold; margin: 0; text-transform: uppercase; color: #1e293b; line-height: 1.4;">
                  BIÊN BẢN SOÁT XÉT CÔNG TÁC QUẢN LÝ AN TOÀN,<br/>SỨC KHỎE, BẢO VỆ MÔI TRƯỜNG
                </h3>
                <p style="font-size: 10px; font-style: italic; color: #64748b; margin: 4px 0 0 0; font-weight: 600;">Master's Review of the SLMS</p>
              </td>
              <td style="width: 25%; padding: 10px; text-align: left; font-size: 9px; line-height: 1.4;">
                <div><strong>Mã biểu mẫu:</strong> <span style="font-family: monospace; font-weight: bold; color: #2563eb;">TL-02-01</span></div>
                <div><strong>Ngày ban hành:</strong> <span style="font-family: monospace;">20/10/2016</span></div>
                <div><strong>Lần sửa đổi:</strong> <span style="font-family: monospace;">00</span></div>
                <div><strong>Trang:</strong> 1 / 2</div>
              </td>
            </tr>
          </table>

          <!-- Metadata Information Row -->
          <table style="width: 100%; border-collapse: collapse; border-left: 1px solid #cbd5e1; border-right: 1px solid #cbd5e1; border-bottom: 1px solid #cbd5e1; padding: 5px; background-color: #f8fafc; font-size: 11px;">
            <tr>
              <td style="width: 50%; padding: 6px 12px; border-right: 1px solid #e2e8f0;"><strong>Tên tàu / Ship's Name:</strong> <span style="color: #0f172a; font-weight: 500;">${shipName}</span></td>
              <td style="width: 50%; padding: 6px 12px;"><strong>Loại tàu / Ship Type:</strong> <span style="color: #0f172a; font-weight: 500;">${shipType}</span></td>
            </tr>
            <tr>
              <td style="width: 50%; padding: 6px 12px; border-right: 1px solid #e2e8f0; border-top: 1px solid #e2e8f0;"><strong>Thuyền trưởng / Master:</strong> <span style="color: #0f172a; font-weight: 500;">${masterName}</span></td>
              <td style="width: 50%; padding: 6px 12px; border-top: 1px solid #e2e8f0;"><strong>Ngày lập / Date:</strong> <span style="font-family: monospace; color: #0f172a; font-weight: 500;">${reviewDate}</span></td>
            </tr>
          </table>

          <!-- Safety Questions list -->
          <div style="margin-top: 25px;">
            ${items.map(item => `
              <div style="margin-bottom: 15px; border-bottom: 1px dashed #e2e8f0; padding-bottom: 12px; page-break-inside: avoid;">
                <div style="font-weight: bold; color: #1e293b; display: flex; align-items: flex-start; gap: 8px; margin-bottom: 6px;">
                  <span style="display: inline-block; background-color: #f1f5f9; color: #475569; width: 18px; height: 18px; border-radius: 50%; text-align: center; font-size: 10px; line-height: 18px; flex-shrink: 0; font-family: sans-serif;">${item.num}</span>
                  <div style="font-size: 11px; font-family: sans-serif;">
                    <p style="margin: 0; color: #334155;">${item.en}</p>
                    <p style="margin: 2px 0 0 0; color: #64748b; font-style: italic; font-weight: 500;">${item.vi}</p>
                  </div>
                </div>
                <div style="background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 4px; padding: 10px; font-size: 11px; color: #0f172a; line-height: 1.5; white-space: pre-wrap; font-family: Arial, sans-serif;">${item.val}</div>
              </div>
            `).join('')}
          </div>
        </div>
      `;
    } else if (code === 'TL-26-03') {
      const shipName = metadata['shipName'] || metadata['Tàu thực thi'] || 'N/A';
      const monthYear = metadata['monthYear'] || 'N/A';

      const printCategories = [
        {
          title: "BẾP / GALLEY",
          prefix: "galley",
          items: [
            { num: "1", name: "Các bề mặt làm việc", cycle: "Sau khi sử dụng", method: "Bỏ thức ăn dư thừa và bụi bẩn / Rửa bề mặt bằng chất tẩy để loại bỏ dầu mỡ, thực phẩm và bụi bẩn / Rửa sạch, khử trùng, rửa lại / Lau khô (tự nhiên hoặc bằng khăn sạch)", note: "Đảm bảo dùng đúng nồng độ các chất tẩy rửa và khử trùng." },
            { num: "2", name: "Thớt", cycle: "Sau khi sử dụng", method: "Bỏ thức ăn dư thừa và bụi bẩn / Rửa bề mặt bằng chất tẩy để loại bỏ dầu mỡ, thực phẩm và bụi bẩn / Rửa sạch, khử trùng, rửa lại / Lau khô (tự nhiên hoặc bằng khăn sạch)", note: "" },
            { num: "3", name: "Sàn bếp", cycle: "Sau mỗi bữa ăn.", method: "Bỏ thức ăn dư thừa và bụi bẩn / Lau bề mặt bằng chất tẩy để loại bỏ dầu mỡ, thực phẩm và bụi bẩn / Lau sạch, để khô tự nhiên", note: "" },
            { num: "4", name: "Khu vực vệ sinh tay", cycle: "Sau mỗi bữa ăn.", method: "Rửa bề mặt với chất tẩy rửa / Khử trùng / Rửa sạch, để khô tự nhiên.", note: "" },
            { num: "5", name: "Dụng cụ - dao, đồ mở hộp, máy trộn thức ăn, v.v...", cycle: "Sau khi sử dụng", method: "Rửa bằng chất tẩy để loại bỏ dầu mỡ, thực phẩm và bụi bẩn. / Rửa sạch, khử trùng, rửa sạch / Lau khô (tự nhiên hoặc bằng khăn sạch)", note: "" },
            { num: "6", name: "Bồn rửa / vòi nước", cycle: "Hàng ngày", method: "Rửa bề mặt với chất tẩy rửa / Rửa sạch, để khô tự nhiên", note: "" },
            { num: "7", name: "Các đồ dùng thường xuyên chạm vào – tay nắm cửa, công tắc đèn, điều khiển, điện thoại vv", cycle: "Hàng ngày", method: "Lau sạch bằng chất khử trùng", note: "" },
            { num: "8", name: "Vách ngăn bếp/ sàn tàu", cycle: "Hàng ngày", method: "Rửa bề mặt với chất tẩy rửa / Rửa sạch, để khô tự nhiên", note: "" },
            { num: "9", name: "Chụp hút mùi/ quạt hút", cycle: "Hàng tuần", method: "Làm sạch, tẩy bằng chất tẩy rửa / Rửa sạch", note: "Đeo găng tay" },
            { num: "10", name: "Tủ lạnh", cycle: "Hàng tuần", method: "Dỡ bỏ thức ăn / Rửa bề mặt bằng chất tẩy rửa / Rửa sạch, khử trùng, rửa sạch / Để khô tự nhiên", note: "" },
            { num: "11", name: "Lò nướng, lò vi sóng, vỉ nướng", cycle: "Hàng tuần", method: "Làm sạch theo hướng dẫn của nhà sản xuất", note: "Đeo găng tay, chất tẩy rửa lò có thể là loại ăn mòn cao" }
          ]
        },
        {
          title: "CÁC KHO THỰC PHẨM / FOOD STORES",
          prefix: "stores",
          items: [
            { num: "1", name: "Quạt thông gió", cycle: "Hàng tuần", method: "Làm sạch, lau chùi", note: "" },
            { num: "2", name: "Sàn nhà kho khô", cycle: "Hàng tuần", method: "Hút bụi, Lau bằng nước lau sàn pha loãng, để khô tự nhiên", note: "" },
            { num: "3", name: "Thực phẩm kho kho dầu", cycle: "Hàng ngày", method: "Lau dọn sạch sẽ", note: "" },
            { num: "4", name: "Sàn nhà, kệ kho lạnh", cycle: "Khi nhận thực phẩm mới", method: "Dỡ bỏ thức ăn / Rửa bề mặt bằng chất tẩy rửa / Rửa sạch, khử trùng, rửa sạch / Để khô tự nhiên", note: "" }
          ]
        },
        {
          title: "PHÒNG Ở / CABINS & ACCOMMODATIONS",
          prefix: "cabins",
          items: [
            { num: "1", name: "Chăn, drap, vỏ gối, rèm vải.", cycle: "Hàng tuần", method: "Thay chăn, drap, vỏ gối, rèm vải sạch", note: "" },
            { num: "2", name: "Giường, tủ", cycle: "Hàng tuần", method: "Lau bụi, bẩn", note: "" },
            { num: "3", name: "Sàn nhà", cycle: "Hàng ngày", method: "Hút bụi, Lau bằng nước lau sàn pha loãng, để khô tự nhiên", note: "Giữ trật tự khi thuyền viên đang nghỉ ngơi" },
            { num: "4", name: "Kệ để giày dép", cycle: "Hàng tuần", method: "Hút bụi , lau sạch.", note: "" },
            { num: "5", name: "Thùng rác", cycle: "Hàng ngày", method: "Gom rác, phân loại, thay túi lót", note: "Phân loại rác theo quy định" },
            { num: "6", name: "Bộ phận thông gió", cycle: "Hàng tuần", method: "Làm sạch, lau chùi", note: "" },
            { num: "7", name: "Phòng vệ sinh, phòng tắm", cycle: "Hàng ngày", method: "Lau chùi", note: "" }
          ]
        },
        {
          title: "KHU VỰC SINH HOẠT CHUNG / COMMON AREAS",
          prefix: "common",
          items: [
            { num: "1", name: "Trần tường", cycle: "Hàng tuần", method: "Lau, chùi sạch", note: "" },
            { num: "2", name: "Sàn", cycle: "Hàng ngày", method: "Hút bụi, Lau bằng nước lau sàn pha loãng, để khô tự nhiên", note: "" },
            { num: "3", name: "Bàn, ghế, tủ", cycle: "Hàng ngày", method: "Lau, chùi sạch", note: "" },
            { num: "4", name: "Ti vi, đầu đĩa, quạt, đèn", cycle: "Hàng tuần", method: "Lau, chùi sạch", note: "" },
            { num: "5", name: "Tranh, khẩu hiệu, áp phích, tờ rơi", cycle: "Hàng tuần", method: "Lau, chùi", note: "" },
            { num: "6", name: "Thùng rác", cycle: "Hàng ngày", method: "Gom rác, phân loại, thay túi lót", note: "" },
            { num: "7", name: "Tủ lạnh", cycle: "Hàng tuần", method: "Rửa sạch, khử trùng, rửa sạch / Để khô tự nhiên", note: "" },
            { num: "8", name: "Bộ phận thông gió", cycle: "Hàng tuần", method: "Làm sạch, lau chùi", note: "" },
            { num: "9", name: "Phòng vệ sinh, phòng tắm", cycle: "Hàng ngày", method: "Lau chùi", note: "" }
          ]
        }
      ];

      bodyContent = `
        <div style="font-family: Arial, sans-serif; margin-bottom: 20px;">
          <!-- Header Grid Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; text-align: center;">
            <tr>
              <td style="width: 25%; border-right: 1px solid #cbd5e1; padding: 10px;">
                <div style="font-size: 24px; margin-bottom: 3px;">⚓</div>
                <div style="font-size: 10px; font-weight: bold; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.5px;">HP SHIPPING</div>
              </td>
              <td style="width: 50%; border-right: 1px solid #cbd5e1; padding: 10px; background-color: #f8fafc;">
                <h3 style="font-size: 12px; font-weight: bold; margin: 0; text-transform: uppercase; color: #1e293b; line-height: 1.4;">
                  LỊCH LÀM VỆ SINH BẾP, CÁC KHO THỰC PHẨM,<br/>KHU VỰC SINH HOẠT CHUNG, PHÒNG Ở
                </h3>
                <p style="font-size: 9px; font-weight: 600; color: #64748b; margin: 4px 0 0 0; text-transform: uppercase; font-family: sans-serif;">
                  ACCOMMODATIONS, STORE, GALLEY CLEANING SCHEDULE
                </p>
              </td>
              <td style="width: 25%; padding: 10px; text-align: left; font-size: 9px; line-height: 1.4;">
                <div><strong>Mã biểu mẫu:</strong> <span style="font-family: monospace; font-weight: bold; color: #2563eb;">TL-26-03</span></div>
                <div><strong>Ngày ban hành:</strong> <span style="font-family: monospace;">20/10/2016</span></div>
                <div><strong>Lần sửa đổi:</strong> <span style="font-family: monospace;">0</span></div>
                <div><strong>Trang:</strong> 1 / 1</div>
              </td>
            </tr>
          </table>

          <!-- Metadata Information Row -->
          <table style="width: 100%; border-collapse: collapse; border-left: 1px solid #cbd5e1; border-right: 1px solid #cbd5e1; border-bottom: 1px solid #cbd5e1; padding: 5px; background-color: #f8fafc; font-size: 11px;">
            <tr>
              <td style="width: 50%; padding: 6px 12px; border-right: 1px solid #e2e8f0;"><strong>Tên tàu / Ship's Name:</strong> <span style="color: #0f172a; font-weight: 500;">${shipName}</span></td>
              <td style="width: 50%; padding: 6px 12px;"><strong>Tháng / Month-Year:</strong> <span style="color: #0f172a; font-weight: 500;">${monthYear}</span></td>
            </tr>
          </table>

          <!-- Main Cleaning Schedule Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; margin-top: 20px; font-size: 10px; font-family: Arial, sans-serif;">
            <thead>
              <tr style="background-color: #f1f5f9; font-weight: bold; border-bottom: 1px solid #cbd5e1; text-align: left;">
                <th style="padding: 6px; border-right: 1px solid #cbd5e1; width: 40px; text-align: center;">STT</th>
                <th style="padding: 6px; border-right: 1px solid #cbd5e1; width: 180px;">Hạng mục / Item</th>
                <th style="padding: 6px; border-right: 1px solid #cbd5e1; width: 100px;">Chu kỳ / Frequency</th>
                <th style="padding: 6px; border-right: 1px solid #cbd5e1; width: 240px;">Phương thức vệ sinh / Method</th>
                <th style="padding: 6px; border-right: 1px solid #cbd5e1; width: 120px;">Người thực hiện / Performer</th>
                <th style="padding: 6px;">Lưu ý / Note</th>
              </tr>
            </thead>
            <tbody>
              ${printCategories.map(category => `
                <tr style="background-color: #eff6ff; font-weight: bold; border-bottom: 1px solid #cbd5e1;">
                  <td colspan="6" style="padding: 6px; text-transform: uppercase; font-size: 10px; color: #1e40af;">${category.title}</td>
                </tr>
                ${category.items.map(item => {
                  const key = `${category.prefix}_perf_${item.num}`;
                  const performer = metadata[key] || '---';
                  return `
                    <tr style="border-bottom: 1px solid #cbd5e1; page-break-inside: avoid;">
                      <td style="padding: 5px; border-right: 1px solid #cbd5e1; text-align: center; color: #64748b;">${item.num}</td>
                      <td style="padding: 5px; border-right: 1px solid #cbd5e1; font-weight: bold; color: #1e293b;">${item.name}</td>
                      <td style="padding: 5px; border-right: 1px solid #cbd5e1; font-style: italic; color: #475569;">${item.cycle}</td>
                      <td style="padding: 5px; border-right: 1px solid #cbd5e1; color: #334155; line-height: 1.4;">${item.method}</td>
                      <td style="padding: 5px; border-right: 1px solid #cbd5e1; font-weight: bold; color: #1d4ed8;">${performer}</td>
                      <td style="padding: 5px; font-style: italic; color: #64748b; font-size: 9px;">${item.note}</td>
                    </tr>
                  `;
                }).join('')}
              `).join('')}
            </tbody>
          </table>
        </div>
      `;
    } else if (code === 'TL-15-01') {
      const vessel = metadata['vessel'] || 'N/A';
      const location = metadata['location'] || 'N/A';
      const supplyBarge = metadata['supplyBarge'] || 'N/A';
      const bunkerDate = metadata['bunkerDate'] || formattedDate;
      const foreDraft = metadata['foreDraft'] || 'N/A';
      const aftDraft = metadata['aftDraft'] || 'N/A';

      // Parse arrays safely
      let products: any[] = [];
      try { products = typeof metadata['products'] === 'string' ? JSON.parse(metadata['products']) : (metadata['products'] || []); } catch(e){}
      if (!Array.isArray(products)) products = [];

      let personnel: any[] = [];
      try { personnel = typeof metadata['personnel'] === 'string' ? JSON.parse(metadata['personnel']) : (metadata['personnel'] || []); } catch(e){}
      if (!Array.isArray(personnel)) personnel = [];

      let distribution: any[] = [];
      try { distribution = typeof metadata['distribution'] === 'string' ? JSON.parse(metadata['distribution']) : (metadata['distribution'] || []); } catch(e){}
      if (!Array.isArray(distribution)) distribution = [];

      let gauging: any[] = [];
      try { gauging = typeof metadata['gauging'] === 'string' ? JSON.parse(metadata['gauging']) : (metadata['gauging'] || []); } catch(e){}
      if (!Array.isArray(gauging)) gauging = [];

      let signaturesList: any[] = [];
      try { signaturesList = typeof metadata['signatures_list'] === 'string' ? JSON.parse(metadata['signatures_list']) : (metadata['signatures_list'] || []); } catch(e){}
      if (!Array.isArray(signaturesList)) signaturesList = [];

      const renderCheckbox = (val: any) => {
        const isChecked = val === true || val === 'true';
        return isChecked 
          ? `<span style="font-family: Arial, sans-serif; font-weight: bold; color: #16a34a; font-size: 14px;">[✓]</span>` 
          : `<span style="font-family: Arial, sans-serif; font-weight: bold; color: #dc2626; font-size: 14px;">[ ]</span>`;
      };

      bodyContent = `
        <style>
          .bunker-section-title {
            font-size: 11px;
            font-weight: bold;
            text-transform: uppercase;
            color: #1e3a8a;
            border-bottom: 1.5px solid #1e3a8a;
            padding-bottom: 2px;
            margin-top: 15px;
            margin-bottom: 6px;
          }
          .bunker-table {
            width: 100% !important;
            border-collapse: collapse !important;
            margin: 6px 0 !important;
            font-size: 9.5px !important;
          }
          .bunker-table th, .bunker-table td {
            border: 1px solid #94a3b8 !important;
            padding: 4px 6px !important;
            text-align: center;
          }
          .bunker-table th {
            background-color: #f1f5f9 !important;
            font-weight: bold;
            font-size: 9px !important;
          }
          .checklist-box {
            background-color: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 4px;
            padding: 8px;
            margin-top: 5px;
            font-size: 9.5px;
          }
          .checklist-item {
            display: flex;
            align-items: flex-start;
            gap: 6px;
            margin-bottom: 4px;
          }
        </style>

        <!-- PAGE 1 -->
        <div style="page-break-after: always;">
          <!-- Header Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; text-align: center;">
            <tr>
              <td style="width: 25%; border-right: 1px solid #cbd5e1; padding: 6px;">
                <div style="font-size: 18px; margin-bottom: 1px;">⚓</div>
                <div style="font-size: 8px; font-weight: bold; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.5px;">HP SHIPPING</div>
              </td>
              <td style="width: 50%; border-right: 1px solid #cbd5e1; padding: 6px; background-color: #f8fafc;">
                <h3 style="font-size: 11px; font-weight: bold; margin: 0; text-transform: uppercase; color: #1e293b; line-height: 1.3;">
                  KẾ HOẠCH NHẬN NHIÊN LIỆU
                </h3>
                <p style="font-size: 9px; font-weight: 600; color: #64748b; margin: 2px 0 0 0; text-transform: uppercase; font-family: sans-serif; letter-spacing: 0.5px;">
                  BUNKERING PLAN
                </p>
              </td>
              <td style="width: 25%; padding: 6px; text-align: left; font-size: 8.5px; line-height: 1.3;">
                <div><strong>Mã biểu mẫu:</strong> <span style="font-family: monospace; font-weight: bold; color: #2563eb;">TL-15-01</span></div>
                <div><strong>Ngày ban hành:</strong> <span style="font-family: monospace;">20/10/2016</span></div>
                <div><strong>Lần sửa đổi:</strong> <span style="font-family: monospace;">0</span></div>
                <div><strong>Trang:</strong> 1 / 3</div>
              </td>
            </tr>
          </table>

          <!-- General Details -->
          <table style="width: 100%; border-collapse: collapse; border-left: 1px solid #cbd5e1; border-right: 1px solid #cbd5e1; border-bottom: 1px solid #cbd5e1; padding: 4px; background-color: #f8fafc; font-size: 10px;">
            <tr>
              <td style="width: 33%; padding: 4px 8px; border-right: 1px solid #e2e8f0;"><strong>Tàu / Vessel:</strong> ${vessel}</td>
              <td style="width: 33%; padding: 4px 8px; border-right: 1px solid #e2e8f0;"><strong>Vị trí / Location:</strong> ${location}</td>
              <td style="width: 34%; padding: 4px 8px;"><strong>Xà lan/Cảng / Supply Barge/Terminal:</strong> ${supplyBarge}</td>
            </tr>
            <tr>
              <td style="width: 33%; padding: 4px 8px; border-right: 1px solid #e2e8f0; border-top: 1px solid #e2e8f0;"><strong>Ngày nhận / Date:</strong> ${bunkerDate}</td>
              <td style="width: 33%; padding: 4px 8px; border-right: 1px solid #e2e8f0; border-top: 1px solid #e2e8f0;"><strong>Mớn nước mũi / Fore Draft:</strong> ${foreDraft}</td>
              <td style="width: 34%; padding: 4px 8px; border-top: 1px solid #e2e8f0;"><strong>Mớn nước lái / Aft Draft:</strong> ${aftDraft}</td>
            </tr>
          </table>

          <!-- Section 1 -->
          <div class="bunker-section-title">1. Loại nhiên liệu nhận / Product to be Handled</div>
          <table class="bunker-table">
            <thead>
              <tr>
                <th style="width: 5%;">STT</th>
                <th style="width: 25%;">Chủng loại / Grade</th>
                <th style="width: 12%;">Tỷ trọng / Density</th>
                <th style="width: 15%;">Lượng nhận / Stemmed Qty (Mts)</th>
                <th style="width: 15%;">Lượng có sẵn / Qty onboard (Mts)</th>
                <th style="width: 13%;">Thời gian dự kiến / Duration</th>
                <th style="width: 15%;">Lượng dự kiến hoàn thành / ROB</th>
              </tr>
            </thead>
            <tbody>
              ${products.length > 0 ? products.map((row, idx) => `
                <tr>
                  <td>${idx + 1}</td>
                  <td style="text-align: left; font-weight: bold;">${row.grade || ''}</td>
                  <td>${row.density || ''}</td>
                  <td>${row.stemmedQty || ''}</td>
                  <td>${row.qtyOnboard || ''}</td>
                  <td>${row.duration || ''}</td>
                  <td>${row.robCompletion || ''}</td>
                </tr>
              `).join('') : `<tr><td colspan="7" style="color: #64748b; font-style: italic;">Không có dữ liệu / No data</td></tr>`}
            </tbody>
          </table>

          <!-- Section 2 -->
          <div class="bunker-section-title">2. Trách nhiệm của thành viên tham gia / Responsibilities of Personnel</div>
          <table class="bunker-table">
            <thead>
              <tr>
                <th style="width: 5%;">STT</th>
                <th style="width: 25%;">Chức danh / Rank</th>
                <th style="width: 30%;">Họ tên / Name</th>
                <th style="width: 40%;">Nhiệm vụ cụ thể / Specific Duty</th>
              </tr>
            </thead>
            <tbody>
              ${personnel.length > 0 ? personnel.map((row, idx) => `
                <tr>
                  <td>${idx + 1}</td>
                  <td style="text-align: left; font-weight: bold;">${row.rank || ''}</td>
                  <td style="text-align: left;">${row.name || ''}</td>
                  <td style="text-align: left;">${row.duty || ''}</td>
                </tr>
              `).join('') : `<tr><td colspan="4" style="color: #64748b; font-style: italic;">Không có dữ liệu / No data</td></tr>`}
            </tbody>
          </table>

          <!-- Section 3 & 4 & 5 & 6 Checklists -->
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-top: 10px;">
            <div class="checklist-box">
              <div style="font-weight: bold; color: #1e293b; border-bottom: 1px solid #cbd5e1; padding-bottom: 3px; margin-bottom: 5px;">3. Thiết bị đo & Độ chính xác / Calibration & Accuracy</div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_pressure_gauge'])}
                <span>Áp kế nhận hàng đã hiệu chuẩn / Pressure gauge calibrated</span>
              </div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_sounding_tape'])}
                <span>Thước đo dầu có dán tem hiệu chuẩn / Sounding tape calibrated</span>
              </div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_calibration_table'])}
                <span>Bảng hiệu chuẩn két sẵn sàng / Tank calibration table ready</span>
              </div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_computer'])}
                <span>Máy tính tính toán đã kiểm tra / Calculation computer checked</span>
              </div>
            </div>

            <div class="checklist-box">
              <div style="font-weight: bold; color: #1e293b; border-bottom: 1px solid #cbd5e1; padding-bottom: 3px; margin-bottom: 5px;">4. Báo động mức cao / High-level alarms</div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_reset_alarm'])}
                <span>Đã reset và thử hoạt động tốt còi/đèn báo động mức cao (95% & 98%) / High level alarms tested & operational</span>
              </div>
              <div style="font-weight: bold; color: #1e293b; border-bottom: 1px solid #cbd5e1; padding-bottom: 3px; margin-top: 8px; margin-bottom: 5px;">5. Đo nhiệt độ / Temp measuring</div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_temp_sensor'])}
                <span>Cảm biến nhiệt độ tự động / Remote temp sensor</span>
              </div>
              <div class="checklist-item">
                ${renderCheckbox(metadata['chk_thermometer'])}
                <span>Đo bằng nhiệt kế cầm tay / Portable thermometer</span>
              </div>
            </div>
          </div>

          <div class="checklist-box" style="margin-top: 8px;">
            <div style="font-weight: bold; color: #1e293b; border-bottom: 1px solid #cbd5e1; padding-bottom: 3px; margin-bottom: 5px;">6. Kiểm soát hơi thoát và đo khí độc / Vapor control and gas check</div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
              <div>
                <div class="checklist-item">
                  ${renderCheckbox(metadata['chk_ventilate_sounding'])}
                  <span>Thông gió khu vực lỗ đo và hộp van xả / Ventilate sounding pipes</span>
                </div>
                <div class="checklist-item">
                  ${renderCheckbox(metadata['chk_h2s_benzene'])}
                  <span>Kiểm tra hàm lượng khí H2S và Benzene / H2S & Benzene concentration checked</span>
                </div>
              </div>
              <div>
                <div class="checklist-item">
                  ${renderCheckbox(metadata['chk_hc_detector'])}
                  <span>Máy đo khí cháy HC hoạt động tốt / HC gas detector calibrated</span>
                </div>
                <div class="checklist-item">
                  ${renderCheckbox(metadata['chk_respiratory_ready'])}
                  <span>Thiết bị hỗ trợ hô hấp sẵn sàng khẩn cấp / SCBA ready for emergency use</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- PAGE 2 -->
        <div style="page-break-after: always;">
          <!-- Header Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; text-align: center;">
            <tr>
              <td style="width: 25%; border-right: 1px solid #cbd5e1; padding: 6px;">
                <div style="font-size: 18px; margin-bottom: 1px;">⚓</div>
                <div style="font-size: 8px; font-weight: bold; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.5px;">HP SHIPPING</div>
              </td>
              <td style="width: 50%; border-right: 1px solid #cbd5e1; padding: 6px; background-color: #f8fafc;">
                <h3 style="font-size: 11px; font-weight: bold; margin: 0; text-transform: uppercase; color: #1e293b; line-height: 1.3;">
                  KẾ HOẠCH NHẬN NHIÊN LIỆU
                </h3>
                <p style="font-size: 9px; font-weight: 600; color: #64748b; margin: 2px 0 0 0; text-transform: uppercase; font-family: sans-serif; letter-spacing: 0.5px;">
                  BUNKERING PLAN
                </p>
              </td>
              <td style="width: 25%; padding: 6px; text-align: left; font-size: 8.5px; line-height: 1.3;">
                <div><strong>Mã biểu mẫu:</strong> <span style="font-family: monospace; font-weight: bold; color: #2563eb;">TL-15-01</span></div>
                <div><strong>Ngày ban hành:</strong> <span style="font-family: monospace;">20/10/2016</span></div>
                <div><strong>Lần sửa đổi:</strong> <span style="font-family: monospace;">0</span></div>
                <div><strong>Trang:</strong> 2 / 3</div>
              </td>
            </tr>
          </table>

          <!-- Section 7 -->
          <div class="bunker-section-title">7. Phân phối nhiên liệu và sơ đồ đường ống / Distribution of Bunker Oil and Pipe Line-up</div>
          <div style="background-color: #f8fafc; border: 1px solid #cbd5e1; padding: 6px; border-radius: 4px; font-size: 9px; margin-bottom: 6px; line-height: 1.3;">
            <strong>Mô tả thiết lập van nhận nhiên liệu / Piping line-up description:</strong><br/>
            <span style="font-family: sans-serif; color: #1e293b; white-space: pre-wrap;">${metadata['line_up_piping'] || 'Chưa thiết lập sơ đồ van / Piping diagram not set'}</span>
          </div>
          <table class="bunker-table">
            <thead>
              <tr>
                <th style="width: 5%;">STT</th>
                <th style="width: 20%;">Két nhận / Receiving Tank</th>
                <th style="width: 25%;">Các van mở / Valves opened</th>
                <th style="width: 25%;">Các van đóng cách ly / Isolated</th>
                <th style="width: 15%;">Két chứa tràn / Overflow Tank</th>
                <th style="width: 10%;">Van tràn mở</th>
              </tr>
            </thead>
            <tbody>
              ${distribution.length > 0 ? distribution.map((row, idx) => `
                <tr>
                  <td>${idx + 1}</td>
                  <td style="font-weight: bold;">${row.recvTank || ''}</td>
                  <td>${row.valveOpened || ''}</td>
                  <td>${row.valveClosed || ''}</td>
                  <td>${row.overflowTank || ''}</td>
                  <td>${row.overflowValveOpened || ''}</td>
                </tr>
              `).join('') : `<tr><td colspan="6" style="color: #64748b; font-style: italic;">Không có dữ liệu / No data</td></tr>`}
            </tbody>
          </table>

          <!-- Section 8 -->
          <div class="bunker-section-title">8. Lưu lượng nhận nhiên liệu dự kiến / Expected Loading Rates</div>
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; font-size: 9.5px; background-color: #f8fafc;">
            <tr style="text-align: center;">
              <td style="padding: 6px; border-right: 1px solid #cbd5e1; width: 33%;">
                <div style="color: #64748b; font-size: 8.5px;">Lưu lượng ban đầu / Initial Rate</div>
                <div style="font-size: 11px; font-weight: bold; color: #1e293b; margin-top: 2px;">${metadata['rate_initial'] || 'N/A'} Mts/Hr</div>
              </td>
              <td style="padding: 6px; border-right: 1px solid #cbd5e1; width: 33%;">
                <div style="color: #64748b; font-size: 8.5px;">Lưu lượng tối đa / Max Rate</div>
                <div style="font-size: 11px; font-weight: bold; color: #1e293b; margin-top: 2px;">${metadata['rate_max'] || 'N/A'} Mts/Hr</div>
              </td>
              <td style="padding: 6px; width: 34%;">
                <div style="color: #64748b; font-size: 8.5px;">Lưu lượng topping-off / Topping-off Rate</div>
                <div style="font-size: 11px; font-weight: bold; color: #1e293b; margin-top: 2px;">${metadata['rate_topping'] || 'N/A'} Mts/Hr</div>
              </td>
            </tr>
          </table>

          <!-- Section 9 -->
          <div class="bunker-section-title">9. Bảng đo các két trước và sau nhận / Gauging of Tanks</div>
          <table class="bunker-table" style="font-size: 8.5px !important;">
            <thead>
              <tr style="font-size: 8px !important;">
                <th rowspan="2" style="width: 4%;">STT</th>
                <th rowspan="2" style="width: 12%;">Két / Tank</th>
                <th colspan="2" style="width: 18%;">Dung tích 85%</th>
                <th colspan="3" style="width: 25%;">Đo trước nhận</th>
                <th colspan="2" style="width: 17%;">Dự kiến sau nhận</th>
                <th colspan="2" style="width: 16%;">Mức giảm tốc</th>
                <th rowspan="2" style="width: 8%;">Thứ tự / Seq</th>
              </tr>
              <tr style="font-size: 7.5px !important; background-color: #f8fafc;">
                <th>Sound (m)</th>
                <th>Vol (m³)</th>
                <th>Sound (m)</th>
                <th>Vol (m³)</th>
                <th>Temp (°C)</th>
                <th>Sound (m)</th>
                <th>Vol (m³)</th>
                <th>Sound (m)</th>
                <th>Vol (m³)</th>
              </tr>
            </thead>
            <tbody>
              ${gauging.length > 0 ? gauging.map((row, idx) => `
                <tr>
                  <td>${idx + 1}</td>
                  <td style="font-weight: bold;">${row.tank || ''}</td>
                  <td>${row.capSound || ''}</td>
                  <td style="font-weight: 550;">${row.capVol || ''}</td>
                  <td>${row.preSound || ''}</td>
                  <td style="font-weight: 550;">${row.preVol || ''}</td>
                  <td>${row.preTemp || ''}</td>
                  <td>${row.postSound || ''}</td>
                  <td style="font-weight: 550;">${row.postVol || ''}</td>
                  <td>${row.reduceSound || ''}</td>
                  <td style="font-weight: 550;">${row.reduceVol || ''}</td>
                  <td>${row.seq || ''}</td>
                </tr>
              `).join('') : `<tr><td colspan="12" style="color: #64748b; font-style: italic;">Không có dữ liệu / No data</td></tr>`}
            </tbody>
          </table>
          
          <!-- Sum Calculations -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; font-size: 8.5px; background-color: #f8fafc; margin-top: 4px; text-align: center;">
            <tr style="font-weight: bold; color: #475569;">
              <td style="padding: 5px; border-right: 1px solid #cbd5e1;">Tổng dung tích 85% / Total 85% Cap: <span style="color: #0f172a; font-family: monospace; font-size: 9.5px; margin-left: 2px;">${gauging.reduce((sum, r) => sum + (parseFloat(r.capVol) || 0), 0).toFixed(2)} m³</span></td>
              <td style="padding: 5px; border-right: 1px solid #cbd5e1;">Tổng trước nhận / Total Pre-bunkering: <span style="color: #0f172a; font-family: monospace; font-size: 9.5px; margin-left: 2px;">${gauging.reduce((sum, r) => sum + (parseFloat(r.preVol) || 0), 0).toFixed(2)} m³</span></td>
              <td style="padding: 5px;">Tổng sau nhận dự kiến / Total Final Expected: <span style="color: #0f172a; font-family: monospace; font-size: 9.5px; margin-left: 2px;">${gauging.reduce((sum, r) => sum + (parseFloat(r.postVol) || 0), 0).toFixed(2)} m³</span></td>
            </tr>
          </table>
        </div>

        <!-- PAGE 3 -->
        <div>
          <!-- Header Table -->
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; text-align: center;">
            <tr>
              <td style="width: 25%; border-right: 1px solid #cbd5e1; padding: 6px;">
                <div style="font-size: 18px; margin-bottom: 1px;">⚓</div>
                <div style="font-size: 8px; font-weight: bold; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.5px;">HP SHIPPING</div>
              </td>
              <td style="width: 50%; border-right: 1px solid #cbd5e1; padding: 6px; background-color: #f8fafc;">
                <h3 style="font-size: 11px; font-weight: bold; margin: 0; text-transform: uppercase; color: #1e293b; line-height: 1.3;">
                  KẾ HOẠCH NHẬN NHIÊN LIỆU
                </h3>
                <p style="font-size: 9px; font-weight: 600; color: #64748b; margin: 2px 0 0 0; text-transform: uppercase; font-family: sans-serif; letter-spacing: 0.5px;">
                  BUNKERING PLAN
                </p>
              </td>
              <td style="width: 25%; padding: 6px; text-align: left; font-size: 8.5px; line-height: 1.3;">
                <div><strong>Mã biểu mẫu:</strong> <span style="font-family: monospace; font-weight: bold; color: #2563eb;">TL-15-01</span></div>
                <div><strong>Ngày ban hành:</strong> <span style="font-family: monospace;">20/10/2016</span></div>
                <div><strong>Lần sửa đổi:</strong> <span style="font-family: monospace;">0</span></div>
                <div><strong>Trang:</strong> 3 / 3</div>
              </td>
            </tr>
          </table>

          <!-- Section 10 -->
          <div class="bunker-section-title">10. Thông tin liên lạc & Ngắt khẩn cấp / Communications & Emergency Stop</div>
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; font-size: 9.5px; background-color: #f8fafc;">
            <tr>
              <td style="padding: 6px; border-right: 1px solid #cbd5e1; width: 50%;">
                <strong>Phương thức liên lạc / Comm method:</strong> ${metadata['comm_ship_barge'] || 'N/A'}
              </td>
              <td style="padding: 6px; width: 50%;">
                <strong>Tín hiệu dừng khẩn cấp / Emergency stop signal:</strong> ${metadata['comm_stop_signal'] || 'N/A'}
              </td>
            </tr>
          </table>

          <!-- Section 11 -->
          <div class="bunker-section-title">11. Các địa chỉ liên lạc khẩn cấp tại địa phương / Local Emergency Contacts</div>
          <div style="background-color: #f8fafc; border: 1px solid #cbd5e1; padding: 6px; border-radius: 4px; font-size: 9px; line-height: 1.3;">
            <span style="font-family: sans-serif; color: #1e293b; white-space: pre-wrap;">${metadata['local_contacts'] || 'Chưa cập nhật / Not updated'}</span>
          </div>

          <!-- Section 12 -->
          <div class="bunker-section-title">12. Thiết bị ứng phó sự cố dầu tràn / Oil Spill Equipment Location</div>
          <table style="width: 100%; border-collapse: collapse; border: 1px solid #cbd5e1; font-size: 9.5px; background-color: #f8fafc;">
            <tr>
              <td style="padding: 6px; border-right: 1px solid #cbd5e1; width: 50%;">
                <strong>Vị trí SOPEP tàu / Ship's SOPEP Box:</strong> ${metadata['spill_loc1'] || 'N/A'}
              </td>
              <td style="padding: 6px; width: 50%;">
                <strong>Vị trí thiết bị xà lan / Barge SOPEP Box:</strong> ${metadata['spill_loc2'] || 'N/A'}
              </td>
            </tr>
          </table>

          <!-- Section 13 -->
          <div class="bunker-section-title">13. Xác nhận hiểu rõ kế hoạch / Crew Sign-off List</div>
          <p style="font-size: 8.5px; font-style: italic; color: #64748b; margin-top: 0; margin-bottom: 4px;">
            Chúng tôi xác nhận đã hiểu rõ kế hoạch nhận nhiên liệu và các biện pháp ứng phó sự cố dầu tràn. / We confirm that we understand the bunkering plan.
          </p>
          <table class="bunker-table">
            <thead>
              <tr>
                <th style="width: 5%;">STT</th>
                <th style="width: 25%;">Chức danh / Rank</th>
                <th style="width: 35%;">Họ tên / Name</th>
                <th style="width: 35%;">Ký tên / Signature</th>
              </tr>
            </thead>
            <tbody>
              ${signaturesList.length > 0 ? signaturesList.map((row, idx) => `
                <tr>
                  <td>${idx + 1}</td>
                  <td style="font-weight: bold; text-align: left;">${row.rank || ''}</td>
                  <td style="text-align: left;">${row.name || ''}</td>
                  <td>
                    ${row.signed 
                      ? `<span style="display: inline-block; background-color: #f0fdf4; color: #16a34a; font-weight: bold; border: 1px solid #bbf7d0; padding: 2px 6px; rounded: 3px; font-size: 8px;">✓ ĐÃ KÝ / SIGNED</span>` 
                      : `<span style="color: #94a3b8; font-style: italic;">Chưa ký / Not signed</span>`}
                  </td>
                </tr>
              `).join('') : `<tr><td colspan="4" style="color: #64748b; font-style: italic;">Không có dữ liệu / No data</td></tr>`}
            </tbody>
          </table>
        </div>
      `;
    } else {
      const sortedEntries = Object.entries(metadata);
      bodyContent = `
        <div class="form-title-section">
          <h2 style="font-size: 18px; font-weight: bold; margin-bottom: 5px;">${title}</h2>
          <p style="font-size: 11px; color: #64748b; margin-top: 0; margin-bottom: 20px;">Mẫu biểu kỹ thuật số đã kiểm soát - SMS System</p>
        </div>
        
        <h3 style="font-size: 13px; font-weight: bold; text-transform: uppercase; border-bottom: 1px solid #000; padding-bottom: 4px; margin-top: 20px;">I. THÔNG TIN CHUNG (GENERAL INFORMATION)</h3>
        <table class="form-info-table">
          <tr>
            <td><strong>Tên tàu thực thi:</strong> ${metadata['Tên tàu'] || metadata['Tàu thực thi'] || 'N/A'}</td>
            <td><strong>Quy trình liên kết:</strong> ${metadata['Quy trình liên kết'] || 'N/A'}</td>
          </tr>
          <tr>
            <td><strong>Người lập hồ sơ:</strong> ${metadata['Người lập'] || metadata['Người lập hồ sơ'] || 'N/A'}</td>
            <td><strong>Ngày ghi nhận:</strong> ${metadata['Ngày lập'] || metadata['Ngày khởi tạo'] || 'N/A'}</td>
          </tr>
        </table>

        <h3 style="font-size: 13px; font-weight: bold; text-transform: uppercase; border-bottom: 1px solid #000; padding-bottom: 4px; margin-top: 25px;">II. NỘI DUNG BIỂU MẪU CHI TIẾT (FORM DATA DETAILS)</h3>
        <table class="form-data-table">
          <thead>
            <tr>
              <th style="width: 5%;">TT</th>
              <th style="width: 65%;">Nội dung kiểm tra / Chỉ mục đánh giá</th>
              <th style="width: 30%; text-align: center;">Trạng thái ghi nhận</th>
            </tr>
          </thead>
          <tbody>
            ${sortedEntries
              .filter(([key]) => !['Tàu thực thi', 'Người lập', 'Ngày lập', 'Mã biểu mẫu', 'Quy trình liên kết', 'Tên tàu', 'Người lập hồ sơ', 'Ngày khởi tạo', 'vesselName', 'filledBy', 'filledDate', 'procedureCode', 'formCode', 'formTitle', 'shipName', 'shipType', 'masterName', 'reviewDate'].includes(key))
              .map(([key, val], idx) => {
                const cleanedKey = key
                  .replace('chk_', '')
                  .replace('chk', '')
                  .replace('prep_', '')
                  .replace('gas_', '')
                  .replace(/([A-Z])/g, ' $1')
                  .replace(/^./, str => str.toUpperCase());
                
                let displayVal = String(val);
                if (typeof val === 'boolean') {
                  displayVal = val 
                    ? '<span class="status-yes">✓ Có / Đạt (Yes/Pass)</span>' 
                    : '<span class="status-no">✗ Không / Chưa đạt (No/Fail)</span>';
                } else if (val === 'true') {
                  displayVal = '<span class="status-yes">✓ Có / Đạt (Yes/Pass)</span>';
                } else if (val === 'false') {
                  displayVal = '<span class="status-no">✗ Không / Chưa đạt (No/Fail)</span>';
                }

                return `
                  <tr>
                    <td style="text-align: center;">${idx + 1}</td>
                    <td><strong>${cleanedKey}</strong></td>
                    <td style="text-align: center; font-weight: 600;">${displayVal}</td>
                  </tr>
                `;
              }).join('')}
          </tbody>
        </table>
      `;
    }
  }

  // 3. Build signatures HTML
  let sigsHtml = '';
  if (signatures && signatures.length > 0) {
    sigsHtml = `
      <div class="signatures-container">
        ${signatures.map(sig => `
          <div class="signature-stamp">
            <div class="sig-rank">${sig.rank || 'Người lập biểu'}</div>
            <div class="sig-stamp">
              <span class="stamp-check">✓</span>
              <span class="stamp-text">ĐÃ KÝ SỐ</span>
            </div>
            <div class="sig-name">${sig.name || ''}</div>
            <div class="sig-code">${sig.sigCode || ''}</div>
            <div class="sig-date">${sig.timestamp ? sig.timestamp.replace('T', ' ').substring(0, 16) + ' UTC' : ''}</div>
          </div>
        `).join('')}
      </div>
    `;
  }

  // 4. Construct the complete HTML
  const finalHtml = `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8" />
  <title>${code ? code + ' - ' : ''}${title}</title>
  <style>
    @page {
      size: A4;
      margin: 20mm 15mm 18mm 15mm;
    }
    
    body {
      font-family: "Times New Roman", Times, Georgia, serif;
      font-size: 13.5px;
      line-height: 1.6;
      color: #0f172a;
      background-color: #fff;
      margin: 0;
      padding: 0;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }

    /* Page Header */
    .page-header-fixed {
      position: fixed;
      top: -15mm;
      left: 0;
      right: 0;
      height: 12mm;
      border-bottom: 2px solid #0f172a;
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      padding-bottom: 4px;
      z-index: 9999;
    }

    .header-left {
      font-weight: bold;
      font-size: 10px;
      text-transform: uppercase;
      font-family: Arial, sans-serif;
    }

    .header-right {
      font-size: 9px;
      text-align: right;
      font-family: monospace;
      color: #475569;
    }

    /* Page Footer */
    .page-footer-fixed {
      position: fixed;
      bottom: -12mm;
      left: 0;
      right: 0;
      height: 10mm;
      border-top: 1px solid #94a3b8;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 8.5px;
      color: #64748b;
      font-family: Arial, sans-serif;
      z-index: 9999;
    }

    /* Layout Spacing */
    .header-spacer {
      height: 15mm;
    }

    .footer-spacer {
      height: 12mm;
    }

    .print-layout-table {
      width: 100%;
      border-collapse: collapse;
    }

    .document-body {
      text-align: justify;
    }

    /* Watermark */
    .watermark-container {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%) rotate(-35deg);
      font-size: 44px;
      font-weight: 900;
      color: rgba(30, 41, 59, 0.05);
      border: 6px dashed rgba(30, 41, 59, 0.05);
      padding: 12px 24px;
      text-transform: uppercase;
      letter-spacing: 4px;
      white-space: nowrap;
      pointer-events: none;
      z-index: -1000;
      text-align: center;
      font-family: Arial, sans-serif;
    }

    /* Headings */
    h1, h2, h3, h4 {
      font-family: Arial, sans-serif;
      color: #0f172a;
      margin-top: 18px;
      margin-bottom: 8px;
      page-break-after: avoid;
    }

    h1 {
      font-size: 19px;
      text-align: center;
      margin-top: 0;
      margin-bottom: 22px;
      text-transform: uppercase;
    }

    h2 {
      font-size: 14.5px;
      border-bottom: 1px solid #94a3b8;
      padding-bottom: 2px;
      margin-top: 24px;
    }

    h3 {
      font-size: 13px;
    }

    p {
      margin-top: 8px;
      margin-bottom: 8px;
    }

    /* Converted word tables */
    table:not(.print-layout-table) {
      width: 100% !important;
      border-collapse: collapse !important;
      margin: 16px 0 !important;
      font-size: 12.5px !important;
    }

    table:not(.print-layout-table) th,
    table:not(.print-layout-table) td {
      border: 1px solid #475569 !important;
      padding: 6px 10px !important;
      vertical-align: top;
      word-break: break-word;
    }

    table:not(.print-layout-table) th {
      background-color: #f1f5f9 !important;
      font-weight: bold;
      color: #0f172a;
      text-transform: uppercase;
      font-size: 11.5px;
      font-family: Arial, sans-serif;
    }

    tr {
      page-break-inside: avoid;
    }

    /* Lists */
    ol, ul {
      margin-top: 8px;
      margin-bottom: 8px;
      padding-left: 20px;
    }

    li {
      margin-bottom: 4px;
    }

    /* Metadata Grid */
    .metadata-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 6px;
      background-color: #f8fafc;
      border: 1px solid #cbd5e1;
      padding: 10px 14px;
      border-radius: 6px;
      margin-bottom: 24px;
      font-size: 12px;
    }

    .metadata-item {
      display: flex;
    }

    .metadata-label {
      color: #475569;
      font-weight: bold;
      width: 110px;
      flex-shrink: 0;
    }

    .metadata-value {
      color: #0f172a;
    }

    /* General Form Styling */
    .form-info-table {
      width: 100% !important;
      border-collapse: collapse;
      margin-top: 8px !important;
      margin-bottom: 16px !important;
    }

    .form-info-table td {
      border: none !important;
      padding: 4px 0 !important;
      width: 50%;
      font-size: 13px;
    }

    .form-data-table {
      width: 100% !important;
      border-collapse: collapse !important;
      margin-top: 8px !important;
    }

    .form-data-table th, .form-data-table td {
      border: 1px solid #475569 !important;
      padding: 6px 10px !important;
    }

    .form-data-table th {
      background-color: #f1f5f9 !important;
      font-family: Arial, sans-serif;
      font-size: 11.5px;
      font-weight: bold;
    }

    .status-yes {
      color: #047857;
      font-family: Arial, sans-serif;
    }

    .status-no {
      color: #b91c1c;
      font-family: Arial, sans-serif;
    }

    /* Signature Stamps */
    .signatures-container {
      display: flex;
      justify-content: space-around;
      margin-top: 35px;
      page-break-inside: avoid;
    }

    .signature-stamp {
      width: 190px;
      text-align: center;
      font-size: 11px;
      border: 1px dashed #94a3b8;
      padding: 8px;
      border-radius: 6px;
      background-color: rgba(16, 185, 129, 0.01);
    }

    .sig-rank {
      color: #475569;
      font-weight: bold;
      margin-bottom: 4px;
      font-family: Arial, sans-serif;
    }

    .sig-stamp {
      border: 2px solid #059669;
      color: #059669;
      font-weight: bold;
      font-size: 9.5px;
      display: inline-block;
      padding: 1px 6px;
      border-radius: 3px;
      transform: rotate(-2deg);
      margin: 6px 0;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-family: Arial, sans-serif;
    }

    .stamp-check {
      font-size: 11px;
      margin-right: 3px;
    }

    .sig-name {
      font-weight: bold;
      color: #0f172a;
    }

    .sig-code {
      font-family: monospace;
      color: #64748b;
      font-size: 9px;
      margin-top: 2px;
    }

    .sig-date {
      color: #94a3b8;
      font-size: 8.5px;
      margin-top: 1px;
    }
  </style>
</head>
<body>

  <!-- Dynamic Fixed Header -->
  <div class="page-header-fixed">
    <div class="header-left">${subtitle}</div>
    <div class="header-right">${code ? code + ' | ' : ''}Rev ${version || '1.0'}</div>
  </div>

  <!-- Dynamic Fixed Footer -->
  <div class="page-footer-fixed">
    <div>HỆ THỐNG QUẢN LÝ AN TOÀN HÀNG HẢI - TÀI LIỆU ĐÃ KIỂM SOÁT</div>
    <div>MARITIME EDGE SYSTEM</div>
  </div>

  <!-- Watermark -->
  ${watermark ? `<div class="watermark-container">${watermark}</div>` : ''}

  <!-- Outer Print Layout Table -->
  <table class="print-layout-table">
    <thead>
      <tr>
        <td>
          <div class="header-spacer"></div>
        </td>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>
          <div class="document-body">
            ${isForm ? '' : `<h1>${title}</h1>`}
            ${metaHtml}
            ${bodyContent}
            ${sigsHtml}
          </div>
        </td>
      </tr>
    </tbody>
    <tfoot>
      <tr>
        <td>
          <div class="footer-spacer"></div>
        </td>
      </tr>
    </tbody>
  </table>

  <script>
    window.onload = function() {
      // Trigger printing after styles and content load
      setTimeout(function() {
        window.print();
      }, 300);
    };
  </script>
</body>
</html>`;

  printWindow.document.write(finalHtml);
  printWindow.document.close();
}
