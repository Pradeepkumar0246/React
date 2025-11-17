import { apiClient } from './apiClient';

export const authApi = {
  login: (credentials: { officeEmail: string; password: string }) => 
    apiClient.post('/Auth/login', credentials),
  
  getCurrentUser: () => 
    apiClient.get('/Auth/me'),
};