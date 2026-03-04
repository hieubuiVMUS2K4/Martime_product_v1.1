import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';
import 'dart:convert';
import 'dart:typed_data';
import '../../../data/models/maintenance_task.dart';
import '../../../data/models/task_checklist_item.dart';
import '../../../data/repositories/task_repository.dart';
import '../../providers/task_provider.dart';
import '../../providers/sync_provider.dart';
import '../../widgets/common/loading_widget.dart';
import '../../../l10n/app_localizations.dart';
import '../../../core/cache/cache_manager.dart';
import '../../../core/constants/cache_keys.dart';
import '../../../core/di/service_locator.dart';
import '../../../core/cache/sync_queue.dart';

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
  
  // Spare parts from task requirements (display only)
  List<Map<String, dynamic>> _requiredSpareParts = [];
  
  // Actually used spare parts (selected from inventory)
  List<Map<String, dynamic>> _actuallyUsedSpareParts = [];

  Future<void> _pickImage(ImageSource source) async {
    try {
      setState(() => _isUploadingPhoto = true);
      
      // Production settings: Compress heavily for maritime bandwidth
      // Target: ~150KB per image for fast upload over satellite
      final XFile? image = await _picker.pickImage(
        source: source,
        imageQuality: 35, // Aggressive compression for bandwidth
        maxWidth: 800,    // Smaller dimension for faster transfer
        maxHeight: 600,
      );

      if (image != null) {
        final File imageFile = File(image.path);
        final Uint8List imageBytes = await imageFile.readAsBytes();
        final int imageSize = imageBytes.length;
        
        // Check size - should be under 300KB after compression
        if (imageSize > 300 * 1024) {
          if (mounted) {
            final l10n = AppLocalizations.of(context);
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(l10n.imageStillLarge(_formatBytes(imageSize))),
                backgroundColor: Colors.orange,
                duration: const Duration(seconds: 1),
              ),
            );
          }
          // Try picking again with even more compression
          final XFile? recompressed = await _picker.pickImage(
            source: source,
            imageQuality: 20,
            maxWidth: 640,
            maxHeight: 480,
          );
          if (recompressed != null) {
            final recompressedBytes = await File(recompressed.path).readAsBytes();
            _addImageToList(recompressedBytes, recompressed.path);
            return;
          }
        }
        
        _addImageToList(imageBytes, image.path);
      }
    } catch (e) {
      if (mounted) {
        final l10n = AppLocalizations.of(context);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.errorSelectingPhoto(e.toString()))),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isUploadingPhoto = false);
      }
    }
  }

  void _addImageToList(Uint8List imageBytes, String path) {
    final String base64Image = base64Encode(imageBytes);
    final String extension = path.split('.').last.toLowerCase();
    final String mimeType = extension == 'png' ? 'image/png' : 'image/jpeg';
    final String dataUrl = 'data:$mimeType;base64,$base64Image';
    
    setState(() {
      _photoUrls.add(dataUrl);
    });
    
    if (mounted) {
      final l10n = AppLocalizations.of(context);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(l10n.photoAdded(_photoUrls.length, _formatBytes(imageBytes.length))),
          backgroundColor: Colors.green,
          duration: const Duration(seconds: 1),
        ),
      );
    }
  }

  String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(0)} KB';
    return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
  }

  void _showImageSourceActionSheet() {
    final l10n = AppLocalizations.of(context);
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.photo_camera),
              title: Text(l10n.takePhoto),
              onTap: () {
                Navigator.pop(context);
                _pickImage(ImageSource.camera);
              },
            ),
            ListTile(
              leading: const Icon(Icons.photo_library),
              title: Text(l10n.selectFromGallery),
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
    
    // Parse required spare parts (display only)
    _initializeRequiredSpareParts();
    
    // Load draft spare parts from cache (if user previously exited without submitting)
    _loadDraftSpareParts();
    
    // Load draft form data (notes, etc.)
    _loadDraftFormData();

    // Load latest checklist status from cache/API
    _loadChecklistStatus();
  }

  Future<void> _loadChecklistStatus() async {
    try {
      final repository = sl<TaskRepository>();
      // Use taskId (which is the task code/ID used for API calls)
      final items = await repository.getTaskChecklist(widget.task.taskId);
      
      // Load draft checklist state (local changes not yet submitted)
      await _loadDraftChecklistState(items);
      
      if (mounted) {
        setState(() {
          _checklistItems = items;
        });
        debugPrint('✅ Loaded ${items.length} checklist items from repository');
      }
    } catch (e) {
      debugPrint('⚠️ Error loading checklist status: $e');
    }
  }
  
  /// Load draft checklist state from cache
  Future<void> _loadDraftChecklistState(List<TaskChecklistItem> items) async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftChecklistPrefix}${widget.task.id}';
      final cached = await cacheManager.getDataNoExpiry(draftKey);
      
      if (cached != null && cached is Map<String, dynamic>) {
        final draftStates = Map<String, dynamic>.from(cached);
        
        // Apply draft states to items
        for (int i = 0; i < items.length; i++) {
          final itemId = items[i].id;
          if (draftStates.containsKey(itemId)) {
            final draftState = draftStates[itemId] as Map<String, dynamic>;
            items[i] = items[i].copyWith(
              isCompleted: draftState['isCompleted'] ?? items[i].isCompleted,
              readingValue: draftState['readingValue']?.toDouble(),
              remarks: draftState['remarks'],
              isAbnormal: draftState['isAbnormal'] ?? items[i].isAbnormal,
            );
          }
        }
        
        debugPrint('📦 Applied draft checklist state for task ${widget.task.id}');
      }
    } catch (e) {
      debugPrint('⚠️ Error loading draft checklist state: $e');
    }
  }
  
  /// Save draft checklist state to cache
  Future<void> _saveDraftChecklistState() async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftChecklistPrefix}${widget.task.id}';
      
      // Create state map from current checklist
      final draftStates = <String, dynamic>{};
      
      for (final item in _checklistItems) {
        draftStates[item.id] = {
          'isCompleted': item.isCompleted,
          'readingValue': item.readingValue,
          'remarks': item.remarks,
          'isAbnormal': item.isAbnormal,
          'savedAt': DateTime.now().toIso8601String(),
        };
      }
      
      await cacheManager.saveDataForOffline(draftKey, draftStates);
      debugPrint('💾 Saved draft checklist state for task ${widget.task.id}');
    } catch (e) {
      debugPrint('⚠️ Error saving draft checklist state: $e');
    }
  }
  
  /// Load draft spare parts from cache - similar to checklist pending mechanism
  Future<void> _loadDraftSpareParts() async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftSparePartsPrefix}${widget.task.id}';
      final cached = await cacheManager.getDataNoExpiry(draftKey);
      
      if (cached != null && cached is List) {
        setState(() {
          _actuallyUsedSpareParts = List<Map<String, dynamic>>.from(
            cached.map((item) => Map<String, dynamic>.from(item as Map))
          );
        });
        debugPrint('📦 Loaded ${_actuallyUsedSpareParts.length} draft spare parts for task ${widget.task.id}');
      }
    } catch (e) {
      debugPrint('⚠️ Error loading draft spare parts: $e');
    }
  }
  
  /// Save draft spare parts to cache when user adds/removes/edits
  Future<void> _saveDraftSpareParts() async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftSparePartsPrefix}${widget.task.id}';
      
      if (_actuallyUsedSpareParts.isEmpty) {
        // Clear cache if empty
        await cacheManager.clearCache(draftKey);
      } else {
        // Save to cache for offline persistence
        await cacheManager.saveDataForOffline(draftKey, _actuallyUsedSpareParts);
      }
      
      // Sync to server for real-time visibility on edge frontend
      await _syncSparePartsToServer();
      
      debugPrint('💾 Saved ${_actuallyUsedSpareParts.length} draft spare parts for task ${widget.task.id}');
    } catch (e) {
      debugPrint('⚠️ Error saving draft spare parts: $e');
    }
  }
  
  /// Sync spare parts to server for real-time visibility
  Future<void> _syncSparePartsToServer() async {
    try {
      final repository = sl<TaskRepository>();
      await repository.syncSparePartsUsed(
        taskCode: widget.task.taskId,
        sparePartsUsed: _actuallyUsedSpareParts,
      );
      debugPrint('✅ Synced spare parts to server for task ${widget.task.taskId}');
    } catch (e) {
      debugPrint('⚠️ Error syncing spare parts to server: $e');
      // Don't throw - this is for real-time visibility, not critical
    }
  }
  
  /// Clear draft after successful submission
  Future<void> _clearDraftData() async {
    try {
      final cacheManager = sl<CacheManager>();
      await cacheManager.clearCache('${CacheKeys.draftSparePartsPrefix}${widget.task.id}');
      await cacheManager.clearCache('${CacheKeys.draftTaskFormPrefix}${widget.task.id}');
      await cacheManager.clearCache('${CacheKeys.draftChecklistPrefix}${widget.task.id}');
      debugPrint('🧹 Cleared draft data for task ${widget.task.id}');
    } catch (e) {
      debugPrint('⚠️ Error clearing draft data: $e');
    }
  }
  
  /// Load draft form data (notes)
  Future<void> _loadDraftFormData() async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftTaskFormPrefix}${widget.task.id}';
      final cached = await cacheManager.getDataNoExpiry(draftKey);
      
      if (cached != null && cached is Map) {
        final data = Map<String, dynamic>.from(cached);
        if (data['notes'] != null && _notesController.text.isEmpty) {
          _notesController.text = data['notes'].toString();
        }
        debugPrint('📦 Loaded draft form data for task ${widget.task.id}');
      }
    } catch (e) {
      debugPrint('⚠️ Error loading draft form data: $e');
    }
  }
  
  /// Save draft form data
  Future<void> _saveDraftFormData() async {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftTaskFormPrefix}${widget.task.id}';
      
      final data = {
        'notes': _notesController.text,
        'savedAt': DateTime.now().toIso8601String(),
      };
      
      await cacheManager.saveDataForOffline(draftKey, data);
    } catch (e) {
      debugPrint('⚠️ Error saving draft form data: $e');
    }
  }
  
  void _initializeRequiredSpareParts() {
    try {
      // Use requiredSpareParts field (from schedule config), NOT sparePartsUsed (which is for completion)
      final spareParts = widget.task.requiredSpareParts;
      if (spareParts != null && spareParts.startsWith('[')) {
        final parsed = json.decode(spareParts);
        if (parsed is List) {
          _requiredSpareParts = List<Map<String, dynamic>>.from(
            parsed.map((item) => Map<String, dynamic>.from(item))
          );
        }
      }
    } catch (e) {
      debugPrint('Error parsing spare parts: $e');
    }
  }

  @override
  void dispose() {
    // Save draft states before disposing (user might return later)
    // Use unawaited to avoid blocking dispose
    // Note: These are fire-and-forget operations
    _saveDraftFormData();
    _saveDraftSparePartsSync();
    _saveDraftChecklistStateSync();
    
    _runningHoursController.dispose();
    _sparePartsController.dispose();
    _notesController.dispose();
    // Clear cached photo bytes
    _cachedPhotoBytes.clear();
    super.dispose();
  }
  
  // Synchronous version for dispose - won't wait for network
  void _saveDraftSparePartsSync() {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftSparePartsPrefix}${widget.task.id}';
      
      if (_actuallyUsedSpareParts.isEmpty) {
        cacheManager.clearCache(draftKey);
      } else {
        cacheManager.saveDataForOffline(draftKey, _actuallyUsedSpareParts);
      }
    } catch (e) {
      debugPrint('⚠️ Error saving draft spare parts sync: $e');
    }
  }
  
  // Synchronous version for dispose - won't wait for network
  void _saveDraftChecklistStateSync() {
    try {
      final cacheManager = sl<CacheManager>();
      final draftKey = '${CacheKeys.draftChecklistPrefix}${widget.task.id}';
      
      final draftStates = <String, dynamic>{};
      for (final item in _checklistItems) {
        draftStates[item.id] = {
          'isCompleted': item.isCompleted,
          'readingValue': item.readingValue,
          'remarks': item.remarks,
          'isAbnormal': item.isAbnormal,
          'savedAt': DateTime.now().toIso8601String(),
        };
      }
      
      cacheManager.saveDataForOffline(draftKey, draftStates);
    } catch (e) {
      debugPrint('⚠️ Error saving draft checklist sync: $e');
    }
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
          content: Text(l10n.minPhotosRequired(widget.task.requiredPhotos, _photoUrls.length)),
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

        // Build spare parts usage JSON from actually used list
        final sparePartsJson = _actuallyUsedSpareParts.isNotEmpty 
            ? json.encode(_actuallyUsedSpareParts)
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
      
      // Clear draft data after successful submission
      await _clearDraftData();

      if (mounted) {
        // Show success message
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                const Icon(Icons.check_circle, color: Colors.white),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    syncProvider.isOnline
                        ? l10n.taskCompletedSuccessfully
                        : l10n.taskSavedWillSync,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
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

                    // Required Spare Parts Section (display only - reference)
                    if (widget.task.sparePartsUsed != null && widget.task.sparePartsUsed!.isNotEmpty) ...[
                      _buildRequiredSparePartsSection(context),
                      const SizedBox(height: 16),
                    ],

                    // Actually Used Spare Parts Section (user selects from inventory)
                    _buildActuallyUsedSparePartsSection(context),
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

                    // Photos Section
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          l10n.reportPhotos,
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
                              l10n.photosRequired(_photoUrls.length, widget.task.requiredPhotos),
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
                        label: Text(_photoUrls.length >= 5 ? l10n.maxPhotosReached(5) : l10n.uploadPhotoOrTake),
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
                Flexible(
                  child: Text(
                    'Checklist Progress',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                ),
                const SizedBox(width: 8),
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
    
    // Calculate new state BEFORE updating
    final newIsCompleted = !item.isCompleted;
    
    try {
      final taskProvider = Provider.of<TaskProvider>(context, listen: false);
      
      // Update local state FIRST for instant UI feedback
      _checklistItems[index] = item.copyWith(isCompleted: newIsCompleted);
      
      // Single setState after all state changes
      if (mounted) setState(() {});
      
      // Call API in background with explicit isCompleted state
      taskProvider.completeChecklistItem(
        taskCode: widget.task.taskId,
        itemId: item.id,
        readingValue: item.readingValue,
        isCompleted: newIsCompleted, // Pass explicit state for toggle support
      ).then((_) {
        // Save draft state after successful API call
        _saveDraftChecklistState();
      }).catchError((e) {
        // Revert on error
        if (mounted) {
          _checklistItems[index] = item; // Revert to original
          setState(() {});
          final l10n = AppLocalizations.of(context)!;
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(l10n.errorPrefix(e.toString())),
              backgroundColor: Colors.red,
            ),
          );
        }
      });
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
      builder: (ctx) => _ReadingInputDialog(
        item: item,
        controller: readingController,
        onSubmit: (value, isAbnormal) async {
          Navigator.pop(ctx);
          if (isAbnormal) {
            // Show confirmation dialog for abnormal values
            final l10n = AppLocalizations.of(context);
            final confirmed = await showDialog<bool>(
              context: context,
              builder: (confirmCtx) => AlertDialog(
                title: Row(
                  children: [
                    Icon(Icons.warning_amber, color: Colors.orange.shade700),
                    const SizedBox(width: 8),
                    Flexible(child: Text(l10n.abnormalValue)),
                  ],
                ),
                content: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      l10n.measuredValueIs(value.toString(), item.unit ?? ''),
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                    ),
                    const SizedBox(height: 8),
                    if (item.minValue != null && item.maxValue != null)
                      Text(
                        l10n.allowedRange(item.minValue.toString(), item.maxValue.toString(), item.unit ?? ''),
                        style: TextStyle(color: Colors.grey.shade600),
                      ),
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.orange.shade50,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.orange.shade300),
                      ),
                      child: Text(
                        l10n.confirmAbnormalValue,
                        style: const TextStyle(fontSize: 14),
                      ),
                    ),
                  ],
                ),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.pop(confirmCtx, false),
                    child: Text(l10n.reenter),
                  ),
                  ElevatedButton(
                    onPressed: () => Navigator.pop(confirmCtx, true),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.orange,
                    ),
                    child: Text(l10n.confirmRecord),
                  ),
                ],
              ),
            );
            
            if (confirmed != true) {
              // Re-open the reading dialog
              _showReadingDialog(index);
              return;
            }
          }
          await _submitReadingValue(index, value);
        },
      ),
    );
  }

  Future<void> _submitReadingValue(int index, double? readingValue) async {
    if (readingValue == null) return;
    
    setState(() => _isTogglingChecklist = true);
    final l10n = AppLocalizations.of(context);
    
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
        remarks: isAbnormal ? l10n.valueOutOfRange : null,
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
      
      // Save draft state for offline persistence
      await _saveDraftChecklistState();
      
      // Trigger immediate sync if online to update edge frontend
      try {
        final syncQueue = sl<SyncQueue>();
        await syncQueue.processSyncQueue();
      } catch (e) {
        debugPrint('⚠️ Error processing sync queue: $e');
        // Don't throw - checklist update already succeeded
      }
      
      if (isAbnormal && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(l10n.valueOutOfRangeWarning(readingValue.toString(), item.minValue?.toString() ?? '', item.maxValue?.toString() ?? '')),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.errorMessage(e.toString())), backgroundColor: Colors.red),
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
                          child: Text(
                            AppLocalizations.of(context).required,
                            style: const TextStyle(fontSize: 10, color: Colors.red, fontWeight: FontWeight.bold),
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
                                : AppLocalizations.of(context).enterMeasuredValue,
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

  /// Build Required Spare Parts Section - DISPLAY ONLY
  Widget _buildRequiredSparePartsSection(BuildContext context) {
    if (_requiredSpareParts.isEmpty) return const SizedBox.shrink();
    final l10n = AppLocalizations.of(context);

    return Card(
      color: Colors.blue.shade50,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.list_alt, color: Colors.blue.shade700, size: 20),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    l10n.sparePartsReference,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: Colors.blue.shade900,
                        ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            ..._requiredSpareParts.map((part) => _buildRequiredSparePartItem(part)),
          ],
        ),
      ),
    );
  }

  Widget _buildRequiredSparePartItem(Map<String, dynamic> part) {
    final l10n = AppLocalizations.of(context);
    final materialName = part['materialName']?.toString() ?? 
                         part['name']?.toString() ?? 
                         l10n.unknownMaterial;
    final materialCode = part['materialCode']?.toString() ?? 
                         part['code']?.toString() ?? '';
    final quantityRequired = part['quantityRequired'] ?? 0;
    final isMandatory = part['isMandatory'] ?? false;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Icon(
            isMandatory ? Icons.warning_amber : Icons.inventory_2,
            color: isMandatory ? Colors.red : Colors.grey,
            size: 16,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  materialName,
                  style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
                ),
                if (materialCode.isNotEmpty)
                  Text(
                    l10n.materialCodeQuantity(materialCode, quantityRequired.toString()),
                    style: TextStyle(fontSize: 10, color: Colors.grey.shade600),
                  ),
              ],
            ),
          ),
          if (isMandatory)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: Colors.red.shade100,
                borderRadius: BorderRadius.circular(4),
              ),
              child: Text(
                l10n.required,
                style: const TextStyle(fontSize: 10, color: Colors.red, fontWeight: FontWeight.bold),
              ),
            ),
        ],
      ),
    );
  }

  /// Build Actually Used Spare Parts Section - COMBOBOX from inventory
  Widget _buildActuallyUsedSparePartsSection(BuildContext context) {
    final l10n = AppLocalizations.of(context);
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
                    l10n.sparePartsActuallyUsed,
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
              l10n.selectMaterialFromInventory,
              style: TextStyle(
                fontSize: 12,
                color: Colors.amber.shade800,
                fontStyle: FontStyle.italic,
              ),
            ),
            const SizedBox(height: 12),
            
            // List of actually used spare parts
            if (_actuallyUsedSpareParts.isNotEmpty) ...[
              ..._actuallyUsedSpareParts.asMap().entries.map((entry) => 
                _buildActuallyUsedItem(entry.key, entry.value)),
              const SizedBox(height: 8),
            ],
            
            // Add button
            OutlinedButton.icon(
              onPressed: _showAddMaterialDialog,
              icon: const Icon(Icons.add),
              label: Text(l10n.addUsedMaterial),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                side: BorderSide(color: Colors.amber.shade700),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActuallyUsedItem(int index, Map<String, dynamic> item) {
    final l10n = AppLocalizations.of(context);
    final materialName = item['materialName']?.toString() ?? 'Unknown';
    final materialCode = item['materialCode']?.toString() ?? '';
    final quantityUsed = item['quantityUsed'] ?? 0;
    final unit = item['unit']?.toString() ?? 'pcs';
    final onHand = item['onHandQuantity'] ?? 0;

    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.amber.shade200),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  materialName,
                  style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  l10n.materialCodeStock(materialCode, onHand.toString(), unit),
                  style: TextStyle(fontSize: 10, color: Colors.grey.shade600),
                ),
                const SizedBox(height: 4),
                Wrap(
                  spacing: 4,
                  runSpacing: 4,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [
                    Text(l10n.quantityUsed, style: const TextStyle(fontSize: 12)),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                      decoration: BoxDecoration(
                        color: Colors.amber.shade100,
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        '$quantityUsed $unit',
                        style: TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                          color: Colors.amber.shade900,
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          IconButton(
            icon: const Icon(Icons.edit, size: 20),
            onPressed: () => _showEditQuantityDialog(index, item),
            color: Colors.blue,
          ),
          IconButton(
            icon: const Icon(Icons.delete, size: 20),
            onPressed: () {
              setState(() {
                _actuallyUsedSpareParts.removeAt(index);
              });
              _saveDraftSpareParts(); // Auto-save after removal
            },
            color: Colors.red,
          ),
        ],
      ),
    );
  }

  void _showAddMaterialDialog() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (ctx) => _MaterialSelectionSheet(
        taskProvider: Provider.of<TaskProvider>(context, listen: false),
        requiredParts: _requiredSpareParts,
        alreadyUsedParts: _actuallyUsedSpareParts,
        onMaterialSelected: (material) {
          Navigator.pop(ctx);
          _showQuantityInputDialog(material);
        },
      ),
    );
  }

  void _showQuantityInputDialog(Map<String, dynamic> material) {
    final quantityController = TextEditingController(text: '1');
    final name = material['name']?.toString() ?? 'Unknown';
    final onHand = material['onHandQuantity'] ?? 0;
    final unit = material['unit']?.toString() ?? 'pcs';
    final l10n = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.enterQuantity),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(name, style: const TextStyle(fontWeight: FontWeight.bold)),
            Text(l10n.stockOnHand(onHand.toString(), unit), style: TextStyle(color: Colors.grey.shade600)),
            const SizedBox(height: 16),
            TextField(
              controller: quantityController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: InputDecoration(
                labelText: l10n.quantityUsed,
                suffixText: unit,
                border: const OutlineInputBorder(),
              ),
              autofocus: true,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(l10n.cancel),
          ),
          ElevatedButton(
            onPressed: () {
              final qty = double.tryParse(quantityController.text) ?? 0;
              if (qty > 0) {
                setState(() {
                  _actuallyUsedSpareParts.add({
                    'materialItemId': material['id']?.toString() ?? '',
                    'materialCode': material['itemCode']?.toString() ?? '',
                    'materialName': material['name']?.toString() ?? '',
                    'quantityUsed': qty,
                    'unit': unit,
                    'onHandQuantity': onHand,
                  });
                });
                _saveDraftSpareParts(); // Auto-save after adding
                Navigator.pop(context);
              }
            },
            child: Text(l10n.add),
          ),
        ],
      ),
    );
  }

  void _showEditQuantityDialog(int index, Map<String, dynamic> item) {
    final quantityController = TextEditingController(
      text: (item['quantityUsed'] ?? 1).toString(),
    );
    final name = item['materialName']?.toString() ?? 'Unknown';
    final onHand = item['onHandQuantity'] ?? 0;
    final unit = item['unit']?.toString() ?? 'pcs';
    final l10n = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.editQuantity),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(name, style: const TextStyle(fontWeight: FontWeight.bold)),
            Text(l10n.stockOnHand(onHand.toString(), unit), style: TextStyle(color: Colors.grey.shade600)),
            const SizedBox(height: 16),
            TextField(
              controller: quantityController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: InputDecoration(
                labelText: l10n.quantityUsed,
                suffixText: unit,
                border: const OutlineInputBorder(),
              ),
              autofocus: true,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(l10n.cancel),
          ),
          ElevatedButton(
            onPressed: () {
              final qty = double.tryParse(quantityController.text) ?? 0;
              if (qty > 0) {
                setState(() {
                  _actuallyUsedSpareParts[index]['quantityUsed'] = qty;
                });
                _saveDraftSpareParts(); // Auto-save after editing
                Navigator.pop(context);
              }
            },
            child: Text(l10n.save),
          ),
        ],
      ),
    );
  }
}

