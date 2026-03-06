import React from 'react';
import { SectionCard, ReadOnlyField } from '../vessel/VesselDataFields';

interface Vessel {
  hfoCbm?: number | null;
  mdoCbm?: number | null;
  lubOilCbm?: number | null;
  freshWaterCbm?: number | null;
  ballastWaterCbm?: number | null;
  noOfBallastTanks?: number | null;
  teuTotal?: number | null;
  teuOnDeck?: number | null;
  teuUnderDeck?: number | null;
  grainCbm?: number | null;
  balesCbm?: number | null;
  noOfCargoHolds?: number | null;
  noOfHatches?: number | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface TanksCargoTabProps {
  vessel: Vessel;
}

// Edge sync badge component
const EdgeSyncBadge: React.FC<{ lastSync?: string | Date | null }> = ({ lastSync }) => {
  if (!lastSync) return null;
  
  const syncDate = new Date(lastSync);
  const isRecent = (Date.now() - syncDate.getTime()) < 24 * 60 * 60 * 1000;
  
  return (
    <div
      style={{
        padding: '8px 12px',
        fontSize: '13px',
        fontWeight: 500,
        color: isRecent ? '#065f46' : '#6b7280',
        backgroundColor: isRecent ? '#d1fae5' : '#f3f4f6',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      🔄 Last synced from Edge: {syncDate.toLocaleString('vi-VN')}
    </div>
  );
};

export function TanksCargoTab({ vessel }: TanksCargoTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Tanks Capacity */}
      <SectionCard title="Tanks Capacity (100%)">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="HFO" value={vessel.hfoCbm} suffix="cbm" />
          <ReadOnlyField label="MDO" value={vessel.mdoCbm} suffix="cbm" />
          <ReadOnlyField label="Lub. Oil" value={vessel.lubOilCbm} suffix="cbm" />
          <ReadOnlyField label="Fresh Water" value={vessel.freshWaterCbm} suffix="cbm" />
          <ReadOnlyField label="Ballast Water" value={vessel.ballastWaterCbm} suffix="cbm" />
          <ReadOnlyField label="No. of Ballast Tanks" value={vessel.noOfBallastTanks} />
        </div>
      </SectionCard>

      {/* Cargo Capacity */}
      <SectionCard title="Cargo Capacity">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="TEU Total" value={vessel.teuTotal} />
          <ReadOnlyField label="TEU on Deck" value={vessel.teuOnDeck} />
          <ReadOnlyField label="TEU Under Deck" value={vessel.teuUnderDeck} />
          <ReadOnlyField label="Grain" value={vessel.grainCbm} suffix="cbm" />
          <ReadOnlyField label="Bales" value={vessel.balesCbm} suffix="cbm" />
          <ReadOnlyField label="No. of Cargo Holds" value={vessel.noOfCargoHolds} />
          <ReadOnlyField label="No. of Hatches" value={vessel.noOfHatches} />
        </div>
      </SectionCard>
    </div>
  );
}
