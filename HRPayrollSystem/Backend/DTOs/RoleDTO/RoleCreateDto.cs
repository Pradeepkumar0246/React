using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.DTOs.RoleDTO
{
    public class RoleCreateDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
        public string RoleName { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        public int? DepartmentId { get; set; }
    }
}