import { authApi } from '../api/authApi';
import type { LoginRequest, LoginResponse, UserDetails } from '../types/auth';

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await authApi.login(credentials);
    return response.data;
  },
  
  getCurrentUser: async (): Promise<UserDetails> => {
    const response = await authApi.getCurrentUser();
    return response.data;
  },
};