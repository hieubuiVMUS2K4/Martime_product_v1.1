import React, { useState, useRef, useEffect, useMemo } from 'react';
import { ChevronDown, Search, Check } from 'lucide-react';

export interface MultiSelectOption {
  id: number;
  label: string;
  /** Dòng phụ hiển thị mờ bên phải (mã chứng chỉ, bộ phận...). */
  hint?: string;
}

interface Props {
  label: string;
  options: MultiSelectOption[];
  /** Tập id đang được chọn. Rỗng = không hiện gì (giống Excel bỏ tick hết). */
  selected: Set<number>;
  onChange: (next: Set<number>) => void;
  width?: number;
}

/**
 * Bộ lọc đa chọn kiểu Excel: ô tìm kiếm, chọn tất cả / bỏ chọn tất cả, danh sách tick.
 * Mặc định trang gọi sẽ tick sẵn toàn bộ khi dữ liệu về.
 */
export const MultiSelectFilter: React.FC<Props> = ({ label, options, selected, onChange, width = 220 }) => {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const boxRef = useRef<HTMLDivElement>(null);

  // Bấm ra ngoài thì đóng. Không dùng onBlur vì click vào checkbox bên trong cũng làm mất focus.
  useEffect(() => {
    if (!open) return;
    const onDocClick = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', onDocClick);
    return () => document.removeEventListener('mousedown', onDocClick);
  }, [open]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return options;
    return options.filter(o =>
      o.label.toLowerCase().includes(q) || (o.hint ?? '').toLowerCase().includes(q)
    );
  }, [options, search]);

  const allSelected = options.length > 0 && selected.size === options.length;
  const summary = allSelected
    ? 'Tất cả'
    : selected.size === 0
      ? 'Chưa chọn'
      : `Đã chọn ${selected.size}/${options.length}`;

  const toggle = (id: number) => {
    const next = new Set(selected);
    next.has(id) ? next.delete(id) : next.add(id);
    onChange(next);
  };

  return (
    <div ref={boxRef} style={{ position: 'relative', width }}>
      <button
        onClick={() => setOpen(o => !o)}
        style={{
          width: '100%', display: 'flex', alignItems: 'center', gap: 6,
          padding: '5px 9px', fontSize: 12.5, textAlign: 'left',
          border: '1px solid #d6dee8', borderRadius: 5, background: '#fff',
          color: selected.size === 0 ? '#b91c1c' : '#16283d', cursor: 'pointer',
        }}
      >
        <span style={{ color: '#6b7c8f' }}>{label}:</span>
        <span style={{ fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{summary}</span>
        <ChevronDown size={13} style={{ marginLeft: 'auto', flexShrink: 0, color: '#8695a6' }} />
      </button>

      {open && (
        <div style={{
          position: 'absolute', top: '100%', left: 0, marginTop: 3, zIndex: 60,
          width: Math.max(width, 260), background: '#fff',
          border: '1px solid #d6dee8', borderRadius: 6, boxShadow: '0 6px 18px rgba(11,37,69,.13)',
        }}>
          <div style={{ padding: 7, borderBottom: '1px solid #eef2f7', position: 'relative' }}>
            <Search size={12} style={{ position: 'absolute', left: 15, top: '50%', transform: 'translateY(-50%)', color: '#99aab8' }} />
            <input
              autoFocus
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Tìm..."
              style={{ width: '100%', padding: '5px 8px 5px 26px', fontSize: 12, border: '1px solid #e2eaf2', borderRadius: 4, outline: 'none' }}
            />
          </div>

          <div style={{ display: 'flex', gap: 10, padding: '6px 10px', borderBottom: '1px solid #eef2f7' }}>
            <button
              onClick={() => onChange(new Set(options.map(o => o.id)))}
              style={{ fontSize: 11.5, color: '#0b2545', background: 'none', border: 'none', cursor: 'pointer', fontWeight: 600, padding: 0 }}
            >Chọn tất cả</button>
            <button
              onClick={() => onChange(new Set())}
              style={{ fontSize: 11.5, color: '#6b7c8f', background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
            >Bỏ chọn</button>
          </div>

          <div style={{ maxHeight: 260, overflowY: 'auto', padding: '3px 0' }}>
            {filtered.length === 0 ? (
              <p style={{ fontSize: 12, color: '#8695a6', textAlign: 'center', padding: 14, margin: 0 }}>Không tìm thấy</p>
            ) : filtered.map(o => {
              const on = selected.has(o.id);
              return (
                <button
                  key={o.id}
                  onClick={() => toggle(o.id)}
                  style={{
                    width: '100%', display: 'flex', alignItems: 'center', gap: 8,
                    padding: '5px 10px', fontSize: 12.5, textAlign: 'left',
                    background: on ? '#f4f8fd' : 'none', border: 'none', cursor: 'pointer',
                  }}
                >
                  <span style={{
                    width: 14, height: 14, flexShrink: 0, borderRadius: 3,
                    border: `1px solid ${on ? '#0b2545' : '#c3cedb'}`, background: on ? '#0b2545' : '#fff',
                    display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                  }}>
                    {on && <Check size={10} color="#fff" strokeWidth={3} />}
                  </span>
                  <span style={{ flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', color: '#16283d' }}>
                    {o.label}
                  </span>
                  {o.hint && (
                    <span style={{ fontFamily: 'monospace', fontSize: 10.5, color: '#8695a6', flexShrink: 0 }}>{o.hint}</span>
                  )}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};
