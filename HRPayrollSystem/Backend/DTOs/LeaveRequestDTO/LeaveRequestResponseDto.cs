using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO
{
    public class LeaveRequestResponseDto
    {
        public int LeaveRequestId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public bool IsHalfDay { get; set; }
        public HalfDayPeriod? HalfDayPeriod { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApproverName { get; set; }
    }
}