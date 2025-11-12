using HRPayrollSystem_Payslip.Enums;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ILeaveBalanceRepository
    {
        Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(string employeeId, int year);
        Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(string employeeId, LeaveType leaveType, int year);
        Task<LeaveBalance> CreateAsync(LeaveBalance leaveBalance);
        Task<LeaveBalance> UpdateAsync(LeaveBalance leaveBalance);
        Task<bool> ExistsAsync(string employeeId, LeaveType leaveType, int year);
        Task CreateDefaultBalancesAsync(string employeeId, int year);
        Task ProcessYearEndAsync(int year);
        Task<Employee?> GetEmployeeAsync(string employeeId);
    }
}