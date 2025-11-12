export interface LeaveRequest {
  leaveRequestId: number;
  employeeId: string;
  employeeName: string;
  leaveType: 'Casual' | 'Sick' | 'Earned' | 'Maternity' | 'Paternity' | 'Emergency' | 'LOP';
  fromDate: string;
  toDate: string;
  numberOfDays: number;
  isHalfDay: boolean;
  halfDayPeriod?: 'Morning' | 'Afternoon';
  reason?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  approvedBy?: string;
  approverName?: string;
}

export interface LeaveRequestFilters {
  search: string;
  employeeId: string;
  status: string;
  leaveType: string;
  fromDate: string;
  toDate: string;
}