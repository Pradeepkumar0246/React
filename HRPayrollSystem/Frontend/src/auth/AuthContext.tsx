import React, { createContext, useContext, useReducer, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';
import type { AuthState, User, LoginRequest } from '../types/auth';
import { authService } from '../services/authService';

interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

type AuthAction =
  | { type: 'LOGIN_START' }
  | { type: 'LOGIN_SUCCESS'; payload: { user: User; token: string } }
  | { type: 'LOGIN_ERROR' }
  | { type: 'LOGOUT' }
  | { type: 'RESTORE_AUTH'; payload: { user: User; token: string } };

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, isLoading: true };
    case 'LOGIN_SUCCESS':
    case 'RESTORE_AUTH':
      return {
        ...state,
        user: action.payload.user,
        token: action.payload.token,
        isAuthenticated: true,
        isLoading: false,
      };
    case 'LOGIN_ERROR':
      return { ...state, isLoading: false };
    case 'LOGOUT':
      return {
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      };
    default:
      return state;
  }
};

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode(token) as { exp: number };
        if (decoded.exp * 1000 > Date.now()) {
          const user: User = {
            employeeId: localStorage.getItem('empid') || '',
            name: localStorage.getItem('name') || '',
            role: localStorage.getItem('role') || '',
            email: localStorage.getItem('email') || '',
            profilePicture: localStorage.getItem('profilePicture') || '',
          };
          dispatch({ type: 'RESTORE_AUTH', payload: { user, token } });
        } else {
          localStorage.removeItem('token');
          localStorage.removeItem('empid');
          localStorage.removeItem('name');
          localStorage.removeItem('role');
          localStorage.removeItem('email');
          localStorage.removeItem('profilePicture');
        }
      } catch {
        localStorage.removeItem('token');
        localStorage.removeItem('empid');
        localStorage.removeItem('name');
        localStorage.removeItem('role');
        localStorage.removeItem('email');
        localStorage.removeItem('profilePicture');
      }
    }
  }, []);

  const login = async (credentials: LoginRequest) => {
    dispatch({ type: 'LOGIN_START' });
    try {
      const response = await authService.login(credentials);
      console.log('Login response:', response);
      
      const result = response.result;
      const user: User = {
        employeeId: result.employeeId,
        name: result.name,
        role: result.role,
        email: credentials.officeEmail,
        profilePicture: result.profilepicture || '',
      };
      console.log('User object:', user);

      localStorage.setItem('token', result.token);
      localStorage.setItem('empid', result.employeeId);
      localStorage.setItem('name', result.name);
      localStorage.setItem('role', result.role);
      localStorage.setItem('email', credentials.officeEmail);
      localStorage.setItem('profilePicture', result.profilepicture || '');
      console.log('Stored in localStorage - role:', result.role);

      dispatch({ type: 'LOGIN_SUCCESS', payload: { user, token: result.token } });
    } catch (error) {
      dispatch({ type: 'LOGIN_ERROR' });
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('empid');
    localStorage.removeItem('name');
    localStorage.removeItem('role');
    localStorage.removeItem('email');
    localStorage.removeItem('profilePicture');
    dispatch({ type: 'LOGOUT' });
  };

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};