# Quick Test Guide - Server Configuration

## 🚀 Quick Start Testing

### **Step 1: Rebuild Mobile App**

```bash
cd frontend-mobile
flutter clean
flutter pub get
flutter run -d windows
```

---

### **Step 2: Test Server Configuration Dialog**

#### **Test 1: Open Dialog**
1. App launches to Login Screen
2. See current server URL below login form
3. Click **"Server Settings"** button
4. ✅ Server Configuration Dialog opens

#### **Test 2: View Examples**
1. In dialog, click **"?"** (help) icon
2. ✅ Example URLs dialog appears
3. Click any example
4. ✅ URL is filled in input field

#### **Test 3: Paste from Clipboard**
1. Copy this: `http://192.168.1.100:5001`
2. Click **"📋"** (paste) icon
3. ✅ URL pasted into field

#### **Test 4: URL Validation**
1. Enter invalid URL: `not-a-url`
2. Try to test connection
3. ✅ Shows validation error: "Invalid URL format"

4. Enter valid URL: `http://localhost:5001`
5. ✅ No validation error

---

### **Step 3: Test Connection**

#### **Scenario A: Server Running**
```powershell
# In terminal 1: Start Edge Server
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

In mobile app:
1. Enter: `http://localhost:5001`
2. Click **"Test"** button
3. Wait 2-5 seconds
4. ✅ **Result:** "✓ Connection successful!" (green)

#### **Scenario B: Server Stopped**
```powershell
# Stop Edge Server (Ctrl+C in terminal 1)
```

In mobile app:
1. Enter: `http://localhost:5001`
2. Click **"Test"** button
3. Wait 5 seconds
4. ✅ **Result:** "✗ Connection failed: ..." (red)

#### **Scenario C: Wrong IP**
In mobile app:
1. Enter: `http://192.168.99.99:5001`
2. Click **"Test"** button
3. ✅ **Result:** Connection timeout or failure

---

### **Step 4: Save and Verify Persistence**

1. Enter valid URL: `http://localhost:5001`
2. Click **"Test"** → Wait for success
3. Click **"Save"**
4. ✅ SnackBar: "Server URL updated to: http://localhost:5001"
5. ✅ Dialog closes
6. ✅ Login screen shows new URL

**Close and reopen app:**
7. ✅ URL still shows: `http://localhost:5001`

---

### **Step 5: Test Login Flow**

#### **With Correct Server:**
```powershell
# Ensure Edge Server is running
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

In mobile app:
1. Verify server URL: `http://localhost:5001`
2. Enter Crew ID: `CM001`
3. Enter Password: `password123`
4. Click **"Login"**
5. ✅ Login successful → Navigate to Dashboard

#### **With Wrong Server:**
In mobile app:
1. Click "Server Settings"
2. Change URL to: `http://192.168.99.99:5001`
3. Click "Save"
4. Enter Crew ID: `CM001`
5. Enter Password: `password123`
6. Click "Login"
7. ✅ Login fails → Error message shown

---

## 🧪 Detailed Test Cases

### **TC-01: Default URL Loaded**
**Steps:**
1. Fresh install app
2. Open app
**Expected:**
- Default URL shown: `http://192.168.1.100:5001`

---

### **TC-02: URL Validation - Empty**
**Steps:**
1. Open Server Settings
2. Clear URL field
3. Try to test
**Expected:**
- Error: "Please enter server URL"

---

### **TC-03: URL Validation - Missing Protocol**
**Steps:**
1. Enter: `192.168.1.100:5001`
2. Try to test
**Expected:**
- Error: "Invalid URL format (must start with http:// or https://)"

---

### **TC-04: URL Validation - Wrong Protocol**
**Steps:**
1. Enter: `ftp://192.168.1.100:5001`
2. Try to test
**Expected:**
- Error: "Invalid URL format (must start with http:// or https://)"

---

### **TC-05: Test Connection - Success**
**Precondition:**
- Edge Server running on localhost:5001
**Steps:**
1. Enter: `http://localhost:5001`
2. Click "Test"
**Expected:**
- Loading indicator appears
- After 2-3 seconds: "✓ Connection successful!" (green)

---

### **TC-06: Test Connection - Server Down**
**Precondition:**
- Edge Server NOT running
**Steps:**
1. Enter: `http://localhost:5001`
2. Click "Test"
**Expected:**
- Loading indicator appears
- After 5 seconds: "✗ Connection failed: ..." (red/orange)

---

