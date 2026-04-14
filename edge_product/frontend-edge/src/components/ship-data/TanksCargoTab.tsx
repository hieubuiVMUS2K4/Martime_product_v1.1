import type { SaveShipData } from '@/types/ship-data.types';
import { SectionCard, FormField, CbmField } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface TanksCargoTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function TanksCargoTab({ data, onChange }: TanksCargoTabProps) {
  const { t } = useTranslationSafe();
  const numSet = (field: string) => (value: number | undefined) => onChange(field, value);
  const intSet = (field: string) => (value: string) => onChange(field, value ? parseInt(value) : undefined);

  return (
    <div className="space-y-4">
      {/* Tank Capacities */}
      <SectionCard title={t('shipData.tanksCargoTab.tankCapacities')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <CbmField label={t('shipData.tanksCargoTab.hfo')} valueCbm={data.hfoCbm} onChange={numSet('hfoCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.mdo')} valueCbm={data.mdoCbm} onChange={numSet('mdoCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.lubOil')} valueCbm={data.lubOilCbm} onChange={numSet('lubOilCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.sludge')} valueCbm={data.sludgeCbm} onChange={numSet('sludgeCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.bilgeWater')} valueCbm={data.bilgeWaterCbm} onChange={numSet('bilgeWaterCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.sewage')} valueCbm={data.sewageCbm} onChange={numSet('sewageCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.freshWater')} valueCbm={data.freshWaterCbm} onChange={numSet('freshWaterCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.ballastWater')} valueCbm={data.ballastWaterCbm} onChange={numSet('ballastWaterCbm')} />
        </div>
        <div className="mt-3">
          <FormField label={t('shipData.tanksCargoTab.noOfBallastTanks')} value={data.noOfBallastTanks} onChange={intSet('noOfBallastTanks')} type="number" className="w-48" />
        </div>
      </SectionCard>

      {/* Cargo Spaces */}
      <SectionCard title={t('shipData.tanksCargoTab.cargoSpaces')}>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <FormField label={t('shipData.tanksCargoTab.teuTotal')} value={data.teuTotal} onChange={intSet('teuTotal')} type="number" />
          <FormField label={t('shipData.tanksCargoTab.teuOnDeck')} value={data.teuOnDeck} onChange={intSet('teuOnDeck')} type="number" />
          <FormField label={t('shipData.tanksCargoTab.teuUnderDeck')} value={data.teuUnderDeck} onChange={intSet('teuUnderDeck')} type="number" />
          <CbmField label={t('shipData.tanksCargoTab.grain')} valueCbm={data.grainCbm} onChange={numSet('grainCbm')} />
          <CbmField label={t('shipData.tanksCargoTab.bales')} valueCbm={data.balesCbm} onChange={numSet('balesCbm')} />
          <FormField label={t('shipData.tanksCargoTab.noOfCargoHolds')} value={data.noOfCargoHolds} onChange={intSet('noOfCargoHolds')} type="number" />
          <FormField label={t('shipData.tanksCargoTab.noOfHatches')} value={data.noOfHatches} onChange={intSet('noOfHatches')} type="number" />
        </div>
      </SectionCard>
    </div>
  );
}
