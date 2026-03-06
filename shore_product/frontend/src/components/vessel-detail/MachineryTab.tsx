import React from 'react';
import { SectionCard, ReadOnlyField, PowerFieldReadOnly } from '../vessel/VesselDataFields';

interface Vessel {
  anchorChainPort?: number | null;
  anchorChainStarboard?: number | null;
  anchorChainStern?: number | null;
  harbourGeneratorMaker?: string | null;
  harbourGeneratorMaxPowerKW?: number | null;
  azimuthEngFwdCount?: number | null;
  azimuthEngFwdMaxPowerKW?: number | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface MachineryTabProps {
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

export function MachineryTab({ vessel }: MachineryTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Anchor Chain */}
      <SectionCard title="Anchor Chain">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Port" value={vessel.anchorChainPort} suffix="shackles" />
          <ReadOnlyField label="Starboard" value={vessel.anchorChainStarboard} suffix="shackles" />
          <ReadOnlyField label="Stern" value={vessel.anchorChainStern} suffix="shackles" />
        </div>
      </SectionCard>

      {/* Harbour / Emergency Generator */}
      <SectionCard title="Harbour / Emergency Generator">
        <div className="grid grid-cols-2 md:grid-cols-2 gap-3">
          <ReadOnlyField label="Maker" value={vessel.harbourGeneratorMaker} />
          <PowerFieldReadOnly label="Max Power" valueKW={vessel.harbourGeneratorMaxPowerKW} />
        </div>
      </SectionCard>

      {/* Azimuth Engine */}
      <SectionCard title="Azimuth Engine">
        <div className="grid grid-cols-2 md:grid-cols-2 gap-3">
          <ReadOnlyField label="No. of Azimuth Eng. FWD" value={vessel.azimuthEngFwdCount} />
          <PowerFieldReadOnly label="FWD Max Power" valueKW={vessel.azimuthEngFwdMaxPowerKW} />
        </div>
      </SectionCard>
    </div>
  );
}
