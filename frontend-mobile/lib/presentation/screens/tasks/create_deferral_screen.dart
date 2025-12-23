import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';
import 'dart:convert';
import 'dart:typed_data';
import '../../../data/models/maintenance_task.dart';
import '../../../data/models/create_deferral_request_dto.dart';
import '../../providers/task_provider.dart';
import '../../../l10n/app_localizations.dart';

class CreateDeferralScreen extends StatefulWidget {
  final MaintenanceTask task;

  const CreateDeferralScreen({
    super.key,
    required this.task,
  });

  @override
  State<CreateDeferralScreen> createState() => _CreateDeferralScreenState();
}

class _CreateDeferralScreenState extends State<CreateDeferralScreen> {
  final _formKey = GlobalKey<FormState>();
  final _reasonController = TextEditingController();
  final _additionalNoteController = TextEditingController();
  final _rootCauseController = TextEditingController();
  final _preventiveMeasuresController = TextEditingController();
  
  DateTime? _proposedDate;
  String _priority = 'NORMAL';
  bool _isSubmitting = false;
  
  // Common reasons for deferral - will be populated from l10n
  String? _selectedReason;
  List<String> _commonReasons = [];
  
  // Common reasons for overdue tasks (more detailed)
  String? _selectedOverdueReason;
  List<String> _commonOverdueReasons = [];
  
  // Photo upload state (optional)
  final List<String> _photoUrls = [];
  bool _isUploadingPhoto = false;
  final ImagePicker _picker = ImagePicker();

  @override
  void dispose() {
    _reasonController.dispose();
    _additionalNoteController.dispose();
    _rootCauseController.dispose();
    _preventiveMeasuresController.dispose();
    super.dispose();
  }

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

  Future<void> _selectDate(BuildContext context) async {
    final now = DateTime.now();
    final currentDue = widget.task.nextDueAt != null ? DateTime.parse(widget.task.nextDueAt!) : now;
    final initialDate = currentDue.isAfter(now) ? currentDue.add(const Duration(days: 1)) : now.add(const Duration(days: 1));
    
    final picked = await showDatePicker(
      context: context,
      initialDate: initialDate,
      firstDate: now,
      lastDate: now.add(const Duration(days: 365)),
    );

    if (picked != null && picked != _proposedDate) {
      setState(() {
        _proposedDate = picked;
      });
    }
  }

