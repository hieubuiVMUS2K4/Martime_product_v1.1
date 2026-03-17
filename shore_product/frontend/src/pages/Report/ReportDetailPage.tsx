import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Ship, Anchor, Waves, Fuel, MapPin, FileText, Wrench, Bell } from 'lucide-react';
import { ENV } from '../../config/env';
import './ReportDetailPage.css';

// ─────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────

interface MaintenanceSummary {
  tasksCompletedLast24h: number;
  tasksInProgress: number;
  overdueTasks: number;
  criticalTasksDueSoon: number;
  totalScheduledToday: number;
  pendingDeferrals: number;
  criticalMaintenanceNotes: string | null;
}

interface AlarmSummary {
  activeAlarms: number;
  acknowledgedAlarms: number;
  resolvedLast24h: number;
  criticalAlarms: number;
  warningAlarms: number;
  safetyNotes: string | null;
}

interface ReportDetail {
  id: string;
  reportNumber: string;
  reportTypeId: number;
  typeCode: string;
  typeName: string;
  reportDateTime: string;
  status: string;
  preparedBy: string | null;
  masterSignature: string | null;
  signedAt: string | null;
  remarks: string | null;
  isTransmitted: boolean;
  transmittedAt: string | null;
  originNode: string;
  createdAt: string;
  updatedAt: string | null;
  childReport: Record<string, unknown> | null;
}

// ─────────────────────────────────────────────────────────────
// Label maps
// ─────────────────────────────────────────────────────────────

const TYPE_META: Record<string, { label: string; icon: React.ReactNode }> = {
  NOON:      { label: 'Báo cáo Trưa',       icon: <Ship size={16} /> },
  DEPARTURE: { label: 'Báo cáo Khởi hành',  icon: <Anchor size={16} /> },
  ARRIVAL:   { label: 'Báo cáo Cập cảng',   icon: <Waves size={16} /> },
  BUNKER:    { label: 'Báo cáo Bunker',      icon: <Fuel size={16} /> },
  POSITION:  { label: 'Báo cáo Vị trí',     icon: <MapPin size={16} /> },
};

const STATUS_LABEL: Record<string, string> = {
  DRAFT: 'Nháp', SUBMITTED: 'Chờ duyệt', APPROVED: 'Đã duyệt',
  REJECTED: 'Từ chối', TRANSMITTED: 'Đã truyền',
};

// ─────────────────────────────────────────────────────────────
// Field definitions per report type
// ─────────────────────────────────────────────────────────────

interface FieldDef {
  key: string;
  label: string;
  unit?: string;
  fmt?: 'date' | 'datetime' | 'coord' | 'number';
}

