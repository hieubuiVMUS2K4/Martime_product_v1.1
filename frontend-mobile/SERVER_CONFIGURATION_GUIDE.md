# Server Configuration Feature - Complete Guide

## 🎯 Overview

The Server Configuration feature allows users to easily connect the mobile app to the Edge Server running on their local network (LAN). This eliminates the need to hardcode IP addresses and provides a user-friendly interface for network setup.

---

## ✨ Features Implemented

### 1. **Server Configuration Dialog**
- ✅ Input field for server URL with validation
- ✅ Test connection button to verify server accessibility
- ✅ Example URLs with quick selection
- ✅ Paste from clipboard support
- ✅ Real-time URL validation
- ✅ Visual feedback for connection status
- ✅ Persistent storage using Hive

### 2. **Login Screen Integration**
- ✅ Current server URL display below login form
- ✅ "Server Settings" button to open configuration dialog
- ✅ Automatic URL loading on screen init
- ✅ Visual indicator showing active server

### 3. **Dynamic API Client**
- ✅ Singleton ApiClient pattern
- ✅ Load server URL from storage on app start
- ✅ Update base URL when user changes settings
- ✅ Automatic logout when server changes

### 4. **Storage Layer**
- ✅ `ServerConfigStorage` class for URL persistence
- ✅ Default URL fallback
- ✅ URL validation helpers
- ✅ Example URLs provider

---

## 📱 User Flow

### **Step 1: Open App**
```
Login Screen
├── [Show current server URL]
└── [Server Settings Button]
```

### **Step 2: Configure Server**
```
User clicks "Server Settings"
↓
Server Configuration Dialog opens
├── Current URL pre-filled
├── Examples available
├── Test connection
└── Save new URL
```

### **Step 3: Test Connection**
```
User enters URL: http://192.168.1.100:5001
↓
Clicks "Test" button
↓
App tests connection to /swagger/index.html
↓
Shows result:
✓ Connection successful! (green)
or
✗ Connection failed: [error] (red)
```

### **Step 4: Save and Login**
```
User clicks "Save"
↓
URL saved to Hive storage
↓
ApiClient updated with new URL
↓
User logged out (if was logged in)
↓
Ready to login with new server
```

---

## 🔧 Technical Implementation

### **Files Created:**

1. **`lib/core/storage/server_config_storage.dart`**
   - Manages server URL persistence
   - Provides validation and examples
   - Default URL: `http://192.168.1.100:5001`

2. **`lib/presentation/widgets/server_config_dialog.dart`**
   - Full-screen dialog for server configuration
   - Test connection functionality
   - Example URLs helper
   - Paste from clipboard support

### **Files Modified:**

3. **`lib/presentation/screens/auth/login_screen.dart`**
   - Added server URL display
   - Added "Server Settings" button
   - Auto-load URL on init

4. **`lib/core/network/api_client.dart`**
   - Singleton pattern implementation
   - `initialize()` method to load saved URL
   - `updateBaseUrl()` to change URL dynamically

5. **`lib/main.dart`**
   - Added `ApiClient().initialize()` on app start

---

## 🚀 How to Use

### **For End Users:**

1. **Launch the Mobile App**
   ```
   flutter run -d windows
   ```

2. **On Login Screen:**
   - See current server URL displayed below the form
   - Click "Server Settings" button

3. **In Server Configuration Dialog:**
   - View current URL
   - Click "?" icon to see example URLs
   - Enter new URL: `http://YOUR_IP:5001`
   - Click "Test" to verify connection
   - Wait for green "✓ Connection successful!" message
   - Click "Save" to apply

4. **Login:**
   - Use your Crew ID (e.g., CM001)
   - Password: password123

---

## 🔍 Server Setup Requirements

### **On PC (Edge Server):**

1. **Find Your IP Address:**
   ```powershell
   ipconfig
   ```
   Look for "IPv4 Address" (e.g., 192.168.1.100)

2. **Start Edge Server on LAN Mode:**
   ```powershell
   cd edge-services
   dotnet run --urls "http://0.0.0.0:5001"
   ```
   ⚠️ **Important:** Must use `0.0.0.0` not `localhost` for LAN access

3. **Check Firewall:**
   - Windows may block port 5001
   - Allow "dotnet.exe" through Windows Firewall

### **On Mobile Device:**

1. **Connect to Same WiFi:**
   - PC and mobile must be on same network
   - Check WiFi name matches on both devices

2. **Enter Server URL:**
   - Format: `http://YOUR_PC_IP:5001`
   - Example: `http://192.168.1.100:5001`

---

## ✅ Validation Rules

### **URL Format:**
- ✅ Must start with `http://` or `https://`
- ✅ Must have valid host (IP or domain)
- ✅ Port is optional (defaults to 80/443)
- ❌ Cannot be empty
- ❌ Cannot have invalid characters

### **Examples of Valid URLs:**
```
✓ http://192.168.1.100:5001
✓ http://localhost:5001
✓ http://10.0.0.50:5001
✓ https://edge-server.local:5001
```

### **Examples of Invalid URLs:**
```
✗ 192.168.1.100:5001           (missing http://)
✗ http://                       (missing host)
✗ ftp://192.168.1.100:5001     (wrong protocol)
✗ http://edge server:5001      (space in URL)
```

---

## 🧪 Testing

### **Test Connection Feature:**

