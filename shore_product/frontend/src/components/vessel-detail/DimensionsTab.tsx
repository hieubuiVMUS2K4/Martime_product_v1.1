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

export function DimensionsTab({ vessel }: DimensionsTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Principal Dimensions */}
      <SectionCard title="Principal Dimensions">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <MetricFieldReadOnly label="LOA" valueM={vessel.loa} />
          <MetricFieldReadOnly label="LBP" valueM={vessel.lbp} />
          <MetricFieldReadOnly label="Breadth Moulded" valueM={vessel.breadthMoulded} />
          <MetricFieldReadOnly label="Depth Moulded" valueM={vessel.depthMoulded} />
          <MetricFieldReadOnly label="Draft Moulded" valueM={vessel.draftMoulded} />
          <MetricFieldReadOnly label="Draft Scantling" valueM={vessel.draftScantling} />
          <MetricFieldReadOnly label="Draft Full Ballast" valueM={vessel.draftFullBallast} />
          <MetricFieldReadOnly label="H Max. Airdraft" valueM={vessel.hMaxAirdraft} />
        </div>
      </SectionCard>

      {/* Displacement & Coefficients */}
      <SectionCard title="Displacement & Coefficients">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <ReadOnlyField label="Light Ship" value={vessel.lightShip} suffix="MT" />
          <ReadOnlyField label="Block Coefficient" value={vessel.blockCoefficient} />
          <ReadOnlyField label="TPC at Summer Draft" value={vessel.tpcAtSummerDraft} suffix="MT/cm" />
        </div>
      </SectionCard>

      {/* Tonnage */}
      <SectionCard title="Tonnage">
        <div className="grid grid-cols-3 gap-3">
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">International</div>
            <div className="space-y-2">
              <ReadOnlyField label="Gross Tonnage" value={vessel.grossTonnageInternational} />
              <ReadOnlyField label="Nett Tonnage" value={vessel.nettTonnageInternational} />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Suez Canal</div>
            <div className="space-y-2">
              <ReadOnlyField label="Gross Tonnage" value={vessel.grossTonnageSuezCanal} />
              <ReadOnlyField label="Nett Tonnage" value={0} />
            </div>
          </div>
          <div>
            <div className="text-xs font-medium text-gray-500 mb-2 text-center">Panama Canal</div>
            <div className="space-y-2">
              <ReadOnlyField label="Gross Tonnage" value={vessel.grossTonnagePanamaCanal} />
              <ReadOnlyField label="Nett Tonnage" value={0} />
            </div>
          </div>
        </div>
      </SectionCard>
    </div>
  );
}
