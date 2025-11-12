import { apiClient } from './apiClient';

export const payslipApi = {
  getAll: () => apiClient.get('/Payslip'),
  
  getById: (id: number) => apiClient.get(`/Payslip/${id}`),
  
  getByEmployee: (employeeId: string) => apiClient.get(`/Payslip/employee/${employeeId}`),
  
  getByPayroll: (payrollId: number) => apiClient.get(`/Payslip/payroll/${payrollId}`),
  
  download: (id: number) => apiClient.get(`/Payslip/${id}/download`, { responseType: 'blob' }),
};