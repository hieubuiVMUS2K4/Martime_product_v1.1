import React, { useState, useEffect, useRef, useCallback } from 'react';
import { searchPorts, type PortOption } from '../../services/port.service';

interface PortSelectProps {
  value: string;            // current portCode or portName displayed
  portCode?: string;
  onChange: (port: { portCode: string; portName: string; country?: string }) => void;
  placeholder?: string;
}

export const PortSelect: React.FC<PortSelectProps> = ({ value, onChange, placeholder }) => {
  const [query, setQuery] = useState(value || '');
  const [options, setOptions] = useState<PortOption[]>([]);
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const wrapperRef = useRef<HTMLDivElement>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout>>();

  // Sync external value
  useEffect(() => { setQuery(value || ''); }, [value]);

  // Close on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const doSearch = useCallback((text: string) => {
    clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(async () => {
      setLoading(true);
      const results = await searchPorts(text, 30);
      setOptions(results);
      setLoading(false);
    }, 250);
  }, []);

  function handleInput(e: React.ChangeEvent<HTMLInputElement>) {
    const v = e.target.value;
    setQuery(v);
    setOpen(true);
    doSearch(v);
  }

  function handleFocus() {
    setOpen(true);
    if (options.length === 0) doSearch(query);
  }

  function handleSelect(port: PortOption) {
    setQuery(`${port.portCode} - ${port.portName}`);
    setOpen(false);
    onChange({ portCode: port.portCode, portName: port.portName, country: port.country });
  }

  return (
    <div ref={wrapperRef} style={{ position: 'relative' }}>
      <input
        type="text"
        value={query}
        onChange={handleInput}
        onFocus={handleFocus}
        placeholder={placeholder || 'Tìm cảng...'}
        autoComplete="off"
      />
      {open && (
        <div style={{
          position: 'absolute', top: '100%', left: 0, right: 0,
          maxHeight: 220, overflowY: 'auto', background: '#fff',
          border: '1px solid #ccc', borderRadius: 4, zIndex: 100,
          boxShadow: '0 4px 12px rgba(0,0,0,.15)',
        }}>
          {loading && <div style={{ padding: 8, color: '#888' }}>Đang tìm...</div>}
          {!loading && options.length === 0 && <div style={{ padding: 8, color: '#888' }}>Không tìm thấy cảng</div>}
          {options.map(p => (
            <div
              key={p.id}
              style={{ padding: '6px 10px', cursor: 'pointer', borderBottom: '1px solid #eee' }}
              onMouseDown={() => handleSelect(p)}
            >
              <strong>{p.portCode}</strong> — {p.portName}
              {p.country && <span style={{ color: '#888', marginLeft: 6 }}>({p.country})</span>}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
