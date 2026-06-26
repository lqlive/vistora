import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ArrowLeftIcon,
  ChartBarIcon,
  ChartPieIcon,
  GlobeAltIcon,
  HashtagIcon,
  MagnifyingGlassIcon,
  PresentationChartLineIcon,
  Squares2X2Icon,
  TableCellsIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline';
import PageHeader from '../../shared/components/PageHeader';
import { listDatasets } from '../datasets/api';
import ChartCanvas from './ChartCanvas';
import { createChart, getChart, mapChartToRequest, updateChart } from './api';
import type {
  ChartRequest,
  ChartVizType,
  DatasetColumnResponse,
  DatasetResponse,
} from '../../types';

type WellKey = 'axis' | 'legend' | 'values' | 'filters';

type ChartWells = Record<WellKey, string[]>;

interface VizMeta {
  type: ChartVizType;
  Icon: React.ComponentType<{ className?: string }>;
}

const vizCatalog: VizMeta[] = [
  { type: 'Bar Chart', Icon: ChartBarIcon },
  { type: 'Line Chart', Icon: PresentationChartLineIcon },
  { type: 'Area Chart', Icon: PresentationChartLineIcon },
  { type: 'Pie Chart', Icon: ChartPieIcon },
  { type: 'Table', Icon: TableCellsIcon },
  { type: 'Big Number', Icon: HashtagIcon },
  { type: 'Time-series', Icon: PresentationChartLineIcon },
  { type: 'Heatmap', Icon: Squares2X2Icon },
  { type: 'World Map', Icon: GlobeAltIcon },
  { type: 'Treemap', Icon: Squares2X2Icon },
];

const emptyWells: ChartWells = { axis: [], legend: [], values: [], filters: [] };

const defaultRequest: ChartRequest = {
  name: '',
  vizType: 'Bar Chart',
  dataset: '',
  description: null,
  configuration: null,
  favorite: false,
};

