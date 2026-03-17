import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Ship, ArrowLeft, Save, Loader2, AlertCircle } from 'lucide-react';
import { ENV } from '../../config/env';
import { VesselCrewTab } from '../../components/vessel-detail/VesselCrewTab';
import { BasicDataTab } from '../../components/vessel-detail/BasicDataTab';
import { DimensionsTab } from '../../components/vessel-detail/DimensionsTab';
import { MachineryTab } from '../../components/vessel-detail/MachineryTab';
import { ShipownerTab } from '../../components/vessel-detail/ShipownerTab';
import { ChartererTab } from '../../components/vessel-detail/ChartererTab';
import { ClassFlagStateTab } from '../../components/vessel-detail/ClassFlagStateTab';
import { InsuranceTab } from '../../components/vessel-detail/InsuranceTab';
import { RadioCommTab } from '../../components/vessel-detail/RadioCommTab';
import { TanksCargoTab } from '../../components/vessel-detail/TanksCargoTab';
import { VesselCertificateTab } from '../../components/vessel-detail/VesselCertificateTab';
import { useToast } from '../../components/common/Toast';
import './VesselDetailPage.css';

// ============================================================
// EXTENDED VESSEL TYPE with all fields from backend
// ============================================================
interface Vessel {
  id: string;
  imo: string;
  name: string;
  callSign: string;
  vesselType: string;
  grossTonnage: number;
  deadWeight: number;
  buildDate: string;
  flag: string;
  isActive: boolean;
  
  // Extended Basic Data
  officialNumber?: string;
  portOfRegistry?: string;
  previousName?: string;
  previousFlag?: string;
  mmsiNumber?: string;
  classNotation?: string;
  classRegisterNumber?: string;
  shipyardCountry?: string;
  shipyardName?: string;
  yardNo?: string;
  companyImoNumber?: string;
  suezCanalIdNumber?: string;
  keelLaidDate?: string;
  yearBuilt?: number;
  dateOfRegistry?: string;
  ownerImoNumber?: string;
  panamaCanalIdNumber?: string;
  maxPersonsAllowedOB?: number;
  serviceSpeedKts?: number;
  vrpNumber?: string;
  vrpType?: string;
  noOfCrewSafeManning?: number;
  maxPassengersAllowedOB?: number;
  
  // Dimensions
  loa?: number;
  lbp?: number;
  breadthMoulded?: number;
  depthMoulded?: number;
  draftMoulded?: number;
  draftScantling?: number;
  draftFullBallast?: number;
  hMaxAirdraft?: number;
  lightShip?: number;
  blockCoefficient?: number;
  tpcAtSummerDraft?: number;
  grossTonnageInternational?: number;
  grossTonnageSuezCanal?: number;
  grossTonnagePanamaCanal?: number;
  nettTonnageInternational?: number;
  
  // Machinery
  anchorChainPort?: number;
  anchorChainStarboard?: number;
  anchorChainStern?: number;
  harbourGeneratorMaker?: string;
  harbourGeneratorMaxPowerKW?: number;
  azimuthEngFwdCount?: number;
  azimuthEngFwdMaxPowerKW?: number;
  
  // Shipowner (Shore Master - Editable)
  shipownerName?: string;
  shipownerStreet?: string;
  shipownerCountry?: string;
  shipownerZip?: string;
  shipownerCity?: string;
  shipownerPhone?: string;
  shipownerFax?: string;
  shipownerEmail?: string;
  shipownerContactPerson?: string;
  
  managingOwnerName?: string;
  managingOwnerEmail?: string;
  managingOwnerContactPerson?: string;
  
  operatorName?: string;
  operatorEmail?: string;
  operatorContactPerson?: string;
  
  csoFirstName?: string;
  csoLastName?: string;
  csoEmail?: string;
  csoPhone24h?: string;
  
  dpaFirstName?: string;
  dpaLastName?: string;
  dpaEmail?: string;
  dpaPhone24h?: string;
  
  // Charterer (Shore Master - Editable)
  chartererName?: string;
  chartererStreet?: string;
  chartererCountry?: string;
  chartererZip?: string;
  chartererCity?: string;
  chartererPhone?: string;
  chartererEmail?: string;
  chartererContactPerson?: string;
  
  bareboatChartererName?: string;
  bareboatChartererEmail?: string;
  bareboatChartererContactPerson?: string;
  
  // Class / Flag State
  classSocietyName?: string;
  classSocietyCountry?: string;
  classSocietyEmail?: string;
  classSocietyContactPerson?: string;
  
  flagStateName?: string;
  flagStateCountry?: string;
  flagStateEmail?: string;
  flagStateContactPerson?: string;
  
  // Insurance (Shore Master - Editable)
  piClubName?: string;
  piClubStreet?: string;
  piClubCountry?: string;
  piClubZip?: string;
  piClubCity?: string;
  piClubPhone?: string;
  piClubEmail?: string;
  piClubContactPerson?: string;
  
