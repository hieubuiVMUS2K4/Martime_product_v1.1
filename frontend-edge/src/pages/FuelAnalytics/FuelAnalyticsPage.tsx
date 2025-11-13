import { useEffect, useState, useCallback } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { 
  Activity, 
  Fuel, 
  Ship, 
  Wind,
  AlertTriangle,
  CheckCircle2,
  Gauge,
  Target,
  Award,
  TrendingUp,
  TrendingDown
} from 'lucide-react';
import { fuelAnalyticsService, type DashboardData } from '@/services/fuelAnalyticsService';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts';

const FuelAnalyticsPage = () => {
  const [dashboardData, setDashboardData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Helper function to safely format numbers
  const safeFixed = (value: number | null | undefined, decimals: number): string => {
    return value != null ? value.toFixed(decimals) : 'N/A';
  };

  const loadDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      const data = await fuelAnalyticsService.getDashboard();
      setDashboardData(data);
      setError(null);
    } catch (err) {
      setError('Failed to load fuel analytics data');
      console.error('Error loading dashboard:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDashboardData();
    const interval = setInterval(loadDashboardData, 5 * 60 * 1000); // 5 minutes
    return () => clearInterval(interval);
  }, [loadDashboardData]);

  const getCIIRatingColor = (rating: string) => {
    switch (rating) {
      case 'A': return 'border-l-green-500';
      case 'B': return 'border-l-blue-500';
      case 'C': return 'border-l-yellow-500';
      case 'D': return 'border-l-orange-500';
      case 'E': return 'border-l-red-500';
      default: return 'border-l-gray-500';
    }
  };

  const getCIIRatingBadgeVariant = (rating: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (rating) {
      case 'A':
      case 'B': return 'default';
      case 'C': return 'secondary';
      case 'D':
      case 'E': return 'destructive';
      default: return 'outline';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:from-gray-900 dark:via-blue-950 dark:to-gray-900">
        <div className="text-center space-y-4">
          <div className="animate-spin rounded-full h-16 w-16 border-4 border-blue-600 border-t-transparent mx-auto"></div>
          <p className="text-slate-600 dark:text-slate-400 font-medium">Loading Maritime Analytics...</p>
        </div>
      </div>
    );
  }

  if (error || !dashboardData) {
    return (
      <div className="flex items-center justify-center min-h-screen p-4">
        <Alert variant="destructive" className="max-w-md">
          <AlertTriangle className="h-5 w-5" />
          <AlertDescription className="ml-2">{error || 'No data available'}</AlertDescription>
        </Alert>
      </div>
    );
  }

  const { weeklySummary, monthlySummary, ciiRating, trend } = dashboardData;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:from-gray-900 dark:via-blue-950 dark:to-gray-900">
      <div className="w-full px-3 py-4 space-y-4">
        {/* Professional Maritime Header */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-blue-100 dark:border-blue-900 p-4">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <div className="flex items-center gap-3">
              <div className="p-2.5 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg shadow-md">
                <Fuel className="h-7 w-7 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 dark:from-blue-400 dark:to-blue-600 bg-clip-text text-transparent">
                  Fuel Efficiency Analytics
                </h1>
                <p className="text-xs text-slate-600 dark:text-slate-400 mt-0.5 flex items-center gap-2">
                  <Ship className="h-3.5 w-3.5" />
                  IMO DCS, EU MRV & CII Compliance Monitoring System
                </p>
              </div>
            </div>
            <div className="flex flex-col items-end gap-1.5">
              <Badge variant="outline" className="bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800 text-xs">
                <CheckCircle2 className="h-3 w-3 mr-1 text-green-600" />
                System Active
              </Badge>
              <div className="text-xs text-slate-500 dark:text-slate-400">
                {new Date(dashboardData.generatedAt).toLocaleString('en-GB', {
                  day: '2-digit',
                  month: 'short',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                  timeZoneName: 'short'
                })}
              </div>
            </div>
          </div>
        </div>

        {/* IMO CII Compliance Alert */}
        {ciiRating && (
          <Alert className={`border-l-4 shadow-lg ${getCIIRatingColor(ciiRating.rating)} bg-white dark:bg-gray-800`}>
            <div className="flex flex-col md:flex-row md:items-start gap-3 p-1.5">
              <div className="flex-shrink-0">
                <div className={`w-14 h-14 rounded-xl flex items-center justify-center ${
                  ciiRating.rating === 'A' || ciiRating.rating === 'B' ? 'bg-green-100 dark:bg-green-900 text-green-600' :
                  ciiRating.rating === 'C' ? 'bg-yellow-100 dark:bg-yellow-900 text-yellow-600' :
                  'bg-red-100 dark:bg-red-900 text-red-600'
                }`}>
                  <Award className="h-7 w-7" />
                </div>
              </div>
              <div className="flex-1 space-y-2">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="font-bold text-lg">IMO CII Rating {ciiRating.year}</h3>
                  <Badge 
                    variant={getCIIRatingBadgeVariant(ciiRating.rating)}
                    className="text-base px-3 py-0.5 font-bold"
                  >
                    {ciiRating.rating}
                  </Badge>
                  {ciiRating.isCompliant ? (
                    <div className="flex items-center gap-1 text-green-600 dark:text-green-400">
                      <CheckCircle2 className="h-5 w-5" />
                      <span className="text-sm font-medium">Compliant</span>
                    </div>
                  ) : (
                    <div className="flex items-center gap-1 text-red-600 dark:text-red-400">
                      <AlertTriangle className="h-5 w-5" />
                      <span className="text-sm font-medium">Action Required</span>
                    </div>
                  )}
                </div>
                <p className="text-xs font-medium text-slate-700 dark:text-slate-300">
                  {ciiRating.complianceStatus}
                </p>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-3 p-3 bg-slate-50 dark:bg-gray-900 rounded-lg">
                  <div className="space-y-0.5">
                    <span className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wide">Actual CII</span>
                    <div className="text-xl font-bold">{safeFixed(ciiRating.actualCII, 2)}</div>
                    <span className="text-xs text-slate-500">gCO₂/dwt-nm</span>
                  </div>
                  <div className="space-y-0.5">
                    <span className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wide">Required CII</span>
                    <div className="text-xl font-bold text-blue-600">{safeFixed(ciiRating.requiredCII, 2)}</div>
                    <span className="text-xs text-slate-500">gCO₂/dwt-nm</span>
                  </div>
                  <div className="space-y-0.5">
                    <span className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wide">Deviation</span>
                    <div className={`text-xl font-bold ${(ciiRating.deviationPercent ?? 0) > 0 ? 'text-red-600' : 'text-green-600'}`}>
                      {(ciiRating.deviationPercent ?? 0) > 0 ? '+' : ''}{safeFixed(ciiRating.deviationPercent, 1)}%
                    </div>
                    <span className="text-xs text-slate-500">from target</span>
                  </div>
                </div>
              </div>
            </div>
          </Alert>
        )}

        {/* Key Performance Indicators */}
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-3">
          <Card className="bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/20 dark:to-blue-800/20 border-blue-200 dark:border-blue-800 shadow-md hover:shadow-xl transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-1.5">
              <CardTitle className="text-xs font-semibold uppercase tracking-wide text-blue-700 dark:text-blue-300">
                Weekly Fuel
              </CardTitle>
              <Fuel className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            </CardHeader>
            <CardContent className="pt-1">
              <div className="text-2xl font-bold text-blue-900 dark:text-blue-100">
                {safeFixed(weeklySummary.totalFuelConsumedMT, 1)} MT
              </div>
              <p className="text-xs text-blue-600 dark:text-blue-400 mt-1.5 flex items-center gap-1">
                <Target className="h-3 w-3" />
                {safeFixed(weeklySummary.fuelPerNauticalMile, 2)} MT/NM
              </p>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-green-50 to-green-100 dark:from-green-900/20 dark:to-green-800/20 border-green-200 dark:border-green-800 shadow-md hover:shadow-xl transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-1.5">
              <CardTitle className="text-xs font-semibold uppercase tracking-wide text-green-700 dark:text-green-300">
                EEOI
              </CardTitle>
              <Activity className="h-4 w-4 text-green-600 dark:text-green-400" />
            </CardHeader>
            <CardContent className="pt-1">
              <div className="text-2xl font-bold text-green-900 dark:text-green-100">
                {safeFixed(weeklySummary.eeoi, 1)}
              </div>
              <p className="text-xs text-green-600 dark:text-green-400 mt-1.5">
                gCO₂/tonne-nm
              </p>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-purple-50 to-purple-100 dark:from-purple-900/20 dark:to-purple-800/20 border-purple-200 dark:border-purple-800 shadow-md hover:shadow-xl transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-1.5">
              <CardTitle className="text-xs font-semibold uppercase tracking-wide text-purple-700 dark:text-purple-300">
                Distance
              </CardTitle>
              <Ship className="h-4 w-4 text-purple-600 dark:text-purple-400" />
            </CardHeader>
            <CardContent className="pt-1">
              <div className="text-2xl font-bold text-purple-900 dark:text-purple-100">
                {safeFixed(weeklySummary.distanceNauticalMiles, 1)} NM
              </div>
              <p className="text-xs text-purple-600 dark:text-purple-400 mt-1.5">
                {safeFixed(weeklySummary.averageSpeedKnots, 1)} knots avg
              </p>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-orange-50 to-orange-100 dark:from-orange-900/20 dark:to-orange-800/20 border-orange-200 dark:border-orange-800 shadow-md hover:shadow-xl transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-1.5">
              <CardTitle className="text-xs font-semibold uppercase tracking-wide text-orange-700 dark:text-orange-300">
                CO₂ Emissions
              </CardTitle>
              <Wind className="h-4 w-4 text-orange-600 dark:text-orange-400" />
            </CardHeader>
            <CardContent className="pt-1">
              <div className="text-2xl font-bold text-orange-900 dark:text-orange-100">
                {safeFixed(weeklySummary.cO2EmissionsMT, 1)} MT
              </div>
              <div className="text-xs text-orange-600 dark:text-orange-400 mt-1.5 flex items-center gap-2">
                <Badge variant={getCIIRatingBadgeVariant(weeklySummary.ciiRating)} className="text-xs">
                  {weeklySummary.ciiRating}
                </Badge>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Detailed Analytics Tabs */}
        <Tabs defaultValue="overview" className="space-y-3">
          <TabsList className="bg-white dark:bg-gray-800 p-1 shadow-md">
            <TabsTrigger value="overview" className="data-[state=active]:bg-blue-600 data-[state=active]:text-white text-xs">
              Overview
            </TabsTrigger>
            <TabsTrigger value="trend" className="data-[state=active]:bg-blue-600 data-[state=active]:text-white text-xs">
              Trend
            </TabsTrigger>
            <TabsTrigger value="engine" className="data-[state=active]:bg-blue-600 data-[state=active]:text-white text-xs">
              Engine
            </TabsTrigger>
            {ciiRating && (
              <TabsTrigger value="compliance" className="data-[state=active]:bg-blue-600 data-[state=active]:text-white text-xs">
                CII
              </TabsTrigger>
            )}
          </TabsList>

          {/* Overview Tab */}
          <TabsContent value="overview" className="space-y-3">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
              <Card className="shadow-lg">
                <CardHeader className="bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-900/20 dark:to-blue-800/20 p-4">
                  <CardTitle className="flex items-center gap-2 text-base">
                    <TrendingUp className="h-4 w-4 text-blue-600" />
                    Weekly Performance
                  </CardTitle>
                  <CardDescription className="text-xs">Last 7 days</CardDescription>
                </CardHeader>
                <CardContent className="space-y-2 pt-4">
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">Fuel</span>
                    <span className="font-bold">{safeFixed(weeklySummary.totalFuelConsumedMT, 2)} MT</span>
                  </div>
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">Distance</span>
                    <span className="font-bold">{safeFixed(weeklySummary.distanceNauticalMiles, 2)} NM</span>
                  </div>
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">SFOC</span>
                    <span className="font-bold">{safeFixed(weeklySummary.sfoc, 2)} g/kWh</span>
                  </div>
                  <div className="flex justify-between py-1.5 text-sm">
                    <span className="text-xs font-medium text-slate-600">Quality</span>
                    <span className="font-bold">{safeFixed(weeklySummary.dataQualityScore, 0)}%</span>
                  </div>
                </CardContent>
              </Card>

              <Card className="shadow-lg">
                <CardHeader className="bg-gradient-to-r from-green-50 to-green-100 dark:from-green-900/20 dark:to-green-800/20 p-4">
                  <CardTitle className="flex items-center gap-2 text-base">
                    <TrendingDown className="h-4 w-4 text-green-600" />
                    Monthly Performance
                  </CardTitle>
                  <CardDescription className="text-xs">Last 30 days</CardDescription>
                </CardHeader>
                <CardContent className="space-y-2 pt-4">
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">Fuel</span>
                    <span className="font-bold">{safeFixed(monthlySummary.totalFuelConsumedMT, 2)} MT</span>
                  </div>
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">EEOI</span>
                    <span className="font-bold">{safeFixed(monthlySummary.eeoi, 2)}</span>
                  </div>
                  <div className="flex justify-between py-1.5 border-b text-sm">
                    <span className="text-xs font-medium text-slate-600">CII</span>
                    <Badge variant={getCIIRatingBadgeVariant(monthlySummary.ciiRating)} className="text-xs">
                      {monthlySummary.ciiRating}
                    </Badge>
                  </div>
                  <div className="flex justify-between py-1.5 text-sm">
                    <span className="text-xs font-medium text-slate-600">Quality</span>
                    <span className="font-bold">{safeFixed(monthlySummary.dataQualityScore, 0)}%</span>
                  </div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {/* Trend Tab */}
          <TabsContent value="trend" className="space-y-3">
            <Card className="shadow-lg">
              <CardHeader className="p-4">
                <CardTitle className="text-base">Fuel Consumption</CardTitle>
              </CardHeader>
              <CardContent className="p-2">
                <ResponsiveContainer width="100%" height={250}>
                  <AreaChart data={trend.filter(d => d.fuelConsumedMT > 0)}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" tickFormatter={(d) => new Date(d).toLocaleDateString('en', { month: 'short', day: 'numeric' })} />
                    <YAxis />
                    <Tooltip />
                    <Area type="monotone" dataKey="fuelConsumedMT" fill="#2563eb" stroke="#1e40af" />
                  </AreaChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card className="shadow-lg">
              <CardHeader className="p-4">
                <CardTitle className="text-base">CO₂ Emissions</CardTitle>
              </CardHeader>
              <CardContent className="p-2">
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={trend.filter(d => d.cO2EmissionsMT > 0)}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" tickFormatter={(d) => new Date(d).toLocaleDateString('en', { month: 'short', day: 'numeric' })} />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="cO2EmissionsMT" fill="#16a34a" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Engine Tab */}
          <TabsContent value="engine" className="space-y-3">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              <Card>
                <CardHeader className="p-4">
                  <CardTitle className="flex items-center gap-2 text-base">
                    <Gauge className="h-4 w-4" />
                    RPM
                  </CardTitle>
                </CardHeader>
                <CardContent className="pt-0">
                  <div className="text-3xl font-bold">{safeFixed(weeklySummary.avgMainEngineRPM, 0)}</div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="p-4">
                  <CardTitle className="flex items-center gap-2 text-base">
                    <Activity className="h-4 w-4" />
                    Load
                  </CardTitle>
                </CardHeader>
                <CardContent className="pt-0">
                  <div className="text-3xl font-bold">{safeFixed(weeklySummary.avgMainEngineLoad, 1)}%</div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="p-4">
                  <CardTitle className="flex items-center gap-2 text-base">
                    <Target className="h-4 w-4" />
                    SFOC
                  </CardTitle>
                </CardHeader>
                <CardContent className="pt-0">
                  <div className="text-3xl font-bold">{safeFixed(weeklySummary.sfoc, 1)}</div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {/* CII Tab */}
          {ciiRating && (
            <TabsContent value="compliance">
              <Card>
                <CardHeader className="p-4">
                  <CardTitle className="text-base">CII Boundaries</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="grid grid-cols-5 gap-2">
                    {Object.entries(ciiRating.boundaries).map(([r, v]) => {
                      const letter = r.replace('rating', '').toUpperCase();
                      return (
                        <div key={r} className={`p-2.5 rounded text-center transition-all ${
                          letter === ciiRating.rating ? 'bg-blue-500 text-white scale-105' : 'bg-gray-100'
                        }`}>
                          <div className="font-bold text-sm">{letter}</div>
                          <div className="text-xs">≤{v.toFixed(1)}</div>
                        </div>
                      );
                    })}
                  </div>
                  <div className="mt-3 space-y-1.5">
                    {ciiRating.recommendations.map((rec, i) => (
                      <div key={i} className="text-xs p-2 bg-gray-50 rounded">{rec}</div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>
          )}
        </Tabs>
      </div>
    </div>
  );
};

export default FuelAnalyticsPage;
