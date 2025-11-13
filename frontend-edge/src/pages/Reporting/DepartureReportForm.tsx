/**
 * Departure Report Form
 * SOLAS Compliant - Port Departure Notification
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Anchor, MapPin, Save, Send, Ship, User } from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { CreateDepartureReportDto } from '../../types/reporting.types';

export function DepartureReportForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<CreateDepartureReportDto>({
    departureDateTime: new Date().toISOString(),
    voyageId: 0,
    portName: '',
    draftForward: 0,
    draftAft: 0
  });

  const handleChange = (field: keyof CreateDepartureReportDto, value: any) => {
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
    if (!formData.departureDateTime) {
      setError('Departure date/time is required');
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
      const report = await ReportingService.createDepartureReport(formData);
      
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
          <Anchor className="h-8 w-8 text-blue-600" />
          Departure Report
        </h1>
        <p className="text-gray-600 mt-2">
          SOLAS Compliant Port Departure Notification
        </p>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
          <p className="font-semibold">Error</p>
          <p className="text-sm">{error}</p>
        </div>
      )}

      <form className="space-y-6">
        {/* Departure Information */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Departure Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Departure Date/Time <span className="text-red-600">*</span>
              </label>
              <input
                type="datetime-local"
                value={formData.departureDateTime}
                onChange={(e) => handleChange('departureDateTime', e.target.value)}
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
                value={formData.voyageId || ''}
                onChange={(e) => handleChange('voyageId', parseInt(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
                min="1"
              />
            </div>
          </div>
        </div>

        {/* Port Information */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <MapPin className="h-5 w-5 text-blue-600" />
            Port Information
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                Pilot Off Time
              </label>
              <input
                type="time"
                value={formData.pilotOffTime || ''}
                onChange={(e) => handleChange('pilotOffTime', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Last Line Let Go Time
              </label>
              <input
                type="time"
                value={formData.lastLineLetGoTime || ''}
                onChange={(e) => handleChange('lastLineLetGoTime', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
        </div>

        {/* Vessel Draft */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <Ship className="h-5 w-5 text-blue-600" />
            Vessel Draft
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Draft Forward (meters)
              </label>
              <input
                type="number"
                step="0.01"
                value={formData.draftForward}
                onChange={(e) => handleChange('draftForward', parseFloat(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                min="0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Draft Aft (meters)
              </label>
              <input
                type="number"
                step="0.01"
                value={formData.draftAft}
                onChange={(e) => handleChange('draftAft', parseFloat(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                min="0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Draft Midship (meters)
              </label>
              <input
                type="number"
                step="0.01"
                value={formData.draftMidship}
                onChange={(e) => handleChange('draftMidship', parseFloat(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                min="0"
              />
            </div>
          </div>
        </div>

        {/* Cargo & Fuel */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Cargo & Fuel Status</h2>
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
                Cargo Description
              </label>
              <input
                type="text"
                value={formData.cargoDescription || ''}
                onChange={(e) => handleChange('cargoDescription', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                placeholder="Type of cargo"
              />
            </div>

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
          </div>
        </div>

        {/* Next Port */}
        <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Destination Port</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Destination Port
              </label>
              <input
                type="text"
                value={formData.destinationPort || ''}
                onChange={(e) => handleChange('destinationPort', e.target.value)}
                placeholder="Destination port"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Estimated Arrival
              </label>
              <input
                type="datetime-local"
                value={formData.estimatedArrival || ''}
                onChange={(e) => handleChange('estimatedArrival', e.target.value)}
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
            placeholder="Additional information, incidents, or observations..."
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
