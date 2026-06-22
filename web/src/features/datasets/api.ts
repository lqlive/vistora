import { api } from '../../lib/apiClient/http';
import { formatRelativeDate } from '../../shared/utils/format';
import type { DatasetItem, DatasetRequest, DatasetResponse } from '../../types';

export const listDatasets = async (): Promise<DatasetResponse[]> => {
  const response = await api.get<DatasetResponse[]>('/api/datasets');
  return response.data;
};

export const createDataset = async (request: DatasetRequest): Promise<DatasetResponse> => {
  const response = await api.post<DatasetResponse>('/api/datasets', request);
  return response.data;
};

export const updateDataset = async (
  id: string,
  request: DatasetRequest
): Promise<DatasetResponse> => {
  const response = await api.put<DatasetResponse>(`/api/datasets/${id}`, request);
  return response.data;
};

export const deleteDataset = async (id: string): Promise<void> => {
  await api.delete(`/api/datasets/${id}`);
};

export const mapDatasetToItem = (dataset: DatasetResponse): DatasetItem => ({
  id: dataset.id,
  name: dataset.name,
  sql: dataset.sql,
  description: dataset.description,
  type: dataset.sql.trim().toLowerCase().startsWith('select') ? 'virtual' : 'physical',
  database: dataset.dataSources.length > 1 ? `${dataset.dataSources.length} sources` : 'Data source',
  schema: dataset.dataSources.map((source) => source.alias).filter(Boolean).join(', ') || '-',
  owners: [],
  charts: 0,
  modified: formatRelativeDate(dataset.updatedAt),
});
