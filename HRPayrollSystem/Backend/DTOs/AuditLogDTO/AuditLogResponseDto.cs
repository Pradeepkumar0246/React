using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.AuditLogDTO
{
    public class AuditLogResponseDto
    {
        public int AuditId { get; set; }
        public string EmployeeId { get; set; }
        public ActionType Action { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; }
    }
}