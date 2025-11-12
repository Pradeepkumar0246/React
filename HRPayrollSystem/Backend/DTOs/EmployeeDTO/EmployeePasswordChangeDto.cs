using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDTO
{
    public class EmployeePasswordChangeDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 50 characters.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}