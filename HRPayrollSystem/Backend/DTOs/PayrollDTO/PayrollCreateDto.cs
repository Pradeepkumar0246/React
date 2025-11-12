using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.PayrollDTO
{
    public class PayrollCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Payroll month is required.")]
        public DateTime PayrollMonth { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment status is required.")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}