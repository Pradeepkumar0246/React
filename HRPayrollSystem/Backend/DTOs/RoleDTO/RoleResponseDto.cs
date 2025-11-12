namespace HRPayrollSystem_Payslip.DTOs.RoleDTO
{
    public class RoleResponseDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
    }
}