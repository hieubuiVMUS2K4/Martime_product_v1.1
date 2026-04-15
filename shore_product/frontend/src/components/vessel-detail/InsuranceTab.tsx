import React from 'react';
import { SectionCard, EditableField } from '../vessel/VesselDataFields';

interface Vessel {
  piClubName?: string | null;
  piClubStreet?: string | null;
  piClubCountry?: string | null;
  piClubZip?: string | null;
  piClubCity?: string | null;
  piClubPhone?: string | null;
  piClubEmail?: string | null;
  piClubContactPerson?: string | null;
  hmClubName?: string | null;
  hmClubEmail?: string | null;
  hmClubContactPerson?: string | null;
  lastShoreSyncAt?: string | Date | null;
}

interface InsuranceTabProps {
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
        color: '#0b7a72',
        backgroundColor: '#e0f2fe',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      📋 Dữ liệu bờ — Có thể chỉnh sửa
    </div>
  );
};

export function InsuranceTab({ formData = {}, onChange }: InsuranceTabProps) {
  const handleChange = (field: keyof Vessel, value: Vessel[keyof Vessel]) => {
    if (onChange) {
      onChange(field, value);
    }
  };

  return (
    <div className="space-y-4">
      <ShoreMasterBadge />
      
      {/* P&I Club */}
      <SectionCard title="P&I Club">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <EditableField label="Tên hội" value={formData.piClubName}
            onChange={v => handleChange('piClubName', v)} placeholder="VD: UK P&I Club" className="col-span-2" />
          <EditableField label="Địa chỉ" value={formData.piClubStreet}
            onChange={v => handleChange('piClubStreet', v)} className="col-span-2" />
          <EditableField label="Quốc gia" value={formData.piClubCountry}
            onChange={v => handleChange('piClubCountry', v)} />
          <EditableField label="Mã bưu chính" value={formData.piClubZip}
            onChange={v => handleChange('piClubZip', v)} />
          <EditableField label="Thành phố" value={formData.piClubCity}
            onChange={v => handleChange('piClubCity', v)} />
          <EditableField label="Điện thoại" value={formData.piClubPhone} type="tel"
            onChange={v => handleChange('piClubPhone', v)} />
          <EditableField label="Email" value={formData.piClubEmail} type="email"
            onChange={v => handleChange('piClubEmail', v)} />
          <EditableField label="Người liên hệ" value={formData.piClubContactPerson}
            onChange={v => handleChange('piClubContactPerson', v)} />
        </div>
      </SectionCard>

      {/* H&M Club */}
      <SectionCard title="Hội H&M (Bảo hiểm thân tàu & máy móc)">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <EditableField label="Tên hội" value={formData.hmClubName}
            onChange={v => handleChange('hmClubName', v)} />
          <EditableField label="Email" value={formData.hmClubEmail} type="email"
            onChange={v => handleChange('hmClubEmail', v)} />
          <EditableField label="Người liên hệ" value={formData.hmClubContactPerson}
            onChange={v => handleChange('hmClubContactPerson', v)} />
        </div>
      </SectionCard>
    </div>
  );
}

