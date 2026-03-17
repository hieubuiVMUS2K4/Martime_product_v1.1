import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { voyageApi } from '../../services/voyage.service';
import type {
  CreateBunkerPlanRequest,
  CreateCargoPlanRequest,
  CreateCrewChangePlanRequest,
  CreateVoyageRequest,
  CreatePlanLegRequest,
  CreatePortCallRequest,
  CreateCostEstimateRequest,
  CreateRevenueEstimateRequest,
  CreateVoyageExpenseRequest,
  CreateVoyageAdvancePaymentRequest,
  CreateVoyageDisbursementRequest,
  CreateVoyageActualRevenueRequest,
  CreateVoyageSettlementRequest,
  VoyageDetail,
} from '../../types/voyage.types';
import { PortSelect } from '../../components/common/PortSelect';
import './VoyageManagement.css';

const VOYAGE_STATUSES = ['PLANNING', 'APPROVED', 'READY', 'UNDERWAY', 'ARRIVED', 'COMPLETED', 'CANCELLED'];
const CHARTER_TYPES = ['Time Charter', 'Voyage Charter', 'Bareboat Charter', 'Contract of Affreightment', 'Spot Charter'];
const LEG_TYPES = ['PASSAGE', 'PORT_STAY', 'ANCHORAGE', 'CANAL_TRANSIT', 'BUNKERING'];
const CALL_TYPES = ['LOADING', 'DISCHARGE', 'BUNKERING', 'CREW_CHANGE', 'MAINTENANCE', 'INSPECTION', 'TRANSIT'];
const COST_CATEGORIES = ['BUNKER', 'PORT_CHARGES', 'CANAL_DUES', 'CREW_WAGES', 'INSURANCE', 'MAINTENANCE', 'PROVISIONS', 'OTHER'];
const REVENUE_CATEGORIES = ['FREIGHT', 'DEMURRAGE', 'CHARTER_HIRE', 'OTHER'];
const ADVANCE_TYPES = ['PORT_ADVANCE', 'CREW_WAGES', 'BUNKER', 'OTHER'];
const EXPENSE_STATUSES = ['DRAFT', 'SUBMITTED', 'APPROVED', 'REJECTED'];
const ADVANCE_STATUSES = ['PENDING', 'PAID', 'SETTLED'];
const DISBURSEMENT_STATUSES = ['RECORDED', 'VERIFIED', 'PAID'];
const ACTUAL_REVENUE_STATUSES = ['INVOICED', 'RECEIVED', 'CANCELLED'];
const SETTLEMENT_STATUSES = ['DRAFT', 'PREPARED', 'REVIEWED', 'APPROVED', 'CLOSED'];
const CARGO_PLAN_TYPES = ['LOADING', 'DISCHARGING', 'TRANSSHIPMENT'];
const BUNKER_FUEL_TYPES = ['HFO', 'VLSFO', 'MGO', 'MDO', 'LNG'];
const BUNKER_OPERATION_TYPES = ['SUPPLY', 'TRANSFER'];
const CREW_CHANGE_TYPES = ['EMBARK', 'DISEMBARK', 'ROTATION'];

interface FormState {
  voyageNumber: string;
  vesselIMO: string;
  vesselName: string;
  vesselFlag: string;
  callSign: string;
  charterType: string;
  departurePort: string;
  departurePortCode: string;
  departureTime: string;
  arrivalPort: string;
  arrivalPortCode: string;
  arrivalTime: string;
  previousPortCode: string;
  previousPortName: string;
  cargoType: string;
  cargoWeight: string;
  plannedDistance: string;
  plannedDurationHours: string;
  plannedAverageSpeed: string;
  plannedFuelConsumption: string;
  voyageInstructions: string;
  voyageStatus: string;
}

const emptyForm: FormState = {
  voyageNumber: '',
  vesselIMO: '',
  vesselName: '',
  vesselFlag: '',
  callSign: '',
  charterType: '',
  departurePort: '',
  departurePortCode: '',
  departureTime: '',
  arrivalPort: '',
  arrivalPortCode: '',
  arrivalTime: '',
  previousPortCode: '',
  previousPortName: '',
  cargoType: '',
  cargoWeight: '',
  plannedDistance: '',
  plannedDurationHours: '',
  plannedAverageSpeed: '',
  plannedFuelConsumption: '',
  voyageInstructions: '',
  voyageStatus: 'PLANNING',
};

const emptyLeg: CreatePlanLegRequest = {
  sequence: 1, legType: 'PASSAGE', fromPortCode: '', fromPortName: '',
  toPortCode: '', toPortName: '', plannedDistance: undefined,
  plannedDurationHours: undefined, plannedAverageSpeed: undefined,
};

const emptyPortCall: CreatePortCallRequest = {
  sequence: 1, callType: 'LOADING', portCode: '', portName: '',
};

const emptyCost: CreateCostEstimateRequest = {
  sequence: 1, costCategory: '', estimatedAmount: 0, currency: 'USD',
};

const emptyRevenue: CreateRevenueEstimateRequest = {
  sequence: 1, revenueCategory: '', estimatedAmount: 0, currency: 'USD',
};

const emptyCargoPlan: CreateCargoPlanRequest = {
  sequence: 1, operationType: 'LOADING', cargoType: '', plannedQuantity: 0, unit: 'MT',
};

const emptyBunkerPlan: CreateBunkerPlanRequest = {
  sequence: 1, fuelType: 'VLSFO', plannedQuantity: 0, operationType: 'SUPPLY',
};

const emptyCrewChangePlan: CreateCrewChangePlanRequest = {
  sequence: 1, changeType: 'ROTATION',
};

const emptyExpenseRequest: CreateVoyageExpenseRequest = {
  costCategory: '', requestedAmount: 0, currency: 'USD', exchangeRate: 1, allocationScope: 'VOYAGE', status: 'DRAFT',
};

const emptyAdvancePayment: CreateVoyageAdvancePaymentRequest = {
  advanceType: '', amount: 0, currency: 'USD', exchangeRate: 1, status: 'PENDING',
};

const emptyDisbursement: CreateVoyageDisbursementRequest = {
  costCategory: '', amount: 0, currency: 'USD', exchangeRate: 1, allocationScope: 'VOYAGE', status: 'RECORDED',
};

