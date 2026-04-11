import React from 'react';
import { SectionCard, EditableField } from '../vessel/VesselDataFields';

interface Vessel {
  chartererName?: string | null;
  chartererStreet?: string | null;
  chartererCountry?: string | null;
  chartererZip?: string | null;
  chartererCity?: string | null;
  chartererPhone?: string | null;
  chartererEmail?: string | null;
  chartererContactPerson?: string | null;
  bareboatChartererName?: string | null;
  bareboatChartererEmail?: string | null;
  bareboatChartererContactPerson?: string | null;
  lastShoreSyncAt?: string | Date | null;
}

interface ChartererTabProps {
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
      📋 Shore Master — Editable
    </div>
  );
};

export function ChartererTab({ formData = {}, onChange }: ChartererTabProps) {
  const handleChange = (field: keyof Vessel, value: Vessel[keyof Vessel]) => {
    if (onChange) {
      onChange(field, value);
    }
  };

  return (
    <div className="space-y-4">
      <ShoreMasterBadge />
      
      {/* Charterer */}
      <SectionCard title="Charterer">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <EditableField label="Company Name" value={formData.chartererName}
            onChange={v => handleChange('chartererName', v)} className="col-span-2" />
          <EditableField label="Street Address" value={formData.chartererStreet}
            onChange={v => handleChange('chartererStreet', v)} className="col-span-2" />
          <EditableField label="Country" value={formData.chartererCountry}
            onChange={v => handleChange('chartererCountry', v)} />
          <EditableField label="ZIP" value={formData.chartererZip}
            onChange={v => handleChange('chartererZip', v)} />
          <EditableField label="City" value={formData.chartererCity}
            onChange={v => handleChange('chartererCity', v)} />
          <EditableField label="Phone" value={formData.chartererPhone} type="tel"
            onChange={v => handleChange('chartererPhone', v)} />
          <EditableField label="Email" value={formData.chartererEmail} type="email"
            onChange={v => handleChange('chartererEmail', v)} />
          <EditableField label="Contact Person" value={formData.chartererContactPerson}
            onChange={v => handleChange('chartererContactPerson', v)} />
        </div>
      </SectionCard>

      {/* Bareboat Charterer */}
      <SectionCard title="Bareboat Charterer">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          <EditableField label="Company Name" value={formData.bareboatChartererName}
            onChange={v => handleChange('bareboatChartererName', v)} />
          <EditableField label="Email" value={formData.bareboatChartererEmail} type="email"
            onChange={v => handleChange('bareboatChartererEmail', v)} />
          <EditableField label="Contact Person" value={formData.bareboatChartererContactPerson}
            onChange={v => handleChange('bareboatChartererContactPerson', v)} />
        </div>
      </SectionCard>
    </div>
  );
}

