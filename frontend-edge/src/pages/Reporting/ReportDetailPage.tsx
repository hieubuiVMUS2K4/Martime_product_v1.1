/**
 * Report Detail Page
 * View report details and perform workflow actions
 */

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Ship, 
  Calendar, 
  User, 
  CheckCircle, 
  XCircle, 
  Send,
  FileText,
  Clock,
  ArrowLeft,
  History,
  RotateCcw
} from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import type { 
  NoonReportDto,
  ReportStatus,
  WorkflowHistoryDto
} from '../../types/reporting.types';

const STATUS_CONFIG: Record<ReportStatus, { color: string; icon: React.ReactNode; label: string }> = {
  DRAFT: {
    color: 'bg-gray-100 text-gray-700 border-gray-300',
    icon: <FileText className="h-5 w-5" />,
    label: 'Draft'
  },
  SUBMITTED: {
    color: 'bg-yellow-100 text-yellow-700 border-yellow-300',
    icon: <Clock className="h-5 w-5" />,
    label: 'Pending Approval'
  },
  APPROVED: {
    color: 'bg-blue-100 text-blue-700 border-blue-300',
    icon: <CheckCircle className="h-5 w-5" />,
    label: 'Approved'
  },
  REJECTED: {
    color: 'bg-red-100 text-red-700 border-red-300',
    icon: <XCircle className="h-5 w-5" />,
    label: 'Rejected'
  },
  TRANSMITTED: {
    color: 'bg-green-100 text-green-700 border-green-300',
    icon: <Send className="h-5 w-5" />,
    label: 'Transmitted'
  }
};

