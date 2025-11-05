import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../../providers/watchkeeping_provider.dart';
import '../../../data/models/watchkeeping_log.dart';
import '../../widgets/common/loading_widget.dart';

class WatchLogDetailScreen extends StatefulWidget {
  final int logId;

  const WatchLogDetailScreen({super.key, required this.logId});

  @override
  State<WatchLogDetailScreen> createState() => _WatchLogDetailScreenState();
}

class _WatchLogDetailScreenState extends State<WatchLogDetailScreen> {
  WatchkeepingLog? _log;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadLog();
  }

  Future<void> _loadLog() async {
    final provider = Provider.of<WatchkeepingProvider>(context, listen: false);
    final log = await provider.getLogById(widget.logId);
    setState(() {
      _log = log;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Watch Log Details'),
        actions: [
          if (_log != null && !_log!.isSigned)
            IconButton(
              icon: const Icon(Icons.add_chart),
              onPressed: () => _showUpdateLogDialog(),
              tooltip: 'Update Log Entry',
            ),
        ],
      ),
      body: _isLoading
          ? const Center(child: LoadingWidget(message: 'Loading watch log...'))
          : _log == null
              ? const Center(child: Text('Watch log not found'))
              : SingleChildScrollView(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Header Card
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                children: [
                                  Icon(
                                    Icons.calendar_today,
                                    color: Theme.of(context).colorScheme.primary,
                                  ),
                                  const SizedBox(width: 8),
                                  Text(
                                    DateFormat('EEEE, MMMM d, yyyy').format(_log!.watchDate),
                                    style: Theme.of(context).textTheme.titleLarge,
                                  ),
                                ],
                              ),
                              const SizedBox(height: 12),
                              Text(
                                _log!.watchPeriodDisplay,
                                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                              const SizedBox(height: 8),
                              Row(
                                children: [
                                  Container(
                                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                    decoration: BoxDecoration(
                                      color: _log!.watchType == 'NAVIGATION'
                                          ? Colors.blue.withOpacity(0.1)
                                          : Colors.orange.withOpacity(0.1),
                                      borderRadius: BorderRadius.circular(20),
                                    ),
                                    child: Text(
                                      _log!.watchTypeDisplay,
                                      style: TextStyle(
                                        color: _log!.watchType == 'NAVIGATION' ? Colors.blue : Colors.orange,
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ),
                                  const Spacer(),
                                  if (_log!.isSigned)
                                    Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                      decoration: BoxDecoration(
                                        color: Colors.green.withOpacity(0.1),
                                        borderRadius: BorderRadius.circular(20),
                                      ),
                                      child: const Row(
                                        children: [
                                          Icon(Icons.check_circle, color: Colors.green, size: 16),
                                          SizedBox(width: 4),
                                          Text(
                                            'Signed',
                                            style: TextStyle(
                                              color: Colors.green,
                                              fontWeight: FontWeight.bold,
                                            ),
                                          ),
                                        ],
                                      ),
                                    )
                                  else
                                    Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                      decoration: BoxDecoration(
                                        color: Colors.red.withOpacity(0.1),
                                        borderRadius: BorderRadius.circular(20),
                                      ),
                                      child: const Row(
                                        children: [
                                          Icon(Icons.warning, color: Colors.red, size: 16),
                                          SizedBox(width: 4),
                                          Text(
                                            'Unsigned',
                                            style: TextStyle(
                                              color: Colors.red,
                                              fontWeight: FontWeight.bold,
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ),

                      const SizedBox(height: 16),

                      // Personnel
                      _buildSection(
                        'Personnel',
                        Icons.people,
                        [
                          _buildInfoRow('Officer on Watch', _log!.officerOnWatch),
                          if (_log!.lookout != null)
                            _buildInfoRow('Lookout', _log!.lookout!),
                        ],
                      ),

                      // Weather & Sea Conditions
                      _buildSection(
                        'Weather & Sea Conditions',
                        Icons.wb_cloudy,
                        [
                          if (_log!.weatherConditions != null)
                            _buildInfoRow('Weather', _log!.weatherConditions!),
                          if (_log!.seaState != null)
                            _buildInfoRow('Sea State', _log!.seaStateDisplay),
                          if (_log!.visibility != null)
                            _buildInfoRow('Visibility', _log!.visibilityDisplay),
                        ],
                      ),

                      // Navigation Data
                      if (_log!.courseLogged != null || _log!.speedLogged != null || _log!.positionLat != null)
                        _buildSection(
                          'Navigation Data',
                          Icons.explore,
                          [
                            if (_log!.courseLogged != null)
                              _buildInfoRow('Course', '${_log!.courseLogged!.toStringAsFixed(0)}° True'),
                            if (_log!.speedLogged != null)
                              _buildInfoRow('Speed', '${_log!.speedLogged!.toStringAsFixed(1)} knots'),
                            if (_log!.distanceRun != null)
                              _buildInfoRow('Distance Run', '${_log!.distanceRun!.toStringAsFixed(1)} NM'),
                            if (_log!.positionLat != null)
                              _buildInfoRow('Position', _log!.positionDisplay),
                          ],
                        ),

                      // Engine Status
                      if (_log!.engineStatus != null)
                        _buildSection(
                          'Engine Status',
                          Icons.settings,
                          [
                            _buildInfoRow('Status', _log!.engineStatus!),
                          ],
                        ),

                      // Notable Events
                      if (_log!.hasNotableEvents)
                        _buildSection(
                          'Notable Events',
                          Icons.event_note,
                          [
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: Colors.orange.withOpacity(0.1),
                                borderRadius: BorderRadius.circular(8),
                                border: Border.all(color: Colors.orange.withOpacity(0.3)),
                              ),
                              child: Text(
                                _log!.notableEvents!,
                                style: Theme.of(context).textTheme.bodyMedium,
                              ),
                            ),
                          ],
                        ),

                      // Signature
                      if (_log!.isSigned)
                        _buildSection(
                          'Master Signature',
                          Icons.draw,
                          [
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(
                                color: Colors.green.withOpacity(0.1),
                                borderRadius: BorderRadius.circular(8),
                                border: Border.all(color: Colors.green.withOpacity(0.3)),
                              ),
                              child: Row(
                                children: [
                                  const Icon(Icons.check_circle, color: Colors.green),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: Text(
                                      _log!.masterSignature!,
                                      style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                                            fontStyle: FontStyle.italic,
                                            fontWeight: FontWeight.bold,
                                          ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                    ],
                  ),
                ),
    );
  }

  Widget _buildSection(String title, IconData icon, List<Widget> children) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
            const SizedBox(width: 8),
            Text(
              title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              children: children,
            ),
          ),
        ),
        const SizedBox(height: 16),
      ],
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(
              label,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.grey[600],
                  ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    fontWeight: FontWeight.w500,
                  ),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _showUpdateLogDialog() async {
    // Simple dialog to add notable events or update weather conditions
    final notesController = TextEditingController(text: _log?.notableEvents ?? '');
    
    final result = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Add Log Entry'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Add notable events or observations during your watch:'),
            const SizedBox(height: 16),
            TextField(
              controller: notesController,
              decoration: const InputDecoration(
                labelText: 'Notable Events',
                hintText: 'Course alterations, ships sighted, weather changes, etc.',
                border: OutlineInputBorder(),
              ),
              maxLines: 4,
              autofocus: true,
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.blue.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Row(
                children: [
                  Icon(Icons.info_outline, size: 16, color: Colors.blue),
                  SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'Only the Master can sign and finalize watch logs.',
                      style: TextStyle(fontSize: 12, color: Colors.blue),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Save'),
          ),
        ],
      ),
    );

    if (result == true && mounted) {
      // In a real app, you would call an API to update the log
      // For now, just show a success message
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Log entry saved. Waiting for Master signature.'),
        ),
      );
    }
  }
}
