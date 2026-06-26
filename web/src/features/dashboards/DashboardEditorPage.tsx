import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ArrowLeftIcon,
  ChartBarIcon,
  ChartPieIcon,
  HashtagIcon,
  MagnifyingGlassIcon,
  PresentationChartLineIcon,
  Squares2X2Icon,
  TableCellsIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline';
import PageHeader from '../../shared/components/PageHeader';
import ChartCanvas from '../charts/ChartCanvas';
import { listCharts } from '../charts/api';
import { createDashboard, getDashboard, updateDashboard } from './api';
import type { ChartResponse, ChartVizType, DashboardRequest } from '../../types';

type WellKey = 'axis' | 'legend' | 'values' | 'filters';

type ChartWells = Record<WellKey, string[]>;

interface DashboardTile {
  chartId: string;
  w: number; // columns (1-12)
  h: number; // row units
}

const COLUMNS = 12;
const ROW_HEIGHT = 110;
const GRID_GAP = 12;
const DEFAULT_TILE: Omit<DashboardTile, 'chartId'> = { w: 6, h: 3 };

const CHART_ID_MIME = 'application/x-nexova-chart';
const TILE_INDEX_MIME = 'application/x-nexova-tile';

const emptyWells: ChartWells = { axis: [], legend: [], values: [], filters: [] };

const defaultRequest: DashboardRequest = {
  name: '',
  status: 'draft',
  description: null,
  configuration: null,
  favorite: false,
};

