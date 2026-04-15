// ============================================================
// Crew Management Types — matches backend Shore API DTOs
// ============================================================

// --- Core Entities ---

export interface CrewMember {
  id: string;
  crewId: string;
  fullName: string;
  rankId?: number;
  rankName?: string;
  rankCode?: string;
  department?: string;
  countryId?: number;
  countryName?: string;
  dateOfBirth?: string;
  joinDate?: string;
  embarkDate?: string;
  disembarkDate?: string;
  contractEnd?: string;
  isOnboard: boolean;
  onboardStatus?: string;
  vesselId?: string;
  vesselName?: string;
  emergencyContact?: string;
  emailAddress?: string;
  phoneNumber?: string;
  address?: string;
  placeOfBirth?: string;
  idCardNumber?: string;
  maritalStatus?: string;
  height?: number;
  weight?: number;
  bloodGroup?: string;
  clothingSize?: string;
  shoeSize?: string;
  cateringSize?: string;
  isSmoker?: boolean;
  isCovidVaccinated?: boolean;
  nextOfKinName?: string;
  nextOfKinRelation?: string;
  nextOfKinPhone?: string;
  nextOfKinAddress?: string;
  educationInstitution?: string;
  educationCourse?: string;
  educationPeriodYears?: number;
  educationGraduationYear?: number;
  notes?: string;
  avatarUrl?: string;
  originNode?: string;
  isSynced?: boolean;
  createdAt?: string;
  updatedAt?: string;
  // Review workflow fields (synced from edge)
  onboardStatusChangedAt?: string;
  onboardStatusChangedBy?: string;
  reviewChecklist?: string;
  reviewNotes?: string;
  edgeChanges?: string;
  edgeChangesViewed?: boolean;
}

export interface CrewDetail extends CrewMember {
  certificates?: CrewCertificate[];
  passportNumber?: string;
  passportExpiry?: string;
  seamanBookNumber?: string;
}

// --- Certificates ---

export interface CertificateType {
  id: number;
  certificateCode: string;
  certificateName: string;
  category?: string;
  validityPeriodMonths?: number;
  description?: string;
  isMandatory: boolean;
  isActive: boolean;
  isRequiredForRank?: boolean;
  createdAt?: string;
}

export interface CrewCertificate {
  id: number;
  crewMemberId: string;
  crewMemberName?: string;
  vesselId?: string;
  certificateId: number;
  certificateName?: string;
  certificateCode?: string;
  category?: string;
  certificateNumber?: string;
  issueDate?: string;
  expiryDate?: string;
  issuingAuthority?: string;
  certificateOfCompetency?: string;
  countryId?: number;
  countryName?: string;
  notes?: string;
  status?: string;
  daysUntilExpiry?: number;
  fileUrl?: string;
  documentFilePath?: string;
  originNode?: string;
  isSynced?: boolean;
}

// --- Documents ---

export interface CrewDocument {
  id: string;
  crewMemberId: string;
  documentType: string;
  documentNumber?: string;
  issueDate?: string;
  expiryDate?: string;
  countryId?: number;
  countryName?: string;
  fileUrl?: string;
  notes?: string;
  category: string;
  createdAt?: string;
  updatedAt?: string;
}

// --- Service Records ---

export interface ServiceRecord {
  id: string;
  crewMemberId: string;
  vesselName: string;
  vesselIMO?: string;
  rankDuringService?: string;
  signOnDate: string;
  signOffDate?: string;
  tradingArea?: string;
  remarks?: string;
  createdAt?: string;
}

// --- Reference Data ---

export interface Rank {
  id: number;
  rankCode: string;
  rankName: string;
  department?: string;
  sortOrder?: number;
}

export interface Country {
  id: number;
  countryCode: string;
  countryName: string;
  flagImageUrl?: string;
}

// --- Request DTOs ---

export interface CreateCrewRequest {
  crewId: string;
  fullName: string;
  rankId?: number;
  department?: string;
  countryId?: number;
  dateOfBirth?: string;
  joinDate?: string;
  embarkDate?: string;
  contractEnd?: string;
  isOnboard?: boolean;
  vesselId?: string;
  emergencyContact?: string;
  emailAddress?: string;
  phoneNumber?: string;
  address?: string;
  placeOfBirth?: string;
  idCardNumber?: string;
  maritalStatus?: string;
  height?: number;
  weight?: number;
  bloodGroup?: string;
  clothingSize?: string;
  shoeSize?: string;
  cateringSize?: string;
  isSmoker?: boolean;
  isCovidVaccinated?: boolean;
  nextOfKinName?: string;
  nextOfKinRelation?: string;
  nextOfKinPhone?: string;
  nextOfKinAddress?: string;
  educationInstitution?: string;
  educationCourse?: string;
  educationPeriodYears?: number;
  educationGraduationYear?: number;
  notes?: string;
}

export type UpdateCrewRequest = Partial<CreateCrewRequest>;

export interface CrewCertificateRequest {
  crewMemberId: string;
  certificateId: number;
  certificateNumber?: string;
  issueDate?: string;
  expiryDate?: string;
  issuingAuthority?: string;
  certificateOfCompetency?: string;
  countryId?: number;
  notes?: string;
}

export interface CreateServiceRecordRequest {
  vesselName: string;
  vesselIMO?: string;
  rankDuringService?: string;
  signOnDate: string;
  signOffDate?: string;
  tradingArea?: string;
  remarks?: string;
}

export interface CreateDocumentRequest {
  documentType: string;
  documentNumber?: string;
  issueDate?: string;
  expiryDate?: string;
  countryId?: number;
  fileUrl?: string;
  notes?: string;
}

// --- API Response wrappers ---

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  totalPages: number;
  page: number;
  pageSize: number;
}

export interface ComplianceReport {
  crewMemberId: string;
  crewMemberName: string;
  rankName?: string;
  totalRequired: number;
  totalHeld: number;
  compliancePercentage: number;
  missingCertificates: string[];
  expiringCertificates: string[];
}

// --- UI State Types ---

export interface CrewFilters {
  search: string;
  isOnboard?: boolean | null;
  department?: string;
  rankId?: number | null;
  vesselId?: string | null;
  rankName?: string;
  vesselName?: string;
  page: number;
  pageSize: number;
}

export type CertificateStatusType = 'VALID' | 'EXPIRING_SOON' | 'EXPIRED' | 'SUSPENDED' | 'REVOKED';

export type TabKey = 'personal' | 'certificates' | 'documents' | 'service-history';

export interface VesselSimple {
  id: string;
  name: string;
  imo: string;
}
