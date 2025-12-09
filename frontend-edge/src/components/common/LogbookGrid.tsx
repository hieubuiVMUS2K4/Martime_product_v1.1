import React from 'react';

interface LogbookGridProps {
  children: React.ReactNode;
  title?: string;
  actions?: React.ReactNode;
}

export const LogbookGrid: React.FC<LogbookGridProps> = ({ children, title, actions }) => {
  return (
    <div className="bg-industrial-bg min-h-screen p-4">
      <div className="flex justify-between items-center mb-6 border-b border-industrial-border pb-4">
        {title && (
          <h1 className="text-2xl font-mono text-industrial-text-amber uppercase tracking-widest">
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