  hmClubName?: string;
  hmClubEmail?: string;
  hmClubContactPerson?: string;
  
  // Radio Communication
  inmarsatPhone1?: string;
  inmarsatPhone2?: string;
  inmarsatFax1?: string;
  emailAddress1?: string;
  emailAddress2?: string;
  gsmPhone?: string;
  seaAreaA1?: boolean;
  seaAreaA2?: boolean;
  seaAreaA3?: boolean;
  seaAreaA4?: boolean;
  ais?: boolean;
  navtex?: boolean;
  epirbNumber?: string;
  epirbMaker?: string;
  
  // Tanks & Cargo
  hfoCbm?: number;
  mdoCbm?: number;
  lubOilCbm?: number;
  freshWaterCbm?: number;
  ballastWaterCbm?: number;
  noOfBallastTanks?: number;
  teuTotal?: number;
  teuOnDeck?: number;
  teuUnderDeck?: number;
  grainCbm?: number;
  balesCbm?: number;
  noOfCargoHolds?: number;
  noOfHatches?: number;
  
  // Sync metadata
  lastEdgeSyncAt?: string;
  lastShoreSyncAt?: string;
}

type TabId = 'basic-data' | 'dimensions' | 'machinery' | 'shipowner' | 'charterer' | 'class-flag-state' | 'insurance' | 'radio-comm' | 'tanks-cargo' | 'certificates' | 'crew';

const TABS: { id: TabId; label: string; edgeSource: boolean }[] = [
  { id: 'basic-data', label: 'Basic Data', edgeSource: true },
  { id: 'dimensions', label: 'Dimensions', edgeSource: true },
  { id: 'machinery', label: 'Machinery', edgeSource: true },
  { id: 'shipowner', label: 'Shipowner', edgeSource: false },
  { id: 'charterer', label: 'Charterer', edgeSource: false },
  { id: 'class-flag-state', label: 'Class / Flag State', edgeSource: true },
  { id: 'insurance', label: 'Insurance', edgeSource: false },
  { id: 'radio-comm', label: 'Radio Comm.', edgeSource: true },
  { id: 'tanks-cargo', label: 'Tanks & Cargo', edgeSource: true },
  { id: 'certificates', label: 'Chứng chỉ', edgeSource: false },
  { id: 'crew', label: 'Crew / Thuyền viên', edgeSource: false },
];

// ============================================================
// API Helper
// ============================================================
const BASE = ENV.API_BASE_URL;

async function apiFetch<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json() as Promise<T>;
}

