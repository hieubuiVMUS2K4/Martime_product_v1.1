/**
 * Bunker Report Form
 * MARPOL Annex VI Compliant - Fuel Bunkering Report
 */

import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Fuel, AlertTriangle, Save, Send } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreateBunkerReportDto } from '../../types/reporting.types';
import {
  createSetFieldRef,
  extractBackendFieldErrors,
  focusFirstValidationError,
  getValidationInputClassName,
  renderValidationFieldError,
} from './reportFormValidation';

const BUNKER_FIELD_NAME_MAP: Record<string, string> = {
  BunkerDate: 'bunkerDate',
  PortName: 'portName',
  PortCode: 'portCode',
  SupplierName: 'supplierName',
  BDNNumber: 'bdnNumber',
  FuelType: 'fuelType',
  FuelGrade: 'fuelGrade',
  QuantityReceived: 'quantityReceived',
  SulphurContent: 'sulphurContent',
  Density: 'density',
  Viscosity: 'viscosity',
  FlashPoint: 'flashPoint',
  ROBBefore: 'robBefore',
  ROBAfter: 'robAfter',
  PreparedBy: 'preparedBy',
};

export function BunkerReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [marpol, setMarpol] = useState<string[]>([]);
  const fieldRefs = useRef<Record<string, HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement | null>>({});
  const setFieldRef = createSetFieldRef(fieldRefs);

  const [formData, setFormData] = useState<CreateBunkerReportDto>({
    bunkerDate: new Date().toISOString().split('T')[0],
    portName: '',
    supplierName: '',
    fuelType: 'MARINE_FUEL_OIL',
    quantityReceived: 0,
    sampleSealed: false,
  });

  const handleChange = (field: keyof CreateBunkerReportDto, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));

    if (fieldErrors[field]) {
      setFieldErrors((prev) => {
        const nextErrors = { ...prev };
        delete nextErrors[field];
        return nextErrors;
      });
    }

    if (field === 'sulphurContent' && typeof value === 'number') {
      const warnings: string[] = [];
      if (value > 0.5) {
        warnings.push('MARPOL VI: Global sulphur limit is 0.50% m/m');
      }
      if (value > 0.1) {
        warnings.push('SECA: Emission Control Area limit is 0.10% m/m');
      }
      setMarpol(warnings);
    }
  };

  const applyFieldErrors = (nextFieldErrors: Record<string, string>) => {
    setError(null);
    setFieldErrors(nextFieldErrors);

    if (Object.keys(nextFieldErrors).length > 0) {
      setTimeout(() => focusFirstValidationError(fieldRefs, nextFieldErrors), 100);
    }
  };

  const mapValidationMessageToField = (message: string): string | null => {
    const normalized = message.toLowerCase();

    if (normalized.includes('bunker date')) return 'bunkerDate';
    if (normalized.includes('port name')) return 'portName';
    if (normalized.includes('supplier name')) return 'supplierName';
    if (normalized.includes('bdn')) return 'bdnNumber';
    if (normalized.includes('fuel type')) return 'fuelType';
    if (normalized.includes('quantity received')) return 'quantityReceived';
    if (normalized.includes('sulphur')) return 'sulphurContent';
    if (normalized.includes('flash point')) return 'flashPoint';
    if (normalized.includes('density')) return 'density';
    if (normalized.includes('viscosity')) return 'viscosity';
    if (normalized.includes('rob before')) return 'robBefore';
    if (normalized.includes('rob after')) return 'robAfter';
    if (normalized.includes('prepared by')) return 'preparedBy';

    return null;
  };

  const validateForm = (): boolean => {
    const nextFieldErrors: Record<string, string> = {};

    if (!formData.bunkerDate) {
      nextFieldErrors.bunkerDate = 'Bunker date is required';
    }

    if (!formData.portName?.trim()) {
      nextFieldErrors.portName = 'Port name is required';
    }

    if (!formData.supplierName?.trim()) {
      nextFieldErrors.supplierName = 'Supplier name is required';
    }

    if (!formData.bdnNumber?.trim()) {
      nextFieldErrors.bdnNumber = 'BDN number is required';
    }

    if (formData.quantityReceived <= 0) {
      nextFieldErrors.quantityReceived = 'Quantity received must be greater than 0';
    }

    if (formData.quantityReceived > 10000) {
      nextFieldErrors.quantityReceived = 'Quantity received must be less than or equal to 10000 MT';
    }

    if (formData.sulphurContent !== undefined && formData.sulphurContent < 0) {
      nextFieldErrors.sulphurContent = 'Sulphur content cannot be negative';
    }

    if (formData.sulphurContent !== undefined && formData.sulphurContent > 3.5) {
      nextFieldErrors.sulphurContent = 'Sulphur content exceeds maximum allowed (3.5%)';
    }

    if (formData.density !== undefined && formData.density < 0) {
      nextFieldErrors.density = 'Density cannot be negative';
    }

    if (formData.viscosity !== undefined && formData.viscosity < 0) {
      nextFieldErrors.viscosity = 'Viscosity cannot be negative';
    }

    if (formData.flashPoint !== undefined && formData.flashPoint < 60) {
      nextFieldErrors.flashPoint = 'Flash point must be at least 60°C';
    }

    if (formData.robBefore !== undefined && formData.robBefore < 0) {
      nextFieldErrors.robBefore = 'ROB before cannot be negative';
    }

    if (formData.robAfter !== undefined && formData.robAfter < 0) {
      nextFieldErrors.robAfter = 'ROB after cannot be negative';
    }

    if (Object.keys(nextFieldErrors).length > 0) {
      applyFieldErrors(nextFieldErrors);
      return false;
    }

    setFieldErrors({});
    setError(null);
    return true;
  };

  const handleSubmit = async (asDraft = false) => {
    if (!asDraft && !validateForm()) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setFieldErrors({});

      const report = await ReportingService.createBunkerReport(formData);

      if (!asDraft) {
        await ReportingService.submitReport(report.reportId);
      }

      navigate('/reporting/reports');
    } catch (err) {
      const backendFieldErrors = extractBackendFieldErrors(err, BUNKER_FIELD_NAME_MAP, mapValidationMessageToField);
      if (Object.keys(backendFieldErrors).length > 0) {
        applyFieldErrors(backendFieldErrors);
        return;
      }

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
        <p className="text-gray-600 mt-2">MARPOL Annex VI Compliant Fuel Bunkering Report</p>
      </div>

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
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Bunker Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Bunker Date <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('bunkerDate')}
                type="date"
                value={formData.bunkerDate}
                onChange={(e) => handleChange('bunkerDate', e.target.value)}
                className={getValidationInputClassName(fieldErrors, 'bunkerDate')}
                required
              />
              {renderValidationFieldError(fieldErrors, 'bunkerDate')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Port Name <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('portName')}
                type="text"
                value={formData.portName}
                onChange={(e) => handleChange('portName', e.target.value)}
                placeholder="e.g., Singapore"
                className={getValidationInputClassName(fieldErrors, 'portName')}
                required
              />
              {renderValidationFieldError(fieldErrors, 'portName')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Port Code</label>
              <input
                type="text"
                value={formData.portCode || ''}
                onChange={(e) => handleChange('portCode', e.target.value)}
                placeholder="e.g., SGSIN"
                className={getValidationInputClassName(fieldErrors, 'portCode')}
                maxLength={5}
              />
              {renderValidationFieldError(fieldErrors, 'portCode')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Supplier Name <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('supplierName')}
                type="text"
                value={formData.supplierName}
                onChange={(e) => handleChange('supplierName', e.target.value)}
                placeholder="Fuel supplier company"
                className={getValidationInputClassName(fieldErrors, 'supplierName')}
                required
              />
              {renderValidationFieldError(fieldErrors, 'supplierName')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                BDN Number <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('bdnNumber')}
                type="text"
                value={formData.bdnNumber || ''}
                onChange={(e) => handleChange('bdnNumber', e.target.value)}
                placeholder="Bunker Delivery Note number"
                className={getValidationInputClassName(fieldErrors, 'bdnNumber')}
              />
              {renderValidationFieldError(fieldErrors, 'bdnNumber')}
              <p className="text-xs text-gray-500 mt-1">Bunker Delivery Note (BDN)</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Fuel Specifications</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Fuel Type <span className="text-red-600">*</span>
              </label>
              <select
                ref={setFieldRef('fuelType')}
                value={formData.fuelType}
                onChange={(e) => handleChange('fuelType', e.target.value)}
                className={getValidationInputClassName(fieldErrors, 'fuelType')}
                required
              >
                <option value="MARINE_FUEL_OIL">Marine Fuel Oil (MFO)</option>
                <option value="MARINE_DIESEL_OIL">Marine Diesel Oil (MDO)</option>
                <option value="MARINE_GAS_OIL">Marine Gas Oil (MGO)</option>
                <option value="HEAVY_FUEL_OIL">Heavy Fuel Oil (HFO)</option>
                <option value="LOW_SULPHUR_FUEL_OIL">Low Sulphur Fuel Oil (LSFO)</option>
                <option value="ULTRA_LOW_SULPHUR_FUEL_OIL">Ultra Low Sulphur Fuel Oil (ULSFO)</option>
              </select>
              {renderValidationFieldError(fieldErrors, 'fuelType')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Fuel Grade</label>
              <input
                type="text"
                value={formData.fuelGrade || ''}
                onChange={(e) => handleChange('fuelGrade', e.target.value)}
                placeholder="e.g., ISO-F-RMG 380"
                className={getValidationInputClassName(fieldErrors, 'fuelGrade')}
              />
              {renderValidationFieldError(fieldErrors, 'fuelGrade')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Quantity Received (MT) <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('quantityReceived')}
                type="number"
                step="0.001"
                value={formData.quantityReceived}
                onChange={(e) => handleChange('quantityReceived', parseFloat(e.target.value) || 0)}
                className={getValidationInputClassName(fieldErrors, 'quantityReceived')}
                required
                min="0"
              />
              {renderValidationFieldError(fieldErrors, 'quantityReceived')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Sulphur Content (% m/m) <span className="text-red-600">*</span>
              </label>
              <input
                ref={setFieldRef('sulphurContent')}
                type="number"
                step="0.01"
                value={formData.sulphurContent || ''}
                onChange={(e) => handleChange('sulphurContent', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'sulphurContent')}
                placeholder="0.50"
                max="3.5"
              />
              {renderValidationFieldError(fieldErrors, 'sulphurContent')}
              <p className="text-xs text-gray-500 mt-1">Global limit: 0.50% | SECA: 0.10%</p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Density @ 15°C (kg/m3)</label>
              <input
                ref={setFieldRef('density')}
                type="number"
                step="0.1"
                value={formData.density || ''}
                onChange={(e) => handleChange('density', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'density')}
                placeholder="991.0"
              />
              {renderValidationFieldError(fieldErrors, 'density')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Viscosity @ 50°C (cSt)</label>
              <input
                ref={setFieldRef('viscosity')}
                type="number"
                step="0.1"
                value={formData.viscosity || ''}
                onChange={(e) => handleChange('viscosity', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'viscosity')}
                placeholder="380.0"
              />
              {renderValidationFieldError(fieldErrors, 'viscosity')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Flash Point (°C)</label>
              <input
                ref={setFieldRef('flashPoint')}
                type="number"
                step="0.1"
                value={formData.flashPoint || ''}
                onChange={(e) => handleChange('flashPoint', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'flashPoint')}
                placeholder="60.0"
                min="60"
              />
              {renderValidationFieldError(fieldErrors, 'flashPoint')}
              <p className="text-xs text-gray-500 mt-1">Minimum 60°C for safety</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Remaining On Board (ROB)</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">ROB Before Bunkering (MT)</label>
              <input
                ref={setFieldRef('robBefore')}
                type="number"
                step="0.1"
                value={formData.robBefore || ''}
                onChange={(e) => handleChange('robBefore', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'robBefore')}
                min="0"
              />
              {renderValidationFieldError(fieldErrors, 'robBefore')}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">ROB After Bunkering (MT)</label>
              <input
                ref={setFieldRef('robAfter')}
                type="number"
                step="0.1"
                value={formData.robAfter || ''}
                onChange={(e) => handleChange('robAfter', parseFloat(e.target.value))}
                className={getValidationInputClassName(fieldErrors, 'robAfter')}
                min="0"
              />
              {renderValidationFieldError(fieldErrors, 'robAfter')}
            </div>
          </div>
        </div>

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
                <span className="text-sm font-medium text-gray-700">Sample Sealed (MARPOL Requirement)</span>
              </label>
              <p className="text-xs text-gray-500 mt-1 ml-7">MARPOL Annex VI requires fuel samples to be sealed and retained onboard</p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Sample Number</label>
              <input
                type="text"
                value={formData.sampleNumber || ''}
                onChange={(e) => handleChange('sampleNumber', e.target.value)}
                placeholder="Sample identification number"
                className={getValidationInputClassName(fieldErrors, 'sampleNumber')}
              />
              {renderValidationFieldError(fieldErrors, 'sampleNumber')}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Remarks</h2>
          <textarea
            ref={setFieldRef('remarks')}
            value={formData.remarks || ''}
            onChange={(e) => handleChange('remarks', e.target.value)}
            rows={4}
            placeholder="Additional information, observations, or notes..."
            className={getValidationInputClassName(fieldErrors, 'remarks')}
          />
          {renderValidationFieldError(fieldErrors, 'remarks')}
        </div>

        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Prepared By</label>
            <input
              ref={setFieldRef('preparedBy')}
              type="text"
              value={formData.preparedBy || ''}
              onChange={(e) => handleChange('preparedBy', e.target.value)}
              placeholder="Chief Engineer name"
              className={getValidationInputClassName(fieldErrors, 'preparedBy')}
            />
            {renderValidationFieldError(fieldErrors, 'preparedBy')}
          </div>
        </div>

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