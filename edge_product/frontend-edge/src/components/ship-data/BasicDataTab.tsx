import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField } from './ShipDataFields';

interface BasicDataTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

const VESSEL_TYPES = [
  'Bulk Carrier', 'Container Ship', 'Tanker', 'Chemical Tanker', 'LNG Carrier', 'LPG Carrier',
  'General Cargo', 'Ro-Ro', 'Car Carrier', 'Passenger Ship', 'Cruise Ship', 'Ferry',
  'Offshore Supply Vessel', 'Tug', 'Dredger', 'Fishing Vessel', 'Other',
];

export function BasicDataTab({ data, onChange }: BasicDataTabProps) {
  const set = (field: string) => (value: string) => {
    if (['yearBuilt', 'maxPersonsAllowedOB', 'maxPassengersAllowedOB', 'noOfCrewSafeManning'].includes(field)) {
      onChange(field, value ? parseInt(value) : undefined);
    } else if (['serviceSpeedKts'].includes(field)) {
      onChange(field, value ? parseFloat(value) : undefined);
    } else {
      onChange(field, value);
    }
  };

  return (
    <div className="space-y-4">
      {/* Identification */}
      <SectionCard title="Identification">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="IMO Number" value={data.imoNumber} onChange={set('imoNumber')} required placeholder="e.g. 9074729" />
          <FormField label="Official Number" value={data.officialNumber} onChange={set('officialNumber')} />
          <FormField label="Call Sign" value={data.callSign} onChange={set('callSign')} placeholder="e.g. A8LU7" />
          <FormField label="MMSI Number" value={data.mmsiNumber} onChange={set('mmsiNumber')} placeholder="9 digits" />
        </div>
      </SectionCard>

      {/* Ship Details */}
      <SectionCard title="Ship Details">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="Ship Name" value={data.shipName} onChange={set('shipName')} required className="col-span-2" />
          <FormField label="Flag" value={data.flag} onChange={set('flag')} required />
          <FormField label="Port of Registry" value={data.portOfRegistry} onChange={set('portOfRegistry')} required />
          <FormField label="Previous Name" value={data.previousName} onChange={set('previousName')} />
          <FormField label="Previous Flag" value={data.previousFlag} onChange={set('previousFlag')} />
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Type of Vessel</label>
            <select
              value={data.typeOfVessel ?? ''}
              onChange={(e) => onChange('typeOfVessel', e.target.value)}
              className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Select...</option>
              {VESSEL_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <FormField label="Service Speed" value={data.serviceSpeedKts} onChange={set('serviceSpeedKts')} type="number" suffix="kts" />
        </div>
      </SectionCard>

      {/* Classification */}
      <SectionCard title="Classification">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="Class Notation" value={data.classNotation} onChange={set('classNotation')} className="col-span-2" />
          <FormField label="Class Register Number" value={data.classRegisterNumber} onChange={set('classRegisterNumber')} />
        </div>
      </SectionCard>

      {/* Construction */}
      <SectionCard title="Construction">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="Shipyard Country" value={data.shipyardCountry} onChange={set('shipyardCountry')} />
          <FormField label="Shipyard Name" value={data.shipyardName} onChange={set('shipyardName')} />
          <FormField label="Yard No." value={data.yardNo} onChange={set('yardNo')} />
          <FormField label="Year Built" value={data.yearBuilt} onChange={set('yearBuilt')} type="number" />
          <FormField label="Keel Laid Date" value={data.keelLaidDate?.substring(0, 10)} onChange={set('keelLaidDate')} type="date" />
          <FormField label="Date of Registry" value={data.dateOfRegistry?.substring(0, 10)} onChange={set('dateOfRegistry')} type="date" />
        </div>
      </SectionCard>

      {/* Company & Registration */}
      <SectionCard title="Company & Registration">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="Company IMO Number" value={data.companyImoNumber} onChange={set('companyImoNumber')} />
          <FormField label="Owner IMO Number" value={data.ownerImoNumber} onChange={set('ownerImoNumber')} />
          <FormField label="Suez Canal ID Number" value={data.suezCanalIdNumber} onChange={set('suezCanalIdNumber')} />
          <FormField label="Panama Canal ID Number" value={data.panamaCanalIdNumber} onChange={set('panamaCanalIdNumber')} />
          <FormField label="VRP Number" value={data.vrpNumber} onChange={set('vrpNumber')} />
          <FormField label="VRP Type" value={data.vrpType} onChange={set('vrpType')} />
        </div>
      </SectionCard>

      {/* Manning */}
      <SectionCard title="Manning">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label="Max Persons Allowed O/B" value={data.maxPersonsAllowedOB} onChange={set('maxPersonsAllowedOB')} type="number" />
          <FormField label="Max Passengers Allowed O/B" value={data.maxPassengersAllowedOB} onChange={set('maxPassengersAllowedOB')} type="number" />
          <FormField label="No. of Crew (Safe Manning)" value={data.noOfCrewSafeManning} onChange={set('noOfCrewSafeManning')} type="number" />
        </div>
      </SectionCard>
    </div>
  );
}
