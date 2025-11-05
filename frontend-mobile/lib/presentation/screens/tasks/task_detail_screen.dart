import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../../data/models/maintenance_task.dart';
import '../../../data/models/task_checklist_item.dart';
import '../../providers/task_provider.dart';
import '../../widgets/task/priority_badge.dart';
import '../../widgets/task/status_badge.dart';
import 'complete_task_screen.dart';
import '../../../l10n/app_localizations.dart';

class TaskDetailScreen extends StatefulWidget {
  final MaintenanceTask task;

  const TaskDetailScreen({
    super.key,
    required this.task,
  });

  @override
  State<TaskDetailScreen> createState() => _TaskDetailScreenState();
}

class _TaskDetailScreenState extends State<TaskDetailScreen> {
  List<TaskChecklistItem>? _checklistItems;
  bool _loadingChecklist = false;
  String? _checklistError;

  @override
  void initState() {
    super.initState();
    // Load checklist after frame is built to avoid setState during build
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadChecklistIfNeeded();
    });
  }

  Future<void> _loadChecklistIfNeeded() async {
    // Chỉ load checklist nếu task có TaskType
    if (!widget.task.hasTaskType || !mounted) return;
    
    setState(() {
      _loadingChecklist = true;
      _checklistError = null;
    });

    try {
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      await taskProvider.fetchTaskChecklist(widget.task.id);
      
      if (mounted) {
        setState(() {
          _checklistItems = taskProvider.currentChecklist;
          _loadingChecklist = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _checklistError = e.toString();
          _loadingChecklist = false;
        });
      }
    }
  }

  Future<void> _refreshTaskData() async {
    // Refresh cả task info và checklist khi Captain update
    final taskProvider = Provider.of<TaskProvider>(context, listen: false);
    
    try {
      // Reload task list để có task mới nhất
      await taskProvider.fetchMyTasks();
      
      // Reload checklist nếu có TaskType
      if (widget.task.hasTaskType) {
        await _loadChecklistIfNeeded();
      }
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(AppLocalizations.of(context).taskDataRefreshed),
            duration: Duration(seconds: 1),
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(AppLocalizations.of(context).failedToRefresh(e.toString())),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final taskProvider = Provider.of<TaskProvider>(context);
    final dateFormat = DateFormat('dd MMM yyyy');
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.taskDetails),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: l10n.refreshTaskData,
            onPressed: _refreshTaskData,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _refreshTaskData,
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
            // Overdue Warning Banner
            if (widget.task.isOverdue && !widget.task.isCompleted)
              Container(
                width: double.infinity,
                color: Colors.red.shade700,
                padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                child: Row(
                  children: [
                    const Icon(Icons.warning, color: Colors.white),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        l10n.overdueDaysPastDue(widget.task.daysUntilDue.abs()),
                        style: const TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            
            // Header Card
            Card(
              margin: const EdgeInsets.all(16),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Equipment Name
                    Text(
                      widget.task.equipmentName,
                      style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                    const SizedBox(height: 8),

                    // Task ID
                    Row(
                      children: [
                        Icon(Icons.tag, size: 16, color: Colors.grey.shade600),
                        const SizedBox(width: 4),
                        Text(
                          '${l10n.taskId} ${widget.task.taskId}',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),

                    // Badges
                    Row(
                      children: [
                        PriorityBadge(priority: widget.task.priority),
                        const SizedBox(width: 8),
                        StatusBadge(task: widget.task),
                      ],
                    ),
                  ],
                ),
              ),
            ),

            // Task Description
            _buildSection(
              context,
              title: l10n.description,
              icon: Icons.description,
              child: Text(
                widget.task.taskDescription,
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ),

            // Task Type & Interval
            _buildSection(
              context,
              title: l10n.maintenanceSchedule,
              icon: Icons.schedule,
              child: Column(
                children: [
                  _buildInfoRow(
                    l10n.type,
                    widget.task.taskType.replaceAll('_', ' '),
                  ),
                  if (widget.task.intervalHours != null)
                    _buildInfoRow(
                      l10n.interval,
                      l10n.runningHoursValue(widget.task.intervalHours!.toInt()),
                    ),
                  if (widget.task.intervalDays != null)
                    _buildInfoRow(
                      l10n.interval,
                      l10n.daysValue(widget.task.intervalDays!.toInt()),
                    ),
                ],
              ),
            ),

            // Due Date Info
            _buildSection(
              context,
              title: l10n.schedule,
              icon: Icons.calendar_today,
              child: Column(
                children: [
                  if (widget.task.lastDoneAt != null)
                    _buildInfoRow(
                      l10n.lastDone,
                      dateFormat.format(DateTime.parse(widget.task.lastDoneAt!)),
                    ),
                  _buildInfoRow(
                    l10n.nextDue,
                    dateFormat.format(DateTime.parse(widget.task.nextDueAt)),
                  ),
                  _buildInfoRow(
                    l10n.daysUntilDue,
                    l10n.daysValue(widget.task.daysUntilDue),
                    valueColor: widget.task.isOverdue
                        ? Colors.red.shade700
                        : widget.task.isDueSoon
                            ? Colors.orange.shade700
                            : null,
                  ),
                ],
              ),
            ),

            // Running Hours
            if (widget.task.runningHoursAtLastDone != null)
              _buildSection(
                context,
                title: l10n.runningHours,
                icon: Icons.access_time,
                child: Column(
                  children: [
                    _buildInfoRow(
                      l10n.atLastMaintenance,
                      l10n.hoursValue(widget.task.runningHoursAtLastDone!.toInt()),
                    ),
                  ],
                ),
              ),

            // Assignment
            if (widget.task.assignedTo != null)
              _buildSection(
                context,
                title: l10n.assignment,
                icon: Icons.person,
                child: Column(
                  children: [
                    _buildInfoRow(l10n.assignedTo, widget.task.assignedTo!),
                  ],
                ),
              ),

            // ========== CHECKLIST SECTION (CHỈ HIỂN thị NẾU CÓ TASKTYPE) ==========
            if (widget.task.hasTaskType) ...[
              _buildSection(
                context,
                title: l10n.taskChecklist,
                icon: Icons.checklist,
                child: _buildChecklistContent(),
              ),
            ],

            // Completion Info
            if (widget.task.isCompleted)
              _buildSection(
                context,
                title: l10n.completionDetails,
                icon: Icons.check_circle,
                child: Column(
                  children: [
                    if (widget.task.completedBy != null)
                      _buildInfoRow(l10n.completedBy, widget.task.completedBy!),
                    if (widget.task.completedAt != null)
                      _buildInfoRow(
                        l10n.completedAt,
                        dateFormat.format(DateTime.parse(widget.task.completedAt!)),
                      ),
                    if (widget.task.runningHoursAtCompletion != null)
                      _buildInfoRow(
                        l10n.runningHours,
                        l10n.hoursValue(widget.task.runningHoursAtCompletion!.toInt()),
                      ),
                    if (widget.task.sparePartsUsed != null)
                      _buildInfoRow(l10n.spareParts, widget.task.sparePartsUsed!),
                    if (widget.task.notes != null) ...[
                      const SizedBox(height: 8),
                      Text(
                        '${l10n.notes}:',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 4),
                      Text(widget.task.notes!),
                    ],
                  ],
                ),
              ),

            const SizedBox(height: 100), // Space for FAB
          ],
        ),
        ),
      ),
      floatingActionButton: _buildActionButton(context, taskProvider),
    );
  }

  Widget _buildSection(
    BuildContext context, {
    required String title,
    required IconData icon,
    required Widget child,
  }) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
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
            const SizedBox(height: 12),
            child,
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value, {Color? valueColor}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 140,
            child: Text(
              label,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: TextStyle(
                fontWeight: FontWeight.w600,
                color: valueColor,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildChecklistContent() {
    final l10n = AppLocalizations.of(context);
    if (_loadingChecklist) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(24.0),
          child: CircularProgressIndicator(),
        ),
      );
    }

    if (_checklistError != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            children: [
              const Icon(Icons.error_outline, color: Colors.red, size: 48),
              const SizedBox(height: 8),
              Text(
                l10n.errorLoadingChecklist,
                style: TextStyle(color: Colors.red.shade700, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 4),
              Text(
                _checklistError!,
                style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 12),
              ElevatedButton.icon(
                onPressed: _loadChecklistIfNeeded,
                icon: const Icon(Icons.refresh),
                label: Text(l10n.tryAgain),
              ),
            ],
          ),
        ),
      );
    }

    if (_checklistItems == null || _checklistItems!.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            children: [
              Icon(Icons.checklist, color: Colors.grey.shade400, size: 64),
              const SizedBox(height: 12),
              Text(
                l10n.noChecklistYet,
                style: TextStyle(color: Colors.grey.shade600, fontSize: 16),
              ),
              const SizedBox(height: 4),
              Text(
                l10n.thisTaskHasNoDetails,
                style: TextStyle(color: Colors.grey.shade500, fontSize: 12),
              ),
            ],
          ),
        ),
      );
    }

    // Hiển thị danh sách checklist items
    return Column(
      children: [
        // Progress bar
        if (_checklistItems!.isNotEmpty) ...[
          _buildProgressBar(),
          const SizedBox(height: 16),
        ],
        
        // Checklist items
        ListView.separated(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: _checklistItems!.length,
          separatorBuilder: (context, index) => const Divider(height: 1),
          itemBuilder: (context, index) {
            final item = _checklistItems![index];
            return _buildChecklistItem(item, index + 1);
          },
        ),
      ],
    );
  }

  Widget _buildProgressBar() {
    final l10n = AppLocalizations.of(context);
    final totalItems = _checklistItems!.length;
    final completedItems = _checklistItems!.where((item) => item.isCompleted).length;
    final progress = totalItems > 0 ? completedItems / totalItems : 0.0;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              l10n.progressCount(completedItems, totalItems),
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600),
            ),
            Text(
              '${(progress * 100).toStringAsFixed(0)}%',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.bold,
                color: progress == 1.0 ? Colors.green : Colors.blue,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        LinearProgressIndicator(
          value: progress,
          backgroundColor: Colors.grey.shade200,
          valueColor: AlwaysStoppedAnimation<Color>(
            progress == 1.0 ? Colors.green : Colors.blue,
          ),
          minHeight: 8,
        ),
      ],
    );
  }

  Widget _buildChecklistItem(TaskChecklistItem item, int index) {
    final l10n = AppLocalizations.of(context);
    final detail = item.taskDetail;
    final isCompleted = item.isCompleted;
    
    return ListTile(
      contentPadding: const EdgeInsets.symmetric(horizontal: 0, vertical: 8),
      leading: CircleAvatar(
        radius: 16,
        backgroundColor: isCompleted ? Colors.green : Colors.grey.shade300,
        child: isCompleted
            ? const Icon(Icons.check, color: Colors.white, size: 18)
            : Text(
                '$index',
                style: TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                  color: Colors.grey.shade600,
                ),
              ),
      ),
      title: Row(
        children: [
          Expanded(
            child: Text(
              detail.detailName,
              style: TextStyle(
                fontWeight: FontWeight.w600,
                decoration: isCompleted ? TextDecoration.lineThrough : null,
                color: isCompleted ? Colors.grey : Colors.black87,
              ),
            ),
          ),
          if (detail.isMandatory)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: Colors.red.shade50,
                borderRadius: BorderRadius.circular(4),
                border: Border.all(color: Colors.red.shade200),
              ),
              child: Text(
                l10n.mandatory,
                style: TextStyle(
                  fontSize: 9,
                  fontWeight: FontWeight.bold,
                  color: Colors.red.shade700,
                ),
              ),
            ),
        ],
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SizedBox(height: 4),
          Row(
            children: [
              _buildDetailTypeBadge(detail.detailType),
              if (detail.detailType == 'MEASUREMENT' && detail.unit != null) ...[
                const SizedBox(width: 8),
                Text(
                  l10n.unitLabel(detail.unit!),
                  style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                ),
              ],
            ],
          ),
          if (detail.description != null && detail.description!.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(
              detail.description!,
              style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
            ),
          ],
          if (isCompleted && item.executionDetail != null) ...[
            const SizedBox(height: 6),
            _buildExecutionResult(item),
          ],
        ],
      ),
      trailing: widget.task.isInProgress
          ? IconButton(
              icon: Icon(
                isCompleted ? Icons.edit : Icons.check_circle_outline,
                color: isCompleted ? Colors.blue : Colors.green,
              ),
              onPressed: () {
                _showChecklistItemDialog(item);
              },
            )
          : null,
    );
  }

  Widget _buildDetailTypeBadge(String type) {
    Color color;
    String label;
    IconData icon;

    final l10n = AppLocalizations.of(context);

    switch (type) {
      case 'MEASUREMENT':
        color = Colors.blue;
        label = l10n.measurement;
        icon = Icons.straighten;
        break;
      case 'CHECKLIST':
        color = Colors.green;
        label = l10n.checklist;
        icon = Icons.check_box;
        break;
      case 'INSPECTION':
        color = Colors.orange;
        label = l10n.inspection;
        icon = Icons.search;
        break;
      default:
        color = Colors.grey;
        label = type;
        icon = Icons.info;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: color),
          const SizedBox(width: 4),
          Text(
            label,
            style: TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildExecutionResult(TaskChecklistItem item) {
    final execution = item.executionDetail!;
    final l10n = AppLocalizations.of(context);
    
    return Container(
      padding: const EdgeInsets.all(8),
      decoration: BoxDecoration(
        color: Colors.green.shade50,
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: Colors.green.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.check_circle, size: 14, color: Colors.green.shade700),
              const SizedBox(width: 4),
              Text(
                l10n.completed,
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.bold,
                  color: Colors.green.shade700,
                ),
              ),
            ],
          ),
          if (execution.measuredValue != null) ...[
            const SizedBox(height: 4),
            Text(
              l10n.measuredValueWithUnit(
                execution.measuredValue!,
                item.taskDetail.unit ?? "",
              ),
              style: const TextStyle(fontSize: 11),
            ),
          ],
          if (execution.notes != null && execution.notes!.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(
              '${l10n.notes}: ${execution.notes}',
              style: const TextStyle(fontSize: 11),
            ),
          ],
        ],
      ),
    );
  }

  void _showChecklistItemDialog(TaskChecklistItem item) {
    final detail = item.taskDetail;
    final isAlreadyCompleted = item.isCompleted;
    final l10n = AppLocalizations.of(context);

    // Controllers for input
    final measurementController = TextEditingController(
      text: item.executionDetail?.measuredValue ?? '',
    );
    final notesController = TextEditingController(
      text: item.executionDetail?.notes ?? '',
    );
    bool checkResult = item.executionDetail?.checkResult ?? false;

    showDialog(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) {
          return AlertDialog(
            title: Row(
              children: [
                Expanded(
                  child: Text(
                    detail.detailName,
                    style: const TextStyle(fontSize: 18),
                  ),
                ),
                if (detail.isMandatory)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.red.shade50,
                      borderRadius: BorderRadius.circular(4),
                      border: Border.all(color: Colors.red.shade200),
                    ),
                    child: Text(
                      l10n.mandatory,
                      style: TextStyle(
                        fontSize: 9,
                        fontWeight: FontWeight.bold,
                        color: Colors.red.shade700,
                      ),
                    ),
                  ),
              ],
            ),
            content: SingleChildScrollView(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Type badge
                  _buildDetailTypeBadge(detail.detailType),
                  
                  if (detail.description != null && detail.description!.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    Text(
                      detail.description!,
                      style: TextStyle(color: Colors.grey.shade700, fontSize: 14),
                    ),
                  ],

                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 16),

                  // Input based on type
                  if (detail.detailType == 'CHECKLIST') ...[
                    Text(
                      l10n.checkResult,
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(
                          child: ElevatedButton.icon(
                            onPressed: () {
                              setDialogState(() {
                                checkResult = true;
                              });
                            },
                            icon: Icon(
                              checkResult ? Icons.check_circle : Icons.check_circle_outline,
                              color: checkResult ? Colors.white : Colors.green,
                            ),
                            label: Text(l10n.okPass),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: checkResult ? Colors.green : Colors.grey.shade200,
                              foregroundColor: checkResult ? Colors.white : Colors.black87,
                              padding: const EdgeInsets.symmetric(vertical: 12),
                            ),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: ElevatedButton.icon(
                            onPressed: () {
                              setDialogState(() {
                                checkResult = false;
                              });
                            },
                            icon: Icon(
                              checkResult == false ? Icons.cancel : Icons.cancel_outlined,
                              color: checkResult == false ? Colors.white : Colors.red,
                            ),
                            label: Text(l10n.ngFail),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: checkResult == false ? Colors.red : Colors.grey.shade200,
                              foregroundColor: checkResult == false ? Colors.white : Colors.black87,
                              padding: const EdgeInsets.symmetric(vertical: 12),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],

                  if (detail.detailType == 'MEASUREMENT') ...[
                    Text(
                      l10n.measuredValue,
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: measurementController,
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      decoration: InputDecoration(
                        hintText: l10n.enterValue,
                        suffixText: detail.unit ?? '',
                        border: const OutlineInputBorder(),
                        contentPadding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 12,
                        ),
                      ),
                    ),
                    if (detail.minValue != null || detail.maxValue != null) ...[
                      const SizedBox(height: 8),
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.blue.shade50,
                          borderRadius: BorderRadius.circular(6),
                          border: Border.all(color: Colors.blue.shade200),
                        ),
                        child: Row(
                          children: [
                            Icon(Icons.info_outline, size: 16, color: Colors.blue.shade700),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                l10n.limitRange(
                                  detail.minValue?.toString() ?? '?',
                                  detail.maxValue?.toString() ?? '?',
                                  detail.unit ?? '',
                                ),
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.blue.shade700,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ],

                  if (detail.detailType == 'INSPECTION') ...[
                    Text(
                      l10n.observationNotes,
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: notesController,
                      maxLines: 4,
                      decoration: InputDecoration(
                        hintText: l10n.enterDetailedNotes,
                        border: OutlineInputBorder(),
                        contentPadding: EdgeInsets.all(12),
                      ),
                    ),
                  ],

                  // Notes for all types
                  if (detail.detailType != 'INSPECTION') ...[
                    const SizedBox(height: 16),
                    Text(
                      l10n.notesOptional,
                      style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: notesController,
                      maxLines: 3,
                      decoration: InputDecoration(
                        hintText: l10n.addNotesIfNeeded,
                        border: OutlineInputBorder(),
                        contentPadding: EdgeInsets.all(12),
                      ),
                    ),
                  ],

                  if (isAlreadyCompleted) ...[
                    const SizedBox(height: 12),
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: Colors.green.shade50,
                        borderRadius: BorderRadius.circular(6),
                        border: Border.all(color: Colors.green.shade200),
                      ),
                      child: Row(
                        children: [
                          Icon(Icons.check_circle, color: Colors.green.shade700, size: 20),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              l10n.alreadyCompletedCanUpdate,
                              style: TextStyle(
                                fontSize: 12,
                                color: Colors.green.shade700,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: Text(l10n.cancel),
              ),
              ElevatedButton.icon(
                onPressed: () async {
                  // Validate based on type
                  if (detail.detailType == 'MEASUREMENT') {
                    if (measurementController.text.trim().isEmpty) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.pleaseEnterMeasuredValue),
                          backgroundColor: Colors.red,
                        ),
                      );
                      return;
                    }

                    // Validate min/max
                    final value = double.tryParse(measurementController.text.trim());
                    if (value == null) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.invalidValue),
                          backgroundColor: Colors.red,
                        ),
                      );
                      return;
                    }

                    if (detail.minValue != null && value < detail.minValue!) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.valueTooLow(detail.minValue.toString())),
                          backgroundColor: Colors.orange,
                        ),
                      );
                    }

                    if (detail.maxValue != null && value > detail.maxValue!) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.valueTooHigh(detail.maxValue.toString())),
                          backgroundColor: Colors.orange,
                        ),
                      );
                    }
                  }

                  if (detail.detailType == 'INSPECTION' && notesController.text.trim().isEmpty) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(l10n.pleaseEnterObservationNote),
                        backgroundColor: Colors.red,
                      ),
                    );
                    return;
                  }

                  // Save data
                  try {
                    final taskProvider = Provider.of<TaskProvider>(context, listen: false);
                    
                    await taskProvider.completeChecklistItem(
                      taskId: widget.task.id,
                      detailId: detail.id,
                      measuredValue: detail.detailType == 'MEASUREMENT'
                          ? measurementController.text.trim()
                          : null,
                      checkResult: detail.detailType == 'CHECKLIST' ? checkResult : null,
                      inspectionNotes: notesController.text.trim().isNotEmpty
                          ? notesController.text.trim()
                          : null,
                      photoUrl: null, // TODO: Add photo feature later
                      isCompleted: true,
                    );

                    if (context.mounted) {
                      Navigator.pop(context);
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.savedItem(detail.detailName)),
                          backgroundColor: Colors.green,
                        ),
                      );
                      
                      // Reload checklist to update UI
                      setState(() {
                        _checklistItems = taskProvider.currentChecklist;
                      });
                    }
                  } catch (e) {
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(l10n.errorMessage(e.toString())),
                          backgroundColor: Colors.red,
                        ),
                      );
                    }
                  }
                },
                icon: const Icon(Icons.check),
                label: Text(isAlreadyCompleted ? l10n.update : l10n.complete),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.green,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget? _buildActionButton(BuildContext context, TaskProvider taskProvider) {
    final l10n = AppLocalizations.of(context);
    if (widget.task.isCompleted) {
      return null; // No action for completed tasks
    }

    if (widget.task.isPending) {
      return FloatingActionButton.extended(
        onPressed: taskProvider.isLoading
            ? null
            : () async {
                // Show warning if task is overdue
                if (widget.task.isOverdue) {
                  final confirmed = await showDialog<bool>(
                    context: context,
                    builder: (context) => AlertDialog(
                      icon: const Icon(Icons.warning, color: Colors.red, size: 48),
                      title: Text(l10n.taskOverdue),
                      content: Text(
                        l10n.thisTaskIsOverdue,
                      ),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(context, false),
                          child: Text(l10n.cancel),
                        ),
                        ElevatedButton(
                          onPressed: () => Navigator.pop(context, true),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.red,
                          ),
                          child: Text(l10n.startAnyway),
                        ),
                      ],
                    ),
                  );
                  
                  if (confirmed != true) return;
                }
                
                try {
                  await taskProvider.startTask(widget.task.id);
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                          widget.task.isOverdue 
                            ? l10n.overdueTaskStarted
                            : l10n.taskStartedSuccessfully,
                        ),
                        backgroundColor: widget.task.isOverdue ? Colors.orange : null,
                      ),
                    );
                    Navigator.pop(context);
                  }
                } catch (e) {
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(l10n.failedToStartTask(e.toString())),
                        backgroundColor: Colors.red,
                      ),
                    );
                  }
                }
              },
        icon: const Icon(Icons.play_arrow),
        label: Text(l10n.startTask),
        backgroundColor: widget.task.isOverdue ? Colors.red : null,
      );
    }

    if (widget.task.isInProgress) {
      return FloatingActionButton.extended(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => CompleteTaskScreen(task: widget.task),
            ),
          );
        },
        icon: const Icon(Icons.check),
        label: Text(l10n.completeTask),
        backgroundColor: Colors.green,
      );
    }

    return null;
  }
}
