import { apiClient } from './apiClient';
import type { DepartmentCreatePayload, DepartmentUpdatePayload, RoleCreatePayload, RoleUpdatePayload, SalaryStructureCreatePayload, SalaryStructureUpdatePayload } from '../types/api';

export const commonApi = {
  // Department APIs
  getDepartments: () => apiClient.get('/Department'),
  createDepartment: (data: DepartmentCreatePayload) => apiClient.post('/Department', data),
  updateDepartment: (id: number, data: DepartmentUpdatePayload) => apiClient.put(`/Department/${id}`, data),
  deleteDepartment: (id: number) => apiClient.delete(`/Department/${id}`),
  
  // Role APIs
  getRoles: () => apiClient.get('/Role'),
  createRole: (data: RoleCreatePayload) => apiClient.post('/Role', data),
  updateRole: (id: number, data: RoleUpdatePayload) => apiClient.put(`/Role/${id}`, data),
  deleteRole: (id: number) => apiClient.delete(`/Role/${id}`),
  
  // Salary Structure APIs
  getSalaryStructures: () => apiClient.get('/SalaryStructure'),
  getSalaryStructureByEmployee: (employeeId: string) => apiClient.get(`/SalaryStructure/employee/${employeeId}`),
  createSalaryStructure: (data: SalaryStructureCreatePayload) => apiClient.post('/SalaryStructure', data),
  updateSalaryStructure: (id: number, data: SalaryStructureUpdatePayload) => apiClient.put(`/SalaryStructure/${id}`, data),
  deleteSalaryStructure: (id: number) => apiClient.delete(`/SalaryStructure/${id}`),
  
  // Leave Balance APIs
  getLeaveBalanceByEmployee: (employeeId: string, year: number) => 
    apiClient.get(`/LeaveBalance/employee/${employeeId}?year=${year}`),
  
  // Audit Log APIs
  getAuditLogs: () => apiClient.get('/AuditLog'),
};