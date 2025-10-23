# 📋 TODO LIST - FLUTTER MOBILE APP DEVELOPMENT

## ⚡ PRIORITY 1: Critical Features (Week 1-2)

### Backend API Integration
- [ ] **auth_api.dart**
  - [ ] POST /api/auth/login
  - [ ] POST /api/auth/refresh
  - [ ] POST /api/auth/logout
  - [ ] Test với Edge Server

- [ ] **task_api.dart**
  - [ ] GET /api/maintenance/tasks/my-tasks
  - [ ] GET /api/maintenance/tasks/{id}
  - [ ] POST /api/maintenance/tasks/{id}/start
  - [ ] POST /api/maintenance/tasks/{id}/complete
  - [ ] Test với Edge Server

- [ ] **crew_api.dart**
  - [ ] GET /api/crew/me
  - [ ] GET /api/crew/me/certificates
  - [ ] Test với Edge Server

### Update Providers với Real API
- [ ] Update `auth_provider.dart`
  - [ ] Replace mock login với API call
  - [ ] Implement token refresh
  - [ ] Handle API errors

- [ ] Update `task_provider.dart`
  - [ ] Fetch tasks từ API
  - [ ] Implement task start
  - [ ] Implement task complete
  - [ ] Update local cache

---

## 🎨 PRIORITY 2: UI Screens (Week 2-3)

### Task Screens
- [ ] **task_list_screen.dart**
  - [ ] Display tasks trong cards
  - [ ] Filter: All, Pending, In Progress, Overdue
  - [ ] Pull to refresh
  - [ ] Search by equipment name
  - [ ] Sort by priority/due date
  - [ ] Navigate to detail

- [ ] **task_detail_screen.dart**
  - [ ] Show full task details
  - [ ] Equipment information
  - [ ] Last maintenance info
  - [ ] Running hours
  - [ ] Spare parts history
  - [ ] Action buttons (Start/Complete)
  - [ ] Handle offline mode

- [ ] **complete_task_screen.dart**
  - [ ] Form validation
  - [ ] Running hours input
  - [ ] Spare parts used
  - [ ] Notes textarea
  - [ ] Photo upload (optional)
  - [ ] Offline support với sync queue
  - [ ] Show success message

### Profile Screens
- [ ] **profile_screen.dart**
  - [ ] Display crew info (readonly)
  - [ ] Personal details
  - [ ] Employment info
  - [ ] Certificate status overview
  - [ ] Navigate to certificates

- [ ] **certificates_screen.dart**
  - [ ] STCW certificate card
  - [ ] Medical certificate card
  - [ ] Passport card
  - [ ] Visa card
  - [ ] Color-coded status (valid/expiring/expired)
  - [ ] Days until expiry
  - [ ] Alert badges

### Schedule Screen
- [ ] **schedule_screen.dart**
  - [ ] Calendar widget
  - [ ] Mark dates with tasks
  - [ ] List tasks by selected date
  - [ ] Filter by week/month
  - [ ] Due date indicators
  - [ ] Navigate to task detail

### Settings Screen
- [ ] **settings_screen.dart**
  - [ ] Display app version
  - [ ] Cache statistics
  - [ ] Clear cache button
  - [ ] Navigate to server config

- [ ] **server_config_screen.dart**
  - [ ] Input server IP/URL
  - [ ] Test connection button
  - [ ] Save configuration
  - [ ] Show current server

---

## 🧩 PRIORITY 3: Reusable Widgets (Week 3)

### Common Widgets
- [ ] **loading_widget.dart**
  - [ ] Circular progress indicator
  - [ ] Loading message
  - [ ] Overlay variant

- [ ] **error_widget.dart**
  - [ ] Error icon
  - [ ] Error message
  - [ ] Retry button
  - [ ] Custom styling

- [ ] **empty_state_widget.dart**
  - [ ] Empty icon
  - [ ] Empty message
  - [ ] Call-to-action button

### Task Widgets
- [ ] **task_card.dart**
  - [ ] Equipment name
  - [ ] Task description preview
  - [ ] Priority badge
  - [ ] Status badge
  - [ ] Due date with countdown
  - [ ] Overdue indicator
  - [ ] Tap to detail

- [ ] **priority_badge.dart**
  - [ ] Color-coded by priority
  - [ ] CRITICAL - Red
  - [ ] HIGH - Orange
  - [ ] NORMAL - Blue
  - [ ] LOW - Gray

- [ ] **status_badge.dart**
  - [ ] Color-coded by status
  - [ ] PENDING - Blue
  - [ ] IN_PROGRESS - Orange
  - [ ] COMPLETED - Green
  - [ ] OVERDUE - Red

