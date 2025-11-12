using HRPayrollSystem_Payslip.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRPayrollSystem_Payslip.Models
{
    public class LeaveRequest
    {
        [Key]
        [Required]
        public int LeaveRequestId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [Precision(10, 2)]
        public decimal NumberOfDays { get; set; }

        public bool IsHalfDay { get; set; } = false;

        public HalfDayPeriod? HalfDayPeriod { get; set; }

        [StringLength(250)]
        public string? Reason { get; set; }

        [Required]
        public LeaveStatus Status { get; set; }

        [ForeignKey("Approver")]
        public string? ApprovedBy { get; set; }

        // Navigation Properties
        public Employee? Employee { get; set; }
        public Employee? Approver { get; set; }
    }

}
