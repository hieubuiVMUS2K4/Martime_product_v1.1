import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';

interface ChartererTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ChartererTab({ data, onChange }: ChartererTabProps) {
  return (
    <div className="space-y-4">
      <ContactCard title="Charterer" prefix="charterer" data={data} onChange={onChange} />
      <ContactCard title="Bareboat Charterer" prefix="bareboatCharterer" data={data} onChange={onChange} />
    </div>
  );
}
