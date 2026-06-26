import { api } from './http';
import type {
  DataSourceResponse,
  EngineColumnInfo,
  EngineExplainResult,
  EngineQueryResult,
  EngineTableInfo,
} from '../../types';

// Schema introspection and connection testing are exposed under the data source
// resource, while ad-hoc SQL execution lives under /api/query. Every call now
// references data sources by id; the server resolves their connection details.
export const listEngineTables = async (
  dataSource: DataSourceResponse
): Promise<EngineTableInfo[]> => {
  const response = await api.get<EngineTableInfo[]>(`/api/datasources/${dataSource.id}/tables`);
  return response.data;
};

export const listEngineColumns = async (
  dataSource: DataSourceResponse,
  table: string,
  schema?: string | null
): Promise<EngineColumnInfo[]> => {
  const response = await api.get<EngineColumnInfo[]>(
    `/api/datasources/${dataSource.id}/tables/${encodeURIComponent(table)}/columns`,
    { params: schema ? { schema } : undefined }
  );
  return response.data;
};

export const testEngineConnection = async (
  dataSource: DataSourceResponse
): Promise<void> => {
  await api.post(`/api/datasources/${dataSource.id}/test-connection`);
};

export const queryEngine = async (
  dataSource: DataSourceResponse,
  sql: string,
  limit = 1000
): Promise<EngineQueryResult> => {
  const response = await api.post<EngineQueryResult>('/api/query', {
    dataSourceIds: [dataSource.id],
    sql,
    limit,
    timeoutMs: 30000,
  });
  return response.data;
};

export const explainEngine = async (
  dataSource: DataSourceResponse,
  sql: string,
  limit = 1000
): Promise<EngineExplainResult> => {
  const response = await api.post<EngineExplainResult>('/api/query/explain', {
    dataSourceId: dataSource.id,
    sql,
    limit,
    timeoutMs: 30000,
  });
  return response.data;
};

export const federatedQueryEngine = async (
  dataSourceIds: string[],
  sql: string,
  limit = 1000
): Promise<EngineQueryResult> => {
  const response = await api.post<EngineQueryResult>('/api/query', {
    dataSourceIds,
    sql,
    limit,
    timeoutMs: 30000,
  });
  return response.data;
};
