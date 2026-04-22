import { apiClient } from '@/shared/services/apiClient';

export const authService = {
  /**
   * POST /api/v1/auth/login
   * @param {{ email: string, password: string }} credentials
   */
  login: (credentials) => apiClient.post('/auth/login', credentials),
};
