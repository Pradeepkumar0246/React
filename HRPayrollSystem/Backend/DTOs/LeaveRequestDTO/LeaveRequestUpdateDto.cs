using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO
{
    public class LeaveRequestUpdateDto
    {
        [Required(ErrorMessage = "Leave request ID is required.")]
        public int LeaveRequestId { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Leave type is required.")]
        public LeaveType LeaveType { get; set; }

        [Required(ErrorMessage = "From date is required.")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required.")]
        public DateTime ToDate { get; set; }

        public bool IsHalfDay { get; set; } = false;

        public HalfDayPeriod? HalfDayPeriod { get; set; }

        [StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
        public string? Reason { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public LeaveStatus Status { get; set; }

        public string? ApprovedBy { get; set; }
    }
}