/// Separate StatefulWidget for material selection to handle its own state
class _MaterialSelectionSheet extends StatefulWidget {
  final TaskProvider taskProvider;
  final List<Map<String, dynamic>> requiredParts;
  final List<Map<String, dynamic>> alreadyUsedParts;
  final Function(Map<String, dynamic>) onMaterialSelected;

  const _MaterialSelectionSheet({
    required this.taskProvider,
    required this.requiredParts,
    required this.alreadyUsedParts,
    required this.onMaterialSelected,
  });

  @override
  State<_MaterialSelectionSheet> createState() => _MaterialSelectionSheetState();
}

class _MaterialSelectionSheetState extends State<_MaterialSelectionSheet> {
  List<Map<String, dynamic>> _materials = [];
  bool _isLoading = true;
  bool _isOffline = false;
  bool _showRequiredOnly = false;
  late final Set<String> _requiredCodes;
  late final Set<String> _requiredNames;
  final _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _requiredCodes = widget.requiredParts
        .map((p) => (p['materialCode'] ?? '').toString().trim().toLowerCase())
        .where((c) => c.isNotEmpty)
        .toSet();
    _requiredNames = widget.requiredParts
        .map((p) => (p['materialName'] ?? '').toString().trim().toLowerCase())
        .where((n) => n.isNotEmpty)
        .toSet();
    // UX: Default to Required view when we actually have a required list
    _showRequiredOnly = widget.requiredParts.isNotEmpty;
    _loadMaterials();
  }

  Future<void> _loadMaterials() async {
    setState(() => _isLoading = true);
    try {
      final materials = await widget.taskProvider.fetchAvailableMaterials();
      final isOnline = await widget.taskProvider.isOnline();
      if (mounted) {
        setState(() {
          _materials = materials;
          _isLoading = false;
          // Check if we're using cached data (materials loaded but might be offline)
          _isOffline = !isOnline;
        });
      }
    } catch (e) {
      debugPrint('Error loading materials: $e');
      if (mounted) {
        setState(() {
          _isLoading = false;
          _isOffline = true;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    bool matchesRequired(Map<String, dynamic> m) {
      if (_requiredCodes.isEmpty && _requiredNames.isEmpty) return false;
      final code = (m['itemCode'] ?? '').toString().trim().toLowerCase();
      final name = (m['name'] ?? '').toString().trim().toLowerCase();
      return (_requiredCodes.isNotEmpty && _requiredCodes.contains(code)) ||
          (_requiredNames.isNotEmpty && _requiredNames.contains(name));
    }

    final filtered = _materials.where((m) {
      final query = _searchController.text.toLowerCase();
      if (query.isEmpty) return true;
      final name = (m['name'] ?? '').toString().toLowerCase();
      final code = (m['itemCode'] ?? '').toString().toLowerCase();
      return name.contains(query) || code.contains(query);
    }).where((m) {
      if (!_showRequiredOnly) return true;
      return matchesRequired(m);
    }).toList();

    final hasRequiredList = widget.requiredParts.isNotEmpty;

    return DraggableScrollableSheet(
      initialChildSize: 0.7,
      minChildSize: 0.5,
      maxChildSize: 0.9,
      expand: false,
      builder: (context, scrollController) => Container(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(Icons.inventory_2, color: Colors.amber),
                const SizedBox(width: 8),
                Text(
                  l10n.selectFromGallery.replaceAll('Gallery', 'Inventory'),
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                const Spacer(),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.pop(context),
                ),
              ],
            ),
            const SizedBox(height: 12),
            // Required-first toggle
            Row(
              children: [
                ChoiceChip(
                  label: Text(l10n.required),
                  selected: _showRequiredOnly,
                  onSelected: hasRequiredList
                      ? (_) => setState(() => _showRequiredOnly = true)
                      : null,
                ),
                const SizedBox(width: 8),
                ChoiceChip(
                  label: Text(l10n.spareParts),
                  selected: !_showRequiredOnly,
                  onSelected: (_) => setState(() => _showRequiredOnly = false),
                ),
              ],
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: l10n.searchByEquipmentName,
                prefixIcon: const Icon(Icons.search),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
                contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              ),
              onChanged: (_) => setState(() {}),
            ),
            const SizedBox(height: 12),
            // Offline indicator banner
            if (_isOffline && _materials.isNotEmpty)
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                margin: const EdgeInsets.only(bottom: 12),
                decoration: BoxDecoration(
                  color: Colors.orange.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.orange.shade200),
                ),
                child: Row(
                  children: [
                    Icon(Icons.wifi_off, color: Colors.orange.shade700, size: 18),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'Offline mode - using cached data',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.orange.shade800,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            Text(l10n.materialsCount(filtered.length), style: TextStyle(color: Colors.grey.shade600)),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const CircularProgressIndicator(),
                          const SizedBox(height: 16),
                          Text(l10n.loadingMaterials),
                        ],
                      ),
                    )
                  : filtered.isEmpty
                      ? Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              const Icon(Icons.inventory_2_outlined, size: 48, color: Colors.grey),
                              const SizedBox(height: 16),
                              Text(
                                _showRequiredOnly
                                    ? 'No required materials found in inventory'
                                    : l10n.noMaterialsFound,
                              ),
                              const SizedBox(height: 8),
                              if (_showRequiredOnly)
                                TextButton.icon(
                                  onPressed: () => setState(() => _showRequiredOnly = false),
                                  icon: const Icon(Icons.list),
                                  label: const Text('Show all materials'),
                                )
                              else
                                TextButton.icon(
                                  onPressed: _loadMaterials,
                                  icon: const Icon(Icons.refresh),
                                  label: Text(l10n.reload),
                                ),
                            ],
                          ),
                        )
                      : ListView.builder(
                          controller: scrollController,
                          itemCount: filtered.length,
                          itemBuilder: (context, index) {
                            final material = filtered[index];
                            final id = material['id']?.toString() ?? '';
                            final name = material['name']?.toString() ?? 'Unknown';
                            final code = material['itemCode']?.toString() ?? '';
                            final onHand = material['onHandQuantity'] ?? 0;
                            final unit = material['unit']?.toString() ?? 'pcs';
                            
                            // Check if already added
                            final alreadyAdded = widget.alreadyUsedParts.any(
                              (p) => p['materialItemId'] == id
                            );

                            return ListTile(
                              leading: Icon(
                                Icons.inventory_2,
                                color: alreadyAdded ? Colors.grey : Colors.amber.shade700,
                              ),
                              title: Text(name, maxLines: 2, overflow: TextOverflow.ellipsis),
                              subtitle: Text(l10n.materialCode(code, onHand.toString(), unit)),
                              trailing: alreadyAdded
                                  ? const Icon(Icons.check, color: Colors.green)
                                  : Icon(Icons.add_circle, color: Colors.amber.shade700),
                              onTap: alreadyAdded
                                  ? null
                                  : () => widget.onMaterialSelected(material),
                            );
                          },
                        ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }
}

