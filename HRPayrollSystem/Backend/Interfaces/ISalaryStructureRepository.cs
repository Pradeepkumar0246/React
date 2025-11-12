using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface ISalaryStructureRepository
    {
        Task<IEnumerable<SalaryStructure>> GetAllAsync();
        Task<SalaryStructure?> GetByIdAsync(int id);
        Task<SalaryStructure?> GetByEmployeeIdAsync(string employeeId);
        Task<SalaryStructure> CreateAsync(SalaryStructure salaryStructure);
        Task<SalaryStructure> UpdateAsync(SalaryStructure salaryStructure);

        Task<bool> ExistsAsync(int id);
        Task<bool> EmployeeExistsAsync(string employeeId);
        Task<bool> EmployeeHasSalaryStructureAsync(string employeeId, int? excludeId = null);
    }
}