// ============================================================
// EXTERNAL REQUEST TYPES
// ============================================================

export interface ExternalRequest {
  id: string;
  vesselId: string;
  vesselName?: string;
  assignmentId?: string;
  rankId: number;
  rankName?: string;
  agencyName?: string;
  agencyEmail?: string;
  requiredCount: number;
  nationalityPreference?: string;
  requiredByDate?: string;
  responseSlaDate?: string;
  status: string;
  sentAt?: string;
  viewedAt?: string;
  closedAt?: string;
  notes?: string;
  mandatoryDocuments?: string;
  createdAt: string;
  createdBy?: string;
  candidateCount: number;
  shortlistedCount: number;
}

export interface CreateExternalRequestRequest {
  vesselId: string;
  assignmentId?: string;
  rankId: number;
  agencyName: string;
  agencyEmail?: string;
  requiredCount: number;
  nationalityPreference?: string;
  requiredByDate?: string;
  responseSlaDate?: string;
  notes?: string;
  mandatoryDocuments?: string;
}

export interface UpdateExternalRequestRequest {
  agencyName?: string;
  agencyEmail?: string;
  nationalityPreference?: string;
  requiredByDate?: string;
  responseSlaDate?: string;
  notes?: string;
  mandatoryDocuments?: string;
}

export interface ChangeExternalRequestStatusRequest {
  newStatus: string;
  reason?: string;
}

export interface ExternalCandidate {
  id: string;
  externalRequestId: string;
  candidateName: string;
  nationality?: string;
  rankId?: number;
  rankName?: string;
  contactEmail?: string;
  contactPhone?: string;
  status: string;
  complianceResult?: string;
  profileSummary?: string;
  notes?: string;
  submittedAt: string;
  submittedBy?: string;
  reviewedAt?: string;
  reviewedBy?: string;
  linkedCrewMemberId?: string;
}

export interface SubmitCandidateRequest {
  externalRequestId: string;
  candidateName: string;
  nationality?: string;
  rankId?: number;
  contactEmail?: string;
  contactPhone?: string;
  profileSummary?: string;
  notes?: string;
}

export interface ReviewCandidateRequest {
  newStatus: string;
  notes?: string;
}

export interface ExternalRequestMessage {
  id: string;
  externalRequestId: string;
  author: string;
  authorRole?: string;
  content: string;
  postedAt: string;
}

export interface CreateExternalMessageRequest {
  content: string;
}

// ============================================================
// TRAVEL REQUEST TYPES
// ============================================================

export interface TravelRequest {
  id: string;
  assignmentId: string;
  crewMemberId: string;
  crewName?: string;
  vesselName?: string;
  status: string;
  travelType?: string;
  departurePort?: string;
  arrivalPort?: string;
  departureDate?: string;
  arrivalDate?: string;
  reportingDate?: string;
  specialRequirements?: string;
  baggageNotes?: string;
  visaRequirements?: string;
  vendorName?: string;
  bookingReference?: string;
  estimatedCost?: number;
  currency?: string;
  notes?: string;
  createdAt: string;
  segmentCount: number;
  segments: TravelSegment[];
}

export interface CreateTravelRequestRequest {
  assignmentId: string;
  travelType: string;
  departurePort?: string;
  arrivalPort?: string;
  departureDate?: string;
  arrivalDate?: string;
  reportingDate?: string;
  specialRequirements?: string;
  baggageNotes?: string;
  visaRequirements?: string;
  notes?: string;
}

export interface UpdateTravelRequestRequest {
  departurePort?: string;
  arrivalPort?: string;
  departureDate?: string;
  arrivalDate?: string;
  reportingDate?: string;
  specialRequirements?: string;
  baggageNotes?: string;
  visaRequirements?: string;
  vendorName?: string;
  bookingReference?: string;
  estimatedCost?: number;
  currency?: string;
  notes?: string;
}

export interface ChangeTravelStatusRequest {
  newStatus: string;
  reason?: string;
}

export interface TravelSegment {
  id: string;
  travelRequestId: string;
  sequenceOrder: number;
  segmentType: string;
  origin: string;
  destination: string;
  carrierName?: string;
  flightNumber?: string;
  departureTime?: string;
  arrivalTime?: string;
  confirmationNumber?: string;
  notes?: string;
}

export interface CreateTravelSegmentRequest {
  travelRequestId: string;
  sequenceOrder: number;
  segmentType: string;
  origin: string;
  destination: string;
  carrierName?: string;
  flightNumber?: string;
  departureTime?: string;
  arrivalTime?: string;
  confirmationNumber?: string;
  notes?: string;
}

export interface TravelStatusHistory {
  id: string;
  fromStatus: string;
  toStatus: string;
  changedBy: string;
  reason?: string;
  changedAt: string;
}

// ============================================================
// STATUS CONSTANTS
// ============================================================

export const ExternalRequestStatus = {
  Draft: 'Draft',
  Sent: 'Sent',
  Viewed: 'Viewed',
  InProgress: 'InProgress',
  CandidateSubmitted: 'CandidateSubmitted',
  Shortlisted: 'Shortlisted',
  Closed: 'Closed',
  Cancelled: 'Cancelled',
} as const;

export const TravelRequestStatus = {
  Draft: 'Draft',
  Pending: 'Pending',
  BookingInProgress: 'BookingInProgress',
  Booked: 'Booked',
  InTransit: 'InTransit',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Reissued: 'Reissued',
} as const;

export const TravelSegmentType = {
  Flight: 'Flight',
  Ground: 'Ground',
  Ferry: 'Ferry',
  Hotel: 'Hotel',
  Transfer: 'Transfer',
} as const;
