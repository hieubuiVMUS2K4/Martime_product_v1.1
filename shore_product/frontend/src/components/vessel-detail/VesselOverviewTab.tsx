import React from 'react';
import {
  Ship, Navigation, Activity, Users, Anchor, Wind,
  Thermometer, Clock, MapPin, Fuel, Gauge, Wrench,
  AlertTriangle, CheckCircle, XCircle
} from 'lucide-react';

interface Vessel {
  id: string;
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  grossTonnage: number;
  deadWeight: number;
  buildDate: string;
  flag: string;
  isActive: boolean;
  loa?: number;
  lbp?: number;
  breadthMoulded?: number;
  depthMoulded?: number;
  draftMoulded?: number;
  serviceSpeedKts?: number;
  yearBuilt?: number;
  portOfRegistry?: string;
  mmsiNumber?: string;
  masterName?: string;
  noOfCrewSafeManning?: number;
  maxPersonsAllowedOB?: number;
  hfoCbm?: number;
  mdoCbm?: number;
  freshWaterCbm?: number;
  [key: string]: any;
}

interface VesselStatus {
  latitude?: number;
  longitude?: number;
  speedOverGround?: number;
  courseOverGround?: number;
  timestamp?: string;
  captainName?: string;
  engineRunning?: boolean;
  crewCount?: number;
  lastReport?: string;
}

interface Props {
  vessel: Vessel;
  vesselStatus: VesselStatus | null;
}

/** Format số với dấu phân cách */
const fmt = (n?: number, unit = '') => n != null ? `${n.toLocaleString('en-US')}${unit}` : '—';

