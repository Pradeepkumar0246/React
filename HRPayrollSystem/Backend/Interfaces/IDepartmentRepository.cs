using HRPayrollSystem_Payslip.Models;

namespace HRPayrollSystem_Payslip.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<Department> CreateAsync(Department department);
        Task<Department> UpdateAsync(Department department);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> DepartmentNameExistsAsync(string departmentName, int? excludeId = null);
        Task<int> GetEmployeeCountAsync(int departmentId);
    }
}