import React from 'react';
import type { WorkStatus } from '../../../types/work.types';
import './StatusBadge.css';

export interface StatusBadgeProps {
  status: WorkStatus;
  size?: 'sm' | 'md' | 'lg';
}

const statusLabels: Record<WorkStatus, string> = {
  pending: 'Chờ duyệt',
  approved: 'Đã duyệt',
  'in-progress': 'Đang thực hiện',
  completed: 'Đã hoàn thành',
  overdue: 'Quá hạn',
};

export const StatusBadge: React.FC<StatusBadgeProps> = ({ 
  status, 
  size = 'md' 
}) => {
  return (
    <span className={`status-badge status-${status} status-${size}`}>
      {statusLabels[status]}
    </span>
  );
};
