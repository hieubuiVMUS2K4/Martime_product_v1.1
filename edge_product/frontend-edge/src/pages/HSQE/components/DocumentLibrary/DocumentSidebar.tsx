/**
 * Document Sidebar - Tree View for SMS Library
 * Shows folder structure: ISM Manual, ISPS, STCW, Garbage, etc.
 */
import { useState, useMemo } from 'react';
import { 
  ChevronRight, ChevronDown, Folder, FileText, 
  Search, Plus, MoreVertical, FolderPlus
} from 'lucide-react';
import type { DocTreeNode, ViewMode } from './types';

interface DocumentSidebarProps {
  documents: DocTreeNode[];
  selectedDocId: string | null;
  onSelectDocument: (id: string) => void;
  onCreateFolder: () => void;
  onCreateDocument: (parentCode?: string) => void;
  viewMode: ViewMode;
  onViewModeChange: (mode: ViewMode) => void;
}

export function DocumentSidebar({
  documents,
  selectedDocId,
  onSelectDocument,
  onCreateFolder,
  onCreateDocument,
  viewMode,
  onViewModeChange,
}: DocumentSidebarProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedNodes, setExpandedNodes] = useState<string[]>([]);
  const [contextMenuId, setContextMenuId] = useState<string | null>(null);

  // Build tree structure from flat documents
  const treeData = useMemo(() => {
    const roots = documents.filter(d => !d.parentId);
    const buildTree = (parentId: string): DocTreeNode[] => {
      return documents
        .filter(d => d.parentId === parentId)
        .map(d => ({
          ...d,
          children: buildTree(d.id),
        }));
    };
    return roots.map(r => ({
      ...r,
      children: buildTree(r.id),
    }));
  }, [documents]);

  // Filter by search
  const filteredTree = useMemo(() => {
    if (!searchQuery.trim()) return treeData;
    const q = searchQuery.toLowerCase();
    const matchesSearch = (node: DocTreeNode): boolean => {
      if (node.title.toLowerCase().includes(q) || node.code.toLowerCase().includes(q)) return true;
      return node.children.some(matchesSearch);
    };
    return treeData.filter(matchesSearch);
  }, [treeData, searchQuery]);

  const toggleExpand = (id: string) => {
    setExpandedNodes(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Published': return 'bg-green-500';
      case 'Pending_DPA': return 'bg-amber-500';
      case 'Draft': return 'bg-red-500';
      case 'Obsolete': return 'bg-slate-400';
      default: return 'bg-slate-400';
    }
  };

  const renderNode = (node: DocTreeNode, depth: number = 0) => {
    const isExpanded = expandedNodes.includes(node.id);
    const isSelected = selectedDocId === node.id;
    const hasChildren = node.children.length > 0;
    const isFolder = node.type === 'manual' || node.type === 'chapter' || (hasChildren && node.type !== 'form');

    return (
      <div key={node.id}>
        <div
          className={`group flex items-center gap-1 py-1.5 px-2 cursor-pointer rounded transition-all duration-100 ${
            isSelected
              ? 'bg-blue-100 dark:bg-blue-900/40 text-blue-900 dark:text-blue-200'
              : 'hover:bg-slate-100 dark:hover:bg-slate-700/40 text-slate-700 dark:text-slate-300'
          }`}
          style={{ paddingLeft: `${depth * 16 + 8}px` }}
          onClick={() => {
            onSelectDocument(node.id);
            if (isFolder) toggleExpand(node.id);
          }}
        >
          {/* Expand/Collapse Arrow */}
          {isFolder ? (
            <button
              onClick={(e) => { e.stopPropagation(); toggleExpand(node.id); }}
              className="p-0.5 hover:bg-slate-200 dark:hover:bg-slate-600 rounded flex-shrink-0"
            >
              {isExpanded ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronRight className="w-3.5 h-3.5" />}
            </button>
          ) : (
            <span className="w-4.5 flex-shrink-0" />
          )}

          {/* Status Indicator */}
          <span className={`w-2 h-2 rounded-full flex-shrink-0 ${getStatusColor(node.status)}`} />

          {/* Icon */}
          {isFolder ? (
            <Folder className={`w-4 h-4 flex-shrink-0 ${isSelected ? 'text-blue-600' : 'text-amber-500'}`} />
          ) : (
            <FileText className={`w-4 h-4 flex-shrink-0 ${isSelected ? 'text-blue-500' : 'text-slate-400'}`} />
          )}

          {/* Label */}
          <span className="text-xs truncate flex-1 font-medium">
            {node.title}
          </span>

          {/* Context Menu Trigger */}
          <button
            onClick={(e) => {
              e.stopPropagation();
              setContextMenuId(contextMenuId === node.id ? null : node.id);
            }}
            className="p-0.5 opacity-0 group-hover:opacity-100 hover:bg-slate-200 dark:hover:bg-slate-600 rounded flex-shrink-0 transition-opacity"
          >
            <MoreVertical className="w-3 h-3" />
          </button>
        </div>

        {/* Context Menu */}
        {contextMenuId === node.id && (
          <div className="ml-8 mr-2 mb-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg shadow-lg py-1 z-50">
            <button
              onClick={() => { onCreateDocument(node.code); setContextMenuId(null); }}
              className="w-full flex items-center gap-2 px-3 py-1.5 text-xs text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700"
            >
              <Plus className="w-3 h-3" /> Thêm tài liệu con
            </button>
            <button
              onClick={() => { setContextMenuId(null); }}
              className="w-full flex items-center gap-2 px-3 py-1.5 text-xs text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700"
            >
              <FolderPlus className="w-3 h-3" /> Thêm Chapter
            </button>
          </div>
        )}

        {/* Children */}
        {isExpanded && node.children.map(child => renderNode(child, depth + 1))}
      </div>
    );
  };

  return (
    <div className="flex flex-col h-full border-r border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800">
      {/* View Mode Toggles */}
      <div className="flex border-b border-slate-200 dark:border-slate-700">
        <button
          onClick={() => onViewModeChange('library')}
          className={`flex-1 py-2 text-xs font-semibold text-center transition-colors ${
            viewMode === 'library'
              ? 'bg-slate-100 dark:bg-slate-700 text-blue-600 dark:text-blue-400 border-b-2 border-blue-600'
              : 'text-slate-500 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-700/50'
          }`}
        >
          Library View
        </button>
        <button
          onClick={() => onViewModeChange('location')}
          className={`flex-1 py-2 text-xs font-semibold text-center transition-colors ${
            viewMode === 'location'
              ? 'bg-slate-100 dark:bg-slate-700 text-blue-600 dark:text-blue-400 border-b-2 border-blue-600'
              : 'text-slate-500 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-700/50'
          }`}
        >
          Location View
        </button>
      </div>

      {/* Search */}
      <div className="p-2 border-b border-slate-200 dark:border-slate-700">
        <div className="flex items-center gap-1.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-md px-2 py-1.5">
          <span className="text-xs text-slate-400 font-medium">Search</span>
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="flex-1 bg-transparent text-xs text-slate-700 dark:text-slate-300 outline-none min-w-0"
            placeholder=""
          />
          <Search className="w-3.5 h-3.5 text-slate-400 flex-shrink-0" />
        </div>
      </div>

      {/* Action Bar */}
      <div className="flex items-center gap-1 px-2 py-1.5 border-b border-slate-200 dark:border-slate-700">
        <select className="text-xs border border-slate-200 dark:border-slate-700 rounded px-1.5 py-1 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 flex-1">
          <option>-- Execute Action --</option>
          <option>Create New Manual</option>
          <option>Create New Chapter</option>
          <option>Import Document</option>
        </select>
        <button
          onClick={onCreateFolder}
          className="p-1.5 bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded hover:bg-blue-100 transition flex-shrink-0"
          title="Tạo thư mục mới"
        >
          <FolderPlus className="w-3.5 h-3.5" />
        </button>
      </div>

      {/* Tree View */}
      <div className="flex-1 overflow-y-auto py-1">
        {filteredTree.length === 0 ? (
          <div className="px-4 py-8 text-center">
            <FileText className="w-8 h-8 text-slate-300 dark:text-slate-600 mx-auto mb-2" />
            <p className="text-xs text-slate-400 dark:text-slate-500">
              {searchQuery ? 'Không tìm thấy tài liệu' : 'Chưa có tài liệu nào'}
            </p>
          </div>
        ) : (
          filteredTree.map(node => renderNode(node))
        )}
      </div>
    </div>
  );
}
