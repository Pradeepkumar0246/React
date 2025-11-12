using HRPayrollSystem_Payslip.DTOs.AuditLogDTO;
using HRPayrollSystem_Payslip.Interfaces;
using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAllAuditLogsAsync()
        {
            var auditLogs = await _auditLogRepository.GetAllAsync();
            return auditLogs.Select(MapToResponseDto);
        }

        public async Task<AuditLogResponseDto?> GetAuditLogByIdAsync(int id)
        {
            var auditLog = await _auditLogRepository.GetByIdAsync(id);
            return auditLog == null ? null : MapToResponseDto(auditLog);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByActionTypeAsync(string actionType)
        {
            var auditLogs = await _auditLogRepository.GetByActionTypeAsync(actionType);
            return auditLogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByEmployeeIdAsync(string employeeId)
        {
            var auditLogs = await _auditLogRepository.GetByEmployeeIdAsync(employeeId);
            return auditLogs.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                throw new ArgumentException("From date cannot be greater than to date.");
            }

            var auditLogs = await _auditLogRepository.GetByDateRangeAsync(fromDate, toDate);
            return auditLogs.Select(MapToResponseDto);
        }







        private static AuditLogResponseDto MapToResponseDto(AuditLog auditLog)
        {
            return new AuditLogResponseDto
            {
                AuditId = auditLog.AuditId,
                EmployeeId = auditLog.EmployeeId,
                Action = auditLog.Action,
                TableName = auditLog.TableName,
                RecordId = auditLog.RecordId,
                OldValues = auditLog.OldValues,
                NewValues = auditLog.NewValues,
                Timestamp = auditLog.Timestamp,
                TimeAgo = GetTimeAgo(auditLog.Timestamp)
            };
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalDays >= 365)
                return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays >= 30)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            
            return "Just now";
        }
    }
}