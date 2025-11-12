using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.LeaveBalanceDTO
{
    public class LeaveBalanceResponseDto
    {
        public int LeaveBalanceId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public LeaveType LeaveType { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public int Year { get; set; }
    }
}