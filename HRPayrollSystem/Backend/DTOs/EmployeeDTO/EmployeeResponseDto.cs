using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.EmployeeDTO
{
    public class EmployeeResponseDto
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string MobileNumber { get; set; }
        public string? AlternateNumber { get; set; }
        public string PersonalEmail { get; set; }
        public string OfficeEmail { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string? ReportingManagerId { get; set; }
        public string? ReportingManagerName { get; set; }
        public EmployeeStatus Status { get; set; }
        public decimal? NetSalary { get; set; }
        public int SubordinatesCount { get; set; }
    }
}