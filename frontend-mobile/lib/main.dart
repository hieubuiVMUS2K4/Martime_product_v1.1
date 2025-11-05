import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:provider/provider.dart';
import 'app.dart';
import 'core/network/api_client.dart';
import 'core/localization/locale_provider.dart';
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
  
  // Initialize LocaleProvider
  final localeProvider = LocaleProvider();
  await localeProvider.initialize();
  
  runApp(
    ChangeNotifierProvider.value(
      value: localeProvider,
      child: const MyApp(),
    ),
  );
}
