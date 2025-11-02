class LoginResponse {
  final String accessToken;
  final String refreshToken;
  final int userId;
  final String crewId;
  final String fullName;
  final String position;
  final String? role;
  final int? roleId;
  final String? roleCode;
  final int expiresIn;

  LoginResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.userId,
    required this.crewId,
    required this.fullName,
    required this.position,
    this.role,
    this.roleId,
    this.roleCode,
    this.expiresIn = 86400,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    // Handle nested user object from new API response
    final user = json['user'] as Map<String, dynamic>?;
    
    if (user != null) {
      // New API format: {success, message, accessToken, refreshToken, expiresIn, user: {...}}
      return LoginResponse(
        accessToken: json['accessToken'] as String,
        refreshToken: json['refreshToken'] as String,
        userId: user['id'] as int,
        crewId: (user['crewId'] as String?) ?? '',
        fullName: (user['fullName'] as String?) ?? '',
        position: (user['position'] as String?) ?? '',
        role: user['roleName'] as String?,
        roleId: user['roleId'] as int?,
        roleCode: user['roleCode'] as String?,
        expiresIn: json['expiresIn'] as int? ?? 86400,
      );
    } else {
      // Legacy API format (backward compatibility)
      return LoginResponse(
        accessToken: json['accessToken'] as String,
        refreshToken: json['refreshToken'] as String,
        userId: json['userId'] as int,
        crewId: (json['crewId'] as String?) ?? '',
        fullName: (json['fullName'] as String?) ?? '',
        position: (json['position'] as String?) ?? '',
        role: json['role'] as String?,
        roleId: json['roleId'] as int?,
        roleCode: json['roleCode'] as String?,
        expiresIn: json['expiresIn'] as int? ?? 86400,
      );
    }
  }

  Map<String, dynamic> toJson() {
    return {
      'accessToken': accessToken,
      'refreshToken': refreshToken,
      'userId': userId,
      'crewId': crewId,
      'fullName': fullName,
      'position': position,
      'role': role,
      'roleId': roleId,
      'roleCode': roleCode,
      'expiresIn': expiresIn,
    };
  }
}
