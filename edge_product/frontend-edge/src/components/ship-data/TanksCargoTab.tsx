import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField, CbmField } from './ShipDataFields';

interface TanksCargoTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function TanksCargoTab({ data, onChange }: TanksCargoTabProps) {
  const numSet = (field: string) => (value: number | undefined) => onChange(field, value);
  const intSet = (field: string) => (value: string) => onChange(field, value ? parseInt(value) : undefined);

  return (
    <div className="space-y-4">
      {/* Tank Capacities */}
      <SectionCard title="Tank Capacities">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <CbmField label="HFO" valueCbm={data.hfoCbm} onChange={numSet('hfoCbm')} />
          <CbmField label="MDO" valueCbm={data.mdoCbm} onChange={numSet('mdoCbm')} />
          <CbmField label="Lub Oil" valueCbm={data.lubOilCbm} onChange={numSet('lubOilCbm')} />
          <CbmField label="Sludge" valueCbm={data.sludgeCbm} onChange={numSet('sludgeCbm')} />
          <CbmField label="Bilge Water" valueCbm={data.bilgeWaterCbm} onChange={numSet('bilgeWaterCbm')} />
          <CbmField label="Sewage" valueCbm={data.sewageCbm} onChange={numSet('sewageCbm')} />
          <CbmField label="Fresh Water" valueCbm={data.freshWaterCbm} onChange={numSet('freshWaterCbm')} />
          <CbmField label="Ballast Water" valueCbm={data.ballastWaterCbm} onChange={numSet('ballastWaterCbm')} />
        </div>
        <div className="mt-3">
          <FormField label="No. of Ballast Tanks" value={data.noOfBallastTanks} onChange={intSet('noOfBallastTanks')} type="number" className="w-48" />
        </div>
      </SectionCard>

      {/* Cargo Spaces */}
      <SectionCard title="Cargo Spaces">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label="TEU Total" value={data.teuTotal} onChange={intSet('teuTotal')} type="number" />
          <FormField label="TEU (On Deck)" value={data.teuOnDeck} onChange={intSet('teuOnDeck')} type="number" />
          <FormField label="TEU (Under Deck)" value={data.teuUnderDeck} onChange={intSet('teuUnderDeck')} type="number" />
          <CbmField label="Grain" valueCbm={data.grainCbm} onChange={numSet('grainCbm')} />
          <CbmField label="Bales" valueCbm={data.balesCbm} onChange={numSet('balesCbm')} />
          <FormField label="No. of Cargo Holds" value={data.noOfCargoHolds} onChange={intSet('noOfCargoHolds')} type="number" />
          <FormField label="No. of Hatches" value={data.noOfHatches} onChange={intSet('noOfHatches')} type="number" />
        </div>
      </SectionCard>
    </div>
  );
}
