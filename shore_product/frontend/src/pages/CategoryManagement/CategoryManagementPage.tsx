import React from 'react';
import { useSearchParams } from 'react-router-dom';
import { CrewListPage } from '../CrewManagement/CrewListPage';
import { CertificateTypesTab } from './CertificateTypesTab';
import './CategoryManagementPage.css';

type TabId = 'crew' | 'certificate-types';

export const CategoryManagementPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const tabParam = searchParams.get('tab') as TabId | null;
  const activeTab: TabId =
    tabParam && ['crew', 'certificate-types'].includes(tabParam) ? tabParam : 'crew';

  return (
    <div className="cat-page">
      {/* Content */}
      <div className="cat-content">
        {activeTab === 'crew' && <CrewListPage />}
        {activeTab === 'certificate-types' && <CertificateTypesTab />}
      </div>
    </div>
  );
};
