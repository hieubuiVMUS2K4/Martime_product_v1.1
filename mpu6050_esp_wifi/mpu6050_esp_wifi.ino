#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <math.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>
#include <WiFiClientSecure.h> 

// ======================== CẤU HÌNH ========================
const char* WIFI_SSID     = "tinhvdth";
const char* WIFI_PASSWORD = "123456789tt";
const char* EDGE_API_URL  = "https://vesselmaritimevmu.site";
const char* API_ENDPOINT  = "/api/telemetry/navigation";

#define L298N_ENA  15  // D8
#define L298N_IN1  16  // D0
#define L298N_IN2  13  // D7
#define ENC_A      14  // D5
#define ENC_B      12  // D6
#define BTN_POWER  2   // D4

const unsigned long SEND_INTERVAL_MS = 1000;
const float PULSES_PER_REV = 330.0; 

// --- HIỆU CHUẨN (CALIBRATION) ---
const float PITCH_OFFSET = -4.0; // Sai số chúi hiện tại
const float ROLL_OFFSET  = -3.6; // Sai số lắc ngang hiện tại

// ======================== BIẾN TOÀN CỤC ========================
Adafruit_MPU6050 mpu;
float pitch = 0, roll = 0;
unsigned long last_sensor_time = 0;
unsigned long last_send_time = 0;

volatile bool isRunning = false;
volatile unsigned long last_debounce_power = 0;
volatile long pulse_count = 0;

int motorSpeed = 255; 
float current_rpm = 0;

// ======================== CÁC HÀM NGẮT (ISR) ========================

void IRAM_ATTR encoder_isr() {
  if (digitalRead(ENC_B) == LOW) pulse_count++;
  else pulse_count--;
}

void IRAM_ATTR power_isr() {
  unsigned long now = millis();
  if (now - last_debounce_power > 250) {
    isRunning = !isRunning;
    last_debounce_power = now;
  }
}

// ======================== SETUP ========================

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  pinMode(L298N_ENA, OUTPUT);
  pinMode(L298N_IN1, OUTPUT);
  pinMode(L298N_IN2, OUTPUT);
  
  pinMode(BTN_POWER, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(BTN_POWER), power_isr, FALLING);

  pinMode(ENC_A, INPUT_PULLUP);
  pinMode(ENC_B, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(ENC_A), encoder_isr, RISING);

  if (!mpu.begin()) {
    Serial.println("[LOI] MPU6050!");
    while (1) delay(10);
  }

  // --- TĂNG ĐỘ NHẠY PHẦN CỨNG ---
  mpu.setAccelerometerRange(MPU6050_RANGE_2_G);   // Mức nhạy nhất (2G)
  mpu.setGyroRange(MPU6050_RANGE_250_DEG);        // Mức nhạy nhất (250 deg/s)
  mpu.setFilterBandwidth(MPU6050_BAND_44_HZ);     // Tăng băng thông để phản ứng nhanh hơn
  
  last_sensor_time = millis();
  connectToWiFi();
}

void loop() {
  updateMotor(); 
  readAndFilterSensor();

  if (millis() - last_send_time >= SEND_INTERVAL_MS) {
    last_send_time = millis();
    
    noInterrupts();
    long pulses = pulse_count;
    pulse_count = 0;
    interrupts();
    float dt = SEND_INTERVAL_MS / 1000.0;
    current_rpm = (pulses / PULSES_PER_REV) * (60.0 / dt);

    // Dữ liệu đã được chuẩn hóa về 0
    float normalized_pitch = pitch - PITCH_OFFSET;
    float normalized_roll  = roll - ROLL_OFFSET;

    Serial.printf("[SENSOR] Pitch: %.2f | Roll: %.2f | RPM: %.1f\n", 
                  normalized_pitch, normalized_roll, current_rpm);

    if (WiFi.status() == WL_CONNECTED) {
      sendToEdge(normalized_pitch, normalized_roll, current_rpm);
    } else {
      WiFi.begin(WIFI_SSID, WIFI_PASSWORD); 
    }
  }
  delay(10); 
}

void updateMotor() {
  if (!isRunning) {
    analogWrite(L298N_ENA, 0);
    digitalWrite(L298N_IN1, LOW);
    digitalWrite(L298N_IN2, LOW);
    return;
  }
  digitalWrite(L298N_IN1, HIGH);
  digitalWrite(L298N_IN2, LOW);
  analogWrite(L298N_ENA, motorSpeed);
}

void readAndFilterSensor() {
  unsigned long current_time = millis();
  float dt = (current_time - last_sensor_time) / 1000.0;
  if (dt <= 0 || dt > 0.5) dt = 0.01;
  last_sensor_time = current_time;

  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // Tính góc từ Accel (Trọng lực)
  float accel_pitch = atan2(a.acceleration.x, sqrt(a.acceleration.y * a.acceleration.y + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;
  float accel_roll = atan2(a.acceleration.y, sqrt(a.acceleration.x * a.acceleration.x + a.acceleration.z * a.acceleration.z)) * 180.0 / PI;

  // --- TĂNG ĐỘ NHẠY BỘ LỌC ---
  // Thay đổi 0.96/0.04 thành 0.90/0.10 để phản ứng với góc nghiêng nhanh hơn
  pitch = 0.90 * (pitch + (g.gyro.y * 180.0 / PI) * dt) + 0.10 * accel_pitch;
  roll  = 0.90 * (roll  + (g.gyro.x * 180.0 / PI) * dt) + 0.10 * accel_roll;
}

void sendToEdge(float pitch_val, float roll_val, float speed_val) {
  WiFiClientSecure client;
  client.setInsecure(); 
  HTTPClient http;
  http.setTimeout(500); // Tăng lên một chút để ổn định HTTPS
  
  if (http.begin(client, String(EDGE_API_URL) + API_ENDPOINT)) {
    http.addHeader("Content-Type", "application/json");
    JsonDocument doc;
    doc["pitch"] = pitch_val;
    doc["roll"] = roll_val;
    doc["speed"] = speed_val;
    String jsonString;
    serializeJson(doc, jsonString);
    http.POST(jsonString);
    http.end();
  }
}

void connectToWiFi() {
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  int timeout = 0;
  while (WiFi.status() != WL_CONNECTED && timeout < 20) {
    delay(500);
    timeout++;
  }
}