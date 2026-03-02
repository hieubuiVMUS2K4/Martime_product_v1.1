import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField, CheckboxField } from './ShipDataFields';

interface RadioCommTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function RadioCommTab({ data, onChange }: RadioCommTabProps) {
  return (
    <div className="space-y-4">
      {/* INMARSAT */}
      <SectionCard title="INMARSAT">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label="Telex #1" value={data.inmarsatTelex1} onChange={(v) => onChange('inmarsatTelex1', v)} />
          <FormField label="Telex #2" value={data.inmarsatTelex2} onChange={(v) => onChange('inmarsatTelex2', v)} />
          <FormField label="Phone #1" value={data.inmarsatPhone1} onChange={(v) => onChange('inmarsatPhone1', v)} />
          <FormField label="Phone #2" value={data.inmarsatPhone2} onChange={(v) => onChange('inmarsatPhone2', v)} />
          <FormField label="Fax #1" value={data.inmarsatFax1} onChange={(v) => onChange('inmarsatFax1', v)} />
          <FormField label="Fax #2" value={data.inmarsatFax2} onChange={(v) => onChange('inmarsatFax2', v)} />
          <FormField label="Email Address #1" value={data.emailAddress1} onChange={(v) => onChange('emailAddress1', v)} />
          <FormField label="Email Address #2" value={data.emailAddress2} onChange={(v) => onChange('emailAddress2', v)} />
          <FormField label="GSM Phone" value={data.gsmPhone} onChange={(v) => onChange('gsmPhone', v)} />
        </div>
      </SectionCard>

      {/* Sea Areas (GMDSS) */}
      <SectionCard title="Sea Areas (GMDSS)">
        <div className="flex flex-wrap gap-6">
          <CheckboxField label="A1" checked={data.seaAreaA1} onChange={(v) => onChange('seaAreaA1', v)} />
          <CheckboxField label="A2" checked={data.seaAreaA2} onChange={(v) => onChange('seaAreaA2', v)} />
          <CheckboxField label="A3" checked={data.seaAreaA3} onChange={(v) => onChange('seaAreaA3', v)} />
          <CheckboxField label="A4" checked={data.seaAreaA4} onChange={(v) => onChange('seaAreaA4', v)} />
        </div>
      </SectionCard>

      {/* Radio Equipment On Board */}
      <SectionCard title="Radio Equipment On Board">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="py-2 px-3 text-left text-xs font-medium text-gray-500 w-48">Equipment</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">HF</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">MF</th>
                <th className="py-2 px-3 text-center text-xs font-medium text-gray-500">VHF</th>
              </tr>
            </thead>
            <tbody>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">DSC</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscHF ?? false} onChange={(e) => onChange('dscHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscMF ?? false} onChange={(e) => onChange('dscMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.dscVHF ?? false} onChange={(e) => onChange('dscVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">Radiotelephone</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneHF ?? false} onChange={(e) => onChange('radiotelephoneHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneMF ?? false} onChange={(e) => onChange('radiotelephoneMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelephoneVHF ?? false} onChange={(e) => onChange('radiotelephoneVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
              <tr className="border-b border-gray-100">
                <td className="py-2 px-3 text-sm">Radiotelegraph</td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphHF ?? false} onChange={(e) => onChange('radiotelegraphHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphMF ?? false} onChange={(e) => onChange('radiotelegraphMF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
                <td className="py-2 px-3 text-center"><input type="checkbox" checked={data.radiotelegraphVHF ?? false} onChange={(e) => onChange('radiotelegraphVHF', e.target.checked)} className="w-4 h-4 rounded border-gray-300 text-blue-600" /></td>
              </tr>
            </tbody>
          </table>
        </div>
        <div className="mt-4 grid grid-cols-2 md:grid-cols-4 gap-4">
          <CheckboxField label="Navtex" checked={data.navtex} onChange={(v) => onChange('navtex', v)} />
          <CheckboxField label="AIS" checked={data.ais} onChange={(v) => onChange('ais', v)} />
          <CheckboxField label="SART Transponder" checked={data.sartTransponder} onChange={(v) => onChange('sartTransponder', v)} />
          <CheckboxField label="Radiotelex" checked={data.radiotelex} onChange={(v) => onChange('radiotelex', v)} />
        </div>
        <div className="mt-3">
          <FormField label="Other Radio Equipment" value={data.otherRadioEquipment} onChange={(v) => onChange('otherRadioEquipment', v)} />
        </div>
      </SectionCard>

      {/* EPIRB */}
      <SectionCard title="EPIRB (Emergency Position Indicating Radio Beacon)">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label="EPIRB Number" value={data.epirbNumber} onChange={(v) => onChange('epirbNumber', v)} />
          <FormField label="Operating System" value={data.epirbOperatingSystem} onChange={(v) => onChange('epirbOperatingSystem', v)} />
          <FormField label="Maker" value={data.epirbMaker} onChange={(v) => onChange('epirbMaker', v)} />
          <FormField label="Model" value={data.epirbModel} onChange={(v) => onChange('epirbModel', v)} />
          <FormField label="Frequency" value={data.epirbFrequency} onChange={(v) => onChange('epirbFrequency', v)} />
        </div>
      </SectionCard>
    </div>
  );
}
