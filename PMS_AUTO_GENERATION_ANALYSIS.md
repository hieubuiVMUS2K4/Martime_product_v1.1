# 🔍 PMS Auto Task Generation - Logic Analysis & Optimization

## 📊 Current System Analysis

### ✅ What Works Well in Gantt Chart

**Today Indicator (Blue Vertical Line)**
- **Purpose**: Visual reference point showing current date
- **Benefits**:
  - Quick identification of overdue tasks (left of line)
  - Upcoming tasks visibility (right of line)
  - Timeline perspective at a glance
- **Industry Standard**: Yes, this is essential in all professional Gantt charts
- **Keep**: ✅ Absolutely

**Other Good Features**:
- Color-coded priority levels (Critical/High/Medium/Low)
- Progress bars showing time elapsed
- Due date milestones (blue circles)
- Equipment grouping on left panel
- View modes (Week/Month/Quarter)

---

## ⚠️ CRITICAL ISSUE: Auto Task Generation Logic

### Current Implementation

```csharp
// From MaintenanceSchedulerService.cs line 94
var daysUntilDue = (schedule.NextDueDate.Value.Date - now.Date).Days;

if (daysUntilDue <= schedule.DaysBeforeDue)
{
    // Generate task
}
```

**Default Values**:
- `DaysBeforeDue` = 7 days (default)
- Can be configured per schedule: 7, 10, 14 days

### ❌ Problem Identified

**Scenario**: 
- Task requires 3 days to complete (`estimatedDurationHours = 24h`)
- `DaysBeforeDue = 2` (auto-generate 2 days before due)
- Task generated on Day -2
- Due date: Day 0
- **Result**: Only 2 days to complete a 3-day task! ❌

**Real Example from Database**:
```sql
Main Engine - 500H Service
- EstimatedDurationHours: 4 hours
- DaysBeforeDue: 10 days
- Interval: 500 running hours

Generator #1 - 250H Service  
- EstimatedDurationHours: 2 hours
- DaysBeforeDue: 7 days
- Interval: 250 running hours
```

---

## 🌍 Industry Best Practices - How Real Ships Do It

### 1. **Leading Time = Work Duration + Buffer**

**Formula from ISM Code & Classification Societies**:
```
Auto-Generate Date = Due Date - (Estimated Duration + Safety Buffer)

Where:
- Safety Buffer = 2-3x estimated duration (for unexpected delays)
- Minimum = 7 days (for routine tasks)
- Maximum = 30 days (for major overhauls)
```

**Example**:
```
Task: Main Engine Overhaul
- Estimated Duration: 5 days
- Safety Buffer: 10 days (2x)
- Auto-Generate: 15 days before due date
```

### 2. **Classification Society Requirements**

**From IACS (International Association of Classification Societies)**:

| Task Criticality | Minimum Lead Time |
|-----------------|-------------------|
| CRITICAL | 30 days |
| HIGH | 14 days |
| MEDIUM | 7 days |
| LOW | 7 days |

### 3. **Real-World Maritime PMS Systems**

Popular systems: AMOS, Sertica, Mespas

**Common Pattern**:
```
Lead Time = MAX(
    EstimatedDuration × 3,
    MinimumLeadTime based on Priority,
    SparePartsOrderTime + 7 days
)
```

### 4. **Spare Parts Consideration**

Critical factor often missed:
- Generator spare parts: 7-14 days delivery
- Main engine parts: 30-60 days delivery
- Critical safety equipment: 90 days delivery

**Best Practice**:
```csharp
if (task.RequiresSpareParts) 
{
    leadTime = Math.Max(leadTime, sparePartsLeadTime + 7);
}
```

---

## 🔧 Recommended Solution

### Option 1: Dynamic Calculation (RECOMMENDED)

```csharp
public int CalculateOptimalLeadTime(MaintenanceSchedule schedule)
{
    // Base: 3x estimated duration (man-hours to calendar days)
    var baseDays = (int)Math.Ceiling(schedule.EstimatedDurationHours / 8.0 * 3);
    
    // Priority-based minimum
    var minDays = schedule.Priority switch
    {
        "CRITICAL" => 30,
        "HIGH" => 14,
        "MEDIUM" => 7,
        "LOW" => 7,
        _ => 7
    };
    
    // Spare parts lead time
    var sparePartsLeadTime = 0;
    if (schedule.ScheduleSpareParts?.Any() == true)
    {
        sparePartsLeadTime = 7; // Default 7 days for parts
        
        // Check if critical parts (e.g., from main supplier)
        if (schedule.RequiresCriticalParts)
            sparePartsLeadTime = 30;
    }
    
    // Take maximum of all factors
    return Math.Max(Math.Max(baseDays, minDays), sparePartsLeadTime);
}
```

