import React, { useState, useEffect } from 'react';

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
        <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider">
          {label}
        </label>
      )}
      <div className="flex gap-2 items-center">
        {/* Degrees */}
        <input
          type="number"
          value={degrees}
          onChange={handleDegreesChange}
          className="bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-3 w-20 text-center focus:border-industrial-text-amber focus:outline-none"
          placeholder="00"
          min="0"
          max={type === 'latitude' ? '90' : '180'}
        />
        <span className="text-industrial-text-amber font-mono text-2xl">°</span>

        {/* Minutes */}
        <input
          type="number"
          value={minutes}
          onChange={handleMinutesChange}
          step="0.01"
          className="bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-3 w-24 text-center focus:border-industrial-text-amber focus:outline-none"
          placeholder="00.00"
          min="0"
          max="59.99"
        />
        <span className="text-industrial-text-amber font-mono text-2xl">'</span>

        {/* Hemisphere Selector */}
        <div className="flex border-2 border-industrial-border">
          {hemisphereOptions.map(h => (
            <button
              key={h}
              onClick={() => handleHemisphereChange(h)}
              className={`
                px-4 py-3 font-mono font-bold text-lg
                ${hemisphere === h 
                  ? 'bg-industrial-text-amber text-black' 
                  : 'bg-industrial-surface text-gray-500 hover:bg-white/5'
                }
              `}
            >
              {h}
            </button>
          ))}
        </div>
      </div>
      
      {/* Decimal Display */}
      <div className="text-xs font-mono text-gray-500 mt-1">
        Decimal: <span className="text-industrial-text-green">{value.toFixed(6)}°</span>
      </div>
    </div>
  );
};
