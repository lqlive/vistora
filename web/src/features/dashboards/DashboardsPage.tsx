import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  PlusIcon,
  Squares2X2Icon,
  ListBulletIcon,
  PencilSquareIcon,
  TrashIcon,
  ShareIcon,
} from '@heroicons/react/24/outline';
import PageHeader from '../../shared/components/PageHeader';
import FilterBar from '../../shared/components/FilterBar';
import DataTable, { Column } from '../../shared/components/DataTable';
import Tag from '../../shared/components/Tag';
import OwnerAvatars from '../../shared/components/OwnerAvatars';
import FavoriteStar from '../../shared/components/FavoriteStar';
import ConfirmDialog from '../../shared/components/ConfirmDialog';
import {
  deleteDashboard,
  listDashboards,
  mapDashboardToItem,
  mapDashboardToRequest,
  updateDashboard,
} from './api';
import type { DashboardItem } from '../../types';
import classNames from 'classnames';

const Dashboards: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<DashboardItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState('all');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('All');
  const [view, setView] = useState<'card' | 'table'>('table');
  const [deleting, setDeleting] = useState<DashboardItem | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const dashboards = await listDashboards();
        if (!cancelled) {
          setItems(dashboards.map(mapDashboardToItem));
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Failed to load dashboards');
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

  const toggleFavorite = useCallback(async (dashboard: DashboardItem) => {
    const nextFavorite = !dashboard.favorite;
    setItems((current) =>
      current.map((item) => (item.id === dashboard.id ? { ...item, favorite: nextFavorite } : item))
    );

    try {
      setError(null);
      const updated = await updateDashboard(String(dashboard.id), {
        ...mapDashboardToRequest(dashboard),
        favorite: nextFavorite,
      });
      const item = mapDashboardToItem(updated);
      setItems((current) => current.map((value) => (value.id === item.id ? item : value)));
    } catch (toggleError) {
      setItems((current) =>
        current.map((item) =>
          item.id === dashboard.id ? { ...item, favorite: dashboard.favorite } : item
        )
      );
      setError(toggleError instanceof Error ? toggleError.message : 'Failed to update dashboard');
    }
  }, []);

  const confirmDelete = useCallback(async () => {
    if (!deleting) return;

    try {
      setError(null);
      await deleteDashboard(String(deleting.id));
      setItems((current) => current.filter((dashboard) => dashboard.id !== deleting.id));
      setDeleting(null);
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Failed to delete dashboard');
    }
  }, [deleting]);

  const filtered = useMemo(() => {
    return items.filter((d) => {
      if (tab === 'favorite' && !d.favorite) return false;
      if (status !== 'All' && d.status !== status.toLowerCase()) return false;
      if (search && !d.title.toLowerCase().includes(search.toLowerCase())) return false;
      return true;
    });
  }, [items, tab, status, search]);

  const columns: Column<DashboardItem>[] = [
    {
      key: 'title',
      header: 'Name',
      render: (d) => (
        <div className="flex items-center gap-2">
          <FavoriteStar active={d.favorite} onToggle={() => toggleFavorite(d)} />
          <button
            type="button"
            onClick={() => navigate(`/dashboards/${d.id}/edit`)}
            className="font-medium text-gray-900 hover:underline"
          >
            {d.title}
          </button>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (d) => (
        <Tag variant={d.status === 'published' ? 'success' : 'neutral'} dot>
          {d.status === 'published' ? 'Published' : 'Draft'}
        </Tag>
      ),
    },
    { key: 'charts', header: 'Charts', render: (d) => <span>{d.charts}</span> },
    { key: 'owners', header: 'Owners', render: (d) => <OwnerAvatars owners={d.owners} /> },
    { key: 'modifiedBy', header: 'Modified by', render: (d) => <span>{d.modifiedBy}</span> },
    {
      key: 'modified',
      header: 'Last modified',
      render: (d) => <span className="text-accent-400">{d.modified}</span>,
    },
    {
      key: 'actions',
      header: '',
      className: 'w-px',
      render: (d) => (
        <div className="flex items-center gap-3 text-gray-400">
          <button
            className="hover:text-gray-900"
            title="Edit"
            onClick={() => navigate(`/dashboards/${d.id}/edit`)}
          >
            <PencilSquareIcon className="h-4 w-4" />
          </button>
          <button className="hover:text-gray-900" title="Share">
            <ShareIcon className="h-4 w-4" />
          </button>
          <button className="hover:text-error-400" title="Delete" onClick={() => setDeleting(d)}>
            <TrashIcon className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Dashboards"
        actions={
          <button className="btn-primary" onClick={() => navigate('/dashboards/new')}>
            <PlusIcon className="h-4 w-4" /> Dashboard
          </button>
        }
        tabs={[
          { key: 'all', label: 'All', count: items.length },
          { key: 'favorite', label: 'Favorite', count: items.filter((d) => d.favorite).length },
          { key: 'mine', label: 'Mine' },
        ]}
        activeTab={tab}
        onTabChange={setTab}
      />

      <div className="flex items-center gap-3">
        <div className="flex-1">
          <FilterBar
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Search dashboards"
            filters={[
              {
                label: 'Status',
                options: ['All', 'Published', 'Draft'],
                value: status,
                onChange: setStatus,
              },
            ]}
          />
        </div>
        <div className="mb-4 flex items-center overflow-hidden rounded border border-gray-300">
          <button
            onClick={() => setView('card')}
            className={classNames('p-1.5', view === 'card' ? 'bg-gray-100 text-gray-900' : 'text-gray-400')}
            title="Card view"
          >
            <Squares2X2Icon className="h-4 w-4" />
          </button>
          <button
            onClick={() => setView('table')}
            className={classNames('p-1.5', view === 'table' ? 'bg-gray-100 text-gray-900' : 'text-gray-400')}
            title="Table view"
          >
            <ListBulletIcon className="h-4 w-4" />
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {view === 'table' ? (
        <DataTable
          columns={columns}
          rows={loading ? [] : filtered}
          rowKey={(d) => d.id}
          loading={loading}
          emptyText="No dashboards found"
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {filtered.map((d) => (
            <div key={d.id} className="card group overflow-hidden">
              <button
                type="button"
                onClick={() => navigate(`/dashboards/${d.id}/edit`)}
                className="relative flex h-32 w-full items-center justify-center border-b border-gray-100 bg-gray-50"
              >
                <Squares2X2Icon className="h-12 w-12 text-gray-300 transition-transform group-hover:scale-110" />
                <span className="absolute right-2 top-2" onClick={(event) => event.stopPropagation()}>
                  <FavoriteStar active={d.favorite} onToggle={() => toggleFavorite(d)} />
                </span>
              </button>
              <div className="p-3">
                <div className="truncate text-sm font-semibold text-accent-700">{d.title}</div>
                <div className="mt-0.5 text-xs text-accent-400">
                  {d.charts} charts · {d.modified}
                </div>
                <div className="mt-3 flex items-center justify-between">
                  <Tag variant={d.status === 'published' ? 'success' : 'neutral'} dot>
                    {d.status === 'published' ? 'Published' : 'Draft'}
                  </Tag>
                  <OwnerAvatars owners={d.owners} />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <ConfirmDialog
        open={!!deleting}
        title="Delete dashboard"
        message={`Delete "${deleting?.title ?? 'this dashboard'}"? This action cannot be undone.`}
        confirmLabel="Delete"
        destructive
        onCancel={() => setDeleting(null)}
        onConfirm={confirmDelete}
      />
    </div>
  );
};

export default Dashboards;
