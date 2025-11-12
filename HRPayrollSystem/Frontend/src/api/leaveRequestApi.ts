import { apiClient } from './apiClient';
import type { LeaveRequestCreatePayload, LeaveRequestUpdatePayload, LeaveRequestStatusUpdatePayload } from '../types/api';

export const leaveRequestApi = {
  getAll: () => apiClient.get('/LeaveRequest'),
  
  getById: (id: number) => apiClient.get(`/LeaveRequest/${id}`),
  
  getByEmployee: (employeeId: string) => apiClient.get(`/LeaveRequest/employee/${employeeId}`),
  
  getByStatus: (status: string) => apiClient.get(`/LeaveRequest/status/${status}`),
  
  create: (data: LeaveRequestCreatePayload) => apiClient.post('/LeaveRequest', data),
  
  update: (id: number, data: LeaveRequestUpdatePayload) => apiClient.put(`/LeaveRequest/${id}`, data),
  
  updateStatus: (id: number, data: LeaveRequestStatusUpdatePayload) => 
    apiClient.put(`/LeaveRequest/${id}/status`, data),
};