using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.Models
{
    public class Role
    {
        [Key]
        [Required]
        public int RoleId { get; set; }

        [Required, StringLength(100)]
        public string RoleName { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public int? DepartmentId { get; set; }

        // Navigation Properties
        public Department? Department { get; set; }
        public ICollection<Employee>? Employees { get; set; }
    }

}
