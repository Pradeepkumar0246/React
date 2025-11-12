using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<Attendance?> GetByIdAsync(int id);
        Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<Attendance>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Attendance> CreateAsync(Attendance attendance);
        Task<Attendance> UpdateAsync(Attendance attendance);

        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<bool> AttendanceExistsForDateAsync(string employeeId, DateTime date, int? excludeId = null);
        Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date);
    }
}