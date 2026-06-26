import { api } from '../../lib/apiClient/http';
import { formatRelativeDate } from '../../shared/utils/format';
import type { DashboardItem, DashboardRequest, DashboardResponse } from '../../types';

export const listDashboards = async (): Promise<DashboardResponse[]> => {
  const response = await api.get<DashboardResponse[]>('/api/dashboards');
  return response.data;
};

export const getDashboard = async (id: string): Promise<DashboardResponse> => {
  const response = await api.get<DashboardResponse>(`/api/dashboards/${id}`);
  return response.data;
};

export const createDashboard = async (request: DashboardRequest): Promise<DashboardResponse> => {
  const response = await api.post<DashboardResponse>('/api/dashboards', request);
  return response.data;
};

export const updateDashboard = async (
  id: string,
  request: DashboardRequest
): Promise<DashboardResponse> => {
  const response = await api.put<DashboardResponse>(`/api/dashboards/${id}`, request);
  return response.data;
};

export const deleteDashboard = async (id: string): Promise<void> => {
  await api.delete(`/api/dashboards/${id}`);
};

const countCharts = (configuration?: string | null): number => {
  if (!configuration) return 0;

  try {
    const parsed = JSON.parse(configuration) as { chartIds?: unknown };
    return Array.isArray(parsed.chartIds) ? parsed.chartIds.length : 0;
  } catch {
    return 0;
  }
};

const normalizeStatus = (status: string): 'published' | 'draft' =>
  status.toLowerCase() === 'published' ? 'published' : 'draft';

export const mapDashboardToItem = (dashboard: DashboardResponse): DashboardItem => ({
  id: dashboard.id,
  title: dashboard.name,
  status: normalizeStatus(dashboard.status),
  description: dashboard.description,
  configuration: dashboard.configuration,
  owners: [],
  modified: formatRelativeDate(dashboard.updatedAt),
  modifiedBy: '-',
  favorite: dashboard.favorite,
  charts: countCharts(dashboard.configuration),
});

export const mapDashboardToRequest = (dashboard: DashboardItem): DashboardRequest => ({
  name: dashboard.title,
  status: dashboard.status,
  description: dashboard.description ?? null,
  configuration: dashboard.configuration ?? null,
  favorite: dashboard.favorite,
});
