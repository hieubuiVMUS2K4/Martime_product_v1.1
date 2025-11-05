import 'package:equatable/equatable.dart';

/// Request to complete a checklist item
class CompleteChecklistItemRequest extends Equatable {
  final double? measuredValue; // Changed from String to double to match backend
  final bool? checkResult;
  final String? notes; // Changed from inspectionNotes to match backend field name
  final String? photoUrl;
  final String? completedBy; // Added to match backend field

  const CompleteChecklistItemRequest({
    this.measuredValue,
    this.checkResult,
    this.notes,
    this.photoUrl,
    this.completedBy,
  });

  Map<String, dynamic> toJson() {
    return {
      'measuredValue': measuredValue,
      'checkResult': checkResult,
      'notes': notes, // Backend expects 'notes' not 'inspectionNotes'
      'photoUrl': photoUrl,
      'completedBy': completedBy ?? 'Unknown', // Backend requires this field
    };
  }

  @override
  List<Object?> get props => [
        measuredValue,
        checkResult,
        notes,
        photoUrl,
        completedBy,
      ];
}
