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
  final _rootCauseController = TextEditingController();
  final _preventiveMeasuresController = TextEditingController();
  
  DateTime? _proposedDate;
  String _priority = 'NORMAL';
  bool _isSubmitting = false;
  
  // Photo upload state (required for OVERDUE tasks)
  final List<String> _photoUrls = [];
  bool _isUploadingPhoto = false;
  final ImagePicker _picker = ImagePicker();

  @override
  void dispose() {
    _reasonController.dispose();
    _rootCauseController.dispose();
    _preventiveMeasuresController.dispose();
    super.dispose();
  }

  Future<void> _pickImage(ImageSource source) async {
    try {
      setState(() => _isUploadingPhoto = true);
      
      final XFile? image = await _picker.pickImage(
        source: source,
        imageQuality: 20, // Giảm xuống 20% để tránh timeout
        maxWidth: 480,    // Giảm xuống 480px
        maxHeight: 360,
      );

      if (image != null) {
        final File imageFile = File(image.path);
        final Uint8List imageBytes = await imageFile.readAsBytes();
        
        if (imageBytes.length > 200 * 1024) { // Giảm xuống 200KB
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Ảnh quá lớn (max 200KB), vui lòng chọn ảnh khác hoặc chụp lại')),
            );
          }
          return;
        }
        
        final String base64Image = base64Encode(imageBytes);
        final String extension = image.path.split('.').last.toLowerCase();
        final String mimeType = extension == 'png' ? 'image/png' : 'image/jpeg';
        final String dataUrl = 'data:$mimeType;base64,$base64Image';
        
        setState(() {
          _photoUrls.add(dataUrl);
        });
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi chọn ảnh: $e')),
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
    if (!_formKey.currentState!.validate()) return;
    if (_proposedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng chọn ngày hoãn đến')),
      );
      return;
    }

    // Validate photos for OVERDUE tasks
    if (widget.task.isOverdue && _photoUrls.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Task quá hạn bắt buộc phải có ảnh chứng minh (spare parts order, weather report, v.v.)'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      print('📤 Creating deferral request for task ${widget.task.id}');
      print('   Reason length: ${_reasonController.text.trim().length}');
      print('   Photos: ${_photoUrls.length}');
      if (widget.task.isOverdue) {
        print('   Root cause length: ${_rootCauseController.text.trim().length}');
        print('   Preventive measures length: ${_preventiveMeasuresController.text.trim().length}');
      }
      
      final dto = CreateDeferralRequestDto(
        taskId: widget.task.id,
        reason: _reasonController.text.trim(),
        proposedDueDate: _proposedDate!.toIso8601String(),
        priority: _priority,
        rootCause: widget.task.isOverdue ? _rootCauseController.text.trim() : null,
        preventiveMeasures: widget.task.isOverdue ? _preventiveMeasuresController.text.trim() : null,
        attachments: _photoUrls.isNotEmpty ? _photoUrls : null,
      );

      await Provider.of<TaskProvider>(context, listen: false).createDeferralRequest(dto);
      print('✅ Deferral request sent successfully');

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Đã gửi yêu cầu hoãn task'),
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
            errorMsg = 'Request timeout - ảnh có thể quá lớn. Vui lòng thử ảnh nhỏ hơn';
          } else if (errorMsg.contains('400')) {
            errorMsg = 'Validation error - kiểm tra lại form (reason, photos, v.v.)';
          } else if (errorMsg.contains('500')) {
            errorMsg = 'Server error - vui lòng thử lại sau';
          }
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Lỗi: $errorMsg'),
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
    final isOverdue = widget.task.isOverdue;
    final dateFormat = DateFormat('dd/MM/yyyy');

    return Scaffold(
      appBar: AppBar(
        title: const Text('Xin hoãn Task'),
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
                    Text('Hạn hiện tại: ${widget.task.nextDueAt != null ? dateFormat.format(DateTime.parse(widget.task.nextDueAt!)) : "N/A"}'),
                    if (isOverdue)
                      const Text(
                        'ĐANG QUÁ HẠN',
                        style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold),
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
                decoration: const InputDecoration(
                  labelText: 'Xin hoãn đến ngày *',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.calendar_today),
                ),
                child: Text(
                  _proposedDate != null ? dateFormat.format(_proposedDate!) : 'Chọn ngày',
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
              decoration: const InputDecoration(
                labelText: 'Mức độ ưu tiên',
                border: OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 'LOW', child: Text('Thấp')),
                DropdownMenuItem(value: 'NORMAL', child: Text('Bình thường')),
                DropdownMenuItem(value: 'HIGH', child: Text('Cao')),
              ],
              onChanged: (val) => setState(() => _priority = val!),
            ),
            const SizedBox(height: 16),

            // Reason
            TextFormField(
              controller: _reasonController,
              decoration: const InputDecoration(
                labelText: 'Lý do xin hoãn *',
                border: OutlineInputBorder(),
                helperText: 'Tối thiểu 20 ký tự (50 nếu quá hạn)',
              ),
              maxLines: 3,
              validator: (value) {
                if (value == null || value.isEmpty) return 'Vui lòng nhập lý do';
                final minLength = isOverdue ? 50 : 20;
                if (value.length < minLength) {
                  return 'Lý do phải có ít nhất $minLength ký tự';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),

            // Overdue specific fields
            if (isOverdue) ...[
              const Divider(),
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 8),
                child: Text(
                  'Thông tin bổ sung (Bắt buộc do quá hạn)',
                  style: TextStyle(fontWeight: FontWeight.bold, color: Colors.red),
                ),
              ),
              TextFormField(
                controller: _rootCauseController,
                decoration: const InputDecoration(
                  labelText: 'Nguyên nhân gốc rễ *',
                  border: OutlineInputBorder(),
                ),
                maxLines: 2,
                validator: (value) {
                  if (value == null || value.length < 20) {
                    return 'Vui lòng nhập ít nhất 20 ký tự';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _preventiveMeasuresController,
                decoration: const InputDecoration(
                  labelText: 'Biện pháp phòng ngừa *',
                  border: OutlineInputBorder(),
                ),
                maxLines: 2,
                validator: (value) {
                  if (value == null || value.length < 20) {
                    return 'Vui lòng nhập ít nhất 20 ký tự';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),

              // Photo upload section for OVERDUE
              const Text(
                'Ảnh chứng minh (Bắt buộc) *',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
              ),
              const SizedBox(height: 8),
              const Text(
                'Vui lòng đính kèm spare parts order, weather report, Class email, hoặc tài liệu khác chứng minh lý do hoãn',
                style: TextStyle(fontSize: 12, color: Colors.grey),
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
                label: Text(_isUploadingPhoto ? 'Đang tải...' : 'Thêm ảnh'),
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
                  : const Text('GỬI YÊU CẦU'),
            ),
          ],
        ),
      ),
    );
  }
}
