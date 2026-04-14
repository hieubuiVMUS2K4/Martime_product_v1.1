import React from 'react';
import { SectionCard, ReadOnlyField, CheckboxDisplay } from '../vessel/VesselDataFields';

interface Vessel {
  inmarsatPhone1?: string | null;
  inmarsatPhone2?: string | null;
  inmarsatFax1?: string | null;
  emailAddress1?: string | null;
  emailAddress2?: string | null;
  gsmPhone?: string | null;
  seaAreaA1?: boolean | null;
  seaAreaA2?: boolean | null;
  seaAreaA3?: boolean | null;
  seaAreaA4?: boolean | null;
  ais?: boolean | null;
  navtex?: boolean | null;
  epirbNumber?: string | null;
  epirbMaker?: string | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface RadioCommTabProps {
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
      🔄 Đồng bộ lần cuối từ Edge: {syncDate.toLocaleString('vi-VN')}
    </div>
  );
};

export function RadioCommTab({ vessel }: RadioCommTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* INMARSAT */}
      <SectionCard title="INMARSAT">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Điện thoại 1" value={vessel.inmarsatPhone1} />
          <ReadOnlyField label="Điện thoại 2" value={vessel.inmarsatPhone2} />
          <ReadOnlyField label="Fax 1" value={vessel.inmarsatFax1} />
          <ReadOnlyField label="Email 1" value={vessel.emailAddress1} />
          <ReadOnlyField label="Email 2" value={vessel.emailAddress2} />
          <ReadOnlyField label="Điện thoại GSM" value={vessel.gsmPhone} />
        </div>
      </SectionCard>

      {/* Sea Areas */}
      <SectionCard title="Vùng biển (Quy định IV/2)">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <CheckboxDisplay label="Vùng biển A1" checked={vessel.seaAreaA1} />
          <CheckboxDisplay label="Vùng biển A2" checked={vessel.seaAreaA2} />
          <CheckboxDisplay label="Vùng biển A3" checked={vessel.seaAreaA3} />
          <CheckboxDisplay label="Vùng biển A4" checked={vessel.seaAreaA4} />
        </div>
      </SectionCard>

      {/* Equipment */}
      <SectionCard title="Thiết bị">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <CheckboxDisplay label="AIS" checked={vessel.ais} />
          <CheckboxDisplay label="NAVTEX" checked={vessel.navtex} />
        </div>
      </SectionCard>

      {/* EPIRB */}
      <SectionCard title="EPIRB">
        <div className="grid grid-cols-2 md:grid-cols-2 gap-3">
          <ReadOnlyField label="Số EPIRB" value={vessel.epirbNumber} />
          <ReadOnlyField label="Hãng sản xuất" value={vessel.epirbMaker} />
        </div>
      </SectionCard>
    </div>
  );
}
