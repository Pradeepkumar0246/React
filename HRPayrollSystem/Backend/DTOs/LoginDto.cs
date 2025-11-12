using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs
{
    public class LoginDto
    {
        [Required, EmailAddress]
        public string OfficeEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}