const emptyActualRevenue: CreateVoyageActualRevenueRequest = {
  revenueCategory: '', amount: 0, currency: 'USD', exchangeRate: 1, status: 'INVOICED',
};

const emptySettlement: CreateVoyageSettlementRequest = {
  status: 'DRAFT',
};

export const VoyageFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [form, setForm] = useState<FormState>(emptyForm);
  const [planLegs, setPlanLegs] = useState<CreatePlanLegRequest[]>([]);
  const [portCalls, setPortCalls] = useState<CreatePortCallRequest[]>([]);
  const [cargoPlans, setCargoPlans] = useState<CreateCargoPlanRequest[]>([]);
  const [bunkerPlans, setBunkerPlans] = useState<CreateBunkerPlanRequest[]>([]);
  const [crewChangePlans, setCrewChangePlans] = useState<CreateCrewChangePlanRequest[]>([]);
  const [costEstimates, setCostEstimates] = useState<CreateCostEstimateRequest[]>([]);
  const [revenueEstimates, setRevenueEstimates] = useState<CreateRevenueEstimateRequest[]>([]);
  const [expenseRequests, setExpenseRequests] = useState<CreateVoyageExpenseRequest[]>([]);
  const [advancePayments, setAdvancePayments] = useState<CreateVoyageAdvancePaymentRequest[]>([]);
  const [disbursements, setDisbursements] = useState<CreateVoyageDisbursementRequest[]>([]);
  const [actualRevenues, setActualRevenues] = useState<CreateVoyageActualRevenueRequest[]>([]);
  const [settlements, setSettlements] = useState<CreateVoyageSettlementRequest[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeSection, setActiveSection] = useState<string>('basic');

  useEffect(() => {
    if (isEdit) {
      loadVoyage();
    }
  }, [id]);

  async function loadVoyage() {
    if (!id) return;
    setLoading(true);
    try {
      const detail: VoyageDetail = await voyageApi.getVoyageDetail(id);
      setForm({
        voyageNumber: detail.voyageNumber || '',
        vesselIMO: detail.vesselIMO || '',
        vesselName: detail.vesselName || '',
        vesselFlag: detail.vesselFlag || '',
        callSign: detail.callSign || '',
        charterType: detail.charterType || '',
        departurePort: detail.departurePort || '',
        departurePortCode: detail.departurePortCode || '',
        departureTime: detail.departureTime ? detail.departureTime.slice(0, 16) : '',
        arrivalPort: detail.arrivalPort || '',
        arrivalPortCode: detail.arrivalPortCode || '',
        arrivalTime: detail.arrivalTime ? detail.arrivalTime.slice(0, 16) : '',
        previousPortCode: detail.previousPortCode || '',
        previousPortName: detail.previousPortName || '',
        cargoType: detail.cargoType || '',
        cargoWeight: detail.cargoWeight?.toString() || '',
        plannedDistance: detail.plannedDistance?.toString() || '',
        plannedDurationHours: detail.plannedDurationHours?.toString() || '',
        plannedAverageSpeed: detail.plannedAverageSpeed?.toString() || '',
        plannedFuelConsumption: detail.plannedFuelConsumption?.toString() || '',
        voyageInstructions: detail.voyageInstructions || '',
        voyageStatus: detail.voyageStatus || 'PLANNING',
      });
      setPlanLegs(detail.planLegs.map(l => ({
        sequence: l.sequence,
        legType: l.legType,
        fromPortCode: l.fromPortCode,
        fromPortName: l.fromPortName,
        toPortCode: l.toPortCode,
        toPortName: l.toPortName,
        plannedDepartureTime: l.plannedDepartureTime,
        plannedArrivalTime: l.plannedArrivalTime,
        plannedDistance: l.plannedDistance,
        plannedDurationHours: l.plannedDurationHours,
        plannedAverageSpeed: l.plannedAverageSpeed,
        cargoActivity: l.cargoActivity,
        crewChangePlanned: l.crewChangePlanned,
        bunkerSupplyPlanned: l.bunkerSupplyPlanned,
        plannedFuelConsumption: l.plannedFuelConsumption,
        weatherRoutingNotes: l.weatherRoutingNotes,
        notes: l.notes,
      })));
      setPortCalls(detail.portCalls.map(p => ({
        sequence: p.sequence,
        callType: p.callType,
        portCode: p.portCode,
        portName: p.portName,
        country: p.country,
        arrivalTime: p.arrivalTime,
        departureTime: p.departureTime,
        berthNumber: p.berthNumber,
        remarks: p.remarks,
      })));
      setCargoPlans((detail.cargoPlans || []).map((cp, i) => ({
        planLegId: cp.planLegId,
        sequence: cp.sequence ?? i + 1,
        operationType: cp.operationType,
        cargoType: cp.cargoType,
        cargoDescription: cp.cargoDescription,
        plannedQuantity: cp.plannedQuantity,
        unit: cp.unit,
        portCode: cp.portCode,
        portName: cp.portName,
        shipperName: cp.shipperName,
        consigneeName: cp.consigneeName,
        specialRequirements: cp.specialRequirements,
        notes: cp.notes,
      })));
      setBunkerPlans((detail.bunkerPlans || []).map((bp, i) => ({
        planLegId: bp.planLegId,
        sequence: bp.sequence ?? i + 1,
        fuelType: bp.fuelType,
        plannedQuantity: bp.plannedQuantity,
        operationType: bp.operationType,
        portCode: bp.portCode,
        portName: bp.portName,
        estimatedCostUsd: bp.estimatedCostUsd,
        supplierName: bp.supplierName,
        notes: bp.notes,
      })));
      setCrewChangePlans((detail.crewChangePlans || []).map((ccp, i) => ({
        planLegId: ccp.planLegId,
        sequence: ccp.sequence ?? i + 1,
        crewMemberId: ccp.crewMemberId,
        rankId: ccp.rankId,
        changeType: ccp.changeType,
        portCode: ccp.portCode,
        portName: ccp.portName,
        plannedDate: ccp.plannedDate,
        replacementReason: ccp.replacementReason,
        notes: ccp.notes,
      })));
      setCostEstimates((detail.costEstimates || []).map((c, i) => ({
        sequence: c.sequence ?? i + 1,
        costCategory: c.costCategory || '',
        description: c.description,
        estimatedAmount: c.estimatedAmount || 0,
        currency: c.currency || 'USD',
        notes: c.notes,
      })));
      setRevenueEstimates((detail.revenueEstimates || []).map((r, i) => ({
        sequence: r.sequence ?? i + 1,
        revenueCategory: r.revenueCategory || '',
        description: r.description,
        estimatedAmount: r.estimatedAmount || 0,
        currency: r.currency || 'USD',
        notes: r.notes,
      })));
      setExpenseRequests((detail.expenseRequests || []).map(er => ({
        requestNumber: er.requestNumber,
        costCategory: er.costCategory || '',
        allocationScope: er.allocationScope,
        description: er.description,
        requestedAmount: er.requestedAmount || 0,
        currency: er.currency || 'USD',
        exchangeRate: er.exchangeRate ?? 1,
        vendorName: er.vendorName,
        portCode: er.portCode,
        portName: er.portName,
        status: er.status || 'DRAFT',
        requestedBy: er.requestedBy,
        approvedBy: er.approvedBy,
        approvedAmount: er.approvedAmount,
        notes: er.notes,
      })));
      setAdvancePayments((detail.advancePayments || []).map(ap => ({
        advanceNumber: ap.advanceNumber,
        advanceType: ap.advanceType || '',
        description: ap.description,
        amount: ap.amount || 0,
        currency: ap.currency || 'USD',
        exchangeRate: ap.exchangeRate ?? 1,
        recipientName: ap.recipientName,
        portCode: ap.portCode,
        portName: ap.portName,
        status: ap.status || 'PENDING',
        paidBy: ap.paidBy,
        settledAmount: ap.settledAmount,
        notes: ap.notes,
      })));
      setDisbursements((detail.disbursements || []).map(d => ({
        disbursementNumber: d.disbursementNumber,
        costCategory: d.costCategory || '',
        allocationScope: d.allocationScope,
        description: d.description,
        amount: d.amount || 0,
        currency: d.currency || 'USD',
        exchangeRate: d.exchangeRate ?? 1,
        vendorName: d.vendorName,
        invoiceNumber: d.invoiceNumber,
        portCode: d.portCode,
        portName: d.portName,
        status: d.status || 'RECORDED',
        notes: d.notes,
      })));
      setActualRevenues((detail.actualRevenues || []).map(ar => ({
        revenueNumber: ar.revenueNumber,
        revenueCategory: ar.revenueCategory || '',
        description: ar.description,
        amount: ar.amount || 0,
        currency: ar.currency || 'USD',
        exchangeRate: ar.exchangeRate ?? 1,
        payerName: ar.payerName,
        invoiceNumber: ar.invoiceNumber,
        status: ar.status || 'INVOICED',
        notes: ar.notes,
      })));
      setSettlements((detail.settlements || []).map(s => ({
        settlementNumber: s.settlementNumber,
        status: s.status || 'DRAFT',
        totalExpenseApproved: s.totalExpenseApproved,
        totalAdvanced: s.totalAdvanced,
        totalDisbursed: s.totalDisbursed,
        totalRevenue: s.totalRevenue,
        netResult: s.netResult,
        advanceBalance: s.advanceBalance,
        finalSettlementAmount: s.finalSettlementAmount,
        summary: s.summary,
        preparedBy: s.preparedBy,
        reviewedBy: s.reviewedBy,
        approvedBy: s.approvedBy,
        notes: s.notes,
      })));
    } catch (err) {
      setError('Không thể tải dữ liệu hải trình');
    } finally {
      setLoading(false);
    }
  }

  function updateField(field: keyof FormState, value: string) {
    setForm(prev => ({ ...prev, [field]: value }));
  }

  function toNum(v: string) {
    const n = parseFloat(v);
    return isNaN(n) ? undefined : n;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form.voyageNumber.trim()) {
      setError('Số hải trình (Voyage Number) là bắt buộc');
      return;
    }

    setSaving(true);
    setError(null);
    try {
      const payload: CreateVoyageRequest = {
        voyageNumber: form.voyageNumber.trim(),
        vesselIMO: form.vesselIMO || undefined,
        vesselName: form.vesselName || undefined,
        vesselFlag: form.vesselFlag || undefined,
        callSign: form.callSign || undefined,
        charterType: form.charterType || undefined,
        departurePort: form.departurePort || undefined,
        departurePortCode: form.departurePortCode || undefined,
        departureTime: form.departureTime || undefined,
        arrivalPort: form.arrivalPort || undefined,
        arrivalPortCode: form.arrivalPortCode || undefined,
        arrivalTime: form.arrivalTime || undefined,
        previousPortCode: form.previousPortCode || undefined,
        previousPortName: form.previousPortName || undefined,
        cargoType: form.cargoType || undefined,
        cargoWeight: toNum(form.cargoWeight),
        plannedDistance: toNum(form.plannedDistance),
        plannedDurationHours: toNum(form.plannedDurationHours),
        plannedAverageSpeed: toNum(form.plannedAverageSpeed),
        plannedFuelConsumption: toNum(form.plannedFuelConsumption),
        voyageInstructions: form.voyageInstructions || undefined,
        voyageStatus: form.voyageStatus,
        planLegs: planLegs.length > 0 ? planLegs : undefined,
        portCalls: portCalls.length > 0 ? portCalls : undefined,
        cargoPlans: cargoPlans.length > 0 ? cargoPlans : undefined,
        bunkerPlans: bunkerPlans.length > 0 ? bunkerPlans : undefined,
        crewChangePlans: crewChangePlans.length > 0 ? crewChangePlans : undefined,
        costEstimates: costEstimates.length > 0 ? costEstimates : undefined,
        revenueEstimates: revenueEstimates.length > 0 ? revenueEstimates : undefined,
        expenseRequests: expenseRequests.length > 0 ? expenseRequests : undefined,
        advancePayments: advancePayments.length > 0 ? advancePayments : undefined,
        disbursements: disbursements.length > 0 ? disbursements : undefined,
        actualRevenues: actualRevenues.length > 0 ? actualRevenues : undefined,
        settlements: settlements.length > 0 ? settlements : undefined,
      };

      if (isEdit) {
        await voyageApi.updateVoyage(id, payload);
        navigate(`/voyages/${id}`);
      } else {
        const result = await voyageApi.createVoyage(payload);
        navigate(`/voyages/${result.id}`);
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Lỗi khi lưu hải trình');
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="voyage-loading">Đang tải...</div>;

  const sections = [
    { key: 'basic', label: 'Thông tin chung' },
    { key: 'route', label: 'Tuyến đường' },
    { key: 'planning', label: 'Kế hoạch chặng' },
    { key: 'portcalls', label: 'Cảng ghé' },
    { key: 'commercial', label: 'Kế hoạch hàng hóa' },
    { key: 'financial', label: 'Tài chính ước tính' },
    { key: 'actual_financial', label: 'Tài chính thực tế' },
  ];

  return (
    <div className="voyage-form-page">
      <div className="voyage-form-header">
        <div className="voyage-form-title-row">
          <button className="btn-back" onClick={() => navigate(isEdit ? `/voyages/${id}` : '/voyages')}>
            ← Quay lại
          </button>
          <h1>{isEdit ? 'Chỉnh sửa hải trình' : 'Tạo hải trình mới'}</h1>
        </div>
        {error && <div className="voyage-form-error">{error}</div>}
      </div>

      <div className="voyage-form-sections">
        {sections.map(s => (
          <button
            key={s.key}
            className={`voyage-form-section-btn ${activeSection === s.key ? 'active' : ''}`}
            onClick={() => setActiveSection(s.key)}
          >
            {s.label}
          </button>
        ))}
      </div>

      <form onSubmit={handleSubmit} className="voyage-form-body">
        {/* Thông tin chung */}
        {activeSection === 'basic' && (
          <div className="voyage-form-section">
            <h2>Thông tin chung</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Số hải trình *</label>
                <input type="text" value={form.voyageNumber} onChange={e => updateField('voyageNumber', e.target.value)} required />
              </div>
              <div className="form-group">
                <label>Trạng thái</label>
                <select value={form.voyageStatus} onChange={e => updateField('voyageStatus', e.target.value)}>
                  {VOYAGE_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Tên tàu</label>
                <input type="text" value={form.vesselName} onChange={e => updateField('vesselName', e.target.value)} />
              </div>
              <div className="form-group">
                <label>IMO</label>
                <input type="text" value={form.vesselIMO} onChange={e => updateField('vesselIMO', e.target.value)} maxLength={10} />
              </div>
              <div className="form-group">
                <label>Quốc tịch tàu</label>
                <input type="text" value={form.vesselFlag} onChange={e => updateField('vesselFlag', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Hô hiệu (Call Sign)</label>
                <input type="text" value={form.callSign} onChange={e => updateField('callSign', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Loại thuê tàu</label>
                <select value={form.charterType} onChange={e => updateField('charterType', e.target.value)}>
                  <option value="">-- Chọn --</option>
                  {CHARTER_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Loại hàng hóa</label>
                <input type="text" value={form.cargoType} onChange={e => updateField('cargoType', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Trọng lượng hàng (MT)</label>
                <input type="number" step="0.1" value={form.cargoWeight} onChange={e => updateField('cargoWeight', e.target.value)} />
              </div>
              <div className="form-group full-width">
                <label>Chỉ thị hải trình</label>
                <textarea value={form.voyageInstructions} onChange={e => updateField('voyageInstructions', e.target.value)} rows={3} />
              </div>
            </div>
          </div>
        )}

        {/* Tuyến đường */}
        {activeSection === 'route' && (
          <div className="voyage-form-section">
            <h2>Tuyến đường</h2>
            <div className="form-grid">
              <div className="form-group">
                <label>Cảng đi</label>
                <PortSelect
                  value={form.departurePortCode ? `${form.departurePortCode} - ${form.departurePort}` : form.departurePort}
                  onChange={p => setForm(prev => ({ ...prev, departurePort: p.portName, departurePortCode: p.portCode }))}
                  placeholder="Tìm cảng đi..."
                />
              </div>
              <div className="form-group">
                <label>Thời gian khởi hành</label>
                <input type="datetime-local" value={form.departureTime} onChange={e => updateField('departureTime', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Cảng đến</label>
                <PortSelect
                  value={form.arrivalPortCode ? `${form.arrivalPortCode} - ${form.arrivalPort}` : form.arrivalPort}
                  onChange={p => setForm(prev => ({ ...prev, arrivalPort: p.portName, arrivalPortCode: p.portCode }))}
                  placeholder="Tìm cảng đến..."
                />
              </div>
              <div className="form-group">
                <label>Thời gian đến</label>
                <input type="datetime-local" value={form.arrivalTime} onChange={e => updateField('arrivalTime', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Cảng trước đó</label>
                <PortSelect
                  value={form.previousPortCode ? `${form.previousPortCode} - ${form.previousPortName}` : form.previousPortName}
                  onChange={p => setForm(prev => ({ ...prev, previousPortName: p.portName, previousPortCode: p.portCode }))}
                  placeholder="Tìm cảng trước đó..."
                />
              </div>
              <div className="form-group">
                <label>Quãng đường dự kiến (NM)</label>
                <input type="number" step="0.1" value={form.plannedDistance} onChange={e => updateField('plannedDistance', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Thời gian dự kiến (giờ)</label>
                <input type="number" step="0.1" value={form.plannedDurationHours} onChange={e => updateField('plannedDurationHours', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Tốc độ trung bình (knots)</label>
                <input type="number" step="0.1" value={form.plannedAverageSpeed} onChange={e => updateField('plannedAverageSpeed', e.target.value)} />
              </div>
              <div className="form-group">
                <label>Nhiên liệu dự kiến (MT)</label>
                <input type="number" step="0.1" value={form.plannedFuelConsumption} onChange={e => updateField('plannedFuelConsumption', e.target.value)} />
              </div>
            </div>
          </div>
        )}

        {/* Kế hoạch chặng */}
        {activeSection === 'planning' && (
          <div className="voyage-form-section">
            <div className="section-header-row">
              <h2>Kế hoạch chặng (Plan Legs)</h2>
              <button type="button" className="btn-add" onClick={() => setPlanLegs(prev => [...prev, { ...emptyLeg, sequence: prev.length + 1 }])}>
                + Thêm chặng
              </button>
            </div>
            {planLegs.length === 0 && <p className="empty-hint">Chưa có chặng nào. Bấm "Thêm chặng" để bắt đầu.</p>}
            {planLegs.map((leg, i) => (
              <div key={i} className="sub-entity-card">
                <div className="sub-entity-header">
                  <span>Chặng #{leg.sequence}</span>
                  <button type="button" className="btn-remove" onClick={() => setPlanLegs(prev => prev.filter((_, j) => j !== i).map((l, j) => ({ ...l, sequence: j + 1 })))}>Xóa</button>
                </div>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Loại chặng</label>
                    <select value={leg.legType || 'PASSAGE'} onChange={e => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, legType: e.target.value } : l))}>
                      {LEG_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Từ cảng</label>
                    <PortSelect
                      value={leg.fromPortCode ? `${leg.fromPortCode} - ${leg.fromPortName}` : (leg.fromPortName || '')}
                      onChange={p => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, fromPortCode: p.portCode, fromPortName: p.portName } : l))}
                      placeholder="Tìm cảng đi..."
                    />
                  </div>
                  <div className="form-group">
                    <label>Đến cảng</label>
                    <PortSelect
                      value={leg.toPortCode ? `${leg.toPortCode} - ${leg.toPortName}` : (leg.toPortName || '')}
                      onChange={p => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, toPortCode: p.portCode, toPortName: p.portName } : l))}
                      placeholder="Tìm cảng đến..."
                    />
                  </div>
                  <div className="form-group">
                    <label>Khoảng cách (NM)</label>
                    <input type="number" step="0.1" value={leg.plannedDistance ?? ''} onChange={e => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, plannedDistance: parseFloat(e.target.value) || undefined } : l))} />
                  </div>
                  <div className="form-group">
                    <label>Thời gian (giờ)</label>
                    <input type="number" step="0.1" value={leg.plannedDurationHours ?? ''} onChange={e => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, plannedDurationHours: parseFloat(e.target.value) || undefined } : l))} />
                  </div>
                  <div className="form-group">
                    <label>Tốc độ (knots)</label>
                    <input type="number" step="0.1" value={leg.plannedAverageSpeed ?? ''} onChange={e => setPlanLegs(prev => prev.map((l, j) => j === i ? { ...l, plannedAverageSpeed: parseFloat(e.target.value) || undefined } : l))} />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Cảng ghé */}
        {activeSection === 'portcalls' && (
          <div className="voyage-form-section">
            <div className="section-header-row">
              <h2>Cảng ghé (Port Calls)</h2>
              <button type="button" className="btn-add" onClick={() => setPortCalls(prev => [...prev, { ...emptyPortCall, sequence: prev.length + 1 }])}>
                + Thêm cảng ghé
              </button>
            </div>
            {portCalls.length === 0 && <p className="empty-hint">Chưa có cảng ghé nào.</p>}
            {portCalls.map((pc, i) => (
              <div key={i} className="sub-entity-card">
                <div className="sub-entity-header">
                  <span>Cảng #{pc.sequence}</span>
                  <button type="button" className="btn-remove" onClick={() => setPortCalls(prev => prev.filter((_, j) => j !== i).map((p, j) => ({ ...p, sequence: j + 1 })))}>Xóa</button>
                </div>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Loại</label>
                    <select value={pc.callType || 'LOADING'} onChange={e => setPortCalls(prev => prev.map((p, j) => j === i ? { ...p, callType: e.target.value } : p))}>
                      {CALL_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Cảng</label>
                    <PortSelect
                      value={pc.portCode ? `${pc.portCode} - ${pc.portName}` : pc.portName}
                      onChange={p => setPortCalls(prev => prev.map((x, j) => j === i ? { ...x, portCode: p.portCode, portName: p.portName, country: p.country } : x))}
                      placeholder="Tìm cảng..."
                    />
                  </div>
                  <div className="form-group">
                    <label>Thời gian đến</label>
                    <input type="datetime-local" value={pc.arrivalTime ? pc.arrivalTime.slice(0, 16) : ''} onChange={e => setPortCalls(prev => prev.map((p, j) => j === i ? { ...p, arrivalTime: e.target.value || undefined } : p))} />
                  </div>
                  <div className="form-group">
                    <label>Thời gian rời</label>
                    <input type="datetime-local" value={pc.departureTime ? pc.departureTime.slice(0, 16) : ''} onChange={e => setPortCalls(prev => prev.map((p, j) => j === i ? { ...p, departureTime: e.target.value || undefined } : p))} />
                  </div>
                  <div className="form-group">
                    <label>Bến (Berth)</label>
                    <input type="text" value={pc.berthNumber || ''} onChange={e => setPortCalls(prev => prev.map((p, j) => j === i ? { ...p, berthNumber: e.target.value } : p))} />
                  </div>
                  <div className="form-group">
                    <label>Ghi chú</label>
                    <input type="text" value={pc.remarks || ''} onChange={e => setPortCalls(prev => prev.map((p, j) => j === i ? { ...p, remarks: e.target.value } : p))} />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Kế hoạch hàng hóa / bunker / đổi crew */}
        {activeSection === 'commercial' && (
          <div className="voyage-form-section">
            <div className="section-header-row">
              <h2>Kế hoạch hàng hóa</h2>
              <button type="button" className="btn-add" onClick={() => setCargoPlans(prev => [...prev, { ...emptyCargoPlan, sequence: prev.length + 1 }])}>
                + Thêm cargo plan
              </button>
            </div>
            {cargoPlans.length === 0 && <p className="empty-hint">Chưa có cargo plan nào.</p>}
            {cargoPlans.map((cp, i) => (
              <div key={i} className="sub-entity-card">
                <div className="sub-entity-header">
                  <span>Cargo #{cp.sequence}</span>
                  <button type="button" className="btn-remove" onClick={() => setCargoPlans(prev => prev.filter((_, j) => j !== i).map((item, j) => ({ ...item, sequence: j + 1 })))}>Xóa</button>
                </div>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Loại nghiệp vụ</label>
                    <select value={cp.operationType || 'LOADING'} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, operationType: e.target.value } : item))}>
                      {CARGO_PLAN_TYPES.map(type => <option key={type} value={type}>{type}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Loại hàng</label>
                    <input type="text" value={cp.cargoType} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, cargoType: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Số lượng kế hoạch</label>
                    <input type="number" step="0.01" value={cp.plannedQuantity} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, plannedQuantity: parseFloat(e.target.value) || 0 } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Đơn vị</label>
                    <input type="text" value={cp.unit || 'MT'} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, unit: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Cảng</label>
                    <input type="text" value={cp.portName || ''} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, portName: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Mã cảng</label>
                    <input type="text" value={cp.portCode || ''} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, portCode: e.target.value } : item))} maxLength={5} />
                  </div>
                  <div className="form-group">
                    <label>Shipper</label>
                    <input type="text" value={cp.shipperName || ''} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, shipperName: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Consignee</label>
                    <input type="text" value={cp.consigneeName || ''} onChange={e => setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, consigneeName: e.target.value } : item))} />
                  </div>
                  <div className="form-group full-width">
                    <label>Mô tả / yêu cầu đặc biệt</label>
                    <textarea value={`${cp.cargoDescription || ''}${cp.specialRequirements ? `\n${cp.specialRequirements}` : ''}`.trim()} onChange={e => {
                      const value = e.target.value;
                      setCargoPlans(prev => prev.map((item, j) => j === i ? { ...item, cargoDescription: value || undefined, specialRequirements: undefined } : item));
                    }} rows={3} />
                  </div>
                </div>
              </div>
            ))}

            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Kế hoạch bunker</h2>
              <button type="button" className="btn-add" onClick={() => setBunkerPlans(prev => [...prev, { ...emptyBunkerPlan, sequence: prev.length + 1 }])}>
                + Thêm bunker plan
              </button>
            </div>
            {bunkerPlans.length === 0 && <p className="empty-hint">Chưa có bunker plan nào.</p>}
            {bunkerPlans.map((bp, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="sub-entity-header">
                  <span>Bunker #{bp.sequence}</span>
                  <button type="button" className="btn-remove" onClick={() => setBunkerPlans(prev => prev.filter((_, j) => j !== i).map((item, j) => ({ ...item, sequence: j + 1 })))}>Xóa</button>
                </div>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Fuel type</label>
                    <select value={bp.fuelType || 'VLSFO'} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, fuelType: e.target.value } : item))}>
                      {BUNKER_FUEL_TYPES.map(type => <option key={type} value={type}>{type}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Operation</label>
                    <select value={bp.operationType || 'SUPPLY'} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, operationType: e.target.value } : item))}>
                      {BUNKER_OPERATION_TYPES.map(type => <option key={type} value={type}>{type}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Số lượng</label>
                    <input type="number" step="0.01" value={bp.plannedQuantity} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, plannedQuantity: parseFloat(e.target.value) || 0 } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Cảng</label>
                    <input type="text" value={bp.portName || ''} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, portName: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Mã cảng</label>
                    <input type="text" value={bp.portCode || ''} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, portCode: e.target.value } : item))} maxLength={5} />
                  </div>
                  <div className="form-group">
                    <label>Chi phí ước tính</label>
                    <input type="number" step="0.01" value={bp.estimatedCostUsd ?? ''} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, estimatedCostUsd: parseFloat(e.target.value) || undefined } : item))} />
                  </div>
                  <div className="form-group full-width">
                    <label>Supplier / ghi chú</label>
                    <input type="text" value={bp.supplierName || bp.notes || ''} onChange={e => setBunkerPlans(prev => prev.map((item, j) => j === i ? { ...item, supplierName: e.target.value } : item))} />
                  </div>
                </div>
              </div>
            ))}

            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Kế hoạch thay đổi crew</h2>
              <button type="button" className="btn-add" onClick={() => setCrewChangePlans(prev => [...prev, { ...emptyCrewChangePlan, sequence: prev.length + 1 }])}>
                + Thêm crew change
              </button>
            </div>
            {crewChangePlans.length === 0 && <p className="empty-hint">Chưa có crew change plan nào.</p>}
            {crewChangePlans.map((ccp, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="sub-entity-header">
                  <span>Crew Change #{ccp.sequence}</span>
                  <button type="button" className="btn-remove" onClick={() => setCrewChangePlans(prev => prev.filter((_, j) => j !== i).map((item, j) => ({ ...item, sequence: j + 1 })))}>Xóa</button>
                </div>
                <div className="form-grid">
                  <div className="form-group">
                    <label>Loại thay đổi</label>
                    <select value={ccp.changeType || 'ROTATION'} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, changeType: e.target.value } : item))}>
                      {CREW_CHANGE_TYPES.map(type => <option key={type} value={type}>{type}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Crew Member ID</label>
                    <input type="text" value={ccp.crewMemberId || ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, crewMemberId: e.target.value || undefined } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Rank ID</label>
                    <input type="number" value={ccp.rankId ?? ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, rankId: parseInt(e.target.value, 10) || undefined } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Cảng</label>
                    <input type="text" value={ccp.portName || ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, portName: e.target.value } : item))} />
                  </div>
                  <div className="form-group">
                    <label>Mã cảng</label>
                    <input type="text" value={ccp.portCode || ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, portCode: e.target.value } : item))} maxLength={5} />
                  </div>
                  <div className="form-group">
                    <label>Ngày dự kiến</label>
                    <input type="datetime-local" value={ccp.plannedDate ? ccp.plannedDate.slice(0, 16) : ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, plannedDate: e.target.value || undefined } : item))} />
                  </div>
                  <div className="form-group full-width">
                    <label>Lý do / ghi chú</label>
                    <input type="text" value={ccp.replacementReason || ccp.notes || ''} onChange={e => setCrewChangePlans(prev => prev.map((item, j) => j === i ? { ...item, replacementReason: e.target.value } : item))} />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Tài chính */}
        {activeSection === 'financial' && (
          <div className="voyage-form-section">
            <div className="section-header-row">
              <h2>Chi phí ước tính</h2>
              <button type="button" className="btn-add" onClick={() => setCostEstimates(prev => [...prev, { ...emptyCost, sequence: prev.length + 1 }])}>
                + Thêm chi phí
              </button>
            </div>
            {costEstimates.map((ce, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Danh mục</label>
                    <select value={ce.costCategory} onChange={e => setCostEstimates(prev => prev.map((c, j) => j === i ? { ...c, costCategory: e.target.value } : c))}>
                      <option value="">-- Chọn --</option>
                      {COST_CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Mô tả</label>
                    <input type="text" value={ce.description || ''} onChange={e => setCostEstimates(prev => prev.map((c, j) => j === i ? { ...c, description: e.target.value } : c))} />
                  </div>
                  <div className="form-group">
                    <label>Số tiền (USD)</label>
                    <input type="number" step="0.01" value={ce.estimatedAmount} onChange={e => setCostEstimates(prev => prev.map((c, j) => j === i ? { ...c, estimatedAmount: parseFloat(e.target.value) || 0 } : c))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setCostEstimates(prev => prev.filter((_, j) => j !== i).map((c, j) => ({ ...c, sequence: j + 1 })))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Doanh thu ước tính</h2>
              <button type="button" className="btn-add" onClick={() => setRevenueEstimates(prev => [...prev, { ...emptyRevenue, sequence: prev.length + 1 }])}>
                + Thêm doanh thu
              </button>
            </div>
            {revenueEstimates.map((re, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Danh mục</label>
                    <select value={re.revenueCategory} onChange={e => setRevenueEstimates(prev => prev.map((r, j) => j === i ? { ...r, revenueCategory: e.target.value } : r))}>
                      <option value="">-- Chọn --</option>
                      {REVENUE_CATEGORIES.map(r => <option key={r} value={r}>{r}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Mô tả</label>
                    <input type="text" value={re.description || ''} onChange={e => setRevenueEstimates(prev => prev.map((r, j) => j === i ? { ...r, description: e.target.value } : r))} />
                  </div>
                  <div className="form-group">
                    <label>Số tiền (USD)</label>
                    <input type="number" step="0.01" value={re.estimatedAmount} onChange={e => setRevenueEstimates(prev => prev.map((r, j) => j === i ? { ...r, estimatedAmount: parseFloat(e.target.value) || 0 } : r))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setRevenueEstimates(prev => prev.filter((_, j) => j !== i).map((r, j) => ({ ...r, sequence: j + 1 })))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {(costEstimates.length > 0 || revenueEstimates.length > 0) && (
              <div className="financial-summary-box">
                <div className="fin-row">
                  <span>Tổng chi phí ước tính:</span>
                  <strong>${costEstimates.reduce((s, c) => s + c.estimatedAmount, 0).toLocaleString()}</strong>
                </div>
                <div className="fin-row">
                  <span>Tổng doanh thu ước tính:</span>
                  <strong>${revenueEstimates.reduce((s, r) => s + r.estimatedAmount, 0).toLocaleString()}</strong>
                </div>
                <div className="fin-row total">
                  <span>Lợi nhuận ước tính:</span>
                  <strong>${(revenueEstimates.reduce((s, r) => s + r.estimatedAmount, 0) - costEstimates.reduce((s, c) => s + c.estimatedAmount, 0)).toLocaleString()}</strong>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Tài chính thực tế */}
        {activeSection === 'actual_financial' && (
          <div className="voyage-form-section">
            {/* Expense Requests */}
            <div className="section-header-row">
              <h2>Yêu cầu chi phí</h2>
              <button type="button" className="btn-add" onClick={() => setExpenseRequests(prev => [...prev, { ...emptyExpenseRequest }])}>
                + Thêm yêu cầu
              </button>
            </div>
            {expenseRequests.map((er, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Danh mục chi phí</label>
                    <select value={er.costCategory} onChange={e => setExpenseRequests(prev => prev.map((x, j) => j === i ? { ...x, costCategory: e.target.value } : x))}>
                      <option value="">-- Chọn --</option>
                      {COST_CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Số tiền yêu cầu</label>
                    <input type="number" step="0.01" value={er.requestedAmount} onChange={e => setExpenseRequests(prev => prev.map((x, j) => j === i ? { ...x, requestedAmount: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Nhà cung cấp</label>
                    <input type="text" value={er.vendorName || ''} onChange={e => setExpenseRequests(prev => prev.map((x, j) => j === i ? { ...x, vendorName: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select value={er.status || 'DRAFT'} onChange={e => setExpenseRequests(prev => prev.map((x, j) => j === i ? { ...x, status: e.target.value } : x))}>
                      {EXPENSE_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Ghi chú</label>
                    <input type="text" value={er.notes || ''} onChange={e => setExpenseRequests(prev => prev.map((x, j) => j === i ? { ...x, notes: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setExpenseRequests(prev => prev.filter((_, j) => j !== i))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {/* Advance Payments */}
            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Tạm ứng</h2>
              <button type="button" className="btn-add" onClick={() => setAdvancePayments(prev => [...prev, { ...emptyAdvancePayment }])}>
                + Thêm tạm ứng
              </button>
            </div>
            {advancePayments.map((ap, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Loại tạm ứng</label>
                    <select value={ap.advanceType} onChange={e => setAdvancePayments(prev => prev.map((x, j) => j === i ? { ...x, advanceType: e.target.value } : x))}>
                      <option value="">-- Chọn --</option>
                      {ADVANCE_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Số tiền</label>
                    <input type="number" step="0.01" value={ap.amount} onChange={e => setAdvancePayments(prev => prev.map((x, j) => j === i ? { ...x, amount: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Người nhận</label>
                    <input type="text" value={ap.recipientName || ''} onChange={e => setAdvancePayments(prev => prev.map((x, j) => j === i ? { ...x, recipientName: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select value={ap.status || 'PENDING'} onChange={e => setAdvancePayments(prev => prev.map((x, j) => j === i ? { ...x, status: e.target.value } : x))}>
                      {ADVANCE_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Ghi chú</label>
                    <input type="text" value={ap.notes || ''} onChange={e => setAdvancePayments(prev => prev.map((x, j) => j === i ? { ...x, notes: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setAdvancePayments(prev => prev.filter((_, j) => j !== i))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {/* Disbursements */}
            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Giải ngân</h2>
              <button type="button" className="btn-add" onClick={() => setDisbursements(prev => [...prev, { ...emptyDisbursement }])}>
                + Thêm giải ngân
              </button>
            </div>
            {disbursements.map((d, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Danh mục chi phí</label>
                    <select value={d.costCategory} onChange={e => setDisbursements(prev => prev.map((x, j) => j === i ? { ...x, costCategory: e.target.value } : x))}>
                      <option value="">-- Chọn --</option>
                      {COST_CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Số tiền</label>
                    <input type="number" step="0.01" value={d.amount} onChange={e => setDisbursements(prev => prev.map((x, j) => j === i ? { ...x, amount: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Nhà cung cấp</label>
                    <input type="text" value={d.vendorName || ''} onChange={e => setDisbursements(prev => prev.map((x, j) => j === i ? { ...x, vendorName: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Số hóa đơn</label>
                    <input type="text" value={d.invoiceNumber || ''} onChange={e => setDisbursements(prev => prev.map((x, j) => j === i ? { ...x, invoiceNumber: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select value={d.status || 'RECORDED'} onChange={e => setDisbursements(prev => prev.map((x, j) => j === i ? { ...x, status: e.target.value } : x))}>
                      {DISBURSEMENT_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setDisbursements(prev => prev.filter((_, j) => j !== i))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {/* Actual Revenues */}
            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Doanh thu thực tế</h2>
              <button type="button" className="btn-add" onClick={() => setActualRevenues(prev => [...prev, { ...emptyActualRevenue }])}>
                + Thêm doanh thu
              </button>
            </div>
            {actualRevenues.map((ar, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Danh mục</label>
                    <select value={ar.revenueCategory} onChange={e => setActualRevenues(prev => prev.map((x, j) => j === i ? { ...x, revenueCategory: e.target.value } : x))}>
                      <option value="">-- Chọn --</option>
                      {REVENUE_CATEGORIES.map(r => <option key={r} value={r}>{r}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Số tiền</label>
                    <input type="number" step="0.01" value={ar.amount} onChange={e => setActualRevenues(prev => prev.map((x, j) => j === i ? { ...x, amount: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Người trả</label>
                    <input type="text" value={ar.payerName || ''} onChange={e => setActualRevenues(prev => prev.map((x, j) => j === i ? { ...x, payerName: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select value={ar.status || 'INVOICED'} onChange={e => setActualRevenues(prev => prev.map((x, j) => j === i ? { ...x, status: e.target.value } : x))}>
                      {ACTUAL_REVENUE_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Ghi chú</label>
                    <input type="text" value={ar.notes || ''} onChange={e => setActualRevenues(prev => prev.map((x, j) => j === i ? { ...x, notes: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setActualRevenues(prev => prev.filter((_, j) => j !== i))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {/* Settlements */}
            <div className="section-header-row" style={{ marginTop: 24 }}>
              <h2>Quyết toán</h2>
              <button type="button" className="btn-add" onClick={() => setSettlements(prev => [...prev, { ...emptySettlement }])}>
                + Thêm quyết toán
              </button>
            </div>
            {settlements.map((s, i) => (
              <div key={i} className="sub-entity-card compact">
                <div className="form-grid">
                  <div className="form-group">
                    <label>Trạng thái</label>
                    <select value={s.status || 'DRAFT'} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, status: e.target.value } : x))}>
                      {SETTLEMENT_STATUSES.map(st => <option key={st} value={st}>{st}</option>)}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Tổng chi phí duyệt</label>
                    <input type="number" step="0.01" value={s.totalExpenseApproved || 0} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, totalExpenseApproved: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Tổng tạm ứng</label>
                    <input type="number" step="0.01" value={s.totalAdvanced || 0} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, totalAdvanced: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Tổng giải ngân</label>
                    <input type="number" step="0.01" value={s.totalDisbursed || 0} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, totalDisbursed: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Tổng doanh thu</label>
                    <input type="number" step="0.01" value={s.totalRevenue || 0} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, totalRevenue: parseFloat(e.target.value) || 0 } : x))} />
                  </div>
                  <div className="form-group">
                    <label>Ghi chú</label>
                    <input type="text" value={s.notes || ''} onChange={e => setSettlements(prev => prev.map((x, j) => j === i ? { ...x, notes: e.target.value } : x))} />
                  </div>
                  <div className="form-group">
                    <button type="button" className="btn-remove" onClick={() => setSettlements(prev => prev.filter((_, j) => j !== i))}>Xóa</button>
                  </div>
                </div>
              </div>
            ))}

            {/* Actual Financial Summary */}
            {(disbursements.length > 0 || actualRevenues.length > 0) && (
              <div className="financial-summary-box" style={{ marginTop: 24 }}>
                <div className="fin-row">
                  <span>Tổng giải ngân (thực tế):</span>
                  <strong>${disbursements.reduce((s, d) => s + d.amount * (d.exchangeRate || 1), 0).toLocaleString()}</strong>
                </div>
                <div className="fin-row">
                  <span>Tổng doanh thu (thực tế):</span>
                  <strong>${actualRevenues.reduce((s, r) => s + r.amount * (r.exchangeRate || 1), 0).toLocaleString()}</strong>
                </div>
                <div className="fin-row total">
                  <span>Lợi nhuận thực tế:</span>
                  <strong>${(actualRevenues.reduce((s, r) => s + r.amount * (r.exchangeRate || 1), 0) - disbursements.reduce((s, d) => s + d.amount * (d.exchangeRate || 1), 0)).toLocaleString()}</strong>
                </div>
              </div>
            )}
          </div>
        )}

        <div className="voyage-form-actions">
          <button type="button" className="btn-cancel" onClick={() => navigate(isEdit ? `/voyages/${id}` : '/voyages')}>
            Hủy
          </button>
          <button type="submit" className="btn-save" disabled={saving}>
            {saving ? 'Đang lưu...' : isEdit ? 'Cập nhật hải trình' : 'Tạo hải trình'}
          </button>
        </div>
      </form>
    </div>
  );
};
