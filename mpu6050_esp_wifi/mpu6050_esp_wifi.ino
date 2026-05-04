/*
 * MPU6050 → Edge Dashboard (WiFi)
 * 
 * Đọc dữ liệu Pitch & Roll từ MPU6050, gửi lên Edge Backend qua HTTP POST.
 * Edge backend: http://<EDGE_IP>:5001/api/telemetry/navigation
 * 
 * Hardware:
 *   - ESP32 (ESP32 Dev Module)
 *   - MPU6050 (I2C: SDA→GPIO21, SCL→GPIO22)
 * 
 * Libraries cần cài (qua Arduino Library Manager):
 *   - Adafruit MPU6050
 *   - Adafruit Sensor
 *   - ArduinoJson (v7)
 * 
 * Cấu hình:
 *   1. Sửa WIFI_SSID, WIFI_PASSWORD
 *   2. Sửa EDGE_API_URL (IP của máy chạy Edge backend)
 *   3. Nạp code và mở Serial Monitor (115200 baud)
 */

#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <math.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

// ======================== CẤU HÌNH ========================

// --- WiFi ---
const char* WIFI_SSID     = "tinhvdth";
const char* WIFI_PASSWORD = "123456789tt";

// --- Edge Backend ---
// Địa chỉ IP máy chạy Edge backend (cổng mặc định 5001)
// Ví dụ: "http://192.168.1.100:5001"
const char* EDGE_API_URL  = "http://192.168.1.3:5001";
const char* API_ENDPOINT  = "/api/telemetry/navigation";

// --- Cảm biến ---
// Khoảng thời gian gửi dữ liệu (milliseconds)
const unsigned long SEND_INTERVAL_MS = 1000;  // 2 giây/lần

// ======================== KHAI BÁO ========================

Adafruit_MPU6050 mpu;

// Bộ lọc bù (Complementary Filter)
float pitch = 0;
float roll = 0;
unsigned long last_sensor_time = 0;
unsigned long last_send_time = 0;

// Đếm số lần gửi
unsigned long send_count = 0;
unsigned long fail_count = 0;

// ======================== SETUP ========================

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("\n========================================");
  Serial.println("  MPU6050 → Edge Dashboard (WiFi)");
  Serial.println("========================================");

  // --- Khởi tạo MPU6050 ---
  if (!mpu.begin()) {
    Serial.println("[LỖI] Không tìm thấy MPU6050! Kiểm tra kết nối I2C.");
    while (1) { delay(10); }
  }
  Serial.println("[OK] MPU6050 khởi tạo thành công");

  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  last_sensor_time = millis();

  // --- Kết nối WiFi ---
  connectToWiFi();
}

// ======================== LOOP ========================

void loop() {
  // 1. Đọc cảm biến và tính Pitch/Roll
  readAndFilterSensor();

  // 2. Gửi dữ liệu lên Edge Backend theo chu kỳ
  if (millis() - last_send_time >= SEND_INTERVAL_MS) {
    last_send_time = millis();

    // Lấy giá trị cuối cùng sau khi trừ bì (zeroing)
    float final_pitch = pitch - 0.68;
    float final_roll  = roll - 0.11;

    // In ra Serial để debug
    Serial.print("[DATA] Pitch: ");
    Serial.print(final_pitch, 2);
    Serial.print("°\tRoll: ");
    Serial.println(final_roll, 2);

    // Gửi lên Edge
    sendToEdge(final_pitch, final_roll);
  }

  delay(20);  // Tần số đọc cảm biến ~50Hz
}

// ======================== KẾT NỐI WIFI ========================

void connectToWiFi() {
  Serial.print("[WiFi] Đang kết nối tới ");
  Serial.println(WIFI_SSID);

  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 40) {
    delay(500);
    Serial.print(".");
    attempts++;
  }

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\n[OK] WiFi đã kết nối!");
    Serial.print("    IP: ");
    Serial.println(WiFi.localIP());
  } else {
    Serial.println("\n[LỖI] Không thể kết nối WiFi!");
    Serial.println("    ESP sẽ chạy ở chế độ offline (chỉ in Serial)");
    Serial.println("    Sẽ thử kết nối lại sau mỗi 30 giây.");
  }
}

void checkWiFiConnection() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("[WiFi] Mất kết nối! Đang thử kết nối lại...");
    WiFi.disconnect();
    WiFi.reconnect();

    int attempts = 0;
    while (WiFi.status() != WL_CONNECTED && attempts < 20) {
      delay(500);
      attempts++;
    }

    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("[OK] WiFi đã kết nối lại!");
    } else {
      Serial.println("[LỖI] Không thể kết nối lại WiFi.");
    }
  }
}

