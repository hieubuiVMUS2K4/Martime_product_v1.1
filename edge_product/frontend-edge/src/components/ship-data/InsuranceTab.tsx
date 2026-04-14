import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface InsuranceTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function InsuranceTab({ data, onChange }: InsuranceTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      <ContactCard title={t('shipData.insuranceTab.piClub')} prefix="piClub" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.insuranceTab.hmClub')} prefix="hmClub" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.insuranceTab.piClub')} prefix="piClub" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.insuranceTab.hmClub')} prefix="hmClub" data={data} onChange={onChange} />
    </div>
  );
}
