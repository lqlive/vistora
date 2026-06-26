import { api } from '../../lib/apiClient/http';
import { formatRelativeDate } from '../../shared/utils/format';
import type { ChartItem, ChartRequest, ChartResponse, ChartVizType } from '../../types';

export const listCharts = async (): Promise<ChartResponse[]> => {
  const response = await api.get<ChartResponse[]>('/api/charts');
  return response.data;
};

export const getChart = async (id: string): Promise<ChartResponse> => {
  const response = await api.get<ChartResponse>(`/api/charts/${id}`);
  return response.data;
};

export const createChart = async (request: ChartRequest): Promise<ChartResponse> => {
  const response = await api.post<ChartResponse>('/api/charts', request);
  return response.data;
};

export const updateChart = async (
  id: string,
  request: ChartRequest
): Promise<ChartResponse> => {
  const response = await api.put<ChartResponse>(`/api/charts/${id}`, request);
  return response.data;
};

export const deleteChart = async (id: string): Promise<void> => {
  await api.delete(`/api/charts/${id}`);
};

export const mapChartToItem = (chart: ChartResponse): ChartItem => ({
  id: chart.id,
  name: chart.name,
  vizType: chart.vizType as ChartVizType,
  dataset: chart.dataset,
  description: chart.description,
  configuration: chart.configuration,
  owners: [],
  modified: formatRelativeDate(chart.updatedAt),
  modifiedBy: '-',
  favorite: chart.favorite,
});

export const mapChartToRequest = (chart: ChartResponse): ChartRequest => ({
  name: chart.name,
  vizType: chart.vizType,
  dataset: chart.dataset,
  description: chart.description,
  configuration: chart.configuration,
  favorite: chart.favorite,
});
