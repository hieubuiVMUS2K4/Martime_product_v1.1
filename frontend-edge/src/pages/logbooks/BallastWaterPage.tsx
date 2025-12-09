import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { CoordinatePicker } from '../../components/common/CoordinatePicker';
import { toast } from 'sonner';
import { logbookService } from '../../services/logbook.service';
import type { BallastWaterRecordResponseDto } from '../../types/logbook.types';

// BWM Convention Operation Codes
const BWM_OPERATIONS = [
  { code: '1', name: 'Ballast water uptake', requiresTreatment: false },
  { code: '2', name: 'Ballast water circulation/exchange at sea', requiresTreatment: false },
  { code: '3', name: 'Ballast water exchange - sequential method', requiresTreatment: false },
  { code: '4', name: 'Ballast water exchange - flow-through method', requiresTreatment: false },
  { code: '5', name: 'Ballast water discharge at sea', requiresTreatment: true },
  { code: '6', name: 'Ballast water discharge to reception facility', requiresTreatment: false },
  { code: '7', name: 'Accidental/exceptional uptake or discharge', requiresTreatment: false },
  { code: '8', name: 'Ballast water management (D-2 treatment)', requiresTreatment: true },
  { code: '9', name: 'Discharge of sediment', requiresTreatment: false },
];

const EXCHANGE_METHODS = [
  { value: 'SEQUENTIAL', label: 'Sequential (Empty-Refill)' },
  { value: 'FLOW_THROUGH', label: 'Flow-Through (Continuous)' },
];

const TREATMENT_SYSTEMS = [
  { value: 'UV', label: 'UV Disinfection' },
  { value: 'ELECTROLYSIS', label: 'Electrolysis (Chlorination)' },
  { value: 'FILTRATION', label: 'Filtration + UV' },
  { value: 'OZONE', label: 'Ozone Treatment' },
];

