import { commonApi } from '../api/commonApi';
import type { Department } from '../types/employee';

export const departmentService = {
  getAll: async (): Promise<Department[]> => {
    const response = await commonApi.getDepartments();
    return response.data;
  },

  getById: async (id: number): Promise<Department> => {
    const response = await commonApi.getDepartments();
    return response.data.find((dept: Department) => dept.departmentId === id) || {} as Department;
  },

  create: async (data: { departmentName: string; description?: string }): Promise<Department> => {
    const response = await commonApi.createDepartment(data);
    return response.data;
  },

  update: async (id: number, data: { departmentId: number; departmentName: string; description?: string }): Promise<Department> => {
    const response = await commonApi.updateDepartment(id, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await commonApi.deleteDepartment(id);
  },
};