const NOON_FIELDS: { section: string; fields: FieldDef[] }[] = [
  { section: 'Vị trí & Hành trình', fields: [
    { key: 'latitude', label: 'Vĩ độ', fmt: 'coord' },
    { key: 'longitude', label: 'Kinh độ', fmt: 'coord' },
    { key: 'courseOverGround', label: 'Hướng đi (COG)', unit: '°' },
    { key: 'speedOverGround', label: 'Tốc độ (SOG)', unit: 'kn' },
    { key: 'distanceTraveled', label: 'Quãng đường đã đi', unit: 'NM' },
    { key: 'distanceToGo', label: 'Quãng đường còn lại', unit: 'NM' },
    { key: 'estimatedTimeOfArrival', label: 'ETA', fmt: 'datetime' },
  ]},
  { section: 'Thời tiết', fields: [
    { key: 'weatherConditions', label: 'Điều kiện thời tiết' },
    { key: 'seaState', label: 'Trạng thái biển' },
    { key: 'windDirection', label: 'Hướng gió' },
    { key: 'windSpeed', label: 'Tốc độ gió', unit: 'kn' },
    { key: 'airTemperature', label: 'Nhiệt độ không khí', unit: '°C' },
    { key: 'seaTemperature', label: 'Nhiệt độ nước biển', unit: '°C' },
    { key: 'barometricPressure', label: 'Áp suất khí quyển', unit: 'hPa' },
    { key: 'visibility', label: 'Tầm nhìn' },
  ]},
  { section: 'Nhiên liệu tiêu thụ', fields: [
    { key: 'fuelOilConsumed', label: 'FO tiêu thụ', unit: 'MT' },
    { key: 'dieselOilConsumed', label: 'DO tiêu thụ', unit: 'MT' },
    { key: 'lubOilConsumed', label: 'LO tiêu thụ', unit: 'L' },
    { key: 'freshWaterConsumed', label: 'Nước ngọt tiêu thụ', unit: 'MT' },
  ]},
  { section: 'Nhiên liệu còn lại (ROB)', fields: [
    { key: 'fuelOilROB', label: 'FO ROB', unit: 'MT' },
    { key: 'dieselOilROB', label: 'DO ROB', unit: 'MT' },
    { key: 'lubOilROB', label: 'LO ROB', unit: 'L' },
    { key: 'freshWaterROB', label: 'Nước ngọt ROB', unit: 'MT' },
  ]},
  { section: 'Máy móc', fields: [
    { key: 'mainEngineRunningHours', label: 'Giờ chạy máy chính' },
    { key: 'mainEngineRPM', label: 'RPM máy chính' },
    { key: 'mainEnginePower', label: 'Công suất máy chính', unit: 'kW' },
    { key: 'auxEngineRunningHours', label: 'Giờ chạy máy phụ' },
  ]},
  { section: 'Hàng hóa', fields: [
    { key: 'cargoOnBoard', label: 'Hàng trên tàu', unit: 'MT' },
    { key: 'cargoDescription', label: 'Mô tả hàng hóa' },
  ]},
  { section: 'Ghi chú', fields: [
    { key: 'operationalRemarks', label: 'Ghi chú vận hành' },
    { key: 'machineryRemarks', label: 'Ghi chú máy móc' },
    { key: 'cargoRemarks', label: 'Ghi chú hàng hóa' },
    { key: 'maintenanceRemarks', label: 'Ghi chú bảo trì' },
  ]},
  { section: 'Thuyền viên & An toàn', fields: [
    { key: 'crewOnBoard', label: 'Thuyền viên' },
    { key: 'passengersOnBoard', label: 'Hành khách' },
    { key: 'certificatesExpiringSoon', label: 'Chứng chỉ sắp hết hạn (30 ngày)' },
    { key: 'safetyDrillsConducted', label: 'Diễn tập an toàn' },
    { key: 'safetyIncidents', label: 'Sự cố an toàn' },
  ]},
];

const DEPARTURE_FIELDS: { section: string; fields: FieldDef[] }[] = [
  { section: 'Thông tin cảng', fields: [
    { key: 'portName', label: 'Cảng khởi hành' },
    { key: 'portCode', label: 'UN/LOCODE' },
    { key: 'departureDateTime', label: 'Thời gian khởi hành (UTC)', fmt: 'datetime' },
    { key: 'pilotOnBoardTime', label: 'Hoa tiêu lên tàu', fmt: 'datetime' },
    { key: 'lastLineAshoreTime', label: 'Thả dây cuối', fmt: 'datetime' },
  ]},
  { section: 'Vị trí & Mớn nước', fields: [
    { key: 'departureLatitude', label: 'Vĩ độ', fmt: 'coord' },
    { key: 'departureLongitude', label: 'Kinh độ', fmt: 'coord' },
    { key: 'draftForward', label: 'Mớn nước mũi', unit: 'm' },
    { key: 'draftAft', label: 'Mớn nước lái', unit: 'm' },
    { key: 'draftMidship', label: 'Mớn nước giữa', unit: 'm' },
  ]},
  { section: 'Nhiên liệu ROB', fields: [
    { key: 'fuelOilROB', label: 'FO ROB', unit: 'MT' },
    { key: 'dieselOilROB', label: 'DO ROB', unit: 'MT' },
    { key: 'lubOilROB', label: 'LO ROB', unit: 'L' },
    { key: 'freshWaterROB', label: 'Nước ngọt ROB', unit: 'MT' },
  ]},
  { section: 'Cảng tiếp theo', fields: [
    { key: 'nextPort', label: 'Cảng tiếp theo' },
    { key: 'nextPortCode', label: 'UN/LOCODE' },
    { key: 'distanceToNextPort', label: 'Khoảng cách', unit: 'NM' },
    { key: 'estimatedTimeOfArrival', label: 'ETA', fmt: 'datetime' },
  ]},
  { section: 'Hàng hóa & Người', fields: [
    { key: 'cargoOnBoard', label: 'Hàng trên tàu', unit: 'MT' },
    { key: 'cargoDescription', label: 'Mô tả hàng hóa' },
    { key: 'crewOnBoard', label: 'Thuyền viên' },
    { key: 'passengersOnBoard', label: 'Hành khách' },
  ]},
  { section: 'Ghi chú', fields: [
    { key: 'remarks', label: 'Ghi chú' },
  ]},
];

