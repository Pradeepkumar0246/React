using HRPayrollSystem_Payslip.DTOs.LeaveBalanceDTO;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ILeaveBalanceService
    {
        Task<IEnumerable<LeaveBalanceResponseDto>> GetEmployeeLeaveBalancesAsync(string employeeId, int? year = null);
        Task<bool> HasSufficientBalanceAsync(string employeeId, LeaveType leaveType, decimal requestedDays, int year);
        Task UpdateLeaveBalanceAsync(string employeeId, LeaveType leaveType, decimal usedDays, int year);
        Task CreateDefaultBalancesAsync(string employeeId);
        Task ProcessYearEndAsync();
    }
}