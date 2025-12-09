import React from 'react';

interface MaritimeInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const MaritimeInput: React.FC<MaritimeInputProps> = ({ label, error, className, ...props }) => {
  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider">
          {label}
        </label>
      )}
      <input
        className={`
          bg-industrial-surface 
          border-2 border-industrial-border 
          text-white font-mono text-lg p-4 
          focus:border-industrial-text-amber focus:outline-none 
          disabled:opacity-50 disabled:cursor-not-allowed
          ${error ? 'border-maritime-critical' : ''}
          ${className || ''}
        `}
        {...props}
      />
      {error && (
        <span className="text-maritime-critical text-xs font-mono">{error}</span>
      )}
    </div>
  );
};
