import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';

interface ClassFlagStateTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ClassFlagStateTab({ data, onChange }: ClassFlagStateTabProps) {
  return (
    <div className="space-y-4">
      <ContactCard title="Classification Society" prefix="classSociety" data={data} onChange={onChange} />
      <ContactCard title="Flag State Administration" prefix="flagState" data={data} onChange={onChange} />
    </div>
  );
}