### Option 2: Simple Rule-Based (QUICK FIX)

```csharp
// Quick fix: Enforce minimums
public int GetMinimumLeadTime(string priority)
{
    return priority switch
    {
        "CRITICAL" => 30,
        "HIGH" => 14,
        "MEDIUM" => 10,
        "LOW" => 7,
        _ => 7
    };
}

// In scheduler:
var requiredLeadTime = GetMinimumLeadTime(schedule.Priority);
if (schedule.DaysBeforeDue < requiredLeadTime)
{
    _logger.LogWarning(
        "Schedule {Code} has insufficient lead time ({Days} days). " +
        "Minimum required: {Required} days for {Priority} priority",
        schedule.ScheduleCode, schedule.DaysBeforeDue, 
        requiredLeadTime, schedule.Priority);
    
    // Use system minimum instead
    schedule.DaysBeforeDue = requiredLeadTime;
}
```

### Option 3: Validation on Creation (PREVENTIVE)

Add validation when creating schedules:

```csharp
public async Task<ValidationResult> ValidateSchedule(CreateMaintenanceScheduleDto dto)
{
    var errors = new List<string>();
    
    // Calculate minimum required lead time
    var workDays = (int)Math.Ceiling(dto.EstimatedDurationHours / 8.0);
    var safetyBuffer = workDays * 2;
    var minLeadTime = workDays + safetyBuffer;
    
    if (dto.DaysBeforeDue < minLeadTime)
    {
        errors.Add(
            $"Lead time ({dto.DaysBeforeDue} days) is less than " +
            $"required minimum ({minLeadTime} days) for a " +
            $"{dto.EstimatedDurationHours}-hour task"
        );
    }
    
    // Priority-based validation
    var priorityMin = GetMinimumLeadTime(dto.Priority);
    if (dto.DaysBeforeDue < priorityMin)
    {
        errors.Add(
            $"{dto.Priority} priority tasks require minimum " +
            $"{priorityMin} days lead time"
        );
    }
    
    return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
}
```

---

## 📈 Enhanced Gantt Chart Features

### Additional Optimizations

1. **Work Period Visualization**
   ```
   Currently: Only milestone (due date)
   Better: Show work period bar (start → due date)
   ```

2. **Resource Capacity Line**
   ```
   Show crew availability / man-hours capacity per week
   Alert when overbooked
   ```

3. **Dependencies & Critical Path**
   ```
   Some tasks depend on others (e.g., dry dock tasks)
   Show dependency arrows
   Highlight critical path
   ```

4. **Spare Parts Readiness Indicator**
   ```
   ⚠️ Parts not ordered
   📦 Parts in transit
   ✅ Parts in stock
   ```

5. **Weather/Port Window**
   ```
   Some tasks require calm sea / port stay
   Show weather forecast overlay
   Show port call schedule
   ```

---

## 🎯 Immediate Action Items

### Priority 1: Fix Lead Time Logic (This Week)
- [ ] Implement Option 2 (Quick Fix) in MaintenanceSchedulerService
- [ ] Update existing schedules with correct minimums
- [ ] Add logging for insufficient lead time warnings

### Priority 2: Add Validation (Next Sprint)
- [ ] Implement Option 3 in MaintenanceScheduleController
- [ ] Add UI validation in ScheduleConfigPage
- [ ] Show warning badge in UI when lead time is insufficient

### Priority 3: Enhanced Calculation (Future)
- [ ] Implement Option 1 (Dynamic calculation)
- [ ] Add spare parts lead time tracking
- [ ] Create "What-if" scheduling tool

---

## 📚 References

- **ISM Code**: Chapter 10 - Maintenance of ship and equipment
- **IACS PR 38**: Planned Maintenance Systems
- **IMO Resolution A.1070(28)**: Survey guidelines
- **ISO 55000**: Asset Management Systems

---

## 💡 Key Takeaway

**The current `DaysBeforeDue` field is user-editable but needs validation!**

Users can set it too low, causing tasks to be generated without sufficient time for:
1. Ordering spare parts
2. Allocating crew resources  
3. Scheduling port time (if needed)
4. Handling unexpected delays

**Solution**: System should **suggest** optimal lead time based on:
- Task duration
- Priority level
- Spare parts requirements
- Historical completion time

But allow experienced planners to override with warnings.
