import { api } from '../../lib/apiClient/http';
import type { QueryDocumentRequest, QueryDocumentResponse } from '../../types';

export type QueryDocumentScope = 'Accessible' | 'My' | 'Shared';

export const listQueryDocuments = async (
  scope: QueryDocumentScope = 'Accessible'
): Promise<QueryDocumentResponse[]> => {
  const response = await api.get<QueryDocumentResponse[]>('/api/user/query-documents', {
    params: { scope },
  });
  return response.data;
};

export const createQueryDocument = async (
  request: QueryDocumentRequest
): Promise<QueryDocumentResponse> => {
  const response = await api.post<QueryDocumentResponse>('/api/user/query-documents', request);
  return response.data;
};

export const updateQueryDocument = async (
  id: string,
  request: QueryDocumentRequest
): Promise<QueryDocumentResponse> => {
  const response = await api.put<QueryDocumentResponse>(`/api/user/query-documents/${id}`, request);
  return response.data;
};

export const deleteQueryDocument = async (id: string): Promise<void> => {
  await api.delete(`/api/user/query-documents/${id}`);
};
