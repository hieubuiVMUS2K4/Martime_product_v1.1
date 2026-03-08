// ============================================================
// Compliance Matrix Types — matches backend DTOs
// ============================================================

// --- Rule Sets ---

export interface ComplianceRuleSet {
  id: string;
  name: string;
  code?: string;
  description?: string;
  authority?: string;
  isActive: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  sortOrder: number;
  ruleCount: number;
  createdAt: string;
  rules: ComplianceRule[];
}

export interface CreateRuleSetRequest {
  name: string;
  code?: string;
  description?: string;
  authority?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
  sortOrder?: number;
}

export interface UpdateRuleSetRequest {
  name?: string;
  description?: string;
  authority?: string;
  isActive?: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  sortOrder?: number;
}

// --- Rules ---

export interface ComplianceRule {
  id: string;
  ruleSetId: string;
  ruleSetName?: string;
  title: string;
  description?: string;
  requirementType: string;
  requiredCertificateId?: number;
  requiredCertificateName?: string;
  requiredDocumentType?: string;
  severity: string;
  evaluationStage: string;
  minDaysBeforeExpiry?: number;
  gracePeriodDays?: number;
  renewWindowDays?: number;
  waiverAllowed: boolean;
  waiverApproverRole?: string;
  allowEquivalent: boolean;
  equivalentCertificateIds?: string;
  isActive: boolean;
  sortOrder: number;
  uiMessage?: string;
  explainabilityText?: string;
  dimensions: ComplianceDimension[];
}

export interface CreateRuleRequest {
  ruleSetId: string;
  title: string;
  description?: string;
  requirementType?: string;
  requiredCertificateId?: number;
  requiredDocumentType?: string;
  severity?: string;
  evaluationStage?: string;
  minDaysBeforeExpiry?: number;
  gracePeriodDays?: number;
  renewWindowDays?: number;
  waiverAllowed?: boolean;
  waiverApproverRole?: string;
  allowEquivalent?: boolean;
  equivalentCertificateIds?: string;
  uiMessage?: string;
  explainabilityText?: string;
  dimensions?: CreateDimensionRequest[];
}

// --- Dimensions ---

export interface ComplianceDimension {
  id: string;
  dimensionType: string;
  operator: string;
  value: string;
}

export interface CreateDimensionRequest {
  dimensionType: string;
  operator?: string;
  value: string;
}

// --- Waivers ---

export interface ComplianceWaiver {
  id: string;
  ruleId: string;
  ruleTitle?: string;
  ruleSetName?: string;
  crewMemberId: string;
  crewName?: string;
  vesselId?: string;
  status: string;
  reason: string;
  conditions?: string;
  requestedAt: string;
  requestedBy: string;
  approvedAt?: string;
  approvedBy?: string;
  approvalNotes?: string;
  validFrom?: string;
  validTo?: string;
}

export interface CreateWaiverRequest {
  ruleId: string;
  crewMemberId: string;
  vesselId?: string;
  reason: string;
  conditions?: string;
  validFrom?: string;
  validTo?: string;
}

export interface ApproveWaiverRequest {
  approvalNotes?: string;
}

// --- Evaluation ---

export interface ComplianceEvaluation {
  crewMemberId: string;
  crewName?: string;
  vesselId?: string;
  vesselName?: string;
  overallResult: string;
  evaluationStage?: string;
  totalRules: number;
  rulesMet: number;
  rulesNotMet: number;
  rulesWarning: number;
  rulesWaived: number;
  nextExpiryDate?: string;
  evaluatedAt: string;
  items: ComplianceEvaluationItem[];
}

export interface ComplianceEvaluationItem {
  ruleId: string;
  ruleTitle: string;
  ruleSetName?: string;
  requirementType: string;
  severity: string;
  result: string; // Met, MetExpiringSoon, NotMet, Waived, NotApplicable
  matchedDocumentInfo?: string;
  expiryDate?: string;
  daysUntilExpiry?: number;
  uiMessage?: string;
  explainabilityText?: string;
  waiverId?: string;
}

export interface SimulationRequest {
  crewMemberId: string;
  vesselId?: string;
  evaluationStage?: string;
  simulatedDate?: string;
}

// --- Snapshots ---

export interface ComplianceSnapshot {
  id: string;
  crewMemberId: string;
  crewName?: string;
  vesselId?: string;
  overallResult: string;
  totalRules: number;
  rulesMet: number;
  rulesNotMet: number;
  rulesWarning: number;
  rulesWaived: number;
  evaluatedAt: string;
  nextExpiryDate?: string;
}

// --- Constants ---

export const RuleSeverity = {
  BLOCKER: 'Blocker',
  WARNING: 'Warning',
  INFO: 'Info',
} as const;

export const EvaluationStage = {
  ONBOARDING: 'Onboarding',
  PRE_TRAVEL: 'PreTravel',
  ON_BOARD: 'OnBoard',
  CONTINUOUS: 'Continuous',
} as const;

export const EvaluationResult = {
  ELIGIBLE: 'Eligible',
  ELIGIBLE_WITH_WARNINGS: 'EligibleWithWarnings',
  NOT_ELIGIBLE: 'NotEligible',
  ELIGIBLE_BY_WAIVER: 'EligibleByWaiver',
} as const;

export const ItemResult = {
  MET: 'Met',
  MET_EXPIRING_SOON: 'MetExpiringSoon',
  NOT_MET: 'NotMet',
  WAIVED: 'Waived',
  NOT_APPLICABLE: 'NotApplicable',
} as const;

export const WaiverStatus = {
  PENDING: 'Pending',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
  EXPIRED: 'Expired',
} as const;
