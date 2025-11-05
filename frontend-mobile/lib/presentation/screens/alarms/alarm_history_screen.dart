import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../data/models/safety_alarm.dart';
import '../../providers/alarm_provider.dart';
import 'alarm_detail_screen.dart';

class AlarmHistoryScreen extends StatefulWidget {
  const AlarmHistoryScreen({super.key});

  @override
  State<AlarmHistoryScreen> createState() => _AlarmHistoryScreenState();
}

class _AlarmHistoryScreenState extends State<AlarmHistoryScreen> {
  int _selectedDays = 7;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AlarmProvider>().fetchAlarmHistory(days: _selectedDays);
    });
  }

  Future<void> _refresh() async {
    await context.read<AlarmProvider>().fetchAlarmHistory(days: _selectedDays);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Alarm History'),
      ),
      body: Consumer<AlarmProvider>(
        builder: (context, provider, child) {
          if (provider.isLoadingHistory) {
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
                    'Lỗi: ${provider.error}',
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

          final alarms = provider.alarmHistory;

          return RefreshIndicator(
            onRefresh: _refresh,
            child: Column(
              children: [
                // Time range selector
                Container(
                  padding: const EdgeInsets.all(16),
                  color: Colors.grey[100],
                  child: Row(
                    children: [
                      const Text(
                        'Show:',
                        style: TextStyle(fontWeight: FontWeight.w500),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Wrap(
                          spacing: 8,
                          children: [7, 30, 90].map((days) {
                            final isSelected = _selectedDays == days;
                            return ChoiceChip(
                              label: Text('$days days'),
                              selected: isSelected,
                              onSelected: (selected) {
                                if (selected) {
                                  setState(() => _selectedDays = days);
                                  provider.fetchAlarmHistory(days: days);
                                }
                              },
                            );
                          }).toList(),
                        ),
                      ),
                    ],
                  ),
                ),

                // Alarm list
                Expanded(
                  child: alarms.isEmpty
                      ? Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.history,
                                  size: 64, color: Colors.grey[400]),
                              const SizedBox(height: 16),
                              Text(
                                'No alarm history',
                                style: TextStyle(
                                  fontSize: 16,
                                  color: Colors.grey[600],
                                ),
                              ),
                            ],
                          ),
                        )
                      : ListView.builder(
                          itemCount: alarms.length,
                          padding: const EdgeInsets.all(16),
                          itemBuilder: (context, index) {
                            final alarm = alarms[index];
                            return _buildHistoryCard(alarm);
                          },
                        ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildHistoryCard(SafetyAlarm alarm) {
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
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: severityColor.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(
                      alarm.isCritical
                          ? Icons.warning_amber
                          : alarm.isWarning
                              ? Icons.error_outline
                              : Icons.info_outline,
                      color: severityColor,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          alarm.typeDisplay,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          DateFormat('dd/MM/yyyy HH:mm')
                              .format(alarm.timestamp.toLocal()),
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey[600],
                          ),
                        ),
                      ],
                    ),
                  ),
                  if (alarm.isResolved)
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.green.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.green),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.check_circle, size: 14, color: Colors.green),
                          SizedBox(width: 4),
                          Text(
                            'Resolved',
                            style: TextStyle(
                              fontSize: 11,
                              color: Colors.green,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    )
                  else if (alarm.isAcknowledged)
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.blue.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.blue),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.check, size: 14, color: Colors.blue),
                          SizedBox(width: 4),
                          Text(
                            'Acknowledged',
                            style: TextStyle(
                              fontSize: 11,
                              color: Colors.blue,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    ),
                ],
              ),
              if (alarm.description != null) ...[
                const SizedBox(height: 8),
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
                  Icon(Icons.location_on, size: 14, color: Colors.grey[600]),
                  const SizedBox(width: 4),
                  Text(
                    alarm.locationDisplay,
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 12,
                    ),
                  ),
                  const Spacer(),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 8,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: severityColor.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      alarm.severityDisplay,
                      style: TextStyle(
                        color: severityColor,
                        fontSize: 11,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
