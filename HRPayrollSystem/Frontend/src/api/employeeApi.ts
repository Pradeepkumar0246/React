import { apiClient } from './apiClient';
import type { EmployeeFormData } from '../types/api';

export const employeeApi = {
  getAll: () => apiClient.get('/Employee'),
  
  getById: (id: string) => apiClient.get(`/Employee/${id}`),
  
  create: (payload: EmployeeFormData) => apiClient.post('/Employee', payload),
  
  update: (id: string, payload: EmployeeFormData) => apiClient.put(`/Employee/${id}`, payload),
  
  delete: (id: string) => apiClient.delete(`/Employee/${id}`),
};