/// Dialog widget for entering reading values with real-time validation
class _ReadingInputDialog extends StatefulWidget {
  final TaskChecklistItem item;
  final TextEditingController controller;
  final Function(double value, bool isAbnormal) onSubmit;

  const _ReadingInputDialog({
    required this.item,
    required this.controller,
    required this.onSubmit,
  });

  @override
  State<_ReadingInputDialog> createState() => _ReadingInputDialogState();
}

class _ReadingInputDialogState extends State<_ReadingInputDialog> {
  bool _isAbnormal = false;
  String? _errorMessage;
  double? _currentValue;

  @override
  void initState() {
    super.initState();
    _validateValue(widget.controller.text);
    widget.controller.addListener(_onTextChanged);
  }

  void _onTextChanged() {
    _validateValue(widget.controller.text);
  }

  void _validateValue(String text) {
    final value = double.tryParse(text);
    setState(() {
      _currentValue = value;
      
      if (text.isEmpty) {
        _errorMessage = null;
        _isAbnormal = false;
        return;
      }
      
      if (value == null) {
        _errorMessage = _getValidNumberError();
        _isAbnormal = false;
        return;
      }
      
      _errorMessage = null;
      
      // Check min/max bounds
      if (widget.item.minValue != null && value < widget.item.minValue!) {
        _isAbnormal = true;
      } else if (widget.item.maxValue != null && value > widget.item.maxValue!) {
        _isAbnormal = true;
      } else {
        _isAbnormal = false;
      }
    });
  }

