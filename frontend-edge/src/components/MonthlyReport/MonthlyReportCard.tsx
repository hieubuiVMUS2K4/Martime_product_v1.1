/**
 * Monthly Report Card Component
 * Individual card for displaying monthly summary report
 */

import React from 'react';
import { 
  Calendar, TrendingUp, Fuel, Anchor, Ship, Settings, FileText 
} from 'lucide-react';
import { MonthlyReportDto, ViewMode } from '../../types/aggregate-reports.types';

interface MonthlyReportCardProps {
  report: MonthlyReportDto;
  onClick: () => void;
  viewMode: ViewMode;
  monthNames: string[];
}

export const MonthlyReportCard: React.FC<MonthlyReportCardProps> = ({ 
  report, 
  onClick, 
  viewMode,
  monthNames 
}) => {
  const totalFuel = (report.totalFuelOilConsumed || 0) + (report.totalDieselOilConsumed || 0);
  
  if (viewMode === 'list') {
    return (
      <div
        onClick={onClick}
        className="border border-gray-200 rounded-lg p-4 hover:border-purple-500 hover:shadow-md transition-all cursor-pointer bg-white flex items-center gap-4"
      >
        <div className="flex-shrink-0 w-16 h-16 bg-purple-50 rounded-lg flex items-center justify-center">
          <FileText className="w-8 h-8 text-purple-600" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="font-semibold text-purple-700">{report.reportNumber}</span>
            <StatusBadge status={report.status} />
          </div>
          <div className="text-sm text-gray-600 mt-1">
            {monthNames[report.month - 1]} {report.year} • {report.voyagesCompleted || 0} voyages
          </div>
        </div>
        <div className="flex gap-6 text-sm">
          <div className="text-center">
            <div className="text-xs text-gray-500">Distance</div>
            <div className="font-semibold text-gray-900">{report.totalDistance?.toFixed(0) || 0} nm</div>
          </div>
          <div className="text-center">
            <div className="text-xs text-gray-500">Fuel</div>
            <div className="font-semibold text-gray-900">{totalFuel.toFixed(1)} MT</div>
          </div>
          <div className="text-center">
            <div className="text-xs text-gray-500">Speed</div>
            <div className="font-semibold text-gray-900">{report.averageSpeed?.toFixed(1) || 0} kn</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div
      onClick={onClick}
      className="border border-gray-200 rounded-lg p-5 hover:border-purple-500 hover:shadow-lg transition-all cursor-pointer bg-white group"
    >
      <div className="flex justify-between items-start mb-3">
        <div className="flex-1">
          <div className="font-semibold text-purple-700 group-hover:text-purple-800 transition-colors">
            {report.reportNumber}
          </div>
          <div className="text-xs text-gray-500 mt-1">
            {monthNames[report.month - 1]} {report.year}
          </div>
        </div>
        <StatusBadge status={report.status} />
      </div>

      <div className="text-xs text-gray-600 mb-4 pb-4 border-b border-gray-100">
        {new Date(report.monthStartDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} 
        {' - '}
        {new Date(report.monthEndDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
      </div>

      <div className="grid grid-cols-2 gap-3 mb-3">
        <MetricBadge 
          icon={<Anchor className="w-3.5 h-3.5" />}
          label="Distance"
          value={`${report.totalDistance?.toFixed(0) || 0} nm`}
          color="blue"
        />
        <MetricBadge 
          icon={<Fuel className="w-3.5 h-3.5" />}
          label="Fuel"
          value={`${totalFuel.toFixed(1)} MT`}
          color="orange"
        />
        <MetricBadge 
          icon={<TrendingUp className="w-3.5 h-3.5" />}
          label="Avg Speed"
          value={`${report.averageSpeed?.toFixed(1) || 0} kn`}
          color="green"
        />
        <MetricBadge 
          icon={<Ship className="w-3.5 h-3.5" />}
          label="Voyages"
          value={report.voyagesCompleted || 0}
          color="purple"
        />
      </div>

      <div className="grid grid-cols-2 gap-2 text-xs mt-4 pt-4 border-t border-gray-100">
        <div className="flex items-center gap-1 text-gray-600">
          <Calendar className="w-3 h-3" />
          Port calls: {report.totalPortCalls || 0}
        </div>
        <div className="flex items-center gap-1 text-gray-600">
          <Settings className="w-3 h-3" />
          Maintenance: {report.totalMaintenanceCompleted || 0}
        </div>
      </div>

      {report.preparedBy && (
        <div className="text-xs text-gray-500 flex items-center gap-1 mt-3 pt-3 border-t border-gray-100">
          <Settings className="w-3 h-3" />
          Prepared by: {report.preparedBy}
        </div>
      )}
    </div>
  );
};

// ============================================================
// SUB-COMPONENTS
// ============================================================

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const colors = {
    DRAFT: 'bg-gray-100 text-gray-700 border-gray-200',
    SIGNED: 'bg-blue-100 text-blue-700 border-blue-200',
    TRANSMITTED: 'bg-green-100 text-green-700 border-green-200',
    ARCHIVED: 'bg-purple-100 text-purple-700 border-purple-200',
  };

  return (
    <span className={`inline-block px-2 py-0.5 text-xs font-medium rounded border ${colors[status as keyof typeof colors] || colors.DRAFT}`}>
      {status}
    </span>
  );
};

const MetricBadge: React.FC<{ 
  icon: React.ReactNode; 
  label: string; 
  value: string | number; 
  color: string;
}> = ({ icon, label, value, color }) => {
  const colors = {
    blue: 'bg-blue-50 text-blue-600 border-blue-100',
    orange: 'bg-orange-50 text-orange-600 border-orange-100',
    green: 'bg-green-50 text-green-600 border-green-100',
    purple: 'bg-purple-50 text-purple-600 border-purple-100',
  };

  return (
    <div className={`border rounded-md p-2 ${colors[color as keyof typeof colors]}`}>
      <div className="flex items-center gap-1 mb-1 opacity-75">
        {icon}
        <span className="text-xs font-medium">{label}</span>
      </div>
      <div className="text-sm font-bold">{value}</div>
    </div>
  );
};
