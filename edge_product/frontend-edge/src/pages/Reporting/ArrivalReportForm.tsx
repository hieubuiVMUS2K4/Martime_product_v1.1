/**
 * Arrival Report Creation Form
 * SOLAS V Compliant - Port Arrival Notification
 */

import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Anchor, MapPin, Save, Send, Ship, User, Fuel } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreateArrivalReportDto } from '../../types/reporting.types';
import {
  createSetFieldRef,
  extractBackendFieldErrors,
  focusFirstValidationError,
  getValidationInputClassName,
  renderValidationFieldError,
} from './reportFormValidation';

const ARRIVAL_FIELD_NAME_MAP: Record<string, string> = {
  VoyageId: 'voyageId',
  PortName: 'portName',
  PortCode: 'portCode',
  ArrivalDateTime: 'arrivalDateTime',
  PilotOnBoardTime: 'pilotOnBoardTime',
  FirstLineAshoreTime: 'anchorDropTime',
  VoyageDistance: 'voyageDistance',
  VoyageDuration: 'voyageDuration',
  AverageSpeed: 'averageSpeed',
  DraftForward: 'draftForward',
  DraftAft: 'draftAft',
  FuelOilROB: 'fuelOilROB',
  TotalFuelConsumed: 'totalFuelConsumed',
  DieselOilROB: 'dieselOilROB',
  CargoOnBoard: 'cargoOnBoard',
  PreparedBy: 'preparedBy',
};

