import React, { useState } from 'react';

interface SignaturePadProps {
  onSign?: (signature: string) => void;
  onSave?: (signature: string) => void | Promise<void>;
  onCancel?: () => void;
  label?: string;
  disabled?: boolean;
}

export const SignaturePad: React.FC<SignaturePadProps> = ({ 
  onSign, 
  onSave,
  onCancel,
  label = "Master's Signature",
  disabled = false 
}) => {
  const [pin, setPin] = useState('');

  const handleSign = async () => {
    if (pin.length >= 4) {
      // Simulate a signature hash or token
      const signatureToken = `SIG-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      if (onSave) {
        await onSave(signatureToken);
      } else if (onSign) {
        onSign(signatureToken);
      }
      setPin('');
    }
  };

  return (
    <div className="bg-industrial-surface border border-industrial-border p-6 max-w-md">
      <h3 className="text-industrial-text-amber font-mono mb-4 uppercase">{label}</h3>
      <div className="flex flex-col gap-4">
        <div className="bg-black/50 h-32 border border-dashed border-gray-600 flex items-center justify-center">
          <span className="text-gray-500 font-mono text-sm">Digital Signature Area</span>
        </div>
        <div className="flex gap-2 items-center">
          <input 
            type="password" 
            placeholder="Enter PIN to Sign"
            className="bg-black border border-industrial-border text-white p-2 font-mono flex-1"
            value={pin}
            onChange={(e) => setPin(e.target.value)}
            disabled={disabled}
          />
          {onCancel && (
            <button 
              onClick={onCancel}
              disabled={disabled}
              className="bg-gray-600 text-white font-bold py-2 px-4 font-mono hover:bg-gray-500 disabled:opacity-50"
            >
              CANCEL
            </button>
          )}
          <button 
            onClick={handleSign}
            disabled={pin.length < 4 || disabled}
            className="bg-industrial-text-amber text-black font-bold py-2 px-4 font-mono hover:bg-yellow-500 disabled:opacity-50"
          >
            SIGN
          </button>
        </div>
      </div>
    </div>
  );
};
