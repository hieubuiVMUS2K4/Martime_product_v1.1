import 'package:hive/hive.dart';
import 'package:uuid/uuid.dart';

part 'sync_item.g.dart';

@HiveType(typeId: 0)
class SyncItem extends HiveObject {
  @HiveField(0)
  final String id;
  
  @HiveField(1)
  final SyncItemType type;
  
  @HiveField(2)
  final Map<String, dynamic> data;
  
  @HiveField(3)
  final DateTime createdAt;
  
  @HiveField(4)
  int retryCount;
  
  SyncItem({
    String? id,
    required this.type,
    required this.data,
    DateTime? createdAt,
    this.retryCount = 0,
  })  : id = id ?? const Uuid().v4(),
        createdAt = createdAt ?? DateTime.now();
}

@HiveType(typeId: 1)
enum SyncItemType {
  @HiveField(0)
  taskComplete,
  
  @HiveField(1)
  taskStart,
  
  @HiveField(2)
  profileUpdate,
}
