import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../data/models/safety_alarm.dart';
import '../../providers/alarm_provider.dart';
import 'alarm_detail_screen.dart';

class AlarmListScreen extends StatefulWidget {
  const AlarmListScreen({super.key});

  @override
  State<AlarmListScreen> createState() => _AlarmListScreenState();
}

class _AlarmListScreenState extends State<AlarmListScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AlarmProvider>().fetchActiveAlarms();
    });
  }

  Future<void> _refresh() async {
    await context.read<AlarmProvider>().fetchActiveAlarms();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Safety Alarms'),
        actions: [
          IconButton(
            icon: const Icon(Icons.analytics_outlined),
            onPressed: () {
              Navigator.pushNamed(context, '/alarms/statistics');
            },
            tooltip: 'Statistics',
          ),
          IconButton(
            icon: const Icon(Icons.history),
            onPressed: () {
              Navigator.pushNamed(context, '/alarms/history');
            },
            tooltip: 'History',
          ),
        ],
      ),
      body: Consumer<AlarmProvider>(
        builder: (context, provider, child) {
          if (provider.isLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (provider.error != null) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: Colors.red),
                  const SizedBox(height: 16),
                  Text(
                    'Error: ${provider.error}',
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: Colors.red),
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: _refresh,
                    child: const Text('Retry'),
                  ),
                ],
              ),
            );
          }

          final alarms = provider.activeAlarms;

          if (alarms.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.check_circle_outline,
                      size: 64, color: Colors.green),
                  const SizedBox(height: 16),
                  const Text(
                    'No Active Alarms',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  const Text('All systems normal'),
                  const SizedBox(height: 24),
                  ElevatedButton.icon(
                    onPressed: () async {
                      await provider.generateSampleAlarms();
                      if (mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                              content: Text('Sample alarms generated')),
                        );
                      }
                    },
                    icon: const Icon(Icons.add_alert),
                    label: const Text('Generate Sample Alarms'),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            child: Column(
              children: [
                // Summary cards
                Container(
                  padding: const EdgeInsets.all(16),
                  color: Colors.grey[100],
                  child: Row(
                    children: [
                      Expanded(
                        child: _buildSummaryCard(
                          'Critical',
                          provider.criticalCount,
                          Colors.red,
                          Icons.warning_amber,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildSummaryCard(
                          'Warning',
                          provider.warningCount,
                          Colors.orange,
                          Icons.error_outline,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _buildSummaryCard(
                          'Unack.',
                          provider.unacknowledgedCount,
                          Colors.blue,
                          Icons.notification_important,
                        ),
                      ),
                    ],
                  ),
                ),
                // Alarm list
                Expanded(
                  child: ListView.builder(
                    itemCount: alarms.length,
                    padding: const EdgeInsets.all(16),
                    itemBuilder: (context, index) {
                      final alarm = alarms[index];
                      return _buildAlarmCard(alarm);
                    },
                  ),
                ),
              ],
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _refresh,
        child: const Icon(Icons.refresh),
      ),
    );
  }

  Widget _buildSummaryCard(
      String label, int count, Color color, IconData icon) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 4),
          Text(
            '$count',
            style: TextStyle(
              fontSize: 24,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
          Text(
            label,
            style: TextStyle(fontSize: 12, color: Colors.grey[600]),
            textAlign: TextAlign.center,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
    );
  }

  Widget _buildAlarmCard(SafetyAlarm alarm) {
    Color severityColor;
    if (alarm.isCritical) {
      severityColor = Colors.red;
    } else if (alarm.isWarning) {
      severityColor = Colors.orange;
    } else {
      severityColor = Colors.blue;
    }

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: alarm.isAcknowledged ? 1 : 3,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: alarm.isAcknowledged
              ? severityColor.withOpacity(0.3)
              : severityColor,
          width: alarm.isAcknowledged ? 1 : 2,
        ),
      ),
      child: InkWell(
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => AlarmDetailScreen(alarm: alarm),
            ),
          );
        },
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 6,
                    ),
                    decoration: BoxDecoration(
                      color: severityColor.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: severityColor),
                    ),
                    child: Text(
                      alarm.severityDisplay,
                      style: TextStyle(
                        color: severityColor,
                        fontWeight: FontWeight.bold,
                        fontSize: 12,
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  if (!alarm.isAcknowledged)
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.blue.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.notification_important,
                              size: 14, color: Colors.blue),
                          SizedBox(width: 4),
                          Text(
                            'NEW',
                            style: TextStyle(
                              color: Colors.blue,
                              fontWeight: FontWeight.bold,
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                    ),
                  const Spacer(),
                  Text(
                    alarm.timeAgo,
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Text(
                alarm.typeDisplay,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              if (alarm.description != null) ...[
                const SizedBox(height: 4),
                Text(
                  alarm.description!,
                  style: TextStyle(
                    color: Colors.grey[700],
                    fontSize: 14,
                  ),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
              const SizedBox(height: 8),
              Row(
                children: [
                  Icon(Icons.location_on, size: 16, color: Colors.grey[600]),
                  const SizedBox(width: 4),
                  Text(
                    alarm.locationDisplay,
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 13,
                    ),
                  ),
                  if (alarm.alarmCode != null) ...[
                    const SizedBox(width: 12),
                    Icon(Icons.tag, size: 16, color: Colors.grey[600]),
                    const SizedBox(width: 4),
                    Text(
                      alarm.alarmCode!,
                      style: TextStyle(
                        color: Colors.grey[600],
                        fontSize: 13,
                        fontFamily: 'monospace',
                      ),
                    ),
                  ],
                ],
              ),
              if (alarm.isAcknowledged) ...[
                const Divider(height: 24),
                Row(
                  children: [
                    Icon(Icons.check_circle, size: 16, color: Colors.green),
                    const SizedBox(width: 6),
                    Text(
                      'Acknowledged by ${alarm.acknowledgedBy}',
                      style: const TextStyle(
                        color: Colors.green,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
