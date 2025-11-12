namespace HRPayrollSystem_Payslip.DTOs.SalaryStructureDTO
{
    public class SalaryStructureResponseDto
    {
        public int SalaryStructureId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal PF { get; set; }
        public decimal Tax { get; set; }
        public decimal NetSalary { get; set; }
        public decimal GrossSalary { get; set; }
    }
}