import React, { useState } from 'react';
import { Users, ShieldCheck, FolderOpen } from 'lucide-react';
import { CrewListPage } from '../CrewManagement/CrewListPage';
import { CertificateTypesTab } from './CertificateTypesTab';
import './CategoryManagementPage.css';

type TabId = 'crew' | 'certificate-types';

const TABS: { id: TabId; label: string; icon: React.ReactNode }[] = [
  { id: 'crew', label: 'Thuyền viên', icon: <Users size={14} /> },
  { id: 'certificate-types', label: 'Loại chứng chỉ', icon: <ShieldCheck size={14} /> },
];

export const CategoryManagementPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabId>('crew');

  return (
    <div className="cat-page">
      {/* Header */}
      <div className="cat-header">
        <FolderOpen size={20} className="cat-header-icon" />
        <h1 className="cat-title">Danh mục</h1>
      </div>

      {/* Tabs */}
      <div className="cat-tabs">
        {TABS.map(tab => (
          <button
            key={tab.id}
            className={`cat-tab-btn${activeTab === tab.id ? ' cat-tab-btn--active' : ''}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.icon}
            {tab.label}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="cat-content">
        {activeTab === 'crew' && <CrewListPage />}
        {activeTab === 'certificate-types' && <CertificateTypesTab />}
      </div>
    </div>
  );
};