### Certificate Widgets
- [ ] **certificate_status_card.dart**
  - [ ] Certificate name
  - [ ] Issue date
  - [ ] Expiry date
  - [ ] Days until expiry
  - [ ] Status indicator
  - [ ] Alert icon

---

## 🔄 PRIORITY 4: Offline & Sync (Week 4)

### Sync Queue Implementation
- [ ] Update `sync_queue.dart`
  - [ ] Implement _syncItemToServer
  - [ ] Handle task complete sync
  - [ ] Handle task start sync
  - [ ] Retry logic
  - [ ] Error handling

### Background Sync
- [ ] Setup background service
- [ ] Auto-sync when online
- [ ] Periodic sync check
- [ ] Battery optimization
- [ ] Handle app in background

### Offline Indicators
- [ ] Network status banner
- [ ] Sync queue counter
- [ ] Offline mode badge
- [ ] Pending sync notification

---

## 🎯 PRIORITY 5: Polish & Testing (Week 5)

### UI/UX Improvements
- [ ] Add animations
  - [ ] Screen transitions
  - [ ] Card animations
  - [ ] Button feedback
  - [ ] Loading animations

- [ ] Improve error handling
  - [ ] User-friendly error messages
  - [ ] Retry mechanisms
  - [ ] Fallback states

- [ ] Add user feedback
  - [ ] Success snackbars
  - [ ] Error dialogs
  - [ ] Confirmation dialogs
  - [ ] Progress indicators

### Testing
- [ ] Unit tests
  - [ ] Models
  - [ ] Repositories
  - [ ] Providers

- [ ] Widget tests
  - [ ] Login screen
  - [ ] Task list screen
  - [ ] Task card widget

- [ ] Integration tests
  - [ ] Login flow
  - [ ] Task CRUD flow
  - [ ] Offline sync flow

- [ ] Manual testing
  - [ ] Test on Android device
  - [ ] Test offline mode
  - [ ] Test sync queue
  - [ ] Test error scenarios

---

## 🚀 PRIORITY 6: Production Ready (Week 6)

### Build & Release
- [ ] Configure app icons
- [ ] Configure splash screen
- [ ] Setup signing keys
- [ ] Build release APK
- [ ] Test on multiple devices
- [ ] Performance optimization

### Documentation
- [ ] User manual
- [ ] API documentation
- [ ] Deployment guide
- [ ] Troubleshooting guide

### Deployment
- [ ] Setup CI/CD (optional)
- [ ] Deploy to internal testing
- [ ] Collect feedback
- [ ] Fix bugs
- [ ] Release to production

---

## 🎁 NICE TO HAVE (Future Enhancements)

### Advanced Features
- [ ] Push notifications
  - [ ] Task due reminders
  - [ ] Certificate expiry alerts
  - [ ] Sync notifications

- [ ] Photo management
  - [ ] Take photos
  - [ ] Compress images
  - [ ] Upload to server
  - [ ] View task photos

- [ ] QR Code scanning
  - [ ] Scan equipment QR
  - [ ] Auto-fill equipment info

- [ ] Reports
  - [ ] Completed tasks report
  - [ ] Personal statistics
  - [ ] Export to PDF

- [ ] Multi-language
  - [ ] English
  - [ ] Vietnamese
  - [ ] Other languages

### Performance
- [ ] Image caching
- [ ] Lazy loading
- [ ] Pagination
- [ ] Database indexing

### Security
- [ ] Biometric authentication
- [ ] PIN lock
- [ ] Certificate pinning
- [ ] Data encryption

---

## ✅ COMPLETED

- [x] Project setup
- [x] Folder structure
- [x] Dependencies configuration
- [x] Core files (Constants, Network, Auth, Cache)
- [x] Data models
- [x] Providers (Auth, Task, Sync)
- [x] Login screen
- [x] Home screen với dashboard
- [x] Documentation (README, QUICKSTART, PROJECT_SUMMARY)

---

## 📊 PROGRESS TRACKER

```
Phase 1: Foundation      ████████████████████ 100% ✅
Phase 2: API Integration ░░░░░░░░░░░░░░░░░░░░   0% 🚧
Phase 3: UI Screens      ░░░░░░░░░░░░░░░░░░░░   0% 🚧
Phase 4: Widgets         ░░░░░░░░░░░░░░░░░░░░   0% 🚧
Phase 5: Offline/Sync    ░░░░░░░░░░░░░░░░░░░░   0% 🚧
Phase 6: Polish/Test     ░░░░░░░░░░░░░░░░░░░░   0% 🚧
Phase 7: Production      ░░░░░░░░░░░░░░░░░░░░   0% 🚧

Overall Progress: ████░░░░░░░░░░░░░░░░ 20%
```

---

**🚢 Let's build an amazing app for our seafarers! ⚓**
