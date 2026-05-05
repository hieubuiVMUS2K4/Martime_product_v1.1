import React, { useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, Popup, Tooltip, Polyline, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import plannedRouteData from '../../assets/planned-route.json';

// Fix default marker icon issue with bundlers
import iconUrl from 'leaflet/dist/images/marker-icon.png';
import iconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png';
import shadowUrl from 'leaflet/dist/images/marker-shadow.png';

// @ts-expect-error _getIconUrl is internal Leaflet API
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({ iconUrl, iconRetinaUrl, shadowUrl });

/** Tọa độ GPS */
export interface GpsPoint {
  latitude: number;
  longitude: number;
  speedOverGround?: number;
  courseOverGround?: number;
  timestamp?: string;
  satellitesUsed?: number;
}

/** Dữ liệu 1 tàu trên bản đồ (multi-vessel mode) */
export interface VesselTrackData {
  id: string;
  name: string;
  color: string;
  position: GpsPoint;
  route: GpsPoint[];
  dashArray?: string; // e.g. "2,4" or "5,8"
}

/** Props cho VesselMap component */
interface VesselMapProps {
  /** Danh sách điểm GPS cho polyline hành trình (single mode) */
  positions?: GpsPoint[];
  /** Vị trí hiện tại của tàu (marker) (single mode) */
  currentPosition?: GpsPoint | null;
  /** Danh sách nhiều tàu (multi mode) */
  vessels?: VesselTrackData[];
  /** Tự động fit bounds khi có dữ liệu mới */
  autoFit?: boolean;
  /** Chiều cao của map */
  height?: string;
  /** CSS class */
  className?: string;
  /** Id tàu cần focus — map sẽ flyTo vị trí tàu đó */
  focusVesselId?: string;
  /** Callback khi click vào marker tàu */
  onVesselSelect?: (vesselId: string) => void;
}

/** SVG hình thuyền — nhìn từ trên xuống, mũi nhọn hướng lên để hiển thị hướng đi */
const shipSvg = `<svg width="26" height="26" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
  <!-- Mũi tàu nhọn (hướng đi) -->
  <path d="M50 2 L60 28 L55 30 L50 25 L45 30 L40 28 Z" fill="white"/>
  <!-- Thân tàu hình thoi -->
  <path d="M40 28 L20 45 Q15 55 18 65 L30 75 Q40 80 50 80 Q60 80 70 75 L82 65 Q85 55 80 45 L60 28 Z" fill="white"/>
  <!-- Boong tàu -->
  <path d="M38 32 L22 48 Q18 56 22 64 L32 72 Q40 76 50 76 Q60 76 68 72 L78 64 Q82 56 78 48 L62 32 Z" fill="rgba(255,255,255,0.5)"/>
  <!-- Đài chỉ huy -->
  <rect x="42" y="30" width="16" height="14" rx="2" fill="white" stroke="rgba(0,0,0,0.2)" stroke-width="1"/>
  <!-- Cột radar -->
  <line x1="50" y1="28" x2="50" y2="6" stroke="white" stroke-width="1.8" stroke-linecap="round"/>
  <line x1="44" y1="10" x2="56" y2="10" stroke="white" stroke-width="1.2" stroke-linecap="round"/>
  <!-- Cửa sổ cabin -->
  <rect x="45" y="33" width="4" height="3" rx="1" fill="rgba(0,0,0,0.2)"/>
  <rect x="51" y="33" width="4" height="3" rx="1" fill="rgba(0,0,0,0.2)"/>
  <!-- Đường nước -->
  <path d="M18 68 Q30 64 50 66 Q70 68 82 66" stroke="rgba(255,255,255,0.6)" stroke-width="1.5" fill="none"/>
</svg>`;

/** Tạo icon tàu với màu tùy chỉnh và hướng đi (courseOverGround) */
function makeShipIcon(color: string, course?: number) {
  const rotation = course != null ? course : 0;
  return new L.DivIcon({
    className: 'vessel-marker',
    html: `<div style="
      width: 36px; height: 36px;
      background: ${color};
      border: 3px solid white;
      border-radius: 50%;
      box-shadow: 0 3px 10px rgba(0,0,0,0.3), 0 0 0 2px ${color}33;
      display: flex;
      align-items: center;
      justify-content: center;
      transform: rotate(${rotation}deg);
    ">
      ${shipSvg}
    </div>`,
    iconSize: [36, 36],
    iconAnchor: [18, 18],
    popupAnchor: [0, -22],
  });
}

/** Dash array patterns for different routes */
const dashPatterns = [
  '2,4',    // Route 1: South
  '5,8',    // Route 2: North  
  '3,6',    // Route 3: Central
  '4,7',    // Route 4: Southwest
  '6,9',    // Route 5: Regional
];

/** Get dash pattern by vessel index */
function getDashPattern(index: number): string {
  return dashPatterns[index % dashPatterns.length];
}

/** Component con để tự động fly-to và fit bounds */
function MapController({ 
  currentPosition, 
  positions, 
  vessels,
  autoFit,
  focusVesselId,
}: {
  currentPosition: GpsPoint | null;
  positions: GpsPoint[];
  vessels?: VesselTrackData[];
  autoFit: boolean;
  focusVesselId?: string;
}) {
  const map = useMap();
  const prevLat = useRef<number | null>(null);
  const prevLng = useRef<number | null>(null);
  const prevFocusId = useRef<string | null>(null);

  useEffect(() => {
    if (!currentPosition) return;

    // Fly to current position on first load
    if (prevLat.current === null && prevLng.current === null) {
      // In multi mode, don't override the view set by fitBounds
      if (vessels && vessels.length > 1) return;
      map.setView([currentPosition.latitude, currentPosition.longitude], 13);
      prevLat.current = currentPosition.latitude;
      prevLng.current = currentPosition.longitude;
      return;
    }

    // Smooth pan to new position
    if (autoFit && prevLat.current && prevLng.current) {
      const dist = Math.sqrt(
        Math.pow(currentPosition.latitude - prevLat.current, 2) +
        Math.pow(currentPosition.longitude - prevLng.current, 2)
      );
      // Only pan if significant movement (> 0.0001 deg ≈ 11m)
      if (dist > 0.0001) {
        map.panTo([currentPosition.latitude, currentPosition.longitude], { animate: true, duration: 0.5 });
      }
    }

    prevLat.current = currentPosition.latitude;
    prevLng.current = currentPosition.longitude;
  }, [currentPosition, map, autoFit, vessels]);

  // Fit bounds — collect all points from single or multi mode
  useEffect(() => {
    if (!autoFit) return;
    
    let allPoints: GpsPoint[] = [];
    
    if (vessels && vessels.length > 0) {
      vessels.forEach(v => {
        if (v.position) allPoints.push(v.position);
        if (v.route) allPoints = allPoints.concat(v.route);
      });
    } else {
      allPoints = positions;
    }
    
    if (allPoints.length < 2) return;

    const bounds = L.latLngBounds(
      allPoints.map(p => [p.latitude, p.longitude] as [number, number])
    );
    map.fitBounds(bounds, { padding: [50, 50], maxZoom: 15 });
  }, [positions, vessels, map, autoFit]);

  // Fly to focused vessel when focusVesselId changes
  useEffect(() => {
    if (!focusVesselId || focusVesselId === prevFocusId.current) return;
    prevFocusId.current = focusVesselId;

    if (vessels) {
      const vessel = vessels.find(v => v.id === focusVesselId);
      if (vessel && vessel.position) {
        map.flyTo([vessel.position.latitude, vessel.position.longitude], 13, {
          duration: 1,
        });
      }
    }
  }, [focusVesselId, vessels, map]);

  return null;
}

export const VesselMap: React.FC<VesselMapProps> = ({
  positions,
  currentPosition,
  vessels,
  autoFit = true,
  height = '600px',
  className = '',
  focusVesselId,
  onVesselSelect,
}) => {
  const isMulti = vessels && vessels.length > 0;

  // Single mode
  const defaultCenter: [number, number] = !isMulti && currentPosition
    ? [currentPosition.latitude, currentPosition.longitude]
    : [15.0, 108.5]; // Center of Vietnam

  const routePoints: [number, number][] = isMulti
    ? []
    : (positions || [])
        .filter(p => p.latitude && p.longitude)
        .map(p => [p.latitude, p.longitude]);

  const plannedPoints: [number, number][] = (plannedRouteData as any[])
    .filter(p => p.latitude && p.longitude)
    .map(p => [p.latitude, p.longitude]);

  // Palette for multi-vessel mode
  const colorPalette = [
    '#ef4444', '#3b82f6', '#22c55e', '#f59e0b',
    '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
    '#6366f1', '#84cc16', '#06b6d4', '#d946ef',
  ];

  return (
    <div className={`rounded-xl overflow-hidden shadow-lg border border-gray-200 dark:border-gray-700 ${className}`}>
      <MapContainer
        center={defaultCenter}
        zoom={6}
        style={{ height, width: '100%' }}
        scrollWheelZoom={true}
        zoomControl={true}
      >
        {/* OpenStreetMap Tiles */}
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {/* Map Controller */}
        <MapController
          currentPosition={isMulti ? null : currentPosition ?? null}
          positions={isMulti ? [] : (positions || [])}
          vessels={vessels}
          autoFit={autoFit}
          focusVesselId={focusVesselId}
        />

        {/* Tuyến đường dự định Vũng Tàu → Hải Phòng */}
        {plannedPoints.length >= 2 && (
          <Polyline
            positions={plannedPoints}
            color="#6b7280"
            weight={3}
            opacity={0.8}
            dashArray="5, 10"
            smoothFactor={1}
          />
        )}

        {/* ── Multi-vessel mode ── */}
        {isMulti && vessels.map((v, idx) => {
          const vColor = v.color || colorPalette[idx % colorPalette.length];
          const vDashArray = v.dashArray || getDashPattern(idx);
          const vRoute: [number, number][] = (v.route || [])
            .filter(p => p.latitude && p.longitude)
            .map(p => [p.latitude, p.longitude]);
          const hasPos = v.position && v.position.latitude && v.position.longitude;

          return (
            <React.Fragment key={v.id}>
              {/* Route polyline with dashed style */}
              {vRoute.length >= 2 && (
                <Polyline 
                  positions={vRoute} 
                  color={vColor} 
                  weight={2.5} 
                  opacity={0.7}
                  dashArray={vDashArray}
                  smoothFactor={1} 
                />
              )}
              {/* Marker with auto-open tooltip chứa đầy đủ thông tin */}
              {hasPos && (
                <Marker
                  position={[v.position.latitude, v.position.longitude]}
                  icon={makeShipIcon(vColor, v.position.courseOverGround)}
                  eventHandlers={{
                    click: () => onVesselSelect?.(v.id),
                  }}
                >
                  {/* Tooltip permanent — tự động hiện thông tin đầy đủ, không cần click */}
                  <Tooltip permanent direction="top" offset={[0, -12]}>
                    <div
                      className="text-sm min-w-[220px] rounded-lg shadow-xl border bg-white"
                      style={{ borderColor: vColor, borderWidth: '1.5px' }}
                    >
                      {/* Header */}
                      <div
                        className="font-bold text-base px-3 py-2 rounded-t-lg flex items-center justify-between"
                        style={{ backgroundColor: vColor, color: 'white' }}
                      >
                        <span>🚢 {v.name}</span>
                      </div>
                      {/* Body */}
                      <div className="px-3 py-2 space-y-1.5">
                        <div className="grid grid-cols-2 gap-x-3 gap-y-1">
                          <span className="text-gray-500 text-xs">Latitude:</span>
                          <span className="font-mono text-sm font-semibold">{v.position.latitude.toFixed(6)}°</span>
                          <span className="text-gray-500 text-xs">Longitude:</span>
                          <span className="font-mono text-sm font-semibold">{v.position.longitude.toFixed(6)}°</span>
                          {v.position.speedOverGround != null && (
                            <>
                              <span className="text-gray-500 text-xs">Speed:</span>
                              <span className="font-semibold text-sm">{v.position.speedOverGround.toFixed(1)} kn</span>
                            </>
                          )}
                          {v.position.courseOverGround != null && (
                            <>
                              <span className="text-gray-500 text-xs">Course:</span>
                              <span className="font-semibold text-sm">{v.position.courseOverGround.toFixed(1)}°</span>
                            </>
                          )}
                          {v.position.timestamp && (
                            <>
                              <span className="text-gray-500 text-xs">Time (UTC):</span>
                              <span className="text-sm">{new Date(v.position.timestamp).toLocaleTimeString('en-GB')}</span>
                            </>
                          )}
                        </div>
                        <div className="text-xs text-gray-400 pt-1.5 border-t border-gray-100">
                          Route: {vDashArray} • {vRoute.length} waypoints
                        </div>
                      </div>
                    </div>
                  </Tooltip>
                </Marker>
              )}
            </React.Fragment>
          );
        })}

        {/* ── Single-vessel mode (backward compat) ── */}
        {!isMulti && currentPosition && currentPosition.latitude && currentPosition.longitude && (
          <Marker
            position={[currentPosition.latitude, currentPosition.longitude]}
            icon={makeShipIcon('#ef4444', currentPosition.courseOverGround)}
          >
            <Popup>
              <div className="text-sm min-w-[220px]">
                <div className="font-bold text-base mb-2 pb-2 border-b">🚢 Vessel Position</div>
                <div className="space-y-2">
                  <div className="grid grid-cols-2 gap-x-3 gap-y-1 text-gray-700">
                    <span className="text-gray-500 text-xs">Latitude:</span>
                    <span className="font-mono text-sm">{currentPosition.latitude.toFixed(6)}°</span>
                    <span className="text-gray-500 text-xs">Longitude:</span>
                    <span className="font-mono text-sm">{currentPosition.longitude.toFixed(6)}°</span>
                    {currentPosition.speedOverGround != null && (
                      <>
                        <span className="text-gray-500 text-xs">Speed Over Ground:</span>
                        <span className="font-semibold text-sm">{currentPosition.speedOverGround.toFixed(1)} kn</span>
                      </>
                    )}
                    {currentPosition.courseOverGround != null && (
                      <>
                        <span className="text-gray-500 text-xs">Course Over Ground:</span>
                        <span className="font-semibold text-sm">{currentPosition.courseOverGround.toFixed(1)}°</span>
                      </>
                    )}
                    {currentPosition.timestamp && (
                      <>
                        <span className="text-gray-500 text-xs">Time (UTC):</span>
                        <span className="text-sm">{new Date(currentPosition.timestamp).toLocaleTimeString('en-GB')}</span>
                      </>
                    )}
                    {currentPosition.satellitesUsed != null && (
                      <>
                        <span className="text-gray-500 text-xs">Satellites:</span>
                        <span className="text-sm">{currentPosition.satellitesUsed}</span>
                      </>
                    )}
                  </div>
                </div>
              </div>
            </Popup>
          </Marker>
        )}

        {/* Single-mode route */}
        {!isMulti && routePoints.length >= 2 && (
          <Polyline
            positions={routePoints}
            color="#3b82f6"
            weight={3}
            opacity={0.7}
            smoothFactor={1}
          />
        )}
      </MapContainer>
    </div>
  );
};
