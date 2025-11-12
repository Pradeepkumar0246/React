namespace HRPayrollSystem_Payslip.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string? Profilepicture { get; set; }
    }
}