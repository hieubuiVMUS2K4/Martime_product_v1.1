import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface ClassFlagStateTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ClassFlagStateTab({ data, onChange }: ClassFlagStateTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      <ContactCard title={t('shipData.classFlagStateTab.classificationSociety')} prefix="classSociety" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.classFlagStateTab.flagStateAdministration')} prefix="flagState" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.classFlagStateTab.classificationSociety')} prefix="classSociety" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.classFlagStateTab.flagStateAdministration')} prefix="flagState" data={data} onChange={onChange} />
    </div>
  );
}
