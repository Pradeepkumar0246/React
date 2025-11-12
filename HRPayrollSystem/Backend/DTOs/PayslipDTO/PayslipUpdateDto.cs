using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.PayslipDTO
{
    public class PayslipUpdateDto
    {
        [Required(ErrorMessage = "Payslip ID is required.")]
        public int PayslipId { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Payroll ID is required.")]
        public int PayrollId { get; set; }

        public IFormFile? PayslipFile { get; set; }
    }
}