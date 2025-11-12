using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.PayrollDTO
{
    public class PayrollResponseDto
    {
        public int PayrollId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime PayrollMonth { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal Bonus { get; set; }
        public decimal NetPay { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}