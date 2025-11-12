using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.PayrollDTO
{
    public class PayrollGenerateDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Month is required.")]
        public DateTime Month { get; set; } // e.g., 2024-01-01 for January 2024

        public decimal? BonusAmount { get; set; } = 0;

        public decimal? AdditionalDeductions { get; set; } = 0;

        public HRPayrollSystem_Payslip.Enums.PaymentStatus PaymentStatus { get; set; } = HRPayrollSystem_Payslip.Enums.PaymentStatus.Pending;
    }
}