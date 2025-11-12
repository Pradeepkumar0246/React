import { apiClient } from './apiClient';
import type { PayrollGeneratePayload, PayrollUpdatePayload } from '../types/api';

export const payrollApi = {
  getAll: () => apiClient.get('/Payroll'),
  
  getById: (id: number) => apiClient.get(`/Payroll/${id}`),
  
  getByEmployee: (employeeId: string) => apiClient.get(`/Payroll/employee/${employeeId}`),
  
  generate: (data: PayrollGeneratePayload) => apiClient.post('/Payroll/generate', data),
  
  update: (id: number, data: PayrollUpdatePayload) => apiClient.put(`/Payroll/${id}`, data),
  
  delete: (id: number) => apiClient.delete(`/Payroll/${id}`),
};