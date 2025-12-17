export interface LogbookPaginationDto {
  page: number;
  pageSize: number;
  fromDate?: string;
  toDate?: string;
  searchTerm?: string;
}

export interface PaginatedLogbookResponse<T> {
  data: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
}

// Deck Log
export interface CreateDeckLogEntryDto {
  logDateTime: string;
  watchPeriod: string;
  officerOnWatch: string;
  entryType: string;
  description: string;
  latitude?: number;
  longitude?: number;
  courseOverGround?: number;
  speedOverGround?: number;
  heading?: number;
  windDirection?: string;
  windSpeed?: number;
  seaState?: string;
  visibility?: string;
  barometricPressure?: number;
  airTemperature?: number;
  seaTemperature?: number;
  drillType?: string;
  drillSuccessful?: boolean;
  crewOnBoard?: number;
  crewChanges?: string;
  portName?: string;
  portArrivalTime?: string;
  portDepartureTime?: string;
  pilotName?: string;
  pilotOnBoard?: string;
  pilotOffBoard?: string;
  remarks?: string;
}

export interface DeckLogEntryResponseDto extends CreateDeckLogEntryDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

// Engine Log
export interface CreateEngineLogEntryDto {
  logDateTime: string;
  watchPeriod: string;
  engineerOnWatch: string;
  mainEngineStatus: string;
  mainEngineRPM?: number;
  mainEngineLoad?: number;
  mainEngineCoolantTemp?: number;
  mainEngineExhaustTemp?: number;
  mainEngineLubeOilPressure?: number;
  mainEngineLubeOilTemp?: number;
  mainEngineRunningHours?: number;
  fuelOilConsumedME?: number;
  fuelOilConsumedAE?: number;
  fuelOilConsumedBoiler?: number;
  lubeOilConsumed?: number;
  fuelUnit?: string;
  auxEngine1Running?: boolean;
  auxEngine1RunningHours?: number;
  auxEngine1Load?: number;
  auxEngine2Running?: boolean;
  auxEngine2RunningHours?: number;
  auxEngine2Load?: number;
  auxEngine3Running?: boolean;
  auxEngine3RunningHours?: number;
  auxEngine3Load?: number;
  boilerInOperation?: boolean;
  boilerPressure?: number;
  boilerWaterLevel?: number;
  fuelOilROB?: number;
  fuelOilTransfers?: string;
  hasAlarms?: boolean;
  alarmsDescription?: string;
  maintenanceActivities?: string;
  remarks?: string;
}

export interface EngineLogEntryResponseDto extends CreateEngineLogEntryDto {
  id: string;
  chiefEngineerSignature?: string;
  chiefEngineerRemarks?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

// Oil Record
export interface CreateOilRecordEntryDto {
  entryDate: string;
  operationCode: string;
  operationDescription: string;
  locationLat?: number;
  locationLon?: number;
  quantity?: number;
  quantityUnit?: string;
  tankFrom?: string;
  tankTo?: string;
  officerInCharge: string;
  remarks?: string;
}

export interface OilRecordEntryResponseDto extends CreateOilRecordEntryDto {
  id: string;
  masterSignature?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

export interface SignLogbookDto {
  signature: string;
  signedAt: string;
  remarks?: string;
}

// Watchkeeping Log (SOLAS Chapter V/28, STCW Convention, MLC 2006)
export interface CreateWatchkeepingLogDto {
  watchDate: string;
  watchPeriod: string; // 00-04, 04-08, 08-12, 12-16, 16-20, 20-24
  watchType: string; // NAVIGATION, ENGINE
  officerOnWatch: string;
  reliefOfficer?: string; // Officer taking over watch
  lookout?: string;
  
  // STCW Rest Hours Compliance (Mandatory)
  workHours?: number; // Hours worked this watch (default 4h)
  restHoursLast24h?: number; // Minimum 10 hours in any 24-hour period
  restHoursLast7Days?: number; // Minimum 77 hours in any 7-day period
  restHoursCompliant?: boolean; // Auto-calculated compliance
  restHoursException?: string; // If non-compliant, reason must be recorded
  
  // Weather & Navigation
  weatherConditions?: string;
  seaState?: string; // Douglas Sea Scale: Calm, Smooth, Slight, Moderate, Rough, Very Rough, High, Very High, Phenomenal
  visibility?: string; // Good (>5nm), Moderate (2-5nm), Poor (0.5-2nm), Fog (<0.5nm)
  courseLogged?: number;
  speedLogged?: number;
  positionLat?: number;
  positionLon?: number;
  distanceRun?: number;
  
  // Bridge Equipment Status
  engineStatus?: string;
  radarOperational?: boolean;
  ecdisOperational?: boolean;
  aisOperational?: boolean;
  gyroOperational?: boolean;
  autopilotEngaged?: boolean;
  equipmentDefects?: string; // Any navigation equipment failures
  
  // GMDSS Watch (SOLAS Chapter IV)
  gmdssWatchMaintained?: boolean;
  navigationWarningsReceived?: string; // NAVTEX, SafetyNET messages
  
  // Watch Events & Handover
  notableEvents?: string;
  handoverNotes?: string; // Notes for relieving officer (mandatory)
  handoverChecklistCompleted?: boolean;
  watchStartTime?: string;
  watchEndTime?: string;
  
  // Bridge Manning (STCW)
  bridgeManningLevel?: number; // Number of persons on bridge
  lookoutPosted?: boolean; // Mandatory during hours of darkness
  
