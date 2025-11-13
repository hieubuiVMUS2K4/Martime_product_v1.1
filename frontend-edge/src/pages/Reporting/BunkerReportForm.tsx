/**
 * Bunker Report Form
 * MARPOL Annex VI Compliant - Fuel Bunkering Report
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Fuel, AlertTriangle, Save, Send } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreateBunkerReportDto } from '../../types/reporting.types';

export function BunkerReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [marpol, setMarpol] = useState<string[]>([]);

  const [formData, setFormData] = useState<CreateBunkerReportDto>({
    bunkerDate: new Date().toISOString().split('T')[0],
    portName: '',
    supplierName: '',
    fuelType: 'MARINE_FUEL_OIL',
    quantityReceived: 0,
    sampleSealed: false
  });

  const handleChange = (field: keyof CreateBunkerReportDto, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // MARPOL VI validation
    if (field === 'sulphurContent' && typeof value === 'number') {
      const warnings: string[] = [];
      if (value > 0.5) {
        warnings.push('⚠️ MARPOL VI: Global sulphur limit is 0.50% m/m');
      }
      if (value > 0.1) {
        warnings.push('⚠️ SECA: Emission Control Area limit is 0.10% m/m');
      }
      setMarpol(warnings);
    }
  };

  const validateForm = (): boolean => {
    if (!formData.portName?.trim()) {
      setError('Port name is required');
      return false;
    }
    if (!formData.supplierName?.trim()) {
      setError('Supplier name is required');
      return false;
    }
    if (formData.quantityReceived <= 0) {
      setError('Quantity received must be greater than 0');
      return false;
    }
    if (formData.sulphurContent && formData.sulphurContent > 3.5) {
      setError('Sulphur content exceeds maximum allowed (3.5%)');
      return false;
    }
    setError(null);
    return true;
  };

  const handleSubmit = async (asDraft: boolean = false) => {
    if (!asDraft && !validateForm()) {
      return;
    }

    try {
      setLoading(true);
      const report = await ReportingService.createBunkerReport(formData);
      
      if (!asDraft) {
        await ReportingService.submitReport(report.reportId);
      }
      
      navigate('/reporting/reports');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-5xl mx-auto">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
          <Fuel className="h-8 w-8 text-blue-600" />
          Bunker Report
        </h1>
        <p className="text-gray-600 mt-2">
          MARPOL Annex VI Compliant Fuel Bunkering Report
        </p>
      </div>

      {/* MARPOL Warnings */}
      {marpol.length > 0 && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5" />
            <div>
              <h3 className="font-semibold text-amber-900 mb-2">MARPOL VI Compliance Warning</h3>
              <ul className="space-y-1 text-sm text-amber-800">
                {marpol.map((warning, idx) => (
                  <li key={idx}>{warning}</li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
          <p className="font-semibold">Error</p>
          <p className="text-sm">{error}</p>
        </div>
      )}

      <form className="space-y-6">
        {/* Bunker Information */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Bunker Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Bunker Date <span className="text-red-600">*</span>
              </label>
              <input
                type="date"
                value={formData.bunkerDate}
                onChange={(e) => handleChange('bunkerDate', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Port Name <span className="text-red-600">*</span>
              </label>
              <input
                type="text"
                value={formData.portName}
                onChange={(e) => handleChange('portName', e.target.value)}
                placeholder="e.g., Singapore"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Port Code
              </label>
              <input
                type="text"
                value={formData.portCode || ''}
                onChange={(e) => handleChange('portCode', e.target.value)}
                placeholder="e.g., SGSIN"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                maxLength={5}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Supplier Name <span className="text-red-600">*</span>
              </label>
              <input
                type="text"
                value={formData.supplierName}
                onChange={(e) => handleChange('supplierName', e.target.value)}
                placeholder="Fuel supplier company"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                BDN Number
              </label>
              <input
                type="text"
                value={formData.bdnNumber || ''}
                onChange={(e) => handleChange('bdnNumber', e.target.value)}
                placeholder="Bunker Delivery Note number"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
              <p className="text-xs text-gray-500 mt-1">Bunker Delivery Note (BDN)</p>
            </div>
          </div>
        </div>

        {/* Fuel Specifications */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Fuel Specifications</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Fuel Type <span className="text-red-600">*</span>
              </label>
              <select
                value={formData.fuelType}
                onChange={(e) => handleChange('fuelType', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="MARINE_FUEL_OIL">Marine Fuel Oil (MFO)</option>
                <option value="MARINE_DIESEL_OIL">Marine Diesel Oil (MDO)</option>
                <option value="MARINE_GAS_OIL">Marine Gas Oil (MGO)</option>
                <option value="HEAVY_FUEL_OIL">Heavy Fuel Oil (HFO)</option>
                <option value="LOW_SULPHUR_FUEL_OIL">Low Sulphur Fuel Oil (LSFO)</option>
                <option value="ULTRA_LOW_SULPHUR_FUEL_OIL">Ultra Low Sulphur Fuel Oil (ULSFO)</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Fuel Grade
              </label>
              <input
                type="text"
                value={formData.fuelGrade || ''}
                onChange={(e) => handleChange('fuelGrade', e.target.value)}
                placeholder="e.g., ISO-F-RMG 380"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Quantity Received (MT) <span className="text-red-600">*</span>
              </label>
              <input
                type="number"
                step="0.001"
                value={formData.quantityReceived}
                onChange={(e) => handleChange('quantityReceived', parseFloat(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
                min="0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Sulphur Content (% m/m) <span className="text-red-600">*</span>
              </label>
              <input
                type="number"
                step="0.01"
                value={formData.sulphurContent || ''}
                onChange={(e) => handleChange('sulphurContent', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                placeholder="0.50"
                max="3.5"
              />
              <p className="text-xs text-gray-500 mt-1">
                Global limit: 0.50% | SECA: 0.10%
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Density @ 15°C (kg/m³)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.density || ''}
                onChange={(e) => handleChange('density', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                placeholder="991.0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Viscosity @ 50°C (cSt)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.viscosity || ''}
                onChange={(e) => handleChange('viscosity', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                placeholder="380.0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Flash Point (°C)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.flashPoint || ''}
                onChange={(e) => handleChange('flashPoint', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                placeholder="60.0"
                min="60"
              />
              <p className="text-xs text-gray-500 mt-1">Minimum 60°C for safety</p>
            </div>
          </div>
        </div>

        {/* ROB (Remaining On Board) */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Remaining On Board (ROB)</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                ROB Before Bunkering (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.robBefore || ''}
                onChange={(e) => handleChange('robBefore', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                min="0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                ROB After Bunkering (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.robAfter || ''}
                onChange={(e) => handleChange('robAfter', parseFloat(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                min="0"
              />
            </div>
          </div>
        </div>

        {/* Sample Information */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Sample Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={formData.sampleSealed}
                  onChange={(e) => handleChange('sampleSealed', e.target.checked)}
                  className="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700">
                  Sample Sealed (MARPOL Requirement)
                </span>
              </label>
              <p className="text-xs text-gray-500 mt-1 ml-7">
                MARPOL Annex VI requires fuel samples to be sealed and retained onboard
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Sample Number
              </label>
              <input
                type="text"
                value={formData.sampleNumber || ''}
                onChange={(e) => handleChange('sampleNumber', e.target.value)}
                placeholder="Sample identification number"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
        </div>

        {/* Remarks */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Remarks</h2>
          <textarea
            value={formData.remarks || ''}
            onChange={(e) => handleChange('remarks', e.target.value)}
            rows={4}
            placeholder="Additional information, observations, or notes..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Prepared By */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Prepared By
            </label>
            <input
              type="text"
              value={formData.preparedBy || ''}
              onChange={(e) => handleChange('preparedBy', e.target.value)}
              placeholder="Chief Engineer name"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-4 justify-end">
          <button
            type="button"
            onClick={() => navigate('/reporting/reports')}
            className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            disabled={loading}
          >
            Cancel
          </button>

          <button
            type="button"
            onClick={() => handleSubmit(true)}
            className="px-6 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors flex items-center gap-2"
            disabled={loading}
          >
            <Save className="h-4 w-4" />
            Save as Draft
          </button>

          <button
            type="button"
            onClick={() => handleSubmit(false)}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
            disabled={loading}
          >
            {loading ? (
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
            ) : (
              <Send className="h-4 w-4" />
            )}
            Submit Report
          </button>
        </div>
      </form>
    </div>
  );
}
