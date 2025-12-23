# Visual Test Guide - Text Overflow Fixes

## Quick Test Instructions

### 1. Kanban Board (Maintenance Page)
**URL:** `http://localhost:3003/pms/maintenance`

**Test Cases:**
```
✓ Create a task with very long equipment name (100+ chars)
✓ Add a task with long description (500+ chars)
✓ Check that column headers truncate with tooltips
✓ Verify cards maintain consistent size
```

**Expected Behavior:**
- Card titles should truncate with "..." and show full text on hover
- Equipment names should truncate with tooltip
- No horizontal scrolling within cards
- Cards stay within column width

---

### 2. Task Details Modal
**URL:** Click any task card → View full details

**Test Cases:**
```
✓ Add very long rejection reason (1000+ chars)
✓ Add multi-line verification notes with line breaks
✓ Use long equipment group name
✓ Add long task description
```

**Expected Behavior:**
- All text wraps within modal boundaries
- Line breaks preserved in notes/reasons
- Modal stays centered and readable
- No text overflow outside modal

**Test String Examples:**
```
Rejection Reason: "This is a very long rejection reason that contains multiple sentences and should wrap properly within the modal without breaking the layout or causing horizontal scrolling. It includes special characters like @#$%^&*() and numbers 12345678901234567890 to test various scenarios."

Equipment Name: "Main Engine - Starboard Side - Cylinder #3 - Fuel Injection System - High Pressure Pump - Emergency Backup Unit (Installed 2024)"

Notes: "Line 1: Normal operation observed
Line 2: Temperature readings within spec
Line 3: No abnormal sounds detected
Line 4: Visual inspection completed
Line 5: All parameters nominal"
```

---

### 3. Maintenance History
**URL:** `http://localhost:3003/pms/maintenance-history`

**Test Cases:**
```
✓ View completed tasks with long notes
✓ Check spare parts descriptions
✓ Open task detail modal
✓ Verify multi-line text preserves formatting
```

**Expected Behavior:**
- History cards expand vertically, not horizontally
- Notes section wraps text properly
- Modal displays all information without overflow
- Icons remain aligned with text

---

### 4. Deferral Management
**URL:** `http://localhost:3003/pms/deferrals`

**Test Cases:**
```
✓ Submit deferral with long reason (500+ chars)
✓ Add review notes with multiple paragraphs
✓ Check reason display in card view
```

**Expected Behavior:**
- Deferral reasons wrap within card boundaries
- Review notes display properly
- No layout breaking with long text

---

### 5. Equipment Groups Table
**URL:** `http://localhost:3003/pms/equipment-groups`

**Test Cases:**
```
✓ Add equipment group with very long name
✓ Check table cell width constraints
✓ Verify text wrapping in cells
```

**Expected Behavior:**
- Table cells expand vertically for long names
- Text wraps within `max-w-xs` constraint
- Table remains horizontally scrollable but text doesn't overflow cells

---

## Mobile Testing

### Responsive Breakpoints
- **Mobile:** 375px width
- **Tablet:** 768px width
- **Desktop:** 1024px+ width

### Mobile-Specific Checks
```
✓ Kanban columns stack properly
✓ Modal text wraps on small screens
✓ Touch targets remain usable
✓ No horizontal scrolling on text content
```

---

## Browser Compatibility

### Modern Browsers (Should Work Perfectly)
- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Edge 90+
- ✅ Safari 14+

### CSS Properties Used
```css
/* All widely supported */
word-break: break-word;
white-space: pre-wrap;
overflow: hidden;
text-overflow: ellipsis;
```

---

## Common Issues & Solutions

### Issue: Text still overflows
**Solution:** Check if parent has `min-w-0` or proper flex constraints

### Issue: Layout shifts when text wraps
**Solution:** Add `min-h-[...]` to maintain consistent card heights

### Issue: Icons misaligned
**Solution:** Verify `flex-shrink-0` on icon elements

### Issue: Tooltips not showing
**Solution:** Check `title` attribute exists on truncated elements

---

## Performance Testing

### Load Test Scenarios
1. **100 tasks** with long descriptions in Kanban
2. **50+ completed tasks** in history with long notes
3. **20 deferral requests** with long reasons

**Expected Performance:**
- No visible lag or jank
- Smooth scrolling
- Fast modal open/close

---

## Accessibility Checks

### Screen Reader Testing
```
✓ Truncated text has full text in title attribute
✓ Long descriptions are fully readable
✓ No hidden content that screen readers can't access
```

### Keyboard Navigation
```
✓ Tab through cards maintains focus visibility
✓ Modal can be closed with Escape key
✓ All interactive elements reachable
```

---

## Visual Regression Checks

### Before vs After Screenshots
Take screenshots of:
1. Kanban board with long text tasks
2. Task details modal with long rejection reason
3. History page with long notes
4. Deferral card with long reason
5. Equipment groups table with long names

Compare:
- ❌ Old: Text overflows, breaks layout
- ✅ New: Text wraps, layout intact

---

## Sign-off Checklist

- [ ] All test cases passed
- [ ] No console errors
- [ ] Responsive design works
- [ ] Performance acceptable
- [ ] Accessibility verified
- [ ] Visual regression check completed
- [ ] Browser compatibility confirmed

**Tested by:** _____________
**Date:** _____________
**Browser:** _____________
**Device:** _____________
**Status:** ⬜ Pass ⬜ Fail

---

## Edge Cases Covered

✅ **1000+ character text** in single field
✅ **Multiple consecutive line breaks** in notes
✅ **Special characters** (@, #, $, %, &, *)
✅ **URLs and file paths** in text
✅ **Numbers-only strings** (IDs, codes)
✅ **Mixed language content** (if applicable)
✅ **Emoji in text** fields
✅ **Tab and newline characters**

---

**Ready for Production:** ✅ YES / ⬜ NO
