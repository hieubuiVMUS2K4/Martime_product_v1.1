// ============================================================
// TYPE DEFINITIONS - Based on EdgeModels.cs
// ============================================================

export interface PositionData {
  id: number
  timestamp: string
  latitude: number
  longitude: number
  altitude?: number
  speedOverGround?: number
  courseOverGround?: number
  magneticVariation?: number
  fixQuality: number
  satellitesUsed: number
  hdop?: number
  source: string
  isSynced: boolean
  createdAt: string
}

export interface NavigationData {
  id: number
  timestamp: string
  headingTrue?: number
  headingMagnetic?: number
  rateOfTurn?: number
  pitch?: number
  roll?: number
  speedThroughWater?: number
  depth?: number
  windSpeedRelative?: number
  windDirectionRelative?: number
  windSpeedTrue?: number
  windDirectionTrue?: number
  isSynced: boolean
  createdAt: string
}

export interface EngineData {
  id: number
  timestamp: string
  engineId: string
  rpm?: number
  loadPercent?: number
  coolantTemp?: number
  exhaustTemp?: number
  lubeOilPressure?: number
  lubeOilTemp?: number
  fuelPressure?: number
  fuelRate?: number
  runningHours?: number
  startCount?: number
  alarmStatus?: number
  isSynced: boolean
  createdAt: string
}

export interface GeneratorData {
  id: number
  timestamp: string
  generatorId: string
  isRunning: boolean
  voltage?: number
  frequency?: number
  current?: number
  activePower?: number
  powerFactor?: number
  runningHours?: number
  loadPercent?: number
  isSynced: boolean
  createdAt: string
}

export interface TankLevel {
  id: number
  timestamp: string
  tankId: string
  tankType: string
  levelPercent: number
  volumeLiters?: number
  temperature?: number
  isSynced: boolean
  createdAt: string
}

export interface FuelConsumption {
  id: number
  timestamp: string
  fuelType: string
  consumedVolume: number
  consumedMass: number
  tankId?: string
  density?: number
  distanceTraveled?: number
  timeUnderway?: number
  cargoWeight?: number
  co2Emissions?: number
  isSynced: boolean
  createdAt: string
}

export interface EnvironmentalData {
  id: number
  timestamp: string
  airTemperature?: number
  barometricPressure?: number
  humidity?: number
  seaTemperature?: number
  windSpeed?: number
  windDirection?: number
  waveHeight?: number
  visibility?: number
  isSynced: boolean
  createdAt: string
}

export interface SafetyAlarm {
  id: number
  timestamp: string
  alarmType: string
  alarmCode?: string
  severity: 'CRITICAL' | 'WARNING' | 'INFO'
  location?: string
  description?: string
  isAcknowledged: boolean
  acknowledgedAt?: string
  acknowledgedBy?: string
  isResolved: boolean
  resolvedAt?: string
  isSynced: boolean
  createdAt: string
}

export interface AisData {
  id: number
  timestamp: string
  mmsi: string
  messageType: number
  navigationStatus?: number
  rateOfTurn?: number
  speedOverGround?: number
  positionAccuracy?: boolean
  latitude?: number
  longitude?: number
  courseOverGround?: number
  trueHeading?: number
  imoNumber?: string
  callSign?: string
  shipName?: string
  shipType?: number
  dimensionBow?: number
  dimensionStern?: number
  dimensionPort?: number
  dimensionStarboard?: number
  etaMonth?: number
  etaDay?: number
  etaHour?: number
  etaMinute?: number
  draught?: number
  destination?: string
  isSynced: boolean
  createdAt: string
}

export interface VoyageRecord {
  id: number
  voyageNumber: string
  departurePort?: string
  departureTime?: string
  arrivalPort?: string
  arrivalTime?: string
  cargoType?: string
  cargoWeight?: number
  distanceTraveled?: number
  fuelConsumed?: number
  averageSpeed?: number
  voyageStatus: 'PLANNING' | 'UNDERWAY' | 'COMPLETED'
  isSynced: boolean
  createdAt: string
}

// ============================================================
// OPERATIONAL TABLES (SOLAS/ISM/MARPOL Compliance)
// ============================================================

export interface CrewMember {
  id: number
  crewId: string
  fullName: string
  position: string
  rank?: string
  department?: string
  nationality?: string
  dateOfBirth?: string
  phoneNumber?: string
  address?: string
  
  // Position & Employment
  joinDate?: string
  embarkDate?: string
  disembarkDate?: string
  contractEnd?: string
  isOnboard: boolean
  
  // STCW Certificate
  certificateNumber?: string
  certificateIssue?: string
  certificateExpiry?: string
  
  // Medical Certificate
  medicalIssue?: string
  medicalExpiry?: string
  
