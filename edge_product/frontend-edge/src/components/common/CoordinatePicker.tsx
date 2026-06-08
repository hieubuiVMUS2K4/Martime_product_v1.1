import React, { useState, useEffect, useRef } from 'react';
import { apiClient } from '@/services/api.client';

// Cache the latest position request to avoid duplicate concurrent API calls
let latestPositionPromise: Promise<{ latitude: number; longitude: number }> | null = null;

const getLatestPositionDeduplicated = (): Promise<{ latitude: number; longitude: number }> => {
  if (!latestPositionPromise) {
    latestPositionPromise = apiClient.get<{ latitude: number; longitude: number }>('/telemetry/position/latest')
      .catch(err => {
        // Clear promise on error so next attempt can retry
        latestPositionPromise = null;
        throw err;
      });
    
    // Cache for 2 seconds to cover concurrent mounts
    setTimeout(() => {
      latestPositionPromise = null;
    }, 2000);
  }
  return latestPositionPromise;
};


interface CoordinatePickerProps {
  label?: string;
  value?: number; // Decimal degrees
  onChange: (decimalDegrees: number) => void;
  type: 'latitude' | 'longitude';
}

/**
 * CoordinatePicker - Maritime coordinate input component
 * Supports DD°MM.mm' format (Degrees Minutes with decimal)
 * Latitude: -90 to +90, Longitude: -180 to +180
 */
export const CoordinatePicker: React.FC<CoordinatePickerProps> = ({ 
  label, 
  value = 0, 
  onChange, 
  type 
}) => {
  const [degrees, setDegrees] = useState<number>(0);
  const [minutes, setMinutes] = useState<number>(0);
  const [hemisphere, setHemisphere] = useState<string>(type === 'latitude' ? 'N' : 'E');

  // Convert decimal degrees to DMS on mount/value change
  useEffect(() => {
    if (value !== undefined && value !== null) {
      const absValue = Math.abs(value);
      const deg = Math.floor(absValue);
      const min = (absValue - deg) * 60;
      
      setDegrees(deg);
      setMinutes(parseFloat(min.toFixed(2)));
      
      if (type === 'latitude') {
        setHemisphere(value >= 0 ? 'N' : 'S');
      } else {
        setHemisphere(value >= 0 ? 'E' : 'W');
      }
    }
  }, [value, type]);

  // Keep latest onChange in a ref to avoid triggering effect cleanup when it changes
  const onChangeRef = useRef(onChange);
  useEffect(() => {
    onChangeRef.current = onChange;
  }, [onChange]);

  // Auto-fill coordinates from latest telemetry on mount if value is 0 or unset
  useEffect(() => {
    if (value === 0 || value === undefined || value === null) {
      let isMounted = true;
      const autoFetch = async () => {
        try {
          const data = await getLatestPositionDeduplicated();
          if (data && isMounted) {
            const gpsVal = type === 'latitude' ? data.latitude : data.longitude;
            if (gpsVal !== undefined && gpsVal !== null && gpsVal !== 0) {
              console.log(`[CoordinatePicker] Auto-filled ${type}:`, gpsVal);
              onChangeRef.current(parseFloat(gpsVal.toFixed(6)));
            }
          }
        } catch (e) {
          console.error('Auto-fetch GPS failed in CoordinatePicker:', e);
        }
      };
      autoFetch();
      return () => {
        isMounted = false;
      };
    }
  }, [value, type]);

  // Convert DMS to decimal degrees and call onChange
  const updateDecimalDegrees = (deg: number, min: number, hemi: string) => {
    let decimal = deg + (min / 60);
    
    // Apply hemisphere sign
    if (hemi === 'S' || hemi === 'W') {
      decimal = -decimal;
    }
    
    // Validate ranges
    if (type === 'latitude' && (decimal < -90 || decimal > 90)) {
      return;
    }
    if (type === 'longitude' && (decimal < -180 || decimal > 180)) {
      return;
    }
    
    onChange(parseFloat(decimal.toFixed(6)));
  };

  const handleDegreesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = parseInt(e.target.value) || 0;
    const maxDeg = type === 'latitude' ? 90 : 180;
    
    if (val >= 0 && val <= maxDeg) {
      setDegrees(val);
      updateDecimalDegrees(val, minutes, hemisphere);
    }
  };

  const handleMinutesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = parseFloat(e.target.value) || 0;
    
    if (val >= 0 && val < 60) {
      setMinutes(val);
      updateDecimalDegrees(degrees, val, hemisphere);
    }
  };

  const handleHemisphereChange = (hemi: string) => {
    setHemisphere(hemi);
    updateDecimalDegrees(degrees, minutes, hemi);
  };

  const hemisphereOptions = type === 'latitude' ? ['N', 'S'] : ['E', 'W'];

  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label className="text-sm font-semibold text-gray-700 font-sans">
          {label}
        </label>
      )}
      <div className="flex gap-2 items-center">
        {/* Degrees */}
        <input
          type="number"
          value={degrees}
          onChange={handleDegreesChange}
          className="bg-white border border-gray-300 text-gray-900 font-sans text-base p-2.5 w-20 text-center rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 focus:outline-none"
          placeholder="00"
          min="0"
          max={type === 'latitude' ? '90' : '180'}
        />
        <span className="text-gray-500 font-sans text-xl font-bold">°</span>

        {/* Minutes */}
        <input
          type="number"
          value={minutes}
          onChange={handleMinutesChange}
          step="0.01"
          className="bg-white border border-gray-300 text-gray-900 font-sans text-base p-2.5 w-24 text-center rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 focus:outline-none"
          placeholder="00.00"
          min="0"
          max="59.99"
        />
        <span className="text-gray-500 font-sans text-xl font-bold">'</span>

        {/* Hemisphere Selector */}
        <div className="flex border border-gray-300 rounded-lg overflow-hidden">
          {hemisphereOptions.map(h => (
            <button
              key={h}
              type="button"
              onClick={() => handleHemisphereChange(h)}
              className={`
                px-4 py-2.5 font-sans font-bold text-base transition-colors
                ${hemisphere === h 
                  ? 'bg-blue-600 text-white' 
                  : 'bg-white text-gray-600 hover:bg-gray-50'
                }
              `}
            >
              {h}
            </button>
          ))}
        </div>
      </div>
      
      {/* Decimal Display */}
      <div className="text-xs font-sans text-gray-500 mt-1">
        Decimal: <span className="text-blue-600 font-semibold">{value.toFixed(6)}°</span>
      </div>
    </div>
  );
};

