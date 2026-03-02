// Ship Data Types for Ship's Data Management Module

// ═══════════════════════════════════════
// CHILD ENTITY TYPES
// ═══════════════════════════════════════

export interface ShipMainEngine {
  id?: string;
  meType?: string;
  meFuelGrade?: string;
  mePowerKW?: number;
  mcrKW?: number;
  sortOrder: number;
}

export interface ShipAuxiliaryEngine {
  id?: string;
  aeType?: string;
  aeFuelGrade?: string;
  aePowerKW?: number;
  sortOrder: number;
}

export interface ShipPropeller {
  id?: string;
  propellerType?: string;
  numberOfBlades?: number;
  rotation?: string;
  diameterMm?: number;
  propellerPitchGeometricMm?: number;
  pitchRatio?: number;
  sortOrder: number;
}

export interface ShipBowthruster {
  id?: string;
  powerKW?: number;
  sortOrder: number;
}

export interface ShipSternthruster {
  id?: string;
  powerKW?: number;
  sortOrder: number;
}

export interface ShipRudder {
  id?: string;
  rudderType?: string;
  sortOrder: number;
}

export interface ShipShaftGenerator {
  id?: string;
  maxPowerKW?: number;
  sortOrder: number;
}

export interface ShipBoiler {
  id?: string;
  boilerType?: string;
  model?: string;
  sortOrder: number;
}

export interface ShipLoadLine {
  id?: string;
  loadLineType?: string;
  draftM?: number;
  freeboardM?: number;
  displacementMt?: number;
  deadweightMt?: number;
  sortOrder: number;
}

export interface ShipPilotCardData {
  id?: string;
  engineOrder?: string;
  mainEngineRPM?: number;
  speedLoadedKts?: number;
  speedBallastKts?: number;
  sortOrder: number;
}

// ═══════════════════════════════════════
// MAIN SHIP DATA TYPE
// ═══════════════════════════════════════

export interface ShipData {
  id: string;
  // Tab 1: Basic Data
  imoNumber: string;
  officialNumber?: string;
  callSign?: string;
  shipName: string;
  flag: string;
  portOfRegistry: string;
  previousName?: string;
  previousFlag?: string;
  mmsiNumber?: string;
  typeOfVessel?: string;
  classNotation?: string;
  classRegisterNumber?: string;
  shipyardCountry?: string;
  shipyardName?: string;
  yardNo?: string;
  companyImoNumber?: string;
  suezCanalIdNumber?: string;
  keelLaidDate?: string;
  yearBuilt?: number;
  dateOfRegistry?: string;
  ownerImoNumber?: string;
  panamaCanalIdNumber?: string;
  maxPersonsAllowedOB?: number;
  serviceSpeedKts?: number;
  vrpNumber?: string;
  vrpType?: string;
  noOfCrewSafeManning?: number;
  maxPassengersAllowedOB?: number;

  // Tab 2: Dimensions
  loa?: number;
  depthMoulded?: number;
  hMaxAirdraft?: number;
  parallelBodyBallast?: number;
  parallelBodyLoaded?: number;
  lbp?: number;
  draftMoulded?: number;
  dDistance?: number;
  bridgeToAft?: number;
  bridgeToBow?: number;
  bowToBulbousBow?: number;
  breadthMoulded?: number;
  draftScantling?: number;
  airdraftReductionMastFouled?: number;
  lightShip?: number;
  draftFullBallast?: number;
  blockCoefficientNA?: boolean;
  blockCoefficient?: number;
  tpcAtSummerDraft?: number;
  freshWaterAllowanceFwa?: number;
  // Tonnage
  grossTonnageInternational?: number;
  grossTonnageSuezCanal?: number;
  grossTonnagePanamaCanal?: number;
  nettTonnageInternational?: number;
  nettTonnageSuezCanal?: number;
  nettTonnagePanamaCanal?: number;
  // Tanker-specific
  manifoldToWaterlineBallast?: number;
  manifoldToWaterlineLoaded?: number;
  deckToManifold?: number;
  sternToManifold?: number;
  shipsideToManifold?: number;
  bowToManifold?: number;
  manifoldToKeel?: number;
  manifoldToBridge?: number;
  maxLoadingRateShip?: number;
  numberOfLines?: number;
  maxAllowablePressurePsi?: number;
  ventingSystemShip?: string;

  // Tab 3: Machinery
  anchorChainPort?: string;
  anchorChainStarboard?: string;
  anchorChainStern?: string;
  anchorChainSternNA?: boolean;
  bowthrusterNA?: boolean;
  sternthrusterNA?: boolean;
  shaftGeneratorNA?: boolean;
  harbourGeneratorMaker?: string;
  harbourGeneratorMaxPowerKW?: number;
  azimuthEngFwdCount?: number;
  azimuthEngFwdMaxPowerKW?: number;
  azimuthEngAftCount?: number;
  azimuthEngAftMaxPowerKW?: number;

