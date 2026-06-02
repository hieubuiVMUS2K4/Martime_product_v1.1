import React from 'react';
// @ts-ignore
import { FixedSizeList as List } from 'react-window';

export interface Column<T> {
  key: string;
  header: React.ReactNode;
  width: string; // e.g. '100px', '2fr', '10%', etc.
  render?: (item: T, index: number) => React.ReactNode;
  className?: string;
  headerClassName?: string;
}

interface VirtualizedTableProps<T> {
  data: T[];
  columns: Column<T>[];
  height?: number; // Height of the list container in pixels
  rowHeight?: number; // Height of each row in pixels
  headerHeight?: number; // Height of the header in pixels
  onRowClick?: (item: T) => void;
  isLoading?: boolean;
  emptyMessage?: string;
  className?: string;
}

export function VirtualizedTable<T>({
  data,
  columns,
  height = 500,
  rowHeight = 52,
  headerHeight = 44,
  onRowClick,
  isLoading = false,
  emptyMessage = 'Không có dữ liệu',
  className = '',
}: VirtualizedTableProps<T>) {
  // Combine all widths into a CSS grid template columns string
  const gridTemplateColumns = columns.map((col) => col.width).join(' ');

  // Render header row
  const renderHeader = () => (
    <div
      className="grid border-b border-slate-200 dark:border-gray-700 bg-slate-50 dark:bg-gray-800 text-slate-700 dark:text-slate-200 font-semibold text-xs uppercase tracking-wider sticky top-0 z-10"
      style={{
        gridTemplateColumns,
        height: `${headerHeight}px`,
        alignItems: 'center',
      }}
    >
      {columns.map((col, index) => (
        <div
          key={col.key || index}
          className={`px-4 py-2 truncate ${col.headerClassName || ''}`}
        >
          {col.header}
        </div>
      ))}
    </div>
  );

  // Loading indicator
  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-slate-500 dark:text-slate-400">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-teal-500 mb-2"></div>
        <span className="text-sm">Đang tải dữ liệu...</span>
      </div>
    );
  }

  // Empty state
  if (data.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-slate-400 dark:text-slate-500 border border-slate-100 dark:border-gray-800 rounded-lg">
        <span className="text-sm font-medium">{emptyMessage}</span>
      </div>
    );
  }

  // Row renderer component for react-window FixedSizeList
  const Row = ({ index, style }: { index: number; style: React.CSSProperties }) => {
    const item = data[index];
    const isEven = index % 2 === 0;

    return (
      <div
        className={`grid hover:bg-teal-50/50 dark:hover:bg-teal-950/20 border-b border-slate-100 dark:border-gray-800 text-sm text-slate-600 dark:text-slate-300 transition-colors duration-150 cursor-pointer ${
          isEven ? 'bg-white dark:bg-gray-900' : 'bg-slate-50/30 dark:bg-gray-900/50'
        }`}
        style={{
          ...style,
          gridTemplateColumns,
          alignItems: 'center',
        }}
        onClick={() => onRowClick?.(item)}
      >
        {columns.map((col, colIndex) => {
          const content = col.render ? col.render(item, index) : (item as any)[col.key];
          return (
            <div
              key={col.key || colIndex}
              className={`px-4 py-2 truncate ${col.className || ''}`}
            >
              {content !== undefined && content !== null ? content : '—'}
            </div>
          );
        })}
      </div>
    );
  };

  return (
    <div
      className={`flex flex-col w-full border border-slate-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white dark:bg-gray-900 shadow-sm ${className}`}
    >
      {renderHeader()}
      <div style={{ height: `${height}px` }}>
        <List
          height={height}
          itemCount={data.length}
          itemSize={rowHeight}
          width="100%"
        >
          {Row}
        </List>
      </div>
    </div>
  );
}
