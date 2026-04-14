import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface ShipownerTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ShipownerTab({ data, onChange }: ShipownerTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      <ContactCard title={t('shipData.shipownerTab.shipowner')} prefix="shipowner" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.shipownerTab.managingOwner')} prefix="managingOwner" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.shipownerTab.operator')} prefix="operator" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.shipownerTab.cso')} prefix="cso" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title={t('shipData.shipownerTab.dpa')} prefix="dpa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title={t('shipData.shipownerTab.qiUsa')} prefix="qiUsa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title={t('shipData.shipownerTab.qiPanama')}fix="dpa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title={t('shipData.shipownerTab.qiUsa')} prefix="qiUsa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title={t('shipData.shipownerTab.qiPanama')} prefix="qiPanama" data={data} onChange={onChange} showPersonFields show24hPhone />
    </div>
  );
}
