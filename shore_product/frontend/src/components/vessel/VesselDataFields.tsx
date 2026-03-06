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
    <div className={`bg-white border border-gray-200 rounded-lg ${className}`}>
      <div className="px-4 py-3 border-b border-gray-200 bg-gray-50 rounded-t-lg">
        <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">{title}</h3>
      </div>
      <div className="p-4">{children}</div>
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
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="relative">
        <input
          type="text"
          value={displayValue}
          disabled
          className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-700"
        />
        {suffix && (
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">{suffix}</span>
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
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="text"
            value={meterDisplay}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-700"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">{suffix}</span>
        </div>
        <div className="w-24">
          <input
            type="text"
            value={ftIn}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-500 text-center"
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
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="text"
            value={kwDisplay}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-700"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">kW</span>
        </div>
        <div className="flex-1 relative">
          <input
            type="text"
            value={hpDisplay}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-500"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">HP</span>
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
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <input
        type={type}
        value={value ?? ''}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
      />
    </div>
  );
}
