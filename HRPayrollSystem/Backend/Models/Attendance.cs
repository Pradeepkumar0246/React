using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Models
{
    public class Attendance
    {
        [Key]
        [Required]
        public int AttendanceId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan LoginTime { get; set; }

        public TimeSpan? LogoutTime { get; set; } // Nullable for check-in only

        [Required]
        public double WorkingHours { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        // Navigation Property
        public Employee? Employee { get; set; }
    }

}
