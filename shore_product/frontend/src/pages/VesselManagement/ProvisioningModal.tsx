import React, { useState } from 'react';
import { X, AlertTriangle, Loader2, Download, Key, ShieldCheck, RefreshCw } from 'lucide-react';
import { ENV } from '../../config/env';

const BASE = ENV.API_BASE_URL;

interface ProvisioningModalProps {
  vesselId: string;
  vesselName: string;
  imo: string;
  onClose: () => void;
}

type ActionState = 'idle' | 'loading' | 'success' | 'error';

export const ProvisioningModal: React.FC<ProvisioningModalProps> = ({ vesselId, vesselName, imo, onClose }) => {
  const [provisionState, setProvisionState] = useState<ActionState>('idle');
  const [rotateState, setRotateState] = useState<ActionState>('idle');
  const [downloadState, setDownloadState] = useState<ActionState>('idle');
  const [message, setMessage] = useState<string | null>(null);
  const [lastNodeId, setLastNodeId] = useState<string | null>(null);
  const [lastKeyVersion, setLastKeyVersion] = useState<number | null>(null);

  const handleProvision = async () => {
    setProvisionState('loading'); setMessage(null);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/provision`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      setLastNodeId(data.nodeId);
      setLastKeyVersion(data.keyVersion);
      setMessage(data.message ?? 'Secrets đã được sinh thành công.');
      setProvisionState('success');
    } catch (err) {
      setMessage(err instanceof Error ? err.message : 'Provision thất bại');
      setProvisionState('error');
    }
  };

  const handleRotate = async () => {
    const confirmed = window.confirm(
      'Rotate key sẽ khiến tàu cần import lại cấu hình. Nếu đang bật enforce token/HMAC, sync sẽ lỗi ngay cho tới khi tàu re-import. Tiếp tục?'
    );
    if (!confirmed) return;
    setRotateState('loading'); setMessage(null);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/provision/rotate`, { method: 'POST' });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      setLastNodeId(data.nodeId);
      setLastKeyVersion(data.newKeyVersion);
      setMessage(data.message ?? 'Key mới đã được sinh.');
      setRotateState('success');
    } catch (err) {
      setMessage(err instanceof Error ? err.message : 'Rotate key thất bại');
      setRotateState('error');
    }
  };

  const handleDownload = async () => {
    setDownloadState('loading'); setMessage(null);
    try {
      const res = await fetch(`${BASE}/vessels/${vesselId}/provisioning-package`);
      if (!res.ok) throw new Error(await res.text());
      const blob = await res.blob();
      const disposition = res.headers.get('Content-Disposition') ?? '';
      const match = /filename="?([^"]+)"?/.exec(disposition);
      const fileName = match?.[1] ?? `edge-provisioning-${imo}.zip`;
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = fileName;
      document.body.appendChild(a); a.click(); a.remove();
      window.URL.revokeObjectURL(url);
      setDownloadState('success');
    } catch (err) {
      setMessage(err instanceof Error ? err.message : 'Tải Provisioning Package thất bại');
      setDownloadState('error');
    }
  };

  return (
    <div className="vp-overlay" onClick={onClose}>
      <div className="vp-modal" onClick={e => e.stopPropagation()}>
        <div className="vp-modal-head">
          <h2>Cấu hình kết nối Edge — {vesselName}</h2>
          <button className="vp-icon-btn" onClick={onClose}><X size={15} /></button>
        </div>

        <div style={{ padding: '16px 20px', display: 'flex', flexDirection: 'column', gap: 14 }}>
          {message && (
            <div className="vp-form-error" style={{ background: '#f0f9ff', color: '#0369a1', borderColor: '#bae6fd' }}>
              {message}
            </div>
          )}

          {(lastNodeId || lastKeyVersion) && (
            <div style={{ fontSize: 13, color: '#475569' }}>
              {lastNodeId && <div>Node ID: <strong>{lastNodeId}</strong></div>}
              {lastKeyVersion != null && <div>Key version: <strong>{lastKeyVersion}</strong></div>}
            </div>
          )}

          <button
            className="vp-btn vp-btn--primary"
            onClick={handleProvision}
            disabled={provisionState === 'loading'}
            style={{ justifyContent: 'flex-start' }}
          >
            {provisionState === 'loading' ? <Loader2 size={14} className="spin" /> : <ShieldCheck size={14} />}
            Generate Secrets (Provision)
          </button>

          <button
            className="vp-btn"
            onClick={handleDownload}
            disabled={downloadState === 'loading'}
            style={{ justifyContent: 'flex-start' }}
          >
            {downloadState === 'loading' ? <Loader2 size={14} className="spin" /> : <Download size={14} />}
            Tải Provisioning Package (.zip)
          </button>

          <button
            className="vp-btn vp-btn--danger"
            onClick={handleRotate}
            disabled={rotateState === 'loading'}
            style={{ justifyContent: 'flex-start' }}
          >
            {rotateState === 'loading' ? <Loader2 size={14} className="spin" /> : <Key size={14} />}
            Rotate Key
          </button>

          <div style={{ fontSize: 12, color: '#94a3b8', display: 'flex', alignItems: 'flex-start', gap: 6 }}>
            <AlertTriangle size={13} style={{ flexShrink: 0, marginTop: 1 }} />
            <span>
              Sau khi Provision hoặc Rotate, tải Provisioning Package và import vào Edge UI
              (⚙ Cấu hình kết nối bờ trên tàu), sau đó Test Connection trước khi Activate.
            </span>
          </div>
        </div>

        <div className="vp-form-footer">
          <button className="vp-btn" onClick={onClose}>
            <RefreshCw size={12} /> Đóng
          </button>
        </div>
      </div>
    </div>
  );
};
