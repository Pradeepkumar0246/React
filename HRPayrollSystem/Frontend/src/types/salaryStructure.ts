export interface SalaryStructure {
  salaryStructureId: number;
  employeeId: string;
  employeeName: string;
  basicSalary: number;
  hra: number;
  allowances: number;
  deductions: number;
  pf: number;
  tax: number;
  netSalary: number;
  grossSalary: number;
}

export interface SalaryStructureFilters {
  search: string;
  employeeId: string;
}