import 'package:get_it/get_it.dart';
import '../network/api_client.dart';
import '../network/network_info.dart';
import '../cache/cache_manager.dart';
import '../cache/sync_queue.dart';
import '../auth/token_storage.dart';
import '../../data/repositories/task_repository.dart';
import '../../data/repositories/auth_repository.dart';
import '../../data/repositories/alarm_repository.dart';
import '../../data/data_sources/remote/alarm_api.dart';
import '../../data/data_sources/remote/task_api.dart';

/// Service Locator using GetIt
/// 
/// This provides dependency injection without tight coupling.
/// All services are registered as singletons to avoid creating
/// duplicate instances (which was causing performance issues).
/// 
/// Usage:
///   final taskRepo = ServiceLocator.instance<TaskRepository>();
final GetIt sl = GetIt.instance;

/// Alias for easier access
GetIt get ServiceLocator => sl;

/// Initialize all dependencies
/// Call this in main() before runApp()
Future<void> setupServiceLocator() async {
  // ============================================
  // CORE SERVICES (Singletons)
  // ============================================
  
  // Network
  sl.registerLazySingleton<NetworkInfo>(() => NetworkInfo());
  sl.registerLazySingleton<ApiClient>(() => ApiClient());
  
  // Storage
  sl.registerLazySingleton<TokenStorage>(() => TokenStorage());
  sl.registerLazySingleton<CacheManager>(() => CacheManager());
  
  // Sync Queue (depends on NetworkInfo)
  sl.registerLazySingleton<SyncQueue>(
    () => SyncQueue(sl<NetworkInfo>()),
  );

  // ============================================
  // API CLIENTS (Singletons)
  // ============================================
  
  sl.registerLazySingleton<TaskApi>(
    () => TaskApi(sl<ApiClient>().dio),
  );

  // ============================================
  // REPOSITORIES (Singletons)
  // ============================================
  
  sl.registerLazySingleton<TaskRepository>(
    () => TaskRepository(
      apiClient: sl<ApiClient>(),
      networkInfo: sl<NetworkInfo>(),
      cacheManager: sl<CacheManager>(),
      syncQueue: sl<SyncQueue>(),
      tokenStorage: sl<TokenStorage>(),
    ),
  );
  
  sl.registerLazySingleton<AuthRepository>(
    () => AuthRepository(
      apiClient: sl<ApiClient>(),
      tokenStorage: sl<TokenStorage>(),
      networkInfo: sl<NetworkInfo>(),
    ),
  );
  
  sl.registerLazySingleton<AlarmRepository>(
    () => AlarmRepository(
      AlarmApi(sl<ApiClient>().dio),
    ),
  );

  // ============================================
  // INITIALIZATION
  // ============================================
  
  // Initialize ApiClient with saved server URL
  await sl<ApiClient>().initialize();
}

/// Reset service locator (useful for testing)
Future<void> resetServiceLocator() async {
  await sl.reset();
}
