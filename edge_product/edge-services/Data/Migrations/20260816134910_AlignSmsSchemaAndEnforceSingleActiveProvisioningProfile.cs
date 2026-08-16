using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignSmsSchemaAndEnforceSingleActiveProvisioningProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE public.sms_filled_records
    ADD COLUMN IF NOT EXISTS form_code character varying(50) NOT NULL DEFAULT '';

ALTER TABLE public.sms_filled_records
    ADD COLUMN IF NOT EXISTS form_title character varying(300) NOT NULL DEFAULT '';

ALTER TABLE public.sms_filled_records
    ADD COLUMN IF NOT EXISTS procedure_code character varying(50) NOT NULL DEFAULT '';

DROP INDEX IF EXISTS public.idx_epp_is_active;

CREATE UNIQUE INDEX IF NOT EXISTS idx_epp_is_active
    ON public.edge_provisioning_profile (is_active)
    WHERE is_active = true;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS public.idx_epp_is_active;

CREATE INDEX IF NOT EXISTS idx_epp_is_active
    ON public.edge_provisioning_profile (is_active);
");
        }
    }
}
