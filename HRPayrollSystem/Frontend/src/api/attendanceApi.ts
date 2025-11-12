import { apiClient } from './apiClient';
import type { AttendanceCreatePayload, AttendanceUpdatePayload } from '../types/api';

export const attendanceApi = {
  getAll: () => apiClient.get('/Attendance'),
  
  getById: (id: number) => apiClient.get(`/Attendance/${id}`),
  
  getByEmployee: (employeeId: string) => apiClient.get(`/Attendance/employee/${employeeId}`),
  
  getByDateRange: (fromDate: string, toDate: string) => 
    apiClient.get(`/Attendance/daterange?fromDate=${fromDate}&toDate=${toDate}`),
  
  create: (payload: AttendanceCreatePayload) => apiClient.post('/Attendance', payload),
  
  update: (id: number, payload: AttendanceUpdatePayload) => apiClient.put(`/Attendance/${id}`, payload),
  
  checkIn: (employeeId: string) => apiClient.post(`/Attendance/checkin/${employeeId}`, {}),
  
  checkOut: (employeeId: string) => apiClient.put(`/Attendance/checkout/${employeeId}`, {}),
};