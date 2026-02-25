// ============================================================
// ABSTRACT LOG (Nhật ký vắn tắt) - Type Definitions
// Based on edge-services DTOs (AbstractLogDtos.cs)
// ============================================================

// ========== ENUMS ==========

export type AbstractLogStatus = 'DRAFT' | 'FINALIZED'

// ========== DAILY ENTRY ==========

export interface AbstractLogDailyEntry {
  id: string
  abstractLogLegId: string
  dayNumber: number
  entryDate: string
  noonLatitude?: number
  noonLongitude?: number
  windDirectionTrue?: string
  windDirectionRelative?: string
  windForceBeaufort?: number
  seaState?: string
  hoursUnderWay?: number
  hoursPropelling?: number
  hoursDrifting?: number
  hoursAnchor?: number
  hoursPort?: number
  timeZoneChange?: number
  distanceEngine?: number
  distanceLog?: number
  distanceOG?: number
  speedLog?: number
  speedOG?: number
  slipPercent?: number
  avgRPM?: number
  // FOC Propelling (Hp)
  hpMeHsfo?: number
  hpMeVlsfo?: number
  hpMeLsmgo?: number
  hpDeHsfo?: number
  hpDeVlsfo?: number
  hpDeLsmgo?: number
  hpBoilerHsfo?: number
  hpBoilerVlsfo?: number
  hpBoilerLsmgo?: number
  // FOC Detention (Dt)
  dtMeHsfo?: number
  dtMeVlsfo?: number
  dtMeLsmgo?: number
  dtDeHsfo?: number
  dtDeVlsfo?: number
  dtDeLsmgo?: number
  dtBoilerHsfo?: number
  dtBoilerVlsfo?: number
  dtBoilerLsmgo?: number
  // FOC Port
  portMeHsfo?: number
  portMeVlsfo?: number
  portMeLsmgo?: number
  portDeHsfo?: number
  portDeVlsfo?: number
  portDeLsmgo?: number
  portBoilerHsfo?: number
  portBoilerVlsfo?: number
  portBoilerLsmgo?: number
  // Lub Oil & FW
  cylOilConsumed?: number
  sysOilConsumed?: number
  fwProduced?: number
  fwConsumed?: number
  remarks?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateAbstractLogDailyEntryDto {
  entryDate: string
  noonLatitude?: number
  noonLongitude?: number
  windDirectionTrue?: string
  windDirectionRelative?: string
  windForceBeaufort?: number
  seaState?: string
  hoursUnderWay?: number
  hoursPropelling?: number
  hoursDrifting?: number
  hoursAnchor?: number
  hoursPort?: number
  timeZoneChange?: number
  distanceEngine?: number
  distanceLog?: number
  distanceOG?: number
  speedLog?: number
  speedOG?: number
  slipPercent?: number
  avgRPM?: number
  hpMeHsfo?: number
  hpMeVlsfo?: number
  hpMeLsmgo?: number
  hpDeHsfo?: number
  hpDeVlsfo?: number
  hpDeLsmgo?: number
  hpBoilerHsfo?: number
  hpBoilerVlsfo?: number
  hpBoilerLsmgo?: number
  dtMeHsfo?: number
  dtMeVlsfo?: number
  dtMeLsmgo?: number
  dtDeHsfo?: number
  dtDeVlsfo?: number
  dtDeLsmgo?: number
  dtBoilerHsfo?: number
  dtBoilerVlsfo?: number
  dtBoilerLsmgo?: number
  portMeHsfo?: number
  portMeVlsfo?: number
  portMeLsmgo?: number
  portDeHsfo?: number
  portDeVlsfo?: number
  portDeLsmgo?: number
  portBoilerHsfo?: number
  portBoilerVlsfo?: number
  portBoilerLsmgo?: number
  // Lub Oil & FW
  cylOilConsumed?: number
  sysOilConsumed?: number
  fwProduced?: number
  fwConsumed?: number
  remarks?: string
}

export type UpdateAbstractLogDailyEntryDto = CreateAbstractLogDailyEntryDto

// ========== LEG ==========

export interface AbstractLogLeg {
  id: string
  abstractLogVoyageId: string
  legNumber: number
  legLabel?: string
  sequence: number
  departurePort?: string
  departureTime?: string
  departureDraftFore?: number
  departureDraftAft?: number
  departureDraftMean?: number
  arrivalPort?: string
  arrivalTime?: string
  arrivalDraftFore?: number
  arrivalDraftAft?: number
  arrivalDraftMean?: number
  hoursPropelling?: number
  hoursUnderWay?: number
  hoursDrifting?: number
  hoursAnchor?: number
  hoursPort?: number
  distanceProp?: number
  distanceLog?: number
  distanceOG?: number
  speedLog?: number
  speedOG?: number
  slipPercent?: number
  shaftRevolutions?: number
  meFocHsfo?: number
  meFocVlsfo?: number
  meFocLsmgo?: number
  deFocHsfo?: number
  deFocVlsfo?: number
  deFocLsmgo?: number
  boilerFocHsfo?: number
  boilerFocVlsfo?: number
  boilerFocLsmgo?: number
  // Cargo Info
  cargoType?: string
  cargoQuantity?: number
  loadCondition?: 'LADEN' | 'BALLAST' | 'PART_LADEN'
  createdAt: string
  updatedAt?: string
  dailyEntries: AbstractLogDailyEntry[]
}

export interface CreateAbstractLogLegDto {
  departurePort?: string
  departureTime?: string
  departureDraftFore?: number
  departureDraftAft?: number
  arrivalPort?: string
  arrivalTime?: string
  arrivalDraftFore?: number
  arrivalDraftAft?: number
  cargoType?: string
  cargoQuantity?: number
  loadCondition?: 'LADEN' | 'BALLAST' | 'PART_LADEN'
}

export interface UpdateAbstractLogLegDto {
  departurePort?: string
  departureTime?: string
  departureDraftFore?: number
  departureDraftAft?: number
  departureDraftMean?: number
  arrivalPort?: string
  arrivalTime?: string
  arrivalDraftFore?: number
  arrivalDraftAft?: number
  arrivalDraftMean?: number
  hoursPropelling?: number
  hoursUnderWay?: number
  hoursDrifting?: number
  hoursAnchor?: number
  hoursPort?: number
  distanceProp?: number
  distanceLog?: number
  distanceOG?: number
  speedLog?: number
  speedOG?: number
  slipPercent?: number
  shaftRevolutions?: number
  meFocHsfo?: number
  meFocVlsfo?: number
  meFocLsmgo?: number
  deFocHsfo?: number
  deFocVlsfo?: number
  deFocLsmgo?: number
  boilerFocHsfo?: number
  boilerFocVlsfo?: number
  boilerFocLsmgo?: number
  cargoType?: string
  cargoQuantity?: number
  loadCondition?: 'LADEN' | 'BALLAST' | 'PART_LADEN'
}

// ========== VOYAGE (top-level) ==========

export interface AbstractLogVoyage {
  id: string
  voyageId: string
  voyageNumber: string
  shipName: string
  imoNumber?: string
  masterName?: string
  chiefEngineerName?: string
  reportDate?: string
  dateOfLastDocking?: string
  propellerPitch?: string
  commencementTime?: string
  completionTime?: string
  grandTotalHours?: number
  // Fuel ROB Reconciliation
  foRobPrevious?: number
  foReceived?: number
  foConsumedTotal?: number
  foRobCurrent?: number
  doRobPrevious?: number
  doReceived?: number
  doConsumedTotal?: number
  doRobCurrent?: number
  cylOilRobPrevious?: number
  cylOilReceived?: number
  cylOilConsumed?: number
  cylOilRobCurrent?: number
  sysOilRobPrevious?: number
  sysOilReceived?: number
  sysOilConsumed?: number
  sysOilRobCurrent?: number
  genOilRobPrevious?: number
  genOilReceived?: number
  genOilConsumed?: number
  genOilRobCurrent?: number
  fwRobPrevious?: number
  fwProduced?: number
  fwConsumed?: number
  fwRobCurrent?: number
  // Signatures
  masterSignature?: string
  masterSignedAt?: string
  chiefEngineerSignature?: string
  chiefEngineerSignedAt?: string
  remarks?: string
  status: AbstractLogStatus
  createdAt: string
  updatedAt?: string
  legs: AbstractLogLeg[]
}

export interface CreateAbstractLogDto {
  voyageId: string
}

export interface UpdateAbstractLogVoyageDto {
  masterName?: string
  chiefEngineerName?: string
  reportDate?: string
  dateOfLastDocking?: string
  propellerPitch?: string
  foRobPrevious?: number
  foReceived?: number
  foConsumedTotal?: number
  foRobCurrent?: number
  doRobPrevious?: number
  doReceived?: number
  doConsumedTotal?: number
  doRobCurrent?: number
  cylOilRobPrevious?: number
  cylOilReceived?: number
  cylOilConsumed?: number
  cylOilRobCurrent?: number
  sysOilRobPrevious?: number
  sysOilReceived?: number
  sysOilConsumed?: number
  sysOilRobCurrent?: number
  genOilRobPrevious?: number
  genOilReceived?: number
  genOilConsumed?: number
  genOilRobCurrent?: number
  fwRobPrevious?: number
  fwProduced?: number
  fwConsumed?: number
  fwRobCurrent?: number
  // Signatures
  masterSignature?: string
  chiefEngineerSignature?: string
  remarks?: string
  status?: AbstractLogStatus
}

// ========== LIST ITEM ==========

export interface AbstractLogListItem {
  id: string
  voyageId: string
  voyageNumber: string
  shipName: string
  commencementTime?: string
  completionTime?: string
  grandTotalHours?: number
  status: AbstractLogStatus
  legCount: number
  dailyEntryCount: number
  createdAt: string
}
