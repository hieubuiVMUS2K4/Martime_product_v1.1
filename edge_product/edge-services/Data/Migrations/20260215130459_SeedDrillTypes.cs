using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaritimeEdge.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDrillTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed drill types from Excel "Drill Training Schedule (1)"
            // CATEGORY 1: STATION DRILLS
            
            // 1. Abandon ship drill - Monthly
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, trigger_condition, trigger_within_days, warning_days_before, 
                    assigned_to_role, display_order, is_active, is_mandatory, is_fixed_interval, is_document_required,
                    is_secure_history, is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry,
                    instruction_content, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'ABANDON_SHIP_MONTHLY', 'Abandon ship drill (SOLAS III 19.3.2 & 19.3.4)', 
                 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.4', 'At least once every month', 
                 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'MASTER', 1, true, true, true, false, false, false, false, false,
                 '<p><strong>ABANDON SHIP DRILL:</strong> Launch lifeboats with crew and maneuver in water at least once every three months</p>', 
                 NOW(), NOW());
            ");
            
            // 2. Abandon ship drill - On departure trigger (*1)
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, trigger_condition, trigger_within_days, warning_days_before, 
                    assigned_to_role, display_order, is_active, is_mandatory, is_fixed_interval, is_document_required,
                    is_secure_history, is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry,
                    instruction_content, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'ABANDON_SHIP_ON_DEPARTURE', 'Abandon ship drill - Within 24h after departure *1', 
                 'STATION_DRILLS', 'SOLAS III 19.3.2 & 19.3.4', 'Within 24 hours after departure *1', 
                 'ON_EVENT', NULL, 'ON_DEPARTURE', 1, 0, 'MASTER', 2, true, true, false, false, false, false, false, false,
                 '<p><strong>*1 TRIGGER:</strong> If >25% crew turnover, conduct within 24h of departure</p>', 
                 NOW(), NOW());
            ");
            
            // 3. Fire drill - Monthly
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, trigger_condition, trigger_within_days, warning_days_before, 
                    assigned_to_role, display_order, is_active, is_mandatory, is_fixed_interval, is_document_required,
                    is_secure_history, is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry,
                    instruction_content, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'FIRE_DRILL_MONTHLY', 'Fire-fighting drill (SOLAS III 19.3.2)', 
                 'STATION_DRILLS', 'SOLAS III 19.3.2', 'At least once every month', 
                 'MONTHLY', 30, 'ROUTINE', NULL, 7, 'CHIEF_OFFICER', 3, true, true, true, false, false, false, false, false,
                 '<p><strong>FIRE DRILL:</strong> Train various scenarios - engine room, accommodation, galley, cargo hold. EVERY DRILL NEEDS MASTER APPROVAL</p>', 
                 NOW(), NOW());
            ");
            
            // 4. Fire drill - On departure trigger (*1)
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, trigger_condition, trigger_within_days, warning_days_before, 
                    assigned_to_role, display_order, is_active, is_mandatory, is_fixed_interval, is_document_required,
                    is_secure_history, is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry,
                    instruction_content, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'FIRE_DRILL_ON_DEPARTURE', 'Fire-fighting drill - Within 24h after departure *1', 
                 'STATION_DRILLS', 'SOLAS III 19.3.2', 'Within 24 hours after departure *1', 
                 'ON_EVENT', NULL, 'ON_DEPARTURE', 1, 0, 'CHIEF_OFFICER', 4, true, true, false, false, false, false, false, false,
                 '<p><strong>*1 TRIGGER:</strong> If >25% crew turnover</p>', 
                 NOW(), NOW());
            ");
            
            // 5. Rescue boat drill
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'RESCUE_BOAT_QUARTERLY', 'Rescue boat drill (Launching & maneuvering)', 
                 'STATION_DRILLS', 'SOLAS III 19.3.4.6', 'Each month (at least once every 3 months)', 
                 'MONTHLY', 30, 7, 'CHIEF_OFFICER', 5, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // 6. Enclosed space entry
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'ENCLOSED_SPACE_BIMONTHLY', 'Enclosed space entry and rescue drill', 
                 'STATION_DRILLS', 'SOLAS III 19.3.3 & 19.3.6', 'At least once every 2 months', 
                 'BI_MONTHLY', 60, 7, 'CHIEF_OFFICER', 6, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // CATEGORY 2: EXERCISES
            
            // 7. Emergency response drill
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'EMERGENCY_RESPONSE_ANNUAL', 'Emergency response drill', 
                 'EXERCISES', 'SOLAS III 19', 'At least once per year', 
                 'ANNUAL', 365, 30, 'MASTER', 7, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // 8. Cyber security drill
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'CYBER_SECURITY_QUARTERLY', 'Cyber security drill', 
                 'EXERCISES', 'IMO MSC-FAL.1/Circ.3', 'At least 3 times per year', 
                 'QUARTERLY', 120, 14, 'MASTER', 8, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // CATEGORY 3: EDUCATION
            
            // 9. Life-saving and fire equipment training - Monthly
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'LIFESAVING_FIRE_TRAINING_MONTHLY', 'Life-saving and Fire equipment training', 
                 'EDUCATION', 'SOLAS III 19.2.3', 'At least once every month', 
                 'MONTHLY', 30, 7, 'CHIEF_OFFICER', 9, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // 10. Life-saving and fire equipment training - On new crew
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, trigger_condition, trigger_within_days, warning_days_before, assigned_to_role, 
                    display_order, is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'LIFESAVING_FIRE_TRAINING_ON_NEW_CREW', 'Life-saving/Fire training - New crew (within 2 weeks)', 
                 'EDUCATION', 'SOLAS III 19.2.3', 'Within 2 weeks of joining', 
                 'ON_EVENT', 'ON_NEW_CREW', 14, 3, 'CHIEF_OFFICER', 10, true, true, false, false, false, false, false, false, NOW(), NOW());
            ");
            
            // CATEGORY 4: TRAINING
            
            // 11. Familiarization training
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, trigger_condition, trigger_within_days, warning_days_before, assigned_to_role, 
                    display_order, is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'FAMILIARIZATION_ON_JOINING', 'Familiarization training (upon joining)', 
                 'TRAINING', 'SOLAS VI/1 & STCW A-I/6', 'Upon joining', 
                 'ON_EVENT', 'ON_NEW_CREW', 1, 0, 'MASTER', 11, true, true, false, false, false, false, false, false, NOW(), NOW());
            ");
            
            // CATEGORY 5: CHECKS
            
            // 12. Fire line and hoses check
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'FIRE_LINE_HOSES_ANNUAL', 'Fire Line and Fire Hoses check', 
                 'CHECKS', 'SOLAS II-2/10.2.1.6', 'At least once per year', 
                 'ANNUAL', 365, 30, 'CHIEF_OFFICER', 12, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // CATEGORY 6: ISPS
            
            // 13. Security drill - Quarterly
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'SECURITY_DRILL_QUARTERLY', 'Security Drill (ISPS)', 
                 'ISPS', 'ISPS Code A/13.4', 'At least once every 3 months (not exceeding 18 months)', 
                 'QUARTERLY', 90, 14, 'MASTER', 13, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
            
            // 14. Security drill - On new crew
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, trigger_condition, trigger_within_days, warning_days_before, assigned_to_role, 
                    display_order, is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'SECURITY_DRILL_ON_NEW_CREW', 'Security Drill - New crew (within 1 week) *1', 
                 'ISPS', 'ISPS Code A/13.4', 'Within 1 week for new crew *1', 
                 'ON_EVENT', 'ON_NEW_CREW', 7, 1, 'MASTER', 14, true, true, false, false, false, false, false, false, NOW(), NOW());
            ");
            
            // 15. Security exercise - Annual
            migrationBuilder.Sql(@"
                INSERT INTO drill_types (id, drill_code, drill_name, category, regulation_source, regulation_period, 
                    frequency_type, frequency_days, warning_days_before, assigned_to_role, display_order, 
                    is_active, is_mandatory, is_fixed_interval, is_document_required, is_secure_history, 
                    is_crew_member_required, is_mandatory_sign_on_evaluation, has_no_expiry, created_at, updated_at)
                VALUES 
                (gen_random_uuid(), 'SECURITY_EXERCISE_ANNUAL', 'Security Exercise (ISPS)', 
                 'ISPS', 'ISPS Code A/13.5', 'At least once per year (not exceeding 18 months)', 
                 'ANNUAL', 365, 30, 'MASTER', 15, true, true, true, false, false, false, false, false, NOW(), NOW());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all seeded drill types
            migrationBuilder.Sql(@"
                DELETE FROM drill_logs WHERE drill_type_id IN (SELECT id FROM drill_types);
                DELETE FROM drill_schedules WHERE drill_type_id IN (SELECT id FROM drill_types);
                DELETE FROM drill_types;
            ");
        }
    }
}