  // Tab 4: Shipowner
  shipownerName?: string; shipownerStreet?: string; shipownerCountry?: string;
  shipownerZip?: string; shipownerCity?: string; shipownerPhone?: string;
  shipownerFax?: string; shipownerTlx?: string; shipownerEmail?: string;
  shipownerContactPerson?: string;
  managingOwnerName?: string; managingOwnerStreet?: string; managingOwnerCountry?: string;
  managingOwnerZip?: string; managingOwnerCity?: string; managingOwnerPhone?: string;
  managingOwnerFax?: string; managingOwnerTlx?: string; managingOwnerEmail?: string;
  managingOwnerContactPerson?: string;
  operatorName?: string; operatorStreet?: string; operatorCountry?: string;
  operatorZip?: string; operatorCity?: string; operatorPhone?: string;
  operatorFax?: string; operatorTlx?: string; operatorEmail?: string;
  operatorContactPerson?: string;
  csoTitle?: string; csoFirstName?: string; csoLastName?: string;
  csoStreet?: string; csoCountry?: string; csoZip?: string; csoCity?: string;
  csoPhone24h?: string; csoFax?: string; csoTlx?: string; csoEmail?: string;
  dpaTitle?: string; dpaFirstName?: string; dpaLastName?: string;
  dpaStreet?: string; dpaCountry?: string; dpaZip?: string; dpaCity?: string;
  dpaPhone24h?: string; dpaFax?: string; dpaTlx?: string; dpaEmail?: string;
  qiUsaTitle?: string; qiUsaFirstName?: string; qiUsaLastName?: string;
  qiUsaStreet?: string; qiUsaCountry?: string; qiUsaZip?: string; qiUsaCity?: string;
  qiUsaPhone24h?: string; qiUsaFax?: string; qiUsaTlx?: string; qiUsaEmail?: string;
  qiPanamaTitle?: string; qiPanamaFirstName?: string; qiPanamaLastName?: string;
  qiPanamaStreet?: string; qiPanamaCountry?: string; qiPanamaZip?: string; qiPanamaCity?: string;
  qiPanamaPhone24h?: string; qiPanamaFax?: string; qiPanamaTlx?: string; qiPanamaEmail?: string;

  // Tab 5: Charterer
  chartererName?: string; chartererStreet?: string; chartererCountry?: string;
  chartererZip?: string; chartererCity?: string; chartererPhone?: string;
  chartererFax?: string; chartererTlx?: string; chartererEmail?: string;
  chartererContactPerson?: string;
  bareboatChartererName?: string; bareboatChartererStreet?: string; bareboatChartererCountry?: string;
  bareboatChartererZip?: string; bareboatChartererCity?: string; bareboatChartererPhone?: string;
  bareboatChartererFax?: string; bareboatChartererTlx?: string; bareboatChartererEmail?: string;
  bareboatChartererContactPerson?: string;

  // Tab 6: Class / Flag State
  classSocietyName?: string; classSocietyStreet?: string; classSocietyCountry?: string;
  classSocietyZip?: string; classSocietyCity?: string; classSocietyPhone?: string;
  classSocietyFax?: string; classSocietyTlx?: string; classSocietyEmail?: string;
  classSocietyContactPerson?: string;
  flagStateName?: string; flagStateStreet?: string; flagStateCountry?: string;
  flagStateZip?: string; flagStateCity?: string; flagStatePhone?: string;
  flagStateFax?: string; flagStateTlx?: string; flagStateEmail?: string;
  flagStateContactPerson?: string;

  // Tab 7: Insurance
  piClubName?: string; piClubStreet?: string; piClubCountry?: string;
  piClubZip?: string; piClubCity?: string; piClubPhone?: string;
  piClubFax?: string; piClubTlx?: string; piClubEmail?: string;
  piClubContactPerson?: string;
  hmClubName?: string; hmClubStreet?: string; hmClubCountry?: string;
  hmClubZip?: string; hmClubCity?: string; hmClubPhone?: string;
  hmClubFax?: string; hmClubTlx?: string; hmClubEmail?: string;
  hmClubContactPerson?: string;

