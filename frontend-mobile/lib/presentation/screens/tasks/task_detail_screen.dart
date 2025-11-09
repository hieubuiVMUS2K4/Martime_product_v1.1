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

// 🎨 Maritime Professional Color Palette
class MaritimeColors {
  // Primary - Navy Blue (vững chắc, đáng tin cậy)
  static const primary = Color(0xFF1A3A52);        // Navy Blue
  static const primaryLight = Color(0xFF2C5F7F);   // Lighter Navy
  static const primaryDark = Color(0xFF0D1F2D);    // Darker Navy
  
  // Accent - Sage Green (trầm, chuyên nghiệp)
  static const accent = Color(0xFF6B8E7F);         // Sage Green
  static const accentLight = Color(0xFF8FA99D);    // Light Sage
  
  // Status Colors - Maritime theme
  static const completed = Color(0xFF4A7C59);      // Deep Green
  static const inProgress = Color(0xFFD97706);     // Amber
  static const overdue = Color(0xFFC2410C);        // Deep Orange/Red
  static const mandatory = Color(0xFFB91C1C);      // Deep Red
  
  // Neutral - Professional grays
  static const surfaceLight = Color(0xFFF8FAFC);   // Very light gray
  static const surface = Color(0xFFF1F5F9);        // Light gray
  static const border = Color(0xFFCBD5E1);         // Gray border
  static const textPrimary = Color(0xFF0F172A);    // Almost black
  static const textSecondary = Color(0xFF475569);  // Medium gray
  static const textTertiary = Color(0xFF94A3B8);   // Light gray text
}

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
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Overdue Warning Banner - Maritime theme
              if (widget.task.isOverdue && !widget.task.isCompleted)
                Container(
                  width: double.infinity,
                  color: MaritimeColors.overdue,
                  padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 16),
                  child: Row(
                    children: [
                      const Icon(Icons.warning_rounded, color: Colors.white, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          l10n.overdueDaysPastDue(widget.task.daysUntilDue.abs()),
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.w600,
                            fontSize: 13,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              
              // Professional Compact Header Card
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                margin: EdgeInsets.zero,
                child: Padding(
                  padding: const EdgeInsets.all(14),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Equipment Name + Task ID in one row
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Text(
                              widget.task.equipmentName,
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Colors.grey.shade900,
                                height: 1.2,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 6),
                      
                      // Task ID + Badges in one compact row
                      Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                              vertical: 3,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.grey.shade100,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Text(
                              '#${widget.task.taskId}',
                              style: TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.w600,
                                color: Colors.grey.shade700,
                              ),
                            ),
                          ),
                          const SizedBox(width: 6),
                          PriorityBadge(priority: widget.task.priority),
                          const SizedBox(width: 6),
                          StatusBadge(task: widget.task),
                        ],
                      ),
                      
                      const SizedBox(height: 10),
                      Divider(height: 1, color: MaritimeColors.border),
                      const SizedBox(height: 10),
                      
                      // Description (only show if not empty)
                      if (widget.task.taskDescription.trim().isNotEmpty) ...[
                        Text(
                          widget.task.taskDescription,
                          style: TextStyle(
                            fontSize: 13,
                            color: Colors.grey.shade600,
                            height: 1.4,
                          ),
                        ),
                        
                        const SizedBox(height: 12),
                        const Divider(height: 1),
                        const SizedBox(height: 12),
                      ],
                      
                      // Compact Info Grid (2 columns)
                      _buildCompactInfoGrid(context, dateFormat, l10n),
                    ],
                  ),
                ),
              ),

            const SizedBox(height: 8),

            // CHECKLIST SECTION (CHỈ HIỂN thị NẾU CÓ TASKTYPE)
            if (widget.task.hasTaskType) ...[
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                margin: EdgeInsets.zero,
                child: Padding(
                  padding: const EdgeInsets.all(14),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Clean header - no icon
                      Text(
                        l10n.taskChecklist,
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Colors.grey.shade900,
                        ),
                      ),
                      const SizedBox(height: 12),
                      _buildChecklistContent(),
                    ],
                  ),
                ),
              ),
            ],

            const SizedBox(height: 100), // Space for bottom bar + safety margin
          ],
        ),
      ),
    ),
      bottomNavigationBar: _buildBottomActionBar(context, taskProvider),
    );
  }

  Widget _buildCompactInfoGrid(
    BuildContext context,
    DateFormat dateFormat,
    AppLocalizations l10n,
  ) {
    final screenWidth = MediaQuery.of(context).size.width;
    final isSmallScreen = screenWidth < 360;
    final spacing = isSmallScreen ? 6.0 : 8.0;
    
    return Column(
      children: [
        // Row 1: Type + Interval (Horizontal compact)
        Row(
          children: [
            Expanded(
              child: _buildInfoItem(
                label: l10n.type,
                value: widget.task.taskType.replaceAll('_', ' '),
                isSmallScreen: isSmallScreen,
                color: MaritimeColors.primaryLight,
              ),
            ),
            if (widget.task.intervalDays != null || widget.task.intervalHours != null) ...[
              SizedBox(width: spacing),
              Expanded(
                child: _buildInfoItem(
                  label: l10n.interval,
                  value: widget.task.intervalDays != null
                      ? l10n.daysValue(widget.task.intervalDays!.toInt())
                      : l10n.runningHoursValue(widget.task.intervalHours!.toInt()),
                  isSmallScreen: isSmallScreen,
                  color: MaritimeColors.accent,
                ),
              ),
            ],
          ],
        ),
        SizedBox(height: spacing),
        
        // Row 2: Combined Next Due + Days Left (Merged for prominence)
        Container(
          padding: EdgeInsets.all(isSmallScreen ? 10 : 12),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(8),
            gradient: LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: widget.task.isOverdue 
                ? [
                    Colors.red.withOpacity(0.1),
                    Colors.red.withOpacity(0.05),
                  ]
                : widget.task.isDueSoon
                  ? [
                      Colors.orange.withOpacity(0.1),
                      Colors.orange.withOpacity(0.05),
                    ]
                  : [
                      Colors.blue.withOpacity(0.08),
                      Colors.blue.withOpacity(0.03),
                    ],
            ),
            border: Border.all(
              color: widget.task.isOverdue
                  ? Colors.red.shade200
                  : widget.task.isDueSoon
                      ? Colors.orange.shade200
                      : Colors.blue.shade100,
              width: 1,
            ),
          ),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      l10n.nextDue,
                      style: TextStyle(
                        fontSize: isSmallScreen ? 10 : 11,
                        fontWeight: FontWeight.w500,
                        color: MaritimeColors.textTertiary,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      dateFormat.format(DateTime.parse(widget.task.nextDueAt)),
                      style: TextStyle(
                        fontSize: isSmallScreen ? 13 : 14,
                        fontWeight: FontWeight.w700,
                        color: MaritimeColors.textPrimary,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    l10n.daysUntilDue,
                    style: TextStyle(
                      fontSize: isSmallScreen ? 10 : 11,
                      fontWeight: FontWeight.w500,
                      color: MaritimeColors.textTertiary,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.daysValue(widget.task.daysUntilDue),
                    style: TextStyle(
                      fontSize: isSmallScreen ? 13 : 14,
                      fontWeight: FontWeight.w700,
                      color: widget.task.isOverdue
                          ? MaritimeColors.overdue
                          : widget.task.isDueSoon
                              ? MaritimeColors.inProgress
                              : MaritimeColors.completed,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        
        // Row 3: Last Done + Running Hours (if available)
        if (widget.task.lastDoneAt != null || widget.task.runningHoursAtLastDone != null) ...[
          SizedBox(height: spacing),
          Row(
            children: [
              if (widget.task.lastDoneAt != null)
                Expanded(
                  child: _buildInfoItem(
                    label: l10n.lastDone,
                    value: dateFormat.format(DateTime.parse(widget.task.lastDoneAt!)),
                    isSmallScreen: isSmallScreen,
                  ),
                ),
              if (widget.task.lastDoneAt != null && widget.task.runningHoursAtLastDone != null)
                SizedBox(width: spacing),
              if (widget.task.runningHoursAtLastDone != null)
                Expanded(
                  child: _buildInfoItem(
                    label: l10n.runningHours,
                    value: l10n.hoursValue(widget.task.runningHoursAtLastDone!.toInt()),
                    isSmallScreen: isSmallScreen,
                  ),
                ),
            ],
          ),
        ],
        
        // Completion Info (if completed) - Maritime green theme
        if (widget.task.isCompleted) ...[
          SizedBox(height: spacing),
          Container(
            padding: EdgeInsets.all(isSmallScreen ? 10 : 12),
            decoration: BoxDecoration(
              color: MaritimeColors.completed.withOpacity(0.08),
              borderRadius: BorderRadius.circular(4),
              border: Border.all(color: MaritimeColors.completed.withOpacity(0.3)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Container(
                      width: isSmallScreen ? 18 : 20,
                      height: isSmallScreen ? 18 : 20,
                      decoration: const BoxDecoration(
                        color: MaritimeColors.completed,
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.check,
                        size: isSmallScreen ? 11 : 12,
                        color: Colors.white,
                      ),
                    ),
                    SizedBox(width: isSmallScreen ? 6 : 8),
                    Text(
                      l10n.completionDetails,
                      style: TextStyle(
                        fontSize: isSmallScreen ? 11 : 12,
                        fontWeight: FontWeight.w700,
                        color: MaritimeColors.completed,
                      ),
                    ),
                  ],
                ),
                if (widget.task.completedBy != null || 
                    widget.task.completedAt != null ||
                    widget.task.runningHoursAtCompletion != null) ...[
                  SizedBox(height: isSmallScreen ? 6 : 8),
                  if (widget.task.completedBy != null)
                    _buildCompactInfoRow(
                      l10n.completedBy,
                      widget.task.completedBy!,
                      isSmallScreen,
                    ),
                  if (widget.task.completedAt != null)
                    _buildCompactInfoRow(
                      l10n.completedAt,
                      dateFormat.format(DateTime.parse(widget.task.completedAt!)),
                      isSmallScreen,
                    ),
                  if (widget.task.runningHoursAtCompletion != null)
                    _buildCompactInfoRow(
                      l10n.runningHours,
                      l10n.hoursValue(widget.task.runningHoursAtCompletion!.toInt()),
                      isSmallScreen,
                    ),
                  if (widget.task.sparePartsUsed != null)
                    _buildCompactInfoRow(
                      l10n.spareParts,
                      widget.task.sparePartsUsed!,
                      isSmallScreen,
                    ),
                ],
                if (widget.task.notes != null && widget.task.notes!.isNotEmpty) ...[
                  SizedBox(height: isSmallScreen ? 6 : 8),
                  Text(
                    '${l10n.notes}:',
                    style: TextStyle(
                      fontSize: isSmallScreen ? 10 : 11,
                      fontWeight: FontWeight.w600,
                      color: Colors.grey.shade700,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    widget.task.notes!,
                    style: TextStyle(
                      fontSize: isSmallScreen ? 10 : 11,
                      color: Colors.grey.shade800,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildInfoItem({
    required String label,
    required String value,
    required bool isSmallScreen,
    Color? valueColor,
    Color? color,  // Add color parameter for subtle theme variations
    bool fullWidth = false,
  }) {
    final fontSize = isSmallScreen ? 10.0 : 11.0;
    final valueFontSize = isSmallScreen ? 12.0 : 13.0;
    final padding = isSmallScreen ? 8.0 : 10.0;
    
    return Container(
      padding: EdgeInsets.all(padding),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(8),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            (color ?? Theme.of(context).primaryColor).withOpacity(0.08),
            (color ?? Theme.of(context).primaryColor).withOpacity(0.03),
          ],
        ),
        border: Border.all(
          color: (color ?? Theme.of(context).primaryColor).withOpacity(0.15),
          width: 1,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: fontSize,
              color: Colors.grey.shade700,
              fontWeight: FontWeight.w500,
              height: 1.2,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          SizedBox(height: isSmallScreen ? 3 : 4),
          Text(
            value,
            style: TextStyle(
              fontSize: valueFontSize,
              fontWeight: FontWeight.w700,
              color: valueColor ?? Colors.grey.shade900,
              height: 1.2,
            ),
            maxLines: fullWidth ? 2 : 1,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
    );
  }

  Widget _buildCompactInfoRow(String label, String value, bool isSmallScreen) {
    return Padding(
      padding: EdgeInsets.only(bottom: isSmallScreen ? 3 : 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: isSmallScreen ? 90 : 100,
            child: Text(
              label,
              style: TextStyle(
                fontSize: isSmallScreen ? 10 : 11,
                color: Colors.grey.shade600,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: TextStyle(
                fontSize: isSmallScreen ? 10 : 11,
                fontWeight: FontWeight.w600,
                color: Colors.black87,
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
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: MaritimeColors.textSecondary,
              ),
            ),
            Text(
              '${(progress * 100).toStringAsFixed(0)}%',
              style: TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w700,
                color: progress == 1.0 
                    ? MaritimeColors.completed 
                    : MaritimeColors.primary,
              ),
            ),
          ],
        ),
        const SizedBox(height: 6),
        ClipRRect(
          borderRadius: BorderRadius.circular(2),
          child: LinearProgressIndicator(
            value: progress,
            backgroundColor: MaritimeColors.surface,
            valueColor: AlwaysStoppedAnimation<Color>(
              progress == 1.0 
                  ? MaritimeColors.completed 
                  : MaritimeColors.primary,
            ),
            minHeight: 6,
          ),
        ),
      ],
    );
  }

  Widget _buildChecklistItem(TaskChecklistItem item, int index) {
    final l10n = AppLocalizations.of(context);
    final detail = item.taskDetail;
    final isCompleted = item.isCompleted;
    
    return Card(
      elevation: 2,
      margin: const EdgeInsets.only(bottom: 8),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () {
          if (widget.task.isInProgress || isCompleted) {
            // Open dialog for pending items OR completed items (allow editing)
            _showQuickChecklistDialog(item, index);
          }
        },
        child: Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(12),
            gradient: isCompleted 
              ? LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [
                    Colors.green.withOpacity(0.08),
                    Colors.green.withOpacity(0.03),
                  ],
                )
              : null,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header row with number, title, and icon
              Row(
                children: [
                  // Number circle - Modern style
                  Container(
                    width: 32,
                    height: 32,
                    decoration: BoxDecoration(
                      color: isCompleted 
                          ? Colors.green 
                          : Colors.blue.withOpacity(0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Center(
                      child: isCompleted
                          ? const Icon(
                              Icons.check,
                              color: Colors.white,
                              size: 18,
                            )
                          : Text(
                              '$index',
                              style: TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.bold,
                                color: Colors.blue.shade700,
                              ),
                            ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  
                  // Title and badges
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          detail.detailName,
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: isCompleted 
                                ? Colors.grey.shade600
                                : Colors.grey.shade900,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            _buildDetailTypeBadge(detail.detailType),
                            if (detail.isMandatory) ...[
                              const SizedBox(width: 5),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 6,
                                  vertical: 2,
                                ),
                                decoration: BoxDecoration(
                                  color: Colors.red,
                                  borderRadius: BorderRadius.circular(4),
                                ),
                                child: Text(
                                  l10n.mandatory.toUpperCase(),
                                  style: const TextStyle(
                                    fontSize: 9,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.white,
                                    letterSpacing: 0.3,
                                  ),
                                ),
                              ),
                            ],
                            if (detail.detailType == 'MEASUREMENT' && 
                                detail.unit != null) ...[
                              const SizedBox(width: 5),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 6,
                                  vertical: 2,
                                ),
                                decoration: BoxDecoration(
                                  color: Colors.blue.withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(4),
                                ),
                                child: Text(
                                  detail.unit!,
                                  style: TextStyle(
                                    fontSize: 9,
                                    fontWeight: FontWeight.w600,
                                    color: Colors.blue.shade700,
                                  ),
                                ),
                              ),
                            ],
                          ],
                        ),
                      ],
                    ),
                  ),
                  
                  // Action icon - Clean modern
                  Icon(
                    isCompleted
                        ? Icons.edit_outlined
                        : Icons.chevron_right,
                    color: isCompleted 
                        ? Colors.green
                        : Colors.grey.shade400,
                    size: 20,
                  ),
                ],
              ),
              
              // Description if exists
              if (detail.description != null && 
                  detail.description!.isNotEmpty) ...[
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade50,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Icon(
                        Icons.info_outline,
                        size: 14,
                        color: Colors.grey.shade500,
                      ),
                      const SizedBox(width: 6),
                      Expanded(
                        child: Text(
                          detail.description!,
                          style: TextStyle(
                            fontSize: 11,
                            color: Colors.grey.shade700,
                            height: 1.4,
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
      ),
    );
  }

  Widget _buildDetailTypeBadge(String type) {
    Color color;
    String label;

    final l10n = AppLocalizations.of(context);

    switch (type) {
      case 'MEASUREMENT':
        color = Colors.blue;
        label = l10n.measurement;
        break;
      case 'CHECKLIST':
        color = Colors.green;
        label = l10n.checklist;
        break;
      case 'INSPECTION':
        color = Colors.orange;
        label = l10n.inspection;
        break;
      default:
        color = Colors.grey;
        label = type;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        label.toUpperCase(),
        style: TextStyle(
          fontSize: 9,
          fontWeight: FontWeight.w600,
          color: color,
          letterSpacing: 0.3,
        ),
      ),
    );
  }

  void _showQuickChecklistDialog(TaskChecklistItem item, int index) {
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
    bool checkResult = item.executionDetail?.checkResult ?? true; // Default to OK

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) {
          return Container(
            decoration: const BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.only(
                topLeft: Radius.circular(16),
                topRight: Radius.circular(16),
              ),
            ),
            padding: EdgeInsets.only(
              bottom: MediaQuery.of(context).viewInsets.bottom,
            ),
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Header with drag handle
                    Center(
                      child: Container(
                        width: 36,
                        height: 4,
                        decoration: BoxDecoration(
                          color: MaritimeColors.border,
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                    ),
                    const SizedBox(height: 18),

                    // Title with number
                    Row(
                      children: [
                        Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: MaritimeColors.primary.withOpacity(0.15),
                            shape: BoxShape.circle,
                            border: Border.all(
                              color: MaritimeColors.primary.withOpacity(0.3),
                              width: 1.5,
                            ),
                          ),
                          child: Center(
                            child: Text(
                              '$index',
                              style: const TextStyle(
                                fontSize: 17,
                                fontWeight: FontWeight.bold,
                                color: MaritimeColors.primary,
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                detail.detailName,
                                style: const TextStyle(
                                  fontSize: 17,
                                  fontWeight: FontWeight.bold,
                                  color: MaritimeColors.textPrimary,
                                ),
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  _buildDetailTypeBadge(detail.detailType),
                                  if (detail.isMandatory) ...[
                                    const SizedBox(width: 6),
                                    Container(
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 7,
                                        vertical: 3,
                                      ),
                                      decoration: BoxDecoration(
                                        color: MaritimeColors.mandatory,
                                        borderRadius: BorderRadius.circular(3),
                                      ),
                                      child: Text(
                                        l10n.mandatory.toUpperCase(),
                                        style: const TextStyle(
                                          fontSize: 9,
                                          fontWeight: FontWeight.w700,
                                          color: Colors.white,
                                          letterSpacing: 0.3,
                                        ),
                                      ),
                                    ),
                                  ],
                                ],
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),

                    if (detail.description != null && detail.description!.isNotEmpty) ...[
                      const SizedBox(height: 14),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: MaritimeColors.primaryLight.withOpacity(0.08),
                          borderRadius: BorderRadius.circular(6),
                          border: Border.all(
                            color: MaritimeColors.primaryLight.withOpacity(0.25),
                            width: 1,
                          ),
                        ),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Icon(
                              Icons.info_outline, 
                              size: 16, 
                              color: MaritimeColors.primaryLight,
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Text(
                                detail.description!,
                                style: const TextStyle(
                                  fontSize: 13,
                                  color: MaritimeColors.textSecondary,
                                  height: 1.3,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],

                    const SizedBox(height: 24),

                    // ===== INPUT BASED ON TYPE =====
                    
                    // CHECKLIST TYPE: Big OK/NG buttons
                    if (detail.detailType == 'CHECKLIST') ...[
                      Text(
                        l10n.checkResult.toUpperCase(),
                        style: const TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          letterSpacing: 0.5,
                          color: MaritimeColors.textTertiary,
                        ),
                      ),
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          Expanded(
                            child: _buildCheckButton(
                              context: context,
                              label: l10n.okPass,
                              icon: Icons.check_circle,
                              isSelected: checkResult == true,
                              color: MaritimeColors.completed, // Forest Green
                              onTap: () {
                                setDialogState(() {
                                  checkResult = true;
                                });
                              },
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: _buildCheckButton(
                              context: context,
                              label: l10n.ngFail,
                              icon: Icons.cancel,
                              isSelected: checkResult == false,
                              color: MaritimeColors.mandatory, // Deep Red
                              onTap: () {
                                setDialogState(() {
                                  checkResult = false;
                                });
                              },
                            ),
                          ),
                        ],
                      ),
                    ],

                    // MEASUREMENT TYPE: Big number input
                    if (detail.detailType == 'MEASUREMENT') ...[
                      Text(
                        l10n.measuredValue.toUpperCase(),
                        style: const TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          letterSpacing: 0.5,
                          color: MaritimeColors.textTertiary,
                        ),
                      ),
                      const SizedBox(height: 10),
                      TextField(
                        controller: measurementController,
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        autofocus: true,
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w600,
                          color: MaritimeColors.textPrimary,
                          letterSpacing: 0.5,
                        ),
                        decoration: InputDecoration(
                          hintText: '_ _ . _',
                          hintStyle: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.w400,
                            color: MaritimeColors.textTertiary.withOpacity(0.3),
                            letterSpacing: 2,
                          ),
                          suffix: Padding(
                            padding: const EdgeInsets.only(left: 8),
                            child: Text(
                              detail.unit ?? '',
                              style: const TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.w600,
                                color: MaritimeColors.textTertiary,
                                letterSpacing: 0.3,
                              ),
                            ),
                          ),
                          filled: true,
                          fillColor: MaritimeColors.surface,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: MaritimeColors.primary,
                              width: 2,
                            ),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 14,
                            vertical: 12,
                          ),
                        ),
                      ),
                      if (detail.minValue != null || detail.maxValue != null) ...[
                        const SizedBox(height: 10),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                          decoration: BoxDecoration(
                            color: const Color(0xFFFFF4E6),
                            borderRadius: BorderRadius.circular(6),
                            border: Border.all(
                              color: const Color(0xFFFFD699),
                              width: 1,
                            ),
                          ),
                          child: Row(
                            children: [
                              const Icon(
                                Icons.info_outline, 
                                size: 16, 
                                color: Color(0xFF996600),
                              ),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  l10n.limitRange(
                                    detail.minValue?.toString() ?? '?',
                                    detail.maxValue?.toString() ?? '?',
                                    detail.unit ?? '',
                                  ),
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.w600,
                                    color: Color(0xFF663D00),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ],

                    // INSPECTION TYPE: Notes field
                    if (detail.detailType == 'INSPECTION') ...[
                      Text(
                        l10n.observationNotes.toUpperCase(),
                        style: const TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          letterSpacing: 0.5,
                          color: MaritimeColors.textTertiary,
                        ),
                      ),
                      const SizedBox(height: 10),
                      TextField(
                        controller: notesController,
                        maxLines: 4,
                        autofocus: true,
                        style: const TextStyle(
                          fontSize: 14,
                          color: MaritimeColors.textPrimary,
                          height: 1.4,
                        ),
                        decoration: InputDecoration(
                          hintText: l10n.enterDetailedNotes,
                          hintStyle: TextStyle(
                            fontSize: 13,
                            color: MaritimeColors.textTertiary.withOpacity(0.4),
                          ),
                          filled: true,
                          fillColor: MaritimeColors.surface,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: MaritimeColors.primary,
                              width: 2,
                            ),
                          ),
                          contentPadding: const EdgeInsets.all(14),
                        ),
                      ),
                    ],

                    // Optional notes for CHECKLIST and MEASUREMENT
                    if (detail.detailType != 'INSPECTION') ...[
                      const SizedBox(height: 18),
                      Text(
                        l10n.notesOptional.toUpperCase(),
                        style: const TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          letterSpacing: 0.5,
                          color: MaritimeColors.textTertiary,
                        ),
                      ),
                      const SizedBox(height: 10),
                      TextField(
                        controller: notesController,
                        maxLines: 2,
                        decoration: InputDecoration(
                          hintText: l10n.addNotesIfNeeded,
                          hintStyle: TextStyle(
                            fontSize: 13,
                            color: MaritimeColors.textTertiary.withOpacity(0.4),
                          ),
                          filled: true,
                          fillColor: MaritimeColors.surface,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: BorderSide(
                              color: MaritimeColors.border,
                              width: 1,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: MaritimeColors.primary,
                              width: 1.5,
                            ),
                          ),
                          contentPadding: const EdgeInsets.all(12),
                        ),
                      ),
                    ],

                    if (isAlreadyCompleted) ...[
                      const SizedBox(height: 16),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                        decoration: BoxDecoration(
                          color: const Color(0xFFFFF9E6),
                          borderRadius: BorderRadius.circular(6),
                          border: Border.all(
                            color: const Color(0xFFFFE699),
                            width: 1,
                          ),
                        ),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.edit_outlined, 
                              color: Color(0xFF996600), 
                              size: 18,
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Text(
                                l10n.alreadyCompletedCanUpdate,
                                style: const TextStyle(
                                  fontSize: 12,
                                  fontWeight: FontWeight.w600,
                                  color: Color(0xFF663D00),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],

                    const SizedBox(height: 24),

                    // Action buttons
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton(
                            onPressed: () => Navigator.pop(context),
                            style: OutlinedButton.styleFrom(
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              side: BorderSide(
                                color: MaritimeColors.border,
                                width: 1.5,
                              ),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8),
                              ),
                            ),
                            child: Text(
                              l10n.cancel,
                              style: const TextStyle(
                                fontSize: 15,
                                fontWeight: FontWeight.w600,
                                color: MaritimeColors.textSecondary,
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          flex: 2,
                          child: ElevatedButton(
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
                              }

                              if (detail.detailType == 'INSPECTION' && 
                                  notesController.text.trim().isEmpty) {
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
                                final taskProvider = Provider.of<TaskProvider>(
                                  context, 
                                  listen: false,
                                );
                                
                                await taskProvider.completeChecklistItem(
                                  taskId: widget.task.id,
                                  detailId: detail.id,
                                  measuredValue: detail.detailType == 'MEASUREMENT'
                                      ? measurementController.text.trim()
                                      : null,
                                  checkResult: detail.detailType == 'CHECKLIST' 
                                      ? checkResult 
                                      : null,
                                  inspectionNotes: notesController.text.trim().isNotEmpty
                                      ? notesController.text.trim()
                                      : null,
                                  photoUrl: null,
                                  isCompleted: true,
                                );

                                if (context.mounted) {
                                  Navigator.pop(context);
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(
                                      content: Row(
                                        children: [
                                          const Icon(Icons.check_circle, color: Colors.white),
                                          const SizedBox(width: 8),
                                          Text(l10n.savedItem(detail.detailName)),
                                        ],
                                      ),
                                      backgroundColor: Colors.green,
                                      behavior: SnackBarBehavior.floating,
                                    ),
                                  );
                                  
                                  // Reload checklist
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
                            style: ElevatedButton.styleFrom(
                              backgroundColor: MaritimeColors.primary,
                              foregroundColor: Colors.white,
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              elevation: 0,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8),
                              ),
                            ),
                            child: Text(
                              isAlreadyCompleted ? l10n.update : l10n.complete,
                              style: const TextStyle(
                                fontSize: 15,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                    
                    const SizedBox(height: 8),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildCheckButton({
    required BuildContext context,
    required String label,
    required IconData icon,
    required bool isSelected,
    required Color color,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(6),
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
        decoration: BoxDecoration(
          color: isSelected ? color.withOpacity(0.08) : Colors.white,
          borderRadius: BorderRadius.circular(6),
          border: Border.all(
            color: isSelected ? color : MaritimeColors.border,
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected ? [
            BoxShadow(
              color: color.withOpacity(0.15),
              blurRadius: 4,
              offset: const Offset(0, 1),
            ),
          ] : null,
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon,
              size: 20,
              color: isSelected ? color : MaritimeColors.textTertiary,
            ),
            const SizedBox(width: 8),
            Text(
              label,
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: isSelected ? color : MaritimeColors.textSecondary,
                letterSpacing: 0.2,
              ),
            ),
          ],
        ),
      ),
    );
  }

  // New Bottom Action Bar - Professional Maritime Design
  Widget? _buildBottomActionBar(BuildContext context, TaskProvider taskProvider) {
    final l10n = AppLocalizations.of(context);
    
    // Don't show action bar for completed tasks
    if (widget.task.isCompleted) {
      return null;
    }

    // Show Start button for PENDING or OVERDUE tasks
    // Show Complete button for IN_PROGRESS tasks
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 12,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          child: widget.task.canStart
              ? _buildStartTaskButton(context, taskProvider, l10n)
              : widget.task.isInProgress
                  ? _buildCompleteTaskButton(context, l10n)
                  : const SizedBox.shrink(),
        ),
      ),
    );
  }

  Widget _buildStartTaskButton(
    BuildContext context,
    TaskProvider taskProvider,
    AppLocalizations l10n,
  ) {
    return SizedBox(
      width: double.infinity,
      height: 48,
      child: ElevatedButton.icon(
        onPressed: taskProvider.isLoading
            ? null
            : () async {
                // Show warning if task is overdue (by status or by days)
                if (widget.task.isOverdueStatus || widget.task.isOverdue) {
                  final confirmed = await showDialog<bool>(
                    context: context,
                    builder: (context) => AlertDialog(
                      icon: Icon(
                        Icons.warning_rounded,
                        color: MaritimeColors.overdue,
                        size: 44,
                      ),
                      title: Text(l10n.taskOverdue),
                      content: Text(l10n.thisTaskIsOverdue),
                      actions: [
                        TextButton(
                          onPressed: () => Navigator.pop(context, false),
                          child: Text(l10n.cancel),
                        ),
                        ElevatedButton(
                          onPressed: () => Navigator.pop(context, true),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: MaritimeColors.overdue,
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
                        backgroundColor: widget.task.isOverdue 
                            ? MaritimeColors.inProgress 
                            : MaritimeColors.completed,
                      ),
                    );
                    Navigator.pop(context);
                  }
                } catch (e) {
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(l10n.failedToStartTask(e.toString())),
                        backgroundColor: MaritimeColors.overdue,
                      ),
                    );
                  }
                }
              },
        icon: const Icon(Icons.play_arrow_rounded, size: 22),
        label: Text(
          l10n.startTask,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w600,
          ),
        ),
        style: ElevatedButton.styleFrom(
          backgroundColor: widget.task.isOverdueStatus || widget.task.isOverdue
              ? Colors.red 
              : Theme.of(context).primaryColor,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          elevation: 2,
        ),
      ),
    );
  }

  Widget _buildCompleteTaskButton(BuildContext context, AppLocalizations l10n) {
    return SizedBox(
      width: double.infinity,
      height: 48,
      child: ElevatedButton.icon(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => CompleteTaskScreen(task: widget.task),
            ),
          );
        },
        icon: const Icon(Icons.check_circle_rounded, size: 22),
        label: Text(
          l10n.completeTask,
          style: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w600,
          ),
        ),
        style: ElevatedButton.styleFrom(
          backgroundColor: Colors.green,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          elevation: 2,
        ),
      ),
    );
  }
}
