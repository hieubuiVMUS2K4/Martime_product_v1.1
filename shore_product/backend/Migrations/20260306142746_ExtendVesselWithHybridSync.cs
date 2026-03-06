using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class ExtendVesselWithHybridSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VesselType",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IMO",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Flag",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CallSign",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<double>(
                name: "AirdraftReductionMastFouled",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ais",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AnchorChainPort",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnchorChainStarboard",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnchorChainStern",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AnchorChainSternNA",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AzimuthEngAftCount",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AzimuthEngAftMaxPowerKW",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AzimuthEngFwdCount",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AzimuthEngFwdMaxPowerKW",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BalesCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BallastWaterCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BareboatChartererZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BilgeWaterCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BlockCoefficient",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BlockCoefficientNA",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "BowToBulbousBow",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BowToManifold",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BowthrusterNA",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "BreadthMoulded",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BridgeToAft",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BridgeToBow",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChartererZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassNotation",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassRegisterNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSocietyZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyImoNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CsoCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoFirstName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoLastName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoPhone24h",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoTitle",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CsoZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DDistance",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfRegistry",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeckToManifold",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DepthMoulded",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaFirstName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaLastName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaPhone24h",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaTitle",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DpaZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DraftFullBallast",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DraftMoulded",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DraftScantling",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DscHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DscMF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DscVHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress1",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress2",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpirbFrequency",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpirbMaker",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpirbModel",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpirbNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpirbOperatingSystem",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldOwnership",
                table: "Vessels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStatePhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagStateZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FreshWaterAllowanceFwa",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FreshWaterCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GrainCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GrossTonnageInternational",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GrossTonnagePanamaCanal",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GrossTonnageSuezCanal",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GsmPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HMaxAirdraft",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HarbourGeneratorMaker",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HarbourGeneratorMaxPowerKW",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HfoCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HmClubZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatFax1",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatFax2",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatPhone1",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatPhone2",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatTelex1",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InmarsatTelex2",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KeelLaidDate",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEdgeSyncAt",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastShoreSyncAt",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lbp",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LightShip",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Loa",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LubOilCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagingOwnerZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ManifoldToBridge",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ManifoldToKeel",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ManifoldToWaterlineBallast",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ManifoldToWaterlineLoaded",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxAllowablePressurePsi",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxLoadingRateShip",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPassengersAllowedOB",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPersonsAllowedOB",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MdoCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MmsiNumber",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Navtex",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "NettTonnageInternational",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NettTonnagePanamaCanal",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NettTonnageSuezCanal",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfBallastTanks",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfCargoHolds",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfCrewSafeManning",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfHatches",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfLines",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRadioEquipment",
                table: "Vessels",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerImoNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanamaCanalIdNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ParallelBodyBallast",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ParallelBodyLoaded",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PiClubZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortOfRegistry",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousFlag",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousName",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaFirstName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaLastName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaPhone24h",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaTitle",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiPanamaZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaFirstName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaLastName",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaPhone24h",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaTitle",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QiUsaZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelegraphHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelegraphMF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelegraphVHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelephoneHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelephoneMF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RadiotelephoneVHF",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Radiotelex",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SartTransponder",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SeaAreaA1",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SeaAreaA2",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SeaAreaA3",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SeaAreaA4",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ServiceSpeedKts",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SewageCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShaftGeneratorNA",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerCity",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerContactPerson",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerEmail",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerFax",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerName",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerPhone",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerStreet",
                table: "Vessels",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerTlx",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipownerZip",
                table: "Vessels",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShipsideToManifold",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipyardCountry",
                table: "Vessels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipyardName",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SludgeCbm",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SternToManifold",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SternthrusterNA",
                table: "Vessels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SuezCanalIdNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeuOnDeck",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeuTotal",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeuUnderDeck",
                table: "Vessels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TpcAtSummerDraft",
                table: "Vessels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vessels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VentingSystemShip",
                table: "Vessels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VrpNumber",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VrpType",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YardNo",
                table: "Vessels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "Vessels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirdraftReductionMastFouled",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Ais",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AnchorChainPort",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AnchorChainStarboard",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AnchorChainStern",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AnchorChainSternNA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AzimuthEngAftCount",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AzimuthEngAftMaxPowerKW",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AzimuthEngFwdCount",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "AzimuthEngFwdMaxPowerKW",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BalesCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BallastWaterCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BareboatChartererZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BilgeWaterCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BlockCoefficient",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BlockCoefficientNA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BowToBulbousBow",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BowToManifold",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BowthrusterNA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BreadthMoulded",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BridgeToAft",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "BridgeToBow",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ChartererZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassNotation",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassRegisterNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ClassSocietyZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CompanyImoNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoFirstName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoLastName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoPhone24h",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoTitle",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "CsoZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DDistance",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DateOfRegistry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DeckToManifold",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DepthMoulded",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaFirstName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaLastName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaPhone24h",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaTitle",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DpaZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DraftFullBallast",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DraftMoulded",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DraftScantling",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DscHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DscMF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "DscVHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EmailAddress1",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EmailAddress2",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EpirbFrequency",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EpirbMaker",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EpirbModel",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EpirbNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "EpirbOperatingSystem",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FieldOwnership",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStatePhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FlagStateZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FreshWaterAllowanceFwa",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "FreshWaterCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "GrainCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "GrossTonnageInternational",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "GrossTonnagePanamaCanal",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "GrossTonnageSuezCanal",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "GsmPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HMaxAirdraft",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HarbourGeneratorMaker",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HarbourGeneratorMaxPowerKW",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HfoCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "HmClubZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatFax1",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatFax2",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatPhone1",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatPhone2",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatTelex1",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "InmarsatTelex2",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "KeelLaidDate",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "LastEdgeSyncAt",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "LastShoreSyncAt",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Lbp",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "LightShip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Loa",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "LubOilCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManagingOwnerZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManifoldToBridge",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManifoldToKeel",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManifoldToWaterlineBallast",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ManifoldToWaterlineLoaded",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MaxAllowablePressurePsi",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MaxLoadingRateShip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MaxPassengersAllowedOB",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MaxPersonsAllowedOB",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MdoCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "MmsiNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Navtex",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NettTonnageInternational",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NettTonnagePanamaCanal",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NettTonnageSuezCanal",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NoOfBallastTanks",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NoOfCargoHolds",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NoOfCrewSafeManning",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NoOfHatches",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "NumberOfLines",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OfficialNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OperatorZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OtherRadioEquipment",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "OwnerImoNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PanamaCanalIdNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ParallelBodyBallast",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ParallelBodyLoaded",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PiClubZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PortOfRegistry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PreviousFlag",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "PreviousName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaFirstName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaLastName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaPhone24h",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaTitle",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiPanamaZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaFirstName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaLastName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaPhone24h",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaTitle",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "QiUsaZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelegraphHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelegraphMF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelegraphVHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelephoneHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelephoneMF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "RadiotelephoneVHF",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "Radiotelex",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SartTransponder",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SeaAreaA1",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SeaAreaA2",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SeaAreaA3",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SeaAreaA4",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ServiceSpeedKts",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SewageCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShaftGeneratorNA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerCity",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerContactPerson",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerEmail",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerFax",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerPhone",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerStreet",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerTlx",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipownerZip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipsideToManifold",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipyardCountry",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "ShipyardName",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SludgeCbm",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SternToManifold",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SternthrusterNA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "SuezCanalIdNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "TeuOnDeck",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "TeuTotal",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "TeuUnderDeck",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "TpcAtSummerDraft",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "VentingSystemShip",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "VrpNumber",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "VrpType",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "YardNo",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "Vessels");

            migrationBuilder.AlterColumn<string>(
                name: "VesselType",
                table: "Vessels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vessels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "IMO",
                table: "Vessels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Flag",
                table: "Vessels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CallSign",
                table: "Vessels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
