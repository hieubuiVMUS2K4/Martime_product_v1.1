import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';
import { useTranslationSafe } from '@/contexts/I18nContext';

interface ChartererTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ChartererTab({ data, onChange }: ChartererTabProps) {
  const { t } = useTranslationSafe();
  return (
    <div className="space-y-4">
      <ContactCard title={t('shipData.chartererTab.charterer')} prefix="charterer" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.chartererTab.bareboatCharterer')} prefix="bareboatCharterer" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.chartererTab.charterer')} prefix="charterer" data={data} onChange={onChange} />
      <ContactCard title={t('shipData.chartererTab.bareboatCharterer')} prefix="bareboatCharterer" data={data} onChange={onChange} />
    </div>
  );
}
