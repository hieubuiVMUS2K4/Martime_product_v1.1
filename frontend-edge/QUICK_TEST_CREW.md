# ⚡ Quick Test Guide - Crew CRUD
**5-Minute Professional Testing**

## 🎯 Prerequisites
- ✅ Backend running on http://localhost:5001
- ✅ Frontend running on http://localhost:3003
- ✅ Browser at http://localhost:3003/crew
- ✅ DevTools open (F12) → Network tab

---

## 🧪 Test 1: Add New Crew (Happy Path)
**Time: 1 min**

1. Click "Add Crew Member" button
2. Fill form:
   - Crew ID: `TEST001`
   - Full Name: `Test User`
   - Position: `Able Seaman`
   - Nationality: `Vietnam`
3. Click "Add Crew Member" (bottom button)

**✅ Success**: Modal closes, new crew appears in list

**Network**: `POST /api/crew` → 201 Created

---

## 🧪 Test 2: Add Duplicate Crew ID (Error)
**Time: 30 sec**

1. Click "Add Crew Member"
2. Crew ID: `CREW001` (existing)
3. Fill other required fields
4. Save

**✅ Expected**: Red error below "Crew ID" field: "Crew ID 'CREW001' already exists"

**Network**: `POST /api/crew` → 409 Conflict

---

## 🧪 Test 3: Add Missing Required Fields
**Time: 30 sec**

1. Click "Add Crew Member"
2. Leave Crew ID empty
3. Click Save

**✅ Expected**: Red error "Crew ID is required"

---

## 🧪 Test 4: View Crew Detail
**Time: 30 sec**

1. Click on any crew name (e.g., "Captain John Smith")
2. Full-screen page opens

**✅ Expected**: All crew info displayed, certificate status badges

**Network**: `GET /api/crew/{id}` → 200 OK

---

## 🧪 Test 5: Edit Crew (Happy Path)
**Time: 1 min**

1. From detail page, click "Edit Information"
2. Change phone number: `+84 90 999 8888`
3. Click "Save Changes"

**✅ Expected**: 
- Alert: "✅ Crew member updated successfully!"
- Fields revert to view mode
- Changes persist

**Network**: `PUT /api/crew/{id}` → 200 OK

---

## 🧪 Test 6: Edit with Duplicate Crew ID
**Time: 30 sec**

1. Click "Edit Information"
2. Change Crew ID to `CREW002` (existing)
3. Save

**✅ Expected**: Alert "Crew ID 'CREW002' already exists"

**Network**: `PUT /api/crew/{id}` → 409 Conflict

---

## 🧪 Test 7: Delete Crew
**Time: 1 min**

1. From detail page, click red "Delete Crew" button
2. Confirmation dialog shows crew details
3. Click OK

**✅ Expected**:
- Alert: "✅ Crew member deleted successfully!"
- Navigate back to /crew
- Crew removed from list

**Network**: `DELETE /api/crew/{id}` → 200 OK

---

## 🧪 Test 8: Network Error
**Time: 30 sec**

1. Stop backend (Ctrl+C in backend terminal)
2. Try to refresh crew list

**✅ Expected**: Alert "Cannot connect to server. Please check if backend is running."

3. Restart backend: `dotnet run --urls "http://localhost:5001"`

---

## 📊 Quick Checklist

| Test | Status | Time |
|------|--------|------|
| ☐ Add new crew | _____ | 1 min |
| ☐ Duplicate Crew ID error | _____ | 30s |
| ☐ Missing field validation | _____ | 30s |
| ☐ View detail page | _____ | 30s |
| ☐ Edit crew successfully | _____ | 1 min |
| ☐ Edit duplicate ID error | _____ | 30s |
| ☐ Delete with confirmation | _____ | 1 min |
| ☐ Network error handling | _____ | 30s |

**Total Time**: ~5-6 minutes

---

## 🐛 Expected Console Logs

### Success Operations:
```
✅ Crew member updated successfully
✅ Crew member deleted successfully
```

### Error Operations:
```
❌ API Error: {
  status: 409,
  url: "/api/crew",
  message: "Crew ID 'CREW001' already exists",
  details: ""
}
```

---

## 🚀 All Tests Pass?

✅ **READY FOR DEMO!**

Next steps:
1. Show stakeholders
2. Document any feedback
3. Move to Maintenance CRUD

❌ **Issues Found?**

See `BUG_FIXES_SUMMARY.md` for detailed troubleshooting.

---

**Tip**: Keep Network tab open to verify HTTP status codes match expected values!
