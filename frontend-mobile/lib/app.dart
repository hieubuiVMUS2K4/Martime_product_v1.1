import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:google_fonts/google_fonts.dart';
import 'presentation/screens/auth/login_screen.dart';
import 'presentation/screens/home/home_screen.dart';
import 'presentation/screens/alarms/alarm_list_screen.dart';
import 'presentation/screens/alarms/alarm_statistics_screen.dart';
import 'presentation/screens/alarms/alarm_history_screen.dart';
import 'presentation/providers/auth_provider.dart';
import 'presentation/providers/task_provider.dart';
import 'presentation/providers/sync_provider.dart';
import 'presentation/providers/alarm_provider.dart';
import 'providers/watchkeeping_provider.dart';
import 'data/repositories/alarm_repository.dart';
import 'data/data_sources/remote/alarm_api.dart';
import 'core/network/api_client.dart';
import 'core/constants/app_constants.dart';

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => TaskProvider()),
        ChangeNotifierProvider(create: (_) => SyncProvider()),
        ChangeNotifierProvider(
          create: (_) => AlarmProvider(
            AlarmRepository(
              AlarmApi(ApiClient().dio),
            ),
          ),
        ),
        ChangeNotifierProvider(create: (_) => WatchkeepingProvider()),
      ],
      child: MaterialApp(
        title: AppConstants.appName,
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(
            seedColor: Colors.blue,
            brightness: Brightness.light,
          ),
          textTheme: GoogleFonts.interTextTheme(),
          useMaterial3: true,
          appBarTheme: const AppBarTheme(
            centerTitle: true,
            elevation: 0,
          ),
          cardTheme: const CardThemeData(
            elevation: 2,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.all(Radius.circular(12)),
            ),
          ),
          inputDecorationTheme: InputDecorationTheme(
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(8),
            ),
            filled: true,
          ),
        ),
        home: const LoginScreen(),
        routes: {
          '/login': (context) => const LoginScreen(),
          '/home': (context) => const HomeScreen(),
          '/alarms': (context) => const AlarmListScreen(),
          '/alarms/statistics': (context) => const AlarmStatisticsScreen(),
          '/alarms/history': (context) => const AlarmHistoryScreen(),
        },
      ),
    );
  }
}