// ============================================================
// Main Component
// ============================================================
export const VesselDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();

  const [activeTab, setActiveTab] = useState<TabId>('basic-data');
  const [vessel, setVessel] = useState<Vessel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [isDirty, setIsDirty] = useState(false);

  const [formData, setFormData] = useState<Partial<Vessel>>({});

  // Load vessel data
  useEffect(() => {
    if (!id) return;
    setLoading(true);
    setError(null);
    apiFetch<Vessel>(`${BASE}/vessels/${id}`)
      .then(v => {
        setVessel(v);
        // Initialize form data with all fields
        setFormData(v);
      })
      .catch(e => setError(e instanceof Error ? e.message : 'Failed to load vessel'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleChange = useCallback((field: keyof Vessel, value: string | number | boolean | Date | null | undefined) => {
    setFormData(prev => ({ ...prev, [field]: value as Vessel[keyof Vessel] }));
    setIsDirty(true);
  }, []);

  const handleSave = async () => {
    if (!id) return;
    setSaving(true);
    try {
      // Only send commercial fields that can be edited on Shore
      const commercialFields = {
        // Shipowner
        shipownerName: formData.shipownerName,
        shipownerStreet: formData.shipownerStreet,
        shipownerCountry: formData.shipownerCountry,
        shipownerZip: formData.shipownerZip,
        shipownerCity: formData.shipownerCity,
        shipownerPhone: formData.shipownerPhone,
        shipownerFax: formData.shipownerFax,
        shipownerEmail: formData.shipownerEmail,
        shipownerContactPerson: formData.shipownerContactPerson,
        managingOwnerName: formData.managingOwnerName,
        managingOwnerEmail: formData.managingOwnerEmail,
        managingOwnerContactPerson: formData.managingOwnerContactPerson,
        operatorName: formData.operatorName,
        operatorEmail: formData.operatorEmail,
        operatorContactPerson: formData.operatorContactPerson,
        csoFirstName: formData.csoFirstName,
        csoLastName: formData.csoLastName,
        csoEmail: formData.csoEmail,
        csoPhone24h: formData.csoPhone24h,
        dpaFirstName: formData.dpaFirstName,
        dpaLastName: formData.dpaLastName,
        dpaEmail: formData.dpaEmail,
        dpaPhone24h: formData.dpaPhone24h,
        // Charterer
        chartererName: formData.chartererName,
        chartererStreet: formData.chartererStreet,
        chartererCountry: formData.chartererCountry,
        chartererZip: formData.chartererZip,
        chartererCity: formData.chartererCity,
        chartererPhone: formData.chartererPhone,
        chartererEmail: formData.chartererEmail,
        chartererContactPerson: formData.chartererContactPerson,
        bareboatChartererName: formData.bareboatChartererName,
        bareboatChartererEmail: formData.bareboatChartererEmail,
        bareboatChartererContactPerson: formData.bareboatChartererContactPerson,
        // Insurance
        piClubName: formData.piClubName,
        piClubStreet: formData.piClubStreet,
        piClubCountry: formData.piClubCountry,
        piClubZip: formData.piClubZip,
        piClubCity: formData.piClubCity,
        piClubPhone: formData.piClubPhone,
        piClubEmail: formData.piClubEmail,
        piClubContactPerson: formData.piClubContactPerson,
        hmClubName: formData.hmClubName,
        hmClubEmail: formData.hmClubEmail,
        hmClubContactPerson: formData.hmClubContactPerson,
      };

      await apiFetch(`${BASE}/vessels/${id}`, {
        method: 'PUT',
        body: JSON.stringify(commercialFields),
      });
      
      setIsDirty(false);
      toast.success('Đã lưu dữ liệu thương mại. Dữ liệu sẽ đồng bộ sang Edge trong lần pull tiếp theo.');
      
      // Reload
      const updated = await apiFetch<Vessel>(`${BASE}/vessels/${id}`);
      setVessel(updated);
      setFormData(updated);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Không thể lưu');
    } finally {
      setSaving(false);
    }
  };

  const renderTabContent = () => {
    if (!vessel) return null;

    switch (activeTab) {
      case 'basic-data':
        return <BasicDataTab vessel={vessel} />;

      case 'dimensions':
        return <DimensionsTab vessel={vessel} />;

      case 'machinery':
        return <MachineryTab vessel={vessel} />;

      case 'shipowner':
        return <ShipownerTab vessel={vessel} formData={formData} onChange={handleChange} />;

      case 'charterer':
        return <ChartererTab vessel={vessel} formData={formData} onChange={handleChange} />;

      case 'class-flag-state':
        return <ClassFlagStateTab vessel={vessel} />;

      case 'insurance':
        return <InsuranceTab vessel={vessel} formData={formData} onChange={handleChange} />;

      case 'radio-comm':
        return <RadioCommTab vessel={vessel} />;

      case 'tanks-cargo':
        return <TanksCargoTab vessel={vessel} />;

      case 'certificates':
        return <VesselCertificateTab vesselId={id!} vesselName={vessel.name || 'Vessel'} />;

      case 'crew':
        return <VesselCrewTab vesselId={id!} vesselName={vessel.name || 'Vessel'} />;

      default:
        return null;
    }
  };

  if (loading) {
    return (
      <div className="vd-loading-screen">
        <Loader2 size={32} className="vd-spin" />
        <span>Loading vessel data...</span>
      </div>
    );
  }

  if (error || !vessel) {
    return (
      <div className="vd-error-screen">
        <AlertCircle size={32} />
        <span>{error ?? 'Vessel not found'}</span>
        <button className="vd-btn" onClick={() => navigate('/vessels')}>
          Back to list
        </button>
      </div>
    );
  }

  const currentTab = TABS.find(t => t.id === activeTab);
  const isEditable = currentTab && !currentTab.edgeSource;

  return (
    <div className="vd-page-new">
      {/* Header */}
      <div className="vd-header-new">
        <button className="vd-back-btn-new" onClick={() => navigate('/vessels')}>
          <ArrowLeft size={18} />
        </button>
        <div className="vd-header-left">
          <Ship className="vd-ship-icon" size={24} />
          <div>
            <h1 className="vd-title-new">{vessel.name}</h1>
            {vessel.imo && (
              <p className="vd-subtitle">IMO: {vessel.imo} • {vessel.callSign}</p>
            )}
          </div>
          {isDirty && (
            <span className="vd-unsaved-badge">
              Unsaved changes
            </span>
          )}
        </div>
        {isEditable && (
          <button
            className="vd-save-btn-new"
            onClick={handleSave}
            disabled={!isDirty || saving}
          >
            {saving ? (
              <>
                <Loader2 size={16} className="vd-spin" />
                <span>Saving...</span>
              </>
            ) : (
              <>
                <Save size={16} />
                <span>Save Commercial Data</span>
              </>
            )}
          </button>
        )}
      </div>

      {/* Tabs */}
      <div className="vd-tabs-new">
        {TABS.map(tab => (
          <button
            key={tab.id}
            className={`vd-tab-btn ${activeTab === tab.id ? 'vd-tab-btn--active' : ''}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
            {tab.edgeSource && <span className="vd-edge-indicator" title="Synced from Edge">⚡</span>}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="vd-content-new">
        {renderTabContent()}
      </div>
    </div>
  );
};
