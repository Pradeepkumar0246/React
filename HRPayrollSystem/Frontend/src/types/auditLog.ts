export interface AuditLog {
  auditId: number;
  employeeId: string;
  action: 'Created' | 'Updated' | 'Deleted' | 'Login' | 'Logout';
  tableName: string;
  recordId: string;
  oldValues?: string;
  newValues?: string;
  timestamp: string;
  timeAgo: string;
}

export interface AuditLogFilters {
  search: string;
  employeeId: string;
  action: string;
  tableName: string;
  fromDate: string;
  toDate: string;
}