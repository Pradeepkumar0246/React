using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.PayrollDTO
{
    public class PayrollUpdateDto
    {
        [Required(ErrorMessage = "Payroll ID is required.")]
        public int PayrollId { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Payroll month is required.")]
        public DateTime PayrollMonth { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Gross salary must be non-negative.")]
        public decimal GrossSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total deductions must be non-negative.")]
        public decimal TotalDeductions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Net pay must be non-negative.")]
        public decimal NetPay { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment status is required.")]
        public PaymentStatus PaymentStatus { get; set; }
    }
}