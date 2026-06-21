/**
 * HSQE Management Dashboard Page
 * ISM Code / SOLAS Compliance
 */

import { useState } from 'react';
import { ShieldCheck, FileText, AlertOctagon, Activity } from 'lucide-react';
import { DocumentControl } from './components/DocumentControl';
import { IncidentManagement } from './components/IncidentManagement';
import { RiskWorkPermits } from './components/RiskWorkPermits';

type TabType = 'documents' | 'incidents' | 'permits';

export function HSQEPage() {
  const [activeTab, setActiveTab] = useState<TabType>('documents');

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-900 transition-colors duration-200">
      {/* Header Area */}
      <div className="bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700 py-6 px-8 shadow-sm">
        <div className="max-w-[1700px] mx-auto flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400 text-xs font-semibold px-2.5 py-1 rounded-full border border-blue-200 dark:border-blue-800 flex items-center gap-1">
                <ShieldCheck className="w-3.5 h-3.5" /> ISM Code & SOLAS Compliant
              </span>
            </div>
            <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight mt-2">
              Quản lý An toàn, Chất lượng & Môi trường (HSQE)
            </h1>
            <p className="text-slate-500 dark:text-slate-400 text-sm mt-1">
              Hệ thống giám sát tài liệu an toàn, báo cáo sự cố hàng hải & phê duyệt giấy phép làm việc trên tàu
            </p>
          </div>
          
          <div className="flex items-center gap-4 bg-slate-100 dark:bg-slate-700/50 p-1.5 rounded-xl border border-slate-200 dark:border-slate-700">
            <button
              onClick={() => setActiveTab('documents')}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-semibold transition-all duration-200 ${
                activeTab === 'documents'
                  ? 'bg-white dark:bg-slate-800 text-blue-600 dark:text-blue-400 shadow-sm'
                  : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200'
              }`}
            >
              <FileText className="w-4 h-4" />
              Kiểm soát Tài liệu
            </button>
            <button
              onClick={() => setActiveTab('incidents')}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-semibold transition-all duration-200 ${
                activeTab === 'incidents'
                  ? 'bg-white dark:bg-slate-800 text-blue-600 dark:text-blue-400 shadow-sm'
                  : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200'
              }`}
            >
              <AlertOctagon className="w-4 h-4" />
              Sự cố & CAPA
            </button>
            <button
              onClick={() => setActiveTab('permits')}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-semibold transition-all duration-200 ${
                activeTab === 'permits'
                  ? 'bg-white dark:bg-slate-800 text-blue-600 dark:text-blue-400 shadow-sm'
                  : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200'
              }`}
            >
              <Activity className="w-4 h-4" />
              Đánh giá Rủi ro & Giấy phép
            </button>
          </div>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="max-w-[1700px] mx-auto py-8 px-4 sm:px-6 lg:px-8">
        <div className="transition-all duration-300">
          {activeTab === 'documents' && <DocumentControl />}
          {activeTab === 'incidents' && <IncidentManagement />}
          {activeTab === 'permits' && <RiskWorkPermits />}
        </div>
      </div>
    </div>
  );
}
