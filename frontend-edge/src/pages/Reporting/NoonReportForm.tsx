/**
 * Noon Report Creation/Edit Form
 * IMO/SOLAS Compliant Maritime Reporting
 */

import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { 
  Ship, 
  MapPin, 
  Cloud, 
  Fuel, 
  Gauge, 
  Anchor,
  Save,
  Send,
  AlertTriangle,
  Wrench,
  CheckCircle2
} from 'lucide-react';
import { ReportingService } from '../../services/reporting.service';
import { getTasksCompletedLast24Hours, calculateManHours, toTaskSummary } from '../../services/maintenance.service';
import type { CreateNoonReportDto } from '../../types/reporting.types';
import type { TaskSummary } from '../../types/maintenance.types';

export function NoonReportForm() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>(); // Get ID from URL if editing
  const isEditMode = !!id; // Edit mode if ID exists
  
  const [loading, setLoading] = useState(false);
  const [loadingReport, setLoadingReport] = useState(isEditMode);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<string[]>([]);
  const [lastAutoSave, setLastAutoSave] = useState<Date | null>(null);
  
  // Daily Tasks Summary
  const [completedTasks, setCompletedTasks] = useState<TaskSummary[]>([]);
  const [loadingTasks, setLoadingTasks] = useState(true);
  const [totalManHours, setTotalManHours] = useState(0);

  const [formData, setFormData] = useState<CreateNoonReportDto>({
    reportDate: new Date().toISOString().split('T')[0],
    voyageId: 0,
    
    // Position
    latitude: 0,
    longitude: 0,
    courseOverGround: 0,
    speedOverGround: 0,
    
    // Weather
    weatherConditions: 'FAIR',
    windDirection: 'N',
    windSpeed: 0,
    seaState: 'CALM',
    visibility: 'GOOD',
    airTemperature: 0,
    seaTemperature: 0,
    barometricPressure: 1013.25,
    
    // Distance
    distanceTraveled: 0,
    distanceToGo: 0,
    
    // Fuel
    fuelOilROB: 0,
    dieselOilROB: 0,
    fuelOilConsumed: 0,
    dieselOilConsumed: 0,
    
    // Engine
    mainEngineRunningHours: 0,
    auxEngineRunningHours: 0,
    
    // Cargo
    cargoOnBoard: 0,
    
    // Remarks
    generalRemarks: '',
    preparedBy: ''
  });
  
  // Load completed tasks from last 24 hours on mount
  useEffect(() => {
    const loadCompletedTasks = async () => {
      try {
        setLoadingTasks(true);
        const tasks = await getTasksCompletedLast24Hours();
        
        if (!tasks || tasks.length === 0) {
          console.log('ℹ️ No completed maintenance tasks found in last 24 hours');
          return;
        }
        
        const taskSummaries = tasks.map(toTaskSummary);
        setCompletedTasks(taskSummaries);
        
        const manHours = calculateManHours(tasks);
        setTotalManHours(manHours);
        
        // Store task IDs in form data
        const taskIds = tasks.map(t => t.taskId);
        handleChange('completedTaskIds', taskIds);
        handleChange('totalManHours', manHours);
        
        console.log(`✅ Loaded ${tasks.length} completed maintenance tasks`);
      } catch (err) {
        console.error('⚠️ Failed to load completed tasks:', err);
        // Don't block form if tasks fail to load - user can still submit report
        // Set empty arrays to prevent undefined errors
        setCompletedTasks([]);
        setTotalManHours(0);
      } finally {
        setLoadingTasks(false);
      }
    };
    
    loadCompletedTasks();
  }, []);

  // Load existing report data if in edit mode
  useEffect(() => {
    if (!isEditMode || !id) return;
    
    const loadReportData = async () => {
      try {
        setLoadingReport(true);
        const report = await ReportingService.getNoonReport(parseInt(id));
        
        // Populate form with existing data
        setFormData({
          reportDate: report.reportDate.split('T')[0],
          voyageId: report.voyageId,
          latitude: report.latitude,
          longitude: report.longitude,
          courseOverGround: report.courseOverGround,
          speedOverGround: report.speedOverGround,
          weatherConditions: report.weatherConditions,
          windDirection: report.windDirection,
          windSpeed: report.windSpeed,
          seaState: report.seaState,
          visibility: report.visibility,
          airTemperature: report.airTemperature,
          seaTemperature: report.seaTemperature,
          barometricPressure: report.barometricPressure,
          distanceTraveled: report.distanceTraveled,
          fuelOilConsumed: report.fuelOilConsumed,
          fuelOilROB: report.fuelOilROB,
          dieselOilConsumed: report.dieselOilConsumed,
          dieselOilROB: report.dieselOilROB,
          mainEngineRunningHours: report.mainEngineRunningHours,
          auxEngineRunningHours: report.auxEngineRunningHours,
          cargoOnBoard: report.cargoOnBoard,
          generalRemarks: report.generalRemarks || '',
          preparedBy: report.preparedBy || '',
          completedTaskIds: report.completedTaskIds || [],
          totalManHours: report.totalManHours || 0,
        });
        
        console.log('✅ Loaded report data for editing');
      } catch (err) {
        console.error('❌ Failed to load report:', err);
        setError('Failed to load report data');
      } finally {
        setLoadingReport(false);
      }
    };
    
    loadReportData();
  }, [isEditMode, id]);

  // Auto-save draft to localStorage every 1 minute
  useEffect(() => {
    const AUTOSAVE_KEY = `draft-noon-${formData.reportDate}`;
    
    // Load saved draft on mount
    const loadSavedDraft = () => {
      try {
        const saved = localStorage.getItem(AUTOSAVE_KEY);
        if (saved) {
          const shouldLoad = window.confirm(
            'Found an auto-saved draft from a previous session. Load it?'
          );
          if (shouldLoad) {
            const parsed = JSON.parse(saved);
            setFormData(parsed);
            console.log('✅ Loaded auto-saved draft');
          } else {
            localStorage.removeItem(AUTOSAVE_KEY);
          }
        }
      } catch (err) {
        console.error('Failed to load auto-saved draft:', err);
      }
    };
    
    loadSavedDraft();

    // Auto-save interval (every 60 seconds)
    const autoSaveInterval = setInterval(() => {
      // Only auto-save if form has meaningful data
      if (formData.voyageId && formData.voyageId !== 0) {
        try {
          localStorage.setItem(AUTOSAVE_KEY, JSON.stringify(formData));
          setLastAutoSave(new Date());
          console.log('💾 Auto-saved draft at', new Date().toLocaleTimeString());
        } catch (err) {
          console.error('Auto-save failed:', err);
        }
      }
    }, 60000); // 60 seconds

    return () => {
      clearInterval(autoSaveInterval);
    };
  }, [formData]);

  const handleChange = (field: keyof CreateNoonReportDto, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const validateForm = (): boolean => {
    const errors: string[] = [];
    
    // Required fields
    if (!formData.voyageId || formData.voyageId === 0) {
      errors.push('Voyage ID is required');
    }
    
    if (!formData.preparedBy?.trim()) {
      errors.push('Prepared By is required');
    }
    
    // GPS validation
    if (formData.latitude !== undefined && (formData.latitude < -90 || formData.latitude > 90)) {
      errors.push('Latitude must be between -90 and 90');
    }
    
    if (formData.longitude !== undefined && (formData.longitude < -180 || formData.longitude > 180)) {
      errors.push('Longitude must be between -180 and 180');
    }
    
    // Null Island check
    if (formData.latitude === 0 && formData.longitude === 0) {
      errors.push('Invalid position (Null Island) - please enter actual coordinates');
    }
    
    // Speed validation
    if (formData.speedOverGround !== undefined && (formData.speedOverGround < 0 || formData.speedOverGround > 40)) {
      errors.push('Speed must be between 0 and 40 knots');
    }
    
    // Course validation
    if (formData.courseOverGround !== undefined && (formData.courseOverGround < 0 || formData.courseOverGround > 360)) {
      errors.push('Course must be between 0 and 360 degrees');
    }
    
    // Fuel validation
    if (formData.fuelOilROB !== undefined && formData.fuelOilROB < 0) {
      errors.push('Fuel Oil ROB cannot be negative');
    }
    
    if (formData.fuelOilConsumed !== undefined && formData.fuelOilConsumed < 0) {
      errors.push('Fuel Oil Consumed cannot be negative');
    }
    
    setValidationErrors(errors);
    return errors.length === 0;
  };

  const handleSubmit = async (asDraft: boolean = false) => {
    if (!asDraft && !validateForm()) {
      return;
    }
    
    try {
      setLoading(true);
      setError(null);
      
      let reportId: number;
      
      if (isEditMode && id) {
        // UPDATE existing report
        await ReportingService.updateNoonReport(parseInt(id), formData);
        reportId = parseInt(id);
        console.log('✅ Report updated successfully');
      } else {
        // CREATE new report
        const report = await ReportingService.createNoonReport(formData);
        reportId = report.reportId;
        console.log('✅ Report created successfully');
      }
      
      // Auto-submit if not draft
      if (!asDraft) {
        await ReportingService.submitReport(reportId);
        console.log('✅ Report submitted for approval');
      }
      
      // Clear auto-saved draft on successful submission
      const AUTOSAVE_KEY = `draft-noon-${formData.reportDate}`;
      localStorage.removeItem(AUTOSAVE_KEY);
      console.log('✅ Cleared auto-saved draft');
      
      navigate('/reporting/reports');
    } catch (err: any) {
      console.error('Failed to save Noon Report:', err);
      
      // User-friendly error messages
      let errorMsg = 'Không thể tạo Noon Report';
      
      if (err?.response?.data?.error) {
        errorMsg = err.response.data.error;
      } else if (err.message) {
        const msg = err.message.toLowerCase();
        
        if (msg.includes('validation') || msg.includes('invalid')) {
          errorMsg = '❌ Dữ liệu không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc (có dấu *)';
        } else if (msg.includes('duplicate')) {
          errorMsg = `⚠️ Đã tồn tại Noon Report cho ngày ${formData.reportDate}.\n\nVui lòng chỉnh sửa báo cáo hiện tại thay vì tạo mới.`;
        } else if (msg.includes('timeout') || msg.includes('network')) {
          errorMsg = '🌐 Lỗi kết nối mạng. Báo cáo đã được lưu nháp tự động, bạn có thể gửi lại sau.';
        } else if (msg.includes('401') || msg.includes('unauthorized')) {
          errorMsg = '🔒 Phiên đăng nhập đã hết hạn. Dữ liệu đã được lưu nháp, vui lòng đăng nhập lại.';
        } else if (msg.includes('403') || msg.includes('forbidden')) {
          errorMsg = '⛔ Bạn không có quyền tạo báo cáo. Vui lòng liên hệ Chief Officer hoặc Captain.';
        } else if (msg.includes('500') || msg.includes('internal')) {
          errorMsg = '⚠️ Lỗi máy chủ. Dữ liệu đã được lưu nháp tự động, vui lòng thử lại sau.';
        } else {
          errorMsg = err.message;
        }
      }
      
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-5xl mx-auto p-4 sm:p-6">
        {/* Compact Header */}
        <div className="mb-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                <Ship className="h-6 w-6 text-blue-600" />
                {isEditMode ? 'Edit Noon Report' : 'Create Noon Report'}
                {isEditMode && (
                  <span className="text-xs font-medium text-amber-600 bg-amber-50 px-2 py-1 rounded">
                    ✏️ Draft
                  </span>
                )}
              </h1>
              <p className="text-xs text-gray-500 mt-1">
                Daily position report at 12:00 Local Time (LT)
              </p>
            </div>
            
            {/* Auto-save indicator */}
            {lastAutoSave && (
              <div className="text-xs text-gray-500 flex items-center gap-1.5">
                <Save className="h-3.5 w-3.5 text-green-600" />
                <span>Saved {lastAutoSave.toLocaleTimeString()}</span>
              </div>
            )}
          </div>
        </div>

        {/* Compact Validation Errors */}
        {validationErrors.length > 0 && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
            <div className="flex items-start gap-2">
              <AlertTriangle className="h-4 w-4 text-red-600 mt-0.5 flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-semibold text-red-900 mb-1">Validation Errors</h3>
                <ul className="list-disc list-inside space-y-0.5 text-xs text-red-700">
                  {validationErrors.map((err, idx) => (
                    <li key={idx}>{err}</li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        )}

        {/* Compact Loading */}
        {loadingReport && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 mb-4">
            <div className="flex items-center gap-2">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
              <p className="text-sm text-blue-700">Loading report data...</p>
            </div>
          </div>
        )}

        {/* Compact Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-3 py-2 rounded-lg mb-4">
            <p className="text-sm font-semibold">Error</p>
            <p className="text-xs mt-0.5">{error}</p>
          </div>
        )}

        <form className="space-y-4">
          {/* Report Info - Compact */}
          <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
            <h2 className="text-base font-semibold text-gray-900 mb-3">Report Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Report Date <span className="text-red-600">*</span>
                </label>
                <input
                  type="date"
                  value={formData.reportDate}
                  onChange={(e) => handleChange('reportDate', e.target.value)}
                  className="w-full px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>
              
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Voyage ID <span className="text-red-600">*</span>
                </label>
                <input
                  type="number"
                  value={formData.voyageId || ''}
                  onChange={(e) => handleChange('voyageId', parseInt(e.target.value) || 0)}
                  className="w-full px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  min="1"
                />
              </div>
              
              <div className="md:col-span-2">
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Prepared By <span className="text-red-600">*</span>
              </label>
              <input
                type="text"
                value={formData.preparedBy}
                onChange={(e) => handleChange('preparedBy', e.target.value)}
                placeholder="Officer name"
                className="w-full px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>
          </div>
        </div>

        {/* Position Section - Compact */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-1.5">
            <MapPin className="h-3.5 w-3.5 text-blue-600" />
            Position & Navigation
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Latitude (°) <span className="text-red-600">*</span>
              </label>
              <input
                type="number"
                step="0.000001"
                value={formData.latitude}
                onChange={(e) => handleChange('latitude', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g. 10.762622"
                required
              />
              <p className="text-xs text-gray-500 mt-0.5">Range: -90 to +90</p>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Longitude (°) <span className="text-red-600">*</span>
              </label>
              <input
                type="number"
                step="0.000001"
                value={formData.longitude}
                onChange={(e) => handleChange('longitude', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g. 106.660172"
                required
              />
              <p className="text-xs text-gray-500 mt-0.5">Range: -180 to +180</p>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Course Over Ground (°)
              </label>
              <input
                type="number"
                step="1"
                value={formData.courseOverGround}
                onChange={(e) => handleChange('courseOverGround', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
                max="360"
              />
              <p className="text-xs text-gray-500 mt-1">0-360 degrees</p>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Speed Over Ground (knots)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.speedOverGround}
                onChange={(e) => handleChange('speedOverGround', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
                max="40"
              />
            </div>
          </div>
        </div>

        {/* Weather Section */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-2">
            <Cloud className="h-5 w-5 text-blue-600" />
            Weather Conditions
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Weather
              </label>
              <select
                value={formData.weatherConditions}
                onChange={(e) => handleChange('weatherConditions', e.target.value)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="CLEAR">Clear</option>
                <option value="FAIR">Fair</option>
                <option value="CLOUDY">Cloudy</option>
                <option value="RAIN">Rain</option>
                <option value="STORM">Storm</option>
                <option value="FOG">Fog</option>
              </select>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Wind Direction
              </label>
              <select
                value={formData.windDirection}
                onChange={(e) => handleChange('windDirection', e.target.value)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="N">N</option>
                <option value="NE">NE</option>
                <option value="E">E</option>
                <option value="SE">SE</option>
                <option value="S">S</option>
                <option value="SW">SW</option>
                <option value="W">W</option>
                <option value="NW">NW</option>
              </select>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Wind Speed (knots)
              </label>
              <input
                type="number"
                step="1"
                value={formData.windSpeed}
                onChange={(e) => handleChange('windSpeed', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Sea State
              </label>
              <select
                value={formData.seaState}
                onChange={(e) => handleChange('seaState', e.target.value)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="CALM">Calm (0-1)</option>
                <option value="SLIGHT">Slight (2-3)</option>
                <option value="MODERATE">Moderate (4-5)</option>
                <option value="ROUGH">Rough (6-7)</option>
                <option value="VERY_ROUGH">Very Rough (8-9)</option>
              </select>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Visibility
              </label>
              <select
                value={formData.visibility}
                onChange={(e) => handleChange('visibility', e.target.value)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="EXCELLENT">Excellent (&gt;10 nm)</option>
                <option value="GOOD">Good (5-10 nm)</option>
                <option value="MODERATE">Moderate (2-5 nm)</option>
                <option value="POOR">Poor (&lt;2 nm)</option>
              </select>
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Barometric Pressure (hPa)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.barometricPressure}
                onChange={(e) => handleChange('barometricPressure', parseFloat(e.target.value) || 1013.25)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Distance Section */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-2">
            <Anchor className="h-5 w-5 text-blue-600" />
            Distance & Speed
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Distance Traveled (nm)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.distanceTraveled}
                onChange={(e) => handleChange('distanceTraveled', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Distance To Go (nm)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.distanceToGo}
                onChange={(e) => handleChange('distanceToGo', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Fuel Section */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-2">
            <Fuel className="h-5 w-5 text-blue-600" />
            Fuel Status
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Fuel Oil ROB (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.fuelOilROB}
                onChange={(e) => handleChange('fuelOilROB', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Diesel Oil ROB (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.dieselOilROB}
                onChange={(e) => handleChange('dieselOilROB', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Fuel Oil Consumed (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.fuelOilConsumed}
                onChange={(e) => handleChange('fuelOilConsumed', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Diesel Oil Consumed (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.dieselOilConsumed}
                onChange={(e) => handleChange('dieselOilConsumed', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
          </div>
        </div>

        {/* Engine Section */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-2">
            <Gauge className="h-5 w-5 text-blue-600" />
            Engine Running Hours
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Main Engine (hrs)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.mainEngineRunningHours}
                onChange={(e) => handleChange('mainEngineRunningHours', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Auxiliary Engine (hrs)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.auxEngineRunningHours}
                onChange={(e) => handleChange('auxEngineRunningHours', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
          </div>
        </div>

        {/* Cargo Section */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2">Cargo Information</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Cargo On Board (MT)
              </label>
              <input
                type="number"
                step="0.1"
                value={formData.cargoOnBoard}
                onChange={(e) => handleChange('cargoOnBoard', parseFloat(e.target.value) || 0)}
                className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                min="0"
              />
            </div>
          </div>
        </div>

        {/* Daily Tasks Summary - ISM Code Compliance */}
        <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg shadow-sm p-6 border border-blue-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2 flex items-center gap-2">
            <Wrench className="h-5 w-5 text-blue-600" />
            Daily Maintenance Tasks (Last 24 Hours)
            <span className="text-xs font-normal text-gray-500 ml-2">ISM Code - PMS</span>
          </h2>
          
          {loadingTasks ? (
            <div className="flex items-center justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <span className="ml-3 text-gray-600">Loading completed tasks...</span>
            </div>
          ) : completedTasks.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <Wrench className="h-12 w-12 mx-auto mb-3 text-gray-300" />
              <p>No maintenance tasks completed in the last 24 hours</p>
            </div>
          ) : (
            <>
              {/* Summary Statistics */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-2 mb-6">
                <div className="bg-white rounded-lg p-4 border border-blue-200">
                  <div className="text-sm text-gray-600">Total Tasks</div>
                  <div className="text-2xl font-bold text-blue-600">{completedTasks.length}</div>
                </div>
                <div className="bg-white rounded-lg p-4 border border-blue-200">
                  <div className="text-sm text-gray-600">Total Man-Hours</div>
                  <div className="text-2xl font-bold text-green-600">{totalManHours.toFixed(1)} hrs</div>
                </div>
                <div className="bg-white rounded-lg p-4 border border-blue-200">
                  <div className="text-sm text-gray-600">Avg Duration</div>
                  <div className="text-2xl font-bold text-purple-600">
                    {(totalManHours / completedTasks.length).toFixed(1)} hrs
                  </div>
                </div>
              </div>

              {/* Tasks List */}
              <div className="space-y-3">
                {completedTasks.map((task) => (
                  <div 
                    key={task.taskId} 
                    className="bg-white rounded-lg p-4 border border-gray-200 hover:border-blue-300 transition-colors"
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <CheckCircle2 className="h-5 w-5 text-green-500" />
                          <span className="font-semibold text-gray-900">{task.equipmentName}</span>
                          <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${
                            task.priority === 'CRITICAL' ? 'bg-red-100 text-red-800' :
                            task.priority === 'HIGH' ? 'bg-orange-100 text-orange-800' :
                            task.priority === 'NORMAL' ? 'bg-blue-100 text-blue-800' :
                            'bg-gray-100 text-gray-800'
                          }`}>
                            {task.priority}
                          </span>
                        </div>
                        
                        <p className="text-sm text-gray-700 mb-2">{task.taskDescription}</p>
                        
                        <div className="flex flex-wrap gap-2 text-xs text-gray-500">
                          <span>👤 {task.completedBy}</span>
                          <span>⏱️ {task.duration.toFixed(1)} hrs</span>
                          <span>📅 {new Date(task.completedAt).toLocaleString('en-GB', {
                            day: '2-digit',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit'
                          })}</span>
                        </div>
                        
                        {task.sparePartsUsed && (
                          <div className="mt-2 text-xs text-gray-600 bg-gray-50 px-3 py-1.5 rounded">
                            <span className="font-medium">Spare Parts: </span>
                            {task.sparePartsUsed}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-800">
                <strong>Note:</strong> Tasks are automatically linked to this Noon Report for ISM Code compliance and maintenance record-keeping.
              </div>
            </>
          )}
        </div>

        {/* Remarks */}
        <div className="bg-white rounded-lg shadow-sm p-2 border border-gray-200">
          <h2 className="text-sm font-semibold text-gray-900 mb-2">General Remarks</h2>
          <textarea
            value={formData.generalRemarks}
            onChange={(e) => handleChange('generalRemarks', e.target.value)}
            rows={4}
            placeholder="Any additional information..."
            className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>

        {/* Action Buttons */}
        <div className="flex gap-2 justify-end">
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
    </div>
  );
}
