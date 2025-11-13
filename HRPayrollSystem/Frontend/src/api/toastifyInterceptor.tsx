import { toast } from 'react-toastify';
import { apiClient } from './apiClient';

export const setupToastInterceptor = () => {
  apiClient.interceptors.response.use(
    (response) => {
      if (response.data?.message) {
        toast.success(response.data.message);
      }
      return response;
    },
    (error) => {
      const message = error.response?.data?.message || error.message || 'An error occurred';
      toast.error(message);
      return Promise.reject(error);
    }
  );
};