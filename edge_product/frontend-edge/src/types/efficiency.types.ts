// ============================================================
// VOYAGE EFFICIENCY - Type Definitions (Phase 5)
// Based on edge-services DTOs (VoyageEfficiencyDtos.cs)
// ============================================================

export interface EfficiencyDimension {
  label: string
  unit: string
  estimated?: number
  actual?: number
  variance?: number
  variancePercent?: number
  rating: string // BETTER | ON_TARGET | WORSE | N/A
}

export interface CostCategoryComparison {
  category: string
  estimated: number
  actual: number
  variance: number
  variancePercent: number
}

export interface RevenueCategoryComparison {
  category: string
  estimated: number
  actual: number
  variance: number
  variancePercent: number
}

export interface LegEfficiency {
  sequence: number
  legType: string
  fromPort?: string
  toPort?: string
  plannedDistanceNm?: number
  plannedDurationHours?: number
  plannedSpeedKts?: number
  plannedFuelMt?: number
  actualDurationHours?: number
  actualSpeedKts?: number
  durationVarianceHours?: number
  speedVarianceKts?: number
}

export interface VoyageEfficiencyReport {
  voyageId: string
  voyageNumber: string
  voyageStatus: string
  financialStatus: string
  charterType?: string
  commencedAt?: string
  completedAt?: string
  actualVoyageDurationHours?: number

  // Dimensions
  fuel: EfficiencyDimension
  seaTime: EfficiencyDimension
  portTime: EfficiencyDimension
  speed: EfficiencyDimension
  distance: EfficiencyDimension
  cargoProductivity: EfficiencyDimension
  bunkerCost: EfficiencyDimension
  portCost: EfficiencyDimension
  crewChangeCost: EfficiencyDimension
  totalCost: EfficiencyDimension
  totalRevenue: EfficiencyDimension
  totalMargin: EfficiencyDimension

  // Breakdowns
  costBreakdown: CostCategoryComparison[]
  revenueBreakdown: RevenueCategoryComparison[]
  legEfficiency: LegEfficiency[]

  // Overall
  overallScore: number
  overallRating: string // EXCELLENT | GOOD | FAIR | POOR
}
