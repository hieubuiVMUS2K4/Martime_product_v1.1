import 'package:equatable/equatable.dart';

/// Request to complete a checklist item
class CompleteChecklistItemRequest extends Equatable {
  final String? measuredValue;
  final bool? checkResult;
  final String? inspectionNotes;
  final String? photoUrl;
  final bool isCompleted;

  const CompleteChecklistItemRequest({
    this.measuredValue,
    this.checkResult,
    this.inspectionNotes,
    this.photoUrl,
    required this.isCompleted,
  });

  Map<String, dynamic> toJson() {
    return {
      'measuredValue': measuredValue,
      'checkResult': checkResult,
      'inspectionNotes': inspectionNotes,
      'photoUrl': photoUrl,
      'isCompleted': isCompleted,
    };
  }

  @override
  List<Object?> get props => [
        measuredValue,
        checkResult,
        inspectionNotes,
        photoUrl,
        isCompleted,
      ];
}
