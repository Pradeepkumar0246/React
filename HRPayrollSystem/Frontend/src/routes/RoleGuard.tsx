import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

interface RoleGuardProps {
  children: React.ReactNode;
  allowedRoles?: string[];
  redirectTo?: string;
}

const RoleGuard: React.FC<RoleGuardProps> = ({ children, allowedRoles, redirectTo }) => {
  const { user, isLoading } = useAuth();
  const location = useLocation();
  
  // Wait for auth to load
  if (isLoading || !user) {
    return <div>Loading...</div>;
  }
  
  console.log('RoleGuard - User role:', user.role);
  const isHR = user?.role === 'Admin' || user?.role === 'HR Manager';
  const isEmployee = !isHR;
  console.log('RoleGuard - isHR:', isHR, 'isEmployee:', isEmployee);

  // Employee route redirects
  if (isEmployee) {
    const employeeRedirects: { [key: string]: string } = {
      '/payroll': '/employee/payroll',
      '/leave-requests': '/employee/leave-requests', 
      '/documents': '/employee/documents'
    };

    if (employeeRedirects[location.pathname]) {
      return <Navigate to={employeeRedirects[location.pathname]} replace />;
    }

    // Block employee access to HR-only routes
    const hrOnlyRoutes = ['/dashboard', '/employees', '/departments', '/roles', '/salary-structure', '/audit-logs'];
    if (hrOnlyRoutes.includes(location.pathname)) {
      return <Navigate to="/employee/dashboard" replace />;
    }
  }

  // Role-based access control
  if (allowedRoles && !allowedRoles.includes(user?.role || '')) {
    return <Navigate to={redirectTo || (isHR ? '/dashboard' : '/employee/dashboard')} replace />;
  }

  return <>{children}</>;
};

export default RoleGuard;