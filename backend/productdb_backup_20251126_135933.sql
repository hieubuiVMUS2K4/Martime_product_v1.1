--
-- PostgreSQL database dump
--

\restrict 4cCash38UX0tzQ4EO3T4CT0q0WTNsMh7OdQbAyZtxrFA3h04HHy1HPQbcPwl7AT

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

ALTER TABLE IF EXISTS ONLY public."VesselPositions" DROP CONSTRAINT IF EXISTS "FK_VesselPositions_Vessels_VesselId";
ALTER TABLE IF EXISTS ONLY public."VesselAlerts" DROP CONSTRAINT IF EXISTS "FK_VesselAlerts_Vessels_VesselId";
ALTER TABLE IF EXISTS ONLY public."PortCalls" DROP CONSTRAINT IF EXISTS "FK_PortCalls_Vessels_VesselId";
ALTER TABLE IF EXISTS ONLY public."FuelConsumptions" DROP CONSTRAINT IF EXISTS "FK_FuelConsumptions_Vessels_VesselId";
ALTER TABLE IF EXISTS ONLY public."Certificates" DROP CONSTRAINT IF EXISTS "FK_Certificates_Vessels_VesselId";
DROP INDEX IF EXISTS public."IX_Vessels_IMO";
DROP INDEX IF EXISTS public."IX_VesselPositions_VesselId_Timestamp";
DROP INDEX IF EXISTS public."IX_VesselAlerts_VesselId_Timestamp";
DROP INDEX IF EXISTS public."IX_VesselAlerts_IsAcknowledged";
DROP INDEX IF EXISTS public."IX_PortCalls_VesselId_ArrivalTime";
DROP INDEX IF EXISTS public."IX_FuelConsumptions_VesselId";
DROP INDEX IF EXISTS public."IX_Certificates_VesselId_ExpiryDate";
DROP INDEX IF EXISTS public."IX_Certificates_CertificateNumber";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."VoyageRecords" DROP CONSTRAINT IF EXISTS "PK_VoyageRecords";
ALTER TABLE IF EXISTS ONLY public."Vessels" DROP CONSTRAINT IF EXISTS "PK_Vessels";
ALTER TABLE IF EXISTS ONLY public."VesselPositions" DROP CONSTRAINT IF EXISTS "PK_VesselPositions";
ALTER TABLE IF EXISTS ONLY public."VesselAlerts" DROP CONSTRAINT IF EXISTS "PK_VesselAlerts";
ALTER TABLE IF EXISTS ONLY public."Users" DROP CONSTRAINT IF EXISTS "PK_Users";
ALTER TABLE IF EXISTS ONLY public."TankLevels" DROP CONSTRAINT IF EXISTS "PK_TankLevels";
ALTER TABLE IF EXISTS ONLY public."Ships" DROP CONSTRAINT IF EXISTS "PK_Ships";
ALTER TABLE IF EXISTS ONLY public."SafetyAlarms" DROP CONSTRAINT IF EXISTS "PK_SafetyAlarms";
ALTER TABLE IF EXISTS ONLY public."ReportTypes" DROP CONSTRAINT IF EXISTS "PK_ReportTypes";
ALTER TABLE IF EXISTS ONLY public."PositionData" DROP CONSTRAINT IF EXISTS "PK_PositionData";
ALTER TABLE IF EXISTS ONLY public."PortCalls" DROP CONSTRAINT IF EXISTS "PK_PortCalls";
ALTER TABLE IF EXISTS ONLY public."NoonReports" DROP CONSTRAINT IF EXISTS "PK_NoonReports";
ALTER TABLE IF EXISTS ONLY public."NmeaRawData" DROP CONSTRAINT IF EXISTS "PK_NmeaRawData";
ALTER TABLE IF EXISTS ONLY public."NavigationData" DROP CONSTRAINT IF EXISTS "PK_NavigationData";
ALTER TABLE IF EXISTS ONLY public."MaritimeReports" DROP CONSTRAINT IF EXISTS "PK_MaritimeReports";
ALTER TABLE IF EXISTS ONLY public."MaintenanceTasks" DROP CONSTRAINT IF EXISTS "PK_MaintenanceTasks";
ALTER TABLE IF EXISTS ONLY public."GeneratorData" DROP CONSTRAINT IF EXISTS "PK_GeneratorData";
ALTER TABLE IF EXISTS ONLY public."FuelConsumptions" DROP CONSTRAINT IF EXISTS "PK_FuelConsumptions";
ALTER TABLE IF EXISTS ONLY public."FuelConsumptionData" DROP CONSTRAINT IF EXISTS "PK_FuelConsumptionData";
ALTER TABLE IF EXISTS ONLY public."EnvironmentalData" DROP CONSTRAINT IF EXISTS "PK_EnvironmentalData";
ALTER TABLE IF EXISTS ONLY public."EngineData" DROP CONSTRAINT IF EXISTS "PK_EngineData";
ALTER TABLE IF EXISTS ONLY public."CrewMembers" DROP CONSTRAINT IF EXISTS "PK_CrewMembers";
ALTER TABLE IF EXISTS ONLY public."Certificates" DROP CONSTRAINT IF EXISTS "PK_Certificates";
ALTER TABLE IF EXISTS ONLY public."AisData" DROP CONSTRAINT IF EXISTS "PK_AisData";
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."VoyageRecords";
DROP TABLE IF EXISTS public."Vessels";
DROP TABLE IF EXISTS public."VesselPositions";
DROP TABLE IF EXISTS public."VesselAlerts";
DROP TABLE IF EXISTS public."Users";
DROP TABLE IF EXISTS public."TankLevels";
DROP TABLE IF EXISTS public."Ships";
DROP TABLE IF EXISTS public."SafetyAlarms";
DROP TABLE IF EXISTS public."ReportTypes";
DROP TABLE IF EXISTS public."PositionData";
DROP TABLE IF EXISTS public."PortCalls";
DROP TABLE IF EXISTS public."NoonReports";
DROP TABLE IF EXISTS public."NmeaRawData";
DROP TABLE IF EXISTS public."NavigationData";
DROP TABLE IF EXISTS public."MaritimeReports";
DROP TABLE IF EXISTS public."MaintenanceTasks";
DROP TABLE IF EXISTS public."GeneratorData";
DROP TABLE IF EXISTS public."FuelConsumptions";
DROP TABLE IF EXISTS public."FuelConsumptionData";
DROP TABLE IF EXISTS public."EnvironmentalData";
DROP TABLE IF EXISTS public."EngineData";
DROP TABLE IF EXISTS public."CrewMembers";
DROP TABLE IF EXISTS public."Certificates";
DROP TABLE IF EXISTS public."AisData";
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
    "IsSynced" boolean NOT NULL,
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
-- Name: CrewMembers; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."CrewMembers" (
    "Id" uuid NOT NULL,
    "FullName" text NOT NULL,
    "Role" text NOT NULL,
    "ShipId" uuid NOT NULL
);