const ARRIVAL_FIELDS: { section: string; fields: FieldDef[] }[] = [
  { section: 'Thông tin cảng', fields: [
    { key: 'portName', label: 'Cảng đến' },
    { key: 'portCode', label: 'UN/LOCODE' },
    { key: 'arrivalDateTime', label: 'Thời gian cập cảng (UTC)', fmt: 'datetime' },
    { key: 'pilotOnBoardTime', label: 'Hoa tiêu lên tàu', fmt: 'datetime' },
    { key: 'firstLineAshoreTime', label: 'Buộc dây đầu tiên', fmt: 'datetime' },
  ]},
  { section: 'Vị trí', fields: [
    { key: 'arrivalLatitude', label: 'Vĩ độ', fmt: 'coord' },
    { key: 'arrivalLongitude', label: 'Kinh độ', fmt: 'coord' },
  ]},
  { section: 'Thống kê chuyến đi', fields: [
    { key: 'voyageDistance', label: 'Quãng đường chuyến đi', unit: 'NM' },
    { key: 'voyageDuration', label: 'Thời gian chuyến đi', unit: 'giờ' },
    { key: 'averageSpeed', label: 'Tốc độ trung bình', unit: 'kn' },
  ]},
  { section: 'Mớn nước', fields: [
    { key: 'draftForward', label: 'Mớn nước mũi', unit: 'm' },
    { key: 'draftAft', label: 'Mớn nước lái', unit: 'm' },
    { key: 'draftMidship', label: 'Mớn nước giữa', unit: 'm' },
  ]},
  { section: 'Nhiên liệu', fields: [
    { key: 'totalFuelConsumed', label: 'FO tiêu thụ', unit: 'MT' },
    { key: 'totalDieselConsumed', label: 'DO tiêu thụ', unit: 'MT' },
    { key: 'fuelOilROB', label: 'FO ROB', unit: 'MT' },
    { key: 'dieselOilROB', label: 'DO ROB', unit: 'MT' },
    { key: 'lubOilROB', label: 'LO ROB', unit: 'L' },
    { key: 'freshWaterROB', label: 'Nước ngọt ROB', unit: 'MT' },
  ]},
  { section: 'Hàng hóa & Người', fields: [
    { key: 'cargoOnBoard', label: 'Hàng trên tàu', unit: 'MT' },
    { key: 'cargoDescription', label: 'Mô tả hàng hóa' },
    { key: 'crewOnBoard', label: 'Thuyền viên' },
    { key: 'passengersOnBoard', label: 'Hành khách' },
  ]},
  { section: 'Ghi chú', fields: [
    { key: 'remarks', label: 'Ghi chú' },
  ]},
];

