import React from 'react';
import { SectionCard, ReadOnlyField } from '../vessel/VesselDataFields';

interface Vessel {
  classSocietyName?: string | null;
  classSocietyCountry?: string | null;
  classSocietyEmail?: string | null;
  classSocietyContactPerson?: string | null;
  flagStateName?: string | null;
  flagStateCountry?: string | null;
  flagStateEmail?: string | null;
  flagStateContactPerson?: string | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface ClassFlagStateTabProps {
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

export function ClassFlagStateTab({ vessel }: ClassFlagStateTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Classification Society */}
      <SectionCard title="Classification Society">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Company Name" value={vessel.classSocietyName} />
          <ReadOnlyField label="Country" value={vessel.classSocietyCountry} />
          <ReadOnlyField label="Email" value={vessel.classSocietyEmail} />
          <ReadOnlyField label="Contact Person" value={vessel.classSocietyContactPerson} />
        </div>
      </SectionCard>

      {/* Flag State */}
      <SectionCard title="Flag State">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Authority Name" value={vessel.flagStateName} />
          <ReadOnlyField label="Country" value={vessel.flagStateCountry} />
          <ReadOnlyField label="Email" value={vessel.flagStateEmail} />
          <ReadOnlyField label="Contact Person" value={vessel.flagStateContactPerson} />
        </div>
      </SectionCard>
    </div>
  );
}
