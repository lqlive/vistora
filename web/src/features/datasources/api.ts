import { api } from '../../lib/apiClient/http';
import { formatRelativeDate } from '../../shared/utils/format';
import type {
  AddDataSourceFileRequest,
  DataSourceItem,
  DataSourceRequest,
  DataSourceResponse,
  FileUploadResponse,
} from '../../types';

export const listDataSources = async (): Promise<DataSourceResponse[]> => {
  const response = await api.get<DataSourceResponse[]>('/api/datasources');
  return response.data;
};

export const getDataSource = async (id: string): Promise<DataSourceResponse> => {
  const response = await api.get<DataSourceResponse>(`/api/datasources/${id}`);
  return response.data;
};

export const createDataSource = async (request: DataSourceRequest): Promise<DataSourceResponse> => {
  const response = await api.post<DataSourceResponse>('/api/datasources', request);
  return response.data;
};

export const updateDataSource = async (
  id: string,
  request: DataSourceRequest
): Promise<DataSourceResponse> => {
  const response = await api.put<DataSourceResponse>(`/api/datasources/${id}`, request);
  return response.data;
};

export const deleteDataSource = async (id: string): Promise<void> => {
  await api.delete(`/api/datasources/${id}`);
};

export const uploadDataSourceFile = async (
  file: File,
  options?: {
    storageDirectory?: string;
  }
): Promise<FileUploadResponse> => {
  const formData = new FormData();
  formData.append('file', file);
  if (options?.storageDirectory) {
    formData.append('storageDirectory', options.storageDirectory);
  }

  const response = await api.post<FileUploadResponse>('/api/datasources/upload', formData);
  return response.data;
};

export const addFileToDataSource = async (
  dataSourceId: string,
  request: AddDataSourceFileRequest
): Promise<DataSourceResponse> => {
  const response = await api.post<DataSourceResponse>(
    `/api/datasources/${dataSourceId}/files`,
    request
  );
  return response.data;
};

export const uploadFileToDataSource = async (
  dataSourceId: string,
  file: File,
  options?: {
    storageDirectory?: string;
    hasHeader?: boolean;
    delimiter?: string;
    sheet?: string;
  }
): Promise<FileUploadResponse> => {
  const upload = await uploadDataSourceFile(file, {
    storageDirectory: options?.storageDirectory,
  });

  await addFileToDataSource(dataSourceId, {
    ...upload,
    hasHeader: options?.hasHeader,
    delimiter: options?.delimiter,
    sheet: options?.sheet,
  });

  return upload;
};

export const mapDataSourceToItem = (dataSource: DataSourceResponse): DataSourceItem => ({
  id: dataSource.id,
  name: dataSource.name,
  type: formatDataSourceType(dataSource.type),
  host: dataSourceHost(dataSource),
  status: 'connected',
  datasets: 0,
  lastSync: formatRelativeDate(dataSource.updatedAt),
  tables: dataSource.files.map((file) => file.tableName),
});

const dataSourceHost = (dataSource: DataSourceResponse): string => {
  const configuration = dataSource.configuration;
  if (configuration.path) return configuration.path;
  if (configuration.connectionString) return configuration.connectionString;
  if (configuration.host) {
    return configuration.port ? `${configuration.host}:${configuration.port}` : configuration.host;
  }

  return '-';
};

const formatDataSourceType = (type: string): string =>
  type
    .split(/[-_\s]+/)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ');
