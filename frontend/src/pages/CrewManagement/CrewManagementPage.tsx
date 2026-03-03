/**
 * @deprecated Use CrewListPage instead. This component exists for backward compatibility.
 */
import React from 'react';
import { Navigate } from 'react-router-dom';

export const CrewManagementPage: React.FC = () => {
  return <Navigate to="/crew" replace />;
};
