import React, { useState, useEffect } from 'react';
import { LogbookGrid } from '../../components/common/LogbookGrid';
import { MaritimeInput } from '../../components/common/MaritimeInput';
import { logbookService } from '../../services/logbook.service';
import { CreateOilRecordEntryDto, OilRecordEntryResponseDto } from '../../types/logbook.types';
import { toast } from 'sonner';

const OPERATIONS = [
  { code: 'A', desc: 'Ballasting or cleaning of oil fuel tanks' },
  { code: 'B', desc: 'Discharge of dirty ballast or cleaning water from oil fuel tanks' },
  { code: 'C', desc: 'Collection and disposal of oil residues (sludge)' },
  { code: 'D', desc: 'Non-automatic starting of discharge overboard, transfer or other disposal of bilge water' },
  { code: 'H', desc: 'Bunkering of fuel or bulk lubricating oil' },
];

export const OilRecordPage: React.FC = () => {
  const [entries, setEntries] = useState<OilRecordEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    operationCode: '',
    itemNo: '',
    quantity: '',
    tank: '',
    officerInCharge: ''
  });

  const fetchEntries = async () => {
    try {
      setLoading(true);
      const response = await logbookService.getOilEntries({ page: 1, pageSize: 20 });
      setEntries(response.data);
    } catch (error) {
      console.error(error);
      toast.error('Failed to load oil record entries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEntries();
  }, []);

  const handleNext = () => setStep(step + 1);
  const handleBack = () => setStep(step - 1);

  const handleSave = async () => {
    try {
      const operation = OPERATIONS.find(op => op.code === formData.operationCode);
      const entry: CreateOilRecordEntryDto = {
        entryDate: new Date().toISOString(),
        operationCode: formData.operationCode,
        operationDescription: `${operation?.desc || ''} (Item: ${formData.itemNo})`,
        quantity: Number(formData.quantity),
        quantityUnit: 'm3',
        tankFrom: formData.tank,
        officerInCharge: formData.officerInCharge || 'Chief Engineer', // Default if empty
        remarks: `Item No: ${formData.itemNo}`
      };

      await logbookService.createOilEntry(entry);
      toast.success('Oil Record Entry Saved!');
      fetchEntries();
      setShowForm(false);
      // Reset
      setStep(1);
      setFormData({
        operationCode: '',
        itemNo: '',
        quantity: '',
        tank: '',
        officerInCharge: ''
      });
    } catch (error) {
      console.error(error);
      toast.error('Failed to save entry');
    }
  };

  return (
    <LogbookGrid 
      title="Oil Record Book - Part I (Machinery Space)"
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
        <div className="max-w-2xl mx-auto w-full mb-6">
        {/* Progress Bar */}
        <div className="flex justify-between mb-8">
          {[1, 2, 3].map(s => (
            <div key={s} className={`flex items-center ${s < 3 ? 'flex-1' : ''}`}>
              <div className={`
                w-10 h-10 rounded-full flex items-center justify-center font-mono font-bold border-2
                ${step >= s ? 'bg-industrial-text-amber text-black border-industrial-text-amber' : 'bg-transparent text-gray-500 border-gray-500'}
              `}>
                {s}
              </div>
              {s < 3 && (
                <div className={`h-1 flex-1 mx-2 ${step > s ? 'bg-industrial-text-amber' : 'bg-gray-700'}`} />
              )}
            </div>
          ))}
        </div>

        {/* Step 1: Select Operation */}
        {step === 1 && (
          <div className="bg-industrial-surface p-6 border border-industrial-border">
            <h2 className="text-industrial-text-amber font-mono text-xl mb-4 uppercase">Step 1: Select Operation</h2>
            <div className="flex flex-col gap-2">
              {OPERATIONS.map(op => (
                <button
                  key={op.code}
                  onClick={() => {
                    setFormData({ ...formData, operationCode: op.code });
                    handleNext();
                  }}
                  className="text-left p-4 border border-industrial-border hover:bg-white/5 hover:border-industrial-text-amber transition-colors"
                >
                  <span className="text-industrial-text-amber font-bold font-mono mr-4">Code {op.code}</span>
                  <span className="text-white font-mono">{op.desc}</span>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Step 2: Enter Details */}
        {step === 2 && (
          <div className="bg-industrial-surface p-6 border border-industrial-border">
            <h2 className="text-industrial-text-amber font-mono text-xl mb-4 uppercase">Step 2: Enter Details (Code {formData.operationCode})</h2>
            <div className="flex flex-col gap-4">
              <MaritimeInput 
                label="Item No." 
                placeholder="e.g., 12.1"
                value={formData.itemNo}
                onChange={e => setFormData({ ...formData, itemNo: e.target.value })}
              />
              <MaritimeInput 
                label="Quantity (m3)" 
                type="number"
                value={formData.quantity}
                onChange={e => setFormData({ ...formData, quantity: e.target.value })}
              />
              <MaritimeInput 
                label="Tank / Location" 
                placeholder="e.g., Sludge Tank #1"
                value={formData.tank}
                onChange={e => setFormData({ ...formData, tank: e.target.value })}
              />
              <MaritimeInput 
                label="Officer In Charge" 
                placeholder="Name / Rank"
                value={formData.officerInCharge}
                onChange={e => setFormData({ ...formData, officerInCharge: e.target.value })}
              />
              <div className="flex justify-between mt-4">
                <button onClick={handleBack} className="text-white font-mono underline">Back</button>
                <button 
                  onClick={handleNext}
                  className="bg-industrial-text-amber text-black font-bold py-2 px-6 font-mono hover:bg-yellow-500"
                >
                  Next
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Step 3: Review & Sign */}
        {step === 3 && (
          <div className="bg-industrial-surface p-6 border border-industrial-border">
            <h2 className="text-industrial-text-amber font-mono text-xl mb-4 uppercase">Step 3: Review & Confirm</h2>
            <div className="bg-black/30 p-4 mb-6 border border-gray-700 font-mono text-sm">
              <div className="grid grid-cols-2 gap-2">
                <span className="text-gray-400">Operation Code:</span>
                <span className="text-white">{formData.operationCode}</span>
                
                <span className="text-gray-400">Item No:</span>
                <span className="text-white">{formData.itemNo}</span>
                
                <span className="text-gray-400">Quantity:</span>
                <span className="text-white">{formData.quantity} m3</span>
                
                <span className="text-gray-400">Tank:</span>
                <span className="text-white">{formData.tank}</span>

                <span className="text-gray-400">Officer:</span>
                <span className="text-white">{formData.officerInCharge}</span>
              </div>
            </div>
            
            <div className="flex justify-between mt-4">
              <button onClick={handleBack} className="text-white font-mono underline">Back</button>
              <button 
                onClick={handleSave}
                className="bg-industrial-text-green text-black font-bold py-2 px-6 font-mono hover:bg-green-500"
              >
                CONFIRM & SAVE
              </button>
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
              <th className="p-4 border-b border-industrial-border">Code</th>
              <th className="p-4 border-b border-industrial-border">Operation</th>
              <th className="p-4 border-b border-industrial-border">Quantity</th>
              <th className="p-4 border-b border-industrial-border">Tank</th>
              <th className="p-4 border-b border-industrial-border">Officer</th>
              <th className="p-4 border-b border-industrial-border">Status</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-industrial-text-green font-mono">
                  Loading...
                </td>
              </tr>
            )}
            {!loading && entries.length === 0 && (
              <tr>
                <td colSpan={7} className="p-4 text-center text-gray-500 font-mono">
                  No oil record entries. Click "+ New Entry" to start logging.
                </td>
              </tr>
            )}
            {entries.map(entry => (
              <tr key={entry.id} className="border-b border-industrial-border hover:bg-white/5">
                <td className="p-4 font-mono text-white">{new Date(entry.entryDate).toLocaleDateString()}</td>
                <td className="p-4 font-mono text-industrial-text-amber font-bold">{entry.operationCode}</td>
                <td className="p-4 font-mono text-white text-sm">{entry.operationDescription}</td>
                <td className="p-4 font-mono text-white">{entry.quantity} {entry.quantityUnit}</td>
                <td className="p-4 font-mono text-white text-sm">{entry.tankFrom || '-'}</td>
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
            ))}
          </tbody>
        </table>
      </div>
    </LogbookGrid>
  );
};
