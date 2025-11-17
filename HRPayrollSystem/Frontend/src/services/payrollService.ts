import { payrollApi } from '../api/payrollApi';
import { apiClient } from '../api/apiClient';
import type { Payroll, PayrollGenerate } from '../types/payroll';

export const payrollService = {
  getAll: async (): Promise<Payroll[]> => {
    const response = await payrollApi.getAll();
    return response.data;
  },

  getById: async (id: number): Promise<Payroll> => {
    const response = await payrollApi.getById(id);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<Payroll[]> => {
    const response = await payrollApi.getByEmployee(employeeId);
    return response.data;
  },

  getByMonth: async (month: string): Promise<Payroll[]> => {
    const response = await apiClient.get(`/Payroll/month?month=${month}`);
    return response.data;
  },

  generate: async (data: PayrollGenerate): Promise<Payroll> => {
    // Convert payment status string to enum number
    const statusMap = { 'Pending': 0, 'Processed': 1, 'Paid': 2 };
    const paymentStatusNumber = data.paymentStatus ? statusMap[data.paymentStatus] : 0;
    
    const payload = {
      employeeId: data.employeeId,
      month: data.month + '-01', // Convert YYYY-MM to YYYY-MM-01
      bonusAmount: data.bonusAmount || 0,
      additionalDeductions: data.additionalDeductions || 0,
      paymentStatus: paymentStatusNumber
    };
    const response = await payrollApi.generate(payload);
    return response.data;
  },

  update: async (id: number, data: Partial<Payroll>): Promise<Payroll> => {
    // Ensure all required fields are present for PayrollUpdatePayload
    const payload = {
      payrollId: id,
      employeeId: data.employeeId!,
      payrollMonth: data.payrollMonth!,
      grossSalary: data.grossSalary!,
      totalDeductions: data.totalDeductions!,
      bonus: data.bonus!,
      netPay: data.netPay!,
      paymentDate: data.paymentDate!,
      paymentStatus: data.paymentStatus!
    };
    const response = await payrollApi.update(id, payload);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await payrollApi.delete(id);
  },
};