using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.DepartmentDTO
{
    public class DepartmentCreateDto
    {
        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters.")]
        public string DepartmentName { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }
    }
}