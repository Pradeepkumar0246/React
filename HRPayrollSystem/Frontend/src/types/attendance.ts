export interface Attendance {
  attendanceId: number;
  employeeId: string;
  employeeName: string;
  date: string;
  loginTime: string;
  logoutTime?: string;
  workingHours: number;
  status: 'Present' | 'Absent' | 'Leave' | 'Holiday';
}

export interface AttendanceFilters {
  search: string;
  employeeId: string;
  status: string;
  fromDate: string;
  toDate: string;
}