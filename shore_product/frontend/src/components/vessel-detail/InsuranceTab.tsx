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
  onChange?: (field: keyof Vessel, value: any) => void;
}

// Shore Master badge component
const ShoreMasterBadge: React.FC = () => {
  return (
    <div
      style={{
        padding: '8px 12px',
        fontSize: '13px',
        fontWeight: 500,
        color: '#0369a1',
        backgroundColor: '#e0f2fe',
        borderRadius: '6px',
        width: 'fit-content',
        marginBottom: '12px'
      }}
    >
      📋 Shore Master — Editable
    </div>
  );
};

export function InsuranceTab({ vessel, formData = {}, onChange }: InsuranceTabProps) {
  const handleChange = (field: keyof Vessel, value: any) => {
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
          <EditableField label="Club Name" value={formData.piClubName}
            onChange={v => handleChange('piClubName', v)} placeholder="e.g. UK P&I Club" className="col-span-2" />
          <EditableField label="Street Address" value={formData.piClubStreet}
            onChange={v => handleChange('piClubStreet', v)} className="col-span-2" />
          <EditableField label="Country" value={formData.piClubCountry}
            onChange={v => handleChange('piClubCountry', v)} />
          <EditableField label="ZIP" value={formData.piClubZip}
            onChange={v => handleChange('piClubZip', v)} />
          <EditableField label="City" value={formData.piClubCity}
            onChange={v => handleChange('piClubCity', v)} />
          <EditableField label="Phone" value={formData.piClubPhone} type="tel"
            onChange={v => handleChange('piClubPhone', v)} />
          <EditableField label="Email" value={formData.piClubEmail} type="email"
            onChange={v => handleChange('piClubEmail', v)} />
          <EditableField label="Contact Person" value={formData.piClubContactPerson}
            onChange={v => handleChange('piClubContactPerson', v)} />
        </div>
      </SectionCard>

      {/* H&M Club */}
      <SectionCard title="H&M Club (Hull & Machinery Insurance)">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <EditableField label="Club Name" value={formData.hmClubName}
            onChange={v => handleChange('hmClubName', v)} />
          <EditableField label="Email" value={formData.hmClubEmail} type="email"
            onChange={v => handleChange('hmClubEmail', v)} />
          <EditableField label="Contact Person" value={formData.hmClubContactPerson}
            onChange={v => handleChange('hmClubContactPerson', v)} />
        </div>
      </SectionCard>
    </div>
  );
}
