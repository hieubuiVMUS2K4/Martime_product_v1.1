#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <math.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

// ======================== CẤU HÌNH ========================
const char* WIFI_SSID     = "tinhvdth";
const char* WIFI_PASSWORD = "123456789tt";
const char* EDGE_API_URL  = "http://192.168.0.157:5001";
const char* API_ENDPOINT  = "/api/telemetry/navigation";

// --- Cấu hình chân Pin (ESP8266) ---
#define L298N_ENA  15  // D8 (PWM)
#define L298N_IN1  16  // D0
#define L298N_IN2  13  // D7
#define ENC_A      14  // D5
#define ENC_B      12  // D6

// Chuyển nút Bật/Tắt sang D4, bỏ D3
#define BTN_POWER  2   // D4

// --- Thông số hệ thống ---
const unsigned long SEND_INTERVAL_MS = 1000;
const float PULSES_PER_REV = 330.0; 

// ======================== BIẾN TOÀN CỤC ========================
Adafruit_MPU6050 mpu;
float pitch = 0, roll = 0;
unsigned long last_sensor_time = 0;
unsigned long last_send_time = 0;

// Biến trạng thái motor
volatile bool isRunning = false;
volatile unsigned long last_debounce_power = 0;
volatile long pulse_count = 0;

int motorSpeed = 255; 
float current_rpm = 0;

// ======================== CÁC HÀM NGẮT (ISR) ========================

// Ngắt đọc Encoder
void IRAM_ATTR encoder_isr() {
  if (digitalRead(ENC_B) == LOW) pulse_count++;
  else pulse_count--;
}

// Ngắt nút Bật/Tắt trên chân D4
void IRAM_ATTR power_isr() {
  unsigned long now = millis();
  // Chống nhiễu nút nhấn (Debounce) 250ms
  if (now - last_debounce_power > 250) {
    isRunning = !isRunning;
    last_debounce_power = now;
  }
}

// ======================== KHỞI TẠO (SETUP) ========================

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("\n--- HE THONG KHOI TẠO (NUT D4: ON/OFF) ---");

  // Cấu hình Motor
  analogWriteRange(255);
  pinMode(L298N_ENA, OUTPUT);
  pinMode(L298N_IN1, OUTPUT);
  pinMode(L298N_IN2, OUTPUT);
  
  // Cấu hình Nút bấm trên chân D4 với ngắt
  pinMode(BTN_POWER, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(BTN_POWER), power_isr, FALLING);

  // Cấu hình Encoder với ngắt
  pinMode(ENC_A, INPUT_PULLUP);
  pinMode(ENC_B, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(ENC_A), encoder_isr, RISING);

  // Khởi tạo cảm biến MPU6050
  if (!mpu.begin()) {
    Serial.println("[LOI] Khong tim thay MPU6050!");
    while (1) delay(10);
  }
  
  last_sensor_time = millis();
  connectToWiFi();
}

// ======================== VÒNG LẶP (LOOP) ========================
void loop() {
  // ƯU TIÊN 1: Luôn cập nhật trạng thái motor ngay đầu vòng lặp
  updateMotor(); 
  
  // ƯU TIÊN 2: Đọc cảm biến liên tục
  readAndFilterSensor();

  // Xử lý gửi dữ liệu telemetry
  if (millis() - last_send_time >= SEND_INTERVAL_MS) {
    last_send_time = millis();
    
    // Tính toán RPM
    noInterrupts();
    long pulses = pulse_count;
    pulse_count = 0;
    interrupts();
    float dt = SEND_INTERVAL_MS / 1000.0;
    current_rpm = (pulses / PULSES_PER_REV) * (60.0 / dt);

    // In ra Serial để bạn theo dõi
    Serial.printf("[DEBUG] P: %.2f | RPM: %.1f | Motor: %s\n", 
                  pitch - 0.68, current_rpm, isRunning ? "ON" : "OFF");

    // CHỈ GỬI DỮ LIỆU NẾU ĐÃ CÓ WIFI
    // Nếu không có WiFi, lệnh này sẽ bị bỏ qua, không gây treo máy
    if (WiFi.status() == WL_CONNECTED) {
      sendToEdge(pitch - 0.68, roll - 0.11, current_rpm);
    } else {
      Serial.println("[WiFi] Khong co ket noi - Dang chay offline...");
      // Thử kết nối lại một cách lặng lẽ, không dùng vòng lặp while gây treo
      WiFi.begin(WIFI_SSID, WIFI_PASSWORD); 
    }
  }
  
  delay(10); 
}

// ======================== HÀM BỔ TRỢ ========================

void updateMotor() {
  if (!isRunning) {
    analogWrite(L298N_ENA, 0);
    digitalWrite(L298N_IN1, LOW);
    digitalWrite(L298N_IN2, LOW);
    return;
  }

  // Luôn quay cố định một chiều (IN1: HIGH, IN2: LOW)
  digitalWrite(L298N_IN1, HIGH);
  digitalWrite(L298N_IN2, LOW);
  analogWrite(L298N_ENA, motorSpeed);
}

void readAndFilterSensor() {
  unsigned long current_time = millis();
  float dt = (current_time - last_sensor_time) / 1000.0;
  if (dt <= 0 || dt > 1.0) dt = 0.02;
  last_sensor_time = current_time;

  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // Tính Pitch/Roll từ gia tốc kế
  float accel_pitch = atan2(a.acceleration.x, sqrt(a.acceleration.y * a.acceleration.y + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;
  float accel_roll = atan2(a.acceleration.y, sqrt(a.acceleration.x * a.acceleration.x + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;

  // Bộ lọc bù (Complementary Filter)
  pitch = 0.96 * (pitch + (g.gyro.y * 180.0 / PI) * dt) + 0.04 * accel_pitch;
  roll  = 0.96 * (roll  + (g.gyro.x * 180.0 / PI) * dt) + 0.04 * accel_roll;
}
// Cập nhật lại hàm gửi dữ liệu để tránh chờ đợi quá lâu
void sendToEdge(float pitch_val, float roll_val, float speed_val) {
  WiFiClient client;
  HTTPClient http;
  
  // Thiết lập thời gian chờ (timeout) ngắn (ví dụ 200ms) 
  // để nếu server lỗi thì thoát ra ngay
  http.setTimeout(200); 
  
  String url = String(EDGE_API_URL) + API_ENDPOINT;
  if (http.begin(client, url)) {
    http.addHeader("Content-Type", "application/json");

    JsonDocument doc;
    doc["pitch"] = pitch_val;
    doc["roll"] = roll_val;
    doc["speed"] = speed_val;

    String jsonString;
    serializeJson(doc, jsonString);

    // Gửi POST
    int httpCode = http.POST(jsonString);
    
    if (httpCode > 0) {
      // Serial.printf("[HTTP] Code: %d\n", httpCode);
    }
    http.end();
  }
}
// Chỉnh lại hàm connectToWiFi để không bị kẹt vĩnh viễn lúc khởi động
void connectToWiFi() {
  Serial.print("[WiFi] Dang ket noi");
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  int timeout = 0;
  // Chỉ chờ tối đa 10 giây, nếu không được thì bỏ qua để vào loop chạy motor
  while (WiFi.status() != WL_CONNECTED && timeout < 20) {
    delay(500);
    Serial.print(".");
    timeout++;
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\n[OK] WiFi Connected!");
  } else {
    Serial.println("\n[TIMEOUT] WiFi kẹt - Chạy chế độ Offline.");
  }
}