# 🎯 QUICK TEST - Server Configuration

## 📋 Testing Checklist

### ✅ **Pre-flight Check:**
- [x] Edge Server running on `0.0.0.0:5001`
- [x] Your IP: `192.168.1.138`
- [x] Flutter app building...

---

## 🧪 **Test Steps:**

### **Test 1: View Current URL**
1. ✅ App opens to Login Screen
2. ✅ Scroll down below login form
3. **EXPECT:** See blue badge with URL (default: `http://192.168.1.100:5001`)

---

### **Test 2: Open Server Settings**
1. ✅ Click **"Server Settings"** button
2. **EXPECT:** Dialog opens with current URL pre-filled

---

### **Test 3: View Examples**
1. ✅ In dialog, click **"?"** icon (help)
2. **EXPECT:** Example URLs dialog appears
3. ✅ Click any example
4. **EXPECT:** URL fills into input field

---

### **Test 4: Configure Your Server**
1. ✅ Enter URL: `http://192.168.1.138:5001`
2. ✅ Click **"Test"** button
3. **EXPECT:** 
   - Loading indicator appears
   - After 2-3 seconds: **"✓ Connection successful!"** (green box)

---

### **Test 5: Test with Wrong URL**
1. ✅ Change URL to: `http://192.168.1.999:5001`
2. ✅ Click **"Test"** button
3. **EXPECT:** 
   - After 5 seconds: **"✗ Connection failed: ..."** (red/orange box)

---

### **Test 6: Save Configuration**
1. ✅ Enter correct URL: `http://192.168.1.138:5001`
2. ✅ Test connection → Wait for green success
3. ✅ Click **"Save"** button
4. **EXPECT:**
   - Green SnackBar: "Server URL updated to: http://192.168.1.138:5001"
   - Dialog closes
   - Login screen shows new URL in blue badge

---

### **Test 7: Login with New Server**
1. ✅ See updated URL on login screen
2. ✅ Enter Crew ID: `CM001`
3. ✅ Enter Password: `password123`
4. ✅ Click **"Login"**
5. **EXPECT:**
   - Login successful
   - Navigate to Dashboard
   - Shows John Smith, Chief Engineer

---

### **Test 8: Verify Persistence (Optional)**
1. ✅ Close the app completely
2. ✅ Reopen the app
3. **EXPECT:** Login screen shows `http://192.168.1.138:5001` (persisted)

---

## 🎯 **Quick Test URLs:**

### ✅ **Valid URLs to test:**
```
http://192.168.1.138:5001  ← Your server (should work)
http://localhost:5001       ← Localhost (should work)
```

### ❌ **Invalid URLs to test:**
```
192.168.1.138:5001         ← Missing http:// (validation error)
http://                     ← Empty host (validation error)
http://192.168.1.999:5001  ← Wrong IP (connection failed)
```

---

## 🐛 **If Something Goes Wrong:**

### **Problem: "Connection failed" even with correct URL**

**Check:**
```powershell
# Verify server is listening on all interfaces
netstat -an | findstr :5001
# Should show: 0.0.0.0:5001
```

**Fix:**
```powershell
# Restart Edge Server with correct binding
cd edge-services
dotnet run --urls "http://0.0.0.0:5001"
```

---

### **Problem: Login fails after configuring**

**Check database has crew members:**
```powershell
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT crew_id, full_name FROM crew_members;"
```

**If empty, run:**
```powershell
cd edge-services
.\INSERT-SAMPLE-DATA.ps1
```

---

### **Problem: Validation error on URL**

**Make sure URL:**
- ✅ Starts with `http://` or `https://`
- ✅ Has valid host (IP or domain)
- ✅ No spaces or special characters

---

## 📸 **Expected Screenshots:**

### **1. Login Screen with URL:**
```
┌─────────────────────────────┐
│      Maritime Crew App      │
│                             │
│  [Crew ID input]            │
│  [Password input]           │
│  [     Login     ]          │
│                             │
│  🔵 http://192.168.1.138:5001 ← Here!
│                             │
│  ⚙️ Server Settings         │
└─────────────────────────────┘
```

### **2. Server Config Dialog:**
```
┌─────────────────────────────────┐
│ ⚙️ Server Configuration         │
│                                 │
│ Server URL:                     │
│ ┌─────────────────────┐  [?][📋]│
│ │http://192.168.1.138:│         │
│ │5001                 │         │
│ └─────────────────────┘         │
│                                 │
│ ┌───────────────────────────┐   │
│ │✓ Connection successful!   │   │ ← Green!
│ └───────────────────────────┘   │
│                                 │
│ 💡 How to connect to Edge Server│
│                                 │
│ [Cancel]      [Test]   [Save]   │
└─────────────────────────────────┘
```

### **3. After Save - SnackBar:**
```
┌─────────────────────────────────┐
│ ✓ Server URL updated to:        │
│   http://192.168.1.138:5001     │ ← Green SnackBar
└─────────────────────────────────┘
```

---

## ✅ **Success Criteria:**

- [ ] Can open Server Settings dialog
- [ ] Can view example URLs
- [ ] Can paste from clipboard
- [ ] URL validation works (shows error for invalid format)
- [ ] Test connection shows green success for valid server
- [ ] Test connection shows red failure for invalid server
- [ ] Save button updates URL
- [ ] URL displays on login screen
- [ ] Login works with configured server
- [ ] URL persists after app restart

---

## 🎉 **When All Tests Pass:**

You've successfully tested the Server Configuration feature! 🚀

**What this means:**
- ✅ Users can configure server URL without code changes
- ✅ Connection testing works before committing
- ✅ URL persists across app restarts
- ✅ Easy switching between servers (dev/prod)
- ✅ No more hardcoded IP addresses!

---

**Your server:** `http://192.168.1.138:5001`  
**Test account:** CM001 / password123  
**Status:** Ready to test! 🎯
