/**
 * Arrival Report Creation Form
 * SOLAS V Compliant - Port Arrival Notification
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Anchor, MapPin, Save, Send, Ship, User, Fuel } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreateArrivalReportDto } from '../../types/reporting.types';

export function ArrivalReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<CreateArrivalReportDto>({
    arrivalDateTime: new Date().toISOString(),
    voyageId: 0,
    portName: '',
    draftForward: 0,
    draftAft: 0
  });

  const handleChange = (field: keyof CreateArrivalReportDto, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const validateForm = (): boolean => {
    if (!formData.voyageId || formData.voyageId === 0) {
      setError('Voyage ID is required');
      return false;
    }
    if (!formData.portName?.trim()) {
      setError('Port name is required');
      return false;
    }
    if (!formData.arrivalDateTime) {
      setError('Arrival date/time is required');
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
      setError(null);

      const response = await ReportingService.createArrivalReport(formData);

      if (!asDraft) {
        // Submit for approval
        await ReportingService.submitReport(response.reportId);
      }

      navigate('/reporting/reports');
    } catch (err: any) {
      setError(err.message || 'Failed to create arrival report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      <div className="max-w-5xl mx-auto">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
            <Ship className="h-8 w-8 text-blue-600" />
            Arrival Report
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            SOLAS V Compliant - Port Arrival Notification
          </p>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        {/* Form */}
        <form className="space-y-6">
          {/* Basic Information */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Arrival Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Arrival Date/Time <span className="text-red-600">*</span>
                </label>
                <input
                  type="datetime-local"
                  value={formData.arrivalDateTime}
                  onChange={(e) => handleChange('arrivalDateTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Voyage ID <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  value={formData.voyageId}
                  onChange={(e) => handleChange('voyageId', parseInt(e.target.value) || 0)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                  min="1"
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
                  placeholder="e.g., Port of Singapore"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Port Code (UN/LOCODE)
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
            </div>
          </div>

          {/* Pilot Information */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <User className="h-5 w-5 text-blue-600" />
              Pilot Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Pilot On Board Time
                </label>
                <input
                  type="time"
                  value={formData.pilotOnBoardTime || ''}
                  onChange={(e) => handleChange('pilotOnBoardTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Anchor Drop Time
                </label>
                <input
                  type="time"
                  value={formData.anchorDropTime || ''}
                  onChange={(e) => handleChange('anchorDropTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
          </div>

          {/* Voyage Statistics */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Anchor className="h-5 w-5 text-blue-600" />
              Voyage Statistics
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Total Distance (nm)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.voyageDistance || ''}
                  onChange={(e) => handleChange('voyageDistance', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Voyage Duration (hours)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.voyageDuration || ''}
                  onChange={(e) => handleChange('voyageDuration', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Average Speed (knots)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.averageSpeed || ''}
                  onChange={(e) => handleChange('averageSpeed', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>
            </div>
          </div>

          {/* Draft Information */}
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
                  type="number"
                  step="0.01"
                  value={formData.draftForward}
                  onChange={(e) => handleChange('draftForward', parseFloat(e.target.value) || 0)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Draft Aft (meters) <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={formData.draftAft}
                  onChange={(e) => handleChange('draftAft', parseFloat(e.target.value) || 0)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                  required
                />
              </div>
            </div>
          </div>

          {/* Fuel & Cargo */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Fuel className="h-5 w-5 text-blue-600" />
              Fuel & Cargo Status
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Fuel Oil ROB (MT)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.fuelOilROB || ''}
                  onChange={(e) => handleChange('fuelOilROB', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Total Fuel Consumed (MT)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.totalFuelConsumed || ''}
                  onChange={(e) => handleChange('totalFuelConsumed', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Diesel Oil ROB (MT)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.dieselOilROB || ''}
                  onChange={(e) => handleChange('dieselOilROB', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Cargo On Board (MT)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.cargoOnBoard || ''}
                  onChange={(e) => handleChange('cargoOnBoard', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Cargo Discharged at Port (MT)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.cargoDischargedAtPort || ''}
                  onChange={(e) => handleChange('cargoDischargedAtPort', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
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
              placeholder="Any additional information about the arrival..."
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
                placeholder="Officer name"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Action Buttons */}
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
