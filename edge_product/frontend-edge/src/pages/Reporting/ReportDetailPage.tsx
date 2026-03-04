/**
 * Report Detail Page
 * View report details and perform workflow actions
 * Enhanced with Maintenance & Alarm Summary from integrated modules
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
  RotateCcw,
  Wrench,
  Users,
  Fuel,
  Compass,
  Cloud,
  Anchor,
  TrendingUp,
  Bell,
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
      // Use generic endpoint that auto-detects report type
      const data = await ReportingService.getReportById(id!);
      setReport(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load report');
    } finally {
      setLoading(false);
    }
  };

  const loadWorkflowHistory = async () => {
    try {
      const data = await ReportingService.getWorkflowHistory(id!);
      setHistory(data.history);
    } catch (err) {
      console.error('Failed to load history:', err);
    }
  };

  const handleApprove = async () => {
    try {
      await ReportingService.approveReport(id!, approvalData);
      setShowApproveModal(false);
      await loadReportDetails();
      await loadWorkflowHistory();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to approve report');
    }
  };

  const handleReject = async () => {
    try {
      await ReportingService.rejectReport(id!, rejectionReason);
      setShowRejectModal(false);
      await loadReportDetails();
      await loadWorkflowHistory();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to reject report');
    }
  };

  const handleTransmit = async () => {
    try {
      await ReportingService.transmitReport(id!, {
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
                  await ReportingService.submitReport(id!);
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
                await ReportingService.reopenReport(id!, corrections);
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

        {/* Summary Cards - Crew, Maintenance, Alarms - Minimalist Design */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          {/* Crew Summary Card */}
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-sm text-gray-700 flex items-center gap-2">
                <Users className="h-4 w-4 text-gray-500" />
                Crew & Safety
              </h3>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <p className="text-xs text-gray-500">Crew On Board</p>
                <p className="text-2xl font-bold text-gray-900">{report.crewOnBoard || 0}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Passengers</p>
                <p className="text-2xl font-bold text-gray-900">{report.passengersOnBoard || 0}</p>
              </div>
            </div>
            {report.safetyDrillsConducted && (
              <div className="mt-3 pt-3 border-t border-gray-100">
                <p className="text-xs text-gray-500">Safety Drills</p>
                <p className="text-sm text-gray-700">{report.safetyDrillsConducted}</p>
              </div>
            )}
          </div>

          {/* Maintenance Summary Card */}
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-sm text-gray-700 flex items-center gap-2">
                <Wrench className="h-4 w-4 text-gray-500" />
                Maintenance
              </h3>
            </div>
            {report.maintenanceSummary ? (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <p className="text-xs text-gray-500">Completed (24h)</p>
                    <p className="text-2xl font-bold text-gray-900">{report.maintenanceSummary.tasksCompletedLast24h}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500">In Progress</p>
                    <p className="text-2xl font-bold text-gray-900">{report.maintenanceSummary.tasksInProgress}</p>
                  </div>
                </div>
                <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-2 gap-3">
                  <div>
                    <p className="text-xs text-gray-500">Overdue</p>
                    <p className={`text-lg font-bold ${report.maintenanceSummary.overdueTasks > 0 ? 'text-red-600' : 'text-gray-900'}`}>
                      {report.maintenanceSummary.overdueTasks}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500">Upcoming (7d)</p>
                    <p className="text-lg font-bold text-gray-900">{report.maintenanceSummary.upcomingTasksNext7Days}</p>
                  </div>
                </div>
              </>
            ) : (
              <p className="text-gray-400 text-sm italic">No data available</p>
            )}
          </div>

          {/* Alarm Summary Card */}
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold text-sm text-gray-700 flex items-center gap-2">
                <Bell className="h-4 w-4 text-gray-500" />
                Alarms
              </h3>
            </div>
            {report.alarmSummary ? (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <p className="text-xs text-gray-500">Active</p>
                    <p className="text-2xl font-bold text-gray-900">{report.alarmSummary.activeAlarms}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500">Critical</p>
                    <p className={`text-2xl font-bold ${report.alarmSummary.criticalAlarms > 0 ? 'text-red-600' : 'text-gray-900'}`}>
                      {report.alarmSummary.criticalAlarms}
                    </p>
                  </div>
                </div>
                <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-2 gap-3">
                  <div>
                    <p className="text-xs text-gray-500">Unacknowledged</p>
                    <p className={`text-lg font-bold ${report.alarmSummary.unacknowledgedAlarms > 0 ? 'text-amber-600' : 'text-gray-900'}`}>
                      {report.alarmSummary.unacknowledgedAlarms}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500">Resolved (24h)</p>
                    <p className="text-lg font-bold text-green-600">{report.alarmSummary.resolvedLast24h}</p>
                  </div>
                </div>
              </>
            ) : (
              <p className="text-gray-400 text-sm italic">No data available</p>
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-4">
            {/* Report Information */}
            <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
              <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <FileText className="h-5 w-5 text-blue-600" />
                Report Information
              </h2>
              <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-500">Report Date</p>
                <p className="font-semibold flex items-center gap-2 text-gray-900">
                  <Calendar className="h-4 w-4 text-blue-500" />
                  {new Date(report.reportDate).toLocaleDateString()}
                </p>
              </div>
              
              <div>
                <p className="text-sm text-gray-500">Voyage ID</p>
                <p className="font-semibold text-gray-900">
                  <span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded-md text-sm">
                    #{report.voyageId || 'N/A'}
                  </span>
                </p>
              </div>
              
              <div>
                <p className="text-sm text-gray-500">Prepared By</p>
                <p className="font-semibold flex items-center gap-2 text-gray-900">
                  <User className="h-4 w-4 text-blue-500" />
                  {report.preparedBy || 'N/A'}
                </p>
              </div>
              
              <div>
                <p className="text-sm text-gray-500">Created</p>
                <p className="font-semibold text-sm text-gray-900">
                  {new Date(report.createdAt).toLocaleString()}
                </p>
              </div>
            </div>
          </div>

          {/* Position Data */}
          <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Compass className="h-5 w-5 text-indigo-600" />
              Position & Navigation
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase tracking-wide">Latitude</p>
                <p className="font-semibold text-sm font-mono text-gray-900">{report.latitude?.toFixed(6) || 'N/A'}°</p>
              </div>
              
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase tracking-wide">Longitude</p>
                <p className="font-semibold text-sm font-mono text-gray-900">{report.longitude?.toFixed(6) || 'N/A'}°</p>
              </div>
              
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase tracking-wide">COG</p>
                <p className="font-semibold text-sm text-gray-900">{report.courseOverGround || 'N/A'}°</p>
              </div>
              
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase tracking-wide">SOG</p>
                <p className="font-semibold text-sm text-gray-900">{report.speedOverGround || 'N/A'} kts</p>
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4 mt-4">
              <div className="bg-blue-50 rounded-lg p-3">
                <p className="text-xs text-blue-600 uppercase tracking-wide">Distance Traveled</p>
                <p className="font-semibold text-lg text-blue-900">{report.distanceTraveled || 0} nm</p>
              </div>
              <div className="bg-blue-50 rounded-lg p-3">
                <p className="text-xs text-blue-600 uppercase tracking-wide">Distance To Go</p>
                <p className="font-semibold text-lg text-blue-900">{report.distanceToGo || 0} nm</p>
              </div>
            </div>
          </div>

          {/* Weather */}
          <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Cloud className="h-5 w-5 text-sky-500" />
              Weather Conditions
            </h2>
            <div className="grid grid-cols-3 gap-3">
              <div className="bg-sky-50 rounded-lg p-3 text-center">
                <p className="text-xs text-sky-600 uppercase">Condition</p>
                <p className="font-semibold text-sm text-sky-900 mt-1">{report.weatherConditions || 'N/A'}</p>
              </div>
              
              <div className="bg-sky-50 rounded-lg p-3 text-center">
                <p className="text-xs text-sky-600 uppercase">Wind</p>
                <p className="font-semibold text-sm text-sky-900 mt-1">{report.windDirection || ''} {report.windSpeed || 'N/A'} kts</p>
              </div>
              
              <div className="bg-sky-50 rounded-lg p-3 text-center">
                <p className="text-xs text-sky-600 uppercase">Sea State</p>
                <p className="font-semibold text-sm text-sky-900 mt-1">{report.seaState || 'N/A'}</p>
              </div>
            </div>
            
            <div className="grid grid-cols-4 gap-3 mt-4">
              <div className="text-center">
                <p className="text-xs text-gray-500">Air Temp</p>
                <p className="font-semibold text-gray-900">{report.airTemperature || 'N/A'}°C</p>
              </div>
              <div className="text-center">
                <p className="text-xs text-gray-500">Sea Temp</p>
                <p className="font-semibold text-gray-900">{report.seaTemperature || 'N/A'}°C</p>
              </div>
              <div className="text-center">
                <p className="text-xs text-gray-500">Pressure</p>
                <p className="font-semibold text-gray-900">{report.barometricPressure || 'N/A'} hPa</p>
              </div>
              <div className="text-center">
                <p className="text-xs text-gray-500">Visibility</p>
                <p className="font-semibold text-gray-900">{report.visibility || 'N/A'}</p>
              </div>
            </div>
          </div>

          {/* Fuel - Enhanced */}
          <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Fuel className="h-5 w-5 text-orange-500" />
              Fuel Status
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="bg-orange-50 rounded-lg p-3">
                <p className="text-xs text-orange-600 uppercase tracking-wide">FO ROB</p>
                <p className="font-bold text-lg text-orange-900">{report.fuelOilROB?.toFixed(1) || 'N/A'} MT</p>
              </div>
              
              <div className="bg-orange-50 rounded-lg p-3">
                <p className="text-xs text-orange-600 uppercase tracking-wide">DO ROB</p>
                <p className="font-bold text-lg text-orange-900">{report.dieselOilROB?.toFixed(1) || 'N/A'} MT</p>
              </div>
              
              <div className="bg-red-50 rounded-lg p-3">
                <p className="text-xs text-red-600 uppercase tracking-wide">FO Consumed</p>
                <p className="font-bold text-lg text-red-900">{report.fuelOilConsumed?.toFixed(1) || 'N/A'} MT</p>
              </div>
              
              <div className="bg-red-50 rounded-lg p-3">
                <p className="text-xs text-red-600 uppercase tracking-wide">DO Consumed</p>
                <p className="font-bold text-lg text-red-900">{report.dieselOilConsumed?.toFixed(1) || 'N/A'} MT</p>
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4 mt-4">
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase">Lub Oil ROB</p>
                <p className="font-semibold text-gray-900">{report.lubOilROB?.toFixed(1) || 'N/A'} MT</p>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <p className="text-xs text-gray-500 uppercase">Fresh Water ROB</p>
                <p className="font-semibold text-gray-900">{report.freshWaterROB?.toFixed(1) || 'N/A'} MT</p>
              </div>
            </div>
          </div>

          {/* Engine Parameters */}
          <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-purple-500" />
              Engine Parameters
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="bg-purple-50 rounded-lg p-3">
                <p className="text-xs text-purple-600 uppercase tracking-wide">M/E Hours</p>
                <p className="font-bold text-lg text-purple-900">{report.mainEngineRunningHours || 'N/A'} h</p>
              </div>
              <div className="bg-purple-50 rounded-lg p-3">
                <p className="text-xs text-purple-600 uppercase tracking-wide">M/E RPM</p>
                <p className="font-bold text-lg text-purple-900">{report.mainEngineRPM || 'N/A'}</p>
              </div>
              <div className="bg-purple-50 rounded-lg p-3">
                <p className="text-xs text-purple-600 uppercase tracking-wide">M/E Power</p>
                <p className="font-bold text-lg text-purple-900">{report.mainEnginePower || 'N/A'} kW</p>
              </div>
              <div className="bg-purple-50 rounded-lg p-3">
                <p className="text-xs text-purple-600 uppercase tracking-wide">A/E Hours</p>
                <p className="font-bold text-lg text-purple-900">{report.auxEngineRunningHours || 'N/A'} h</p>
              </div>
            </div>
          </div>

          {/* Cargo */}
          {(report.cargoOnBoard || report.cargoDescription) && (
            <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
              <h2 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Anchor className="h-5 w-5 text-teal-500" />
                Cargo Information
              </h2>
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-teal-50 rounded-lg p-4">
                  <p className="text-xs text-teal-600 uppercase">Cargo On Board</p>
                  <p className="font-bold text-2xl text-teal-900">{report.cargoOnBoard || 0} MT</p>
                </div>
                {report.cargoDescription && (
                  <div className="bg-gray-50 rounded-lg p-4">
                    <p className="text-xs text-gray-500 uppercase">Description</p>
                    <p className="font-semibold text-gray-900">{report.cargoDescription}</p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Remarks Section - Enhanced */}
          <div className="bg-white rounded-xl shadow-sm p-5 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-4">Remarks & Notes</h2>
            <div className="space-y-4">
              {report.operationalRemarks && (
                <div className="bg-blue-50 rounded-lg p-4 border-l-4 border-blue-500">
                  <p className="text-xs font-semibold text-blue-700 uppercase mb-1">Operational</p>
                  <p className="text-sm text-blue-900 whitespace-pre-wrap">{report.operationalRemarks}</p>
                </div>
              )}
              {report.machineryRemarks && (
                <div className="bg-purple-50 rounded-lg p-4 border-l-4 border-purple-500">
                  <p className="text-xs font-semibold text-purple-700 uppercase mb-1">Machinery</p>
                  <p className="text-sm text-purple-900 whitespace-pre-wrap">{report.machineryRemarks}</p>
                </div>
              )}
              {report.cargoRemarks && (
                <div className="bg-teal-50 rounded-lg p-4 border-l-4 border-teal-500">
                  <p className="text-xs font-semibold text-teal-700 uppercase mb-1">Cargo</p>
                  <p className="text-sm text-teal-900 whitespace-pre-wrap">{report.cargoRemarks}</p>
                </div>
              )}
              {report.maintenanceRemarks && (
                <div className="bg-emerald-50 rounded-lg p-4 border-l-4 border-emerald-500">
                  <p className="text-xs font-semibold text-emerald-700 uppercase mb-1">Maintenance</p>
                  <p className="text-sm text-emerald-900 whitespace-pre-wrap">{report.maintenanceRemarks}</p>
                </div>
              )}
              {report.safetyIncidents && (
                <div className="bg-red-50 rounded-lg p-4 border-l-4 border-red-500">
                  <p className="text-xs font-semibold text-red-700 uppercase mb-1">Safety Incidents</p>
                  <p className="text-sm text-red-900 whitespace-pre-wrap">{report.safetyIncidents}</p>
                </div>
              )}
              {report.generalRemarks && (
                <div className="bg-gray-50 rounded-lg p-4 border-l-4 border-gray-400">
                  <p className="text-xs font-semibold text-gray-600 uppercase mb-1">General</p>
                  <p className="text-sm text-gray-700 whitespace-pre-wrap">{report.generalRemarks}</p>
                </div>
              )}
              {!report.operationalRemarks && !report.machineryRemarks && !report.cargoRemarks && 
               !report.maintenanceRemarks && !report.safetyIncidents && !report.generalRemarks && (
                <p className="text-sm text-gray-500 italic">No remarks recorded</p>
              )}
            </div>
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          {/* Master Signature */}
          {report.masterSignature && (
            <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
              <h3 className="font-semibold text-sm text-gray-900 mb-2 flex items-center gap-2">
                <CheckCircle className="h-4 w-4 text-green-600" />
                Master Signature
              </h3>
              <p className="text-sm text-gray-700 font-medium">{report.masterSignature}</p>
              {report.signedAt && (
                <p className="text-xs text-gray-500 mt-2">
                  Signed: {new Date(report.signedAt).toLocaleString()}
                </p>
              )}
            </div>
          )}

          {/* Transmission Status */}
          {report.isTransmitted && (
            <div className="bg-gradient-to-r from-green-500 to-emerald-500 rounded-xl p-4 text-white shadow-lg">
              <h3 className="font-semibold text-sm mb-2 flex items-center gap-2">
                <Send className="h-4 w-4" />
                Successfully Transmitted
              </h3>
              <p className="text-xs text-green-100">
                {report.transmittedAt && new Date(report.transmittedAt).toLocaleString()}
              </p>
            </div>
          )}

          {/* Maintenance Detail Card (Sidebar) */}
          {report.maintenanceSummary && (
            <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
              <h3 className="font-semibold text-sm text-gray-900 mb-3 flex items-center gap-2">
                <Wrench className="h-4 w-4 text-emerald-600" />
                PMS Details
              </h3>
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">Critical Equipment Issues</span>
                  <span className={`text-sm font-bold ${report.maintenanceSummary.criticalEquipmentIssues > 0 ? 'text-red-600' : 'text-gray-700'}`}>
                    {report.maintenanceSummary.criticalEquipmentIssues}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">Spares Used (24h)</span>
                  <span className="text-sm font-semibold text-gray-700">{report.maintenanceSummary.sparesUsedLast24h}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">Total Man-Hours (24h)</span>
                  <span className="text-sm font-semibold text-gray-700">{report.maintenanceSummary.totalManHoursLast24h}h</span>
                </div>
              </div>
            </div>
          )}

          {/* Alarm Detail Card (Sidebar) */}
          {report.alarmSummary && (
            <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
              <h3 className="font-semibold text-sm text-gray-900 mb-3 flex items-center gap-2">
                <Bell className="h-4 w-4 text-amber-600" />
                Alarm Details
              </h3>
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">Warning Alarms</span>
                  <span className="text-sm font-semibold text-amber-600">{report.alarmSummary.warningAlarms}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">Acknowledged</span>
                  <span className="text-sm font-semibold text-gray-700">{report.alarmSummary.acknowledgedAlarms}</span>
                </div>
              </div>
            </div>
          )}

          {/* Workflow History */}
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-200">
            <h3 className="font-semibold text-sm text-gray-900 mb-3 flex items-center gap-2">
              <History className="h-4 w-4 text-blue-600" />
              Workflow History
            </h3>
            <div className="space-y-3">
              {history.length === 0 ? (
                <p className="text-xs text-gray-500 italic">No history available</p>
              ) : (
                history.map((entry, idx) => {
                  const getStatusColor = (status: string) => {
                    switch (status.toUpperCase()) {
                      case 'APPROVED': return 'border-green-500 bg-green-50';
                      case 'REJECTED': return 'border-red-500 bg-red-50';
                      case 'SUBMITTED': return 'border-yellow-500 bg-yellow-50';
                      case 'TRANSMITTED': return 'border-blue-500 bg-blue-50';
                      default: return 'border-gray-400 bg-gray-50';
                    }
                  };
                  return (
                    <div key={idx} className={`border-l-4 pl-3 py-2 rounded-r-lg ${getStatusColor(entry.toStatus)}`}>
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-medium text-gray-500">{entry.fromStatus}</span>
                        <span className="text-gray-400">→</span>
                        <span className="text-xs font-bold text-gray-900">{entry.toStatus}</span>
                      </div>
                      <p className="text-xs text-gray-600 mt-1">{entry.changedBy}</p>
                      <p className="text-xs text-gray-400">
                        {new Date(entry.changedAt).toLocaleString()}
                      </p>
                      {entry.remarks && (
                        <p className="text-xs text-gray-700 mt-1 italic">"{entry.remarks}"</p>
                      )}
                    </div>
                  );
                })
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Approve Modal */}
      {showApproveModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-2xl">
            <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-blue-600" />
              Approve Report
            </h3>
            
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
                  className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
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
                  className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
                />
              </div>
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowApproveModal(false)}
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg hover:bg-gray-50 font-medium transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleApprove}
                disabled={!approvalData.masterSignature.trim()}
                className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium transition-colors"
              >
                Approve
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-2xl">
            <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
              <XCircle className="h-5 w-5 text-red-600" />
              Reject Report
            </h3>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Rejection Reason <span className="text-red-600">*</span>
              </label>
              <textarea
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                rows={4}
                placeholder="Please provide reason for rejection..."
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all"
                required
              />
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowRejectModal(false)}
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg hover:bg-gray-50 font-medium transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleReject}
                disabled={!rejectionReason.trim()}
                className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium transition-colors"
              >
                Reject
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Transmit Modal */}
      {showTransmitModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 max-w-md w-full shadow-2xl">
            <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
              <Send className="h-5 w-5 text-green-600" />
              Transmit Report
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Transmission Method
                </label>
                <select
                  value={transmitData.transmissionMethod}
                  onChange={(e) => setTransmitData({ ...transmitData, transmissionMethod: e.target.value })}
                  className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500 transition-all"
                >
                  <option value="EMAIL">📧 Email</option>
                  <option value="TELEX">📠 Telex</option>
                  <option value="INMARSAT">📡 Inmarsat</option>
                  <option value="VSAT">🛰️ VSAT</option>
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
                  className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500 transition-all"
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
                  className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500 transition-all"
                />
              </div>
            </div>
            
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setShowTransmitModal(false)}
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg hover:bg-gray-50 font-medium transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleTransmit}
                disabled={!transmitData.recipientEmails.trim()}
                className="flex-1 px-4 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium transition-colors"
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
