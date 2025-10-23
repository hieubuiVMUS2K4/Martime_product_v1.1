class LoginResponse {
  final String accessToken;
  final String refreshToken;
  final int userId;
  final String crewId;
  final String fullName;
  final String position;
  final String? role;

  LoginResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.userId,
    required this.crewId,
    required this.fullName,
    required this.position,
    this.role,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      accessToken: json['accessToken'],
      refreshToken: json['refreshToken'],
      userId: json['userId'],
      crewId: json['crewId'],
      fullName: json['fullName'],
      position: json['position'],
      role: json['role'],
    );
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
    };
  }
}