  Future<void> _submit() async {
    final l10n = AppLocalizations.of(context);
    if (!_formKey.currentState!.validate()) return;
    if (_proposedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l10n.pleaseSelectDeferralDate)),
      );
      return;
    }

    // Validate reason selection
    final isOverdue = widget.task.isOverdue;
    final selectedReason = isOverdue ? _selectedOverdueReason : _selectedReason;
    if (selectedReason == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(l10n.pleaseSelectDeferralReason)),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      // Build final reason: selected reason + optional note
      String finalReason = selectedReason;
      final additionalNote = _additionalNoteController.text.trim();
      if (additionalNote.isNotEmpty) {
        finalReason = '$selectedReason. $additionalNote';
      }

      print('📤 Creating deferral request for task ${widget.task.id}');
      print('   Reason: $finalReason');
      print('   Photos: ${_photoUrls.length}');
      if (isOverdue) {
        print('   Root cause: ${_rootCauseController.text.trim()}');
        print('   Preventive measures: ${_preventiveMeasuresController.text.trim()}');
      }
      
      final dto = CreateDeferralRequestDto(
        taskId: widget.task.id,
        reason: finalReason,
        proposedDueDate: _proposedDate!.toIso8601String(),
        priority: _priority,
        rootCause: isOverdue ? _rootCauseController.text.trim() : null,
        preventiveMeasures: isOverdue ? _preventiveMeasuresController.text.trim() : null,
        attachments: _photoUrls.isNotEmpty ? _photoUrls : null,
      );

      await Provider.of<TaskProvider>(context, listen: false).createDeferralRequest(dto);
      print('✅ Deferral request sent successfully');

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(l10n.deferralRequestSent),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context); // Close screen
        Navigator.pop(context); // Close detail screen (optional, or just refresh)
      }
    } catch (e) {
      print('❌ Error creating deferral: $e');
      if (mounted) {
        // Extract meaningful error message
        String errorMsg = e.toString();
        if (errorMsg.contains('DioException')) {
          if (errorMsg.contains('timeout')) {
            errorMsg = l10n.requestTimeoutError;
          } else if (errorMsg.contains('400')) {
            errorMsg = l10n.validationError;
          } else if (errorMsg.contains('500')) {
            errorMsg = l10n.serverError;
          }
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(l10n.errorMessage(errorMsg)),
            backgroundColor: Colors.red,
            duration: const Duration(seconds: 5),
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final isOverdue = widget.task.isOverdue;
    final dateFormat = DateFormat('dd/MM/yyyy');
    
    // Populate reasons from l10n
    if (_commonReasons.isEmpty) {
      _commonReasons = [
        l10n.deferralReasonPartsPending,
        l10n.deferralReasonWeatherCondition,
        l10n.deferralReasonHigherPriority,
        l10n.deferralReasonAwaitingApproval,
        l10n.deferralReasonPersonnelUnavailable,
        l10n.deferralReasonEquipmentInUse,
        l10n.deferralReasonOther,
      ];
    }
    if (_commonOverdueReasons.isEmpty) {
      _commonOverdueReasons = [
        l10n.overdueReasonPartsPending,
        l10n.overdueReasonExternalFactors,
        l10n.overdueReasonHigherPriority,
        l10n.overdueReasonResourceShortage,
        l10n.overdueReasonAwaitingApproval,
        l10n.overdueReasonTechnicalIssue,
        l10n.overdueReasonOther,
      ];
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.requestDeferral),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // Task Info Summary
            Card(
              color: Colors.grey.shade50,
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.task.displayName,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                    ),
                    const SizedBox(height: 4),
                    Text(l10n.currentDueDate(widget.task.nextDueAt != null ? dateFormat.format(DateTime.parse(widget.task.nextDueAt!)) : "N/A")),
                    if (isOverdue)
                      Text(
                        l10n.statusOverdue.toUpperCase(),
                        style: const TextStyle(color: Colors.red, fontWeight: FontWeight.bold),
                      ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),

            // Proposed Date
            InkWell(
              onTap: () => _selectDate(context),
              child: InputDecorator(
                decoration: InputDecoration(
                  labelText: '${l10n.deferralRequest} *',
                  border: const OutlineInputBorder(),
                  prefixIcon: const Icon(Icons.calendar_today),
                ),
                child: Text(
                  _proposedDate != null ? dateFormat.format(_proposedDate!) : l10n.selectDate,
                  style: TextStyle(
                    color: _proposedDate != null ? Colors.black : Colors.grey,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Priority
            DropdownButtonFormField<String>(
              value: _priority,
              decoration: InputDecoration(
                labelText: l10n.taskPriority,
                border: const OutlineInputBorder(),
              ),
              items: [
                DropdownMenuItem(value: 'LOW', child: Text(l10n.priorityLow)),
                DropdownMenuItem(value: 'NORMAL', child: Text(l10n.priorityNormal)),
                DropdownMenuItem(value: 'HIGH', child: Text(l10n.priorityHigh)),
              ],
              onChanged: (val) => setState(() => _priority = val!),
            ),
            const SizedBox(height: 16),

            // Reason Selection - Normal tasks
            if (!isOverdue) ...[
              DropdownButtonFormField<String>(
                value: _selectedReason,
                decoration: InputDecoration(
                  labelText: '${l10n.deferralRequest} *',
                  border: const OutlineInputBorder(),
                  prefixIcon: const Icon(Icons.format_list_bulleted),
                ),
                hint: Text(l10n.selectReason),
                items: _commonReasons.map((reason) {
                  return DropdownMenuItem(value: reason, child: Text(reason));
                }).toList(),
                onChanged: (val) => setState(() => _selectedReason = val),
                validator: (value) {
                  if (value == null) return l10n.pleaseSelectDeferralReason;
                  return null;
                },
              ),
              const SizedBox(height: 16),
              
              // Additional note (optional)
              TextFormField(
                controller: _additionalNoteController,
                decoration: InputDecoration(
                  labelText: l10n.notesOptional,
                  border: const OutlineInputBorder(),
                  hintText: l10n.enterMoreDetails,
                  prefixIcon: const Icon(Icons.note_add),
                ),
                maxLines: 2,
              ),
              const SizedBox(height: 16),
            ],

            // Overdue specific fields
            if (isOverdue) ...[
              // Reason Selection - Overdue tasks
              DropdownButtonFormField<String>(
                value: _selectedOverdueReason,
                decoration: InputDecoration(
                  labelText: '${l10n.deferralRequest} *',
                  border: const OutlineInputBorder(),
                  prefixIcon: const Icon(Icons.format_list_bulleted),
                ),
                hint: Text(l10n.selectReason),
                items: _commonOverdueReasons.map((reason) {
                  return DropdownMenuItem(value: reason, child: Text(reason));
                }).toList(),
                onChanged: (val) => setState(() => _selectedOverdueReason = val),
                validator: (value) {
                  if (value == null) return l10n.pleaseSelectDeferralReason;
                  return null;
                },
              ),
              const SizedBox(height: 16),
              
              // Additional note for overdue (optional)
              TextFormField(
                controller: _additionalNoteController,
                decoration: InputDecoration(
                  labelText: l10n.notesOptional,
                  border: const OutlineInputBorder(),
                  hintText: l10n.enterMoreDetails,
                  prefixIcon: const Icon(Icons.note_add),
                ),
                maxLines: 2,
              ),
              const SizedBox(height: 16),
              
              const Divider(),
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Text(
                  l10n.additionalInfoOverdue,
                  style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.orange),
                ),
              ),
              TextFormField(
                controller: _rootCauseController,
                decoration: InputDecoration(
                  labelText: l10n.rootCauseLabel,
                  border: const OutlineInputBorder(),
                  hintText: l10n.rootCauseHint,
                ),
                maxLines: 2,
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _preventiveMeasuresController,
                decoration: InputDecoration(
                  labelText: l10n.preventiveMeasuresLabel,
                  border: const OutlineInputBorder(),
                  hintText: l10n.preventiveMeasuresHint,
                ),
                maxLines: 2,
              ),
              const SizedBox(height: 16),

              // Photo upload section for OVERDUE (optional)
              Text(
                l10n.evidencePhotosOptional,
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
              ),
              const SizedBox(height: 8),
              Text(
                l10n.evidencePhotosHint,
                style: const TextStyle(fontSize: 12, color: Colors.grey),
              ),
              const SizedBox(height: 12),
              
              // Photo thumbnails
              if (_photoUrls.isNotEmpty)
                SizedBox(
                  height: 100,
                  child: ListView.builder(
                    scrollDirection: Axis.horizontal,
                    itemCount: _photoUrls.length,
                    itemBuilder: (context, index) {
                      return Stack(
                        children: [
                          Container(
                            margin: const EdgeInsets.only(right: 8),
                            width: 100,
                            height: 100,
                            decoration: BoxDecoration(
                              border: Border.all(color: Colors.grey),
                              borderRadius: BorderRadius.circular(8),
                              image: DecorationImage(
                                image: MemoryImage(
                                  base64Decode(_photoUrls[index].split(',')[1])
                                ),
                                fit: BoxFit.cover,
                              ),
                            ),
                          ),
                          Positioned(
                            top: 4,
                            right: 12,
                            child: GestureDetector(
                              onTap: () {
                                setState(() {
                                  _photoUrls.removeAt(index);
                                });
                              },
                              child: Container(
                                decoration: const BoxDecoration(
                                  color: Colors.red,
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(
                                  Icons.close,
                                  color: Colors.white,
                                  size: 20,
                                ),
                              ),
                            ),
                          ),
                        ],
                      );
                    },
                  ),
                ),
              if (_photoUrls.isNotEmpty) const SizedBox(height: 12),
              
              // Add photo button
              OutlinedButton.icon(
                onPressed: _isUploadingPhoto ? null : _showImageSourceActionSheet,
                icon: _isUploadingPhoto
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.add_photo_alternate),
                label: Text(_isUploadingPhoto ? l10n.addingPhoto : l10n.addPhoto),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                ),
              ),
            ],

            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: _isSubmitting ? null : _submit,
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
                backgroundColor: Theme.of(context).primaryColor,
                foregroundColor: Colors.white,
              ),
              child: _isSubmitting
                  ? const CircularProgressIndicator(color: Colors.white)
                  : Text(l10n.submitRequest),
            ),
          ],
        ),
      ),
    );
  }
}
