import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../../data/models/maintenance_task.dart';
import '../../providers/task_provider.dart';
import '../../providers/sync_provider.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../../l10n/app_localizations.dart';

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
    final l10n = AppLocalizations.of(context);
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
                      ? l10n.taskCompletedSuccessfully
                      : l10n.taskSavedWillSync,
                ),
              ],
            ),
            backgroundColor: Colors.green,
            duration: const Duration(seconds: 3),
          ),
        );

        // Pop twice to go back to list
        if (mounted) {
          Navigator.pop(context); // Close complete screen
          if (mounted) {
            Navigator.pop(context); // Close detail screen
          }
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(AppLocalizations.of(context).errorCompletingTask(e.toString())),
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
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.completeTask),
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
                              '${l10n.taskId}: ${widget.task.taskId}',
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
                                l10n.offlineTaskWillSync,
                                style: TextStyle(color: Colors.orange.shade900),
                              ),
                            ),
                          ],
                        ),
                      ),

                    // Running Hours Field
                    Text(
                      l10n.runningHoursRequired,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _runningHoursController,
                      decoration: InputDecoration(
                        hintText: l10n.enterCurrentRunningHours,
                        prefixIcon: const Icon(Icons.access_time),
                        suffixText: l10n.hours,
                        border: const OutlineInputBorder(),
                      ),
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(RegExp(r'^\d+\.?\d{0,2}')),
                      ],
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return l10n.pleaseEnterRunningHours;
                        }
                        final hours = double.tryParse(value);
                        if (hours == null || hours < 0) {
                          return l10n.pleaseEnterValidNumber;
                        }
                        if (widget.task.runningHoursAtLastDone != null &&
                            hours < widget.task.runningHoursAtLastDone!) {
                          return l10n.runningHoursCannotBeLess(widget.task.runningHoursAtLastDone!);
                        }
                        return null;
                      },
                    ),

                    const SizedBox(height: 24),

                    // Spare Parts Field
                    Text(
                      l10n.sparePartsUsed,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _sparePartsController,
                      decoration: InputDecoration(
                        hintText: l10n.listSparePartsUsed,
                        prefixIcon: const Icon(Icons.build),
                        border: const OutlineInputBorder(),
                      ),
                      maxLines: 2,
                    ),

                    const SizedBox(height: 24),

                    // Notes Field
                    Text(
                      l10n.notes,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _notesController,
                      decoration: InputDecoration(
                        hintText: l10n.addAdditionalNotes,
                        prefixIcon: const Icon(Icons.notes),
                        border: const OutlineInputBorder(),
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
                            : Text(
                                l10n.completeTask,
                                style: const TextStyle(
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
                  ? l10n.completingTask
                  : l10n.savingForOfflineSync,
            ),
        ],
      ),
    );
  }
}
