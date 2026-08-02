import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { Loader2, Users, ChevronDown, ChevronRight, AlertTriangle, RefreshCw } from 'lucide-react';
import { certificateApi } from '../../../services/crew.service';
import { useToast } from '../../../components/common/Toast';
import { MultiSelectFilter } from './MultiSelectFilter';
import { AddCrewCertificateModal } from '../../CrewManagement/AddCrewCertificateModal';
import type { ComplianceMatrix, ComplianceStatus, CrewCertificate, CrewCertStatus } from '../../../types/crew.types';
import '../Crew/CrewListPage.css';
import './RankComplianceTab.css';

const DEPARTMENT_LABELS: Record<string, string> = {
  DECK: 'Boong', ENGINE: 'Máy', CATERING: 'Phục vụ',
};

/** Màu và nhãn cho từng trạng thái ô. Thiếu và hết hạn tô đỏ để đập vào mắt. */
const STATUS_STYLE: Record<ComplianceStatus, { bg: string; color: string; border: string; short: string; label: string }> = {
  VALID:         { bg: '#f0fdf4', color: '#15803d', border: '#bbf7d0', short: '✓', label: 'Còn hiệu lực' },
  EXPIRING_SOON: { bg: '#fffbeb', color: '#b45309', border: '#fde68a', short: '!', label: 'Sắp hết hạn' },
  EXPIRED:       { bg: '#fef2f2', color: '#b91c1c', border: '#fecaca', short: '✕', label: 'Hết hạn' },
  MISSING:       { bg: '#fef2f2', color: '#b91c1c', border: '#fca5a5', short: '—', label: 'Chưa có' },
};

/* Bề rộng hai cột ghim và số cột chứng chỉ tối đa hiện cùng lúc. */
const NAME_W = 210;
const GAP_W = 78;
const MAX_VISIBLE_CERTS = 10;
const MIN_CERT_W = 74;

