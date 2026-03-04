class CacheKeys {
  static const String accessToken = 'access_token';
  static const String refreshToken = 'refresh_token';
  static const String userId = 'user_id';
  static const String crewId = 'crew_id';
  static const String userProfile = 'user_profile';
  static const String myTasks = 'my_tasks';
  static const String serverUrl = 'server_url';
  
  // Offline cache for checklist and materials
  static const String availableMaterials = 'available_materials';
  static const String checklistPrefix = 'task_checklist_';
  static const String offlineChecklistProgress = 'offline_checklist_progress';
  
  // Draft spare parts used (saved when user exits without submitting)
  static const String draftSparePartsPrefix = 'draft_spare_parts_';
  
  // Draft task completion form (notes, photos, etc.)
  static const String draftTaskFormPrefix = 'draft_task_form_';
  
  // Draft checklist progress (saved when user exits without submitting)
  static const String draftChecklistPrefix = 'draft_checklist_';
  
  // Hive Box Names
  static const String cacheBox = 'cache_box';
  static const String syncQueueBox = 'sync_queue';
}