const vizIcon = (viz: string): React.ComponentType<{ className?: string }> => {
  switch (viz as ChartVizType) {
    case 'Pie Chart':
    case 'Treemap':
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

const parseTiles = (configuration?: string | null): DashboardTile[] => {
  if (!configuration) return [];

  try {
    const parsed = JSON.parse(configuration) as {
      tiles?: { chartId?: unknown; w?: unknown; h?: unknown }[];
      chartIds?: unknown;
    };

    if (Array.isArray(parsed.tiles)) {
      return parsed.tiles
        .filter((tile) => typeof tile.chartId === 'string')
        .map((tile) => ({
          chartId: tile.chartId as string,
          w: typeof tile.w === 'number' ? clamp(tile.w, 1, COLUMNS) : DEFAULT_TILE.w,
          h: typeof tile.h === 'number' ? clamp(tile.h, 1, 12) : DEFAULT_TILE.h,
        }));
    }

    if (Array.isArray(parsed.chartIds)) {
      return parsed.chartIds
        .filter((value): value is string => typeof value === 'string')
        .map((chartId) => ({ chartId, ...DEFAULT_TILE }));
    }

    return [];
  } catch {
    return [];
  }
};

const parseWells = (configuration?: string | null): ChartWells => {
  if (!configuration) return { ...emptyWells };

  try {
    const parsed = JSON.parse(configuration) as Partial<ChartWells> & { wells?: Partial<ChartWells> };
    const source = parsed.wells ?? parsed;
    return {
      axis: Array.isArray(source.axis) ? source.axis : [],
      legend: Array.isArray(source.legend) ? source.legend : [],
      values: Array.isArray(source.values) ? source.values : [],
      filters: Array.isArray(source.filters) ? source.filters : [],
    };
  } catch {
    return { ...emptyWells };
  }
};

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

const DashboardEditorPage: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isNew = !id;

  const [request, setRequest] = useState<DashboardRequest>(defaultRequest);
  const [tiles, setTiles] = useState<DashboardTile[]>([]);
  const [charts, setCharts] = useState<ChartResponse[]>([]);
  const [chartSearch, setChartSearch] = useState('');
  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [canvasDragOver, setCanvasDragOver] = useState(false);

  const gridRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);

        const [chartItems, dashboard] = await Promise.all([
          listCharts(),
          id ? getDashboard(id) : Promise.resolve(null),
        ]);

        if (cancelled) return;

        setCharts(chartItems);
        if (dashboard) {
          setRequest({
            name: dashboard.name,
            status: dashboard.status,
            description: dashboard.description ?? null,
            configuration: dashboard.configuration ?? null,
            favorite: dashboard.favorite,
          });
          setTiles(parseTiles(dashboard.configuration));
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Failed to load dashboard');
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
  }, [id]);

  const chartsById = useMemo(() => {
    const map = new Map<string, ChartResponse>();
    charts.forEach((chart) => map.set(chart.id, chart));
    return map;
  }, [charts]);

  const usedChartIds = useMemo(() => new Set(tiles.map((tile) => tile.chartId)), [tiles]);

  const filteredCharts = useMemo(() => {
    const term = chartSearch.trim().toLowerCase();
    if (!term) return charts;
    return charts.filter((chart) => chart.name.toLowerCase().includes(term));
  }, [charts, chartSearch]);

  const addChart = (chartId: string, atIndex?: number) => {
    setTiles((current) => {
      if (current.some((tile) => tile.chartId === chartId)) {
        return current;
      }
      const tile: DashboardTile = { chartId, ...DEFAULT_TILE };
      if (atIndex === undefined) {
        return [...current, tile];
      }
      const next = [...current];
      next.splice(atIndex, 0, tile);
      return next;
    });
  };

  const removeTile = (chartId: string) =>
    setTiles((current) => current.filter((tile) => tile.chartId !== chartId));

  const moveTile = (from: number, to: number) =>
    setTiles((current) => {
      if (from === to) return current;
      const next = [...current];
      const [moved] = next.splice(from, 1);
      next.splice(clamp(to, 0, next.length), 0, moved);
      return next;
    });

  const resizeTile = (index: number, w: number, h: number) =>
    setTiles((current) =>
      current.map((tile, tileIndex) =>
        tileIndex === index ? { ...tile, w: clamp(w, 1, COLUMNS), h: clamp(h, 1, 12) } : tile
      )
    );

  const handleSave = async () => {
    if (!request.name.trim()) {
      setError('Dashboard name is required');
      return;
    }

    try {
      setSaving(true);
      setError(null);

      const payload: DashboardRequest = {
        name: request.name.trim(),
        status: request.status,
        description: request.description?.trim() || null,
        configuration: JSON.stringify({
          tiles,
          chartIds: tiles.map((tile) => tile.chartId),
        }),
        favorite: request.favorite,
      };

      const saved = id ? await updateDashboard(id, payload) : await createDashboard(payload);
      navigate(`/dashboards/${saved.id}/edit`, { replace: true });
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Failed to save dashboard');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isNew ? 'New Dashboard' : 'Edit Dashboard'}
        actions={
          <div className="flex items-center gap-2">
            <button className="btn-secondary" onClick={() => navigate('/dashboards')}>
              <ArrowLeftIcon className="h-4 w-4" /> Back
            </button>
            <button className="btn-primary" onClick={handleSave} disabled={saving || loading}>
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        }
      />

      {error && (
        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-3 rounded-t-xl border-b border-gray-200 bg-white px-4 py-3">
        <input
          value={request.name}
          onChange={(event) => setRequest((current) => ({ ...current, name: event.target.value }))}
          className="input max-w-sm"
          placeholder="Dashboard name"
        />
        <select
          value={request.status}
          onChange={(event) => setRequest((current) => ({ ...current, status: event.target.value }))}
          className="input max-w-[160px]"
        >
          <option value="draft">Draft</option>
          <option value="published">Published</option>
        </select>
      </div>

      <div className="grid min-h-[600px] grid-cols-[minmax(0,1fr)_300px] overflow-hidden rounded-b-xl border border-t-0 border-gray-200">
        {/* Canvas */}
        <section className="flex flex-col bg-[#f5f6f8] p-5">
          <div
            ref={gridRef}
            onDragOver={(event) => {
              if (event.dataTransfer.types.includes(CHART_ID_MIME)) {
                event.preventDefault();
                setCanvasDragOver(true);
              }
            }}
            onDragLeave={() => setCanvasDragOver(false)}
            onDrop={(event) => {
              setCanvasDragOver(false);
              const chartId = event.dataTransfer.getData(CHART_ID_MIME);
              if (chartId) {
                event.preventDefault();
                addChart(chartId);
              }
            }}
            className={`flex-1 rounded-lg border-2 transition-colors ${
              tiles.length === 0
                ? 'flex items-center justify-center border-dashed bg-white'
                : 'grid content-start gap-3 border-transparent'
            } ${canvasDragOver ? '!border-primary-500 bg-primary-50/40' : 'border-gray-300'}`}
            style={
              tiles.length === 0
                ? undefined
                : {
                    gridTemplateColumns: `repeat(${COLUMNS}, minmax(0, 1fr))`,
                    gridAutoRows: `${ROW_HEIGHT}px`,
                  }
            }
          >
            {tiles.length === 0 ? (
              <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
                <Squares2X2Icon className="h-14 w-14 text-gray-300" />
                <p className="mt-4 text-sm font-medium text-gray-500">
                  Drag and drop charts here
                </p>
                <p className="mt-1 text-xs text-gray-400">
                  Or click a chart in the right panel to add it.
                </p>
              </div>
            ) : (
              tiles.map((tile, index) => {
                const chart = chartsById.get(tile.chartId);
                return (
                  <DashboardGridTile
                    key={tile.chartId}
                    index={index}
                    tile={tile}
                    chart={chart}
                    gridRef={gridRef}
                    onRemove={() => removeTile(tile.chartId)}
                    onMove={moveTile}
                    onResize={resizeTile}
                    onDropChart={(chartId) => addChart(chartId, index)}
                  />
                );
              })
            )}
          </div>

          <div className="mt-4">
            <label className="mb-1 block text-xs font-medium text-gray-500">Description</label>
            <textarea
              value={request.description ?? ''}
              onChange={(event) =>
                setRequest((current) => ({ ...current, description: event.target.value }))
              }
              className="input min-h-[60px]"
              placeholder="Optional dashboard description"
            />
          </div>
        </section>

        {/* Charts pane */}
        <aside className="flex flex-col border-l border-gray-200 bg-white">
          <div className="border-b border-gray-100 p-4">
            <h2 className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Charts
            </h2>
            <div className="relative">
              <MagnifyingGlassIcon className="pointer-events-none absolute left-2.5 top-2.5 h-4 w-4 text-gray-400" />
              <input
                value={chartSearch}
                onChange={(event) => setChartSearch(event.target.value)}
                className="input pl-8"
                placeholder="Search charts"
              />
            </div>
          </div>

          <div className="flex-1 space-y-2 overflow-auto p-3">
            {loading ? (
              <p className="px-1 py-4 text-sm text-gray-400">Loading charts...</p>
            ) : filteredCharts.length === 0 ? (
              <p className="px-1 py-4 text-sm text-gray-400">
                {charts.length === 0 ? 'No charts available yet.' : 'No charts match your search.'}
              </p>
            ) : (
              filteredCharts.map((chart) => {
                const Icon = vizIcon(chart.vizType);
                const added = usedChartIds.has(chart.id);
                return (
                  <div
                    key={chart.id}
                    draggable={!added}
                    onDragStart={(event) =>
                      event.dataTransfer.setData(CHART_ID_MIME, chart.id)
                    }
                    onClick={() => !added && addChart(chart.id)}
                    className={`flex items-center gap-3 rounded-lg border p-3 transition-all ${
                      added
                        ? 'cursor-default border-gray-100 bg-gray-50 opacity-60'
                        : 'cursor-grab border-gray-200 bg-white hover:border-primary-400 hover:shadow-sm active:cursor-grabbing'
                    }`}
                  >
                    <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-primary-50 text-primary-500">
                      <Icon className="h-5 w-5" />
                    </span>
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm font-medium text-gray-900">
                        {chart.name}
                      </span>
                      <span className="block truncate text-[11px] text-gray-400">
                        {chart.vizType} · {chart.dataset}
                      </span>
                    </span>
                    {added && (
                      <span className="shrink-0 text-[10px] font-medium uppercase text-gray-400">
                        Added
                      </span>
                    )}
                  </div>
                );
              })
            )}
          </div>
        </aside>
      </div>
    </div>
  );
};

interface DashboardGridTileProps {
  index: number;
  tile: DashboardTile;
  chart?: ChartResponse;
  gridRef: React.RefObject<HTMLDivElement>;
  onRemove: () => void;
  onMove: (from: number, to: number) => void;
  onResize: (index: number, w: number, h: number) => void;
  onDropChart: (chartId: string) => void;
}

const DashboardGridTile: React.FC<DashboardGridTileProps> = ({
  index,
  tile,
  chart,
  gridRef,
  onRemove,
  onMove,
  onResize,
  onDropChart,
}) => {
  const [dragOver, setDragOver] = useState(false);

  const handleResizeStart = (event: React.PointerEvent) => {
    event.preventDefault();
    event.stopPropagation();

    const grid = gridRef.current;
    if (!grid) return;

    const columnWidth = (grid.clientWidth - GRID_GAP * (COLUMNS - 1)) / COLUMNS;
    const startX = event.clientX;
    const startY = event.clientY;
    const startW = tile.w;
    const startH = tile.h;

    const handleMove = (moveEvent: PointerEvent) => {
      const deltaCols = Math.round((moveEvent.clientX - startX) / (columnWidth + GRID_GAP));
      const deltaRows = Math.round((moveEvent.clientY - startY) / (ROW_HEIGHT + GRID_GAP));
      onResize(index, startW + deltaCols, startH + deltaRows);
    };

    const handleUp = () => {
      window.removeEventListener('pointermove', handleMove);
      window.removeEventListener('pointerup', handleUp);
    };

    window.addEventListener('pointermove', handleMove);
    window.addEventListener('pointerup', handleUp);
  };

  return (
    <div
      onDragOver={(event) => {
        if (
          event.dataTransfer.types.includes(TILE_INDEX_MIME) ||
          event.dataTransfer.types.includes(CHART_ID_MIME)
        ) {
          event.preventDefault();
          setDragOver(true);
        }
      }}
      onDragLeave={() => setDragOver(false)}
      onDrop={(event) => {
        event.preventDefault();
        event.stopPropagation();
        setDragOver(false);
        const fromIndex = event.dataTransfer.getData(TILE_INDEX_MIME);
        if (fromIndex !== '') {
          onMove(Number(fromIndex), index);
          return;
        }
        const chartId = event.dataTransfer.getData(CHART_ID_MIME);
        if (chartId) {
          onDropChart(chartId);
        }
      }}
      className={`group relative flex flex-col overflow-hidden rounded-lg border bg-white shadow-sm transition-colors ${
        dragOver ? 'border-primary-500 ring-2 ring-primary-200' : 'border-gray-200'
      }`}
      style={{ gridColumn: `span ${tile.w}`, gridRow: `span ${tile.h}` }}
    >
      <div
        draggable
        onDragStart={(event) => {
          event.dataTransfer.setData(TILE_INDEX_MIME, String(index));
          event.dataTransfer.effectAllowed = 'move';
        }}
        className="flex cursor-grab items-center justify-between border-b border-gray-100 bg-gray-50/60 px-3 py-2 active:cursor-grabbing"
      >
        <span className="truncate text-sm font-semibold text-gray-900">
          {chart ? chart.name : 'Chart unavailable'}
        </span>
        <button
          type="button"
          title="Remove"
          onClick={onRemove}
          className="ml-2 shrink-0 text-gray-400 opacity-0 transition-opacity hover:text-gray-700 group-hover:opacity-100"
        >
          <XMarkIcon className="h-4 w-4" />
        </button>
      </div>

      <div className="min-h-0 flex-1 p-2">
        {chart ? (
          <ChartCanvas vizType={chart.vizType} wells={parseWells(chart.configuration)} />
        ) : (
          <div className="flex h-full items-center justify-center text-xs text-gray-400">
            This chart was deleted.
          </div>
        )}
      </div>

      <span
        onPointerDown={handleResizeStart}
        title="Resize"
        className="absolute bottom-0 right-0 flex h-5 w-5 cursor-se-resize items-center justify-center text-gray-300 opacity-0 transition-opacity group-hover:opacity-100"
      >
        <svg width="10" height="10" viewBox="0 0 10 10" fill="currentColor">
          <path d="M9 1v8H1l8-8z" opacity="0.5" />
          <circle cx="8" cy="8" r="1" />
          <circle cx="8" cy="5" r="1" />
          <circle cx="5" cy="8" r="1" />
        </svg>
      </span>
    </div>
  );
};

export default DashboardEditorPage;
