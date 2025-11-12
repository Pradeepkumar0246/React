using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.AttendanceDTO
{
    public class AttendanceResponseDto
    {
        public int AttendanceId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan LoginTime { get; set; }
        public TimeSpan? LogoutTime { get; set; }
        public double WorkingHours { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}