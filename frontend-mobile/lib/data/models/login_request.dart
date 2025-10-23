class LoginRequest {
  final String crewId;
  final String password;

  LoginRequest({
    required this.crewId,
    required this.password,
  });

  Map<String, dynamic> toJson() {
    return {
      'crewId': crewId,
      'password': password,
    };
  }
}
