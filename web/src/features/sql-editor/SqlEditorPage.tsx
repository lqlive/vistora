import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  MagnifyingGlassIcon,
  ChevronRightIcon,
  ChevronDownIcon,
  FolderIcon,
  TableCellsIcon,
  CircleStackIcon,
  CheckCircleIcon,
  PlayIcon,
  BookmarkIcon,
  DocumentTextIcon,
  ArrowPathIcon,
  ViewColumnsIcon,
  CubeIcon,
  VariableIcon,
  ShieldCheckIcon,
  XMarkIcon,
  ClipboardDocumentIcon,
  PlusIcon,
  Square3Stack3DIcon,
  PencilSquareIcon,
  TrashIcon,
  EllipsisHorizontalIcon,
} from '@heroicons/react/24/outline';
import classNames from 'classnames';
import ConfirmDialog from '../../shared/components/ConfirmDialog';
import TextInputDialog from '../../shared/components/TextInputDialog';
import { useClickOutside } from '../../shared/hooks/useClickOutside';
import { getDataSource, listDataSources } from '../datasources/api';
import { federatedQueryEngine, listEngineTables } from '../../lib/apiClient/engine';
import { createQueryDocument, deleteQueryDocument, listQueryDocuments, updateQueryDocument } from './api';
import type {
  DataSourceResponse,
  EngineColumnInfo,
  EngineQueryResult,
  EngineTableInfo,
  GridColumnType,
  QueryDocumentResponse,
} from '../../types';

// ---------------------------------------------------------------------------
// Explorer tree
// ---------------------------------------------------------------------------
type NodeIcon =
  | 'db'
  | 'schema'
  | 'folder'
  | 'table'
  | 'view'
  | 'func'
  | 'proc'
  | 'query'
  | 'security';

interface TreeNode {
  id: string;
  label: string;
  icon: NodeIcon;
  selection?: TableSelection;
  queryDocument?: QueryDocumentResponse;
  children?: TreeNode[];
  defaultOpen?: boolean;
  active?: boolean;
}

interface TableSelection {
  dataSource: DataSourceResponse;
  table: EngineTableInfo;
}

interface DataSourceTables {
  dataSource: DataSourceResponse;
  tables: EngineTableInfo[];
}

const nodeIcon = (icon: NodeIcon) => {
  const cls = 'h-4 w-4 shrink-0';
  switch (icon) {
    case 'db':
      return <CircleStackIcon className={`${cls} text-gray-500`} />;
    case 'schema':
      return <CubeIcon className={`${cls} text-gray-400`} />;
    case 'folder':
      return <FolderIcon className={`${cls} text-gray-400`} />;
    case 'table':
      return <TableCellsIcon className={`${cls} text-gray-400`} />;
    case 'view':
      return <ViewColumnsIcon className={`${cls} text-gray-400`} />;
    case 'func':
      return <VariableIcon className={`${cls} text-gray-400`} />;
    case 'proc':
      return <Square3Stack3DIcon className={`${cls} text-gray-400`} />;
    case 'query':
      return <DocumentTextIcon className={`${cls} text-gray-400`} />;
    case 'security':
      return <ShieldCheckIcon className={`${cls} text-gray-400`} />;
  }
};

