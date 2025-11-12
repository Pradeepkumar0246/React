// Generic API Response wrapper
export interface ApiResponse<T = any> {
  data: T;
  message?: string;
  success?: boolean;
}

// Generic API Error interface
export interface ApiError {
  message: string;
  status?: number;
  code?: string;
  details?: string;
  errors?: Record<string, string[]>;
}

// Attendance API Payloads
export interface AttendanceCreatePayload {
  employeeId: string;
  date: string;
  loginTime: string;
  logoutTime?: string | null;
  status: number; // Backend expects enum as number
}

export interface AttendanceUpdatePayload extends AttendanceCreatePayload {
  attendanceId: number;
}

// Employee API Payloads
export interface EmployeeCreatePayload {
  firstName: string;
  lastName: string;
  mobileNumber: string;
  alternateNumber?: string;
  personalEmail: string;
  officeEmail: string;
  password: string;
  departmentId: number;
  roleId: number;
  employmentType: number; // Backend expects enum as number
  dateOfJoining: string;
  reportingManagerId?: string;
}

export interface EmployeeUpdatePayload extends Omit<EmployeeCreatePayload, 'password'> {
  employeeId: string;
}

// FormData interfaces
export interface EmployeeFormData extends FormData {
  append(name: keyof EmployeeCreatePayload | 'profilePicture', value: string | Blob): void;
}

export interface DocumentFormData extends FormData {
  append(name: 'employeeId' | 'category' | 'documentFile' | 'documentId', value: string | Blob): void;
}

// Payroll API Payloads
export interface PayrollGeneratePayload {
  employeeId: string;
  month: string;
  bonusAmount?: number;
  additionalDeductions?: number;
  paymentStatus: number; // Backend expects enum as number
}

export interface PayrollUpdatePayload {
  payrollId: number;
  employeeId: string;
  payrollMonth: string;
  grossSalary: number;
  totalDeductions: number;
  bonus: number;
  netPay: number;
  paymentDate: string;
  paymentStatus: 'Pending' | 'Processed' | 'Paid';
}

// Leave Request API Payloads
export interface LeaveRequestCreatePayload {
  employeeId: string;
  leaveType: string;
  fromDate: string;
  toDate: string;
  isHalfDay: boolean;
  halfDayPeriod?: string;
  reason?: string;
}

export interface LeaveRequestUpdatePayload extends LeaveRequestCreatePayload {
  leaveRequestId: number;
  status: string;
  approvedBy?: string;
}

export interface LeaveRequestStatusUpdatePayload {
  status: string;
  approvedBy?: string;
}

// Department API Payloads
export interface DepartmentCreatePayload {
  departmentName: string;
  description?: string;
}

export interface DepartmentUpdatePayload extends DepartmentCreatePayload {
  departmentId: number;
}

// Role API Payloads
export interface RoleCreatePayload {
  roleName: string;
  description?: string;
  departmentId?: number;
}

export interface RoleUpdatePayload extends RoleCreatePayload {
  roleId: number;
}

// Salary Structure API Payloads
export interface SalaryStructureCreatePayload {
  employeeId: string;
  basicSalary: number;
  hra: number;
  da: number;
  conveyanceAllowance: number;
  medicalAllowance: number;
  otherAllowances: number;
  pf: number;
  esi: number;
  professionalTax: number;
  otherDeductions: number;
}

export interface SalaryStructureUpdatePayload extends SalaryStructureCreatePayload {
  salaryStructureId: number;
}