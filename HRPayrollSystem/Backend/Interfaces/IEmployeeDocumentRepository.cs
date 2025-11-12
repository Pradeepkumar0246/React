using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IEmployeeDocumentRepository
    {
        Task<IEnumerable<EmployeeDocument>> GetAllAsync();
        Task<EmployeeDocument?> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeDocument>> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<EmployeeDocument>> GetByCategoryAsync(string category);
        Task<EmployeeDocument> CreateAsync(EmployeeDocument employeeDocument);
        Task<EmployeeDocument> UpdateAsync(EmployeeDocument employeeDocument);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
    }
}