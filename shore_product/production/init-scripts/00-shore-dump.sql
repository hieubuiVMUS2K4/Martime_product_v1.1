--
-- PostgreSQL database dump
--

\restrict p3xFAqlyNjiCo9DYc3h6VLgoqr8pEaylm0hNbaL1antbftmWenkfDkVWgsaCMgP

-- Dumped from database version 15.14 (Debian 15.14-1.pgdg13+1)
-- Dumped by pg_dump version 15.14 (Debian 15.14-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AisData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."AisData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "Mmsi" character varying(9) NOT NULL,
    "SpeedOverGround" double precision,
    "Latitude" double precision,
    "Longitude" double precision,
    "CourseOverGround" double precision,
    "ShipName" character varying(120),
    "Destination" character varying(120),
    "EtaMonth" integer,
    "EtaDay" integer,
    "EtaHour" integer,
    "EtaMinute" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."AisData" OWNER TO product;

--
-- Name: Certificates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."Certificates" (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "CertificateType" text NOT NULL,
    "CertificateName" text NOT NULL,
    "IssuingAuthority" text NOT NULL,
    "IssueDate" timestamp with time zone NOT NULL,
    "ExpiryDate" timestamp with time zone NOT NULL,
    "CertificateNumber" text NOT NULL,
    "IsValid" boolean NOT NULL,
    "DocumentPath" text
);


ALTER TABLE public."Certificates" OWNER TO product;

--
-- Name: EngineData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."EngineData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "EngineId" character varying(50) NOT NULL,
    "Rpm" double precision,
    "LoadPercent" double precision,
    "FuelRate" double precision,
    "RunningHours" double precision,
    "AlarmStatus" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."EngineData" OWNER TO product;

--
-- Name: FuelConsumptionData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."FuelConsumptionData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "FuelType" character varying(20) NOT NULL,
    "ConsumedVolume" double precision NOT NULL,
    "ConsumedMass" double precision NOT NULL,
    "TankId" character varying(50),
    "Density" double precision,
    "DistanceTraveled" double precision,
    "TimeUnderway" double precision,
    "CargoWeight" double precision,
    "Co2Emissions" double precision,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."FuelConsumptionData" OWNER TO product;

--
-- Name: FuelConsumptions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."FuelConsumptions" (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "ReportDate" timestamp with time zone NOT NULL,
    "FuelConsumed" double precision NOT NULL,
    "FuelType" text NOT NULL,
    "DistanceTraveled" double precision NOT NULL,
    "AverageSpeed" double precision NOT NULL,
    "FuelEfficiency" double precision NOT NULL
);


ALTER TABLE public."FuelConsumptions" OWNER TO product;

--
-- Name: GeneratorData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."GeneratorData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "GeneratorId" character varying(50) NOT NULL,
    "IsRunning" boolean NOT NULL,
    "RunningHours" double precision,
    "LoadPercent" double precision,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."GeneratorData" OWNER TO product;

--
-- Name: MaintenanceTasks; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."MaintenanceTasks" (
    "Id" uuid NOT NULL,
    "TaskDescription" text NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "ActualDuration" integer,
    "ActualRunningHours" double precision,
    "AssignedDepartment" character varying(20),
    "AssignedTo" character varying(100),
    "CancellationReason" text,
    "CancelledAt" timestamp with time zone,
    "CancelledBy" character varying(50),
    "ChecklistCompleted" boolean DEFAULT false NOT NULL,
    "CompletedAt" timestamp with time zone,
    "CompletedBy" character varying(100),
    "CreatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    "DeferralCount" integer DEFAULT 0 NOT NULL,
    "DeletedAt" timestamp with time zone,
    "EquipmentAssetId" uuid,
    "EquipmentAssetName" character varying(200),
    "EquipmentGroupId" uuid,
    "EquipmentGroupName" character varying(200),
    "EquipmentId" character varying(100),
    "EquipmentName" character varying(200),
    "EstimatedDuration" integer,
    "HasPendingDeferral" boolean DEFAULT false NOT NULL,
    "IntervalDays" integer,
    "IntervalHours" double precision,
    "IsCms" boolean DEFAULT false NOT NULL,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "IsSynced" boolean DEFAULT false NOT NULL,
    "LastDeferredAt" timestamp with time zone,
    "LastDeferredBy" character varying(50),
    "LastDoneAt" timestamp with time zone,
    "LastRejectedAt" timestamp with time zone,
    "LastRejectedBy" character varying(50),
    "NextDueAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    "Notes" text,
    "OriginNode" character varying(50) DEFAULT ''::character varying NOT NULL,
    "PhotosUploaded" integer DEFAULT 0 NOT NULL,
    "Priority" character varying(20) DEFAULT ''::character varying NOT NULL,
    "RejectionCount" integer DEFAULT 0 NOT NULL,
    "RejectionHistory" jsonb,
    "RejectionReason" text,
    "RequiredPhotos" integer DEFAULT 0 NOT NULL,
    "RequiredSpareParts" character varying(4000),
    "RunningHoursAtLastDone" double precision,
    "ScheduleId" uuid,
    "SparePartsUsed" character varying(4000),
    "StartedAt" timestamp with time zone,
    "StartedBy" character varying(50),
    "Status" character varying(20) DEFAULT ''::character varying NOT NULL,
    "SubmittedAt" timestamp with time zone,
    "SubmittedBy" character varying(50),
    "SyncedAt" timestamp with time zone,
    "TaskId" character varying(50) DEFAULT ''::character varying NOT NULL,
    "TaskType" character varying(50) DEFAULT ''::character varying NOT NULL,
    "TaskTypeId" integer,
    "VerificationNotes" text,
    "VerificationResult" character varying(20),
    "VerifiedAt" timestamp with time zone,
    "VerifiedBy" character varying(50),
    "VesselId" uuid
);


ALTER TABLE public."MaintenanceTasks" OWNER TO product;

--
-- Name: PortCalls; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."PortCalls" (
    "Id" uuid NOT NULL,
    "VesselId" uuid,
    "PortCode" text NOT NULL,
    "PortName" text NOT NULL,
    "ArrivalTime" timestamp with time zone,
    "DepartureTime" timestamp with time zone,
    "PortFees" numeric(12,2) NOT NULL,
    "CargoQuantity" numeric(10,3),
    "CargoType" text NOT NULL,
    "Purpose" text NOT NULL,
    "BerthNumber" text,
    "CallType" text DEFAULT ''::text NOT NULL,
    "CargoOpsCompleted" boolean DEFAULT false NOT NULL,
    "Country" text,
    "CreatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    "DraftAft" double precision,
    "DraftFore" double precision,
    "IsSynced" boolean DEFAULT false NOT NULL,
    "OriginNode" text DEFAULT ''::text NOT NULL,
    "PilotOffBoard" timestamp with time zone,
    "PilotOnBoard" timestamp with time zone,
    "PortId" integer,
    "Remarks" text,
    "Sequence" integer DEFAULT 0 NOT NULL,
    "SyncVersion" bigint DEFAULT 0 NOT NULL,
    "UpdatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL,
    "VoyageId" uuid,
    "VoyagePlanLegId" uuid
);


ALTER TABLE public."PortCalls" OWNER TO product;

--
-- Name: PositionData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."PositionData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "SpeedOverGround" double precision,
    "CourseOverGround" double precision,
    "Source" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."PositionData" OWNER TO product;

--
-- Name: SafetyAlarms; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."SafetyAlarms" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "AlarmType" character varying(50) NOT NULL,
    "AlarmCode" character varying(50),
    "Severity" character varying(20) NOT NULL,
    "Location" character varying(100),
    "Description" character varying(500),
    "IsAcknowledged" boolean NOT NULL,
    "AcknowledgedAt" timestamp with time zone,
    "AcknowledgedBy" character varying(100),
    "IsResolved" boolean NOT NULL,
    "ResolvedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."SafetyAlarms" OWNER TO product;

--
-- Name: Ships; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."Ships" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "IMO" text NOT NULL,
    "Capacity" integer NOT NULL
);


ALTER TABLE public."Ships" OWNER TO product;

--
-- Name: TankLevels; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."TankLevels" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "TankId" character varying(50) NOT NULL,
    "TankType" character varying(20) NOT NULL,
    "LevelPercent" double precision NOT NULL,
    "VolumeLiters" double precision,
    "Temperature" double precision,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."TankLevels" OWNER TO product;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Username" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Role" text NOT NULL
);


ALTER TABLE public."Users" OWNER TO product;

--
-- Name: VesselAlerts; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."VesselAlerts" (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AlertType" text NOT NULL,
    "Message" text NOT NULL,
    "Severity" text NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "IsAcknowledged" boolean NOT NULL,
    "AcknowledgedAt" timestamp with time zone,
    "AcknowledgedBy" text,
    "Data" text NOT NULL
);


ALTER TABLE public."VesselAlerts" OWNER TO product;

--
-- Name: VesselPositions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."VesselPositions" (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "Speed" double precision,
    "Course" double precision,
    "Timestamp" timestamp with time zone NOT NULL,
    "Source" text NOT NULL
);


ALTER TABLE public."VesselPositions" OWNER TO product;

