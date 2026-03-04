using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class InitialSyncSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AisData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Mmsi = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    SpeedOverGround = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", nullable: true),
                    ShipName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Destination = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    EtaMonth = table.Column<int>(type: "integer", nullable: true),
                    EtaDay = table.Column<int>(type: "integer", nullable: true),
                    EtaHour = table.Column<int>(type: "integer", nullable: true),
                    EtaMinute = table.Column<int>(type: "integer", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AisData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrewMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ShipId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EngineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rpm = table.Column<double>(type: "double precision", nullable: true),
                    LoadPercent = table.Column<double>(type: "double precision", nullable: true),
                    FuelRate = table.Column<double>(type: "double precision", nullable: true),
                    RunningHours = table.Column<double>(type: "double precision", nullable: true),
                    AlarmStatus = table.Column<int>(type: "integer", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentalData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AirTemperature = table.Column<double>(type: "double precision", nullable: true),
                    BarometricPressure = table.Column<double>(type: "double precision", nullable: true),
                    Humidity = table.Column<double>(type: "double precision", nullable: true),
                    SeaTemperature = table.Column<double>(type: "double precision", nullable: true),
                    WindSpeed = table.Column<double>(type: "double precision", nullable: true),
                    WindDirection = table.Column<double>(type: "double precision", nullable: true),
                    WaveHeight = table.Column<double>(type: "double precision", nullable: true),
                    Visibility = table.Column<double>(type: "double precision", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentalData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FuelConsumptionData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FuelType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConsumedVolume = table.Column<double>(type: "double precision", nullable: false),
                    ConsumedMass = table.Column<double>(type: "double precision", nullable: false),
                    TankId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Density = table.Column<double>(type: "double precision", nullable: true),
                    DistanceTraveled = table.Column<double>(type: "double precision", nullable: true),
                    TimeUnderway = table.Column<double>(type: "double precision", nullable: true),
                    CargoWeight = table.Column<double>(type: "double precision", nullable: true),
                    Co2Emissions = table.Column<double>(type: "double precision", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelConsumptionData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratorData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRunning = table.Column<bool>(type: "boolean", nullable: false),
                    Voltage = table.Column<double>(type: "double precision", nullable: true),
                    Frequency = table.Column<double>(type: "double precision", nullable: true),
                    Current = table.Column<double>(type: "double precision", nullable: true),
                    ActivePower = table.Column<double>(type: "double precision", nullable: true),
                    PowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    RunningHours = table.Column<double>(type: "double precision", nullable: true),
                    LoadPercent = table.Column<double>(type: "double precision", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratorData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShipId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaritimeReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportTypeId = table.Column<int>(type: "integer", nullable: false),
                    ReportDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreparedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MasterSignature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportData = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    IsTransmitted = table.Column<bool>(type: "boolean", nullable: false),
                    TransmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritimeReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavigationData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeadingTrue = table.Column<double>(type: "double precision", nullable: true),
                    HeadingMagnetic = table.Column<double>(type: "double precision", nullable: true),
                    RateOfTurn = table.Column<double>(type: "double precision", nullable: true),
                    Pitch = table.Column<double>(type: "double precision", nullable: true),
                    Roll = table.Column<double>(type: "double precision", nullable: true),
                    SpeedThroughWater = table.Column<double>(type: "double precision", nullable: true),
                    Depth = table.Column<double>(type: "double precision", nullable: true),
                    WindSpeedRelative = table.Column<double>(type: "double precision", nullable: true),
                    WindDirectionRelative = table.Column<double>(type: "double precision", nullable: true),
                    WindSpeedTrue = table.Column<double>(type: "double precision", nullable: true),
                    WindDirectionTrue = table.Column<double>(type: "double precision", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmeaRawData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentenceType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RawSentence = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ChecksumValid = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmeaRawData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoonReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaritimeReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", nullable: true),
                    SpeedOverGround = table.Column<double>(type: "double precision", nullable: true),
                    DistanceTraveled = table.Column<double>(type: "double precision", nullable: true),
                    DistanceToGo = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedTimeOfArrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WeatherConditions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeaState = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AirTemperature = table.Column<double>(type: "double precision", nullable: true),
                    SeaTemperature = table.Column<double>(type: "double precision", nullable: true),
                    BarometricPressure = table.Column<double>(type: "double precision", nullable: true),
                    WindDirection = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WindSpeed = table.Column<double>(type: "double precision", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FuelOilConsumed = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilConsumed = table.Column<double>(type: "double precision", nullable: true),
                    LubOilConsumed = table.Column<double>(type: "double precision", nullable: true),
                    FreshWaterConsumed = table.Column<double>(type: "double precision", nullable: true),
                    FuelOilROB = table.Column<double>(type: "double precision", nullable: true),
                    DieselOilROB = table.Column<double>(type: "double precision", nullable: true),
                    LubOilROB = table.Column<double>(type: "double precision", nullable: true),
                    FreshWaterROB = table.Column<double>(type: "double precision", nullable: true),
                    MainEngineRunningHours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MainEngineRPM = table.Column<double>(type: "double precision", nullable: true),
                    MainEnginePower = table.Column<double>(type: "double precision", nullable: true),
                    AuxEngineRunningHours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CargoOnBoard = table.Column<double>(type: "double precision", nullable: true),
                    CargoDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperationalRemarks = table.Column<string>(type: "text", nullable: true),
                    MachineryRemarks = table.Column<string>(type: "text", nullable: true),
                    CargoRemarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoonReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PositionData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    SpeedOverGround = table.Column<double>(type: "double precision", nullable: true),
                    CourseOverGround = table.Column<double>(type: "double precision", nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RegulationReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresMasterSignature = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateSchema = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlarmType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AlarmCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyAlarms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IMO = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TankLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TankId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TankType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LevelPercent = table.Column<double>(type: "double precision", nullable: false),
                    VolumeLiters = table.Column<double>(type: "double precision", nullable: true),
                    Temperature = table.Column<double>(type: "double precision", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IMO = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CallSign = table.Column<string>(type: "text", nullable: false),
                    VesselType = table.Column<string>(type: "text", nullable: false),
                    GrossTonnage = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    DeadWeight = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    BuildDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Flag = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoyageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoyageNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeparturePort = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalPort = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CargoType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CargoWeight = table.Column<double>(type: "double precision", nullable: true),
                    DistanceTraveled = table.Column<double>(type: "double precision", nullable: true),
                    FuelConsumed = table.Column<double>(type: "double precision", nullable: true),
                    AverageSpeed = table.Column<double>(type: "double precision", nullable: true),
                    VoyageStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateType = table.Column<string>(type: "text", nullable: false),
                    CertificateName = table.Column<string>(type: "text", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CertificateNumber = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FuelConsumptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FuelConsumed = table.Column<double>(type: "double precision", precision: 8, scale: 3, nullable: false),
                    FuelType = table.Column<string>(type: "text", nullable: false),
                    DistanceTraveled = table.Column<double>(type: "double precision", precision: 8, scale: 2, nullable: false),
                    AverageSpeed = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    FuelEfficiency = table.Column<double>(type: "double precision", precision: 6, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelConsumptions_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortCode = table.Column<string>(type: "text", nullable: false),
                    PortName = table.Column<string>(type: "text", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PortFees = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CargoQuantity = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    CargoType = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortCalls_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselAlerts_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 10, scale: 7, nullable: false),
                    Longitude = table.Column<double>(type: "double precision", precision: 10, scale: 7, nullable: false),
                    Speed = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    Course = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselPositions_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateNumber",
                table: "Certificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_VesselId_ExpiryDate",
                table: "Certificates",
                columns: new[] { "VesselId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FuelConsumptions_VesselId",
                table: "FuelConsumptions",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_VesselId_ArrivalTime",
                table: "PortCalls",
                columns: new[] { "VesselId", "ArrivalTime" });

            migrationBuilder.CreateIndex(
                name: "IX_VesselAlerts_IsAcknowledged",
                table: "VesselAlerts",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_VesselAlerts_VesselId_Timestamp",
                table: "VesselAlerts",
                columns: new[] { "VesselId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VesselPositions_VesselId_Timestamp",
                table: "VesselPositions",
                columns: new[] { "VesselId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_IMO",
                table: "Vessels",
                column: "IMO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AisData");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.DropTable(
                name: "EngineData");

            migrationBuilder.DropTable(
                name: "EnvironmentalData");

            migrationBuilder.DropTable(
                name: "FuelConsumptionData");

            migrationBuilder.DropTable(
                name: "FuelConsumptions");

            migrationBuilder.DropTable(
                name: "GeneratorData");

            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropTable(
                name: "MaritimeReports");

            migrationBuilder.DropTable(
                name: "NavigationData");

            migrationBuilder.DropTable(
                name: "NmeaRawData");

            migrationBuilder.DropTable(
                name: "NoonReports");

            migrationBuilder.DropTable(
                name: "PortCalls");

            migrationBuilder.DropTable(
                name: "PositionData");

            migrationBuilder.DropTable(
                name: "ReportTypes");

            migrationBuilder.DropTable(
                name: "SafetyAlarms");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "TankLevels");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VesselAlerts");

            migrationBuilder.DropTable(
                name: "VesselPositions");

            migrationBuilder.DropTable(
                name: "VoyageRecords");

            migrationBuilder.DropTable(
                name: "Vessels");
        }
    }
}
