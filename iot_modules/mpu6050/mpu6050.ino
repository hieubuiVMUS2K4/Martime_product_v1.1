#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <math.h>

Adafruit_MPU6050 mpu;

// Khai báo các biến toàn cục cho Bộ lọc bù
float pitch = 0;
float roll = 0;
unsigned long last_time;

void setup(void) {
  Serial.begin(115200);

  if (!mpu.begin()) {
    Serial.println("Không tìm thấy chip MPU6050!");
    while (1) { delay(10); }
  }
  Serial.println("MPU6050 - Che do Bo Loc Bu (Thuyen)");

  mpu.setAccelerometerRange(MPU6050_RANGE_8_G); 
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  delay(100);
  last_time = millis(); // Khởi tạo mốc thời gian ban đầu
}

void loop() {
  // 1. Tính toán thời gian trôi qua (Delta time - dt)
  unsigned long current_time = millis();
  float dt = (current_time - last_time) / 1000.0; // Đổi ra đơn vị giây
  last_time = current_time;

  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // 2. Hiệu chuẩn dữ liệu thô (Dựa trên thông số bạn đã đo)
  float real_accel_x = a.acceleration.x - (-2.32); 
  float real_accel_y = a.acceleration.y - (-0.75);
  float real_accel_z = a.acceleration.z - 0.87;    

  float real_gyro_x = g.gyro.x - (-0.01);
  float real_gyro_y = g.gyro.y - 0.11;

  // 3. Tính góc từ Gia tốc (Bị giật khi có rung lắc)
  float accel_pitch = atan2(real_accel_x, sqrt(real_accel_y * real_accel_y + real_accel_z * real_accel_z)) * 180.0 / PI;
  float accel_roll = atan2(real_accel_y, sqrt(real_accel_x * real_accel_x + real_accel_z * real_accel_z)) * 180.0 / PI;

  // 4. Chuyển đổi Gyro từ Radian/s sang Độ/s
  float gyro_pitch_rate = real_gyro_y * 180.0 / PI; 
  float gyro_roll_rate  = real_gyro_x * 180.0 / PI; 

  // 5. ÁP DỤNG BỘ LỌC BÙ (COMPLEMENTARY FILTER)
// 5. ÁP DỤNG BỘ LỌC BÙ (COMPLEMENTARY FILTER)
  pitch = 0.96 * (pitch + gyro_pitch_rate * dt) + 0.04 * accel_pitch;
  roll  = 0.96 * (roll + gyro_roll_rate * dt)  + 0.04 * accel_roll;

  // 6. TRỪ BÌ (ZEROING) GÓC NGHIÊNG CUỐI CÙNG
  // Trừ đi sai số dư thừa khi đặt trên mặt phẳng chuẩn
  float final_pitch = pitch - 0.68;
  float final_roll = roll - 0.11;

  // ================= IN DỮ LIỆU ĐÃ LỌC VÀ TRỪ BÌ =================
  Serial.print("Pitch: "); 
  Serial.print(final_pitch, 2); 
  Serial.print(" \t|\tRoll: "); 
  Serial.println(final_roll, 2);

  delay(20); 
}