using System.ComponentModel.DataAnnotations;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.DTOs.AttendanceDTO
{
    public class AttendanceCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required.")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Login time is required.")]
        public TimeSpan LoginTime { get; set; }

        public TimeSpan? LogoutTime { get; set; } // Optional for check-in only

        [Required(ErrorMessage = "Status is required.")]
        public AttendanceStatus Status { get; set; }
    }
}