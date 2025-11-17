export interface LoginRequest {
  officeEmail: string;
  password: string;
}

export interface LoginResponse {
  message: string;
  result: {
    token: string;
    employeeId: string;
    name: string;
    role: string;
    profilepicture?: string;
  };
}

export interface User {
  employeeId: string;
  name: string;
  role: string;
  email: string;
  profilePicture?: string;
}

export interface UserDetails {
  employeeId: string;
  name: string;
  role: string;
  email: string;
  profilePicture?: string;
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}