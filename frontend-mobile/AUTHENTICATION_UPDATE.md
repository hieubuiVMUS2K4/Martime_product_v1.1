# 🔐 CẬP NHẬT HỆ THỐNG ĐĂNG NHẬP MOBILE APP

## 📋 Tổng quan

Mobile app đã được cập nhật để tương thích với hệ thống authentication mới của backend, sử dụng bảng `users` và `roles`.

## 🔄 Thay đổi chính

### 1. **Backend Changes**
- ✅ Thêm bảng `roles` (quản lý phân quyền)
- ✅ Thêm bảng `users` (tài khoản người dùng)
- ✅ Mỗi user liên kết với `crew_member` thông qua `crew_id`
- ✅ Password mặc định được hash từ ngày sinh (format: `ddMMyyyy`)

### 2. **Mobile App Changes**
- ✅ Cập nhật `LoginRequest`: `crewId` → `username`
- ✅ Cập nhật `LoginResponse`: thêm `roleId`, `roleCode`, `expiresIn`
- ✅ Cập nhật `AuthApi`: sử dụng endpoint `/api/auth/login` mới
- ✅ Cập nhật UI: thêm helper text hướng dẫn password mặc định

## 🚀 Hướng dẫn sử dụng

### **Đăng nhập lần đầu**

1. **Username**: Nhập mã thuyền viên (Crew ID)
   - Ví dụ: `CM001`, `CM002`, `CM003`

2. **Password**: Nhập password mặc định (ngày sinh)
   - Format: `ddMMyyyy` (ngày tháng năm sinh)
   - Ví dụ: Sinh ngày 15/05/1990 → password: `15051990`
   - Nếu không có ngày sinh trong database → password: `123456`

3. **Admin Account**:
   - Username: `admin`
   - Password: `admin123`

### **Ví dụ đăng nhập**

```
Crew Member: Nguyễn Văn A (CM001)
Ngày sinh: 15/05/1990

Username: CM001
Password: 15051990
```

```
Admin:
Username: admin
Password: admin123
```

## 🔧 Cấu trúc API

### **Login Endpoint**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "CM001",
  "password": "15051990"
}
```

### **Response**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "accessToken": "access_1_CM001_1234567890_abc123",
  "refreshToken": "refresh_1_CM001_1234567890_xyz789",
  "expiresIn": 86400,
  "user": {
    "id": 1,
    "username": "CM001",
    "roleId": 2,
    "roleName": "Người dùng",
    "roleCode": "USER",
    "crewId": "CM001",
    "fullName": "Nguyễn Văn A",
    "position": "Deck Officer",
    "isActive": true,
    "lastLoginAt": "2025-11-02T10:30:00Z"
  }
}
```

## 📊 Roles (Phân quyền)

| Role Code | Role Name        | Description                    |
|-----------|------------------|--------------------------------|
| ADMIN     | Quản trị viên    | Toàn quyền quản lý hệ thống    |
| USER      | Người dùng       | Quyền xem và cập nhật cơ bản   |

## 🔑 Password Management

### **Đổi mật khẩu**
Sử dụng endpoint: `POST /api/auth/change-password`

```json
{
  "userId": 1,
  "oldPassword": "15051990",
  "newPassword": "newpassword123",
  "confirmPassword": "newpassword123"
}
```

### **Reset mật khẩu về mặc định**
Sử dụng endpoint: `POST /api/auth/reset-password`

```json
{
  "username": "CM001"
}
```

Response:
```json
{
  "success": true,
  "message": "Reset mật khẩu thành công",
  "defaultPassword": "15051990"
}
```

## 🛠️ Development Notes

### **Files Modified**

1. **`lib/data/models/login_request.dart`**
   - Changed: `crewId` → `username`

2. **`lib/data/models/login_response.dart`**
   - Added: `roleId`, `roleCode`, `expiresIn`
   - Updated: `fromJson()` to handle new API response format

3. **`lib/data/data_sources/remote/auth_api.dart`**
   - Using new endpoint: `/api/auth/login`
   - Added: `/api/auth/login-legacy` for backward compatibility

4. **`lib/data/repositories/auth_repository.dart`**
   - Updated to use `username` instead of `crewId` in login request

5. **`lib/presentation/screens/auth/login_screen.dart`**
   - Added helper text for better UX
   - Updated labels to clarify username = Crew ID

### **Backward Compatibility**

Backend hỗ trợ endpoint `/api/auth/login-legacy` để tương thích ngược với các phiên bản cũ của mobile app (nếu cần).

## ✅ Testing Checklist

- [ ] Đăng nhập với admin account (`admin` / `admin123`)
- [ ] Đăng nhập với crew member (Crew ID + ngày sinh)
- [ ] Đăng nhập với crew member không có ngày sinh (password: `123456`)
- [ ] Đăng nhập thất bại (sai password)
- [ ] Đăng nhập thất bại (username không tồn tại)
- [ ] Token được lưu vào secure storage
- [ ] Refresh token hoạt động
- [ ] Logout xóa token đúng cách

## 🐛 Troubleshooting

### **Lỗi: "Tên đăng nhập không tồn tại"**
→ Kiểm tra xem user đã được tạo trong database chưa (chạy script `insert-roles-and-users.sql`)

### **Lỗi: "Mật khẩu không đúng"**
→ Kiểm tra format ngày sinh (phải là `ddMMyyyy`)

### **Lỗi: "Server error"**
→ Kiểm tra backend service đang chạy và database connection OK

### **Lỗi: "No internet connection"**
→ Kiểm tra server URL trong Settings và network connectivity

## 📝 Next Steps

1. **Implement Change Password UI** (trong Settings screen)
2. **Add Role-based Access Control** (hiển thị features theo role)
3. **Add Password Validation** (min length, complexity)
4. **Add "Forgot Password" flow** (reset về password mặc định)
5. **Add Biometric Authentication** (fingerprint/face ID)

## 📞 Contact

Nếu có vấn đề hoặc câu hỏi, vui lòng liên hệ team development.

---

**Last Updated**: November 2, 2025
**Version**: 1.0.0
