// This is a basic Flutter widget test for Maritime Crew App.

import 'package:flutter_test/flutter_test.dart';

import 'package:maritime_crew_app/app.dart';

void main() {
  testWidgets('App smoke test', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    await tester.pumpWidget(const MyApp());

    // Verify that the app loads without crashing
    await tester.pumpAndSettle();

    // Basic verification that app initializes
    expect(find.byType(MyApp), findsOneWidget);
  });
}
