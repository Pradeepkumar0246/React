using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Models
{
    public class Employee
    {
        [Key]
        [Required]
        public string EmployeeId { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string? ProfilePicture { get; set; }

        [Required, Phone, StringLength(10)]
        public string MobileNumber { get; set; }

        [Phone, StringLength(10)]
        public string? AlternateNumber { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string PersonalEmail { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string OfficeEmail { get; set; }

        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        public DateTime DateOfJoining { get; set; }

        [ForeignKey("ReportingManager")]
        public string? ReportingManagerId { get; set; }

        [Required]
        public EmployeeStatus Status { get; set; }

        // Navigation Properties
        public Department? Department { get; set; }
        public Role? Role { get; set; }
        public SalaryStructure? SalaryStructure { get; set; }
        public Employee? ReportingManager { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }
        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
        public ICollection<Payroll>? Payrolls { get; set; }
        public ICollection<Payslip>? Payslips { get; set; }
        public ICollection<EmployeeDocument>? Documents { get; set; }

    }
}