ALTER TABLE public."CrewMembers" OWNER TO product;

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
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."EngineData" OWNER TO product;

--
-- Name: EnvironmentalData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."EnvironmentalData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "AirTemperature" double precision,
    "BarometricPressure" double precision,
    "Humidity" double precision,
    "SeaTemperature" double precision,
    "WindSpeed" double precision,
    "WindDirection" double precision,
    "WaveHeight" double precision,
    "Visibility" double precision,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."EnvironmentalData" OWNER TO product;

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
    "IsSynced" boolean NOT NULL,
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
    "Voltage" double precision,
    "Frequency" double precision,
    "Current" double precision,
    "ActivePower" double precision,
    "PowerFactor" double precision,
    "RunningHours" double precision,
    "LoadPercent" double precision,
    "IsSynced" boolean NOT NULL,
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
    "Title" text NOT NULL,
    "Description" text NOT NULL,
    "ScheduledAt" timestamp with time zone NOT NULL,
    "ShipId" uuid NOT NULL
);


ALTER TABLE public."MaintenanceTasks" OWNER TO product;

--
-- Name: MaritimeReports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."MaritimeReports" (
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
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "OriginNode" character varying(50) NOT NULL,
    "DeletedAt" timestamp with time zone,
    "DeletedBy" character varying(100),
    "DeletedReason" text
);


