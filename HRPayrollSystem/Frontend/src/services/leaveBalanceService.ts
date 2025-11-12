import { commonApi } from '../api/commonApi';
import { apiClient } from '../api/apiClient';

type LeaveBalance = {
  leaveType: string;
  allocatedDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
};

export const leaveBalanceService = {
  getByEmployee: async (employeeId: string, year?: number): Promise<LeaveBalance[]> => {
    if (year) {
      const response = await commonApi.getLeaveBalanceByEmployee(employeeId, year);
      return response.data;
    } else {
      const response = await apiClient.get(`/LeaveBalance/employee/${employeeId}`);
      return response.data;
    }
  }
};