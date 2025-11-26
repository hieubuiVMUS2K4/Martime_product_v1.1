/**
 * Position Report Creation Form
 * SOLAS V Reg 19.2.1.4 - Special Position Reporting
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapPin, Save, Send, AlertTriangle, Ship } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreatePositionReportDto } from '../../types/reporting.types';

export function PositionReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<CreatePositionReportDto>({
    reportDateTime: new Date().toISOString(),
    latitude: 0,
    longitude: 0,
    reportReason: 'ROUTING'
  });

  const handleChange = (field: keyof CreatePositionReportDto, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const validateForm = (): boolean => {
    if (!formData.reportDateTime) {
      setError('Report date/time is required');
      return false;
    }
    
    if (formData.latitude < -90 || formData.latitude > 90) {
      setError('Latitude must be between -90 and 90 degrees');
      return false;
    }
    
    if (formData.longitude < -180 || formData.longitude > 180) {
      setError('Longitude must be between -180 and 180 degrees');
      return false;
    }
    
    if (formData.latitude === 0 && formData.longitude === 0) {
      setError('Invalid position (Null Island) - please enter actual coordinates');
      return false;
    }
    
    if (!formData.reportReason?.trim()) {
      setError('Report reason is required');
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

      const response = await ReportingService.createPositionReport(formData);

      if (!asDraft) {
        // Submit for approval
        await ReportingService.submitReport(response.reportId);
      }

      navigate('/reporting/reports');
    } catch (err: any) {
      setError(err.message || 'Failed to create position report');
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
            <MapPin className="h-8 w-8 text-blue-600" />
            Position Report
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            SOLAS V Regulation 19.2.1.4 - Special Position Reporting
          </p>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-center gap-3">
            <AlertTriangle className="h-5 w-5 text-red-600" />
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        {/* Form */}
        <form className="space-y-6">
          {/* Basic Information */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Report Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Report Date/Time <span className="text-red-600">*</span>
                </label>
                <input
                  type="datetime-local"
                  value={formData.reportDateTime}
                  onChange={(e) => handleChange('reportDateTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Report Reason <span className="text-red-600">*</span>
                </label>
                <select
                  value={formData.reportReason}
                  onChange={(e) => handleChange('reportReason', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="ROUTING">Routing Report</option>
                  <option value="DEVIATION">Deviation from Route</option>
                  <option value="EMERGENCY">Emergency</option>
                  <option value="SEARCH_RESCUE">Search and Rescue</option>
                  <option value="SPECIAL_AREA">Special Area Transit</option>
                  <option value="PIRACY_AREA">High Risk/Piracy Area</option>
                  <option value="WEATHER">Severe Weather</option>
                  <option value="POLLUTION">Pollution Incident</option>
                  <option value="MEDICAL">Medical Emergency</option>
                  <option value="OTHER">Other</option>
                </select>
                <p className="text-xs text-gray-500 mt-1">
                  Select reason for position report per SOLAS V
                </p>
              </div>
            </div>
          </div>

          {/* Position Information */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <MapPin className="h-5 w-5 text-blue-600" />
              Current Position
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Latitude (°) <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  step="0.000001"
                  value={formData.latitude}
                  onChange={(e) => handleChange('latitude', parseFloat(e.target.value) || 0)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., 1.290270"
                  min="-90"
                  max="90"
                  required
                />
                <p className="text-xs text-gray-500 mt-1">Range: -90 to 90 (North positive)</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Longitude (°) <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  step="0.000001"
                  value={formData.longitude}
                  onChange={(e) => handleChange('longitude', parseFloat(e.target.value) || 0)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., 103.851959"
                  min="-180"
                  max="180"
                  required
                />
                <p className="text-xs text-gray-500 mt-1">Range: -180 to 180 (East positive)</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Course Over Ground (°)
                </label>
                <input
                  type="number"
                  step="1"
                  value={formData.courseOverGround || ''}
                  onChange={(e) => handleChange('courseOverGround', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                  max="360"
                  placeholder="0-360"
                />
                <p className="text-xs text-gray-500 mt-1">Degrees true</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Speed Over Ground (knots)
                </label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.speedOverGround || ''}
                  onChange={(e) => handleChange('speedOverGround', parseFloat(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  min="0"
                  max="50"
                  placeholder="0-50"
                />
              </div>
            </div>

            {/* Warning for Null Island */}
            {formData.latitude === 0 && formData.longitude === 0 && (
              <div className="mt-4 p-3 bg-yellow-50 border border-yellow-200 rounded-lg flex items-center gap-2">
                <AlertTriangle className="h-5 w-5 text-yellow-600" />
                <p className="text-sm text-yellow-800">
                  Warning: Position (0,0) is "Null Island" - Please enter actual coordinates
                </p>
              </div>
            )}
          </div>

          {/* Voyage Information */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Ship className="h-5 w-5 text-blue-600" />
              Voyage Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Last Port
                </label>
                <input
                  type="text"
                  value={formData.lastPort || ''}
                  onChange={(e) => handleChange('lastPort', e.target.value)}
                  placeholder="e.g., Port of Singapore"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Next Port
                </label>
                <input
                  type="text"
                  value={formData.nextPort || ''}
                  onChange={(e) => handleChange('nextPort', e.target.value)}
                  placeholder="e.g., Port of Hong Kong"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  ETA at Next Port
                </label>
                <input
                  type="datetime-local"
                  value={formData.eta || ''}
                  onChange={(e) => handleChange('eta', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
          </div>

          {/* Cargo & Crew */}
          <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Cargo & Personnel</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                  Crew On Board
                </label>
                <input
                  type="number"
                  value={formData.crewOnBoard || ''}
                  onChange={(e) => handleChange('crewOnBoard', parseInt(e.target.value))}
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
              placeholder="Provide details about the situation requiring this position report..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
            <p className="text-xs text-gray-500 mt-1">
              {formData.reportReason === 'EMERGENCY' && '⚠️ For emergencies, include nature of emergency, assistance required'}
              {formData.reportReason === 'PIRACY_AREA' && '⚠️ For piracy areas, include security measures taken'}
              {formData.reportReason === 'POLLUTION' && '⚠️ For pollution, include substance type, quantity, actions taken'}
              {formData.reportReason === 'MEDICAL' && '⚠️ For medical emergencies, include patient condition, treatment needed'}
            </p>
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
                placeholder="Officer name and rank"
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
