using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewAndSyncTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF EXISTS since these tables may not exist in all environments
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"DrillLogs\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"DrillSchedules\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"CrewMembers\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"DrillTypes\" CASCADE;");

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CertificateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ValidityPeriodMonths = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CountryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FlagImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RankCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConflictDetail = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_outbox",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_outbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "country_certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_country_certificates_certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_country_certificates_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RankId = table.Column<int>(type: "integer", nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmbarkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisembarkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOnboard = table.Column<bool>(type: "boolean", nullable: false),
                    EmergencyContact = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmailAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdCardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaritalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    BloodGroup = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    ClothingSize = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ShoeSize = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CateringSize = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsSmoker = table.Column<bool>(type: "boolean", nullable: true),
                    IsCovidVaccinated = table.Column<bool>(type: "boolean", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NextOfKinName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NextOfKinRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NextOfKinPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NextOfKinAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EducationInstitution = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EducationCourse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EducationPeriodYears = table.Column<int>(type: "integer", nullable: true),
                    EducationGraduationYear = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_members_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "rank_certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RankId = table.Column<int>(type: "integer", nullable: false),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rank_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rank_certificates_certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rank_certificates_ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crew_certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CertificateOfCompetency = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    DocumentFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crew_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crew_certificates_certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_crew_certificates_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crew_certificates_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employment_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employment_documents_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employment_documents_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_health_documents_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seafarer_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seafarer_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seafarer_documents_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_seafarer_documents_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VesselFlag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VesselType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VesselGrt = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    VesselDwt = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    VesselYearBuilt = table.Column<int>(type: "integer", nullable: true),
                    TradeArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MainEngineType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MainEnginePowerKw = table.Column<int>(type: "integer", nullable: true),
                    MainEngineMaker = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BoilerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HasExhaustGasScrubber = table.Column<bool>(type: "boolean", nullable: true),
                    Ecdis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RankAtTime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BoardingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisembarkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BoardingPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    BoardingPortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DisembarkPortCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    DisembarkPortName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    BoardingRecords = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SyncVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrewMemberId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_records_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_service_records_crew_members_CrewMemberId1",
                        column: x => x.CrewMemberId1,
                        principalTable: "crew_members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "travel_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrewMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_travel_documents_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_travel_documents_crew_members_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "crew_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_certificates_CertificateCode",
                table: "certificates",
                column: "CertificateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_countries_CountryCode",
                table: "countries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_country_certificates_CertificateId",
                table: "country_certificates",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_country_certificates_CountryId_CertificateId",
                table: "country_certificates",
                columns: new[] { "CountryId", "CertificateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_CertificateId",
                table: "crew_certificates",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_CountryId",
                table: "crew_certificates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_CrewMemberId_CertificateId",
                table: "crew_certificates",
                columns: new[] { "CrewMemberId", "CertificateId" });

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_ExpiryDate",
                table: "crew_certificates",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_crew_certificates_IsSynced",
                table: "crew_certificates",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_CrewId",
                table: "crew_members",
                column: "CrewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_FullName",
                table: "crew_members",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_IsOnboard",
                table: "crew_members",
                column: "IsOnboard");

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_IsSynced",
                table: "crew_members",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_crew_members_RankId",
                table: "crew_members",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_employment_documents_CountryId",
                table: "employment_documents",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_employment_documents_CrewMemberId",
                table: "employment_documents",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_health_documents_CrewMemberId",
                table: "health_documents",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_CertificateId",
                table: "rank_certificates",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_rank_certificates_RankId_CertificateId",
                table: "rank_certificates",
                columns: new[] { "RankId", "CertificateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ranks_RankCode",
                table: "ranks",
                column: "RankCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seafarer_documents_CountryId",
                table: "seafarer_documents",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_seafarer_documents_CrewMemberId",
                table: "seafarer_documents",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_service_records_CrewMemberId",
                table: "service_records",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_service_records_CrewMemberId1",
                table: "service_records",
                column: "CrewMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_service_records_IsSynced",
                table: "service_records",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_sync_logs_OriginNode",
                table: "sync_logs",
                column: "OriginNode");

            migrationBuilder.CreateIndex(
                name: "IX_sync_logs_ProcessedAt",
                table: "sync_logs",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_sync_outbox_DeliveredAt",
                table: "sync_outbox",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_sync_outbox_TableName_RecordKey",
                table: "sync_outbox",
                columns: new[] { "TableName", "RecordKey" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_documents_CountryId",
                table: "travel_documents",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_travel_documents_CrewMemberId",
                table: "travel_documents",
                column: "CrewMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_certificates");

            migrationBuilder.DropTable(
                name: "crew_certificates");

            migrationBuilder.DropTable(
                name: "employment_documents");

            migrationBuilder.DropTable(
                name: "health_documents");

            migrationBuilder.DropTable(
                name: "rank_certificates");

            migrationBuilder.DropTable(
                name: "seafarer_documents");

            migrationBuilder.DropTable(
                name: "service_records");

            migrationBuilder.DropTable(
                name: "sync_logs");

            migrationBuilder.DropTable(
                name: "sync_outbox");

            migrationBuilder.DropTable(
                name: "travel_documents");

            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "crew_members");

            migrationBuilder.DropTable(
                name: "ranks");

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
                name: "DrillTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DrillCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrillName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FrequencyDays = table.Column<int>(type: "integer", nullable: false),
                    FrequencyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    RegulationPeriod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RegulationSource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TriggerCondition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TriggerWithinDays = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarningDaysBefore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrillTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrillSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToCrewId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrillTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VesselId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    IsAutoGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    LastExecutedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OverdueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    ScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScheduledMonth = table.Column<int>(type: "integer", nullable: false),
                    ScheduledYear = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrillSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrillSchedules_CrewMembers_AssignedToCrewId",
                        column: x => x.AssignedToCrewId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DrillSchedules_DrillTypes_DrillTypeId",
                        column: x => x.DrillTypeId,
                        principalTable: "DrillTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrillSchedules_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DrillLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConductedByCrewId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrillScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrillTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedByCrewId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    GeneralRemarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: false),
                    LessonsLearned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewCrewCount = table.Column<int>(type: "integer", nullable: false),
                    OriginNode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OverallAssessment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Participants = table.Column<string>(type: "jsonb", nullable: true),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalParticipants = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrillLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrillLogs_CrewMembers_ConductedByCrewId",
                        column: x => x.ConductedByCrewId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DrillLogs_CrewMembers_VerifiedByCrewId",
                        column: x => x.VerifiedByCrewId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DrillLogs_DrillSchedules_DrillScheduleId",
                        column: x => x.DrillScheduleId,
                        principalTable: "DrillSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DrillLogs_DrillTypes_DrillTypeId",
                        column: x => x.DrillTypeId,
                        principalTable: "DrillTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_ConductedByCrewId",
                table: "DrillLogs",
                column: "ConductedByCrewId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_DrillScheduleId",
                table: "DrillLogs",
                column: "DrillScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_DrillTypeId",
                table: "DrillLogs",
                column: "DrillTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_ExecutionDate",
                table: "DrillLogs",
                column: "ExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_LogCode",
                table: "DrillLogs",
                column: "LogCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrillLogs_VerifiedByCrewId",
                table: "DrillLogs",
                column: "VerifiedByCrewId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_AssignedToCrewId",
                table: "DrillSchedules",
                column: "AssignedToCrewId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_DrillTypeId",
                table: "DrillSchedules",
                column: "DrillTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_DueDate",
                table: "DrillSchedules",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_ScheduleCode",
                table: "DrillSchedules",
                column: "ScheduleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_ScheduledYear_ScheduledMonth",
                table: "DrillSchedules",
                columns: new[] { "ScheduledYear", "ScheduledMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_Status",
                table: "DrillSchedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DrillSchedules_VesselId",
                table: "DrillSchedules",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_DrillTypes_Category_DisplayOrder",
                table: "DrillTypes",
                columns: new[] { "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DrillTypes_DrillCode",
                table: "DrillTypes",
                column: "DrillCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrillTypes_IsActive",
                table: "DrillTypes",
                column: "IsActive");
        }
    }
}