// ======================== ĐỌC & LỌC CẢM BIẾN ========================

void readAndFilterSensor() {
  unsigned long current_time = millis();
  float dt = (current_time - last_sensor_time) / 1000.0;
  if (dt <= 0 || dt > 1.0) dt = 0.02;  // Tránh chia 0 hoặc dt quá lớn
  last_sensor_time = current_time;

  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // Hiệu chuẩn (calibration offsets - dựa trên giá trị đo được)
  float real_accel_x = a.acceleration.x - (-2.32);
  float real_accel_y = a.acceleration.y - (-0.75);
  float real_accel_z = a.acceleration.z - 0.87;

  float real_gyro_x = g.gyro.x - (-0.01);
  float real_gyro_y = g.gyro.y - 0.11;

  // Góc từ gia tốc kế (bị nhiễu khi rung lắc)
  float accel_pitch = atan2(real_accel_x,
    sqrt(real_accel_y * real_accel_y + real_accel_z * real_accel_z)) * 180.0 / PI;
  float accel_roll = atan2(real_accel_y,
    sqrt(real_accel_x * real_accel_x + real_accel_z * real_accel_z)) * 180.0 / PI;

  // Tốc độ góc từ gyro (độ/giây)
  float gyro_pitch_rate = real_gyro_y * 180.0 / PI;
  float gyro_roll_rate  = real_gyro_x * 180.0 / PI;

  // Bộ lọc bù (Complementary Filter)
  pitch = 0.96 * (pitch + gyro_pitch_rate * dt) + 0.04 * accel_pitch;
  roll  = 0.96 * (roll  + gyro_roll_rate  * dt) + 0.04 * accel_roll;
}

// ======================== GỬI LÊN EDGE BACKEND ========================

void sendToEdge(float pitch_val, float roll_val) {
  // Kiểm tra kết nối WiFi
  if (WiFi.status() != WL_CONNECTED) {
    checkWiFiConnection();
    if (WiFi.status() != WL_CONNECTED) {
      fail_count++;
      return;
    }
  }
  WiFiClient client; // Khai báo thêm đối tượng WiFiClient
  HTTPClient http;
  String url = String(EDGE_API_URL) + API_ENDPOINT;
  http.begin(client, url); // Truyền thêm 'client' vào đây
  http.addHeader("Content-Type", "application/json");
  http.addHeader("X-Source", "esp-mpu6050");

  // Tạo JSON payload
  // Dùng ArduinoJson v7
  JsonDocument doc;
  doc["pitch"] = pitch_val;
  doc["roll"] = roll_val;
  doc["headingTrue"] = 0;            // Không có la bàn
  doc["headingMagnetic"] = 0;
  doc["speedThroughWater"] = 0;
  doc["depth"] = 0;

  String jsonString;
  serializeJson(doc, jsonString);

  Serial.print("[HTTP] Gửi dữ liệu... ");
  int httpCode = http.POST(jsonString);

  if (httpCode > 0) {
    Serial.print("Mã phản hồi: ");
    Serial.println(httpCode);
    if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_CREATED) {
      send_count++;
    } else {
      String response = http.getString();
      Serial.print("    Lỗi: ");
      Serial.println(response.substring(0, 120));
      fail_count++;
    }
  } else {
    Serial.print("Gửi thất bại! Lỗi: ");
    Serial.println(http.errorToString(httpCode).c_str());
    fail_count++;
  }

  http.end();
}

// ======================== THÔNG TIN BỔ SUNG ========================

/*
 * Cấu hình phần cứng:
 *   ESP32       MPU6050
 *   GPIO21  →   SDA
 *   GPIO22  →   SCL
 *   3.3V    →   VCC
 *   GND     →   GND
 *
 * Kiểm tra:
 *   1. Mở Serial Monitor với baud 115200
 *   2. Xem log kết nối WiFi
 *   3. Kiểm tra dữ liệu gửi lên Edge
 *      - Edge log: "POST /api/telemetry/navigation" → 200 OK
 *      - Dashboard: Xem Pitch/Roll trên giao diện
 * 
 * API Edge Backend:
 *   POST http://<ip>:5001/api/telemetry/navigation
 *   Body: { "pitch": 1.23, "roll": -0.45, ... }
 *
 * Xem thống kê:
 *   Serial sẽ hiển thị:
 *     [STAT] Đã gửi: 50 | Thất bại: 2 | Tỉ lệ: 96%
 *   sau mỗi 10 lần gửi.
 */