--
-- Name: Vessels; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."Vessels" (
    "Id" uuid NOT NULL,
    "IMO" character varying(20) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "CallSign" character varying(20) NOT NULL,
    "VesselType" character varying(200) NOT NULL,
    "GrossTonnage" double precision NOT NULL,
    "DeadWeight" double precision NOT NULL,
    "BuildDate" timestamp with time zone NOT NULL,
    "Flag" character varying(100) NOT NULL,
    "IsActive" boolean NOT NULL,
    "OfficialNumber" character varying(50),
    "PortOfRegistry" character varying(200),
    "PreviousName" character varying(200),
    "PreviousFlag" character varying(100),
    "MmsiNumber" character varying(20),
    "ClassNotation" character varying(200),
    "ClassRegisterNumber" character varying(50),
    "ShipyardCountry" character varying(100),
    "ShipyardName" character varying(200),
    "YardNo" character varying(50),
    "CompanyImoNumber" character varying(50),
    "SuezCanalIdNumber" character varying(50),
    "KeelLaidDate" timestamp with time zone,
    "YearBuilt" integer,
    "DateOfRegistry" timestamp with time zone,
    "OwnerImoNumber" character varying(50),
    "PanamaCanalIdNumber" character varying(50),
    "MaxPersonsAllowedOB" integer,
    "ServiceSpeedKts" double precision,
    "VrpNumber" character varying(50),
    "VrpType" character varying(50),
    "NoOfCrewSafeManning" integer,
    "MaxPassengersAllowedOB" integer,
    "Loa" double precision,
    "Lbp" double precision,
    "BreadthMoulded" double precision,
    "DepthMoulded" double precision,
    "DraftMoulded" double precision,
    "DraftScantling" double precision,
    "DraftFullBallast" double precision,
    "HMaxAirdraft" double precision,
    "AirdraftReductionMastFouled" double precision,
    "DDistance" double precision,
    "BridgeToAft" double precision,
    "BridgeToBow" double precision,
    "BowToBulbousBow" double precision,
    "ParallelBodyBallast" double precision,
    "ParallelBodyLoaded" double precision,
    "LightShip" double precision,
    "BlockCoefficientNA" boolean NOT NULL,
    "BlockCoefficient" double precision,
    "TpcAtSummerDraft" double precision,
    "FreshWaterAllowanceFwa" double precision,
    "GrossTonnageInternational" double precision,
    "GrossTonnageSuezCanal" double precision,
    "GrossTonnagePanamaCanal" double precision,
    "NettTonnageInternational" double precision,
    "NettTonnageSuezCanal" double precision,
    "NettTonnagePanamaCanal" double precision,
    "ManifoldToWaterlineBallast" double precision,
    "ManifoldToWaterlineLoaded" double precision,
    "DeckToManifold" double precision,
    "SternToManifold" double precision,
    "ShipsideToManifold" double precision,
    "BowToManifold" double precision,
    "ManifoldToKeel" double precision,
    "ManifoldToBridge" double precision,
    "MaxLoadingRateShip" double precision,
    "NumberOfLines" integer,
    "MaxAllowablePressurePsi" double precision,
    "VentingSystemShip" character varying(200),
    "AnchorChainPort" integer,
    "AnchorChainStarboard" integer,
    "AnchorChainStern" integer,
    "AnchorChainSternNA" boolean NOT NULL,
    "BowthrusterNA" boolean NOT NULL,
    "SternthrusterNA" boolean NOT NULL,
    "ShaftGeneratorNA" boolean NOT NULL,
    "HarbourGeneratorMaker" character varying(200),
    "HarbourGeneratorMaxPowerKW" double precision,
    "AzimuthEngFwdCount" integer,
    "AzimuthEngFwdMaxPowerKW" double precision,
    "AzimuthEngAftCount" integer,
    "AzimuthEngAftMaxPowerKW" double precision,
    "ShipownerName" character varying(300),
    "ShipownerStreet" character varying(300),
    "ShipownerCountry" character varying(100),
    "ShipownerZip" character varying(20),
    "ShipownerCity" character varying(100),
    "ShipownerPhone" character varying(50),
    "ShipownerFax" character varying(50),
    "ShipownerTlx" character varying(50),
    "ShipownerEmail" character varying(200),
    "ShipownerContactPerson" character varying(200),
    "ManagingOwnerName" character varying(300),
    "ManagingOwnerStreet" character varying(300),
    "ManagingOwnerCountry" character varying(100),
    "ManagingOwnerZip" character varying(20),
    "ManagingOwnerCity" character varying(100),
    "ManagingOwnerPhone" character varying(50),
    "ManagingOwnerFax" character varying(50),
    "ManagingOwnerTlx" character varying(50),
    "ManagingOwnerEmail" character varying(200),
    "ManagingOwnerContactPerson" character varying(200),
    "OperatorName" character varying(300),
    "OperatorStreet" character varying(300),
    "OperatorCountry" character varying(100),
    "OperatorZip" character varying(20),
    "OperatorCity" character varying(100),
    "OperatorPhone" character varying(50),
    "OperatorFax" character varying(50),
    "OperatorTlx" character varying(50),
    "OperatorEmail" character varying(200),
    "OperatorContactPerson" character varying(200),
    "CsoTitle" character varying(20),
    "CsoFirstName" character varying(100),
    "CsoLastName" character varying(100),
    "CsoStreet" character varying(300),
    "CsoCountry" character varying(100),
    "CsoZip" character varying(20),
    "CsoCity" character varying(100),
    "CsoPhone24h" character varying(50),
    "CsoFax" character varying(50),
    "CsoTlx" character varying(50),
    "CsoEmail" character varying(200),
    "DpaTitle" character varying(20),
    "DpaFirstName" character varying(100),
    "DpaLastName" character varying(100),
    "DpaStreet" character varying(300),
    "DpaCountry" character varying(100),
    "DpaZip" character varying(20),
    "DpaCity" character varying(100),
    "DpaPhone24h" character varying(50),
    "DpaFax" character varying(50),
    "DpaTlx" character varying(50),
    "DpaEmail" character varying(200),
    "QiUsaTitle" character varying(20),
    "QiUsaFirstName" character varying(100),
    "QiUsaLastName" character varying(100),
    "QiUsaStreet" character varying(300),
    "QiUsaCountry" character varying(100),
    "QiUsaZip" character varying(20),
    "QiUsaCity" character varying(100),
    "QiUsaPhone24h" character varying(50),
    "QiUsaFax" character varying(50),
    "QiUsaTlx" character varying(50),
    "QiUsaEmail" character varying(200),
    "QiPanamaTitle" character varying(20),
    "QiPanamaFirstName" character varying(100),
    "QiPanamaLastName" character varying(100),
    "QiPanamaStreet" character varying(300),
    "QiPanamaCountry" character varying(100),
    "QiPanamaZip" character varying(20),
    "QiPanamaCity" character varying(100),
    "QiPanamaPhone24h" character varying(50),
    "QiPanamaFax" character varying(50),
    "QiPanamaTlx" character varying(50),
    "QiPanamaEmail" character varying(200),
    "ChartererName" character varying(300),
    "ChartererStreet" character varying(300),
    "ChartererCountry" character varying(100),
    "ChartererZip" character varying(20),
    "ChartererCity" character varying(100),
    "ChartererPhone" character varying(50),
    "ChartererFax" character varying(50),
    "ChartererTlx" character varying(50),
    "ChartererEmail" character varying(200),
    "ChartererContactPerson" character varying(200),
    "BareboatChartererName" character varying(300),
    "BareboatChartererStreet" character varying(300),
    "BareboatChartererCountry" character varying(100),
    "BareboatChartererZip" character varying(20),
    "BareboatChartererCity" character varying(100),
    "BareboatChartererPhone" character varying(50),
    "BareboatChartererFax" character varying(50),
    "BareboatChartererTlx" character varying(50),
    "BareboatChartererEmail" character varying(200),
    "BareboatChartererContactPerson" character varying(200),
    "ClassSocietyName" character varying(300),
    "ClassSocietyStreet" character varying(300),
    "ClassSocietyCountry" character varying(100),
    "ClassSocietyZip" character varying(20),
    "ClassSocietyCity" character varying(100),
    "ClassSocietyPhone" character varying(50),
    "ClassSocietyFax" character varying(50),
    "ClassSocietyTlx" character varying(50),
    "ClassSocietyEmail" character varying(200),
    "ClassSocietyContactPerson" character varying(200),
    "FlagStateName" character varying(300),
    "FlagStateStreet" character varying(300),
    "FlagStateCountry" character varying(100),
    "FlagStateZip" character varying(20),
    "FlagStateCity" character varying(100),
    "FlagStatePhone" character varying(50),
    "FlagStateFax" character varying(50),
    "FlagStateTlx" character varying(50),
    "FlagStateEmail" character varying(200),
    "FlagStateContactPerson" character varying(200),
    "PiClubName" character varying(300),
    "PiClubStreet" character varying(300),
    "PiClubCountry" character varying(100),
    "PiClubZip" character varying(20),
    "PiClubCity" character varying(100),
    "PiClubPhone" character varying(50),
    "PiClubFax" character varying(50),
    "PiClubTlx" character varying(50),
    "PiClubEmail" character varying(200),
    "PiClubContactPerson" character varying(200),
    "HmClubName" character varying(300),
    "HmClubStreet" character varying(300),
    "HmClubCountry" character varying(100),
    "HmClubZip" character varying(20),
    "HmClubCity" character varying(100),
    "HmClubPhone" character varying(50),
    "HmClubFax" character varying(50),
    "HmClubTlx" character varying(50),
    "HmClubEmail" character varying(200),
    "HmClubContactPerson" character varying(200),
    "InmarsatTelex1" character varying(50),
    "InmarsatTelex2" character varying(50),
    "InmarsatPhone1" character varying(50),
    "InmarsatPhone2" character varying(50),
    "InmarsatFax1" character varying(50),
    "InmarsatFax2" character varying(50),
    "EmailAddress1" character varying(200),
    "EmailAddress2" character varying(200),
    "GsmPhone" character varying(50),
    "SeaAreaA1" boolean NOT NULL,
    "SeaAreaA2" boolean NOT NULL,
    "SeaAreaA3" boolean NOT NULL,
    "SeaAreaA4" boolean NOT NULL,
    "DscHF" boolean NOT NULL,
    "DscMF" boolean NOT NULL,
    "DscVHF" boolean NOT NULL,
    "RadiotelephoneHF" boolean NOT NULL,
    "RadiotelephoneMF" boolean NOT NULL,
    "RadiotelephoneVHF" boolean NOT NULL,
    "RadiotelegraphHF" boolean NOT NULL,
    "RadiotelegraphMF" boolean NOT NULL,
    "RadiotelegraphVHF" boolean NOT NULL,
    "Navtex" boolean NOT NULL,
    "Ais" boolean NOT NULL,
    "SartTransponder" boolean NOT NULL,
    "Radiotelex" boolean NOT NULL,
    "OtherRadioEquipment" character varying(500),
    "EpirbNumber" character varying(50),
    "EpirbOperatingSystem" character varying(50),
    "EpirbMaker" character varying(100),
    "EpirbModel" character varying(100),
    "EpirbFrequency" character varying(50),
    "HfoCbm" double precision,
    "MdoCbm" double precision,
    "LubOilCbm" double precision,
    "SludgeCbm" double precision,
    "BilgeWaterCbm" double precision,
    "SewageCbm" double precision,
    "FreshWaterCbm" double precision,
    "BallastWaterCbm" double precision,
    "NoOfBallastTanks" integer,
    "TeuTotal" integer,
    "TeuOnDeck" integer,
    "TeuUnderDeck" integer,
    "GrainCbm" double precision,
    "BalesCbm" double precision,
    "NoOfCargoHolds" integer,
    "NoOfHatches" integer,
    "LastEdgeSyncAt" timestamp with time zone,
    "LastShoreSyncAt" timestamp with time zone,
    "FieldOwnership" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Vessels" OWNER TO product;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO product;

--
-- Name: arrival_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.arrival_reports (
    "Id" uuid NOT NULL,
    "MaritimeReportId" uuid NOT NULL,
    "PortName" character varying(200),
    "PortCode" character varying(10),
    "ArrivalDateTime" timestamp with time zone,
    "PilotOnBoardTime" timestamp with time zone,
    "ArrivalLatitude" double precision,
    "ArrivalLongitude" double precision,
    "VoyageDistance" double precision,
    "VoyageDuration" double precision,
    "AverageSpeed" double precision,
    "DraftForward" double precision,
    "DraftAft" double precision,
    "DraftMidship" double precision,
    "TotalFuelConsumed" double precision,
    "TotalDieselConsumed" double precision,
    "FuelOilROB" double precision,
    "DieselOilROB" double precision,
    "LubOilROB" double precision,
    "FreshWaterROB" double precision,
    "CargoOnBoard" double precision,
    "CargoDescription" character varying(200),
    "PassengersOnBoard" integer,
    "Remarks" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CrewOnBoard" integer,
    "FirstLineAshoreTime" timestamp with time zone,
    "VoyageId" uuid
);


ALTER TABLE public.arrival_reports OWNER TO product;

--
-- Name: assignment_comments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.assignment_comments (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "Author" character varying(100) NOT NULL,
    "AuthorRole" character varying(50),
    "Content" character varying(2000) NOT NULL,
    "PostedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.assignment_comments OWNER TO product;

--
-- Name: assignment_confirmations; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.assignment_confirmations (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "Response" character varying(30) NOT NULL,
    "RespondedAt" timestamp with time zone,
    "RespondedBy" character varying(100),
    "DeclineReason" character varying(1000),
    "Notes" character varying(1000),
    "SentAt" timestamp with time zone NOT NULL,
    "SentBy" character varying(100)
);


ALTER TABLE public.assignment_confirmations OWNER TO product;

--
-- Name: assignment_conflicts; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.assignment_conflicts (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "ConflictType" character varying(50) NOT NULL,
    "Severity" character varying(20) NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "RelatedEntityId" uuid,
    "RelatedEntityType" character varying(100),
    "IsResolved" boolean NOT NULL,
    "ResolutionNote" character varying(500),
    "DetectedAt" timestamp with time zone NOT NULL,
    "ResolvedAt" timestamp with time zone
);


ALTER TABLE public.assignment_conflicts OWNER TO product;

--
-- Name: assignment_status_history; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.assignment_status_history (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "FromStatus" character varying(50) NOT NULL,
    "ToStatus" character varying(50) NOT NULL,
    "ChangedBy" character varying(100),
    "Reason" character varying(500),
    "ChangedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.assignment_status_history OWNER TO product;

--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.audit_logs (
    "Id" uuid NOT NULL,
    "Action" character varying(30) NOT NULL,
    "EntityType" character varying(100) NOT NULL,
    "EntityId" character varying(100) NOT NULL,
    "Actor" character varying(100) NOT NULL,
    "SourceChannel" character varying(20),
    "BeforeState" text,
    "AfterState" text,
    "CorrelationId" character varying(100),
    "Details" character varying(1000),
    "IpAddress" character varying(50),
    "Timestamp" timestamp with time zone NOT NULL
);


ALTER TABLE public.audit_logs OWNER TO product;

--
-- Name: bunker_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.bunker_reports (
    "Id" uuid NOT NULL,
    "MaritimeReportId" uuid NOT NULL,
    "BunkerDate" timestamp with time zone NOT NULL,
    "PortName" character varying(200),
    "PortCode" character varying(10),
    "SupplierName" character varying(200),
    "BDNNumber" character varying(50),
    "FuelType" character varying(20),
    "FuelGrade" character varying(50),
    "QuantityReceived" double precision,
    "Density" double precision,
    "SulphurContent" double precision,
    "Viscosity" double precision,
    "FlashPoint" double precision,
    "ROBefore" double precision,
    "ROBAfter" double precision,
    "TanksLoaded" character varying(200),
    "SealNumbers" character varying(200),
    "ChiefEngineerSignature" character varying(100),
    "Remarks" text,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.bunker_reports OWNER TO product;

--
-- Name: cargo_operations; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.cargo_operations (
    "Id" uuid NOT NULL,
    "OperationId" character varying(50) NOT NULL,
    "VoyageId" uuid,
    "VoyagePlanLegId" uuid,
    "OperationType" character varying(20) NOT NULL,
    "CargoType" character varying(100) NOT NULL,
    "CargoDescription" text,
    "Quantity" double precision NOT NULL,
    "Unit" character varying(20) NOT NULL,
    "LoadingPort" character varying(100),
    "DischargePort" character varying(100),
    "LoadedAt" timestamp with time zone,
    "DischargedAt" timestamp with time zone,
    "Shipper" character varying(200),
    "Consignee" character varying(200),
    "BillOfLading" character varying(100),
    "SealNumbers" character varying(500),
    "SpecialRequirements" text,
    "Status" character varying(20) NOT NULL,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.cargo_operations OWNER TO product;

--
-- Name: certificates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.certificates (
    "Id" integer NOT NULL,
    "CertificateCode" character varying(50) NOT NULL,
    "CertificateName" character varying(200) NOT NULL,
    "Category" character varying(50),
    "ValidityPeriodMonths" integer,
    "Description" text,
    "IsMandatory" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.certificates OWNER TO product;

--
-- Name: certificates_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.certificates ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."certificates_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: compliance_dimensions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.compliance_dimensions (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "DimensionType" character varying(50) NOT NULL,
    "Operator" character varying(20) NOT NULL,
    "Value" character varying(500) NOT NULL
);


ALTER TABLE public.compliance_dimensions OWNER TO product;

--
-- Name: compliance_rule_sets; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.compliance_rule_sets (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50),
    "Description" text,
    "Authority" character varying(100),
    "IsActive" boolean NOT NULL,
    "EffectiveFrom" timestamp with time zone,
    "EffectiveTo" timestamp with time zone,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.compliance_rule_sets OWNER TO product;

--
-- Name: compliance_rules; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.compliance_rules (
    "Id" uuid NOT NULL,
    "RuleSetId" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" text,
    "RequirementType" character varying(50) NOT NULL,
    "RequiredCertificateId" integer,
    "RequiredDocumentType" character varying(100),
    "Severity" character varying(20) NOT NULL,
    "EvaluationStage" character varying(30) NOT NULL,
    "MinDaysBeforeExpiry" integer,
    "GracePeriodDays" integer,
    "RenewWindowDays" integer,
    "WaiverAllowed" boolean NOT NULL,
    "WaiverApproverRole" character varying(100),
    "AllowEquivalent" boolean NOT NULL,
    "EquivalentCertificateIds" text,
    "IsActive" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "UiMessage" character varying(500),
    "ExplainabilityText" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.compliance_rules OWNER TO product;

--
-- Name: compliance_snapshots; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.compliance_snapshots (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid,
    "OverallResult" character varying(30) NOT NULL,
    "TotalRules" integer NOT NULL,
    "RulesMet" integer NOT NULL,
    "RulesNotMet" integer NOT NULL,
    "RulesWarning" integer NOT NULL,
    "RulesWaived" integer NOT NULL,
    "EvaluationDetails" jsonb,
    "EvaluatedAt" timestamp with time zone NOT NULL,
    "EvaluationStage" character varying(30),
    "NextExpiryDate" timestamp with time zone
);


ALTER TABLE public.compliance_snapshots OWNER TO product;

--
-- Name: compliance_waivers; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.compliance_waivers (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid,
    "Status" character varying(30) NOT NULL,
    "Reason" text NOT NULL,
    "Conditions" text,
    "RequestedAt" timestamp with time zone NOT NULL,
    "RequestedBy" character varying(100) NOT NULL,
    "ApprovedAt" timestamp with time zone,
    "ApprovedBy" character varying(100),
    "ApprovalNotes" text,
    "ValidFrom" timestamp with time zone,
    "ValidTo" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.compliance_waivers OWNER TO product;

--
-- Name: countries; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.countries (
    "Id" integer NOT NULL,
    "CountryCode" character varying(3) NOT NULL,
    "CountryName" character varying(100) NOT NULL,
    "FlagImageUrl" character varying(255),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.countries OWNER TO product;

--
-- Name: countries_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.countries ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."countries_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: country_certificates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.country_certificates (
    "Id" integer NOT NULL,
    "CountryId" integer NOT NULL,
    "CertificateId" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.country_certificates OWNER TO product;

--
-- Name: country_certificates_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.country_certificates ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."country_certificates_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: crew_access_grants; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_access_grants (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignmentId" uuid,
    "Status" character varying(30) NOT NULL,
    "Module" character varying(100) NOT NULL,
    "GrantedAt" timestamp with time zone,
    "RevokedAt" timestamp with time zone,
    "RevokeReason" character varying(500),
    "GrantedBy" character varying(100),
    "RevokedBy" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.crew_access_grants OWNER TO product;

--
-- Name: crew_assignments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_assignments (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "RankId" integer NOT NULL,
    "ManningPositionId" uuid,
    "Status" character varying(50) NOT NULL,
    "StatusChangedAt" timestamp with time zone,
    "StatusChangedBy" character varying(100),
    "PlannedStartDate" timestamp with time zone,
    "PlannedEndDate" timestamp with time zone,
    "ActualStartDate" timestamp with time zone,
    "ActualEndDate" timestamp with time zone,
    "JoinPortCode" character varying(20),
    "JoinPortName" character varying(200),
    "LeavePortCode" character varying(20),
    "LeavePortName" character varying(200),
    "IsEquivalentRank" boolean NOT NULL,
    "OriginalRankId" integer,
    "EquivalentRankJustification" character varying(500),
    "ComplianceResult" character varying(50),
    "ComplianceEvaluatedAt" timestamp with time zone,
    "Notes" character varying(2000),
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100)
);


ALTER TABLE public.crew_assignments OWNER TO product;

--
-- Name: crew_certificates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_certificates (
    "Id" integer NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "CertificateId" integer NOT NULL,
    "CertificateNumber" character varying(100) NOT NULL,
    "IssueDate" timestamp with time zone NOT NULL,
    "ExpiryDate" timestamp with time zone NOT NULL,
    "IssuingAuthority" character varying(200),
    "CertificateOfCompetency" character varying(200),
    "CountryId" integer,
    "DocumentFilePath" character varying(500),
    "Status" character varying(20) NOT NULL,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.crew_certificates OWNER TO product;

--
-- Name: crew_certificates_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.crew_certificates ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."crew_certificates_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: crew_certificates_id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

CREATE SEQUENCE public.crew_certificates_id_seq
    START WITH 77
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.crew_certificates_id_seq OWNER TO product;

--
-- Name: crew_certificates_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: product
--

ALTER SEQUENCE public.crew_certificates_id_seq OWNED BY public.crew_certificates."Id";


--
-- Name: crew_document_submissions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_document_submissions (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "DocumentType" character varying(50) NOT NULL,
    "DocumentTitle" character varying(200),
    "DocumentNumber" character varying(100),
    "IssuingAuthority" character varying(200),
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "IssuingCountryId" integer,
    "Status" character varying(20) NOT NULL,
    "StatusChangedAt" timestamp with time zone,
    "StatusChangedBy" character varying(100),
    "OnboardingCaseId" uuid,
    "IsActiveSubmission" boolean NOT NULL,
    "SensitivityLevel" character varying(20) NOT NULL,
    "SubmittedBy" character varying(100),
    "SubmittedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.crew_document_submissions OWNER TO product;

--
-- Name: crew_document_versions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_document_versions (
    "Id" uuid NOT NULL,
    "SubmissionId" uuid NOT NULL,
    "VersionNumber" integer NOT NULL,
    "FilePath" character varying(500),
    "OriginalFileName" character varying(300),
    "ContentType" character varying(100),
    "FileSizeBytes" bigint,
    "FileChecksum" character varying(64),
    "IsActiveVersion" boolean NOT NULL,
    "IsLocked" boolean NOT NULL,
    "UploadedBy" character varying(100),
    "UploadedAt" timestamp with time zone NOT NULL,
    "LockedAt" timestamp with time zone
);


ALTER TABLE public.crew_document_versions OWNER TO product;

--
-- Name: crew_members; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_members (
    "Id" uuid NOT NULL,
    "CrewId" character varying(50) NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "RankId" integer,
    "Department" character varying(100),
    "DateOfBirth" timestamp with time zone,
    "JoinDate" timestamp with time zone,
    "EmbarkDate" timestamp with time zone,
    "DisembarkDate" timestamp with time zone,
    "ContractEnd" timestamp with time zone,
    "IsOnboard" boolean NOT NULL,
    "EmergencyContact" character varying(500),
    "EmailAddress" character varying(200),
    "PhoneNumber" character varying(50),
    "Address" character varying(500),
    "PlaceOfBirth" character varying(200),
    "IdCardNumber" character varying(50),
    "MaritalStatus" character varying(20),
    "Height" integer,
    "Weight" numeric(5,2),
    "BloodGroup" character varying(5),
    "ClothingSize" character varying(10),
    "ShoeSize" character varying(10),
    "CateringSize" character varying(10),
    "IsSmoker" boolean,
    "IsCovidVaccinated" boolean,
    "PhotoUrl" character varying(500),
    "NextOfKinName" character varying(200),
    "NextOfKinRelation" character varying(50),
    "NextOfKinPhone" character varying(50),
    "NextOfKinAddress" character varying(500),
    "EducationInstitution" character varying(300),
    "EducationCourse" character varying(200),
    "EducationPeriodYears" integer,
    "EducationGraduationYear" integer,
    "Notes" text,
    "Status" character varying(20) NOT NULL,
    "StatusChangedAt" timestamp with time zone,
    "StatusChangedBy" character varying(100),
    "PoolStatus" character varying(20),
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CountryId" integer,
    "VesselId" uuid,
    "OnboardStatus" character varying(20),
    "OnboardStatusChangedAt" timestamp with time zone,
    "OnboardStatusChangedBy" character varying(100),
    "ReviewChecklist" text,
    "ReviewNotes" text,
    "EdgeChanges" text,
    "EdgeChangesViewed" boolean DEFAULT false
);


ALTER TABLE public.crew_members OWNER TO product;

--
-- Name: crew_status_history; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.crew_status_history (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "FromStatus" character varying(20) NOT NULL,
    "ToStatus" character varying(20) NOT NULL,
    "Reason" character varying(500),
    "ChangedBy" character varying(100) NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.crew_status_history OWNER TO product;

--
-- Name: departure_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.departure_reports (
    "Id" uuid NOT NULL,
    "MaritimeReportId" uuid NOT NULL,
    "PortName" character varying(200),
    "PortCode" character varying(10),
    "DepartureDateTime" timestamp with time zone,
    "LastLineAshoreTime" timestamp with time zone,
    "DepartureLatitude" double precision,
    "DepartureLongitude" double precision,
    "DraftForward" double precision,
    "DraftAft" double precision,
    "DraftMidship" double precision,
    "FuelOilROB" double precision,
    "DieselOilROB" double precision,
    "LubOilROB" double precision,
    "FreshWaterROB" double precision,
    "DistanceToNextPort" double precision,
    "PilotOnBoardTime" timestamp with time zone,
    "NextPort" character varying(100),
    "NextPortCode" character varying(10),
    "CargoOnBoard" double precision,
    "CargoDescription" character varying(200),
    "PassengersOnBoard" integer,
    "Remarks" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CrewOnBoard" integer,
    "EstimatedTimeOfArrival" timestamp with time zone,
    "VoyageId" uuid
);


ALTER TABLE public.departure_reports OWNER TO product;

--
-- Name: document_verification_actions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.document_verification_actions (
    "Id" uuid NOT NULL,
    "TaskId" uuid NOT NULL,
    "ActionType" character varying(30) NOT NULL,
    "ReasonCode" character varying(50),
    "Comment" character varying(2000),
    "PerformedBy" character varying(100) NOT NULL,
    "PerformedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.document_verification_actions OWNER TO product;

--
-- Name: document_verification_tasks; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.document_verification_tasks (
    "Id" uuid NOT NULL,
    "SubmissionId" uuid NOT NULL,
    "VersionId" uuid NOT NULL,
    "AssignedTo" character varying(100),
    "Priority" character varying(20) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "DueAt" timestamp with time zone,
    "StartedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone,
    "Outcome" character varying(20),
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.document_verification_tasks OWNER TO product;

--
-- Name: employment_documents; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.employment_documents (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "DocumentType" character varying(50) NOT NULL,
    "DocumentNumber" character varying(100) NOT NULL,
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "FileUrl" character varying(500),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CountryId" integer
);


ALTER TABLE public.employment_documents OWNER TO product;

--
-- Name: equipment_assets; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.equipment_assets (
    "Id" uuid NOT NULL,
    "AssetCode" character varying(50) NOT NULL,
    "AssetName" character varying(200) NOT NULL,
    "Category" character varying(50) NOT NULL,
    "Manufacturer" character varying(200),
    "Model" character varying(100),
    "SerialNumber" character varying(100),
    "InstallationDate" timestamp with time zone,
    "CurrentRunningHours" double precision,
    "LastRunningHoursUpdate" timestamp with time zone,
    "Location" character varying(100),
    "Criticality" character varying(20) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "DefaultExecutorRole" character varying(50),
    "ApproverRole" character varying(50),
    "TechnicalSpecs" text,
    "Notes" text,
    "ParentId" uuid,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "VesselId" uuid
);


ALTER TABLE public.equipment_assets OWNER TO product;

--
-- Name: equipment_group_members; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.equipment_group_members (
    "Id" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "AssetId" uuid NOT NULL,
    "SequenceOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.equipment_group_members OWNER TO product;

--
-- Name: equipment_groups; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.equipment_groups (
    "Id" uuid NOT NULL,
    "GroupCode" character varying(50) NOT NULL,
    "GroupName" character varying(200) NOT NULL,
    "Category" character varying(50),
    "Department" character varying(50),
    "PicRole" character varying(50),
    "Description" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.equipment_groups OWNER TO product;

--
-- Name: external_candidates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.external_candidates (
    "Id" uuid NOT NULL,
    "ExternalRequestId" uuid NOT NULL,
    "CandidateName" character varying(200) NOT NULL,
    "Nationality" character varying(100),
    "RankId" integer,
    "ContactEmail" character varying(100),
    "ContactPhone" character varying(50),
    "Status" character varying(50) NOT NULL,
    "ComplianceResult" text,
    "ProfileSummary" text,
    "Notes" text,
    "SubmittedAt" timestamp with time zone NOT NULL,
    "SubmittedBy" text,
    "ReviewedAt" timestamp with time zone,
    "ReviewedBy" text,
    "LinkedCrewMemberId" uuid
);


ALTER TABLE public.external_candidates OWNER TO product;

--
-- Name: external_request_messages; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.external_request_messages (
    "Id" uuid NOT NULL,
    "ExternalRequestId" uuid NOT NULL,
    "Author" character varying(100) NOT NULL,
    "AuthorRole" character varying(50),
    "Content" text NOT NULL,
    "PostedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.external_request_messages OWNER TO product;

--
-- Name: external_requests; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.external_requests (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignmentId" uuid,
    "RankId" integer NOT NULL,
    "AgencyName" character varying(200),
    "AgencyEmail" character varying(200),
    "RequiredCount" integer NOT NULL,
    "NationalityPreference" text,
    "RequiredByDate" timestamp with time zone,
    "ResponseSlaDate" timestamp with time zone,
    "Status" character varying(50) NOT NULL,
    "SentAt" timestamp with time zone,
    "ViewedAt" timestamp with time zone,
    "ClosedAt" timestamp with time zone,
    "Notes" text,
    "MandatoryDocuments" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100)
);


ALTER TABLE public.external_requests OWNER TO product;

--
-- Name: health_documents; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.health_documents (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "DocumentType" character varying(50) NOT NULL,
    "DocumentNumber" character varying(100) NOT NULL,
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "FileUrl" character varying(500),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.health_documents OWNER TO product;

--
-- Name: inventory_stocks; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.inventory_stocks (
    "Id" integer NOT NULL,
    "MaterialItemId" uuid NOT NULL,
    "StoreLocationId" uuid NOT NULL,
    "Quantity" numeric(18,4) NOT NULL,
    "UnitCost" numeric(18,4) NOT NULL,
    "LastReceiptDate" timestamp with time zone,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.inventory_stocks OWNER TO product;

--
-- Name: inventory_stocks_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.inventory_stocks ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."inventory_stocks_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: maintenance_histories; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.maintenance_histories (
    "Id" uuid NOT NULL,
    "ScheduleId" uuid NOT NULL,
    "TaskId" uuid,
    "ExecutedAt" timestamp with time zone NOT NULL,
    "ExecutedRunningHours" double precision,
    "CompletedBy" character varying(100),
    "ActualDurationHours" double precision,
    "SparePartsUsed" text,
    "TotalSparePartsCost" numeric,
    "Notes" text,
    "ConditionAfter" character varying(20),
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.maintenance_histories OWNER TO product;

--
-- Name: maintenance_schedules; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.maintenance_schedules (
    "Id" uuid NOT NULL,
    "ScheduleCode" character varying(50) NOT NULL,
    "EquipmentGroupId" uuid,
    "EquipmentAssetId" uuid,
    "ScheduleName" character varying(200) NOT NULL,
    "MaintenanceCategory" character varying(20) NOT NULL,
    "IntervalType" character varying(20) NOT NULL,
    "IntervalHours" integer,
    "IntervalDays" integer,
    "DaysBeforeDue" integer NOT NULL,
    "LastExecutedAt" timestamp with time zone,
    "LastExecutedRunningHours" double precision,
    "NextDueDate" timestamp with time zone,
    "NextDueRunningHours" double precision,
    "Priority" character varying(20) NOT NULL,
    "EstimatedDurationHours" double precision,
    "AutoGenerate" boolean NOT NULL,
    "AssignedToRole" character varying(50),
    "Instructions" text,
    "IsActive" boolean NOT NULL,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.maintenance_schedules OWNER TO product;

--
-- Name: manning_positions; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.manning_positions (
    "Id" uuid NOT NULL,
    "ManningStandardId" uuid NOT NULL,
    "RankId" integer NOT NULL,
    "RequiredCount" integer NOT NULL,
    "AllowEquivalent" boolean NOT NULL,
    "Notes" character varying(500),
    "SortOrder" integer NOT NULL,
    "IsActive" boolean NOT NULL
);


ALTER TABLE public.manning_positions OWNER TO product;

--
-- Name: maritime_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.maritime_reports (
    "Id" uuid NOT NULL,
    "ReportNumber" character varying(50) NOT NULL,
    "ReportTypeId" integer NOT NULL,
    "ReportDateTime" timestamp with time zone NOT NULL,
    "VoyageId" uuid,
    "Status" character varying(30) NOT NULL,
    "PreparedBy" character varying(100),
    "MasterSignature" character varying(100),
    "SignedAt" timestamp with time zone,
    "ReportData" text NOT NULL,
    "Remarks" text,
    "IsTransmitted" boolean NOT NULL,
    "TransmittedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "OriginNode" character varying(50) NOT NULL,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" character varying(100),
    "DeletedReason" text
);


ALTER TABLE public.maritime_reports OWNER TO product;

--
-- Name: material_categories; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.material_categories (
    "Id" bigint NOT NULL,
    "CategoryCode" character varying(50) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" text,
    "ParentCategoryId" bigint,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.material_categories OWNER TO product;

--
-- Name: material_categories_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.material_categories ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."material_categories_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: material_item_equipments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.material_item_equipments (
    "Id" uuid NOT NULL,
    "MaterialItemId" uuid NOT NULL,
    "EquipmentAssetId" uuid NOT NULL,
    "Notes" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.material_item_equipments OWNER TO product;

--
-- Name: material_items; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.material_items (
    "Id" uuid NOT NULL,
    "ItemCode" character varying(50) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "CategoryId" bigint NOT NULL,
    "Specification" text,
    "Unit" character varying(20) NOT NULL,
    "OnHandQuantity" double precision NOT NULL,
    "MinStock" double precision,
    "MaxStock" double precision,
    "ReorderLevel" double precision,
    "ReorderQuantity" double precision,
    "Location" character varying(100),
    "Manufacturer" character varying(100),
    "Supplier" character varying(200),
    "PartNumber" character varying(100),
    "Barcode" character varying(50),
    "UnitCost" numeric(18,4),
    "Currency" character varying(3),
    "Notes" text,
    "ImageUrl" character varying(500),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "VesselId" uuid
);


ALTER TABLE public.material_items OWNER TO product;

--
-- Name: material_request_items; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.material_request_items (
    "Id" integer NOT NULL,
    "RequestId" integer NOT NULL,
    "EquipmentAssetId" uuid,
    "MaterialItemId" uuid,
    "ItemName" character varying(200) NOT NULL,
    "Description" text,
    "Unit" character varying(20) NOT NULL,
    "QuantityOnHand" numeric(18,4) NOT NULL,
    "QuantityRequested" numeric(18,4) NOT NULL,
    "Note" text
);


ALTER TABLE public.material_request_items OWNER TO product;

--
-- Name: material_request_items_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.material_request_items ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."material_request_items_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: material_requests; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.material_requests (
    "Id" integer NOT NULL,
    "RequestCode" character varying(50) NOT NULL,
    "VesselName" character varying(150),
    "Urgency" character varying(20) NOT NULL,
    "NeededDate" timestamp with time zone NOT NULL,
    "RequestDate" timestamp with time zone NOT NULL,
    "RequestedBy" character varying(100),
    "Notes" text,
    "Attachments" text,
    "Status" character varying(20) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.material_requests OWNER TO product;

--
-- Name: material_requests_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.material_requests ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."material_requests_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: noon_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.noon_reports (
    "Id" uuid NOT NULL,
    "MaritimeReportId" uuid NOT NULL,
    "ReportDate" timestamp with time zone NOT NULL,
    "Latitude" double precision,
    "Longitude" double precision,
    "CourseOverGround" double precision,
    "SpeedOverGround" double precision,
    "DistanceTraveled" double precision,
    "DistanceToGo" double precision,
    "EstimatedTimeOfArrival" timestamp with time zone,
    "WeatherConditions" character varying(50),
    "SeaState" character varying(20),
    "AirTemperature" double precision,
    "SeaTemperature" double precision,
    "BarometricPressure" double precision,
    "WindDirection" character varying(20),
    "WindSpeed" double precision,
    "Visibility" character varying(20),
    "FuelOilConsumed" double precision,
    "DieselOilConsumed" double precision,
    "LubOilConsumed" double precision,
    "FreshWaterConsumed" double precision,
    "FuelOilROB" double precision,
    "DieselOilROB" double precision,
    "LubOilROB" double precision,
    "FreshWaterROB" double precision,
    "MainEngineRunningHours" character varying(50),
    "MainEngineRPM" double precision,
    "MainEnginePower" double precision,
    "AuxEngineRunningHours" character varying(50),
    "CargoOnBoard" double precision,
    "CargoDescription" character varying(100),
    "OperationalRemarks" text,
    "MachineryRemarks" text,
    "CargoRemarks" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CrewOnBoard" integer,
    "MaintenanceRemarks" text,
    "PassengersOnBoard" integer,
    "SafetyDrillsConducted" character varying(500),
    "SafetyIncidents" character varying(500),
    "AlarmSummaryJson" text,
    "CertificatesExpiringSoon" integer,
    "MaintenanceSummaryJson" text
);


ALTER TABLE public.noon_reports OWNER TO product;

--
-- Name: onboard_events; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.onboard_events (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignmentId" uuid,
    "EventType" character varying(50) NOT NULL,
    "EventTimestamp" timestamp with time zone NOT NULL,
    "PortCode" character varying(20),
    "PortName" character varying(200),
    "ConfirmedBy" character varying(200),
    "ConfirmedByRole" character varying(100),
    "SignOffReason" character varying(50),
    "Remarks" character varying(2000),
    "OriginalEventId" uuid,
    "Source" character varying(20) NOT NULL,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.onboard_events OWNER TO product;

--
-- Name: onboarding_cases; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.onboarding_cases (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "ReferenceVesselId" uuid,
    "ReferenceVesselName" character varying(100),
    "VesselGroupCode" character varying(50),
    "FlagState" character varying(20),
    "Status" character varying(30) NOT NULL,
    "StatusChangedAt" timestamp with time zone,
    "StatusChangedBy" character varying(100),
    "InvitedAt" timestamp with time zone,
    "ActivatedAt" timestamp with time zone,
    "DueDate" timestamp with time zone,
    "Notes" text,
    "CreatedBy" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.onboarding_cases OWNER TO product;

--
-- Name: onboarding_checklist_items; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.onboarding_checklist_items (
    "Id" uuid NOT NULL,
    "OnboardingCaseId" uuid NOT NULL,
    "ItemType" character varying(30) NOT NULL,
    "Title" character varying(500) NOT NULL,
    "Description" character varying(1000),
    "Status" character varying(20) NOT NULL,
    "RequiredDocumentType" character varying(50),
    "RequiredCertificateId" integer,
    "SourceRuleId" character varying(100),
    "IsMandatory" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "CompletedAt" timestamp with time zone,
    "CompletedBy" character varying(100),
    "CompletionNotes" character varying(500),
    "WaivedBy" character varying(100),
    "WaiverReason" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.onboarding_checklist_items OWNER TO product;

--
-- Name: ports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.ports (
    "Id" integer NOT NULL,
    "PortCode" character varying(5) NOT NULL,
    "PortName" character varying(150) NOT NULL,
    "Country" character varying(100),
    "CountryCode" character varying(2),
    "Latitude" double precision,
    "Longitude" double precision,
    "TimeZone" character varying(50),
    "IsActive" boolean NOT NULL,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" text NOT NULL
);


ALTER TABLE public.ports OWNER TO product;

--
-- Name: ports_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.ports ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ports_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: position_reports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.position_reports (
    "Id" uuid NOT NULL,
    "MaritimeReportId" uuid NOT NULL,
    "ReportDateTime" timestamp with time zone NOT NULL,
    "Latitude" double precision,
    "Longitude" double precision,
    "CourseOverGround" double precision,
    "SpeedOverGround" double precision,
    "WeatherConditions" character varying(50),
    "SeaState" character varying(20),
    "WindSpeed" double precision,
    "WindDirection" character varying(20),
    "FuelOilROB" double precision,
    "DieselOilROB" double precision,
    "NextPort" character varying(200),
    "ETA" timestamp with time zone,
    "DistanceToGo" double precision,
    "Remarks" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CargoOnBoard" double precision,
    "CrewOnBoard" integer,
    "LastPort" character varying(100),
    "ReportReason" character varying(50)
);


ALTER TABLE public.position_reports OWNER TO product;

--
-- Name: rank_certificates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.rank_certificates (
    "Id" integer NOT NULL,
    "RankId" integer NOT NULL,
    "CertificateId" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.rank_certificates OWNER TO product;

--
-- Name: rank_certificates_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.rank_certificates ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."rank_certificates_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ranks; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.ranks (
    "Id" integer NOT NULL,
    "RankCode" character varying(10) NOT NULL,
    "RankName" character varying(100) NOT NULL,
    "Department" character varying(20) NOT NULL,
    "SortOrder" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.ranks OWNER TO product;

--
-- Name: ranks_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.ranks ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ranks_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: report_types; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.report_types (
    "Id" integer NOT NULL,
    "TypeCode" character varying(50) NOT NULL,
    "TypeName" character varying(100) NOT NULL,
    "Category" character varying(50) NOT NULL,
    "Description" text,
    "RegulationReference" character varying(100),
    "Frequency" character varying(30) NOT NULL,
    "IsMandatory" boolean NOT NULL,
    "RequiresMasterSignature" boolean NOT NULL,
    "TemplateSchema" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.report_types OWNER TO product;

--
-- Name: report_types_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.report_types ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."report_types_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: schedule_checklist_templates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.schedule_checklist_templates (
    "Id" uuid NOT NULL,
    "ScheduleId" uuid NOT NULL,
    "SequenceOrder" integer NOT NULL,
    "CheckpointDescription" character varying(500) NOT NULL,
    "RequiresReading" boolean NOT NULL,
    "NormalRangeMin" double precision,
    "NormalRangeMax" double precision,
    "Unit" character varying(20),
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.schedule_checklist_templates OWNER TO product;

--
-- Name: schedule_spare_parts; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.schedule_spare_parts (
    "Id" uuid NOT NULL,
    "ScheduleId" uuid NOT NULL,
    "MaterialItemId" uuid NOT NULL,
    "QuantityRequired" double precision NOT NULL,
    "IsMandatory" boolean NOT NULL,
    "Notes" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.schedule_spare_parts OWNER TO product;

--
-- Name: seafarer_documents; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.seafarer_documents (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "DocumentType" character varying(50) NOT NULL,
    "DocumentNumber" character varying(100) NOT NULL,
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "FileUrl" character varying(500),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CountryId" integer
);


ALTER TABLE public.seafarer_documents OWNER TO product;

--
-- Name: service_records; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.service_records (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselName" character varying(200) NOT NULL,
    "VesselFlag" character varying(50),
    "VesselType" character varying(50),
    "VesselGrt" numeric(12,2),
    "VesselDwt" numeric(12,2),
    "VesselYearBuilt" integer,
    "TradeArea" character varying(100),
    "MainEngineType" character varying(100),
    "MainEnginePowerKw" integer,
    "MainEngineMaker" character varying(100),
    "BoilerType" character varying(100),
    "HasExhaustGasScrubber" boolean,
    "Ecdis" character varying(100),
    "RankAtTime" character varying(100),
    "BoardingDate" timestamp with time zone NOT NULL,
    "DisembarkDate" timestamp with time zone,
    "BoardingPortCode" character varying(5),
    "BoardingPortName" character varying(150),
    "DisembarkPortCode" character varying(5),
    "DisembarkPortName" character varying(150),
    "BoardingRecords" text,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CrewMemberId1" uuid
);


ALTER TABLE public.service_records OWNER TO product;

--
-- Name: shore_notifications; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.shore_notifications (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "Type" character varying(50) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Message" character varying(500) NOT NULL,
    "VesselId" uuid,
    "VesselName" character varying(200),
    "CrewMemberId" uuid,
    "CrewName" character varying(200),
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "IsRead" boolean DEFAULT false NOT NULL
);


ALTER TABLE public.shore_notifications OWNER TO product;

--
-- Name: sign_off_records; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sign_off_records (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignmentId" uuid,
    "RankId" integer NOT NULL,
    "SignOffDate" timestamp with time zone NOT NULL,
    "PortCode" character varying(20),
    "PortName" character varying(200),
    "Reason" character varying(50) NOT NULL,
    "ReasonDetail" character varying(2000),
    "SignedOffBy" character varying(200) NOT NULL,
    "Remarks" character varying(2000),
    "OnboardEventId" uuid,
    "SignOnRecordId" uuid,
    "Source" text NOT NULL,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.sign_off_records OWNER TO product;

--
-- Name: sign_on_records; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sign_on_records (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignmentId" uuid,
    "RankId" integer NOT NULL,
    "SignOnDate" timestamp with time zone NOT NULL,
    "PortCode" character varying(20),
    "PortName" character varying(200),
    "SignedOnBy" character varying(200) NOT NULL,
    "Remarks" character varying(2000),
    "OnboardEventId" uuid,
    "Source" text NOT NULL,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.sign_on_records OWNER TO product;

--
-- Name: stock_receipt_items; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.stock_receipt_items (
    "Id" integer NOT NULL,
    "ReceiptId" integer NOT NULL,
    "StoreLocationId" uuid,
    "MaterialItemId" uuid,
    "ItemCode" character varying(50),
    "ItemName" character varying(200) NOT NULL,
    "Description" text,
    "Unit" character varying(20) NOT NULL,
    "QuantityRequested" numeric(18,4) NOT NULL,
    "QuantityReceived" numeric(18,4) NOT NULL,
    "UnitCost" numeric(18,4),
    "Currency" character varying(3),
    "Note" text
);


ALTER TABLE public.stock_receipt_items OWNER TO product;

--
-- Name: stock_receipt_items_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.stock_receipt_items ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."stock_receipt_items_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: stock_receipts; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.stock_receipts (
    "Id" integer NOT NULL,
    "ReceiptCode" character varying(50) NOT NULL,
    "VesselName" character varying(150),
    "SupplierCode" character varying(50),
    "SupplierName" character varying(200),
    "ReceivedDate" timestamp with time zone NOT NULL,
    "ReceiptDate" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100),
    "Notes" text,
    "Attachments" text,
    "MaterialRequestId" integer,
    "Status" character varying(20) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.stock_receipts OWNER TO product;

--
-- Name: stock_receipts_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.stock_receipts ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."stock_receipts_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: store_locations; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.store_locations (
    "Id" uuid NOT NULL,
    "LocationCode" character varying(50) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(500),
    "ParentId" uuid,
    "Address" character varying(300),
    "ManagerName" character varying(100),
    "Phone" character varying(50),
    "Email" character varying(100),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.store_locations OWNER TO product;

--
-- Name: sync_idempotency_records; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sync_idempotency_records (
    "Id" bigint NOT NULL,
    "IdempotencyKey" character varying(200) NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "ProcessedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.sync_idempotency_records OWNER TO product;

--
-- Name: sync_idempotency_records_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.sync_idempotency_records ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sync_idempotency_records_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sync_logs; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sync_logs (
    "Id" bigint NOT NULL,
    "Direction" character varying(20) NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "TableName" character varying(50) NOT NULL,
    "RecordKey" character varying(50) NOT NULL,
    "ActionType" character varying(10) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "ConflictDetail" text,
    "ProcessedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.sync_logs OWNER TO product;

--
-- Name: sync_logs_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.sync_logs ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sync_logs_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sync_node_trackers; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sync_node_trackers (
    "Id" integer NOT NULL,
    "NodeId" character varying(50) NOT NULL,
    "ShipName" character varying(120),
    "ImoNumber" character varying(20),
    "LastPushAt" timestamp with time zone,
    "TotalReceivedCount" bigint NOT NULL,
    "LastPushBatchSize" integer NOT NULL,
    "LastReceivedVersion" bigint NOT NULL,
    "LastPullAt" timestamp with time zone,
    "TotalDeliveredCount" bigint NOT NULL,
    "PendingOutboxCount" integer NOT NULL,
    "LastAcknowledgedId" bigint NOT NULL,
    "LastHeartbeatAt" timestamp with time zone,
    "CurrentNetworkType" character varying(30),
    "IsOnline" boolean NOT NULL,
    "ConsecutiveFailures" integer NOT NULL,
    "LastError" character varying(500),
    "LastErrorAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "IsRegistered" boolean DEFAULT false NOT NULL,
    "IsRevoked" boolean DEFAULT false NOT NULL,
    "KeyVersion" integer DEFAULT 1 NOT NULL,
    "LastKeyRotatedAt" timestamp with time zone,
    "LastSignedRequestAt" timestamp with time zone,
    "RevokedAt" timestamp with time zone,
    "RevokedReason" character varying(500),
    "SigningKey" character varying(500),
    "LastAcknowledgedKeyVersion" integer,
    "LastKeyVersionAcknowledgedAt" timestamp with time zone,
    "PreviousSigningKey" character varying(500),
    "PreviousKeyVersion" integer,
    "PreviousKeyGraceUntil" timestamp with time zone
);


ALTER TABLE public.sync_node_trackers OWNER TO product;

--
-- Name: sync_node_trackers_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.sync_node_trackers ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sync_node_trackers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sync_outbox; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sync_outbox (
    "Id" bigint NOT NULL,
    "TargetNode" character varying(50) NOT NULL,
    "TableName" character varying(50) NOT NULL,
    "RecordKey" character varying(50) NOT NULL,
    "ActionType" integer NOT NULL,
    "Payload" text NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "DeliveredAt" timestamp with time zone
);


ALTER TABLE public.sync_outbox OWNER TO product;

--
-- Name: sync_outbox_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.sync_outbox ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sync_outbox_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sync_table_stats; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.sync_table_stats (
    "Id" integer NOT NULL,
    "NodeId" character varying(50) NOT NULL,
    "TableName" character varying(50) NOT NULL,
    "TotalSynced" bigint NOT NULL,
    "TotalConflicts" bigint NOT NULL,
    "TotalFailed" bigint NOT NULL,
    "LastSyncAt" timestamp with time zone,
    "SnapshotAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.sync_table_stats OWNER TO product;

--
-- Name: sync_table_stats_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.sync_table_stats ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sync_table_stats_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: travel_documents; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.travel_documents (
    "Id" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "DocumentType" character varying(50) NOT NULL,
    "DocumentNumber" character varying(100) NOT NULL,
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "FileUrl" character varying(500),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CountryId" integer
);


ALTER TABLE public.travel_documents OWNER TO product;

--
-- Name: travel_requests; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.travel_requests (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "Status" character varying(50) NOT NULL,
    "TravelType" character varying(50),
    "DeparturePort" character varying(200),
    "ArrivalPort" character varying(200),
    "DepartureDate" timestamp with time zone,
    "ArrivalDate" timestamp with time zone,
    "ReportingDate" timestamp with time zone,
    "SpecialRequirements" text,
    "BaggageNotes" text,
    "VisaRequirements" text,
    "VendorName" character varying(200),
    "BookingReference" character varying(200),
    "EstimatedCost" numeric(12,2),
    "Currency" character varying(10),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100)
);


ALTER TABLE public.travel_requests OWNER TO product;

--
-- Name: travel_segments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.travel_segments (
    "Id" uuid NOT NULL,
    "TravelRequestId" uuid NOT NULL,
    "SequenceOrder" integer NOT NULL,
    "SegmentType" character varying(50) NOT NULL,
    "Origin" character varying(200),
    "Destination" character varying(200),
    "CarrierName" character varying(100),
    "FlightNumber" character varying(50),
    "DepartureTime" timestamp with time zone,
    "ArrivalTime" timestamp with time zone,
    "ConfirmationNumber" character varying(100),
    "Notes" text
);


ALTER TABLE public.travel_segments OWNER TO product;

--
-- Name: travel_status_history; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.travel_status_history (
    "Id" uuid NOT NULL,
    "TravelRequestId" uuid NOT NULL,
    "FromStatus" character varying(50) NOT NULL,
    "ToStatus" character varying(50) NOT NULL,
    "ChangedBy" character varying(100),
    "Reason" text,
    "ChangedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.travel_status_history OWNER TO product;

--
-- Name: vessel_certificate_assignments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.vessel_certificate_assignments (
    "Id" integer NOT NULL,
    "CertificateId" integer NOT NULL,
    "VesselId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    "IsSynced" boolean NOT NULL,
    "LastSyncedAt" timestamp with time zone
);


ALTER TABLE public.vessel_certificate_assignments OWNER TO product;

--
-- Name: vessel_certificate_assignments_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public.vessel_certificate_assignments ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."vessel_certificate_assignments_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: vessel_manning_standards; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.vessel_manning_standards (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "DocumentReference" character varying(100),
    "IsActive" boolean NOT NULL,
    "EffectiveFrom" timestamp with time zone,
    "EffectiveTo" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100)
);


ALTER TABLE public.vessel_manning_standards OWNER TO product;

--
-- Name: voyage_actual_revenues; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_actual_revenues (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "RevenueNumber" character varying(20) NOT NULL,
    "RevenueCategory" character varying(30) NOT NULL,
    "Description" character varying(200),
    "Amount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "ExchangeRate" double precision NOT NULL,
    "AmountUsd" double precision NOT NULL,
    "PayerName" character varying(200),
    "InvoiceNumber" character varying(100),
    "InvoiceDate" timestamp with time zone,
    "Status" character varying(20) NOT NULL,
    "ReceivedAt" timestamp with time zone,
    "PaymentReference" character varying(100),
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_actual_revenues OWNER TO product;

--
-- Name: voyage_advance_payments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_advance_payments (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "AdvanceNumber" character varying(20) NOT NULL,
    "AdvanceType" character varying(30) NOT NULL,
    "Description" character varying(200),
    "Amount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "ExchangeRate" double precision NOT NULL,
    "AmountUsd" double precision NOT NULL,
    "RecipientName" character varying(200),
    "PortCode" character varying(10),
    "PortName" character varying(100),
    "Status" character varying(20) NOT NULL,
    "PaidAt" timestamp with time zone,
    "PaidBy" character varying(100),
    "PaymentReference" character varying(100),
    "SettledAmount" double precision NOT NULL,
    "UnsettledBalance" double precision NOT NULL,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_advance_payments OWNER TO product;

--
-- Name: voyage_bunker_plans; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_bunker_plans (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "PlanLegId" uuid,
    "Sequence" integer NOT NULL,
    "FuelType" character varying(20) NOT NULL,
    "PlannedQuantity" double precision NOT NULL,
    "OperationType" character varying(20) NOT NULL,
    "PortCode" character varying(5),
    "PortName" character varying(150),
    "EstimatedCostUsd" double precision,
    "SupplierName" character varying(200),
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_bunker_plans OWNER TO product;

--
-- Name: voyage_cargo_plans; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_cargo_plans (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "PlanLegId" uuid,
    "Sequence" integer NOT NULL,
    "OperationType" character varying(30) NOT NULL,
    "CargoType" character varying(50) NOT NULL,
    "CargoDescription" text,
    "PlannedQuantity" double precision NOT NULL,
    "Unit" character varying(10) NOT NULL,
    "PortCode" character varying(5),
    "PortName" character varying(150),
    "ShipperName" character varying(200),
    "ConsigneeName" character varying(200),
    "SpecialRequirements" text,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_cargo_plans OWNER TO product;

--
-- Name: voyage_cost_estimates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_cost_estimates (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "Sequence" integer NOT NULL,
    "CostCategory" character varying(30) NOT NULL,
    "Description" character varying(200),
    "EstimatedAmount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_cost_estimates OWNER TO product;

--
-- Name: voyage_crew_assignments; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_crew_assignments (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "CrewMemberId" uuid NOT NULL,
    "RankId" integer,
    "Role" character varying(20) NOT NULL,
    "EmbarkPortCode" character varying(5),
    "EmbarkPortName" character varying(150),
    "EmbarkDate" timestamp with time zone,
    "DisembarkPortCode" character varying(5),
    "DisembarkPortName" character varying(150),
    "DisembarkDate" timestamp with time zone,
    "WatchSchedule" character varying(20),
    "Status" character varying(20) NOT NULL,
    "Remarks" character varying(500),
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_crew_assignments OWNER TO product;

--
-- Name: voyage_crew_change_plans; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_crew_change_plans (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "PlanLegId" uuid,
    "Sequence" integer NOT NULL,
    "CrewMemberId" uuid,
    "RankId" integer,
    "ChangeType" character varying(20) NOT NULL,
    "PortCode" character varying(5),
    "PortName" character varying(150),
    "PlannedDate" timestamp with time zone,
    "ReplacementReason" character varying(200),
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_crew_change_plans OWNER TO product;

--
-- Name: voyage_disbursements; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_disbursements (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "ExpenseRequestId" uuid,
    "AdvancePaymentId" uuid,
    "DisbursementNumber" character varying(20) NOT NULL,
    "CostCategory" character varying(30) NOT NULL,
    "AllocationScope" character varying(20) NOT NULL,
    "Description" character varying(200),
    "Amount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "ExchangeRate" double precision NOT NULL,
    "AmountUsd" double precision NOT NULL,
    "VendorName" character varying(200),
    "InvoiceNumber" character varying(100),
    "InvoiceDate" timestamp with time zone,
    "DueDate" timestamp with time zone,
    "PortCode" character varying(10),
    "PortName" character varying(100),
    "Status" character varying(20) NOT NULL,
    "VerifiedBy" character varying(100),
    "VerifiedAt" timestamp with time zone,
    "PaidAt" timestamp with time zone,
    "PaymentReference" character varying(100),
    "Notes" text,
    "SupportingDocuments" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_disbursements OWNER TO product;

--
-- Name: voyage_expense_requests; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_expense_requests (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "RequestNumber" character varying(20) NOT NULL,
    "CostCategory" character varying(30) NOT NULL,
    "AllocationScope" character varying(20) NOT NULL,
    "Description" character varying(200),
    "RequestedAmount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "ExchangeRate" double precision NOT NULL,
    "RequestedAmountUsd" double precision NOT NULL,
    "VendorName" character varying(200),
    "VendorReference" character varying(100),
    "PortCode" character varying(10),
    "PortName" character varying(100),
    "Status" character varying(20) NOT NULL,
    "RequestedBy" character varying(100),
    "RequestedAt" timestamp with time zone,
    "ApprovedBy" character varying(100),
    "ApprovedAt" timestamp with time zone,
    "ApprovedAmount" double precision,
    "ApprovedAmountUsd" double precision,
    "ApprovalNotes" text,
    "Notes" text,
    "SupportingDocuments" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_expense_requests OWNER TO product;

--
-- Name: voyage_log_entries; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_log_entries (
    "Id" uuid NOT NULL,
    "VoyageId" uuid,
    "VoyagePlanLegId" uuid,
    "EventType" character varying(20) NOT NULL,
    "EventDateTime" timestamp with time zone NOT NULL,
    "EventDateTimeLocal" timestamp with time zone,
    "TimeZone" character varying(10),
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "PortName" character varying(100),
    "PortLocode" character varying(10),
    "PortCountry" character varying(50),
    "BerthNumber" character varying(50),
    "DistanceToGo" double precision,
    "DistanceFromLast" double precision,
    "TotalVoyageDistance" double precision,
    "CourseOverGround" double precision,
    "SpeedOverGround" double precision,
    "PilotName" character varying(100),
    "PilotStation" character varying(100),
    "OfficerOnWatch" character varying(100) NOT NULL,
    "MasterSignature" text,
    "SignedAt" timestamp with time zone,
    "Remarks" character varying(1000),
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_log_entries OWNER TO product;

--
-- Name: voyage_plan_legs; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_plan_legs (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "Sequence" integer NOT NULL,
    "LegType" character varying(30) NOT NULL,
    "FromPortCode" character varying(5),
    "FromPortName" character varying(100),
    "ToPortCode" character varying(5),
    "ToPortName" character varying(100),
    "PlannedDepartureTime" timestamp with time zone,
    "PlannedArrivalTime" timestamp with time zone,
    "PlannedDistance" double precision,
    "PlannedDurationHours" double precision,
    "PlannedAverageSpeed" double precision,
    "CargoActivity" character varying(30),
    "CrewChangePlanned" boolean NOT NULL,
    "BunkerSupplyPlanned" boolean NOT NULL,
    "PlannedFuelConsumption" double precision,
    "WeatherRoutingNotes" text,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_plan_legs OWNER TO product;

--
-- Name: voyage_records; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_records (
    "Id" uuid NOT NULL,
    "VoyageNumber" character varying(50) NOT NULL,
    "DeparturePort" character varying(50),
    "DepartureTime" timestamp with time zone,
    "ArrivalPort" character varying(50),
    "ArrivalTime" timestamp with time zone,
    "CargoType" character varying(100),
    "CargoWeight" double precision,
    "DistanceTraveled" double precision,
    "FuelConsumed" double precision,
    "AverageSpeed" double precision,
    "VoyageStatus" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL,
    "ActualProfitMargin" double precision,
    "ApprovedAt" timestamp with time zone,
    "ArrivalPortCode" character varying(5),
    "ArrivedAt" timestamp with time zone,
    "CallSign" character varying(20),
    "CancelledAt" timestamp with time zone,
    "CharterType" character varying(40),
    "CommencedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone,
    "DeparturePortCode" character varying(5),
    "EstimatedProfitMargin" double precision,
    "FinancialClosedAt" timestamp with time zone,
    "FinancialClosedBy" character varying(100),
    "FinancialStatus" character varying(30) DEFAULT ''::character varying NOT NULL,
    "IsSynced" boolean DEFAULT false NOT NULL,
    "OutstandingBalance" double precision,
    "PlannedAverageSpeed" double precision,
    "PlannedDistance" double precision,
    "PlannedDurationHours" double precision,
    "PlannedFuelConsumption" double precision,
    "PreviousPortCode" character varying(5),
    "PreviousPortName" character varying(100),
    "ReadyAt" timestamp with time zone,
    "SyncVersion" bigint DEFAULT 0 NOT NULL,
    "TotalActualCost" double precision,
    "TotalActualRevenue" double precision,
    "TotalAdvanced" double precision,
    "TotalDisbursed" double precision,
    "TotalEstimatedCost" double precision,
    "TotalEstimatedRevenue" double precision,
    "VesselFlag" character varying(50),
    "VesselIMO" character varying(10),
    "VesselName" character varying(100),
    "VoyageInstructions" text
);


ALTER TABLE public.voyage_records OWNER TO product;

--
-- Name: voyage_revenue_estimates; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_revenue_estimates (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "Sequence" integer NOT NULL,
    "RevenueCategory" character varying(30) NOT NULL,
    "Description" character varying(200),
    "EstimatedAmount" double precision NOT NULL,
    "Currency" character varying(3) NOT NULL,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_revenue_estimates OWNER TO product;

--
-- Name: voyage_reviews; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_reviews (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "ReviewStatus" character varying(30) NOT NULL,
    "Notes" text,
    "ReviewedBy" character varying(100),
    "ReviewedAt" timestamp with time zone,
    "Tags" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public.voyage_reviews OWNER TO product;

--
-- Name: voyage_settlements; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_settlements (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "SettlementNumber" character varying(20) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "TotalExpenseApproved" double precision NOT NULL,
    "TotalAdvanced" double precision NOT NULL,
    "TotalDisbursed" double precision NOT NULL,
    "TotalRevenue" double precision NOT NULL,
    "NetResult" double precision NOT NULL,
    "AdvanceBalance" double precision NOT NULL,
    "FinalSettlementAmount" double precision,
    "Summary" text,
    "PreparedBy" character varying(100),
    "PreparedAt" timestamp with time zone,
    "ReviewedBy" character varying(100),
    "ReviewedAt" timestamp with time zone,
    "ApprovedBy" character varying(100),
    "ApprovedAt" timestamp with time zone,
    "ApprovalNotes" text,
    "Notes" text,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_settlements OWNER TO product;

--
-- Name: voyage_status_history; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public.voyage_status_history (
    "Id" uuid NOT NULL,
    "VoyageId" uuid NOT NULL,
    "FromStatus" character varying(20),
    "ToStatus" character varying(20) NOT NULL,
    "ChangedBy" character varying(100) NOT NULL,
    "Notes" text,
    "ChangedAt" timestamp with time zone NOT NULL,
    "IsSynced" boolean NOT NULL,
    "SyncVersion" bigint NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public.voyage_status_history OWNER TO product;

--
-- Name: AisData PK_AisData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."AisData"
    ADD CONSTRAINT "PK_AisData" PRIMARY KEY ("Id");


--
-- Name: Certificates PK_Certificates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "PK_Certificates" PRIMARY KEY ("Id");


--
-- Name: EngineData PK_EngineData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."EngineData"
    ADD CONSTRAINT "PK_EngineData" PRIMARY KEY ("Id");


--
-- Name: FuelConsumptionData PK_FuelConsumptionData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."FuelConsumptionData"
    ADD CONSTRAINT "PK_FuelConsumptionData" PRIMARY KEY ("Id");


--
-- Name: FuelConsumptions PK_FuelConsumptions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."FuelConsumptions"
    ADD CONSTRAINT "PK_FuelConsumptions" PRIMARY KEY ("Id");


--
-- Name: GeneratorData PK_GeneratorData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."GeneratorData"
    ADD CONSTRAINT "PK_GeneratorData" PRIMARY KEY ("Id");


--
-- Name: MaintenanceTasks PK_MaintenanceTasks; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."MaintenanceTasks"
    ADD CONSTRAINT "PK_MaintenanceTasks" PRIMARY KEY ("Id");


--
-- Name: PortCalls PK_PortCalls; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PortCalls"
    ADD CONSTRAINT "PK_PortCalls" PRIMARY KEY ("Id");


--
-- Name: PositionData PK_PositionData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PositionData"
    ADD CONSTRAINT "PK_PositionData" PRIMARY KEY ("Id");


--
-- Name: SafetyAlarms PK_SafetyAlarms; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."SafetyAlarms"
    ADD CONSTRAINT "PK_SafetyAlarms" PRIMARY KEY ("Id");


--
-- Name: Ships PK_Ships; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."Ships"
    ADD CONSTRAINT "PK_Ships" PRIMARY KEY ("Id");


--
-- Name: TankLevels PK_TankLevels; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."TankLevels"
    ADD CONSTRAINT "PK_TankLevels" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: VesselAlerts PK_VesselAlerts; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."VesselAlerts"
    ADD CONSTRAINT "PK_VesselAlerts" PRIMARY KEY ("Id");


--
-- Name: VesselPositions PK_VesselPositions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."VesselPositions"
    ADD CONSTRAINT "PK_VesselPositions" PRIMARY KEY ("Id");


--
-- Name: Vessels PK_Vessels; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."Vessels"
    ADD CONSTRAINT "PK_Vessels" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: arrival_reports PK_arrival_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.arrival_reports
    ADD CONSTRAINT "PK_arrival_reports" PRIMARY KEY ("Id");


--
-- Name: assignment_comments PK_assignment_comments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_comments
    ADD CONSTRAINT "PK_assignment_comments" PRIMARY KEY ("Id");


--
-- Name: assignment_confirmations PK_assignment_confirmations; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_confirmations
    ADD CONSTRAINT "PK_assignment_confirmations" PRIMARY KEY ("Id");


--
-- Name: assignment_conflicts PK_assignment_conflicts; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_conflicts
    ADD CONSTRAINT "PK_assignment_conflicts" PRIMARY KEY ("Id");


--
-- Name: assignment_status_history PK_assignment_status_history; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_status_history
    ADD CONSTRAINT "PK_assignment_status_history" PRIMARY KEY ("Id");


--
-- Name: audit_logs PK_audit_logs; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT "PK_audit_logs" PRIMARY KEY ("Id");


--
-- Name: bunker_reports PK_bunker_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.bunker_reports
    ADD CONSTRAINT "PK_bunker_reports" PRIMARY KEY ("Id");


--
-- Name: cargo_operations PK_cargo_operations; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.cargo_operations
    ADD CONSTRAINT "PK_cargo_operations" PRIMARY KEY ("Id");


--
-- Name: certificates PK_certificates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.certificates
    ADD CONSTRAINT "PK_certificates" PRIMARY KEY ("Id");


--
-- Name: compliance_dimensions PK_compliance_dimensions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_dimensions
    ADD CONSTRAINT "PK_compliance_dimensions" PRIMARY KEY ("Id");


--
-- Name: compliance_rule_sets PK_compliance_rule_sets; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_rule_sets
    ADD CONSTRAINT "PK_compliance_rule_sets" PRIMARY KEY ("Id");


--
-- Name: compliance_rules PK_compliance_rules; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_rules
    ADD CONSTRAINT "PK_compliance_rules" PRIMARY KEY ("Id");


--
-- Name: compliance_snapshots PK_compliance_snapshots; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_snapshots
    ADD CONSTRAINT "PK_compliance_snapshots" PRIMARY KEY ("Id");


--
-- Name: compliance_waivers PK_compliance_waivers; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_waivers
    ADD CONSTRAINT "PK_compliance_waivers" PRIMARY KEY ("Id");


--
-- Name: countries PK_countries; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.countries
    ADD CONSTRAINT "PK_countries" PRIMARY KEY ("Id");


--
-- Name: country_certificates PK_country_certificates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.country_certificates
    ADD CONSTRAINT "PK_country_certificates" PRIMARY KEY ("Id");


--
-- Name: crew_access_grants PK_crew_access_grants; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_access_grants
    ADD CONSTRAINT "PK_crew_access_grants" PRIMARY KEY ("Id");


--
-- Name: crew_assignments PK_crew_assignments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_assignments
    ADD CONSTRAINT "PK_crew_assignments" PRIMARY KEY ("Id");


--
-- Name: crew_certificates PK_crew_certificates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_certificates
    ADD CONSTRAINT "PK_crew_certificates" PRIMARY KEY ("Id");


--
-- Name: crew_document_submissions PK_crew_document_submissions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_document_submissions
    ADD CONSTRAINT "PK_crew_document_submissions" PRIMARY KEY ("Id");


--
-- Name: crew_document_versions PK_crew_document_versions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_document_versions
    ADD CONSTRAINT "PK_crew_document_versions" PRIMARY KEY ("Id");


--
-- Name: crew_members PK_crew_members; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_members
    ADD CONSTRAINT "PK_crew_members" PRIMARY KEY ("Id");


--
-- Name: crew_status_history PK_crew_status_history; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_status_history
    ADD CONSTRAINT "PK_crew_status_history" PRIMARY KEY ("Id");


--
-- Name: departure_reports PK_departure_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.departure_reports
    ADD CONSTRAINT "PK_departure_reports" PRIMARY KEY ("Id");


--
-- Name: document_verification_actions PK_document_verification_actions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.document_verification_actions
    ADD CONSTRAINT "PK_document_verification_actions" PRIMARY KEY ("Id");


--
-- Name: document_verification_tasks PK_document_verification_tasks; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.document_verification_tasks
    ADD CONSTRAINT "PK_document_verification_tasks" PRIMARY KEY ("Id");


--
-- Name: employment_documents PK_employment_documents; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.employment_documents
    ADD CONSTRAINT "PK_employment_documents" PRIMARY KEY ("Id");


--
-- Name: equipment_assets PK_equipment_assets; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_assets
    ADD CONSTRAINT "PK_equipment_assets" PRIMARY KEY ("Id");


--
-- Name: equipment_group_members PK_equipment_group_members; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_group_members
    ADD CONSTRAINT "PK_equipment_group_members" PRIMARY KEY ("Id");


--
-- Name: equipment_groups PK_equipment_groups; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_groups
    ADD CONSTRAINT "PK_equipment_groups" PRIMARY KEY ("Id");


--
-- Name: external_candidates PK_external_candidates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_candidates
    ADD CONSTRAINT "PK_external_candidates" PRIMARY KEY ("Id");


--
-- Name: external_request_messages PK_external_request_messages; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_request_messages
    ADD CONSTRAINT "PK_external_request_messages" PRIMARY KEY ("Id");


--
-- Name: external_requests PK_external_requests; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_requests
    ADD CONSTRAINT "PK_external_requests" PRIMARY KEY ("Id");


--
-- Name: health_documents PK_health_documents; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.health_documents
    ADD CONSTRAINT "PK_health_documents" PRIMARY KEY ("Id");


--
-- Name: inventory_stocks PK_inventory_stocks; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.inventory_stocks
    ADD CONSTRAINT "PK_inventory_stocks" PRIMARY KEY ("Id");


--
-- Name: maintenance_histories PK_maintenance_histories; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.maintenance_histories
    ADD CONSTRAINT "PK_maintenance_histories" PRIMARY KEY ("Id");


--
-- Name: maintenance_schedules PK_maintenance_schedules; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.maintenance_schedules
    ADD CONSTRAINT "PK_maintenance_schedules" PRIMARY KEY ("Id");


--
-- Name: manning_positions PK_manning_positions; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.manning_positions
    ADD CONSTRAINT "PK_manning_positions" PRIMARY KEY ("Id");


--
-- Name: maritime_reports PK_maritime_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.maritime_reports
    ADD CONSTRAINT "PK_maritime_reports" PRIMARY KEY ("Id");


--
-- Name: material_categories PK_material_categories; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_categories
    ADD CONSTRAINT "PK_material_categories" PRIMARY KEY ("Id");


--
-- Name: material_item_equipments PK_material_item_equipments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_item_equipments
    ADD CONSTRAINT "PK_material_item_equipments" PRIMARY KEY ("Id");


--
-- Name: material_items PK_material_items; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_items
    ADD CONSTRAINT "PK_material_items" PRIMARY KEY ("Id");


--
-- Name: material_request_items PK_material_request_items; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_request_items
    ADD CONSTRAINT "PK_material_request_items" PRIMARY KEY ("Id");


--
-- Name: material_requests PK_material_requests; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_requests
    ADD CONSTRAINT "PK_material_requests" PRIMARY KEY ("Id");


--
-- Name: noon_reports PK_noon_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.noon_reports
    ADD CONSTRAINT "PK_noon_reports" PRIMARY KEY ("Id");


--
-- Name: onboard_events PK_onboard_events; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboard_events
    ADD CONSTRAINT "PK_onboard_events" PRIMARY KEY ("Id");


--
-- Name: onboarding_cases PK_onboarding_cases; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboarding_cases
    ADD CONSTRAINT "PK_onboarding_cases" PRIMARY KEY ("Id");


--
-- Name: onboarding_checklist_items PK_onboarding_checklist_items; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboarding_checklist_items
    ADD CONSTRAINT "PK_onboarding_checklist_items" PRIMARY KEY ("Id");


--
-- Name: ports PK_ports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.ports
    ADD CONSTRAINT "PK_ports" PRIMARY KEY ("Id");


--
-- Name: position_reports PK_position_reports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.position_reports
    ADD CONSTRAINT "PK_position_reports" PRIMARY KEY ("Id");


--
-- Name: rank_certificates PK_rank_certificates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.rank_certificates
    ADD CONSTRAINT "PK_rank_certificates" PRIMARY KEY ("Id");


--
-- Name: ranks PK_ranks; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.ranks
    ADD CONSTRAINT "PK_ranks" PRIMARY KEY ("Id");


--
-- Name: report_types PK_report_types; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.report_types
    ADD CONSTRAINT "PK_report_types" PRIMARY KEY ("Id");


--
-- Name: schedule_checklist_templates PK_schedule_checklist_templates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.schedule_checklist_templates
    ADD CONSTRAINT "PK_schedule_checklist_templates" PRIMARY KEY ("Id");


--
-- Name: schedule_spare_parts PK_schedule_spare_parts; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.schedule_spare_parts
    ADD CONSTRAINT "PK_schedule_spare_parts" PRIMARY KEY ("Id");


--
-- Name: seafarer_documents PK_seafarer_documents; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.seafarer_documents
    ADD CONSTRAINT "PK_seafarer_documents" PRIMARY KEY ("Id");


--
-- Name: service_records PK_service_records; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.service_records
    ADD CONSTRAINT "PK_service_records" PRIMARY KEY ("Id");


--
-- Name: sign_off_records PK_sign_off_records; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "PK_sign_off_records" PRIMARY KEY ("Id");


--
-- Name: sign_on_records PK_sign_on_records; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_on_records
    ADD CONSTRAINT "PK_sign_on_records" PRIMARY KEY ("Id");


--
-- Name: stock_receipt_items PK_stock_receipt_items; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.stock_receipt_items
    ADD CONSTRAINT "PK_stock_receipt_items" PRIMARY KEY ("Id");


--
-- Name: stock_receipts PK_stock_receipts; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.stock_receipts
    ADD CONSTRAINT "PK_stock_receipts" PRIMARY KEY ("Id");


--
-- Name: store_locations PK_store_locations; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.store_locations
    ADD CONSTRAINT "PK_store_locations" PRIMARY KEY ("Id");


--
-- Name: sync_idempotency_records PK_sync_idempotency_records; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sync_idempotency_records
    ADD CONSTRAINT "PK_sync_idempotency_records" PRIMARY KEY ("Id");


--
-- Name: sync_logs PK_sync_logs; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sync_logs
    ADD CONSTRAINT "PK_sync_logs" PRIMARY KEY ("Id");


--
-- Name: sync_node_trackers PK_sync_node_trackers; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sync_node_trackers
    ADD CONSTRAINT "PK_sync_node_trackers" PRIMARY KEY ("Id");


--
-- Name: sync_outbox PK_sync_outbox; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sync_outbox
    ADD CONSTRAINT "PK_sync_outbox" PRIMARY KEY ("Id");


--
-- Name: sync_table_stats PK_sync_table_stats; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sync_table_stats
    ADD CONSTRAINT "PK_sync_table_stats" PRIMARY KEY ("Id");


--
-- Name: travel_documents PK_travel_documents; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_documents
    ADD CONSTRAINT "PK_travel_documents" PRIMARY KEY ("Id");


--
-- Name: travel_requests PK_travel_requests; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_requests
    ADD CONSTRAINT "PK_travel_requests" PRIMARY KEY ("Id");


--
-- Name: travel_segments PK_travel_segments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_segments
    ADD CONSTRAINT "PK_travel_segments" PRIMARY KEY ("Id");


--
-- Name: travel_status_history PK_travel_status_history; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_status_history
    ADD CONSTRAINT "PK_travel_status_history" PRIMARY KEY ("Id");


--
-- Name: vessel_certificate_assignments PK_vessel_certificate_assignments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.vessel_certificate_assignments
    ADD CONSTRAINT "PK_vessel_certificate_assignments" PRIMARY KEY ("Id");


--
-- Name: vessel_manning_standards PK_vessel_manning_standards; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.vessel_manning_standards
    ADD CONSTRAINT "PK_vessel_manning_standards" PRIMARY KEY ("Id");


--
-- Name: voyage_actual_revenues PK_voyage_actual_revenues; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_actual_revenues
    ADD CONSTRAINT "PK_voyage_actual_revenues" PRIMARY KEY ("Id");


--
-- Name: voyage_advance_payments PK_voyage_advance_payments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_advance_payments
    ADD CONSTRAINT "PK_voyage_advance_payments" PRIMARY KEY ("Id");


--
-- Name: voyage_bunker_plans PK_voyage_bunker_plans; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_bunker_plans
    ADD CONSTRAINT "PK_voyage_bunker_plans" PRIMARY KEY ("Id");


--
-- Name: voyage_cargo_plans PK_voyage_cargo_plans; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_cargo_plans
    ADD CONSTRAINT "PK_voyage_cargo_plans" PRIMARY KEY ("Id");


--
-- Name: voyage_cost_estimates PK_voyage_cost_estimates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_cost_estimates
    ADD CONSTRAINT "PK_voyage_cost_estimates" PRIMARY KEY ("Id");


--
-- Name: voyage_crew_assignments PK_voyage_crew_assignments; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_assignments
    ADD CONSTRAINT "PK_voyage_crew_assignments" PRIMARY KEY ("Id");


--
-- Name: voyage_crew_change_plans PK_voyage_crew_change_plans; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_change_plans
    ADD CONSTRAINT "PK_voyage_crew_change_plans" PRIMARY KEY ("Id");


--
-- Name: voyage_disbursements PK_voyage_disbursements; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_disbursements
    ADD CONSTRAINT "PK_voyage_disbursements" PRIMARY KEY ("Id");


--
-- Name: voyage_expense_requests PK_voyage_expense_requests; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_expense_requests
    ADD CONSTRAINT "PK_voyage_expense_requests" PRIMARY KEY ("Id");


--
-- Name: voyage_log_entries PK_voyage_log_entries; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_log_entries
    ADD CONSTRAINT "PK_voyage_log_entries" PRIMARY KEY ("Id");


--
-- Name: voyage_plan_legs PK_voyage_plan_legs; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_plan_legs
    ADD CONSTRAINT "PK_voyage_plan_legs" PRIMARY KEY ("Id");


--
-- Name: voyage_records PK_voyage_records; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_records
    ADD CONSTRAINT "PK_voyage_records" PRIMARY KEY ("Id");


--
-- Name: voyage_revenue_estimates PK_voyage_revenue_estimates; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_revenue_estimates
    ADD CONSTRAINT "PK_voyage_revenue_estimates" PRIMARY KEY ("Id");


--
-- Name: voyage_reviews PK_voyage_reviews; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_reviews
    ADD CONSTRAINT "PK_voyage_reviews" PRIMARY KEY ("Id");


--
-- Name: voyage_settlements PK_voyage_settlements; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_settlements
    ADD CONSTRAINT "PK_voyage_settlements" PRIMARY KEY ("Id");


--
-- Name: voyage_status_history PK_voyage_status_history; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_status_history
    ADD CONSTRAINT "PK_voyage_status_history" PRIMARY KEY ("Id");


--
-- Name: shore_notifications shore_notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.shore_notifications
    ADD CONSTRAINT shore_notifications_pkey PRIMARY KEY ("Id");


--
-- Name: IX_Certificates_CertificateNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_Certificates_CertificateNumber" ON public."Certificates" USING btree ("CertificateNumber");


--
-- Name: IX_Certificates_VesselId_ExpiryDate; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_Certificates_VesselId_ExpiryDate" ON public."Certificates" USING btree ("VesselId", "ExpiryDate");


--
-- Name: IX_FuelConsumptions_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_FuelConsumptions_VesselId" ON public."FuelConsumptions" USING btree ("VesselId");


--
-- Name: IX_MaintenanceTasks_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_MaintenanceTasks_VesselId" ON public."MaintenanceTasks" USING btree ("VesselId");


--
-- Name: IX_PortCalls_CallType; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_CallType" ON public."PortCalls" USING btree ("CallType");


--
-- Name: IX_PortCalls_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_IsSynced" ON public."PortCalls" USING btree ("IsSynced");


--
-- Name: IX_PortCalls_PortCode; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_PortCode" ON public."PortCalls" USING btree ("PortCode");


--
-- Name: IX_PortCalls_PortId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_PortId" ON public."PortCalls" USING btree ("PortId");


--
-- Name: IX_PortCalls_VesselId_ArrivalTime; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_VesselId_ArrivalTime" ON public."PortCalls" USING btree ("VesselId", "ArrivalTime");


--
-- Name: IX_PortCalls_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_VoyageId_Sequence" ON public."PortCalls" USING btree ("VoyageId", "Sequence");


--
-- Name: IX_PortCalls_VoyagePlanLegId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_VoyagePlanLegId" ON public."PortCalls" USING btree ("VoyagePlanLegId");


--
-- Name: IX_VesselAlerts_IsAcknowledged; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_VesselAlerts_IsAcknowledged" ON public."VesselAlerts" USING btree ("IsAcknowledged");


--
-- Name: IX_VesselAlerts_VesselId_Timestamp; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_VesselAlerts_VesselId_Timestamp" ON public."VesselAlerts" USING btree ("VesselId", "Timestamp");


--
-- Name: IX_VesselPositions_VesselId_Timestamp; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_VesselPositions_VesselId_Timestamp" ON public."VesselPositions" USING btree ("VesselId", "Timestamp");


--
-- Name: IX_Vessels_IMO; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_Vessels_IMO" ON public."Vessels" USING btree ("IMO");


--
-- Name: IX_arrival_reports_MaritimeReportId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_arrival_reports_MaritimeReportId" ON public.arrival_reports USING btree ("MaritimeReportId");


--
-- Name: IX_assignment_comments_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_assignment_comments_AssignmentId" ON public.assignment_comments USING btree ("AssignmentId");


--
-- Name: IX_assignment_confirmations_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_assignment_confirmations_AssignmentId" ON public.assignment_confirmations USING btree ("AssignmentId");


--
-- Name: IX_assignment_conflicts_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_assignment_conflicts_AssignmentId" ON public.assignment_conflicts USING btree ("AssignmentId");


--
-- Name: IX_assignment_conflicts_AssignmentId_IsResolved; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_assignment_conflicts_AssignmentId_IsResolved" ON public.assignment_conflicts USING btree ("AssignmentId", "IsResolved");


--
-- Name: IX_assignment_status_history_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_assignment_status_history_AssignmentId" ON public.assignment_status_history USING btree ("AssignmentId");


--
-- Name: IX_audit_logs_Actor; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_audit_logs_Actor" ON public.audit_logs USING btree ("Actor");


--
-- Name: IX_audit_logs_CorrelationId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_audit_logs_CorrelationId" ON public.audit_logs USING btree ("CorrelationId");


--
-- Name: IX_audit_logs_EntityType_EntityId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_audit_logs_EntityType_EntityId" ON public.audit_logs USING btree ("EntityType", "EntityId");


--
-- Name: IX_audit_logs_Timestamp; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_audit_logs_Timestamp" ON public.audit_logs USING btree ("Timestamp");


--
-- Name: IX_bunker_reports_MaritimeReportId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_bunker_reports_MaritimeReportId" ON public.bunker_reports USING btree ("MaritimeReportId");


--
-- Name: IX_cargo_operations_BillOfLading; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_cargo_operations_BillOfLading" ON public.cargo_operations USING btree ("BillOfLading");


--
-- Name: IX_cargo_operations_CargoType; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_cargo_operations_CargoType" ON public.cargo_operations USING btree ("CargoType");


--
-- Name: IX_cargo_operations_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_cargo_operations_IsSynced" ON public.cargo_operations USING btree ("IsSynced");


--
-- Name: IX_cargo_operations_OperationId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_cargo_operations_OperationId" ON public.cargo_operations USING btree ("OperationId");


--
-- Name: IX_cargo_operations_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_cargo_operations_Status" ON public.cargo_operations USING btree ("Status");


--
-- Name: IX_cargo_operations_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_cargo_operations_VoyageId" ON public.cargo_operations USING btree ("VoyageId");


--
-- Name: IX_certificates_CertificateCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_certificates_CertificateCode" ON public.certificates USING btree ("CertificateCode");


--
-- Name: IX_compliance_dimensions_DimensionType_Value; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_dimensions_DimensionType_Value" ON public.compliance_dimensions USING btree ("DimensionType", "Value");


--
-- Name: IX_compliance_dimensions_RuleId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_dimensions_RuleId" ON public.compliance_dimensions USING btree ("RuleId");


--
-- Name: IX_compliance_rule_sets_Code; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_compliance_rule_sets_Code" ON public.compliance_rule_sets USING btree ("Code") WHERE ("Code" IS NOT NULL);


--
-- Name: IX_compliance_rule_sets_IsActive; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rule_sets_IsActive" ON public.compliance_rule_sets USING btree ("IsActive");


--
-- Name: IX_compliance_rules_EvaluationStage; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rules_EvaluationStage" ON public.compliance_rules USING btree ("EvaluationStage");


--
-- Name: IX_compliance_rules_IsActive; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rules_IsActive" ON public.compliance_rules USING btree ("IsActive");


--
-- Name: IX_compliance_rules_RequiredCertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rules_RequiredCertificateId" ON public.compliance_rules USING btree ("RequiredCertificateId");


--
-- Name: IX_compliance_rules_RuleSetId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rules_RuleSetId" ON public.compliance_rules USING btree ("RuleSetId");


--
-- Name: IX_compliance_rules_Severity; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_rules_Severity" ON public.compliance_rules USING btree ("Severity");


--
-- Name: IX_compliance_snapshots_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_snapshots_CrewMemberId" ON public.compliance_snapshots USING btree ("CrewMemberId");


--
-- Name: IX_compliance_snapshots_CrewMemberId_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_snapshots_CrewMemberId_VesselId" ON public.compliance_snapshots USING btree ("CrewMemberId", "VesselId");


--
-- Name: IX_compliance_snapshots_EvaluatedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_snapshots_EvaluatedAt" ON public.compliance_snapshots USING btree ("EvaluatedAt");


--
-- Name: IX_compliance_snapshots_OverallResult; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_snapshots_OverallResult" ON public.compliance_snapshots USING btree ("OverallResult");


--
-- Name: IX_compliance_waivers_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_waivers_CrewMemberId" ON public.compliance_waivers USING btree ("CrewMemberId");


--
-- Name: IX_compliance_waivers_CrewMemberId_RuleId_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_waivers_CrewMemberId_RuleId_Status" ON public.compliance_waivers USING btree ("CrewMemberId", "RuleId", "Status");


--
-- Name: IX_compliance_waivers_RuleId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_waivers_RuleId" ON public.compliance_waivers USING btree ("RuleId");


--
-- Name: IX_compliance_waivers_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_compliance_waivers_Status" ON public.compliance_waivers USING btree ("Status");


--
-- Name: IX_countries_CountryCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_countries_CountryCode" ON public.countries USING btree ("CountryCode");


--
-- Name: IX_country_certificates_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_country_certificates_CertificateId" ON public.country_certificates USING btree ("CertificateId");


--
-- Name: IX_country_certificates_CountryId_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_country_certificates_CountryId_CertificateId" ON public.country_certificates USING btree ("CountryId", "CertificateId");


--
-- Name: IX_crew_access_grants_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_access_grants_AssignmentId" ON public.crew_access_grants USING btree ("AssignmentId");


--
-- Name: IX_crew_access_grants_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_access_grants_CrewMemberId" ON public.crew_access_grants USING btree ("CrewMemberId");


--
-- Name: IX_crew_access_grants_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_access_grants_Status" ON public.crew_access_grants USING btree ("Status");


--
-- Name: IX_crew_access_grants_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_access_grants_VesselId" ON public.crew_access_grants USING btree ("VesselId");


--
-- Name: IX_crew_assignments_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_CrewMemberId" ON public.crew_assignments USING btree ("CrewMemberId");


--
-- Name: IX_crew_assignments_CrewMemberId_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_CrewMemberId_Status" ON public.crew_assignments USING btree ("CrewMemberId", "Status");


--
-- Name: IX_crew_assignments_ManningPositionId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_ManningPositionId" ON public.crew_assignments USING btree ("ManningPositionId");


--
-- Name: IX_crew_assignments_PlannedStartDate_PlannedEndDate; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_PlannedStartDate_PlannedEndDate" ON public.crew_assignments USING btree ("PlannedStartDate", "PlannedEndDate");


--
-- Name: IX_crew_assignments_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_RankId" ON public.crew_assignments USING btree ("RankId");


--
-- Name: IX_crew_assignments_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_Status" ON public.crew_assignments USING btree ("Status");


--
-- Name: IX_crew_assignments_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_VesselId" ON public.crew_assignments USING btree ("VesselId");


--
-- Name: IX_crew_assignments_VesselId_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_assignments_VesselId_Status" ON public.crew_assignments USING btree ("VesselId", "Status");


--
-- Name: IX_crew_certificates_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_certificates_CertificateId" ON public.crew_certificates USING btree ("CertificateId");


--
-- Name: IX_crew_certificates_CountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_certificates_CountryId" ON public.crew_certificates USING btree ("CountryId");


--
-- Name: IX_crew_certificates_CrewMemberId_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_certificates_CrewMemberId_CertificateId" ON public.crew_certificates USING btree ("CrewMemberId", "CertificateId");


--
-- Name: IX_crew_certificates_ExpiryDate; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_certificates_ExpiryDate" ON public.crew_certificates USING btree ("ExpiryDate");


--
-- Name: IX_crew_certificates_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_certificates_IsSynced" ON public.crew_certificates USING btree ("IsSynced");


--
-- Name: IX_crew_document_submissions_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_submissions_CrewMemberId" ON public.crew_document_submissions USING btree ("CrewMemberId");


--
-- Name: IX_crew_document_submissions_CrewMemberId_DocumentType_IsActiv~; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_submissions_CrewMemberId_DocumentType_IsActiv~" ON public.crew_document_submissions USING btree ("CrewMemberId", "DocumentType", "IsActiveSubmission");


--
-- Name: IX_crew_document_submissions_IssuingCountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_submissions_IssuingCountryId" ON public.crew_document_submissions USING btree ("IssuingCountryId");


--
-- Name: IX_crew_document_submissions_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_submissions_Status" ON public.crew_document_submissions USING btree ("Status");


--
-- Name: IX_crew_document_versions_SubmissionId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_versions_SubmissionId" ON public.crew_document_versions USING btree ("SubmissionId");


--
-- Name: IX_crew_document_versions_SubmissionId_IsActiveVersion; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_document_versions_SubmissionId_IsActiveVersion" ON public.crew_document_versions USING btree ("SubmissionId", "IsActiveVersion");


--
-- Name: IX_crew_members_CountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_CountryId" ON public.crew_members USING btree ("CountryId");


--
-- Name: IX_crew_members_CrewId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_crew_members_CrewId" ON public.crew_members USING btree ("CrewId");


--
-- Name: IX_crew_members_FullName; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_FullName" ON public.crew_members USING btree ("FullName");


--
-- Name: IX_crew_members_IsOnboard; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_IsOnboard" ON public.crew_members USING btree ("IsOnboard");


--
-- Name: IX_crew_members_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_IsSynced" ON public.crew_members USING btree ("IsSynced");


--
-- Name: IX_crew_members_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_RankId" ON public.crew_members USING btree ("RankId");


--
-- Name: IX_crew_members_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_members_VesselId" ON public.crew_members USING btree ("VesselId");


--
-- Name: IX_crew_status_history_ChangedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_status_history_ChangedAt" ON public.crew_status_history USING btree ("ChangedAt");


--
-- Name: IX_crew_status_history_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_crew_status_history_CrewMemberId" ON public.crew_status_history USING btree ("CrewMemberId");


--
-- Name: IX_departure_reports_MaritimeReportId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_departure_reports_MaritimeReportId" ON public.departure_reports USING btree ("MaritimeReportId");


--
-- Name: IX_document_verification_actions_TaskId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_actions_TaskId" ON public.document_verification_actions USING btree ("TaskId");


--
-- Name: IX_document_verification_tasks_AssignedTo; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_tasks_AssignedTo" ON public.document_verification_tasks USING btree ("AssignedTo");


--
-- Name: IX_document_verification_tasks_DueAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_tasks_DueAt" ON public.document_verification_tasks USING btree ("DueAt");


--
-- Name: IX_document_verification_tasks_Status_Priority; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_tasks_Status_Priority" ON public.document_verification_tasks USING btree ("Status", "Priority");


--
-- Name: IX_document_verification_tasks_SubmissionId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_tasks_SubmissionId" ON public.document_verification_tasks USING btree ("SubmissionId");


--
-- Name: IX_document_verification_tasks_VersionId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_document_verification_tasks_VersionId" ON public.document_verification_tasks USING btree ("VersionId");


--
-- Name: IX_employment_documents_CountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_employment_documents_CountryId" ON public.employment_documents USING btree ("CountryId");


--
-- Name: IX_employment_documents_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_employment_documents_CrewMemberId" ON public.employment_documents USING btree ("CrewMemberId");


--
-- Name: IX_equipment_assets_ParentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_equipment_assets_ParentId" ON public.equipment_assets USING btree ("ParentId");


--
-- Name: IX_equipment_assets_VesselId_AssetCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_equipment_assets_VesselId_AssetCode" ON public.equipment_assets USING btree ("VesselId", "AssetCode");


--
-- Name: IX_equipment_group_members_AssetId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_equipment_group_members_AssetId" ON public.equipment_group_members USING btree ("AssetId");


--
-- Name: IX_equipment_group_members_GroupId_AssetId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_equipment_group_members_GroupId_AssetId" ON public.equipment_group_members USING btree ("GroupId", "AssetId");


--
-- Name: IX_equipment_groups_GroupCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_equipment_groups_GroupCode" ON public.equipment_groups USING btree ("GroupCode");


--
-- Name: IX_external_candidates_ExternalRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_candidates_ExternalRequestId" ON public.external_candidates USING btree ("ExternalRequestId");


--
-- Name: IX_external_candidates_LinkedCrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_candidates_LinkedCrewMemberId" ON public.external_candidates USING btree ("LinkedCrewMemberId");


--
-- Name: IX_external_candidates_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_candidates_RankId" ON public.external_candidates USING btree ("RankId");


--
-- Name: IX_external_candidates_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_candidates_Status" ON public.external_candidates USING btree ("Status");


--
-- Name: IX_external_request_messages_ExternalRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_request_messages_ExternalRequestId" ON public.external_request_messages USING btree ("ExternalRequestId");


--
-- Name: IX_external_requests_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_requests_AssignmentId" ON public.external_requests USING btree ("AssignmentId");


--
-- Name: IX_external_requests_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_requests_RankId" ON public.external_requests USING btree ("RankId");


--
-- Name: IX_external_requests_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_requests_Status" ON public.external_requests USING btree ("Status");


--
-- Name: IX_external_requests_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_external_requests_VesselId" ON public.external_requests USING btree ("VesselId");


--
-- Name: IX_health_documents_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_health_documents_CrewMemberId" ON public.health_documents USING btree ("CrewMemberId");


--
-- Name: IX_inventory_stocks_MaterialItemId_StoreLocationId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_inventory_stocks_MaterialItemId_StoreLocationId" ON public.inventory_stocks USING btree ("MaterialItemId", "StoreLocationId");


--
-- Name: IX_maintenance_schedules_ScheduleCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_maintenance_schedules_ScheduleCode" ON public.maintenance_schedules USING btree ("ScheduleCode");


--
-- Name: IX_manning_positions_ManningStandardId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_manning_positions_ManningStandardId" ON public.manning_positions USING btree ("ManningStandardId");


--
-- Name: IX_manning_positions_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_manning_positions_RankId" ON public.manning_positions USING btree ("RankId");


--
-- Name: IX_maritime_reports_OriginNode; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_maritime_reports_OriginNode" ON public.maritime_reports USING btree ("OriginNode");


--
-- Name: IX_maritime_reports_ReportDateTime; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_maritime_reports_ReportDateTime" ON public.maritime_reports USING btree ("ReportDateTime");


--
-- Name: IX_maritime_reports_ReportNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_maritime_reports_ReportNumber" ON public.maritime_reports USING btree ("ReportNumber");


--
-- Name: IX_maritime_reports_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_maritime_reports_Status" ON public.maritime_reports USING btree ("Status");


--
-- Name: IX_material_categories_CategoryCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_material_categories_CategoryCode" ON public.material_categories USING btree ("CategoryCode");


--
-- Name: IX_material_item_equipments_MaterialItemId_EquipmentAssetId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_material_item_equipments_MaterialItemId_EquipmentAssetId" ON public.material_item_equipments USING btree ("MaterialItemId", "EquipmentAssetId");


--
-- Name: IX_material_items_VesselId_ItemCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_material_items_VesselId_ItemCode" ON public.material_items USING btree ("VesselId", "ItemCode");


--
-- Name: IX_material_request_items_RequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_material_request_items_RequestId" ON public.material_request_items USING btree ("RequestId");


--
-- Name: IX_material_requests_RequestCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_material_requests_RequestCode" ON public.material_requests USING btree ("RequestCode");


--
-- Name: IX_noon_reports_MaritimeReportId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_noon_reports_MaritimeReportId" ON public.noon_reports USING btree ("MaritimeReportId");


--
-- Name: IX_onboard_events_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_AssignmentId" ON public.onboard_events USING btree ("AssignmentId");


--
-- Name: IX_onboard_events_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_CrewMemberId" ON public.onboard_events USING btree ("CrewMemberId");


--
-- Name: IX_onboard_events_EventTimestamp; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_EventTimestamp" ON public.onboard_events USING btree ("EventTimestamp");


--
-- Name: IX_onboard_events_EventType; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_EventType" ON public.onboard_events USING btree ("EventType");


--
-- Name: IX_onboard_events_OriginalEventId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_OriginalEventId" ON public.onboard_events USING btree ("OriginalEventId");


--
-- Name: IX_onboard_events_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboard_events_VesselId" ON public.onboard_events USING btree ("VesselId");


--
-- Name: IX_onboarding_cases_CreatedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboarding_cases_CreatedAt" ON public.onboarding_cases USING btree ("CreatedAt");


--
-- Name: IX_onboarding_cases_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboarding_cases_CrewMemberId" ON public.onboarding_cases USING btree ("CrewMemberId");


--
-- Name: IX_onboarding_cases_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboarding_cases_Status" ON public.onboarding_cases USING btree ("Status");


--
-- Name: IX_onboarding_checklist_items_OnboardingCaseId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboarding_checklist_items_OnboardingCaseId" ON public.onboarding_checklist_items USING btree ("OnboardingCaseId");


--
-- Name: IX_onboarding_checklist_items_OnboardingCaseId_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_onboarding_checklist_items_OnboardingCaseId_Status" ON public.onboarding_checklist_items USING btree ("OnboardingCaseId", "Status");


--
-- Name: IX_ports_CountryCode; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_ports_CountryCode" ON public.ports USING btree ("CountryCode");


--
-- Name: IX_ports_IsActive; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_ports_IsActive" ON public.ports USING btree ("IsActive");


--
-- Name: IX_ports_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_ports_IsSynced" ON public.ports USING btree ("IsSynced");


--
-- Name: IX_ports_PortCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_ports_PortCode" ON public.ports USING btree ("PortCode");


--
-- Name: IX_ports_PortName; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_ports_PortName" ON public.ports USING btree ("PortName");


--
-- Name: IX_position_reports_MaritimeReportId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_position_reports_MaritimeReportId" ON public.position_reports USING btree ("MaritimeReportId");


--
-- Name: IX_rank_certificates_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_rank_certificates_CertificateId" ON public.rank_certificates USING btree ("CertificateId");


--
-- Name: IX_rank_certificates_RankId_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_rank_certificates_RankId_CertificateId" ON public.rank_certificates USING btree ("RankId", "CertificateId");


--
-- Name: IX_ranks_RankCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_ranks_RankCode" ON public.ranks USING btree ("RankCode");


--
-- Name: IX_report_types_TypeCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_report_types_TypeCode" ON public.report_types USING btree ("TypeCode");


--
-- Name: IX_schedule_checklist_templates_ScheduleId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_schedule_checklist_templates_ScheduleId" ON public.schedule_checklist_templates USING btree ("ScheduleId");


--
-- Name: IX_schedule_spare_parts_ScheduleId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_schedule_spare_parts_ScheduleId" ON public.schedule_spare_parts USING btree ("ScheduleId");


--
-- Name: IX_seafarer_documents_CountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_seafarer_documents_CountryId" ON public.seafarer_documents USING btree ("CountryId");


--
-- Name: IX_seafarer_documents_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_seafarer_documents_CrewMemberId" ON public.seafarer_documents USING btree ("CrewMemberId");


--
-- Name: IX_service_records_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_service_records_CrewMemberId" ON public.service_records USING btree ("CrewMemberId");


--
-- Name: IX_service_records_CrewMemberId1; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_service_records_CrewMemberId1" ON public.service_records USING btree ("CrewMemberId1");


--
-- Name: IX_service_records_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_service_records_IsSynced" ON public.service_records USING btree ("IsSynced");


--
-- Name: IX_shore_notifications_CreatedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_shore_notifications_CreatedAt" ON public.shore_notifications USING btree ("CreatedAt");


--
-- Name: IX_shore_notifications_IsRead; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_shore_notifications_IsRead" ON public.shore_notifications USING btree ("IsRead");


--
-- Name: IX_sign_off_records_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_AssignmentId" ON public.sign_off_records USING btree ("AssignmentId");


--
-- Name: IX_sign_off_records_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_CrewMemberId" ON public.sign_off_records USING btree ("CrewMemberId");


--
-- Name: IX_sign_off_records_OnboardEventId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_OnboardEventId" ON public.sign_off_records USING btree ("OnboardEventId");


--
-- Name: IX_sign_off_records_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_RankId" ON public.sign_off_records USING btree ("RankId");


--
-- Name: IX_sign_off_records_SignOffDate; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_SignOffDate" ON public.sign_off_records USING btree ("SignOffDate");


--
-- Name: IX_sign_off_records_SignOnRecordId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_SignOnRecordId" ON public.sign_off_records USING btree ("SignOnRecordId");


--
-- Name: IX_sign_off_records_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_off_records_VesselId" ON public.sign_off_records USING btree ("VesselId");


--
-- Name: IX_sign_on_records_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_AssignmentId" ON public.sign_on_records USING btree ("AssignmentId");


--
-- Name: IX_sign_on_records_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_CrewMemberId" ON public.sign_on_records USING btree ("CrewMemberId");


--
-- Name: IX_sign_on_records_OnboardEventId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_OnboardEventId" ON public.sign_on_records USING btree ("OnboardEventId");


--
-- Name: IX_sign_on_records_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_RankId" ON public.sign_on_records USING btree ("RankId");


--
-- Name: IX_sign_on_records_SignOnDate; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_SignOnDate" ON public.sign_on_records USING btree ("SignOnDate");


--
-- Name: IX_sign_on_records_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sign_on_records_VesselId" ON public.sign_on_records USING btree ("VesselId");


--
-- Name: IX_stock_receipt_items_ReceiptId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_stock_receipt_items_ReceiptId" ON public.stock_receipt_items USING btree ("ReceiptId");


--
-- Name: IX_stock_receipts_MaterialRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_stock_receipts_MaterialRequestId" ON public.stock_receipts USING btree ("MaterialRequestId");


--
-- Name: IX_stock_receipts_ReceiptCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_stock_receipts_ReceiptCode" ON public.stock_receipts USING btree ("ReceiptCode");


--
-- Name: IX_store_locations_LocationCode; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_store_locations_LocationCode" ON public.store_locations USING btree ("LocationCode");


--
-- Name: IX_store_locations_ParentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_store_locations_ParentId" ON public.store_locations USING btree ("ParentId");


--
-- Name: IX_sync_idempotency_records_IdempotencyKey; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_sync_idempotency_records_IdempotencyKey" ON public.sync_idempotency_records USING btree ("IdempotencyKey");


--
-- Name: IX_sync_idempotency_records_ProcessedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_idempotency_records_ProcessedAt" ON public.sync_idempotency_records USING btree ("ProcessedAt");


--
-- Name: IX_sync_logs_OriginNode; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_logs_OriginNode" ON public.sync_logs USING btree ("OriginNode");


--
-- Name: IX_sync_logs_ProcessedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_logs_ProcessedAt" ON public.sync_logs USING btree ("ProcessedAt");


--
-- Name: IX_sync_node_trackers_IsOnline; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_node_trackers_IsOnline" ON public.sync_node_trackers USING btree ("IsOnline");


--
-- Name: IX_sync_node_trackers_IsRegistered; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_node_trackers_IsRegistered" ON public.sync_node_trackers USING btree ("IsRegistered");


--
-- Name: IX_sync_node_trackers_IsRevoked; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_node_trackers_IsRevoked" ON public.sync_node_trackers USING btree ("IsRevoked");


--
-- Name: IX_sync_node_trackers_NodeId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_sync_node_trackers_NodeId" ON public.sync_node_trackers USING btree ("NodeId");


--
-- Name: IX_sync_outbox_DeliveredAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_outbox_DeliveredAt" ON public.sync_outbox USING btree ("DeliveredAt");


--
-- Name: IX_sync_outbox_TableName_RecordKey; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_outbox_TableName_RecordKey" ON public.sync_outbox USING btree ("TableName", "RecordKey");


--
-- Name: IX_sync_table_stats_NodeId_TableName; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_sync_table_stats_NodeId_TableName" ON public.sync_table_stats USING btree ("NodeId", "TableName");


--
-- Name: IX_travel_documents_CountryId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_documents_CountryId" ON public.travel_documents USING btree ("CountryId");


--
-- Name: IX_travel_documents_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_documents_CrewMemberId" ON public.travel_documents USING btree ("CrewMemberId");


--
-- Name: IX_travel_requests_AssignmentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_requests_AssignmentId" ON public.travel_requests USING btree ("AssignmentId");


--
-- Name: IX_travel_requests_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_requests_CrewMemberId" ON public.travel_requests USING btree ("CrewMemberId");


--
-- Name: IX_travel_requests_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_requests_Status" ON public.travel_requests USING btree ("Status");


--
-- Name: IX_travel_segments_TravelRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_segments_TravelRequestId" ON public.travel_segments USING btree ("TravelRequestId");


--
-- Name: IX_travel_status_history_TravelRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_travel_status_history_TravelRequestId" ON public.travel_status_history USING btree ("TravelRequestId");


--
-- Name: IX_vessel_certificate_assignments_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_vessel_certificate_assignments_CertificateId" ON public.vessel_certificate_assignments USING btree ("CertificateId");


--
-- Name: IX_vessel_certificate_assignments_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_vessel_certificate_assignments_IsSynced" ON public.vessel_certificate_assignments USING btree ("IsSynced");


--
-- Name: IX_vessel_certificate_assignments_VesselId_CertificateId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_vessel_certificate_assignments_VesselId_CertificateId" ON public.vessel_certificate_assignments USING btree ("VesselId", "CertificateId");


--
-- Name: IX_vessel_manning_standards_IsActive; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_vessel_manning_standards_IsActive" ON public.vessel_manning_standards USING btree ("IsActive");


--
-- Name: IX_vessel_manning_standards_VesselId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_vessel_manning_standards_VesselId" ON public.vessel_manning_standards USING btree ("VesselId");


--
-- Name: IX_voyage_actual_revenues_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_actual_revenues_IsSynced" ON public.voyage_actual_revenues USING btree ("IsSynced");


--
-- Name: IX_voyage_actual_revenues_RevenueCategory; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_actual_revenues_RevenueCategory" ON public.voyage_actual_revenues USING btree ("RevenueCategory");


--
-- Name: IX_voyage_actual_revenues_RevenueNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_actual_revenues_RevenueNumber" ON public.voyage_actual_revenues USING btree ("RevenueNumber");


--
-- Name: IX_voyage_actual_revenues_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_actual_revenues_Status" ON public.voyage_actual_revenues USING btree ("Status");


--
-- Name: IX_voyage_actual_revenues_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_actual_revenues_VoyageId" ON public.voyage_actual_revenues USING btree ("VoyageId");


--
-- Name: IX_voyage_advance_payments_AdvanceNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_advance_payments_AdvanceNumber" ON public.voyage_advance_payments USING btree ("AdvanceNumber");


--
-- Name: IX_voyage_advance_payments_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_advance_payments_IsSynced" ON public.voyage_advance_payments USING btree ("IsSynced");


--
-- Name: IX_voyage_advance_payments_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_advance_payments_Status" ON public.voyage_advance_payments USING btree ("Status");


--
-- Name: IX_voyage_advance_payments_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_advance_payments_VoyageId" ON public.voyage_advance_payments USING btree ("VoyageId");


--
-- Name: IX_voyage_bunker_plans_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_bunker_plans_IsSynced" ON public.voyage_bunker_plans USING btree ("IsSynced");


--
-- Name: IX_voyage_bunker_plans_PlanLegId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_bunker_plans_PlanLegId" ON public.voyage_bunker_plans USING btree ("PlanLegId");


--
-- Name: IX_voyage_bunker_plans_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_bunker_plans_VoyageId_Sequence" ON public.voyage_bunker_plans USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_cargo_plans_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_cargo_plans_IsSynced" ON public.voyage_cargo_plans USING btree ("IsSynced");


--
-- Name: IX_voyage_cargo_plans_PlanLegId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_cargo_plans_PlanLegId" ON public.voyage_cargo_plans USING btree ("PlanLegId");


--
-- Name: IX_voyage_cargo_plans_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_cargo_plans_VoyageId_Sequence" ON public.voyage_cargo_plans USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_cost_estimates_CostCategory; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_cost_estimates_CostCategory" ON public.voyage_cost_estimates USING btree ("CostCategory");


--
-- Name: IX_voyage_cost_estimates_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_cost_estimates_IsSynced" ON public.voyage_cost_estimates USING btree ("IsSynced");


--
-- Name: IX_voyage_cost_estimates_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_cost_estimates_VoyageId_Sequence" ON public.voyage_cost_estimates USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_crew_assignments_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_assignments_CrewMemberId" ON public.voyage_crew_assignments USING btree ("CrewMemberId");


--
-- Name: IX_voyage_crew_assignments_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_assignments_IsSynced" ON public.voyage_crew_assignments USING btree ("IsSynced");


--
-- Name: IX_voyage_crew_assignments_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_assignments_RankId" ON public.voyage_crew_assignments USING btree ("RankId");


--
-- Name: IX_voyage_crew_assignments_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_assignments_Status" ON public.voyage_crew_assignments USING btree ("Status");


--
-- Name: IX_voyage_crew_assignments_VoyageId_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_crew_assignments_VoyageId_CrewMemberId" ON public.voyage_crew_assignments USING btree ("VoyageId", "CrewMemberId");


--
-- Name: IX_voyage_crew_change_plans_CrewMemberId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_change_plans_CrewMemberId" ON public.voyage_crew_change_plans USING btree ("CrewMemberId");


--
-- Name: IX_voyage_crew_change_plans_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_change_plans_IsSynced" ON public.voyage_crew_change_plans USING btree ("IsSynced");


--
-- Name: IX_voyage_crew_change_plans_PlanLegId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_change_plans_PlanLegId" ON public.voyage_crew_change_plans USING btree ("PlanLegId");


--
-- Name: IX_voyage_crew_change_plans_RankId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_crew_change_plans_RankId" ON public.voyage_crew_change_plans USING btree ("RankId");


--
-- Name: IX_voyage_crew_change_plans_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_crew_change_plans_VoyageId_Sequence" ON public.voyage_crew_change_plans USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_disbursements_AdvancePaymentId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_AdvancePaymentId" ON public.voyage_disbursements USING btree ("AdvancePaymentId");


--
-- Name: IX_voyage_disbursements_CostCategory; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_CostCategory" ON public.voyage_disbursements USING btree ("CostCategory");


--
-- Name: IX_voyage_disbursements_DisbursementNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_disbursements_DisbursementNumber" ON public.voyage_disbursements USING btree ("DisbursementNumber");


--
-- Name: IX_voyage_disbursements_ExpenseRequestId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_ExpenseRequestId" ON public.voyage_disbursements USING btree ("ExpenseRequestId");


--
-- Name: IX_voyage_disbursements_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_IsSynced" ON public.voyage_disbursements USING btree ("IsSynced");


--
-- Name: IX_voyage_disbursements_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_Status" ON public.voyage_disbursements USING btree ("Status");


--
-- Name: IX_voyage_disbursements_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_disbursements_VoyageId" ON public.voyage_disbursements USING btree ("VoyageId");


--
-- Name: IX_voyage_expense_requests_CostCategory; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_expense_requests_CostCategory" ON public.voyage_expense_requests USING btree ("CostCategory");


--
-- Name: IX_voyage_expense_requests_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_expense_requests_IsSynced" ON public.voyage_expense_requests USING btree ("IsSynced");


--
-- Name: IX_voyage_expense_requests_RequestNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_expense_requests_RequestNumber" ON public.voyage_expense_requests USING btree ("RequestNumber");


--
-- Name: IX_voyage_expense_requests_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_expense_requests_Status" ON public.voyage_expense_requests USING btree ("Status");


--
-- Name: IX_voyage_expense_requests_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_expense_requests_VoyageId" ON public.voyage_expense_requests USING btree ("VoyageId");


--
-- Name: IX_voyage_log_entries_EventDateTime; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_log_entries_EventDateTime" ON public.voyage_log_entries USING btree ("EventDateTime");


--
-- Name: IX_voyage_log_entries_EventType; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_log_entries_EventType" ON public.voyage_log_entries USING btree ("EventType");


--
-- Name: IX_voyage_log_entries_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_log_entries_IsSynced" ON public.voyage_log_entries USING btree ("IsSynced");


--
-- Name: IX_voyage_log_entries_PortLocode; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_log_entries_PortLocode" ON public.voyage_log_entries USING btree ("PortLocode");


--
-- Name: IX_voyage_log_entries_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_log_entries_VoyageId" ON public.voyage_log_entries USING btree ("VoyageId");


--
-- Name: IX_voyage_plan_legs_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_plan_legs_IsSynced" ON public.voyage_plan_legs USING btree ("IsSynced");


--
-- Name: IX_voyage_plan_legs_LegType; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_plan_legs_LegType" ON public.voyage_plan_legs USING btree ("LegType");


--
-- Name: IX_voyage_plan_legs_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_plan_legs_VoyageId_Sequence" ON public.voyage_plan_legs USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_records_DepartureTime; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_records_DepartureTime" ON public.voyage_records USING btree ("DepartureTime");


--
-- Name: IX_voyage_records_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_records_IsSynced" ON public.voyage_records USING btree ("IsSynced");


--
-- Name: IX_voyage_records_VesselIMO; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_records_VesselIMO" ON public.voyage_records USING btree ("VesselIMO");


--
-- Name: IX_voyage_records_VoyageNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_records_VoyageNumber" ON public.voyage_records USING btree ("VoyageNumber");


--
-- Name: IX_voyage_records_VoyageStatus; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_records_VoyageStatus" ON public.voyage_records USING btree ("VoyageStatus");


--
-- Name: IX_voyage_revenue_estimates_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_revenue_estimates_IsSynced" ON public.voyage_revenue_estimates USING btree ("IsSynced");


--
-- Name: IX_voyage_revenue_estimates_RevenueCategory; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_revenue_estimates_RevenueCategory" ON public.voyage_revenue_estimates USING btree ("RevenueCategory");


--
-- Name: IX_voyage_revenue_estimates_VoyageId_Sequence; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_revenue_estimates_VoyageId_Sequence" ON public.voyage_revenue_estimates USING btree ("VoyageId", "Sequence");


--
-- Name: IX_voyage_reviews_ReviewStatus; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_reviews_ReviewStatus" ON public.voyage_reviews USING btree ("ReviewStatus");


--
-- Name: IX_voyage_reviews_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_reviews_VoyageId" ON public.voyage_reviews USING btree ("VoyageId");


--
-- Name: IX_voyage_settlements_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_settlements_IsSynced" ON public.voyage_settlements USING btree ("IsSynced");


--
-- Name: IX_voyage_settlements_SettlementNumber; Type: INDEX; Schema: public; Owner: product
--

CREATE UNIQUE INDEX "IX_voyage_settlements_SettlementNumber" ON public.voyage_settlements USING btree ("SettlementNumber");


--
-- Name: IX_voyage_settlements_Status; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_settlements_Status" ON public.voyage_settlements USING btree ("Status");


--
-- Name: IX_voyage_settlements_VoyageId; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_settlements_VoyageId" ON public.voyage_settlements USING btree ("VoyageId");


--
-- Name: IX_voyage_status_history_IsSynced; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_status_history_IsSynced" ON public.voyage_status_history USING btree ("IsSynced");


--
-- Name: IX_voyage_status_history_ToStatus; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_status_history_ToStatus" ON public.voyage_status_history USING btree ("ToStatus");


--
-- Name: IX_voyage_status_history_VoyageId_ChangedAt; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_voyage_status_history_VoyageId_ChangedAt" ON public.voyage_status_history USING btree ("VoyageId", "ChangedAt");


--
-- Name: Certificates FK_Certificates_Vessels_VesselId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_Vessels_VesselId" FOREIGN KEY ("VesselId") REFERENCES public."Vessels"("Id") ON DELETE CASCADE;


--
-- Name: FuelConsumptions FK_FuelConsumptions_Vessels_VesselId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."FuelConsumptions"
    ADD CONSTRAINT "FK_FuelConsumptions_Vessels_VesselId" FOREIGN KEY ("VesselId") REFERENCES public."Vessels"("Id") ON DELETE CASCADE;


--
-- Name: PortCalls FK_PortCalls_Vessels_VesselId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PortCalls"
    ADD CONSTRAINT "FK_PortCalls_Vessels_VesselId" FOREIGN KEY ("VesselId") REFERENCES public."Vessels"("Id") ON DELETE CASCADE;


--
-- Name: PortCalls FK_PortCalls_ports_PortId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PortCalls"
    ADD CONSTRAINT "FK_PortCalls_ports_PortId" FOREIGN KEY ("PortId") REFERENCES public.ports("Id") ON DELETE SET NULL;


--
-- Name: PortCalls FK_PortCalls_voyage_plan_legs_VoyagePlanLegId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PortCalls"
    ADD CONSTRAINT "FK_PortCalls_voyage_plan_legs_VoyagePlanLegId" FOREIGN KEY ("VoyagePlanLegId") REFERENCES public.voyage_plan_legs("Id") ON DELETE SET NULL;


--
-- Name: PortCalls FK_PortCalls_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."PortCalls"
    ADD CONSTRAINT "FK_PortCalls_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: VesselAlerts FK_VesselAlerts_Vessels_VesselId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."VesselAlerts"
    ADD CONSTRAINT "FK_VesselAlerts_Vessels_VesselId" FOREIGN KEY ("VesselId") REFERENCES public."Vessels"("Id") ON DELETE CASCADE;


--
-- Name: VesselPositions FK_VesselPositions_Vessels_VesselId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."VesselPositions"
    ADD CONSTRAINT "FK_VesselPositions_Vessels_VesselId" FOREIGN KEY ("VesselId") REFERENCES public."Vessels"("Id") ON DELETE CASCADE;


--
-- Name: assignment_comments FK_assignment_comments_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_comments
    ADD CONSTRAINT "FK_assignment_comments_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE CASCADE;


--
-- Name: assignment_confirmations FK_assignment_confirmations_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_confirmations
    ADD CONSTRAINT "FK_assignment_confirmations_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE CASCADE;


--
-- Name: assignment_conflicts FK_assignment_conflicts_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_conflicts
    ADD CONSTRAINT "FK_assignment_conflicts_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE CASCADE;


--
-- Name: assignment_status_history FK_assignment_status_history_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.assignment_status_history
    ADD CONSTRAINT "FK_assignment_status_history_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE CASCADE;


--
-- Name: cargo_operations FK_cargo_operations_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.cargo_operations
    ADD CONSTRAINT "FK_cargo_operations_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE SET NULL;


--
-- Name: compliance_dimensions FK_compliance_dimensions_compliance_rules_RuleId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_dimensions
    ADD CONSTRAINT "FK_compliance_dimensions_compliance_rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES public.compliance_rules("Id") ON DELETE CASCADE;


--
-- Name: compliance_rules FK_compliance_rules_compliance_rule_sets_RuleSetId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_rules
    ADD CONSTRAINT "FK_compliance_rules_compliance_rule_sets_RuleSetId" FOREIGN KEY ("RuleSetId") REFERENCES public.compliance_rule_sets("Id") ON DELETE CASCADE;


--
-- Name: compliance_snapshots FK_compliance_snapshots_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_snapshots
    ADD CONSTRAINT "FK_compliance_snapshots_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: compliance_waivers FK_compliance_waivers_compliance_rules_RuleId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_waivers
    ADD CONSTRAINT "FK_compliance_waivers_compliance_rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES public.compliance_rules("Id") ON DELETE CASCADE;


--
-- Name: compliance_waivers FK_compliance_waivers_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.compliance_waivers
    ADD CONSTRAINT "FK_compliance_waivers_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: country_certificates FK_country_certificates_certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.country_certificates
    ADD CONSTRAINT "FK_country_certificates_certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public.certificates("Id") ON DELETE CASCADE;


--
-- Name: country_certificates FK_country_certificates_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.country_certificates
    ADD CONSTRAINT "FK_country_certificates_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE CASCADE;


--
-- Name: crew_access_grants FK_crew_access_grants_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_access_grants
    ADD CONSTRAINT "FK_crew_access_grants_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE SET NULL;


--
-- Name: crew_access_grants FK_crew_access_grants_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_access_grants
    ADD CONSTRAINT "FK_crew_access_grants_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: crew_assignments FK_crew_assignments_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_assignments
    ADD CONSTRAINT "FK_crew_assignments_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: crew_assignments FK_crew_assignments_manning_positions_ManningPositionId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_assignments
    ADD CONSTRAINT "FK_crew_assignments_manning_positions_ManningPositionId" FOREIGN KEY ("ManningPositionId") REFERENCES public.manning_positions("Id") ON DELETE SET NULL;


--
-- Name: crew_assignments FK_crew_assignments_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_assignments
    ADD CONSTRAINT "FK_crew_assignments_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE RESTRICT;


--
-- Name: crew_certificates FK_crew_certificates_certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_certificates
    ADD CONSTRAINT "FK_crew_certificates_certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public.certificates("Id") ON DELETE RESTRICT;


--
-- Name: crew_certificates FK_crew_certificates_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_certificates
    ADD CONSTRAINT "FK_crew_certificates_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: crew_certificates FK_crew_certificates_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_certificates
    ADD CONSTRAINT "FK_crew_certificates_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: crew_document_submissions FK_crew_document_submissions_countries_IssuingCountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_document_submissions
    ADD CONSTRAINT "FK_crew_document_submissions_countries_IssuingCountryId" FOREIGN KEY ("IssuingCountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: crew_document_submissions FK_crew_document_submissions_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_document_submissions
    ADD CONSTRAINT "FK_crew_document_submissions_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: crew_document_versions FK_crew_document_versions_crew_document_submissions_Submission~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_document_versions
    ADD CONSTRAINT "FK_crew_document_versions_crew_document_submissions_Submission~" FOREIGN KEY ("SubmissionId") REFERENCES public.crew_document_submissions("Id") ON DELETE CASCADE;


--
-- Name: crew_members FK_crew_members_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_members
    ADD CONSTRAINT "FK_crew_members_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: crew_members FK_crew_members_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_members
    ADD CONSTRAINT "FK_crew_members_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE SET NULL;


--
-- Name: crew_status_history FK_crew_status_history_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.crew_status_history
    ADD CONSTRAINT "FK_crew_status_history_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: document_verification_actions FK_document_verification_actions_document_verification_tasks_T~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.document_verification_actions
    ADD CONSTRAINT "FK_document_verification_actions_document_verification_tasks_T~" FOREIGN KEY ("TaskId") REFERENCES public.document_verification_tasks("Id") ON DELETE CASCADE;


--
-- Name: document_verification_tasks FK_document_verification_tasks_crew_document_submissions_Submi~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.document_verification_tasks
    ADD CONSTRAINT "FK_document_verification_tasks_crew_document_submissions_Submi~" FOREIGN KEY ("SubmissionId") REFERENCES public.crew_document_submissions("Id") ON DELETE CASCADE;


--
-- Name: document_verification_tasks FK_document_verification_tasks_crew_document_versions_VersionId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.document_verification_tasks
    ADD CONSTRAINT "FK_document_verification_tasks_crew_document_versions_VersionId" FOREIGN KEY ("VersionId") REFERENCES public.crew_document_versions("Id") ON DELETE RESTRICT;


--
-- Name: employment_documents FK_employment_documents_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.employment_documents
    ADD CONSTRAINT "FK_employment_documents_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: employment_documents FK_employment_documents_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.employment_documents
    ADD CONSTRAINT "FK_employment_documents_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: equipment_assets FK_equipment_assets_equipment_assets_ParentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_assets
    ADD CONSTRAINT "FK_equipment_assets_equipment_assets_ParentId" FOREIGN KEY ("ParentId") REFERENCES public.equipment_assets("Id") ON DELETE RESTRICT;


--
-- Name: equipment_group_members FK_equipment_group_members_equipment_assets_AssetId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_group_members
    ADD CONSTRAINT "FK_equipment_group_members_equipment_assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES public.equipment_assets("Id") ON DELETE CASCADE;


--
-- Name: equipment_group_members FK_equipment_group_members_equipment_groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.equipment_group_members
    ADD CONSTRAINT "FK_equipment_group_members_equipment_groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public.equipment_groups("Id") ON DELETE CASCADE;


--
-- Name: external_candidates FK_external_candidates_crew_members_LinkedCrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_candidates
    ADD CONSTRAINT "FK_external_candidates_crew_members_LinkedCrewMemberId" FOREIGN KEY ("LinkedCrewMemberId") REFERENCES public.crew_members("Id");


--
-- Name: external_candidates FK_external_candidates_external_requests_ExternalRequestId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_candidates
    ADD CONSTRAINT "FK_external_candidates_external_requests_ExternalRequestId" FOREIGN KEY ("ExternalRequestId") REFERENCES public.external_requests("Id") ON DELETE CASCADE;


--
-- Name: external_candidates FK_external_candidates_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_candidates
    ADD CONSTRAINT "FK_external_candidates_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id");


--
-- Name: external_request_messages FK_external_request_messages_external_requests_ExternalRequest~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_request_messages
    ADD CONSTRAINT "FK_external_request_messages_external_requests_ExternalRequest~" FOREIGN KEY ("ExternalRequestId") REFERENCES public.external_requests("Id") ON DELETE CASCADE;


--
-- Name: external_requests FK_external_requests_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_requests
    ADD CONSTRAINT "FK_external_requests_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE SET NULL;


--
-- Name: external_requests FK_external_requests_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.external_requests
    ADD CONSTRAINT "FK_external_requests_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE RESTRICT;


--
-- Name: health_documents FK_health_documents_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.health_documents
    ADD CONSTRAINT "FK_health_documents_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: manning_positions FK_manning_positions_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.manning_positions
    ADD CONSTRAINT "FK_manning_positions_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE RESTRICT;


--
-- Name: manning_positions FK_manning_positions_vessel_manning_standards_ManningStandardId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.manning_positions
    ADD CONSTRAINT "FK_manning_positions_vessel_manning_standards_ManningStandardId" FOREIGN KEY ("ManningStandardId") REFERENCES public.vessel_manning_standards("Id") ON DELETE CASCADE;


--
-- Name: material_request_items FK_material_request_items_material_requests_RequestId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.material_request_items
    ADD CONSTRAINT "FK_material_request_items_material_requests_RequestId" FOREIGN KEY ("RequestId") REFERENCES public.material_requests("Id") ON DELETE CASCADE;


--
-- Name: onboard_events FK_onboard_events_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboard_events
    ADD CONSTRAINT "FK_onboard_events_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE SET NULL;


--
-- Name: onboard_events FK_onboard_events_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboard_events
    ADD CONSTRAINT "FK_onboard_events_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: onboard_events FK_onboard_events_onboard_events_OriginalEventId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboard_events
    ADD CONSTRAINT "FK_onboard_events_onboard_events_OriginalEventId" FOREIGN KEY ("OriginalEventId") REFERENCES public.onboard_events("Id") ON DELETE SET NULL;


--
-- Name: onboarding_cases FK_onboarding_cases_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboarding_cases
    ADD CONSTRAINT "FK_onboarding_cases_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: onboarding_checklist_items FK_onboarding_checklist_items_onboarding_cases_OnboardingCaseId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.onboarding_checklist_items
    ADD CONSTRAINT "FK_onboarding_checklist_items_onboarding_cases_OnboardingCaseId" FOREIGN KEY ("OnboardingCaseId") REFERENCES public.onboarding_cases("Id") ON DELETE CASCADE;


--
-- Name: rank_certificates FK_rank_certificates_certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.rank_certificates
    ADD CONSTRAINT "FK_rank_certificates_certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public.certificates("Id") ON DELETE CASCADE;


--
-- Name: rank_certificates FK_rank_certificates_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.rank_certificates
    ADD CONSTRAINT "FK_rank_certificates_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE CASCADE;


--
-- Name: schedule_checklist_templates FK_schedule_checklist_templates_maintenance_schedules_Schedule~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.schedule_checklist_templates
    ADD CONSTRAINT "FK_schedule_checklist_templates_maintenance_schedules_Schedule~" FOREIGN KEY ("ScheduleId") REFERENCES public.maintenance_schedules("Id") ON DELETE CASCADE;


--
-- Name: schedule_spare_parts FK_schedule_spare_parts_maintenance_schedules_ScheduleId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.schedule_spare_parts
    ADD CONSTRAINT "FK_schedule_spare_parts_maintenance_schedules_ScheduleId" FOREIGN KEY ("ScheduleId") REFERENCES public.maintenance_schedules("Id") ON DELETE CASCADE;


--
-- Name: seafarer_documents FK_seafarer_documents_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.seafarer_documents
    ADD CONSTRAINT "FK_seafarer_documents_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: seafarer_documents FK_seafarer_documents_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.seafarer_documents
    ADD CONSTRAINT "FK_seafarer_documents_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: service_records FK_service_records_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.service_records
    ADD CONSTRAINT "FK_service_records_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: service_records FK_service_records_crew_members_CrewMemberId1; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.service_records
    ADD CONSTRAINT "FK_service_records_crew_members_CrewMemberId1" FOREIGN KEY ("CrewMemberId1") REFERENCES public.crew_members("Id");


--
-- Name: sign_off_records FK_sign_off_records_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "FK_sign_off_records_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE SET NULL;


--
-- Name: sign_off_records FK_sign_off_records_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "FK_sign_off_records_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: sign_off_records FK_sign_off_records_onboard_events_OnboardEventId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "FK_sign_off_records_onboard_events_OnboardEventId" FOREIGN KEY ("OnboardEventId") REFERENCES public.onboard_events("Id") ON DELETE SET NULL;


--
-- Name: sign_off_records FK_sign_off_records_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "FK_sign_off_records_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE RESTRICT;


--
-- Name: sign_off_records FK_sign_off_records_sign_on_records_SignOnRecordId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_off_records
    ADD CONSTRAINT "FK_sign_off_records_sign_on_records_SignOnRecordId" FOREIGN KEY ("SignOnRecordId") REFERENCES public.sign_on_records("Id") ON DELETE SET NULL;


--
-- Name: sign_on_records FK_sign_on_records_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_on_records
    ADD CONSTRAINT "FK_sign_on_records_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE SET NULL;


--
-- Name: sign_on_records FK_sign_on_records_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_on_records
    ADD CONSTRAINT "FK_sign_on_records_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: sign_on_records FK_sign_on_records_onboard_events_OnboardEventId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_on_records
    ADD CONSTRAINT "FK_sign_on_records_onboard_events_OnboardEventId" FOREIGN KEY ("OnboardEventId") REFERENCES public.onboard_events("Id") ON DELETE SET NULL;


--
-- Name: sign_on_records FK_sign_on_records_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.sign_on_records
    ADD CONSTRAINT "FK_sign_on_records_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE RESTRICT;


--
-- Name: stock_receipt_items FK_stock_receipt_items_stock_receipts_ReceiptId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.stock_receipt_items
    ADD CONSTRAINT "FK_stock_receipt_items_stock_receipts_ReceiptId" FOREIGN KEY ("ReceiptId") REFERENCES public.stock_receipts("Id") ON DELETE CASCADE;


--
-- Name: stock_receipts FK_stock_receipts_material_requests_MaterialRequestId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.stock_receipts
    ADD CONSTRAINT "FK_stock_receipts_material_requests_MaterialRequestId" FOREIGN KEY ("MaterialRequestId") REFERENCES public.material_requests("Id") ON DELETE SET NULL;


--
-- Name: store_locations FK_store_locations_store_locations_ParentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.store_locations
    ADD CONSTRAINT "FK_store_locations_store_locations_ParentId" FOREIGN KEY ("ParentId") REFERENCES public.store_locations("Id") ON DELETE RESTRICT;


--
-- Name: travel_documents FK_travel_documents_countries_CountryId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_documents
    ADD CONSTRAINT "FK_travel_documents_countries_CountryId" FOREIGN KEY ("CountryId") REFERENCES public.countries("Id") ON DELETE SET NULL;


--
-- Name: travel_documents FK_travel_documents_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_documents
    ADD CONSTRAINT "FK_travel_documents_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: travel_requests FK_travel_requests_crew_assignments_AssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_requests
    ADD CONSTRAINT "FK_travel_requests_crew_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES public.crew_assignments("Id") ON DELETE CASCADE;


--
-- Name: travel_requests FK_travel_requests_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_requests
    ADD CONSTRAINT "FK_travel_requests_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: travel_segments FK_travel_segments_travel_requests_TravelRequestId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_segments
    ADD CONSTRAINT "FK_travel_segments_travel_requests_TravelRequestId" FOREIGN KEY ("TravelRequestId") REFERENCES public.travel_requests("Id") ON DELETE CASCADE;


--
-- Name: travel_status_history FK_travel_status_history_travel_requests_TravelRequestId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.travel_status_history
    ADD CONSTRAINT "FK_travel_status_history_travel_requests_TravelRequestId" FOREIGN KEY ("TravelRequestId") REFERENCES public.travel_requests("Id") ON DELETE CASCADE;


--
-- Name: vessel_certificate_assignments FK_vessel_certificate_assignments_certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.vessel_certificate_assignments
    ADD CONSTRAINT "FK_vessel_certificate_assignments_certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public.certificates("Id") ON DELETE CASCADE;


--
-- Name: voyage_actual_revenues FK_voyage_actual_revenues_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_actual_revenues
    ADD CONSTRAINT "FK_voyage_actual_revenues_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_advance_payments FK_voyage_advance_payments_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_advance_payments
    ADD CONSTRAINT "FK_voyage_advance_payments_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_bunker_plans FK_voyage_bunker_plans_voyage_plan_legs_PlanLegId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_bunker_plans
    ADD CONSTRAINT "FK_voyage_bunker_plans_voyage_plan_legs_PlanLegId" FOREIGN KEY ("PlanLegId") REFERENCES public.voyage_plan_legs("Id") ON DELETE SET NULL;


--
-- Name: voyage_bunker_plans FK_voyage_bunker_plans_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_bunker_plans
    ADD CONSTRAINT "FK_voyage_bunker_plans_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_cargo_plans FK_voyage_cargo_plans_voyage_plan_legs_PlanLegId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_cargo_plans
    ADD CONSTRAINT "FK_voyage_cargo_plans_voyage_plan_legs_PlanLegId" FOREIGN KEY ("PlanLegId") REFERENCES public.voyage_plan_legs("Id") ON DELETE SET NULL;


--
-- Name: voyage_cargo_plans FK_voyage_cargo_plans_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_cargo_plans
    ADD CONSTRAINT "FK_voyage_cargo_plans_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_cost_estimates FK_voyage_cost_estimates_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_cost_estimates
    ADD CONSTRAINT "FK_voyage_cost_estimates_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_crew_assignments FK_voyage_crew_assignments_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_assignments
    ADD CONSTRAINT "FK_voyage_crew_assignments_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE CASCADE;


--
-- Name: voyage_crew_assignments FK_voyage_crew_assignments_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_assignments
    ADD CONSTRAINT "FK_voyage_crew_assignments_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE SET NULL;


--
-- Name: voyage_crew_assignments FK_voyage_crew_assignments_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_assignments
    ADD CONSTRAINT "FK_voyage_crew_assignments_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_crew_change_plans FK_voyage_crew_change_plans_crew_members_CrewMemberId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_change_plans
    ADD CONSTRAINT "FK_voyage_crew_change_plans_crew_members_CrewMemberId" FOREIGN KEY ("CrewMemberId") REFERENCES public.crew_members("Id") ON DELETE SET NULL;


--
-- Name: voyage_crew_change_plans FK_voyage_crew_change_plans_ranks_RankId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_change_plans
    ADD CONSTRAINT "FK_voyage_crew_change_plans_ranks_RankId" FOREIGN KEY ("RankId") REFERENCES public.ranks("Id") ON DELETE SET NULL;


--
-- Name: voyage_crew_change_plans FK_voyage_crew_change_plans_voyage_plan_legs_PlanLegId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_change_plans
    ADD CONSTRAINT "FK_voyage_crew_change_plans_voyage_plan_legs_PlanLegId" FOREIGN KEY ("PlanLegId") REFERENCES public.voyage_plan_legs("Id") ON DELETE SET NULL;


--
-- Name: voyage_crew_change_plans FK_voyage_crew_change_plans_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_crew_change_plans
    ADD CONSTRAINT "FK_voyage_crew_change_plans_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_disbursements FK_voyage_disbursements_voyage_advance_payments_AdvancePayment~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_disbursements
    ADD CONSTRAINT "FK_voyage_disbursements_voyage_advance_payments_AdvancePayment~" FOREIGN KEY ("AdvancePaymentId") REFERENCES public.voyage_advance_payments("Id") ON DELETE SET NULL;


--
-- Name: voyage_disbursements FK_voyage_disbursements_voyage_expense_requests_ExpenseRequest~; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_disbursements
    ADD CONSTRAINT "FK_voyage_disbursements_voyage_expense_requests_ExpenseRequest~" FOREIGN KEY ("ExpenseRequestId") REFERENCES public.voyage_expense_requests("Id") ON DELETE SET NULL;


--
-- Name: voyage_disbursements FK_voyage_disbursements_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_disbursements
    ADD CONSTRAINT "FK_voyage_disbursements_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_expense_requests FK_voyage_expense_requests_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_expense_requests
    ADD CONSTRAINT "FK_voyage_expense_requests_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_log_entries FK_voyage_log_entries_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_log_entries
    ADD CONSTRAINT "FK_voyage_log_entries_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE SET NULL;


--
-- Name: voyage_plan_legs FK_voyage_plan_legs_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_plan_legs
    ADD CONSTRAINT "FK_voyage_plan_legs_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_revenue_estimates FK_voyage_revenue_estimates_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_revenue_estimates
    ADD CONSTRAINT "FK_voyage_revenue_estimates_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_reviews FK_voyage_reviews_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_reviews
    ADD CONSTRAINT "FK_voyage_reviews_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_settlements FK_voyage_settlements_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_settlements
    ADD CONSTRAINT "FK_voyage_settlements_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- Name: voyage_status_history FK_voyage_status_history_voyage_records_VoyageId; Type: FK CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public.voyage_status_history
    ADD CONSTRAINT "FK_voyage_status_history_voyage_records_VoyageId" FOREIGN KEY ("VoyageId") REFERENCES public.voyage_records("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict p3xFAqlyNjiCo9DYc3h6VLgoqr8pEaylm0hNbaL1antbftmWenkfDkVWgsaCMgP



-- Seed migration history so EF does not re-run already-applied migrations
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
('20260310144509_InitialCreate', '8.0.0'),
('20260310154924_ReplaceNationalityWithCountryId', '8.0.0'),
('20260311031412_AddVesselIdToCrewMember', '8.0.0'),
('20260311155340_AddVesselCertificateAssignments', '8.0.0'),
('20260315180409_AddCrewReviewWorkflowColumns', '8.0.0'),
('20260311040145_AddBunkerAndPositionReports', '8.0.0'),
('20260311065927_AlignReportFieldsWithEdge', '8.0.0'),
('20260315123341_AddVoyageSyncCoreMirror', '8.0.0'),
('20260315130449_AddVoyageMirrorPlanningCore', '8.0.0'),
('20260315141013_AddPmsMaterials', '8.0.0'),
('20260315141316_AddVoyageMirrorPlanningFinancial', '8.0.0'),
('20260317031411_AddFullMaintenanceTasks', '8.0.0'),
('20260317031827_RebuildMaintenanceTasks', '8.0.0'),
('20260319130406_AddVesselIdToMaterialItems', '8.0.0'),
('20260321120000_AddSyncNodeSecurityRegistry', '8.0.0'),
('20260321173000_AddSyncKeyRotationGraceWindow', '8.0.0')
ON CONFLICT DO NOTHING;
