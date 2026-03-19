using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncStateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All DDL in this block uses IF NOT EXISTS / IF EXISTS guards.
            // The DB already has most of these changes applied outside of EF migrations,
            // so we only actually create what's genuinely missing (sync_state table).
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- ── Drop stale FK (may already be gone) ──────────────────────────────
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_maintenance_schedules_equipment_groups_equipment_group_id'
                          AND table_schema = 'public'
                    ) THEN
                        ALTER TABLE public.maintenance_schedules
                            DROP CONSTRAINT ""FK_maintenance_schedules_equipment_groups_equipment_group_id"";
                    END IF;

                    -- ── Drop nationality if still present ────────────────────────────────
                    ALTER TABLE public.crew_members DROP COLUMN IF EXISTS nationality;

                    -- ── Rename index (may already be renamed) ────────────────────────────
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='IX_maintenance_schedules_equipment_group_id') THEN
                        ALTER INDEX public.""IX_maintenance_schedules_equipment_group_id"" RENAME TO idx_schedule_equipment_group;
                    END IF;

                    -- ── Rename column (may already be renamed) ───────────────────────────
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='equipment_assets' AND column_name='ParentId') THEN
                        ALTER TABLE public.equipment_assets RENAME COLUMN ""ParentId"" TO parent_id;
                    END IF;

                    -- ── material_items.image_url ─────────────────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='material_items' AND column_name='image_url') THEN
                        ALTER TABLE public.material_items ADD COLUMN image_url character varying(500);
                    END IF;

                    -- ── maintenance_tasks.equipment_asset_id ─────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='maintenance_tasks' AND column_name='equipment_asset_id') THEN
                        ALTER TABLE public.maintenance_tasks ADD COLUMN equipment_asset_id uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='maintenance_tasks' AND column_name='equipment_asset_name') THEN
                        ALTER TABLE public.maintenance_tasks ADD COLUMN equipment_asset_name character varying(200);
                    END IF;

                    -- ── maintenance_schedules columns ────────────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='maintenance_schedules' AND column_name='equipment_asset_id') THEN
                        ALTER TABLE public.maintenance_schedules ADD COLUMN equipment_asset_id uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='maintenance_schedules' AND column_name='maintenance_category') THEN
                        ALTER TABLE public.maintenance_schedules ADD COLUMN maintenance_category character varying(20) NOT NULL DEFAULT '';
                    END IF;
                    -- Allow equipment_group_id to be nullable
                    ALTER TABLE public.maintenance_schedules ALTER COLUMN equipment_group_id DROP NOT NULL;

                    -- ── crew_members columns ─────────────────────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='country_id') THEN
                        ALTER TABLE public.crew_members ADD COLUMN country_id integer;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='edge_changes') THEN
                        ALTER TABLE public.crew_members ADD COLUMN edge_changes text;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='edge_changes_viewed') THEN
                        ALTER TABLE public.crew_members ADD COLUMN edge_changes_viewed boolean NOT NULL DEFAULT false;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='onboard_status') THEN
                        ALTER TABLE public.crew_members ADD COLUMN onboard_status character varying(20);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='onboard_status_changed_at') THEN
                        ALTER TABLE public.crew_members ADD COLUMN onboard_status_changed_at timestamp with time zone;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='onboard_status_changed_by') THEN
                        ALTER TABLE public.crew_members ADD COLUMN onboard_status_changed_by character varying(100);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='review_checklist') THEN
                        ALTER TABLE public.crew_members ADD COLUMN review_checklist text;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='crew_members' AND column_name='review_notes') THEN
                        ALTER TABLE public.crew_members ADD COLUMN review_notes text;
                    END IF;

                    -- ── Tables: create only if not exists ────────────────────────────────
                    CREATE TABLE IF NOT EXISTS public.inventory_stock (
                        id serial PRIMARY KEY,
                        material_item_id uuid NOT NULL,
                        store_location_id uuid NOT NULL,
                        quantity numeric(14,3) NOT NULL,
                        unit_cost numeric(18,2) NOT NULL,
                        last_receipt_date timestamp with time zone,
                        updated_at timestamp with time zone NOT NULL,
                        origin_node character varying(50) NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS public.material_item_equipments (
                        id uuid PRIMARY KEY,
                        material_item_id uuid NOT NULL,
                        equipment_asset_id uuid NOT NULL,
                        notes character varying(500),
                        created_at timestamp with time zone NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS public.material_requests (
                        id serial PRIMARY KEY,
                        request_code character varying(50) NOT NULL,
                        vessel_name character varying(150),
                        voyage_id uuid,
                        voyage_name character varying(150),
                        urgency character varying(20) NOT NULL,
                        needed_date timestamp with time zone NOT NULL,
                        request_date timestamp with time zone NOT NULL,
                        requested_by character varying(100),
                        notes text,
                        attachments text,
                        status character varying(20) NOT NULL,
                        is_active boolean NOT NULL,
                        created_at timestamp with time zone NOT NULL,
                        updated_at timestamp with time zone NOT NULL,
                        origin_node character varying(50) NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS public.store_locations (
                        id uuid PRIMARY KEY,
                        location_code character varying(50) NOT NULL,
                        name character varying(200) NOT NULL,
                        description character varying(500),
                        parent_id uuid REFERENCES public.store_locations(id) ON DELETE RESTRICT,
                        address character varying(300),
                        manager_name character varying(100),
                        phone character varying(50),
                        email character varying(100),
                        is_active boolean NOT NULL,
                        is_synced boolean NOT NULL,
                        created_at timestamp with time zone NOT NULL,
                        updated_at timestamp with time zone NOT NULL,
                        origin_node character varying(50) NOT NULL
                    );

                    -- ── sync_state: the whole reason for this migration ──────────────────
                    CREATE TABLE IF NOT EXISTS public.sync_state (
                        key character varying(100) PRIMARY KEY,
                        value text NOT NULL,
                        updated_at timestamp with time zone NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS public.material_request_items (
                        id serial PRIMARY KEY,
                        request_id integer NOT NULL REFERENCES public.material_requests(id) ON DELETE CASCADE,
                        equipment_asset_id uuid,
                        material_item_id uuid,
                        item_name character varying(200) NOT NULL,
                        description text,
                        unit character varying(20) NOT NULL,
                        quantity_on_hand numeric(14,3) NOT NULL,
                        quantity_requested numeric(14,3) NOT NULL,
                        note text
                    );

                    CREATE TABLE IF NOT EXISTS public.stock_receipts (
                        id serial PRIMARY KEY,
                        receipt_code character varying(50) NOT NULL,
                        vessel_name character varying(150),
                        voyage_id uuid,
                        voyage_name character varying(150),
                        supplier_code character varying(50),
                        supplier_name character varying(200),
                        received_date timestamp with time zone NOT NULL,
                        receipt_date timestamp with time zone NOT NULL,
                        created_by character varying(100),
                        notes text,
                        attachments text,
                        material_request_id integer REFERENCES public.material_requests(id) ON DELETE SET NULL,
                        status character varying(20) NOT NULL,
                        is_active boolean NOT NULL,
                        created_at timestamp with time zone NOT NULL,
                        updated_at timestamp with time zone NOT NULL,
                        origin_node character varying(50) NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS public.stock_receipt_items (
                        id serial PRIMARY KEY,
                        receipt_id integer NOT NULL REFERENCES public.stock_receipts(id) ON DELETE CASCADE,
                        store_location_id uuid,
                        material_item_id uuid,
                        item_code character varying(50),
                        item_name character varying(200) NOT NULL,
                        description text,
                        unit character varying(20) NOT NULL,
                        quantity_requested numeric(14,3) NOT NULL,
                        quantity_received numeric(14,3) NOT NULL,
                        unit_cost numeric(18,2),
                        currency character varying(3),
                        note text
                    );

                    -- ── Indexes: create only if not exists ───────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_maintenance_equipment_asset') THEN
                        CREATE INDEX idx_maintenance_equipment_asset ON public.maintenance_tasks(equipment_asset_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_schedule_equipment_asset') THEN
                        CREATE INDEX idx_schedule_equipment_asset ON public.maintenance_schedules(equipment_asset_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='IX_equipment_assets_parent_id') THEN
                        CREATE INDEX ""IX_equipment_assets_parent_id"" ON public.equipment_assets(parent_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_crew_country_id') THEN
                        CREATE INDEX idx_crew_country_id ON public.crew_members(country_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_crew_onboard_status') THEN
                        CREATE INDEX idx_crew_onboard_status ON public.crew_members(onboard_status) WHERE onboard_status IS NOT NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='uk_inventory_material_location') THEN
                        CREATE UNIQUE INDEX uk_inventory_material_location ON public.inventory_stock(material_item_id, store_location_id);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_material_request_status') THEN
                        CREATE INDEX idx_material_request_status ON public.material_requests(status);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='uk_material_request_code') THEN
                        CREATE UNIQUE INDEX uk_material_request_code ON public.material_requests(request_code);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_stock_receipt_status') THEN
                        CREATE INDEX idx_stock_receipt_status ON public.stock_receipts(status);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='uk_stock_receipt_code') THEN
                        CREATE UNIQUE INDEX uk_stock_receipt_code ON public.stock_receipts(receipt_code);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_store_location_active') THEN
                        CREATE INDEX idx_store_location_active ON public.store_locations(is_active) WHERE is_active = true;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='idx_store_location_synced') THEN
                        CREATE INDEX idx_store_location_synced ON public.store_locations(is_synced) WHERE is_synced = false;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE schemaname='public' AND indexname='uk_store_locations_code') THEN
                        CREATE UNIQUE INDEX uk_store_locations_code ON public.store_locations(location_code);
                    END IF;

                    -- ── FKs: add only if not exists ──────────────────────────────────────
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name='f_k_crew_members_countries_country_id' AND table_schema='public') THEN
                        ALTER TABLE public.crew_members ADD CONSTRAINT f_k_crew_members_countries_country_id
                            FOREIGN KEY (country_id) REFERENCES public.countries(id) ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name='FK_maintenance_schedules_equipment_assets_equipment_asset_id' AND table_schema='public') THEN
                        ALTER TABLE public.maintenance_schedules ADD CONSTRAINT ""FK_maintenance_schedules_equipment_assets_equipment_asset_id""
                            FOREIGN KEY (equipment_asset_id) REFERENCES public.equipment_assets(id) ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name='FK_maintenance_schedules_equipment_groups_equipment_group_id' AND table_schema='public') THEN
                        ALTER TABLE public.maintenance_schedules ADD CONSTRAINT ""FK_maintenance_schedules_equipment_groups_equipment_group_id""
                            FOREIGN KEY (equipment_group_id) REFERENCES public.equipment_groups(id) ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_crew_members_countries_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_assets_equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "inventory_stock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_item_equipments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_request_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipt_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "store_locations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sync_state",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_receipts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "material_requests",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "idx_maintenance_equipment_asset",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropIndex(
                name: "idx_schedule_equipment_asset",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropIndex(
                name: "IX_equipment_assets_parent_id",
                schema: "public",
                table: "equipment_assets");

            migrationBuilder.DropIndex(
                name: "idx_crew_country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropIndex(
                name: "idx_crew_onboard_status",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "public",
                table: "material_items");

            migrationBuilder.DropColumn(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_asset_name",
                schema: "public",
                table: "maintenance_tasks");

            migrationBuilder.DropColumn(
                name: "equipment_asset_id",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "maintenance_category",
                schema: "public",
                table: "maintenance_schedules");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "edge_changes",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "edge_changes_viewed",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status_changed_at",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "onboard_status_changed_by",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "review_checklist",
                schema: "public",
                table: "crew_members");

            migrationBuilder.DropColumn(
                name: "review_notes",
                schema: "public",
                table: "crew_members");

            migrationBuilder.RenameIndex(
                name: "idx_schedule_equipment_group",
                schema: "public",
                table: "maintenance_schedules",
                newName: "IX_maintenance_schedules_equipment_group_id");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                schema: "public",
                table: "equipment_assets",
                newName: "ParentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                schema: "public",
                table: "crew_members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8764), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8774) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8776) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8777) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8778) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8779), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8780) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8781) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8782) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8783) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8784) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "ranks",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8785), new DateTime(2026, 3, 15, 14, 12, 59, 372, DateTimeKind.Utc).AddTicks(8786) });

            migrationBuilder.AddForeignKey(
                name: "FK_maintenance_schedules_equipment_groups_equipment_group_id",
                schema: "public",
                table: "maintenance_schedules",
                column: "equipment_group_id",
                principalSchema: "public",
                principalTable: "equipment_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
