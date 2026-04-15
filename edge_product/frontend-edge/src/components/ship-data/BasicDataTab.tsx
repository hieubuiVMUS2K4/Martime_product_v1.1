import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

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
  const { t } = useTranslationSafe();
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
      <SectionCard title={t('shipData.basic.identification')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.imoNumber')} value={data.imoNumber} onChange={set('imoNumber')} required placeholder="e.g. 9074729" />
          <FormField label={t('shipData.basic.officialNumber')} value={data.officialNumber} onChange={set('officialNumber')} />
          <FormField label={t('shipData.basic.callSign')} value={data.callSign} onChange={set('callSign')} placeholder="e.g. A8LU7" />
          <FormField label={t('shipData.basic.mmsiNumber')} value={data.mmsiNumber} onChange={set('mmsiNumber')} placeholder="9 digits" />
        </div>
      </SectionCard>

      {/* Ship Details */}
      <SectionCard title={t('shipData.basic.shipDetails')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.shipName')} value={data.shipName} onChange={set('shipName')} required className="col-span-2" />
          <FormField label={t('shipData.basic.flag')} value={data.flag} onChange={set('flag')} required />
          <FormField label={t('shipData.basic.portOfRegistry')} value={data.portOfRegistry} onChange={set('portOfRegistry')} required />
          <FormField label={t('shipData.basic.previousName')} value={data.previousName} onChange={set('previousName')} />
          <FormField label={t('shipData.basic.previousFlag')} value={data.previousFlag} onChange={set('previousFlag')} />
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">{t('shipData.basic.typeOfVessel')}</label>
            <select
              value={data.typeOfVessel ?? ''}
              onChange={(e) => onChange('typeOfVessel', e.target.value)}
              className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">{t('shipData.common.select')}</option>
              {VESSEL_TYPES.map(t2 => <option key={t2} value={t2}>{t2}</option>)}
            </select>
          </div>
          <FormField label={t('shipData.basic.serviceSpeed')} value={data.serviceSpeedKts} onChange={set('serviceSpeedKts')} type="number" suffix="kts" />
        </div>
      </SectionCard>

      {/* Classification */}
      <SectionCard title={t('shipData.basic.classification')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.classNotation')} value={data.classNotation} onChange={set('classNotation')} className="col-span-2" />
          <FormField label={t('shipData.basic.classRegisterNumber')} value={data.classRegisterNumber} onChange={set('classRegisterNumber')} />
        </div>
      </SectionCard>

      {/* Construction */}
      <SectionCard title={t('shipData.basic.construction')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.shipyardCountry')} value={data.shipyardCountry} onChange={set('shipyardCountry')} />
          <FormField label={t('shipData.basic.shipyardName')} value={data.shipyardName} onChange={set('shipyardName')} />
          <FormField label={t('shipData.basic.yardNo')} value={data.yardNo} onChange={set('yardNo')} />
          <FormField label={t('shipData.basic.yearBuilt')} value={data.yearBuilt} onChange={set('yearBuilt')} type="number" />
          <FormField label={t('shipData.basic.keelLaidDate')} value={data.keelLaidDate?.substring(0, 10)} onChange={set('keelLaidDate')} type="date" />
          <FormField label={t('shipData.basic.dateOfRegistry')} value={data.dateOfRegistry?.substring(0, 10)} onChange={set('dateOfRegistry')} type="date" />
        </div>
      </SectionCard>

      {/* Company & Registration */}
      <SectionCard title={t('shipData.basic.companyRegistration')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.companyImoNumber')} value={data.companyImoNumber} onChange={set('companyImoNumber')} />
          <FormField label={t('shipData.basic.ownerImoNumber')} value={data.ownerImoNumber} onChange={set('ownerImoNumber')} />
          <FormField label={t('shipData.basic.suezCanalIdNumber')} value={data.suezCanalIdNumber} onChange={set('suezCanalIdNumber')} />
          <FormField label={t('shipData.basic.panamaCanalIdNumber')} value={data.panamaCanalIdNumber} onChange={set('panamaCanalIdNumber')} />
          <FormField label={t('shipData.basic.vrpNumber')} value={data.vrpNumber} onChange={set('vrpNumber')} />
          <FormField label={t('shipData.basic.vrpType')} value={data.vrpType} onChange={set('vrpType')} />
        </div>
      </SectionCard>

      {/* Manning */}
      <SectionCard title={t('shipData.basic.manning')}>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <FormField label={t('shipData.basic.maxPersonsAllowedOB')} value={data.maxPersonsAllowedOB} onChange={set('maxPersonsAllowedOB')} type="number" />
          <FormField label={t('shipData.basic.maxPassengersAllowedOB')} value={data.maxPassengersAllowedOB} onChange={set('maxPassengersAllowedOB')} type="number" />
          <FormField label={t('shipData.basic.noOfCrewSafeManning')} value={data.noOfCrewSafeManning} onChange={set('noOfCrewSafeManning')} type="number" />
        </div>
      </SectionCard>
    </div>
  );
}
