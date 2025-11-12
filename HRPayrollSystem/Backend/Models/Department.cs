using System.ComponentModel.DataAnnotations;

namespace HRPayrollSystem_Payslip.Models
{
    public class Department
    {
        [Key]
        [Required]
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string DepartmentName { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        // Navigation Property
        public ICollection<Employee>? Employees { get; set; }
    }

}
