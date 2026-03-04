import React from 'react';
import './Input.css';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  fullWidth?: boolean;
}

export const Input: React.FC<InputProps> = ({
  label,
  error,
  fullWidth = false,
  className = '',
  ...props
}) => {
  return (
    <div className={`input-wrapper ${fullWidth ? 'input-full-width' : ''}`}>
      {label && <label className="input-label">{label}</label>}
      <input
        className={`input ${error ? 'input-error' : ''} ${className}`}
        {...props}
      />
      {error && <span className="input-error-message">{error}</span>}
    </div>
  );
};

export interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  fullWidth?: boolean;
  options: Array<{ value: string | number; label: string }>;
}

export const Select: React.FC<SelectProps> = ({
  label,
  error,
  fullWidth = false,
  options,
  className = '',
  ...props
}) => {
  return (
    <div className={`input-wrapper ${fullWidth ? 'input-full-width' : ''}`}>
      {label && <label className="input-label">{label}</label>}
      <select
        className={`input select ${error ? 'input-error' : ''} ${className}`}
        {...props}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <span className="input-error-message">{error}</span>}
    </div>
  );
};
