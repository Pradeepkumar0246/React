using HRPayrollSystem_Payslip.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRPayrollSystem_Payslip.Models
{
    public class LeaveBalance
    {
        [Key]
        [Required]
        public int LeaveBalanceId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public string EmployeeId { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        [Precision(10, 2)]
        public decimal AllocatedDays { get; set; }

        [Required]
        [Precision(10, 2)]
        public decimal UsedDays { get; set; } = 0;

        [Required]
        [Precision(10, 2)]
        public decimal RemainingDays { get; set; }

        [Required]
        public int Year { get; set; }

        // Navigation Property
        public Employee? Employee { get; set; }
    }
}