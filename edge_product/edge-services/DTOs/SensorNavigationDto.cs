namespace MaritimeEdge.DTOs;

/// <summary>
/// DTO nhận dữ liệu Pitch/Roll từ cảm biến MPU6050 (ESP32) gửi lên
/// </summary>
public class SensorNavigationDto
{
    /// <summary>Góc Pitch (Chúi) - độ</summary>
    public double? Pitch { get; set; }

    /// <summary>Góc Roll (nghiêng) - độ</summary>
    public double? Roll { get; set; }

    /// <summary>Hướng đi thật (nếu có la bàn) - độ</summary>
    public double? HeadingTrue { get; set; }

    /// <summary>Hướng đi từ (nếu có la bàn) - độ</summary>
    public double? HeadingMagnetic { get; set; }

    /// <summary>Tốc độ qua nước (nếu có) - knots</summary>
    public double? SpeedThroughWater { get; set; }

    /// <summary>Độ sâu (nếu có cảm biến) - mét</summary>
    public double? Depth { get; set; }
}
