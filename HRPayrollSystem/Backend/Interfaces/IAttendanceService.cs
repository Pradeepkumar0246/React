using HRPayrollSystem_Payslip.DTOs.AttendanceDTO;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceResponseDto>> GetAllAttendanceAsync();
        Task<AttendanceResponseDto?> GetAttendanceByIdAsync(int id);
        Task<IEnumerable<AttendanceResponseDto>> GetAttendanceByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<AttendanceResponseDto>> GetAttendanceByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<AttendanceResponseDto> CreateAttendanceAsync(AttendanceCreateDto attendanceCreateDto);
        Task<AttendanceResponseDto> UpdateAttendanceAsync(AttendanceUpdateDto attendanceUpdateDto);

        Task<AttendanceResponseDto> CheckInAsync(string employeeId);
        Task<AttendanceResponseDto> CheckOutAsync(string employeeId);
    }
}