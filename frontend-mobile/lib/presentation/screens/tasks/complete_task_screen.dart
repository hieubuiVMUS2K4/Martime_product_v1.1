import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../../data/models/maintenance_task.dart';
import '../../providers/task_provider.dart';
import '../../providers/sync_provider.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/common/loading_widget.dart';

class CompleteTaskScreen extends StatefulWidget {
  final MaintenanceTask task;

  const CompleteTaskScreen({
    super.key,
    required this.task,
  });

  @override
  State<CompleteTaskScreen> createState() => _CompleteTaskScreenState();
}

class _CompleteTaskScreenState extends State<CompleteTaskScreen> {
  final _formKey = GlobalKey<FormState>();
  final _runningHoursController = TextEditingController();
  final _sparePartsController = TextEditingController();
  final _notesController = TextEditingController();

  bool _isSubmitting = false;

  @override
  void initState() {
    super.initState();
    // Pre-fill running hours if available
    if (widget.task.runningHoursAtLastDone != null) {
      _runningHoursController.text = widget.task.runningHoursAtLastDone.toString();
    }
  }

  @override
  void dispose() {
    _runningHoursController.dispose();
    _sparePartsController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  Future<void> _submitCompletion() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      final syncProvider = Provider.of<SyncProvider>(context, listen: false);
      final authProvider = Provider.of<AuthProvider>(context, listen: false);

      final completedBy = authProvider.fullName ?? 'Unknown';
      final completedByCrewId = authProvider.crewId ?? 'UNKNOWN';

      await taskProvider.completeTask(
        taskId: widget.task.id,
        completedBy: completedBy,
        completedByCrewId: completedByCrewId,
        runningHours: double.tryParse(_runningHoursController.text),
        sparePartsUsed: _sparePartsController.text.trim().isEmpty
            ? null
            : _sparePartsController.text.trim(),
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
      );

      // Trigger sync if online
      if (syncProvider.isOnline) {
        await syncProvider.syncQueue();
      }

      if (mounted) {
        // Show success message
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                const Icon(Icons.check_circle, color: Colors.white),
                const SizedBox(width: 8),
                Text(
                  syncProvider.isOnline
                      ? 'Task completed successfully!'
                      : 'Task saved. Will sync when online.',
                ),
              ],
            ),
            backgroundColor: Colors.green,
            duration: const Duration(seconds: 3),
          ),
        );

        // Pop twice to go back to list
        Navigator.pop(context); // Close complete screen
        Navigator.pop(context); // Close detail screen
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error completing task: $e'),
            backgroundColor: Colors.red,
            duration: const Duration(seconds: 4),
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final syncProvider = Provider.of<SyncProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Complete Task'),
      ),
      body: Stack(
        children: [
          SingleChildScrollView(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Task Info Card
                    Card(
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              widget.task.equipmentName,
                              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              'Task ID: ${widget.task.taskId}',
                              style: TextStyle(color: Colors.grey.shade600),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              widget.task.taskDescription,
                              style: Theme.of(context).textTheme.bodyMedium,
                            ),
                          ],
                        ),
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Offline Warning
                    if (!syncProvider.isOnline)
                      Container(
                        padding: const EdgeInsets.all(12),
                        margin: const EdgeInsets.only(bottom: 16),
                        decoration: BoxDecoration(
                          color: Colors.orange.shade50,
                          border: Border.all(color: Colors.orange.shade300),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Row(
                          children: [
                            Icon(Icons.cloud_off, color: Colors.orange.shade700),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Text(
                                'You are offline. Task will be synced when connection is restored.',
                                style: TextStyle(color: Colors.orange.shade900),
                              ),
                            ),
                          ],
                        ),
                      ),

                    // Running Hours Field
                    Text(
                      'Running Hours *',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _runningHoursController,
                      decoration: const InputDecoration(
                        hintText: 'Enter current running hours',
                        prefixIcon: Icon(Icons.access_time),
                        suffixText: 'hours',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(RegExp(r'^\d+\.?\d{0,2}')),
                      ],
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'Please enter running hours';
                        }
                        final hours = double.tryParse(value);
                        if (hours == null || hours < 0) {
                          return 'Please enter a valid number';
                        }
                        if (widget.task.runningHoursAtLastDone != null &&
                            hours < widget.task.runningHoursAtLastDone!) {
                          return 'Running hours cannot be less than last maintenance (${widget.task.runningHoursAtLastDone})';
                        }
                        return null;
                      },
                    ),

                    const SizedBox(height: 24),

                    // Spare Parts Field
                    Text(
                      'Spare Parts Used',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _sparePartsController,
                      decoration: const InputDecoration(
                        hintText: 'List any spare parts used (optional)',
                        prefixIcon: Icon(Icons.build),
                        border: OutlineInputBorder(),
                      ),
                      maxLines: 2,
                    ),

                    const SizedBox(height: 24),

                    // Notes Field
                    Text(
                      'Notes',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _notesController,
                      decoration: const InputDecoration(
                        hintText: 'Add any additional notes or observations (optional)',
                        prefixIcon: Icon(Icons.notes),
                        border: OutlineInputBorder(),
                      ),
                      maxLines: 4,
                    ),

                    const SizedBox(height: 32),

                    // Submit Button
                    SizedBox(
                      width: double.infinity,
                      height: 50,
                      child: ElevatedButton(
                        onPressed: _isSubmitting ? null : _submitCompletion,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.green,
                          foregroundColor: Colors.white,
                        ),
                        child: _isSubmitting
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : const Text(
                                'Complete Task',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                      ),
                    ),

                    const SizedBox(height: 16),
                  ],
                ),
              ),
            ),
          ),

          // Loading Overlay
          if (_isSubmitting)
            LoadingOverlay(
              message: syncProvider.isOnline
                  ? 'Completing task...'
                  : 'Saving for offline sync...',
            ),
        ],
      ),
    );
  }
}
