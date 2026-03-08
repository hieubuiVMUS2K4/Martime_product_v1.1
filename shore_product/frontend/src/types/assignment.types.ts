// ============================================================
// Assignment & Planning Types — matches backend DTOs
// ============================================================

// --- Enums / Constants ---

export const AssignmentStatus = {
  DRAFT: 'Draft',
  PROPOSED: 'Proposed',
  PENDING_CREW_CONFIRMATION: 'PendingCrewConfirmation',
  CONFIRMED: 'Confirmed',
  TRAVEL_IN_PROGRESS: 'TravelInProgress',
  READY_TO_JOIN: 'ReadyToJoin',
  ON_BOARDED: 'OnBoarded',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled',
  DECLINED: 'Declined',
} as const;

export const ACTIVE_STATUSES = [
  AssignmentStatus.DRAFT,
  AssignmentStatus.PROPOSED,
  AssignmentStatus.PENDING_CREW_CONFIRMATION,
  AssignmentStatus.CONFIRMED,
  AssignmentStatus.TRAVEL_IN_PROGRESS,
  AssignmentStatus.READY_TO_JOIN,
  AssignmentStatus.ON_BOARDED,
];

export const ConflictSeverity = {
  BLOCKER: 'Blocker',
  WARNING: 'Warning',
  INFO: 'Info',
} as const;

export const PositionFillStatus = {
  OPEN: 'Open',
  PROPOSED: 'Proposed',
  FILLED: 'Filled',
  SHORTAGE: 'Shortage',
} as const;

export const ConfirmationResponse = {
  PENDING: 'Pending',
  CONFIRMED: 'Confirmed',
  DECLINED: 'Declined',
} as const;

// --- Manning Standard ---

export interface VesselManningStandard {
  id: string;
  vesselId: string;
  vesselName?: string;
  name: string;
  description?: string;
  documentReference?: string;
  isActive: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  positionCount: number;
  totalRequired: number;
  filledCount: number;
  positions: ManningPosition[];
}

export interface CreateManningStandardRequest {
  vesselId: string;
  name: string;
  description?: string;
  documentReference?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface ManningPosition {
  id: string;
  rankId: number;
  rankName?: string;
  department?: string;
  requiredCount: number;
  allowEquivalent: boolean;
  notes?: string;
  sortOrder: number;
  fillStatus: string;
  currentlyFilled: number;
}

export interface CreateManningPositionRequest {
  manningStandardId: string;
  rankId: number;
  requiredCount?: number;
  allowEquivalent?: boolean;
  notes?: string;
  sortOrder?: number;
}

// --- Crew Assignment ---

export interface CrewAssignment {
  id: string;
  crewMemberId: string;
  crewName?: string;
  crewCode?: string;
  vesselId: string;
  vesselName?: string;
  rankId: number;
  rankName?: string;
  manningPositionId?: string;
  status: string;
  statusChangedAt?: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  joinPortCode?: string;
  joinPortName?: string;
  leavePortCode?: string;
  leavePortName?: string;
  isEquivalentRank: boolean;
  originalRankId?: number;
  originalRankName?: string;
  equivalentRankJustification?: string;
  complianceResult?: string;
  complianceEvaluatedAt?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  conflictCount: number;
  blockerCount: number;
  commentCount: number;
}

export interface CreateAssignmentRequest {
  crewMemberId: string;
  vesselId: string;
  rankId: number;
  manningPositionId?: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
  joinPortCode?: string;
  joinPortName?: string;
  leavePortCode?: string;
  leavePortName?: string;
  isEquivalentRank?: boolean;
  originalRankId?: number;
  equivalentRankJustification?: string;
  notes?: string;
}

export interface UpdateAssignmentRequest {
  plannedStartDate?: string;
  plannedEndDate?: string;
  joinPortCode?: string;
  joinPortName?: string;
  leavePortCode?: string;
  leavePortName?: string;
  notes?: string;
}

export interface ChangeAssignmentStatusRequest {
  newStatus: string;
  reason?: string;
}

// --- Confirmation ---

export interface AssignmentConfirmation {
  id: string;
  assignmentId: string;
  response: string;
  respondedAt?: string;
  respondedBy?: string;
  declineReason?: string;
  notes?: string;
  sentAt: string;
  sentBy?: string;
}

export interface SendConfirmationRequest {
  assignmentId: string;
  notes?: string;
}

export interface RespondConfirmationRequest {
  response: string; // Confirmed | Declined
  declineReason?: string;
  notes?: string;
}

// --- Conflict ---

export interface AssignmentConflict {
  id: string;
  assignmentId: string;
  conflictType: string;
  severity: string;
  description: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  isResolved: boolean;
  resolutionNote?: string;
  detectedAt: string;
  resolvedAt?: string;
}

// --- Comment ---

export interface AssignmentComment {
  id: string;
  assignmentId: string;
  author: string;
  authorRole?: string;
  content: string;
  postedAt: string;
}

export interface CreateCommentRequest {
  content: string;
}

// --- Planning Board ---

export interface VesselPlanningBoard {
  vesselId: string;
  vesselName: string;
  vesselType?: string;
  flag?: string;
  manningStandard?: VesselManningStandard;
  activeAssignments: CrewAssignment[];
  shortageCount: number;
  totalPositions: number;
  filledPositions: number;
}

// --- Candidate Search ---

export interface CandidateSearchRequest {
  vesselId: string;
  rankId: number;
  startDate?: string;
  endDate?: string;
  includeEquivalentRanks?: boolean;
}

export interface Candidate {
  crewMemberId: string;
  crewName: string;
  crewCode?: string;
  rankId: number;
  rankName?: string;
  nationality?: string;
  poolStatus: string;
  complianceResult?: string;
  availableFrom?: string;
  lastDisembark?: string;
  isEquivalentRank: boolean;
  potentialConflicts: AssignmentConflict[];
}

// --- Status History ---

export interface AssignmentStatusHistory {
  id: string;
  fromStatus: string;
  toStatus: string;
  changedBy?: string;
  reason?: string;
  changedAt: string;
}
