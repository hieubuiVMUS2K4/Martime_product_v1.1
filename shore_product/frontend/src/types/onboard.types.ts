// ============================================================
// ONBOARD EVENT TYPES
// ============================================================

export interface OnboardEventDto {
  id: string;
  crewMemberId: string;
  crewName?: string;
  vesselId: string;
  vesselName?: string;
  assignmentId?: string;
  eventType: string;
  eventTimestamp: string;
  portCode?: string;
  portName?: string;
  confirmedBy?: string;
  confirmedByRole?: string;
  signOffReason?: string;
  remarks?: string;
  originalEventId?: string;
  source: string;
  isSynced: boolean;
  createdAt: string;
}

export interface CreateOnboardEventRequest {
  crewMemberId: string;
  vesselId: string;
  assignmentId?: string;
  eventType: string;
  eventTimestamp: string;
  portCode?: string;
  portName?: string;
  confirmedBy?: string;
  confirmedByRole?: string;
  signOffReason?: string;
  remarks?: string;
  originalEventId?: string;
  source?: string;
}

// ============================================================
// ACCESS GRANT TYPES
// ============================================================

export interface CrewAccessGrantDto {
  id: string;
  crewMemberId: string;
  crewName?: string;
  vesselId: string;
  vesselName?: string;
  assignmentId?: string;
  status: string;
  module: string;
  grantedAt?: string;
  revokedAt?: string;
  revokeReason?: string;
  grantedBy?: string;
  revokedBy?: string;
  createdAt: string;
}

export interface GrantAccessRequest {
  crewMemberId: string;
  vesselId: string;
  assignmentId?: string;
  module?: string;
  grantedBy?: string;
}

export interface RevokeAccessRequest {
  revokeReason?: string;
  revokedBy?: string;
}

export interface SuspendAccessRequest {
  reason?: string;
  suspendedBy?: string;
}

// ============================================================
// SIGN-ON TYPES
// ============================================================

export interface SignOnRecordDto {
  id: string;
  crewMemberId: string;
  crewName?: string;
  vesselId: string;
  vesselName?: string;
  assignmentId?: string;
  rankId: number;
  rankName?: string;
  signOnDate: string;
  portCode?: string;
  portName?: string;
  signedOnBy: string;
  remarks?: string;
  onboardEventId?: string;
  source: string;
  isSynced: boolean;
  createdAt: string;
}

export interface CreateSignOnRequest {
  crewMemberId: string;
  vesselId: string;
  assignmentId?: string;
  rankId: number;
  signOnDate: string;
  portCode?: string;
  portName?: string;
  signedOnBy: string;
  remarks?: string;
  onboardEventId?: string;
  source?: string;
}

// ============================================================
// SIGN-OFF TYPES
// ============================================================

export interface SignOffRecordDto {
  id: string;
  crewMemberId: string;
  crewName?: string;
  vesselId: string;
  vesselName?: string;
  assignmentId?: string;
  rankId: number;
  rankName?: string;
  signOffDate: string;
  portCode?: string;
  portName?: string;
  reason: string;
  reasonDetail?: string;
  signedOffBy: string;
  remarks?: string;
  onboardEventId?: string;
  signOnRecordId?: string;
  source: string;
  isSynced: boolean;
  createdAt: string;
}

export interface CreateSignOffRequest {
  crewMemberId: string;
  vesselId: string;
  assignmentId?: string;
  rankId: number;
  signOffDate: string;
  portCode?: string;
  portName?: string;
  reason: string;
  reasonDetail?: string;
  signedOffBy: string;
  remarks?: string;
  onboardEventId?: string;
  signOnRecordId?: string;
  source?: string;
}

// ============================================================
// CONSTANTS
// ============================================================

export const OnboardEventType = {
  Arrived: 'Arrived',
  SignedOn: 'SignedOn',
  SignedOff: 'SignedOff',
  Departed: 'Departed',
  CorrectionArrived: 'CorrectionArrived',
  CorrectionSignedOn: 'CorrectionSignedOn',
  CorrectionSignedOff: 'CorrectionSignedOff',
} as const;

export const AccessGrantStatus = {
  NotGranted: 'NotGranted',
  PendingSync: 'PendingSync',
  Granted: 'Granted',
  Suspended: 'Suspended',
  Revoked: 'Revoked',
} as const;

export const SignOffReason = {
  ContractEnd: 'ContractEnd',
  MutualAgreement: 'MutualAgreement',
  Medical: 'Medical',
  Disciplinary: 'Disciplinary',
  CompanyRequest: 'CompanyRequest',
  CrewRequest: 'CrewRequest',
  Emergency: 'Emergency',
  VesselChange: 'VesselChange',
  Other: 'Other',
} as const;

export const EVENT_TYPE_LABELS: Record<string, string> = {
  Arrived: 'Đã lên tàu',
  SignedOn: 'Đã ký nhận',
  SignedOff: 'Đã ký rời',
  Departed: 'Đã rời tàu',
  CorrectionArrived: 'Sửa - Lên tàu',
  CorrectionSignedOn: 'Sửa - Ký nhận',
  CorrectionSignedOff: 'Sửa - Ký rời',
};

export const ACCESS_STATUS_LABELS: Record<string, string> = {
  NotGranted: 'Chưa cấp',
  PendingSync: 'Chờ đồng bộ',
  Granted: 'Đã cấp',
  Suspended: 'Tạm ngưng',
  Revoked: 'Thu hồi',
};

export const SIGN_OFF_REASON_LABELS: Record<string, string> = {
  ContractEnd: 'Hết hợp đồng',
  MutualAgreement: 'Thỏa thuận',
  Medical: 'Y tế',
  Disciplinary: 'Kỷ luật',
  CompanyRequest: 'Yêu cầu công ty',
  CrewRequest: 'Yêu cầu thuyền viên',
  Emergency: 'Khẩn cấp',
  VesselChange: 'Chuyển tàu',
  Other: 'Khác',
};
