using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDrillTypesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================================
            // Seed 36 Drill Types (SOLAS/ISPS/STCW Compliance)
            // Based on: Drill Training Schedule for Bulk Carrier with Free-Fall Lifeboat
            // =====================================================================

            // STATION_DRILLS (14 drills)
            migrationBuilder.Sql(@"
                INSERT INTO ""DrillTypes"" (""Id"", ""DrillCode"", ""DrillName"", ""Category"", ""RegulationSource"", ""RegulationPeriod"", ""FrequencyType"", ""FrequencyDays"", ""TriggerCondition"", ""TriggerWithinDays"", ""WarningDaysBefore"", ""AssignedToRole"", ""IsMandatory"", ""DisplayOrder"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES 
                (gen_random_uuid(), 'ABANDON_SHIP_MONTHLY', 'Abandon ship drill (SOLAS III 19.3.4 / except 19.3.4.1 & 5)', 'STATION_DRILLS', 'SOLAS III 19.3.4', 'At least once every month', 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'MASTER', true, 1, true, NOW(), NOW()),
                (gen_random_uuid(), 'ABANDON_SHIP_DEPARTURE', 'Abandon ship drill - Within 24 hours after departure *1', 'STATION_DRILLS', 'SOLAS III 19.3.4.1', 'Within 24 hours after departure', 'ON_EVENT', 1, 'ON_DEPARTURE', 1, 0, 'MASTER', true, 2, true, NOW(), NOW()),
                (gen_random_uuid(), 'RESCUE_BOAT_MONTHLY', 'Rescue boat drill (Launching & maneuvering) - SOLAS III 19.3.4.6', 'STATION_DRILLS', 'SOLAS III 19.3.4.6', 'Each month (at least once every 3 months in all cases)', 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 3, true, NOW(), NOW()),
                (gen_random_uuid(), 'FIRE_DRILL_MONTHLY', 'Fire-fighting drill (SOLAS III 19.3.2 & 19.3.2.2)', 'STATION_DRILLS', 'SOLAS III 19.3.2', 'At least once every month', 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 4, true, NOW(), NOW()),
                (gen_random_uuid(), 'FIRE_DRILL_DEPARTURE', 'Fire-fighting drill - Within 24 hours after departure *1', 'STATION_DRILLS', 'SOLAS III 19.3.2.2', 'Within 24 hours after departure', 'ON_EVENT', 1, 'ON_DEPARTURE', 1, 0, 'CHIEF_OFFICER', true, 5, true, NOW(), NOW()),
                (gen_random_uuid(), 'FLOOD_FIGHTING_DRILL', 'Flood-fighting drill (Japanese regulation)', 'STATION_DRILLS', 'Japanese regulation', 'At least once every month', 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 6, true, NOW(), NOW()),
                (gen_random_uuid(), 'ENCLOSED_SPACE_DRILL', 'Enclosed space entry and rescue drill (SOLAS III 19.3.3 & 19.3.6)', 'STATION_DRILLS', 'SOLAS III 19.3.6', 'At least once every two (2) months', 'BI_MONTHLY', 60, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 7, true, NOW(), NOW()),
                (gen_random_uuid(), 'ABANDON_SHIP_3M', 'Abandon ship drill (Free-Fall or simulated launching)', 'STATION_DRILLS', 'SOLAS III 19.3.4.4', 'At least once every three (3) months', 'QUARTERLY', 90, 'ROUTINE', NULL, 7, 'MASTER', true, 8, true, NOW(), NOW()),
                (gen_random_uuid(), 'SOPEP_DRILL', 'Discharged oil removal drill (SOPEP § 6.2)', 'STATION_DRILLS', 'SOPEP § 6.2', 'At least once every three (3) months', 'QUARTERLY', 90, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 9, true, NOW(), NOW()),
                (gen_random_uuid(), 'EMERGENCY_STEERING', 'Emergency steering drill (SOLAS V 26.4)', 'STATION_DRILLS', 'SOLAS V 26.4', 'At least once every three (3) months', 'QUARTERLY', 90, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 10, true, NOW(), NOW()),
                (gen_random_uuid(), 'ABANDON_SHIP_6M', 'Abandon ship drill (Free-Fall or simulated launching) - 6 months', 'STATION_DRILLS', 'SOLAS III 19.3.4.4', 'Not more than six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', NULL, 14, 'MASTER', true, 11, true, NOW(), NOW()),
                (gen_random_uuid(), 'RECOVERY_MOB', 'Recovery of Persons from the Water Drill (MSC.1/Circ.1447)', 'STATION_DRILLS', 'MSC.1/Circ.1447', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', NULL, 14, 'CHIEF_OFFICER', true, 12, true, NOW(), NOW()),
                (gen_random_uuid(), 'COLLISION_GROUNDING_DRILL', 'Collision, Grounding, Heavy weather damage & Structural failure drill (VIQ 5.13)', 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', NULL, 14, 'MASTER', true, 13, true, NOW(), NOW()),
                (gen_random_uuid(), 'SERIOUS_INJURY_DRILL', 'Serious injury drill (VIQ 5.13)', 'STATION_DRILLS', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', NULL, 14, 'MASTER', true, 14, true, NOW(), NOW());
            ");

            // EXERCISES (11 drills)
            migrationBuilder.Sql(@"
                INSERT INTO ""DrillTypes"" (""Id"", ""DrillCode"", ""DrillName"", ""Category"", ""RegulationSource"", ""RegulationPeriod"", ""FrequencyType"", ""FrequencyDays"", ""TriggerCondition"", ""WarningDaysBefore"", ""AssignedToRole"", ""IsMandatory"", ""DisplayOrder"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (gen_random_uuid(), 'MOB_DRILL', 'Man overboard drill', 'EXERCISES', 'MSC.1/Circ.1447', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 15, true, NOW(), NOW()),
                (gen_random_uuid(), 'BLACKOUT_DRILL', 'Blackout drill (Critical machinery failure/VIQ 5.13)', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_ENGINEER', true, 16, true, NOW(), NOW()),
                (gen_random_uuid(), 'MAIN_ENGINE_EMERGENCY', 'Main engine emergency operation drill (Critical machinery failure/VIQ 5.13)', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_ENGINEER', true, 17, true, NOW(), NOW()),
                (gen_random_uuid(), 'EXPLOSION_DRILL', 'Explosion drill (VIQ 5.13)', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 18, true, NOW(), NOW()),
                (gen_random_uuid(), 'HELICOPTER_DRILL', 'Helicopter operation drill (VIQ 5.13)', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 19, true, NOW(), NOW()),
                (gen_random_uuid(), 'EMERGENCY_TOWING', 'Emergency Towing Equipment drill (VIQ 5.13)', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 20, true, NOW(), NOW()),
                (gen_random_uuid(), 'EMERGENCY_RESPONSE', 'Emergency response drill (Right ship)', 'EXERCISES', 'Right ship', 'Once a year', 'ANNUAL', 365, 'ROUTINE', 30, 'MASTER', true, 21, true, NOW(), NOW()),
                (gen_random_uuid(), 'CYBER_SECURITY', 'Cyber security drill', 'EXERCISES', 'Internal policy', 'At least three (3) times each calendar year', 'QUARTERLY', 120, 'ROUTINE', 14, 'MASTER', true, 22, true, NOW(), NOW()),
                (gen_random_uuid(), 'CARGO_SHIFTING_JETTISON', 'Cargo Shifting/Cargo Jettison', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 23, true, NOW(), NOW()),
                (gen_random_uuid(), 'INJURED_EVACUATION_SAR', 'Injured people/evacuation/SAR', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_OFFICER', true, 24, true, NOW(), NOW()),
                (gen_random_uuid(), 'TECHNICAL_FAILURES', 'Technical Failures', 'EXERCISES', 'VIQ 5.13', 'At least once every six (6) months', 'SEMI_ANNUAL', 180, 'ROUTINE', 14, 'CHIEF_ENGINEER', true, 25, true, NOW(), NOW());
            ");

            // TRAINING (5 drills)
            migrationBuilder.Sql(@"
                INSERT INTO ""DrillTypes"" (""Id"", ""DrillCode"", ""DrillName"", ""Category"", ""RegulationSource"", ""RegulationPeriod"", ""FrequencyType"", ""FrequencyDays"", ""TriggerCondition"", ""TriggerWithinDays"", ""WarningDaysBefore"", ""AssignedToRole"", ""IsMandatory"", ""DisplayOrder"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (gen_random_uuid(), 'FAMILIARIZATION_TRAINING', 'Familiarization training (STCW A-VI/1, 1.1-1.7)', 'TRAINING', 'STCW A-VI/1', 'Upon joining the ship', 'ON_EVENT', 0, 'ON_NEW_CREW', 1, 0, 'MASTER', true, 26, true, NOW(), NOW()),
                (gen_random_uuid(), 'LIFESAVING_EQUIPMENT_TRAINING', 'Method of using lifesaving and fire extinguishing equipment (SOLAS III 19.4.1)', 'TRAINING', 'SOLAS III 19.4.1', 'Within 2 weeks after crewmembers join the ship', 'ON_EVENT', 0, 'ON_NEW_CREW', 14, 3, 'CHIEF_OFFICER', true, 27, true, NOW(), NOW()),
                (gen_random_uuid(), 'SURVIVAL_AT_SEA', 'Survival at sea (SOLAS III 19.4.1)', 'TRAINING', 'SOLAS III 19.4.1', 'At least once every month', 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', true, 28, true, NOW(), NOW()),
                (gen_random_uuid(), 'DAVIT_LIFERAFT_TRAINING', 'Method of using Davit-launched life-raft launching appliances (SOLAS III 19.3.3)', 'TRAINING', 'SOLAS III 19.3.3', 'Not more than four (4) months', 'QUARTERLY', 120, 'ROUTINE', NULL, 14, 'CHIEF_OFFICER', true, 29, true, NOW(), NOW()),
                (gen_random_uuid(), 'CYBER_SECURITY_EDUCATION', 'Cyber security shipboard education', 'TRAINING', 'Internal policy', 'At least three (3) times each calendar year', 'QUARTERLY', 120, 'ROUTINE', NULL, 14, 'MASTER', true, 30, true, NOW(), NOW());
            ");

            // CHECKS (3 checks)
            migrationBuilder.Sql(@"
                INSERT INTO ""DrillTypes"" (""Id"", ""DrillCode"", ""DrillName"", ""Category"", ""RegulationSource"", ""RegulationPeriod"", ""FrequencyType"", ""FrequencyDays"", ""TriggerCondition"", ""WarningDaysBefore"", ""AssignedToRole"", ""IsMandatory"", ""DisplayOrder"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (gen_random_uuid(), 'FIRE_PUMP_CHECK', 'Emergency Fire Pump (Vacuum Pump)', 'CHECKS', 'SOLAS II-3, 14.2.2.1', 'At least once on vessel under the ballast passage', 'ON_EVENT', 0, 'ON_BALLAST', 0, 'CHIEF_ENGINEER', true, 31, true, NOW(), NOW()),
                (gen_random_uuid(), 'VACUUM_PUMP_OVERHAUL', 'Overhaul inspections of Vacuum Pump (NK Guidance B3 2.3)', 'CHECKS', 'NK Guidance B3 2.3', 'Every 5 years', 'FIVE_YEARS', 1825, 'ROUTINE', 90, 'CHIEF_ENGINEER', true, 32, true, NOW(), NOW()),
                (gen_random_uuid(), 'FIRE_LINE_HOSES_CHECK', 'Fire Line and Fire Hoses (SOLAS II-2, 14.2.2.1)', 'CHECKS', 'SOLAS II-2, 14.2.2.1', 'Once a year', 'ANNUAL', 365, 'ROUTINE', 30, 'CHIEF_ENGINEER', true, 33, true, NOW(), NOW());
            ");

            // ISPS (3 drills)
            migrationBuilder.Sql(@"
                INSERT INTO ""DrillTypes"" (""Id"", ""DrillCode"", ""DrillName"", ""Category"", ""RegulationSource"", ""RegulationPeriod"", ""FrequencyType"", ""FrequencyDays"", ""TriggerCondition"", ""WarningDaysBefore"", ""AssignedToRole"", ""IsMandatory"", ""DisplayOrder"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (gen_random_uuid(), 'ISPS_SECURITY_DRILL', 'Security Drill (ISPS Code Part B 13.6)', 'ISPS', 'ISPS Code Part B 13.6', 'At least three (3) times each calendar year', 'QUARTERLY', 120, 'ROUTINE', 14, 'MASTER', true, 34, true, NOW(), NOW()),
                (gen_random_uuid(), 'ISPS_EXERCISE', 'Exercise (ISPS Code Part B 13.7)', 'ISPS', 'ISPS Code Part B 13.7', 'At least once each calendar year with no more than 18 months', 'ANNUAL', 365, 'ROUTINE', 30, 'MASTER', true, 35, true, NOW(), NOW()),
                (gen_random_uuid(), 'SAFETY_COMMITTEE_MEETING', 'Safety Committee Meeting', 'ISPS', 'Internal policy', 'At least once every month', 'MONTHLY', 30, 'ROUTINE', 7, 'MASTER', true, 36, true, NOW(), NOW());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all drill types
            migrationBuilder.Sql(@"DELETE FROM ""DrillLogs"";");
            migrationBuilder.Sql(@"DELETE FROM ""DrillSchedules"";");
            migrationBuilder.Sql(@"DELETE FROM ""DrillTypes"";");
        }
    }
}
