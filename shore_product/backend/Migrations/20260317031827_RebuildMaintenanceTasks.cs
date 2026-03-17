using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace productapi.Migrations
{
    /// <inheritdoc />
    public partial class RebuildMaintenanceTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ShipId",
                table: "MaintenanceTasks");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "MaintenanceTasks",
                newName: "TaskDescription");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                table: "MaintenanceTasks",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<int>(
                name: "ActualDuration",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ActualRunningHours",
                table: "MaintenanceTasks",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedDepartment",
                table: "MaintenanceTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "MaintenanceTasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "MaintenanceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChecklistCompleted",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MaintenanceTasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeferralCount",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EquipmentAssetId",
                table: "MaintenanceTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentAssetName",
                table: "MaintenanceTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EquipmentGroupId",
                table: "MaintenanceTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentGroupName",
                table: "MaintenanceTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentId",
                table: "MaintenanceTasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentName",
                table: "MaintenanceTasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDuration",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPendingDeferral",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IntervalHours",
                table: "MaintenanceTasks",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCms",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeferredAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeferredBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDoneAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRejectedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastRejectedBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextDueAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MaintenanceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginNode",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PhotosUploaded",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "MaintenanceTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RejectionCount",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionHistory",
                table: "MaintenanceTasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "MaintenanceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredPhotos",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredSpareParts",
                table: "MaintenanceTasks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RunningHoursAtLastDone",
                table: "MaintenanceTasks",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "MaintenanceTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SparePartsUsed",
                table: "MaintenanceTasks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartedBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MaintenanceTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskId",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaskType",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TaskTypeId",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "MaintenanceTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationResult",
                table: "MaintenanceTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "MaintenanceTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedBy",
                table: "MaintenanceTasks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualDuration",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ActualRunningHours",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "AssignedDepartment",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ChecklistCompleted",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "DeferralCount",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentAssetId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentAssetName",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentGroupId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentGroupName",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EquipmentName",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "HasPendingDeferral",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IntervalDays",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IntervalHours",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IsCms",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "LastDeferredAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "LastDeferredBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "LastDoneAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "LastRejectedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "LastRejectedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "NextDueAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "OriginNode",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "PhotosUploaded",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RejectionCount",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RejectionHistory",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RequiredPhotos",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RequiredSpareParts",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "RunningHoursAtLastDone",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "SparePartsUsed",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "StartedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "SyncedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "TaskType",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "TaskTypeId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "VerificationResult",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "MaintenanceTasks");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "MaintenanceTasks",
                newName: "ScheduledAt");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "MaintenanceTasks",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MaintenanceTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipId",
                table: "MaintenanceTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