const TreeItem: React.FC<{
  node: TreeNode;
  depth: number;
  onTableSelect?: (selection: TableSelection) => void;
  onQuerySelect?: (queryDocument: QueryDocumentResponse) => void;
  onQueryRename?: (queryDocument: QueryDocumentResponse) => void;
  onQueryDelete?: (queryDocument: QueryDocumentResponse) => void;
}> = ({ node, depth, onTableSelect, onQuerySelect, onQueryRename, onQueryDelete }) => {
  const [open, setOpen] = useState(!!node.defaultOpen);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const hasChildren = !!node.children?.length;
  const canEditQuery = node.queryDocument?.isOwner;

  useClickOutside(menuRef, () => setMenuOpen(false));

  const handleSelect = () => {
    if (node.selection) {
      onTableSelect?.(node.selection);
      return;
    }

    if (node.queryDocument) {
      onQuerySelect?.(node.queryDocument);
      return;
    }

    if (hasChildren) {
      setOpen((o) => !o);
    }
  };

  return (
    <div className="relative" ref={menuRef}>
      <div
        className={classNames(
          'group w-full flex items-center gap-1.5 py-2 pr-2 text-left hover:bg-gray-50 text-[13px]',
          node.active ? 'bg-gray-100 text-gray-900 font-medium' : 'text-gray-700'
        )}
        style={{ paddingLeft: depth * 14 + 8 }}
      >
        <button
          type="button"
          onClick={handleSelect}
          className="flex min-w-0 flex-1 items-center gap-1.5 text-left"
        >
          {hasChildren ? (
            open ? (
              <ChevronDownIcon className="h-3 w-3 text-gray-400 shrink-0" />
            ) : (
              <ChevronRightIcon className="h-3 w-3 text-gray-400 shrink-0" />
            )
          ) : (
            <span className="w-3 shrink-0" />
          )}
          {nodeIcon(node.icon)}
          <span className="truncate font-mono">{node.label}</span>
        </button>
        {canEditQuery && (
          <div className="ml-auto pr-1">
            <button
              type="button"
              title="Query actions"
              onClick={(event) => {
                event.stopPropagation();
                setMenuOpen((value) => !value);
              }}
              className={classNames(
                'hidden rounded p-0.5 text-gray-400 hover:bg-gray-100 hover:text-gray-700 group-hover:block',
                menuOpen && 'block'
              )}
            >
              <EllipsisHorizontalIcon className="h-4 w-4" />
            </button>
          </div>
        )}
      </div>
      {canEditQuery && menuOpen && (
        <div className="absolute right-2 top-7 z-30 w-36 rounded-md border border-gray-200 bg-white py-1 shadow-lg">
          <button
            type="button"
            onClick={() => {
              setMenuOpen(false);
              onQueryRename?.(node.queryDocument!);
            }}
            className="flex w-full items-center gap-2 px-3 py-2 text-left text-xs text-gray-700 hover:bg-gray-50"
          >
            <PencilSquareIcon className="h-4 w-4 text-gray-400" />
            Rename
          </button>
          <button
            type="button"
            onClick={() => {
              setMenuOpen(false);
              onQueryDelete?.(node.queryDocument!);
            }}
            className="flex w-full items-center gap-2 px-3 py-2 text-left text-xs text-red-600 hover:bg-red-50"
          >
            <TrashIcon className="h-4 w-4 text-red-400" />
            Delete
          </button>
        </div>
      )}
      {open &&
        node.children?.map((c) => (
          <TreeItem
            key={c.id}
            node={c}
            depth={depth + 1}
            onTableSelect={onTableSelect}
            onQuerySelect={onQuerySelect}
            onQueryRename={onQueryRename}
            onQueryDelete={onQueryDelete}
          />
        ))}
    </div>
  );
};

// ---------------------------------------------------------------------------
// Column type badge
// ---------------------------------------------------------------------------
const TypeBadge: React.FC<{ type: GridColumnType }> = ({ type }) => {
  if (type === 'date') {
    return (
      <span className="text-gray-400" title="date">
        <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth={2}>
          <rect x="3" y="4" width="18" height="17" rx="2" />
          <path d="M3 9h18M8 2v4M16 2v4" />
        </svg>
      </span>
    );
  }
  const label = type === 'string' ? 'ABC' : type === 'decimal' ? '1.2' : '123';
  return (
    <span className="text-[10px] font-mono font-semibold text-gray-400" title={type}>
      {label}
    </span>
  );
};

