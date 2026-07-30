import React from 'react';
import { SectionCard, ReadOnlyField, MetricFieldReadOnly } from '../vessel/VesselDataFields';

interface Vessel {
  loa?: number | null;
  lbp?: number | null;
  breadthMoulded?: number | null;
  depthMoulded?: number | null;
  draftMoulded?: number | null;
  draftScantling?: number | null;
  draftFullBallast?: number | null;
  hMaxAirdraft?: number | null;
  lightShip?: number | null;
  blockCoefficient?: number | null;
  tpcAtSummerDraft?: number | null;
  grossTonnageInternational?: number | null;
  nettTonnageInternational?: number | null;
  grossTonnageSuezCanal?: number | null;
  grossTonnagePanamaCanal?: number | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface DimensionsTabProps {
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

export function DimensionsTab({ vessel }: DimensionsTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Principal Dimensions */}
      <SectionCard title="Kích thước chính">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricFieldReadOnly label="LOA" valueM={vessel.loa} />
          <MetricFieldReadOnly label="Chiều dài thiết kế (LBP)" valueM={vessel.lbp} />
          <MetricFieldReadOnly label="Chiều rộng theo đường nước" valueM={vessel.breadthMoulded} />
          <MetricFieldReadOnly label="Chiều cao molo" valueM={vessel.depthMoulded} />
          <MetricFieldReadOnly label="Mỹp dự kiến" valueM={vessel.draftMoulded} />
          <MetricFieldReadOnly label="Mỹp tối đa" valueM={vessel.draftScantling} />
          <MetricFieldReadOnly label="Mỹp dằn bà laỳt" valueM={vessel.draftFullBallast} />
          <MetricFieldReadOnly label="Chiều cao không cần tữ tối đa" valueM={vessel.hMaxAirdraft} />
        </div>
      </SectionCard>

      {/* Displacement & Coefficients */}
      <SectionCard title="Lượng chiếm nước & Hệ số">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Tàu không tải" value={vessel.lightShip} suffix="MT" />
          <ReadOnlyField label="Hệ số khối" value={vessel.blockCoefficient} />
          <ReadOnlyField label="TPC tại mỹp hè " value={vessel.tpcAtSummerDraft} suffix="MT/cm" />
        </div>
      </SectionCard>

      {/* Tonnage */}
      <SectionCard title="Trọng tải">
        <div className="grid grid-cols-3 gap-3">
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Quốc tế</div>
            <div className="space-y-2">
              <ReadOnlyField label="Tổng dung tích" value={vessel.grossTonnageInternational} />
              <ReadOnlyField label="Dung tích thuần" value={vessel.nettTonnageInternational} />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Kênh đào Suez</div>
            <div className="space-y-2">
              <ReadOnlyField label="Tổng dung tích" value={vessel.grossTonnageSuezCanal} />
              <ReadOnlyField label="Dung tích thuần" value={0} />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Kênh đào Panama</div>
            <div className="space-y-2">
              <ReadOnlyField label="Tổng dung tích" value={vessel.grossTonnagePanamaCanal} />
              <ReadOnlyField label="Dung tích thuần" value={0} />
            </div>
          </div>
        </div>
      </SectionCard>
    </div>
  );
}
