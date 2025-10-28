import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5001';

export interface FuelEfficiencyMetrics {
  periodStart: string;
  periodEnd: string;
  periodType: string;
  distanceNauticalMiles: number;
  timeUnderwayHours: number;
  averageSpeedKnots: number;
  totalFuelConsumedMT: number;
  mainEngineFuelMT: number;
  eeoi: number;
  fuelPerNauticalMile: number;
  fuelPerHour: number;
  sfoc: number | null;
  cO2EmissionsMT: number;
  cii: number;
  ciiRating: string;
  avgMainEngineRPM: number;
  avgMainEngineLoad: number;
  avgWindSpeed: number;
  cargoWeightMT: number;
  dataPointsCount: number;
  dataQualityScore: number;
}

export interface TrendDataPoint {
  date: string;
  fuelConsumedMT: number;
  distanceNM: number;
  fuelPerNM: number;
  cO2EmissionsMT: number;
}

export interface CIIRatingDetails {
  year: number;
  actualCII: number;
  requiredCII: number;
  rating: string;
  isCompliant: boolean;
  deviationPercent: number;
  complianceStatus: string;
  boundaries: {
    ratingA: number;
    ratingB: number;
    ratingC: number;
    ratingD: number;
    ratingE: number;
  };
  recommendations: string[];
}

export interface DashboardData {
  weeklySummary: FuelEfficiencyMetrics;
  monthlySummary: FuelEfficiencyMetrics;
  monthlyComparison: any;
  ciiRating: CIIRatingDetails | null;
  trend: TrendDataPoint[];
  generatedAt: string;
}

export const fuelAnalyticsService = {
  async getDashboard(): Promise<DashboardData> {
    const response = await axios.get(`${API_BASE_URL}/api/fuel-analytics/dashboard`);
    return response.data;
  },

  async getWeeklySummary(): Promise<FuelEfficiencyMetrics> {
    const response = await axios.get(`${API_BASE_URL}/api/fuel-analytics/summary/weekly`);
    return response.data;
  },

  async getMonthlySummary(): Promise<FuelEfficiencyMetrics> {
    const response = await axios.get(`${API_BASE_URL}/api/fuel-analytics/summary/monthly`);
    return response.data;
  },

  async getCIIRating(year?: number): Promise<CIIRatingDetails> {
    const params = year ? `?year=${year}` : '';
    const response = await axios.get(`${API_BASE_URL}/api/fuel-analytics/cii-rating${params}`);
    return response.data;
  },

  async getTrend(startDate: string, endDate: string, groupBy: 'HOURLY' | 'DAILY' | 'WEEKLY' = 'DAILY'): Promise<TrendDataPoint[]> {
    const response = await axios.get(`${API_BASE_URL}/api/fuel-analytics/trend`, {
      params: { startDate, endDate, groupBy }
    });
    return response.data;
  }
};
