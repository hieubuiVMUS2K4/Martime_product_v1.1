import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
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
  final _rootCauseController = TextEditingController();
  final _preventiveMeasuresController = TextEditingController();
  
  DateTime? _proposedDate;
  String _priority = 'NORMAL';
  bool _isSubmitting = false;

  @override
  void dispose() {
    _reasonController.dispose();
    _rootCauseController.dispose();
    _preventiveMeasuresController.dispose();
    super.dispose();
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

    setState(() => _isSubmitting = true);

    try {
      final dto = CreateDeferralRequestDto(
        taskId: widget.task.id,
        reason: _reasonController.text.trim(),
        proposedDueDate: _proposedDate!.toIso8601String(),
        priority: _priority,
        rootCause: widget.task.isOverdue ? _rootCauseController.text.trim() : null,
        preventiveMeasures: widget.task.isOverdue ? _preventiveMeasuresController.text.trim() : null,
      );

      await Provider.of<TaskProvider>(context, listen: false).createDeferralRequest(dto);

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
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Lỗi: ${e.toString()}'),
            backgroundColor: Colors.red,
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
