import React, { useEffect, useRef } from 'react';
import { MapContainer, TileLayer, Marker, Popup, Polyline, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import plannedRouteData from '../../assets/planned-route.json';

// Fix default marker icon issue with bundlers
// @ts-expect-error _getIconUrl is internal Leaflet API
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

export interface GpsPoint {
  latitude: number;
  longitude: number;
  speedOverGround?: number;
  courseOverGround?: number;
  timestamp?: string;
  satellitesUsed?: number;
}

interface VesselMapProps {
  positions: GpsPoint[];
  currentPosition: GpsPoint | null;
  autoFit?: boolean;
  height?: string;
  className?: string;
}

/** Icon tàu tùy chỉnh */
const shipIcon = new L.DivIcon({
  className: 'vessel-marker',
  html: `<div style="
    width: 32px; height: 32px;
    background: #3b82f6;
    border: 3px solid white;
    border-radius: 50%;
    box-shadow: 0 2px 8px rgba(0,0,0,0.3);
    display: flex;
    align-items: center;
    justify-content: center;
  ">
    <svg width="16" height="16" viewBox="0 0 24 24" fill="white" stroke="white">
      <path d="M12 2L4 20h16L12 2z" />
    </svg>
  </div>`,
  iconSize: [32, 32],
  iconAnchor: [16, 16],
  popupAnchor: [0, -20],
});

function MapController({ currentPosition, positions, autoFit }: {
  currentPosition: GpsPoint | null;
  positions: GpsPoint[];
  autoFit: boolean;
}) {
  const map = useMap();
  const prevLat = useRef<number | null>(null);
  const prevLng = useRef<number | null>(null);

  useEffect(() => {
    if (!currentPosition) return;
    if (prevLat.current === null && prevLng.current === null) {
      map.setView([currentPosition.latitude, currentPosition.longitude], 13);
      prevLat.current = currentPosition.latitude;
      prevLng.current = currentPosition.longitude;
      return;
    }
    if (autoFit && prevLat.current && prevLng.current) {
      const dist = Math.sqrt(
        Math.pow(currentPosition.latitude - prevLat.current, 2) +
        Math.pow(currentPosition.longitude - prevLng.current, 2)
      );
      if (dist > 0.0001) {
        map.panTo([currentPosition.latitude, currentPosition.longitude], { animate: true, duration: 0.5 });
      }
    }
    prevLat.current = currentPosition.latitude;
    prevLng.current = currentPosition.longitude;
  }, [currentPosition, map, autoFit]);

  useEffect(() => {
    if (positions.length < 2 || !autoFit) return;
    const bounds = L.latLngBounds(
      positions.map(p => [p.latitude, p.longitude] as [number, number])
    );
    map.fitBounds(bounds, { padding: [50, 50], maxZoom: 15 });
  }, [positions, map, autoFit]);

  return null;
}

export const VesselMap: React.FC<VesselMapProps> = ({
  positions,
  currentPosition,
  autoFit = true,
  height = '600px',
  className = '',
}) => {
  const defaultCenter: [number, number] = currentPosition
    ? [currentPosition.latitude, currentPosition.longitude]
    : [10.7769, 106.7009];

  const routePoints: [number, number][] = positions
    .filter(p => p.latitude && p.longitude)
    .map(p => [p.latitude, p.longitude]);

  const plannedPoints: [number, number][] = (plannedRouteData as any[])
    .filter(p => p.latitude && p.longitude)
    .map(p => [p.latitude, p.longitude]);

  return (
    <div className={`rounded-xl overflow-hidden shadow-lg border border-gray-200 dark:border-gray-700 ${className}`}>
      <MapContainer
        center={defaultCenter}
        zoom={13}
        style={{ height, width: '100%' }}
        scrollWheelZoom={true}
        zoomControl={true}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <MapController currentPosition={currentPosition} positions={positions} autoFit={autoFit} />

        {currentPosition && currentPosition.latitude && currentPosition.longitude && (
          <Marker position={[currentPosition.latitude, currentPosition.longitude]} icon={shipIcon}>
            <Popup>
              <div className="text-sm min-w-[180px]">
                <div className="font-bold text-base mb-1">🚢 Ship Position</div>
                <div className="grid grid-cols-2 gap-x-3 gap-y-1 text-gray-700">
                  <span className="text-gray-500">Lat:</span>
                  <span className="font-mono">{currentPosition.latitude.toFixed(6)}°</span>
                  <span className="text-gray-500">Lon:</span>
                  <span className="font-mono">{currentPosition.longitude.toFixed(6)}°</span>
                  {currentPosition.speedOverGround != null && (
                    <>
                      <span className="text-gray-500">SOG:</span>
                      <span className="font-semibold">{currentPosition.speedOverGround.toFixed(1)} kn</span>
                    </>
                  )}
                  {currentPosition.courseOverGround != null && (
                    <>
                      <span className="text-gray-500">COG:</span>
                      <span className="font-semibold">{currentPosition.courseOverGround.toFixed(1)}°</span>
                    </>
                  )}
                  {currentPosition.timestamp && (
                    <>
                      <span className="text-gray-500">Time:</span>
                      <span>{new Date(currentPosition.timestamp).toLocaleTimeString()}</span>
                    </>
                  )}
                </div>
              </div>
            </Popup>
          </Marker>
        )}

        {routePoints.length >= 2 && (
          <Polyline positions={routePoints} color="#3b82f6" weight={3} opacity={0.7} smoothFactor={1} />
        )}
        {plannedPoints.length >= 2 && (
          <Polyline positions={plannedPoints} color="#6b7280" weight={3} opacity={0.8} dashArray="5, 10" smoothFactor={1} />
        )}
      </MapContainer>
    </div>
  );
};
