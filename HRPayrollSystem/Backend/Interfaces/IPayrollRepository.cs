using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IPayrollRepository
    {
        Task<IEnumerable<Payroll>> GetAllAsync();
        Task<Payroll?> GetByIdAsync(int id);
        Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<Payroll>> GetByMonthAsync(DateTime month);
        Task<Payroll> CreateAsync(Payroll payroll);
        Task<Payroll> UpdateAsync(Payroll payroll);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<bool> PayrollExistsForMonthAsync(string employeeId, DateTime month, int? excludeId = null);
        Task<SalaryStructure?> GetEmployeeSalaryStructureAsync(string employeeId);
        Task<Payslip> CreatePayslipAsync(Payslip payslip);
        Task<IEnumerable<Attendance>> GetAttendanceRecordsAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<LeaveRequest>> GetLeaveRecordsAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
    }
}