import React, { useState, useEffect, useCallback } from 'react';
import { VesselMap, type VesselTrackData } from '@/components/vessel/VesselMap';
import axios from 'axios';
import { getAuthToken } from '@/services/api.client';

const API_BASE = '/api';

interface PositionResponse {
  latest: {
    id: string;
    timestamp: string;
    latitude: number;
    longitude: number;
    speedOverGround: number | null;
    courseOverGround: number | null;
    source: string;
    originNode: string;
    createdAt: string;
  } | null;
  route: Array<{
    id: string;
    timestamp: string;
    latitude: number;
    longitude: number;
    speedOverGround: number | null;
    courseOverGround: number | null;
    source: string;
    originNode: string;
    createdAt: string;
  }>;
  stats: {
    totalPoints: number;
    distanceNm: number;
    avgSpeedKn: number;
  };
}

interface VesselOption {
  id: string;
  name: string;
  imo: string;
  originNode: string;
}

const TIME_FILTERS = [
  { label: '6h', hours: 6 },
  { label: '24h', hours: 24 },
  { label: '3d', hours: 72 },
  { label: '7d', hours: 168 },
  { label: '30d', hours: 720 },
];

// Color palette for vessels on the map
const VESSEL_COLORS = [
  '#ef4444', '#3b82f6', '#22c55e', '#f59e0b',
  '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
  '#6366f1', '#84cc16', '#06b6d4', '#d946ef',
];

export const VesselTrackingPage: React.FC = () => {
  const [vessels, setVessels] = useState<VesselOption[]>([]);
  const [selectedHours, setSelectedHours] = useState<number>(24);
  const [allVesselData, setAllVesselData] = useState<VesselTrackData[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Lấy danh sách tàu và dữ liệu vị trí của tất cả
  useEffect(() => {
    const fetchVessels = async () => {
      try {
        const token = getAuthToken();
        const res = await axios.get(`${API_BASE}/vessels`, {
          headers: { Authorization: `Bearer ${token}` },
          params: { pageSize: 100 },
        });
        const data = res.data;
        const list = (data?.data || data || []).map((v: any) => ({
          id: v.id,
          name: v.name || v.vesselName || 'Unknown',
          imo: v.imo || v.imoNumber || '',
          originNode: v.originNode || '',
        }));
        setVessels(list);
      } catch (err) {
        console.error('Failed to load vessels:', err);
      }
    };
    fetchVessels();
  }, []);

  // Lấy dữ liệu vị trí cho TẤT CẢ tàu
  const fetchAllPositions = useCallback(async () => {
    if (vessels.length === 0) return;
    setLoading(true);
    setError(null);
    try {
      const token = getAuthToken();
      const headers = { Authorization: `Bearer ${token}` };
      
      const results = await Promise.allSettled(
        vessels.map(v =>
          axios.get<PositionResponse>(
            `${API_BASE}/VesselTelemetry/vessel/${v.id}/realtime`,
            { headers, params: { hours: selectedHours } }
          )
        )
      );

      const trackData: VesselTrackData[] = [];
      results.forEach((result, idx) => {
        if (result.status === 'fulfilled' && result.value.data?.latest) {
          const d = result.value.data;
          const v = vessels[idx];
          trackData.push({
            id: v.id,
            name: v.name,
            color: VESSEL_COLORS[idx % VESSEL_COLORS.length],
            position: {
              latitude: d.latest.latitude,
              longitude: d.latest.longitude,
              speedOverGround: d.latest.speedOverGround ?? undefined,
              courseOverGround: d.latest.courseOverGround ?? undefined,
              timestamp: d.latest.timestamp,
            },
            route: (d.route || []).map(p => ({
              latitude: p.latitude,
              longitude: p.longitude,
              speedOverGround: p.speedOverGround ?? undefined,
              courseOverGround: p.courseOverGround ?? undefined,
              timestamp: p.timestamp,
            })),
          });
        }
      });

      setAllVesselData(trackData);
      if (trackData.length === 0) {
        setError('Không có tàu nào có dữ liệu vị trí.');
      }
    } catch (err: any) {
      setError(err?.message || 'Failed to load position data');
      console.error('Failed to load positions:', err);
    } finally {
      setLoading(false);
    }
  }, [vessels, selectedHours]);

  // Auto-refresh mỗi 30s
  useEffect(() => {
    fetchAllPositions();
    const interval = setInterval(fetchAllPositions, 30000);
    return () => clearInterval(interval);
  }, [fetchAllPositions]);

  // Parse dữ liệu cho map (giữ cho backward-compat)
  const hasData = allVesselData.length > 0;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="p-4 md:p-6 max-w-[1600px] mx-auto">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            🗺️ Vessel Tracking
          </h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">
            Real-time GPS position tracking — all vessels on one map
          </p>
        </div>

        {/* Controls Bar */}
        <div className="flex flex-wrap items-center gap-3 mb-4 bg-white dark:bg-gray-800 rounded-xl p-3 shadow-sm border border-gray-200 dark:border-gray-700">
          {/* Legend */}
          <div className="flex flex-wrap items-center gap-3">
            {allVesselData.map(v => (
              <div key={v.id} className="flex items-center gap-1.5 text-xs">
                <span className="inline-block w-3 h-3 rounded-full" style={{ backgroundColor: v.color }} />
                <span className="font-medium text-gray-700 dark:text-gray-200">{v.name}</span>
              </div>
            ))}
            {allVesselData.length === 0 && !loading && (
              <span className="text-sm text-gray-500 italic">No vessels with position data</span>
            )}
          </div>

          {/* Spacer */}
          <div className="flex-1" />

          {/* Time Filters */}
          <div className="flex items-center gap-1">
            <label className="text-sm font-medium text-gray-600 dark:text-gray-300 mr-1">Time:</label>
            {TIME_FILTERS.map(f => (
              <button
                key={f.hours}
                onClick={() => setSelectedHours(f.hours)}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                  selectedHours === f.hours
                    ? 'bg-blue-600 text-white shadow-sm'
                    : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                }`}
              >
                {f.label}
              </button>
            ))}
          </div>

          {/* Manual Refresh */}
          <button
            onClick={fetchAllPositions}
            disabled={loading}
            className="px-4 py-2 rounded-lg bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors flex items-center gap-1"
          >
            {loading ? (
              <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              '↻'
            )}
            Refresh
          </button>
        </div>

        {/* Error */}
        {error && (
          <div className="mb-4 p-3 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 rounded-lg text-red-700 dark:text-red-300 text-sm">
            {error}
          </div>
        )}

        {/* Map */}
        {loading && allVesselData.length === 0 ? (
          <div className="flex items-center justify-center h-[500px] bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
            <div className="text-center">
              <div className="inline-block w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-3" />
              <div className="text-gray-500">Loading vessel positions...</div>
            </div>
          </div>
        ) : (
          <VesselMap
            vessels={allVesselData.length > 0 ? allVesselData : undefined}
            positions={[]}
            currentPosition={null}
            autoFit={true}
            height="600px"
            className="mb-4"
          />
        )}

        {/* Summary info */}
        {allVesselData.length > 0 && (
          <div className="mt-4 p-4 bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
            <div className="text-sm text-gray-600 dark:text-gray-300">
              <span className="font-semibold text-gray-900 dark:text-white">Tracking {allVesselData.length} vessel(s)</span>
              {' · '}Last update: {new Date().toLocaleTimeString()}
              {' · '}Time range: {selectedHours}h
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
