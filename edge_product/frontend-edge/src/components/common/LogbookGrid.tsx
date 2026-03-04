import React from 'react';

interface LogbookGridProps {
  children: React.ReactNode;
  title?: string;
  actions?: React.ReactNode;
}

export const LogbookGrid: React.FC<LogbookGridProps> = ({ children, title, actions }) => {
  return (
    <div className="bg-white min-h-screen p-6">
      <div className="flex justify-between items-center mb-6 border-b border-gray-200 pb-4">
        {title && (
          <h1 className="text-2xl font-bold text-gray-900">
            {title}
          </h1>
        )}
        <div className="flex gap-2">
          {actions}
        </div>
      </div>
      <div className="grid gap-4">
        {children}
      </div>
    </div>
  );
};
