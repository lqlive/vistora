import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  PlusIcon,
  PencilSquareIcon,
  TrashIcon,
  TableCellsIcon,
} from '@heroicons/react/24/outline';
import PageHeader from '../../shared/components/PageHeader';
import FilterBar from '../../shared/components/FilterBar';
import DataTable, { Column } from '../../shared/components/DataTable';
import Tag from '../../shared/components/Tag';
import OwnerAvatars from '../../shared/components/OwnerAvatars';
import ConfirmDialog from '../../shared/components/ConfirmDialog';
import { createDataset, deleteDataset, listDatasets, mapDatasetToItem, updateDataset } from './api';
import DatasetDialog from './DatasetDialog';
import type { DatasetItem, DatasetRequest } from '../../types';

const Datasets: React.FC = () => {
  const [datasets, setDatasets] = useState<DatasetItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [type, setType] = useState('All');
  const [database, setDatabase] = useState('All');
  const [editingDataset, setEditingDataset] = useState<DatasetItem | null>(null);
  const [deletingDataset, setDeletingDataset] = useState<DatasetItem | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const items = await listDatasets();
        if (!cancelled) {
          setDatasets(items.map(mapDatasetToItem));
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Failed to load datasets');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    load();

    return () => {
      cancelled = true;
    };
  }, []);

  const dbOptions = useMemo(
    () => ['All', ...Array.from(new Set(datasets.map((d) => d.database)))],
    [datasets]
  );

  const filtered = useMemo(() => {
    return datasets.filter((d) => {
      if (type !== 'All' && d.type !== type.toLowerCase()) return false;
      if (database !== 'All' && d.database !== database) return false;
      if (search && !d.name.toLowerCase().includes(search.toLowerCase())) return false;
      return true;
    });
  }, [datasets, search, type, database]);

  const handleCreateDataset = useCallback(() => {
    setEditingDataset(null);
    setDialogOpen(true);
  }, []);

  const handleEditDataset = useCallback((dataset: DatasetItem) => {
    setEditingDataset(dataset);
    setDialogOpen(true);
  }, []);

  const handleSaveDataset = useCallback(async (request: DatasetRequest) => {
    try {
      setSaving(true);
      setError(null);

      const saved = editingDataset
        ? await updateDataset(String(editingDataset.id), request)
        : await createDataset(request);

      setDatasets((current) => {
        const item = mapDatasetToItem(saved);
        return editingDataset
          ? current.map((dataset) => (dataset.id === item.id ? item : dataset))
          : [...current, item];
      });
      setDialogOpen(false);
      setEditingDataset(null);
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Failed to save dataset');
    } finally {
      setSaving(false);
    }
  }, [editingDataset]);

  const confirmDeleteDataset = useCallback(async () => {
    if (!deletingDataset) return;

    try {
      setError(null);
      await deleteDataset(String(deletingDataset.id));
      setDatasets((current) => current.filter((dataset) => dataset.id !== deletingDataset.id));
      setDeletingDataset(null);
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Failed to delete dataset');
    }
  }, [deletingDataset]);

  const columns: Column<DatasetItem>[] = [
    {
      key: 'name',
      header: 'Name',
      render: (d) => (
        <div className="flex items-center gap-2">
          <TableCellsIcon className="h-4 w-4 text-gray-400" />
          <Link
            to={`/sql-editor/${d.id}`}
            className="font-medium text-gray-900 hover:underline font-mono text-sm"
          >
            {d.name}
          </Link>
        </div>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (d) => (
        <Tag variant={d.type === 'physical' ? 'info' : 'warning'}>
          {d.type === 'physical' ? 'Physical' : 'Virtual'}
        </Tag>
      ),
    },
    { key: 'database', header: 'Database', render: (d) => <span>{d.database}</span> },
    { key: 'schema', header: 'Schema', render: (d) => <span className="text-accent-500">{d.schema}</span> },
    { key: 'charts', header: 'Charts', render: (d) => <span>{d.charts}</span> },
    { key: 'owners', header: 'Owners', render: (d) => <OwnerAvatars owners={d.owners} /> },
    { key: 'modified', header: 'Last modified', render: (d) => <span className="text-accent-400">{d.modified}</span> },
    {
      key: 'actions',
      header: '',
      className: 'w-px',
      render: (d) => (
        <div className="flex items-center gap-3 text-gray-400">
          <button
            className="hover:text-gray-900"
            title="Edit"
            onClick={() => handleEditDataset(d)}
          >
            <PencilSquareIcon className="h-4 w-4" />
          </button>
          <button
            className="hover:text-error-400"
            title="Delete"
            onClick={() => setDeletingDataset(d)}
          >
            <TrashIcon className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Datasets"
        actions={
          <button className="btn-primary" onClick={handleCreateDataset}>
            <PlusIcon className="h-4 w-4" /> Dataset
          </button>
        }
      />

      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search datasets"
        filters={[
          { label: 'Type', options: ['All', 'Physical', 'Virtual'], value: type, onChange: setType },
          { label: 'Database', options: dbOptions, value: database, onChange: setDatabase },
        ]}
      />

      {error && (
        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <DataTable
        columns={columns}
        rows={loading ? [] : filtered}
        rowKey={(d) => d.id}
        loading={loading}
        emptyText="No datasets found"
      />
      <DatasetDialog
        open={dialogOpen}
        dataset={editingDataset}
        saving={saving}
        onCancel={() => {
          if (saving) return;
          setDialogOpen(false);
          setEditingDataset(null);
        }}
        onConfirm={handleSaveDataset}
      />
      <ConfirmDialog
        open={!!deletingDataset}
        title="Delete dataset"
        message={`Delete "${deletingDataset?.name ?? 'this dataset'}"? This action cannot be undone.`}
        confirmLabel="Delete"
        destructive
        onCancel={() => setDeletingDataset(null)}
        onConfirm={confirmDeleteDataset}
      />
    </div>
  );
};

export default Datasets;
