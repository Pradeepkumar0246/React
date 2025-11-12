export interface Payroll {
  payrollId: number;
  employeeId: string;
  employeeName: string;
  payrollMonth: string;
  grossSalary: number;
  totalDeductions: number;
  bonus: number;
  netPay: number;
  paymentDate: string;
  paymentStatus: 'Pending' | 'Processed' | 'Paid';
}

export interface Payslip {
  payslipId: number;
  employeeId: string;
  employeeName: string;
  payrollId: number;
  payrollMonth: string;
  filePath: string;
  fileName: string;
  generatedDate: string;
  netPay: number;
}

export interface PayrollFilters {
  search: string;
  employeeId: string;
  paymentStatus: string;
  month: string;
}

export interface PayrollGenerate {
  employeeId: string;
  month: string;
  bonusAmount?: number;
  additionalDeductions?: number;
  paymentStatus?: 'Pending' | 'Processed' | 'Paid';
}