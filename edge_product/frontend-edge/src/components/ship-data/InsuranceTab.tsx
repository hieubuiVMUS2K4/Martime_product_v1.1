import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';

interface InsuranceTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function InsuranceTab({ data, onChange }: InsuranceTabProps) {
  return (
    <div className="space-y-4">
      <ContactCard title="P & I Club" prefix="piClub" data={data} onChange={onChange} />
      <ContactCard title="H & M Club (Hull & Machinery)" prefix="hmClub" data={data} onChange={onChange} />
    </div>
  );
}
