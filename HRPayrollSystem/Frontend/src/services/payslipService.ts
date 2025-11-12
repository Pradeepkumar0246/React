import { payslipApi } from '../api/payslipApi';
import type { Payslip } from '../types/payroll';

export const payslipService = {
  getAll: async (): Promise<Payslip[]> => {
    const response = await payslipApi.getAll();
    return response.data;
  },

  getById: async (id: number): Promise<Payslip> => {
    const response = await payslipApi.getById(id);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<Payslip[]> => {
    const response = await payslipApi.getByEmployee(employeeId);
    return response.data;
  },

  getByPayroll: async (payrollId: number): Promise<Payslip> => {
    const response = await payslipApi.getByPayroll(payrollId);
    return response.data;
  },

  download: async (id: number): Promise<Blob> => {
    const response = await payslipApi.download(id);
    return response.data;
  },
};