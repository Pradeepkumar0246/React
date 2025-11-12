namespace HRPayrollSystem_Payslip.DTOs.DepartmentDTO
{
    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string? Description { get; set; }
        public int EmployeeCount { get; set; }
    }
}