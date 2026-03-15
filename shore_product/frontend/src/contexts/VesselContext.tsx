import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { ENV } from '../config/env';

export interface VesselInfo {
  id: string;
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  flag: string;
  isActive: boolean;
}

interface VesselContextValue {
  vessels: VesselInfo[];
  selectedVessel: VesselInfo | null;
  /** null = "Tất cả tàu" */
  selectedVesselId: string | null;
  selectVessel: (vesselId: string | null) => void;
  isLoading: boolean;
  /** Convenience: selected IMO or null for fleet-wide */
  selectedIMO: string | null;
}

const VesselContext = createContext<VesselContextValue | undefined>(undefined);

const STORAGE_KEY = 'shore_selected_vessel';

export const VesselProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [vessels, setVessels] = useState<VesselInfo[]>([]);
  const [selectedVesselId, setSelectedVesselId] = useState<string | null>(() => {
    return localStorage.getItem(STORAGE_KEY);
  });
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchVessels = async () => {
      try {
        const res = await fetch(`${ENV.API_BASE_URL}/vessels`);
        if (!res.ok) throw new Error('Failed to fetch vessels');
        const data: VesselInfo[] = await res.json();
        setVessels(data.filter(v => v.isActive));
      } catch (err) {
        console.error('VesselContext: failed to load vessels', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchVessels();
  }, []);

  const selectVessel = useCallback((vesselId: string | null) => {
    setSelectedVesselId(vesselId);
    if (vesselId) {
      localStorage.setItem(STORAGE_KEY, vesselId);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  const selectedVessel = vessels.find(v => v.id === selectedVesselId) ?? null;
  const selectedIMO = selectedVessel?.imo ?? null;

  return (
    <VesselContext.Provider value={{
      vessels,
      selectedVessel,
      selectedVesselId,
      selectVessel,
      isLoading,
      selectedIMO,
    }}>
      {children}
    </VesselContext.Provider>
  );
};

export const useVessel = (): VesselContextValue => {
  const ctx = useContext(VesselContext);
  if (!ctx) throw new Error('useVessel must be used within VesselProvider');
  return ctx;
};
