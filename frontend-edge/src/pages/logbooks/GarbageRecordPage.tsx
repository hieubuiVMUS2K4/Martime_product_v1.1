import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { CoordinatePicker } from '../../components/common/CoordinatePicker';
import { toast } from 'sonner';
import { logbookService } from '../../services/logbook.service';
import type { GarbageRecordResponseDto } from '../../types/logbook.types';

// MARPOL Annex V Garbage Categories
const GARBAGE_CATEGORIES = [
  { code: 'A', name: 'Plastics', color: 'red', dischargeAllowed: false },
  { code: 'B', name: 'Food Wastes', color: 'green', dischargeAllowed: true },
  { code: 'C', name: 'Domestic Wastes', color: 'blue', dischargeAllowed: true },
  { code: 'D', name: 'Cooking Oil', color: 'orange', dischargeAllowed: false },
  { code: 'E', name: 'Incinerator Ashes', color: 'gray', dischargeAllowed: true },
  { code: 'F', name: 'Operational Wastes', color: 'yellow', dischargeAllowed: false },
  { code: 'G', name: 'Cargo Residues (non-HME)', color: 'purple', dischargeAllowed: true },
  { code: 'H', name: 'Cargo Residues (HME)', color: 'red', dischargeAllowed: false },
  { code: 'I', name: 'Animal Carcasses', color: 'brown', dischargeAllowed: true },
  { code: 'J', name: 'Fishing Gear', color: 'teal', dischargeAllowed: false },
  { code: 'K', name: 'E-waste', color: 'red', dischargeAllowed: false },
];

const OPERATION_TYPES = [
  { code: '1', name: 'Discharge into the sea' },
  { code: '2', name: 'Discharge to reception facilities' },
  { code: '3', name: 'Incineration' },
  { code: '4', name: 'Accidental or exceptional discharge' },
];