export const BallastWaterPage: React.FC = () => {
  const [entries, setEntries] = useState<BallastWaterRecordResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [step, setStep] = useState(1);
  const [selectedOperation, setSelectedOperation] = useState<typeof BWM_OPERATIONS[0] | null>(null);
  const [formData, setFormData] = useState({
    ballastTank: '',
    volume: '',
    startLat: 0,
    startLon: 0,
    endLat: 0,
    endLon: 0,
    waterDepth: '',
    distanceFromLand: '',
    exchangeMethod: '',
    exchangeVolumePercent: '',
    treatmentSystemUsed: false,
    treatmentSystemType: '',
    treatmentSuccessful: true,
    salinityBefore: '',
    salinityAfter: '',
    portName: '',
    receptionFacility: '',
    officerInCharge: '',
    remarks: ''
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getBallastWaterEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error('Failed to load ballast water entries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleOperationSelect = (op: typeof BWM_OPERATIONS[0]) => {
    setSelectedOperation(op);
    setStep(2);
  };

  const handleSave = async () => {
    // Validation
    if (!formData.ballastTank || !formData.volume) {
      toast.error('Tank and volume are required');
      return;
    }

    // D-2 Treatment validation
    if (selectedOperation?.code === '5' || selectedOperation?.code === '8') {
      if (!formData.treatmentSystemUsed) {
        toast.error('D-2 treatment system required for discharge at sea (BWM Convention)');
        return;
      }
      if (!formData.treatmentSystemType) {
        toast.error('Please select treatment system type');
        return;
      }
    }

    // Exchange validation (codes 2, 3, 4)
    if (['2', '3', '4'].includes(selectedOperation?.code || '')) {
      if (!formData.exchangeMethod) {
        toast.error('Please select exchange method');
        return;
      }
      if (!formData.distanceFromLand || parseFloat(formData.distanceFromLand) < 200) {
        toast.warning('D-1 Exchange should be performed at least 200 NM from land');
      }
      if (!formData.waterDepth || parseFloat(formData.waterDepth) < 200) {
        toast.warning('D-1 Exchange should be performed in water deeper than 200 meters');
      }
    }

    try {
      const entry = {
        operationDateTime: new Date().toISOString(),
        operationCode: selectedOperation?.code || '',
        operationDescription: selectedOperation?.name || '',
        ballastTank: formData.ballastTank,
        volume: parseFloat(formData.volume),
        startLatitude: formData.startLat,
        startLongitude: formData.startLon,
        endLatitude: formData.endLat,
        endLongitude: formData.endLon,
        waterDepth: formData.waterDepth ? parseFloat(formData.waterDepth) : undefined,
        distanceFromLand: formData.distanceFromLand ? parseFloat(formData.distanceFromLand) : undefined,
        exchangeMethod: formData.exchangeMethod || undefined,
        exchangeVolumePercent: formData.exchangeVolumePercent ? parseFloat(formData.exchangeVolumePercent) : undefined,
        treatmentSystemUsed: formData.treatmentSystemUsed,
        treatmentSystemType: formData.treatmentSystemType || undefined,
        treatmentSuccessful: formData.treatmentSuccessful,
        salinityBefore: formData.salinityBefore ? parseFloat(formData.salinityBefore) : undefined,
        salinityAfter: formData.salinityAfter ? parseFloat(formData.salinityAfter) : undefined,
        portName: formData.portName || undefined,
        receptionFacility: formData.receptionFacility || undefined,
        officerInCharge: formData.officerInCharge || 'Chief Officer',
        remarks: formData.remarks || undefined
      };

      await logbookService.createBallastWaterEntry(entry);
      toast.success('Ballast Water Record Entry Saved!');
      fetchEntries();
      setShowForm(false);
      
      // Reset
      setStep(1);
      setSelectedOperation(null);
      setFormData({
        ballastTank: '',
        volume: '',
        startLat: 0,
        startLon: 0,
        endLat: 0,
        endLon: 0,
        waterDepth: '',
        distanceFromLand: '',
        exchangeMethod: '',
        exchangeVolumePercent: '',
        treatmentSystemUsed: false,
        treatmentSystemType: '',
        treatmentSuccessful: true,
        salinityBefore: '',
        salinityAfter: '',
        portName: '',
        receptionFacility: '',
        officerInCharge: '',
        remarks: ''
      });
    } catch (error) {
      console.error(error);
      toast.error('Failed to save entry');
    }
  };

  return (
    <LogbookGrid 
      title="Ballast Water Record Book - BWM Convention"
      actions={
        <button
          onClick={() => setShowForm(!showForm)}
          className="bg-industrial-text-amber text-black font-bold py-2 px-6 font-mono hover:bg-yellow-500 uppercase tracking-wider"
        >
          {showForm ? 'Cancel' : '+ New Entry'}
        </button>
      }
    >
      {showForm && (
      <div className="max-w-5xl mx-auto w-full mb-6">
        {/* Progress */}
        <div className="flex justify-between mb-8">
          {[1, 2].map(s => (
            <div key={s} className={`flex items-center ${s < 2 ? 'flex-1' : ''}`}>
              <div className={`
                w-12 h-12 rounded-full flex items-center justify-center font-mono font-bold border-2
                ${step >= s ? 'bg-industrial-text-amber text-black border-industrial-text-amber' : 'bg-transparent text-gray-500 border-gray-500'}
              `}>
                {s}
              </div>
              {s < 2 && (
                <div className={`h-1 flex-1 mx-2 ${step > s ? 'bg-industrial-text-amber' : 'bg-gray-700'}`} />
              )}
            </div>
          ))}
        </div>

        {/* Step 1: Select Operation */}
        {step === 1 && (
          <div className="bg-industrial-surface p-6 border border-industrial-border">
            <h2 className="text-industrial-text-amber font-mono text-xl mb-6 uppercase">
              Step 1: Select BWM Operation
            </h2>
            <div className="flex flex-col gap-3">
              {BWM_OPERATIONS.map(op => (
                <button
                  key={op.code}
                  onClick={() => handleOperationSelect(op)}
                  className="text-left p-4 border border-industrial-border hover:bg-white/5 hover:border-industrial-text-amber transition-colors"
                >
                  <div className="flex justify-between items-center">
                    <div>
                      <span className="text-industrial-text-amber font-bold font-mono mr-4">
                        Code {op.code}
                      </span>
                      <span className="text-white font-mono">{op.name}</span>
                    </div>
                    {op.requiresTreatment && (
                      <span className="bg-blue-600 text-white text-xs px-2 py-1 font-mono rounded">
                        D-2 REQUIRED
                      </span>
                    )}
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Step 2: Enter Details */}
        {step === 2 && selectedOperation && (
          <div className="bg-industrial-surface p-6 border border-industrial-border">
            <h2 className="text-industrial-text-amber font-mono text-xl mb-4 uppercase">
              Step 2: Ballast Operation Details
            </h2>
            
            <div className="bg-black/30 p-4 mb-6 border border-gray-700">
              <span className="text-gray-400 font-mono text-sm">Operation: </span>
              <span className="text-white font-mono font-bold">
                Code {selectedOperation.code} - {selectedOperation.name}
              </span>
            </div>

            <div className="flex flex-col gap-6">
              {/* Tank & Volume */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <MaritimeInput
                  label="Ballast Tank ID"
                  value={formData.ballastTank}
                  onChange={e => setFormData({ ...formData, ballastTank: e.target.value })}
                  placeholder="e.g., No. 1 Port"
                />
                <MaritimeInput
                  label="Volume (m³)"
                  type="number"
                  step="0.1"
                  value={formData.volume}
                  onChange={e => setFormData({ ...formData, volume: e.target.value })}
                  placeholder="0.0"
                />
              </div>

              {/* Position at Start */}
              <div className="border border-blue-600 bg-blue-900/10 p-4">
                <div className="text-blue-400 font-mono text-sm mb-4 uppercase">Start Position</div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <CoordinatePicker
                    label="Start Latitude"
                    type="latitude"
                    value={formData.startLat}
                    onChange={lat => setFormData({ ...formData, startLat: lat })}
                  />
                  <CoordinatePicker
                    label="Start Longitude"
                    type="longitude"
                    value={formData.startLon}
                    onChange={lon => setFormData({ ...formData, startLon: lon })}
                  />
                </div>
              </div>

              {/* Exchange-specific fields (codes 2, 3, 4) */}
              {['2', '3', '4'].includes(selectedOperation.code) && (
                <div className="border border-green-600 bg-green-900/10 p-4">
                  <div className="text-green-400 font-mono text-sm mb-4 uppercase">D-1 Exchange Parameters</div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                        Exchange Method
                      </label>
                      <select
                        value={formData.exchangeMethod}
                        onChange={e => setFormData({ ...formData, exchangeMethod: e.target.value })}
                        className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                      >
                        <option value="">Select Method</option>
                        {EXCHANGE_METHODS.map(m => (
                          <option key={m.value} value={m.value}>{m.label}</option>
                        ))}
                      </select>
                    </div>
                    <MaritimeInput
                      label="Exchange Volume (%)"
                      type="number"
                      min="0"
                      max="100"
                      value={formData.exchangeVolumePercent}
                      onChange={e => setFormData({ ...formData, exchangeVolumePercent: e.target.value })}
                      placeholder="95%"
                    />
                    <MaritimeInput
                      label="Water Depth (m)"
                      type="number"
                      value={formData.waterDepth}
                      onChange={e => setFormData({ ...formData, waterDepth: e.target.value })}
                      placeholder="Min 200m recommended"
                    />
                    <MaritimeInput
                      label="Distance from Land (NM)"
                      type="number"
                      value={formData.distanceFromLand}
                      onChange={e => setFormData({ ...formData, distanceFromLand: e.target.value })}
                      placeholder="Min 200 NM"
                    />
                    <MaritimeInput
                      label="Salinity Before (PPT)"
                      type="number"
                      step="0.1"
                      value={formData.salinityBefore}
                      onChange={e => setFormData({ ...formData, salinityBefore: e.target.value })}
                      placeholder="e.g., 15.0"
                    />
                    <MaritimeInput
                      label="Salinity After (PPT)"
                      type="number"
                      step="0.1"
                      value={formData.salinityAfter}
                      onChange={e => setFormData({ ...formData, salinityAfter: e.target.value })}
                      placeholder="e.g., 32.5"
                    />
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                    <CoordinatePicker
                      label="End Latitude"
                      type="latitude"
                      value={formData.endLat}
                      onChange={lat => setFormData({ ...formData, endLat: lat })}
                    />
                    <CoordinatePicker
                      label="End Longitude"
                      type="longitude"
                      value={formData.endLon}
                      onChange={lon => setFormData({ ...formData, endLon: lon })}
                    />
                  </div>
                </div>
              )}

              {/* D-2 Treatment System (codes 5, 8) */}
              {(selectedOperation.code === '5' || selectedOperation.code === '8') && (
                <div className="border border-purple-600 bg-purple-900/10 p-4">
                  <div className="text-purple-400 font-mono text-sm mb-4 uppercase flex items-center gap-2">
                    D-2 Treatment System (MANDATORY)
                    <span className="bg-red-600 text-white text-xs px-2 py-1 rounded">REQUIRED</span>
                  </div>
                  <div className="flex items-center gap-4 mb-4">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.treatmentSystemUsed}
                        onChange={e => setFormData({ ...formData, treatmentSystemUsed: e.target.checked })}
                        className="w-6 h-6"
                      />
                      <span className="text-white font-mono">Treatment System Used</span>
                    </label>
                  </div>
                  {formData.treatmentSystemUsed && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                          Treatment System Type
                        </label>
                        <select
                          value={formData.treatmentSystemType}
                          onChange={e => setFormData({ ...formData, treatmentSystemType: e.target.value })}
                          className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                        >
                          <option value="">Select System</option>
                          {TREATMENT_SYSTEMS.map(t => (
                            <option key={t.value} value={t.value}>{t.label}</option>
                          ))}
                        </select>
                      </div>
                      <div>
                        <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                          Treatment Result
                        </label>
                        <select
                          value={formData.treatmentSuccessful ? 'SUCCESS' : 'FAILURE'}
                          onChange={e => setFormData({ ...formData, treatmentSuccessful: e.target.value === 'SUCCESS' })}
                          className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono text-lg p-4 focus:border-industrial-text-amber focus:outline-none"
                        >
                          <option value="SUCCESS">Successful</option>
                          <option value="FAILURE">Failed / Partial</option>
                        </select>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* Port/Facility (code 6) */}
              {selectedOperation.code === '6' && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <MaritimeInput
                    label="Port Name"
                    value={formData.portName}
                    onChange={e => setFormData({ ...formData, portName: e.target.value })}
                    placeholder="Port of discharge"
                  />
                  <MaritimeInput
                    label="Reception Facility"
                    value={formData.receptionFacility}
                    onChange={e => setFormData({ ...formData, receptionFacility: e.target.value })}
                    placeholder="Facility name"
                  />
                </div>
              )}

              {/* Officer & Remarks */}
              <MaritimeInput
                label="Officer In Charge"
                value={formData.officerInCharge}
                onChange={e => setFormData({ ...formData, officerInCharge: e.target.value })}
                placeholder="Name / Rank"
              />
              
              <div>
                <label className="text-industrial-text-amber font-mono text-sm uppercase tracking-wider block mb-2">
                  Remarks
                </label>
                <textarea
                  value={formData.remarks}
                  onChange={e => setFormData({ ...formData, remarks: e.target.value })}
                  className="w-full bg-industrial-surface border-2 border-industrial-border text-white font-mono p-4 focus:border-industrial-text-amber focus:outline-none h-24 resize-none"
                  placeholder="Additional notes..."
                />
              </div>

              {/* Actions */}
              <div className="flex justify-between mt-6">
                <button 
                  onClick={() => setStep(1)}
                  className="text-white font-mono underline hover:text-industrial-text-amber"
                >
                  ← Back
                </button>
                <button
                  onClick={handleSave}
                  className="bg-industrial-text-green text-black font-bold py-3 px-8 font-mono hover:bg-green-500 uppercase tracking-wider"
                >
                  Save Entry
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
      )}

      {/* Entries Table */}
      <div className="bg-industrial-surface border border-industrial-border overflow-x-auto mt-6">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-black text-industrial-text-amber font-mono text-sm uppercase">
              <th className="p-4 border-b border-industrial-border">Date</th>
              <th className="p-4 border-b border-industrial-border">Operation</th>
              <th className="p-4 border-b border-industrial-border">Tank</th>
              <th className="p-4 border-b border-industrial-border">Volume (m³)</th>
              <th className="p-4 border-b border-industrial-border">Position</th>
              <th className="p-4 border-b border-industrial-border">Treatment</th>
              <th className="p-4 border-b border-industrial-border">Officer</th>
              <th className="p-4 border-b border-industrial-border">Status</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={8} className="p-4 text-center text-industrial-text-green font-mono">
                  Loading...
                </td>
              </tr>
            )}
            {!loading && entries.length === 0 && (
              <tr>
                <td colSpan={8} className="p-4 text-center text-gray-500 font-mono">
                  No ballast water records. Click "+ New Entry" to start logging.
                </td>
              </tr>
            )}
            {entries.map(entry => {
              const operation = BWM_OPERATIONS.find(op => op.code === entry.operationCode);
              return (
                <tr key={entry.id} className="border-b border-industrial-border hover:bg-white/5">
                  <td className="p-4 font-mono text-white">{new Date(entry.operationDateTime).toLocaleDateString()}</td>
                  <td className="p-4 font-mono text-white text-sm">
                    <span className="bg-blue-600 px-2 py-1 text-xs">
                      {entry.operationCode}
                    </span>
                    <div className="text-xs text-gray-400 mt-1">{operation?.name}</div>
                  </td>
                  <td className="p-4 font-mono text-white">{entry.ballastTank}</td>
                  <td className="p-4 font-mono text-white">{entry.volume.toFixed(2)}</td>
                  <td className="p-4 font-mono text-white text-xs">
                    {entry.portName || `${entry.startLatitude.toFixed(2)}°, ${entry.startLongitude.toFixed(2)}°`}
                  </td>
                  <td className="p-4 font-mono text-white text-xs">
                    {entry.treatmentSystemUsed ? (
                      <span className="bg-green-600 px-2 py-1 text-xs">
                        {entry.treatmentSystemType}
                      </span>
                    ) : (
                      <span className="text-gray-500">No treatment</span>
                    )}
                  </td>
                  <td className="p-4 font-mono text-white text-sm">{entry.officerInCharge}</td>
                  <td className="p-4">
                    {entry.masterSignature ? (
                      <span className="bg-industrial-text-green text-black text-xs px-2 py-1 font-mono font-bold">
                        SIGNED
                      </span>
                    ) : (
                      <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-mono font-bold">
                        DRAFT
                      </span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </LogbookGrid>
  );
};