const BUNKER_FIELDS: { section: string; fields: FieldDef[] }[] = [
  { section: 'Thông tin Bunker', fields: [
    { key: 'bunkerDate', label: 'Ngày nhận dầu', fmt: 'date' },
    { key: 'portName', label: 'Cảng' },
    { key: 'portCode', label: 'Mã cảng' },
    { key: 'supplierName', label: 'Nhà cung cấp' },
    { key: 'bdnNumber', label: 'Số BDN' },
  ]},
  { section: 'Thông tin nhiên liệu', fields: [
    { key: 'fuelType', label: 'Loại nhiên liệu' },
    { key: 'fuelGrade', label: 'Cấp nhiên liệu' },
    { key: 'quantityReceived', label: 'Số lượng nhận', unit: 'MT' },
    { key: 'density', label: 'Tỷ trọng', unit: 'kg/m³' },
    { key: 'sulphurContent', label: 'Hàm lượng lưu huỳnh', unit: '%' },
    { key: 'viscosity', label: 'Độ nhớt', unit: 'cSt' },
    { key: 'flashPoint', label: 'Điểm chớp cháy', unit: '°C' },
  ]},
  { section: 'Tồn kho', fields: [
    { key: 'roBefore', label: 'ROB trước khi nhận', unit: 'MT' },
    { key: 'robAfter', label: 'ROB sau khi nhận', unit: 'MT' },
    { key: 'tanksLoaded', label: 'Bồn chứa' },
    { key: 'sealNumbers', label: 'Số seal' },
  ]},
  { section: 'Ký tên & Ghi chú', fields: [
    { key: 'chiefEngineerSignature', label: 'Máy trưởng ký' },
    { key: 'remarks', label: 'Ghi chú' },
  ]},
];

const POSITION_FIELDS: { section: string; fields: FieldDef[] }[] = [
  { section: 'Vị trí & Hành trình', fields: [
    { key: 'reportDateTime', label: 'Thời gian', fmt: 'datetime' },
    { key: 'latitude', label: 'Vĩ độ', fmt: 'coord' },
    { key: 'longitude', label: 'Kinh độ', fmt: 'coord' },
    { key: 'courseOverGround', label: 'Hướng đi (COG)', unit: '°' },
    { key: 'speedOverGround', label: 'Tốc độ (SOG)', unit: 'kn' },
    { key: 'reportReason', label: 'Lý do báo cáo' },
  ]},
  { section: 'Thời tiết', fields: [
    { key: 'weatherConditions', label: 'Điều kiện thời tiết' },
    { key: 'seaState', label: 'Trạng thái biển' },
    { key: 'windDirection', label: 'Hướng gió' },
    { key: 'windSpeed', label: 'Tốc độ gió', unit: 'kn' },
  ]},
  { section: 'Nhiên liệu & Hành trình', fields: [
    { key: 'fuelOilROB', label: 'FO ROB', unit: 'MT' },
    { key: 'dieselOilROB', label: 'DO ROB', unit: 'MT' },
    { key: 'nextPort', label: 'Cảng tiếp theo' },
    { key: 'eta', label: 'ETA', fmt: 'datetime' },
    { key: 'distanceToGo', label: 'Quãng đường còn lại', unit: 'NM' },
    { key: 'lastPort', label: 'Cảng trước' },
    { key: 'cargoOnBoard', label: 'Hàng trên tàu', unit: 'MT' },
    { key: 'crewOnBoard', label: 'Thuyền viên' },
  ]},
  { section: 'Ghi chú', fields: [
    { key: 'remarks', label: 'Ghi chú' },
  ]},
];

const FIELD_MAP: Record<string, { section: string; fields: FieldDef[] }[]> = {
  NOON: NOON_FIELDS,
  DEPARTURE: DEPARTURE_FIELDS,
  ARRIVAL: ARRIVAL_FIELDS,
  BUNKER: BUNKER_FIELDS,
  POSITION: POSITION_FIELDS,
};

// ─────────────────────────────────────────────────────────────
// Formatters
// ─────────────────────────────────────────────────────────────

function fmtValue(val: unknown, fmt?: string, unit?: string): string {
  if (val == null || val === '') return '—';

  let display: string;
  if (fmt === 'date') {
    display = new Date(val as string).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  } else if (fmt === 'datetime') {
    display = new Date(val as string).toLocaleString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  } else if (fmt === 'coord') {
    display = typeof val === 'number' ? val.toFixed(6) : String(val);
  } else if (fmt === 'number') {
    display = typeof val === 'number' ? val.toLocaleString('vi-VN') : String(val);
  } else {
    display = String(val);
  }

  return unit ? `${display} ${unit}` : display;
}

function fmtDateTime(dateStr: string | null) {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  });
}

