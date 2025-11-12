export interface Employee {
  employeeId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profilePicture?: string;
  mobileNumber: string;
  alternateNumber?: string;
  personalEmail: string;
  officeEmail: string;
  departmentId: number;
  departmentName: string;
  roleId: number;
  roleName: string;
  employmentType: 'FullTime' | 'PartTime' | 'Contract' | 'Intern';
  dateOfJoining: string;
  reportingManagerId?: string;
  reportingManagerName?: string;
  status: 'Active' | 'Inactive';
  netSalary?: number;
  subordinatesCount: number;
}

export interface EmployeeFilters {
  search: string;
  departmentId: number | '';
  roleId: number | '';
  status: 'Active' | 'Inactive' | '';
  fromDate: string;
  toDate: string;
}

export interface Department {
  departmentId: number;
  departmentName: string;
  description?: string;
  employeeCount: number;
}

export interface Role {
  roleId: number;
  roleName: string;
  description?: string;
  departmentId?: number;
  departmentName?: string;
  employeeCount: number;
}