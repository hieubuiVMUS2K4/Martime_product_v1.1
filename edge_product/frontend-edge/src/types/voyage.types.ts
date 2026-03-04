// ============================================================
// VOYAGE, PORT, CREW ASSIGNMENT - Type Definitions
// Based on edge-services DTOs (VoyageDtos.cs)
// ============================================================

// ========== PORT ==========

export interface Port {
  id: number
  portCode: string     // UN/LOCODE (5 chars, e.g. VNSGN)
  portName: string
  country?: string
  countryCode?: string
  latitude?: number
  longitude?: number
  timeZone?: string
  isActive: boolean
}

export interface CreatePortDto {
  portCode: string
  portName: string
  country?: string
  countryCode?: string
  latitude?: number
  longitude?: number
  timeZone?: string
}

export interface UpdatePortDto {
  portName?: string
  country?: string
  countryCode?: string
  latitude?: number
  longitude?: number
  timeZone?: string
  isActive?: boolean
}

export interface PortSearchQuery {
  search?: string
  countryCode?: string
  isActive?: boolean
  page?: number
  pageSize?: number
}

// ========== PORT CALL ==========

export type PortCallType = 'DEPARTURE' | 'ARRIVAL' | 'TRANSIT' | 'BUNKERING' | 'DRYDOCK'

export interface PortCall {
  id: string   // Guid
  voyageId: string
  portId?: number
  portCode?: string
  portName: string
  country?: string
  callType: PortCallType
  sequence: number
  arrivalTime?: string
  departureTime?: string
  berthNumber?: string
  pilotOnBoard?: string
  pilotOffBoard?: string
  draftFore?: number
  draftAft?: number
  cargoOpsCompleted: boolean
  remarks?: string
  createdAt: string
}

export interface CreatePortCallDto {
  voyageId: string
  portId?: number
  portCode?: string
  portName: string
  country?: string
  callType: string
  sequence?: number
  arrivalTime?: string
  departureTime?: string
  berthNumber?: string
  pilotOnBoard?: string
  pilotOffBoard?: string
  draftFore?: number
  draftAft?: number
  cargoOpsCompleted?: boolean
  remarks?: string
}

export interface UpdatePortCallDto {
  portId?: number
  portCode?: string
  portName?: string
  country?: string
  callType?: string
  sequence?: number
  arrivalTime?: string
  departureTime?: string
  berthNumber?: string
  pilotOnBoard?: string
  pilotOffBoard?: string
  draftFore?: number
  draftAft?: number
  cargoOpsCompleted?: boolean
  remarks?: string
}

// ========== VOYAGE CREW ASSIGNMENT ==========

export type CrewRole = 'REGULAR' | 'SUPERNUMERARY' | 'OBSERVER' | 'TRAINEE' | 'RIDER'
export type AssignmentStatus = 'ASSIGNED' | 'ONBOARD' | 'DISEMBARKED' | 'CANCELLED'

export interface VoyageCrewAssignment {
  id: string   // Guid
  voyageId: string
  voyageNumber?: string
  crewMemberId: string
  crewName?: string
  crewId?: string
  rankId?: number
  rankName?: string
  role: CrewRole
  embarkPortCode?: string
  embarkPortName?: string
  embarkDate?: string
  disembarkPortCode?: string
  disembarkPortName?: string
  disembarkDate?: string
  watchSchedule?: string
  status: AssignmentStatus
  remarks?: string
  createdAt: string
}

export interface CreateCrewAssignmentDto {
  voyageId: string
  crewMemberId: string
  rankId?: number
  role?: string
  embarkPortCode?: string
  embarkPortName?: string
  embarkDate?: string
  watchSchedule?: string
  remarks?: string
}

export interface UpdateCrewAssignmentDto {
  rankId?: number
  role?: string
  embarkPortCode?: string
  embarkPortName?: string
  embarkDate?: string
  disembarkPortCode?: string
  disembarkPortName?: string
  disembarkDate?: string
  watchSchedule?: string
  status?: string
  remarks?: string
}

