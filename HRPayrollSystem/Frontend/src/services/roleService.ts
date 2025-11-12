import { commonApi } from '../api/commonApi';
import { apiClient } from '../api/apiClient';
import type { Role } from '../types/employee';

export const roleService = {
  getAll: async (): Promise<Role[]> => {
    const response = await commonApi.getRoles();
    return response.data;
  },

  getById: async (id: number): Promise<Role> => {
    const response = await apiClient.get(`/Role/${id}`);
    return response.data;
  },

  create: async (data: { roleName: string; description?: string; departmentId?: number }): Promise<Role> => {
    const response = await commonApi.createRole(data);
    return response.data;
  },

  update: async (id: number, data: { roleId: number; roleName: string; description?: string; departmentId?: number }): Promise<Role> => {
    const response = await commonApi.updateRole(id, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await commonApi.deleteRole(id);
  },
};