### **TC-07: Save URL**
**Steps:**
1. Enter: `http://localhost:5001`
2. Click "Save"
**Expected:**
- SnackBar: "Server URL updated to: http://localhost:5001" (green)
- Dialog closes
- Login screen shows: `http://localhost:5001`

---

### **TC-08: URL Persists After Restart**
**Steps:**
1. Save URL: `http://localhost:5001`
2. Close app completely
3. Reopen app
**Expected:**
- Login screen shows: `http://localhost:5001`

---

### **TC-09: Login After URL Change**
**Steps:**
1. Login with default URL (if server running)
2. Go to Dashboard
3. Back to Settings
4. Change server URL
5. Save
**Expected:**
- User logged out
- Redirected to Login Screen
- New URL shown

---

### **TC-10: Example URLs**
**Steps:**
1. Open Server Settings
2. Click "?" icon
3. Click any example URL
**Expected:**
- Example URL filled in input field
- Help dialog closes

---

### **TC-11: Paste from Clipboard**
**Steps:**
1. Copy URL to clipboard: `http://192.168.1.50:5001`
2. Open Server Settings
3. Click "📋" icon
**Expected:**
- URL pasted into field

---

## 🎯 Acceptance Criteria

- ✅ User can view current server URL on Login Screen
- ✅ User can open Server Configuration Dialog
- ✅ User can see example URLs and use them
- ✅ User can paste URL from clipboard
- ✅ URL validation works correctly
- ✅ Test connection provides accurate feedback
- ✅ URL persists after saving
- ✅ URL persists after app restart
- ✅ Login works with configured server
- ✅ Login fails with wrong server
- ✅ Changing server logs user out
- ✅ No compile errors
- ✅ No runtime crashes
- ✅ UI is responsive and user-friendly

---

## 🐛 Known Issues to Check

### **Issue 1: Hive Box Not Initialized**
**Symptom:** App crashes on startup
**Check:** `main.dart` has `await Hive.openBox('server_config')`
**Fix:** Already handled in `ServerConfigStorage._getBox()`

### **Issue 2: ApiClient Not Singleton**
**Symptom:** URL changes don't persist
**Check:** `ApiClient()` returns same instance
**Fix:** Already implemented with factory constructor

### **Issue 3: Server Uses localhost**
**Symptom:** Mobile can't connect from other device
**Check:** Edge Server binds to `0.0.0.0` not `localhost`
**Fix:** Update server startup command

---

## 📝 Test Results Template

```markdown
## Test Results - Server Configuration Feature

**Date:** 2025-10-23
**Tester:** [Your Name]
**Platform:** Windows Desktop

### Test Cases

| TC# | Test Case | Status | Notes |
|-----|-----------|--------|-------|
| TC-01 | Default URL Loaded | ⬜ PASS / FAIL | |
| TC-02 | URL Validation - Empty | ⬜ PASS / FAIL | |
| TC-03 | URL Validation - Missing Protocol | ⬜ PASS / FAIL | |
| TC-04 | URL Validation - Wrong Protocol | ⬜ PASS / FAIL | |
| TC-05 | Test Connection - Success | ⬜ PASS / FAIL | |
| TC-06 | Test Connection - Server Down | ⬜ PASS / FAIL | |
| TC-07 | Save URL | ⬜ PASS / FAIL | |
| TC-08 | URL Persists After Restart | ⬜ PASS / FAIL | |
| TC-09 | Login After URL Change | ⬜ PASS / FAIL | |
| TC-10 | Example URLs | ⬜ PASS / FAIL | |
| TC-11 | Paste from Clipboard | ⬜ PASS / FAIL | |

### Summary
- **Total:** 11
- **Passed:** 0
- **Failed:** 0
- **Blocked:** 0

### Issues Found
1. [Describe any issues]
2. ...

### Recommendations
1. [Suggestions for improvement]
2. ...
```

---

## 🔄 Regression Testing

After implementing Server Configuration, verify these still work:

- ✅ Normal login flow (without changing server)
- ✅ Logout functionality
- ✅ Navigation to other screens
- ✅ Data loading from API
- ✅ Offline mode fallback
- ✅ Sync functionality

---

## ⚡ Quick Commands

### **Start Edge Server:**
```powershell
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

### **Run Mobile App:**
```bash
cd frontend-mobile
flutter run -d windows
```

### **Check Server Listening:**
```powershell
netstat -an | findstr :5001
```

### **Get Your IP:**
```powershell
ipconfig | findstr IPv4
```

### **Test API with curl:**
```powershell
curl http://localhost:5001/swagger/index.html
```

---

**Status:** ✅ Ready for Testing  
**Estimated Testing Time:** 15-20 minutes
