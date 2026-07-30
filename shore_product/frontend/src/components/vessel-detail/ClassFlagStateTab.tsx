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
        color: isRecent ? '#16375f' : '#6b7280',
        backgroundColor: isRecent ? '#dce9f8' : '#f3f4f6',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      🔄 Đồng bộ lần cuối từ Edge: {syncDate.toLocaleString('vi-VN')}
    </div>
  );
};

export function ClassFlagStateTab({ vessel }: ClassFlagStateTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Classification Society */}
      <SectionCard title="Tổ chức đăng kiểm">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Tên công ty" value={vessel.classSocietyName} />
          <ReadOnlyField label="Quốc gia" value={vessel.classSocietyCountry} />
          <ReadOnlyField label="Email" value={vessel.classSocietyEmail} />
          <ReadOnlyField label="Người liên hệ" value={vessel.classSocietyContactPerson} />
        </div>
      </SectionCard>

      {/* Flag State */}
      <SectionCard title="Cơ quan quản lý cờ">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Tên cơ quan" value={vessel.flagStateName} />
          <ReadOnlyField label="Quốc gia" value={vessel.flagStateCountry} />
          <ReadOnlyField label="Email" value={vessel.flagStateEmail} />
          <ReadOnlyField label="Người liên hệ" value={vessel.flagStateContactPerson} />
        </div>
      </SectionCard>
    </div>
  );
}
