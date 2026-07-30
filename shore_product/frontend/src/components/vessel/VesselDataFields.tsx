import React from 'react';

// ═══════════════════════════════════════
// Section Card - Read-only wrapper for vessel data sections
// ═══════════════════════════════════════
interface SectionCardProps {
  title: string;
  children: React.ReactNode;
  className?: string;
}

export function SectionCard({ title, children, className = '' }: SectionCardProps) {
  return (
    <div className={className} style={{ background: '#fff', border: '1px solid #d6dee8', borderRadius: 6, overflow: 'hidden', boxShadow: '0 1px 4px rgba(0,60,120,0.05)' }}>
      <div style={{ background: '#dce9f8', borderBottom: '1px solid #a9bdd6', padding: '7px 14px' }}>
        <h3 style={{ fontSize: 11.5, fontWeight: 700, color: '#0b2545', textTransform: 'uppercase', letterSpacing: '0.04em', margin: 0 }}>{title}</h3>
      </div>
      <div style={{ padding: '14px' }}>{children}</div>
    </div>
  );
}

// ═══════════════════════════════════════
// Read-only Field - Standard display field
// ═══════════════════════════════════════
interface ReadOnlyFieldProps {
  label: string;
  value: string | number | undefined | null;
  suffix?: string;
  className?: string;
}

export function ReadOnlyField({ label, value, suffix, className = '' }: ReadOnlyFieldProps) {
  const displayValue = value != null ? value.toString() : '-';
  return (
    <div className={className}>
      <label style={{ display: 'block', fontSize: 11, fontWeight: 600, color: '#0b2545', marginBottom: 3, textTransform: 'uppercase', letterSpacing: '0.03em' }}>{label}</label>
      <div style={{ position: 'relative' }}>
        <input
          type="text"
          value={displayValue}
          disabled
          style={{ width: '100%', padding: '4px 8px', fontSize: 12.5, color: '#14202e', background: '#f9fbfd', border: '1px solid #a9bdd6', borderRadius: 3, boxSizing: 'border-box', cursor: 'default' }}
        />
        {suffix && (
          <span style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)', fontSize: 11, color: '#6b7c8f' }}>{suffix}</span>
        )}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// Metric Field - m with ft'in" display
// ═══════════════════════════════════════
interface MetricFieldReadOnlyProps {
  label: string;
  valueM: number | undefined | null;
  suffix?: string;
  className?: string;
}

const M_TO_FT = 3.28084;

export function MetricFieldReadOnly({ label, valueM, suffix = 'm', className = '' }: MetricFieldReadOnlyProps) {
  const ftIn = (() => {
    if (valueM == null) return '-';
    const totalInches = valueM * M_TO_FT * 12;
    const feet = Math.floor(totalInches / 12);
    const inches = Math.round(totalInches % 12);
    return `${feet}' ${inches}"`;
  })();

  const meterDisplay = valueM != null ? valueM.toFixed(2) : '-';

  return (
    <div className={className}>
      <label style={{ display: 'block', fontSize: 11, fontWeight: 600, color: '#0b2545', marginBottom: 3, textTransform: 'uppercase', letterSpacing: '0.03em' }}>{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="text"
            value={meterDisplay}
            disabled
            style={{ width: '100%', padding: '4px 8px', fontSize: 12.5, color: '#14202e', background: '#f9fbfd', border: '1px solid #a9bdd6', borderRadius: 3, boxSizing: 'border-box' }}
          />
          <span style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)', fontSize: 11, color: '#6b7c8f' }}>{suffix}</span>
        </div>
        <div style={{ width: 80 }}>
          <input
            type="text"
            value={ftIn}
            disabled
            style={{ width: '100%', padding: '4px 8px', fontSize: 11.5, color: '#6b7c8f', background: '#f9fbfd', border: '1px solid #a9bdd6', borderRadius: 3, textAlign: 'center', boxSizing: 'border-box' }}
          />
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// Power Field - kW with HP display
// ═══════════════════════════════════════
interface PowerFieldReadOnlyProps {
  label: string;
  valueKW: number | undefined | null;
  className?: string;
}

const KW_TO_HP = 1.34102;

export function PowerFieldReadOnly({ label, valueKW, className = '' }: PowerFieldReadOnlyProps) {
  const kwDisplay = valueKW != null ? valueKW.toFixed(2) : '-';
  const hpDisplay = valueKW != null ? (valueKW * KW_TO_HP).toFixed(2) : '-';

  return (
    <div className={className}>
      <label style={{ display: 'block', fontSize: 11, fontWeight: 600, color: '#0b2545', marginBottom: 3, textTransform: 'uppercase', letterSpacing: '0.03em' }}>{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="text"
            value={kwDisplay}
            disabled
            style={{ width: '100%', padding: '4px 8px', fontSize: 12.5, color: '#14202e', background: '#f9fbfd', border: '1px solid #a9bdd6', borderRadius: 3, boxSizing: 'border-box' }}
          />
          <span style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)', fontSize: 11, color: '#6b7c8f' }}>kW</span>
        </div>
        <div className="flex-1 relative">
          <input
            type="text"
            value={hpDisplay}
            disabled
            style={{ width: '100%', padding: '4px 8px', fontSize: 12.5, color: '#6b7c8f', background: '#f9fbfd', border: '1px solid #a9bdd6', borderRadius: 3, boxSizing: 'border-box' }}
          />
          <span style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)', fontSize: 11, color: '#6b7c8f' }}>HP</span>
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// Checkbox Display - Read-only checkbox
// ═══════════════════════════════════════
interface CheckboxDisplayProps {
  label: string;
  checked: boolean | undefined | null;
  className?: string;
}

export function CheckboxDisplay({ label, checked, className = '' }: CheckboxDisplayProps) {
  return (
    <div className={className}>
      <label className="flex items-center gap-2 text-sm text-gray-700">
        <input
          type="checkbox"
          checked={checked ?? false}
          disabled
          className="w-4 h-4 rounded border-gray-300"
        />
        <span>{label}</span>
      </label>
    </div>
  );
}

// ═══════════════════════════════════════
// Editable Field - For Shore master fields
// ═══════════════════════════════════════
interface EditableFieldProps {
  label: string;
  value: string | number | undefined | null;
  onChange: (value: string) => void;
  type?: 'text' | 'number' | 'email' | 'tel';
  placeholder?: string;
  className?: string;
}

export function EditableField({
  label, value, onChange, type = 'text', placeholder, className = ''
}: EditableFieldProps) {
  return (
    <div className={className}>
      <label style={{ display: 'block', fontSize: 11, fontWeight: 600, color: '#0b2545', marginBottom: 3, textTransform: 'uppercase', letterSpacing: '0.03em' }}>{label}</label>
      <input
        type={type}
        value={value ?? ''}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        style={{ width: '100%', padding: '4px 8px', fontSize: 12.5, color: '#14202e', background: '#fff', border: '1px solid #a9bdd6', borderRadius: 3, boxSizing: 'border-box', outline: 'none' }}
        onFocus={e => e.currentTarget.style.borderColor = '#1b4c7e'}
        onBlur={e => e.currentTarget.style.borderColor = '#a9bdd6'}
      />
    </div>
  );
}


