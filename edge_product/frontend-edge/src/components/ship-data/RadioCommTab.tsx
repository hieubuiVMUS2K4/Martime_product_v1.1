import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField, CheckboxField } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface RadioCommTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function RadioCommTab({ data, onChange }: RadioCommTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      {/* INMARSAT */}
      <SectionCard title={t('shipData.radioCommTab.inmarsat')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label={t('shipData.radioCommTab.telex1')} value={data.inmarsatTelex1} onChange={(v) => onChange('inmarsatTelex1', v)} />
          <FormField label={t('shipData.radioCommTab.telex2')} value={data.inmarsatTelex2} onChange={(v) => onChange('inmarsatTelex2', v)} />
          <FormField label={t('shipData.radioCommTab.phone1')} value={data.inmarsatPhone1} onChange={(v) => onChange('inmarsatPhone1', v)} />
          <FormField label={t('shipData.radioCommTab.phone2')} value={data.inmarsatPhone2} onChange={(v) => onChange('inmarsatPhone2', v)} />
          <FormField label={t('shipData.radioCommTab.fax1')} value={data.inmarsatFax1} onChange={(v) => onChange('inmarsatFax1', v)} />
          <FormField label={t('shipData.radioCommTab.fax2')} value={data.inmarsatFax2} onChange={(v) => onChange('inmarsatFax2', v)} />
          <FormField label={t('shipData.radioCommTab.emailAddress1')} value={data.emailAddress1} onChange={(v) => onChange('emailAddress1', v)} />
          <FormField label={t('shipData.radioCommTab.emailAddress2')} value={data.emailAddress2} onChange={(v) => onChange('emailAddress2', v)} />
          <FormField label={t('shipData.radioCommTab.gsmPhone')} value={data.gsmPhone} onChange={(v) => onChange('gsmPhone', v)} />
        </div>
      </SectionCard>

      {/* Sea Areas (GMDSS) */}
      <SectionCard title={t('shipData.radioCommTab.seaAreasGMDSS')}>
        <div className="flex flex-wrap gap-6">
          <CheckboxField label="A1" checked={data.seaAreaA1} onChange={(v) => onChange('seaAreaA1', v)} />
          <CheckboxField label="A2" checked={data.seaAreaA2} onChange={(v) => onChange('seaAreaA2', v)} />
          <CheckboxField label="A3" checked={data.seaAreaA3} onChange={(v) => onChange('seaAreaA3', v)} />
          <CheckboxField label="A4" checked={data.seaAreaA4} onChange={(v) => onChange('seaAreaA4', v)} />
        </div>
      </SectionCard>

      {/* Radio Equipment On Board */}
      <SectionCard title={t('shipData.radioCommTab.radioEquipmentOnBoard')}>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="py-2 px-3 text-left text-xs font-medium text-gray-500 w-48">{t('shipData.radioCommTab.equipment')}</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">HF</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">MF</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">VHF</th>
              </tr>
            </thead>
            <tbody>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">{t('shipData.radioCommTab.dsc')}</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscHF ?? false} onChange={(e) => onChange('dscHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscMF ?? false} onChange={(e) => onChange('dscMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscVHF ?? false} onChange={(e) => onChange('dscVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">{t('shipData.radioCommTab.radiotelephone')}</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneHF ?? false} onChange={(e) => onChange('radiotelephoneHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneMF ?? false} onChange={(e) => onChange('radiotelephoneMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneVHF ?? false} onChange={(e) => onChange('radiotelephoneVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">{t('shipData.radioCommTab.radiotelegraph')}</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphHF ?? false} onChange={(e) => onChange('radiotelegraphHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphMF ?? false} onChange={(e) => onChange('radiotelegraphMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphVHF ?? false} onChange={(e) => onChange('radiotelegraphVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
            </tbody>
          </table>
        </div>
        <div className="mt-4 grid grid-cols-2 md:grid-cols-4 gap-4">
          <CheckboxField label={t('shipData.radioCommTab.navtex')} checked={data.navtex} onChange={(v) => onChange('navtex', v)} />
          <CheckboxField label={t('shipData.radioCommTab.ais')} checked={data.ais} onChange={(v) => onChange('ais', v)} />
          <CheckboxField label={t('shipData.radioCommTab.sartTransponder')} checked={data.sartTransponder} onChange={(v) => onChange('sartTransponder', v)} />
          <CheckboxField label={t('shipData.radioCommTab.radiotelex')} checked={data.radiotelex} onChange={(v) => onChange('radiotelex', v)} />
        </div>
        <div className="mt-3">
          <FormField label={t('shipData.radioCommTab.otherRadioEquipment')} value={data.otherRadioEquipment} onChange={(v) => onChange('otherRadioEquipment', v)} />
        </div>
      </SectionCard>

      {/* EPIRB */}
      <SectionCard title={t('shipData.radioCommTab.epirb')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label={t('shipData.radioCommTab.epirbNumber')} value={data.epirbNumber} onChange={(v) => onChange('epirbNumber', v)} />
          <FormField label={t('shipData.radioCommTab.operatingSystem')} value={data.epirbOperatingSystem} onChange={(v) => onChange('epirbOperatingSystem', v)} />
          <FormField label={t('shipData.radioCommTab.maker')} value={data.epirbMaker} onChange={(v) => onChange('epirbMaker', v)} />
          <FormField label={t('shipData.radioCommTab.model')} value={data.epirbModel} onChange={(v) => onChange('epirbModel', v)} />
          <FormField label={t('shipData.radioCommTab.frequency')} value={data.epirbFrequency} onChange={(v) => onChange('epirbFrequency', v)} />
        </div>
      </SectionCard>
    </div>
  );
}
