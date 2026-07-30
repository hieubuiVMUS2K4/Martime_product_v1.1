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

export function TanksCargoTab({ vessel }: TanksCargoTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Tanks Capacity */}
      <SectionCard title="Sức chứa két (100%)">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Dầu nhiên liệu HFO" value={vessel.hfoCbm} suffix="cbm" />
          <ReadOnlyField label="Dầu MDO" value={vessel.mdoCbm} suffix="cbm" />
          <ReadOnlyField label="Dầu bôi trơn" value={vessel.lubOilCbm} suffix="cbm" />
          <ReadOnlyField label="Nước ngọt" value={vessel.freshWaterCbm} suffix="cbm" />
          <ReadOnlyField label="Nước dằn bàlaỳt" value={vessel.ballastWaterCbm} suffix="cbm" />
          <ReadOnlyField label="Số két bálát" value={vessel.noOfBallastTanks} />
        </div>
      </SectionCard>

      {/* Cargo Capacity */}
      <SectionCard title="Sức chứa hàng">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Tổng TEU" value={vessel.teuTotal} />
          <ReadOnlyField label="TEU trên boong" value={vessel.teuOnDeck} />
          <ReadOnlyField label="TEU dưới boong" value={vessel.teuUnderDeck} />
          <ReadOnlyField label="Lúa mì" value={vessel.grainCbm} suffix="cbm" />
          <ReadOnlyField label="Kiện hàng" value={vessel.balesCbm} suffix="cbm" />
          <ReadOnlyField label="Số hầm hàng" value={vessel.noOfCargoHolds} />
          <ReadOnlyField label="Số nắp hầm" value={vessel.noOfHatches} />
        </div>
      </SectionCard>
    </div>
  );
}
