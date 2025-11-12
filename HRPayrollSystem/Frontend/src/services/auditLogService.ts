import { commonApi } from '../api/commonApi';
import { apiClient } from '../api/apiClient';
import type { AuditLog } from '../types/auditLog';

export const auditLogService = {
  getAll: async (): Promise<AuditLog[]> => {
    const response = await commonApi.getAuditLogs();
    return response.data;
  },

  getById: async (id: number): Promise<AuditLog> => {
    const response = await apiClient.get(`/AuditLog/${id}`);
    return response.data;
  },

  getByActionType: async (actionType: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLog/action/${actionType}`);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLog/employee/${employeeId}`);
    return response.data;
  },

  getByDateRange: async (fromDate: string, toDate: string): Promise<AuditLog[]> => {
    const response = await apiClient.get(`/AuditLog/daterange?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  },
};