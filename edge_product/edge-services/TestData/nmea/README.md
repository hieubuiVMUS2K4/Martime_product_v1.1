# NMEA Test Data Playback

Thư mục này chứa dữ liệu mô phỏng GPS chuẩn NMEA 0183 nhằm mục đích kiểm thử hệ thống định vị của Vessel Edge (vị trí tàu, hướng mũi tàu, vận tốc tàu).

## File Dữ Liệu
* **vietnam-coastal-route.nmea**: File chứa tuyến đường chạy dọc bờ biển Việt Nam, kéo dài từ Vũng Tàu ra Hải Phòng. File được tự động sinh bởi công cụ generator.

## Cách Hoạt Động Của NMEA Playback
Background Service `NmeaPlaybackService` đã được thêm vào hệ thống `Edge`. Service này sẽ đọc file NMEA từng khoảng thời gian (mặc định 1 giây), xử lý các bản tin qua `NmeaParser`, sau đó lưu thông tin toạ độ thực tế vào Edge DB (bảng `PositionData`, v.v.).

Để sử dụng luồng này, bạn cấu hình cờ `NmeaPlayback:Enabled` trong `appsettings.json` bằng `true`. (Cấu hình này đã được bật mặc định trong quá trình phát triển). Khi cờ này kích hoạt, file `vietnam-coastal-route.nmea` sẽ được phát lại liên tục.

```json
  "NmeaPlayback": {
    "Enabled": true,
    "FilePath": "./TestData/nmea/vietnam-coastal-route.nmea",
    "IntervalSeconds": 1,
    "LoopRoute": true
  }
```

## Tái Tạo Dữ Liệu
Trong quá trình kiểm thử nếu bạn muốn tái tạo (generate) file toạ độ NMEA khác, có thể dùng script trong dự án (chạy tại thư mục `edge-services`):

```bash
npm run generate:nmea:vietnam
```

Bạn cũng có thể thay đổi tham số để tăng tốc độ chạy tàu hoặc khoảng thời gian bắt đầu chạy. Xem cấu hình tuỳ chọn trong script thông qua lệnh:
```bash
node ./Scripts/generate-vietnam-route-nmea.js --help
```