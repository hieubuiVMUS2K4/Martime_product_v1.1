import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/alarm_provider.dart';

class AlarmStatisticsScreen extends StatefulWidget {
  const AlarmStatisticsScreen({super.key});

  @override
  State<AlarmStatisticsScreen> createState() => _AlarmStatisticsScreenState();
}

class _AlarmStatisticsScreenState extends State<AlarmStatisticsScreen> {
  int _selectedDays = 30;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AlarmProvider>().fetchStatistics(days: _selectedDays);
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Alarm Statistics'),
      ),
      body: Consumer<AlarmProvider>(
        builder: (context, provider, child) {
          if (provider.isLoadingStats) {
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
                    onPressed: () {
                      provider.fetchStatistics(days: _selectedDays);
                    },
                    child: const Text('Retry'),
                  ),
                ],
              ),
            );
          }

          final stats = provider.statistics;
          if (stats == null) {
            return const Center(child: Text('No data available'));
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Time range selector
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Time Range',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          children: [7, 30, 90].map((days) {
                            final isSelected = _selectedDays == days;
                            return ChoiceChip(
                              label: Text('$days days'),
                              selected: isSelected,
                              onSelected: (selected) {
                                if (selected) {
                                  setState(() => _selectedDays = days);
                                  provider.fetchStatistics(days: days);
                                }
                              },
                            );
                          }).toList(),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Overview cards
                Row(
                  children: [
                    Expanded(
                      child: _buildStatCard(
                        'Total',
                        '${stats.total}',
                        Icons.notifications,
                        Colors.blue,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _buildStatCard(
                        'Active',
                        '${stats.active}',
                        Icons.warning_amber,
                        Colors.orange,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: _buildStatCard(
                        'Acknowledged',
                        '${stats.acknowledged}',
                        Icons.check_circle,
                        Colors.green,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _buildStatCard(
                        'Resolved',
                        '${stats.resolved}',
                        Icons.done_all,
                        Colors.purple,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 24),

                // By severity
                const Text(
                  'By Severity',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 12),
                ...stats.bySeverity.map((item) {
                  Color color;
                  if (item.severity.toUpperCase() == 'CRITICAL') {
                    color = Colors.red;
                  } else if (item.severity.toUpperCase() == 'WARNING') {
                    color = Colors.orange;
                  } else {
                    color = Colors.blue;
                  }
                  
                  final percentage = stats.total > 0 
                      ? (item.count / stats.total * 100).toStringAsFixed(1)
                      : '0.0';

                  return _buildProgressBar(
                    item.severity,
                    item.count,
                    stats.total,
                    color,
                    percentage,
                  );
                }),
                const SizedBox(height: 24),

                // By type
                const Text(
                  'By Alarm Type',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 12),
                ...stats.byType.map((item) {
                  final percentage = stats.total > 0
                      ? (item.count / stats.total * 100).toStringAsFixed(1)
                      : '0.0';
                      
                  return _buildProgressBar(
                    item.type,
                    item.count,
                    stats.total,
                    Colors.indigo,
                    percentage,
                  );
                }),
                const SizedBox(height: 24),

                // By location
                if (stats.byLocation.isNotEmpty) ...[
                  const Text(
                    'By Location',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 12),
                  ...stats.byLocation.map((item) {
                    final percentage = stats.total > 0
                        ? (item.count / stats.total * 100).toStringAsFixed(1)
                        : '0.0';
                        
                    return _buildProgressBar(
                      item.location,
                      item.count,
                      stats.total,
                      Colors.teal,
                      percentage,
                    );
                  }),
                ],
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildStatCard(
      String label, String value, IconData icon, Color color) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: 8),
            Text(
              value,
              style: TextStyle(
                fontSize: 28,
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: TextStyle(
                fontSize: 13,
                color: Colors.grey[600],
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProgressBar(
      String label, int count, int total, Color color, String percentage) {
    final progress = total > 0 ? count / total : 0.0;

    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                label,
                style: const TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
              ),
              Text(
                '$count ($percentage%)',
                style: TextStyle(
                  fontSize: 14,
                  color: Colors.grey[600],
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 12,
              backgroundColor: Colors.grey[200],
              valueColor: AlwaysStoppedAnimation<Color>(color),
            ),
          ),
        ],
      ),
    );
  }
}
