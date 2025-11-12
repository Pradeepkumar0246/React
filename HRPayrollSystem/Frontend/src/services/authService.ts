import { authApi } from '../api/authApi';
import type { LoginRequest, LoginResponse } from '../types/auth';

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await authApi.login(credentials);
    return response.data;
  },
};