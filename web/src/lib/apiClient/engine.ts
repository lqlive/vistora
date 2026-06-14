import { api } from './http';
import type {
  DataSourceResponse,
  EngineColumnInfo,
  EngineDataSourceConnection,
  EngineExplainResult,
  EngineQueryResult,
  EngineTableInfo,
} from '../../types';

// The query engine is hosted in-process by Nexova under /api/query (it is no longer a
// separate service), so every engine call goes through the main API instance.
export const listEngineTables = async (
  dataSource: DataSourceResponse
): Promise<EngineTableInfo[]> => {
  const response = await api.post<EngineTableInfo[]>('/api/query/schema/tables', {
    dataSource: toEngineDataSource(dataSource),
  });
  return response.data;
};

export const listEngineColumns = async (
  dataSource: DataSourceResponse,
  table: string,
  schema?: string | null
): Promise<EngineColumnInfo[]> => {
  const response = await api.post<EngineColumnInfo[]>('/api/query/schema/columns', {
    dataSource: toEngineDataSource(dataSource),
    schema,
    table,
  });
  return response.data;
};

export const testEngineConnection = async (
  dataSource: DataSourceResponse
): Promise<void> => {
  await api.post('/api/query/test-connection', {
    dataSource: toEngineDataSource(dataSource),
  });
};

export const queryEngine = async (
  dataSource: DataSourceResponse,
  sql: string,
  limit = 1000
): Promise<EngineQueryResult> => {
  const response = await api.post<EngineQueryResult>('/api/query', {
    dataSource: toEngineDataSource(dataSource),
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
    dataSource: toEngineDataSource(dataSource),
    sql,
    limit,
    timeoutMs: 30000,
  });
  return response.data;
};

export const federatedQueryEngine = async (
  dataSources: EngineDataSourceConnection[],
  sql: string,
  limit = 1000
): Promise<EngineQueryResult> => {
  const response = await api.post<EngineQueryResult>('/api/query/federated', {
    dataSources,
    sql,
    limit,
    timeoutMs: 30000,
  });
  return response.data;
};

const toEngineDataSource = (dataSource: DataSourceResponse): EngineDataSourceConnection => {
  const configuration = dataSource.configuration;
  return compactObject({
    type: dataSource.type,
    connectionString: configuration.connectionString,
    host: configuration.host,
    port: configuration.port,
    database: configuration.database,
    username: configuration.username,
    password: configuration.password,
    schema: configuration.schema,
    path: configuration.path,
  });
};

const compactObject = <T extends Record<string, unknown>>(value: T): T =>
  Object.fromEntries(
    Object.entries(value).filter(([, fieldValue]) => fieldValue !== null && fieldValue !== undefined && fieldValue !== '')
  ) as T;