export const RankComplianceTab: React.FC = () => {
  const toast = useToast();

  const [data, setData] = useState<ComplianceMatrix | null>(null);
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  /* Bộ lọc đa chọn — mặc định tick sẵn toàn bộ khi dữ liệu về. */
  const [selCerts, setSelCerts] = useState<Set<number>>(new Set());
  const [selRanks, setSelRanks] = useState<Set<number>>(new Set());
  const [onlyGaps, setOnlyGaps] = useState(false);
  const [searchCrew, setSearchCrew] = useState('');

  /* Bấm vào một ô của lưới để thao tác thẳng trên đúng chứng chỉ của đúng người. */
  const [modalOpen, setModalOpen] = useState(false);
  const [modalCrew, setModalCrew] = useState<{ id: string; name: string; rankId?: number } | null>(null);
  const [modalCertId, setModalCertId] = useState<number | undefined>();
  const [modalEditing, setModalEditing] = useState<CrewCertificate | null>(null);

  /**
   * Ô trống  -> thêm mới, đặt sẵn loại chứng chỉ và thuyền viên.
   * Ô đã có  -> sửa/gia hạn ngay trên bản ghi đó, không tạo bản mới.
   */
  const openCell = async (
    crew: { crewMemberId: string; crewName: string; rankId?: number },
    certificateId: number,
    crewCertificateId?: number,
  ) => {
    setModalCrew({ id: crew.crewMemberId, name: crew.crewName, rankId: crew.rankId });
    setModalCertId(certificateId);
    if (crewCertificateId) {
      try {
        setModalEditing(await certificateApi.getCrewCertificateById(crewCertificateId));
      } catch {
        toast.error('Không tải được chứng chỉ để sửa');
        return;
      }
    } else {
      setModalEditing(null);
    }
    setModalOpen(true);
  };

  /* Đo bề rộng vùng bảng để chia cột cho vừa khít. */
  const areaRef = useRef<HTMLDivElement>(null);
  const [areaWidth, setAreaWidth] = useState(0);

  useEffect(() => {
    const el = areaRef.current;
    if (!el) return;
    const ro = new ResizeObserver(entries => setAreaWidth(entries[0].contentRect.width));
    ro.observe(el);
    setAreaWidth(el.clientWidth);
    return () => ro.disconnect();
  }, [loading]);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await certificateApi.getComplianceMatrix(false);
      setData(res);
      setSelCerts(new Set(res.certificates.map(c => c.certificateId)));
      setSelRanks(new Set(res.ranks.map(r => r.rankId)));
      // Mở sẵn các chức danh đang có vấn đề để không phải bấm từng cái.
      setExpanded(new Set(res.ranks.filter(r => r.gapCount > 0).map(r => r.rankId)));
    } catch {
      toast.error('Không tải được dữ liệu tuân thủ chứng chỉ');
    } finally {
      setLoading(false);
    }
  }, [toast]);

  useEffect(() => { fetchData(); }, [fetchData]);

  /** Tra trạng thái theo (chứng chỉ, thuyền viên) — dựng một lần thay vì tìm tuyến tính mỗi ô. */
  const statusIndex = useMemo(() => {
    const map = new Map<string, CrewCertStatus>();
    data?.certificates.forEach(cert =>
      cert.crew.forEach(c => map.set(`${cert.certificateId}|${c.crewMemberId}`, c))
    );
    return map;
  }, [data]);

  /**
   * Mỗi chức danh một khối: cột là đúng bộ chứng chỉ khai báo cho chức danh đó
   * (người cùng chức danh có chung bộ yêu cầu), hàng là thuyền viên.
   */
  const blocks = useMemo(() => {
    if (!data) return [];
    const q = searchCrew.trim().toLowerCase();

    return data.ranks
      .filter(rank => selRanks.has(rank.rankId))
      .map(rank => {
        let crew = data.crew.filter(c => c.rankId === rank.rankId);
        if (q) crew = crew.filter(c =>
          c.crewName.toLowerCase().includes(q) || (c.crewCode ?? '').toLowerCase().includes(q)
        );
        if (onlyGaps) crew = crew.filter(c => c.gapCount > 0);

        const crewIds = new Set(crew.map(c => c.crewMemberId));
        const certs = data.certificates.filter(cert =>
          selCerts.has(cert.certificateId) && cert.crew.some(x => crewIds.has(x.crewMemberId))
        );

        return { rank, crew, certs };
      })
      .filter(b => b.crew.length > 0 && b.certs.length > 0);
  }, [data, searchCrew, onlyGaps, selCerts, selRanks]);

  const toggle = (rankId: number) => setExpanded(prev => {
    const s = new Set(prev);
    s.has(rankId) ? s.delete(rankId) : s.add(rankId);
    return s;
  });

  /**
   * Bề rộng một cột chứng chỉ. Từ 10 loại trở xuống thì chia đều cho vừa khít khung;
   * nhiều hơn thì khoá ở bề rộng của 10 cột, phần dư đẩy ra thanh cuộn ngang.
   */
  const certColWidth = (n: number) => {
    const usable = areaWidth - NAME_W - GAP_W;
    if (usable <= 0) return MIN_CERT_W;
    return Math.max(MIN_CERT_W, usable / Math.min(n, MAX_VISIBLE_CERTS));
  };

  if (loading) {
    return <div className="cl-loading"><Loader2 size={28} className="spin" /><p>Đang tính tuân thủ chứng chỉ...</p></div>;
  }
  if (!data) return null;

  const crewWithGaps = data.crew.filter(c => c.gapCount > 0).length;

  return (
    <div className="cl-page" style={{ padding: 0, minHeight: 'auto' }}>
      <div className="cl-header">
        <div className="cl-header-left">
          <Users size={16} className="cl-header-icon" />
          <h1 className="cl-title">Tuân thủ theo chức danh</h1>
        </div>
        <div className="cl-header-right">
          <button className="cl-btn" onClick={fetchData} title="Tính lại">
            <RefreshCw size={13} /> Làm mới
          </button>
        </div>
      </div>

      <div className="cl-stats">
        <div className="cl-stat"><span className="cl-stat-val">{data.crewTotal}</span><span className="cl-stat-lbl">Thuyền viên</span></div>
        <div className="cl-stat"><span className="cl-stat-val" style={{ color: crewWithGaps ? '#b91c1c' : undefined }}>{crewWithGaps}</span><span className="cl-stat-lbl">Người đang thiếu</span></div>
        <div className="cl-stat"><span className="cl-stat-val" style={{ color: data.totalGaps ? '#b91c1c' : undefined }}>{data.totalGaps}</span><span className="cl-stat-lbl">Lượt thiếu / hết hạn</span></div>
        <div className="cl-stat"><span className="cl-stat-val">{data.ranks.filter(r => r.gapCount > 0).length}</span><span className="cl-stat-lbl">Chức danh có vấn đề</span></div>
      </div>

      {/* Bộ lọc */}
      <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', margin: '10px 0' }}>
        <div className="cl-search-wrap" style={{ minWidth: 180 }}>
          <input className="cl-cf" placeholder="Tìm thuyền viên..." value={searchCrew} onChange={e => setSearchCrew(e.target.value)} />
        </div>

        <MultiSelectFilter
          label="Chức danh"
          width={210}
          options={data.ranks.map(r => ({ id: r.rankId, label: r.rankName, hint: r.rankCode }))}
          selected={selRanks}
          onChange={setSelRanks}
        />

        <MultiSelectFilter
          label="Loại CC"
          width={240}
          options={data.certificates.map(c => ({ id: c.certificateId, label: c.certificateName, hint: c.certificateCode }))}
          selected={selCerts}
          onChange={setSelCerts}
        />

        <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12.5, color: '#4a5b6d', cursor: 'pointer' }}>
          <input type="checkbox" checked={onlyGaps} onChange={e => setOnlyGaps(e.target.checked)} />
          Chỉ người đang thiếu
        </label>

        <div style={{ marginLeft: 'auto', display: 'flex', gap: 10, alignItems: 'center' }}>
          {(Object.keys(STATUS_STYLE) as ComplianceStatus[]).map(k => (
            <span key={k} style={{ display: 'inline-flex', alignItems: 'center', gap: 4, fontSize: 11, color: '#6b7c8f' }}>
              <span style={{
                width: 14, height: 14, borderRadius: 3, display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                background: STATUS_STYLE[k].bg, color: STATUS_STYLE[k].color,
                border: `1px solid ${STATUS_STYLE[k].border}`, fontSize: 9, fontWeight: 700,
              }}>{STATUS_STYLE[k].short}</span>
              {STATUS_STYLE[k].label}
            </span>
          ))}
        </div>
      </div>

      <div ref={areaRef}>
        {blocks.length === 0 ? (
          <div className="cl-table-card">
            <div className="cl-empty" style={{ padding: 40 }}>
              <Users size={24} />
              <p>{selRanks.size === 0 || selCerts.size === 0
                ? 'Chưa chọn chức danh hoặc loại chứng chỉ nào để hiển thị'
                : 'Không có dữ liệu khớp bộ lọc'}</p>
            </div>
          </div>
        ) : blocks.map(({ rank, crew, certs }) => {
          const open = expanded.has(rank.rankId);
          const colW = certColWidth(certs.length);
          const scrolls = certs.length > MAX_VISIBLE_CERTS;

          return (
            <div key={rank.rankId} className="cl-table-card" style={{ marginBottom: 10, overflow: 'hidden' }}>
              <button
                onClick={() => toggle(rank.rankId)}
                style={{
                  width: '100%', display: 'flex', alignItems: 'center', gap: 10, padding: '10px 14px',
                  background: rank.gapCount > 0 ? '#fffaf5' : '#f7f9fc', border: 'none',
                  borderBottom: open ? '1px solid #e2eaf2' : 'none', cursor: 'pointer', textAlign: 'left',
                }}
              >
                {open ? <ChevronDown size={15} color="#6b7c8f" /> : <ChevronRight size={15} color="#6b7c8f" />}
                <span style={{ fontFamily: 'monospace', fontWeight: 700, fontSize: 12, color: '#0b2545' }}>{rank.rankCode}</span>
                <span style={{ fontWeight: 600, fontSize: 13.5, color: '#16283d' }}>{rank.rankName}</span>
                {rank.department && (
                  <span style={{ fontSize: 11, color: '#6b7c8f', background: '#eef2f7', padding: '1px 7px', borderRadius: 999 }}>
                    {DEPARTMENT_LABELS[rank.department] || rank.department}
                  </span>
                )}
                <span style={{ marginLeft: 'auto', display: 'flex', gap: 14, alignItems: 'center', fontSize: 12 }}>
                  <span style={{ color: '#6b7c8f' }}>
                    {crew.length} người · {certs.length} loại CC{scrolls ? ' (kéo ngang để xem thêm)' : ''}
                  </span>
                  {rank.gapCount > 0 ? (
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5, color: '#b91c1c', fontWeight: 600 }}>
                      <AlertTriangle size={13} />
                      {rank.crewWithGaps} người thiếu · {rank.gapCount} lượt
                    </span>
                  ) : (
                    <span style={{ color: '#15803d', fontWeight: 600 }}>Đủ chứng chỉ</span>
                  )}
                </span>
              </button>

              {open && (
                <div style={{ overflowX: scrolls ? 'auto' : 'hidden' }}>
                  <table
                    className="cl-table rct-table"
                    style={{ tableLayout: 'fixed', width: NAME_W + GAP_W + certs.length * colW }}
                  >
                    <colgroup>
                      <col style={{ width: NAME_W }} />
                      {certs.map(c => <col key={c.certificateId} style={{ width: colW }} />)}
                      <col style={{ width: GAP_W }} />
                    </colgroup>
                    <thead>
                      <tr className="cl-tr-labels">
                        <th style={{ position: 'sticky', left: 0, zIndex: 3, background: '#f7f9fc', textAlign: 'left' }}>
                          THUYỀN VIÊN
                        </th>
                        {certs.map(c => (
                          <th key={c.certificateId} style={{ textAlign: 'center' }} title={c.certificateName}>
                            <span style={{ fontFamily: 'monospace', fontSize: 11 }}>{c.certificateCode}</span>
                            {c.isMandatory && <span style={{ color: '#dc2626', marginLeft: 2 }}>*</span>}
                          </th>
                        ))}
                        <th style={{
                          position: 'sticky', right: 0, zIndex: 3, background: '#f7f9fc',
                          textAlign: 'center', borderRight: 'none', boxShadow: scrolls ? '-4px 0 6px -4px rgba(11,37,69,.18)' : undefined,
                        }}>
                          THIẾU
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {crew.map((m, idx) => {
                        const rowBg = idx % 2 === 1 ? '#fafcfe' : '#fff';
                        return (
                          <tr key={m.crewMemberId} className={`cl-tr${idx % 2 === 1 ? ' cl-tr--alt' : ''}`}>
                            <td style={{
                              position: 'sticky', left: 0, zIndex: 2, background: rowBg,
                              boxShadow: scrolls ? '4px 0 6px -4px rgba(11,37,69,.18)' : undefined,
                            }}>
                              <div style={{ fontWeight: 600, fontSize: 12.5 }}>{m.crewName}</div>
                              {m.crewCode && <div style={{ fontFamily: 'monospace', fontSize: 10.5, color: '#8695a6' }}>{m.crewCode}</div>}
                            </td>

                            {certs.map(c => {
                              const cell = statusIndex.get(`${c.certificateId}|${m.crewMemberId}`);
                              if (!cell) {
                                // Loại này không bắt buộc với người đó — để trống, không phải lỗi.
                                return <td key={c.certificateId} style={{ textAlign: 'center', color: '#d5dde6' }}>·</td>;
                              }
                              const st = STATUS_STYLE[cell.status];
                              const tip = `${c.certificateName} — ${st.label}`
                                + (cell.expiryDate ? `\nHết hạn: ${new Date(cell.expiryDate).toLocaleDateString('vi-VN')}` : '')
                                + (cell.status !== 'MISSING' && cell.daysUntilExpiry != null ? `\nCòn ${cell.daysUntilExpiry} ngày` : '');
                              const action = cell.status === 'MISSING' ? 'Bấm để thêm chứng chỉ này'
                                : cell.status === 'EXPIRED' ? 'Bấm để gia hạn'
                                : 'Bấm để sửa thông tin';
                              return (
                                <td key={c.certificateId} style={{ textAlign: 'center' }}>
                                  <button
                                    type="button"
                                    title={`${tip}
${action}`}
                                    onClick={() => openCell(m, c.certificateId, cell.crewCertificateId)}
                                    style={{
                                      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                                      minWidth: 26, height: 21, padding: '0 6px', borderRadius: 4,
                                      background: st.bg, color: st.color, border: `1px solid ${st.border}`,
                                      fontSize: 11, fontWeight: 700, cursor: 'pointer',
                                      transition: 'transform .08s, box-shadow .08s',
                                    }}
                                    onMouseEnter={e => {
                                      e.currentTarget.style.transform = 'scale(1.12)';
                                      e.currentTarget.style.boxShadow = '0 1px 5px rgba(11,37,69,.22)';
                                    }}
                                    onMouseLeave={e => {
                                      e.currentTarget.style.transform = 'none';
                                      e.currentTarget.style.boxShadow = 'none';
                                    }}
                                  >{st.short}</button>
                                </td>
                              );
                            })}

                            <td style={{
                              position: 'sticky', right: 0, zIndex: 2, background: rowBg,
                              textAlign: 'center', borderRight: 'none',
                              boxShadow: scrolls ? '-4px 0 6px -4px rgba(11,37,69,.18)' : undefined,
                            }}>
                              {m.gapCount > 0
                                ? <span style={{ color: '#b91c1c', fontWeight: 700, fontSize: 12.5 }}>{m.gapCount}</span>
                                : <span style={{ color: '#15803d', fontSize: 12.5 }}>0</span>}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {modalOpen && modalCrew && (
        <AddCrewCertificateModal
          isOpen={modalOpen}
          crewMemberId={modalCrew.id}
          crewMemberName={modalCrew.name}
          rankId={modalCrew.rankId}
          presetCertificateId={modalCertId}
          editingCertificate={modalEditing}
          onClose={() => { setModalOpen(false); setModalEditing(null); setModalCrew(null); }}
          onSave={() => { setModalOpen(false); setModalEditing(null); setModalCrew(null); fetchData(); }}
        />
      )}

      <p style={{ fontSize: 11.5, color: '#8695a6', margin: '8px 2px' }}>
        Bấm vào một ô để thao tác ngay: ô trống thì thêm chứng chỉ, ô hết hạn thì gia hạn trên
        chính bản ghi đó, ô còn hiệu lực thì sửa thông tin.
        Cột hiển thị đúng bộ chứng chỉ khai báo cho chức danh đó ở tab Danh mục. Dấu
        <span style={{ color: '#dc2626' }}> *</span> đánh dấu loại bắt buộc theo luật — là thuộc tính
        của loại chứng chỉ, không phải yêu cầu áp cho mọi chức danh.
      </p>
    </div>
  );
};
