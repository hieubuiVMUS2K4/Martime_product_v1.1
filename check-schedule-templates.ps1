# Quick check script for schedule checklist templates
Write-Host "`n=== Checking Schedule 'aa' (sc-dtest) ===" -ForegroundColor Cyan

# Get schedule ID
$scheduleQuery = "SELECT id, schedule_code, schedule_name FROM maintenance_schedules WHERE schedule_code = 'sc-dtest';"
Write-Host "`nSchedule Info:" -ForegroundColor Yellow
docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge -c $scheduleQuery

# Get checklist templates count
$templatesQuery = "SELECT COUNT(*) as template_count FROM schedule_checklist_templates WHERE schedule_id = '4481eca8-ce8f-43d9-adda-45a98d57357c';"
Write-Host "`nChecklist Templates Count:" -ForegroundColor Yellow
docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge -c $templatesQuery

# Get checklist templates details
$detailsQuery = "SELECT sequence_order, checkpoint_description, requires_reading, normal_range_min, normal_range_max, unit FROM schedule_checklist_templates WHERE schedule_id = '4481eca8-ce8f-43d9-adda-45a98d57357c' ORDER BY sequence_order;"
Write-Host "`nChecklist Templates Details:" -ForegroundColor Yellow
docker exec -i maritime-edge-postgres psql -U edge_user -d maritime_edge -c $detailsQuery

Write-Host "`n=== Done ===" -ForegroundColor Green
