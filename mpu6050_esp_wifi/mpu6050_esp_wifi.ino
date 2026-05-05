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
const char* EDGE_API_URL  = "http://192.168.0.157:5001";
const char* API_ENDPOINT  = "/api/telemetry/navigation";

// --- Pin cho Motor L298N (ESP8266) ---
#define L298N_ENA  15  // D8 (PWM)
#define L298N_IN1  16  // D0
#define L298N_IN2  13  // D7

// --- Pin cho Encoder ---
#define ENC_A      14  // D5
#define ENC_B      12  // D6

// --- Pin cho Nút nhấn ---
#define BTN_ON_OFF 0   // D3
#define BTN_DIR    2   // D4

// --- Thông số hệ thống ---
const unsigned long SEND_INTERVAL_MS = 1000;
const float PULSES_PER_REV = 330.0; // Thay đổi theo motor của bạn

// ======================== KHAI BÁO BIẾN ========================

Adafruit_MPU6050 mpu;

// Cảm biến & Lọc
float pitch = 0, roll = 0;
unsigned long last_sensor_time = 0;
unsigned long last_send_time = 0;

// Trạng thái Motor
bool isRunning = false;
bool isForward = true;
int motorSpeed = 150; // Tốc độ 0-255
volatile long pulse_count = 0;
float current_rpm = 0;

// Đếm thống kê
unsigned long send_count = 0;
unsigned long fail_count = 0;

// ======================== HÀM NGẮT ENCODER ========================

void IRAM_ATTR encoder_isr() {
  if (digitalRead(ENC_B) == LOW) pulse_count++;
  else pulse_count--;
}

// ======================== SETUP ========================

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("\n========================================");
  Serial.println("   MPU6050 + Motor Control (ESP8266)");
  Serial.println("========================================");

  // --- Khởi tạo Motor & Nút nhấn ---
  analogWriteRange(255); // Đưa dải PWM về 0-255
  pinMode(L298N_ENA, OUTPUT);
  pinMode(L298N_IN1, OUTPUT);
  pinMode(L298N_IN2, OUTPUT);
  
  pinMode(BTN_ON_OFF, INPUT_PULLUP);
  pinMode(BTN_DIR, INPUT_PULLUP);

  // --- Khởi tạo Encoder ---
  pinMode(ENC_A, INPUT_PULLUP);
  pinMode(ENC_B, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(ENC_A), encoder_isr, RISING);

  // --- Khởi tạo MPU6050 ---
  if (!mpu.begin()) {
    Serial.println("[LỖI] Không tìm thấy MPU6050!");
    while (1) { delay(10); }
  }
  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  last_sensor_time = millis();
  connectToWiFi();
}

// ======================== LOOP ========================

void loop() {
  // 1. Xử lý nút nhấn
  handleButtons();

  // 2. Cập nhật trạng thái Motor
  updateMotor();

  // 3. Đọc cảm biến Pitch/Roll
  readAndFilterSensor();

  // 4. Gửi dữ liệu theo chu kỳ
  if (millis() - last_send_time >= SEND_INTERVAL_MS) {
    float dt = (millis() - last_send_time) / 1000.0;
    
    // Tính RPM
    noInterrupts();
    long pulses = pulse_count;
    pulse_count = 0;
    interrupts();
    current_rpm = (pulses / PULSES_PER_REV) * (60.0 / dt);

    last_send_time = millis();

    float final_pitch = pitch - 0.68;
    float final_roll  = roll - 0.11;

    Serial.printf("[DATA] P: %.2f | R: %.2f | RPM: %.1f | Mode: %s\n", 
                  final_pitch, final_roll, current_rpm, isRunning ? "RUN" : "STOP");

    sendToEdge(final_pitch, final_roll, current_rpm);
  }

  delay(10); // Đảm bảo loop chạy mượt
}

// ======================== ĐIỀU KHIỂN MOTOR & NÚT ========================

void handleButtons() {
  // Nút Bật/Tắt
  if (digitalRead(BTN_ON_OFF) == LOW) {
    delay(50); // Debounce
    if (digitalRead(BTN_ON_OFF) == LOW) {
      isRunning = !isRunning;
      while(digitalRead(BTN_ON_OFF) == LOW); // Chờ nhả nút
    }
  }

  // Nút Đảo chiều
  if (digitalRead(BTN_DIR) == LOW) {
    delay(50);
    if (digitalRead(BTN_DIR) == LOW) {
      isForward = !isForward;
      while(digitalRead(BTN_DIR) == LOW);
    }
  }
}

void updateMotor() {
  if (!isRunning) {
    analogWrite(L298N_ENA, 0);
    digitalWrite(L298N_IN1, LOW);
    digitalWrite(L298N_IN2, LOW);
    return;
  }

  if (isForward) {
    digitalWrite(L298N_IN1, HIGH);
    digitalWrite(L298N_IN2, LOW);
  } else {
    digitalWrite(L298N_IN1, LOW);
    digitalWrite(L298N_IN2, HIGH);
  }
  analogWrite(L298N_ENA, motorSpeed);
}

// ======================== KẾT NỐI WIFI ========================

void connectToWiFi() {
  Serial.print("[WiFi] Đang kết nối tới ");
  Serial.println(WIFI_SSID);
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 40) {
    delay(500); Serial.print("."); attempts++;
  }
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("\n[OK] WiFi đã kết nối! IP: " + WiFi.localIP().toString());
  }
}

void checkWiFiConnection() {
  if (WiFi.status() != WL_CONNECTED) {
    WiFi.disconnect();
    WiFi.reconnect();
    delay(2000);
  }
}

// ======================== ĐỌC & LỌC CẢM BIẾN ========================

void readAndFilterSensor() {
  unsigned long current_time = millis();
  float dt = (current_time - last_sensor_time) / 1000.0;
  if (dt <= 0 || dt > 1.0) dt = 0.02;
  last_sensor_time = current_time;

  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  float accel_pitch = atan2(a.acceleration.x, sqrt(a.acceleration.y * a.acceleration.y + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;
  float accel_roll = atan2(a.acceleration.y, sqrt(a.acceleration.x * a.acceleration.x + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;

  pitch = 0.96 * (pitch + (g.gyro.y * 180.0 / PI) * dt) + 0.04 * accel_pitch;
  roll  = 0.96 * (roll  + (g.gyro.x * 180.0 / PI) * dt) + 0.04 * accel_roll;
}

// ======================== GỬI LÊN EDGE BACKEND ========================

void sendToEdge(float pitch_val, float roll_val, float speed_val) {
  if (WiFi.status() != WL_CONNECTED) {
    checkWiFiConnection();
    if (WiFi.status() != WL_CONNECTED) return;
  }

  WiFiClient client;
  HTTPClient http;
  String url = String(EDGE_API_URL) + API_ENDPOINT;
  
  http.begin(client, url);
  http.addHeader("Content-Type", "application/json");

  JsonDocument doc;
  doc["pitch"] = pitch_val;
  doc["roll"] = roll_val;
  doc["speed"] = speed_val; // Gửi kèm tốc độ RPM
  doc["headingTrue"] = 0;
  doc["depth"] = 0;

  String jsonString;
  serializeJson(doc, jsonString);

  int httpCode = http.POST(jsonString);
  if (httpCode > 0) {
    if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_CREATED) send_count++;
    else fail_count++;
  } else {
    fail_count++;
  }
  http.end();
}