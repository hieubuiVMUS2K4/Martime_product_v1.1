import 'package:equatable/equatable.dart';

class CrewMember extends Equatable {
  final int id;
  final String crewId;
  final String fullName;
  final String position;
  final String? rank;
  final String? department;
  final String? nationality;
  final String? dateOfBirth;
  final String? phoneNumber;
  final String? email;
  final String? address;
  final String? emergencyContact;
  
  // Employment
  final String? joinDate;
  final String? embarkDate;
  final String? disembarkDate;
  final String? contractEnd;
  final bool isOnboard;
  
  // Certificates
  final String? certificateNumber;
  final String? certificateIssue;
  final String? certificateExpiry;
  final String? medicalIssue;
  final String? medicalExpiry;
  final String? passportNumber;
  final String? passportExpiry;
  final String? visaNumber;
  final String? visaExpiry;
  final String? seamanBookNumber;
  
  final String? notes;
  final bool isSynced;
  final String createdAt;
  
  const CrewMember({
    required this.id,
    required this.crewId,
    required this.fullName,
    required this.position,
    this.rank,
    this.department,
    this.nationality,
    this.dateOfBirth,
    this.phoneNumber,
    this.email,
    this.address,
    this.emergencyContact,
    this.joinDate,
    this.embarkDate,
    this.disembarkDate,
    this.contractEnd,
    required this.isOnboard,
    this.certificateNumber,
    this.certificateIssue,
    this.certificateExpiry,
    this.medicalIssue,
    this.medicalExpiry,
    this.passportNumber,
    this.passportExpiry,
    this.visaNumber,
    this.visaExpiry,
    this.seamanBookNumber,
    this.notes,
    required this.isSynced,
    required this.createdAt,
  });
  
  factory CrewMember.fromJson(Map<String, dynamic> json) {
    return CrewMember(
      id: json['id'],
      crewId: json['crewId'],
      fullName: json['fullName'],
      position: json['position'],
      rank: json['rank'],
      department: json['department'],
      nationality: json['nationality'],
      dateOfBirth: json['dateOfBirth'],
      phoneNumber: json['phoneNumber'],
      email: json['email'],
      address: json['address'],
      emergencyContact: json['emergencyContact'],
      joinDate: json['joinDate'],
      embarkDate: json['embarkDate'],
      disembarkDate: json['disembarkDate'],
      contractEnd: json['contractEnd'],
      isOnboard: json['isOnboard'] ?? true,
      certificateNumber: json['certificateNumber'],
      certificateIssue: json['certificateIssue'],
      certificateExpiry: json['certificateExpiry'],
      medicalIssue: json['medicalIssue'],
      medicalExpiry: json['medicalExpiry'],
      passportNumber: json['passportNumber'],
      passportExpiry: json['passportExpiry'],
      visaNumber: json['visaNumber'],
      visaExpiry: json['visaExpiry'],
      seamanBookNumber: json['seamanBookNumber'],
      notes: json['notes'],
      isSynced: json['isSynced'] ?? false,
      createdAt: json['createdAt'],
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'crewId': crewId,
      'fullName': fullName,
      'position': position,
      'rank': rank,
      'department': department,
      'nationality': nationality,
      'dateOfBirth': dateOfBirth,
      'phoneNumber': phoneNumber,
      'email': email,
      'address': address,
      'emergencyContact': emergencyContact,
      'joinDate': joinDate,
      'embarkDate': embarkDate,
      'disembarkDate': disembarkDate,
      'contractEnd': contractEnd,
      'isOnboard': isOnboard,
      'certificateNumber': certificateNumber,
      'certificateIssue': certificateIssue,
      'certificateExpiry': certificateExpiry,
      'medicalIssue': medicalIssue,
      'medicalExpiry': medicalExpiry,
      'passportNumber': passportNumber,
      'passportExpiry': passportExpiry,
      'visaNumber': visaNumber,
      'visaExpiry': visaExpiry,
      'seamanBookNumber': seamanBookNumber,
      'notes': notes,
      'isSynced': isSynced,
      'createdAt': createdAt,
    };
  }
  
  // Certificate status checks
  bool get isCertificateExpiring {
    if (certificateExpiry == null) return false;
    final expiry = DateTime.parse(certificateExpiry!);
    final daysLeft = expiry.difference(DateTime.now()).inDays;
    return daysLeft <= 90 && daysLeft > 0;
  }
  
  bool get isCertificateExpired {
    if (certificateExpiry == null) return false;
    final expiry = DateTime.parse(certificateExpiry!);
    return expiry.isBefore(DateTime.now());
  }
  
  bool get isMedicalExpiring {
    if (medicalExpiry == null) return false;
    final expiry = DateTime.parse(medicalExpiry!);
    final daysLeft = expiry.difference(DateTime.now()).inDays;
    return daysLeft <= 90 && daysLeft > 0;
  }
  
  bool get isMedicalExpired {
    if (medicalExpiry == null) return false;
    final expiry = DateTime.parse(medicalExpiry!);
    return expiry.isBefore(DateTime.now());
  }
  
  bool get isPassportExpiring {
    if (passportExpiry == null) return false;
    final expiry = DateTime.parse(passportExpiry!);
    final daysLeft = expiry.difference(DateTime.now()).inDays;
    return daysLeft <= 180 && daysLeft > 0; // 6 months warning
  }
  
  bool get isPassportExpired {
    if (passportExpiry == null) return false;
    final expiry = DateTime.parse(passportExpiry!);
    return expiry.isBefore(DateTime.now());
  }
  
  @override
  List<Object?> get props => [id, crewId, fullName, position];
}