export const GarbageRecordPage: React.FC = () => {
  const [entries, setEntries] = useState<GarbageRecordResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [step, setStep] = useState(1);
  const [selectedCategory, setSelectedCategory] = useState<typeof GARBAGE_CATEGORIES[0] | null>(null);
  const [formData, setFormData] = useState({
    operationType: '',
    quantity: '',
    unit: 'm³',
    latitude: 0,
    longitude: 0,
    portName: '',
    receptionFacility: '',
    officerInCharge: '',
    remarks: ''
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getGarbageEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error('Failed to load garbage record entries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleCategorySelect = (category: typeof GARBAGE_CATEGORIES[0]) => {
    setSelectedCategory(category);
    
    // Auto-restrict discharge to sea for prohibited categories
    if (!category.dischargeAllowed && formData.operationType === '1') {
      setFormData({ ...formData, operationType: '' });
      toast.warning(`${category.name} cannot be discharged to sea (MARPOL Annex V)`);
    }
    
    setStep(2);
  };

  const handleOperationSelect = (opCode: string) => {
    // Validate discharge to sea for restricted categories
    if (opCode === '1' && selectedCategory && !selectedCategory.dischargeAllowed) {
      toast.error(`Cannot discharge ${selectedCategory.name} to sea - MARPOL Violation!`);
      return;
    }
    
    setFormData({ ...formData, operationType: opCode });
    setStep(3);
  };

  const handleSave = async () => {
    // Validation
    if (!formData.quantity || parseFloat(formData.quantity) <= 0) {
      toast.error('Please enter quantity');
      return;
    }
    
    if (formData.operationType === '1' && (!formData.latitude || !formData.longitude)) {
      toast.error('Position is required for discharge to sea');
      return;
    }
    
    if (formData.operationType === '2' && !formData.portName) {
      toast.error('Port name is required for discharge to facility');
      return;
    }
    
    try {
      const entry = {
        operationDateTime: new Date().toISOString(),
        operationCode: formData.operationType,
        garbageCategory: selectedCategory?.code || '',
        description: selectedCategory?.name || '',
        quantity: parseFloat(formData.quantity),
        quantityUnit: formData.unit,
        latitude: formData.latitude,
        longitude: formData.longitude,
        portName: formData.portName,
        receptionFacility: formData.receptionFacility,
        officerInCharge: formData.officerInCharge || 'Chief Officer',
        remarks: formData.remarks
      };
      
      await logbookService.createGarbageEntry(entry);
      toast.success('Garbage Record Entry Saved!');
      fetchEntries();
      setShowForm(false);
      
      // Reset
      setStep(1);
      setSelectedCategory(null);
      setFormData({
        operationType: '',
        quantity: '',
        unit: 'm³',
        latitude: 0,
        longitude: 0,
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
      title="Garbage Record Book - MARPOL Annex V"
      actions={
        <button
          onClick={() => setShowForm(!showForm)}
          className="bg-blue-600 text-white font-semibold py-2.5 px-6 rounded-lg shadow-md hover:bg-blue-700 "
        >
          {showForm ? 'Cancel' : '+ New Entry'}
        </button>
      }
    >
      {showForm && (
      <div className="max-w-4xl mx-auto w-full mb-6">
        {/* Progress Steps */}
        <div className="flex justify-between mb-8">
          {[1, 2, 3].map(s => (
            <div key={s} className={`flex items-center ${s < 3 ? 'flex-1' : ''}`}>
              <div className={`
                w-12 h-12 rounded-full flex items-center justify-center font-sans font-bold border-2
                ${step >= s ? 'bg-blue-600 text-white border-blue-500' : 'bg-transparent text-gray-500 border-gray-500'}
              `}>
                {s}
              </div>
              {s < 3 && (
                <div className={`h-1 flex-1 mx-2 ${step > s ? 'bg-blue-600' : 'bg-gray-700'}`} />
              )}
            </div>
          ))}
        </div>

        {/* Step 1: Select Garbage Category */}
        {step === 1 && (
          <div className="bg-white p-6 border border-gray-200 rounded-lg shadow-sm">
            <h2 className="text-blue-600 font-sans text-xl font-bold mb-6">
              Step 1: Select Garbage Category
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              {GARBAGE_CATEGORIES.map(cat => (
                <button
                  key={cat.code}
                  onClick={() => handleCategorySelect(cat)}
                  className="p-4 border-2 border-gray-200 hover:border-blue-500 transition-colors text-left relative group"
                >
                  <div className="flex items-start justify-between">
                    <div>
                      <div className="text-blue-600 font-bold font-sans text-2xl mb-1">
                        {cat.code}
                      </div>
                      <div className="text-gray-900 font-sans text-sm">{cat.name}</div>
                    </div>
                    {!cat.dischargeAllowed && (
                      <div className="text-red-500 text-xs font-sans border border-red-500 px-1 py-0.5">
                        NO SEA
                      </div>
                    )}
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Step 2: Select Operation Type */}
        {step === 2 && selectedCategory && (
          <div className="bg-white p-6 border border-gray-200 rounded-lg shadow-sm">
            <h2 className="text-blue-600 font-sans text-xl font-bold mb-4">
              Step 2: Select Operation Type
            </h2>
            <div className="bg-gray-50/30 p-4 mb-6 border border-gray-200">
              <span className="text-gray-400 font-sans text-sm">Selected Category: </span>
              <span className="text-blue-600 font-sans font-bold text-lg">
                {selectedCategory.code} - {selectedCategory.name}
              </span>
            </div>
            <div className="flex flex-col gap-3">
              {OPERATION_TYPES.map(op => {
                const isDisabled = op.code === '1' && !selectedCategory.dischargeAllowed;
                return (
                  <button
                    key={op.code}
                    onClick={() => handleOperationSelect(op.code)}
                    disabled={isDisabled}
                    className={`
                      text-left p-4 border border-gray-200 transition-colors
                      ${isDisabled 
                        ? 'opacity-30 cursor-not-allowed' 
                        : 'hover:bg-gray-50 hover:border-blue-500'
                      }
                    `}
                  >
                    <span className="text-blue-600 font-bold font-sans mr-4">
                      Code {op.code}
                    </span>
                    <span className="text-gray-900 font-sans">{op.name}</span>
                    {isDisabled && (
                      <span className="ml-4 text-red-500 text-xs font-sans">(PROHIBITED BY MARPOL)</span>
                    )}
                  </button>
                );
              })}
            </div>
            <button 
              onClick={() => setStep(1)} 
              className="mt-6 text-gray-900 font-sans underline hover:text-blue-600"
            >
              ← Back to Categories
            </button>
          </div>
        )}

        {/* Step 3: Enter Details & Save */}
        {step === 3 && selectedCategory && (
          <div className="bg-white p-6 border border-gray-200 rounded-lg shadow-sm">
            <h2 className="text-blue-600 font-sans text-xl font-bold mb-6">
              Step 3: Enter Details
            </h2>
            
            <div className="bg-gray-50/30 p-4 mb-6 border border-gray-200 flex justify-between">
              <div>
                <span className="text-gray-400 font-sans text-sm">Category: </span>
                <span className="text-gray-900 font-sans font-bold">{selectedCategory.code} - {selectedCategory.name}</span>
              </div>
              <div>
                <span className="text-gray-400 font-sans text-sm">Operation: </span>
                <span className="text-gray-900 font-sans font-bold">
                  {OPERATION_TYPES.find(o => o.code === formData.operationType)?.name}
                </span>
              </div>
            </div>

            <div className="flex flex-col gap-4">
              {/* Quantity */}
              <div className="grid grid-cols-3 gap-4">
                <div className="col-span-2">
                  <MaritimeInput
                    label="Estimated Quantity"
                    type="number"
                    step="0.01"
                    value={formData.quantity}
                    onChange={e => setFormData({ ...formData, quantity: e.target.value })}
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="text-blue-600 font-sans text-sm  block mb-2">
                    Unit
                  </label>
                  <select
                    value={formData.unit}
                    onChange={e => setFormData({ ...formData, unit: e.target.value })}
                    className="w-full bg-white border-2 border-gray-200 text-gray-900 font-sans text-lg p-4 focus:border-blue-500 focus:outline-none"
                  >
                    <option value="m³">m³</option>
                    <option value="kg">kg</option>
                    <option value="liters">liters</option>
                  </select>
                </div>
              </div>

              {/* Position (for discharge to sea) */}
              {formData.operationType === '1' && (
                <div className="border border-yellow-600 bg-yellow-900/10 p-4">
                  <div className="text-yellow-500 font-sans text-sm font-semibold mb-4">
                    ⚠ Position Required for Discharge to Sea
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <CoordinatePicker
                      label="Latitude"
                      type="latitude"
                      value={formData.latitude}
                      onChange={lat => setFormData({ ...formData, latitude: lat })}
                    />
                    <CoordinatePicker
                      label="Longitude"
                      type="longitude"
                      value={formData.longitude}
                      onChange={lon => setFormData({ ...formData, longitude: lon })}
                    />
                  </div>
                </div>
              )}

              {/* Port & Facility (for discharge to facility) */}
              {formData.operationType === '2' && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border border-blue-600 bg-blue-900/10 p-4">
                  <MaritimeInput
                    label="Port Name"
                    value={formData.portName}
                    onChange={e => setFormData({ ...formData, portName: e.target.value })}
                    placeholder="e.g., Port of Singapore"
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
                <label className="text-blue-600 font-sans text-sm  block mb-2">
                  Remarks (Optional)
                </label>
                <textarea
                  value={formData.remarks}
                  onChange={e => setFormData({ ...formData, remarks: e.target.value })}
                  className="w-full bg-white border-2 border-gray-200 text-gray-900 font-sans p-4 focus:border-blue-500 focus:outline-none h-24 resize-none"
                  placeholder="Additional notes..."
                />
              </div>

              {/* Actions */}
              <div className="flex justify-between mt-6">
                <button 
                  onClick={() => setStep(2)}
                  className="text-gray-900 font-sans underline hover:text-blue-600"
                >
                  ← Back
                </button>
                <button
                  onClick={handleSave}
                  className="bg-green-600 text-white font-semibold py-2.5 px-8 rounded-lg shadow-md hover:bg-green-700 "
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
      <div className="bg-white border border-gray-200 overflow-x-auto mt-6">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-50 text-blue-600 font-sans text-sm font-semibold">
              <th className="p-4 border-b border-gray-200">Date</th>
              <th className="p-4 border-b border-gray-200">Operation</th>
              <th className="p-4 border-b border-gray-200">Category</th>
              <th className="p-4 border-b border-gray-200">Quantity</th>
              <th className="p-4 border-b border-gray-200">Location</th>
              <th className="p-4 border-b border-gray-200">Officer</th>
              <th className="p-4 border-b border-gray-200">Status</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-green-600 font-sans">
                  Loading...
                </td>
              </tr>
            )}
            {!loading && entries.length === 0 && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-gray-500 font-sans">
                  No garbage records. Click "+ New Entry" to start logging.
                </td>
              </tr>
            )}
            {entries.map(entry => {
              const operation = OPERATION_TYPES.find(op => op.code === entry.operationCode);
              const category = GARBAGE_CATEGORIES.find(cat => cat.code === entry.garbageCategory);
              return (
                <tr key={entry.id} className="border-b border-gray-200 hover:bg-gray-50">
                  <td className="p-4 font-sans text-gray-900">{new Date(entry.operationDateTime).toLocaleDateString()}</td>
                  <td className="p-4 font-sans text-gray-900 text-sm">{operation?.name || entry.operationCode}</td>
                  <td className="p-4 font-sans text-gray-900">
                    <span className={`px-2 py-1 text-xs bg-${category?.color}-600`}>
                      {entry.garbageCategory}: {entry.description}
                    </span>
                  </td>
                  <td className="p-4 font-sans text-gray-900">{entry.quantity} {entry.quantityUnit}</td>
                  <td className="p-4 font-sans text-gray-900 text-xs">
                    {entry.portName || `${entry.latitude?.toFixed(2)}°, ${entry.longitude?.toFixed(2)}°`}
                  </td>
                  <td className="p-4 font-sans text-gray-900 text-sm">{entry.officerInCharge}</td>
                  <td className="p-4">
                    {entry.masterSignature ? (
                      <span className="bg-green-600 text-white text-xs px-2 py-1 font-sans font-bold">
                        SIGNED
                      </span>
                    ) : (
                      <span className="bg-yellow-600 text-black text-xs px-2 py-1 font-sans font-bold">
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