  // Travel Documents
  passportNumber?: string
  passportExpiry?: string
  visaNumber?: string
  visaExpiry?: string
  seamanBookNumber?: string
  
  // Emergency Contact
  emergencyContact?: string
  emailAddress?: string
  
  // Additional
  notes?: string
  
  isSynced: boolean
  createdAt: string
}

export interface MaintenanceTask {
  id: number
  taskId: string
  equipmentId: string
  equipmentName: string
  taskType: string
  taskDescription: string
  intervalHours?: number
  intervalDays?: number
  lastDoneAt?: string
  nextDueAt: string
  runningHoursAtLastDone?: number
  priority: 'CRITICAL' | 'HIGH' | 'NORMAL' | 'LOW'
  status: 'PENDING' | 'OVERDUE' | 'IN_PROGRESS' | 'COMPLETED'
  assignedTo?: string
  completedAt?: string
  completedBy?: string
  notes?: string
  sparePartsUsed?: string
  isSynced: boolean
  createdAt: string
}

export interface CargoOperation {
  id: number
  operationId: string
  voyageId?: number
  operationType: 'LOADING' | 'DISCHARGING'
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
  status: 'PLANNED' | 'LOADING' | 'LOADED' | 'DISCHARGING' | 'DISCHARGED'
  isSynced: boolean
  createdAt: string
}

export interface WatchkeepingLog {
  id: number
  watchDate: string
  watchPeriod: string
  watchType: 'NAVIGATION' | 'ENGINE'
  officerOnWatch: string
  lookout?: string
  weatherConditions?: string
  seaState?: string
  visibility?: string
  courseLogged?: number
  speedLogged?: number
  positionLat?: number
  positionLon?: number
  distanceRun?: number
  engineStatus?: string
  notableEvents?: string
  masterSignature?: string
  isSynced: boolean
  createdAt: string
}

export interface OilRecordBook {
  id: number
  entryDate: string
  operationCode: string
  operationDescription: string
  locationLat?: number
  locationLon?: number
  quantity?: number
  quantityUnit?: string
  tankFrom?: string
  tankTo?: string
  officerInCharge: string
  masterSignature?: string
  remarks?: string
  isSynced: boolean
  createdAt: string
}

export interface SyncQueue {
  id: number
  tableName: string
  recordId: number
  payload: string
  priority: number
  retryCount: number
  maxRetries: number
  nextRetryAt?: string
  lastError?: string
  createdAt: string
  syncedAt?: string
}

// ============================================================
// TASK MANAGEMENT SYSTEM
// ============================================================

export interface TaskType {
  id: number
  taskTypeCode: string
  typeName: string
  category: string
  description?: string | null
  estimatedDurationMinutes?: number | null
  requiresApproval: boolean
  priority: string
  isActive: boolean
  createdAt: string
  updatedAt?: string | null
  totalDetails?: number
  completedTasks?: number
  pendingTasks?: number
}

export interface TaskDetail {
  id: number
  taskTypeId: number
  detailCode: string
  detailName: string
  detailType: string
  orderIndex: number
  description?: string | null
  isMandatory: boolean
  expectedValue?: string | null
  minValue?: number | null
  maxValue?: number | null
  unit?: string | null
  isActive: boolean
  createdAt: string
  updatedAt?: string | null
}

export interface TaskTypeWithDetails {
  taskType: TaskType
  details: TaskDetail[]
}

export interface TaskTypeCategoryStats {
  category: string
  count: number
  activeCount: number
  totalDetails: number
}

// ============================================================
// UI/UX Types
// ============================================================

export interface DashboardStats {
  totalAlarms: number
  criticalAlarms: number
  pendingMaintenance: number
  crewOnboard: number
  fuelLevel: number
  syncStatus: 'ONLINE' | 'OFFLINE' | 'SYNCING'
  lastSyncAt?: string
  unsyncedRecords: number
}

export interface AlarmSummary {
  critical: number
  warning: number
  info: number
  unacknowledged: number
}

export interface MaterialCategory {
  id: number;
  categoryCode: string;
  name: string;
  description?: string | null;
  parentCategoryId?: number | null;
  isActive: boolean;
  isSynced: boolean;
  createdAt: string;
}

export interface MaterialItem {
  id: number;
  itemCode: string;
  name: string;
  categoryId: number;
  specification?: string | null;
  unit: string;
  onHandQuantity: number;
  minStock?: number | null;
  maxStock?: number | null;
  reorderLevel?: number | null;
  reorderQuantity?: number | null;
  location?: string | null;
  manufacturer?: string | null;
  supplier?: string | null;
  partNumber?: string | null;
  barcode?: string | null;
  batchTracked: boolean;
  serialTracked: boolean;
  expiryRequired: boolean;
  unitCost?: number | null;
  currency: string;
  notes?: string | null;
  isActive: boolean;
  isSynced: boolean;
  createdAt: string;
}
