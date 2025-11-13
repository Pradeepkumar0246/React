import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import ProtectedRoute from './ProtectedRoute';
import RoleGuard from './RoleGuard';
import UnauthorizedPage from './UnauthorizedPage';

// Pages
import Login from '../pages/Login';
import HRDashboard from '../pages/HRDashboard';
import EmployeeDashboard from '../pages/EmployeeDashboard';
import Employees from '../pages/Employees';
import Departments from '../pages/Departments';
import Roles from '../pages/Roles';
import AttendancePage from '../pages/Attendance';
import LeaveRequestsPage from '../pages/LeaveRequests';
import PayrollPage from '../pages/Payroll';
import SalaryStructurePage from '../pages/SalaryStructure';
import DocumentsPage from '../pages/Documents';
import AuditLogsPage from '../pages/AuditLogs';
import EmployeePayslips from '../pages/employee/EmployeePayslips';
import EmployeeLeaveRequests from '../pages/employee/EmployeeLeaveRequests';
import EmployeeDocuments from '../pages/employee/EmployeeDocuments';

const AppRouter: React.FC = () => {
  const { isAuthenticated, user, isLoading } = useAuth();
  const isHR = user?.role === 'Admin' || user?.role === 'HR Manager';
  
  console.log('AppRouter - User:', user?.role, 'isHR:', isHR, 'isLoading:', isLoading);

  return (
    <Routes>
      {/* Public Routes */}
      <Route 
        path="/login" 
        element={isAuthenticated ? <Navigate to={isHR ? "/dashboard" : "/employee/dashboard"} replace /> : <Login />} 
      />

      {/* HR/Admin Routes */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <HRDashboard />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/employees"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <Employees />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/departments"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <Departments />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/roles"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <Roles />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/salary-structure"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <SalaryStructurePage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/audit-logs"
        element={
          <ProtectedRoute>
            <RoleGuard allowedRoles={['Admin', 'HR Manager']}>
              <AuditLogsPage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />

      {/* Shared Routes with Role-based Redirects */}
      <Route
        path="/attendance"
        element={
          <ProtectedRoute>
            <RoleGuard>
              <AttendancePage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/leave-requests"
        element={
          <ProtectedRoute>
            <RoleGuard>
              <LeaveRequestsPage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/payroll"
        element={
          <ProtectedRoute>
            <RoleGuard>
              <PayrollPage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/documents"
        element={
          <ProtectedRoute>
            <RoleGuard>
              <DocumentsPage />
            </RoleGuard>
          </ProtectedRoute>
        }
      />

      {/* Employee Routes */}
      <Route
        path="/employee/dashboard"
        element={
          <ProtectedRoute>
            <EmployeeDashboard />
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/employee/payroll"
        element={
          <ProtectedRoute>
            <EmployeePayslips />
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/employee/leave-requests"
        element={
          <ProtectedRoute>
            <EmployeeLeaveRequests />
          </ProtectedRoute>
        }
      />
      
      <Route
        path="/employee/documents"
        element={
          <ProtectedRoute>
            <EmployeeDocuments />
          </ProtectedRoute>
        }
      />

      {/* Error Routes */}
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="/404" element={<UnauthorizedPage />} />

      {/* Default Routes */}
      <Route path="/" element={isAuthenticated ? <Navigate to={isHR ? "/dashboard" : "/employee/dashboard"} replace /> : <Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  );
};

export default AppRouter;