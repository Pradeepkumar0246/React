using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDTO
{
    public class EmployeeUpdateDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; }

        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string MobileNumber { get; set; }

        [Phone]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Alternate number must be exactly 10 digits.")]
        public string? AlternateNumber { get; set; }

        [Required(ErrorMessage = "Personal email is required.")]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Personal email cannot exceed 100 characters.")]
        public string PersonalEmail { get; set; }

        [Required(ErrorMessage = "Office email is required.")]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Office email cannot exceed 100 characters.")]
        public string OfficeEmail { get; set; }

        [Required(ErrorMessage = "Department ID is required.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Role ID is required.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Employment type is required.")]
        public EmploymentType EmploymentType { get; set; }

        [Required(ErrorMessage = "Date of joining is required.")]
        public DateTime DateOfJoining { get; set; }

        public string? ReportingManagerId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public EmployeeStatus Status { get; set; }
    }
}