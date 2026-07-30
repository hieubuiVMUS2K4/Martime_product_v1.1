import React from 'react';
import { SectionCard, EditableField } from '../vessel/VesselDataFields';

interface Vessel {
  shipownerName?: string | null;
  shipownerStreet?: string | null;
  shipownerCountry?: string | null;
  shipownerZip?: string | null;
  shipownerCity?: string | null;
  shipownerPhone?: string | null;
  shipownerEmail?: string | null;
  shipownerContactPerson?: string | null;
  managingOwnerName?: string | null;
  managingOwnerEmail?: string | null;
  managingOwnerContactPerson?: string | null;
  operatorName?: string | null;
  operatorEmail?: string | null;
  operatorContactPerson?: string | null;
  csoFirstName?: string | null;
  csoLastName?: string | null;
  csoEmail?: string | null;
  csoPhone24h?: string | null;
  dpaFirstName?: string | null;
  dpaLastName?: string | null;
  dpaEmail?: string | null;
  dpaPhone24h?: string | null;
  lastShoreSyncAt?: string | Date | null;
}

interface ShipownerTabProps {
  vessel: Vessel;
  formData?: Partial<Vessel>;
  onChange?: (field: keyof Vessel, value: string | Date | null | undefined) => void;
}

// Shore Master badge component
const ShoreMasterBadge: React.FC = () => {
  return (
    <div
      style={{
        padding: '8px 12px',
        fontSize: '13px',
        fontWeight: 500,
        color: '#0b2545',
        backgroundColor: '#dce9f8',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      📋 Dữ liệu bờ — Có thể chỉnh sửa
    </div>
  );
};

export function ShipownerTab({ formData = {}, onChange }: ShipownerTabProps) {
  const handleChange = (field: keyof Vessel, value: Vessel[keyof Vessel]) => {
    if (onChange) {
      onChange(field, value);
    }
  };

  return (
    <div className="space-y-4">
      <ShoreMasterBadge />
      
      {/* Shipowner */}
      <SectionCard title="Chủ tàu">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <EditableField label="Tên công ty" value={formData.shipownerName}
            onChange={v => handleChange('shipownerName', v)} className="col-span-2" />
          <EditableField label="Địa chỉ" value={formData.shipownerStreet}
            onChange={v => handleChange('shipownerStreet', v)} className="col-span-2" />
          <EditableField label="Quốc gia" value={formData.shipownerCountry}
            onChange={v => handleChange('shipownerCountry', v)} />
          <EditableField label="Mã bưu chính" value={formData.shipownerZip}
            onChange={v => handleChange('shipownerZip', v)} />
          <EditableField label="Thành phố" value={formData.shipownerCity}
            onChange={v => handleChange('shipownerCity', v)} />
          <EditableField label="Điện thoại" value={formData.shipownerPhone} type="tel"
            onChange={v => handleChange('shipownerPhone', v)} />
          <EditableField label="Email" value={formData.shipownerEmail} type="email"
            onChange={v => handleChange('shipownerEmail', v)} />
          <EditableField label="Người liên hệ" value={formData.shipownerContactPerson}
            onChange={v => handleChange('shipownerContactPerson', v)} />
        </div>
      </SectionCard>

      {/* Managing Owner */}
      <SectionCard title="Chủ tàu quản lý">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <EditableField label="Tên công ty" value={formData.managingOwnerName}
            onChange={v => handleChange('managingOwnerName', v)} />
          <EditableField label="Email" value={formData.managingOwnerEmail} type="email"
            onChange={v => handleChange('managingOwnerEmail', v)} />
          <EditableField label="Người liên hệ" value={formData.managingOwnerContactPerson}
            onChange={v => handleChange('managingOwnerContactPerson', v)} />
        </div>
      </SectionCard>

      {/* Operator */}
      <SectionCard title="Đơn vị khai thác">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <EditableField label="Tên công ty" value={formData.operatorName}
            onChange={v => handleChange('operatorName', v)} />
          <EditableField label="Email" value={formData.operatorEmail} type="email"
            onChange={v => handleChange('operatorEmail', v)} />
          <EditableField label="Người liên hệ" value={formData.operatorContactPerson}
            onChange={v => handleChange('operatorContactPerson', v)} />
        </div>
      </SectionCard>

      {/* CSO */}
      <SectionCard title="Sĩ quan an ninh công ty (CSO)">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <EditableField label="Tên" value={formData.csoFirstName}
            onChange={v => handleChange('csoFirstName', v)} />
          <EditableField label="Họ" value={formData.csoLastName}
            onChange={v => handleChange('csoLastName', v)} />
          <EditableField label="Email" value={formData.csoEmail} type="email"
            onChange={v => handleChange('csoEmail', v)} />
          <EditableField label="Điện thoại (24h)" value={formData.csoPhone24h} type="tel"
            onChange={v => handleChange('csoPhone24h', v)} />
        </div>
      </SectionCard>

      {/* DPA */}
      <SectionCard title="Người được chỉ định trên bờ (DPA)">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <EditableField label="Tên" value={formData.dpaFirstName}
            onChange={v => handleChange('dpaFirstName', v)} />
          <EditableField label="Họ" value={formData.dpaLastName}
            onChange={v => handleChange('dpaLastName', v)} />
          <EditableField label="Email" value={formData.dpaEmail} type="email"
            onChange={v => handleChange('dpaEmail', v)} />
          <EditableField label="Điện thoại (24h)" value={formData.dpaPhone24h} type="tel"
            onChange={v => handleChange('dpaPhone24h', v)} />
        </div>
      </SectionCard>
    </div>
  );
}