/** Helper: tô màu trạng thái */
const StatusBadge = ({ label, active, activeColor = 'text-green-600 bg-green-50', inactiveColor = 'text-gray-400 bg-gray-50' }: { label: string; active: boolean; activeColor?: string; inactiveColor?: string }) => (
  <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold ${active ? activeColor : inactiveColor}`}>
    <span className={`inline-block w-2 h-2 rounded-full ${active ? 'bg-green-500 animate-pulse' : 'bg-gray-300'}`} />
    {label}
  </span>
);

const InfoRow = ({ label, value, icon }: { label: string; value: string | React.ReactNode; icon?: React.ReactNode }) => (
  <div className="flex items-center justify-between py-2.5 border-b border-gray-50 last:border-0">
    <div className="flex items-center gap-2 text-sm text-gray-500">
      {icon && <span className="w-4 h-4 flex-shrink-0">{icon}</span>}
      <span>{label}</span>
    </div>
    <div className="text-sm font-semibold text-gray-800 text-right">{value}</div>
  </div>
);

const SectionCard = ({ title, icon, children }: { title: string; icon: React.ReactNode; children: React.ReactNode }) => (
  <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
    <div className="px-4 py-3 bg-gray-50 border-b border-gray-100 flex items-center gap-2">
      {icon}
      <h3 className="font-bold text-sm text-gray-800 uppercase tracking-wider">{title}</h3>
    </div>
    <div className="px-4 py-1">
      {children}
    </div>
  </div>
);

export const VesselOverviewTab: React.FC<Props> = ({ vessel, vesselStatus }) => {
  const isEngineRunning = vesselStatus?.engineRunning ?? false;

  return (
    <div className="space-y-5">
      {/* ── Dòng 1: Cards trạng thái sống ── */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Engine Status */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <div className="flex items-start justify-between mb-3">
            <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${isEngineRunning ? 'bg-green-100' : 'bg-gray-100'}`}>
              <Activity className={`w-6 h-6 ${isEngineRunning ? 'text-green-600' : 'text-gray-400'}`} />
            </div>
            <StatusBadge label={isEngineRunning ? 'Running' : 'Stopped'} active={isEngineRunning} />
          </div>
          <p className="text-sm font-semibold text-gray-700 mb-1">Main Engine</p>
          <p className="text-2xl font-bold text-gray-900 font-mono">
            {vesselStatus?.speedOverGround != null ? `${vesselStatus.speedOverGround.toFixed(1)} kn` : '—'}
          </p>
          <p className="text-xs text-gray-400 mt-1">Speed Through Water</p>
        </div>

        {/* Position */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <div className="flex items-start justify-between mb-3">
            <div className="w-12 h-12 rounded-xl flex items-center justify-center bg-indigo-100">
              <MapPin className="w-6 h-6 text-indigo-600" />
            </div>
            <span className="text-[10px] text-gray-400 font-mono">
              {vesselStatus?.timestamp
                ? new Date(vesselStatus.timestamp).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
                : '—'}
            </span>
          </div>
          <p className="text-sm font-semibold text-gray-700 mb-1">Current Position</p>
          {vesselStatus?.latitude != null ? (
            <>
              <p className="text-sm font-bold text-gray-900 font-mono">
                {vesselStatus.latitude.toFixed(4)}°N
              </p>
              <p className="text-sm font-bold text-gray-900 font-mono">
                {vesselStatus.longitude.toFixed(4)}°E
              </p>
            </>
          ) : (
            <p className="text-sm text-gray-400">No position data</p>
          )}
          {vesselStatus?.courseOverGround != null && (
            <p className="text-xs text-gray-400 mt-1">
              Course: {vesselStatus.courseOverGround.toFixed(0)}°
            </p>
          )}
        </div>

        {/* Crew & Captain */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <div className="flex items-start justify-between mb-3">
            <div className="w-12 h-12 rounded-xl flex items-center justify-center bg-amber-100">
              <Users className="w-6 h-6 text-amber-600" />
            </div>
          </div>
          <p className="text-sm font-semibold text-gray-700 mb-1">Crew Onboard</p>
          <p className="text-2xl font-bold text-gray-900">
            {vesselStatus?.crewCount != null ? `${vesselStatus.crewCount}` : fmt(vessel.noOfCrewSafeManning)}
          </p>
          <div className="mt-2 pt-2 border-t border-gray-100">
            <p className="text-xs text-gray-500">Captain</p>
            <p className="text-sm font-bold text-gray-800 truncate">
              {vesselStatus?.captainName || vessel.masterName || '—'}
            </p>
          </div>
        </div>

        {/* Vessel Info */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <div className="flex items-start justify-between mb-3">
            <div className="w-12 h-12 rounded-xl flex items-center justify-center bg-blue-100">
              <Ship className="w-6 h-6 text-blue-600" />
            </div>
          </div>
          <p className="text-sm font-semibold text-gray-700 mb-1">{vessel.vesselType || 'Vessel'}</p>
          <p className="text-xs text-gray-500 font-mono">IMO {vessel.imo}</p>
          <div className="mt-2 pt-2 border-t border-gray-100 space-y-1">
            <div className="flex justify-between text-xs">
              <span className="text-gray-400">Flag</span>
              <span className="font-semibold text-gray-700">{vessel.flag || '—'}</span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-gray-400">Call Sign</span>
              <span className="font-semibold text-gray-700">{vessel.callSign || '—'}</span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-gray-400">MMSI</span>
              <span className="font-semibold text-gray-700">{vessel.mmsiNumber || '—'}</span>
            </div>
          </div>
        </div>
      </div>

      {/* ── Dòng 2: Thông tin chi tiết tàu ── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Thông số kỹ thuật */}
        <SectionCard title="Dimensions & Tonnage" icon={<Ship className="w-4 h-4 text-blue-600" />}>
          <InfoRow label="L.O.A" value={fmt(vessel.loa, ' m')} icon={<span className="text-[10px]">📏</span>} />
          <InfoRow label="L.B.P" value={fmt(vessel.lbp, ' m')} />
          <InfoRow label="Breadth (Moulded)" value={fmt(vessel.breadthMoulded, ' m')} />
          <InfoRow label="Depth (Moulded)" value={fmt(vessel.depthMoulded, ' m')} />
          <InfoRow label="Draft (Moulded)" value={fmt(vessel.draftMoulded, ' m')} />
          <InfoRow label="Gross Tonnage" value={fmt(vessel.grossTonnage)} />
          <InfoRow label="Deadweight" value={fmt(vessel.deadWeight, ' t')} />
          <InfoRow label="Year Built" value={vessel.yearBuilt ? `${vessel.yearBuilt}` : '—'} />
          <InfoRow label="Service Speed" value={fmt(vessel.serviceSpeedKts, ' kn')} />
        </SectionCard>

        {/* Thông tin chủ tàu & quản lý */}
        <SectionCard title="Management & Registry" icon={<Wrench className="w-4 h-4 text-gray-600" />}>
          <InfoRow label="Port of Registry" value={vessel.portOfRegistry || '—'} icon={<Anchor className="w-3.5 h-3.5" />} />
          <InfoRow label="Flag State" value={vessel.flag || '—'} />
          <InfoRow label="Safe Manning" value={vessel.noOfCrewSafeManning ? `${vessel.noOfCrewSafeManning} persons` : '—'} />
          <InfoRow label="Max Persons OB" value={vessel.maxPersonsAllowedOB ? `${vessel.maxPersonsAllowedOB}` : '—'} />
          <InfoRow label="Build Date" value={vessel.buildDate ? new Date(vessel.buildDate).toLocaleDateString('vi-VN') : '—'} />
          <InfoRow label="Vessel Type" value={vessel.vesselType || '—'} />
        </SectionCard>

        {/* Tanks & Cargo */}
        <SectionCard title="Tanks & Cargo Capacity" icon={<Fuel className="w-4 h-4 text-amber-600" />}>
          <InfoRow label="HFO Capacity" value={fmt(vessel.hfoCbm, ' m³')} icon={<Fuel className="w-3.5 h-3.5" />} />
          <InfoRow label="MDO Capacity" value={fmt(vessel.mdoCbm, ' m³')} />
          <InfoRow label="Fresh Water" value={fmt(vessel.freshWaterCbm, ' m³')} />
          <InfoRow label="Lub Oil" value={fmt(vessel.lubOilCbm, ' m³')} />
        </SectionCard>
      </div>

      {/* ── Dòng 3: Last Sync / Timestamps ── */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
        <div className="flex items-center gap-2 mb-2">
          <Clock className="w-4 h-4 text-gray-500" />
          <h3 className="font-bold text-sm text-gray-800">Sync & Timestamp</h3>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs">
          <div>
            <p className="text-gray-400">Last Edge Sync</p>
            <p className="font-semibold text-gray-700 mt-0.5">
              {vessel.lastEdgeSyncAt
                ? new Date(vessel.lastEdgeSyncAt).toLocaleString('vi-VN')
                : '—'}
            </p>
          </div>
          <div>
            <p className="text-gray-400">Last Shore Sync</p>
            <p className="font-semibold text-gray-700 mt-0.5">
              {vessel.lastShoreSyncAt
                ? new Date(vessel.lastShoreSyncAt).toLocaleString('vi-VN')
                : '—'}
            </p>
          </div>
          <div>
            <p className="text-gray-400">Last Position Report</p>
            <p className="font-semibold text-gray-700 mt-0.5">
              {vesselStatus?.timestamp
                ? new Date(vesselStatus.timestamp).toLocaleString('vi-VN')
                : '—'}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
