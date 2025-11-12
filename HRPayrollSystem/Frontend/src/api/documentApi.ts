import { apiClient } from './apiClient';

export const documentApi = {
  getAll: () => apiClient.get('/EmployeeDocument'),
  
  getById: (id: number) => apiClient.get(`/EmployeeDocument/${id}`),
  
  getByEmployee: (employeeId: string) => apiClient.get(`/EmployeeDocument/employee/${employeeId}`),
  
  getByCategory: (category: string) => apiClient.get(`/EmployeeDocument/category/${category}`),
  
  create: (formData: FormData) => apiClient.post('/EmployeeDocument', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  
  update: (id: number, formData: FormData) => apiClient.put(`/EmployeeDocument/${id}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
  
  delete: (id: number) => apiClient.delete(`/EmployeeDocument/${id}`),
  
  download: (id: number) => apiClient.get(`/EmployeeDocument/${id}/download`, { responseType: 'blob' }),
};