// ---------------------------------------------------------------------------
// Page
// ---------------------------------------------------------------------------
const SqlEditor: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [datasource, setDatasource] = useState<DataSourceResponse | null>(null);
  const [sourceTables, setSourceTables] = useState<DataSourceTables[]>([]);
  const [queryDocuments, setQueryDocuments] = useState<QueryDocumentResponse[]>([]);
  const [activeQueryDocument, setActiveQueryDocument] = useState<QueryDocumentResponse | null>(null);
  const [activeTable, setActiveTable] = useState<EngineTableInfo | null>(null);
  const [result, setResult] = useState<EngineQueryResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [running, setRunning] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState('Run a query to see results.');
  const [search, setSearch] = useState('');
  const [resultTab, setResultTab] = useState<'messages' | 'results'>('results');
  const [sql, setSql] = useState('');
  const [renamingDocument, setRenamingDocument] = useState<QueryDocumentResponse | null>(null);
  const [deletingDocument, setDeletingDocument] = useState<QueryDocumentResponse | null>(null);

  useEffect(() => {
    let cancelled = false;

    const loadEditorData = async () => {
      try {
        setLoading(true);
        setError(null);

        const sources = await listDataSources();
        if (sources.length === 0) {
          throw new Error('No datasource found. Create a datasource first.');
        }

        let preferredSource: DataSourceResponse | undefined;
        if (id) {
          try {
            preferredSource = await getDataSource(id);
          } catch {
            preferredSource = undefined;
          }
        }

        const orderedSources = preferredSource
          ? [preferredSource, ...sources.filter((source) => source.id !== preferredSource.id)]
          : sources;
        const [loadedSourceTables, loadedQueryDocuments] = await Promise.all([
          Promise.all(
            orderedSources.map(async (source) => ({
              dataSource: source,
              tables: await listEngineTables(source),
            }))
          ),
          listQueryDocuments(),
        ]);
        const firstGroup = loadedSourceTables.find((group) => group.tables.length > 0) ?? loadedSourceTables[0];
        const firstTable = firstGroup?.tables[0] ?? null;
        if (!cancelled) {
          setSourceTables(loadedSourceTables);
          setQueryDocuments(loadedQueryDocuments);
          setDatasource(firstGroup?.dataSource ?? null);
          setActiveTable(firstTable);
          const firstQueryDocument = loadedQueryDocuments.find((document) => document.isOwner) ?? null;
          setActiveQueryDocument(firstQueryDocument);
          setSql(firstQueryDocument?.sql ?? (firstTable ? `select * from ${tableReference(firstTable)}` : ''));
          setMessage(firstTable ? 'Ready.' : 'No tables found for this datasource.');
        }
      } catch (loadError) {
        if (!cancelled) {
          const nextError = loadError instanceof Error ? loadError.message : 'Failed to load SQL editor data';
          setError(nextError);
          setMessage(nextError);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadEditorData();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const runQuery = useCallback(async () => {
    if (!datasource || !sql.trim()) return;

    try {
      setRunning(true);
      setError(null);
      setMessage('Running query...');
      const dataSourceIds = sourceTables.map((group) => group.dataSource.id);
      const queryResult = await federatedQueryEngine(dataSourceIds, sql, 10000);
      setResult(queryResult);
      setResultTab('results');
      setMessage(
        `(${queryResult.rowCount} rows affected)\nTotal execution time: ${queryResult.durationMs} ms`
      );
    } catch (runError) {
      const nextError = runError instanceof Error ? runError.message : 'Failed to run query';
      setError(nextError);
      setMessage(nextError);
      setResultTab('messages');
    } finally {
      setRunning(false);
    }
  }, [datasource, sourceTables, sql]);

  const handleTableSelect = (selection: TableSelection) => {
    setDatasource(selection.dataSource);
    setActiveTable(selection.table);
    setActiveQueryDocument(null);
    setSql(`select * from ${tableReference(selection.table)}`);
    setResult(null);
    setMessage('Ready.');
    setResultTab('results');
  };

  const handleQuerySelect = (queryDocument: QueryDocumentResponse) => {
    setActiveQueryDocument(queryDocument);
    setActiveTable(null);
    setSql(queryDocument.sql);
    setResult(null);
    setMessage('Ready.');
    setResultTab('results');
  };

  const handleSaveQuery = useCallback(async () => {
    if (!sql.trim()) return;

    try {
      setError(null);
      setMessage('Saving query...');
      const saved = activeQueryDocument
        ? await updateQueryDocument(activeQueryDocument.id, {
            name: activeQueryDocument.name,
            sql,
            isShared: activeQueryDocument.isShared,
          })
        : await createQueryDocument({
            name: nextQueryName(queryDocuments),
            sql,
            isShared: false,
          });

      setQueryDocuments((current) => {
        const exists = current.some((document) => document.id === saved.id);
        return exists
          ? current.map((document) => (document.id === saved.id ? saved : document))
          : [...current, saved];
      });
      setActiveQueryDocument(saved);
      setActiveTable(null);
      setMessage('Query saved.');
      setResultTab('messages');
    } catch (saveError) {
      const nextError = saveError instanceof Error ? saveError.message : 'Failed to save query';
      setError(nextError);
      setMessage(nextError);
      setResultTab('messages');
    }
  }, [activeQueryDocument, queryDocuments, sql]);

  const handleRenameQueryDocument = useCallback((queryDocument: QueryDocumentResponse) => {
    setRenamingDocument(queryDocument);
  }, []);

  const confirmRenameQueryDocument = useCallback(async (nextName: string) => {
    if (!renamingDocument || nextName === renamingDocument.name) {
      setRenamingDocument(null);
      return;
    }

    try {
      setError(null);
      setMessage('Renaming query...');
      const renamed = await updateQueryDocument(renamingDocument.id, {
        name: nextName,
        sql: activeQueryDocument?.id === renamingDocument.id ? sql : renamingDocument.sql,
        isShared: renamingDocument.isShared,
      });

      setQueryDocuments((current) =>
        current.map((document) => (document.id === renamed.id ? renamed : document))
      );
      if (activeQueryDocument?.id === renamed.id) {
        setActiveQueryDocument(renamed);
      }
      setMessage('Query renamed.');
      setResultTab('messages');
      setRenamingDocument(null);
    } catch (renameError) {
      const nextError = renameError instanceof Error ? renameError.message : 'Failed to rename query';
      setError(nextError);
      setMessage(nextError);
      setResultTab('messages');
    }
  }, [activeQueryDocument, renamingDocument, sql]);

  const handleDeleteQueryDocument = useCallback((queryDocument: QueryDocumentResponse) => {
    setDeletingDocument(queryDocument);
  }, []);

  const confirmDeleteQueryDocument = useCallback(async () => {
    if (!deletingDocument) return;

    try {
      setError(null);
      setMessage('Deleting query...');
      await deleteQueryDocument(deletingDocument.id);

      const remaining = queryDocuments.filter((document) => document.id !== deletingDocument.id);
      const wasActive = activeQueryDocument?.id === deletingDocument.id;
      const nextDocument = wasActive ? remaining.find((document) => document.isOwner) ?? null : activeQueryDocument;
      const fallbackGroup = sourceTables.find((group) => group.tables.length > 0);
      const fallbackTable = fallbackGroup?.tables[0] ?? null;

      setQueryDocuments(remaining);
      setActiveQueryDocument(nextDocument);
      if (wasActive) {
        setActiveTable(nextDocument ? null : fallbackTable);
        setDatasource(nextDocument ? datasource : fallbackGroup?.dataSource ?? null);
        setSql(nextDocument?.sql ?? (fallbackTable ? `select * from ${tableReference(fallbackTable)}` : ''));
        setResult(null);
      }
      setMessage('Query deleted.');
      setResultTab('messages');
      setDeletingDocument(null);
    } catch (deleteError) {
      const nextError = deleteError instanceof Error ? deleteError.message : 'Failed to delete query';
      setError(nextError);
      setMessage(nextError);
      setResultTab('messages');
    }
  }, [activeQueryDocument, deletingDocument, datasource, queryDocuments, sourceTables]);

  const dataSourceTrees: TreeNode[] = useMemo(
    () =>
      sourceTables.map((group) => {
        const tableNodes = group.tables
          .filter((table) => table.name.toLowerCase().includes(search.toLowerCase()))
          .map((table) => ({
            id: `tbl-${group.dataSource.id}-${table.schema ?? 'default'}-${table.name}`,
            label: table.name,
            icon: 'table' as NodeIcon,
            selection: { dataSource: group.dataSource, table },
            active:
              !activeQueryDocument &&
              group.dataSource.id === datasource?.id &&
              table.name === activeTable?.name &&
              table.schema === activeTable?.schema,
          }));

        return {
          id: group.dataSource.id,
          label: group.dataSource.name,
          icon: 'db' as NodeIcon,
          defaultOpen: group.dataSource.id === datasource?.id || sourceTables.length === 1,
          children: [
            {
              id: `${group.dataSource.id}-schemas`,
              label: 'Schemas',
              icon: 'folder' as NodeIcon,
              defaultOpen: group.dataSource.id === datasource?.id || sourceTables.length === 1,
              children: [
                {
                  id: `${group.dataSource.id}-tables`,
                  label: 'Tables',
                  icon: 'folder' as NodeIcon,
                  defaultOpen: group.dataSource.id === datasource?.id || sourceTables.length === 1,
                  children: tableNodes,
                },
              ],
            },
          ],
        };
      }),
    [activeQueryDocument, activeTable, datasource, search, sourceTables]
  );

  const queriesTree: TreeNode = useMemo(
    () => {
      const visibleDocuments = queryDocuments.filter((document) =>
        document.name.toLowerCase().includes(search.toLowerCase())
      );
      const myQueries = visibleDocuments.filter((document) => document.isOwner);
      const sharedQueries = visibleDocuments.filter((document) => !document.isOwner && document.isShared);

      const toQueryNode = (document: QueryDocumentResponse): TreeNode => ({
        id: `query-${document.id}`,
        label: document.name,
        icon: 'query',
        queryDocument: document,
        active: activeQueryDocument?.id === document.id,
      });

      return {
        id: 'queries',
        label: 'Queries',
        icon: 'folder',
        defaultOpen: true,
        children: [
          {
            id: 'my-queries',
            label: 'My queries',
            icon: 'folder',
            defaultOpen: true,
            children: myQueries.map(toQueryNode),
          },
          {
            id: 'shared',
            label: 'Shared queries',
            icon: 'folder',
            defaultOpen: true,
            children: sharedQueries.map(toQueryNode),
          },
        ],
      };
    },
    [activeQueryDocument, queryDocuments, search]
  );

  const columns = result?.columns ?? [];
  const rows = result?.rows ?? [];
  const lineCount = sql.split('\n').length;

  const ToolbarButton: React.FC<{
    icon: React.ReactNode;
    label: string;
    accent?: boolean;
    onClick?: () => void;
    disabled?: boolean;
  }> = ({ icon, label, accent, onClick, disabled }) => (
    <button
      onClick={onClick}
      disabled={disabled}
      className={classNames(
        'flex items-center gap-1 px-2 h-7 rounded text-xs hover:bg-gray-100 transition-colors',
        accent ? 'text-green-600' : 'text-gray-600'
      )}
    >
      {icon}
      {label}
    </button>
  );

  return (
    <>
    <div className="flex h-[calc(100vh-7rem)] border border-gray-200 rounded-lg overflow-hidden bg-white">
      {/* Explorer */}
      <aside className="w-64 shrink-0 border-r border-gray-100 flex flex-col">
        <div className="h-9 flex items-center justify-between px-3 border-b border-gray-100">
          <span className="text-xs font-semibold text-gray-700">Explorer</span>
          <div className="flex items-center gap-2 text-gray-400">
            <button
              type="button"
              onClick={() => navigate('/datasources?create=1')}
              className="hover:text-gray-700"
              title="Add datasource"
            >
              <PlusIcon className="h-4 w-4" />
            </button>
            <MagnifyingGlassIcon className="h-4 w-4 hover:text-gray-700 cursor-pointer" />
          </div>
        </div>
        <div className="p-2 border-b border-gray-100">
          <div className="relative">
            <MagnifyingGlassIcon className="h-3.5 w-3.5 text-gray-400 absolute left-2.5 top-1/2 -translate-y-1/2" />
            <input
              className="input h-7 text-xs pl-8"
              placeholder="Search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
        <div className="flex-1 overflow-y-auto py-1">
          {dataSourceTrees.map((tree) => (
            <TreeItem
              key={tree.id}
              node={tree}
              depth={0}
              onTableSelect={handleTableSelect}
              onQuerySelect={handleQuerySelect}
              onQueryRename={handleRenameQueryDocument}
              onQueryDelete={handleDeleteQueryDocument}
            />
          ))}
          <TreeItem
            node={queriesTree}
            depth={0}
            onQuerySelect={handleQuerySelect}
            onQueryRename={handleRenameQueryDocument}
            onQueryDelete={handleDeleteQueryDocument}
          />
        </div>
      </aside>

      {/* Editor + results */}
      <section className="flex-1 flex flex-col min-w-0">
        {/* Tab bar */}
        <div className="h-9 flex items-center justify-between border-b border-gray-100 bg-gray-50/60 pl-2 pr-3">
          <div className="flex items-center gap-2 px-3 h-8 bg-white border border-b-0 border-gray-200 rounded-t-md text-sm text-gray-900 -mb-px">
            <DocumentTextIcon className="h-4 w-4 text-green-600" />
            <span className="text-xs">{activeQueryDocument?.name ?? 'SQL query'}</span>
            <XMarkIcon className="h-3.5 w-3.5 text-gray-400 hover:text-gray-700" />
          </div>
          <ArrowPathIcon className="h-4 w-4 text-gray-400 hover:text-gray-700 cursor-pointer" />
        </div>

        {/* Toolbar */}
        <div className="h-9 flex items-center gap-1 border-b border-gray-100 px-2">
          <ToolbarButton
            icon={<PlayIcon className="h-4 w-4" />}
            label={running ? 'Running...' : 'Run'}
            accent
            onClick={runQuery}
            disabled={running || !datasource || !sql.trim()}
          />
          <span className="w-px h-4 bg-gray-200 mx-1" />
          <ToolbarButton
            icon={<BookmarkIcon className="h-4 w-4" />}
            label="Save"
            onClick={handleSaveQuery}
            disabled={!sql.trim()}
          />
          <ToolbarButton icon={<ViewColumnsIcon className="h-4 w-4" />} label="Save as view" />
          <ToolbarButton icon={<VariableIcon className="h-4 w-4" />} label="New measure" />
          <ToolbarButton icon={<ClipboardDocumentIcon className="h-4 w-4" />} label="Copy query" />
        </div>

        {/* SQL editor */}
        <div className="h-40 flex border-b border-gray-100 overflow-auto">
          <div className="select-none text-right text-gray-300 py-2 pr-3 pl-3 border-r border-gray-100 bg-gray-50/40 font-mono text-sm leading-6">
            {Array.from({ length: lineCount }).map((_, i) => (
              <div key={i}>{i + 1}</div>
            ))}
          </div>
          <textarea
            value={sql}
            onChange={(e) => setSql(e.target.value)}
            spellCheck={false}
            className="flex-1 p-2 font-mono text-sm leading-6 text-gray-800 resize-none focus:outline-none"
          />
        </div>

        {/* Results header */}
        <div className="h-9 flex items-center justify-between border-b border-gray-100 px-3">
          <div className="flex items-center gap-4 text-xs">
            <button
              onClick={() => setResultTab('messages')}
              className={classNames(
                'py-1',
                resultTab === 'messages'
                  ? 'text-gray-900 font-medium border-b-2 border-gray-900 -mb-[2px]'
                  : 'text-gray-500 hover:text-gray-800'
              )}
            >
              Messages
            </button>
            <button
              onClick={() => setResultTab('results')}
              className={classNames(
                'py-1',
                resultTab === 'results'
                  ? 'text-gray-900 font-medium border-b-2 border-gray-900 -mb-[2px]'
                  : 'text-gray-500 hover:text-gray-800'
              )}
            >
              Results
            </button>
            <span className="text-gray-400 hover:text-gray-700 cursor-pointer">Open in Excel</span>
          </div>
          <div className="flex items-center gap-3 text-xs text-gray-400">
            <span>Limit 10,000 rows</span>
            <ClipboardDocumentIcon className="h-4 w-4 hover:text-gray-700 cursor-pointer" />
            <div className="relative">
              <MagnifyingGlassIcon className="h-3.5 w-3.5 text-gray-400 absolute left-2 top-1/2 -translate-y-1/2" />
              <input className="input h-6 text-xs pl-7 w-36" placeholder="Search" />
            </div>
          </div>
        </div>

        {/* Results body */}
        <div className="flex-1 overflow-auto">
          {resultTab === 'messages' ? (
            <div className="p-4 font-mono text-xs text-gray-600">
              {message.split('\n').map((line) => (
                <React.Fragment key={line}>
                  {line}
                  <br />
                </React.Fragment>
              ))}
            </div>
          ) : (
            error ? (
              <div className="p-4 font-mono text-xs text-red-700">{error}</div>
            ) : loading ? (
              <div className="p-4 font-mono text-xs text-gray-500">Loading datasource...</div>
            ) : rows.length === 0 ? (
              <div className="p-4 font-mono text-xs text-gray-500">No results yet.</div>
            ) : (
              <table className="border-collapse text-xs w-full">
              <thead className="sticky top-0 z-10">
                <tr className="bg-gray-50">
                  <th className="sticky left-0 z-20 bg-gray-50 w-10 px-2 py-1.5 border-b border-r border-gray-200 text-gray-400 font-normal text-right">
                    #
                  </th>
                  {columns.map((col) => (
                    <th
                      key={col.name}
                      className="px-3 py-1.5 border-b border-r border-gray-200 text-left font-medium text-gray-600 whitespace-nowrap"
                    >
                      <span className="flex items-center gap-1.5">
                        <TypeBadge type={mapEngineColumnType(col)} />
                        {col.name}
                      </span>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {rows.map((row, ri) => (
                  <tr key={ri} className="hover:bg-gray-50">
                    <td className="sticky left-0 z-10 bg-white w-10 px-2 py-1.5 border-b border-r border-gray-100 text-gray-400 text-right">
                      {ri + 1}
                    </td>
                    {row.map((cell, ci) => (
                      <td
                        key={ci}
                        className={classNames(
                          'px-3 py-1.5 border-b border-r border-gray-100 whitespace-nowrap',
                          isNumericColumn(columns[ci])
                            ? 'text-right font-mono text-gray-700'
                            : 'text-gray-700'
                        )}
                      >
                        {formatCell(cell)}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
              </table>
            )
          )}
        </div>

        {/* Status bar */}
        <div className="h-7 flex items-center justify-between px-3 border-t border-gray-100 bg-gray-50/60 text-xs text-gray-500">
          <span className="flex items-center gap-1.5">
            <CheckCircleIcon className="h-4 w-4 text-green-500" />
            {running ? 'Running...' : result ? `Succeeded · ${result.durationMs} ms` : loading ? 'Loading...' : 'Ready'}
          </span>
          <span>
            {columns.length} columns · {rows.length} rows
          </span>
        </div>
      </section>
    </div>
    <TextInputDialog
      open={!!renamingDocument}
      title="Rename query"
      label="Query name"
      initialValue={renamingDocument?.name ?? ''}
      confirmLabel="Rename"
      onCancel={() => setRenamingDocument(null)}
      onConfirm={confirmRenameQueryDocument}
    />
    <ConfirmDialog
      open={!!deletingDocument}
      title="Delete query"
      message={`Delete "${deletingDocument?.name ?? 'this query'}"? This action cannot be undone.`}
      confirmLabel="Delete"
      destructive
      onCancel={() => setDeletingDocument(null)}
      onConfirm={confirmDeleteQueryDocument}
    />
    </>
  );
};

const tableReference = (table: EngineTableInfo): string =>
  table.schema
    ? `${quoteIdentifier(table.schema)}.${quoteIdentifier(table.name)}`
    : quoteIdentifier(table.name);

const quoteIdentifier = (value: string): string => `"${value.replace(/"/g, '""')}"`;

const nextQueryName = (documents: QueryDocumentResponse[]): string => {
  const names = new Set(documents.map((document) => document.name.toLowerCase()));
  for (let index = documents.length + 1; ; index++) {
    const name = `SQL query ${index}`;
    if (!names.has(name.toLowerCase())) {
      return name;
    }
  }
};

const mapEngineColumnType = (column: EngineColumnInfo): GridColumnType => {
  const type = column.type.toLowerCase();
  if (type.includes('date') || type.includes('timestamp')) return 'date';
  if (type.includes('decimal') || type.includes('float') || type.includes('double')) return 'decimal';
  if (type.includes('int') || type.includes('uint')) return 'int';
  return 'string';
};

const isNumericColumn = (column: EngineColumnInfo | undefined): boolean => {
  if (!column) return false;
  const type = mapEngineColumnType(column);
  return type === 'int' || type === 'decimal';
};

const formatCell = (value: unknown): string => {
  if (value === null || value === undefined) return '';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
};

export default SqlEditor;
