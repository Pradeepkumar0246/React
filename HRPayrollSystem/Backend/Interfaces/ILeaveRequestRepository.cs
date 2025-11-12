using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
        Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest);
        Task<LeaveRequest> UpdateAsync(LeaveRequest leaveRequest);

        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime fromDate, DateTime toDate, int? excludeId = null);
    }
}