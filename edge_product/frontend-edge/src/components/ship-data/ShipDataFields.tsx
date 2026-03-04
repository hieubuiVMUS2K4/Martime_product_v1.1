import type React from 'react';

// ═══════════════════════════════════════
// Section Card - Wrapper for form sections
// ═══════════════════════════════════════
interface SectionCardProps {
  title: string;
  children: React.ReactNode;
  className?: string;
  headerAction?: React.ReactNode;
}

export function SectionCard({ title, children, className = '', headerAction }: SectionCardProps) {
  return (
    <div className={`bg-white border border-gray-200 rounded-lg ${className}`}>
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 bg-gray-50 rounded-t-lg">
        <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">{title}</h3>
        {headerAction}
      </div>
      <div className="p-4">{children}</div>
    </div>
  );
}

// ═══════════════════════════════════════
// Form Field - Standard text/number input
// ═══════════════════════════════════════
interface FormFieldProps {
  label: string;
  value: string | number | undefined | null;
  onChange: (value: string) => void;
  type?: 'text' | 'number' | 'date';
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  className?: string;
  suffix?: string;
}

export function FormField({
  label, value, onChange, type = 'text', placeholder, required, disabled, className = '', suffix
}: FormFieldProps) {
  return (
    <div className={className}>
      <label className="block text-xs font-medium text-gray-600 mb-1">
        {label}{required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      <div className="relative">
        <input
          type={type}
          value={value ?? ''}
          onChange={(e) => onChange(e.target.value)}
          placeholder={placeholder}
          required={required}
          disabled={disabled}
          className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100 disabled:text-gray-500"
        />
        {suffix && (
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">{suffix}</span>
        )}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// Power Field - kW input with HP auto-calc
// ═══════════════════════════════════════
interface PowerFieldProps {
  label: string;
  valueKW: number | undefined | null;
  onChange: (kw: number | undefined) => void;
  disabled?: boolean;
  className?: string;
}

const KW_TO_HP = 1.34102;

export function PowerField({ label, valueKW, onChange, disabled, className = '' }: PowerFieldProps) {
  const hpValue = valueKW != null ? Math.round(valueKW * KW_TO_HP * 100) / 100 : '';
  return (
    <div className={className}>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="number"
            value={valueKW ?? ''}
            onChange={(e) => onChange(e.target.value ? parseFloat(e.target.value) : undefined)}
            disabled={disabled}
            className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">kW</span>
        </div>
        <div className="flex-1 relative">
          <input
            type="text"
            value={hpValue}
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
// Metric Field - m input with ft'in" auto-calc
// ═══════════════════════════════════════
interface MetricFieldProps {
  label: string;
  valueM: number | undefined | null;
  onChange: (m: number | undefined) => void;
  disabled?: boolean;
  className?: string;
  suffix?: string;
}

const M_TO_FT = 3.28084;

export function MetricField({ label, valueM, onChange, disabled, className = '', suffix = 'm' }: MetricFieldProps) {
  const ftIn = (() => {
    if (valueM == null) return '';
    const totalInches = valueM * M_TO_FT * 12;
    const feet = Math.floor(totalInches / 12);
    const inches = Math.round(totalInches % 12);
    return `${feet}' ${inches}"`;
  })();

  return (
    <div className={className}>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="number"
            step="0.01"
            value={valueM ?? ''}
            onChange={(e) => onChange(e.target.value ? parseFloat(e.target.value) : undefined)}
            disabled={disabled}
            className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">{suffix}</span>
        </div>
        <div className="w-24">
          <input
            type="text"
            value={ftIn}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-500"
          />
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// CBM Field - cbm input with cb feet auto-calc
// ═══════════════════════════════════════
interface CbmFieldProps {
  label: string;
  valueCbm: number | undefined | null;
  onChange: (cbm: number | undefined) => void;
  disabled?: boolean;
  className?: string;
}

const CBM_TO_CBFT = 35.3147;

export function CbmField({ label, valueCbm, onChange, disabled, className = '' }: CbmFieldProps) {
  const cbFt = valueCbm != null ? Math.round(valueCbm * CBM_TO_CBFT * 100) / 100 : '';
  return (
    <div className={className}>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-1">
        <div className="flex-1 relative">
          <input
            type="number"
            step="0.01"
            value={valueCbm ?? ''}
            onChange={(e) => onChange(e.target.value ? parseFloat(e.target.value) : undefined)}
            disabled={disabled}
            className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">cbm</span>
        </div>
        <div className="w-32 relative">
          <input
            type="text"
            value={cbFt}
            disabled
            className="w-full px-2 py-1.5 text-sm border border-gray-200 rounded bg-gray-50 text-gray-500"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">cb ft</span>
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════
// Checkbox Field
// ═══════════════════════════════════════
interface CheckboxFieldProps {
  label: string;
  checked: boolean | undefined;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  className?: string;
}

export function CheckboxField({ label, checked, onChange, disabled, className = '' }: CheckboxFieldProps) {
  return (
    <label className={`flex items-center gap-2 cursor-pointer ${className}`}>
      <input
        type="checkbox"
        checked={checked ?? false}
        onChange={(e) => onChange(e.target.checked)}
        disabled={disabled}
        className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
      />
      <span className="text-sm text-gray-700">{label}</span>
    </label>
  );
}

// ═══════════════════════════════════════
// Select Field
// ═══════════════════════════════════════
interface SelectFieldProps {
  label: string;
  value: string | undefined;
  onChange: (value: string) => void;
  options: { value: string; label: string }[];
  placeholder?: string;
  disabled?: boolean;
  className?: string;
}

export function SelectField({ label, value, onChange, options, placeholder, disabled, className = '' }: SelectFieldProps) {
  return (
    <div className={className}>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <select
        value={value ?? ''}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
        className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
      >
        {placeholder && <option value="">{placeholder}</option>}
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>{opt.label}</option>
        ))}
      </select>
    </div>
  );
}

// ═══════════════════════════════════════
// Contact Card - Reusable contact form for organizations  
// ═══════════════════════════════════════
interface ContactCardProps {
  title: string;
  prefix: string;
  data: Record<string, any>;
  onChange: (field: string, value: string) => void;
  showPersonFields?: boolean;       // CSO/DPA/QI person fields with Title, FirstName, LastName
  show24hPhone?: boolean;           // Phone 24h instead of regular Phone  
}

export function ContactCard({ title, prefix, data, onChange, showPersonFields = false, show24hPhone = false }: ContactCardProps) {
  const get = (field: string) => data[`${prefix}${field}`] ?? '';
  const set = (field: string) => (value: string) => onChange(`${prefix}${field}`, value);

  return (
    <SectionCard title={title}>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
        {showPersonFields ? (
          <>
            <FormField label="Title" value={get('Title')} onChange={set('Title')} />
            <FormField label="First Name" value={get('FirstName')} onChange={set('FirstName')} />
            <FormField label="Last Name" value={get('LastName')} onChange={set('LastName')} />
          </>
        ) : (
          <FormField label="Name" value={get('Name')} onChange={set('Name')} className="col-span-2 md:col-span-3" />
        )}
        <FormField label="Street" value={get('Street')} onChange={set('Street')} className="col-span-2" />
        <FormField label="Country" value={get('Country')} onChange={set('Country')} />
        <FormField label="ZIP" value={get('Zip')} onChange={set('Zip')} />
        <FormField label="City" value={get('City')} onChange={set('City')} />
        {show24hPhone ? (
          <FormField label="Phone 24h" value={get('Phone24h')} onChange={set('Phone24h')} />
        ) : (
          <FormField label="Phone" value={get('Phone')} onChange={set('Phone')} />
        )}
        <FormField label="Fax" value={get('Fax')} onChange={set('Fax')} />
        <FormField label="TLX" value={get('Tlx')} onChange={set('Tlx')} />
        <FormField label="Email" value={get('Email')} onChange={set('Email')} />
        {!showPersonFields && (
          <FormField label="Contact Person" value={get('ContactPerson')} onChange={set('ContactPerson')} />
        )}
      </div>
    </SectionCard>
  );
}

// ═══════════════════════════════════════
// Dynamic List Controls (Add / Delete)
// ═══════════════════════════════════════
interface DynamicListHeaderProps {
  onAdd: () => void;
  disabled?: boolean;
  label?: string;
}

export function DynamicListHeader({ onAdd, disabled, label = 'Add' }: DynamicListHeaderProps) {
  return (
    <button
      type="button"
      onClick={onAdd}
      disabled={disabled}
      className="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium text-blue-700 bg-blue-50 border border-blue-200 rounded hover:bg-blue-100 disabled:opacity-50"
    >
      <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
      {label}
    </button>
  );
}

interface DynamicListDeleteProps {
  onDelete: () => void;
  disabled?: boolean;
}

export function DynamicListDelete({ onDelete, disabled }: DynamicListDeleteProps) {
  return (
    <button
      type="button"
      onClick={onDelete}
      disabled={disabled}
      className="p-1 text-red-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors disabled:opacity-50"
      title="Remove"
    >
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
    </button>
  );
}