export function ArrivalReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const fieldRefs = useRef<Record<string, HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement | null>>({});
  const setFieldRef = createSetFieldRef(fieldRefs);

  const [formData, setFormData] = useState<CreateArrivalReportDto>({
    arrivalDateTime: new Date().toISOString().slice(0, 16),
    voyageId: undefined,
    portName: '',
    draftForward: undefined,
    draftAft: undefined,
  });

  const handleChange = (field: keyof CreateArrivalReportDto, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));

    if (fieldErrors[field]) {
      setFieldErrors((prev) => {
        const nextErrors = { ...prev };
        delete nextErrors[field];
        return nextErrors;
      });
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

    if (normalized.includes('port name')) return 'portName';
    if (normalized.includes('arrival date')) return 'arrivalDateTime';
    if (normalized.includes('voyage')) return 'voyageId';
    if (normalized.includes('draft forward')) return 'draftForward';
    if (normalized.includes('draft aft')) return 'draftAft';
    if (normalized.includes('fuel oil rob')) return 'fuelOilROB';
    if (normalized.includes('diesel oil rob')) return 'dieselOilROB';
    if (normalized.includes('fuel consumed')) return 'totalFuelConsumed';
    if (normalized.includes('voyage distance')) return 'voyageDistance';
    if (normalized.includes('voyage duration')) return 'voyageDuration';
    if (normalized.includes('average speed')) return 'averageSpeed';
    if (normalized.includes('cargo on board')) return 'cargoOnBoard';
    if (normalized.includes('prepared by')) return 'preparedBy';

    return null;
  };

  const validateForm = (): boolean => {
    const nextFieldErrors: Record<string, string> = {};

    if (!formData.portName?.trim()) {
      nextFieldErrors.portName = 'Port name is required';
    }

    if (!formData.arrivalDateTime) {
      nextFieldErrors.arrivalDateTime = 'Arrival date/time is required';
    }

    if (formData.draftForward === undefined) {
      nextFieldErrors.draftForward = 'Draft forward is required';
    } else if (formData.draftForward < 0) {
      nextFieldErrors.draftForward = 'Draft forward cannot be negative';
    }

    if (formData.draftAft === undefined) {
      nextFieldErrors.draftAft = 'Draft aft is required';
    } else if (formData.draftAft < 0) {
      nextFieldErrors.draftAft = 'Draft aft cannot be negative';
    }

    if (formData.voyageDistance !== undefined && formData.voyageDistance < 0) {
      nextFieldErrors.voyageDistance = 'Voyage distance cannot be negative';
    }

    if (formData.voyageDuration !== undefined && formData.voyageDuration < 0) {
      nextFieldErrors.voyageDuration = 'Voyage duration cannot be negative';
    }

    if (formData.averageSpeed !== undefined && formData.averageSpeed < 0) {
      nextFieldErrors.averageSpeed = 'Average speed cannot be negative';
    }

    if (formData.fuelOilROB !== undefined && formData.fuelOilROB < 0) {
      nextFieldErrors.fuelOilROB = 'Fuel Oil ROB cannot be negative';
    }

    if (formData.totalFuelConsumed !== undefined && formData.totalFuelConsumed < 0) {
      nextFieldErrors.totalFuelConsumed = 'Total fuel consumed cannot be negative';
    }

    if (formData.dieselOilROB !== undefined && formData.dieselOilROB < 0) {
      nextFieldErrors.dieselOilROB = 'Diesel Oil ROB cannot be negative';
    }

    if (formData.cargoOnBoard !== undefined && formData.cargoOnBoard < 0) {
      nextFieldErrors.cargoOnBoard = 'Cargo on board cannot be negative';
    }

    if (formData.cargoDischargedAtPort !== undefined && formData.cargoDischargedAtPort < 0) {
      nextFieldErrors.cargoDischargedAtPort = 'Cargo discharged cannot be negative';
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

      const cleanedData = {
        ...formData,
        voyageId: formData.voyageId && formData.voyageId.trim() !== '' ? formData.voyageId : undefined,
      };

      const response = await ReportingService.createArrivalReport(cleanedData);

      if (!asDraft) {
        await ReportingService.submitReport(response.reportId);
      }

      navigate('/reporting/reports');
    } catch (err: any) {
      const backendFieldErrors = extractBackendFieldErrors(err, ARRIVAL_FIELD_NAME_MAP, mapValidationMessageToField);
      if (Object.keys(backendFieldErrors).length > 0) {
        applyFieldErrors(backendFieldErrors);
        return;
      }

      setError(err.message || 'Failed to create arrival report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      <div className="max-w-5xl mx-auto">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
            <Ship className="h-8 w-8 text-blue-600" />
            Arrival Report
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">SOLAS V Compliant - Port Arrival Notification</p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        <form className="space-y-6">
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Arrival Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Arrival Date/Time <span className="text-red-600">*</span>
                </label>
                <input
                  ref={setFieldRef('arrivalDateTime')}
                  type="datetime-local"
                  value={formData.arrivalDateTime}
                  onChange={(e) => handleChange('arrivalDateTime', e.target.value)}
                  className={getValidationInputClassName(fieldErrors, 'arrivalDateTime')}
                  required
                />
                {renderValidationFieldError(fieldErrors, 'arrivalDateTime')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Voyage ID (Optional)</label>
                <input
                  ref={setFieldRef('voyageId')}
                  type="text"
                  value={formData.voyageId || ''}
                  onChange={(e) => handleChange('voyageId', e.target.value || undefined)}
                  className={getValidationInputClassName(fieldErrors, 'voyageId')}
                  placeholder="Leave empty if no voyage"
                />
                {renderValidationFieldError(fieldErrors, 'voyageId')}
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
                  placeholder="e.g., Port of Singapore"
                  className={getValidationInputClassName(fieldErrors, 'portName')}
                  required
                />
                {renderValidationFieldError(fieldErrors, 'portName')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Port Code (UN/LOCODE)</label>
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
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <User className="h-5 w-5 text-blue-600" />
              Pilot Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Pilot On Board Time</label>
                <input
                  type="time"
                  value={formData.pilotOnBoardTime || ''}
                  onChange={(e) => handleChange('pilotOnBoardTime', e.target.value)}
                  className={getValidationInputClassName(fieldErrors, 'pilotOnBoardTime')}
                />
                {renderValidationFieldError(fieldErrors, 'pilotOnBoardTime')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Anchor Drop Time</label>
                <input
                  ref={setFieldRef('anchorDropTime')}
                  type="time"
                  value={formData.anchorDropTime || ''}
                  onChange={(e) => handleChange('anchorDropTime', e.target.value)}
                  className={getValidationInputClassName(fieldErrors, 'anchorDropTime')}
                />
                {renderValidationFieldError(fieldErrors, 'anchorDropTime')}
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Anchor className="h-5 w-5 text-blue-600" />
              Voyage Statistics
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Total Distance (nm)</label>
                <input
                  ref={setFieldRef('voyageDistance')}
                  type="number"
                  step="0.1"
                  value={formData.voyageDistance || ''}
                  onChange={(e) => handleChange('voyageDistance', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'voyageDistance')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'voyageDistance')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Voyage Duration (hours)</label>
                <input
                  ref={setFieldRef('voyageDuration')}
                  type="number"
                  step="0.1"
                  value={formData.voyageDuration || ''}
                  onChange={(e) => handleChange('voyageDuration', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'voyageDuration')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'voyageDuration')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Average Speed (knots)</label>
                <input
                  ref={setFieldRef('averageSpeed')}
                  type="number"
                  step="0.1"
                  value={formData.averageSpeed || ''}
                  onChange={(e) => handleChange('averageSpeed', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'averageSpeed')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'averageSpeed')}
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <MapPin className="h-5 w-5 text-blue-600" />
              Draft Survey
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Draft Forward (meters) <span className="text-red-600">*</span>
                </label>
                <input
                  ref={setFieldRef('draftForward')}
                  type="number"
                  step="0.01"
                  value={formData.draftForward ?? ''}
                  onChange={(e) => handleChange('draftForward', e.target.value === '' ? undefined : parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'draftForward')}
                  min="0"
                  required
                />
                {renderValidationFieldError(fieldErrors, 'draftForward')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Draft Aft (meters) <span className="text-red-600">*</span>
                </label>
                <input
                  ref={setFieldRef('draftAft')}
                  type="number"
                  step="0.01"
                  value={formData.draftAft ?? ''}
                  onChange={(e) => handleChange('draftAft', e.target.value === '' ? undefined : parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'draftAft')}
                  min="0"
                  required
                />
                {renderValidationFieldError(fieldErrors, 'draftAft')}
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Fuel className="h-5 w-5 text-blue-600" />
              Fuel & Cargo Status
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Fuel Oil ROB (MT)</label>
                <input
                  ref={setFieldRef('fuelOilROB')}
                  type="number"
                  step="0.1"
                  value={formData.fuelOilROB || ''}
                  onChange={(e) => handleChange('fuelOilROB', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'fuelOilROB')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'fuelOilROB')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Total Fuel Consumed (MT)</label>
                <input
                  ref={setFieldRef('totalFuelConsumed')}
                  type="number"
                  step="0.1"
                  value={formData.totalFuelConsumed || ''}
                  onChange={(e) => handleChange('totalFuelConsumed', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'totalFuelConsumed')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'totalFuelConsumed')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Diesel Oil ROB (MT)</label>
                <input
                  ref={setFieldRef('dieselOilROB')}
                  type="number"
                  step="0.1"
                  value={formData.dieselOilROB || ''}
                  onChange={(e) => handleChange('dieselOilROB', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'dieselOilROB')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'dieselOilROB')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Cargo On Board (MT)</label>
                <input
                  ref={setFieldRef('cargoOnBoard')}
                  type="number"
                  step="0.1"
                  value={formData.cargoOnBoard || ''}
                  onChange={(e) => handleChange('cargoOnBoard', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'cargoOnBoard')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'cargoOnBoard')}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Cargo Discharged at Port (MT)</label>
                <input
                  ref={setFieldRef('cargoDischargedAtPort')}
                  type="number"
                  step="0.1"
                  value={formData.cargoDischargedAtPort || ''}
                  onChange={(e) => handleChange('cargoDischargedAtPort', parseFloat(e.target.value))}
                  className={getValidationInputClassName(fieldErrors, 'cargoDischargedAtPort')}
                  min="0"
                />
                {renderValidationFieldError(fieldErrors, 'cargoDischargedAtPort')}
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
              placeholder="Any additional information about the arrival..."
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
                placeholder="Officer name"
                className={getValidationInputClassName(fieldErrors, 'preparedBy')}
              />
              {renderValidationFieldError(fieldErrors, 'preparedBy')}
            </div>
          </div>

          <div className="flex gap-4 justify-end">
            <button
              type="button"
              onClick={() => navigate('/reporting/reports')}
              className="px-6 py-3 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
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
              <Save className="h-5 w-5" />
              {loading ? 'Saving...' : 'Save Draft'}
            </button>
            <button
              type="button"
              onClick={() => handleSubmit(false)}
              className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
              disabled={loading}
            >
              <Send className="h-5 w-5" />
              {loading ? 'Submitting...' : 'Submit Report'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}