function parseSummaryJson<T>(val: unknown): T | null {
  if (val == null) return null;
  if (typeof val === 'object') return val as T;
  if (typeof val === 'string') {
    try { return JSON.parse(val) as T; } catch { return null; }
  }
  return null;
}

// ─────────────────────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────────────────────

export const ReportDetailPage: React.FC = () => {
  const { reportId } = useParams<{ reportId: string }>();
  const navigate = useNavigate();
  const [report, setReport] = useState<ReportDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!reportId) return;
    setLoading(true);
    setError(null);
    fetch(`${ENV.API_BASE_URL}/reports/${reportId}`)
      .then(res => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
      })
      .then((data: ReportDetail) => setReport(data))
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [reportId]);

  if (loading) {
    return (
      <div className="rd-container">
        <div className="rd-loading">Đang tải báo cáo…</div>
      </div>
    );
  }

  if (error || !report) {
    return (
      <div className="rd-container">
        <div className="rd-error">
          <p>Không thể tải báo cáo.</p>
          <button className="rd-back-btn" onClick={() => navigate(-1)}>← Quay lại</button>
        </div>
      </div>
    );
  }

  const typeMeta = TYPE_META[report.typeCode] ?? { label: report.typeName, icon: <FileText size={16} /> };
  const sections = FIELD_MAP[report.typeCode] ?? [];
  const child = report.childReport ?? {};

  return (
    <div className="rd-container">
      {/* Header */}
      <div className="rd-header">
        <button className="rd-back-btn" onClick={() => navigate(-1)}>
          <ArrowLeft size={14} /> Quay lại
        </button>

        <div className="rd-header__main">
          <div className="rd-header__icon">{typeMeta.icon}</div>
          <div>
            <h1 className="rd-header__title">{report.reportNumber}</h1>
            <p className="rd-header__subtitle">{typeMeta.label}</p>
          </div>
          <span className={`rd-status rd-status--${report.status}`}>
            {STATUS_LABEL[report.status] ?? report.status}
          </span>
        </div>
      </div>

      {/* Report meta */}
      <div className="rd-meta-card">
        <div className="rd-meta-grid">
          <div className="rd-meta-item">
            <span className="rd-meta-label">Thời gian báo cáo</span>
            <span className="rd-meta-value">{fmtDateTime(report.reportDateTime)}</span>
          </div>
          <div className="rd-meta-item">
            <span className="rd-meta-label">Người lập</span>
            <span className="rd-meta-value">{report.preparedBy ?? '—'}</span>
          </div>
          <div className="rd-meta-item">
            <span className="rd-meta-label">Thuyền trưởng ký</span>
            <span className="rd-meta-value">{report.masterSignature ?? '—'}</span>
          </div>
          <div className="rd-meta-item">
            <span className="rd-meta-label">Ngày ký</span>
            <span className="rd-meta-value">{fmtDateTime(report.signedAt)}</span>
          </div>
          <div className="rd-meta-item">
            <span className="rd-meta-label">Tàu (Origin)</span>
            <span className="rd-meta-value">{report.originNode}</span>
          </div>
          <div className="rd-meta-item">
            <span className="rd-meta-label">Ngày truyền</span>
            <span className="rd-meta-value">{fmtDateTime(report.transmittedAt)}</span>
          </div>
        </div>
        {report.remarks && (
          <div className="rd-meta-remarks">
            <span className="rd-meta-label">Ghi chú chung</span>
            <p>{report.remarks}</p>
          </div>
        )}
      </div>

      {/* Child report sections */}
      {sections.length === 0 && (
        <div className="rd-empty-child">Không có dữ liệu chi tiết cho loại báo cáo này.</div>
      )}

      {sections.map(({ section, fields }) => {
        const hasData = fields.some(f => child[f.key] != null && child[f.key] !== '');
        if (!hasData) return null;

        return (
          <div key={section} className="rd-section">
            <h2 className="rd-section__title">{section}</h2>
            <div className="rd-field-grid">
              {fields.map(f => {
                const val = child[f.key];
                if (val == null) return null;
                return (
                  <div key={f.key} className="rd-field">
                    <span className="rd-field__label">{f.label}</span>
                    <span className="rd-field__value">{fmtValue(val, f.fmt, f.unit)}</span>
                  </div>
                );
              })}
            </div>
          </div>
        );
      })}

      {/* PMS & Alarm Summaries for NOON reports */}
      {report.typeCode === 'NOON' && (() => {
        const maintenanceSummary = parseSummaryJson<MaintenanceSummary>(child.maintenanceSummaryJson);
        const alarmSummary = parseSummaryJson<AlarmSummary>(child.alarmSummaryJson);
        const certsExpiring = child.certificatesExpiringSoon as number | null | undefined;

        return (
          <div className="rd-summary-row">
            {/* PMS Summary */}
            {maintenanceSummary && (
              <div className="rd-section">
                <h2 className="rd-section__title">
                  <Wrench size={14} style={{ display: 'inline', marginRight: 6, color: '#059669' }} />
                  Tổng hợp bảo trì (PMS)
                </h2>
                <div className="rd-summary-list">
                  <div className="rd-summary-item"><span>Hoàn thành (24h)</span><span className="rd-summary-val">{maintenanceSummary.tasksCompletedLast24h}</span></div>
                  <div className="rd-summary-item"><span>Đang thực hiện</span><span className="rd-summary-val">{maintenanceSummary.tasksInProgress}</span></div>
                  <div className="rd-summary-item"><span>Quá hạn</span><span className="rd-summary-val rd-val--danger">{maintenanceSummary.overdueTasks}</span></div>
                  <div className="rd-summary-item"><span>Quan trọng sắp đến hạn</span><span className="rd-summary-val">{maintenanceSummary.criticalTasksDueSoon}</span></div>
                  <div className="rd-summary-item"><span>Lên lịch hôm nay</span><span className="rd-summary-val">{maintenanceSummary.totalScheduledToday}</span></div>
                  <div className="rd-summary-item"><span>Chờ hoãn</span><span className="rd-summary-val">{maintenanceSummary.pendingDeferrals}</span></div>
                </div>
                {maintenanceSummary.criticalMaintenanceNotes && (
                  <p className="rd-summary-notes">{maintenanceSummary.criticalMaintenanceNotes}</p>
                )}
              </div>
            )}

            {/* Alarm Summary */}
            {alarmSummary && (
              <div className="rd-section">
                <h2 className="rd-section__title">
                  <Bell size={14} style={{ display: 'inline', marginRight: 6, color: '#d97706' }} />
                  Tổng hợp cảnh báo
                </h2>
                <div className="rd-summary-list">
                  <div className="rd-summary-item"><span>Đang hoạt động</span><span className="rd-summary-val">{alarmSummary.activeAlarms}</span></div>
                  <div className="rd-summary-item"><span>Đã xác nhận</span><span className="rd-summary-val">{alarmSummary.acknowledgedAlarms}</span></div>
                  <div className="rd-summary-item"><span>Nghiêm trọng</span><span className="rd-summary-val rd-val--danger">{alarmSummary.criticalAlarms}</span></div>
                  <div className="rd-summary-item"><span>Cảnh báo</span><span className="rd-summary-val rd-val--warning">{alarmSummary.warningAlarms}</span></div>
                  <div className="rd-summary-item"><span>Đã xử lý (24h)</span><span className="rd-summary-val rd-val--success">{alarmSummary.resolvedLast24h}</span></div>
                </div>
                {alarmSummary.safetyNotes && (
                  <p className="rd-summary-notes">{alarmSummary.safetyNotes}</p>
                )}
              </div>
            )}

            {/* Certificates expiring */}
            {certsExpiring != null && certsExpiring > 0 && (
              <div className="rd-section">
                <h2 className="rd-section__title">Chứng chỉ sắp hết hạn</h2>
                <div className="rd-summary-list">
                  <div className="rd-summary-item">
                    <span>Chứng chỉ hết hạn trong 30 ngày</span>
                    <span className="rd-summary-val rd-val--warning">{certsExpiring}</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        );
      })()}
    </div>
  );
};
