using HRPayrollSystem_Payslip.DTOs.LeaveRequestDTO;
using HRPayrollSystem_Payslip.Enums;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestResponseDto>> GetAllLeaveRequestsAsync();
        Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id);
        Task<IEnumerable<LeaveRequestResponseDto>> GetLeaveRequestsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<LeaveRequestResponseDto>> GetLeaveRequestsByStatusAsync(string status);
        Task<LeaveRequestResponseDto> CreateLeaveRequestAsync(LeaveRequestCreateDto leaveRequestCreateDto);
        Task<LeaveRequestResponseDto> UpdateLeaveRequestAsync(LeaveRequestUpdateDto leaveRequestUpdateDto);

        Task<LeaveRequestResponseDto> UpdateLeaveRequestStatusAsync(int id, LeaveStatus status, string? approvedBy = null);
    }
}