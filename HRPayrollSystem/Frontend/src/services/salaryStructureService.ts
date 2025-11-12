import { commonApi } from '../api/commonApi';
import { apiClient } from '../api/apiClient';
import type { SalaryStructure } from '../types/salaryStructure';

export const salaryStructureService = {
  getAll: async (): Promise<SalaryStructure[]> => {
    const response = await commonApi.getSalaryStructures();
    return response.data;
  },

  getById: async (id: number): Promise<SalaryStructure> => {
    const response = await apiClient.get(`/SalaryStructure/${id}`);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<SalaryStructure> => {
    const response = await commonApi.getSalaryStructureByEmployee(employeeId);
    return response.data;
  },

  create: async (data: Omit<SalaryStructure, 'salaryStructureId' | 'employeeName' | 'grossSalary' | 'netSalary'>): Promise<SalaryStructure> => {
    // Map SalaryStructure fields to SalaryStructureCreatePayload
    const payload = {
      employeeId: data.employeeId,
      basicSalary: data.basicSalary,
      hra: data.hra,
      da: 0, // Default value as not in SalaryStructure
      conveyanceAllowance: 0, // Default value as not in SalaryStructure
      medicalAllowance: 0, // Default value as not in SalaryStructure
      otherAllowances: data.allowances,
      pf: data.pf,
      esi: 0, // Default value as not in SalaryStructure
      professionalTax: data.tax,
      otherDeductions: data.deductions
    };
    const response = await commonApi.createSalaryStructure(payload);
    return response.data;
  },

  update: async (id: number, data: Omit<SalaryStructure, 'employeeName' | 'grossSalary'>): Promise<SalaryStructure> => {
    // Map SalaryStructure fields to SalaryStructureUpdatePayload
    const payload = {
      salaryStructureId: data.salaryStructureId,
      employeeId: data.employeeId,
      basicSalary: data.basicSalary,
      hra: data.hra,
      da: 0, // Default value as not in SalaryStructure
      conveyanceAllowance: 0, // Default value as not in SalaryStructure
      medicalAllowance: 0, // Default value as not in SalaryStructure
      otherAllowances: data.allowances,
      pf: data.pf,
      esi: 0, // Default value as not in SalaryStructure
      professionalTax: data.tax,
      otherDeductions: data.deductions
    };
    const response = await commonApi.updateSalaryStructure(id, payload);
    return response.data;
  },
};