  // Fatigue Management (MLC 2006)
  fatigueRiskLevel?: 'LOW' | 'MEDIUM' | 'HIGH';
  fatigueAssessmentDone?: boolean;
}

export interface WatchkeepingLogResponseDto extends CreateWatchkeepingLogDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

// Rest Hours Compliance Check DTO
export interface RestHoursComplianceDto {
  officerName: string;
  restHoursLast24h: number;
  restHoursLast7Days: number;
  isCompliant24h: boolean; // >= 10 hours
  isCompliant7Days: boolean; // >= 77 hours
  isOverallCompliant: boolean;
  complianceMessage?: string;
}

// Garbage Record
export interface CreateGarbageRecordDto {
  operationDateTime: string;
  operationCode: string;
  garbageCategory: string;
  description: string;
  quantity: number;
  quantityUnit: string;
  latitude?: number;
  longitude?: number;
  portName?: string;
  receptionFacility?: string;
  receiptNumber?: string;
  incinerationStartTime?: string;
  incinerationEndTime?: string;
  incineratorDetails?: string;
  accidentalDischargeReason?: string;
  accidentalDischargeMeasures?: string;
  officerInCharge: string;
  remarks?: string;
}

export interface GarbageRecordResponseDto extends CreateGarbageRecordDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

// Ballast Water Record
export interface CreateBallastWaterRecordDto {
  operationDateTime: string;
  operationCode: string;
  operationDescription: string;
  ballastTank: string;
  volume: number;
  startLatitude: number;
  startLongitude: number;
  endLatitude?: number;
  endLongitude?: number;
  waterDepth?: number;
  distanceFromLand?: number;
  exchangeMethod?: string;
  exchangeVolumePercent?: number;
  treatmentSystemUsed: boolean;
  treatmentSystemType?: string;
  treatmentSuccessful?: boolean;
  salinityBefore?: number;
  salinityAfter?: number;
  portName?: string;
  receptionFacility?: string;
  officerInCharge: string;
  remarks?: string;
}

export interface BallastWaterRecordResponseDto extends CreateBallastWaterRecordDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}
// ==================== VOYAGE LOG ====================
// Nhật ký Hành trình (SOLAS Chapter V)

export interface VoyageLogEventInfo {
  code: string;
  nameEn: string;
  nameVi: string;
  icon: string;
  color: string;
  requiresPort: boolean;
}

export interface CreateVoyageLogEntryDto {
  voyageId?: string;
  eventType: string;
  eventDateTime: string;
  eventDateTimeLocal?: string;
  timeZone?: string;
  latitude: number;
  longitude: number;
  portName?: string;
  portLocode?: string;
  portCountry?: string;
  berthNumber?: string;
  distanceToGo?: number;
  distanceFromLast?: number;
  totalVoyageDistance?: number;
  courseOverGround?: number;
  speedOverGround?: number;
  pilotName?: string;
  pilotStation?: string;
  officerOnWatch: string;
  remarks?: string;
}

export interface VoyageLogEntryResponseDto extends CreateVoyageLogEntryDto {
  id: string;
  masterSignature?: string;
  signedAt?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
}

export interface SignVoyageLogEntryDto {
  signature: string;
  remarks?: string;
}

export interface VoyageLogQueryDto {
  page?: number;
  pageSize?: number;
  fromDate?: string;
  toDate?: string;
  eventType?: string;
  voyageId?: string;
  portLocode?: string;
}

export interface VoyageLogTimelineItem {
  id: string;
  eventType: string;
  eventName: string;
  eventIcon: string;
  eventColor: string;
  eventDateTime: string;
  location: string;
  details?: string;
  isSigned: boolean;
}

// Voyage Log Event Types Constants
export const VOYAGE_LOG_EVENT_TYPES = {
  // Port Events
  DEP: { code: 'DEP', nameEn: 'Departure', nameVi: 'Rời cảng', icon: '🚢', color: '#22c55e', requiresPort: true },
  ARR: { code: 'ARR', nameEn: 'Arrival', nameVi: 'Cập cảng', icon: '⚓', color: '#3b82f6', requiresPort: true },
  ANCHOR_DROP: { code: 'ANCHOR_DROP', nameEn: 'Anchor Drop', nameVi: 'Thả neo', icon: '⚓', color: '#6b7280', requiresPort: false },
  ANCHOR_UP: { code: 'ANCHOR_UP', nameEn: 'Anchor Up', nameVi: 'Kéo neo', icon: '⚓', color: '#6b7280', requiresPort: false },
  // Passage Events
  COSP: { code: 'COSP', nameEn: 'Commencement of Sea Passage', nameVi: 'Bắt đầu hành trình', icon: '🌊', color: '#8b5cf6', requiresPort: false },
  EOSP: { code: 'EOSP', nameEn: 'End of Sea Passage', nameVi: 'Kết thúc hành trình', icon: '🌊', color: '#8b5cf6', requiresPort: false },
  NOON: { code: 'NOON', nameEn: 'Noon Position', nameVi: 'Vị trí trưa', icon: '☀️', color: '#eab308', requiresPort: false },
  // Pilot Events
  PILOT_ON: { code: 'PILOT_ON', nameEn: 'Pilot Boarding', nameVi: 'Hoa tiêu lên tàu', icon: '👤', color: '#f97316', requiresPort: false },
  PILOT_OFF: { code: 'PILOT_OFF', nameEn: 'Pilot Disembark', nameVi: 'Hoa tiêu rời tàu', icon: '👤', color: '#f97316', requiresPort: false },
  // Special Events
  DRIFT: { code: 'DRIFT', nameEn: 'Drifting', nameVi: 'Trôi dạt', icon: '⏸️', color: '#ef4444', requiresPort: false },
  DEVIATION: { code: 'DEVIATION', nameEn: 'Route Deviation', nameVi: 'Đổi hành trình', icon: '↪️', color: '#f59e0b', requiresPort: false },
} as const;