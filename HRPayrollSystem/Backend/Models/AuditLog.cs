using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Models
{
    public class AuditLog
    {
        [Key]
        [Required]
        public int AuditId { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [Required]
        public ActionType Action { get; set; }

        [Required]
        public string TableName { get; set; }

        [Required]
        public string RecordId { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
