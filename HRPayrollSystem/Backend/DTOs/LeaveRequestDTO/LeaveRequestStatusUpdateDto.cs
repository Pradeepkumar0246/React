using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO
{
    public class LeaveRequestStatusUpdateDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public LeaveStatus Status { get; set; }

        public string? ApprovedBy { get; set; }
    }
}