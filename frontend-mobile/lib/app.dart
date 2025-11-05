import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'l10n/app_localizations.dart';
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
import 'core/localization/locale_provider.dart';

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    // LocaleProvider đã được provide từ main.dart
    return Consumer<LocaleProvider>(
      builder: (context, localeProvider, child) {
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
            title: 'Maritime Crew App',
            debugShowCheckedModeBanner: false,
            
            // Localization delegates
            localizationsDelegates: const [
              AppLocalizations.delegate,
              GlobalMaterialLocalizations.delegate,
              GlobalWidgetsLocalizations.delegate,
              GlobalCupertinoLocalizations.delegate,
            ],
            
            // Supported locales
            supportedLocales: const [
              Locale('en'), // English
              Locale('vi'), // Vietnamese
              Locale('fil'), // Filipino
              Locale('hi'), // Hindi
              Locale('zh'), // Chinese
              Locale('ja'), // Japanese
              Locale('ko'), // Korean
            ],
            
            // Current locale from provider
            locale: localeProvider.locale,
            
            // Fallback locale resolution
            localeResolutionCallback: (locale, supportedLocales) {
              // Check if current locale is supported
              if (locale != null) {
                for (var supportedLocale in supportedLocales) {
                  if (supportedLocale.languageCode == locale.languageCode) {
                    return supportedLocale;
                  }
                }
              }
              // Default to English if not supported
              return const Locale('en');
            },
            
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
      },
    );
  }
}
