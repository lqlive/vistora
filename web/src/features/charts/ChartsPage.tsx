import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  PlusIcon,
  PencilSquareIcon,
  TrashIcon,
  ChartBarIcon,
  ChartPieIcon,
  PresentationChartLineIcon,
  TableCellsIcon,
  HashtagIcon,
} from '@heroicons/react/24/outline';
import PageHeader from '../../shared/components/PageHeader';
import FilterBar from '../../shared/components/FilterBar';
import DataTable, { Column } from '../../shared/components/DataTable';
import Tag from '../../shared/components/Tag';
import OwnerAvatars from '../../shared/components/OwnerAvatars';
import FavoriteStar from '../../shared/components/FavoriteStar';
import ConfirmDialog from '../../shared/components/ConfirmDialog';
import { deleteChart, listCharts, mapChartToItem, updateChart } from './api';
import type { ChartItem, ChartVizType } from '../../types';

const vizIcon = (viz: ChartVizType) => {
  switch (viz) {
    case 'Pie Chart':
      return ChartPieIcon;
    case 'Line Chart':
    case 'Area Chart':
    case 'Time-series':
      return PresentationChartLineIcon;
    case 'Table':
      return TableCellsIcon;
    case 'Big Number':
      return HashtagIcon;
    default:
      return ChartBarIcon;
  }
};

const Charts: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<ChartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState('all');
  const [search, setSearch] = useState('');
  const [viz, setViz] = useState('All');
  const [deletingChart, setDeletingChart] = useState<ChartItem | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const charts = await listCharts();
        if (!cancelled) {
          setItems(charts.map(mapChartToItem));
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Failed to load charts');
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

  const vizOptions = useMemo(
    () => ['All', ...Array.from(new Set(items.map((c) => c.vizType)))],
    [items]
  );

  const toggleFavorite = useCallback(async (chart: ChartItem) => {
    const nextFavorite = !chart.favorite;
    setItems((current) =>
      current.map((item) => (item.id === chart.id ? { ...item, favorite: nextFavorite } : item))
    );

    try {
      setError(null);
      const updated = await updateChart(String(chart.id), {
        name: chart.name,
        vizType: chart.vizType,
        dataset: chart.dataset,
        description: chart.description,
        configuration: chart.configuration,
        favorite: nextFavorite,
      });
      const item = mapChartToItem(updated);
      setItems((current) => current.map((value) => (value.id === item.id ? item : value)));
    } catch (toggleError) {
      setItems((current) =>
        current.map((item) => (item.id === chart.id ? { ...item, favorite: chart.favorite } : item))
      );
      setError(toggleError instanceof Error ? toggleError.message : 'Failed to update chart');
    }
  }, []);

  const confirmDeleteChart = useCallback(async () => {
    if (!deletingChart) return;

    try {
      setError(null);
      await deleteChart(String(deletingChart.id));
      setItems((current) => current.filter((chart) => chart.id !== deletingChart.id));
      setDeletingChart(null);
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Failed to delete chart');
    }
  }, [deletingChart]);

  const filtered = useMemo(() => {
    return items.filter((c) => {
      if (tab === 'favorite' && !c.favorite) return false;
      if (viz !== 'All' && c.vizType !== viz) return false;
      if (search && !c.name.toLowerCase().includes(search.toLowerCase())) return false;
      return true;
    });
  }, [items, tab, viz, search]);

  const columns: Column<ChartItem>[] = [
    {
      key: 'name',
      header: 'Chart',
      render: (c) => {
        const Icon = vizIcon(c.vizType);
        return (
          <div className="flex items-center gap-2">
            <FavoriteStar active={c.favorite} onToggle={() => toggleFavorite(c)} />
            <Icon className="h-4 w-4 text-accent-400" />
            <Link
              to={`/charts/${c.id}/edit`}
              className="font-medium text-gray-900 hover:underline"
            >
              {c.name}
            </Link>
          </div>
        );
      },
    },
    { key: 'vizType', header: 'Visualization', render: (c) => <Tag variant="primary">{c.vizType}</Tag> },
    { key: 'dataset', header: 'Dataset', render: (c) => <span className="font-mono text-xs">{c.dataset}</span> },
    { key: 'owners', header: 'Owners', render: (c) => <OwnerAvatars owners={c.owners} /> },
    { key: 'modifiedBy', header: 'Modified by', render: (c) => <span>{c.modifiedBy}</span> },
    { key: 'modified', header: 'Last modified', render: (c) => <span className="text-accent-400">{c.modified}</span> },
    {
      key: 'actions',
      header: '',
      className: 'w-px',
      render: (c) => (
        <div className="flex items-center gap-3 text-gray-400">
          <button
            className="hover:text-gray-900"
            title="Edit"
            onClick={() => navigate(`/charts/${c.id}/edit`)}
          >
            <PencilSquareIcon className="h-4 w-4" />
          </button>
          <button
            className="hover:text-error-400"
            title="Delete"
            onClick={() => setDeletingChart(c)}
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
        title="Charts"
        actions={
          <button className="btn-primary" onClick={() => navigate('/charts/new')}>
            <PlusIcon className="h-4 w-4" /> Chart
          </button>
        }
        tabs={[
          { key: 'all', label: 'All', count: items.length },
          { key: 'favorite', label: 'Favorite', count: items.filter((c) => c.favorite).length },
          { key: 'mine', label: 'Mine' },
        ]}
        activeTab={tab}
        onTabChange={setTab}
      />

      <FilterBar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search charts"
        filters={[{ label: 'Type', options: vizOptions, value: viz, onChange: setViz }]}
      />

      {error && (
        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <DataTable
        columns={columns}
        rows={loading ? [] : filtered}
        rowKey={(c) => c.id}
        loading={loading}
        emptyText="No charts found"
      />
      <ConfirmDialog
        open={!!deletingChart}
        title="Delete chart"
        message={`Delete "${deletingChart?.name ?? 'this chart'}"? This action cannot be undone.`}
        confirmLabel="Delete"
        destructive
        onCancel={() => setDeletingChart(null)}
        onConfirm={confirmDeleteChart}
      />
    </div>
  );
};

export default Charts;