ALTER TABLE public."MaritimeReports" OWNER TO product;

--
-- Name: NavigationData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."NavigationData" (
    "Id" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "HeadingTrue" double precision,
    "HeadingMagnetic" double precision,
    "RateOfTurn" double precision,
    "Pitch" double precision,
    "Roll" double precision,
    "SpeedThroughWater" double precision,
    "Depth" double precision,
    "WindSpeedRelative" double precision,
    "WindDirectionRelative" double precision,
    "WindSpeedTrue" double precision,
    "WindDirectionTrue" double precision,
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."NavigationData" OWNER TO product;

--
-- Name: NmeaRawData; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."NmeaRawData" (
    "Id" bigint NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "SentenceType" character varying(10) NOT NULL,
    "RawSentence" character varying(512) NOT NULL,
    "ChecksumValid" boolean NOT NULL,
    "DeviceSource" character varying(50),
    "IsSynced" boolean NOT NULL
);


ALTER TABLE public."NmeaRawData" OWNER TO product;

--
-- Name: NmeaRawData_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public."NmeaRawData" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."NmeaRawData_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: NoonReports; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."NoonReports" (
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
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."NoonReports" OWNER TO product;

--
-- Name: PortCalls; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."PortCalls" (
    "Id" uuid NOT NULL,
    "VesselId" uuid NOT NULL,
    "PortCode" text NOT NULL,
    "PortName" text NOT NULL,
    "ArrivalTime" timestamp with time zone,
    "DepartureTime" timestamp with time zone,
    "PortFees" numeric(12,2) NOT NULL,
    "CargoQuantity" numeric(10,3),
    "CargoType" text NOT NULL,
    "Purpose" text NOT NULL
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
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."PositionData" OWNER TO product;

--
-- Name: ReportTypes; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."ReportTypes" (
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


ALTER TABLE public."ReportTypes" OWNER TO product;

--
-- Name: ReportTypes_Id_seq; Type: SEQUENCE; Schema: public; Owner: product
--

ALTER TABLE public."ReportTypes" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ReportTypes_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


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
    "IsSynced" boolean NOT NULL,
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
    "IsSynced" boolean NOT NULL,
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
    "IMO" text NOT NULL,
    "Name" text NOT NULL,
    "CallSign" text NOT NULL,
    "VesselType" text NOT NULL,
    "GrossTonnage" double precision NOT NULL,
    "DeadWeight" double precision NOT NULL,
    "BuildDate" timestamp with time zone NOT NULL,
    "Flag" text NOT NULL,
    "IsActive" boolean NOT NULL
);


ALTER TABLE public."Vessels" OWNER TO product;

--
-- Name: VoyageRecords; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."VoyageRecords" (
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
    "IsSynced" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "OriginNode" character varying(50) NOT NULL
);


ALTER TABLE public."VoyageRecords" OWNER TO product;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: product
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO product;

--
-- Data for Name: AisData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."AisData" ("Id", "Timestamp", "Mmsi", "SpeedOverGround", "Latitude", "Longitude", "CourseOverGround", "ShipName", "Destination", "EtaMonth", "EtaDay", "EtaHour", "EtaMinute", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: Certificates; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."Certificates" ("Id", "VesselId", "CertificateType", "CertificateName", "IssuingAuthority", "IssueDate", "ExpiryDate", "CertificateNumber", "IsValid", "DocumentPath") FROM stdin;
\.


--
-- Data for Name: CrewMembers; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."CrewMembers" ("Id", "FullName", "Role", "ShipId") FROM stdin;
\.


--
-- Data for Name: EngineData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."EngineData" ("Id", "Timestamp", "EngineId", "Rpm", "LoadPercent", "FuelRate", "RunningHours", "AlarmStatus", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: EnvironmentalData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."EnvironmentalData" ("Id", "Timestamp", "AirTemperature", "BarometricPressure", "Humidity", "SeaTemperature", "WindSpeed", "WindDirection", "WaveHeight", "Visibility", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: FuelConsumptionData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."FuelConsumptionData" ("Id", "Timestamp", "FuelType", "ConsumedVolume", "ConsumedMass", "TankId", "Density", "DistanceTraveled", "TimeUnderway", "CargoWeight", "Co2Emissions", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: FuelConsumptions; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."FuelConsumptions" ("Id", "VesselId", "ReportDate", "FuelConsumed", "FuelType", "DistanceTraveled", "AverageSpeed", "FuelEfficiency") FROM stdin;
\.


--
-- Data for Name: GeneratorData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."GeneratorData" ("Id", "Timestamp", "GeneratorId", "IsRunning", "Voltage", "Frequency", "Current", "ActivePower", "PowerFactor", "RunningHours", "LoadPercent", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: MaintenanceTasks; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."MaintenanceTasks" ("Id", "Title", "Description", "ScheduledAt", "ShipId") FROM stdin;
\.


--
-- Data for Name: MaritimeReports; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."MaritimeReports" ("Id", "ReportNumber", "ReportTypeId", "ReportDateTime", "VoyageId", "Status", "PreparedBy", "MasterSignature", "SignedAt", "ReportData", "Remarks", "IsTransmitted", "TransmittedAt", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode", "DeletedAt", "DeletedBy", "DeletedReason") FROM stdin;
\.


--
-- Data for Name: NavigationData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."NavigationData" ("Id", "Timestamp", "HeadingTrue", "HeadingMagnetic", "RateOfTurn", "Pitch", "Roll", "SpeedThroughWater", "Depth", "WindSpeedRelative", "WindDirectionRelative", "WindSpeedTrue", "WindDirectionTrue", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: NmeaRawData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."NmeaRawData" ("Id", "Timestamp", "SentenceType", "RawSentence", "ChecksumValid", "DeviceSource", "IsSynced") FROM stdin;
\.


--
-- Data for Name: NoonReports; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."NoonReports" ("Id", "MaritimeReportId", "ReportDate", "Latitude", "Longitude", "CourseOverGround", "SpeedOverGround", "DistanceTraveled", "DistanceToGo", "EstimatedTimeOfArrival", "WeatherConditions", "SeaState", "AirTemperature", "SeaTemperature", "BarometricPressure", "WindDirection", "WindSpeed", "Visibility", "FuelOilConsumed", "DieselOilConsumed", "LubOilConsumed", "FreshWaterConsumed", "FuelOilROB", "DieselOilROB", "LubOilROB", "FreshWaterROB", "MainEngineRunningHours", "MainEngineRPM", "MainEnginePower", "AuxEngineRunningHours", "CargoOnBoard", "CargoDescription", "OperationalRemarks", "MachineryRemarks", "CargoRemarks", "CreatedAt") FROM stdin;
\.


--
-- Data for Name: PortCalls; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."PortCalls" ("Id", "VesselId", "PortCode", "PortName", "ArrivalTime", "DepartureTime", "PortFees", "CargoQuantity", "CargoType", "Purpose") FROM stdin;
\.


--
-- Data for Name: PositionData; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."PositionData" ("Id", "Timestamp", "Latitude", "Longitude", "SpeedOverGround", "CourseOverGround", "Source", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: ReportTypes; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."ReportTypes" ("Id", "TypeCode", "TypeName", "Category", "Description", "RegulationReference", "Frequency", "IsMandatory", "RequiresMasterSignature", "TemplateSchema", "IsActive", "CreatedAt") FROM stdin;
\.


--
-- Data for Name: SafetyAlarms; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."SafetyAlarms" ("Id", "Timestamp", "AlarmType", "AlarmCode", "Severity", "Location", "Description", "IsAcknowledged", "AcknowledgedAt", "AcknowledgedBy", "IsResolved", "ResolvedAt", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: Ships; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."Ships" ("Id", "Name", "IMO", "Capacity") FROM stdin;
\.


--
-- Data for Name: TankLevels; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."TankLevels" ("Id", "Timestamp", "TankId", "TankType", "LevelPercent", "VolumeLiters", "Temperature", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."Users" ("Id", "Username", "PasswordHash", "Role") FROM stdin;
\.


--
-- Data for Name: VesselAlerts; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."VesselAlerts" ("Id", "VesselId", "AlertType", "Message", "Severity", "Timestamp", "IsAcknowledged", "AcknowledgedAt", "AcknowledgedBy", "Data") FROM stdin;
\.


--
-- Data for Name: VesselPositions; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."VesselPositions" ("Id", "VesselId", "Latitude", "Longitude", "Speed", "Course", "Timestamp", "Source") FROM stdin;
\.


--
-- Data for Name: Vessels; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."Vessels" ("Id", "IMO", "Name", "CallSign", "VesselType", "GrossTonnage", "DeadWeight", "BuildDate", "Flag", "IsActive") FROM stdin;
\.


--
-- Data for Name: VoyageRecords; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."VoyageRecords" ("Id", "VoyageNumber", "DeparturePort", "DepartureTime", "ArrivalPort", "ArrivalTime", "CargoType", "CargoWeight", "DistanceTraveled", "FuelConsumed", "AverageSpeed", "VoyageStatus", "IsSynced", "CreatedAt", "UpdatedAt", "OriginNode") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: product
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251124135354_InitialSyncSchema	8.0.0
\.


--
-- Name: NmeaRawData_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: product
--

SELECT pg_catalog.setval('public."NmeaRawData_Id_seq"', 1, false);


--
-- Name: ReportTypes_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: product
--

SELECT pg_catalog.setval('public."ReportTypes_Id_seq"', 1, false);


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
-- Name: CrewMembers PK_CrewMembers; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."CrewMembers"
    ADD CONSTRAINT "PK_CrewMembers" PRIMARY KEY ("Id");


--
-- Name: EngineData PK_EngineData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."EngineData"
    ADD CONSTRAINT "PK_EngineData" PRIMARY KEY ("Id");


--
-- Name: EnvironmentalData PK_EnvironmentalData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."EnvironmentalData"
    ADD CONSTRAINT "PK_EnvironmentalData" PRIMARY KEY ("Id");


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
-- Name: MaritimeReports PK_MaritimeReports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."MaritimeReports"
    ADD CONSTRAINT "PK_MaritimeReports" PRIMARY KEY ("Id");


--
-- Name: NavigationData PK_NavigationData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."NavigationData"
    ADD CONSTRAINT "PK_NavigationData" PRIMARY KEY ("Id");


--
-- Name: NmeaRawData PK_NmeaRawData; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."NmeaRawData"
    ADD CONSTRAINT "PK_NmeaRawData" PRIMARY KEY ("Id");


--
-- Name: NoonReports PK_NoonReports; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."NoonReports"
    ADD CONSTRAINT "PK_NoonReports" PRIMARY KEY ("Id");


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
-- Name: ReportTypes PK_ReportTypes; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."ReportTypes"
    ADD CONSTRAINT "PK_ReportTypes" PRIMARY KEY ("Id");


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
-- Name: VoyageRecords PK_VoyageRecords; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."VoyageRecords"
    ADD CONSTRAINT "PK_VoyageRecords" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: product
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


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
-- Name: IX_PortCalls_VesselId_ArrivalTime; Type: INDEX; Schema: public; Owner: product
--

CREATE INDEX "IX_PortCalls_VesselId_ArrivalTime" ON public."PortCalls" USING btree ("VesselId", "ArrivalTime");


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
-- PostgreSQL database dump complete
--

\unrestrict 4cCash38UX0tzQ4EO3T4CT0q0WTNsMh7OdQbAyZtxrFA3h04HHy1HPQbcPwl7AT

