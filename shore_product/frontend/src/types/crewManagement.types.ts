// ============================================================
// Crew Management Workflow Types — matches backend DTOs
// ============================================================

// --- Onboarding ---

export interface OnboardingCase {
  id: string;
  crewMemberId: string;
  crewName?: string;
  crewCode?: string;
  referenceVesselId?: string;
  referenceVesselName?: string;
  vesselGroupCode?: string;
  flagState?: string;
  status: string;
  statusChangedAt?: string;
  invitedAt?: string;
  activatedAt?: string;
  dueDate?: string;
  notes?: string;
  createdBy: string;
  createdAt: string;
  totalItems: number;
  completedItems: number;
  mandatoryItems: number;
  mandatoryCompleted: number;
  checklistItems: OnboardingChecklistItem[];
}

export interface OnboardingChecklistItem {
  id: string;
  onboardingCaseId: string;
  itemType: string;
  title: string;
  description?: string;
  status: string;
  requiredDocumentType?: string;
  requiredCertificateId?: number;
  sourceRuleId?: string;
  isMandatory: boolean;
  sortOrder: number;
  completedAt?: string;
  completedBy?: string;
  completionNotes?: string;
  waivedBy?: string;
  waiverReason?: string;
}

export interface CreateOnboardingCaseRequest {
  crewMemberId: string;
  referenceVesselId?: string;
  referenceVesselName?: string;
  vesselGroupCode?: string;
  flagState?: string;
  dueDate?: string;
  notes?: string;
}

export interface UpdateChecklistItemRequest {
  status?: string;
  completionNotes?: string;
}

export interface WaiveChecklistItemRequest {
  waiverReason: string;
}

// --- Document Workflow ---

export interface DocumentSubmission {
  id: string;
  crewMemberId: string;
  crewName?: string;
  documentType: string;
  documentTitle?: string;
  documentNumber?: string;
  issuingAuthority?: string;
  issueDate?: string;
  expiryDate?: string;
  issuingCountryId?: number;
  issuingCountryName?: string;
  status: string;
  statusChangedAt?: string;
  isActiveSubmission: boolean;
  sensitivityLevel: string;
  submittedBy?: string;
  submittedAt?: string;
  createdAt: string;
  currentVersion?: DocumentVersion;
  totalVersions: number;
  verificationStatus?: string;
  lastVerifiedAt?: string;
}

export interface DocumentVersion {
  id: string;
  submissionId: string;
  versionNumber: number;
  originalFileName?: string;
  contentType?: string;
  fileSizeBytes?: number;
  isActiveVersion: boolean;
  isLocked: boolean;
  uploadedBy?: string;
  uploadedAt: string;
}

export interface VerificationTask {
  id: string;
  submissionId: string;
  versionId: string;
  assignedTo?: string;
  priority: string;
  status: string;
  dueAt?: string;
  completedAt?: string;
  outcome?: string;
  createdAt: string;
  documentType?: string;
  crewName?: string;
  crewMemberId?: string;
  actions: VerificationAction[];
}

export interface VerificationAction {
  id: string;
  actionType: string;
  reasonCode?: string;
  comment?: string;
  performedBy: string;
  performedAt: string;
}

export interface CreateDocumentSubmissionRequest {
  crewMemberId: string;
  documentType: string;
  documentTitle?: string;
  documentNumber?: string;
  issuingAuthority?: string;
  issueDate?: string;
  expiryDate?: string;
  issuingCountryId?: number;
  onboardingCaseId?: string;
  sensitivityLevel?: string;
}

export interface SubmitForVerificationRequest {
  priority?: string;
  assignTo?: string;
}

export interface PerformVerificationRequest {
  actionType: string;
  reasonCode?: string;
  comment?: string;
}

export interface AddVersionRequest {
  originalFileName?: string;
  contentType?: string;
  fileSizeBytes?: number;
  checksum?: string;
}

// --- Crew Status ---

export interface ChangeCrewStatusRequest {
  newStatus: string;
  reason?: string;
}

export interface CrewStatusHistory {
  id: string;
  crewMemberId: string;
  fromStatus: string;
  toStatus: string;
  reason?: string;
  changedBy: string;
  changedAt: string;
}

// --- Audit Log ---

export interface AuditLog {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  actor: string;
  sourceChannel?: string;
  details?: string;
  correlationId?: string;
  timestamp: string;
}

export interface AuditLogQuery {
  entityType?: string;
  entityId?: string;
  actor?: string;
  action?: string;
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}

// --- Status Constants ---

export const CrewStatus = {
  DRAFT: 'Draft',
  ACTIVE: 'Active',
  INACTIVE: 'Inactive',
  RETIRED: 'Retired',
  SUSPENDED: 'Suspended',
} as const;

export const OnboardingCaseStatus = {
  DRAFT: 'Draft',
  INVITED: 'Invited',
  IN_PROGRESS: 'InProgress',
  PENDING_REVIEW: 'PendingReview',
  RETURNED: 'ReturnedForCompletion',
  APPROVED: 'Approved',
  ACTIVATED: 'Activated',
  CANCELLED: 'Cancelled',
} as const;

export const ChecklistItemStatus = {
  PENDING: 'Pending',
  IN_PROGRESS: 'InProgress',
  COMPLETED: 'Completed',
  WAIVED: 'Waived',
  NOT_APPLICABLE: 'NotApplicable',
} as const;

export const DocumentSubmissionStatus = {
  DRAFT: 'Draft',
  SUBMITTED: 'Submitted',
  SENT_FOR_VERIFICATION: 'SentForVerification',
  UNDER_REVIEW: 'UnderReview',
  VERIFIED: 'Verified',
  REJECTED: 'Rejected',
  EXPIRED: 'Expired',
  ARCHIVED: 'Archived',
  SUPERSEDED: 'Superseded',
} as const;

export const VerificationPriority = {
  NORMAL: 'Normal',
  URGENT: 'Urgent',
  CRITICAL: 'Critical',
} as const;
