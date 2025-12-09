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

// Watchkeeping Log
export interface CreateWatchkeepingLogDto {
  watchDate: string;
  watchPeriod: string;
  watchType: string;
  officerOnWatch: string;
  lookout?: string;
  weatherConditions?: string;
  seaState?: string;
  visibility?: string;
  courseLogged?: number;
  speedLogged?: number;
  positionLat?: number;
  positionLon?: number;
  distanceRun?: number;
  engineStatus?: string;
  notableEvents?: string;
}

export interface WatchkeepingLogResponseDto extends CreateWatchkeepingLogDto {
  id: string;
  masterSignature?: string;
  isSynced: boolean;
  createdAt: string;
  updatedAt: string;
  originNode: string;
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
