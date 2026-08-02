export interface CleaningScheduleItem {
  num: string;
  name: string;
  cycle: string;
  method: string;
  note: string;
}

export interface CleaningScheduleCategory {
  title: string;
  prefix: string;
  items: CleaningScheduleItem[];
}

export const cleaningCategories: CleaningScheduleCategory[] = [
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
