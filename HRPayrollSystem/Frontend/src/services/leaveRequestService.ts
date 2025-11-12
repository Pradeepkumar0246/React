import { leaveRequestApi } from '../api/leaveRequestApi';
import type { LeaveRequest } from '../types/leaveRequest';

export const leaveRequestService = {
  getAll: async (): Promise<LeaveRequest[]> => {
    const response = await leaveRequestApi.getAll();
    return response.data;
  },

  getById: async (id: number): Promise<LeaveRequest> => {
    const response = await leaveRequestApi.getById(id);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<LeaveRequest[]> => {
    const response = await leaveRequestApi.getByEmployee(employeeId);
    return response.data;
  },

  getByStatus: async (status: string): Promise<LeaveRequest[]> => {
    const response = await leaveRequestApi.getByStatus(status);
    return response.data;
  },

  create: async (data: { employeeId: string; leaveType: string; fromDate: string; toDate: string; isHalfDay: boolean; halfDayPeriod?: string; reason?: string }): Promise<LeaveRequest> => {
    const response = await leaveRequestApi.create(data);
    return response.data;
  },

  update: async (id: number, data: { leaveRequestId: number; employeeId: string; leaveType: string; fromDate: string; toDate: string; isHalfDay: boolean; halfDayPeriod?: string; reason?: string; status: string; approvedBy?: string }): Promise<LeaveRequest> => {
    const response = await leaveRequestApi.update(id, data);
    return response.data;
  },

  updateStatus: async (id: number, status: string, approvedBy?: string): Promise<LeaveRequest> => {
    const response = await leaveRequestApi.updateStatus(id, { status, approvedBy });
    return response.data;
  },
};