/**
 * Document Sidebar - Tree View for SMS Library
 * Shows folder structure: ISM Manual, ISPS, STCW, Garbage, etc.
 */
import { useState, useMemo } from 'react';
import { 
  ChevronRight, ChevronDown, Folder, FileText, 
  Search, Plus, MoreVertical, FolderPlus
} from 'lucide-react';
import type { DocCategory, DocTreeNode, ViewMode } from './types';

interface DocumentSidebarProps {
  documents: DocTreeNode[];
  selectedDocId: string | null;
  onSelectDocument: (id: string) => void;
  onCreateDocument: (parentCode?: string, category?: DocCategory) => void;
  viewMode: ViewMode;
  onViewModeChange: (mode: ViewMode) => void;
}

export function DocumentSidebar({
  documents,
  selectedDocId,
  onSelectDocument,
  onCreateDocument,
  viewMode,
  onViewModeChange,
}: DocumentSidebarProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedNodes, setExpandedNodes] = useState<string[]>([]);
  const [contextMenuId, setContextMenuId] = useState<string | null>(null);

  const createVirtualFolder = (id: string, title: string): DocTreeNode => ({
    id,
    code: id,
    title,
    type: 'manual',
    children: [],
    status: 'Published',
    currentVersion: '',
    lastModified: '',
    author: '',
    content: '',
    category: 'MANUAL',
    editCount: 0,
    watermarkText: '',
    revisions: [],
    syncStatuses: [],
    attachments: [],
    readLogs: [],
    isVirtual: true,
  });

  const inferLibraryFolderId = (doc: DocTreeNode): string => {
    const haystack = `${doc.code} ${doc.title} ${doc.category}`.toLowerCase();

    if (doc.category === 'CHECKLIST' || haystack.includes('checklist')) return 'library-checklists';
    if (doc.category === 'FORM' || haystack.includes('form')) return 'library-forms';
    if (haystack.includes('isps') || haystack.includes('security')) return 'library-isps';
    if (haystack.includes('stcw') || haystack.includes('seafarer')) return 'library-stcw';
    if (haystack.includes('garbage')) return 'library-garbage';
    if (haystack.includes('ballast')) return 'library-ballast';
    if (haystack.includes('emergency') || haystack.includes('khẩn cấp')) return 'library-emergency';
    if (haystack.includes('policy') || haystack.includes('chính sách')) return 'library-policies';
    if (haystack.includes('work instruction') || haystack.includes('hướng dẫn')) return 'library-work-instructions';
    if (doc.category === 'PROCEDURE') return 'library-work-instructions';
    return 'library-ism';
  };

  const isDocumentControlChild = (doc: DocTreeNode): boolean => {
    const haystack = `${doc.title} ${doc.code}`.toLowerCase();
    return (
      haystack.includes('danh mục tài liệu') ||
      haystack.includes('phiếu yêu cầu') ||
      haystack.includes('sửa đổi') ||
      haystack.includes('bổ sung')
    );
  };

  // Build SMS Library hierarchy from API data. When backend has no parentId yet,
  // group documents into Marad-like manuals and attach related forms under procedures.
  const treeData = useMemo(() => {
    const hasExplicitHierarchy = documents.some(d => d.parentId);

    if (!hasExplicitHierarchy) {
      const folders = [
        createVirtualFolder('library-ism', 'ISM Manual'),
        createVirtualFolder('library-isps', 'ISPS Manual'),
        createVirtualFolder('library-stcw', 'STCW Guide'),
        createVirtualFolder('library-garbage', 'Garbage Management Plan'),
        createVirtualFolder('library-ballast', 'Ballast Water Management Plan'),
        createVirtualFolder('library-emergency', 'Emergency Procedures'),
        createVirtualFolder('library-policies', 'Company Policies'),
        createVirtualFolder('library-work-instructions', 'Work Instructions'),
        createVirtualFolder('library-checklists', 'Checklists'),
        createVirtualFolder('library-forms', 'Forms'),
      ];
      const folderMap = new Map(folders.map(folder => [folder.id, folder]));
      const nodeByCode = new Map(documents.map(doc => [doc.code, { ...doc, children: [] as DocTreeNode[] }]));
      const nestedDocumentIds = new Set<string>();
      const docControlProcedure = documents.find(doc =>
        doc.category === 'PROCEDURE' && doc.title.toLowerCase().includes('kiểm soát tài liệu')
      );
      const docControlNode: DocTreeNode | null = docControlProcedure ? nodeByCode.get(docControlProcedure.code) ?? null : null;

      documents.forEach((doc) => {
        const codeParts = doc.code.split('-');
        if (codeParts.length <= 2) return;

        const parentCode = codeParts.slice(0, -1).join('-');
        const parentNode = nodeByCode.get(parentCode);
        const childNode = nodeByCode.get(doc.code);
        if (parentNode && childNode) {
          parentNode.children.push(childNode);
          nestedDocumentIds.add(doc.id);
        }
      });

      documents.forEach((doc) => {
        if (nestedDocumentIds.has(doc.id)) return;
        if (docControlProcedure && doc.id === docControlProcedure.id) return;

        const node = nodeByCode.get(doc.code) ?? { ...doc, children: [] };
        if (doc.category === 'MANUAL' || doc.category === 'SMS_HANDBOOK') {
          folders.unshift(node);
          return;
        }

        if (docControlNode && isDocumentControlChild(doc)) {
          docControlNode.children.push(node);
          return;
        }

        const folder = folderMap.get(inferLibraryFolderId(doc)) ?? folderMap.get('library-ism');
        folder?.children.push(node);
      });

      if (docControlNode) {
        folderMap.get('library-ism')?.children.unshift(docControlNode);
      }

      return folders.filter(folder => folder.children.length > 0);
    }

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
    const isFolder = Boolean(node.isVirtual) || node.type === 'manual' || hasChildren;

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
            if (!node.isVirtual) onSelectDocument(node.id);
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
          {!node.isVirtual && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                setContextMenuId(contextMenuId === node.id ? null : node.id);
              }}
              className="p-0.5 opacity-0 group-hover:opacity-100 hover:bg-slate-200 dark:hover:bg-slate-600 rounded flex-shrink-0 transition-opacity"
            >
              <MoreVertical className="w-3 h-3" />
            </button>
          )}
        </div>

        {/* Context Menu */}
        {contextMenuId === node.id && (
          <div className="ml-8 mr-2 mb-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg shadow-lg py-1 z-50">
            <button
              onClick={() => { onCreateDocument(node.code, 'FORM'); setContextMenuId(null); }}
              className="w-full flex items-center gap-2 px-3 py-1.5 text-xs text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700"
            >
              <Plus className="w-3 h-3" /> Thêm tài liệu con
            </button>
            <button
              onClick={() => { onCreateDocument(node.code, 'PROCEDURE'); setContextMenuId(null); }}
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
        <select
          className="text-xs border border-slate-200 dark:border-slate-700 rounded px-1.5 py-1 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 flex-1"
          defaultValue=""
          onChange={(e) => {
            const action = e.target.value;
            if (action === 'manual') onCreateDocument(undefined, 'MANUAL');
            if (action === 'chapter') onCreateDocument(undefined, 'PROCEDURE');
            if (action === 'checklist') onCreateDocument(undefined, 'CHECKLIST');
            if (action === 'form') onCreateDocument(undefined, 'FORM');
            if (action === 'import') onCreateDocument(undefined, 'EXTERNAL');
            e.target.value = '';
          }}
        >
          <option value="">-- Execute Action --</option>
          <option value="manual">Create New Manual</option>
          <option value="chapter">Create New Chapter</option>
          <option value="checklist">Create New Checklist</option>
          <option value="form">Create New Form</option>
          <option value="import">Import Document</option>
        </select>
        <button
          onClick={() => onCreateDocument(undefined, 'PROCEDURE')}
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
