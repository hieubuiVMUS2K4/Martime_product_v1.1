using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace EdgeCollector.Services
{
    public class NmeaCollectorService : BackgroundService
    {
        private readonly ILogger<NmeaCollectorService> _logger;
        private readonly ILocalStorageService _storage;
        private SerialPort? _serialPort;

        public NmeaCollectorService(ILogger<NmeaCollectorService> logger, ILocalStorageService storage)
        {
            _logger = logger;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Configure serial port for NMEA data
                _serialPort = new SerialPort("COM1", 4800, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += OnNmeaDataReceived;
                _serialPort.Open();

                _logger.LogInformation("NMEA Collector started on COM1");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NMEA Collector");
            }
        }

        private async void OnNmeaDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var data = _serialPort?.ReadExisting();
                if (!string.IsNullOrEmpty(data))
                {
                    var sensorData = new SensorData
                    {
                        Type = "NMEA",
                        Data = data,
                        Timestamp = DateTime.UtcNow,
                        Source = "GPS/AIS"
                    };

                    await _storage.StoreAsync(sensorData);
                    _logger.LogDebug($"NMEA data stored: {data.Substring(0, Math.Min(50, data.Length))}...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NMEA data");
            }
        }

        public override void Dispose()
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
            base.Dispose();
        }
    }

    public class SensorData
    {
        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public interface ILocalStorageService
    {
        Task StoreAsync(SensorData data);
        Task<List<SensorData>> GetPendingAsync();
        Task MarkSentAsync(int id);
    }

    public class LocalStorageService : ILocalStorageService
    {
        public Task StoreAsync(SensorData data)
        {
            // Store in SQLite for offline capability
            throw new NotImplementedException();
        }

        public Task<List<SensorData>> GetPendingAsync()
        {
            throw new NotImplementedException();
        }

        public Task MarkSentAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}