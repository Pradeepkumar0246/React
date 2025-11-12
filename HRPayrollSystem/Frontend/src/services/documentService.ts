import { documentApi } from '../api/documentApi';
import type { Document } from '../types/document';

export const documentService = {
  getAll: async (): Promise<Document[]> => {
    const response = await documentApi.getAll();
    return response.data;
  },

  getById: async (id: number): Promise<Document> => {
    const response = await documentApi.getById(id);
    return response.data;
  },

  getByEmployee: async (employeeId: string): Promise<Document[]> => {
    const response = await documentApi.getByEmployee(employeeId);
    return response.data;
  },

  getByCategory: async (category: string): Promise<Document[]> => {
    const response = await documentApi.getByCategory(category);
    return response.data;
  },

  upload: async (employeeId: string, category: string, file: File): Promise<Document> => {
    const categoryMap: { [key: string]: number } = {
      'IDProof': 0, 'Experience': 1, 'Skill': 2, 'Education': 3, 'Contract': 4, 'Offer': 5, 'Medical': 6
    };
    
    const formData = new FormData();
    formData.append('employeeId', employeeId);
    formData.append('category', categoryMap[category].toString());
    formData.append('documentFile', file);
    
    const response = await documentApi.create(formData);
    return response.data;
  },

  update: async (id: number, employeeId: string, category: string, file?: File): Promise<Document> => {
    const categoryMap: { [key: string]: number } = {
      'IDProof': 0, 'Experience': 1, 'Skill': 2, 'Education': 3, 'Contract': 4, 'Offer': 5, 'Medical': 6
    };
    
    const formData = new FormData();
    formData.append('documentId', id.toString());
    formData.append('employeeId', employeeId);
    formData.append('category', categoryMap[category].toString());
    if (file) formData.append('documentFile', file);
    
    const response = await documentApi.update(id, formData);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await documentApi.delete(id);
  },

  download: async (id: number): Promise<Blob> => {
    const response = await documentApi.download(id);
    return response.data;
  },
};