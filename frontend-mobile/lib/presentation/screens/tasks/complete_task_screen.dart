import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';
import 'dart:convert';
import 'dart:typed_data';
import '../../../data/models/maintenance_task.dart';
import '../../../data/models/task_checklist_item.dart';
import '../../providers/task_provider.dart';
import '../../providers/sync_provider.dart';
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
  
  // Photo Upload State
  final List<String> _photoUrls = [];
  bool _isUploadingPhoto = false;
  final ImagePicker _picker = ImagePicker();

  bool _isSubmitting = false;
  
  // Checklist state - local copy that can be toggled
  late List<TaskChecklistItem> _checklistItems;
  bool _isTogglingChecklist = false;
  
  // Spare parts usage state - maps materialItemId to quantity used
  final Map<String, TextEditingController> _sparePartsUsageControllers = {};
  List<Map<String, dynamic>> _requiredSpareParts = [];

  Future<void> _pickImage(ImageSource source) async {
    try {
      setState(() => _isUploadingPhoto = true);
      
      // Giảm kích thước và chất lượng ảnh để tránh crash
      final XFile? image = await _picker.pickImage(
        source: source,
        imageQuality: 30, // Giảm chất lượng xuống 30%
        maxWidth: 640,    // Giảm kích thước tối đa
        maxHeight: 480,
      );

      if (image != null) {
        // Convert image to base64 for sending to server
        final File imageFile = File(image.path);
        final Uint8List imageBytes = await imageFile.readAsBytes();
        
        // Kiểm tra kích thước - bỏ qua nếu > 500KB
        if (imageBytes.length > 500 * 1024) {
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Ảnh quá lớn, vui lòng chọn ảnh khác')),
            );
          }
          return;
        }
        
        final String base64Image = base64Encode(imageBytes);
        
        // Determine mime type from file extension
        final String extension = image.path.split('.').last.toLowerCase();
        final String mimeType = extension == 'png' ? 'image/png' : 'image/jpeg';
        
        // Create data URL format
        final String dataUrl = 'data:$mimeType;base64,$base64Image';
        
        setState(() {
          _photoUrls.add(dataUrl);
        });
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error picking image: $e')),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isUploadingPhoto = false);
      }
    }
  }

  void _showImageSourceActionSheet() {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.photo_camera),
              title: const Text('Chụp ảnh'),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.camera);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: const Text('Chọn từ thư viện'),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.gallery);
              },
            ),
          ],
        ),
      ),
    );
  }

  @override
  void initState() {
    super.initState();
    // Pre-fill running hours if available
    if (widget.task.runningHoursAtLastDone != null) {
      _runningHoursController.text = widget.task.runningHoursAtLastDone.toString();
    }
    // Initialize local checklist state from task
    _checklistItems = List.from(widget.task.checklistItems);
    
    // Parse required spare parts and create controllers for each
    _initializeSparePartsControllers();
  }
  
  void _initializeSparePartsControllers() {
    try {
      if (widget.task.sparePartsUsed != null && widget.task.sparePartsUsed!.startsWith('[')) {
        final parsed = json.decode(widget.task.sparePartsUsed!);
        if (parsed is List) {
          _requiredSpareParts = List<Map<String, dynamic>>.from(
            parsed.map((item) => Map<String, dynamic>.from(item))
          );
          
          // Create a TextEditingController for each spare part
          for (var part in _requiredSpareParts) {
            final materialItemId = part['materialItemId']?.toString() ?? '';
            if (materialItemId.isNotEmpty) {
              _sparePartsUsageControllers[materialItemId] = TextEditingController(
                text: '0', // Default to 0
              );
            }
          }
        }
      }
    } catch (e) {
      debugPrint('Error parsing spare parts: $e');
    }
  }

  @override
  void dispose() {
    _runningHoursController.dispose();
    _sparePartsController.dispose();
    _notesController.dispose();
    // Dispose all spare parts controllers
    for (var controller in _sparePartsUsageControllers.values) {
      controller.dispose();
    }
    // Clear cached photo bytes
    _cachedPhotoBytes.clear();
    super.dispose();
  }

  // Cache để tránh decode base64 nhiều lần
  final Map<int, Uint8List> _cachedPhotoBytes = {};

  Widget _buildPhotoThumbnail(int index) {
    final path = _photoUrls[index];
    final isBase64 = path.startsWith('data:image');
    
    Widget imageWidget;
    
    if (isBase64) {
      // Sử dụng cache để tránh decode lại
      if (!_cachedPhotoBytes.containsKey(index)) {
        try {
          final base64Data = path.split(',').last;
          _cachedPhotoBytes[index] = base64Decode(base64Data);
        } catch (e) {
          _cachedPhotoBytes[index] = Uint8List(0);
        }
      }
      
      final bytes = _cachedPhotoBytes[index]!;
      if (bytes.isEmpty) {
        imageWidget = const Center(
          child: Icon(Icons.broken_image, color: Colors.grey),
        );
      } else {
        imageWidget = Image.memory(
          bytes,
          fit: BoxFit.cover,
          width: 100,
          height: 100,
          cacheWidth: 100, // Giảm memory bằng cách resize
          cacheHeight: 100,
          gaplessPlayback: true,
          errorBuilder: (_, __, ___) => const Center(
            child: Icon(Icons.broken_image, color: Colors.grey),
          ),
        );
      }
    } else {
      // File path - hiếm khi xảy ra
      imageWidget = Image.file(
        File(path),
        fit: BoxFit.cover,
        width: 100,
        height: 100,
        cacheWidth: 100,
        cacheHeight: 100,
        errorBuilder: (_, __, ___) => const Center(
          child: Icon(Icons.broken_image, color: Colors.grey),
        ),
      );
    }
    
    return Stack(
      children: [
        Container(
          width: 100,
          height: 100,
          margin: const EdgeInsets.only(right: 8),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: Colors.grey.shade300),
            color: Colors.grey.shade100,
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: imageWidget,
          ),
        ),
        Positioned(
          top: 4,
          right: 12,
          child: InkWell(
            onTap: () {
              setState(() {
                _photoUrls.removeAt(index);
                _cachedPhotoBytes.remove(index);
                // Re-index cached photos
                final newCache = <int, Uint8List>{};
                _cachedPhotoBytes.forEach((key, value) {
                  if (key > index) {
                    newCache[key - 1] = value;
                  } else {
                    newCache[key] = value;
                  }
                });
                _cachedPhotoBytes.clear();
                _cachedPhotoBytes.addAll(newCache);
              });
            },
            child: Container(
              padding: const EdgeInsets.all(4),
              decoration: const BoxDecoration(
                color: Colors.red,
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.close, size: 12, color: Colors.white),
            ),
          ),
        ),
      ],
    );
  }

  Future<void> _submitCompletion() async {
    final l10n = AppLocalizations.of(context);
    if (!_formKey.currentState!.validate()) {
      return;
    }

    // Validate photos
    if (widget.task.requiredPhotos > 0 && _photoUrls.length < widget.task.requiredPhotos) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Yêu cầu tối thiểu ${widget.task.requiredPhotos} ảnh. Hiện có: ${_photoUrls.length}'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      final syncProvider = Provider.of<SyncProvider>(context, listen: false);

        // Build spare parts usage JSON from input controllers
        final sparePartsUsageList = _buildSparePartsUsageJson();
        final sparePartsJson = sparePartsUsageList.isNotEmpty 
            ? json.encode(sparePartsUsageList)
            : _sparePartsController.text.trim().isEmpty ? null : _sparePartsController.text.trim();

        // Backend uses headers for user identity; body carries workflow fields only.
        await taskProvider.submitTask(
        taskId: widget.task.id,
        runningHours: double.tryParse(_runningHoursController.text),
        sparePartsUsed: sparePartsJson,
        notes: _notesController.text.trim().isEmpty
            ? null
            : _notesController.text.trim(),
        photoUrls: _photoUrls.isNotEmpty ? _photoUrls : null,
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
                              widget.task.displayName,
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

                    // Checklist Progress Section
                    if (_checklistItems.isNotEmpty) ...[
                      _buildChecklistSection(context, l10n),
                      const SizedBox(height: 24),
                    ],

                    // Required Spare Parts Section (from schedule config)
                    if (widget.task.sparePartsUsed != null && widget.task.sparePartsUsed!.isNotEmpty) ...[
                      _buildRequiredSparePartsSection(context),
                      const SizedBox(height: 24),
                    ],

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

                    // Photos Section
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Hình ảnh báo cáo',
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                        if (widget.task.requiredPhotos > 0)
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                            decoration: BoxDecoration(
                              color: _photoUrls.length >= widget.task.requiredPhotos
                                  ? Colors.green.shade50
                                  : Colors.orange.shade50,
                              borderRadius: BorderRadius.circular(4),
                              border: Border.all(
                                color: _photoUrls.length >= widget.task.requiredPhotos
                                    ? Colors.green
                                    : Colors.orange,
                              ),
                            ),
                            child: Text(
                              '${_photoUrls.length}/${widget.task.requiredPhotos} Required',
                              style: TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.bold,
                                color: _photoUrls.length >= widget.task.requiredPhotos
                                    ? Colors.green.shade700
                                    : Colors.orange.shade700,
                              ),
                            ),
                          ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    
                    // Photo Grid
                    if (_photoUrls.isNotEmpty)
                      Container(
                        height: 100,
                        margin: const EdgeInsets.only(bottom: 12),
                        child: ListView.builder(
                          scrollDirection: Axis.horizontal,
                          itemCount: _photoUrls.length,
                          cacheExtent: 100, // Giới hạn cache để giảm memory
                          itemBuilder: (context, index) {
                            return _buildPhotoThumbnail(index);
                          },
                        ),
                      ),

                    // Add Photo Button
                    SizedBox(
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        onPressed: (_isUploadingPhoto || _photoUrls.length >= 5)
                            ? null
                            : _showImageSourceActionSheet,
                        icon: _isUploadingPhoto 
                            ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                            : const Icon(Icons.add_a_photo),
                        label: Text(_photoUrls.length >= 5 ? 'Đã đạt tối đa 5 ảnh' : 'Chụp ảnh / Tải lên'),
                        style: OutlinedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          side: BorderSide(color: _photoUrls.length >= 5 ? Colors.grey : Theme.of(context).primaryColor),
                        ),
                      ),
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

  /// Build Checklist Progress Section
  Widget _buildChecklistSection(BuildContext context, AppLocalizations l10n) {
    final items = _checklistItems;
    final completedCount = items.where((item) => item.isCompleted).length;
    final progress = items.isEmpty ? 0.0 : completedCount / items.length;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Checklist Progress',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: progress == 1.0 ? Colors.green.shade100 : Colors.orange.shade100,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    '$completedCount/${items.length}',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      color: progress == 1.0 ? Colors.green.shade700 : Colors.orange.shade700,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            // Progress bar
            ClipRRect(
              borderRadius: BorderRadius.circular(4),
              child: LinearProgressIndicator(
                value: progress,
                minHeight: 8,
                backgroundColor: Colors.grey.shade200,
                valueColor: AlwaysStoppedAnimation(
                  progress == 1.0 ? Colors.green : Colors.orange,
                ),
              ),
            ),
            const SizedBox(height: 16),
            // Checklist items - now interactive
            ...items.asMap().entries.map((entry) => _buildChecklistItem(entry.value, entry.key)),
          ],
        ),
      ),
    );
  }

  Future<void> _toggleChecklistItem(int index) async {
    if (_isTogglingChecklist) return;
    
    final item = _checklistItems[index];
    
    // If item requires reading and not completed, show dialog to enter reading
    if (item.requiresReading && !item.isCompleted) {
      _showReadingDialog(index);
      return;
    }
    
    setState(() => _isTogglingChecklist = true);
    
    try {
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      
      // Call API to toggle checklist item
      await taskProvider.completeChecklistItem(
        taskCode: widget.task.taskId,
        itemId: item.id,
        readingValue: item.readingValue,
      );
      
      // Update local state
      setState(() {
        _checklistItems[index] = item.copyWith(isCompleted: !item.isCompleted);
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Lỗi: ${e.toString()}'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isTogglingChecklist = false);
      }
    }
  }

  void _showReadingDialog(int index) {
    final item = _checklistItems[index];
    final readingController = TextEditingController(text: item.readingValue?.toString() ?? '');
    
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(item.assetName),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (item.checkpointDescription != null)
              Text(
                item.checkpointDescription!,
                style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
              ),
            const SizedBox(height: 16),
            TextField(
              controller: readingController,
              decoration: InputDecoration(
                labelText: 'Giá trị đo',
                suffixText: item.unit ?? '',
                border: const OutlineInputBorder(),
                hintText: item.minValue != null && item.maxValue != null
                    ? 'Phạm vi: ${item.minValue} - ${item.maxValue}'
                    : null,
              ),
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              await _submitReadingValue(index, double.tryParse(readingController.text));
            },
            child: const Text('Xác nhận'),
          ),
        ],
      ),
    );
  }

  Future<void> _submitReadingValue(int index, double? readingValue) async {
    if (readingValue == null) return;
    
    setState(() => _isTogglingChecklist = true);
    
    try {
      final item = _checklistItems[index];
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      
      // Check if reading is within acceptable range
      bool isAbnormal = false;
      if (item.minValue != null && readingValue < item.minValue!) {
        isAbnormal = true;
      }
      if (item.maxValue != null && readingValue > item.maxValue!) {
        isAbnormal = true;
      }
      
      // Call API
      await taskProvider.completeChecklistItem(
        taskCode: widget.task.taskId,
        itemId: item.id,
        readingValue: readingValue,
        remarks: isAbnormal ? 'Giá trị ngoài phạm vi cho phép' : null,
        isAbnormal: isAbnormal,
      );
      
      // Update local state
      setState(() {
        _checklistItems[index] = item.copyWith(
          isCompleted: true,
          readingValue: readingValue,
          isAbnormal: isAbnormal,
        );
      });
      
      if (isAbnormal && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('⚠️ Giá trị $readingValue ngoài phạm vi cho phép (${item.minValue} - ${item.maxValue})'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: ${e.toString()}'), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isTogglingChecklist = false);
      }
    }
  }

  Widget _buildChecklistItem(TaskChecklistItem item, int index) {
    return InkWell(
      onTap: _isTogglingChecklist ? null : () => _toggleChecklistItem(index),
      borderRadius: BorderRadius.circular(8),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 4),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Checkbox icon - tappable
            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              child: Icon(
                item.isCompleted ? Icons.check_circle : Icons.radio_button_unchecked,
                color: item.isCompleted 
                    ? Colors.green 
                    : (item.isMandatory ? Colors.orange : Colors.grey),
                size: 24,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          item.assetName,
                          style: TextStyle(
                            fontWeight: FontWeight.w500,
                            fontSize: 14,
                            decoration: item.isCompleted ? TextDecoration.lineThrough : null,
                            color: item.isCompleted ? Colors.grey : Colors.black87,
                          ),
                        ),
                      ),
                      if (item.isMandatory && !item.isCompleted)
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                          decoration: BoxDecoration(
                            color: Colors.red.shade100,
                            borderRadius: BorderRadius.circular(4),
                          ),
                          child: const Text(
                            'Bắt buộc',
                            style: TextStyle(fontSize: 10, color: Colors.red, fontWeight: FontWeight.bold),
                          ),
                        ),
                    ],
                  ),
                  if (item.checkpointDescription != null && item.checkpointDescription!.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(top: 2),
                      child: Text(
                        item.checkpointDescription!,
                        style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                      ),
                    ),
                  if (item.requiresReading) ...[
                    const SizedBox(height: 4),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: item.isCompleted
                            ? (item.isAbnormal ? Colors.red.shade50 : Colors.blue.shade50)
                            : Colors.grey.shade100,
                        borderRadius: BorderRadius.circular(4),
                        border: Border.all(
                          color: item.isCompleted
                              ? (item.isAbnormal ? Colors.red.shade300 : Colors.blue.shade300)
                              : Colors.grey.shade300,
                        ),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.straighten,
                            size: 14,
                            color: item.isAbnormal ? Colors.red : Colors.blue,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            item.readingValue != null
                                ? '${item.readingValue} ${item.unit ?? ''}'
                                : 'Nhập giá trị đo',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w500,
                              color: item.isAbnormal ? Colors.red : Colors.blue.shade700,
                            ),
                          ),
                          if (item.minValue != null && item.maxValue != null) ...[
                            const SizedBox(width: 8),
                            Text(
                              '(${item.minValue}-${item.maxValue})',
                              style: TextStyle(fontSize: 10, color: Colors.grey.shade600),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// Build Required Spare Parts Section with input fields
  Widget _buildRequiredSparePartsSection(BuildContext context) {
    if (_requiredSpareParts.isEmpty) return const SizedBox.shrink();

    return Card(
      color: Colors.amber.shade50,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.build, color: Colors.amber.shade700, size: 20),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Vật tư sử dụng',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: Colors.amber.shade900,
                        ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),
            Text(
              'Nhập số lượng đã sử dụng - sẽ tự động trừ kho',
              style: TextStyle(
                fontSize: 12,
                color: Colors.amber.shade800,
                fontStyle: FontStyle.italic,
              ),
            ),
            const SizedBox(height: 12),
            ..._requiredSpareParts.map((part) => _buildSparePartInputItem(part)),
          ],
        ),
      ),
    );
  }

  Widget _buildSparePartInputItem(Map<String, dynamic> part) {
    final materialItemId = part['materialItemId']?.toString() ?? 'Unknown';
    final materialName = part['materialName']?.toString() ?? 
                         part['name']?.toString() ?? 
                         'Vật tư không xác định';
    final materialCode = part['materialCode']?.toString() ?? 
                         part['code']?.toString() ?? '';
    final quantityRequired = part['quantityRequired'] ?? 0;
    final isMandatory = part['isMandatory'] ?? false;
    final controller = _sparePartsUsageControllers[materialItemId];

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Icon(
            isMandatory ? Icons.warning_amber : Icons.inventory_2,
            color: isMandatory ? Colors.red : Colors.grey,
            size: 18,
          ),
          const SizedBox(width: 8),
          Expanded(
            flex: 2,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  materialName,
                  style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                if (materialCode.isNotEmpty)
                  Text(
                    'Mã: $materialCode',
                    style: TextStyle(fontSize: 10, color: Colors.grey.shade500),
                  ),
                Text(
                  'Yêu cầu: $quantityRequired',
                  style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
          // Quantity input field
          SizedBox(
            width: 80,
            height: 40,
            child: TextFormField(
              controller: controller,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold),
              decoration: InputDecoration(
                contentPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                  borderSide: BorderSide(color: Colors.amber.shade300),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                  borderSide: BorderSide(color: Colors.amber.shade300),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                  borderSide: BorderSide(color: Colors.amber.shade700, width: 2),
                ),
                filled: true,
                fillColor: Colors.white,
                hintText: '0',
              ),
              inputFormatters: [
                FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d*')),
              ],
              validator: isMandatory
                  ? (value) {
                      final qty = double.tryParse(value ?? '0') ?? 0;
                      if (qty <= 0) {
                        return 'Bắt buộc';
                      }
                      return null;
                    }
                  : null,
            ),
          ),
          if (isMandatory) ...[
            const SizedBox(width: 4),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
              decoration: BoxDecoration(
                color: Colors.red.shade100,
                borderRadius: BorderRadius.circular(4),
              ),
              child: const Text(
                '*',
                style: TextStyle(
                  fontSize: 14,
                  color: Colors.red,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
  
  /// Build JSON array of spare parts usage to send to backend
  List<Map<String, dynamic>> _buildSparePartsUsageJson() {
    final usageList = <Map<String, dynamic>>[];
    
    for (var part in _requiredSpareParts) {
      final materialItemId = part['materialItemId']?.toString() ?? '';
      final controller = _sparePartsUsageControllers[materialItemId];
      final quantityUsed = double.tryParse(controller?.text ?? '0') ?? 0;
      
      if (quantityUsed > 0) {
        usageList.add({
          'materialItemId': materialItemId,
          'quantityUsed': quantityUsed,
          'quantityRequired': part['quantityRequired'] ?? 0,
          'isMandatory': part['isMandatory'] ?? false,
        });
      }
    }
    
    return usageList;
  }

  dynamic _decodeJson(String jsonStr) {
    // Simple JSON decode - in production use dart:convert
    try {
      return json.decode(jsonStr);
    } catch (e) {
      return [];
    }
  }
}
