using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services
{
    /// <summary>
    /// Background service to simulate real-time telemetry data
    /// In production, this would be replaced by actual sensor data collection
    /// </summary>
    public class TelemetrySimulatorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelemetrySimulatorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();

        public TelemetrySimulatorService(
            IServiceProvider serviceProvider,
            ILogger<TelemetrySimulatorService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check if simulator is enabled in configuration
            var enabled = _configuration.GetValue<bool>("TelemetrySimulator:Enabled", true);
            
            if (!enabled)
            {
                _logger.LogInformation("Telemetry Simulator is DISABLED in configuration. No data will be generated.");
                return; // Exit immediately
            }

            _logger.LogInformation("Telemetry Simulator Service started");

            // Get configurable interval (default: 60 seconds)
            var intervalSeconds = _configuration.GetValue<int>("TelemetrySimulator:IntervalSeconds", 60);
            var retentionHours = _configuration.GetValue<int>("TelemetrySimulator:DataRetentionHours", 24);
            
            _logger.LogInformation(
                "Simulator config - Interval: {Interval}s, Retention: {Retention}h", 
                intervalSeconds, retentionHours);

            // Wait 10 seconds before starting simulation
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();

                    await SimulatePositionData(dbContext);
                    await SimulateNavigationData(dbContext);
                    await SimulateEngineData(dbContext);
                    await SimulateGeneratorData(dbContext);
                    await SimulateEnvironmentalData(dbContext);

                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogDebug("Telemetry data simulated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error simulating telemetry data");
                }

                // Update based on configured interval (default: 60 seconds)
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Telemetry Simulator Service stopped");
        }

        private async Task SimulatePositionData(EdgeDbContext dbContext)
        {
            // Get last position or create initial one
            var lastPosition = await dbContext.PositionData
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();

            var baseLatitude = lastPosition?.Latitude ?? 10.7769;
            var baseLongitude = lastPosition?.Longitude ?? 106.7009;

            // Simulate vessel movement (0.001 degree = ~111 meters)
            var deltaLat = (_random.NextDouble() - 0.5) * 0.002; // ±222m
            var deltaLon = (_random.NextDouble() - 0.5) * 0.002;

            var newPosition = new PositionData
            {
                Timestamp = DateTime.UtcNow,
                Latitude = baseLatitude + deltaLat,
                Longitude = baseLongitude + deltaLon,
                Altitude = 5.0 + (_random.NextDouble() - 0.5) * 2.0,
                SpeedOverGround = 12.0 + (_random.NextDouble() - 0.5) * 2.0, // 10-14 knots
                CourseOverGround = 95.0 + (_random.NextDouble() - 0.5) * 10.0, // 90-100 degrees
                FixQuality = 1,
                SatellitesUsed = 8 + _random.Next(0, 4),
                Hdop = 1.0 + _random.NextDouble(),
                Source = "GPS",
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.PositionData.AddAsync(newPosition);

            // Keep only last 1000 position records
            var oldPositions = await dbContext.PositionData
                .OrderByDescending(p => p.Timestamp)
                .Skip(1000)
                .ToListAsync();
            
            if (oldPositions.Any())
            {
                dbContext.PositionData.RemoveRange(oldPositions);
            }
        }

        private async Task SimulateNavigationData(EdgeDbContext dbContext)
        {
            var navigation = new NavigationData
            {
                Timestamp = DateTime.UtcNow,
                HeadingTrue = 95.0 + (_random.NextDouble() - 0.5) * 5.0,
                HeadingMagnetic = 93.5 + (_random.NextDouble() - 0.5) * 5.0,
                RateOfTurn = (_random.NextDouble() - 0.5) * 4.0,
                Pitch = (_random.NextDouble() - 0.5) * 3.0,
                Roll = (_random.NextDouble() - 0.5) * 4.0,
                SpeedThroughWater = 12.0 + (_random.NextDouble() - 0.5) * 1.0,
                Depth = 25.0 + (_random.NextDouble() - 0.5) * 5.0,
                WindSpeedRelative = 15.0 + (_random.NextDouble() - 0.5) * 5.0,
                WindDirectionRelative = 45.0 + (_random.NextDouble() - 0.5) * 20.0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.NavigationData.AddAsync(navigation);

            // Keep only last 1000 records
            var oldRecords = await dbContext.NavigationData
                .OrderByDescending(n => n.Timestamp)
                .Skip(1000)
                .ToListAsync();
            
            if (oldRecords.Any())
            {
                dbContext.NavigationData.RemoveRange(oldRecords);
            }
        }

        private async Task SimulateEngineData(EdgeDbContext dbContext)
        {
            // Main Engine
            var mainEngine = new EngineData
            {
                Timestamp = DateTime.UtcNow,
                EngineId = "MAIN_ENGINE",
                Rpm = 720.0 + (_random.NextDouble() - 0.5) * 20.0,
                LoadPercent = 75.0 + (_random.NextDouble() - 0.5) * 5.0,
                CoolantTemp = 82.0 + (_random.NextDouble() - 0.5) * 3.0,
                ExhaustTemp = 380.0 + (_random.NextDouble() - 0.5) * 20.0,
                LubeOilPressure = 4.2 + (_random.NextDouble() - 0.5) * 0.3,
                LubeOilTemp = 65.0 + (_random.NextDouble() - 0.5) * 5.0,
                FuelPressure = 5.5 + (_random.NextDouble() - 0.5) * 0.5,
                FuelRate = 145.0 + (_random.NextDouble() - 0.5) * 10.0,
                RunningHours = 12543.5 + 0.0014, // Increment by ~5 seconds
                StartCount = 1250,
                AlarmStatus = 0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.EngineData.AddAsync(mainEngine);

            // Aux Engine
            var auxEngine = new EngineData
            {
                Timestamp = DateTime.UtcNow,
                EngineId = "AUX_ENGINE_1",
                Rpm = 1500.0 + (_random.NextDouble() - 0.5) * 50.0,
                LoadPercent = 45.0 + (_random.NextDouble() - 0.5) * 10.0,
                CoolantTemp = 78.0 + (_random.NextDouble() - 0.5) * 3.0,
                ExhaustTemp = 350.0 + (_random.NextDouble() - 0.5) * 20.0,
                LubeOilPressure = 3.8 + (_random.NextDouble() - 0.5) * 0.3,
                LubeOilTemp = 62.0 + (_random.NextDouble() - 0.5) * 5.0,
                FuelPressure = 4.8 + (_random.NextDouble() - 0.5) * 0.5,
                FuelRate = 45.0 + (_random.NextDouble() - 0.5) * 5.0,
                RunningHours = 8234.2 + 0.0014,
                StartCount = 890,
                AlarmStatus = 0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.EngineData.AddAsync(auxEngine);

            // Cleanup old records
            var oldRecords = await dbContext.EngineData
                .OrderByDescending(e => e.Timestamp)
                .Skip(2000)
                .ToListAsync();
            
            if (oldRecords.Any())
            {
                dbContext.EngineData.RemoveRange(oldRecords);
            }
        }

        private async Task SimulateGeneratorData(EdgeDbContext dbContext)
        {
            // Generator 1 (Running)
            var gen1 = new GeneratorData
            {
                Timestamp = DateTime.UtcNow,
                GeneratorId = "GEN_1",
                IsRunning = true,
                Voltage = 440.0 + (_random.NextDouble() - 0.5) * 5.0,
                Frequency = 60.0 + (_random.NextDouble() - 0.5) * 0.2,
                Current = 85.5 + (_random.NextDouble() - 0.5) * 5.0,
                ActivePower = 320.0 + (_random.NextDouble() - 0.5) * 20.0,
                PowerFactor = 0.92 + (_random.NextDouble() - 0.5) * 0.02,
                RunningHours = 5432.8 + 0.0014,
                LoadPercent = 65.0 + (_random.NextDouble() - 0.5) * 5.0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.GeneratorData.AddAsync(gen1);

            // Generator 2 (Standby)
            var gen2 = new GeneratorData
            {
                Timestamp = DateTime.UtcNow,
                GeneratorId = "GEN_2",
                IsRunning = false,
                Voltage = 0,
                Frequency = 0,
                Current = 0,
                ActivePower = 0,
                PowerFactor = 0,
                RunningHours = 3210.5,
                LoadPercent = 0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.GeneratorData.AddAsync(gen2);

            // Emergency Generator (Standby)
            var emerGen = new GeneratorData
            {
                Timestamp = DateTime.UtcNow,
                GeneratorId = "EMER_GEN",
                IsRunning = false,
                Voltage = 0,
                Frequency = 0,
                Current = 0,
                ActivePower = 0,
                PowerFactor = 0,
                RunningHours = 124.2,
                LoadPercent = 0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.GeneratorData.AddAsync(emerGen);

            // Cleanup
            var oldRecords = await dbContext.GeneratorData
                .OrderByDescending(g => g.Timestamp)
                .Skip(3000)
                .ToListAsync();
            
            if (oldRecords.Any())
            {
                dbContext.GeneratorData.RemoveRange(oldRecords);
            }
        }

        private async Task SimulateEnvironmentalData(EdgeDbContext dbContext)
        {
            var env = new EnvironmentalData
            {
                Timestamp = DateTime.UtcNow,
                AirTemperature = 28.0 + (_random.NextDouble() - 0.5) * 3.0,
                BarometricPressure = 1013.25 + (_random.NextDouble() - 0.5) * 5.0,
                Humidity = 75.0 + (_random.NextDouble() - 0.5) * 10.0,
                SeaTemperature = 26.0 + (_random.NextDouble() - 0.5) * 2.0,
                WindSpeed = 15.0 + (_random.NextDouble() - 0.5) * 5.0,
                WindDirection = 45.0 + (_random.NextDouble() - 0.5) * 20.0,
                WaveHeight = 1.5 + (_random.NextDouble() - 0.5) * 0.5,
                Visibility = 10.0 + (_random.NextDouble() - 0.5) * 2.0,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.EnvironmentalData.AddAsync(env);

            // Cleanup
            var oldRecords = await dbContext.EnvironmentalData
                .OrderByDescending(e => e.Timestamp)
                .Skip(1000)
                .ToListAsync();
            
            if (oldRecords.Any())
            {
                dbContext.EnvironmentalData.RemoveRange(oldRecords);
            }
        }
    }
}
