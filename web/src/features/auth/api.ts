import { api, API_BASE_URL } from '../../lib/apiClient/http';
import type { UserResponse } from '../../types';

// Returns the currently authenticated user, or null when not signed in.
export const getCurrentUser = async (): Promise<UserResponse | null> => {
  try {
    const response = await api.get<UserResponse>('/api/user/me');
    return response.data;
  } catch (error) {
    if (isUnauthorized(error)) {
      return null;
    }
    throw error;
  }
};

// GitHub OAuth requires a full-page redirect, so we navigate the browser to the
// backend challenge endpoint rather than issuing an XHR request.
export const loginWithGitHub = (returnUrl: string = window.location.href): void => {
  const target = `${API_BASE_URL}/api/user/github/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  window.location.href = target;
};

export const logout = async (): Promise<void> => {
  await api.post('/api/user/logout');
};

const isUnauthorized = (error: unknown): boolean =>
  typeof error === 'object' &&
  error !== null &&
  'response' in error &&
  (error as { response?: { status?: number } }).response?.status === 401;