  // Tab 8: Radio Communication
  inmarsatTelex1?: string; inmarsatTelex2?: string;
  inmarsatPhone1?: string; inmarsatPhone2?: string;
  inmarsatFax1?: string; inmarsatFax2?: string;
  emailAddress1?: string; emailAddress2?: string;
  gsmPhone?: string;
  seaAreaA1?: boolean; seaAreaA2?: boolean; seaAreaA3?: boolean; seaAreaA4?: boolean;
  dscHF?: boolean; dscMF?: boolean; dscVHF?: boolean;
  radiotelephoneHF?: boolean; radiotelephoneMF?: boolean; radiotelephoneVHF?: boolean;
  radiotelegraphHF?: boolean; radiotelegraphMF?: boolean; radiotelegraphVHF?: boolean;
  navtex?: boolean; ais?: boolean; sartTransponder?: boolean; radiotelex?: boolean;
  otherRadioEquipment?: string;
  epirbNumber?: string; epirbOperatingSystem?: string;
  epirbMaker?: string; epirbModel?: string; epirbFrequency?: string;

  // Tab 9: Tanks & Cargo
  hfoCbm?: number; mdoCbm?: number; lubOilCbm?: number;
  sludgeCbm?: number; bilgeWaterCbm?: number; sewageCbm?: number;
  freshWaterCbm?: number; ballastWaterCbm?: number;
  noOfBallastTanks?: number;
  teuTotal?: number; teuOnDeck?: number; teuUnderDeck?: number;
  grainCbm?: number; balesCbm?: number;
  noOfCargoHolds?: number; noOfHatches?: number;

  // Child collections
  mainEngines: ShipMainEngine[];
  auxiliaryEngines: ShipAuxiliaryEngine[];
  propellers: ShipPropeller[];
  bowthrusters: ShipBowthruster[];
  sternthrusters: ShipSternthruster[];
  rudders: ShipRudder[];
  shaftGenerators: ShipShaftGenerator[];
  boilers: ShipBoiler[];
  loadLines: ShipLoadLine[];
  pilotCardData: ShipPilotCardData[];

  createdAt?: string;
  updatedAt?: string;
}

// Save request type - same fields as ShipData without id/timestamps
export type SaveShipData = Omit<ShipData, 'id' | 'createdAt' | 'updatedAt'>;

// API response wrapper
export interface ShipDataResponse {
  exists: boolean;
  data: ShipData | null;
}

export interface SaveShipDataResponse {
  success: boolean;
  data: ShipData;
}

// ═══════════════════════════════════════
// TAB DEFINITIONS
// ═══════════════════════════════════════

export type ShipDataTabId = 
  | 'basic-data'
  | 'dimensions'
  | 'machinery'
  | 'shipowner'
  | 'charterer'
  | 'class-flag-state'
  | 'insurance'
  | 'radio-comm'
  | 'tanks-cargo';

export interface ShipDataTab {
  id: ShipDataTabId;
  label: string;
  icon?: string;
}

// ═══════════════════════════════════════
// CONVERSION HELPERS
// ═══════════════════════════════════════

/** kW → HP conversion factor */
export const KW_TO_HP = 1.34102;

/** m → ft conversion factor */
export const M_TO_FT = 3.28084;

/** cbm → cb feet conversion factor */
export const CBM_TO_CBFT = 35.3147;

/** Convert kW to HP */
export const kwToHp = (kw?: number): number | undefined => 
  kw != null ? Math.round(kw * KW_TO_HP * 100) / 100 : undefined;

/** Convert meters to feet string (e.g., "100' 3\"") */
export const mToFtIn = (m?: number): string => {
  if (m == null) return '';
  const totalInches = m * M_TO_FT * 12;
  const feet = Math.floor(totalInches / 12);
  const inches = Math.round(totalInches % 12);
  return `${feet}' ${inches}"`;
};

/** Convert cbm to cb feet */
export const cbmToCbFt = (cbm?: number): number | undefined =>
  cbm != null ? Math.round(cbm * CBM_TO_CBFT * 100) / 100 : undefined;

/** Calculate pitch ratio (Pitch / Diameter) */
export const calcPitchRatio = (pitchMm?: number, diameterMm?: number): number | undefined =>
  pitchMm != null && diameterMm != null && diameterMm !== 0
    ? Math.round((pitchMm / diameterMm) * 10000) / 10000
    : undefined;

// ═══════════════════════════════════════
// DEFAULT EMPTY DATA
// ═══════════════════════════════════════

export const createEmptyShipData = (): SaveShipData => ({
  imoNumber: '',
  shipName: '',
  flag: '',
  portOfRegistry: '',
  mainEngines: [],
  auxiliaryEngines: [],
  propellers: [],
  bowthrusters: [],
  sternthrusters: [],
  rudders: [],
  shaftGenerators: [],
  boilers: [],
  loadLines: [],
  pilotCardData: [],
});
