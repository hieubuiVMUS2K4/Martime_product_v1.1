class ApiConstants {
  // Base URL - Có thể thay đổi trong Settings
  static String baseUrl = 'http://192.168.1.100:5001'; // LAN IP của Edge Server
  
  // Auth Endpoints
  static const String login = '/api/auth/login';
  static const String logout = '/api/auth/logout';
  static const String refreshToken = '/api/auth/refresh';
  
  // Crew Endpoints
  static const String crewProfile = '/api/crew/me';
  static const String crewCertificates = '/api/crew/me/certificates';
  
  // Maintenance Task Endpoints
  static const String myTasks = '/api/maintenance/tasks/my-tasks';
  static const String taskDetail = '/api/maintenance/tasks'; // /{id}
  static const String completeTask = '/api/maintenance/tasks'; // /{id}/complete
  static const String startTask = '/api/maintenance/tasks'; // /{id}/start
  
  // Schedule Endpoints
  static const String mySchedule = '/api/schedule/my-schedule';
  static const String upcomingTasks = '/api/schedule/upcoming';
  
  // Sync Endpoints
  static const String syncStatus = '/api/sync/status';
  static const String syncData = '/api/sync/upload';
  
  // Timeouts
  static const int connectionTimeout = 30000; // 30 seconds
  static const int receiveTimeout = 30000;
  
  // Cache Duration
  static const Duration cacheDuration = Duration(hours: 1);
}
