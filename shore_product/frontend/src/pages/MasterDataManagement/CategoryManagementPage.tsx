import React from 'react';
import { useSearchParams } from 'react-router-dom';
import { CrewListPage } from './Crew/CrewListPage';
import { CertificateTypesTab } from './CertificateTypes/CertificateTypesTab';
import { MaterialCatalogPage } from './Materials/MaterialCatalogPage';
import { RankPage } from './Ranks/RankPage';
import { CountryPage } from './Countries/CountryPage';
import './CategoryManagementPage.css';

type TabId = 'crew' | 'certificate-types' | 'materials' | 'ranks' | 'countries';

const VALID_TABS: TabId[] = ['crew', 'certificate-types', 'materials', 'ranks', 'countries'];

export const CategoryManagementPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const tabParam = searchParams.get('tab') as TabId | null;
  const activeTab: TabId = tabParam && VALID_TABS.includes(tabParam) ? tabParam : 'crew';

  return (
    <div className="cat-page">
      {/* Content */}
      <div className="cat-content">
        {activeTab === 'crew' && <CrewListPage />}
        {activeTab === 'certificate-types' && <CertificateTypesTab />}
        {activeTab === 'materials' && <MaterialCatalogPage />}
        {activeTab === 'ranks' && <RankPage />}
        {activeTab === 'countries' && <CountryPage />}
      </div>
    </div>
  );
};
