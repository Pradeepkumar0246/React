namespace HRPayrollSystem_Payslip.DTOs.PayrollDTO
{
    public class SalaryCalculationDto
    {
        public decimal AdjustedBasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal Allowances { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal DeductedAmount { get; set; }
        public int TotalDaysInMonth { get; set; }
    }
}