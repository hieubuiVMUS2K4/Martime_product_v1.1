// ============================================================
// Phase 3 — Voyage Operations Cockpit Types
// ============================================================

export interface VoyageCockpitDto {
  voyageId: string
  voyageNumber: string
  voyageStatus: string
  charterType?: string

  vesselName?: string
  vesselIMO?: string
  vesselFlag?: string

  departurePort?: string
  departurePortCode?: string
  departureTime?: string
  arrivalPort?: string
  arrivalPortCode?: string
  arrivalTime?: string

  overview: CockpitOverview
  legs: CockpitLegPerformance[]
  timeline: CockpitTimelineEvent[]
  fuelSummary: CockpitFuelSummaryItem[]
  cargoSummary: CockpitCargoSummary
}

export interface CockpitOverview {
  plannedDistanceNm?: number
  actualDistanceNm?: number
  distanceVarianceNm?: number

  plannedDurationHours?: number
  actualDurationHours?: number
  durationVarianceHours?: number

  plannedSpeedKnots?: number
  actualSpeedKnots?: number

  plannedFuelMt?: number
  actualFuelMt?: number
  fuelVarianceMt?: number

  plannedCostUsd?: number
  plannedRevenueUsd?: number

  totalEvents: number
  totalLegs: number
}

export interface CockpitLegPerformance {
  planLegId: string
  sequence: number
  legType: string

  fromPortCode?: string
  fromPortName?: string
  toPortCode?: string
  toPortName?: string

  plannedDeparture?: string
  plannedArrival?: string
  plannedDistanceNm?: number
  plannedDurationHours?: number
  plannedSpeedKnots?: number
  plannedFuelMt?: number

  actualDeparture?: string
  actualArrival?: string
  actualDistanceNm?: number
  actualDurationHours?: number
  actualSpeedKnots?: number
  actualFuelMt?: number

  durationVarianceHours?: number
  distanceVarianceNm?: number
  speedVarianceKnots?: number
  fuelVarianceMt?: number

  events: CockpitTimelineEvent[]
}

export type CockpitEventSource =
  | 'LOG'
  | 'PORT_CALL'
  | 'NOON_REPORT'
  | 'DEPARTURE_REPORT'
  | 'ARRIVAL_REPORT'
  | 'BUNKER_REPORT'
  | 'POSITION_REPORT'
  | 'CARGO_OP'
  | 'FUEL'
  | 'STATUS_CHANGE'

export interface CockpitTimelineEvent {
  id: string
  source: CockpitEventSource
  sourceId: string
  eventType: string
  timestamp: string
  latitude?: number
  longitude?: number
  planLegId?: string
  planLegSequence?: number
  title: string
  description?: string
  icon: string
  color: string
  speedKnots?: number
  courseDegs?: number
  distanceNm?: number
  fuelConsumedMt?: number
  cargoQuantity?: number
  cargoUnit?: string
  portName?: string
  portCode?: string
  reportStatus?: string
}

export interface CockpitFuelSummaryItem {
  fuelType: string
  plannedMt: number
  actualMt: number
  varianceMt: number
}

export interface CockpitCargoSummary {
  totalPlannedLoading: number
  totalPlannedDischarging: number
  totalActualLoaded: number
  totalActualDischarged: number
}
