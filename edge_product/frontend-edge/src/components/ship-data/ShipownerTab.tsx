import type { SaveShipData } from '@/types/ship-data.types';
import { ContactCard } from './ShipDataFields';

interface ShipownerTabProps {
  data: SaveShipData;
  onChange: (field: string, value: any) => void;
}

export function ShipownerTab({ data, onChange }: ShipownerTabProps) {
  return (
    <div className="space-y-4">
      <ContactCard title="Shipowner" prefix="shipowner" data={data} onChange={onChange} />
      <ContactCard title="Managing Owner" prefix="managingOwner" data={data} onChange={onChange} />
      <ContactCard title="Operator" prefix="operator" data={data} onChange={onChange} />
      <ContactCard title="CSO (Company Security Officer)" prefix="cso" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title="DPA (Designated Person Ashore)" prefix="dpa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title="QI (Qualified Individual) - USA" prefix="qiUsa" data={data} onChange={onChange} showPersonFields show24hPhone />
      <ContactCard title="QI (Qualified Individual) - Panama" prefix="qiPanama" data={data} onChange={onChange} showPersonFields show24hPhone />
    </div>
  );
}