export interface BulkAssignCrewDto {
  voyageId: string
  crewMemberIds: string[]
  embarkPortCode?: string
  embarkPortName?: string
  embarkDate?: string
}

// ========== VOYAGE (Extended) ==========

export interface VoyageDetail {
  id: string   // Guid
  voyageNumber: string
  
  // Vessel Info
  vesselIMO?: string
  vesselName?: string
  vesselFlag?: string
  callSign?: string
  
  // Port Info
  departurePort?: string
  departurePortCode?: string
  departureTime?: string
  arrivalPort?: string
  arrivalPortCode?: string
  arrivalTime?: string
  previousPortCode?: string
  previousPortName?: string
  
  // Performance
  cargoType?: string
  cargoWeight?: number
  distanceTraveled?: number
  fuelConsumed?: number
  averageSpeed?: number
  voyageStatus: string
  
  createdAt: string
  updatedAt: string
  
  // Related data
  portCalls: PortCall[]
  crewAssignments: VoyageCrewAssignment[]
  logEntryCount: number
  cargoOperationCount: number
}

export interface CreateVoyageDto {
  voyageNumber: string
  departurePort?: string
  departurePortCode?: string
  departureTime?: string
  arrivalPort?: string
  arrivalPortCode?: string
  arrivalTime?: string
  previousPortCode?: string
  previousPortName?: string
  cargoType?: string
  cargoWeight?: number
}

export interface UpdateVoyageDto {
  voyageNumber?: string
  departurePort?: string
  departurePortCode?: string
  departureTime?: string
  arrivalPort?: string
  arrivalPortCode?: string
  arrivalTime?: string
  previousPortCode?: string
  previousPortName?: string
  cargoType?: string
  cargoWeight?: number
  distanceTraveled?: number
  fuelConsumed?: number
  averageSpeed?: number
  voyageStatus?: string
}

// ========== FAL FORM 5 ==========

export interface FalForm5 {
  voyageNumber: string
  vesselName?: string
  vesselIMO?: string
  vesselFlag?: string
  callSign?: string
  portOfArrival?: string
  portOfArrivalCode?: string
  dateOfArrival?: string
  arrivedFrom?: string
  crewList: FalCrewEntry[]
}

export interface FalCrewEntry {
  no: number
  fullName?: string
  rank?: string
  nationality?: string
  dateOfBirth?: string
  placeOfBirth?: string
  travelDocumentType?: string
  travelDocumentNumber?: string
}

// ========== CARGO OPERATIONS ==========

export type CargoOperationType = 'LOADING' | 'DISCHARGING'
export type CargoStatus = 'PLANNED' | 'LOADING' | 'LOADED' | 'DISCHARGING' | 'DISCHARGED'

export interface VoyageCargoOperation {
  id: string
  operationId: string
  voyageId?: string
  operationType: CargoOperationType
  cargoType: string
  cargoDescription?: string
  quantity: number
  unit: string
  loadingPort?: string
  dischargePort?: string
  loadedAt?: string
  dischargedAt?: string
  shipper?: string
  consignee?: string
  billOfLading?: string
  sealNumbers?: string
  specialRequirements?: string
  status: CargoStatus
  createdAt: string
}

export interface CreateCargoOperationDto {
  voyageId?: string
  operationType: string
  cargoType: string
  cargoDescription?: string
  quantity: number
  unit: string
  loadingPort?: string
  dischargePort?: string
  loadedAt?: string
  dischargedAt?: string
  shipper?: string
  consignee?: string
  billOfLading?: string
  sealNumbers?: string
  specialRequirements?: string
}

export interface UpdateCargoOperationDto {
  operationType?: string
  cargoType?: string
  cargoDescription?: string
  quantity?: number
  unit?: string
  loadingPort?: string
  dischargePort?: string
  loadedAt?: string
  dischargedAt?: string
  shipper?: string
  consignee?: string
  billOfLading?: string
  sealNumbers?: string
  specialRequirements?: string
  status?: string
}