  String _getValidNumberError() {
    return AppLocalizations.of(context).pleaseEnterValidNumber;
  }

  @override
  void dispose() {
    widget.controller.removeListener(_onTextChanged);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final hasRange = widget.item.minValue != null || widget.item.maxValue != null;
    
    return AlertDialog(
      title: Text(widget.item.assetName),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Description
            if (widget.item.checkpointDescription != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(
                  widget.item.checkpointDescription!,
                  style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
                ),
              ),
            
            // Range indicator
            if (hasRange)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(12),
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.blue.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.blue.shade200),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.info_outline, size: 16, color: Colors.blue.shade700),
                        const SizedBox(width: 6),
                        Text(
                          l10n.allowedRange('', '', '').split(':')[0],
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: Colors.blue.shade700,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 4),
                    Text(
                      _buildRangeText(),
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w500,
                        color: Colors.blue.shade900,
                      ),
                    ),
                  ],
                ),
              ),
            
            // Input field
            TextField(
              controller: widget.controller,
              decoration: InputDecoration(
                labelText: l10n.enterMeasuredValue,
                suffixText: widget.item.unit ?? '',
                border: OutlineInputBorder(
                  borderSide: BorderSide(
                    color: _isAbnormal ? Colors.orange : Colors.grey,
                  ),
                ),
                enabledBorder: OutlineInputBorder(
                  borderSide: BorderSide(
                    color: _isAbnormal ? Colors.orange : Colors.grey.shade400,
                    width: _isAbnormal ? 2 : 1,
                  ),
                ),
                focusedBorder: OutlineInputBorder(
                  borderSide: BorderSide(
                    color: _isAbnormal ? Colors.orange : Colors.blue,
                    width: 2,
                  ),
                ),
                errorText: _errorMessage,
                errorBorder: const OutlineInputBorder(
                  borderSide: BorderSide(color: Colors.red, width: 2),
                ),
              ),
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              autofocus: true,
            ),
            
            // Warning message for abnormal values
            if (_isAbnormal && _currentValue != null)
              Container(
                width: double.infinity,
                margin: const EdgeInsets.only(top: 12),
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.orange.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.orange.shade300),
                ),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Icon(Icons.warning_amber, color: Colors.orange.shade700, size: 20),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            l10n.valueOutOfRange,
                            style: TextStyle(
                              fontWeight: FontWeight.bold,
                              color: Colors.orange.shade900,
                              fontSize: 13,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            _buildAbnormalReason(l10n),
                            style: TextStyle(
                              color: Colors.orange.shade800,
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: Text(l10n.cancel),
        ),
        ElevatedButton(
          onPressed: _currentValue == null || _errorMessage != null
              ? null
              : () => widget.onSubmit(_currentValue!, _isAbnormal),
          style: ElevatedButton.styleFrom(
            backgroundColor: _isAbnormal ? Colors.orange : null,
          ),
          child: Text(_isAbnormal ? l10n.confirmRecord : l10n.confirm),
        ),
      ],
    );
  }

  String _buildRangeText() {
    final min = widget.item.minValue;
    final max = widget.item.maxValue;
    final unit = widget.item.unit ?? '';
    
    if (min != null && max != null) {
      return '$min - $max $unit';
    } else if (min != null) {
      return '≥ $min $unit';
    } else if (max != null) {
      return '≤ $max $unit';
    }
    return '';
  }

  String _buildAbnormalReason(AppLocalizations l10n) {
    if (_currentValue == null) return '';
    
    final min = widget.item.minValue;
    final max = widget.item.maxValue;
    final unit = widget.item.unit ?? '';
    
    if (min != null && _currentValue! < min) {
      return l10n.valueOutOfRangeWarning(_currentValue.toString(), min.toString(), max?.toString() ?? '');
    } else if (max != null && _currentValue! > max) {
      return l10n.valueOutOfRangeWarning(_currentValue.toString(), min?.toString() ?? '', max.toString());
    }
    return '';
  }
}
