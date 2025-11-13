// Validation utility functions
export const validateEmail = (email: string): string | null => {
  if (!email) return 'Email is required';
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) return 'Invalid email format';
  return null;
};

export const validateMobileNumber = (mobile: string): string | null => {
  if (!mobile) return 'Mobile number is required';
  if (!/^[6-9]\d{9}$/.test(mobile)) return 'Invalid mobile number';
  return null;
};

export const validateRequired = (value: string, fieldName: string): string | null => {
  if (!value || value.trim() === '') return `${fieldName} is required`;
  return null;
};

export const validateStringLength = (value: string, min: number, max: number, fieldName: string): string | null => {
  if (value.length < min) return `${fieldName} must be at least ${min} characters`;
  if (value.length > max) return `${fieldName} must not exceed ${max} characters`;
  return null;
};

export const validateLettersOnly = (value: string, fieldName: string): string | null => {
  if (!/^[a-zA-Z\s]+$/.test(value)) return `${fieldName} must contain only letters`;
  return null;
};

export const validatePositiveNumber = (value: number, fieldName: string): string | null => {
  if (value < 0) return `${fieldName} must be positive`;
  return null;
};

export const validateDecimalPlaces = (value: number, places: number, fieldName: string): string | null => {
  const decimal = value.toString().split('.')[1];
  if (decimal && decimal.length > places) return `${fieldName} can have maximum ${places} decimal places`;
  return null;
};

export const validateDateNotFuture = (date: string, fieldName: string): string | null => {
  if (new Date(date) > new Date()) return `${fieldName} cannot be in the future`;
  return null;
};

export const validateDateRange = (fromDate: string, toDate: string): string | null => {
  if (new Date(fromDate) > new Date(toDate)) return 'To date must be after from date';
  return null;
};

export const validateTimeRange = (loginTime: string, logoutTime: string): string | null => {
  if (logoutTime && loginTime >= logoutTime) return 'Logout time must be after login time';
  return null;
};

export const validatePassword = (password: string): string | null => {
  if (!password) return 'Password is required';
  if (password.length < 8) return 'Password must be at least 8 characters';
  if (!/(?=.*[a-z])/.test(password)) return 'Password must contain at least one lowercase letter';
  if (!/(?=.*[A-Z])/.test(password)) return 'Password must contain at least one uppercase letter';
  if (!/(?=.*\d)/.test(password)) return 'Password must contain at least one number';
  if (!/(?=.*[@$!%*?&])/.test(password)) return 'Password must contain at least one special character';
  return null;
};

export const validateFileSize = (file: File, maxSizeMB: number): string | null => {
  if (file.size > maxSizeMB * 1024 * 1024) return `File size must not exceed ${maxSizeMB}MB`;
  return null;
};

export const validateFileType = (file: File, allowedTypes: string[]): string | null => {
  const fileType = file.type.toLowerCase();
  const fileName = file.name.toLowerCase();
  const isValidType = allowedTypes.some(type => 
    fileType.includes(type) || fileName.endsWith(`.${type}`)
  );
  if (!isValidType) return `File must be one of: ${allowedTypes.join(', ')}`;
  return null;
};