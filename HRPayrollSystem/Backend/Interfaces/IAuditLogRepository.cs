using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetByActionTypeAsync(string actionType);
        Task<IEnumerable<AuditLog>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        Task<bool> ExistsAsync(int id);
    }
}