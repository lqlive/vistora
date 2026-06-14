import { api } from '../../lib/apiClient/http';
import { formatRelativeDate } from '../../shared/utils/format';
import type { DatasetItem, DatasetResponse } from '../../types';

export const listDatasets = async (): Promise<DatasetResponse[]> => {
  const response = await api.get<DatasetResponse[]>('/api/datasets');
  return response.data;
};

export const mapDatasetToItem = (dataset: DatasetResponse): DatasetItem => ({
  id: dataset.id,
  name: dataset.name,
  type: dataset.sql.trim().toLowerCase().startsWith('select') ? 'virtual' : 'physical',
  database: dataset.dataSources.length > 1 ? `${dataset.dataSources.length} sources` : 'Data source',
  schema: dataset.dataSources.map((source) => source.alias).filter(Boolean).join(', ') || '-',
  owners: [],
  charts: 0,
  modified: formatRelativeDate(dataset.updatedAt),
});
