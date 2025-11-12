import { employeeApi } from '../api/employeeApi';
import { apiClient } from '../api/apiClient';
import type { Employee, Department, Role } from '../types/employee';

export const employeeService = {
  // Employee APIs
  getAllEmployees: async (): Promise<Employee[]> => {
    const response = await employeeApi.getAll();
    return response.data;
  },

  getEmployeeById: async (id: string): Promise<Employee> => {
    const response = await employeeApi.getById(id);
    return response.data;
  },

  createEmployee: async (formData: FormData): Promise<Employee> => {
    try {
      const response = await apiClient.post('/Employee', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { data?: { errors?: Record<string, string[]> } } };
      console.error('Create error details:', apiError.response?.data);
      console.error('Validation errors:', apiError.response?.data?.errors);
      for (const [key, value] of formData.entries()) {
        console.log('FormData:', key, value);
      }
      throw error;
    }
  },

  updateEmployee: async (id: string, formData: FormData): Promise<Employee> => {
    formData.append('EmployeeId', id);
    try {
      const response = await apiClient.put(`/Employee/${id}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      return response.data;
    } catch (error) {
      const apiError = error as { response?: { data?: string } };
      console.error('Update error details:', apiError.response?.data);
      throw error;
    }
  },

  deleteEmployee: async (id: string): Promise<void> => {
    await employeeApi.delete(id);
  },

  // Department APIs
  getAllDepartments: async (): Promise<Department[]> => {
    const response = await apiClient.get('/Department');
    return response.data;
  },

  // Role APIs
  getAllRoles: async (): Promise<Role[]> => {
    const response = await apiClient.get('/Role');
    return response.data;
  },
};