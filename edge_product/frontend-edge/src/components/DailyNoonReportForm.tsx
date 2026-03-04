/**
 * Daily Noon Report Form Wrapper
 * Re-exports existing NoonReportForm from pages for unified reporting interface
 */

import React from 'react';
import { NoonReportForm } from '../pages/Reporting/NoonReportForm';

const DailyNoonReportForm: React.FC = () => {
  return <NoonReportForm />;
};

export default DailyNoonReportForm;
