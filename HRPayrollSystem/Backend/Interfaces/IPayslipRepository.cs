using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IPayslipRepository
    {
        Task<IEnumerable<Payslip>> GetAllAsync();
        Task<Payslip?> GetByIdAsync(int id);
        Task<IEnumerable<Payslip>> GetByEmployeeIdAsync(string employeeId);
        Task<Payslip?> GetByPayrollIdAsync(int payrollId);
        Task<Payslip> UpdateAsync(Payslip payslip);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<bool> PayrollExistsAsync(int payrollId);
        Task<bool> PayslipExistsForPayrollAsync(int payrollId, int? excludeId = null);
        Task<Payroll?> GetPayrollByIdAsync(int payrollId);
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
    }
}