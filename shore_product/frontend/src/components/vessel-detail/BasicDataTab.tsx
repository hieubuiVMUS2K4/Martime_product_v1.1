import React from 'react';
import { SectionCard, ReadOnlyField } from '../vessel/VesselDataFields';

interface Vessel {
  id?: string;
  imo?: string | null;
  officialNumber?: string | null;
  callSign?: string | null;
  mmsiNumber?: string | null;
  name?: string;
  flag?: string | null;
  portOfRegistry?: string | null;
  previousName?: string | null;
  previousFlag?: string | null;
  vesselType?: string | null;
  serviceSpeedKts?: number | null;
  classNotation?: string | null;
  classRegisterNumber?: string | null;
  shipyardCountry?: string | null;
  shipyardName?: string | null;
  yardNo?: string | null;
  yearBuilt?: number | null;
  keelLaidDate?: string | Date | null;
  dateOfRegistry?: string | Date | null;
  companyImoNumber?: string | null;
  ownerImoNumber?: string | null;
  suezCanalIdNumber?: string | null;
  panamaCanalIdNumber?: string | null;
  vrpNumber?: string | null;
  vrpType?: string | null;
  maxPersonsAllowedOB?: number | null;
  maxPassengersAllowedOB?: number | null;
  noOfCrewSafeManning?: number | null;
  lastEdgeSyncAt?: string | Date | null;
}

interface BasicDataTabProps {
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

export function BasicDataTab({ vessel }: BasicDataTabProps) {
  return (
    <div className="space-y-4">
      <EdgeSyncBadge lastSync={vessel.lastEdgeSyncAt} />
      
      {/* Identification */}
      <SectionCard title="Identification">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="IMO Number" value={vessel.imo} />
          <ReadOnlyField label="Official Number" value={vessel.officialNumber} />
          <ReadOnlyField label="Call Sign" value={vessel.callSign} />
          <ReadOnlyField label="MMSI Number" value={vessel.mmsiNumber} />
        </div>
      </SectionCard>

      {/* Ship Details */}
      <SectionCard title="Ship Details">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Ship Name" value={vessel.name} className="col-span-2" />
          <ReadOnlyField label="Flag" value={vessel.flag} />
          <ReadOnlyField label="Port of Registry" value={vessel.portOfRegistry} />
          <ReadOnlyField label="Previous Name" value={vessel.previousName} />
          <ReadOnlyField label="Previous Flag" value={vessel.previousFlag} />
          <ReadOnlyField label="Type of Vessel" value={vessel.vesselType} />
          <ReadOnlyField label="Service Speed" value={vessel.serviceSpeedKts} suffix="kts" />
        </div>
      </SectionCard>

      {/* Classification */}
      <SectionCard title="Classification">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Class Notation" value={vessel.classNotation} className="col-span-2" />
          <ReadOnlyField label="Class Register Number" value={vessel.classRegisterNumber} />
        </div>
      </SectionCard>

      {/* Construction */}
      <SectionCard title="Construction">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Shipyard Country" value={vessel.shipyardCountry} />
          <ReadOnlyField label="Shipyard Name" value={vessel.shipyardName} />
          <ReadOnlyField label="Yard No." value={vessel.yardNo} />
          <ReadOnlyField label="Year Built" value={vessel.yearBuilt} />
          <ReadOnlyField label="Keel Laid Date" value={vessel.keelLaidDate?.toString().substring(0, 10)} />
          <ReadOnlyField label="Date of Registry" value={vessel.dateOfRegistry?.toString().substring(0, 10)} />
        </div>
      </SectionCard>

      {/* Company & Registration */}
      <SectionCard title="Company & Registration">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Company IMO Number" value={vessel.companyImoNumber} />
          <ReadOnlyField label="Owner IMO Number" value={vessel.ownerImoNumber} />
          <ReadOnlyField label="Suez Canal ID Number" value={vessel.suezCanalIdNumber} />
          <ReadOnlyField label="Panama Canal ID Number" value={vessel.panamaCanalIdNumber} />
          <ReadOnlyField label="VRP Number" value={vessel.vrpNumber} />
          <ReadOnlyField label="VRP Type" value={vessel.vrpType} />
        </div>
      </SectionCard>

      {/* Manning */}
      <SectionCard title="Manning">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <ReadOnlyField label="Max Persons Allowed O/B" value={vessel.maxPersonsAllowedOB} />
          <ReadOnlyField label="Max Passengers Allowed O/B" value={vessel.maxPassengersAllowedOB} />
          <ReadOnlyField label="No. of Crew (Safe Manning)" value={vessel.noOfCrewSafeManning} />
        </div>
      </SectionCard>
    </div>
  );
}
