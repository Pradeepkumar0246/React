import { attendanceApi } from '../api/attendanceApi';
import type { Attendance } from '../types/attendance';

export const attendanceService = {
  getAll: async (): Promise<Attendance[]> => {
    const response = await attendanceApi.getAll();
    return response.data;
  },

  getById: async (id: number): Promise<Attendance> => {
    const response = await attendanceApi.getById(id);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<Attendance[]> => {
    const response = await attendanceApi.getByEmployee(employeeId);
    return response.data;
  },

  getByDateRange: async (fromDate: string, toDate: string): Promise<Attendance[]> => {
    const response = await attendanceApi.getByDateRange(fromDate, toDate);
    return response.data;
  },

  create: async (data: { employeeId: string; date: string; loginTime: string; logoutTime?: string; status: string }): Promise<Attendance> => {
    try {
      const statusMap: { [key: string]: number } = { 'Present': 0, 'Absent': 1, 'Leave': 2, 'Holiday': 3 };
      const payload = {
        employeeId: data.employeeId,
        date: data.date,
        loginTime: data.loginTime.length === 5 ? data.loginTime + ':00' : data.loginTime,
        logoutTime: data.logoutTime ? (data.logoutTime.length === 5 ? data.logoutTime + ':00' : data.logoutTime) : null,
        status: statusMap[data.status] ?? 0
      };
      console.log('Creating attendance:', payload);
      const response = await attendanceApi.create(payload);
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { data?: string } };
      console.error('Create attendance error:', apiError.response?.data);
      throw error;
    }
  },

  update: async (id: number, data: { attendanceId: number; employeeId: string; date: string; loginTime: string; logoutTime?: string; status: string }): Promise<Attendance> => {
    const statusMap: { [key: string]: number } = { 'Present': 0, 'Absent': 1, 'Leave': 2, 'Holiday': 3 };
    const formatTime = (time: string) => time.length === 5 ? time + ':00' : time.length === 8 ? time : time;
    const payload = {
      attendanceId: data.attendanceId,
      employeeId: data.employeeId,
      date: data.date,
      loginTime: formatTime(data.loginTime),
      logoutTime: data.logoutTime ? formatTime(data.logoutTime) : null,
      status: statusMap[data.status] ?? 0
    };
    try {
      console.log('Updating attendance:', payload);
      const response = await attendanceApi.update(id, payload);
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { status?: number; data?: { errors?: Record<string, string[]> } } };
      console.error('Update attendance error details:');
      console.error('Status:', apiError.response?.status);
      console.error('Data:', apiError.response?.data);
      console.error('Validation errors:', apiError.response?.data?.errors);
      console.error('Payload sent:', payload);
      throw error;
    }
  },

  checkIn: async (employeeId: string): Promise<Attendance> => {
    try {
      if (!employeeId) {
        throw new Error('Employee ID is required for check-in');
      }
      console.log('Check-in for employee:', employeeId);
      const response = await attendanceApi.checkIn(employeeId);
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { status?: number; data?: string } };
      console.error('Check-in error details:');
      console.error('Status:', apiError.response?.status);
      console.error('Data:', apiError.response?.data);
      console.error('URL:', `/Attendance/checkin/${employeeId}`);
      console.error('Employee ID:', employeeId);
      throw error;
    }
  },

  checkOut: async (employeeId: string): Promise<Attendance> => {
    try {
      if (!employeeId) {
        throw new Error('Employee ID is required for check-out');
      }
      console.log('Check-out for employee:', employeeId);
      const response = await attendanceApi.checkOut(employeeId);
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { status?: number; data?: string } };
      console.error('Check-out error details:');
      console.error('Status:', apiError.response?.status);
      console.error('Data:', apiError.response?.data);
      console.error('URL:', `/Attendance/checkout/${employeeId}`);
      console.error('Employee ID:', employeeId);
      throw error;
    }
  },
};