1. **Success Scenario:**
   ```
   Input: http://192.168.1.100:5001
   Server: Running and accessible
   Result: ✓ Connection successful!
   ```

2. **Failed Connection:**
   ```
   Input: http://192.168.1.200:5001
   Server: Not running or wrong IP
   Result: ✗ Connection failed: [DioException...]
   ```

3. **Invalid URL:**
   ```
   Input: not-a-valid-url
   Result: Form validation error
   ```

### **Test Login Flow:**

1. **Configure server** → Save
2. **App shows new URL** on login screen
3. **Enter credentials:**
   - Crew ID: CM001
   - Password: password123
4. **Login successful** → Navigate to Dashboard

---

## 📦 Storage Details

### **Hive Box:**
```dart
Box: 'server_config'
Key: 'server_url'
Type: String
Default: 'http://192.168.1.100:5001'
```

### **Storage Methods:**

```dart
// Get current URL
final url = await ServerConfigStorage.getServerUrl();

// Save new URL
await ServerConfigStorage.saveServerUrl('http://192.168.1.50:5001');

// Reset to default
await ServerConfigStorage.resetToDefault();

// Validate URL
bool isValid = ServerConfigStorage.isValidUrl('http://example.com');

// Get examples
List<String> examples = ServerConfigStorage.getExampleUrls();
```

---

## 🐛 Troubleshooting

### **Problem: "Connection failed"**

**Check:**
1. Edge Server is running: `netstat -an | findstr :5001`
2. Server uses `0.0.0.0` not `localhost`
3. Both devices on same WiFi network
4. Windows Firewall allows port 5001
5. URL format is correct (http:// included)

**Solution:**
```powershell
# Stop server
Ctrl+C

# Start with correct binding
dotnet run --urls "http://0.0.0.0:5001"

# Check it's listening on all interfaces
netstat -an | findstr :5001
# Should show: 0.0.0.0:5001
```

---

### **Problem: "Login fails after changing server"**

**Check:**
1. New server has database with crew members
2. Crew ID exists in new server's database
3. Password is correct (default: password123)

**Solution:**
```powershell
# Check crew members in database
docker exec maritime-edge-postgres psql -U edge_user -d maritime_edge -c "SELECT crew_id, full_name FROM crew_members;"
```

---

### **Problem: "URL not persisting"**

**Check:**
1. Hive box initialized in main.dart
2. No errors in console during save
3. ServerConfigStorage called correctly

**Solution:**
```dart
// Test storage directly
await ServerConfigStorage.saveServerUrl('http://test:5001');
final saved = await ServerConfigStorage.getServerUrl();
print('Saved URL: $saved'); // Should print: http://test:5001
```

---

## 🎨 UI Components

### **Login Screen:**
```dart
[Logo: Sailing Ship Icon]
Maritime Crew App
Login to manage your tasks

[Card with Form]
├── Crew ID input
├── Password input
└── Login button

[Server URL Display]
🔵 http://192.168.1.100:5001

[Server Settings Button]
```

### **Server Config Dialog:**
```dart
⚙️ Server Configuration

[URL Input Field]
http://192.168.1.100:5001
[?] [📋]

[Test Result Box]
✓ Connection successful!

[Info Card]
💡 How to connect to Edge Server:
1. Find your PC IP (ipconfig)
2. Start server: dotnet run --urls "http://0.0.0.0:5001"
3. Enter URL: http://YOUR_IP:5001
4. Test connection before saving

[Cancel] [Test] [Save]
```

---

## 📊 Code Quality

### **Features:**
- ✅ Type-safe with Dart strong typing
- ✅ Proper error handling with try-catch
- ✅ Loading states for async operations
- ✅ Input validation with forms
- ✅ User feedback with SnackBars
- ✅ Clean code with separation of concerns
- ✅ Singleton pattern for ApiClient
- ✅ Async/await for all I/O operations

### **Testing Checklist:**
- [ ] Test with valid URLs
- [ ] Test with invalid URLs
- [ ] Test connection success
- [ ] Test connection failure
- [ ] Test URL persistence
- [ ] Test login after URL change
- [ ] Test example URLs
- [ ] Test paste from clipboard
- [ ] Test on different WiFi networks
- [ ] Test with server stopped/started

---

## 🔄 Future Enhancements

### **Potential Improvements:**

1. **QR Code Scanning:**
   - Generate QR code on Edge Server startup
   - Scan QR code to auto-configure URL

2. **Network Discovery:**
   - Auto-detect Edge Server on LAN
   - List available servers

3. **Multiple Servers:**
   - Save multiple server profiles
   - Quick switch between servers

4. **Health Check:**
   - Periodic connection monitoring
   - Auto-reconnect on failure

5. **Advanced Settings:**
   - Custom timeout values
   - Retry policies
   - SSL certificate validation

---

## 📝 Summary

**What was implemented:**
- ✅ Complete server configuration UI
- ✅ URL validation and testing
- ✅ Persistent storage with Hive
- ✅ Dynamic ApiClient updates
- ✅ User-friendly examples and help
- ✅ Integration with login flow
- ✅ Comprehensive error handling

**Result:**
Users can now easily configure and connect to any Edge Server on their local network without modifying code or recompiling the app. The feature includes validation, testing, persistence, and a polished UI for the best user experience.

---

**Created:** 2025-10-23  
**Status:** ✅ Complete and Ready for Testing