export function ReportDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [report, setReport] = useState<NoonReportDto | null>(null);
  const [history, setHistory] = useState<WorkflowHistoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Modal states
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showTransmitModal, setShowTransmitModal] = useState(false);
  
  const [approvalData, setApprovalData] = useState({
    masterSignature: '',
    approvalRemarks: ''
  });
  
  const [rejectionReason, setRejectionReason] = useState('');
  
  const [transmitData, setTransmitData] = useState({
    transmissionMethod: 'EMAIL',
    recipientEmails: '',
    transmissionRemarks: ''
  });

  useEffect(() => {
    if (id) {
      loadReportDetails();
      loadWorkflowHistory();
    }
  }, [id]);

  const loadReportDetails = async () => {
    try {
      setLoading(true);
      // For now, only Noon Report - extend for other types
      const data = await ReportingService.getNoonReport(parseInt(id!));
      setReport(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load report');
    } finally {
      setLoading(false);
    }
  };

  const loadWorkflowHistory = async () => {
    try {
      const data = await ReportingService.getWorkflowHistory(parseInt(id!));
      setHistory(data.history);
    } catch (err) {
      console.error('Failed to load history:', err);
    }
  };

  const handleApprove = async () => {
    try {
      await ReportingService.approveReport(parseInt(id!), approvalData);
      setShowApproveModal(false);
      await loadReportDetails();
      await loadWorkflowHistory();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to approve report');
    }
  };

  const handleReject = async () => {
    try {
      await ReportingService.rejectReport(parseInt(id!), rejectionReason);
      setShowRejectModal(false);
      await loadReportDetails();
      await loadWorkflowHistory();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to reject report');
    }
  };

  const handleTransmit = async () => {
    try {
      await ReportingService.transmitReport(parseInt(id!), {
        transmissionMethod: transmitData.transmissionMethod,
        recipientEmails: transmitData.recipientEmails.split(';').map(e => e.trim()).filter(e => e),
        includeAttachments: false
      });
      setShowTransmitModal(false);
      await loadReportDetails();
      await loadWorkflowHistory();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to transmit report');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error || !report) {
    return (
      <div className="p-8">
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
          <p className="font-semibold">Error</p>
          <p className="text-sm">{error || 'Report not found'}</p>
        </div>
      </div>
    );
  }

  const statusConfig = STATUS_CONFIG[report.status];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-3 py-4">
        {/* Header */}
        <button
          onClick={() => navigate('/reporting/reports')}
          className="flex items-center gap-1.5 text-gray-600 hover:text-gray-900 mb-3 text-sm"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Reports
        </button>
        
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-gray-900 flex items-center gap-2">
              <Ship className="h-5 w-5 text-blue-600" />
              Noon Report
            </h1>
            <p className="text-gray-600 mt-1 font-mono text-sm">
              {report.reportNumber}
            </p>
          </div>
          
          <div className={`flex items-center gap-2 px-3 py-1.5 rounded-lg border ${statusConfig.color}`}>
            {statusConfig.icon}
            <span className="font-semibold">{statusConfig.label}</span>
          </div>
        </div>

        {report.status === 'DRAFT' && (
          <div className="mb-4 flex gap-2">
            <button
              onClick={async () => {
                if (!window.confirm('Submit this report for approval?\n\nOnce submitted, you cannot edit it unless it is rejected or reopened.')) {
                  return;
                }
                
                try {
                  await ReportingService.submitReport(parseInt(id!));
                  await loadReportDetails();
                  await loadWorkflowHistory();
                  alert('✓ Report submitted successfully!\n\nIt is now pending approval by the Master.');
                } catch (err) {
                  alert(err instanceof Error ? err.message : 'Failed to submit report');
                }
              }}
              className="flex items-center gap-2 px-4 py-2 bg-yellow-600 text-white text-sm rounded-lg hover:bg-yellow-700 transition-colors"
            >
              <Send className="h-4 w-4" />
              Submit for Approval
            </button>
            
            <button
              onClick={() => {
                // Navigate to edit page (you'll need to create this route)
                navigate(`/reporting/noon/edit/${id}`);
              }}
              className="flex items-center gap-2 px-4 py-2 bg-gray-600 text-white text-sm rounded-lg hover:bg-gray-700 transition-colors"
            >
              <FileText className="h-4 w-4" />
              Edit Report
            </button>
          </div>
        )}

        {report.status === 'SUBMITTED' && (
          <div className="mb-4 flex gap-2">
            <button
              onClick={() => setShowApproveModal(true)}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors"
            >
              <CheckCircle className="h-4 w-4" />
              Approve Report
            </button>
            
            <button
              onClick={() => setShowRejectModal(true)}
              className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 transition-colors"
            >
              <XCircle className="h-4 w-4" />
              Reject Report
            </button>
          </div>
        )}

        {report.status === 'APPROVED' && !report.isTransmitted && (
          <div className="mb-4">
            <button
              onClick={() => setShowTransmitModal(true)}
              className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white text-sm rounded-lg hover:bg-green-700 transition-colors"
            >
              <Send className="h-4 w-4" />
              Transmit Report
            </button>
          </div>
        )}

        {report.status === 'REJECTED' && (
          <div className="mb-4">
            <button
              onClick={async () => {
              const corrections = prompt('What corrections will you make to this report?');
              if (!corrections) return;
              
              try {
                await ReportingService.reopenReport(parseInt(id!), corrections);
                await loadReportDetails();
                await loadWorkflowHistory();
                alert('Report reopened successfully. You can now edit and resubmit.');
              } catch (err) {
                alert(err instanceof Error ? err.message : 'Failed to reopen report');
              }
            }}
              className="flex items-center gap-2 px-4 py-2 bg-orange-600 text-white text-sm rounded-lg hover:bg-orange-700 transition-colors"
            >
              <RotateCcw className="h-4 w-4" />
              Reopen for Corrections
            </button>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-4">
            {/* Report Information */}
            <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
              <h2 className="text-base font-semibold text-gray-900 mb-3">Report Information</h2>
              <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Report Date</p>
                <p className="font-semibold flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-gray-400" />
                  {new Date(report.reportDate).toLocaleDateString()}
                </p>
              </div>
              
              <div>
                <p className="text-sm text-gray-600">Voyage ID</p>
                <p className="font-semibold">#{report.voyageId || 'N/A'}</p>
              </div>
              
              <div>
                <p className="text-sm text-gray-600">Prepared By</p>
                <p className="font-semibold flex items-center gap-2">
                  <User className="h-4 w-4 text-gray-400" />
                  {report.preparedBy || 'N/A'}
                </p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">Created</p>
                <p className="font-semibold text-sm">
                  {new Date(report.createdAt).toLocaleString()}
                </p>
              </div>
            </div>
          </div>

          {/* Position Data */}
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-3">Position & Navigation</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-600">Latitude</p>
                <p className="font-semibold text-sm font-mono">{report.latitude?.toFixed(6)}°</p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">Longitude</p>
                <p className="font-semibold text-sm font-mono">{report.longitude?.toFixed(6)}°</p>
              </div>
            </div>
          </div>

          {/* Weather */}
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-3">Weather Conditions</h2>
            <div className="grid grid-cols-3 gap-3">
              <div>
                <p className="text-xs text-gray-600">Condition</p>
                <p className="font-semibold text-sm">{report.weatherConditions || 'N/A'}</p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">Wind</p>
                <p className="font-semibold text-sm">{report.windDirection} {report.windSpeed} kts</p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">Sea State</p>
                <p className="font-semibold text-sm">{report.seaState || 'N/A'}</p>
              </div>
            </div>
          </div>

          {/* Fuel */}
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-3">Fuel Status</h2>
            <div className="grid grid-cols-3 gap-3">
              <div>
                <p className="text-xs text-gray-600">FO ROB</p>
                <p className="font-semibold text-sm">{report.fuelOilROB?.toFixed(1)} MT</p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">DO ROB</p>
                <p className="font-semibold text-sm">{report.dieselOilROB?.toFixed(1)} MT</p>
              </div>
              
              <div>
                <p className="text-xs text-gray-600">FO Consumed</p>
                <p className="font-semibold text-sm">{report.fuelOilConsumed?.toFixed(1)} MT</p>
              </div>
            </div>
          </div>

          {/* Remarks */}
          {report.generalRemarks && (
            <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
              <h2 className="text-base font-semibold text-gray-900 mb-3">General Remarks</h2>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{report.generalRemarks}</p>
            </div>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          {/* Master Signature */}
          {report.masterSignature && (
            <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
              <h3 className="font-semibold text-sm text-gray-900 mb-2">Master Signature</h3>
              <p className="text-sm text-gray-700">{report.masterSignature}</p>
              {report.signedAt && (
                <p className="text-xs text-gray-500 mt-2">
                  Signed: {new Date(report.signedAt).toLocaleString()}
                </p>
              )}
            </div>
          )}

          {/* Transmission Status */}
          {report.isTransmitted && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-4">
              <h3 className="font-semibold text-sm text-green-900 mb-2 flex items-center gap-2">
                <Send className="h-4 w-4" />
                Transmitted
              </h3>
              <p className="text-xs text-green-700">
                {report.transmittedAt && new Date(report.transmittedAt).toLocaleString()}
              </p>
            </div>
          )}

          {/* Workflow History */}
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-200">
            <h3 className="font-semibold text-sm text-gray-900 mb-3 flex items-center gap-2">
              <History className="h-4 w-4" />
              Workflow History
            </h3>
            <div className="space-y-3">
              {history.length === 0 ? (
                <p className="text-xs text-gray-500">No history available</p>
              ) : (
                history.map((entry, idx) => (
                  <div key={idx} className="border-l-2 border-blue-600 pl-3 py-1">
                    <p className="font-semibold text-xs">{entry.fromStatus} → {entry.toStatus}</p>
                    <p className="text-xs text-gray-600">{entry.changedBy}</p>
                    <p className="text-xs text-gray-500">
                      {new Date(entry.changedAt).toLocaleString()}
                    </p>
                    {entry.remarks && (
                      <p className="text-xs text-gray-700 mt-1">{entry.remarks}</p>
                    )}
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Approve Modal */}
      {showApproveModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">Approve Report</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Master Signature <span className="text-red-600">*</span>
                </label>
                <input
                  type="text"
                  value={approvalData.masterSignature}
                  onChange={(e) => setApprovalData({ ...approvalData, masterSignature: e.target.value })}
                  placeholder="Captain name"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  required
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Approval Remarks
                </label>
                <textarea
                  value={approvalData.approvalRemarks}
                  onChange={(e) => setApprovalData({ ...approvalData, approvalRemarks: e.target.value })}
                  rows={3}
                  placeholder="Optional remarks..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowApproveModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleApprove}
                disabled={!approvalData.masterSignature.trim()}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Approve
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">Reject Report</h3>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rejection Reason <span className="text-red-600">*</span>
              </label>
              <textarea
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                rows={4}
                placeholder="Please provide reason for rejection..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                required
              />
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowRejectModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleReject}
                disabled={!rejectionReason.trim()}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Reject
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Transmit Modal */}
      {showTransmitModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-semibold mb-4">Transmit Report</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Transmission Method
                </label>
                <select
                  value={transmitData.transmissionMethod}
                  onChange={(e) => setTransmitData({ ...transmitData, transmissionMethod: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500"
                >
                  <option value="EMAIL">Email</option>
                  <option value="TELEX">Telex</option>
                  <option value="INMARSAT">Inmarsat</option>
                  <option value="VSAT">VSAT</option>
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Recipient Emails <span className="text-red-600">*</span>
                </label>
                <input
                  type="text"
                  value={transmitData.recipientEmails}
                  onChange={(e) => setTransmitData({ ...transmitData, recipientEmails: e.target.value })}
                  placeholder="email1@company.com;email2@company.com"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500"
                  required
                />
                <p className="text-xs text-gray-500 mt-1">Separate multiple emails with semicolon (;)</p>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Transmission Remarks
                </label>
                <textarea
                  value={transmitData.transmissionRemarks}
                  onChange={(e) => setTransmitData({ ...transmitData, transmissionRemarks: e.target.value })}
                  rows={3}
                  placeholder="Optional remarks..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500"
                />
              </div>
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowTransmitModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleTransmit}
                disabled={!transmitData.recipientEmails.trim()}
                className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Transmit
              </button>
            </div>
          </div>
        </div>
      )}
      </div>
    </div>
  );
}