const wellsForViz = (vizType: string): { key: WellKey; label: string }[] => {
  switch (vizType) {
    case 'Table':
      return [
        { key: 'values', label: 'Columns' },
        { key: 'filters', label: 'Filters' },
      ];
    case 'Big Number':
      return [
        { key: 'values', label: 'Fields' },
        { key: 'filters', label: 'Filters' },
      ];
    case 'Pie Chart':
    case 'Treemap':
      return [
        { key: 'legend', label: 'Legend' },
        { key: 'values', label: 'Values' },
        { key: 'filters', label: 'Filters' },
      ];
    default:
      return [
        { key: 'axis', label: 'Axis' },
        { key: 'legend', label: 'Legend' },
        { key: 'values', label: 'Values' },
        { key: 'filters', label: 'Filters' },
      ];
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

const isNumericField = (column: DatasetColumnResponse): boolean =>
  /int|decimal|double|float|numeric|number|real|long|money/i.test(column.type);

const ChartEditorPage: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isNew = !id;

  const [request, setRequest] = useState<ChartRequest>(defaultRequest);
  const [wells, setWells] = useState<ChartWells>({ ...emptyWells });
  const [datasets, setDatasets] = useState<DatasetResponse[]>([]);
  const [fieldSearch, setFieldSearch] = useState('');
  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);

        const [datasetItems, chart] = await Promise.all([
          listDatasets(),
          id ? getChart(id) : Promise.resolve(null),
        ]);

        if (cancelled) return;

        setDatasets(datasetItems);
        const nextRequest = chart ? mapChartToRequest(chart) : defaultRequest;
        setRequest({
          ...nextRequest,
          dataset: nextRequest.dataset || datasetItems[0]?.name || '',
        });
        setWells(parseWells(nextRequest.configuration));
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Failed to load chart');
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

  const selectedDataset = useMemo(
    () => datasets.find((dataset) => dataset.name === request.dataset),
    [datasets, request.dataset]
  );

  const fields = selectedDataset?.columns ?? [];

  const filteredFields = useMemo(() => {
    const term = fieldSearch.trim().toLowerCase();
    if (!term) return fields;
    return fields.filter((field) => field.name.toLowerCase().includes(term));
  }, [fields, fieldSearch]);

  const activeWells = wellsForViz(request.vizType);
  const assignedCount = activeWells.reduce((total, well) => total + wells[well.key].length, 0);

  const fieldInAnyWell = (name: string): boolean =>
    (Object.keys(wells) as WellKey[]).some((key) => wells[key].includes(name));

  const addFieldToWell = (wellKey: WellKey, name: string) => {
    setWells((current) =>
      current[wellKey].includes(name)
        ? current
        : { ...current, [wellKey]: [...current[wellKey], name] }
    );
  };

  const removeFieldFromWell = (wellKey: WellKey, name: string) => {
    setWells((current) => ({
      ...current,
      [wellKey]: current[wellKey].filter((field) => field !== name),
    }));
  };

  const toggleField = (name: string) => {
    if (fieldInAnyWell(name)) {
      setWells((current) => ({
        axis: current.axis.filter((field) => field !== name),
        legend: current.legend.filter((field) => field !== name),
        values: current.values.filter((field) => field !== name),
        filters: current.filters.filter((field) => field !== name),
      }));
      return;
    }

    const defaultWell = activeWells[0]?.key ?? 'values';
    addFieldToWell(defaultWell, name);
  };

  const handleSave = async () => {
    if (!request.name.trim() || !request.vizType.trim() || !request.dataset.trim()) {
      setError('Name, visualization and dataset are required');
      return;
    }

    try {
      setSaving(true);
      setError(null);

      const payload: ChartRequest = {
        name: request.name.trim(),
        vizType: request.vizType.trim(),
        dataset: request.dataset.trim(),
        description: request.description?.trim() || null,
        configuration: JSON.stringify({ wells }),
        favorite: request.favorite,
      };

      const saved = id ? await updateChart(id, payload) : await createChart(payload);
      navigate(`/charts/${saved.id}/edit`, { replace: true });
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Failed to save chart');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <PageHeader
        title={isNew ? 'New Chart' : 'Edit Chart'}
        actions={
          <div className="flex items-center gap-2">
            <button className="btn-secondary" onClick={() => navigate('/charts')}>
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

      <div className="flex items-center gap-3 border-b border-gray-200 bg-white px-4 py-3 rounded-t-xl">
        <input
          value={request.name}
          onChange={(event) => setRequest((current) => ({ ...current, name: event.target.value }))}
          className="input max-w-sm"
          placeholder="Chart name"
        />
      </div>

      <div className="grid min-h-[560px] grid-cols-[minmax(0,1fr)_300px_260px] overflow-hidden rounded-b-xl border border-t-0 border-gray-200">
        {/* Canvas */}
        <section className="flex flex-col bg-gray-100 p-6">
          <div className="flex flex-1 flex-col items-center justify-center overflow-hidden rounded-lg border border-gray-200 bg-white">
            <CanvasPreview
              vizType={request.vizType}
              name={request.name}
              wells={wells}
              hasFields={assignedCount > 0}
            />
          </div>

          <div className="mt-4">
            <label className="mb-1 block text-xs font-medium text-gray-500">Description</label>
            <textarea
              value={request.description ?? ''}
              onChange={(event) =>
                setRequest((current) => ({ ...current, description: event.target.value }))
              }
              className="input min-h-[64px]"
              placeholder="Optional chart description"
            />
          </div>
        </section>

        {/* Visualizations pane */}
        <aside className="flex flex-col gap-4 border-l border-gray-200 bg-white p-4">
          <div>
            <h2 className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Visualizations
            </h2>
            <div className="grid grid-cols-5 gap-1.5">
              {vizCatalog.map(({ type, Icon }) => {
                const active = request.vizType === type;
                return (
                  <button
                    key={type}
                    type="button"
                    title={type}
                    onClick={() => setRequest((current) => ({ ...current, vizType: type }))}
                    className={`flex aspect-square items-center justify-center rounded border transition-colors ${
                      active
                        ? 'border-gray-900 bg-gray-900 text-white'
                        : 'border-gray-200 bg-white text-gray-500 hover:border-gray-300 hover:bg-gray-50'
                    }`}
                  >
                    <Icon className="h-4 w-4" />
                  </button>
                );
              })}
            </div>
          </div>

          <div className="space-y-3 border-t border-gray-100 pt-3">
            {activeWells.map((well) => (
              <FieldWell
                key={well.key}
                label={well.label}
                fields={wells[well.key]}
                onDropField={(name) => addFieldToWell(well.key, name)}
                onRemove={(name) => removeFieldFromWell(well.key, name)}
              />
            ))}
          </div>
        </aside>

        {/* Fields pane */}
        <aside className="flex flex-col border-l border-gray-200 bg-white">
          <div className="border-b border-gray-100 p-4">
            <h2 className="mb-2 text-xs font-semibold uppercase tracking-wide text-gray-500">
              Fields
            </h2>
            <select
              value={request.dataset}
              onChange={(event) =>
                setRequest((current) => ({ ...current, dataset: event.target.value }))
              }
              className="input"
            >
              <option value="">Select dataset</option>
              {datasets.map((dataset) => (
                <option key={dataset.id} value={dataset.name}>
                  {dataset.name}
                </option>
              ))}
            </select>

            <div className="relative mt-2">
              <MagnifyingGlassIcon className="pointer-events-none absolute left-2.5 top-2.5 h-4 w-4 text-gray-400" />
              <input
                value={fieldSearch}
                onChange={(event) => setFieldSearch(event.target.value)}
                className="input pl-8"
                placeholder="Search fields"
              />
            </div>
          </div>

          <div className="flex-1 overflow-auto p-2">
            {loading ? (
              <p className="px-2 py-4 text-sm text-gray-400">Loading fields...</p>
            ) : filteredFields.length === 0 ? (
              <p className="px-2 py-4 text-sm text-gray-400">
                {fields.length === 0
                  ? 'No fields available for this dataset yet.'
                  : 'No fields match your search.'}
              </p>
            ) : (
              <ul className="space-y-0.5">
                {filteredFields.map((field) => {
                  const selected = fieldInAnyWell(field.name);
                  return (
                    <li key={field.id || field.name}>
                      <div
                        role="button"
                        tabIndex={0}
                        aria-pressed={selected}
                        draggable
                        onDragStart={(event) =>
                          event.dataTransfer.setData('text/plain', field.name)
                        }
                        onClick={() => toggleField(field.name)}
                        onKeyDown={(event) => {
                          if (event.key === 'Enter' || event.key === ' ') {
                            event.preventDefault();
                            toggleField(field.name);
                          }
                        }}
                        className={`group flex select-none items-center gap-2.5 rounded-md px-2 py-2 text-sm text-gray-700 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-gray-400 ${
                          selected ? 'bg-gray-100' : 'hover:bg-gray-100'
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={selected}
                          readOnly
                          tabIndex={-1}
                          className="pointer-events-none h-4 w-4 shrink-0 rounded border-gray-300 text-gray-900"
                        />
                        <span
                          className={`w-7 shrink-0 text-center text-[10px] font-semibold ${
                            isNumericField(field) ? 'text-blue-500' : 'text-amber-500'
                          }`}
                        >
                          {isNumericField(field) ? '123' : 'Abc'}
                        </span>
                        <span className="flex-1 truncate">{field.name}</span>
                        <span
                          aria-hidden
                          title="Drag to a well"
                          className="ml-1 hidden shrink-0 cursor-grab text-gray-300 group-hover:block active:cursor-grabbing"
                        >
                          <svg width="12" height="12" viewBox="0 0 12 12" fill="currentColor">
                            <circle cx="3" cy="2" r="1" />
                            <circle cx="9" cy="2" r="1" />
                            <circle cx="3" cy="6" r="1" />
                            <circle cx="9" cy="6" r="1" />
                            <circle cx="3" cy="10" r="1" />
                            <circle cx="9" cy="10" r="1" />
                          </svg>
                        </span>
                      </div>
                    </li>
                  );
                })}
              </ul>
            )}
          </div>
        </aside>
      </div>
    </div>
  );
};

interface FieldWellProps {
  label: string;
  fields: string[];
  onDropField: (name: string) => void;
  onRemove: (name: string) => void;
}

const FieldWell: React.FC<FieldWellProps> = ({ label, fields, onDropField, onRemove }) => {
  const [dragOver, setDragOver] = useState(false);

  return (
    <div>
      <p className="mb-1 text-xs font-medium text-gray-600">{label}</p>
      <div
        onDragOver={(event) => {
          event.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(event) => {
          event.preventDefault();
          setDragOver(false);
          const name = event.dataTransfer.getData('text/plain');
          if (name) onDropField(name);
        }}
        className={`min-h-[40px] rounded-md border border-dashed p-1.5 transition-colors ${
          dragOver ? 'border-gray-900 bg-gray-50' : 'border-gray-300 bg-gray-50/40'
        }`}
      >
        {fields.length === 0 ? (
          <p className="px-1 py-1 text-xs text-gray-400">Add data fields here</p>
        ) : (
          <div className="flex flex-wrap gap-1.5">
            {fields.map((name) => (
              <span
                key={name}
                className="inline-flex items-center gap-1 rounded bg-gray-900 px-2 py-1 text-xs text-white"
              >
                {name}
                <button
                  type="button"
                  onClick={() => onRemove(name)}
                  className="text-gray-300 hover:text-white"
                >
                  <XMarkIcon className="h-3 w-3" />
                </button>
              </span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

interface CanvasPreviewProps {
  vizType: string;
  name: string;
  wells: ChartWells;
  hasFields: boolean;
}

const CanvasPreview: React.FC<CanvasPreviewProps> = ({ vizType, name, wells, hasFields }) => {
  const meta = vizCatalog.find((item) => item.type === vizType) ?? vizCatalog[0];
  const Icon = meta.Icon;

  if (!hasFields) {
    return (
      <div className="flex w-full max-w-md flex-col items-center px-6 text-center">
        <Icon className="h-16 w-16 text-gray-300" />
        <p className="mt-4 text-sm text-gray-400">
          Drag fields onto the wells to build your {vizType.toLowerCase()}.
        </p>
      </div>
    );
  }

  return (
    <div className="flex h-full w-full flex-col px-4 py-3">
      {name && (
        <h3 className="mb-2 shrink-0 text-center text-sm font-semibold text-gray-900">{name}</h3>
      )}
      <div className="min-h-0 flex-1">
        <ChartCanvas vizType={vizType} wells={wells} />
      </div>
    </div>
  );
};

export default ChartEditorPage;
