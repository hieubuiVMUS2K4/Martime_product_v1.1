import React, { useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, Popup, Polyline, useMap } from 'react-leaflet';
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
}

/** SVG hình thuyền — nhìn từ trên xuống */
const shipSvg = `<svg width="24" height="24" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
  <!-- Thân tàu -->
  <path d="M10 70 Q20 40 30 30 L70 30 Q80 40 90 70 L85 75 Q75 65 65 60 L35 60 Q25 65 15 75 Z" fill="white"/>
  <!-- Mũi tàu -->
  <path d="M45 20 L50 10 L55 20 Z" fill="white"/>
  <!-- Đài chỉ huy -->
  <rect x="40" y="25" width="20" height="18" rx="2" fill="white" stroke="rgba(0,0,0,0.3)" stroke-width="1.5"/>
  <!-- Cột ăn-ten -->
  <line x1="50" y1="10" x2="50" y2="5" stroke="white" stroke-width="1.5" stroke-linecap="round"/>
  <line x1="46" y1="7" x2="54" y2="7" stroke="white" stroke-width="1" stroke-linecap="round"/>
  <!-- Cửa sổ cabin -->
  <rect x="43" y="28" width="5" height="4" rx="1" fill="rgba(0,0,0,0.25)"/>
  <rect x="52" y="28" width="5" height="4" rx="1" fill="rgba(0,0,0,0.25)"/>
  <!-- Đường nước -->
  <path d="M12 72 Q25 68 50 70 Q75 72 88 70" stroke="rgba(255,255,255,0.7)" stroke-width="1.5" fill="none"/>
</svg>`;

/** Tạo icon tàu với màu tùy chỉnh */
function makeShipIcon(color: string) {
  return new L.DivIcon({
    className: 'vessel-marker',
    html: `<div style="
      width: 36px; height: 36px;
      background: ${color};
      border: 3px solid white;
      border-radius: 8px;
      box-shadow: 0 3px 10px rgba(0,0,0,0.3), 0 0 0 2px ${color}33;
      display: flex;
      align-items: center;
      justify-content: center;
    ">
      ${shipSvg}
    </div>`,
    iconSize: [36, 36],
    iconAnchor: [18, 18],
    popupAnchor: [0, -22],
  });
}

/** Component con để tự động fly-to và fit bounds */
function MapController({ 
  currentPosition, 
  positions, 
  vessels,
  autoFit 
}: {
  currentPosition: GpsPoint | null;
  positions: GpsPoint[];
  vessels?: VesselTrackData[];
  autoFit: boolean;
}) {
  const map = useMap();
  const prevLat = useRef<number | null>(null);
  const prevLng = useRef<number | null>(null);

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

  return null;
}

export const VesselMap: React.FC<VesselMapProps> = ({
  positions,
  currentPosition,
  vessels,
  autoFit = true,
  height = '600px',
  className = '',
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
          const vRoute: [number, number][] = (v.route || [])
            .filter(p => p.latitude && p.longitude)
            .map(p => [p.latitude, p.longitude]);
          const hasPos = v.position && v.position.latitude && v.position.longitude;

          return (
            <React.Fragment key={v.id}>
              {/* Route polyline */}
              {vRoute.length >= 2 && (
                <Polyline positions={vRoute} color={vColor} weight={2.5} opacity={0.6} smoothFactor={1} />
              )}
              {/* Marker */}
              {hasPos && (
                <Marker
                  position={[v.position.latitude, v.position.longitude]}
                  icon={makeShipIcon(vColor)}
                >
                  <Popup>
                    <div className="text-sm min-w-[180px]">
                      <div className="font-bold text-base mb-1" style={{ color: vColor }}>🚢 {v.name}</div>
                      <div className="grid grid-cols-2 gap-x-3 gap-y-1 text-gray-700">
                        <span className="text-gray-500">Lat:</span>
                        <span className="font-mono">{v.position.latitude.toFixed(6)}°</span>
                        <span className="text-gray-500">Lon:</span>
                        <span className="font-mono">{v.position.longitude.toFixed(6)}°</span>
                        {v.position.speedOverGround != null && (
                          <><span className="text-gray-500">Speed:</span><span className="font-semibold">{v.position.speedOverGround.toFixed(1)} kn</span></>
                        )}
                        {v.position.courseOverGround != null && (
                          <><span className="text-gray-500">Course:</span><span className="font-semibold">{v.position.courseOverGround.toFixed(1)}°</span></>
                        )}
                        {v.position.timestamp && (
                          <><span className="text-gray-500">Time:</span><span>{new Date(v.position.timestamp).toLocaleTimeString()}</span></>
                        )}
                      </div>
                    </div>
                  </Popup>
                </Marker>
              )}
            </React.Fragment>
          );
        })}

        {/* ── Single-vessel mode (backward compat) ── */}
        {!isMulti && currentPosition && currentPosition.latitude && currentPosition.longitude && (
          <Marker
            position={[currentPosition.latitude, currentPosition.longitude]}
            icon={makeShipIcon('#ef4444')}
          >
            <Popup>
              <div className="text-sm min-w-[180px]">
                <div className="font-bold text-base mb-1">🚢 Vessel Position</div>
                <div className="grid grid-cols-2 gap-x-3 gap-y-1 text-gray-700">
                  <span className="text-gray-500">Lat:</span>
                  <span className="font-mono">{currentPosition.latitude.toFixed(6)}°</span>
                  <span className="text-gray-500">Lon:</span>
                  <span className="font-mono">{currentPosition.longitude.toFixed(6)}°</span>
                  {currentPosition.speedOverGround != null && (
                    <><span className="text-gray-500">Speed:</span><span className="font-semibold">{currentPosition.speedOverGround.toFixed(1)} kn</span></>
                  )}
                  {currentPosition.courseOverGround != null && (
                    <><span className="text-gray-500">Course:</span><span className="font-semibold">{currentPosition.courseOverGround.toFixed(1)}°</span></>
                  )}
                  {currentPosition.timestamp && (
                    <><span className="text-gray-500">Time:</span><span>{new Date(currentPosition.timestamp).toLocaleTimeString()}</span></>
                  )}
                  {currentPosition.satellitesUsed != null && (
                    <><span className="text-gray-500">Satellites:</span><span>{currentPosition.satellitesUsed}</span></>
                  )}
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
