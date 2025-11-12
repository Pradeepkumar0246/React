using HRPayrollSystem_Payslip.DTOs.AuditLogDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogResponseDto>> GetAllAuditLogsAsync();
        Task<AuditLogResponseDto?> GetAuditLogByIdAsync(int id);
        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByActionTypeAsync(string actionType);
        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByDateRangeAsync(DateTime fromDate, DateTime toDate);

    }
}