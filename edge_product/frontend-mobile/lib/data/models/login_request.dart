class LoginRequest {
  final String username;  // Changed from crewId to username
  final String password;

  LoginRequest({
    required this.username,
    required this.password,
  });

  Map<String, dynamic> toJson() {
    return {
      'username': username,  // Changed from crewId to username
      'password': password,
    };
  }
}
