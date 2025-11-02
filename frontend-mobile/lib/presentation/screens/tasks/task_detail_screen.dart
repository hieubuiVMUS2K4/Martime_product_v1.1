import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../../data/models/maintenance_task.dart';
import '../../../data/models/task_checklist_item.dart';
import '../../providers/task_provider.dart';
import '../../widgets/task/priority_badge.dart';
import '../../widgets/task/status_badge.dart';
import 'complete_task_screen.dart';

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

  @override
  Widget build(BuildContext context) {
    final taskProvider = Provider.of<TaskProvider>(context);
    final dateFormat = DateFormat('dd MMM yyyy');

    return Scaffold(
      appBar: AppBar(
        title: const Text('Task Details'),
      ),
      body: SingleChildScrollView(
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
                        '⚠️ OVERDUE: ${widget.task.daysUntilDue.abs()} days past due date',
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
                          'Task ID: ${widget.task.taskId}',
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
              title: 'Description',
              icon: Icons.description,
              child: Text(
                widget.task.taskDescription,
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ),

            // Task Type & Interval
            _buildSection(
              context,
              title: 'Maintenance Schedule',
              icon: Icons.schedule,
              child: Column(
                children: [
                  _buildInfoRow(
                    'Type',
                    widget.task.taskType.replaceAll('_', ' '),
                  ),
                  if (widget.task.intervalHours != null)
                    _buildInfoRow(
                      'Interval',
                      '${widget.task.intervalHours} running hours',
                    ),
                  if (widget.task.intervalDays != null)
                    _buildInfoRow(
                      'Interval',
                      '${widget.task.intervalDays} days',
                    ),
                ],
              ),
            ),

            // Due Date Info
            _buildSection(
              context,
              title: 'Schedule',
              icon: Icons.calendar_today,
              child: Column(
                children: [
                  if (widget.task.lastDoneAt != null)
                    _buildInfoRow(
                      'Last Done',
                      dateFormat.format(DateTime.parse(widget.task.lastDoneAt!)),
                    ),
                  _buildInfoRow(
                    'Next Due',
                    dateFormat.format(DateTime.parse(widget.task.nextDueAt)),
                  ),
                  _buildInfoRow(
                    'Days Until Due',
                    '${widget.task.daysUntilDue} days',
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
                title: 'Running Hours',
                icon: Icons.access_time,
                child: Column(
                  children: [
                    _buildInfoRow(
                      'At Last Maintenance',
                      '${widget.task.runningHoursAtLastDone} hours',
                    ),
                  ],
                ),
              ),

            // Assignment
            if (widget.task.assignedTo != null)
              _buildSection(
                context,
                title: 'Assignment',
                icon: Icons.person,
                child: Column(
                  children: [
                    _buildInfoRow('Assigned To', widget.task.assignedTo!),
                  ],
                ),
              ),

            // ========== CHECKLIST SECTION (CHỈ HIỂN thị NẾU CÓ TASKTYPE) ==========
            if (widget.task.hasTaskType) ...[
              _buildSection(
                context,
                title: '📋 Checklist công việc',
                icon: Icons.checklist,
                child: _buildChecklistContent(),
              ),
            ],

            // Completion Info
            if (widget.task.isCompleted)
              _buildSection(
                context,
                title: 'Completion Details',
                icon: Icons.check_circle,
                child: Column(
                  children: [
                    if (widget.task.completedBy != null)
                      _buildInfoRow('Completed By', widget.task.completedBy!),
                    if (widget.task.completedAt != null)
                      _buildInfoRow(
                        'Completed At',
                        dateFormat.format(DateTime.parse(widget.task.completedAt!)),
                      ),
                    if (widget.task.runningHoursAtCompletion != null)
                      _buildInfoRow(
                        'Running Hours',
                        '${widget.task.runningHoursAtCompletion} hours',
                      ),
                    if (widget.task.sparePartsUsed != null)
                      _buildInfoRow('Spare Parts', widget.task.sparePartsUsed!),
                    if (widget.task.notes != null) ...[
                      const SizedBox(height: 8),
                      const Text(
                        'Notes:',
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
                'Lỗi tải checklist',
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
                label: const Text('Thử lại'),
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
                'Chưa có checklist',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 16),
              ),
              const SizedBox(height: 4),
              Text(
                'Task này chưa có chi tiết công việc',
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
              'Tiến độ: $completedItems/$totalItems',
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
                'BẮT BUỘC',
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
                  'Đơn vị: ${detail.unit}',
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

    switch (type) {
      case 'MEASUREMENT':
        color = Colors.blue;
        label = 'Đo đạc';
        icon = Icons.straighten;
        break;
      case 'CHECKLIST':
        color = Colors.green;
        label = 'Kiểm tra';
        icon = Icons.check_box;
        break;
      case 'INSPECTION':
        color = Colors.orange;
        label = 'Quan sát';
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
                'Đã hoàn thành',
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
              'Giá trị đo: ${execution.measuredValue} ${item.taskDetail.unit ?? ""}',
              style: const TextStyle(fontSize: 11),
            ),
          ],
          if (execution.inspectionNotes != null && execution.inspectionNotes!.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(
              'Ghi chú: ${execution.inspectionNotes}',
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

    // Controllers for input
    final measurementController = TextEditingController(
      text: item.executionDetail?.measuredValue ?? '',
    );
    final notesController = TextEditingController(
      text: item.executionDetail?.inspectionNotes ?? '',
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
                      'BẮT BUỘC',
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
                    const Text(
                      'Kết quả kiểm tra:',
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
                            label: const Text('OK / Đạt'),
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
                              !checkResult && item.executionDetail != null
                                  ? Icons.cancel
                                  : Icons.cancel_outlined,
                              color: !checkResult && item.executionDetail != null
                                  ? Colors.white
                                  : Colors.red,
                            ),
                            label: const Text('NG / Lỗi'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: !checkResult && item.executionDetail != null
                                  ? Colors.red
                                  : Colors.grey.shade200,
                              foregroundColor: !checkResult && item.executionDetail != null
                                  ? Colors.white
                                  : Colors.black87,
                              padding: const EdgeInsets.symmetric(vertical: 12),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],

                  if (detail.detailType == 'MEASUREMENT') ...[
                    const Text(
                      'Giá trị đo được:',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: measurementController,
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      decoration: InputDecoration(
                        hintText: 'Nhập giá trị...',
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
                                'Giới hạn: ${detail.minValue ?? '?'} - ${detail.maxValue ?? '?'} ${detail.unit ?? ''}',
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
                    const Text(
                      'Ghi chú quan sát:',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: notesController,
                      maxLines: 4,
                      decoration: const InputDecoration(
                        hintText: 'Nhập ghi chú chi tiết...',
                        border: OutlineInputBorder(),
                        contentPadding: EdgeInsets.all(12),
                      ),
                    ),
                  ],

                  // Notes for all types
                  if (detail.detailType != 'INSPECTION') ...[
                    const SizedBox(height: 16),
                    const Text(
                      'Ghi chú (tùy chọn):',
                      style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: notesController,
                      maxLines: 3,
                      decoration: const InputDecoration(
                        hintText: 'Thêm ghi chú nếu cần...',
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
                              'Đã hoàn thành trước đó. Bạn có thể cập nhật lại.',
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
                child: const Text('Hủy'),
              ),
              ElevatedButton.icon(
                onPressed: () async {
                  // Validate based on type
                  if (detail.detailType == 'MEASUREMENT') {
                    if (measurementController.text.trim().isEmpty) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Vui lòng nhập giá trị đo!'),
                          backgroundColor: Colors.red,
                        ),
                      );
                      return;
                    }

                    // Validate min/max
                    final value = double.tryParse(measurementController.text.trim());
                    if (value == null) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Giá trị không hợp lệ!'),
                          backgroundColor: Colors.red,
                        ),
                      );
                      return;
                    }

                    if (detail.minValue != null && value < detail.minValue!) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text('Giá trị quá thấp! Tối thiểu: ${detail.minValue}'),
                          backgroundColor: Colors.orange,
                        ),
                      );
                    }

                    if (detail.maxValue != null && value > detail.maxValue!) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text('Giá trị quá cao! Tối đa: ${detail.maxValue}'),
                          backgroundColor: Colors.orange,
                        ),
                      );
                    }
                  }

                  if (detail.detailType == 'INSPECTION' && notesController.text.trim().isEmpty) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text('Vui lòng nhập ghi chú quan sát!'),
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
                          content: Text('✅ Đã lưu: ${detail.detailName}'),
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
                          content: Text('❌ Lỗi: $e'),
                          backgroundColor: Colors.red,
                        ),
                      );
                    }
                  }
                },
                icon: const Icon(Icons.check),
                label: Text(isAlreadyCompleted ? 'Cập nhật' : 'Hoàn thành'),
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
                      title: const Text('Task Overdue'),
                      content: Text(
                        'This task is ${widget.task.daysUntilDue.abs()} days overdue!\n\n'
                        'Due date: ${widget.task.nextDueAt}\n\n'
                        'Do you still want to start this task?',
                      ),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(context, false),
                          child: const Text('Cancel'),
                        ),
                        ElevatedButton(
                          onPressed: () => Navigator.pop(context, true),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.red,
                          ),
                          child: const Text('Start Anyway'),
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
                            ? 'Overdue task started! Please complete ASAP.'
                            : 'Task started successfully!',
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
                        content: Text('Failed to start task: $e'),
                        backgroundColor: Colors.red,
                      ),
                    );
                  }
                }
              },
        icon: const Icon(Icons.play_arrow),
        label: Text(widget.task.isOverdue ? 'Start Overdue Task' : 'Start Task'),
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
        label: const Text('Complete Task'),
        backgroundColor: Colors.green,
      );
    }

    return null;
  }
}
