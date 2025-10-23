import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'app.dart';
import 'core/network/api_client.dart';
import 'data/models/sync_item.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Initialize Hive
  await Hive.initFlutter();
  
  // Register Hive adapters
  Hive.registerAdapter(SyncItemAdapter());
  Hive.registerAdapter(SyncItemTypeAdapter());
  
  // Open Hive boxes
  await Hive.openBox('cache_box');
  await Hive.openBox<SyncItem>('sync_queue');
  
  // Initialize ApiClient with saved server URL
  await ApiClient().initialize();
  
  runApp(const MyApp());
}
