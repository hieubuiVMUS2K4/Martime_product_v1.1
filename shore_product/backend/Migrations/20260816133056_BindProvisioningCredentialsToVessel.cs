using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class BindProvisioningCredentialsToVessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE public.service_records
    DROP CONSTRAINT IF EXISTS ""FK_service_records_crew_members_CrewMemberId1"";

DROP INDEX IF EXISTS public.""IX_service_records_CrewMemberId1"";
DROP INDEX IF EXISTS public.""IX_sync_node_trackers_NodeApiTokenHash"";

ALTER TABLE public.service_records
    DROP COLUMN IF EXISTS ""CrewMemberId1"";

ALTER TABLE public.sync_node_trackers
    ADD COLUMN IF NOT EXISTS ""VesselId"" uuid;

CREATE TABLE IF NOT EXISTS public.ism_elements (
    ""Id"" integer NOT NULL,
    ""ChapterName"" character varying(200) NOT NULL,
    ""IsSynced"" boolean NOT NULL,
    ""OriginNode"" character varying(50) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_ism_elements"" PRIMARY KEY (""Id"")
);

CREATE TABLE IF NOT EXISTS public.sms_procedures (
    ""Id"" uuid NOT NULL,
    ""IsmElementId"" integer NOT NULL,
    ""ProcedureCode"" character varying(50) NOT NULL,
    ""Title"" character varying(200) NOT NULL,
    ""Content"" text NOT NULL,
    ""FilePath"" character varying(500),
    ""Version"" character varying(20) NOT NULL,
    ""PublishDate"" timestamp with time zone NOT NULL,
    ""Status"" character varying(30) NOT NULL,
    ""ObsoleteDate"" timestamp with time zone,
    ""ChangeNote"" character varying(1000),
    ""WatermarkText"" character varying(100) NOT NULL,
    ""IsSynced"" boolean NOT NULL,
    ""OriginNode"" character varying(50) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_sms_procedures"" PRIMARY KEY (""Id"")
);

CREATE TABLE IF NOT EXISTS public.sms_form_templates (
    ""Id"" uuid NOT NULL,
    ""SmsProcedureId"" uuid NOT NULL,
    ""FormCode"" character varying(50) NOT NULL,
    ""Title"" character varying(200) NOT NULL,
    ""ContentSchema"" text NOT NULL,
    ""IsSynced"" boolean NOT NULL,
    ""OriginNode"" character varying(50) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_sms_form_templates"" PRIMARY KEY (""Id"")
);

CREATE TABLE IF NOT EXISTS public.sms_procedure_acknowledgements (
    ""Id"" uuid NOT NULL,
    ""SmsProcedureId"" uuid NOT NULL,
    ""UserName"" character varying(100) NOT NULL,
    ""Rank"" character varying(100) NOT NULL,
    ""AcknowledgedAt"" timestamp with time zone NOT NULL,
    ""IsSynced"" boolean NOT NULL,
    ""OriginNode"" character varying(50) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_sms_procedure_acknowledgements"" PRIMARY KEY (""Id"")
);

CREATE TABLE IF NOT EXISTS public.sms_filled_records (
    ""Id"" uuid NOT NULL,
    ""SmsFormTemplateId"" uuid NOT NULL,
    ""FormCode"" character varying(50) NOT NULL,
    ""FormTitle"" character varying(300) NOT NULL,
    ""ProcedureCode"" character varying(50) NOT NULL,
    ""VesselName"" character varying(100) NOT NULL,
    ""FilledBy"" character varying(100) NOT NULL,
    ""FilledDate"" timestamp with time zone NOT NULL,
    ""FilledData"" text NOT NULL,
    ""DigitalSignatures"" text NOT NULL,
    ""Status"" character varying(30) NOT NULL,
    ""IsSynced"" boolean NOT NULL,
    ""OriginNode"" character varying(50) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    ""UpdatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_sms_filled_records"" PRIMARY KEY (""Id"")
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_sync_node_trackers_ImoNumber""
    ON public.sync_node_trackers (""ImoNumber"")
    WHERE ""ImoNumber"" IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_sync_node_trackers_NodeApiTokenHash""
    ON public.sync_node_trackers (""NodeApiTokenHash"")
    WHERE ""NodeApiTokenHash"" IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_sync_node_trackers_VesselId""
    ON public.sync_node_trackers (""VesselId"")
    WHERE ""VesselId"" IS NOT NULL;

CREATE INDEX IF NOT EXISTS ""IX_sms_filled_records_FormCode""
    ON public.sms_filled_records (""FormCode"");

CREATE INDEX IF NOT EXISTS ""IX_sms_filled_records_SmsFormTemplateId""
    ON public.sms_filled_records (""SmsFormTemplateId"");

CREATE INDEX IF NOT EXISTS ""IX_sms_filled_records_Status""
    ON public.sms_filled_records (""Status"");

CREATE INDEX IF NOT EXISTS ""IX_sms_filled_records_VesselName""
    ON public.sms_filled_records (""VesselName"");

CREATE INDEX IF NOT EXISTS ""IX_sms_form_templates_FormCode""
    ON public.sms_form_templates (""FormCode"");

CREATE INDEX IF NOT EXISTS ""IX_sms_form_templates_SmsProcedureId""
    ON public.sms_form_templates (""SmsProcedureId"");

CREATE INDEX IF NOT EXISTS ""IX_sms_procedure_acknowledgements_SmsProcedureId_UserName_Rank""
    ON public.sms_procedure_acknowledgements (""SmsProcedureId"", ""UserName"", ""Rank"");

CREATE INDEX IF NOT EXISTS ""IX_sms_procedures_IsmElementId""
    ON public.sms_procedures (""IsmElementId"");

CREATE INDEX IF NOT EXISTS ""IX_sms_procedures_ProcedureCode""
    ON public.sms_procedures (""ProcedureCode"");

CREATE INDEX IF NOT EXISTS ""IX_sms_procedures_Status""
    ON public.sms_procedures (""Status"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS public.""IX_sync_node_trackers_ImoNumber"";
DROP INDEX IF EXISTS public.""IX_sync_node_trackers_NodeApiTokenHash"";
DROP INDEX IF EXISTS public.""IX_sync_node_trackers_VesselId"";

ALTER TABLE public.sync_node_trackers
    DROP COLUMN IF EXISTS ""VesselId"";

CREATE INDEX IF NOT EXISTS ""IX_sync_node_trackers_NodeApiTokenHash""
    ON public.sync_node_trackers (""NodeApiTokenHash"");
");
        }
    }
}
