/**
 * Create Deferral Request Modal
 * For crew to request task deferral
 * PMS Workflow v2.0
 */

import { useState } from 'react';
import { Clock, AlertTriangle, Shield, X } from 'lucide-react';
import { createDeferralRequest, type CreateDeferralDto } from '@/services/maintenance.service';
import type { MaintenanceTask } from '@/types/maintenance.types';
import { format, parseISO, addDays, differenceInDays } from 'date-fns';
import { toast } from 'sonner';

interface CreateDeferralModalProps {
  task: MaintenanceTask;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function CreateDeferralModal({ task, isOpen, onClose, onSuccess }: CreateDeferralModalProps) {
  const [reason, setReason] = useState('');
  const [proposedDate, setProposedDate] = useState('');
  const [classPermissionLetter, setClassPermissionLetter] = useState('');
  const [loading, setLoading] = useState(false);

  // Calculate deferral days
  const currentDueDate = parseISO(task.nextDueAt);
  const deferralDays = proposedDate 
    ? differenceInDays(new Date(proposedDate), currentDueDate) 
    : 0;

  // Check if this is a CMS item and deferral exceeds 90 days
  const requiresClassPermission = task.isCms && deferralDays > 90;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!reason.trim()) {
      toast.error('Please provide a reason for deferral');
      return;
    }
    
    if (!proposedDate) {
      toast.error('Please select a proposed due date');
      return;
    }
    
    if (deferralDays <= 0) {
      toast.error('Proposed date must be after current due date');
      return;
    }
    
    if (requiresClassPermission && !classPermissionLetter.trim()) {
      toast.error('Class permission letter is required for CMS items with deferral > 90 days');
      return;
    }

    try {
      setLoading(true);
      const dto: CreateDeferralDto = {
        taskId: task.id,
        reason: reason.trim(),
        proposedDueDate: proposedDate,
        classPermissionLetter: requiresClassPermission ? classPermissionLetter.trim() : undefined
      };
      
      await createDeferralRequest(dto);
      toast.success('Deferral request submitted');
      onSuccess();
      handleClose();
    } catch (err: any) {
      console.error('Error creating deferral:', err);
      toast.error(err?.response?.data?.error || 'Failed to submit deferral request');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setReason('');
    setProposedDate('');
    setClassPermissionLetter('');
    onClose();
  };

  // Pre-fill suggested dates (7, 14, 30 days)
  const suggestedDates = [7, 14, 30].map(days => ({
    days,
    date: format(addDays(currentDueDate, days), 'yyyy-MM-dd'),
    label: `+${days} days`
  }));

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <Clock className="w-6 h-6 text-yellow-500" />
              <h3 className="text-lg font-semibold text-gray-900">Request Deferral</h3>
            </div>
            <button
              onClick={handleClose}
              className="p-1 hover:bg-gray-100 rounded-full transition-colors"
            >
              <X className="w-5 h-5 text-gray-500" />
            </button>
          </div>

          {/* Task Info */}
          <div className="bg-gray-50 rounded-lg p-4 mb-4">
            <div className="space-y-2 text-sm">
              <div>
                <span className="font-medium">Task ID:</span> {task.taskId}
              </div>
              <div>
                <span className="font-medium">Equipment:</span> {task.equipmentGroupName || task.equipmentName}
              </div>
              <div className="flex items-center gap-2">
                <span className="font-medium">Current Due Date:</span> 
                <span className="text-amber-600 font-medium">
                  {format(currentDueDate, 'dd MMM yyyy')}
                </span>
              </div>
              {task.isCms && (
                <div className="flex items-center gap-1 text-blue-700 mt-2 pt-2 border-t border-gray-200">
                  <Shield className="w-4 h-4" />
                  <span className="font-medium">CMS Item</span>
                  <span className="text-xs text-gray-500">(Class Maintenance Survey)</span>
                </div>
              )}
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            {/* Reason */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Reason for Deferral *
              </label>
              <textarea
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="e.g., Awaiting spare parts delivery, vessel in port operations, bad weather conditions..."
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-yellow-500 focus:border-transparent"
                required
              />
            </div>

            {/* Proposed Date */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Proposed Due Date *
              </label>
              
              {/* Quick select buttons */}
              <div className="flex gap-2 mb-2">
                {suggestedDates.map(({ days, date, label }) => (
                  <button
                    key={days}
                    type="button"
                    onClick={() => setProposedDate(date)}
                    className={`px-3 py-1.5 text-sm rounded-md border transition-colors ${
                      proposedDate === date
                        ? 'bg-yellow-500 text-white border-yellow-500'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>
              
              <input
                type="date"
                value={proposedDate}
                onChange={(e) => setProposedDate(e.target.value)}
                min={format(addDays(currentDueDate, 1), 'yyyy-MM-dd')}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-yellow-500 focus:border-transparent"
                required
              />
              
              {deferralDays > 0 && (
                <p className="mt-1 text-sm text-gray-600">
                  Deferral period: <span className="font-medium text-amber-600">+{deferralDays} days</span>
                </p>
              )}
            </div>

            {/* CMS Warning & Class Permission */}
            {task.isCms && (
              <div className={`mb-4 p-3 rounded-lg border ${
                deferralDays > 90 
                  ? 'bg-red-50 border-red-200' 
                  : 'bg-blue-50 border-blue-200'
              }`}>
                <div className="flex items-start gap-2">
                  <AlertTriangle className={`w-5 h-5 ${
                    deferralDays > 90 ? 'text-red-600' : 'text-blue-600'
                  }`} />
                  <div className="flex-1">
                    <p className={`text-sm font-medium ${
                      deferralDays > 90 ? 'text-red-800' : 'text-blue-800'
                    }`}>
                      CMS Item Deferral
                    </p>
                    <p className={`text-sm ${
                      deferralDays > 90 ? 'text-red-700' : 'text-blue-700'
                    }`}>
                      {deferralDays > 90 
                        ? 'Deferral exceeds 90 days - Class permission letter is required'
                        : 'Deferral up to 90 days allowed without Class permission'
                      }
                    </p>
                  </div>
                </div>
                
                {requiresClassPermission && (
                  <div className="mt-3">
                    <label className="block text-sm font-medium text-red-800 mb-2">
                      Class Permission Letter Reference *
                    </label>
                    <input
                      type="text"
                      value={classPermissionLetter}
                      onChange={(e) => setClassPermissionLetter(e.target.value)}
                      placeholder="e.g., DNV-GL/2024/ABC123"
                      className="w-full px-3 py-2 border border-red-300 rounded-md focus:ring-2 focus:ring-red-500 focus:border-transparent"
                      required
                    />
                  </div>
                )}
              </div>
            )}

            {/* Info */}
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3 mb-4">
              <p className="text-sm text-yellow-800">
                Your deferral request will be sent to the Master/C/E for review. 
                You will be notified once it's approved or rejected.
              </p>
            </div>

            {/* Actions */}
            <div className="flex gap-3">
              <button
                type="button"
                onClick={handleClose}
                disabled={loading}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={loading || !reason.trim() || !proposedDate || deferralDays <= 0}
                className="flex-1 px-4 py-2 bg-yellow-500 text-white rounded-md hover:bg-yellow-600 disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {loading ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    Submitting...
                  </>
                ) : (
                  <>
                    <Clock className="w-4 h-4" />
                    Submit Request
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
